## 2026-07-23 — GSU-M2.7C.5E.2.4B.5: Current whole-rock state documentation freeze

**Status:** Documentation-only freeze complete. Sparse-riverbed whole-rock work is paused for a separate Ground/cloud-shading workstream. The current implementation is the authoritative continuation baseline, but it is **not** feature-complete because Bank-owned sparse rocks remain absent even when Bank Feature Safety Margin and Whole Feature Return Fade are both `0`.

### Purpose

Freeze the exact accepted and unresolved state before another thread makes minor Ground-shader changes for cloud-related shading. This update changes documentation only. It does not attempt to diagnose or correct the remaining Bank-rock defect.

### Accepted current behavior

- `GSU-M2.7C.5E.2.4A.2` removed the Primary-Ground-coloured line at the Bank/Riverbed dry-surface handoff. User scene validation confirmed the line is gone.
- Algorithm-10 non-ID centre anchors reconstruct whole-rock ownership without feature IDs, centre/radius arrays, metadata textures, or fragment searches.
- `GSU-M2.7C.5E.2.4B.3` moved sparse-rock handling into final dry-response composition. The algorithm-10 boundary-sweep proof reports, for Ultra Sparse, Very Sparse, and Sparse candidates:
  - hard partial rocks: `0`;
  - removed-state residual: `0.00000`;
  - fade-inconsistent rocks: `0`;
  - anchor owner mismatches / invalid samples / inconsistent rocks / ungated emitted-response pixels: `0 / 0 / 0 / 0`.
- `GSU-M2.7C.5E.2.4B.4` separated feature-policy ownership from same-surface detail-sampling reuse. The passing proof reports feature ownership inconsistent / Bank-to-Riverbed / Riverbed-to-Bank policy leaks as `0 / 0 / 0` for all three candidates.
- User scene validation confirmed Bank feature settings no longer affect Riverbed-owned rocks; only Bank-owned response changes.
- The proof verdict is `PASS WITH SOURCE GEOMETRY DRIFT WARNING`. The eleven raw-source geometry fingerprint warnings predate this ownership freeze and are not accepted as proof failures.

### Known unresolved Bank defect

Observed user scene behavior:

- Bank Feature Safety Margin = `0`;
- Bank Whole Feature Return Fade = `0`;
- no sparse stones remain visible on the Bank.

This is a confirmed unresolved defect. Its cause has not been established. Do not document the absence of Bank rocks as intended conservative culling, and do not tune defaults or change the candidate-wide support radius until a facts-based runtime audit identifies the exact failing equation, field, or ownership input.

The first future stone-work task is therefore:

```text
Audit why Bank-owned whole-rock application weight is zero at zero Bank safety/fade controls.
```

The audit must trace the live Bank centre ownership, Bank inward distance, centre-evaluated application weight, required clearance, support-radius subtraction, and final rock-delta weight. It must distinguish proof behavior from live corridor/Ground interpolation and must not begin from a presumed cause.

### Frozen runtime and payload contracts

Until the Bank defect is deliberately resumed, preserve:

1. the `2.4A.2` normalized direct Bank/Riverbed handoff that prevents Primary Ground from reappearing between the two secondary surfaces;
2. algorithm version `10` and the Palette Form non-ID centre-anchor contract;
3. complete feature-response evidence from slope, cavity, form, and roughness;
4. final-response decomposition into fragment-local substrate response plus a centre-evaluated whole-rock response delta;
5. independent Bank/Riverbed feature-policy ownership selected from the reconstructed rock centre;
6. zero Bank-to-Riverbed and Riverbed-to-Bank policy leakage;
7. existing fixed candidate assets, installer paths, GUID preservation, and no-numbered-copy behavior;
8. the current no-ID, no-search-array, no-metadata-texture, no-extra-texture-sample architecture.

### Separate cloud-shading workstream boundary

A later thread is authorized to make minor Ground-shader changes for cloud-related shading. That work is separate from sparse-rock ownership and may change illumination response, but it must not silently alter the frozen contracts above.

Before modifying any shared Ground shader/include, the cloud-shading patch must audit and preserve, or explicitly declare and justify a change to:

- Bank/Riverbed substrate-composition weights;
- `2.4A.2` direct handoff behavior;
- algorithm-10 anchor decoding and support-radius transport;
- whole-rock owner selection;
- whole-rock final-response delta weighting;
- Bank/Riverbed feature-policy isolation;
- feature evidence and payload sampling count.

Cloud shading must remain lighting/shadow modulation. It must not become a new surface-identity mask, reintroduce Primary Ground in the Bank/Riverbed handoff, or multiply Bank/Riverbed rock weights by an unrelated cloud field.

When sparse-rock work resumes, compare the then-current Ground shader/includes against the `2.4B.4.1` baseline and reconcile every cloud-shading difference before diagnosing the Bank defect.

### Approved documentation-only scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
4. `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create/delete/move/rename: none.

### Validation and compliance result

- No runtime or Editor source is modified.
- No Unity compilation, proof rerun, installation, regeneration, or scene validation is required for this documentation-only freeze.
- The four documents record the same accepted behaviors, known limitation, future resumption gate, and cloud-shading compatibility boundary.
- The live project remains the source of truth for exact code after subsequent cloud-shading work; this freeze is the authoritative record of the river-surface contracts that such work must preserve.

---

## 2026-07-23 — GSU-M2.7C.5E.2.4B.4.1: Ownership proof compile correction

**Status:** Source correction and static consistency audit complete in the declared two-file scope. Unity C# compilation and the algorithm-10 proof remain pending.

### Trigger and proven cause

Unity reported `CS0103` at `GeneratedMassSparseRiverbedTileAssembler.cs:3819`: `supportRadius` did not exist in the context of `IsFeatureOwnerStateConsistent`. The direct caller `MeasureFeatureOwnerConsistency` already receives the candidate support radius and uses it to compute the ownership inset, but the nested helper multiplied decoded centre offsets by `supportRadius` without accepting that value as a parameter.

### Objective and acceptance

Pass the existing candidate support radius into both `IsFeatureOwnerStateConsistent` calls and add it to the helper signature. Preserve all 2.4B.4 ownership semantics, thresholds, counters, payload bytes, runtime shader behavior, and proof outputs.

### Approved scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`

Create/delete/move/rename: none.

### Implementation and static audit

- Added `supportRadius` to both calls to `IsFeatureOwnerStateConsistent`.
- Added `float supportRadius` to the helper signature.
- The helper continues to decode centre offsets as `encodedOffset * supportRadius`; no equation or threshold changed.
- Exact changed-file scope: two files.
- Caller/signature parity: two calls, each passing the existing candidate support radius.
- Undefined `supportRadius` references inside the helper: zero.
- C# delimiter balance: passed.
- Assembler algorithm remains `10`.
- No runtime shader, validation gate, payload, profile, control, installer, geometry, or generated asset changed.

### Pending validation

- Unity C# compilation must pass.
- Run the algorithm-10 proof and require all existing 2.4B.4 ownership and whole-rock gates to pass.

---

## 2026-07-23 — GSU-M2.7C.5E.2.4B.4: Independent Bank/Riverbed feature ownership

**Status:** Source implementation and post-change static consistency/compliance audit complete in the declared six-file scope. Unity C#/ForwardLit compilation, corrected algorithm-10 proof, scene acceptance, and GPU measurement remain pending.

### Trigger and proven defect

User scene evidence after the passing 2.4B.3 proof shows that whole-rock culling works but Bank Feature Safety Margin removes rocks visibly located in the Riverbed. Current source proves the ownership leak: `Frag` collapses equivalent Bank/Riverbed dry response into `sharedSubstrateWeights = (Ground, shared, 0)`, then calls `ResolveGroundBankLayerDetail` with the complete shared Bank+Riverbed weight while `ResolveGroundRiverbedLayerDetail` receives zero. `ResolveGroundBankLayerDetail` consequently applies `_GroundBankMaterialTransition` to every shared-surface rock, including rocks whose centres are inside the Riverbed.

The accepted 2.4A.2 same-surface sampling optimization is not itself rejected. The defect is that sampling ownership and feature-policy ownership were coupled. A shared dry surface may be sampled once, but its rock must select Bank or Riverbed feature policy from the reconstructed rock centre.

### Objective and acceptance

Preserve one shared dry-detail evaluation for equivalent Bank/Riverbed surfaces while making feature-policy ownership spatially independent:

- a rock whose reconstructed centre is inside the Riverbed inward-distance domain uses only Riverbed Material Blend Distance, Feature Safety Margin, and Whole Feature Return Fade;
- a rock whose reconstructed centre is outside that domain uses only the Bank settings;
- changing Bank feature settings causes zero change to Riverbed-owned rock weights;
- changing Riverbed feature settings causes zero change to Bank-owned rock weights;
- the shared dry response, normalized 2.4A.2 handoff, whole-rock final-response decomposition, hard/fade semantics, payload bytes, support radius, and texture-sample count remain unchanged;
- distinct Bank and Riverbed dry surfaces retain their existing independent detail paths.

The centre ownership classifier uses reconstructed Riverbed inward distance. An enabled Riverbed application owns the rock when its centre is inside the Riverbed domain beyond a numerical inset of `max(0.0001 m, conservativeSupportRadius × 0.02)`; otherwise Bank owns it. The two-percent support inset is only a centre-reconstruction stability guard, not a whole-support ownership rule. The shared feature response continues to use the centre-evaluated shared dry application weight, while only the selected owner supplies the clearance and fade policy.

### Approved scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
4. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
5. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
6. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`

Create/delete/move/rename: none. No installer or generated asset is changed.

### Reviewed evidence, producers, consumers, and invariants

- Current source was reconstructed from `Assets-Code-Archive(13).zip` plus accepted 2.4A, 2.4A.1, 2.4A.2, 2.4B, 2.4B.2, and 2.4B.3 packages. The archive contains no `.git`, so branch, HEAD, status, history, and unrelated live changes cannot be verified.
- `PixelSurfaceGroundForwardPass.hlsl` was reviewed through Bank/Riverbed detail sampling, shared-surface weight collapse, whole-rock delta composition, normal, albedo, smoothness, specular, and `Frag` call order. The exact leak is `sharedSubstrateWeights.z = 0` followed by Bank-only feature-policy evaluation.
- `PixelSurfaceGroundResponse.hlsl` was reviewed through application transitions, world-XZ centre reconstruction, whole-feature application weight, Bank/Riverbed inward-distance producers, and normalized substrate composition.
- `GroundMaterialControls.cs` and `GeneratedGround.cs` were reviewed as direct producers. Bank and Riverbed transition vectors are already uploaded independently; no C# property or serialized-control change is required.
- `PixelSurfaceMaterialDetail.hlsl` was reviewed completely. Its anchor, support-radius, substrate-detail, and final-response fields remain unchanged.
- The algorithm-10 assembler and validator were reviewed through anchor proof, final-response boundary sweep, report fields, failure gates, output fingerprints, and serialization. The new proof adds policy-isolation counters only; payload generation remains unchanged.
- Shared-shader impact: only Ground calls the changed whole-feature ownership helper. Generated Mass includes the Ground response file but does not enable or call this Ground material-property path.

### Implementation sequence

1. Add a same-surface ownership-aware whole-feature weight helper that reconstructs shared application weight, Bank inward distance, and Riverbed inward distance at the common rock centre.
2. Select Bank or Riverbed transition settings from centre Riverbed ownership and calculate one shared whole-rock weight. Keep invalid reconstruction conservative: suppress the discrete-rock delta while retaining substrate response.
3. Pass `sameDrySurface` into Bank detail resolution. Use the ownership-aware helper only for the equivalent shared-surface path; preserve the existing Bank helper for distinct surfaces and the existing Riverbed helper for the Riverbed path.
4. Add algorithm-10 ownership proof counters: reconstructed owner classification must be consistent across every emitted pixel of a rock, and Bank-setting changes affecting Riverbed-owned rocks plus Riverbed-setting changes affecting Bank-owned rocks must all be zero.
5. Update report and failure gates, then complete exact-scope, complete-file reread, producer/consumer, delimiter, preprocessor, sample-site, branch, payload, and algorithm audits.

### Performance and risk

No texture sample, draw call, mesh stream, payload channel, CBUFFER property, runtime allocation, per-frame CPU work, ID, array, metadata texture, or search is added. Equivalent shared-surface feature evaluation adds one Riverbed inward-distance centre reconstruction and owner-policy selection. Distinct-surface paths remain unchanged. `PERFORMANCE EXCEPTION`: none; GPU measurement remains pending.

Primary risks are derivative cost and FXC sensitivity. The implementation must not branch around texture sampling or duplicate the shared detail sample. The additional ownership work remains within the existing uniform whole-feature branch, and compile-before-proof/install remains mandatory.

### Validation gates

- C# and ForwardLit compile before proof or installation.
- Algorithm-10 proof reports zero owner-inconsistent rocks, zero Bank-to-Riverbed policy leaks, and zero Riverbed-to-Bank policy leaks for every candidate.
- Existing anchor, emitted-response, hard/fade boundary-sweep, and residual gates remain passing.
- In scene, changing Bank Feature Safety Margin or Return Fade does not alter Riverbed-owned rocks; changing Riverbed settings does not alter Bank-owned rocks.
- Hard and positive-fade whole-rock behavior remains complete and uniform for both owners.
- GPU timing is compared with 2.4B.3 under the same camera and material settings.

### Implementation result and post-change audit

The implementation changed exactly the six declared files and no others. Equivalent Bank/Riverbed surfaces still sample the shared detail once through the Bank/shared slot. The shared whole-feature evaluator now reconstructs Bank inward distance, Riverbed inward distance, and shared application weight at the rock centre. Riverbed owns the feature when its reconstructed centre clears `max(0.0001 m, conservative support radius × 0.02)` inside the Riverbed domain; otherwise Bank owns it. Only the selected owner transition vector supplies Material Blend Distance, Feature Safety Margin, and Whole Feature Return Fade. Distinct-surface Bank and Riverbed evaluators are unchanged.

The algorithm-10 proof now reports owner-inconsistent rocks, Bank-setting effects on Riverbed-owned rocks, and Riverbed-setting effects on Bank-owned rocks. All three are hard zero gates and participate in the deterministic candidate fingerprint. Payload production, candidate placement, installed assets, installer identity, and algorithm version remain unchanged.

Post-change static evidence:

- exact expected-versus-actual scope: passed, six files;
- complete changed-file reread and direct producer/consumer reconciliation: passed;
- C# and HLSL brace, parenthesis, and bracket balance: passed;
- HLSL preprocessor balance: passed;
- detail sampler call-site parity: one Bank definition/caller and one Riverbed definition/caller;
- Ground ForwardPass texture sampling remains four array samples plus one ordinary 2D sample;
- `[branch]` annotations increase from six to seven; the added branch is material-uniform and surrounds scalar ownership evaluation only, not texture sampling;
- `ddx`/`ddy` source sites remain unchanged; the shared path invokes the existing scalar-gradient helper once more for Riverbed centre ownership;
- accepted proof centre-error ratio is `0.00033 / 0.02924 = 1.13%`; the two-percent support ownership inset exceeds that measured ratio;
- 400,000 randomized owner-threshold trials using the accepted error bound produced zero owner inconsistencies;
- exhaustive proof-policy samples produced zero Bank-to-Riverbed and zero Riverbed-to-Bank setting leaks;
- no ID, search array, metadata texture, ShaderLab property, CBUFFER member, payload channel, profile, control, installer, corridor, wetness, cover, or 2.4A.2 composition edit was introduced;
- assembler algorithm remains `10`.

Unity compilation and execution are unavailable in the reconstruction environment. Compile and proof remain mandatory before scene acceptance. Reinstallation is not required because the payload and installed candidate assets are unchanged.

---

## 2026-07-23 — GSU-M2.7C.5E.2.4B.3: Whole-rock final-response composition

**Status:** Source implementation and post-change static consistency/compliance audit complete in the declared seven-file scope. Unity C#/ForwardLit compilation, corrected algorithm-10 boundary-sweep proof, installation, scene acceptance, and GPU measurement remain pending.

### Trigger and proven defect

The user-run algorithm-10 proof passed the non-ID centre-anchor contract for all three candidates: owner mismatches, invalid samples, inconsistent rocks, and ungated emitted-response pixels were zero; maximum reconstructed-centre error was `0.00033` tile UV; maximum anchor-retention spread was `0.00041`; and mip 3 was accepted. Scene evidence nevertheless shows rocks still phasing across the application boundary.

Read-only review proves the runtime applies whole-feature retention only inside `PS3D_StylizedSurfaceDetail`, then multiplies the resulting rock response by fragment-local Bank/Riverbed substrate weights in final albedo, normal, smoothness, and specular composition. `ResolveGroundWholeFeatureRetention` also ignores `transitionSettings.x`, so a rock can return while part of its silhouette remains inside the ordinary material blend. The existing proof measures anchor reconstruction only; it does not sweep an application boundary through the actual final-response weight.

### Objective and acceptance

Separate each feature-aware layer into a locally composed substrate-only response plus a discrete-rock response delta. Apply the substrate-only response with the existing fragment-local Bank/Riverbed weights, and apply the rock delta with one centre-evaluated whole-rock application weight.

Acceptance requires:

- Material Blend Distance is included automatically in the feature-free distance: `required clearance = Material Blend Distance + Feature Safety Margin`.
- Whole Feature Return Fade `0` yields only fully absent or fully present rocks; positive fade yields one common scalar for the entire rock.
- The common scalar controls rock albedo/form/cavity, normal slope, roughness/smoothness, and dry specular response.
- Feature-texture substrate response continues to use fragment-local application composition.
- Equivalent Bank/Riverbed dry surfaces apply the shared rock delta once through the Bank/shared slot.
- Algorithm-10 B/A anchor channels, placements, support radius, evidence thresholds, installer identity, texture samples, corridor data, controls, and 2.4A.2 direct handoff remain unchanged.

### Approved scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
4. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
5. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`
6. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
7. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`

Create/delete/move/rename: none. Proof images remain local under `Library`.

### Reviewed evidence, producers, consumers, and invariants

- Current source reconstructed from `Assets-Code-Archive(13).zip` plus accepted 2.4A, 2.4A.1, 2.4A.2, 2.4B, and 2.4B.2 packages; the supplied archive has no `.git`, so branch, HEAD, status, and unrelated live changes are unavailable.
- `PixelSurfaceGroundForwardPass.hlsl` was reviewed completely, including both detail samplers, albedo composition, normal composition, smoothness, dry specular, texture-form lighting response, and `Frag` call order.
- `PixelSurfaceGroundResponse.hlsl` was reviewed completely, including the application transition, world-XZ gradient solve, whole-feature retention, normalized Ground/Bank/Riverbed composition, and Bank/Riverbed material blend producers.
- `PixelSurfaceMaterialDetail.hlsl` was reviewed completely. The current feature-retention helper replaces full detail with substrate detail before final composition but carries no final whole-feature application weight.
- `GeneratedMassSparseRiverbedTileAssembler.cs` and `GeneratedMassSparseRiverbedTileAssemblyValidation.cs` were reviewed through payload production, centre-anchor proof, report gates, evidence output, and fingerprints.
- Shared-shader impact: `PixelSurfaceMaterialDetail.hlsl` is consumed by Ground ForwardLit only in the supplied source. `PixelSurfaceGroundResponse.hlsl` is also included by the Generated Mass shader, but all new whole-feature functions remain inside the Ground material-property guard and are not called by Generated Mass.

### Implementation sequence

1. Extend the detail result with an absolute feature-application weight and add a substrate-only detail resolver that preserves ordinary/non-feature payloads unchanged.
2. Replace fragment-detail retention with a centre-evaluated whole-feature application weight. Include Material Blend Distance in the hard/fade threshold and reconstruct the application weight at the same centre through the existing guarded world-XZ gradient solve.
3. Pass final normalized Bank/Riverbed composition weights into detail resolution. Compose substrate-only albedo/normal/smoothness/specular locally, then add full-minus-substrate rock deltas using the absolute whole-rock weights.
4. Preserve equivalent-surface composition by assigning the shared normalized weight to Bank and zero to Riverbed before detail resolution.
5. Extend algorithm-10 proof with hard and fade boundary sweeps over actual emitted feature-response weights; report maximum within-rock spread, partial-rock count, removal residual, and produce a boundary-sweep contact sheet.
6. Update validation/report gates and architecture, then run exact-scope, full-file reread, delimiter/preprocessor, symbol, sample-site, branch, payload, algorithm, and package audits.

### Performance and risk

No texture sample, draw call, mesh stream, payload channel, CBUFFER property, runtime allocation, per-frame CPU work, ID, array, metadata texture, or shader search is added. Runtime adds substrate-detail derivation, a second scalar-gradient solve for feature pixels when whole-feature handling is enabled, and full-minus-substrate scalar/vector delta composition. `PERFORMANCE EXCEPTION`: none; GPU measurement against 2.4A.2 remains mandatory.

Primary risks are FXC sensitivity to expanded final composition and centre-application extrapolation error. Mitigations are branchless scalar/vector delta composition, unchanged sample sites, uniform-only gating around derivative work, conservative zero fallback for invalid gradients, compile-before-proof/install, and numerical boundary-sweep gates.

### Validation gates

- C# and ForwardLit compile before proof or installation.
- Algorithm-10 proof reports zero hard-mode partial rocks, zero removed-response residual within tolerance, and whole-rock hard/fade spread within tolerance for every candidate.
- Boundary-sweep contact sheets show rocks absent/present or uniformly faded, never spatially clipped.
- Installer remains blocked unless the corrected proof passes.
- Scene validation covers hard return, positive fade, Bank and Riverbed boundaries, close/production camera, repeats, and mips.
- GPU timing is compared against the accepted 2.4A.2 baseline.

### Implementation result and post-change audit

The implementation changed exactly the seven declared files and no others. The final runtime path now resolves the feature-aware layer as local substrate response plus a full-minus-substrate discrete-rock delta. The delta is multiplied by `featureApplicationWeight`, an absolute centre-evaluated layer weight that includes the ordinary Material Blend Distance in its required clearance. Bank and Riverbed albedo, world-XZ slope, roughness-derived smoothness, and dry specular all use the same decomposition. Equivalent Bank/Riverbed surfaces continue to place the shared normalized application in Bank and zero Riverbed before detail resolution, so the rock delta is applied once.

The algorithm-10 proof now adds eight-orientation hard/fade boundary sweeps over every emitted rock response, reports hard/fade maximum within-rock weight spread, hard partial-rock count, removed-state residual, and fade inconsistency count, and writes a four-panel `WholeFeatureBoundarySweep` contact sheet. Algorithm version, payload bytes, anchor channels, support-radius metadata, evidence thresholds, installer identity, and installed asset paths are unchanged.

Post-change static evidence:

- exact expected-versus-actual file scope: passed;
- full changed-file reread and producer/consumer reconciliation: passed;
- C# and HLSL delimiter balance: passed;
- HLSL preprocessor balance: passed;
- old whole-feature-retention caller removal and new application-weight caller parity: passed;
- 250,000 randomized full-response decomposition identities: maximum numerical error `6.67e-16`;
- Ground ForwardPass sample sites remain four texture-array samples plus one ordinary texture sample;
- `[branch]`, `ddx`, and `ddy` source-site counts are unchanged; the existing gradient helper is invoked a second time at runtime;
- no ID, search array, metadata texture, ShaderLab property, CBUFFER member, payload channel, corridor edit, installer edit, profile edit, or control edit was introduced;
- assembler algorithm remains `10`.

Unity compilation and execution evidence are unavailable in the reconstruction environment. Compile-before-proof/install remains a hard gate.

---

## 2026-07-23 — GSU-M2.7C.5E.2.4B.2: Complete feature-response gating proof

**Status:** Source correction and static consistency/compliance audit complete in the declared five-file scope. Unity C#/ForwardLit compilation and the corrected algorithm-10 proof remain pending. The canonical installer remains blocked.

### Trigger and facts

The user-run `GeneratedMassSparseRiverbedAssemblyReport` for algorithm 10 was deterministic and passed the centre-anchor contract for all three candidates: owner mismatches, invalid samples, and inconsistent rocks were all zero; maximum reconstructed-centre error was `0.00033` tile UV; maximum retention spread was `0.00041`; and mip 3 was accepted. The only failures were `6`, `10`, and `11` pixels reported as `ungated visible pixels`.

Read-only source review proves two faults in that counter and the matching runtime gate:

1. `PixelSurfaceMaterialDetail.hlsl::PS3D_DecodeStylizedSurfaceDetail` reconstructs feature response from packed slope and cavity, while `PS3D_AssignStylizedSurfaceTextureForm` adds combined-versus-substrate form evidence. It does not include packed roughness, although `PS3D_ApplyStylizedSurfaceFeatureRetention` replaces rock roughness with the substrate-only roughness scalar. A rock pixel whose only surviving emitted response is roughness can therefore bypass whole-feature retention.
2. `GeneratedMassSparseRiverbedTileAssembler.cs::MeasurePalettePayload` compares the reconstructed feature mask against raw `final.Mask`. The runtime payload is emitted through separately filtered `silhouetteCoverage`, so the existing counter also classifies geometric source-mask fragments that were intentionally reduced to pure substrate response as ungated visible rock pixels.

The measured maximum retired substrate-roughness-field deviation is `0.00527`. Adding the maximum 8-bit half-step `0.00196` gives `0.00723`; therefore a `0.008` absolute packed-roughness difference is the conservative feature-evidence threshold.

### Objective and acceptance

Complete runtime feature-response gating and make the proof test the emitted payload rather than raw geometric occupancy. Acceptance requires:

- runtime feature-mask reconstruction includes slope, cavity, combined/substrate form, and packed-roughness difference from the feature substrate-roughness scalar;
- the roughness-evidence threshold is `0.008`;
- the proof uses the same encoded payload channels and thresholds as runtime;
- `ungated emitted-response pixels` is exactly zero for all three candidates;
- raw geometric-mask pixels that emit neutral substrate response are reported separately and are diagnostic only;
- algorithm version, centre-anchor B/A payload, placements, texture samples, IDs, arrays, metadata, installer contract, and 2.4A.2 composition remain unchanged.

### Approved scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
4. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
5. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`

Create/delete/move/rename: none. Proof outputs remain local under `Library`. No canonical asset installation is permitted until the corrected proof passes.

### Reviewed producers, consumers, and invariants

- `BuildPayloadSilhouetteCoverage`, `BuildRuntimePackedDetailPixel`, `EncodePalettePayload`, `MeasurePalettePayload`, and `CandidateResult` were reviewed as the editor producer/proof path.
- `PS3D_DecodeStylizedSurfaceDetail`, `PS3D_AssignStylizedSurfaceTextureForm`, and `PS3D_ApplyStylizedSurfaceFeatureRetention` were reviewed completely.
- Both Bank and Riverbed callers in `PixelSurfaceGroundForwardPass.hlsl` gate whole-feature retention with `result.featureMask`; no caller edit is required.
- `GeneratedGround.ApplySurfaceLayerDetailProperties` transports the feature substrate roughness in `DetailB.z`; no runtime C# or property-contract change is required.
- The supplied archive has no `.git`; branch, HEAD, status, and unrelated live changes remain unavailable.

### Implementation sequence

1. Add packed-roughness evidence to the HLSL feature mask, after resolving the feature substrate-roughness scalar.
2. Add matching named evidence thresholds and packed-channel reconstruction to `MeasurePalettePayload`.
3. Replace the ambiguous raw-mask failure counter with `ungated emitted-response pixels`; add a separate non-failing `geometric-mask pixels with neutral emitted response` diagnostic.
4. Update validator failure/report wording without changing algorithm or installer identity.
5. Update the River-coupled architecture, then run exact-scope, symbol, threshold-parity, delimiter/preprocessor, and payload/sample-count audits.

### Performance and risk

Runtime cost adds one absolute difference, one `step`, and one `max` in feature-texture-form detail decoding. No texture sample, branch, draw call, mesh stream, CBUFFER property, runtime allocation, or CPU update is added. Editor proof cost changes only by constant per-pixel scalar work. `PERFORMANCE EXCEPTION`: none.

Primary risk is a false-positive feature mask on substrate roughness quantization. The `0.008` threshold is above the measured `0.00723` worst-case substrate deviation-plus-quantization bound. Unity proof and scene validation remain mandatory.

### Validation gates

- C# and ForwardLit compile cleanly before proof execution.
- Algorithm-10 proof is deterministic and reports zero owner mismatches, invalid samples, inconsistent rocks, and ungated emitted-response pixels for all candidates.
- Neutral geometric-mask diagnostics do not fail the suite.
- The installer remains blocked until the proof verdict is `PASS`.
- Close, production-distance, repeated-tile, and mip validation remains pending after installation.

### Implementation and post-change audit result

- `PixelSurfaceMaterialDetail.hlsl` resolves the substrate-roughness scalar before feature evidence and unions `abs(packed A - substrate roughness) >= 0.008` with the existing slope/cavity evidence. Form evidence remains added by `PS3D_AssignStylizedSurfaceTextureForm`.
- `GeneratedMassSparseRiverbedTileAssembler.cs` reconstructs the encoded slope, cavity, form, and roughness evidence with named thresholds matching HLSL. It reports `ungated emitted-response pixels` as the hard failure and records raw geometric-mask pixels with neutral emitted response separately.
- `GeneratedMassSparseRiverbedTileAssemblyValidation.cs` retains the zero hard-failure gate and reports the neutral geometric count without failing it. Algorithm version `10` and report/installer identity compatibility are unchanged.
- Exact final scope is the five approved files. No file was created, deleted, moved, renamed, or generated.
- C#/HLSL delimiter checks and HLSL preprocessor balance passed. Producer/consumer symbol reconciliation passed. All four C# evidence thresholds match the HLSL thresholds.
- Exhaustive quantization sampling over the measured substrate range produced maximum encoded substrate roughness evidence `0.00627451`, below the `0.008` gate; the conservative analytical bound remains `0.00723078`.
- Ground ForwardPass sample-site counts remain four array samples and one ordinary texture sample. Branch attributes remain unchanged. No ID, metadata texture, search array, CBUFFER property, payload channel, placement, support-radius, or 2.4A.2 composition change was introduced.
- Unity compilation and the real proof cannot be run in the reconstruction workspace and remain pending. Do not run the installer unless the corrected report returns `VERDICT: PASS`.

---

## 2026-07-22 — GSU-M2.7C.5E.2.4B: Non-ID whole-rock boundary retention

**Status:** Source implementation and static/synthetic validation complete in the declared 18-file scope. Unity C#/FXC compilation, the algorithm-10 Unity proof, canonical installation, scene acceptance, and GPU measurement remain pending and are mandatory in that order.

### Trigger and proven limitation

`GSU-M2.7C.5E.2.4A.2` removed the Generated-Ground seam. The remaining defect is independent: the current feature-retention scalar is calculated from each fragment's own inward boundary distance, so one application boundary can give different retention values to different pixels of the same sparse-riverbed rock. Tuning the existing clearance/fade controls can move or soften that cut but cannot turn it into a rock-wide decision.

The rejected E2.3 family is not reused:

- no feature IDs or normalized IDs;
- no centre/radius arrays;
- no nearest-feature shader loop;
- no metadata texture;
- no additional texture sample.

### Objective and acceptance

Use the existing paired Palette Form sample to carry a stable, non-ID centre anchor. Every visible pixel of one rock must reconstruct the same centre and use the same conservative support radius, so the entire rock is retained, removed, or faded uniformly while substrate-only G response remains available.

Acceptance requires:

- Palette Form algorithm 10 uses `R=combined form`, `G=substrate-only form`, `B/A=signed centre-offset X/Y`, with B sRGB-encoded and A linear.
- B/A are normalized by one candidate-wide conservative support radius and remapped from `[-1,1]` to `[0,1]`.
- The conservative support radius is the largest circumscribed placement radius plus one final-payload texel. Smaller rocks may therefore disappear slightly earlier; a partial rock is not permitted.
- Substrate-only roughness moves from B to hidden scalar entry metadata. The same entry stores the conservative support radius in tile UV.
- Runtime reconstructs centre offset directly from B/A. It does **not** differentiate sampled feature data and does not derive radius from gradient magnitude.
- Only the existing corridor inward-distance field is differentiated to evaluate that distance at the reconstructed centre. Invalid Jacobians are conservatively suppressed.
- Proof requires zero visible-owner mismatches, zero invalid accepted samples, zero inconsistent rocks, bounded centre error/retention spread, and at least mip 3 accepted.
- ForwardLit must compile before proof or installation.
- Existing 2.4A.2 dry composition, wetness, cover, corridor UV3, geometry, draw calls, sample count, ordinary detail entries, fixed asset paths, GUIDs, and tuning remain unchanged.

### Approved and actual source scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
4. `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
5. `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
6. `Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs`
7. `Assets/Game/Procedural/Ground/GeneratedGround.cs`
8. `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
9. `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`
10. `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
11. `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
12. `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
13. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
14. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
15. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
16. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`
17. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
18. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`

Create/delete/move/rename project files: none. Generated proof PNGs/reports and installer-updated canonical assets remain user-run outputs.

### Read-only provenance and review

- `Assets/AGENTS.md` was read completely before editing.
- Current source was reconstructed from `Assets-Code-Archive(13).zip` plus accepted 2.4A, 2.4A.1, and 2.4A.2 packages. Archive SHA-256: `3ad18aa2d8132175b68dae3a18bd3c12c3684c04ee83d7dbe1cc7212d7a79336`.
- The supplied archive contains no `.git`; branch, HEAD, status, and unrelated live changes cannot be verified. Pre-edit hashes were captured for all 18 scoped files.
- Full producer/consumer review covered serialized controls, profile/library metadata, runtime vector packing, assembler generation/downsampling, proof reporting, installer rollback/in-place update, HLSL payload decode, and both Bank/Riverbed evaluators.

### Pre-delivery numerical design correction

The initially considered `B=radial distance` design required differentiating an 8-bit sRGB field and, in its first form, inverting gradient magnitude to recover radius. Standalone quantization tests produced unacceptable radius error. A later paired distance/radius version avoided radius inversion but still required derivatives of a sampled value whose texture evaluation is application-weight gated; that cannot guarantee coherent quad values at the exact boundary being fixed.

The delivered design therefore carries the centre offset directly:

```text
B_linear = offsetFromCentre.x / conservativeRadius * 0.5 + 0.5
A        = offsetFromCentre.y / conservativeRadius * 0.5 + 0.5
```

At runtime:

```text
offsetWorld = (float2(B, A) * 2 - 1) * conservativeRadiusWorld
centre      = fragmentPosition - offsetWorld
edge        = inwardDistanceAtCentre - conservativeRadiusWorld
```

This removes all derivatives of sampled feature data. The only derivative is the coherent interpolated inward-distance field used to translate its value from the current fragment to the reconstructed centre.

### Implemented source contract

1. **Assembler and payload**
   - Algorithm version advanced to `10`.
   - Each texel stores the toroidal signed offset from its nearest placement centre in B/A, normalized by the candidate-wide conservative support radius.
   - Visible raster ownership is checked against nearest-anchor ownership.
   - R/G and packed slope/cavity/roughness response remain unchanged.
   - Scalar metadata records substrate roughness mean and conservative support radius UV.

2. **Library/profile/runtime transport**
   - Hidden entry scalars are signature-participating and resolved by stable ID.
   - Ground layer/material profiles forward them.
   - Feature payloads reuse existing vector components: substrate roughness in `DetailB.z`, support radius UV in `DetailC.x`.
   - No ShaderLab or CBUFFER property was added.

3. **HLSL**
   - `PS3D_StylizedSurfaceDetail` carries normalized centre offset and support-radius metadata.
   - Existing combined/substrate form difference plus packed slope/cavity evidence gates visible rock response; offset length is used only as an anchor-validity guard.
   - Whole-feature retention evaluates centre inward distance, subtracts the candidate-wide support radius, then applies Feature Safety Margin and Whole Feature Return Fade.
   - Both existing Bank/Riverbed detail evaluators retain the FXC-safe unconditional call structure from 2.4A.1; texture sample sites are unchanged.

4. **Proof and installer**
   - Proof/report identity is `GSU-M2.7C.5E.2.4B`; installer identity is `GSU-M2.7C.5E.2.4B.1`.
   - Proof decodes the actual 8-bit sRGB/linear payload, validates visible-rock interiors through generated mips, and reports centre error, retention spread, owner mismatches, invalid samples, inconsistent rocks, and last accepted mip.
   - Installer accepts only a passing algorithm-10 report, parses both scalar metadata values, and updates existing canonical entries in place with rollback preserved.

### Static and synthetic evidence

Pending final post-change audit values are recorded in the delivery audit. Current completed checks include:

- C-like delimiter and HLSL preprocessor balance across all scoped C#/HLSL files;
- producer/consumer identifier reconciliation for the assembler proof fields;
- standalone 1024² 8-bit payload simulations with 12 toroidally separated anchors and generated mips;
- five-seed synthetic base-level maximum centre error approximately `0.00025` tile UV and retention-proxy spread approximately `0.00043` tile UV; mip-3 maxima were approximately `0.00075` and `0.00129` respectively;
- representative accepted mip levels remained below the source `0.01` tolerances, with zero reconstructed-owner mismatches in the tested actual-rock interiors. The source proof additionally samples four bilinear sub-texel positions per relevant cell and rejects any visible rock pixel lacking feature-response gating.

These are static/synthetic results only. They do not substitute for Unity compilation or the project proof.

### Cost and risks

- Runtime: scalar/vector ALU plus one 2×2 derivative solve for feature-aware Bank/Riverbed layers when Feature Safety Margin is nonzero.
- Added texture samples: zero.
- Added draw calls, mesh streams, runtime allocations, per-frame C# work, IDs, searches, arrays, or metadata textures: zero.
- Storage: two hidden scalar floats per feature-aware detail entry; B/A repack existing Palette Form channels.
- Candidate-wide support is deliberately larger than some individual rocks. This can remove a smaller rock earlier but cannot cut through it.
- Inward-distance extrapolation is locally affine per corridor triangle; Unity scene validation remains mandatory where a rock footprint crosses triangle boundaries.
- Exact GPU impact is unmeasured.
- `PERFORMANCE EXCEPTION`: none.

### Mandatory user gate

1. Apply over the accepted 2.4A + 2.4A.1 + 2.4A.2 source state.
2. Confirm C# and `PS3D/Pixel Ground Surface Lit - ForwardLit` compile before running any proof or installer.
3. Run the sparse-riverbed proof; require `GSU-M2.7C.5E.2.4B`, algorithm `10`, `VERDICT: PASS`, and zero owner mismatches/invalid samples/inconsistent rocks.
4. Run the fixed-path installer; require `GSU-M2.7C.5E.2.4B.1`, preserved GUIDs/tuning, and no numbered copies.
5. Validate hard removal and faded return at close and production cameras, including repeated tiles and distance/mip cases.
6. Compare GPU timing against the accepted pre-2.4B scene and return the complete report/error if any gate fails.

---

## 2026-07-22 — GSU-M2.7C.5E.2.4A.2: Normalized direct Bank–Riverbed handoff correction

**Status:** Implemented in the exact four-file scope and statically audited. Unity ForwardLit compilation and visual acceptance remain pending.

### Trigger and proven fault

The user applied `GSU-M2.7C.5E.2.4A.1`. Unity ForwardLit compilation recovered, but the narrow Generated-Ground-green line remained at the Bank/Riverbed boundary. Setting Shore Wet Highlight Strength to zero changed Bank specularity but did not remove the line. The wetness/highlight hypothesis is therefore rejected by direct scene evidence.

The current dry composition proves the remaining source:

- `PixelSurfaceGroundForwardPass.hlsl::ResolvePixelGroundSurfaceColor` adds ordinary Ground albedo only through `substrateWeights.x`.
- `PixelSurfaceGroundResponse.hlsl::ResolveGroundSubstrateCompositionWeights` currently assigns `primary = (1 - bank) * (1 - riverbed)`.
- The 2.4A.1 equivalent-surface path computes `shared = 1 - (1 - bank) * (1 - riverbed)` and then assigns `primary = 1 - shared`, which is algebraically the same residual Primary-Ground coefficient.
- `StylizedRiverCorridorGeometry.cs::ResolveRiverbedSupport` publishes `1` on the final BedSlope vertex and `0` on the first HiddenCover vertex. The continuous corridor triangles interpolate fractional Riverbed Support between those vertices.
- The active `grassland.clean_meadow` profile resolves Riverbed source `Inherit Bank Surface Layer`, uses matching dry multipliers, Bank Material Strength `1`, and Riverbed Material Strength `1`. The same-dry-surface path is therefore active in the reported scene.
- The canonical accepted V3S-A4A.1 architecture defines normalized Bank/Riverbed substrate weights: `secondaryCoverage = saturate(bank + riverbed)`, `primary = 1 - secondaryCoverage`, and Bank/Riverbed share that secondary coverage in their raw ratio. That accepted formula explicitly prevents an unrelated Primary-Ground strip when Bank and Riverbed overlap.

**Proven conclusion:** 2.4A/2.4A.1 retained the exact Primary-Ground leakage coefficient they were intended to remove. The line cannot be eliminated while the shared and different-surface paths retain product-based sequential coverage.

### Objective and acceptance

Restore the accepted normalized direct Bank–Riverbed handoff and keep the compile-safe branchless detail-evaluation shape.

Acceptance requires:

- Unity C# and `PS3D/Pixel Ground Surface Lit - ForwardLit` compile without shader-compiler-process loss.
- Primary Ground weight is exactly `1 - saturate(bank + riverbed)` for the dry substrate composition.
- When Bank plus Riverbed participation reaches or exceeds one, Primary Ground contribution is exactly zero.
- Different Bank and Riverbed materials transition directly in their raw Bank:Riverbed ratio; no third Primary-Ground band is introduced.
- Equivalent Bank/Riverbed dry surfaces collapse to one shared secondary coverage `saturate(bank + riverbed)` and use the Bank detail response only.
- Bank continuation evaluates the boundary lower-layer application with the active outer-extension contribution and boundary-relative Bank transition distance, rather than a weaker reconstructed subset.
- Wetness, Shore highlight, vegetation, snow, frost, Painted Accent exclusion, cover retention, UV3 production, River geometry, payloads, IDs, proof tooling, installers, and serialized assets remain unchanged.

### Approved correction scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
4. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`

Create/delete/move/rename/generate: none.

### Reviewed evidence and related-module audit

Read-only review covered:

- `Assets/AGENTS.md` completely;
- the current 2.4A.1 and 2.4A plan/architecture sections;
- the accepted V3S-A4A.1 normalized-composition section;
- complete current `PixelSurfaceGroundResponse.hlsl` and `PixelSurfaceGroundForwardPass.hlsl`;
- direct property/consumer context in `PixelSurfaceGroundMaterialProperties.hlsl` and `SH_PixelGroundSurfaceLit.shader`;
- Bank/Riverbed control resolution in `GroundMaterialControls.cs`, `GroundSurfaceLayerProfile.cs`, and `GeneratedGround.cs`;
- corridor support/distance production and continuous strip topology in `StylizedRiverCorridorGeometry.cs`;
- active `VisualFrameworkDemo.unity` river settings and `GSSP_Grassland.asset::grassland.clean_meadow` material controls;
- the user screenshots before and after Wet Highlight Strength was set to zero;
- the supplied archive against the applied 2.4A and 2.4A.1 patch deltas.

The supplied archive contains no `.git` directory. Branch, HEAD, history, status, and unrelated live working-tree changes are unavailable. No scene, profile, material, prefab, generated asset, or River source edit is authorized.

Cross-subsystem impact: `PixelSurfaceGroundResponse.hlsl` is also included by `SH_PixelSurfaceLit.shader`. The new boundary-distance helper compiles to zero when `PS3D_GROUND_HAS_RIVERBED_SUPPORT` is absent, the constant boundary-Shore helper is unreferenced there, and all changed composition/Bank consumers remain inside `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`. Generated Mass and ordinary Pixel Surface forward paths do not call the changed functions.

### Implementation sequence

1. Restore `ResolveGroundSubstrateCompositionWeights` to the accepted normalized formula: clamp raw Bank/Riverbed, sum them, saturate total secondary coverage, normalize the two secondary weights by their raw total, and assign Primary Ground the exact complement.
2. Add boundary-relative Bank helpers using the existing UV3 distances. At the Riverbed boundary, Bank inward distance is reconstructed as `bankInward - riverbedInward + bankOutward`; the active outer-extension boundary contribution is `step(extension) * strength` because outward distance is zero at that boundary.
3. Make `ResolveGroundBankEdgeMaterialBlend` use the boundary-relative transition distance and outer-extension boundary contribution while retaining the existing scalar-only control flow.
4. Replace the equivalent-surface bounded-union coverage with `saturate(resolvedBank + riverbed)`, matching the normalized composition's total secondary coverage.
5. Keep both Bank and Riverbed detail evaluators unconditional for FXC stability. Riverbed detail receives zero final dry weight in the equivalent-surface path.
6. Update the canonical River-coupled architecture and perform exact-scope, formula, caller/consumer, delimiter, symbol, and source-diff audits.

### Invariants, risks, and performance

- No new branch around texture sampling, texture sample, shader property, CBUFFER field, array, loop, ID, metadata texture, varying, geometry stream, draw call, allocation, CPU callback, generated asset, or per-frame work.
- The different-surface path changes only substrate ownership mathematics. Wetness and cover remain independent and use their current inputs.
- Normalized composition can increase Bank/Riverbed ownership where their combined partial weights previously left Primary Ground. This is the intended correction and restores the previously accepted A4A.1 contract.
- Boundary continuation adds only scalar arithmetic and reads existing UV3 components/material constants.
- `PERFORMANCE EXCEPTION`: none.
- Unity/FXC and final visual validation are unavailable here and remain pending after static audit.

### Validation gates

1. Exact four-file scope; no serialized/generated/River-source changes.
2. HLSL delimiter/preprocessor validation and changed-symbol call-site audit.
3. Formula checks prove weights are finite, non-negative, sum to one, and Primary Ground is zero when `bank + riverbed >= 1`.
4. Source scan confirms no material-controlled detail-sampling branch, new sample site, ID, array, search, or metadata path.
5. Unity ForwardLit compilation, then same-surface and different-surface boundary inspection.
6. Confirm wetness/highlight/cover behavior remains unchanged and return one complete screenshot or relevant compiler log if acceptance fails.

### Implementation result

- `ResolveGroundSubstrateCompositionWeights` now restores the accepted normalized A4A.1 ownership formula. Primary Ground is the exact complement of combined Bank/Riverbed coverage, and different secondary materials retain their raw participation ratio.
- `ResolveGroundRiverbedBoundaryBankInwardDistance` reconstructs the exact Bank application distance at the Riverbed boundary from the frozen UV3 distances.
- `ResolveGroundRiverbedBoundaryShoreMask` records the exact frozen corridor Shore-mask value at the BedSlope edge: `pow(0.52, 1.32) = 0.4218173`.
- `ResolveGroundOuterBankBoundaryContribution` restores the active outer-extension contribution at zero boundary distance.
- `ResolveGroundBankEdgeMaterialBlend` now evaluates the actual boundary Bank lower layer instead of using the current fragment Shore mask, current fragment inward distance, and a hard-coded zero outer contribution.
- The same-dry-surface path derives shared coverage from the normalized weight triplet and keeps both detail evaluators unconditional.

### Post-change consistency and compliance audit

- Actual persistent delta: **PASS — exactly the four approved files.** No scene, prefab, profile, material, generated asset, River source, C#, ShaderLab, CBUFFER, or texture file changed.
- HLSL delimiter and preprocessor balance: **PASS** for both modified includes.
- Changed-symbol producer/consumer counts: **PASS** — every new helper has one definition and one use; `ResolveGroundSubstrateCompositionWeights` has one definition and one fragment-path call.
- Formula validation: **PASS** across 100,000 randomized Bank/Riverbed inputs. Weights remain finite, non-negative, sum to one, and Primary Ground is exactly zero whenever clamped Bank plus Riverbed participation reaches one.
- Active-profile boundary calculation: **PASS by source model.** With the serialized `grassland.clean_meadow` and river values, the continued boundary Bank application is approximately `0.95488`; normalized Primary Ground remains zero throughout the fractional-support strip until it meets the ordinary Bank-side value at the outer vertex instead of forming the previous internal hump.
- Texture-sampling source-site count: **UNCHANGED** — Ground Response `1 → 1`; Ground ForwardPass `5 → 5` under the audit scan.
- `[branch]` attribute count: **UNCHANGED** — Ground Response `0 → 0`; Ground ForwardPass `6 → 6`. No branch was added around Bank/Riverbed detail sampling or structure assignment.
- `_CLUSTER_LIGHT_LOOP`, property bindings, exact UV3 production, payloads, feature retention, wetness, Shore highlight, cover retention, and debug paths: **UNCHANGED by exact source diff and caller/consumer review.**
- Shared-include impact: **PASS** — the boundary-distance helper compiles to zero without Riverbed support, the constant boundary-Shore helper is unreferenced outside Ground, and the changed composition/Bank consumers remain gated by `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`. Ordinary `SH_PixelSurfaceLit` does not call the changed path.
- Whitespace/error diff checks: **PASS** for both HLSL files.
- Unity 6000.5.0f1 C#/ForwardLit compilation and scene visual validation: **unavailable here and pending, not passed.**

---

## 2026-07-22 — GSU-M2.7C.5E.2.4A.1: FXC-safe branchless same-surface correction

**Status:** Unity ForwardLit compilation passed after this correction, but visual acceptance failed because the Generated-Ground-coloured line remained. Superseded by 2.4A.2.

### Trigger and diagnosis

The user applied `GSU-M2.7C.5E.2.4A` and Unity again reported:

```text
Shader compiler: Compile PS3D/Pixel Ground Surface Lit - ForwardLit, Fragment Program: Lost connection with shader compiler process. Suspected crash in FXC.
Shader error in 'PS3D/Pixel Ground Surface Lit': Lost connection with shader compiler process. Suspected crash in FXC.
```

The 2.4A delta introduced one material-controlled fragment branch around two large `PS3D_StylizedSurfaceDetail` evaluation paths. The baseline had evaluated Bank and Riverbed detail unconditionally. The new branch contained texture-array sampling, nested branches, large structure assignments, and mutually exclusive function-call graphs. No source line is emitted when FXC crashes, so the exact internal compiler fault is unavailable. The new branch is the only 2.4A construct that materially expands and restructures ForwardLit control flow; it is therefore the primary compile-crash suspect.

**Diagnosis classification:** Inference, high confidence. Verification is successful Unity ForwardLit compilation after removing only this control-flow restructuring while retaining the scalar continuity mathematics.

### Objective and acceptance

Restore the baseline branchless Bank/Riverbed detail-evaluation shape while preserving the complete Riverbed-domain Bank continuation and equivalent-surface bounded-union composition.

Acceptance requires:

- Unity C# and ForwardLit compile without shader-compiler-process loss.
- The complete-domain Bank continuation and `max` lower-layer correction remain active.
- Equivalent dry surfaces still use bounded-union substrate weights and do not expose Primary Ground at their internal boundary.
- The equivalent path contributes only the Bank detail result; Riverbed detail may still be evaluated but receives zero final dry weight.
- No feature ID, array, metadata texture, search, payload, corridor, control, profile, generated asset, or installer change is introduced.

### Approved correction scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`

Create/delete/move/rename/generate: none.

### Implementation contract

1. Remove the material-controlled `[branch] if` that conditionally invokes the Bank and Riverbed detail evaluators and assigns whole detail structures.
2. Always invoke the two existing detail evaluators, matching the rollback baseline's control-flow shape.
3. For equivalent dry surfaces, pass the bounded-union application weight to the Bank evaluator so the shared feature response is based on the full shared application.
4. Resolve ordinary sequential substrate weights and equivalent-surface shared weights separately, then select between the scalar weight triplets using `lerp` and the existing binary equivalence scalar.
5. Keep the Riverbed detail result at zero final dry weight in the equivalent case. This removes its internal-boundary contribution without a dynamic structure branch.
6. Defer duplicate Riverbed sample removal. Runtime texture-sample cost returns to the rollback baseline rather than attempting a compiler-risking optimization.

### Performance and validation

- Active fragment sample count is restored to the E2.2.1 rollback baseline: Bank and Riverbed detail evaluators remain present.
- Relative to failed 2.4A, the exact equivalent-surface path no longer skips Riverbed samples. Relative to the compiling rollback baseline, it adds only scalar domain, bounded-union, and weight-selection ALU.
- No draw call, texture memory, mesh stream, allocation, per-frame C# work, or generated asset changes.
- `PERFORMANCE EXCEPTION`: none; this correction abandons an unvalidated optimization and restores baseline sampling cost.
- Required validation: Unity ForwardLit compilation first, then same-surface and different-surface seam inspection.

### Post-change audit

- Actual delta matches the three-file correction scope.
- `PixelSurfaceGroundForwardPass.hlsl` contains no new same-surface `[branch] if` and no conditional whole-structure assignment.
- Both existing detail evaluators are called exactly once in the fragment path, matching the rollback baseline call count.
- Complete Riverbed application-domain continuation, `max` Bank correction, bounded-union shared application, independent wetness, and independent cover paths remain present.
- Texture-sampling source sites, shader properties, CBUFFER bindings, C# property transport, UV3 contract, feature payload, proof tooling, and installer tooling are unchanged from 2.4A.
- Unity evidence: **ForwardLit compilation passed; visual acceptance failed because the green line remained.** The product-based shared coverage is superseded by 2.4A.2 normalized composition.

---

## 2026-07-22 — GSU-M2.7C.5E.2.4A: Bank/Riverbed dry-surface continuity

**Status:** ForwardLit compilation failed because the original same-surface texture-sampling branch crashed FXC. Superseded first by 2.4A.1 for compilation and then by 2.4A.2 for the unresolved visual seam.

### Objective

Remove the narrow Primary-Ground-coloured line at the internal Bank-to-Riverbed boundary. Keep the resolved Bank dry application available as Riverbed's lower layer throughout the complete Riverbed application domain. When Bank and Riverbed resolve the same dry layer with equivalent dry-detail multipliers, treat them as one continuous dry application and evaluate the shared detail once.

### Acceptance criteria

- Unity C# and `PS3D/Pixel Ground Surface Lit - ForwardLit` compile without error or shader-compiler-process loss.
- Primary Ground does not appear solely because Bank hands off to Riverbed.
- Different Bank and Riverbed dry layers continue to use direct sequential Primary Ground → Bank → Riverbed composition.
- Equivalent Bank/Riverbed dry applications use one shared dry-detail result and one bounded-union application weight; the patch must not replace the existing sequential combined weight with `max`.
- Bank continuation fills a missing lower layer and therefore combines with the ordinary Bank application using `max`, not addition.
- Shore hydrology, Riverbed hydrology, exact Riverbed Support, submerged-cover exclusion, snow, frost, vegetation, Painted Accents, corridor geometry, and the accepted `TEXCOORD3` contract remain unchanged.
- No feature ID, feature-layout array, metadata texture, nearest-feature search, payload change, proof run, installer run, draw call, mesh stream, runtime allocation, or per-frame C# work is introduced.
- Only the seven approved files below change.

### Reviewed evidence

- User screenshot `/mnt/data/67755aa8-7a9b-4ae7-b39b-2bae15c43b44.png` shows a narrow green line between the grey-blue Riverbed application and the surrounding Bank surface.
- The user confirmed that E2.3.3 restored project compilation. The preceding E2.3/E2.3.1/E2.3.2 whole-feature array/ID implementations remain rejected historical experiments.
- `PixelSurfaceGroundResponse.hlsl::ResolveGroundRiverBankDomain` multiplies Bank authorization by `(1 - riverbedSupport)`.
- `PixelSurfaceGroundForwardPass.hlsl::Frag` restores Bank below Riverbed with `ResolveGroundBankEdgeMaterialBlend(input) * riverbedSupport`; fractional support can therefore leave both Bank and Riverbed below full participation.
- `PixelSurfaceGroundResponse.hlsl::ResolveGroundSubstrateCompositionWeights` assigns Primary Ground `(1 - bank) * (1 - riverbed)`, so any simultaneous Bank and Riverbed deficit exposes Primary Ground.
- `StylizedRiverCorridorGeometry.cs` already publishes `TEXCOORD3.x = Riverbed Support`, `.z = Bank inward distance`, and `.w = Riverbed inward distance`; no producer change is required.
- `ResolveGroundBoundedUnion` already implements the combined contribution of two identical sequential layers: `1 - (1 - bank) * (1 - riverbed)`.
- `GeneratedGround.cs::ApplySurfaceProfileMaterialProperties` resolves both dry layers and all application multipliers during the existing material refresh, so same-dry-surface equivalence can be calculated without per-frame work.
- The supplied archive contains no `.git` directory. Branch, `HEAD`, status, history, and unrelated live working-tree changes are unavailable. The supplied source is treated as the current E2.3.3 rollback baseline.

### Approved file scope

Modify only:

1. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
2. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
3. `Assets/Game/Procedural/Ground/GeneratedGround.cs`
4. `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`
5. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
6. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
7. `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`

Create/delete/move/rename/generate: none.

### Implementation contract

1. Add one material-refresh-time scalar, `_GroundBankRiverbedSameDrySurface`. It is true only when both layers resolve to the same non-null `GroundSurfaceLayerProfile` and all eight dry-detail application multipliers are equivalent. Material strengths, application transitions, feature-edge controls, cover retention, and hydrology are not part of dry-detail equivalence.
2. Add `ResolveGroundRiverbedApplicationDomain`, derived only from existing River-coupled authorization plus positive Riverbed Support or positive Riverbed inward distance.
3. Replace fractional-support Bank continuation with complete-domain continuation. Combine continuation with ordinary Bank application using `max` so the lower layer is filled but never double-counted.
4. Preserve the ordinary different-surface path exactly: resolve Bank detail, resolve Riverbed detail, and use `ResolveGroundSubstrateCompositionWeights`.
5. For equivalent dry applications, calculate `sharedApplication = ResolveGroundBoundedUnion(resolvedBankApplication, riverbedApplication)`, evaluate Bank detail once using `sharedApplication`, skip Riverbed detail sampling and Riverbed feature-edge suppression, and use `(1 - sharedApplication, sharedApplication, 0)` as substrate weights.
6. Leave wetness and cover paths independent and unchanged.

### File-by-file implementation sequence

1. **Complete:** read-only review of the supplied handoff, rollback plan, River-coupled architecture, seven expected files, direct Ground control/profile producers, corridor UV3 producer, shader properties, CBUFFER, response helpers, and ForwardLit consumers.
2. **Complete:** record this concrete plan as the first project write.
3. **Complete:** added the uniform property and conservative C# equivalence calculation.
4. **Complete:** added the complete Riverbed application-domain helper.
5. **Complete:** corrected lower-layer continuation and added the equivalent-surface shared-detail branch.
6. **Complete:** updated the canonical River-coupled architecture.
7. **Complete for available checks:** exact-scope, property-binding, source, shader, sample-site, and cross-subsystem audits passed; Unity validation remains pending.

### Performance and resource contract

- C#: one constant-time comparison during existing material-property refresh and one scalar property write. No allocation and no per-frame update.
- Different-surface fragment path: one domain test and a small number of scalar operations; texture sample count remains unchanged.
- Equivalent-surface fragment path: the Riverbed packed-detail and optional Palette Form samples are skipped; the shared Bank result is reused.
- No draw-call, renderer, mesh, texture-memory, generated-asset, dispatch, or storage architecture change.
- `PERFORMANCE EXCEPTION`: none.

### Implementation result and post-change audit

Actual project-file delta modifies exactly the seven approved existing files. No file was added, deleted, moved, renamed, generated, or raw-edited as a serialized Unity asset.

Implemented behavior:

- `GeneratedGround.cs` writes `_GroundBankRiverbedSameDrySurface` during the existing material-property refresh. The flag requires the same non-null Ground surface-layer asset and equality, within `0.0001`, of the eight Bank/Riverbed dry-detail multipliers.
- `ResolveGroundRiverbedApplicationDomain` uses existing River-coupled authorization plus positive Riverbed Support or Riverbed inward distance. No corridor producer or stream meaning changed.
- Bank-under-Riverbed continuation now uses the complete application domain and combines with ordinary Bank application through `max`; the former fractional-support multiplication and additive combination are removed.
- Different dry layers retain the existing sequential composition and both existing detail evaluators.
- Equivalent dry layers preserve the sequential combined contribution through `ResolveGroundBoundedUnion`, evaluate Bank detail once at the shared weight, do not call the Riverbed detail evaluator, and use a Primary/shared/zero weight triplet.
- Wetness, cover, support, debug, geometry, payload, proof, installer, and feature-retention source outside the same-surface Riverbed skip remain unchanged.

Available checks passed:

- exact tree comparison reports only the seven approved files changed;
- changed C#/HLSL/ShaderLab delimiters are balanced and HLSL/ShaderLab preprocessor nesting is balanced;
- Pygments C# lexical scanning reports zero error tokens in `GeneratedGround.cs`;
- the new property is present in the C# ID/write path, ShaderLab property block, material CBUFFER, and HLSL consumer;
- `_CLUSTER_LIGHT_LOOP` remains present and `_FORWARD_PLUS` remains absent;
- source texture-sampling sites remain four texture-array sites and one ordinary texture site, matching the rollback baseline; the equivalent-surface source branch omits the Riverbed evaluator call;
- no feature ID, feature-layout array, metadata texture, nearest-feature loop, generated-asset contract, corridor file, control file, profile file, or Inspector file was added or changed;
- conflict-marker, trailing-whitespace, and exact-scope scans passed.

Cross-subsystem audit: the shader is shared by ordinary Ground and River corridor renderers. The new property defaults to zero and is always written by `GeneratedGround`; the Riverbed application domain remains gated by `_GroundRiverCoupledEnabled`, so ordinary Ground remains unaffected. `StylizedRiver` continues to refresh the corridor property block through the existing `GroundSurfaceRenderRole.RiverCorridor` calls.

### Risks and validation limits

- Same-dry-surface detection is deliberately conservative: separate layer assets that happen to resolve identical shader values are not merged in this patch. This cannot create a false merge; it can only miss the sample-reuse optimization.
- No Unity executable, C# project compiler, or HLSL/FXC compiler is available in this environment. Unity C# compilation, ForwardLit compilation, actual branch/sample behavior, GPU timing, and visual seam removal remain user-project validation gates.
- If ForwardLit loses the shader compiler process, revert only this seven-file update and return the complete Console error plus the relevant `Editor.log` section.

---

## 2026-07-22 — GSU-M2.7C.5E.2.3.3: Emergency rollback to the last compiling feature-aware baseline

**Status:** Implemented, statically audited, and user-confirmed compiling after rollback.

### Objective

Restore project and ForwardLit compilation immediately by removing the unvalidated E2.3/E2.3.1/E2.3.2 whole-feature metadata architecture and returning the affected Ground/sparse-riverbed files to the last user-confirmed compiling E2.2.1 implementation. Retain only Unity's required `_CLUSTER_LIGHT_LOOP` keyword replacement.

### Acceptance criteria

- `GeneratedGround.cs` and the Ground editor compile against the restored E2.2.1 contracts.
- `PS3D/Pixel Ground Surface Lit` no longer contains feature-ID decoding, feature metadata arrays, whole-feature centre reconstruction, derivative-based feature lookup, or E2.3 constant-buffer additions.
- The sparse-riverbed proof/installer return to E2.2 / algorithm 7 contracts and no longer require feature-layout metadata.
- Existing E2.2.1 feature-aware substrate/rock suppression remains available with `Discrete Feature Edge Clearance` and `Discrete Feature Return Fade`.
- The shader declares `_CLUSTER_LIGHT_LOOP`, not deprecated `_FORWARD_PLUS`.
- No scene, prefab, material asset, layer, tag, renderer, draw call, mesh stream, or unrelated subsystem is changed.

### Reviewed evidence

- User Console evidence: E2.3 and E2.3.1 repeatedly crash the FXC shader compiler process while compiling `PS3D/Pixel Ground Surface Lit - ForwardLit`.
- `GeneratedMassSparseRiverbedAssemblyReport(1).txt`: E2.3.2 algorithm 9 fails feature-ID reconstruction for Very Sparse (`0.41375`) and Sparse (`0.49202`, decoded maximum `13`). The proof therefore blocks installation.
- `PixelSurfaceMaterialDetail.hlsl::PS3D_AssignStylizedSurfaceTextureForm` in E2.3.2 divides an 8-bit sRGB-encoded premultiplied feature ID by an independently quantized 8-bit alpha mask. The report demonstrates that this contract is not reconstruction-safe.
- E2.2.1 is the last supplied implementation that compiled and ran in the user's project. Its limitations are visual, not project-breaking.
- Git metadata is absent from the supplied archive. Live branch, `HEAD`, status, and unrelated working-tree changes cannot be inspected. Baselines are reconstructed from `Assets-Code-Archive(9).zip` and the accepted E2.1 → E2.2 → E2.2.1 patch sequence.

### Approved file scope

- Four canonical Ground documents.
- `GeneratedGround.cs`, `GroundMaterialControls.cs`, `GroundSurfaceLayerProfile.cs`, and `GeneratedGroundEditor.cs`.
- Ground surface ShaderLab/HLSL files changed by E2.3–E2.3.2.
- Sparse-riverbed assembler, proof validator, installer, detail-library builder/validation, detail-library profile, and material profile changed by E2.3–E2.3.2.

### Implementation sequence

1. **Complete:** record the emergency rollback plan before implementation edits.
2. **Complete:** restored all affected runtime/editor contracts exactly to E2.2.1.
3. **Complete:** retained `_CLUSTER_LIGHT_LOOP` as the sole intentional delta from E2.2.1 ShaderLab.
4. **Complete:** removed all E2.3/E2.3.1/E2.3.2 feature metadata and whole-feature symbols.
5. **Complete:** exact-scope, symbol, lexical, delimiter, preprocessor, property-binding, sample-count, and package-reapplication checks passed.
6. **Complete:** post-change consistency/compliance audit recorded; the user subsequently confirmed that Unity C# and shader compilation recovered.

### Invariants and non-goals

- This is a stability rollback, not another attempt to solve whole-rock edge culling.
- Do not preserve E2.3 Bank–Riverbed continuity code if doing so retains any dependency on the broken E2.3 architecture.
- Do not modify generated assets or require an installer run to recover compilation.
- Do not claim that the original edge-rock or green-line visual defects are solved.

### Implementation result and audit

- Final delta is exactly 20 existing files: four canonical documents and sixteen Ground/PixelSurface implementation files. No file was added, deleted, moved, or renamed inside `Assets`.
- Nineteen implementation/document files are byte-identical to reconstructed E2.2.1. `SH_PixelGroundSurfaceLit.shader` differs from E2.2.1 only by `_FORWARD_PLUS` → `_CLUSTER_LIGHT_LOOP`.
- All E2.3/E2.3.1/E2.3.2 feature metadata arrays, feature-ID decode, whole-feature centre reconstruction, derivative lookup, profile layout transport, and algorithm-8/9 installer contracts are absent.
- E2.2 algorithm version `7`, E2.2 proof identity, E2.2.1 installer reporting/rollback, generic application blending, and per-pixel feature-aware substrate replacement are restored.
- Changed C# files produced zero Pygments error tokens. Changed C#/HLSL/ShaderLab delimiter and HLSL/ShaderLab preprocessor checks passed.
- Ground ForwardLit texture-sample counts match E2.2.1 exactly. No draw-call, mesh-stream, texture-memory, or runtime-CPU architecture change is introduced by this rollback.
- A clean reapplication of the packaged delta to the reconstructed E2.3.2 baseline matched the audited work tree exactly.
- Unity compilation was unavailable during package construction; the user subsequently confirmed that the rollback compiles.

## 2026-07-22 — GSU-M2.7C.5E.2.3.2: Direct Feature-ID Whole-Rock Culling and FXC Compile Restoration

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Palette Form feature-ID payload: **complete for the approved source change**.
- Feature metadata texture-array build and resolution: **complete for the approved source change**.
- Runtime direct metadata lookup: **complete for the approved source change**.
- Installer in-place metadata refresh: **complete for the approved source change**.
- Proof/report and validation updates: **complete for the approved source change**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity compilation remains pending**.
- Unity C# and ForwardLit compilation: **pending in Unity 6000.5.0f1**.
- Runtime whole-feature and layer-continuity validation: **pending after successful compilation**.

### Objective

Restore project compilation by removing the FXC-hostile twelve-feature nearest-centre search and its two `float4[12]` material constant arrays from the Ground ForwardLit fragment path. Preserve complete-rock boundary retention, Bank-under-Riverbed continuity, same-surface continuation, the existing installed candidate assets, and the installer’s canonical in-place refresh contract. Replace fragment-time feature search with one direct feature-ID lookup into a tiny generated metadata texture array.

### Observed failure and reviewed evidence

- Unity `6000.5.0f1` repeatedly reports `Lost connection with shader compiler process. Suspected crash in FXC` while compiling `PS3D/Pixel Ground Surface Lit - ForwardLit` after M2.7C.5E.2.3 and again after M2.7C.5E.2.3.1.
- M2.7C.5E.2.3.1 already removed runtime loops, `break`, and variable indexing, but the shader still crashes. The surviving new fragment path consists of twenty-four explicit feature comparisons, two twelve-entry material constant arrays, derivative-based centre-distance reconstruction, and expanded ForwardLit source.
- `PixelSurfaceGroundResponse.hlsl`: `ResolveGroundBankWholeFeatureRetention` and `ResolveGroundRiverbedWholeFeatureRetention` still perform twelve candidate checks each before calling the valid common `ResolveGroundWholeFeatureRetention` calculation.
- `PixelSurfaceGroundForwardPass.hlsl`: Bank and Riverbed already sample a feature-aware Palette Form. Its `B` channel currently carries substrate roughness while its `A` channel carries feature coverage.
- `PixelSurfaceMaterialDetail.hlsl`: feature-aware payload decode currently resolves combined form from `R`, substrate form from `G`, substrate roughness from `B`, and feature coverage from `A`.
- `GeneratedMassSparseRiverbedTileAssembler.cs`: exact rock ownership is already available per output texel through `FinalBuffers.Owner`, and exact deterministic centre/radius metadata is already produced in `CandidateResult.FeatureLayout`.
- `StylizedSurfaceDetailLibraryBuilder.cs`: the generated packed and Palette Form arrays are library-owned sub-assets. This is the correct ownership location for one additional tiny metadata array aligned to the existing texture-form slice mapping.
- `GeneratedMassSparseRiverbedSurfaceInstaller.cs`: already parses the machine-readable feature layout and updates the three canonical assets in place while preserving GUIDs and authored material/layer tuning.
- `GeneratedGround.cs`: currently uploads two twelve-entry vector arrays through a `MaterialPropertyBlock`; this upload and the corresponding constant-buffer arrays are the path to retire.
- `GeneratedGroundEditor(6).cs` is the authoritative editor baseline. This patch does not modify it.
- Git metadata is absent from the supplied archives, so live branch, `HEAD`, status, and unrelated working-tree changes cannot be inspected. The working baseline was reconstructed from `Assets-Code-Archive(9).zip` plus the accepted D3→E2.3.1 patch sequence.

### Approved file scope

Modify only:

- `Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create/delete/move/rename inside `Assets`: none.

### Quantized feature-ID validation update

Static byte-quantization simulation of the approved premultiplied ID encoding found that 8-bit sRGB `B` plus 8-bit linear `A` can produce a maximum pre-round reconstruction error of approximately `0.21` for the validator's `A > 0.02` samples. The reconstructed value still rounds to the correct integer feature ID because the error remains below `0.5`. The acceptance ceiling is therefore `0.25`, not the originally planned `0.08`; decoded IDs must still be exactly bounded to `1–placementCount`. This changes only validation tolerance for quantization and does not change runtime decoding, payload bytes, metadata selection, or feature identity.

### Direct feature-ID payload contract

For feature-aware Palette Form entries:

- `R`: complete substrate-plus-feature form, sRGB encoded;
- `G`: substrate-only form, sRGB encoded;
- `B`: normalized feature ID multiplied by feature coverage, sRGB encoded;
- `A`: linear discrete-feature coverage.

Feature ID `0` means substrate. Exact generated rocks use stable IDs `1–12` matching placement order. Runtime decodes `B / A`, rounds to the nearest valid ID, and directly selects one metadata texel. Premultiplying ID by coverage preserves the ID through bilinear edge interpolation against substrate.

### Metadata-array contract

- Add one generated linear `RGBAHalf` metadata `Texture2DArray` to `StylizedSurfaceDetailLibrary`.
- Width is `12`, height is `1`, no mipmaps, point filtering, clamp wrapping.
- Depth matches the existing texture-form array depth so the existing texture-form slice index also selects the metadata slice.
- Each texel stores `(centreU, centreV, supportRadius, substrateRoughnessMean)` for one exact feature ID.
- Non-feature texture-form slices contain zero centre/radius and neutral substrate roughness.
- Source feature layouts remain Editor-only entry data and participate in the library signature. Runtime materials and Ground components do not serialize or upload twelve-entry feature arrays.

### Runtime contract

- Remove `_GroundBankFeatureLayout[12]` and `_GroundRiverbedFeatureLayout[12]` from `UnityPerMaterial`.
- Remove the twenty-four nearest-feature comparisons and all search helpers.
- Bind Bank and Riverbed metadata arrays through the existing material-property refresh path.
- On relevant feature pixels only, point-sample one metadata texel using the decoded feature ID and existing texture-form slice.
- Reuse the existing derivative-based complete-feature retention equation with the directly selected centre/radius.
- Feed metadata `W` into feature retention as substrate roughness, replacing the old Palette Form `B` use.
- Preserve Bank-under-Riverbed composition and equivalent-surface reuse from E2.3.
- Preserve `_CLUSTER_LIGHT_LOOP` from E2.3.1.

### Performance and resource contract

- Remove twenty-four feature-centre comparisons and 384 bytes of material constant-array data.
- Add at most one point-sampled metadata-array read on feature pixels when whole-feature transition is enabled.
- Add one tiny generated metadata array: `12 × 1 × texture-form-depth × RGBAHalf`; for the three installed candidates this is 288 bytes before object overhead.
- No draw calls, mesh streams, ordinary surface texture samples, texture-array slices, per-frame CPU processing, or runtime allocations may be added.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the compile failure, reviewed paths, approved scope, direct-ID payload, metadata-array ownership, runtime contract, performance contract, risks, and acceptance criteria.
2. **Complete — proof payload:** advanced the algorithm version, encoded stable feature IDs in Palette Form `B`, preserved `R/G/A`, and updated payload metrics/fingerprints/report validation.
3. **Complete — library:** added Editor-only entry feature layout, generated metadata array ownership/resolution, signature participation, metadata build/validation, and stale-array detection.
4. **Complete — installer:** moved parsed layout ownership from material profiles to canonical library entries, required the new proof version, retained in-place rebuilding/GUID preservation, and verified metadata resolution.
5. **Complete — runtime:** removed constant feature arrays/uploads/searches, bound metadata arrays, decoded feature ID, point-sampled one metadata texel, and reused complete-feature retention directly.
6. **Complete — docs:** recorded the superseding compile-safe architecture and unchanged Inspector controls.
7. **Complete for available checks — final audit:** compared exact scope, reread modified files and direct contracts, verified no editor-baseline drift, ran lexical/structural/property/sample-count/package checks, and recorded pending Unity compilation.

### Acceptance criteria

- Unity ForwardLit compiles without losing the FXC process.
- No `float4[12]` feature-layout arrays or twelve-entry feature searches remain in the Ground shader path.
- Exact feature IDs are deterministic, bounded to each candidate’s exact placement count, and stable across repeated proof runs.
- Every installed feature-aware entry resolves a metadata slice aligned with its Palette Form slice.
- Whole-rock retention uses the directly selected feature centre/radius and retains the existing complete-feature behavior.
- Bank-under-Riverbed continuity and identical-surface continuation remain unchanged.
- Existing canonical asset paths and GUIDs remain stable; material and layer tuning is preserved.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual project-file delta against the reconstructed accepted M2.7C.5E.2.3.1 baseline is exactly the eighteen files in the approved scope. No file was added, deleted, moved, or renamed inside `Assets`.

Implemented behavior:

- assembler algorithm version advanced to `9` and Palette Form `B` now carries premultiplied deterministic feature ID while `R/G/A` preserve combined form, substrate form, and feature coverage;
- proof validation requires decoded IDs `1–placementCount`, a maximum pre-round error of `0.25`, deterministic feature layouts, and unchanged paired-payload/channel contracts;
- source centre/radius layouts moved from runtime material-profile arrays to Editor-only library-entry data;
- library rebuilding creates one generated `12 × 1 × texture-form-depth` linear `RGBAHalf` metadata array with no mipmaps, point filtering, and clamp wrapping, aligned to the Palette Form slice mapping;
- installer requires a passing `GSU-M2.7C.5E.2.3.2`, algorithm-version-9 proof, updates the three canonical entries/assets in place, preserves existing profile/layer tuning and GUID behavior, and verifies aligned metadata resolution;
- `GeneratedGround` no longer allocates or uploads two twelve-entry feature arrays; it binds only the generated Bank/Riverbed metadata arrays and readiness/equivalence flags;
- ForwardLit no longer contains the two feature arrays, loops, variable indexing, unrolled twenty-four-entry search, or nearest-feature helpers. Relevant feature pixels decode one ID and point-sample one metadata texel before reusing the accepted complete-feature retention equation;
- E2.3 Bank-under-Riverbed continuity, same-effective-surface reuse, application controls, and `_CLUSTER_LIGHT_LOOP` remain intact.

Available static checks passed:

- full tree comparison reports exactly the eighteen approved modifications;
- all nine changed C# files produce zero Pygments C# error tokens and pass string/comment-aware delimiter checks;
- all changed HLSL/ShaderLab files pass delimiter and preprocessor-balance checks;
- obsolete feature-array/search symbols are absent from active code and no feature loop or `break` remains in the Ground forward pass;
- ForwardLit ordinary surface sampling remains four detail/form array reads plus one ordinary texture read; the new source contains two conditional metadata sample sites, one for Bank and one for Riverbed, with same-surface reuse still bypassing the duplicate Riverbed detail path;
- C# shader property IDs, ShaderLab metadata-array properties, HLSL texture/sampler declarations, generated-array ownership, and stable-ID resolution paths are present and cross-referenced;
- independent 8-bit sRGB/alpha quantization simulation across feature IDs `1–12` and every nonzero alpha byte reconstructs the correct rounded ID; the maximum error for validated `A > 0.02` samples is approximately `0.2003`, below the `0.25` gate;
- the three-candidate metadata payload is `12 × 1 × 3 × 8 = 288` bytes before Unity object overhead;
- `GeneratedGroundEditor.cs`, `GroundMaterialControls.cs`, `StylizedRiverCorridorGeometry.cs`, and frozen Generated Mass projection files are byte-identical to the reconstructed E2.3.1 baseline.

No local Unity Editor, C# compiler, FXC, or DXC executable is available. The project-compilation restoration, actual proof output, AssetDatabase metadata-array rebuild, installer GUID preservation, runtime whole-rock retention, layer continuity, and GPU timing remain pending in Unity `6000.5.0f1`.

### Risks and validation limits

- Bilinear/mip interpolation can mix feature ID with substrate at silhouettes. Premultiplying normalized ID by feature coverage and decoding by `B/A` is selected to preserve identity at those edges; proof validation must measure ID reconstruction failures.
- At the lowest mips, separate sparse features could theoretically share a footprint. The retention path is relevant only when sampled feature coverage is nonzero near an application transition; visual and target-GPU validation remain required.
- The metadata-array read adds one dependent sample on relevant feature pixels. Its measured GPU cost remains pending.
- No Unity Editor or FXC process is available here. Static checks cannot prove that the external compiler process no longer crashes.

---

## 2026-07-22 — GSU-M2.7C.5E.2.3.1: Compile correction — profile namespace, FXC-safe feature search, clustered-light keyword

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- C# namespace correction: **complete**.
- FXC-safe whole-feature search correction: **complete**.
- URP clustered-light keyword correction: **complete**.
- Static consistency/compliance audit: **complete for available checks**.
- Unity C# and shader compilation: **pending in Unity 6000.5.0f1**.
- Runtime whole-feature and layer-continuity validation: **pending after successful compilation**.

### Objective

Correct the immediate compile failures introduced by M2.7C.5E.2.3 without changing its whole-feature layout data, runtime surface behavior, texture-sample count, installed assets, or Inspector controls.

Observed Unity evidence:

- `GeneratedGround.cs(786,17)` and `(789,17)` report that `StylizedSurfaceMaterialProfile` does not exist in the current context.
- `PS3D/Pixel Ground Surface Lit`, ForwardLit fragment compilation repeatedly loses the FXC shader-compiler process.
- Unity warns that `_FORWARD_PLUS` is deprecated and requests `_CLUSTER_LIGHT_LOOP`.

### Reviewed implementation and findings

- `Assets/Game/Procedural/Ground/GeneratedGround.cs`: M2.7C.5E.2.3 allocates the two twelve-entry feature-layout staging arrays through `StylizedSurfaceMaterialProfile.MaximumDiscreteFeatureCount`, but the file imports `ProgrammaticStylized3D.Geometry` and `ProgrammaticStylized3D.Rivers` only. The profile type is declared in `ProgrammaticStylized3D.Rendering.PixelSurface`; the missing namespace import directly explains both CS0103 errors.
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`: the M2.7C.5E.2.3 Bank and Riverbed nearest-feature searches use runtime loop counts, `break`, and variable indexing into material constant arrays inside the ForwardLit fragment path. Unity reports an FXC process crash rather than an HLSL syntax diagnostic. **Inference — high confidence:** this new dynamic loop/indexing shape is the compiler-crash trigger because it is the only newly introduced high-complexity FXC construct in the failing whole-feature path. Verification requires Unity shader compilation after replacing it.
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`: callers require the same nearest-feature result and need no behavior change.
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`: the fixed maximum remains twelve and the two `float4[12]` constant arrays remain valid; no property contract change is needed.
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`: ForwardLit still declares `#pragma multi_compile _ _FORWARD_PLUS`. The user-provided Unity warning explicitly identifies this keyword as deprecated and requests `_CLUSTER_LIGHT_LOOP`.
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`: declares `MaximumDiscreteFeatureCount = 12` in namespace `ProgrammaticStylized3D.Rendering.PixelSurface`; no profile change is required.
- Git metadata is unavailable in the supplied archives, so branch, `HEAD`, status, history, and unrelated live working-tree changes cannot be inspected here.

### Approved correction scope

Modify only:

- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`

Create/delete/move/rename inside `Assets`: none.

### Implementation contract

1. Add the missing `ProgrammaticStylized3D.Rendering.PixelSurface` namespace import to `GeneratedGround.cs`. Do not alter feature-layout counts, buffers, property IDs, uploads, or material refresh behavior.
2. Preserve the existing two twelve-entry material constant arrays and all whole-feature mathematics.
3. Replace each runtime loop and variable array index with twelve explicit constant-index, uniform-count-gated checks. Use one small helper to compare a candidate against the current nearest feature. This preserves the nearest-centre result while avoiding the FXC-hostile loop/indexing form.
4. Do not add texture samples, draw calls, mesh streams, buffers, keywords, runtime allocations, or per-frame CPU work.
5. Replace only the deprecated ForwardLit keyword declaration `_FORWARD_PLUS` with `_CLUSTER_LIGHT_LOOP`, as requested by Unity. Do not change lighting formulas or other shader variants.

### Acceptance criteria

- The two CS0103 errors are eliminated.
- ForwardLit compiles without losing the shader compiler process.
- The `_FORWARD_PLUS` deprecation warning no longer originates from `SH_PixelGroundSurfaceLit.shader`.
- Bank and Riverbed still select the nearest one of at most twelve deterministic feature entries and use the unchanged centre/radius retention formula.
- Texture sample count, runtime texture memory, draw calls, serialized data, and surface assets remain unchanged.
- No file outside the approved correction scope changes.

### Post-change audit evidence

- `GeneratedGround.cs` now imports `ProgrammaticStylized3D.Rendering.PixelSurface`; both staging arrays still use `StylizedSurfaceMaterialProfile.MaximumDiscreteFeatureCount` and remain twelve entries.
- `PixelSurfaceGroundResponse.hlsl` retains `ResolveGroundWholeFeatureRetention` unchanged. The Bank and Riverbed selectors now use explicit constant indices `0–11`, gated by the uniform feature count, and contain no feature-search loop, `break`, or variable array index.
- `SH_PixelGroundSurfaceLit.shader` changes only the ForwardLit clustered-light variant keyword from `_FORWARD_PLUS` to `_CLUSTER_LIGHT_LOOP`.
- Static delimiter, preprocessor, symbol-reference, constant-index coverage, scope-diff, and package-reapplication checks passed.
- Unity compilation remains pending. The FXC-crash diagnosis is not considered verified until Unity compiles the ForwardLit pass successfully.

### Risks and validation limits

- Explicit constant-index checks increase generated HLSL source size slightly but preserve the same maximum of twelve feature comparisons and avoid dynamic constant-array indexing.
- The `_CLUSTER_LIGHT_LOOP` keyword change is project-version-specific to the reported Unity/URP compiler request; this project is fixed to Unity `6000.5.0f1`.
- No local Unity Editor, FXC process, or C# compiler is available in this environment. Runtime behavior and GPU timing are unchanged by design but remain unmeasured after this correction.

---

## 2026-07-22 — GSU-M2.7C.5E.2.3: Whole-Feature Boundary Culling and Lower-Layer Continuity

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Whole-feature layout payload/report contract: **complete for the approved source change**.
- Installed profile metadata refresh: **complete for the approved source change**.
- Bank and Riverbed whole-feature retention: **complete for the approved source change**.
- Bank-under-Riverbed continuity and equivalent-surface reuse: **complete for the approved source change**.
- Architecture and Inspector documentation: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation, proof run, installer refresh, and visual validation: **pending in Unity 6000.5.0f1**.

### Outcome required

Replace the E2.2 per-fragment feature attenuation that produces partial or dissolving rocks with one deterministic retention decision for each complete generated rock. Keep the existing Palette Form and packed-detail texture samples, texture-array memory, draw calls, and payload resolution unchanged. Correct the Bank-to-Riverbed composition so an upper Riverbed transition always blends over a fully resolved Bank/Primary-Ground lower layer and can never expose a narrow Primary-Ground strip. When Bank and Riverbed resolve to the same surface material with equivalent application settings, reuse the Bank result through the internal boundary instead of independently suppressing or resampling the same feature payload.

### Approved file scope

Modify only:

- `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create/delete/move/rename: none.

No texture, texture-array, material asset, layer asset, scene, prefab, mesh stream, component, renderer, tag, layer, shader pass, or draw-call addition is authorized. No frozen Generated Mass recipe, source mesh, placement count, candidate density, palette, or payload image channel change is authorized.

### Reviewed evidence and current baseline

- `Assets/AGENTS.md`: mandatory read-only review, canonical-plan-first write, exact implementation scope, final consistency audit, Unity `6000.5.0f1`, low-runtime-cost requirement, and no false validation claims.
- Reconstructed source baseline: `/mnt/data/Assets-Code-Archive(9).zip` with D3 → D4 → D5 → E1, user-provided `GeneratedGroundEditor(6).cs`, then E2 → E2.1 → E2.2 → E2.2.1 applied in order. The archive contains no `.git` metadata; branch, `HEAD`, history, and live unrelated changes are unavailable.
- Authoritative editor ancestry: `/mnt/data/GeneratedGroundEditor(6).cs` remains the required pre-E2 editor baseline; current E2.2.1 editor changes are limited to the accepted generic Bank/Riverbed transition additions on top of that source.
- User screenshots `aa3550b9-4076-47f3-af45-8a2dbe851caf.png`, `ebf93db2-ce65-4f6f-96ff-b8ccb4bb6479.png`, `61788914-ce24-4c10-b6ed-4e9576b78bd5.png`, and `f8207cd6-2c3d-48b8-98b6-90773bb9b6f5.png`: rocks remain partially visible, spatially clipped, or reduced to slivers for every practical Edge Clearance / Return Fade combination.
- User screenshots `6e55bca1-aed0-4125-9ca2-e2ce735f2891.png` and `27f1e7bf-f4dc-4d15-bd3d-e118bbe7c180.png`: a narrow Primary-Ground-coloured line appears between Bank and Riverbed, including when both use the same installed sparse surface.
- `PixelSurfaceGroundResponse.hlsl > ResolveGroundSurfaceFeatureRetention`: current retention is calculated independently at every fragment from local inward distance. It cannot make one rock-wide keep/reject decision and therefore necessarily permits eaten silhouettes.
- `PixelSurfaceGroundForwardPass.hlsl > ResolveGroundBankLayerDetail / ResolveGroundRiverbedLayerDetail`: current feature suppression is applied after sampling from the existing two arrays; no additional texture sample is required for a whole-feature decision.
- `GeneratedMassSparseRiverbedTileAssembler.CandidateResult.Placements`: the proof already retains deterministic centre, radius, scale, and source evidence for every one of the exact `6 / 9 / 12` rocks. Normalized centre/radius metadata can be emitted without changing generated images.
- `StylizedSurfaceMaterialProfile`: current profile owns payload identity, world repeat, and response tuning but has no installed deterministic feature-layout metadata.
- `GeneratedMassSparseRiverbedSurfaceInstaller`: current installer owns the three canonical material profiles and already updates them in place while preserving palette/tuning and GUIDs. It is the correct owner for installing proof-emitted layout metadata.
- `PixelSurfaceGroundForwardPass.hlsl > Frag`: current lower Bank continuation is multiplied by interpolated Riverbed Support before substrate weights are resolved. This allows Bank-domain loss and upper-layer gain to overlap imperfectly and expose Primary Ground.
- `PixelSurfaceGroundResponse.hlsl > ResolveGroundSubstrateCompositionWeights`: the final three weights are sequential only when the provided lower Bank weight is continuous. The weighting function itself does not require replacement.
- `GeneratedGround.ApplySurfaceLayerDetailProperties`: one detail repeat covers `layer.DetailWorldScale × application multiplier`; normalized layout metadata can be converted to world offsets and radii without another texture or mesh channel.

### Implementation contract

1. Advance proof identity to `GSU-M2.7C.5E.2.3`, assembler algorithm version `8`, while preserving all generated image bytes and rock placement decisions from algorithm `7`.
2. Emit one machine-readable normalized feature-layout line for every candidate. Each entry contains toroidal centre `u/v` and a conservative normalized support radius derived from the existing placement radius plus the accepted silhouette-filter support padding. Validate count, finite values, normalized bounds, and exact deterministic repetition.
3. Add hidden serialized normalized feature-layout metadata to `StylizedSurfaceMaterialProfile`. The layout is payload metadata, not palette tuning. Expose read-only count/data through the material and Ground-layer contracts.
4. Update the installer to require a passing algorithm-8 proof, parse every candidate layout, and replace only the hidden layout metadata on the existing canonical `SSMP_Riverbed*` profiles. Preserve all palette, response, world-scale, layer tuning, paths, and GUIDs. Verify installed layout count/data.
5. Retain the existing serialized `bankFeatureEdgeClearance` / `riverbedFeatureEdgeClearance` fields for compatibility, but redefine their public and Inspector meaning as **Discrete Feature Safety Margin**. Effective feature-free distance is `Material Blend Distance + Safety Margin`. Retain Return Fade storage but rename its authored meaning to **Whole Feature Return Fade**.
6. Upload at most twelve normalized centre/radius vectors for each active Bank and Riverbed application through `MaterialPropertyBlock.SetVectorArray`. Add no per-frame CPU work; values update only during the existing material-property refresh path.
7. On feature-aware rock pixels, select the nearest toroidal feature centre from the current normalized detail UV. Reconstruct that centre's application-boundary distance from the interpolated inward-distance field and its screen-space world-XZ gradient, subtract the feature radius in world metres, and calculate one feature-wide hard/smooth retention value. Apply that same value to complete form, slope, cavity, and roughness replacement.
8. Execute derivative reconstruction only inside the uniform feature-layout-enabled material path. Execute the centre loop only when sampled feature coverage is nonzero. Keep texture-array sample count unchanged.
9. Keep the resolved Bank lower layer available across the complete Riverbed application domain before Riverbed weight is evaluated. Remove the fractional Riverbed-Support multiplication that currently permits a lower-layer dip.
10. Detect Bank and Riverbed applications that reference the same `GroundSurfaceLayerProfile` and have equivalent complete dry-application settings. In that case, reuse Bank detail through the internal boundary and skip the duplicate Riverbed detail sampling/feature culling. Different settings or different layers continue to blend Riverbed directly over the fully resolved Bank/Primary-Ground result.
11. Keep Shore wetness, Riverbed wetness, support masks, cover-retention rules, corridor geometry, and hydrology controls independent and unchanged.

### Invariants and non-goals

- No additional texture lookup, texture-array slice, runtime texture allocation, draw call, mesh stream, dispatch, or runtime-generated mask.
- No per-frame C# feature search or metadata rebuild.
- No material-name, candidate-name, Bank-only, or Riverbed-only shader branch. Current slots consume one generic normalized feature-layout contract.
- No arbitrary project-wide same-name asset deletion. Installer remains restricted to its canonical paths.
- No claim that screenshot defects are fixed until Unity compilation, proof/install reports, and scene evidence pass.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record evidence, exact scope, whole-feature metadata/retention contract, lower-layer continuity rule, same-surface reuse, performance invariants, and validation.
2. **Complete — controls/editor:** preserved serialized fields, replaced misleading per-pixel terminology with Safety Margin and Whole Feature Return Fade, and documented that Material Blend Distance is included automatically.
3. **Complete — profile/runtime transport:** added hidden normalized feature metadata, Ground-layer forwarding, fixed property arrays, conservative equivalent-application detection, and existing material-refresh transport.
4. **Complete — proof/installer:** emits and validates deterministic layout metadata, parses and updates existing canonical profiles in place, verifies count/data, and advances report identities to E2.3 / algorithm 8.
5. **Complete — HLSL:** implemented nearest-feature whole-retention reconstruction with conservative derivative fallback, retained the exact sample count, made Bank lower-layer continuity complete, and reuses Bank detail for equivalent same-surface applications.
6. **Complete — architecture docs:** superseded per-pixel Edge Clearance behavior and recorded sequential lower-layer continuity for current and future application slots.
7. **Complete for available static checks — final audit:** compared exact delta with scope; reread producers/consumers/contracts; verified editor ancestry, payload logic, sample count, serialized names, installer paths/GUID policy and shader interfaces; clean patch reapplication reproduces the edited tree exactly. Unavailable Unity checks remain pending.

### Acceptance criteria

- Every feature texel associated with one rock receives the same retention decision within numerical tolerance; no spatial clipping contour can pass through that rock.
- Safety Margin `0` still excludes complete rocks from the full Material Blend Distance. Whole Feature Return Fade `0` makes a hard whole-rock return; positive values fade the entire rock uniformly.
- Algorithm-8 proof emits exactly `6 / 9 / 12` finite normalized centre/radius entries matching candidate placements and repeats identically.
- Installer rerun updates the existing three material profiles in place, preserves GUIDs and authored tuning, and creates no copies.
- Bank remains the resolved lower surface throughout Riverbed transition. Primary Ground cannot appear solely because Bank and Riverbed application weights overlap.
- Equivalent same-layer Bank/Riverbed applications reuse the Bank detail result and do not execute a duplicate Riverbed texture-form/detail sample path.
- Ground forward-pass texture sample count remains unchanged from E2.2.1.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual project-file delta against the reconstructed E2.2.1 baseline modifies exactly the fifteen approved files listed in this section. Create/delete/move/rename inside `Assets`: none.

Implemented behavior:

- proof identity advances to `GSU-M2.7C.5E.2.3`, algorithm version `8`; placement, candidate count, substrate formula, image channels and image-generation formulas remain unchanged;
- every candidate now emits exactly one normalized centre/support-radius entry per accepted rock, and the layout participates in deterministic payload/candidate fingerprints;
- installed `StylizedSurfaceMaterialProfile` assets carry a hidden maximum-12 layout array; the installer parses the report, updates that metadata at the canonical existing paths, preserves palette/layer tuning and verifies installed values;
- existing serialized Edge Clearance/Return Fade fields remain compatible, while public/Inspector semantics become Discrete Feature Safety Margin and Whole Feature Return Fade;
- Ground uploads two fixed twelve-vector buffers through the existing material-property refresh path plus counts/equivalent-application state;
- the shader selects the nearest toroidal feature only on sampled feature pixels, reconstructs its centre boundary distance from the coherent world-XZ gradient, subtracts conservative feature radius, and applies one common retention value to feature form, slope, cavity and roughness;
- degenerate derivative reconstruction conservatively rejects the complete feature instead of falling back to fragment-local clipping;
- Bank continuation beneath Riverbed is authorized across the complete Riverbed application domain rather than multiplied by fractional Riverbed Support;
- equivalent same-layer Bank/Riverbed applications reuse the Bank detail result and skip the duplicate Riverbed detail/form sampling path.

Available static checks passed:

- reconstructed-tree comparison reports exactly fifteen approved modifications and no other project delta;
- C# Pygments lexical scan reports zero error tokens and balanced parentheses/brackets/braces for all eight modified C# files;
- HLSL delimiter and preprocessor-balance scans pass for all three modified include files;
- function-signature/reference scans find one gradient definition with two coherent call sites and complete Bank/Riverbed whole-feature wrapper call paths;
- Ground forward-pass source contains the same five texture-sampling statements as E2.2.1 (`4 ×` texture-array and `1 ×` ordinary texture); no sampler, texture declaration, slice or draw path was added;
- independent affine-distance simulation produces the same retention value across all sampled pixels of one feature, including inside Whole Feature Return Fade;
- authoritative `GeneratedGroundEditor(6).cs` lineage is preserved; the E2.3 editor delta is limited to the existing shared application-transition help/enablement block;
- existing payload source mode, library builder, material validator, River corridor geometry, ShaderLab file, wetness/hydrology paths, frozen Generated Mass recipes and source meshes remain unchanged from E2.2.1;
- applying the fifteen-file staged patch to a fresh reconstructed E2.2.1 baseline reproduces the final edited `Assets` tree exactly.

No Unity Editor, C# compiler, or HLSL frontend is available in this environment. Unity compilation, shader compilation, algorithm-8 proof execution, installer refresh/GUID preservation, MaterialPropertyBlock vector-array transport, target-GPU timing and visual confirmation remain pending in Unity `6000.5.0f1`.

### Risks and validation limits

- Screen-space gradient reconstruction is exact for the current linearly interpolated distance field within one rasterized triangle; numerical continuity across triangle edges must be inspected in Unity.
- A conservative support radius can reject a rock slightly earlier than its visible silhouette. This is preferable to permitting partial features but remains a visual tuning risk.
- Same-surface reuse is valid only when the layer reference and complete dry-application settings are equivalent. The equivalence check must be explicit and conservative.
- Unity compilation, shader compilation, MaterialPropertyBlock vector-array transport, actual sample execution, and scene results are unavailable in this environment and remain mandatory validation gates.

---

## 2026-07-22 — GSU-M2.7C.5E.2.2.1: Feature-Payload Library Rebuild Validation and Rollback Correction

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Explicit sRGB feature-payload validation correction: **complete for the approved source change**.
- Detailed library-rebuild failure transport: **complete for the approved source change**.
- Installer rollback safety: **complete for the approved source change**.
- Feature-aware material-validation consistency: **complete for the approved source change**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation and corrected installer run: **pending in Unity 6000.5.0f1**.

### Outcome required

Correct the E2.2 installer failure without regenerating the accepted algorithm-7 payloads or changing any runtime surface, shader, Ground, River, material-profile, layer-profile, scene, prefab, or texture-sampling contract. Feature-aware Palette Form `G/B` channels are stored as sRGB-encoded bytes but represent linear substrate-only form and roughness. Editor validation must decode those channels explicitly before applying linear thresholds. The installer must report the exact builder rejection and must restore the canonical detail-library asset exactly if configuration or rebuild fails.

### Approved file scope

Modify only:

- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`

Create/delete/move/rename: none.

No payload regeneration, assembler, assembly validator, source-mode enum, detail-library runtime schema, material profile, Ground layer, shader/HLSL, GeneratedGround, authoritative `GeneratedGroundEditor(6).cs`, scene, prefab, component, texture, tag, layer, or renderer change is authorized.

### Reviewed evidence and current baseline

- `Assets/AGENTS.md`: mandatory read-only review → persistent plan → exact implementation → final audit; exact scope and no false Unity claims.
- Reconstructed current source: `/mnt/data/Assets-Code-Archive(9).zip`, SHA-256 `55292f20bb2213ab86a0749e1db455cbf40da62816f038b0f509af5a6e88958c`, with D3 → D4 → D5 → E1 → E1.1 → E2 → E2.1 → E2.2 packages applied in order. No `.git` metadata is present, so branch, `HEAD`, history, and live unrelated changes remain unavailable.
- E2.2 patch source: `/mnt/data/GSU_M2_7C_5E_2_2_Feature_Aware_Surface_Application_Transition.zip`, SHA-256 `0626e80322d78a1736899bc4a02fddce20aea893f36b59a5d356695657b434cd`.
- Authoritative Ground editor remains user-provided `/mnt/data/GeneratedGroundEditor(6).cs`, SHA-256 `e3af41c8e6c641b7d5d3e1aa9aa051f2efe0a692dae0b275fca0adf5d5cfbd4b`; it is outside this correction scope and must remain byte-identical.
- User installer report: `/mnt/data/GeneratedMassSparseRiverbedSurfaceInstallReport.txt`, SHA-256 `962539370673e19c68c0f86d8e16beebe320945adad7aad1e5d44083aa92de8f`. It proves all three packed payloads imported, all three feature-aware Palette Form PNGs updated, the canonical library configuration was written, and the rebuild then returned only the generic failure `The sparse-riverbed detail library rebuild failed.`
- `GeneratedMassSparseRiverbedTileAssembler.cs > EncodePalettePayload`: accepted E2.2 payload stores `R/G/B` through `Mathf.LinearToGammaSpace` and stores `A` as linear feature coverage. The assembler proof validates decoded linear substrate-only means.
- `StylizedSurfaceDetailLibraryBuilder.cs > ValidatePrepackedTextureFormEntry`: current feature validation reads `Texture2D.GetPixels`, accumulates `pixel.g` and `pixel.b` directly, then compares them with linear ranges `0.54–0.70` and `0.55–0.80`. This mixes encoded RGB values with linear thresholds and is the identified rebuild blocker.
- `StylizedSurfaceDetailLibraryBuilder.cs > Rebuild`: current public result is Boolean only. With `logResult=false`, the caller receives no validation details.
- `GeneratedMassSparseRiverbedSurfaceInstaller.cs > InstallAllCandidates`: current installer configures and saves the canonical library before calling `Rebuild(library, false)`. On failure it leaves the modified source-mode/entry state in place and reports only one generic line.
- `StylizedSurfaceMaterialValidation.cs > AppendTextureFormReport`: the current generic prepacked-form diagnostic treats RGB disagreement as invalid grayscale and requires at least five percent dark-band coverage. Those assumptions do not apply to feature-aware `R/G/B/A` payloads and would incorrectly reject the accepted sparse feature payload after installation.
- `StylizedSurfaceDetailLibrary.cs`: complete entry/source-mode/generated-array ownership reviewed. No runtime schema change is required.

### Implementation contract

1. Read feature-aware Palette Form pixels as `Color32` and explicitly decode `G/B` from sRGB to linear before computing substrate-only means. Keep `A` as unmodified linear feature coverage.
2. Add a backward-compatible builder overload that returns exact rebuild failure messages while retaining the existing `Rebuild(library, bool)` API for all current callers.
3. Keep ordinary paired grayscale and authored-material validation behavior unchanged.
4. Add a feature-aware material diagnostic path that validates/reports combined form, substrate-only form, substrate roughness, and feature coverage without applying ordinary grayscale-channel-equality or five-percent dark-band rules.
5. Before changing an existing canonical sparse-riverbed library, persist and capture its complete `.asset` bytes. If configuration, validation, rebuild, or an exception fails, restore those bytes and force reimport. If the installer created the library in the failing run, delete that newly created asset instead.
6. Remove staged `Created/Updated/Unchanged library` action text when rollback occurs and report the rollback result explicitly.
7. Add every exact builder failure to the installer report. Retain one generic fallback only when the builder returns no detail.
8. Advance installer/report identity to `GSU-M2.7C.5E.2.2.1`. Continue accepting the existing passing E2.2 algorithm-7 proof; no proof rerun is required.

### Invariants and non-goals

- Do not alter payload bytes, feature thresholds, candidate identities, array formats, array depths, profile/layer tuning, GUID policy, installer canonical paths, shader behavior, runtime sampling, draw calls, or memory.
- Do not add a new source mode or bump the prepacked texture-form generation/signature version; the payload contract is unchanged.
- Do not delete or overwrite external same-name assets.
- Do not claim Unity compilation or a successful installer run from static inspection.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record source evidence, exact failure cause, narrow scope, explicit colour-space contract, detailed-error API, rollback behavior, direct material-validator consistency, invariants, and validation.
2. **Complete — builder:** decode feature `G/B` explicitly and add detailed rebuild-failure transport without breaking existing callers.
3. **Complete — material validation:** add feature-aware diagnostics that match the accepted packed-channel contract.
4. **Complete — installer:** snapshot canonical library state, configure/rebuild transactionally, restore/delete on failure, and include exact builder failures in the copied report.
5. **Complete for available static checks — final audit:** reread final modified files and direct contracts; compare exact source delta; verify authoritative editor and all runtime/shader/payload files remain unchanged; run available syntax/reference/package checks; record Unity validation as pending.

### Acceptance criteria

- Feature-aware `G/B` means are measured in decoded linear space and accepted payload ranges match the passing proof.
- Feature mask `A` remains linear and is not gamma-decoded.
- A builder validation failure appears verbatim in the installer report.
- Existing callers of `Rebuild(library, bool)` remain source-compatible.
- A failed refresh restores the pre-run canonical library bytes and GUID, or removes a library created by the failing run.
- A successful corrected refresh updates the existing canonical library in place and preserves its GUID, material tuning, and layer tuning.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual project-file delta against the reconstructed E2.2 baseline modifies exactly:

- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`

Create/delete/move/rename inside `Assets`: none.

Implemented behavior:

- feature-aware Palette Form validation now reads raw `Color32` bytes, decodes `G/B` explicitly through `Mathf.GammaToLinearSpace`, and keeps alpha as a linear feature mask;
- builder thresholds are shared with feature-aware material diagnostics so the same payload cannot pass one validator and fail another for different assumptions;
- the existing public `Rebuild(library, bool)` API remains intact and forwards to an additive detailed-failure overload;
- material validation now reports builder failures and uses a feature-aware channel report instead of ordinary grayscale/chroma and five-percent dark-band rules;
- the installer captures the canonical library asset bytes and GUID before configuration, includes exact builder failures in its report, removes staged library action text on failure, restores the prior asset bytes/GUID after a failed existing-library rebuild, and deletes a library created by a failing run;
- installer/report identity advances to `GSU-M2.7C.5E.2.2.1`, while proof acceptance remains the existing passing E2.2 algorithm-7 output.

Available static checks passed:

- complete reconstructed-tree comparison reports exactly the four approved modifications;
- C# lexical scanning reports zero error tokens and balanced parentheses/braces/brackets for all three modified source files;
- existing and additive builder overload call sites resolve by source shape;
- feature RGB decoding and linear alpha handling are present in both builder and material validation;
- rollback byte capture/write, forced synchronous reimport, and GUID verification are present in the installer transaction;
- independent byte-level sRGB simulation confirms a linear form value `0.62` is stored near byte `206` (`0.808` raw, outside the old linear maximum) and decodes to approximately `0.617`, while linear roughness `0.68` is stored near byte `215` (`0.843` raw) and decodes to approximately `0.680`;
- authoritative `GeneratedGroundEditor(6).cs`, assembler, assembly validator, detail-library runtime schema, material-profile runtime schema, and all Ground HLSL files remain byte-identical to E2.2;
- conflict-marker and trailing-whitespace scans pass.

No Unity Editor or C# compiler is available in this environment. Unity compilation, AssetDatabase rollback execution, actual array rebuild, installer pass, GUID preservation, and material validation remain pending the corrected Unity `6000.5.0f1` run.

### Risks and validation limits

- Raw asset-byte rollback depends on the canonical library remaining at its fixed asset path; the installer already enforces that path and asset type.
- The current failed E2.2 run may already have left the canonical library stale. The corrected installer is expected to rebuild it successfully; rollback can preserve only the state present immediately before the corrected run.
- Unity compilation, importer behavior, AssetDatabase reimport, and actual rebuild success remain pending until the corrected installer is executed in Unity `6000.5.0f1`.

---

## 2026-07-22 — GSU-M2.7C.5E.2.2: Feature-Aware Surface-Application Transition

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Packed feature-payload generation: **complete for the approved source change**.
- Feature-payload library/runtime transport: **complete for the approved source change**.
- Generic Bank/Riverbed feature-edge controls: **complete for the approved source change**.
- Feature-aware shader composition: **complete for the approved source change**.
- Proof/installer validation updates: **complete for the approved source change**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation, proof, reinstall, GPU comparison, and production-camera validation: **pending in Unity 6000.5.0f1**.

### Outcome required

Preserve the accepted M2.7C.5E.2.1 generic Bank-to-Ground and Riverbed-to-resolved-Bank/Ground material transitions, but stop discrete sparse-rock features from being partially dissolved where a surface application fades. The complete base substrate must continue blending normally. Discrete rock form, slope, cavity, and rock-specific finish must be removed before the application boundary so the transition region resolves to the imported surface's substrate-only payload rather than a partially eaten rock.

The runtime implementation must preserve the existing two texture-array samples per active imported surface. No additional texture, texture-array slice, draw call, runtime allocation, per-frame CPU process, mesh stream, scene object, or renderer is authorized.

### Approved file scope

Modify only:

- `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`

Create/delete/move/rename: none.

No corridor geometry/channel, Ground mesh, River mesh, water shader, vegetation source, Painted Accent source, frozen Generated Mass source recipe, rock placement, candidate density, material-profile asset, scene, prefab, component, tag, layer, or renderer change is authorized.

### Reviewed evidence and current baseline

- `Assets/AGENTS.md`: mandatory read-only review → persistent plan → exact implementation → final consistency audit; Unity `6000.5.0f1`; exact scope; no false compile/runtime claims; shared-shader impact audit required.
- Reconstructed source baseline: `/mnt/data/Assets-Code-Archive(9).zip` with D3 → D4 → D5 → E1 → E2 → E2.1 packages applied in order. The archive contains no `.git` metadata, so branch, `HEAD`, history, and unrelated live working-tree state are unavailable.
- Authoritative Ground editor baseline: user-provided `/mnt/data/GeneratedGroundEditor(6).cs`, SHA-256 `e3af41c8e6c641b7d5d3e1aa9aa051f2efe0a692dae0b275fca0adf5d5cfbd4b`, plus only the accepted E2.1 Bank/Riverbed generic transition additions. The reconstructed editor diff against that file contains only those additions.
- User production screenshot after E2.1 proves the material transition works but a rock intersecting the transition is still partially dissolved. The accepted diagnosis is that one application weight currently fades substrate and discrete rock response together.
- `GeneratedMassSparseRiverbedTileAssembler.cs > BuildFinalEvidence`: algorithm version 6 currently writes Palette Form as grayscale RGBA. `R/G/B` are duplicates and `A` is opaque, leaving three channels available in the already sampled sRGB RGBA32 texture-form payload.
- `GeneratedMassSparseRiverbedTileAssembler.cs > BuildRuntimePackedDetailPixel`: the existing packed detail already contains combined slope/cavity/roughness. Substrate slope and cavity are neutral, and substrate roughness is already calculated before rock interpolation.
- `StylizedSurfaceDetailLibraryBuilder.cs`: texture-form arrays are already `RGBA32`, sRGB, bilinear, mipmapped, and copied without adding a sample. Prepacked texture-form payloads preserve all four source channels.
- `GeneratedMassSparseRiverbedSurfaceInstaller.cs > NormalizePayloadImporter`: Palette Form is imported as uncompressed sRGB RGBA with alpha from input, bilinear filtering, repeat wrap, and mips. The existing import path can preserve the expanded payload without a new asset.
- `PixelSurfaceMaterialDetail.hlsl > PS3D_AssignStylizedSurfaceTextureForm`: runtime currently reads only `formSample.r`. `PixelSurfaceGroundForwardPass.hlsl` samples the texture-form array once for Bank and once for Riverbed; no other shader consumes `PS3D_StylizedSurfaceDetail`, so the shared-include impact is limited to Ground.
- `GroundMaterialControls.cs` and E2.1 transport already pack each application's material transition into one `float4`; `.z/.w` are unused and can carry feature-edge clearance and feature-return fade distance without new shader properties or CBUFFER entries.
- E2.1 corridor inward-distance fields already provide the spatial input required by both Bank and Riverbed feature handling. No corridor regeneration contract change is required beyond the existing E2.1 rebuild.

### Packed feature-payload contract

Sparse candidates advance to assembler algorithm version `7`. The existing `*_PaletteForm.png` remains one sRGB `RGBA32` source and is repacked as:

```text
R = combined substrate + rock Palette Form, gamma encoded
G = substrate-only Palette Form under the rock, gamma encoded
B = substrate-only roughness/finish, gamma encoded
A = discrete-feature coverage mask, linear
```

The existing `*_RuntimePackedDetail.png` remains unchanged in layout:

```text
RG = combined world-space slope
B  = combined cavity
A  = combined roughness
```

No additional output texture is created. Neutral/contrast/alternate previews continue reading channel R for the complete preview. Validation additionally decodes G/B/A and proves that substrate-only data and feature coverage survive seams and mips.

### Feature-payload identity contract

Add one additive library source mode for prepacked paired payloads with discrete-feature channels. Existing modes retain their numeric values and behavior. The new mode:

- resolves through the existing packed and texture-form arrays;
- marks the material/layer as feature-aware through existing runtime property vectors;
- does not reinterpret ordinary authored-material or grayscale texture-form entries;
- remains generic and is not keyed to riverbed names, candidate IDs, or rock-specific shader branches.

The existing detail control vector encodes texture-form payload mode as:

```text
0 = no texture form
1 = ordinary texture form
2 = feature-aware texture form
```

No new material property is required.

### Generic application feature-edge contract

Each current surface application slot receives two additional settings stored beside its existing material transition:

- `Discrete Feature Edge Clearance`: inward feature-free distance from the application boundary, `0–2 m`; zero disables feature suppression for direct A/B timing and visual comparison.
- `Discrete Feature Return Fade`: transition width after the clearance where features return, `0–1 m`; zero performs a hard return after the feature-free zone.

Defaults for Bank and Riverbed are `0.50 m` clearance and `0.15 m` return fade. The accepted sparse candidates use an 8 m world tile and approximately sub-0.5 m rock diameters, so the default clearance is intended to remove rocks intersecting the actual application boundary before they can be partially blended. This remains a visually validated default, not a proof of arbitrary future feature size.

The existing material-transition vector is reused:

```text
x = Material Blend Distance
y = Material Blend Softness
z = Discrete Feature Edge Clearance
w = Discrete Feature Return Fade
```

Future application slots must use the same value contract.

### Shader composition contract

After the existing packed-detail and texture-form samples are decoded, feature-aware payloads retain both combined and substrate-only values. A sparse conditional feature-retention operation uses the slot's inward boundary distance and feature settings to:

- interpolate combined Palette Form toward substrate-only Palette Form;
- attenuate combined slope toward neutral substrate slope;
- attenuate cavity and cavity core toward zero;
- interpolate combined roughness toward substrate-only roughness;
- leave ordinary texture-form and non-texture-form materials unchanged.

The base substrate still uses the existing complete material application weight and therefore blends into the lower resolved surface normally. Feature suppression occurs before final sequential Ground → Bank → Riverbed composition. The operation adds no texture lookup. A coherent uniform/payload gate skips it when clearance is zero or the material is not feature-aware, and a sparse feature-mask gate limits the interpolation arithmetic to feature texels.

### Inspector contract

Expose the same feature controls under both current application transitions:

```text
Hierarchy > Ground > Inspector > Material
> River-Coupled Ground Response — River Bank
> Material Coverage > Application Transition
> Discrete Feature Edge Clearance / Discrete Feature Return Fade
```

```text
Hierarchy > Ground > Inspector > Material
> River-Coupled Ground Response — Riverbed
> Material Coverage > Application Transition
> Discrete Feature Edge Clearance / Discrete Feature Return Fade
```

Help text must state that the controls affect only feature-aware payloads. Setting clearance to zero skips feature-retention arithmetic while retaining identical texture samples and draw calls, providing the requested performance/visual A/B path.

### Installer refresh contract

The installer must continue replacing canonical files/assets in place. It must:

- require a passing E2.2 algorithm-7 proof;
- overwrite the existing Palette Form and packed PNG paths;
- update the existing library entries to the new feature-aware source mode;
- preserve library/profile/layer GUIDs;
- preserve all user-authored SSMP and GSLP tuning;
- report `Created`, `Updated`, or `Unchanged`;
- create no numbered copies.

### Performance contract

Runtime deltas:

- texture samples: `+0`;
- draw calls: `+0`;
- runtime texture-array dimensions/slices/formats: unchanged;
- runtime texture memory: unchanged;
- runtime CPU work and allocations: unchanged;
- mesh streams/vertices: unchanged;
- fragment cost: one inexpensive feature-mode/mask gate after existing sampling; retention arithmetic runs only for feature-aware feature texels while clearance is nonzero.

Unity GPU timing remains required because static instruction counting cannot prove target-hardware cost.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record evidence, exact scope, packed channel contract, additive source mode, generic controls, shader operation, installer behavior, performance contract, risks, and validation.
2. **Complete — payload generation:** advance to algorithm 7; encode combined form, substrate form, substrate roughness, and feature coverage in the existing Palette Form texture; preserve existing rock placement and packed-detail output.
3. **Complete — proof validation/report:** validate expanded channel ranges, seams, feature-mask coverage, payload determinism, and preview stability; update report identity and output contract.
4. **Complete — library/profile transport:** add the feature-aware paired source mode and expose generic `UsesFeatureTextureForm` resolution through library → material → Ground layer.
5. **Complete — application controls/transport:** add Bank/Riverbed clearance and return-fade fields/defaults/copy/properties; pack them into the existing transition vectors; update the authoritative editor controls.
6. **Complete — shader:** decode expanded payload channels, gate feature work by nonzero clearance plus feature mode/mask, compute generic retention from existing inward-distance fields only for affected texels, and apply it to form/slope/cavity/roughness before sequential composition without extra samples.
7. **Complete — installer/validation:** require E2.2 algorithm 7, update canonical entries in place to the new mode, preserve GUIDs/tuning, and validate expanded source/runtime payloads.
8. **Complete — architecture documents:** record generic feature-aware transition ownership, packed-channel semantics, unchanged sample/memory budgets, and supersession of uniform feature/material fading.
9. **Complete for available static checks — final audit:** compare final diff to scope and E2.1 baseline; reread modified files and direct contracts; verify editor provenance, protected files, shader-property parity, source-mode compatibility, payload math, installer idempotence, package reapplication, and all available static checks. Unity compile/run/GPU/visual checks remain pending when unavailable.

### Acceptance criteria

- Palette Form payload uses the specified RGBA contract and remains deterministic/seam-safe.
- Existing ordinary texture-form and authored-material entries render unchanged.
- Bank and Riverbed both expose the generic feature-edge settings.
- Clearance zero produces the previous E2.1 feature behavior without changing samples or draw calls.
- Nonzero clearance produces substrate-only response near the application boundary for feature-aware sparse candidates.
- Complete application blending, wetness, cover retention, and sequential Ground/Bank/Riverbed composition remain unchanged.
- Installer reruns update the existing canonical assets and preserve GUIDs and tuning.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual project-file delta against the reconstructed accepted E2.1 baseline is exactly the eighteen approved modifications listed in this section. No file is created, deleted, moved, or renamed.

Implemented behavior:

- sparse-riverbed assembler algorithm version advances from `6` to `7` without changing rock placement, source order, density, scale, root, or accepted material-response calculations;
- the existing Palette Form PNG/array stores combined form in `R`, substrate-only form in `G`, substrate-only roughness in `B`, and filtered discrete-feature coverage in `A`;
- the existing packed-detail payload remains unchanged in format and slice count;
- one additive source-mode value, `PrepackedDetailWithFeatureTextureForm = 3`, opts materials into the expanded channel contract while preserving serialized values and behavior for modes `0–2`;
- feature-aware identity resolves through library → material profile → Ground layer → existing per-application detail vector;
- Bank and Riverbed both store and expose the shared feature clearance/return-fade settings through `GroundSurfaceApplicationBlendSettings`;
- existing transition vectors carry material distance/softness in `x/y` and feature clearance/return fade in `z/w`; no shader property or CBUFFER field is added;
- feature work is gated after existing texture sampling by nonzero clearance, feature-aware mode, and nonzero feature coverage; only affected feature texels compute retention/interpolation;
- ordinary payloads and clearance-zero feature-aware payloads skip the retention operation;
- installer validation requires a passing `GSU-M2.7C.5E.2.2`, algorithm-version-7 proof and updates the canonical library entry mode in place; existing GUID/tuning-preservation behavior is unchanged.

Available static checks passed:

- exact tree comparison reports only the eighteen approved modified files;
- C# lexical scanning reports zero error tokens for all eleven modified C# files;
- C#/HLSL delimiter and HLSL preprocessor-balance checks pass;
- conflict-marker and trailing-whitespace scans pass;
- all `GroundSurfaceApplicationBlendSettings` constructor sites use the four-value contract;
- serialized Inspector property names match the four new Bank/Riverbed fields;
- feature-aware source-mode resolution is complete across library, material profile, Ground layer, runtime transport, builder, validator, and installer;
- Ground forward-pass texture-sample counts are unchanged: four `SAMPLE_TEXTURE2D_ARRAY` calls and one ordinary `SAMPLE_TEXTURE2D` call before and after the patch;
- default retention simulation yields zero feature contribution through `0.50 m`, cubic return from `0.50–0.65 m`, and full contribution at/after `0.65 m`; clearance zero resolves to full retention and skips the retention gate;
- simulation from the supplied candidate masks predicts feature-mask means/maxima of approximately `0.00410/1.0`, `0.00581/1.0`, and `0.00763/1.0` for Ultra/Very/Sparse, inside the validator's `0.001–0.025` mean and `≥0.90` maximum gates;
- deterministic substrate-form simulation predicts substrate-only form mean approximately `0.6200` and roughness mean approximately `0.6800`, inside the validator and library-builder gates;
- the authoritative editor lineage is preserved: the reconstructed E2.1 editor differs from user-supplied `GeneratedGroundEditor(6).cs` only by the accepted generic material-transition additions, and this patch adds only the approved shared feature controls/drawer wiring;
- `StylizedRiverCorridorGeometry.cs`, `SH_PixelGroundSurfaceLit.shader`, frozen rock baker/validator, `MassGenerator.cs`, and `MeshData.cs` are byte-identical to the E2.1 baseline.

Static analysis is not Unity compilation, shader compilation, proof execution, installation, target-GPU timing, or production-camera validation. Those gates remain pending in Unity `6000.5.0f1`.

### Risks and validation limits

- Pixel-level feature suppression cannot make an arbitrary feature atomically present or absent without feature-center/identity data. The clearance is therefore sized to remove any rock intersecting the actual application boundary; feature return farther inside remains a controlled fade. Production-camera validation is mandatory.
- Mip filtering averages the feature mask and substrate channels. The proof validator must inspect and report payload seams/ranges, but only Unity scene comparison can confirm acceptable distant behavior.
- The new source mode is additive. Incorrect mode assignment would either ignore feature channels or reinterpret an old grayscale payload; installer and library validation must detect this.
- No Unity Editor, shader compiler, or target GPU is available in this environment. Compile, generated-output, in-scene, and performance claims remain pending.

---

## 2026-07-22 — GSU-M2.7C.5E.2.1: Generic Surface-Application Blend Correction

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Shared surface-application blend contract: **complete for the approved source change**.
- Bank-to-Ground application transition: **complete for the approved source change**.
- Riverbed-to-resolved-Bank/Ground application transition: **complete for the approved source change**.
- Sequential complete-response composition: **complete for the approved source change**.
- Authoritative Inspector update: **complete for the approved source change**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation, corridor regeneration, and production-camera validation: **pending in Unity 6000.5.0f1**.

### Outcome required

Replace the rejected Riverbed-specific M2.7C.5E.2 material-blend architecture with one generic surface-application transition contract used by every currently supported secondary Ground surface slot. The immediate visible defect is the River Bank-to-Primary-Ground boundary. Bank must therefore receive the same application-level `Material Blend Distance` and `Material Blend Softness` controls as Riverbed. Riverbed must continue blending into the already-resolved Bank/Ground result. Future surface slots must reuse the same C#/HLSL contract rather than receive another semantic-specific blend implementation.

The accepted M2.7C.5E.2 sparse-rock silhouette smoothing and installer update-in-place behavior remain frozen and are not modified by this correction.

### Approved file scope

Modify only:

- `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`

Create/delete/move/rename: none.

No assembler, payload validator, installer, surface-library schema, material-profile schema, scene, prefab, material/profile asset, texture asset, component, layer, tag, renderer, draw-call, water shader, vegetation source, Painted Accent source, frozen Generated Mass source, or rock-response change is authorized.

### Reviewed evidence and current baseline

- `Assets/AGENTS.md`: mandatory review → persistent plan → exact implementation → post-change audit; Unity `6000.5.0f1`; exact scope; no false compile/runtime claims; shared-shader impact audit required.
- Reconstructed current source baseline: `/mnt/data/Assets-Code-Archive(9).zip` with accepted D3 → D4 → D5 → baseline-safe E1.1 → E2 packages applied in order. The supplied archive has no `.git` metadata, so branch, `HEAD`, history, and unrelated live working-tree state are unavailable.
- `GeneratedGroundEditor.cs` in the E2 baseline is the user-provided authoritative `/mnt/data/GeneratedGroundEditor(6).cs`, SHA-256 `e3af41c8e6c641b7d5d3e1aa9aa051f2efe0a692dae0b275fca0adf5d5cfbd4b`, plus only the rejected E2 Riverbed transition additions. No vegetation editor implementation is present.
- User production screenshots prove the visible hard material boundary is the River Bank-to-Primary-Ground handoff. The user explicitly requires blending controls on both Bank and Riverbed and requires the settings to belong to each surface application slot.
- `GroundMaterialControls.cs`: Bank owns semantic composition controls (`Bank Material Strength`, reach, immediate exposure, waterline strength, core softness, outer extension/fade) but has no generic application-boundary distance/softness pair. Riverbed alone owns the E2 `riverbedMaterialBlendDistance/Softness` pair.
- `PixelSurfaceGroundForwardPass.hlsl > ResolveGroundSimpleBinarySubstrateWeights`: imported texture-form surfaces still replace continuous coverage with a fixed `0.5` ownership cut. This is the direct code path capable of converting an otherwise soft Bank mask into the observed fully opaque edge.
- `PixelSurfaceGroundResponse.hlsl`: Bank and Riverbed blending are separate semantic functions. E2 inserts Bank only as a special complementary Riverbed-edge case instead of composing layers sequentially.
- `StylizedRiverCorridorGeometry.cs`: `TEXCOORD3.y` is outward distance from Riverbed Support; `.z` is a Boolean Bank-domain flag; `.w` is Riverbed inward distance. The producer already knows `handoffHalfWidth` and `acrossDistance`, so it can publish Bank inward distance to the terrain handoff without adding a stream or changing topology.
- `PixelSurfaceGroundForwardTypes.hlsl`: the existing `float4` attribute and `half4` varying preserve all four UV3 components. No varying-layout change is required.
- Existing E2 assembler/validator/installer changes were reviewed and are outside this correction: algorithm-6 silhouette smoothing and canonical in-place refresh remain unchanged.

### Generic application contract

C# exposes one immutable `GroundSurfaceApplicationBlendSettings` value contract containing:

- `Distance`: application-boundary transition distance in metres, clamped to `0–2 m`;
- `Softness`: interpolation shape, clamped to `0–1`.

`GroundMaterialControls` stores one pair per current application slot:

- Bank application: `bankMaterialBlendDistance`, `bankMaterialBlendSoftness`;
- Riverbed application: existing `riverbedMaterialBlendDistance`, `riverbedMaterialBlendSoftness`.

Both default to `0.35 m / 0.75`. Riverbed serialized field names remain unchanged so E2 values survive. Bank receives new fields. Future slots must expose the same value contract and route through the same shader resolver.

HLSL exposes one generic transition function that accepts:

- region/support weight;
- inward distance from that application boundary;
- the slot's distance/softness vector.

Zero distance returns the unmodified historical region weight. Nonzero distance yields zero application weight at the boundary and rises to the slot's authored strength at the configured inward distance. One weight drives the complete layer response.

### Corridor channel correction

Repack the existing River corridor UV3 stream without adding a channel:

```text
TEXCOORD3.x = exact Riverbed Support
TEXCOORD3.y = outward Bank distance from Riverbed Support
TEXCOORD3.z = inward Bank distance from the terrain handoff
TEXCOORD3.w = inward Riverbed distance from Riverbed Support
```

Bank-domain authorization is derived from positive `.z` and `(1 - Riverbed Support)`. Ordinary GeneratedGround remains zero in all UV3 components and therefore unauthorized. The final terrain-handoff vertex publishes zero Bank inward distance, providing the exact Bank application boundary. Hidden corridor geometry beyond the handoff remains unauthorized for Bank material/hydrology and is covered by Primary Ground.

### Sequential surface composition contract

Remove the imported texture-form `0.5` binary ownership cut. All surface types use weighted sequential composition:

```text
resolved = Primary Ground
resolved = Blend(resolved, Bank, Bank application weight)
resolved = Blend(resolved, Riverbed, Riverbed application weight)
```

Equivalent normalized final weights are:

```text
Ground   = (1 - Bank) * (1 - Riverbed)
Bank     = Bank * (1 - Riverbed)
Riverbed = Riverbed
```

The Bank application weight combines its existing semantic composition with its generic boundary transition. Inside the Riverbed transition, the Bank edge response is evaluated as the already-resolved lower layer. If no Bank layer exists, Primary Ground remains underneath.

The same final weights must drive:

- palette/albedo and texture form;
- packed slope/normal;
- cavity;
- roughness/finish;
- dry smoothness;
- dry specular;
- texture-form scene-lighting response.

Wetness transitions and submerged-cover exclusion remain independent.

### Inspector contract

Expose identical application-transition controls in both current slots:

```text
Hierarchy > Ground > Inspector > Material
> River-Coupled Ground Response — Bank
> Material Coverage
> Material Blend Distance
> Material Blend Softness
```

```text
Hierarchy > Ground > Inspector > Material
> River-Coupled Ground Response — Riverbed
> Material Coverage
> Material Blend Distance
> Material Blend Softness
```

Bank semantic composition controls remain available under their existing groups and are not renamed into application blending. Tooltips must distinguish semantic coverage from the final application-boundary transition. Shared-style and local override paths must expose the same fields.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the rejected E2 architecture, immediate Bank defect, exact scope, generic C#/HLSL contract, UV3 repack, sequential composition, invariants, risks, and validation.
2. **Complete — material controls:** add the shared value contract and Bank application fields/properties/default/copy transport while preserving Riverbed serialized names.
3. **Complete — corridor producer:** repack UV3.z as Bank inward distance and preserve x/y/w contracts.
4. **Complete — material transport:** add `_GroundBankMaterialTransition` through C#, ShaderLab, and CBUFFER; keep Riverbed transport through the same value contract.
5. **Complete — shader response/composition:** add the generic transition resolver, use it for Bank and Riverbed, remove the binary ownership cut, and resolve sequential final weights across every material channel.
6. **Complete — Inspector:** add Bank controls in local/shared paths and use one shared application-transition drawing helper for Bank and Riverbed.
7. **Complete — architecture docs:** supersede the Riverbed-only E2 blend contract and update the frozen UV3 mapping and generic application ownership.
8. **Complete for available static checks — post-change audit:** reread all final modified files and direct contracts; verify exact scope, authoritative editor ancestry, UV3 producer/varying/consumer parity, shader-property parity, sequential weights, no assembler/installer drift, syntax/lexical/preprocessor checks, package reapplication, and pending Unity gates.

### Acceptance criteria

- Bank and Riverbed expose identical application-level distance and softness controls in local and shared-style Inspector paths.
- `TEXCOORD3.z` is Bank inward distance end-to-end; Bank authorization derives from that value without adding a stream.
- Imported and non-imported surfaces use the same continuous sequential weight composition; no fixed `0.5` texture-form ownership cut remains.
- Bank blends into Primary Ground at the terrain handoff; Riverbed blends into the resolved Bank/Ground result at Riverbed Support.
- One final weight set drives all material channels.
- Wetness, cover exclusion, stone payloads, and installer behavior are unchanged.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual source delta against the reconstructed M2.7C.5E.2 baseline is exactly the twelve approved modified files. No file is created, deleted, moved, or renamed.

Implemented behavior:

- `GroundSurfaceApplicationBlendSettings` now supplies one clamped distance/softness value contract shared by Bank and Riverbed application slots;
- Bank stores new `bankMaterialBlendDistance` / `bankMaterialBlendSoftness` values, while Riverbed preserves its existing serialized field names and routes through the same contract;
- `_GroundBankMaterialTransition` is transported through `GeneratedGround`, ShaderLab, and the Ground material CBUFFER alongside the existing Riverbed transition vector;
- the authoritative `GeneratedGroundEditor(6).cs` baseline now exposes the same shared Application Transition drawer in local and shared-style Bank and Riverbed Material Coverage sections;
- corridor `TEXCOORD3.z` now stores Bank inward distance from the terrain handoff; the existing four-component producer/varying/fragment path is unchanged;
- `ResolveGroundSurfaceApplicationTransition` is the single HLSL distance/softness resolver used by both slots;
- the imported texture-form `0.5` binary ownership cut and the rejected Riverbed-only edge special case are removed;
- final Ground/Bank/Riverbed weights use sequential composition and drive the existing complete material-response path;
- Shore/Riverbed wetness, exact-support cover exclusion, algorithm-6 rock payload generation, and installer refresh code are unchanged.

Available static checks passed:

- complete tree comparison reports exactly the twelve approved modifications;
- C# and HLSL lexical scans report zero error tokens, and comment/string-excluded delimiter checks pass for every modified code/include file;
- ShaderLab/HLSL raw delimiter and HLSL preprocessor-balance checks pass;
- C#/ShaderLab/CBUFFER/consumer property parity passes for `_GroundBankMaterialTransition` and the existing Riverbed transition;
- obsolete binary-cut and Riverbed-only transition symbols have no remaining active-code references;
- the corridor producer, existing `float4` attribute, existing `half4` varying, and fragment consumers preserve the complete x/y/z/w channel contract;
- Clang 17 HLSL frontend syntax validation passes for the exact generic transition and sequential-composition functions; DXIL code generation/validation is unavailable in this Clang build;
- numerical simulation confirms transition endpoints and monotonicity for linear/cubic softness and confirms sequential weights remain non-negative and sum to one within floating-point tolerance;
- `GeneratedMassSparseRiverbedTileAssembler.cs`, its validator, and `GeneratedMassSparseRiverbedSurfaceInstaller.cs` are byte-identical to M2.7C.5E.2;
- the generic `SH_PixelSurfaceLit.shader` is byte-identical, and every new material-property-dependent HLSL function remains inside `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`.

Unity and a standalone C# compiler are unavailable in this environment. Unity compilation, regenerated corridor UV3 data, live MaterialPropertyBlock values, and production-camera blend quality remain pending.

### Risks and validation limits

- Repacking UV3.z reopens the frozen A4B.3 channel meaning. All producer, varying, resolver, debug, and architecture references must be updated consistently. Unity corridor regeneration is mandatory.
- Bank inward distance is half-precision in the existing varying. Its `0–20 m` expected range remains representable, but production-camera validation must confirm no visible banding.
- Removing the binary cut changes imported Bank and Riverbed transition behavior by design. Existing non-imported surfaces should remain visually compatible because they already used continuous weights; all imported candidates require scene validation.
- Unity and a standalone project compiler are unavailable in this environment. Compilation, corridor regeneration, actual material weights, and visual acceptance remain pending in Unity.

---

## 2026-07-22 — GSU-M2.7C.5E.2: Controllable Riverbed Material Blend, Smooth Sparse-Rock Payload, and Idempotent Refresh

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Dry Riverbed material-transition implementation: **complete for the approved source change**.
- Sparse-rock payload silhouette correction: **complete for the approved source change**.
- Installer refresh/idempotence hardening: **complete for the approved source change**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation, proof regeneration, reinstall, and production-camera validation: **pending in Unity 6000.5.0f1**.

### Objective

Correct the three Unity-observed runtime defects in the installed sparse-riverbed comparison surfaces without changing the accepted exact-count `6 / 9 / 12` placement architecture:

1. replace the hard dry Riverbed material cutoff with an authored inward transition that blends the complete Riverbed surface response into the resolved Bank surface at the Riverbed boundary, or into Primary Ground when no Bank layer exists;
2. smooth only the outer sparse-rock payload silhouette so rocks no longer read as hard 2D pixel cutouts while preserving internal rock form, placement, density, and seamless tiling;
3. make proof regeneration and candidate installation explicitly idempotent at the canonical paths, replacing/updating existing candidate payloads and assets rather than creating numbered copies or parallel libraries.

### Approved file scope

Modify only:

- `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`

Create/delete/move/rename: none.

No scene, prefab, material/profile asset, texture asset, River source, corridor geometry, UV stream, vegetation source, Painted Accent source, layer, tag, component, shader keyword, renderer, or draw-call change is authorized.

### Reviewed evidence and current baseline

- `Assets/AGENTS.md`: mandatory review → persistent plan → exact implementation → post-change audit; Unity `6000.5.0f1`; exact scope; no false compile/runtime claims; shared-shader impact audit required.
- Supplied source archive: `/mnt/data/Assets-Code-Archive(9).zip`, SHA-256 `55292f20bb2213ab86a0749e1db455cbf40da62816f038b0f509af5a6e88958c`; no `.git` metadata. Branch, `HEAD`, history, and unrelated live working-tree changes are unavailable.
- Reconstructed current baseline: accepted M2.7C.5D.3 → 5D.4 → 5D.5 → baseline-safe 5E.1.1 source, with the user-provided `/mnt/data/GeneratedGroundEditor(6).cs` overlaid as the authoritative Ground-editor baseline. Its SHA-256 is `e3af41c8e6c641b7d5d3e1aa9aa051f2efe0a692dae0b275fca0adf5d5cfbd4b`.
- User-provided algorithm-5 proof report `/mnt/data/GeneratedMassSparseRiverbedAssemblyReport.txt`, SHA-256 `730b3d539ea155347c6baad7faf58c1ca24a0fa98a6a1218ecb8152da1bfce53`: deterministic PASS WITH SOURCE GEOMETRY DRIFT WARNING; all paired payload, seam, palette, exact-count, and macro-homogeneity gates passed. The ten accepted raw-source drift warnings are non-blocking and remain unchanged.
- User screenshots show: a straight fully opaque Riverbed boundary, rocks clipped by that boundary, and visibly stair-stepped/pixelated rock silhouettes. These are the active visual acceptance failures.
- `GroundMaterialControls.cs`: dry Riverbed composition exposes only `riverbedMaterialStrength`; the existing `riverbedToBankWetnessBlendDistance` and `riverbedToBankWetnessBlendSoftness` are hydrology-only and cannot control dry substrate composition.
- `PixelSurfaceGroundResponse.hlsl > ResolveGroundRiverbedMaterialBlend`: current dry Riverbed composition is `layer enabled × material strength × exact Riverbed Support`; it has no spatial transition.
- `PixelSurfaceGroundForwardPass.hlsl > ResolveGroundSimpleBinarySubstrateWeights`: every active texture-form substrate replaces continuous combined substrate coverage with a fixed `0.5` binary ownership cut. This directly explains the observed straight hard boundary. The old M2.4.1 binary-cut architecture was intentionally non-interpolating and is now visually rejected for the sparse riverbed use case.
- Existing River geometry already publishes `TEXCOORD3.w` as inward metres from the exact Riverbed Support boundary. `ResolveGroundRiverbedWetness` already proves this distance can drive an inward transition without River or geometry changes.
- Existing Bank material composition is continuous and controlled by Bank strength/reach/exposure/waterline/softness. The dry Riverbed transition can reuse the resolved Bank-edge material response inside Riverbed Support; when no Bank layer exists, the remaining weight naturally resolves to Primary Ground.
- `GeneratedMassSparseRiverbedTileAssembler.cs`: runtime Palette Form and packed detail currently classify each final texel through `final.Mask > 0.5`; `Downsample` stores the maximum of the 2×2 work mask. This produces hard texel silhouettes even though the source was rasterized at 2048².
- `GeneratedMassSparseRiverbedSurfaceInstaller.cs`: canonical paths and `File.Copy(..., overwrite: true)` already avoid normal numbered copies, while `CreateOrLoadAsset` updates exact-path assets and preserves material/layer tuning. Remaining weaknesses are imprecise created/updated/unchanged reporting, refusal to reconcile the dedicated library when its owned entries differ, and no explicit GUID-preservation/duplicate audit.
- `StylizedSurfaceDetailLibraryBuilder.Rebuild`: generated packed/form array sub-assets are explicitly detached, destroyed, and recreated inside the existing library asset; the main library GUID is preserved. Materials reference the library and stable IDs rather than generated array sub-assets.
- Shared-shader impact: `PixelSurfaceGroundMaterialProperties.hlsl` and `PixelSurfaceGroundForwardPass.hlsl` are Ground-shader-only. `PixelSurfaceGroundResponse.hlsl` is also included by generic `SH_PixelSurfaceLit.shader`; all new property-dependent logic must remain under `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`, and generic fallbacks must remain property-free.

### Dry Riverbed transition contract

Add two Riverbed application controls under the existing Riverbed `Material Coverage` subsection:

- `Material Blend Distance`: `0–2 m`, default `0.35 m`. Zero preserves the historical exact-support hard boundary.
- `Material Blend Softness`: `0–1`, default `0.75`. Zero is linear; one uses a cubic smooth transition.

The transition remains entirely inside exact Riverbed Support and uses the existing inward-distance stream. At the boundary the Riverbed material weight is zero. It rises to the authored `Riverbed Material Strength` at the configured inward distance.

Inside that same transition band:

- when a Bank Surface Layer is active, the resolved Bank-edge material response occupies the complementary edge weight and interpolates into the Riverbed layer;
- when no Bank Surface Layer is active, the complementary weight remains Primary Ground;
- all material channels use the same substrate weights: palette/form, packed slope/normal, cavity, dry smoothness, dry specular, roughness/finish, and texture-form lighting response;
- Riverbed wetness keeps its existing independent distance/softness controls and formulas;
- exact Riverbed Support continues to own submerged-cover exclusion. Material blending does not re-enable vegetation, snow, frost, or Painted Accents inside support.

For texture-form materials, the old combined binary-cut path remains active outside Riverbed Support and when Material Blend Distance is zero. Within an enabled Riverbed material transition, substrate composition uses the ordinary continuous weight resolver so the new control is authoritative and cannot be collapsed back to a `0.5` hard cut.

### Sparse-rock silhouette contract

- Preserve the fixed substrate, placements, source IDs, scale sequence, exact counts, coverage guardrails, and all frozen rock-response inputs.
- Advance the proof algorithm version from `5` to `6` because runtime payload bytes change.
- Replace maximum-only 2×2 mask downsampling with fractional work-pixel coverage.
- Build one deterministic toroidal seven-tap separable silhouette-coverage field from the fractional mask. Use it only to interpolate substrate form/flat packed detail into rock form/slope/cavity/roughness across a narrow outer band.
- Do not blur or resample internal rock form, normals, edge wear, root response, source identity, placement, or substrate variation.
- Existing Moderate/diagnostic channels remain available; Palette Form and Runtime Packed Detail are the runtime acceptance payloads.
- Add mechanical report evidence for fractional silhouette coverage and maximum adjacent Palette Form step so a return to fully binary payload edges cannot pass unnoticed.

### Idempotent regeneration and installation contract

- Proof generation continues writing the same canonical `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly` paths; repeated runs replace those files.
- The installer continues importing to the same six canonical PNG asset paths, the same dedicated library path, the same three material paths, and the same three Ground-layer paths.
- Existing exact-path textures, library, materials, and layers are updated in place and retain their `.meta` GUIDs.
- Existing material palette and response tuning remains preserved; only required library and stable-ID references are repaired.
- The dedicated `SSDL_SparseRiverbedCandidates` library is installer-owned. A rerun reconciles it to exactly the three canonical stable IDs in canonical order rather than failing or creating a second library.
- Report each canonical item as `Created`, `Updated`, or `Unchanged`; include pre/post GUIDs and fail if an existing main-asset GUID changes.
- Detect same-type assets with the canonical file name outside the canonical path and report them as duplicate-name warnings. Do not automatically delete or rewrite unrelated assets.
- No installer rerun may create a numbered candidate profile, layer, source texture, or library.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the evidence, approved scope, dry-transition math, payload smoothing contract, installer idempotence contract, shared-shader impact, invariants, risks, and validation gates.
2. **Complete — Ground controls/transport:** add serialized dry Material Blend Distance/Softness fields, accessors, null-reset and copy behavior; bind the packed transition vector through `GeneratedGround`; expose both controls in local/shared authoring using the authoritative `GeneratedGroundEditor(6).cs` baseline.
3. **Complete — shader composition:** declare the new hidden ShaderLab/CBUFFER property; derive the inward transition, complementary Bank-edge blend, and continuous-transition selector; apply one common weight set to every material channel while preserving historical binary behavior outside the transition.
4. **Complete — payload smoothing:** advance to algorithm `6`, create fractional downsampled mask and narrow toroidal silhouette coverage, blend only runtime Palette Form/packed payload edges, and add validation/report metrics.
5. **Complete — installer:** require the passing algorithm-6 proof, reconcile the dedicated library in place, add hash/GUID-aware created/updated/unchanged reporting, and detect but do not delete external duplicate names.
6. **Complete — architecture documents:** update the visual, Inspector, and River-coupled canonical contracts; mark M2.4.1 binary ownership as historical outside the retained compatibility path.
7. **Complete for available static checks — final audit:** compare against the reconstructed baseline; reread all modified files and direct consumers; verify exact scope, authoritative editor preservation, property-name parity, C#/HLSL structure, generic-shader guarding, deterministic formulas, no River/geometry changes, no debug growth, installer path/GUID behavior, package reapplication, and all available static checks.

### Acceptance criteria

- Material Blend Distance/Softness are visible under Riverbed `Material Coverage` in both local and shared-style Inspector paths.
- At distance `0`, current exact-support/binary behavior is preserved.
- At nonzero distance, the hard straight Riverbed substrate edge disappears and the complete material response transitions over the authored inward band.
- Active Bank material blends into custom Riverbed material at the boundary; absent Bank material blends Primary Ground into Riverbed.
- Riverbed wetness controls and submerged-cover exclusion remain independent and unchanged.
- Runtime Palette Form and packed slope/cavity edges are fractional and visibly smoother, while placement counts and candidate topology remain unchanged.
- Repeated proof and installer runs update the same canonical outputs/assets, preserve existing main-asset GUIDs and profile tuning, and create no numbered copies.
- The installer report clearly distinguishes created/updated/unchanged items and reports external duplicate-name warnings without mutating them.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual source delta against the reconstructed current baseline is exactly the fourteen approved modified files. No project file was created, deleted, moved, or renamed.

Implemented behavior:

- `GroundMaterialControls` now owns `Material Blend Distance` (`0–2 m`, default `0.35`) and `Material Blend Softness` (`0–1`, default `0.75`), including accessors, null-source defaults, and shared-style copying.
- `GeneratedGround` sends one `_GroundRiverbedMaterialTransition` vector through the existing renderer-local MaterialPropertyBlock.
- The authoritative user-provided `GeneratedGroundEditor(6).cs` baseline exposes both fields under Riverbed `Material Coverage` in local and shared-style authoring. Existing wetness-transition controls remain separate.
- The Ground shader computes an inward Riverbed material weight from exact support and the existing inward-distance stream. Transition activation requires an enabled nonzero Riverbed layer/material, preventing Bank bleed when Riverbed resolves to Primary Ground or Material Strength is zero.
- The complementary transition edge reuses current Bank-zone composition when a Bank layer is active; otherwise normalized composition leaves Primary Ground. One substrate-weight set drives colour/form, slope/normal, cavity, dry smoothness, dry specular, roughness/finish, and texture-form lighting.
- The historical texture-form binary cut remains active at zero blend distance and outside an active Riverbed transition. It is disabled inside an active transition so interpolation remains authoritative. Riverbed wetness and exact-support cover exclusion are unchanged.
- Sparse-riverbed proof algorithm version advanced to `6`. Final masks retain fractional 2×2 work-pixel coverage, and runtime payload edges use a deterministic toroidal seven-tap Gaussian-like silhouette field with a narrow inward smooth transition. Exact placements, source identities, counts, mechanical coverage, and frozen internal rock response remain unchanged.
- The proof reports fractional silhouette coverage and maximum adjacent Palette Form step; the installer now requires a passing `GSU-M2.7C.5E.2`, algorithm-6 report.
- The installer compares source hashes and importer state, updates exact canonical paths, reconciles its dedicated library to the three canonical stable IDs, preserves existing SSMP/GSLP tuning, verifies existing GUIDs, reports created/updated/unchanged state, and warns about same-name external assets without mutating them.

Available checks passed:

- C# lexical scans report zero error tokens and balanced delimiters for all six modified C# files.
- Shader/HLSL delimiter and preprocessor-stack checks pass for all four modified shader files.
- C#/ShaderLab/CBUFFER property-name parity passes for `_GroundRiverbedMaterialTransition`.
- Both `DrawRiverbedResponseSubsection` callers and its declaration have the same 23-argument contract.
- Formula simulation confirms the default transition resolves `0 / 0.1797 / 0.5 / 0.8203 / 1` at `0 / 25 / 50 / 75 / 100%` of the authored distance, while zero distance returns the historical exact-support weight.
- Approximate payload simulation against the prior accepted binary-mask evidence predicts fractional edge coverage and reduces the worst adjacent Palette Form step from above `0.50` to approximately `0.41`, below the new `0.45` guardrail. The actual algorithm-6 Unity proof remains mandatory.
- The authoritative Ground-editor baseline differs only at the approved serialized bindings, two call sites, method parameters, and Material Coverage UI.
- River corridor geometry, four-component Ground varying contract, surface-library/profile schema and builder, frozen Generated Mass baker/validator, `MassGenerator`, `MeshData`, and vegetation source remain byte-identical to the reconstructed baseline.
- Shared property-dependent response functions remain inside `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`; generic Pixel Surface fallbacks remain property-free.
- Whitespace, conflict-marker, canonical-path, and exact-scope checks pass.

No Unity Editor or C# compiler is available in this environment. Unity compilation, algorithm-6 proof output, installer execution, GUID verification against real `.meta` files, and production-camera visual acceptance remain pending in Unity `6000.5.0f1`.

### Risks and validation limits

- A wide material transition can visibly reduce sparse-rock occupancy near the Riverbed edge. This is intentional authored blending, not tile modification; the control allows a narrow or zero transition.
- Bank-edge material reconstruction must respect Bank enable/strength/spatial settings. Incorrectly forcing a full Bank layer could change accepted Bank authoring, so the shader must reuse the current Bank zone composition rather than invent a constant full weight.
- Fractional payload coverage must not be interpreted as changing source-rock count or assembly coverage contracts. Existing exact-count validation remains based on source placements and the accepted mask threshold.
- `PixelSurfaceGroundResponse.hlsl` is shared with the generic Pixel Surface shader; Unity must compile both shaders before acceptance.
- Static analysis cannot prove production-camera appearance, Unity serialization defaults, generated array replacement, or asset GUID retention. Unity execution remains mandatory.

---

## 2026-07-21 — GSU-M2.7C.5E.1.1: Baseline-Safe GeneratedGround Editor Correction

### Status

- Failure diagnosis: **complete**.
- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Corrected fresh-install package: **complete**.
- Existing-project repair script: **complete**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; live Git restore and Unity compilation remain pending**.

### Objective

Correct the M2.7C.5E.1 packaging fault that replaced the user's live `GeneratedGroundEditor.cs` with the complete editor file from the supplied `Assets-Code-Archive(9)` baseline. That archive editor contains vegetation-coverage and `VegetationBenchmark` integrations that are absent from the user's current live `GeneratedGround`/vegetation branch, producing a cascade of `CS1061` and `CS0246` errors.

Preserve the valid paired-payload library support and three-candidate surface installer, but remove `GeneratedGroundEditor.cs` from the distributable source patch. The optional inline Ground-editor recognition of paired Palette Form entries is deferred. Palette and material response remain editable through the generated `SSMP_Riverbed*` assets, while candidate assignment remains available through the user's existing live Ground editor.

### Approved correction scope

Corrected fresh-install package modifies only:

- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Corrected fresh-install package creates only:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs.meta`

Explicitly excluded from the corrected package:

- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`

External repair/support files at the patch-package root:

- `Apply_GSU_M2_7C_5E_1_1_Correction.ps1`
- `README_GSU_M2_7C_5E_1_1.md`
- `GSU_M2_7C_5E_1_1_Audit.md`

Delete/move/rename inside `Assets`: none.

### Reviewed evidence and diagnosis

- User compile log `Pasted text(131).txt`: `GeneratedGroundEditor.cs` reports missing `VegetationCoverageRevision`, coverage storage/painting/raycast APIs, `ShowVegetationCoverageOverlay`, and `VegetationBenchmark`. The errors are all emitted from the editor file replaced by M2.7C.5E.1.
- Original M2.7C.5E.1 package: `GeneratedGroundEditor.cs` SHA-256 `38ee975b107e33a67bae0879122df9a46db92d6d2615bafde4c24c17c8c8f188`.
- Supplied `Assets-Code-Archive(9)` editor baseline: SHA-256 `dd0dc9027ae3d3d7f7bed60e7fdb48b7e49fd10b5ff8d1d5fe242a9eff4decdb`.
- Direct diff between those two files proves the riverbed patch itself changed only one capability call: `EntryUsesAuthoredMaterialSet` → `EntryUsesTextureForm`. The vegetation implementation was not introduced by that one-line delta; it was inherited by shipping the entire archive-baseline editor file into a different live branch.
- Search across the other M2.7C.5E.1 source files finds no `VegetationCoverage*` or `VegetationBenchmark` dependency. The compile cascade is isolated to the replaced full editor file.
- The generated material-profile editor already uses `EntryUsesTextureForm`, so Base/Dark/Light/Cavity and texture-form controls remain available by selecting each `SSMP_Riverbed*` profile asset even when the live embedded Ground editor retains its pre-patch capability behavior.
- The actual live pre-patch `GeneratedGroundEditor.cs` was not supplied. Reconstructing it from the archive would repeat the baseline error and risk deleting unrelated live-branch editor work.

### Correction architecture

- The corrected source package never contains `GeneratedGroundEditor.cs`.
- For a fresh M2.7C.5E.1 installation, the user's current editor file remains byte-for-byte untouched.
- For a project where the faulty package is already applied, the root-level PowerShell repair script:
  - requires a Git working tree;
  - verifies that the current editor file has the exact known contaminated M2.7C.5E.1 hash before touching it;
  - creates a backup beneath `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedSurfaceInstall/GSU_M2_7C_5E_1_1_Backup`;
  - restores only `GeneratedGroundEditor.cs` from the current branch `HEAD` using `git restore --source=HEAD --worktree`;
  - aborts rather than overwriting when the file hash differs, Git is unavailable, or the branch does not contain the file.
- The script intentionally does not reapply the one-line inline-editor convenience change. That change requires a separately reviewed patch against the user's actual live editor source.

### Preserved behavior

- `PrepackedDetailWithTextureForm` paired-payload source mode remains active.
- Packed-detail and Palette Form array construction remains active.
- Runtime material profiles continue to resolve paired texture-form entries.
- `StylizedSurfaceMaterialProfileEditor` continues exposing palette/form controls.
- The three-candidate installer and generated surface/layer assets remain unchanged.
- No shader/HLSL, runtime Ground, River, vegetation, scene, prefab, tag, layer, or component behavior changes.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the compile evidence, exact contaminated hash, live-source limitation, corrected package scope, guarded repair procedure, deferred inline-editor enhancement, and validation.
2. **Complete — corrected source package:** rebuild M2.7C.5E.1 from the accepted M2.7C.5D.5 baseline while omitting `GeneratedGroundEditor.cs`; retain the other paired-payload and installer files unchanged.
3. **Complete — repair script:** add a hash-guarded Git restore for projects where the contaminated editor file is already present.
4. **Complete — architecture documents:** correct the Inspector ownership statement so palette editing is guaranteed through `SSMP_Riverbed*`; embedded Ground-editor exposure is branch-dependent and not part of this corrected patch.
5. **Complete for static checks — audit:** verify corrected package contents, exact source delta, contaminated-file exclusion, repair-script guard behavior, protected file hashes, and package reapplication.

### Acceptance criteria

- The corrected zip contains no `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs` entry.
- Applying the corrected zip to an M2.7C.5D.5 baseline changes only the eight approved existing files and creates only the installer `.cs/.meta` pair.
- The guarded repair script changes only the contaminated editor file and only when its SHA-256 matches the known faulty package file.
- The repaired editor file is restored from the user's current branch `HEAD`, not from `Assets-Code-Archive(9)`.
- All other M2.7C.5E.1 paired-payload and installer behavior remains present.
- Palette tuning remains available through the generated material-profile assets.
- Unity compilation and installer execution pass in the user's live project.

### Risks and validation limits

- The repair script assumes the faulty package was copied into a Git working tree without committing it. If the contaminated file has been committed or modified after application, the hash guard aborts and the user must restore the correct file from the appropriate commit or backup manually.
- The actual live `GeneratedGroundEditor.cs` is unavailable in supplied evidence; no full replacement can be safely authored here.
- Omitting the one-line embedded-editor capability change means paired Palette Form controls may not appear inline inside the Ground Inspector on branches whose editor still checks authored-material entries only. This does not block runtime rendering or profile-asset editing.
- Unity compilation is unavailable in this environment and remains pending.

---

## 2026-07-21 — GSU-M2.7C.5E.1: Three-Candidate Runtime Surface Comparison

> **Delivery superseded by M2.7C.5E.1.1.** The original package scope below included a full `GeneratedGroundEditor.cs` from an incompatible archive baseline. The corrected delivery excludes that file while preserving the paired-payload library and installer.

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Paired payload library support: **complete for the approved source change**.
- Three-candidate surface installer: **complete for the approved source change**.
- Editor/profile/validation integration: **complete for the approved source change**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation, installation, and in-scene comparison: **pending in Unity 6000.5.0f1**.

### Objective

Promote all three accepted sparse-riverbed geometry candidates into selectable reusable Ground surface layers so their actual game-camera appearance can be compared before choosing a density. The runtime surfaces must consume the M2.7C.5D.5 colour-neutral `PaletteForm + RuntimePackedDetail` payload pair, preserve editable Base/Dark/Light/Cavity palette controls, and avoid creating redundant assets for proof-only Neutral/Higher-Contrast/Alternate palette previews.

The promoted candidates are:

- Ultra Sparse Riverbed — exact six-placement payload;
- Very Sparse Riverbed — exact nine-placement payload;
- Sparse Riverbed — exact twelve-placement payload.

### Approved file scope

Modify only:

- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create only:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs.meta`

Delete/move/rename: none.

The installer is authorized to generate/update these project assets when explicitly run in Unity:

- `Assets/Game/Demo/Profiles/SurfaceMaterials/SparseRiverbedCandidates/SSDL_SparseRiverbedCandidates.asset`;
- six editor-only source PNG assets under `Assets/Game/ArtSources/Editor/SurfaceMaterials/SparseRiverbedCandidates` — one Palette Form and one Runtime Packed Detail texture for each candidate;
- `SSMP_RiverbedUltraSparse.asset`, `SSMP_RiverbedVerySparse.asset`, and `SSMP_RiverbedSparse.asset`;
- `GSLP_RiverbedUltraSparse.asset`, `GSLP_RiverbedVerySparse.asset`, and `GSLP_RiverbedSparse.asset`.

### Reviewed evidence and current contracts

- `Assets/AGENTS.md`: complete read-only review, canonical plan as first write, exact-scope implementation, final compliance audit, Unity 6000.5.0f1, and no false validation claims.
- Authoritative reconstructed source: `/mnt/data/Assets-Code-Archive(9).zip` plus accepted M2.7C.5D.3, M2.7C.5D.4, and M2.7C.5D.5 packages. No `.git` directory is supplied; branch, `HEAD`, history, and live working-tree state are unavailable.
- User request: compare every density candidate in the actual game rather than selecting from evidence images. The palette must remain editable after import.
- `GeneratedMassSparseRiverbedTileAssembler.cs` and `GeneratedMassSparseRiverbedTileAssemblyValidation.cs`: M2.7C.5D.5 generates deterministic per-candidate `*_PaletteForm.png` and `*_RuntimePackedDetail.png` outputs under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly`.
- `StylizedSurfaceDetailLibrary.cs`: current source modes are `PrepackedDetail` and `AuthoredMaterialSet`. There is no direct paired prepacked-detail plus pre-normalized texture-form source contract.
- `StylizedSurfaceDetailLibraryBuilder.cs`: current rebuild logic either copies one prepacked source into the packed array or derives packed/form arrays from five authored material maps. Using the authored path would renormalize the proven Palette Form and is prohibited by the M2.7C.5D.5 hierarchy contract.
- `StylizedSurfaceMaterialProfile.cs`, `PixelSurfaceMaterialDetail.hlsl`, `PixelSurfaceGroundForwardPass.hlsl`, `GeneratedGround.cs`, and `GroundSurfaceLayerProfile.cs`: runtime already supports independent packed detail, texture-form sampling, editable Base/Dark/Light/Cavity palette, Texture Form Strength, Scene Lighting Response, roughness variation, world scale, normal/cavity response, and material-property refresh. No shader or runtime sampling change is required.
- `StylizedSurfaceMaterialProfileEditor.cs`, `StylizedSurfaceMaterialValidation.cs`, and `GeneratedGroundEditor.cs`: current texture-form UI and validation detect only authored-material-set entries and must recognize the new paired prepacked source mode.
- Existing GSLP assets and the Riverbed custom-surface selector prove that additional `GroundSurfaceLayerProfile` assets can be compared without a new runtime component, scene edit, tag, layer, or shader branch.

### Paired payload library contract

- Add `PrepackedDetailWithTextureForm` as a third `StylizedSurfaceDetailSourceMode`.
- A paired entry owns:
  - one linear readable mipmapped prepacked RGBA texture for slope/cavity/roughness;
  - one sRGB readable mipmapped grayscale Palette Form texture already normalized around the Base pivot.
- The builder copies both source mip chains without authored-material normalization or conversion.
- Generated texture-form slice mapping and runtime `UsesTextureForm` behavior must include both authored-material-set and paired-prepacked entries.
- Existing `PrepackedDetail` and `AuthoredMaterialSet` behavior, serialization values, generated arrays, signatures, and material resolution must remain unchanged.
- Existing shader/HLSL contracts remain untouched.

### Installer contract

- Add one explicit menu action:

```text
Tools > PS3D > Install All Sparse Riverbed Surface Candidates
```

- Require a current M2.7C.5D.5 proof report and all six payload PNGs under the existing Library evidence directory. Abort before project mutation when the proof is missing, failed, or not algorithm version 5.
- Copy the six payloads into the dedicated editor-only ArtSources folder and normalize importers:
  - Palette Form: sRGB, readable, Repeat, bilinear, mipmapped, uncompressed, 1024 maximum size;
  - Runtime Packed Detail: linear, readable, Repeat, bilinear, mipmapped, uncompressed, 1024 maximum size.
- Create or update one dedicated three-entry detail library. Abort on unexpected entries or path/type conflicts.
- Rebuild and verify the packed and texture-form arrays and each stable-ID mapping.
- Create three material profiles and three Ground layer profiles. Initial values are set only on first creation; reruns preserve user palette and material tuning.
- Use the M2.7C.5D.5 Higher Contrast palette as the initial editable palette:
  - Base `(0.517, 0.503, 0.458)`;
  - Dark `(0.090, 0.100, 0.095)`;
  - Light `(0.640, 0.620, 0.550)`;
  - Cavity `(0.045, 0.041, 0.034)`.
- Initial comparison values: Texture Form Strength `1.0`, Scene Lighting Response `0.60`, Roughness Variation `1.0`, Detail World Scale `8.0 m`, Detail Normal Strength `0.85`, Detail Cavity Strength `1.0`, Detail Cavity Bias `0.15`, Dry Smoothness `0.16`, Dry Specular Strength `0.05`, zero legacy pixel-cell influence, and zero Ground-layer pixel contrast.
- Write one installation report under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedSurfaceInstall` and copy it to the clipboard.

### Invariants and non-goals

- Do not choose or promote one density over the others.
- Do not create nine palette-variant surfaces. Palette previews are parameter examples; each of the three density surfaces exposes editable palette controls.
- Do not modify the M2.7C.5D.5 generator, frozen rock library, shaders/HLSL, `GeneratedGround` runtime, River runtime, existing scenes/prefabs/material profiles/layers, tags, layers, or components.
- Do not automatically assign any candidate to a scene Ground object.
- Do not overwrite user-tuned material/layer values on installer reruns.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the approved runtime-comparison scope, reviewed contracts, paired payload architecture, installer outputs/defaults, invariants, risks, and validation.
2. **Complete — detail-library profile:** added the paired source mode, serialized Palette Form source, generic texture-form capability helpers, and backward-compatible resolution methods.
3. **Complete — detail-library builder:** validates, imports, copies, maps, and signs paired packed and Palette Form sources without renormalization while preserving existing modes.
4. **Complete — profile/editor/validation consumers:** recognize generic texture-form entries in runtime profile capability, profile preview/UI, material validation, and embedded Ground material controls.
5. **Complete — installer:** implemented the one-button idempotent proof-output import, asset creation/update, array rebuild, verification, report, and clipboard flow.
6. **Complete — architecture documents:** record the three-candidate runtime comparison and paired payload source mode without changing shader/runtime ownership.
7. **Complete for available static checks — post-change audit:** reread all modified/created files and direct contracts, compared final scope to baseline, ran available syntax/reference/asset-path checks, and recorded pending Unity gates.

### Acceptance criteria

- Existing source-mode integer values `0` and `1` retain their behavior; paired mode is additive.
- All three stable IDs resolve both packed and texture-form slices after rebuild.
- The installer creates or verifies exactly three SSMP and three GSLP assets, with no automatic scene assignment.
- Palette and material controls remain editable and survive installer reruns.
- The three layers appear as selectable `GroundSurfaceLayerProfile` assets and can be applied through the existing Custom Riverbed Surface Layer control.
- No shader/runtime sample, draw call, renderer, geometry, component, tag, layer, or per-frame process is added.
- Final source delta remains inside the approved file scope.

### Performance and storage impact

- The runtime shader path is unchanged. A selected sparse-riverbed material still binds one packed-detail array and one texture-form array and uses the existing texture-form sampling path; this patch adds no draw call, renderer, geometry rebuild, component update, or per-frame CPU process.
- The temporary comparison library retains three `1024×1024` RGBA32 packed slices and three `1024×1024` RGBA32 texture-form slices. Including full mip chains, the analytical GPU texture payload is approximately `32 MiB` before Unity object/driver overhead: `2 arrays × 3 slices × 1024² × 4 bytes × 4/3`.
- Editor-only source PNGs remain beneath an `Editor` folder and are not runtime source references. The generated arrays are serialized as sub-assets of the runtime library.
- **PERFORMANCE EXCEPTION — comparison-only three-candidate library:** retaining all three densities in one library is authorized for side-by-side Editor/game-camera selection. After a winner is frozen, a follow-up cleanup should compact the production library to one slice pair, reducing the analytical payload to approximately `10.7 MiB` including mips.

### Post-change implementation and audit evidence

Actual source delta against the reconstructed accepted M2.7C.5D.5 baseline:

Modify:

- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedSurfaceInstaller.cs.meta`

Delete/move/rename: none.

Implemented behavior:

- added additive source mode value `2`, `PrepackedDetailWithTextureForm`; existing values `0` and `1` are unchanged;
- paired entries copy the exact linear packed mip chain and exact sRGB Palette Form mip chain into the existing generated arrays;
- runtime/profile/editor capability now resolves texture form from either authored material sets or paired prepacked payloads;
- one explicit installer verifies a passing local M2.7C.5D.5 algorithm-5 proof before mutation, imports all six payloads, creates/verifies one three-entry library and three SSMP/GSLP pairs, rebuilds and verifies both slice mappings, preserves existing tuning on reruns, writes one report, and performs no scene assignment;
- no shader/HLSL, GeneratedGround runtime, River runtime, scene, prefab, component, tag, or layer changed.

Available static checks passed:

- exact tree comparison reports only the approved nine modified files and two created installer files;
- delimiter/string/comment lexical scanning passed for every modified/created C# file;
- all installer `SerializedProperty` names were matched against current profile, layer, and library serialized fields;
- generic texture-form references and remaining authored-only branches were reviewed; authored-only processing remains confined to the five-map conversion path;
- `GeneratedGroundEditor.cs` retains its original CRLF line endings and has one intentional method-call change;
- frozen D5 generator/validator, shaders/HLSL, `GeneratedGround.cs`, `GroundSurfaceLayerProfile.cs`, Generated Mass sources, and existing assets are byte-identical to the baseline;
- no C# compiler or Unity Editor exists in this environment, so compilation, importer behavior, array generation, installer execution, selector discovery, memory measurement, and visual comparison remain pending.

### Risks and pending validation

- The user-supplied archive contains the M2.7C.5D.4 report rather than an M2.7C.5D.5 Unity report. The installer therefore verifies the live project’s local M2.7C.5D.5 report and payload files at execution time; static packaging cannot prove they exist.
- Detail World Scale `8.0 m` is an explicit first-comparison default derived from the generated rock diameter relative to the 1024 tile. It remains editable and requires production-camera validation.
- Unity compilation, installer execution, generated-array verification, selector appearance, and game-camera comparison are unavailable here and remain pending.

---

## 2026-07-21 — GSU-M2.7C.5D.5: Palette-Neutral Sparse Riverbed Payload Proof

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Palette-neutral payload implementation: **complete for the approved source change**.
- Validation/report implementation: **complete for the approved source change**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation and one-button evidence run: **pending in Unity 6000.5.0f1**.
- Visual payload/palette acceptance: **pending**.

### Objective

Preserve the accepted M2.7C.5D.4 substrate structure and M2.7C.5D.3 exact-count sparse rock architecture, but stop treating the finished RGB Moderate preview as the prospective runtime colour source. Generate a colour-neutral grayscale palette-form payload and a runtime-compatible packed structural payload once, then prove that multiple Base/Dark/Light/Cavity palettes can recolour the same unchanged candidate data without rerunning Generated Mass placement or substrate generation.

The user also requested slightly more bright/dark separation. This patch must demonstrate that contrast increase through palette selection rather than by increasing substrate-noise amplitude and risking renewed macro repetition.

### Approved file scope

Modify only:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create: none.

Delete: none.

Move/rename: none.

Generate inside `Assets`: none. All proof outputs remain under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly`.

### Reviewed evidence and current contracts

- `Assets/AGENTS.md`: mandatory read-only review, persistent canonical plan as first write, exact approved scope, final compliance audit, Unity `6000.5.0f1`, and no false validation claims.
- Reconstructed approved source baseline: `/mnt/data/Assets-Code-Archive(9).zip` plus accepted M2.7C.5D.3 and M2.7C.5D.4 patch packages. No `.git` metadata is supplied; branch, `HEAD`, history, and live working-tree state remain unavailable.
- User-supplied M2.7C.5D.4 Unity evidence: `/mnt/data/GeneratedMassSparseRiverbedAssembly(2).zip`. The report proves algorithm version `4`, repeated-suite identity, shared-substrate identity, exact-count `6 / 9 / 12` placement success, macro-homogeneity gates, seams, root limits, and complete evidence generation. The user visually accepted the substrate as much more usable and requested slightly more palette contrast plus colour control without regeneration.
- `GeneratedMassSparseRiverbedTileAssembler.cs`: complete current implementation reviewed. `BuildFinalEvidence` presently writes a finished baked-colour `Moderate` preview plus structural channels, but no single normalized palette-form texture and no packed RGBA runtime payload.
- `GeneratedMassSparseRiverbedTileAssemblyValidation.cs`: complete current validator reviewed. It writes and validates existing proof outputs only; it cannot prove that multiple palettes share one immutable form/packed payload.
- `StylizedSurfaceMaterialProfile.cs`: complete profile reviewed. Existing runtime colour authority is `BaseColor`, `DarkColor`, `LightColor`, and `CavityColor`; `TextureFormStrength` controls grayscale form influence.
- `GroundSurfaceLayerProfile.cs` and `GeneratedGround.ApplySurfaceProfileMaterialProperties`: reviewed. Existing Ground/Bank/Riverbed profile refresh writes palette colours and form controls through material property blocks without regenerating geometry.
- `PixelSurfaceMaterialDetail.hlsl`: complete detail decode/palette contract reviewed. Runtime packed RGBA semantics are `R/G = signed slope`, `B = cavity`, `A = roughness`; a separate normalized grayscale form drives Dark/Base/Light interpolation.
- `PixelSurfaceGroundForwardPass.hlsl`: reviewed at Bank/Riverbed detail and palette application. Existing shader flow resolves the profile palette at render time.
- `StylizedSurfaceDetailLibrary` and `StylizedSurfaceDetailLibraryBuilder`: reviewed. Current source modes do not yet expose a direct prepacked-detail-plus-separate-form import path; runtime asset promotion remains a later separately approved M2.7C.5E integration concern. This proof must produce the exact paired payload and demonstrate recolouring without modifying that runtime library in the current scope.
- Frozen Generated Mass baker/material/source contracts were rechecked. No frozen source, edge-wear, projection, placement, shader, runtime Ground, or River change is required.

### Palette-form payload contract

- Add one `PaletteForm` grayscale image per candidate. `0.5` is the palette Base pivot; values below `0.5` move toward Dark and values above `0.5` move toward Light. Linear form values are gamma-encoded into the PNG so an sRGB form texture decodes to the intended linear payload at shader sample time.
- The accepted homogeneous substrate structure remains unchanged. Its palette form is centred above the Base pivot so the substrate occupies the Base-to-Light range with restrained micro variation.
- Rock form is derived deterministically from the accepted frozen Moderate scalar response using fixed luminance anchors, then remapped predominantly into the Dark-to-Base range. This preserves the accepted rock plane/root/wear hierarchy while removing RGB colour authority from the payload.
- The form mapping is fixed and candidate-independent. Palette changes must not alter `PaletteForm`, placement, mask, height, normals, root, wear, or source ownership.

### Runtime-packed payload contract

Generate one `RuntimePackedDetail` RGBA image per candidate matching `PS3D_DecodeStylizedSurfaceDetail` / authored material-set packed semantics:

- `R`: signed world-X slope encoded from `[-1,1]` to `[0,1]`;
- `G`: signed world-Z slope encoded from `[-1,1]` to `[0,1]`;
- `B`: normalized root/contact cavity payload;
- `A`: bounded roughness payload derived from existing substrate/rock variation, root, and wear evidence.

Flat substrate pixels use neutral slope `0.5 / 0.5` and zero cavity. This payload is colour-neutral and remains identical for every palette preview.

### Palette proof contract

Generate three previews from the same immutable `PaletteForm` and `RuntimePackedDetail`:

1. **Neutral:** close to the accepted M2.7C.5D.4 scene-compatible greige/stone relationship.
2. **Higher Contrast:** the recommended modest increase in Dark/Light separation without changing form/noise amplitude.
3. **Alternate:** a visibly different but restrained palette proving runtime recolour capability.

The C# preview resolver must mirror the existing HLSL palette path: form selects Dark/Base/Light, broad cavity mixes toward Dark, and cavity core mixes toward Cavity Color. Generate a compact comparison sheet and a 3×3 Higher Contrast preview for repetition review.

### Validation contract

- Validate dimensions and presence of `PaletteForm`, `RuntimePackedDetail`, all three previews, comparison sheet, and Higher Contrast 3×3 evidence.
- Validate palette-form range, non-flatness, and separation between substrate and rock means.
- Validate packed-detail neutral substrate slope/cavity, finite channel ranges, and non-empty rock slope/cavity evidence.
- Calculate and report one palette-payload fingerprint from `PaletteForm` plus `RuntimePackedDetail`.
- Calculate and report preview fingerprints and mean colour differences. Neutral, Higher Contrast, and Alternate previews must differ while the payload fingerprint remains unchanged.
- Include all new outputs in candidate/suite deterministic fingerprints.
- Retain every M2.7C.5D.4 source, placement, coverage, scale, hotspot, root, seam, macro-homogeneity, existing channel, mip, report, and clipboard gate.

### Invariants and non-goals

- Do not change candidate IDs, exact placement counts, placement sequence, source ordering, scale sequence, hotspot limits, substrate formula, substrate seed, rock recipes, frozen response, wear normalization, root generation, or existing Moderate evidence.
- Do not modify `StylizedSurfaceMaterialProfile`, `StylizedSurfaceDetailLibrary`, its builder, `GroundSurfaceLayerProfile`, `GeneratedGround`, shaders/HLSL, runtime materials/profiles, scenes, prefabs, components, layers, tags, or Inspector runtime controls.
- Do not create or import runtime assets. M2.7C.5E remains responsible for selected-candidate promotion and any paired-payload library integration.
- Do not select the final `6 / 9 / 12` candidate automatically.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the accepted Unity evidence, existing runtime palette contracts, paired-payload gap, exact proof outputs, invariants, and validation requirements.
2. **Complete — assembler:** advanced the algorithm version; added palette definitions, gamma-encoded form, linear packed payload, palette previews/comparison output, payload/preview statistics and fingerprints; preserved all accepted placement/substrate behavior.
3. **Complete — validator/report:** validates new payload/previews, writes all evidence, includes fingerprints/statistics in the report, and identifies the run as M2.7C.5D.5.
4. **Complete — architecture documents:** record colour-neutral payload ownership, runtime palette authority, higher-contrast palette direction, and unchanged Editor-only/runtime boundary.
5. **Complete for available static checks — post-change audit:** reread final modified files and related contracts; compared against the reconstructed M2.7C.5D.4 baseline; verified exact scope and unchanged frozen/runtime files; ran available syntax, lexical, reference, payload simulation, package-integrity, and protected-file checks; Unity execution remains pending.

### Acceptance criteria

- Existing M2.7C.5D.4 substrate and sparse placement output remains structurally unchanged except for the algorithm-version/fingerprint update and added evidence arrays.
- Every candidate generates deterministic `PaletteForm` and `RuntimePackedDetail` images at `1024×1024`.
- Substrate palette-form mean is visibly above rock palette-form mean; neither band is flat or fully clipped.
- Packed substrate is neutral in slope/cavity while rocks provide non-empty slope and cavity evidence.
- Neutral, Higher Contrast, and Alternate previews have different preview fingerprints and sufficient mean colour differences while sharing one payload fingerprint.
- Higher Contrast changes palette separation only; it does not change substrate noise, placement, mask, height, normals, root, wear, or packed/form payload.
- All previous validation gates remain active.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual project-file delta against the reconstructed approved M2.7C.5D.4 baseline:

Modify:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create/delete/move/rename inside `Assets`: none.

Implemented behavior:

- assembler algorithm version advanced from `4` to `5`;
- candidate definitions, shared substrate seed/formula, exact nested `6 / 9 / 12` placement sequence, source ordering, scales, hotspot/coverage rules, root processing, and frozen Moderate evidence remain unchanged;
- each candidate now retains `PaletteForm`, `RuntimePackedDetail`, Neutral/Higher Contrast/Alternate palette previews, and a four-panel comparison sheet;
- `PaletteForm` stores gamma-encoded grayscale values whose decoded linear payload uses Base pivot `0.5`, substrate centred near `0.62`, and rocks predominantly in the Dark-to-Base band;
- `RuntimePackedDetail` writes world-X/world-Z slope to `R/G`, root/contact cavity to `B`, and bounded roughness to `A`;
- the preview resolver mirrors the existing HLSL Dark/Base/Light and cavity/cavity-core mapping;
- one payload fingerprint is calculated from form plus packed data; each preview receives a separate fingerprint and mean colour-difference evidence;
- payload seam metrics are added to the existing candidate seam gate;
- the validator writes `PaletteForm`, `RuntimePackedDetail`, three preview images, Higher Contrast 3×3 evidence, and the comparison sheet for every candidate;
- no runtime asset/profile/library/shader or scene change is included.

Available static checks passed:

- complete directory comparison against the reconstructed M2.7C.5D.4 baseline reports exactly the five approved modifications and no other project delta;
- lexical scanning found no unterminated string/comment/character token or mismatched delimiter in either modified C# file;
- trailing-whitespace, tab, NUL, and candidate/member reference scans pass;
- every newly referenced candidate, seam, and palette member resolves to the modified assembler contract;
- new helper methods have one definition each and no obsolete M2.7C.5D.4 report identity remains active in code;
- protected runtime/frozen files are byte-identical to the reconstructed baseline, including the projection baker/validator, `MassGenerator`, `MeshData`, `StylizedSurfaceMaterialProfile`, `StylizedSurfaceDetailLibrary`, its builder, `GeneratedGround`, and both reviewed HLSL contracts;
- independent simulation over the user-supplied M2.7C.5D.4 Unity outputs predicts all three candidates pass the new payload gates: decoded substrate form mean approximately `0.620`, rock form mean approximately `0.332–0.336`, separation approximately `0.284–0.289`, packed rock slope mean approximately `0.381–0.397`, cavity mean approximately `0.050–0.066`, Neutral-to-Higher-Contrast mean colour difference approximately `0.0136`, and Neutral-to-Alternate approximately `0.0407`;
- simulated payload seam means are approximately `0.0016` for form, `0.0002` for packed data, and `0.0005` for the Higher Contrast preview.

Performance impact:

- active-gameplay runtime compute, dirty runtime compute, runtime memory, and build storage remain zero because this is still a Library-only Editor proof;
- each retained candidate adds six `1024²` `Color32` arrays (`PaletteForm`, packed detail, three previews, comparison), approximately `24 MiB`; three retained candidates add approximately `72 MiB` before array overhead;
- Higher Contrast 3×3 evidence adds one sequential temporary `3072²` `Color32` array, approximately `36 MiB`, released after encoding;
- this is an Editor-only evidence-storage exception and does not change the dominant `2048²` generation buffers or runtime architecture.

Static checks and simulation are not Unity compilation or execution. Unity compile status, generated PNGs, final fingerprints/metrics, memory/timing, and visual acceptance remain pending the required Unity `6000.5.0f1` proof run.

### Risks and validation limits

- The current runtime detail-library importer does not yet serialize a direct paired prepacked-detail plus pre-normalized form source. D5.5 proves the payload and palette behavior only; M2.7C.5E must resolve selected-candidate asset promotion without renormalizing away the sparse rock form hierarchy.
- Palette previews can prove parameterized recolouring but cannot prove final in-scene lighting response. Runtime profile integration and scene comparison remain later gates.
- Unity compilation, generated evidence, and visual acceptance cannot be claimed from static simulation.

---

## 2026-07-21 — GSU-M2.7C.5D.4: Micro-Noise Homogeneous Substrate Refinement

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Substrate implementation: **complete for the approved source change**.
- Validation/report implementation: **complete for the approved source change**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation and one-button evidence run: **pending in Unity 6000.5.0f1**.
- Visual substrate and candidate acceptance: **pending**.

### Objective

Keep the accepted M2.7C.5D.3 exact-count sparse rock placement architecture, but replace the visually repetitive shared substrate with a more homogeneous micro-noise muddy field that survives 3×3 repetition, zoom-out, and scene integration beside the existing Ground and PaleRiverSand surfaces. The new substrate must suppress recognizable broad light/dark islands, keep large-area averages flat, and shift almost all visible variation into restrained fine-scale structure.

### Approved file scope

Modify only:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create: none.

Delete: none.

Move/rename: none.

Generate inside `Assets`: none. Evidence remains local under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly`.

### Reviewed evidence and current contracts

- `Assets/AGENTS.md`: mandatory review → persistent plan → exact implementation → final compliance audit; Unity `6000.5.0f1`; exact-scope, evidence, and no-false-validation requirements.
- Supplied authoritative archive: `/mnt/data/Assets-Code-Archive(9).zip`, SHA-256 `55292f20bb2213ab86a0749e1db455cbf40da62816f038b0f509af5a6e88958c`; safe relative paths; no `.git` directory. Branch, `HEAD`, history, and live working-tree state remain unavailable.
- Accepted M2.7C.5D.3 patch package and audit: `/mnt/data/GSU_M2_7C_5D_3_Noise_First_Ultra_Sparse_Scatter_Assembly.zip` and `/mnt/data/GSU_M2_7C_5D_3_Audit.md`; used to reconstruct the current approved baseline before this change.
- User-supplied Unity evidence: `/mnt/data/GeneratedMassSparseRiverbedAssembly(1).zip` and attached `SubstrateOnly`, `SubstrateOnly_3x3`, and in-scene screenshots. The report proves algorithm version `3`, shared-substrate determinism, and exact-count `6 / 9 / 12` placements. The user explicitly rejected the substrate because `SubstrateOnly_3x3` already exposes obvious repeated macro structure and the tone/patch language does not sit cleanly with the scene or PaleRiverSand.
- `GeneratedMassSparseRiverbedTileAssembler.cs`: complete M2.7C.5D.3 implementation reviewed. Rock placement, source uniqueness, hotspot gates, scale distribution, seam ownership, and shared-substrate wiring are correct enough to preserve. The failure is localized to `SubstrateField.Evaluate` and the absence of macro-homogeneity metrics.
- `GeneratedMassSparseRiverbedTileAssemblyValidation.cs`: complete M2.7C.5D.3 validator reviewed. It validates seam and basic luminance statistics, but it does not validate large-window homogeneity; broad patch repetition can pass numerically.
- `GeneratedMassRiverRockProjectionBaker.cs`, `MassGenerator.cs`, and `MeshData.cs`: rechecked to confirm no frozen rock/material/source or shared geometry contract change is needed.
- Canonical architecture and Inspector-boundary documents reviewed at their active M2.7C.5D.3 sections.

### Active correction contract

- Keep the M2.7C.5D.3 exact-count sparse rock placement system unchanged: exact placements `6 / 9 / 12`, nested candidate sequence, unique frozen source IDs, radius-aware spacing, near/broad hotspot limits, mostly-small scales, root caps, deterministic suite comparison, and Editor-only evidence ownership all remain active.
- Replace the shared substrate formula. Eliminate visually dominant low-frequency structure. The new field must use only restrained micro-to-micro-mid seamless value-noise components so the tile reads as one quiet muddy material rather than a composition of bright/dark patches.
- Keep the substrate palette compatible with the scene screenshot: a quiet warm greige / muted muddy beige that is slightly dirtier than PaleRiverSand but not dark mud and not neutral gray fog.
- Add explicit macro-homogeneity measurements. A substrate now fails if large-window block means drift too far from the global mean, even when seam and RMS checks pass.

### Shared substrate replacement contract

- Preserve one fixed shared substrate seed for all candidates and both deterministic suite runs.
- Replace the previous `4 / 12 / 36` broad/medium/fine composition with restrained seamless value noise at approximately `18`, `37`, `79`, `151`, and `281` lattice periods across the tile.
- Weight lower frequencies lightly and higher frequencies more prominently so the final field has quiet micro variation and minimal broad patch identity.
- Use a restrained warm drift only; no named pale/damp lobes, no broad cloudy islands, no directional streak motifs.
- Keep variation-channel ownership for non-rock pixels, but derive it from the same finer-scale substrate field.

### New substrate validation contract

- Retain seam validation, mean luminance, percentile luminance, spread, RMS contrast, and opposite-edge difference.
- Add block-mean macro-homogeneity metrics using 64×64, 128×128, and 256×256 averaged luminance windows.
- Report and validate:
  - maximum absolute block-mean deviation from the global mean at 64/128/256;
  - RMS block-mean deviation at 64/128/256.
- These metrics exist specifically to catch visually obvious broad patches that still satisfy global contrast/spread ranges.

### Invariants and non-goals

- Do not change M2.7C.5D.3 rock counts, source selection, spacing/hotspot logic, scale distribution, candidate IDs, or retained evidence outputs.
- Do not modify the frozen 18-rock Generated Mass source library, unified/fallback wear targets, baker, projection validator, `MassGenerator`, `GeneratedMass`, or shared mesh contracts.
- Do not modify Ground runtime, River runtime, shaders/HLSL, runtime materials/profiles/scenes/prefabs/components/layers/tags, or Inspector controls.
- Do not create or import runtime textures.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the M2.7C.5D.4 objective, authoritative source/evidence inputs, substrate-only failure mode, approved scope, replacement formula contract, new macro-homogeneity metrics, invariants, and validation limits.
2. **Complete — assembler:** advance the algorithm version, replace the shared substrate formula with the new micro-noise warm-greige field, keep the rock-placement path unchanged, and measure/store the new macro-homogeneity metrics.
3. **Complete — validator/report:** tighten the substrate luminance/contrast ranges around the new palette, add hard macro-homogeneity gates, update the report identity to M2.7C.5D.4, and print the new substrate metrics.
4. **Complete — architecture documents:** record that the sparse rock architecture is preserved while the substrate shifts to a more homogeneous micro-noise field and now has explicit macro-homogeneity validation.
5. **Complete for available static checks — post-change audit:** reread all final modified files; confirm no changes outside the approved scope; compare against the reconstructed M2.7C.5D.3 baseline; verify that the rock-placement architecture is unchanged; run available syntax/lexical/reference checks; and record pending Unity compile/evidence gates.

### Acceptance criteria

- Rock-placement architecture is unchanged from M2.7C.5D.3 except for the algorithm-version increment and substrate-derived evidence values.
- `SubstrateOnly` and `SubstrateOnly_3x3` remain deterministic and seamless.
- Shared substrate mean luminance, percentile spread, and RMS contrast remain inside the narrowed M2.7C.5D.4 ranges.
- Maximum block-mean deviation and RMS block-mean deviation at 64/128/256 remain inside the new macro-homogeneity gates.
- The shared substrate fingerprint remains identical across all candidates and across both deterministic suite runs.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual project-file delta against the reconstructed approved M2.7C.5D.3 baseline:

Modify:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Create/delete/move/rename inside `Assets`: none.

Implemented behavior:

- assembler algorithm version advanced to `4`;
- the exact-count `6 / 9 / 12` rock-placement architecture, unique-source contract, hotspot limits, and scale distribution are unchanged;
- the shared substrate now uses restrained micro-noise periods `18 / 37 / 79 / 151 / 281` with a warm greige base and strongly reduced macro drift;
- substrate-only variation remains shared across candidates, but no broad pale/damp or cloudy patch system remains;
- `SubstrateResult` now records macro mean-deviation and macro RMS-deviation metrics at 64/128/256 block scales;
- the validator now enforces those macro-homogeneity ceilings and reports them alongside the existing luminance and seam statistics;
- report identity strings now reference M2.7C.5D.4.

Available static checks passed:

- complete archive comparison against the reconstructed M2.7C.5D.3 baseline reports exactly the five approved modifications and no other source delta;
- frozen baker, projection validator, `MassGenerator`, and `MeshData` remain byte-identical to the reconstructed M2.7C.5D.3 baseline;
- delimiter balance, lexical error-token scan, and obsolete-symbol/reference scans pass;
- no deleted-script state changed relative to M2.7C.5D.3;
- independent substrate-formula simulation predicts mean luminance approximately `0.503`, P05 approximately `0.494`, P95 approximately `0.511`, spread approximately `0.0165`, RMS contrast approximately `0.0050`, block-mean maximum deviation approximately `0.0080 / 0.0057 / 0.0027`, and block-mean RMS deviation approximately `0.0030 / 0.0020 / 0.0012` at `64 / 128 / 256`.

Static simulation is not Unity compilation or execution. No C# compiler or Unity Editor is available in this environment, so compile status, generated outputs, final reported metrics, timing, memory, and visual quality remain pending the required Unity `6000.5.0f1` run.

### Risks and validation limits

- Numeric macro-homogeneity gates reduce but do not eliminate the need for visual review. `SubstrateOnly_3x3` remains a hard human gate.
- A very homogeneous substrate can become too flat. The retained exact-count sparse rock candidates and mips must still be reviewed against the scene.
- The dominant `2048²` rock-processing cost remains fixed; this patch does not materially alter runtime or Editor working-buffer topology.
- Unity compilation and execution cannot be claimed from static checks and remain pending until the user runs the one-button proof in Unity `6000.5.0f1`.

---

## 2026-07-21 — GSU-M2.7C.5D.3: Noise-First Ultra-Sparse Scatter Assembly

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first project write**.
- Assembly implementation: **complete for the approved source change**.
- Validation/report implementation: **complete for the approved source change**.
- Retired handmade-script reconciliation: **complete — both active `.cs` files removed; matching metas were absent**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation and one-button evidence run: **pending in Unity 6000.5.0f1**.
- Visual candidate acceptance: **pending**.

### Objective

Replace the rejected rock-composition-first M2.7C.5D.2 assembly with one shared, seamless, low-contrast muddy substrate and three exact-count ultra-sparse rock candidates. Rocks are secondary interruptions of the substrate. The active system must not create positive rock pockets, reward clustering through a quiet-block budget, force large accent rocks, equalize source counts, or reframe the toroidal origin for a composed presentation.

### Approved file scope

Modify only:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Delete only when present:

- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs`
- matching `.meta` files, if present.

Create: none.

Move/rename: none.

Generate inside `Assets`: none. Evidence remains local under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly`.

### Reviewed evidence and current contracts

- `Assets/AGENTS.md`: mandatory review → persistent plan → exact implementation → final compliance audit; Unity `6000.5.0f1`; exact-scope, evidence, and no-false-validation requirements.
- Supplied authoritative archive: `/mnt/data/Assets-Code-Archive(9).zip`, SHA-256 `55292f20bb2213ab86a0749e1db455cbf40da62816f038b0f509af5a6e88958c`; safe relative paths; no `.git` directory. Branch, `HEAD`, history, and live working-tree state are unavailable.
- Continuation handoff: `/mnt/data/GSU_M2_7C_5D_3_Noise_First_Ultra_Sparse_Continuation_Handoff_2026-07-21(1).md`; records the frozen 18-rock library, accepted isolated-rock response, M2.7C.5D.1 Unity evidence, rejected M2.7C.5D.2 direction, and latest substrate-first target.
- `GeneratedMassSparseRiverbedTileAssembler.cs`: complete 3,099-line M2.7C.5D.2 implementation reviewed. Confirmed defects:
  - `CandidateDefinitions` are coverage-driven at `4.5% / 6.0% / 7.5%`.
  - `MacroField` builds four positive pockets and three voids.
  - `SelectPlacementCenter` weights normal attempts `82%` toward the positive pocket field.
  - precommit quiet-block enforcement rejects placements that consume new blocks, indirectly rewarding local clustering.
  - `SamplePlacementScale` forces placement two into `1.46–1.55`, and forces other early size classes.
  - `MinimumSourceDiversity = 12` plus `MaximumSourceShare = 12%` is incompatible with a six-to-twelve-rock candidate.
  - `SelectPresentationOffset` optimizes edge crossings, centroid, and quadrant balance despite the tile having no privileged toroidal origin.
  - `SubstrateField` constructs 14 broad lobes and seeds them per candidate, confounding density comparison and preserving recognizable patch composition.
- `GeneratedMassSparseRiverbedTileAssemblyValidation.cs`: complete 937-line caller/validator reviewed. It runs a retained suite first and a non-retained suite second; enforces the superseded quiet, diversity, source-share, forced-scale, presentation-offset, and edge-band contracts; writes per-candidate evidence and clipboard report.
- `GeneratedMassRiverRockProjectionBaker.cs`: frozen contracts reviewed at `FrozenSourceDefinition`, `GeneratedFrozenSource`, `GetFrozenSourceDefinitions`, `BuildCurrentRawFingerprintSnapshot`, `GenerateFrozenSource`, frozen light direction, frozen unified/fallback wear targets, and `EvaluateFrozenModerateMaterial`. No baker change is authorized.
- `MassGenerator.cs`: `Generate` and `GenerateUnifiedEdgeWearPreview` contracts reviewed. No generator change is authorized.
- `MeshData.cs`: complete shared mesh-data validation contract reviewed. No change is authorized.
- Canonical architecture and Inspector-boundary documents reviewed at their active M2.7C.5D.2 sections and retired handmade-workflow statements.
- Live archive inconsistency: both retired handmade candidate `.cs` scripts are present and the validation script still registers `Tools > PS3D > Run Sparse Riverbed Candidate Synthesis`; their `.meta` files are absent from the supplied archive. Current canonical documents state that the workflow is retired and absent. Deleting the two active `.cs` files is approved reconciliation, not unrelated cleanup.

### Replacement candidate contract

Exact placement count is the primary contract. Coverage is a post-raster sanity guardrail rather than a placement target.

| Candidate | Exact placements | Accepted final coverage |
|---|---:|---:|
| Ultra Sparse Riverbed | 6 | 0.25–1.00% |
| Very Sparse Riverbed | 9 | 0.45–1.40% |
| Sparse Riverbed | 12 | 0.65–1.80% |

The three candidates use one deterministic nested placement sequence: the nine-placement candidate retains the first six placements and adds three; the twelve-placement candidate retains the first nine and adds three. The substrate is byte-identical across all candidates.

### Placement and anti-hotspot contract

- Preserve direct toroidal 3D-mesh rasterization, burial, processed height/normals, material variation, lighting, root sectors, edge wear, unified `0.52`, fallback `0.56`, and Moderate rock response.
- Remove `MacroField`, positive pocket/void placement scoring, quiet-block precommit rejection, presentation-origin search, and post-placement array shifting.
- Build one deterministic shuffled source order from the 18 frozen IDs. Because all active candidate counts are below 18, every placement uses a unique source ID and no source-repeat/equalization logic is needed.
- Build one deterministic placement sequence using toroidal dart throwing. Candidate centres are sampled uniformly and accepted only when radius-aware spacing, overlap, coverage guardrail, and local hotspot limits pass. Do not choose the farthest of many samples; accept the first valid deterministic candidate so the result does not become an overly regular maximum-distance packing.
- Minimum centre separation: `(candidate radius + existing radius) × 1.05`.
- Near hotspot radius: `320` work pixels. A proposed centre may have at most one existing neighbour inside this radius.
- Broad hotspot radius: `640` work pixels. Maximum centre count including the candidate is `3 / 4 / 5` for the six/nine/twelve-placement candidates.
- Report minimum normalized neighbour separation, maximum near-neighbour count, maximum broad-neighbour count, and rejection counts for spacing/hotspot/overlap/coverage.
- Retain 32×32 occupied-block and quiet-fraction measurement as descriptive evidence only; it is not a precommit gate and has no hard minimum.

### Scale contract

- Remove all placement-index size overrides and the retired `1.40–1.55` accent class.
- Build one deterministic 12-placement finite scale sequence containing exactly nine small entries in `0.55–0.80` and three medium entries in `0.80–1.05`, then reuse its prefixes for the six- and nine-placement candidates.
- Approved scale range remains `0.55–1.20`; no large or accent placement is forced in this proof. A later visually justified candidate may use the unused `1.05–1.20` large interval only through a separately recorded plan change.
- Each candidate must contain at least `65%` small placements and zero accent placements.

### Shared substrate contract

- Use one fixed substrate seed for all candidates and both deterministic suite runs.
- Replace broad authored lobes with seamless periodic hash-based value noise sampled at approximately `4`, `12`, and `36` lattice periods across the tile.
- Keep colour variation low contrast and predominantly luminance-based, with restrained warm/cool muddy drift. Do not create named pale/damp blobs, photographic speckling, or strong broad motifs.
- Generate shared `SubstrateOnly.png` and `SubstrateOnly_3x3.png` evidence once per retained suite.
- Report substrate mean luminance, 5th/95th percentile luminance, RMS contrast, and opposite-edge mean colour difference.
- Reuse the same substrate pixel array when constructing every candidate Moderate/Variation/StableId background.

### Determinism and memory order

- Preserve two complete same-run deterministic builds and fingerprint comparison.
- Build the non-retained suite first, release it, force collection, then build the retained evidence suite second. This prevents retained first-suite evidence from overlapping the second suite’s highest working-buffer phase.
- Candidate and suite fingerprints must include the algorithm version, exact placement sequence, shared substrate fingerprint, channels, debug output, and mip output.

### Invariants and non-goals

- Do not modify `GeneratedMassRiverRockProjectionBaker.cs`, `GeneratedMassRiverRockProjectionValidation.cs`, `MassGenerator`, `GeneratedMass`, frozen source definitions/fingerprints, source recipes/meshes, accepted isolated-rock channels, or frozen material constants.
- Do not modify Ground runtime, River runtime, shaders/HLSL, texture arrays, material/profile assets, scenes, prefabs, components, layers, tags, or Inspector implementation.
- Do not create or import runtime textures.
- Do not automatically promote a numerical winner. Visual review of substrate, 3×3 repetition, sparse distribution, and mips remains mandatory.

### File-by-file implementation sequence

1. **Complete — canonical plan:** record the authoritative source limitation, complete review evidence, approved scope, count-driven candidates, placement/hotspot/scale/substrate contracts, invariants, risks, and validation.
2. **Complete — assembler:** implement algorithm version 3, exact nested placements, unique deterministic source ordering, radius-aware sparse dart throwing, hotspot metrics, restricted size distribution, shared periodic substrate, shared substrate evidence, and removal of pocket/quiet-gate/reframing code.
3. **Complete — validator/report:** reverse deterministic-suite memory order; update candidate identity/count/coverage/scale/hotspot/source contracts; validate shared substrate statistics/evidence; remove superseded hard gates and report wording; retain frozen-source, determinism, root, seam, output, clipboard, and local-file contracts.
4. **Complete — stale-script reconciliation:** delete the two active retired handmade candidate `.cs` files. Delete matching metas only if present; none are present in the supplied archive.
5. **Complete — architecture documents:** record substrate-first ownership, exact-count nested candidates, no intentional pockets, descriptive-only quiet metrics, shared substrate, and unchanged runtime/Inspector boundary; remove active wording that contradicts the retired handmade-script state.
6. **Complete for available static checks — post-change audit:** reread all final modified files and direct contracts; compare final paths and content against the captured archive baseline; verify exact scope, deleted scripts, no frozen-baker/runtime changes, no stale M2.7C.5D.2 active contract, no unresolved symbols/imports, structural syntax, whitespace, patch integrity, and available compile/static checks. Unity compilation/evidence execution remains pending when unavailable.

### Acceptance criteria

- Exact placement counts are `6 / 9 / 12`.
- Candidate placement sequences are nested and deterministic.
- Unique-source count equals placement count; no source is repeated.
- Final coverage remains inside each candidate guardrail.
- All accepted pairs meet the radius-aware spacing rule.
- Near/broad hotspot metrics remain inside the candidate contract.
- The nested scale prefixes contain `5/6`, `7/9`, and `9/12` small placements; all scales remain `0.55–1.05`; large and accent counts are zero.
- Shared substrate pixels and fingerprint are identical for all candidates and both suites.
- Shared substrate statistics remain inside the validator’s restrained contrast/palette gates and periodic edge tolerance.
- Every placement has non-empty root contact and remains below the frozen assembly root-perimeter cap.
- All channel, substrate, 3×3, debug, mip, seam, deterministic, and report outputs pass mechanical validation.
- The two retired handmade `.cs` files are absent.
- No file outside the approved scope changes.

### Post-change implementation and audit evidence

Actual project-file delta against the supplied authoritative archive:

Modify:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

Delete:

- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs`

Create/move/rename inside `Assets`: none. Matching retired-script `.meta` files were not present in the supplied archive.

Implemented behavior:

- assembler algorithm version advanced to `3`;
- exact nested placement counts are `6 / 9 / 12`;
- all candidates use the same fixed substrate and placement seeds;
- positive pockets, voids, quiet-budget placement rejection, source-count equalization, forced giant rocks, and presentation reframing are absent;
- every placement consumes a unique source ID from one deterministic shuffled 18-source order;
- the finite scale sequence contains `9` small and `3` medium entries, with prefix small counts `5 / 7 / 9` for the three candidates and no large/accent entries;
- first-valid toroidal dart throwing enforces radius spacing, near/broad hotspot caps, overlap, and milestone coverage ceilings;
- shared periodic `4 / 12 / 36`-period value noise owns the low-contrast mud substrate;
- retained evidence adds `SubstrateOnly.png` and `SubstrateOnly_3x3.png`;
- the non-retained deterministic suite runs first, is released, and is collected before retained evidence generation;
- temporary 3×3 evidence arrays are released immediately after PNG encoding.

Available static checks passed:

- complete archive comparison reports exactly the five approved modifications and two approved deletions, with no added project files;
- whitespace/error-marker checks and balanced delimiter checks pass;
- C# lexical scanning reports no error tokens;
- assembler/validator member-reference and obsolete-symbol scans pass;
- no active non-document reference to either deleted handmade script or its retired menu command remains;
- frozen baker, projection validator, `MassGenerator`, and `MeshData` are byte-identical to the supplied archive;
- deterministic scale-sequence simulation confirms small fractions `5/6`, `7/9`, and `9/12`;
- substrate-formula simulation predicts mean luminance approximately `0.454`, P05 approximately `0.411`, P95 approximately `0.500`, RMS contrast approximately `0.027`, and opposite-edge mean difference well below the `0.005` gate.

Static simulation is not Unity compilation or execution. No C# compiler or Unity Editor is available in this environment, so compile status, generated-rock coverage, root-contact gates, complete suite determinism, timing, memory, PNG evidence, and visual quality remain pending the required Unity `6000.5.0f1` run.

### Risks and validation limits

- Exact-count placement can fail if spacing/hotspot/coverage constraints are too strict for a specific generated rock sequence. Failure must be reported; do not silently relax contracts.
- Pure hard-spacing placement can appear too regular. First-valid deterministic sampling and weak/no placement scoring are chosen to avoid maximum-distance packing; visual 3×3 review remains the deciding gate.
- Substrate numeric contrast gates cannot prove visual quality or non-repetition. `SubstrateOnly`, `SubstrateOnly_3x3`, candidate 3×3, and mip evidence remain mandatory.
- The dominant `2048²` processing cost remains fixed. Fewer placements lower variable rasterization/search work but do not remove full-domain filtering. No Unity Profiler measurement is available in the supplied archive.
- Unity compilation and execution cannot be claimed from static checks and remain pending until the user runs the one-button proof in Unity `6000.5.0f1`.

---

## 2026-07-21 — GSU-M2.7C.5D.2: Sparser Composition, Light Mud Substrate, and Presentation Reframing

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first write**.
- Assembly implementation: **complete**.
- Validation/report implementation: **complete**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation and one-button evidence run: **pending in Unity 6000.5.0f1**.
- Visual candidate acceptance: **pending**.

### Objective

Correct the complete-tile composition without changing the frozen 18-rock source library or isolated-rock material response. The new assembly must be materially sparser, use wider weighted rock-size variation, produce broad quiet mud regions with irregular local pockets rather than an evenly filled blue-noise field, choose a better toroidal presentation origin, and replace the fixed dark-brown substrate with a lighter muddy base containing restrained pale-silt and darker-damp patches/streaks.

### Approved file scope

Modify only:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

No file creation, deletion, rename, move, or other project-path change is authorized.

### Reviewed evidence and current contracts

- `Assets/AGENTS.md`: mandatory review → persistent plan → exact implementation → post-change audit workflow; Unity 6000.5.0f1; exact-scope and evidence requirements.
- `GeneratedMassSparseRiverbedTileAssembler.cs`: complete 2,491-line M2.7C.5D.1 implementation reviewed. Relevant findings:
  - candidate targets are `7% / 9% / 11%`;
  - placement scale is uniformly sampled from `0.75–1.25`;
  - `SelectBalancedSource` always selects a minimum-use source, producing nearly identical source counts;
  - `MacroField` contains seven positive Gaussian centers and no explicit quiet/negative regions;
  - placement-center scoring uses macro weight plus spacing, and no final presentation-origin search exists;
  - substrate rendering is a fixed dark blend of `(0.16,0.14,0.11)` and `(0.20,0.18,0.14)`.
- `GeneratedMassSparseRiverbedTileAssemblyValidation.cs`: complete 790-line M2.7C.5D.1 caller/validator reviewed. It runs two suites, validates frozen definitions/current source drift, coverage, quiet blocks, diversity, root perimeter, seams, output dimensions, PNG evidence, and clipboard report.
- `GeneratedMassRiverRockProjectionBaker.cs`: direct consumer contract reviewed at `GetFrozenSourceDefinitions`, `GenerateFrozenSource`, `FrozenDiagnosticLightDirection`, frozen unified/fallback wear targets, and `EvaluateFrozenModerateMaterial`. No change is authorized.
- Canonical Ground documents: active M2.7C.5C.2.2 material freeze and M2.7C.5D.1 Editor-only assembly boundary reviewed.
- Unity result archive `GeneratedMassSparseRiverbedAssembly.zip` and report reviewed:
  - suite fingerprints matched: `3e7670db74e460fb94051e69405d858a1cebff9356e3bfa2e78eba21b2e9ff06`;
  - actual coverage: `7.31% / 9.43% / 11.45%`;
  - placements: `54 / 71 / 87`;
  - quiet-budget rejections: `0 / 0 / 0`;
  - source usage was almost perfectly equal;
  - sole hard failure: `Natural_Sparse_Riverbed/6` root perimeter `65.45%` versus `<65%`.
- Repository limitation: supplied source has no `.git` directory. The layered archive through M2.7C.5D.1 is the authoritative baseline for the approved paths.

### Replacement candidate contract

| Candidate | Target coverage | Accepted coverage | Minimum quiet 32×32 block fraction |
|---|---:|---:|---:|
| Very Quiet Sparse Riverbed | 4.5% | 3.8–5.3% | 82% |
| Quiet Sparse Riverbed | 6.0% | 5.2–6.8% | 76% |
| Natural Sparse Riverbed | 7.5% | 6.7–8.5% | 70% |

The old Dense candidate is retired from the active assembly proof.

### Placement and composition contract

- Preserve direct toroidal 3D-mesh rasterization and all frozen source/material channels.
- Replace uniform `0.75–1.25` scale sampling with a deterministic weighted `0.55–1.55` distribution containing many small/medium stones, fewer large stones, and rare large accents.
- Preserve at least 12 unique source IDs and the 12% maximum source-share gate, but stop forcing near-equal source counts after the diversity floor is reached.
- Replace the all-positive macro field with deterministic irregular positive pockets and explicit quiet/void regions. Quiet-budget enforcement remains pre-commit.
- Preserve occasional isolated stones, but do not evenly fill all available space.
- After placement, choose a block-aligned toroidal presentation offset that minimizes edge-crossing weight, side imbalance, and off-centre composition. Block alignment must preserve the 32×32 quiet-block count.
- Record scale range/distribution and presentation metrics in the report.

### Substrate contract

- Replace the fixed dark-brown substrate with a lighter muddy base.
- Add deterministic toroidal pale-silt and darker-damp patches/streaks at broad scale.
- Keep variation restrained and low-frequency; no photographic noise or high-frequency speckling.
- Show substrate variation in Moderate, 3×3, PlacementDebug, StableIdDebug background, mip evidence, and the Variation evidence background.
- Preserve periodicity and seam validation.

### Root-contact correction

- Preserve the accepted isolated-rock root-generation architecture.
- Add an assembly-only post-process that keeps only the strongest perimeter sectors when a placement exceeds a safe 62% affected-perimeter cap.
- Same-run empty contact and complete/pervasive contact remain hard failures.

### Invariants and non-goals

- Do not change `GeneratedMassRiverRockProjectionBaker.cs`, the 18 source definitions, current source fingerprints, Generated Mass recipes, source meshes, processed rock height/normals, frozen variation/lighting/root/wear logic, unified `0.52`, fallback `0.56`, or Moderate rock response.
- Do not modify `MassGenerator`, `GeneratedMass`, Ground runtime, River runtime, shaders/HLSL, material/profile assets, texture arrays, scenes, prefabs, layers, tags, components, or Inspector implementation.
- Do not create or import runtime textures; outputs remain under `Library`.
- Do not promote a candidate automatically from numerical metrics.

### File-by-file implementation sequence

1. **Complete — plan:** record exact evidence, scope, replacement targets, substrate/composition contracts, invariants, risks, and validation gates.
2. **Complete — assembler:** implement algorithm version 2, replacement candidates, weighted scale sampling, diversity-first/non-equalized source selection, pocket/void macro composition, block-aligned presentation reframing, lighter periodic mud substrate, reportable composition metrics, and assembly-only root-perimeter limiting.
3. **Complete — validator:** update M2.7C.5D.2 identity, candidate expectations, report metrics, root cap, evidence wording, and progression while preserving determinism/source/seam/output hard gates.
4. **Complete — architecture docs:** record the lighter mud substrate, sparse-composition ownership, presentation-origin rule, and unchanged runtime/Inspector boundary.
5. **Complete for available checks — post-change audit:** reread all five final files and direct contracts; compare the complete diff against M2.7C.5D.1 and this plan; verify exact scope, no stale identities, no unresolved members/imports, structural parsing, meta stability, package integrity, and available compiler/static checks. Unity compilation and evidence execution remain explicitly pending when unavailable.

### Acceptance criteria

- Both complete suites produce identical fingerprints.
- All three replacement candidates finish inside their new coverage and quiet-block ranges.
- At least 12 source IDs are used and no source exceeds 12%, without near-perfect forced count equality.
- Weighted scale evidence spans at least `0.60–1.45`, includes at least three occupied size classes, and reports the actual minimum/maximum/mean.
- Presentation edge-band placement fraction is below 35%, and the chosen offset is block-aligned.
- Substrate preview is visibly lighter than M2.7C.5D.1 and contains deterministic pale/dark broad variation without seam failure.
- Every placement has non-empty root contact and remains below the validator’s 62% affected-perimeter cap after assembly correction.
- All expected output dimensions/files and seam metrics pass.
- No project/runtime asset outside the approved five files changes.

### Post-change audit evidence

- Exact project-file comparison against the layered M2.7C.5D.1 baseline reports exactly the five approved modified paths and no created/deleted/renamed project path.
- `GeneratedMassRiverRockProjectionBaker.cs`, its validator, all Mass/runtime/shader/scene/prefab/profile/material paths, and both existing assembler `.meta` files remain byte-identical to the baseline.
- Final assembler constants are algorithm `2`, scale `0.55–1.55`, root perimeter cap `0.62`, edge-band cap `0.35`, and presentation alignment `128` work pixels.
- Final candidate definitions exactly match `4.5% / 6.0% / 7.5%` targets and declared coverage/quiet ranges.
- Structural lexical checks pass for braces, parentheses, brackets, method scope, and unexpected duplicate method names. `git diff --no-index --check` reports no whitespace errors.
- Introduced member/reference scan confirms all new assembler fields and methods are defined and consumed by the validator/report.
- Deterministic mathematical spot checks of the periodic substrate field show average RGB approximately `0.43–0.45 / 0.40–0.42 / 0.32–0.34`, versus the retired fixed substrate near `0.17`, and mean opposite-edge sample deltas below `0.0004`. These are offline formula checks, not Unity-render validation.
- An approximate placement simulation using the implemented random, macro, scale, spacing, quiet, source-share and framing rules reached about `4.56% / 6.28% / 7.59%` coverage with `36 / 52 / 64` placements, all 18 sources, scale ranges near `0.55–1.54`, quiet fractions above the declared minimums, and post-frame edge-band fractions below `11%`. This is a non-mesh approximation and does not replace the Unity run.
- No Unity or C# compiler exists in the execution environment. Unity compilation, exact direct-mesh candidate completion, PNG evidence, seam metrics, and visual acceptance remain pending in Unity 6000.5.0f1.

### Risks

- Wider size variance can make very sparse candidates reach coverage with too few placements to satisfy source diversity. The diversity-first selector and weighted distribution must preserve at least 12 placements/IDs.
- Stronger macro quiet zones can stop coverage early. Candidate sample count and pocket weighting must be sufficient without relaxing the quiet contract.
- Reframing requires a toroidal buffer shift; implementation must shift every raw channel and owner index consistently and use quiet-block-aligned offsets.
- Substrate variation must be periodic; preview seams remain a hard gate.
- The Editor-only 2048² working buffers are expensive. No runtime cost is introduced, but Unity execution time/memory remains pending validation.

## 2026-07-21 — GSU-M2.7C.5D.1: Deterministic Seamless Sparse Riverbed Assembly Proof

### Status

- Read-only review: **complete**.
- Canonical plan: **complete — this section is the first write**.
- Baker contract exposure: **complete**.
- Tile assembler implementation: **complete**.
- Assembly validator/evidence implementation: **complete**.
- Architecture-document updates: **complete**.
- Post-change consistency/compliance audit: **complete for available static checks; Unity execution remains pending**.
- Unity compilation and one-button evidence run: **pending in Unity 6000.5.0f1**.
- Visual candidate acceptance: **pending**.

### Objective

Use only the accepted frozen 18-rock Generated Mass library and the frozen Moderate material response to assemble three deterministic, seamless, Editor-only sparse riverbed tile candidates. This patch proves complete-tile composition, repetition, seams, quiet-space control and mip behaviour. It creates no runtime asset and performs no Ground, River, shader, material-profile, scene, prefab or Inspector integration.

### Approved file scope

Create:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs.meta`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs.meta`

Modify:

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

No other project path is authorized.

### Reviewed evidence and current contracts

- `Assets/AGENTS.md`: mandatory four-gate workflow, exact-scope discipline, Unity 6000.5.0f1 constraints and no false completion claims.
- `GeneratedMassRiverRockProjectionBaker.cs`: complete 2,996-line algorithm-8 implementation reviewed. It owns the frozen 18 definitions, recipe construction, unified/fallback mesh generation, direct triangle rasterization, processed height/normals, material variation, directional response, broken root sectors, processed edge wear, Moderate rendering and fixed-frame burial evidence.
- `GeneratedMassRiverRockProjectionValidation.cs`: complete current caller/validator reviewed. It runs two builds, hard-fails same-run nondeterminism and frozen-setting changes, and reports historical source-geometry drift as warning.
- `Assets/Game/Procedural/Core/MeshData.cs`: complete shared geometry contract reviewed; vertices, triangles, normals, colours and UV2 are available and validated.
- `Assets/Game/Procedural/Masses/GeneratedMass.cs` / `MassRecipe`: reviewed serialized recipe fields and public seed/default contract used by the baker.
- `Assets/Game/Procedural/Masses/MassSurfaceFeatureGenerator.cs`: reviewed the immutable `MassSurfaceFeatureSettings` constructor and channel settings.
- `Assets/Game/Procedural/Masses/MassGenerator.cs`: reviewed `Generate` and Editor-only `GenerateUnifiedEdgeWearPreview` producer paths.
- Canonical Ground documents: reviewed active M2.7C.5C.2.2 freeze, accepted 18-rock library, unified `0.52`, fallback `0.56`, Moderate response and M2.7C.5D progression.
- Accepted Unity evidence: `GeneratedMassRiverRockProjection(6).zip`; both algorithm-8 runs matched fingerprint `3602f47e080f259dbaca468c4881bc21cf7c4435d6ad42b9f258abb1884cd3e1`. Current source fingerprints from that accepted run become the M2.7C.5D source snapshot.
- Repository limitation: no `.git` directory exists in the supplied archive. The layered source archive plus accepted patch packages are the authoritative baseline. Existing unrelated or deletion-only project state must be preserved.

### Frozen source snapshot

The assembler must consume exactly these stable IDs and accepted algorithm-8 raw fingerprints:

- `T-05` `4e48dac913b980279ad9de1f600101f913a21c029b152b7bbe57b193b2cd1a60`
- `T-08` `78fcf374d475208ea4c32e65ccfbf6a1fb317df9d088c4106d9d705f1ac6a402`
- `T-09` `635996ff3a4c8b4b7f2703ad41707d83be761e203d377403d246ebdc517cf072`
- `T-10` `65b5a839947217e0184b9354b4015880f5e300a067ccc6107d8d3ecfb5b461c4`
- `T-11` `6a3eb4a4cc1c0965bfe8c7916de93bfab2d05476fd580a2282cae4395bc5df9a`
- `T-12` `ee826776a0d9b728c4ebc021743d93d5769bdd25cc5ab345b7c2e5ed5b64975e`
- `T-13` `cf6493580bdc3452fe642392a3d5981866ad6020adb6b97293b087b72fc1c08e`
- `T-14` `bd9b4c4ec13b90db6fb40b5c4d1c34adb07156053b2797e64212dc229b91450e`
- `T-15` `66062ce43d3d8873dd63b843e291df104d67e58ba3f6c45df8cf997d78385d51`
- `S-00` `79957201bb069bb0505b16d6a28b4731b4c53778189837b69b48c029293fcd25`
- `S-03` `b5a3742996a9bfe2e1ad9d862df2e292931556cea48ae300cfae054d18c6e7ed`
- `S-04` `03d9c266f760d442a1a48a6704aba1236bbedf6ddc11fef324e6f4c0a37dd3d9`
- `S-08` `973ac749ec7055a97e77c4393caff69cd8395a704b74874a6219ccca062ef1f5`
- `S-09` `8c75d7c749c0ec2e77c13c07e0963452fc051732badcf833acfd994c7437d9ba`
- `S-10` `026ee59f376e35bebed969c9752369ba8ad7a86503a5ac379642790abdbe8329`
- `S-12` `b2dd65fa09df4f79bb0d1cf58151cb2eb5563362b3e1af75cc32e9bf328d2b50`
- `S-13` `e2e076156b9b932fdd4d18d00278ab944b66f5a4004c7bada5f7155f86f5e63b`
- `S-14` `bc5876c956883478f69762728b690c43dbcc72b141d5845905a30b29e8a85f6d`

Historical drift from this snapshot is warning-only when frozen definitions remain unchanged and the current two builds are identical. Same-run drift remains a hard failure.

### Candidate definitions

| Candidate | Target coverage | Accepted coverage | Minimum quiet 32×32 block fraction |
|---|---:|---:|---:|
| Quiet Sparse Riverbed | 7.0% | 6.0–8.0% | 72% |
| Natural Sparse Riverbed | 9.0% | 8.0–10.5% | 66% |
| Dense Sparse Riverbed | 11.0% | 10.0–12.5% | 58% |

The proof uses a 1024×1024 final tile and 2048×2048 working raster. The final evidence is produced by deterministic 2× downsampling; wear uses maximum-preserving reduction.

### Placement and composition contract

- Directly rasterize generated 3D meshes; do not rotate pre-baked 2D stamps.
- Toroidal tile space with wrapped rasterization across all required edge/corner copies.
- Controlled placement variation only: Y rotation `0–360°`, uniform scale `0.75–1.25`, burial `18–32%`.
- Use deterministic macro-region weighting plus best-candidate spacing selection. Do not use a grid or directional sine carpet.
- Enforce quiet macro-block budgets before placement commit.
- Use at least 12 source IDs per candidate.
- No stable ID may exceed 12% of committed placements; immediate local repeats are prohibited.
- Keep broad substrate regions visibly empty; clustered pockets and isolated rocks are allowed.

### Material and channel contract

The assembler may generate only:

- rock mask;
- processed height;
- processed normals;
- deterministic material variation;
- upward exposure;
- directional response used only by the Moderate preview;
- broken root darkening;
- processed edge wear;
- stable-ID debug.

The Moderate rendering contract remains frozen to M2.7C.5C.2.2. No material-response retuning is authorized in M2.7C.5D.1.

### Evidence contract

One menu action must run all three candidates twice and write one clipboard report. Per candidate it must write:

- `<Candidate>_Moderate.png`
- `<Candidate>_3x3.png`
- `<Candidate>_PlacementDebug.png`
- `<Candidate>_StableIdDebug.png`
- `<Candidate>_Mask.png`
- `<Candidate>_Height.png`
- `<Candidate>_Normals.png`
- `<Candidate>_Variation.png`
- `<Candidate>_RootDarkening.png`
- `<Candidate>_EdgeWear.png`
- `<Candidate>_MipContactSheet.png`

Report: `GeneratedMassSparseRiverbedAssemblyReport.txt`, also copied to the clipboard.

### Hard validation gates

- Both full assembly runs must produce identical candidate and suite fingerprints.
- Frozen source definitions/settings must remain exact.
- Missing/invalid mesh or output data fails.
- Coverage and quiet-block limits must pass per candidate.
- At least 12 source IDs must be used; maximum stable-ID share must remain at or below 12%.
- All periodic seam deltas for mask, height, normals, variation, root darkening, edge wear and Moderate preview must remain within declared tolerances.
- All output dimensions and expected files must be complete.
- No runtime or project asset may be created or modified.

### Non-goals and invariants

- Do not change `MassGenerator`, `GeneratedMass`, Generated Mass edge-wear implementation, current projection validator, Ground runtime, River runtime, shaders/HLSL, profiles, texture arrays, materials, scenes, prefabs, layers, tags or Inspector implementation.
- Do not create a runtime texture or import generated evidence into `Assets`.
- Do not promote a candidate automatically from numerical metrics.
- Do not retune isolated-rock edge accents, normals, variation, root contact or lighting in this patch.

### File-by-file implementation sequence

1. **Complete — plan:** record this exact objective, evidence, scope, contracts, risks and validation gates.
2. **Complete — projection baker:** exposed immutable frozen-source metadata, accepted algorithm-8 fingerprints, current snapshot generation, direct generated-mesh access and the frozen Moderate evaluator without changing existing evidence output.
3. **Complete — assembler:** implemented deterministic source caching, direct toroidal triangle rasterization, pre-commit spacing/overlap/coverage/quiet checks, owner-aware periodic processing, three candidates and all evidence buffers.
4. **Complete — validator:** implemented one menu action, two-run suite/candidate determinism, frozen-source hard gates, accepted-snapshot drift warnings, coverage/quiet/diversity/root/seam/output gates, PNG output and clipboard report.
5. **Complete — architecture docs:** recorded the assembly ownership, candidate contracts and Inspector/runtime boundary; M2.7C.5E remains separately gated.
6. **Complete for available checks — audit:** final diff contains exactly the approved eight paths; complete final files and direct contracts were reread; bracket/parenthesis balance, C# lexical error scan, member-reference checks, duplicate-class scan, meta presence and scope comparison passed. Unity compilation and one-button execution remain pending because no Unity/C# compiler is available in this environment.

### Risks and controls

- **High offline cost:** cache one generated mesh per frozen source for each build; runtime cost remains zero.
- **Coverage overshoot:** evaluate projected footprint and quiet-block impact before committing each placement; stop at candidate target.
- **Seam artefacts:** rasterize wrapped copies and validate opposite edges for every exported channel.
- **Visible repetition:** enforce source diversity/share limits and provide StableIdDebug plus 3×3 evidence.
- **Small-scale accent collapse:** use frozen Moderate response and maximum-preserving wear downsampling; inspect mip contact sheets before acceptance.
- **Upstream source drift:** compare against the accepted algorithm-8 snapshot and report warning-only when current-run determinism passes.

### Post-change consistency and compliance result

- Exact project diff: one modified Editor C# file, three modified canonical documents, and four newly approved `.cs`/`.meta` paths; no other project path differs from the reconstructed M2.7C.5C.2.2 baseline.
- `GeneratedMassRiverRockProjectionBaker.cs`: existing Build/material-refinement behaviour is unchanged; additions are read-only source exposure, accepted snapshot metadata and an exact Moderate evaluator.
- New assembler: Editor-only; outputs remain in `Library`; direct meshes are generated through the existing Mass APIs and cached once per suite build.
- New validator: current projection validator remains unchanged; the new menu action owns assembly evidence and clipboard reporting.
- Static validation passed: balanced braces/parentheses/brackets, no C# lexer error tokens, no duplicate class names, all cross-file baker/assembler member references resolved by source inspection, and both new `.meta` files are present with unique GUIDs.
- Mathematical feasibility check: an independent approximate toroidal circle simulation using the recorded targets reached 7.11%/79.59%, 9.20%/75.49%, and 11.03%/69.34% coverage/quiet fractions respectively; this supports but does not replace the Unity mesh run.
- Unavailable validation: Unity compilation, actual Generated Mass raster output, exact performance, final seam values and visual repetition/mip acceptance remain pending. Required next action is the one-button Unity report.

### Acceptance and next gate

M2.7C.5D.1 is not visually accepted until the user selects one complete tile candidate after reviewing Moderate, 3×3, placement/stable-ID debug, channels and mip evidence. Runtime integration remains blocked. The next separately approved phase is `GSU-M2.7C.5E — Runtime Ground Material Integration`.

## 2026-07-21 — GSU-M2.7C.5C.2.2: Material freeze at unified 0.52 and geometry-drift warning policy

- Freeze the isolated-rock material response at unified wear target percentile `0.52` and fallback `0.56`.
- Accept the current S-08 accent behavior as the unified-wear reference; do not pursue further per-rock edge-accent tuning.
- Treat historical raw-geometry fingerprint changes on deterministic unified-preview rocks as **warnings**, not hard failures, while frozen recipes/settings and same-run fingerprints remain stable.
- Keep hard failures for changed frozen recipes/settings, missing outputs, and same-run nondeterminism.
- Authoritative progression now advances to `M2.7C.5D — Seamless Sparse Riverbed Assembly`.

## 2026-07-21 — GSU-M2.7C.5C.2.1: Unified/Fallback Accent Midpoint Calibration

**Status:** implemented in the exact approved five-file scope and statically audited. Unity 6000.5 compilation, evidence generation, and visual response freeze remain pending.

### Objective

Perform one final narrow edge-accent calibration without changing the frozen 18-rock library or any non-accent material channel. Preserve the accepted fragmented pattern seen on unified-wear rock `S-08`, reduce its dominance slightly, and raise the integrated fallback-wear accents on `T-15` slightly. The intended Moderate response remains subtle at small screen sizes.

### Approved files

1. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
2. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs`
3. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
4. `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
5. `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

No file may be created, deleted, renamed, or moved.

### Reviewed evidence and findings

- Complete M2.7C.5C.2 baker, validator, and the three canonical documents were reviewed from `GSU_M2_7C_5C_2_Edge_Accent_Calibration.zip`.
- The complete Unity output archive `GeneratedMassRiverRockProjection(4).zip` was reviewed, including `GeneratedMassRiverRockEdgeAccentCalibrationReport.txt`, catalog evidence, response close-ups, processed wear, burial evidence, and all material channels.
- The Unity run passed deterministic generation with matching catalog fingerprint `00dba6b0f34e1dcbf64332c9df432f388c645cbd10fbb0a71b7056db63e13889`.
- User visual selection identifies `T-15` and `S-08` as the best accent references. `S-08` has the preferred fragmented pattern but is slightly too pronounced; `T-15` has the preferred integrated blend but remains slightly too transparent.
- Report evidence identifies `T-15` as an ordinary Generated Mass fallback rock and `S-08` as unified edge-wear preview geometry.
- `NormalizeProcessedEdgeWear` currently targets the 90th percentile at `0.56` for unified geometry and `0.52` for fallback geometry. This direction matches the observed imbalance: unified accents are stronger while fallback accents are weaker.
- The accepted dual-frequency breakup, two-pixel selective dilation, three-pixel silhouette exclusion, support/core material mapping, root sectors, lighting, variation, burial framing, and source geometry do not require redesign.
- Direct caller/consumer review confirms only `GeneratedMassRiverRockProjectionValidation` consumes the baker. No runtime system consumes these Editor-only outputs.
- Git metadata is absent from the supplied source archive. The layered M2.7C.5C.2 package is the authoritative baseline for the five approved paths.

### Invariants and non-goals

- Preserve all 18 frozen IDs, seeds, recipes, rotations, burial values, raw geometry fingerprints, rasterization, masks, heights, normals, variation, upward exposure, directional response, root-darkening, and burial framing exactly.
- Preserve the current fragmentation frequencies, intermittency thresholds, dilation, silhouette exclusion, and support/core thresholds unless static evidence proves a required correction.
- Do not introduce per-rock ID special cases. Calibration must operate through the existing unified/fallback path distinction.
- Do not add new channels, controls, assets, runtime code, tile assembly, profiles, materials, shaders, scenes, prefabs, layers, tags, or Inspector behavior.
- Moderate remains the production-style target. Strong remains diagnostic. Accents must remain subtle enough to avoid line artifacts when rocks are small on screen.

### Implementation sequence

1. **Plan — complete:** record current evidence, exact scope, invariants, midpoint target, risks, and validation contract before code edits.
2. **Path midpoint — complete:** lowered the unified normalized target from `0.56` to `0.53` and raised the fallback target from `0.52` to `0.56`; source extraction and breakup are unchanged.
3. **Response preservation — complete:** retained the existing Neutral/Moderate/Strong support/core mapping unchanged; midpoint calibration is isolated to per-rock normalization.
4. **Validation/report identity — complete:** bumped the algorithm to version `7`, updated report/log identity to M2.7C.5C.2.1, and named `S-08` unified / `T-15` fallback as the visual references.
5. **Architecture documentation — complete:** recorded the path-level midpoint and retained M2.7C.5D as the next phase only after visual approval.
6. **Post-change audit — complete:** reread all five files and direct contracts, compared the complete diff with M2.7C.5C.2, ran available static/scope checks, and recorded Unity validation as pending.

### Acceptance criteria

- Frozen source definitions and raw geometry fingerprints remain unchanged.
- The existing fragmented pattern remains visible on `S-08`, but its Moderate/Strong accents are slightly less dominant than M2.7C.5C.2.
- `T-15` retains its integrated blend, but its Moderate/Strong accents become slightly more visible than M2.7C.5C.2.
- No outer silhouette outline, new continuous accent segment, or triangle-following material noise is introduced.
- Repeated complete Unity builds produce identical fingerprints and output arrays.
- Exactly the approved five paths differ from M2.7C.5C.2.

### Risks and mitigations

- **Risk — all fallback rocks become too prominent.** Mitigation: use a small target increase and preserve existing bounded per-rock gain.
- **Risk — unified accents become invisible.** Mitigation: reduce the unified target only slightly and keep existing breakup/support behavior unchanged.
- **Risk — response differences are too small to judge.** Mitigation: keep `S-08` and `T-15` explicitly named in the report pending gate and compare the same Moderate/Strong evidence.
- **Risk — accidental scope drift changes accepted processing.** Mitigation: verify the final diff contains only normalization constants, algorithm/report identity, and documentation.

### Validation contract

- Lexical delimiter/string/comment balance and namespace/type-scope sanity checks for both changed C# files.
- Byte-level comparison of frozen definition and raw fingerprint contract blocks against M2.7C.5C.2.
- Diff assertion that edge extraction, breakup, dilation, silhouette exclusion, support/core mapping, root, lighting, variation, height, normals, and burial logic remain unchanged.
- Exact approved-path scope comparison.
- Unity 6000.5 compilation and one-button evidence run remain authoritative and pending.


### Post-change audit evidence

- `GeneratedMassRiverRockProjectionBaker.AlgorithmVersion` changed from `6` to `7`; `RawGeometryFingerprintVersion` remains unchanged.
- `UnifiedWearTargetPercentile` changed from `0.56` to `0.53` (approximately 5.4% lower).
- `FallbackWearTargetPercentile` changed from `0.52` to `0.56` (approximately 7.7% higher).
- The frozen 18-rock library and validator raw-fingerprint contracts are byte-identical to M2.7C.5C.2.
- `BuildProcessedEdgeWear`, dual-frequency breakup, selective dilation, silhouette exclusion, root-contact processing, and `BuildProcessedMaterialColor` support/core mapping are byte-identical to M2.7C.5C.2.
- No per-rock ID conditional was added; the adjustment uses the existing `UsedFallbackMesh` path distinction.
- Both changed C# files pass lexical delimiter/string/comment balance checks. Since the baseline compiled in Unity and the code diff is limited to three numeric constants plus report strings, no new symbol or control-flow dependency was introduced.
- Conflict-marker, whitespace, approved-path scope, exact baker-diff, frozen-contract, and accepted-processing-block checks passed.
- Exactly the approved five files differ from M2.7C.5C.2; no project path was created, deleted, renamed, or moved.
- A Unity/C# compiler is unavailable in this environment. Unity 6000.5 compilation, deterministic one-button execution, and visual midpoint approval remain authoritative and pending.

## 2026-07-21 — GSU-M2.7C.5C.2: Edge Accent Calibration and Material-Response Freeze

**Status:** implemented in the exact approved five-file scope and statically audited. Unity 6000.5 compilation, menu execution, generated evidence, and visual acceptance remain pending.

### Objective

Calibrate the M2.7C.5C.1 interior edge-accent response without changing the frozen 18-rock library, Generated Mass recipes, projection geometry, processed height, processed normals, material variation, directional lighting, root sectors, or fixed-frame burial system. The production-target Moderate response must show subtle but readable internal accents across both native-wear and fallback-wear rocks, while avoiding bright line-art artifacts at small screen sizes.

### Approved files

1. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
2. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs`
3. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
4. `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
5. `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

No file may be created, deleted, or renamed.

### Reviewed evidence and findings

- Complete M2.7C.5C.1 baker, validator, and three canonical documents were reviewed from `GSU_M2_7C_5C_1_Directional_Response_Contact_Sectors_Fixed_Burial.zip`.
- The Unity report `GeneratedMassRiverRockMaterialRefinementCorrectionReport.txt` passed determinism with matching fingerprint `7690c55cb0b47fa889c7352f9c81e08de32a7d20e19392ee4ef022872f65f884`, 18 frozen rocks, fixed burial frames, and 20 PNG evidence outputs.
- User visual evidence accepted the overall material response and burial presentation but identified a cross-path imbalance: fallback-wear rocks such as `T-15` show accents that are too weak, while native unified-wear rocks such as `T-13` show accents that are too bright and continuous.
- `BuildProcessedEdgeWear` currently scales native projected wear by `0.88` and fallback curvature wear by `0.46`, then preserves an intermittency floor of `0.35`. This structurally favors native-wear rocks and leaves long connected segments visible.
- `BuildProcessedMaterialColor` currently applies edge wear directly with maximum strengths `0.20`, `0.48`, and `0.74` for Neutral, Moderate, and Strong, using a bright fixed wear colour. This permits high-valued native wear to read as drawn lines while low-valued fallback wear remains nearly invisible.
- Direct caller/consumer review found only `GeneratedMassRiverRockProjectionValidation`; no runtime system consumes the baker. Generated Mass producers and all frozen source contracts remain unchanged.
- Git metadata is absent from the supplied archive. The accepted M2.7C.5C.1 package is the authoritative baseline for the five approved paths.

### Invariants and non-goals

- Preserve all 18 frozen IDs, seeds, recipes, rotations, burial values, source geometry, raw per-rock fingerprints, processed height, processed normals, variation, lighting, root-darkening, and fixed burial framing exactly.
- Preserve the existing outer silhouette exclusion. Edge accents must remain internal, broken, and tied to projected native wear or processed convex structure.
- Do not add mask-distance outlines, fake cracks, source-rock generation, new material channels, seamless tile assembly, runtime integration, Inspector controls, assets, components, layers, tags, materials, profiles, scenes, or prefabs.
- Strong remains diagnostic. Moderate is the intended production-style response and must remain on the subtle side.

### Implementation sequence

1. **Plan — complete:** record evidence, scope, invariants, risks, acceptance criteria, and validation contract before code changes.
2. **Source balancing — complete:** reduce native-wear dominance and increase fallback convex-wear contribution without changing source geometry or silhouette exclusion.
3. **Segment breakup — complete:** lower the continuous intermittency floor and add deterministic secondary breakup so long uniform accents become discontinuous.
4. **Per-rock normalization — complete:** normalize strong processed-wear percentiles into one restrained target range so native and fallback paths produce comparable accent visibility without amplifying near-zero noise.
5. **Support/core mapping — complete:** replace direct wear multiplication with a broad low-contrast support response plus a thinner bounded core response; use less luminous wear colour and lower Moderate/Strong maxima.
6. **Validation/report — complete:** update algorithm/report identity, preserve frozen contracts and raw fingerprints, enforce deterministic outputs, and report the calibrated accent contract.
7. **Architecture docs — complete:** record the accepted subtle-accent direction and keep M2.7C.5D as the next phase only after visual approval.
8. **Post-change audit — complete:** reread all five final files and direct contracts, compare final diff with M2.7C.5C.1 and this plan, run available syntax/static/scope checks, and mark Unity validation pending.

### Acceptance criteria

- The 18 frozen source definitions and authoritative raw per-rock fingerprints remain unchanged.
- Repeated complete builds produce identical catalog fingerprints and output arrays.
- Moderate shows readable internal accents on fallback examples such as `T-15` without making native examples such as `T-13` appear engraved or outlined.
- Strong remains visibly stronger than Moderate but no longer produces dominant continuous bright slashes.
- Outer silhouette exclusion remains active and no complete perimeter outline is introduced.
- Exactly the approved five files differ from M2.7C.5C.1.

### Risks and mitigations

- **Risk — normalization amplifies numerical noise on rocks with no usable wear.** Mitigation: require a minimum per-rock high-percentile signal before applying gain.
- **Risk — fallback strengthening restores triangle noise.** Mitigation: fallback remains derived only from the already processed mild height and blended normals, with deterministic breakup and silhouette exclusion.
- **Risk — accent reduction makes all lines invisible at distance.** Mitigation: retain a low-contrast support component while limiting the bright core component.
- **Risk — Strong remains line-art-like.** Mitigation: cap both support and core strength, lower wear-colour luminance, and increase segment breakup.

### Validation contract

- Static parse and namespace/type-scope checks for both changed C# files.
- Exact frozen-definition and raw-fingerprint comparison against M2.7C.5C.1.
- Exact project-path scope comparison against the approved five files.
- Unity 6000.5 compilation and one-button evidence run remain authoritative and pending until performed by the user.

### Post-change audit evidence

- `GeneratedMassRiverRockProjectionBaker.AlgorithmVersion` is `6`; `RawGeometryFingerprintVersion` remains `4`.
- The baker and validator frozen-library contract blocks are byte-identical to M2.7C.5C.1.
- `BuildProcessedEdgeWear` now reduces native-wear source gain from `0.88` to `0.60`, raises processed fallback contribution from `0.46` to `0.62`, and applies dual-frequency deterministic breakup with a `0.12` low floor.
- `NormalizeProcessedEdgeWear` uses a 64-bin per-rock histogram, 90th-percentile reference, minimum signal `0.055`, target percentiles `0.56` for unified geometry and `0.52` for fallback geometry, and bounded gain `0.55–2.60`.
- `BuildProcessedMaterialColor` now maps wear through a broad support term plus a bounded core term. Moderate maximum contribution is reduced from direct `0.48` multiplication to `0.13` support plus `0.075` core; Strong uses `0.19` plus `0.11`. Wear colour luminance is also reduced.
- Static brace, class-span, namespace-scope, conflict-marker, frozen-contract, raw-fingerprint-version, approved-path-scope, and whitespace checks passed.
- Exactly the approved five files differ from M2.7C.5C.1; no project path was created, deleted, or renamed.
- A Unity/C# compiler is unavailable in this environment. Unity 6000.5 compilation and the one-button evidence run remain pending and authoritative.

## 2026-07-21 — GSU-M2.7C.5C.1: Directional Response, Contact Sectors, and Fixed Burial Evidence

**Status:** implemented in the exact approved five-file scope and statically audited. Unity 6000.5 compilation, menu execution, generated evidence, and visual acceptance remain pending.

### Objective

Correct the M2.7C.5C evidence mapping without changing the frozen 18-rock source library, Generated Mass recipes, projection geometry, processed height fields, or 78/22 two-scale processed-normal blend. The correction must make Neutral, Moderate, and Strong materially distinct; separate upward exposure from directional lighting; replace root hairlines with broken contact sectors; expose existing interior wear; and make burial depth visually comparable under fixed framing.

### Approved files

1. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
2. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs`
3. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
4. `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
5. `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

No file may be created, deleted, or renamed.

### Reviewed evidence and current findings

- Complete M2.7C.5C baker and validator were reviewed from `GSU_M2_7C_5C_Frozen_Rock_Material_Refinement.zip`.
- Direct producers reviewed: `MassRecipe` in `Assets/Game/Procedural/Masses/GeneratedMass.cs`, `MassSurfaceFeatureSettings` in `Assets/Game/Procedural/Masses/MassSurfaceFeatureGenerator.cs`, and `MassGenerator.Generate` / `MassGenerator.GenerateUnifiedEdgeWearPreview` in `Assets/Game/Procedural/Masses/MassGenerator.cs`.
- Direct consumer review found only `GeneratedMassRiverRockProjectionValidation`; no runtime or unrelated project consumer references the baker.
- Git metadata is absent from the supplied archive. The accepted M2.7C.5C package is the authoritative working baseline for the five approved paths.
- `GeneratedMassRiverRockMaterialRefinementReport.txt` records a deterministic pass with matching catalog fingerprint `a7fbf687cdd929ce129c9f6670c9bd71c5ccec6f7cba410255dcdda4a3e216bd`, 18 frozen entries, and the expected evidence set.
- `BuildProcessedBuffers` currently stores processed-normal upward exposure in `processed.Exposure`; `BuildProcessedMaterialColor` then adds that mostly saturated value to material brightness while separately compressing directional `N·L` into a narrow positive range. This makes Neutral, Moderate, and Strong too similar and too pale.
- `BuildSelectiveRootDarkening` produces narrow low-height results without an inward sector expansion stage, so visual grounding is primarily hairline-sized.
- `BuildProcessedEdgeWear` generates valid interior wear but the final colour mapping is too weak to expose it consistently.
- `BuildBurialComparison` calls `RasterizeRockIntoCell` independently for each burial fraction; `RasterizeMesh` recalculates visible height, bounds, scale, and centering per cell, which removes direct apparent-size and height comparison across burial depths.

### Invariants and non-goals

- Preserve all 18 frozen stable IDs, archetypes, shape seeds, surface seeds, rotations, default burial values, Uneven Broad recipe fields, surface-feature settings, and ordering exactly.
- Preserve raw per-rock geometry fingerprints; bumping the combined evidence algorithm version must not change raw geometry generation or rasterization for the main catalog.
- Preserve the existing three-pass broad height, one-pass mild height, and 78/22 processed-normal blend.
- Do not modify `MassGenerator`, `GeneratedMass`, `MassRecipe`, edge-wear architecture, Ground runtime, River, shaders, materials, profiles, scenes, prefabs, layers, tags, or Inspector controls.
- Do not assemble a seamless riverbed tile or add runtime integration.
- Do not use mask-distance perimeter shading, complete contact rings, fake cracks, or new source-rock generation.

### Implementation sequence

1. **Plan — complete:** record this objective, evidence, invariants, risks, scope, implementation order, and validation contract before code changes.
2. **Evidence contract — complete:** extend result buffers with independent `UpwardExposure`, `DirectionalLightResponse`, and close-up evidence outputs while retaining all existing raw/processed diagnostic channels.
3. **Directional material response — complete:** lower the preview base value, derive signed directional response from processed normals, and define clearly separated Neutral / Moderate / Strong response ranges. Upward exposure remains a restrained secondary weathering term only.
4. **Contact sectors — complete:** build wider deterministic burial-owned contact sectors, expand them inward by a bounded 3–7 source pixels, soften the interior transition, and preserve large unaffected perimeter regions.
5. **Wear presentation — complete:** retain silhouette exclusion, selectively widen strong interior wear by at most two pixels, and apply visible value/desaturation response without outlining the rock.
6. **Fixed-frame burial — complete:** compute one source framing contract per rock and reuse identical X/Z centre, scale, height normalization, rotation, and cell framing for 8/18/28/38% burial variants; only burial plane movement may vary. Add a visible substrate reference.
7. **Close-up evidence — complete:** generate a six-rock `Neutral | Moderate | Strong` sheet for `S-12`, `S-13`, `S-14`, `T-05`, `T-13`, and `T-15`.
8. **Validation/report — complete:** enforce unchanged frozen contracts, repeated fingerprints, expected outputs, fixed burial framing metadata, non-empty contact sectors, and substantial unaffected perimeter.
9. **Architecture docs — complete:** update the two remaining canonical documents to record the corrected response ownership and preserve M2.7C.5D / M2.7C.5E as later gates.
10. **Post-change audit — complete:** reread all five final files and related producer contracts; compare the final diff with the M2.7C.5C baseline and this plan; run all available static checks; record Unity compilation and visual acceptance as pending.

### Risks and mitigations

- **Risk — root darkening becomes another complete outline.** Mitigation: sector ownership, directional breakup, inward-only expansion, explicit unaffected-perimeter validation, and no mask-distance source term.
- **Risk — Strong clips to white and hides planes.** Mitigation: lower base albedo range, signed directional response around a neutral midpoint, bounded highlight multiplier, and separate upward exposure.
- **Risk — wear becomes line art.** Mitigation: keep outer silhouette exclusion, threshold strong interior structure, limit dilation to two pixels, and apply restrained colour response.
- **Risk — fixed burial framing clips deep or shallow variants.** Mitigation: derive framing from the full unburied transformed mesh and reserve fixed projection padding for all four variants.
- **Risk — close-up evidence changes source data.** Mitigation: close-ups reuse existing processed buffers or rerasterize the exact frozen definitions without recipe/seed changes.

### Post-change audit evidence

- Changed project paths are limited to the approved two Editor scripts and three canonical documents; no file was created, deleted, or renamed.
- Both changed C# files parse successfully with the available C# grammar; brace/scope inspection found no method at namespace scope and no duplicate method signature.
- The baker and validator each contain the same 18 frozen IDs/settings in the same order. The validator also freezes the authoritative M2.7C.5C raw per-rock fingerprints.
- The main source-rock generation, recipe construction, mesh generation, raw rasterization, frozen definitions, processed-height passes, and 78/22 processed-normal blend remain unchanged. Raw per-rock fingerprint serialization retains contract version 4 while the combined evidence algorithm advances to version 5.
- New evidence arrays are allocated, labelled, fingerprinted, validated, reported, and written: upward exposure, directional response, response close-ups, and fixed-frame burial metadata.
- The close-up contract is exactly `S-12/S-13/S-14/T-05/T-13/T-15`; the burial contract is exactly `S-12/S-14/T-13/T-15` at 8/18/28/38%.
- Root-contact validation requires non-empty contact, perimeter participation above 0.1%, and affected perimeter below 65%.
- Unity and a C# compiler are unavailable in the delivery environment. Unity compilation, deterministic execution, output generation, and visual judgment are therefore not claimed and remain mandatory user-side gates.

### Acceptance and validation contract

- Exactly the approved five files differ from M2.7C.5C; no project path is added or deleted.
- Frozen library definitions remain byte-for-byte equivalent in values and order.
- Both complete runs produce matching catalog and per-rock fingerprints.
- Raw geometry generation/projection data for the main 18-rock catalog remains unchanged from M2.7C.5C apart from the intentional algorithm-version container fingerprint.
- `UpwardExposure` and `DirectionalLightResponse` are independent outputs.
- Neutral, Moderate, and Strong outputs are all generated and measurably non-identical.
- Every root mask has non-zero contact where eligible, never forms a complete perimeter ring, and preserves a substantial unaffected perimeter fraction.
- Burial comparison contains `S-12/S-14/T-13/T-15`, each at `8/18/28/38%`, with fixed framing metadata for all four depths of one source.
- Close-up evidence contains exactly the six approved stable IDs under all three response modes.
- All evidence remains local under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection`.
- Unity 6000.5.0f1 compilation and visual review remain mandatory final gates.

## 2026-07-21 — GSU-M2.7C.5C: Frozen Rock Library and Material-Response Refinement

Status: **Implemented in the exact approved five-file scope and statically audited. Unity 6000.5 compilation, menu execution, generated evidence, and visual material/burial acceptance remain pending.**

### Objective

Freeze the user-approved 18-source Generated Mass river-rock library, stop seed exploration, and refine only the offline projected material response so the accepted silhouettes retain broad geological planes without raw low-poly triangle noise, full-perimeter root outlines, or line-art edge wear.

### Approved files

Modify only:

1. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
2. `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs`
3. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
4. `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
5. `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

No files may be created or deleted. No Generated Mass, Ground runtime, River, shader, scene, prefab, material, profile, texture-array, layer, tag, or Inspector implementation may change.

### Reviewed evidence

- `GeneratedMassRiverRockFamilySweepReport(1).txt` records a deterministic M2.7C.5B.2 pass with matching catalog fingerprints, 32 Uneven Broad Terrain/Squat sources, seven preserved anchors, and all raw/processed evidence outputs.
- User visual review accepted the existing seven anchors and approved the proposed expanded frozen library.
- Current baker evidence:
  - `GeneratedMassRiverRockProjectionBaker.BuildProcessedBuffers` uses one three-pass height field for processed normals.
  - `BuildProcessedBuffers` still blends raw mesh-derived variation, exposure, and crevice back into filtered channels.
  - `BuildProcessedEdgeWear` may preserve native wear near the silhouette and uses one-pixel interior eligibility only.
  - `BuildCatalogImages` derives root darkening from low height × processed crevice × side-facing normal, but the processed crevice source remains perimeter-heavy.
- Current producer contracts were verified without modification:
  - `Assets/Game/Procedural/Masses/GeneratedMass.cs` — `MassRecipe` serialized recipe fields and public seed setters.
  - `Assets/Game/Procedural/Masses/MassSurfaceFeatureGenerator.cs` — `MassSurfaceFeatureSettings` constructor and clamps.
  - `Assets/Game/Procedural/Masses/MassGenerator.cs` — ordinary and unified edge-wear preview mesh generation APIs.

### Frozen library contract

The active source library is exactly these 18 IDs with the generation/projection values already recorded by M2.7C.5B.2:

- Terrain: `T-05`, `T-08`, `T-09`, `T-10`, `T-11`, `T-12`, `T-13`, `T-14`, `T-15`.
- Squat: `S-00`, `S-03`, `S-04`, `S-08`, `S-09`, `S-10`, `S-12`, `S-13`, `S-14`.

Every frozen entry retains its existing archetype, shape seed, surface seed, burial fraction, rotation, Uneven Broad recipe fields, and edge-wear settings. M2.7C.5C may not regenerate, substitute, reindex, or retune source geometry.

### Implementation plan and acceptance criteria

1. **Plan — complete:** record scope, evidence, frozen IDs, invariants, risks, sequence, and validation requirements here before implementation.
2. **Frozen catalog — complete:** replace the 32-source exploration tables with explicit 18-entry frozen definitions; retain stable IDs and exact settings; use a compact 5 × 4 evidence layout with two unused cells.
3. **Two-scale normals — complete:** derive a strongly filtered volume normal and a mildly filtered broad-plane normal, then blend them with a restrained plane contribution. Raw mesh normals remain diagnostic only.
4. **Non-triangular material variation — complete:** replace processed vertex-colour variation with deterministic per-rock low-frequency patches and restrained grain keyed by the frozen surface seed; raw variation remains diagnostic only.
5. **Normal-derived exposure — complete:** derive processed exposure from the blended processed normal, with at most a small diagnostic contribution from the raw Generated Mass exposure channel.
6. **Selective root darkening — complete:** replace perimeter-heavy processed crevice use with a directional, disconnected burial-root field derived from low processed height, side-facing processed normal, deterministic contact-sector modulation, and a restrained raw-crevice contribution.
7. **Interior edge wear — complete:** suppress the outer 2–3 projected pixels, retain native wear only on eligible interior structure, and generate fallback wear only from genuine convex/normal transitions; no complete silhouette outline is permitted.
8. **Material comparison — complete:** output the frozen catalog in neutral, moderate, and strong stylized response variants plus compact processed data audits and the accepted-anchor burial comparison.
9. **Validator/report — complete:** require exactly 18 frozen IDs and exact settings, deterministic repeat fingerprints, complete expected outputs, no non-frozen source, and no runtime integration.
10. **Architecture docs — complete:** record that source generation is frozen and that M2.7C.5C is material/burial refinement only; preserve M2.7C.5D seamless assembly and M2.7C.5E runtime integration as later gates.
11. **Post-change audit — complete with pending Unity gate:** reread all five modified files and related Mass contracts; compare final diff with this scope; run available syntax/structure/static checks; mark Unity compile and visual acceptance pending unless supplied by Unity.

### Post-implementation consistency and compliance audit

- Final project-file comparison against the reconstructed M2.7C.5B.2 baseline reports exactly five modified files: the two projection scripts and three approved canonical documents. No file was created or deleted.
- The baker and validator both parse as valid C# compilation units with the available tree-sitter C# grammar; brace balance is zero; no method/field is emitted at namespace scope; no duplicate private/static method declaration remains.
- Static contract comparison confirms 18 baker frozen definitions and 18 validator contracts match exactly, with 9 Terrain and 9 Squat sources and the approved stable-ID set.
- Validator member-reference checks found no projection-result or rock-evidence field used without a corresponding current contract member.
- The final baker retains only the existing `MassRecipe`, `MassSurfaceFeatureSettings`, `MassGenerator.Generate`, and `MassGenerator.GenerateUnifiedEdgeWearPreview` producer contracts. Those related producer files remain byte-identical to the M2.7C.5B.2 baseline.
- Deterministic formula sampling confirms the new material-variation field stays restrained for representative frozen seeds and the directional contact-sector function covers only approximately 40–44% of the perimeter before height/normal gating, rather than a complete ring. This is an offline mathematical sanity check, not Unity visual proof.
- Unity/C# compilation is unavailable in this container. Authoritative next action: compile in Unity 6000.5.0f1 and run `Tools > PS3D > Run Generated Mass River-Rock Material Refinement` once; inspect the report and all generated PNGs.

### Invariants and non-goals

- Source silhouettes and generation settings are immutable in this patch.
- Unified edge-wear fallback remains acceptable and must not reject a visually accepted rock.
- Raw geometry/material channels remain available as audit evidence but do not directly control final processed appearance.
- All work remains explicit Editor-only generation under `Library`; runtime cost remains zero.
- This patch does not assemble a seamless riverbed tile and does not authorize runtime integration.

### Risks and required checks

- Over-filtering can make rocks read as wax or soap; verify the mild-plane normal restores broad structure without source-triangle starbursts.
- Procedural material variation can become noisy or visibly repetitive; verify it remains low-frequency and per-rock deterministic.
- Root darkening and wear can regress into perimeter outlines; inspect their standalone evidence maps for broken, selective support.
- The 18 frozen definitions must match the accepted M2.7C.5B.2 report exactly.

Previous plan history follows below.

## 2026-07-21 — GSU-M2.7C.5B.2: Uneven-Broad Expansion and Projection Cleanup

**Status:** Implemented in the exact approved five-file scope and statically audited. Unity 6000.5 compilation, authoritative menu execution, generated evidence, and final source-rock selection remain pending. Runtime integration remains prohibited.

### Objective

Preserve the seven user-accepted Uneven Broad Terrain/Squat projections as immutable anchors, replace the six-profile 48-rock catalog with a focused 32-rock Uneven Broad sweep, add raw-versus-processed evidence that suppresses source-triangle visibility without changing silhouettes, and replace the unhelpful fixed-source burial sheet with a labelled accepted-anchor comparison.

### User decisions and observed evidence

- The authoritative M2.7C.5B Unity run passed determinism with matching catalog fingerprint `2c0268cc641336c2dc0404d15e78bfcbeca58337d9f68e8e803ae800f6a7d1eb`, generated 48 entries, and produced the complete evidence archive under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection`.
- The user visually accepted exactly seven sources: `T-12`, `T-13`, `T-14`, `T-15`, `S-12`, `S-13`, and `S-14`.
- All seven accepted sources use the same `UB / Uneven Broad` recipe profile: `Complex`, `High`, `Chipped`, `Wild`, width/height/depth `1.15 / 0.83 / 1.14`, surface variation `0.68`, edge-wear amount/width `1.12 / 0.72`, and burial around 22–24%.
- The masks and macro height forms are acceptable; visible triangle tessellation is strongest in raw normals, variation, and exposure. The next evidence must preserve mask and macro height while filtering presentation channels independently.
- The current burial comparison repeats one poor Squat source and one poor Terrain source. It is not useful for judging burial on accepted rocks and must be replaced.

### Read-only review evidence

Reviewed before this plan write:

- `Assets/AGENTS.md`: mandatory review, plan-first write, exact scope, post-change audit, and no unsupported completion claims.
- Live layered baseline reconstructed from `Assets-Code-Archive(7).zip`, M2.7C.5A, M2.7C.5A.1, M2.7C.5B, and M2.7C.5B.1 packages.
- Complete current `GeneratedMassRiverRockProjectionBaker.cs`: six profile definitions, 48-entry Terrain/Squat catalog construction, typed `MassRecipe` assignment, unified-edge-wear fallback, top-down triangle rasterization, raw channel storage, neutral/stylized conversion, labels, burial comparison, and fingerprints.
- Complete current `GeneratedMassRiverRockProjectionValidation.cs`: two-run determinism, family/anchor/identity/output gates, report/clipboard writing, evidence PNG writing, and stale evidence cleanup.
- Authoritative passing `GeneratedMassRiverRockProjection(1).zip` report and all ten generated evidence images.
- User screenshots selecting the seven accepted IDs and rejecting the existing burial comparison.
- Current top-level M2.7C.5B/M2.7C.5B.1 sections in the three canonical Ground documents.
- Repository search confirms no project consumer of the projection result beyond its validation action. The supplied baseline contains no `.git` directory, so branch, `HEAD`, history, and working-tree comparison are unavailable; the layered supplied baseline is authoritative.

### Approved project-file scope

**Modify only:**

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

**Create/delete:** none.

No `MassGenerator`, `GeneratedMass`, Ground runtime/editor implementation, River, shader/HLSL, scene, prefab, material, profile, layer, texture-array, or Inspector implementation file is authorized.

### Focused catalog contract

1. Generate exactly 32 entries: 16 `TerrainBoulder` and 16 `SquatBoulder`.
2. Every entry uses the existing `UB / Uneven Broad` recipe profile.
3. Preserve these seven accepted anchors with their exact M2.7C.5B settings, IDs, seeds, rotation, burial, family, and recipe values:
   - `T-12`: shape/surface `1579 / 2222`, burial `21.8%`, rotation `201°`;
   - `T-13`: `3821 / 8048`, `22.6%`, `259°`;
   - `T-14`: `6173 / 4645`, `23.4%`, `353°`;
   - `T-15`: `9431 / 7584`, `24.2%`, `68°`;
   - `S-12`: `1693 / 3997`, `21.8%`, `222°`;
   - `S-13`: `4001 / 286`, `22.6%`, `322°`;
   - `S-14`: `6311 / 6588`, `23.4%`, `35°`.
4. Fill the remaining 25 entries with deterministic new shape/surface seeds using the same profile; do not reuse output fingerprints.
5. Stable IDs remain `T-00`–`T-15` and `S-00`–`S-15`; the seven accepted anchors retain their historical IDs.
6. Keep ordinary Generated Mass fallback generation valid; unified edge-wear success remains diagnostic, not an automatic visual gate.

### Projection-cleanup contract

- Preserve the raw mask exactly.
- Preserve raw projected height as the audit source.
- Build a masked, edge-aware filtered height for presentation only. Filtering must not sample outside the rock mask and must not cross large height discontinuities.
- Derive processed presentation normals from the filtered height instead of raw mesh normals.
- Build processed variation with stronger low-pass filtering and only a restrained raw-channel contribution.
- Build processed exposure and crevice with moderate masked filtering plus restrained raw contribution.
- Keep raw channel evidence available and add direct `Raw` versus `Processed` color sheets.
- Build fallback wear from convex height/normal response only where native projected wear is absent; never derive wear from mask distance or create a complete outline.
- Apply processed stylized lighting, selective root darkening, and wear enhancement only to evidence previews. Neutral data remains runtime-independent evidence.

### Burial comparison contract

Replace the current two-source burial sheet with a labelled 4 × 4 matrix using accepted anchors:

- `S-12`;
- `S-14`;
- `T-13`;
- `T-15`.

Each source is shown at `8%`, `18%`, `28%`, and `38%` burial. Every cell must label the source ID and depth. The comparison is evidence only and must not alter frozen catalog anchors.

### Evidence outputs

The single existing menu action remains under `Tools > PS3D` and writes only under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection`:

- `GeneratedMassRiverRockFamilySweepReport.txt`
- `RockFamilySweep_Raw.png`
- `RockFamilySweep_Processed.png`
- `RockFamilySweep_Height.png`
- `RockFamilySweep_ProcessedHeight.png`
- `RockFamilySweep_Normals.png`
- `RockFamilySweep_ProcessedNormals.png`
- `RockFamilySweep_Mask.png`
- `RockFamilySweep_Variation.png`
- `RockFamilySweep_ProcessedVariation.png`
- `RockFamilySweep_Exposure.png`
- `RockFamilySweep_ProcessedExposure.png`
- `RockFamilySweep_Crevice.png`
- `RockFamilySweep_ProcessedCrevice.png`
- `RockFamilySweep_EdgeWear.png`
- `RockFamilySweep_ProcessedEdgeWear.png`
- `RockFamilySweep_BurialComparison.png`

M2.7C.5B `Neutral` and `Stylized` filenames become legacy evidence and are deleted by the validation action.

### Hard validation gates

- exactly 32 entries;
- exactly 16 entries per retained family;
- every entry uses profile code `UB`;
- exactly seven frozen accepted anchors are present with exact ID/seed/burial/rotation values;
- all stable IDs and raw output fingerprints are unique;
- repeated catalog and per-rock fingerprints match;
- all raw and processed image arrays exist at the required resolution;
- every rock has valid geometry, normals, non-empty projected coverage, and required height/normal variation;
- burial comparison contains all four accepted IDs and four required depths;
- no excluded archetype, runtime asset, runtime code, or new project dependency is introduced.

### Implementation sequence and status

1. **Complete — review:** reconstructed and inspected the live layered baseline, generated evidence, accepted IDs, callers, contracts, canonical docs, and repository limitations.
2. **Complete — plan:** this section records objective, evidence, exact scope, invariants, risks, outputs, and validation before code edits.
3. **Complete — baker:** focuses definitions on 32 Uneven Broad entries, freezes the seven accepted anchors, adds mask-constrained height cleanup, height-derived processed normals, filtered material channels, interior curvature-derived fallback wear, raw/processed evidence, and the accepted-anchor 4 × 4 burial matrix.
4. **Complete — validation:** enforces count/profile/anchor/output/determinism/uniqueness/burial gates, writes sixteen evidence PNGs plus the report, and deletes the superseded Neutral/Stylized evidence filenames.
5. **Complete — architecture docs:** records the focused selection result and authoritative M2.7C.5B.2 → M2.7C.5C → M2.7C.5D → M2.7C.5E progression.
6. **Complete locally / Unity pending — post-change audit:** complete final-file reread, exact five-file diff comparison, Tree-sitter C# parse, brace/scope/duplicate-method checks, recipe-field contract scan, output/seed/anchor count scan, and project-consumer search passed. Unity compilation and authoritative menu execution remain pending.

### Post-implementation consistency and compliance audit

- Exact diff against the reconstructed M2.7C.5B.1 baseline contains only the five approved modified files; no project path was added or deleted.
- Both changed C# files parse with the installed C# Tree-sitter grammar with zero error or missing nodes.
- Brace balance is zero with no negative scope depth; AST inspection found no method at namespace scope and no duplicate method declaration.
- All twelve reflected `MassRecipe` private fields still exist with the expected names in current `GeneratedMass.cs`; the Mass generation and edge-wear API call sites are unchanged.
- Static catalog scan confirms 32 definitions, 16 Terrain seeds, 16 Squat seeds, seven exact anchor cases/contracts, one `UB` profile, an 8 × 4 catalog, and sixteen PNG outputs plus the report.
- Repository search found no consumer of `ProjectionResult` outside the validation action; no runtime/editor Ground implementation, River, shader, scene, prefab, material, profile, layer, or texture-array reference was introduced.
- The final code preserves raw masks and raw height, and processed fallback wear contains no mask-distance computation. The only mask use in fallback wear is a four-neighbour interior threshold that suppresses wear at the silhouette.
- The source package contains no `.git` metadata. `HEAD`, branch history, and unrelated working-tree state remain unavailable and were not inferred.
- No Unity or C# compiler is available in this environment. Compilation and the two-run Unity evidence action are pending and are the authoritative remaining technical gate.

### Risks and controls

- **Risk:** filtering destroys silhouette or broad planes. **Control:** mask remains unchanged; filtering is mask-constrained and range-weighted; raw evidence remains available.
- **Risk:** fallback wear recreates the rejected border. **Control:** derive only from interior convex height/normal changes and explicitly exclude mask-distance logic.
- **Risk:** accepted anchors drift. **Control:** define exact anchor constants and hard-validate ID, seeds, burial, rotation, family, and profile.
- **Risk:** processing hides malformed source geometry. **Control:** raw sheets, raw height/normals, and per-rock raw fingerprints remain mandatory.
- **Risk:** offline generation cost increases. **Control:** Editor-only explicit menu action; runtime cost remains zero.

Previous plan history follows below.

## 2026-07-20 — GSU-M2.7C.5B.1: MassRecipe Construction Correction

**Status:** Correction implemented in the approved two-file scope. Unity compilation and authoritative family-sweep execution remain pending.

### Failure evidence

The first M2.7C.5B Unity run failed before generating the first rock. `GeneratedMassRiverRockFamilySweepReport.txt` records `System.ArgumentException: JSON parse error: Invalid value` at `GeneratedMassRiverRockProjectionBaker.CreateRecipe`, specifically the second `JsonUtility.FromJsonOverwrite` call used to apply profile overrides. Both deterministic builds failed at the same location; the reported fingerprint mismatch was a consequence of both builds returning `FAIL`, not evidence of nondeterministic Generated Mass output.

### Approved correction scope

**Modify only:**
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`

### Correction design

- Remove the hand-assembled JSON override string and both recipe-construction `JsonUtility.FromJsonOverwrite` calls.
- Assign the existing private serialized `MassRecipe` fields through a typed Editor-only reflection helper using the exact current field names and types.
- Set `archetype` before `ApplyArchetypeDefaults`, then apply the approved profile values directly.
- Keep public seed setters, all sweep definitions, projection behavior, validation gates, evidence outputs, and runtime non-integration unchanged.
- Throw a precise exception if a required `MassRecipe` field is absent or has an unexpected type, so future contract drift fails explicitly rather than producing malformed JSON.

### Validation

- Static syntax/scope audit of the modified baker.
- Exact review of every reflected `MassRecipe` field against `GeneratedMass.cs`.
- Unity compilation and rerun of `Tools > PS3D > Run Generated Mass River-Rock Family Sweep` remain authoritative.

Previous plan history follows below.

## 2026-07-20 — GSU-M2.7C.5B: Curated Terrain/Squat Sweep and Donor-Tool Retirement

**Status:** Implemented in the exact approved project-file scope and statically audited. Unity 6000.5 compilation, authoritative menu execution, generated evidence, and user selection remain pending. Runtime integration remains prohibited.

### Objective

Retire the remaining M2.7B donor-extraction code and replace the broad six-family Generated Mass projection proof with a deterministic, labelled 48-rock selection sweep containing only `TerrainBoulder` and `SquatBoulder`. Preserve real Generated Mass geometry/data as the source of truth and document the authoritative progression through selected-rock refinement, seamless tile assembly, and later runtime integration.

### User decisions and accepted evidence

- The M2.7C.5A projection action passed deterministic mechanical validation in Unity 6000.5.0f1 with 24 entries, 9 evidence images, matching repeated fingerprint `3be4e4e6e250f741c380015f8c8c8341c044997d26fc0ee7312fbb75c239a207`, and 12 unified-edge-wear fallbacks.
- The user visually accepted the Generated Mass projection architecture as a viable foundation and specifically selected Terrain Boulder and Squat Boulder forms for deeper exploration.
- The user requested removal of the remaining donor-extraction tools because the donor and handmade 2D approaches are superseded and should not remain as stale active code.
- Expensive deterministic Editor-only generation is acceptable. Runtime integration remains prohibited.

### Read-only review evidence

Reviewed before this plan write:

- `Assets/AGENTS.md`: mandatory review, plan-first write, exact scope, post-change audit, and no unsupported validation claims.
- Live baseline reconstructed from `Assets-Code-Archive(7).zip`, `GSU_M2_7C_5A_Retire_2D_And_Project_Generated_Masses.zip`, and `GSU_M2_7C_5A_1_Compile_Correction.zip`.
- Complete current `GeneratedMassRiverRockProjectionBaker.cs`: six-family / four-seed catalog generation, unified-edge-wear fallback, top-down rasterization, projected material channels, evidence conversion, burial comparison, and combined fingerprint.
- Complete current `GeneratedMassRiverRockProjectionValidation.cs`: two-run determinism, hard geometry/evidence checks, report/clipboard output, and PNG writing.
- `GeneratedMass.cs`: `MassArchetype`, `MassRecipe`, serialized profile fields, archetype defaults, and seed accessors.
- `MassSurfaceFeatureGenerator.cs`: immutable `MassSurfaceFeatureSettings` contract.
- `MassGenerator.cs`: public ordinary generation and Editor-only unified edge-wear preview APIs.
- `MeshData.cs`: vertices, triangles, normals, colours, and UV2 validation contract.
- `SparseRiverbedDonorExtractor.cs` and `SparseRiverbedDonorExtractionValidation.cs`: mutually dependent historical donor extraction/evidence tools with no active code consumer. Repository search found only historical documentation references outside those two scripts.
- Authoritative M2.7C.5A Unity report and complete projection evidence archive, including the user-selected Terrain/Squat screenshots.
- The three canonical Ground documents and their historical M2.7B–M2.7C.5A records.

The supplied source has no `.git` directory. Branch, `HEAD`, status, commit history, and unrelated working-tree comparison are unavailable. The layered supplied baseline is authoritative.

### Approved project-file scope

**Delete**

- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractor.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractor.cs.meta` if present
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractionValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractionValidation.cs.meta` if present

**Modify**

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

**Create:** none.

No Mass generator, Generated Mass runtime/editor component, Ground, River, shader, HLSL, scene, prefab, material, profile, layer, texture array, or Inspector implementation file is authorized.

### Curated sweep contract

1. Generate exactly 48 entries: 24 `TerrainBoulder` and 24 `SquatBoulder`.
2. Use six deterministic profile groups across four deterministic seeds per family:
   - Broad Low;
   - Compact High;
   - Long Low;
   - Uneven Broad;
   - Worn;
   - Restrained.
3. Preserve explicit anchor entries for Terrain shape seeds `5727`, `8889`, and `7319`, and Squat shape seeds `5727` and `2223`.
4. Vary actual Mass recipe geometry controls: form complexity, facet density, edge character, shape diversity, grounding, lean, width/height/depth bias, and surface variation.
5. Vary real edge-wear amount and width through `MassSurfaceFeatureSettings`.
6. Use one controlled review burial per entry in the approximate 18–28% range. Burial comparison remains separate and does not count as source-rock variety.
7. Label every catalog cell with a compact stable ID and settings summary. Full settings remain in the copied report.
8. Preserve ordinary fallback generation when unified edge-wear preview fails; label and report every fallback.

### Evidence outputs

The single action remains under `Tools > PS3D` and writes only under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection`:

- `GeneratedMassRiverRockFamilySweepReport.txt`
- `RockFamilySweep_Neutral.png`
- `RockFamilySweep_Stylized.png`
- `RockFamilySweep_Height.png`
- `RockFamilySweep_Normals.png`
- `RockFamilySweep_Mask.png`
- `RockFamilySweep_Variation.png`
- `RockFamilySweep_Exposure.png`
- `RockFamilySweep_Crevice.png`
- `RockFamilySweep_EdgeWear.png`
- `RockFamilySweep_BurialComparison.png`

### Stylized evidence contract

- Neutral preview remains restrained and geometry-readable.
- Stylized preview remaps real `N·L` for stronger plane contrast, applies restrained variation, enhances projected real edge wear, and applies selective root darkening from low projected height, crevice, and side-facing response.
- No mask-distance outline, complete perimeter darkening, fake cracks, painted interior lines, or baked runtime albedo is introduced.
- Neutral projected data remains lighting-independent.

### Hard validation gates

- exactly 48 entries;
- exactly 24 entries per retained family;
- zero excluded archetypes;
- all five approved anchors present;
- every mesh has valid triangles and normals;
- every projection has non-empty coverage and required image arrays;
- repeated catalog fingerprints match;
- repeated per-rock fingerprints match;
- all per-rock fingerprints are unique;
- all evidence remains under `Library`;
- donor extraction menu/code is absent;
- no runtime asset or runtime code changes.

Fallback count, geometry cost, projected metrics, and edge-wear coverage remain report-only diagnostics rather than automatic visual rejection.

### Implementation sequence and status

1. **Complete — plan:** recorded evidence, scope, contracts, risks, and progression before implementation edits.
2. **Complete — retirement:** removed both donor scripts; matching metas are included in the delivery deletion manifest because they were absent from the supplied archive.
3. **Complete — baker:** replaced the broad archetype catalog with explicit Terrain/Squat profile entries, five anchors, stable labels, neutral/stylized previews, burial comparison, and exact per-rock output fingerprints.
4. **Complete — validation:** enforces family/anchor/output/determinism/uniqueness gates and updates the one-button report/output contract.
5. **Complete — architecture docs:** marks donor extraction retired and records the authoritative M2.7C.5B–M2.7C.5E progression.
6. **Complete locally / Unity pending — audit:** final diff and static checks pass; Unity compilation and execution remain authoritative pending evidence.

### Risks and controls

- **Generation time:** 48 meshes and two complete runs are more expensive. This is explicit Editor-only work and adds no runtime cost.
- **Label readability:** reserve a dedicated label strip per cell and use an internal deterministic bitmap font; do not depend on project fonts or assets.
- **Profile duplication:** include profile ID, complete parameters, and per-rock output fingerprint; require all fingerprints to be unique.
- **Unified preview instability:** preserve fallback, label fallback status, and do not reject otherwise useful rocks solely for missing edge-wear geometry.
- **Stylized preview misleading geometry:** retain neutral and raw data outputs beside the stylized preview.

### Authoritative recommended progression

- **M2.7C.5B — Curated family sweep:** current patch; select approximately 12–20 accepted source rocks.
- **M2.7C.5C — Selected-rock material and burial refinement:** freeze selected IDs/settings; refine projected wear visibility, stylized lighting, surface variation, selective root darkening, and accepted burial ranges.
- **M2.7C.5D — Seamless sparse riverbed assembly:** rasterize only selected meshes into a periodic tile with approved scale/rotation/burial variation, broad quiet regions, wrapped placements, 3×3 repetition evidence, and mip validation.
- **M2.7C.5E — Runtime material integration:** only after complete tile acceptance; create reusable Ground material/profile/library payload and validate production-camera appearance, memory, and shader cost.

Runtime integration remains blocked until the preceding visual gates are accepted.

### Post-change consistency and compliance audit

**Actual project-file diff against the reconstructed M2.7C.5A.1 baseline:**

- deleted `SparseRiverbedDonorExtractor.cs`;
- deleted `SparseRiverbedDonorExtractionValidation.cs`;
- modified the two Generated Mass projection scripts;
- modified exactly the three approved canonical documents;
- added no project file;
- changed no Mass, Ground, River, shader, scene, prefab, material, profile, layer, texture-array, vegetation, or Inspector implementation file.

The supplied archive did not contain donor-script `.meta` files. The delivery manifest still names both meta paths for deletion if they exist in the live Unity project.

**Implemented behavior:**

- 48 entries at 1536 x 1536: 24 Terrain and 24 Squat;
- six deterministic profile groups across four seeds per family;
- exact anchors: `T-00` 5727, `T-01` 8889, `T-02` 7319, `S-00` 5727, and `S-01` 2223;
- cell labels include stable ID, profile, unified/fallback path, anchor marker, shape/surface seeds, burial, and edge-wear width;
- neutral, stylized, height, normal, mask, variation, exposure, crevice, edge-wear, and burial outputs;
- repeated catalog and per-rock fingerprints;
- exact rendered-cell output fingerprints exclude stable IDs so duplicate rendered outputs are rejected rather than hidden by identity metadata;
- legacy M2.7C.5A evidence filenames are removed by the new menu action before writing the current sweep.

**Static validation completed:**

- both changed C# files parse with the tree-sitter C# grammar with zero error or missing nodes;
- AST namespace-scope inspection finds only the intended class declaration in each file;
- duplicate-method scan finds no duplicate method declarations;
- deterministic definition-model audit confirms 48 entries, 24 per family, five exact anchors, unique stable IDs, and unique family/shape-seed pairs;
- label-width audit confirms all three compact label lines fit the 192-pixel catalog cell width at 2x bitmap-font scale;
- referenced enums, recipe properties, mesh channels, `MassSurfaceFeatureSettings` constructor, and `MassGenerator.GenerateUnifiedEdgeWearPreview` signature match the current supplied source;
- repository search confirms the donor extraction menu and implementation classes are absent from active Editor code;
- exact scope comparison reports only the approved two deletions and five modifications.

**Unavailable validation:**

No Unity or C# compiler is available in this environment. Unity 6000.5 compilation, two-run menu execution, PNG inspection, and user source-rock selection remain pending and are not represented as passed.

---

## 2026-07-20 — GSU-M2.7C.5A.1: Generated Mass Projection Compile Correction

**Status:** Implemented as a narrow compile correction; Unity recompilation remains pending.

### Evidence

Unity reported:

```text
Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs(230,17): error CS0246: The type or namespace name 'UnifiedEdgeWearPreviewStatus' could not be found
```

The current Mass API declares this status as the nested public type `MassGenerator.UnifiedEdgeWearPreviewStatus` in `Assets/Game/Procedural/Masses/MassGenerator.cs`. The projection baker referenced it without the owning `MassGenerator.` qualifier.

### Approved correction scope

- Modify `GeneratedMassRiverRockProjectionBaker.cs` only to qualify the nested status type.
- Update this canonical plan before the code change.
- Do not change projection behavior, generated recipes, rasterization, evidence outputs, retirement scope, runtime code, Mass code, shaders, scenes, prefabs, or assets.

### Validation

- Confirm the changed declaration matches the exact current API signature.
- Re-scan both new projection scripts for other unqualified `MassGenerator` nested status types.
- Run structural C# parsing where available.
- Unity compilation remains authoritative.

# Ground Generation and Surface Upgrade Plan

## 2026-07-20 — GSU-M2.7C.5A: Retire Handmade 2D Stones and Prove Generated Mass Projection

**Status:** Implemented in the exact approved scope and statically audited. Unity 6000.5 compilation, menu execution, generated evidence, and visual acceptance remain pending. Runtime integration remains prohibited.

### Outcome required

Retire the complete M2.7C.x handmade 2D sparse-riverbed stone generator and replace it with an Editor-only evidence tool that generates actual `Generated Mass` meshes, projects their visible top surfaces into material-data catalogs, and proves whether existing 3D rock geometry can become the source of truth for future sparse riverbed material assembly.

The current 2D generator is not retained as fallback code. Its deterministic placement, quiet-region, and reporting ideas may be reimplemented later only after the Generated Mass source catalog is visually accepted.

### User decision and current evidence

- M2.7C.4 compiled after M2.7C.4.1 but remained visually unacceptable.
- The user explicitly rejected the generated results as non-rock geometry and required retirement before further work.
- The user approved a combined retirement plus `Generated Mass River-Rock Projection Evidence` patch.
- Expensive Editor-only generation is acceptable. Active-gameplay cost remains the controlling performance priority.

### Read-only review evidence

Reviewed before this plan write:

- `Assets/AGENTS.md`: plan-first workflow, exact scope, post-change audit, Unity 6000.5 constraints, and no false validation claims.
- Current source baseline reconstructed from `Assets-Code-Archive(7).zip` plus accepted M2.7C.3, M2.7C.4, and M2.7C.4.1 patches.
- `SparseRiverbedCandidateSynthesizer.cs` and `SparseRiverbedCandidateSynthesisValidation.cs`: complete failed handmade 2D motif source, placement, validation, and evidence pipeline.
- `MassGenerator.cs`: public `Generate`, Editor-only `GenerateUnifiedEdgeWearPreview`, and deterministic mesh production APIs.
- `GeneratedMass.cs`: `MassRecipe`, supported rock archetypes, recipe defaults, and seed controls.
- `MassSurfaceFeatureGenerator.cs`: public `MassSurfaceFeatureSettings` constructor and edge-wear settings.
- `MeshData.cs`: vertices, triangles, normals, vertex colours, and UV2 material data.
- `MassGenerator.MeshOutput.cs`: vertex colour R/G/B/A and UV2.Z contracts for variation, exposure, crevice, and real generated convex edge-wear faces.
- The three canonical Ground documents and the latest rejected M2.7C.4 evidence archive.

No `.git` metadata is present in the supplied archive. Branch, `HEAD`, status, and history remain unavailable; the layered supplied source is authoritative.

### Approved project-file scope

**Delete / retire**

- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs`
- matching `.meta` files if they exist in the live project

**Create**

- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionBaker.cs.meta`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassRiverRockProjectionValidation.cs.meta`

**Modify**

- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`

No Mass generator, runtime Ground, River, shader, HLSL, scene, prefab, material, profile, layer, texture-array, vegetation, or Inspector implementation file is authorized.

### Implementation contract

1. Generate a deterministic catalog of 24 real 3D rocks: four seeds across `TerrainBoulder`, `SquatBoulder`, `FlatSlab`, `PolishedStone`, `LayeredStone`, and restrained `BrokenChunk`.
2. Request real generated edge-wear preview geometry where available; fall back to the ordinary generated mesh only if the preview path throws or yields invalid data. Report every fallback.
3. Apply deterministic Y rotation, controlled height bias, and one of four burial fractions before projection.
4. CPU-rasterize the topmost visible triangle sample into a supersampled 1024 x 1024 catalog. Below-burial samples are discarded.
5. Interpolate real mesh channels into evidence maps:
   - mask from visible geometry;
   - height from topmost Y above burial plane;
   - normal from generated mesh normals;
   - variation from vertex colour R;
   - exposure from vertex colour G;
   - crevice from vertex colour B;
   - convex edge wear from max(vertex colour A, UV2.Z).
6. Generate a separate four-depth burial comparison using one representative mesh per archetype.
7. Run the complete build twice and require identical fingerprints.
8. Write all evidence only under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection` and copy the report to the clipboard.

### Evidence outputs

- `GeneratedMassRiverRockProjectionReport.txt`
- `RockCatalog_Color.png`
- `RockCatalog_Height.png`
- `RockCatalog_Normals.png`
- `RockCatalog_Mask.png`
- `RockCatalog_Variation.png`
- `RockCatalog_Exposure.png`
- `RockCatalog_Crevice.png`
- `RockCatalog_EdgeWear.png`
- `RockCatalog_BurialComparison.png`

### Acceptance criteria

- The handmade 2D candidate action and both implementation scripts are absent from the active project.
- The new action compiles and runs once from `Tools > PS3D > Run Generated Mass River-Rock Projection Evidence`.
- Both runs produce identical fingerprints.
- All 24 meshes and projections succeed, or any fallback is explicitly reported.
- At least 12 projected entries visibly and unambiguously read as rocks before any sparse-tile assembly begins.
- Silhouettes, planes, normals, exposure, crevice, and edge wear must originate from Generated Mass geometry/data.
- No artificial perimeter halo, superellipse motif, fake 2D facet slash, or procedural 2D stone interior feature is permitted.
- Runtime integration remains blocked until the individual projected-rock catalog is visually accepted.

### Performance

This work is explicit Editor-only generation. It may generate high-detail meshes, rasterize millions of triangle samples, and allocate temporary 1024 x 1024 buffers. It adds zero active-gameplay CPU/GPU work, zero draw calls, zero runtime texture samples, and zero runtime memory because all outputs remain local under `Library`.

### Risks and controls

- **Risk:** edge-wear preview generation may fail for particular seeds. **Control:** catch the failure, fall back to the ordinary generated mesh, and report it.
- **Risk:** burial clipping may expose side triangles incorrectly. **Control:** discard each rasterized sample below the burial plane and keep only the topmost Y sample.
- **Risk:** projected results may still fail visually. **Control:** stop at individual-rock evidence; do not implement sparse tile assembly in this patch.
- **Risk:** zip extraction cannot physically delete existing files. **Control:** delivery includes an explicit deletion manifest and deletion-capable unified patch in addition to replacement files.

### Post-change audit and validation state

**Actual project-file changes match the approved scope:**

- deleted the two active handmade 2D candidate `.cs` files from the reconstructed baseline;
- created the two Generated Mass projection `.cs` files and their metas;
- modified exactly the three canonical Ground documents;
- no Mass, runtime, shader, Ground, River, scene, prefab, material, profile, layer, or vegetation file changed.

**Static evidence completed:**

- both new C# files parse with the tree-sitter C# grammar with zero error or missing nodes;
- AST scope inspection confirms no methods, fields, or properties occur directly at namespace scope;
- duplicate-method scan reports no duplicate declarations;
- new references were checked against the current public signatures of `MassGenerator`, `MassRecipe`, `MassSurfaceFeatureSettings`, `UnifiedEdgeWearPreviewStatus`, and `MeshData`;
- both generated script metas use unique GUIDs within the supplied source set;
- exact baseline comparison reports only the approved creations, deletions, and three documentation modifications.

**Unavailable evidence:**

A Unity/C# compiler is not available in this environment. Unity 6000.5 compilation and authoritative menu execution are pending and must not be treated as passed.

---

## 2026-07-20 — GSU-M2.7C.4.1: Premature Class-Closure Compile Correction

**Status:** Implemented as a narrow compile correction. Unity recompilation remains required.

### Evidence

Unity reported a cascade beginning with namespace-scope member errors in `SparseRiverbedCandidateSynthesizer.cs`, including `CS0116`, `CS1527`, inaccessible nested types, and unresolved nested symbols. Direct inspection found one extra closing brace immediately after `ResolveDirectionalContactWeight`, closing `SparseRiverbedCandidateSynthesizer` before `DirectionalCoordinate` and every following method.

### Approved correction scope

**Modify:**

- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs`
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`

### Correction

Remove only the premature closing brace after `ResolveDirectionalContactWeight`. Preserve all M2.7C.4 rock-grammar behavior, candidate definitions, validation logic, evidence outputs, and runtime isolation.

### Validation

- Verify balanced namespace/class/method braces.
- Verify every private method after `ResolveDirectionalContactWeight` remains inside `SparseRiverbedCandidateSynthesizer`.
- Recompile in Unity 6000.5.0f1.
- Do not rerun visual synthesis until compilation is clean.

Previous plan history follows below.

## 2026-07-20 — GSU-M2.7C.4: Natural-Rock Motif Grammar, Broken Contact, and Candidate Renaming

Status: approved implementation scope completed in this patch package; Unity-side visual validation still pending.

Reason for patch:
- M2.7C.3 passed deterministic synthesis and evidence generation, but the user rejected the visuals.
- Rejection reasons were explicit and severe: stones still read as non-rock shapes, with too-uniform perimeter treatment, obvious center-crossing internal lines, overly flat interiors, and insufficient rock-like contour/relief language.
- The next patch therefore keeps the approved sparse-placement and quiet-composition architecture, but rewrites the stone grammar itself.

Approved M2.7C.4 scope:
1. Keep file scope limited to the synthesis evidence generator and the three canonical docs.
2. Preserve all donor-exclusion, determinism, quiet-space, seam, and local-output constraints.
3. Replace the M2.7C.3 "facet-owned pill" read with a more natural rock read by changing:
   - contour generation,
   - crown / facet composition,
   - local feature placement,
   - burial/contact breakup, and
   - preview shading.
4. Rename the two denser candidates to `Natural Sparse Riverbed` and `Dense Sparse Riverbed` to better reflect the intended review set.

Concrete implementation notes:
- Boundary evaluation now injects stronger, still-bounded asymmetry and feature-aware contour perturbation instead of near-oval hulls.
- Facet contribution is localized into rocky patches instead of reading like long clean diagonal cuts across the entire stone.
- Local features are now short / local / edge-biased where appropriate, rather than long center-spanning slashes.
- Contact depression and cavity are directionally broken up, narrower, and more burial-owned so they stop reading like an outline wrapped around every stone.
- Color-preview rendering now uses subtler cavity darkening and more internal tonal variation so the stones read less like flat banded icons.

Validation expectation:
- Re-run `Tools > PS3D > Run Sparse Riverbed Candidate Synthesis`.
- Inspect every generated `ColorPreview`, `ColorPreview_3x3`, `MotifCatalog`, `MotifNormalCatalog`, `PlacementDebug`, and `FinalStructureDebug`.
- Runtime integration remains blocked until at least one candidate is visually accepted.

Previous plan history follows below.

# Ground Generation and Surface Upgrade Plan


## 2026-07-20 — GSU-M2.7C.3: Facet-Owned Stone Geometry, Directional Embedding, and Enforced Quiet Composition

**Status:** Implemented in the exact approved five-file scope and post-change statically audited. C# parser checks, compatibility-stub Roslyn compilation, deterministic offline synthesis, the quiet-budget rejection-path test, and the offline validation/report control path pass. Unity 6000.5 compilation, authoritative menu execution, Unity-encoded evidence, and user visual acceptance remain pending; this item is not complete or accepted.

### Objective

Correct the rejected M2.7C.2 evidence generator rather than weakening its validator. Replace dome-owned final stone height with facet-owned planar stone geometry bounded by rounded silhouettes; replace complete perimeter contact halos with directional burial/contact; enforce candidate quiet-region budgets before committing placements; measure structural readability from the final placed candidate height field rather than copying source-motif metrics; and lower the three candidate coverage contracts to remain substrate-dominant. Continue to generate evidence only under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.

### Authoritative M2.7C.2 Unity evidence and user decision

The authoritative Unity 6000.5.0f1 M2.7C.2 run completed deterministically and generated all 34 expected outputs. It failed exactly two quiet-block checks:

- Mixed Feature Riverbed: 65.63% quiet 32x32 blocks against 66.00%, one block over budget;
- Structured Embedded Stones: 55.86% quiet 32x32 blocks against 58.00%, six blocks over budget.

Quiet Buried Pebbles passed at 75.78% against 72.00%. Coverage, seams, catalog participation, fingerprints, donor exclusion, and output creation passed. Visual review rejected the candidate set because final height/normal/color evidence still reads primarily as smooth pills, capsules, and rounded mounds; cavity remains substantially perimeter-complete; and the 3x3 evidence exposes recognizable repeated compositions. The user explicitly approved proceeding with M2.7C.3.

### Read-only review evidence

Reviewed before the first edit:

- `Assets/AGENTS.md`: mandatory review, plan-first write, exact scope, implementation traceability, post-change audit, Unity constraints, and delivery requirements.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs`, SHA-256 `6919a0d993cd9496a8eefad419001649705e478b4c2cbc3f859e95b346610ae4`: complete M2.7C.2 definitions, deterministic 48-motif catalog, dome-first height, facet blending, feature application, full-perimeter contact, probabilistic density field, post-hoc quiet metric, source-motif candidate metrics, final normals/previews, seams, mips, fingerprints, and deterministic RNG.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs`, SHA-256 `e605ed4fce0a1a92521fd93c105fda15b98fe9035e63a9a42291e4ed43e9e229`: complete two-run validation, source-motif candidate checks, quiet/seam/coverage checks, report, clipboard copy, and Library-only evidence writing.
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md` and `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`: current evidence-only architecture and no-Inspector/runtime ownership.
- `SparseRiverbedCandidates(3).zip`, SHA-256 `bbd738bbb838eceb856fc6560014cf21021eb504c5d48bf343672a768078ae10`: complete M2.7C.2 report and every candidate color, 3x3, mask, height, cavity, normal, roughness, placement, motif-catalog, and mip output.
- `GSU_M2_7C_2_Failure_Exhaustive_Continuation_Handoff_2026-07-20(1).md`: provenance, history, constraints, and exact current failure state.

The supplied archive contains no `.git` directory. Branch, `HEAD`, status, history, commits, and unrelated working-tree comparisons are unavailable. `Assets-Code-Archive(7).zip` is the authoritative source snapshot for this implementation.

### Approved files

```text
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

No `.meta`, runtime, shader, HLSL, Ground, River, hydrology, vegetation, scene, prefab, material, profile, layer, library, mesh, renderer, package, layer, tag, component, or dependency change is authorized.

### Acceptance criteria

1. Final stone height is primarily defined by multiple broad planar facets/shoulders inside rounded analytic silhouettes; a smooth dome may contribute only restrained broad bias.
2. Flat crowns, wedges, offset shoulders, broad ridges, shallow split tops, local cuts, and partly buried slabs remain visibly distinct in final candidate height, normal, and color evidence.
3. Candidate structural metrics are calculated from final placed stone height after scale, relief, embedding, substrate combination, and final field construction. Source-motif metrics remain catalog evidence only.
4. Contact/cavity is directionally concentrated on buried/downhill sectors; exposed/high sectors do not receive a complete dark perimeter halo.
5. A placement is rejected before commit when it would exceed the candidate's deterministic 32x32 occupied-block budget.
6. Density uses irregular toroidal macro regions without the prior dominant three-sine diagonal composition.
7. Coverage contracts are approximately 7%, 9%, and 11%, with accepted ranges 6–8%, 8–10.5%, and 10–12.5%.
8. Repeated synthesis fingerprints match; coverage, quiet blocks, final placed-structure metrics, seams, participation, and output checks pass.
9. No donor pixels or stamps contribute; no output is written under `Assets`; runtime cost remains zero.
10. Runtime integration remains blocked until one candidate is visually accepted.

### File-by-file implementation sequence

1. `Ground_Generation_Surface_Upgrade_Plan.md`: record evidence, objective, acceptance criteria, exact scope, invariants, risks, implementation sequence, and validation before code changes.
2. `SparseRiverbedCandidateSynthesizer.cs`:
   - increment the algorithm version;
   - lower candidate coverage contracts and add explicit occupied-macro-block budgets;
   - generate deterministic irregular toroidal macro regions;
   - reject placements that would exceed the occupied-block budget before commit;
   - make planar facet envelopes own primary stone height and use a narrow rounded boundary shoulder instead of multiplying the full stone by the broad inside field;
   - strengthen bounded structural families while preserving rounded silhouettes;
   - apply directional burial/contact weighting;
   - compute final placed-stone residual/curvature metrics and a structure-debug image from the combined final field;
   - include new metrics and counters in deterministic fingerprints.
3. `SparseRiverbedCandidateSynthesisValidation.cs`:
   - update M2.7C.3 labels;
   - validate occupied-block budgets and final placed-structure metrics;
   - stop treating selected source-motif averages as candidate proof;
   - report macro-budget rejections and final structure evidence;
   - write the new Library-only structure-debug image.
4. `Ground_Visual_Design_and_Architecture.md`: supersede M2.7C.2 with facet-owned height, directional embedding, enforced quiet composition, final-field validation, and revised coverage architecture.
5. `GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`: preserve the single project-level evidence action and explicitly record that the new metrics/output add no GeneratedGround controls.
6. Reread all five final files and the two unchanged historical donor modules; compare final scope against this plan; parse changed C# with every available static tool; scan all introduced symbols/imports; record static results and pending Unity validation here.

### Invariants and non-goals

- Editor-only manual synthesis remains the only execution path.
- All evidence remains under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates` and the report remains copied to the clipboard.
- The 48-motif deterministic catalog, rounded silhouette bounds, arbitrary-angle analytic rasterization, toroidal wrapping, final global normal derivation, semantic roughness, seam metrics, wrapped mips, and donor exclusion remain unless explicitly changed above.
- M2.7B extractor files remain byte-identical and inactive.
- No runtime material is created or promoted.
- No validation threshold is lowered solely to convert the rejected M2.7C.2 result into a pass.

### Risks and controls

- **Facet hardness may become synthetic:** retain rounded analytic masks, broad planes, bounded slopes, narrow antialiased shoulders, and complete motif height/normal catalogs.
- **Macro enforcement may prevent target coverage:** count new occupied blocks before commit, concentrate proposals in deterministic irregular macro regions, retain larger stone buckets, and fail clearly if coverage cannot be reached within budget.
- **Clustering may create obvious blobs or repeats:** use several seed-derived toroidal elliptical regions with varied radii/orientations and inspect 3x3 evidence.
- **Final-field metrics may be dominated by boundaries:** sample only interior stone pixels with occupied neighbours and compare against a local wrapped smooth baseline.
- **Directional contact may become too weak:** preserve a restrained minimum buried-sector contribution and inspect cavity/color evidence.
- **Unity compilation/execution is unavailable in the current environment:** static validation cannot replace the authoritative Unity run; status remains pending until the user executes the one menu action.

### Performance

The change remains manual Editor-only work. Macro-region evaluation, occupied-block accounting, final-field structure measurement, and one additional 512x512 evidence image add bounded authoring-time CPU/memory. Active-gameplay CPU, dirty-triggered runtime compute, runtime memory, draw calls, texture samples, renderers, and per-frame behavior remain unchanged. No `PERFORMANCE EXCEPTION` applies.

### Implemented result

- Algorithm version is 4.
- Facet planes share a crown-profile apex, use bounded outward-facing slopes, and form a continuous lower envelope. Flattened and slab profiles add explicit planar caps. Final normals use stronger diagnostic response, and the color preview uses an oblique light plus a nonzero stone-value floor so planar sectors remain readable without manufacturing a complete dark edge through preview colour.
- Candidate macro regions use deterministic toroidal best-candidate centre separation, bounded elliptical radii/aspects, and 5 / 7 / 8 regions. The former three-sine density layout is absent.
- Candidate coverage contracts are 6–8%, 8–10.5%, and 10–12.5%. Occupied 32x32 macro-block budgets are 71, 87, and 107.
- Proposed stamps are measured against the occupied-block budget before commit. Directional burial weights control broad depression, narrow cavity, and inside contact.
- Final placed-structure thresholds are residual RMS >= 0.018 and high-curvature participation >= 12%. These are measured from the combined final height field; selected source-motif averages remain report-only diagnostics.
- The validator writes 12 images per candidate, including `FinalStructureDebug`, plus one report: 37 local Library outputs in total.

### Post-change consistency and compliance audit

**Expected affected files:** the five approved paths in this section.

**Actual affected files:** exactly the same five paths. Archive-wide SHA-256 comparison against `Assets-Code-Archive(7).zip` found zero added files, zero deleted files, and no other modified path. Script `.meta` files and both M2.7B donor modules remain byte-identical. No scope discrepancy exists.

Static checks performed against the final source:

- tree-sitter C# parse: both changed C# files have no error or missing nodes;
- Roslyn C# compilation: both changed files compile together with a minimal Unity compatibility stub and explicit .NET reference set; this checks C# syntax, declarations, overload use, and introduced references but is not a Unity assembly compile;
- deterministic synthesis harness: the actual `SynthesizeAll()` path ran twice with identical catalog fingerprint `9b8419f356c5c9aed6fde8ebfbc536abbd9a21332a9a89e52ab661e23c4c39a4` and combined fingerprint `981d426e3fcd1b9e3b6504ac5f142d1684c8ec95d1c5261e5a45c2a6b0f4c360`;
- quiet-budget rejection-path harness: temporarily constraining the first candidate to 53 occupied blocks caused three pre-commit quiet-budget rejections, completed synthesis at 52 occupied blocks and 7.47% coverage, and never exceeded the temporary budget; the project definitions were not modified by this test;
- offline validation/report path: the complete menu-action logic produced a report with `VERDICT: PASS` and the expected 37 output paths under a non-Unity stub environment. Stub PNG encoding is not Unity image evidence and is not treated as authoritative;
- generated arrays were exported separately for read-only offline inspection. Planar sectors, flat slabs, wedges, asymmetric crowns, directional substrate depression, and more spatially separated compositions are visible. This is supportive evidence only; user acceptance remains pending;
- source scan confirms zero donor read/sampling dependency, zero output beneath `Assets`, and no runtime, shader, HLSL, Ground, River, vegetation, scene, prefab, material, profile, layer, renderer, package, tag, or component reference was introduced.

Offline deterministic candidate measurements:

| Candidate | Coverage | Quiet blocks | Occupied / budget | Placements | Final residual RMS | Final high curvature |
|---|---:|---:|---:|---:|---:|---:|
| Quiet Embedded Stones | 7.47% | 78.91% | 54 / 71 | 12 | 0.0387 | 32.95% |
| Mixed Sparse Riverbed | 9.17% | 72.27% | 71 / 87 | 11 | 0.0340 | 27.30% |
| Structured Sparse Stones | 11.42% | 66.02% | 87 / 107 | 15 | 0.0346 | 27.48% |

All offline coverage, quiet, occupied-block, final-structure, profile-participation, seam, and mip checks pass. The deterministic seeds naturally remain under their budgets, so their normal run records zero quiet-budget rejections; the separate forced-budget test proves the rejection branch is active.

### Pending authoritative validation

Unity 6000.5.0f1 must compile the final files and execute `Tools > PS3D > Run Sparse Riverbed Candidate Synthesis` once. The resulting report, all 37 Unity-generated outputs, and user visual review remain the acceptance gate. Runtime integration remains prohibited.

## 2026-07-20 — GSU-M2.7C.2: Feature-Rich Procedural Stone Motifs and Lower-Coverage Riverbed Candidates

**Status:** Implemented in the exact approved five-file scope and post-change statically audited. Unity 6000.5 compilation, authoritative menu execution, generated evidence, and user visual acceptance remain pending; this item is not complete or accepted.

### Objective

Replace the visually rejected M2.7C.1 smooth-mound stone model with an Editor-only procedural stone-recipe system in which every rock has a rounded silhouette, a selected crown profile, a selected edge profile, one to three local structural modifiers, and an independently selected burial/contact profile. Lower final visible stone coverage and preserve substantially larger quiet substrate regions. Continue to generate evidence only under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`; do not create runtime profiles, layers, array entries, shaders, scenes, prefabs, meshes, decals, or Inspector controls.

### Accepted user direction

- M2.7C.1 deterministic procedural generation is mechanically valid but visually rejected.
- The current 11.51%, 15.81%, and 19.32% candidates are not sparse enough.
- Current rock height reads as one consistent gradual smoothening rather than rock geometry.
- Rocks require a per-individual feature family. Rounded overall silhouettes remain mandatory; sharp star-like points remain prohibited.
- The approved active direction is feature-rich procedural generation. Donor extraction and donor-stamp synthesis remain superseded evidence only.

### M2.7C.1 Unity evidence and visual rejection

Unity 6000.5.0f1 report `SparseRiverbedCandidateSynthesis.txt`, generated 2026-07-20T13:21:40.9473508Z, records:

- algorithm version 2;
- 36 procedural motifs;
- deterministic identical catalog and combined fingerprints;
- zero extracted donor placements and zero purchased donor pixels;
- candidate coverage 11.51%, 15.81%, and 19.32%;
- seam metrics within the implemented absolute/local limits;
- `VERDICT: PASS` for deterministic evidence generation.

The user rejected all three candidates because coverage remained visually excessive and the height model produced consistently smoothed mounds without uneven planes, hard shoulders, ridges, chips, or other individual geometric features. Visual evidence in the supplied complete archive confirms that density and rock-form quality, not determinism or seam handling, are the active failures.

### Read-only review evidence

Reviewed completely before the first edit:

- `Assets/AGENTS.md`: mandatory read-only review, plan-first write, exact scope, evidence requirements, post-change consistency/compliance audit, Unity constraints, and delivery structure.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs`: complete M2.7C.1 motif catalog, definitions, placement, analytic stamp rasterization, smooth crown/facet/tilt model, contact, substrate, normals, previews, seam metrics, mip helpers, fingerprints, and deterministic RNG.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs`: complete two-run validation, catalog bounds, coverage/family/seam checks, clipboard report, and Library-only evidence writer.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractor.cs` and `SparseRiverbedDonorExtractionValidation.cs`: historical producer/evidence modules; they remain unchanged and are not active synthesis dependencies.
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`, `Ground_Visual_Design_and_Architecture.md`, and `GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`.
- Superseded M2.7C donor-stamp source and current M2.7C.1 source were compared. M2.7C.1 intentionally removed donor sampling, nearest-neighbour stamping, quarter-turn-only rotation, fixed contact radius, and sparse global seam ratios.
- Complete M2.7C.1 Unity report and all supplied candidate color, repeat, mask, height, cavity, normal, roughness, placement, motif-catalog, and mip evidence.

The reconstructed source contains no `.git` directory. Branch, `HEAD`, status, history, commits, and unrelated working-tree comparisons are unavailable. The captured pre-edit SHA-256 values are stored outside the project for all reviewed files. The working baseline is the reconstructed accepted state through M2.7C.1 plus its authoritative Unity report and visual rejection.

### Approved files

```text
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

The existing script `.meta` files remain unchanged. No new file, folder, asset, layer, tag, component, dependency, or serialized object is authorized.

### Procedural stone recipe contract

Each deterministic motif is assembled from independent bounded choices.

#### Silhouette family

1. rounded pebble;
2. broad oval;
3. low slab;
4. softly angular rounded stone;
5. rounded chipped accent.

Silhouettes continue to use ellipse/superellipse distance fields with broad low-amplitude radial deformation, bounded aspect, bounded concavity, and antialiased closed masks. No raw polygon rasterization, acute tips, or star-like shapes are permitted.

#### Crown profile

1. rounded dome;
2. flattened dome;
3. offset shoulder;
4. twin shoulder;
5. one-sided rise;
6. low slab top.

The crown profile owns broad height distribution before local modifiers.

#### Edge profile

1. soft even edge;
2. mixed edge hardness;
3. one-side buried edge;
4. shoulder-and-drop;
5. broad locally chipped edge;
6. flattened side.

The edge profile changes local falloff and burial without producing a constant dark outline.

#### Local structural modifiers

Each motif receives one to three deterministic modifiers selected from:

1. planar facet;
2. diagonal ridge;
3. shallow crease;
4. local depression;
5. secondary rounded lobe;
6. broad rounded notch;
7. buried-side cut.

Modifiers use broad antialiased fields. Their total amplitude is bounded so the stone remains rounded overall. The final height must differ measurably from the corresponding unmodified smooth crown.

#### Burial/contact profile

Each motif independently selects light embedding, half burial, one-side burial, slab setting, or shallow sinking. Contact remains a broad shallow substrate deformation plus a restrained narrow cavity whose strength varies per placement.

### Candidate contract

| Candidate | Stable evidence id | Target coverage | Accepted range | Primary distinction |
|---|---|---:|---:|---|
| Quiet Buried Pebbles | `quiet-buried-pebbles` | 7.5% | 6–9% | broad quiet substrate, rounded/oval stones, stronger burial, restrained features |
| Mixed Feature Riverbed | `mixed-feature-riverbed` | 10.5% | 9–12.5% | balanced families, crown/edge diversity, one to three structural modifiers |
| Structured Embedded Stones | `structured-embedded-stones` | 13.5% | 12–15.5% | more slabs and softly angular stones, stronger facets/shoulders/ridges, still substrate-dominant |

Candidates differ in coverage, family mix, size mix, feature weights, relief, embedding, and substrate treatment. They must not be three density variants of one identical motif language.

### Implementation sequence

1. Increase the synthesis algorithm version and replace the M2.7C.1 candidate definitions with the lower-coverage candidate contract.
2. Extend procedural motifs with explicit crown, edge, burial, and local-feature records. Generate a deterministic catalog with complete family/profile participation.
3. Evaluate a smooth reference crown and a final feature-rich height field separately. Use profile-specific crown and edge functions, then apply one to three bounded structural modifiers.
4. Measure per-motif feature residual RMS and high-curvature participation against the reference crown. Reject catalogs that contain featureless smooth mounds or exceed rounded-silhouette bounds.
5. Lower final placement coverage, strengthen broad quiet-region gating, and retain toroidal spacing/overlap/coverage rejection.
6. Keep analytic arbitrary-angle rasterization, final global normal derivation, periodic substrate generation, semantic roughness, absolute/local seam metrics, and deterministic fingerprints.
7. Expand the report with crown, edge, burial, modifier, residual-complexity, placement-feature, and lower-coverage evidence.
8. Expand motif evidence so the final height and normal structure of every generated recipe can be visually inspected before runtime integration.
9. Run the complete catalog and candidate generation twice and require identical fingerprints and metrics.

### Evidence outputs

The existing action `Tools > PS3D > Run Sparse Riverbed Candidate Synthesis` remains the single entry point. It writes one clipboard report and, per candidate:

```text
ColorPreview.png
ColorPreview_3x3.png
StoneMask.png
Height.png
Cavity.png
Normals.png
Roughness.png
MipContactSheet.png
PlacementDebug.png
MotifCatalog.png
MotifNormalCatalog.png
```

All outputs remain under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.

### Invariants and non-goals

- No donor image, extracted stamp, or purchased pixel contributes to synthesis.
- M2.7B extractor and validation files remain byte-identical.
- No output is written under `Assets`.
- No runtime code, shader, HLSL, Ground, River, hydrology, vegetation, Painted Accent, scene, prefab, material profile, layer, library entry, mesh, renderer, texture sample, draw call, allocation, or per-frame behavior changes.
- No automatic generation on reload or `OnValidate`.
- This patch does not promote a candidate. Runtime integration remains blocked until visual acceptance.

### Risks and controls

- **Risk:** local modifiers create pointed or star-like silhouettes. **Control:** silhouette and height modifiers are separated; silhouette deformation remains bounded and broad, while most structural modifiers affect height only.
- **Risk:** features become noisy rather than geometric. **Control:** one to three broad modifiers, minimum feature width, bounded amplitude, and residual/curvature metrics.
- **Risk:** all stones still share one mound. **Control:** six crown profiles, six edge profiles, five burial profiles, seven modifier types, and complete participation checks.
- **Risk:** hard transitions alias. **Control:** all masks and feature fields use smooth bounded widths; final normals are derived from the combined 512×512 height field.
- **Risk:** candidates remain visually dense. **Control:** lower coverage ranges, stronger quiet-region gating, quiet-block threshold, and measured coverage.
- **Risk:** candidate distinctions collapse. **Control:** explicit per-candidate profile/feature weights and report counts.
- **Risk:** runtime scope drifts. **Control:** exact five-file scope and Library-only outputs.

### Acceptance and validation

- [x] User approved the feature-rich procedural direction and lower-density requirement.
- [x] M2.7C.1 Unity deterministic PASS and visual rejection are recorded.
- [x] Complete read-only review performed before the first edit.
- [x] Canonical plan updated as the first source modification.
- [x] Algorithm version, candidate definitions, and stone-recipe model implemented.
- [x] Every generated motif is assigned one to three structural modifiers; rounded-silhouette limits are implemented and statically verified.
- [x] Crown, edge, burial, and modifier families are represented by explicit deterministic catalog records and validation counters.
- [x] Feature-residual RMS and high-curvature participation are measured and validated against minimum thresholds.
- [ ] Unity-generated candidate coverage reaches 6–9%, 9–12.5%, and 12–15.5% respectively. A non-authoritative offline reconstruction reached approximately 7.78%, 10.55%, and 14.40%.
- [ ] Unity repeated catalog/candidate fingerprints pass. Two-run fingerprint validation is implemented but not executed authoritatively here.
- [x] Motif height and normal catalog writers target `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`; Unity output remains pending.
- [x] Post-change exact-scope, parse, caller/consumer, namespace/import, output-path, and repository-rule audit passes.
- [ ] Unity 6000.5 compilation and menu execution pass.
- [ ] User visually accepts one candidate before runtime integration.

### Implementation result

- `SparseRiverbedCandidateSynthesizer.AlgorithmVersion` is `3`; the catalog contains 48 deterministic rounded motifs.
- Every motif selects one crown, one edge, one burial profile, two to five broad facet planes, and one to three local structural modifiers.
- Implemented local feature types are planar facet, diagonal ridge, shallow crease, local depression, secondary rounded lobe, broad rounded notch, and buried-side cut.
- The final motif height preserves facet and feature structure through multiplicative burial reduction rather than subtractive clipping.
- Candidate definitions are Quiet Buried Pebbles (7.5%), Mixed Feature Riverbed (10.5%), and Structured Embedded Stones (13.5%).
- The existing menu action remains the only entry point and writes report and image evidence under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.
- No runtime or serialized material integration was added.

### Post-change consistency and compliance audit

- Final tree comparison against the captured M2.7C.1 baseline found exactly the five approved file changes and no additions or deletions.
- `SparseRiverbedDonorExtractor.cs`, `SparseRiverbedDonorExtractionValidation.cs`, and both candidate-script `.meta` files are byte-identical to the captured baseline.
- Tree-sitter C# parsing found zero error or missing nodes in both changed scripts and both unchanged direct M2.7B producer/validation files after the final facet and burial changes.
- Source-contract checks confirm algorithm version 3, 48 motifs, lower coverage definitions, all explicit recipe/profile enums, one-to-three modifier assignment, MotifNormalCatalog output, zero donor-extractor dependency, no `AssetDatabase`, no automatic reload execution, and no `OnValidate`.
- All file writes are owned by `SparseRiverbedCandidateSynthesisValidation` and target `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`; no output path targets `Assets`.
- A non-authoritative independent reconstruction measured approximately 7.78%, 10.55%, and 14.40% stone coverage; quiet-block fractions were approximately 77.34%, 67.58%, and 58.98%. It also measured motif feature-residual RMS approximately 0.186–0.442 and high-curvature participation approximately 0.131–0.338. These checks support feasibility only and do not replace Unity execution.
- No `.git` metadata or Unity/Roslyn compiler is available in the execution environment. `HEAD`, history, Unity compilation, authoritative deterministic output, exact coverage, seam metrics, and visual quality remain pending.

### Performance contract

M2.7C.2 remains an explicit Editor-only 512×512 evidence generator. Expensive deterministic generation is permitted. Runtime cost remains zero because no runtime file or serialized candidate changes.

### File-by-file status

- `Ground_Generation_Surface_Upgrade_Plan.md`: **plan, implementation evidence, and post-change audit recorded; Unity validation pending.**
- `SparseRiverbedCandidateSynthesizer.cs`: **implemented and statically parsed; Unity compilation/execution pending.**
- `SparseRiverbedCandidateSynthesisValidation.cs`: **implemented and statically parsed; Unity compilation/execution pending.**
- `Ground_Visual_Design_and_Architecture.md`: **updated with the active feature-rich recipe architecture.**
- `GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`: **updated; project-level menu ownership and absence of Inspector/runtime controls retained.**


## 2026-07-20 — GSU-M2.7C.1: Procedural Rounded-Stone Motif and Riverbed Candidate Generation

**Status:** Implemented in the exact approved five-file scope and post-change statically audited. Unity 6000.5 compilation, exact C# generation, report/evidence output, and visual acceptance remain pending; this item is not complete or accepted.

### Objective

Replace the visually rejected donor-stamp synthesis with an Editor-only procedural material-data generator that creates rounded or softly angular embedded river stones over stylized sediment. The generator must avoid sharp star-like silhouettes, raw polygon corners, uniform dark outlines, nearest-neighbour stamp resampling, and dependence on purchased donor motifs. It produces evidence only under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`; it does not create runtime profiles, layers, array entries, shaders, scenes, prefabs, meshes, decals, or Inspector controls.

### Accepted user direction

- Procedural generation is the active stone-source direction.
- Stones should be circular, oval, rounded, or softly angular in the visual family shown by the user reference image. Polygon structure may contribute to variation, but corners must be rounded and acute star-like tips are prohibited.
- The approved construction is a hybrid rounded primitive: ellipse or superellipse foundation, low-amplitude broad radial perturbation, optional broad flattened/chipped region, smooth distance-field rasterization, asymmetric rounded crown, and restrained embedding/contact response.
- M2.7C donor extraction remains historical evidence only. Stone Ground and Black Gravel contribute zero placements and zero source pixels to M2.7C.1.

### Failed M2.7C evidence

Unity report `SparseRiverbedCandidateSynthesis.txt`, generated 2026-07-20T11:44:44.8126026Z under Unity 6000.5.0f1, records deterministic repeated synthesis and valid coverage of 11.01%, 16.10%, and 20.12%. It reports one automated failure: Sparse River Sediment vertical height seam ratio 2.3351. Visual review rejected all three candidates because:

- donor shapes were nearest-neighbour resampled and limited to quarter-turn rotations;
- normalized donor forms were reconstructed through one fixed mound formula;
- fixed-radius contact generation produced repeated dark outlines;
- the substrate was only a constant plus four low-amplitude sine waves;
- lower mips reduced stones to blocky motifs.

These findings are traceable to the superseded implementation in `SparseRiverbedCandidateSynthesizer.cs`: `BuildStampPixels`, `CommitStamp`, `BuildContactAndFinish`, `BuildPeriodicSubstrate`, and `MeasureSeams`.

### Read-only review evidence

Reviewed completely before the first edit:

- `Assets/AGENTS.md`: mandatory review, plan-first implementation, strict scope, evidence, post-change compliance audit, Unity constraints, and delivery structure.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs`: full M2.7C generation, donor dependency, placement, nearest-neighbour stamping, height/contact/substrate generation, seam metric, fingerprint, and mip helpers.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs`: complete two-run validation, coverage/seam checks, clipboard report, and evidence writer.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractor.cs` and `SparseRiverbedDonorExtractionValidation.cs`: historical producer contract and evidence ownership; neither remains a synthesis dependency.
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`, `Ground_Visual_Design_and_Architecture.md`, and `GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`.
- Unity M2.7C report and all supplied candidate color, repeat, mask, height, cavity, normal, roughness, placement, and mip evidence.
- User-supplied rounded-stone visual reference and explicit prohibition on pointed/star-like procedural stones.

The reconstructed source has no `.git` directory. Branch, `HEAD`, status, history, and commit comparisons are unavailable. The working baseline is the accepted reconstructed state through M2.7C plus its Unity failure report and visual rejection.

### Approved files

```text
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

The existing script `.meta` files remain unchanged. No new file, folder, asset, layer, tag, component, dependency, or serialized object is authorized.

### Procedural motif contract

Generate a deterministic catalog of 36 rounded motifs across five bounded families:

1. rounded pebble;
2. broad oval river stone;
3. low embedded slab;
4. softly angular rounded stone;
5. small rounded chipped accent.

Each motif starts from an ellipse or superellipse with exponent at or above the ellipse range, receives only low-amplitude broad harmonic boundary variation, and may receive one wide shallow flattening/chip. A smooth distance field produces antialiased closed masks. Minimum solidity, bounded aspect, bounded concavity, and minimum corner-radius proxies prevent acute tips and star-like silhouettes.

Height is generated from a separate asymmetrically shifted rounded crown with mild broad faceting, per-motif relief, tilt, and embedding depth. Contact uses two distance bands: a broad shallow substrate depression and a narrow restrained cavity. No constant-width black outline is permitted.

### Candidate contract

| Candidate | Stable evidence id | Target coverage | Accepted range | Primary distinction |
|---|---|---:|---:|---|
| Rounded Pebble Sediment | `rounded-pebble-sediment` | 10.5% | 9–12% | broad rounded/oval stones, calm pale sediment |
| Mixed Rounded Riverbed | `mixed-rounded-riverbed` | 15.5% | 13.5–17.5% | balanced rounded, oval, slab, and softly angular families |
| Embedded Stone Sediment | `embedded-stone-sediment` | 19% | 17–21% | larger lower-relief embedded stones and slightly stronger sediment structure |

Candidates differ in family mix, size mix, relief/embedding, and substrate treatment rather than only density. Every candidate remains substrate-dominant and retains broad stone-free regions.

### Implementation sequence

1. Remove all runtime synthesis dependency on M2.7B donor extraction and source hashes while leaving M2.7B files unchanged.
2. Build a deterministic procedural motif catalog from bounded family definitions and fixed seeds.
3. Rasterize each proposed motif analytically at arbitrary Editor-time rotation using filtered distance fields; do not resample a source bitmap.
4. Use toroidal placement, broad periodic density gating, spacing/overlap rejection, and measured final coverage.
5. Generate periodic sediment from multiple integer-period broad fields, shallow pits/ripples, and restrained fine variation. Candidate-specific parameters remain deterministic and documented.
6. Compose stone masks, asymmetric crowns, broad embedding depressions, restrained contacts, semantic roughness, and final normals from the combined height field.
7. Replace the sparse-field seam ratio with absolute boundary mean/p95 deltas and local edge-neighbour excess metrics; retain 3×3 and mip evidence as mandatory visual evidence.
8. Run the complete generator twice and compare combined, catalog, candidate, placement, and field fingerprints.

### Evidence outputs

The existing action `Tools > PS3D > Run Sparse Riverbed Candidate Synthesis` remains the single entry point. It writes one clipboard report and, per candidate:

```text
ColorPreview.png
ColorPreview_3x3.png
StoneMask.png
Height.png
Cavity.png
Normals.png
Roughness.png
MipContactSheet.png
PlacementDebug.png
MotifCatalog.png
```

All outputs remain under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.

### Invariants and non-goals

- No purchased donor image or extracted stamp contributes to procedural motif generation.
- M2.7B extractor and validation files remain byte-identical.
- No output is written under `Assets`.
- No runtime code, shader, HLSL, Ground, River, hydrology, vegetation, Painted Accent, scene, prefab, material profile, layer, library entry, mesh, renderer, texture sample, draw call, allocation, or per-frame behavior changes.
- No automatic generation on reload or `OnValidate`.
- This patch does not promote a candidate. Runtime integration remains blocked until visual acceptance.

### Risks and controls

- **Risk:** motifs become pointed or star-like. **Control:** superellipse exponent floor, low harmonic amplitudes, broad angular lobes only, wide chip window, analytic smooth mask, and report metrics for minimum radial scale and maximum boundary perturbation.
- **Risk:** motifs look like perfect eggs. **Control:** asymmetric crown shift, broad boundary harmonics, restrained flattening, varied aspect, rotation, relief, and family mix.
- **Risk:** stones still look pasted on. **Control:** per-placement embedding, broad substrate depression, narrow low-strength contact, globally derived normals, and cavity/contact evidence.
- **Risk:** substrate remains flat. **Control:** integer-period macro patches, shallow pits/ripples, fine low-amplitude variation, and candidate-specific substrate evidence.
- **Risk:** distant mips become blocks. **Control:** antialiased distance masks, larger minimum rendered motif size, filtered mip evidence, and mip occupancy reporting.
- **Risk:** sparse seam validator false positives. **Control:** absolute and local edge metrics, deterministic toroidal generation, and visual repeat evidence; no global sparse-field denominator.

### Acceptance and validation

- [x] User approved procedural generation and rounded-stone constraint.
- [x] M2.7C Unity evidence and visual failure are recorded.
- [x] Read-only review completed before the first edit.
- [x] Canonical plan updated before implementation.
- [x] Donor extraction has zero code dependency and zero source contribution.
- [x] Source contract generates exactly 36 deterministic motifs across all five families; Unity execution remains pending.
- [x] Static and offline approximation metrics remain within rounded-shape bounds; exact Unity report remains pending.
- [x] Validation invokes two complete runs and compares catalog, candidate, placement-count, and combined fingerprints; exact Unity result remains pending.
- [x] Non-authoritative offline approximation reached 11.08%, 16.38%, and 19.43%; exact Unity coverage and quiet-block results remain pending.
- [x] Replacement absolute/local seam metric is implemented; non-authoritative approximation passed all limits, exact Unity result pending.
- [x] Evidence writer covers color, repeat, mask, height, cavity, normal, roughness, placement, motif catalog, and mip sheets; Unity file generation pending.
- [x] Tree-sitter C# parsing reports zero syntax-error or missing nodes for both changed files and both preserved M2.7B direct related files.
- [x] Post-change scope and compliance audit passes for the reconstructed source.
- [ ] Unity 6000.5 compilation and menu execution pass.
- [ ] User visually accepts one candidate before runtime integration.

### Implementation result

- Replaced donor extraction/stamp consumption with a deterministic 36-motif procedural catalog. `SparseRiverbedCandidateSynthesizer.cs` contains no reference to `SparseRiverbedDonorExtractor`, donor stable IDs, source hashes, source labels, or bitmap resampling.
- Added five rounded motif families using ellipse/superellipse distance fields, broad low-amplitude harmonics, wide optional flattening, analytic antialiasing, arbitrary rotation, asymmetric crowns, broad faceting, tilt, relief, and embedding.
- Replaced the fixed contact radius with broad shallow depression and narrow restrained contact bands derived from analytic distance.
- Replaced the four-wave substrate with candidate-specific periodic sediment fields, fine variation, shallow pits, and limited ripple response.
- Replaced the sparse global seam ratio with boundary mean, p95, and local edge-excess metrics.
- Updated validation/reporting for procedural catalog metrics, family usage, mip occupancy, motif-catalog evidence, and zero donor contribution.
- Preserved the existing script `.meta` files and both M2.7B extractor/validation files byte-identically.

### Post-change consistency and compliance audit

- Exact reconstructed scope comparison: **PASS — only the five approved files changed.**
- Tree-sitter C# parse: **PASS — zero syntax-error or missing nodes in both changed files and both directly related preserved M2.7B files.**
- Donor independence: **PASS — no `SparseRiverbedDonorExtractor`, donor ID, source hash, donor label, or source-coordinate dependency remains in M2.7C.1 synthesis/validation.**
- Rounded-shape source contracts: **PASS — algorithm version 2, 36 motifs, five families, exponent floor 2.0, arbitrary rotation, analytic distance antialiasing, and no quarter-turn/nearest-neighbour stamp path.**
- Editor-only scope: **PASS — no `AssetDatabase` mutation, automatic reload hook, `OnValidate`, runtime file, output under `Assets`, or `GeneratedGround`/shader/material-property integration.**
- Evidence contract: **PASS — one menu action, two synthesis runs, clipboard report, 3×3 repeat, mask, height, cavity, normal, roughness, placement, motif catalog, and mip outputs under `Library`.**
- Preserved files: **PASS — both existing script `.meta` files and both M2.7B code files remain byte-identical.**
- Non-authoritative Python approximation of the recorded formulas: **PASS for feasibility only — 36 motifs; minimum exponent 2.1041; maximum aspect 1.6918; minimum radial scale 0.8480; maximum perturbation 0.1520; candidate coverage 11.08%, 16.38%, and 19.43%; all approximate seam metrics below the implemented limits. This does not replace Unity C# execution or visual acceptance.**
- Unity/Roslyn compilation and exact C# execution: **PENDING — unavailable in this environment.**
- Production visual acceptance: **PENDING.**

### Performance contract

M2.7C.1 is an explicit Editor-only 512×512 evidence generator. Expensive deterministic dirty-time work is acceptable. Runtime cost remains exactly zero because no runtime file or serialized candidate changes.

### File-by-file implementation status

- `Ground_Generation_Surface_Upgrade_Plan.md`: **implemented and audited.**
- `SparseRiverbedCandidateSynthesizer.cs`: **implemented and statically audited.**
- `SparseRiverbedCandidateSynthesisValidation.cs`: **implemented and statically audited.**
- `Ground_Visual_Design_and_Architecture.md`: **updated.**
- `GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`: **updated.**

## 2026-07-20 — GSU-M2.7C: Stone-Ground-Only Sparse Riverbed Candidate Synthesis

**Status:** Superseded and visually rejected after Unity 6000.5 execution. Determinism and target coverage passed, but the donor-stamp candidates appeared as simplified isolated stickers with uniform contact outlines, weak substrate identity, limited orientation, and blocky lower-mip silhouettes. The only automated failure was the sparse-field seam ratio, which is not retained as the authoritative seam metric. GSU-M2.7C.1 is the authoritative replacement.

### Objective

Add one explicit Editor-only synthesis workflow that consumes only the accepted Stone Ground 01 donor catalog and creates three new sparse riverbed material-data candidates over one deterministic calm substrate. This patch produces diagnostic images and reports under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`; it does not create a runtime surface profile, Ground layer, detail-library entry, shader path, serialized generated texture, scene object, prefab, or Inspector control.

The dense donor layout is never copied. Each candidate places a bounded subset of isolated donor stamps over broad quiet substrate areas and measures its own final visible stone coverage.

### Accepted prerequisite evidence and decisions

Unity report `SparseRiverbedDonorExtraction.txt`, generated 2026-07-20T11:08:58.7956876Z under Unity 6000.5.0f1, records deterministic repeated extraction with combined fingerprint `5359742dfd11d06aa25962d99aa0c15675ef69838f441aa762709f96ea92953b`. Stone Ground 01 produced 54 selected candidates across 19 small, 17 medium, and 18 large records. Source hashes were reported and the extraction verdict was PASS.

The user visually accepted Stone Ground 01 as the primary donor. The Black Gravel 01 accepted sheets contained excessive crescent, shell, disconnected, and fragmentary forms and are excluded from M2.7C. Black Gravel source files remain unchanged and available for possible later research; they contribute zero placements and zero visible area in this patch.

### Read-only review evidence

Reviewed completely before the first edit:

- `Assets/AGENTS.md`: mandatory review, plan-first, strict scope, post-change audit, evidence, Unity constraints, and response structure.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractor.cs`: donor definitions, 512×512 arrays, component labels, candidate bounds and metrics, selected-catalog ordering, source hashing, and deterministic fingerprint ownership.
- `Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractionValidation.cs`: one-action report/clipboard and PNG output conventions under `Library`.
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`: M2.7A/A.1 retirement, M2.7B extraction contract, and M2.7C–E sequencing.
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`: donor-only source ownership and substrate-plus-feature direction.
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`: project-level diagnostics remain outside `GeneratedGround`; synthesized candidates may not enter selectors before later promotion.
- User-supplied M2.7B Unity report and the full Stone Ground / Black Gravel detection, accepted, normal, cavity, and rejection sheets.

The reconstructed source contains no `.git` directory. Branch, `HEAD`, working-tree status, history, and commit comparison are unavailable. The working baseline is the accepted reconstructed state through V1A.6.1, GSU-M2.4.1, GSU-M2.7A.1, and GSU-M2.7B plus the supplied Unity reports and visual decision.

### Approved files

```text
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesizer.cs.meta
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedCandidateSynthesisValidation.cs.meta
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

### Candidate contract

All candidates use one shared deterministic substrate and Stone Ground 01 stamps only:

| Candidate | Stable evidence id | Target stone coverage | Accepted range |
|---|---|---:|---:|
| Very Sparse River Sediment | `very-sparse-river-sediment` | 11% | 9.5–12.5% |
| Sparse River Sediment | `sparse-river-sediment` | 16% | 14–18% |
| Moderate River Sediment | `moderate-river-sediment` | 20% | 18–22% |

The substrate must remain visually dominant, low-frequency, periodic, and identical across the three comparisons. Placement uses deterministic toroidal coordinates, broad periodic density gating, bounded overlap rejection, fixed discrete rotation/scale choices, and no inherited donor-source layout. Eligible synthesis stamps exclude microscopic records, extreme aspect, weak separation, and low compactness even when they were retained for extraction evidence.

Each placed stamp contributes a coherent silhouette, donor-derived local height, and generated embedding/contact response. Final normals are derived from the combined periodic height field so substrate, contact depression, and stones remain coherent. Roughness is generated semantically from substrate, stone mask, and stable placement variation rather than copying donor photographic roughness.

### Invariants and non-goals

- Stone Ground 01 is the only donor used in M2.7C. Black Gravel contributes zero candidate placements.
- Donor source images and `.meta` files remain byte-identical and importer settings remain unchanged.
- All candidate textures, reports, fingerprints, contact sheets, repeats, and mip evidence are written only under `Library`.
- No generated candidate is serialized under `Assets`, added to `StylizedSurfaceDetailLibrary`, exposed through a material profile, or selectable by `GeneratedGround` in this patch.
- No runtime C#, shader, HLSL, Ground, River, hydrology, vegetation, Painted Accent, scene, prefab, material, layer, tag, component, mesh, renderer, draw call, texture sample, memory allocation, or per-frame behavior changes.
- No automatic synthesis runs on domain reload. One explicit project-level menu action performs two runs, validates determinism, writes evidence, and copies the complete report to the clipboard.
- Candidate synthesis does not claim production readiness. M2.7D semantic/runtime integration remains blocked until one candidate is visually accepted.

### Synthesis sequence

1. Run `SparseRiverbedDonorExtractor.ExtractAll` and select the successful `stone-ground-01` donor only.
2. Apply stricter synthesis eligibility to remove tiny/basic, highly elongated, low-compactness, and weakly separated stamps.
3. Generate one 512×512 periodic calm substrate from fixed low-frequency integer-period waves.
4. For each candidate definition, use a fixed deterministic PRNG and broad periodic density field to propose stamp centers, discrete rotations, and restrained scales on a torus.
5. Reject proposals with excessive toroidal center proximity, excessive pixel overlap, or coverage overshoot. Stop within the candidate's accepted coverage range or fail explicitly.
6. Build a combined height field from substrate, partially embedded donor forms, and a narrow contact depression. Derive periodic normals and semantic cavity/roughness fields.
7. Generate fixed palette evidence, grayscale form, stone mask, cavity, normal, roughness, 3×3 repeats, and wrapped mip 0–4 evidence.
8. Repeat the complete synthesis and fail if fingerprints, placement counts, or final coverage differ.

### Evidence outputs

One menu action `Tools > PS3D > Run Sparse Riverbed Candidate Synthesis` writes and copies:

```text
Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates/SparseRiverbedCandidateSynthesis.txt
```

Per candidate it writes:

```text
<ColorPreview>.png
<ColorPreview_3x3>.png
<StoneMask>.png
<Height>.png
<Cavity>.png
<Normals>.png
<Roughness>.png
<MipContactSheet>.png
<PlacementDebug>.png
```

The report includes donor fingerprint, stricter eligible count, seed, target and actual coverage, placement count, size-bucket usage, proposal/rejection counters, largest connected stone-mask region, seam ratios, mip dimensions, output fingerprint, repeated-run equality, source hashes, and confirmation that Black Gravel placements equal zero.

### Risks and controls

- **Risk:** extracted stamps still appear pasted over the substrate. **Control:** partial embedding, a bounded contact depression, globally derived normals, and visual height/cavity evidence.
- **Risk:** target coverage is reached through many small noisy stamps. **Control:** synthesis-specific minimum area, balanced bucket selection, lower small-stamp share, and explicit placement/coverage reports.
- **Risk:** sparse placement accidentally forms a uniform carpet. **Control:** broad periodic density gating, toroidal minimum spacing, placement-debug image, and measured quiet-area evidence.
- **Risk:** toroidal wrapping produces seams or edge crowding. **Control:** all placement, height, normals, mip generation, and seam measurement use wrapped coordinates.
- **Risk:** a donor-based result still looks unsuitable. **Control:** stop after visual evidence. The approved fallback is a later procedurally generated stone-motif patch using the same substrate/synthesis validation contract.

### Acceptance and validation

- [x] M2.7A.1 retirement Unity report passed.
- [x] M2.7B repeated extraction and source-integrity report passed.
- [x] Stone Ground 01 visually accepted as the only M2.7C donor.
- [x] Black Gravel 01 excluded from all M2.7C placements.
- [ ] Unity compilation passes.
- [ ] Two complete synthesis runs produce identical fingerprints and metrics.
- [ ] All three candidates land inside their accepted coverage ranges.
- [ ] Reports confirm Black Gravel placement count and visible coverage are zero.
- [ ] 3×3 and mip 0–4 evidence show no conspicuous repeat seam.
- [ ] Candidate images show broad quiet substrate areas and coherent embedded stones.
- [ ] Donor source hashes remain unchanged.
- [x] Final diff and post-change audit remain inside the approved seven-file scope.
- [x] New and direct-consumer C# files parse with zero syntax-error nodes under the available tree-sitter C# parser.
- [x] Static source-contract checks confirm Stone Ground as the sole donor, Black Gravel placement count fixed at zero, two-run determinism invocation, coverage targets, Library-only output, clipboard report, repeat evidence, and wrapped mip evidence.
- [ ] User visually accepts one candidate before M2.7D begins.

### Performance contract

M2.7C adds no runtime code or data. Synthesis is an explicit Editor-only operation over 512×512 arrays and bounded placement attempts. Temporary textures and arrays are released after evidence generation. The patch adds zero runtime draw calls, texture samples, meshes, renderers, gameplay allocations, per-frame CPU work, or per-chunk work.

### Implementation result

- Added `SparseRiverbedCandidateSynthesizer`, which consumes the existing in-memory extraction result, selects only `stone-ground-01`, applies stricter synthesis eligibility, generates one shared periodic substrate, and produces the three fixed coverage candidates through deterministic toroidal placement.
- Added `SparseRiverbedCandidateSynthesisValidation`, exposing one explicit menu action, two complete synthesis runs, fingerprint/coverage/source-hash checks, clipboard reporting, and all candidate evidence under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.
- The synthesizer derives final normals from combined height, generates a bounded contact cavity and semantic roughness, and writes no asset or importer state.
- Black Gravel is not referenced by stable donor ID in the synthesizer. Its reported placement count is initialized to and validated as zero.

### Post-change consistency and compliance audit

- Exact whole-tree comparison against the reconstructed pre-edit baseline: **PASS — three approved Markdown files modified and four approved new code/meta files added; no other difference.**
- C# syntax parsing: **PASS — both new files and both direct M2.7B producer/caller files parse with zero syntax-error or missing nodes using the available tree-sitter C# grammar.**
- Direct producer preservation: **PASS — `SparseRiverbedDonorExtractor.cs` and `SparseRiverbedDonorExtractionValidation.cs` remain byte-identical to their captured pre-edit hashes.**
- Scope and architecture assertions: **PASS — no `AssetDatabase` mutation, runtime asset creation, `GeneratedGround`, shader, material-property-block, automatic reload callback, `OnValidate`, or output path under `Assets` occurs in the new implementation.**
- GUID audit: **PASS — both new script GUIDs are unique across the reconstructed `Assets` tree.**
- Approximate algorithm sanity check: **PASS, non-authoritative — a Python reconstruction using the user-supplied Stone Ground accepted contact sheets reached 11.20%, 16.11%, and 20.07% coverage with 49, 67, and 97 placements. This does not replace the exact Unity/source-map run.**
- Unity/Roslyn compilation: **PENDING — unavailable in this environment.**
- Exact source-driven output, seam/mip measurements, and production visual acceptance: **PENDING — run the one Unity menu action and review its complete report and evidence.**

## 2026-07-20 — GSU-M2.7B: Donor Stone Extraction and Evidence

**Status:** Accepted as an extraction/evidence phase after Unity 6000.5 execution and visual review. Repeated fingerprints and source hashes passed. Stone Ground 01 is accepted for M2.7C; Black Gravel 01 is retained as source evidence but rejected from first-pass synthesis.

### Objective

Add an Editor-only deterministic donor extractor for the retained Stone Ground 01 and Black Gravel 01 height, ambient-occlusion, and normal maps. Produce a reusable in-memory catalog of isolated stone stamps plus one comprehensive report and diagnostic image set under `Library/SurfaceMaterialDiagnostics/SparseRiverbedDonors`. Do not create any runtime surface material, detail-library entry, Ground layer, shader path, scene object, prefab, or serialized extracted-stamp asset in this patch.

Dense donor coverage is not the synthesis target. M2.7B only identifies useful individual source forms. M2.7C will place a controlled subset over a quiet substrate and will own final stone coverage targets. No donor-source coverage percentage is inherited automatically.

### Accepted prerequisite evidence

Unity report `ImportedSurfaceRetirement.txt`, generated 2026-07-20T10:28:28.9678804Z under Unity 6000.5.0f1, records:

```text
VERDICT: PASS — direct imported runtime surfaces retired; empty logical libraries have valid neutral backing; donor maps preserved.
No retired runtime layer/material assets remain.
No retained material uses a retired detail ID.
Retired detail entries remaining: 0.
Donor source folders retained: 2/2.
Donor source textures retained: 13.
```

M2.7A.1 is therefore accepted and no longer blocks extraction.

### Read-only review evidence

Reviewed before the first edit:

- `Assets/AGENTS.md` in full: mandatory review, plan-first, strict scope, post-change audit, evidence, and response requirements.
- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`: M2.7A/A.1 retirement contract and M2.7B–E sequencing.
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`: donor-only source ownership, empty-library backing, and sparse substrate-plus-feature direction.
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`: no new GeneratedGround foldout, debug view, scene component, or per-River donor control.
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`: current authored source conventions, normal decoding, height/AO use, generated evidence conventions, and Editor-only processing ownership.
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs`: report/clipboard and `Library/SurfaceMaterialDiagnostics` output conventions.
- `Assets/Game/Rendering/PixelSurface/Editor/RetireImportedSurfaceMaterialsMigration.cs`: canonical donor folder paths and preservation contract.
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`: direct imported entries are absent; no library mutation is needed for extraction.
- Retained donor path contract:
  - `Assets/Game/ArtSources/Editor/SurfaceMaterials/StylizedStoneGround01`
  - `Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01`
- The reconstructed source contains no `.git` directory. Branch, `HEAD`, status, history, and comparison to commits are unavailable. The working baseline is the accepted reconstructed state through V1A.6.1, GSU-M2.4.1, and GSU-M2.7A.1 plus the supplied Unity retirement report.

### Approved files

```text
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractor.cs
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractor.cs.meta
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractionValidation.cs
Assets/Game/Rendering/PixelSurface/Editor/SparseRiverbedDonorExtractionValidation.cs.meta
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

### Invariants and non-goals

- Donor source images and their `.meta` files remain byte-identical.
- All extraction output is local under `Library`; no extracted texture or catalog is serialized under `Assets`.
- The extractor reads source bytes directly and does not change TextureImporter settings.
- Extraction is deterministic for identical donor bytes and algorithm version.
- Edge-truncated, microscopic, oversized, weakly separated, extreme-aspect, and incoherent-normal candidates are rejected with explicit reasons.
- The accepted catalog is quality-limited and size-stratified; it is not a copy of every dense donor component.
- M2.7B does not decide final sparse coverage. M2.7C will own target coverage and placement over a quiet substrate.
- No runtime C#, shader, HLSL, Ground, River, hydrology, vegetation, Painted Accent, scene, prefab, material profile, Ground layer, detail-library asset, layer, tag, component, mesh, renderer, draw call, texture sample, or per-frame work change is authorized.
- No automatic extraction runs on domain reload. One explicit menu action performs the full extraction, writes the report and images, and copies the complete report to the clipboard.

### Extraction algorithm

1. Load required donor height, AO, and normal JPEG bytes into temporary readable textures without importer mutation.
2. Area-reduce each donor to a fixed 512×512 analysis grid. Normalize height and AO by robust 5th/95th percentiles.
3. Build a lightly smoothed stone-support field from `0.75 × normalized height + 0.25 × normalized AO`.
4. Use the support-field 70th percentile as the high-confidence component threshold and label 8-connected components.
5. Measure area, bounds, aspect, compactness, support contrast against an exterior ring, AO contact contrast, height range, and decoded-normal coherence.
6. Reject candidates that touch a source edge, are too small or large, exceed bounded aspect, are too sparse inside their bounds, lack support contrast, or lack coherent normal data.
7. Rank eligible candidates deterministically and retain a bounded size-stratified catalog per donor so small, medium, and large forms are all represented.
8. Run the extraction twice from the same loaded data and fail validation if catalog fingerprints differ.

### Evidence outputs

The one menu action `Tools > PS3D > Run Sparse Riverbed Donor Extraction` writes and copies:

```text
Library/SurfaceMaterialDiagnostics/SparseRiverbedDonors/SparseRiverbedDonorExtraction.txt
```

It also writes per donor:

```text
<Donor>_DetectionOverlay.png
<Donor>_AcceptedHeight.png
<Donor>_AcceptedSilhouettes.png
<Donor>_AcceptedNormals.png
<Donor>_AcceptedCavity.png
<Donor>_RejectedExamples.png
```

The report includes source hashes, dimensions, thresholds, raw component count, rejection counts by reason, accepted catalog count, size/aspect/contrast distributions, deterministic fingerprints, and an explicit statement that donor coverage is not final synthesis coverage.

### File-by-file implementation sequence

1. `SparseRiverbedDonorExtractor.cs`: source definitions, byte loading, area reduction, robust normalization, support construction, component measurement, deterministic rejection/ranking, size-stratified catalog selection, and reusable in-memory stamp records for M2.7C.
2. `SparseRiverbedDonorExtractionValidation.cs`: one menu action, two-run determinism check, report/clipboard output, source-hash verification, and all diagnostic PNG generation under `Library`.
3. Update the three canonical Ground documents with extraction ownership, no-Inspector/no-runtime impact, and the M2.7C gate.

### Risks and controls

- **Risk:** dense sources yield too many near-duplicate fragments. **Control:** strict quality rejection, deterministic ranking, size-stratified caps, and visual contact sheets.
- **Risk:** one threshold splits one stone into multiple top-face fragments. **Control:** M2.7B reports component size/shape evidence; M2.7C may use stamps as source motifs rather than claim exact original stone identity. No runtime feature depends on segmentation yet.
- **Risk:** source normal convention differs. **Control:** extraction uses normal Z/coherence only; green-channel orientation is irrelevant to candidate acceptance.
- **Risk:** donor edge seams contaminate candidates. **Control:** all edge-touching components are rejected in this phase.
- **Risk:** extraction appears sparse by selecting only examples while hiding poor raw results. **Control:** report records all raw components and every rejection reason; overlays show accepted and rejected bounds over the full donor.

### Acceptance and validation

- [x] Both retained donor folders and all required height/AO/normal maps resolve in Unity.
- [x] Source SHA-256 hashes before and after extraction match.
- [x] Two repeated extractions produce identical catalog fingerprints.
- [x] Report contains complete raw/accepted/rejected counts and rejection reasons for both donors.
- [x] Visual review completed: Stone Ground 01 shows a coherent varied catalog and is accepted; Black Gravel 01 shows excessive fragmentary/crescent forms and is explicitly excluded from M2.7C.
- [x] Detection overlays expose the complete full-source segmentation rather than only selected examples.
- [x] Static scope proof: no output is written under `Assets` except the approved code, `.meta`, and Markdown files.
- [x] Exact final diff remains inside the approved seven-file scope.
- [x] Unity compilation and extraction execution passed before M2.7B acceptance.
- [x] M2.7C unblocked with Stone-Ground-only synthesis scope after user acceptance.

### Performance contract

M2.7B adds no runtime code or data. Extraction is an explicit Editor-only operation over two 512×512 analysis fields. Temporary arrays and textures are released after the action. The patch adds zero draw calls, runtime samples, gameplay allocations, per-frame CPU work, meshes, or renderers.

### Implementation result

- Added `SparseRiverbedDonorExtractor`, an Editor-only reusable extraction API with fixed donor definitions, direct byte decoding, 512×512 area reduction, robust normalization, smoothed height/AO support, deterministic 8-connected component labeling, explicit quality rejection, bounded size-stratified selection, and SHA-256 catalog fingerprints.
- Added `SparseRiverbedDonorExtractionValidation`, exposing one explicit menu action. It runs extraction twice, verifies deterministic fingerprints and unchanged donor hashes, writes one clipboard report, and produces complete detection overlays plus accepted height/silhouette/normal/cavity and rejected-example contact sheets under `Library`.
- No importer mutation, automatic domain-reload execution, `AssetDatabase` write, runtime asset, detail-library mutation, Ground control, scene object, or serialized extraction result was added.
- Dense donor layout is not copied or treated as target coverage. The report states this explicitly, and M2.7C remains responsible for sparse placement and measured final coverage.

### Post-change consistency and compliance audit

- Scope: **PASS — exactly the seven approved files changed: two new Editor C# files, their two `.meta` files, and three canonical Markdown documents.**
- C# lexical/static structure: **PASS — both new files have balanced delimiters, terminated strings/comments, balanced preprocessor state, required namespaces, one explicit menu action, deterministic two-run validation, source-hash checks, clipboard/report output, and no automatic execution attribute.** A C# compiler, Unity assemblies, and Unity Editor are unavailable in this environment; compilation is pending and not claimed passed.
- GUID/meta review: **PASS — both new script GUIDs are unique in the reconstructed project.**
- Scope/prohibition review: **PASS — no donor image, donor `.meta`, runtime C#, shader, HLSL, Ground, River, vegetation, Painted Accent, scene, prefab, profile, layer, detail-library asset, or generated runtime asset changed.**
- File-write review: **PASS — implementation writes only the report and PNG evidence under `Library/SurfaceMaterialDiagnostics/SparseRiverbedDonors`.**
- Black Gravel offline algorithm check: **PASS with limitation — 512×512 analysis produced 583 raw components, 435 quality-eligible components, and a 96-stamp catalog with 32 small, 32 medium, and 32 large selections. Selected area min/p25/median/p75/max was 31/60/119.5/165/1167 analysis texels. Stone Ground 01 source bytes were unavailable in this environment, so both-donor execution remains a Unity gate.**
- Existing builder, material validation, detail-library contracts, retirement code, runtime consumers, shaders, and Ground code were reread after implementation and remain byte-identical to the pre-edit state.
- M2.7C remains blocked until Unity produces the complete report and the user accepts both donors' contact sheets.

## 2026-07-20 — GSU-M2.7A.1: Empty Logical Detail Library Support and Retirement Retry

**Status:** Accepted in Unity 6000.5.0f1 on 2026-07-20. `ImportedSurfaceRetirement.txt` reported `VERDICT: PASS`, zero retired runtime assets or detail IDs, two retained donor folders, 13 retained donor textures, and valid empty-library neutral backing.

### Outcome required

Allow `StylizedSurfaceDetailLibrary` to contain zero logical entries while retaining one internal neutral packed-detail backing slice. Retry the existing GSU-M2.7A retirement without preserving any rejected imported runtime entry. The internal slice is structural storage only: it is not serialized as a logical `Entry`, cannot resolve through a stable ID, does not appear in selectors, and is not referenced by any retained material profile.

### Observed blocker and reviewed evidence

Unity produced the following no-change preflight failure:

```text
[GSU-M2.7A] Imported-surface retirement made no changes because preflight failed.
Retiring imported entries would leave detail library 'Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset' empty.
```

The current implementation proves the blocker:

- `Assets/Game/Rendering/PixelSurface/Editor/RetireImportedSurfaceMaterialsMigration.cs`, `PreflightLibraries`, rejects `remainingCount <= 0` before any mutation.
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`, `Validate`, reports `The detail library has no entries.` and returns failure.
- The same builder creates the packed `Texture2DArray` with `depth = library.Entries.Count`; zero entries therefore cannot build a valid backing array.
- `NeedsRebuild` also requires packed-array depth to equal logical entry count, which is incompatible with a one-slice internal backing array.
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`, `FindEntryIndex`, `TryResolve`, and `TryResolveAuthoredColor`, already return no resolution for a missing stable ID. A hidden neutral backing slice therefore cannot become a selectable or resolvable material entry.
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs` resolves detail only through a stable entry ID. `Assets/Game/Procedural/Ground/GeneratedGround.cs`, `ApplySurfaceLayerDetailProperties`, disables detail when that resolution fails. No Ground runtime consumer indexes backing slice zero directly.
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`, `PS3D_DecodeStylizedSurfaceDetail`, establishes the neutral packed encoding: `R=0.5`, `G=0.5` yields zero slope; `B=0` yields zero cavity; `A=0.5` yields centred value/finish variation and neutral roughness.
- `StylizedSurfaceDetailLibraryEditor` and `StylizedSurfaceMaterialProfileEditor` tolerate an empty logical entry list; the material profile editor exposes no selectable entry and runtime resolution remains false.
- The reconstructed source has no `.git` directory. Branch, `HEAD`, history, staged state, unrelated working-tree changes, and commit comparison are unavailable. The comparison baseline is the reconstructed accepted state from `Assets-Code-Archive(1).zip`, V1A.6/V1A.6.1, GSU-M2.4.1, and GSU-M2.7A.

### Approved files

```text
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs
Assets/Game/Rendering/PixelSurface/Editor/RetireImportedSurfaceMaterialsMigration.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

No `.meta` change is expected because no file is added, moved, renamed, or deleted by the persistent patch.

### Invariants and non-goals

- Logical entry count may be zero; packed backing depth is `max(1, logical entry count)`.
- Empty logical libraries generate exactly one neutral packed-detail slice with a complete mip chain.
- Empty logical libraries generate no texture-form array and serialize an empty texture-form slice mapping.
- No stable ID resolves when the logical entry list is empty.
- No fake, placeholder, fallback, or user-visible library entry is created.
- GSU-M2.7A reverse-reference checks, donor-source preservation, idempotence, and no-scene-rewrite behavior remain unchanged.
- No shader, HLSL, Ground runtime/editor, River, vegetation, Painted Accent, scene, prefab, layer, tag, component, source image, or generated geometry change is authorized.
- Runtime texture sample count, draw calls, per-frame CPU work, and material behavior remain unchanged because no retained profile can resolve the internal slice.
- M2.7B donor extraction remains blocked until the retirement migration reports PASS.

### File-by-file implementation sequence

1. `StylizedSurfaceDetailLibrary.cs`: expose logical-entry count, required packed backing depth, and internal-neutral-backing state without changing stable-ID resolution.
2. `StylizedSurfaceDetailLibraryBuilder.cs`: accept zero logical entries, create one neutral packed backing slice, update stale-array depth checks, retain null texture-form array state, and report logical versus backing counts.
3. `StylizedSurfaceMaterialValidation.cs`: add a reusable backing-contract report that validates logical count, packed backing depth, texture-form state, mapping length, and non-resolution of forbidden retired IDs.
4. `RetireImportedSurfaceMaterialsMigration.cs`: remove only the empty-library preflight rejection, rebuild the empty logical library, invoke the backing-contract validation, and retain every existing reference/deletion/donor verification gate.
5. Update the three canonical Ground documents to mark direct imported surfaces retired, define empty-library backing ownership, and record that no user-facing placeholder appears in the Inspector.

### Risks and controls

- **Risk:** the backing slice could become accidentally resolvable. **Control:** resolution remains stable-ID based; validation explicitly checks every retired ID returns false.
- **Risk:** an empty library could rebuild continuously. **Control:** `NeedsRebuild` compares packed depth against `max(1, logical count)` and the generated signature records the empty-backing algorithm version.
- **Risk:** neutral values could alter a consumer. **Control:** no logical entry maps to the backing slice; neutral encoding is additionally derived from the active HLSL decoder.
- **Risk:** destructive retirement could begin before all other blockers are cleared. **Control:** all existing reverse-reference and retained-profile preflights run before entry or asset mutation.

### Acceptance and validation

- [x] Source/static contract: empty logical library is accepted by builder validation.
- [x] Unity execution: retirement report confirmed valid empty-library neutral backing and final PASS.
- [x] Unity execution: retirement report confirmed zero retired detail entries and no retained material using a retired detail ID.
- [x] Source/static proof: non-empty library behavior is unchanged because `max(1, n) = n` for every prior positive entry count, the neutral branch is false, and the signature branch is empty-only.
- [x] Source/static proof: GSU-M2.7A no longer rejects `remainingCount == 0`; it records the internal backing action instead.
- [x] Source/static proof: existing external-reference and retained-profile preflights still execute before the first mutation.
- [x] Unity report recorded final `VERDICT: PASS`, zero retired runtime identities, and valid empty-library neutral backing.
- [x] Persistent patch contains no donor image or serialized runtime asset. Live donor retention remains a migration verification gate.
- [x] Exact final diff remains inside the approved seven-file scope.
- [x] Unity compiled and executed the retirement migration under Unity 6000.5.0f1; M2.7A.1 is accepted.

### Implementation result

- `StylizedSurfaceDetailLibrary` now exposes logical entry count, required packed backing depth, and whether the library uses the internal neutral backing slice. Stable-ID resolution is unchanged.
- The builder accepts zero entries, creates one uniform neutral packed-detail slice with complete generated mips, keeps the texture-form array null, stores an empty texture-form mapping, and versions the empty-library signature independently.
- The backing-contract validator reports logical/backing counts, signature freshness, rebuild state, texture-form state, mapping count, logical resolution, and forbidden retired-ID resolution.
- The retirement migration retains all prior reverse-reference and donor-preservation gates, allows an empty logical result, rebuilds affected libraries, and validates only the affected library paths after refresh.
- No user-visible placeholder entry, profile, layer, selector item, Inspector section, runtime branch, shader edit, or Ground runtime edit is introduced.

### Post-change consistency and compliance audit

- Scope: **PASS — exactly the seven approved files changed; no `.meta`, serialized asset, image, shader, HLSL, Ground runtime/editor, River, vegetation, scene, or prefab file changed.**
- C# lexical/parser-oriented checks: **PASS — all four changed C# files and five direct consumers have balanced delimiters/preprocessor blocks with strings/comments excluded.** A C# compiler, Roslyn, Unity assemblies, and Unity Editor are unavailable in this environment; compilation is pending and not claimed passed.
- Namespace/import review: **PASS — all introduced types are covered by existing `System`, `System.Collections.Generic`, `UnityEditor`, and `UnityEngine` imports.**
- Empty-library static simulation: **PASS — logical depth `0` maps to backing depth `1`; neutral RGBA `(0.5, 0.5, 0, 0.5)` decodes to zero slope, zero value/finish variation, and neutral roughness under the current material decoder.**
- Non-empty regression proof: **PASS by equation and diff — for every previous logical depth `n > 0`, required depth remains `n`; entry loops, slice indices, mapping, texture-form generation, and non-empty signatures retain their prior paths.**
- Direct-consumer preservation: **PASS — `StylizedSurfaceMaterialProfile.cs`, `GroundSurfaceLayerProfile.cs`, `GeneratedGround.cs`, both existing material/library editors, and `PixelSurfaceMaterialDetail.hlsl` are byte-identical to the pre-patch state.**
- Migration ordering: **PASS by source inspection — external references, retained material IDs, and library schema are checked before the no-change failure branch and before `RemoveRetiredLibraryEntries`.**
- Runtime/performance: **PASS by scope for code paths — no gameplay code or shader changes.** The one 256² RGBA32 full-mip neutral packed slice is approximately `349,524` bytes (`0.333 MiB`) if the library asset is loaded; player-build inclusion remains unverified pending a build report.
- Unity compilation, actual asset rebuild, migration report, selector cleanup, and donor-source verification: **PENDING.**

## 2026-07-20 — GSU-M2.7A: Retire Direct Imported Surfaces and Freeze Sparse Riverbed Synthesis

**Status:** Superseded by GSU-M2.7A.1. Unity executed the original migration and made no changes because its preflight rejected the valid zero-logical-entry end state. The original implementation and static audit remain historical evidence; the empty-library blocker is corrected only by GSU-M2.7A.1 above.

### Objective

Remove the rejected direct full-cover imported-surface approach from live project data before implementing the replacement. Delete the selectable Stone Ground 01 and Black Gravel 01 Ground layers, their reusable runtime material profiles, and their detail-library entries. Preserve the purchased source maps only as editor-only donor inputs for a new deterministic sparse-riverbed material synthesizer. Record the accepted architecture and the following implementation phases without changing shaders, Ground runtime behavior, River behavior, hydrology, vegetation, scenes, prefabs, or generated geometry in this patch.

### Proven decision

The direct imported materials are visually rejected. Production-camera evidence showed full, uniform high-frequency stone coverage that does not match the subdued stylized Ground language. Pale Sand remains a stronger continuous substrate because it presents broad quiet response rather than a photographic gravel carpet. The accepted replacement is an editor-only synthesizer that extracts useful stone forms from donor height/AO/normal data, procedurally recomposes them sparsely over a calm substrate, and bakes ordinary shared Ground material arrays. No runtime meshes, decals, extra renderers, or per-chunk material generation are permitted.

### Retained versus removed ownership

Remove from live runtime authoring:

- `GSLP_StoneGround01` and legacy `GSLP_FineGravel_ImportedStoneGround01`;
- `SSMP_StoneGround01` and legacy `SSMP_FineGravel_ImportedStoneGround01`;
- `GSLP_BlackGravel01`;
- `SSMP_BlackGravel01`;
- detail-library entries `stone-ground-01`, `fine-gravel-imported-stone-ground-01`, and `black-gravel-01`;
- temporary M2.5/M2.6 one-time migration scripts if they remain.

Retain because the replacement subsystem needs them:

- all Stone Ground 01 and Black Gravel 01 source maps under `Assets/Game/ArtSources/Editor/SurfaceMaterials`;
- the existing shared 256² packed-detail and texture-form array transport;
- editor importer normalization, source-map sampling, area resampling, periodic mip construction, seam metrics, normal/height/AO/roughness conversion helpers, and validation/report infrastructure;
- Pale Sand and all unrelated reusable Ground materials.

The retained helpers are infrastructure, not authorization to create another direct imported full-cover material. New runtime candidates must be produced only by the synthesizer phases below.

### Approved persistent patch scope

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
Assets/Game/Rendering/PixelSurface/Editor/RetireImportedSurfaceMaterialsMigration.cs
Assets/Game/Rendering/PixelSurface/Editor/RetireImportedSurfaceMaterialsMigration.cs.meta
```

Unity-side migration effects:

```text
DELETE direct Stone Ground 01 / Black Gravel 01 Ground-layer assets when present
DELETE direct Stone Ground 01 / Black Gravel 01 material-profile assets when present
REMOVE their three detail-library IDs
REBUILD every affected generated detail library
WRITE/COPY Library/SurfaceMaterialDiagnostics/ImportedSurfaceRetirement.txt
DELETE the temporary migration script only after verified success
```

### Safety contract

- Perform a repository-wide reverse-reference preflight before the first deletion. If a scene, prefab, style, profile, or other asset still references a retired layer/material, abort without modifying anything and list every referencer in the report. The author must switch those uses to Pale Sand or another retained layer and rerun the migration.
- Do not automatically rewrite scenes, prefabs, shared styles, or local Ground selections.
- Do not delete or move donor source maps.
- Do not delete generic array/runtime infrastructure or source-processing helpers needed by the synthesizer.
- Do not alter River masks, binary substrate transitions, hydrology, Ground geometry, vegetation, Painted Accents, shaders, HLSL, layers, tags, or components.
- The migration must be idempotent. Missing retired assets/entries count as already clean, not failure.

### Following implementation phases

#### GSU-M2.7B — Donor Extraction and Evidence

Editor-only analysis of Stone Ground 01 and Black Gravel 01 height/AO/normal sources. Extract, classify, and validate reusable stone stamps; emit accepted/rejected contact sheets and metrics. No runtime material or shader change.

#### GSU-M2.7C — Sparse Riverbed Candidate Synthesis

Generate one calm sand/soil substrate and three seamless sparse-stone candidates: Sparse Rounded Riverbed, Mixed River Sediment, and Angular Gravel Patches. Bake form, explicit stone mask, cavity, normal, and roughness data. Emit 3×3 repeats and mips 0–4. No River placement change.

#### GSU-M2.7D — Semantic Ground Material Integration

Integrate the selected generated candidate through the existing shared arrays. Use the explicit stone mask to differentiate substrate and stone palette/normal/roughness response with the same mesh, renderer, draw calls, and texture-sample budget wherever technically possible. Add only controls proven necessary by candidate evidence.

#### GSU-M2.7E — Production Validation and Promotion

Validate from the production isometric camera, wet/dry/submerged states, tiling, distant stability, shader cost, memory, and source exclusion. Promote one candidate only after explicit visual acceptance; delete rejected generated candidates afterward.

### Acceptance criteria

- Direct Stone Ground 01 and Black Gravel 01 no longer appear in Ground selectors.
- Their runtime profiles and library entries are absent.
- Donor source maps remain unchanged and editor-only.
- No active reference is broken; preflight blocks deletion when references remain.
- Affected libraries rebuild without retired slices.
- Canonical documents identify the sparse synthesized substrate-plus-feature architecture as the only active riverbed gravel direction.
- No runtime performance, shader, mesh, River, vegetation, scene, or prefab change is introduced by this patch.

### Validation gates

1. Exact source/meta inventory and C# syntax/API review for the migration.
2. Preflight simulation verifies every retired path and entry ID, while excluding donor maps and retained infrastructure.
3. Verify no patch file modifies source maps, shaders, HLSL, Ground runtime/editor, River, vegetation, scenes, prefabs, layers, or tags.
4. Unity report must show either a no-change preflight failure with complete external referencers, or successful deletion/pruning/rebuild with zero retired identities remaining.
5. Verify Stone Ground 01 and Black Gravel 01 source-map hashes remain unchanged.
6. Reread all final changed files and reconcile the exact patch scope.

### Implementation result

- Added one idempotent temporary Editor migration. It identifies canonical and legacy Stone Ground 01 paths plus Black Gravel 01, performs a full reverse-reference preflight, removes the three retired detail IDs, deletes retired layer/material assets and superseded import migrations, rebuilds only affected detail libraries, verifies final absence, writes/copies one report, and self-deletes only after success.
- The migration deliberately does not rewrite any scene, prefab, style, profile, or local Ground selection. A remaining reference produces a no-change failure report with the exact referencer paths.
- Donor source directories are queried only for final retention evidence. No source map is moved, changed, or included in the patch.
- Canonical architecture and Inspector documents now identify sparse synthesized substrate-plus-stone-feature materials as the sole active riverbed-gravel direction.
- No runtime C#, Ground runtime/editor, shader, HLSL, River, vegetation, Painted Accent, scene, prefab, layer, tag, component, geometry, or generated runtime asset is modified by the persistent patch.

### Post-change consistency and compliance audit

- Persistent scope: **PASS — exactly three Markdown documents, one temporary Editor migration, and its `.meta`.**
- Migration lexical/syntax-structure audit: **PASS — balanced delimiters, terminated strings/comments, required Unity callbacks, report/clipboard path, reverse-reference preflight, library pruning, rebuild, verification, and self-deletion are present.** A Unity/Roslyn compiler is unavailable in this environment, so Unity compilation remains pending.
- Current API contract review: **PASS by source inspection — `StylizedSurfaceDetailLibrary.Entries`, `StylizedSurfaceDetailLibrary.Entry.StableId`, and `StylizedSurfaceDetailLibraryBuilder.Rebuild(library, logResult)` exist in the reviewed current code.**
- Safety review: **PASS — mutation occurs only after all reference/library preflight failures are resolved; donor paths are never deletion targets; scene/style rewriting is absent.**
- Runtime/performance impact: **PASS by scope — Editor-only one-time asset migration and documentation; no runtime path changes.**
- Black Gravel donor source hashes were recorded before packaging; the patch contains none of those files. Stone Ground donor sources are likewise not part of the patch.
- Unity migration execution, generated-array rebuild, live selector cleanup, and report verification remain pending.

## 2026-07-20 — GSU-M2.6: Black Gravel 01 Authored Material-Set Import

**Status:** Implemented in the exact approved eighteen-file persistent patch scope and post-change statically/offline audited. Read-only review used the current reconstructed GSU-M2.5 source state, `StylizedSurfaceDetailLibrary`, `StylizedSurfaceMaterialProfile`, `GroundSurfaceLayerProfile`, `StylizedSurfaceDetailLibraryBuilder`, the accepted Stone Ground 01 migration contract, the three canonical Ground documents, and all six supplied 2048² Black Gravel source maps. Unity compilation, one-time migration, generated-array rebuild, normal-orientation confirmation, production-camera visual acceptance, and build-report source exclusion remain pending. The supplied source has no `.git` metadata, so branch, `HEAD`, history, and unrelated working-tree state remain unavailable.

### Objective

Import the supplied `stone_blackGravel_01` material set as one new reusable **Black Gravel 01** surface material using the existing single-palette authored-material-set pipeline. Preserve the supplied stone form, normal, cavity, and roughness information while keeping Base, Dark, Light, and Cavity Color as the sole runtime color authority. Do not alter the shader, Ground transport, River behavior, binary substrate transition, vegetation, Painted Accents, scenes, or prefabs.

### Reviewed evidence

- `StylizedSurfaceDetailLibrary.Entry` already owns editor-only authored base-color, normal, height, ambient-occlusion, and roughness references plus normal-green inversion and cavity-composition controls. No schema extension is required.
- `StylizedSurfaceDetailLibraryBuilder.ValidateAuthoredMaterialEntry` requires those five maps to share dimensions and normalizes the base color as sRGB while importing the other maps linearly. All five required supplied maps are 2048×2048.
- `StylizedSurfaceDetailLibraryBuilder.BuildPackedMaterialPixels` already packs normal slopes into RG, height/AO-derived cavity into B, and roughness into A. `BuildAuthoredColorMipChain` already converts base color into periodic normalized grayscale texture form for the single-palette runtime path.
- `StylizedSurfaceMaterialProfile.UsesTextureForm` resolves automatically from the selected authored-material-set library entry; no Payload Mode, authored tint, or additional color path is required.
- `GroundSurfaceLayerProfile` already adapts any reusable material profile into the existing Ground layer selector and retains only cover-compatibility values locally.
- The supplied metallic map is 2048×2048 and every RGB sample is zero. The current material contract has no metallic channel, and the correct runtime interpretation is non-metallic; the file is retained only as editor-source provenance and is not assigned to the library entry.
- The supplied maps show stronger opposite-edge discontinuities in normal, height, and AO than ordinary adjacent variation. Unity visual validation must inspect generated 3×3 repeats and mips before acceptance; the purchased source files remain unchanged.

### Approved persistent patch scope

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
Assets/Game/Rendering/PixelSurface/Editor/BlackGravel01ImportMigration.cs
Assets/Game/Rendering/PixelSurface/Editor/BlackGravel01ImportMigration.cs.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_basecolor.jpg
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_basecolor.jpg.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_normal.jpg
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_normal.jpg.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_height.jpg
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_height.jpg.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_ambientocclusion.jpg
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_ambientocclusion.jpg.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_roughness.jpg
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_roughness.jpg.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_metallic.jpg
Assets/Game/ArtSources/Editor/SurfaceMaterials/BlackGravel01/stone_blackGravel_01_metallic.jpg.meta
```

Unity-side migration outputs:

```text
CREATE Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_BlackGravel01.asset
CREATE Assets/Game/Demo/Profiles/Ground/Layers/GSLP_BlackGravel01.asset
ADD    detail-library entry `black-gravel-01`
REBUILD the affected StylizedSurfaceDetailLibrary generated arrays
WRITE  Library/SurfaceMaterialDiagnostics/BlackGravel01Import.txt
```

The migration script deletes itself only after the new layer, material, entry, source references, and rebuilt arrays verify successfully.

### Invariants and non-goals

- Use stable identity `black-gravel-01`, display name `Black Gravel 01`, material asset `SSMP_BlackGravel01`, and Ground layer asset `GSLP_BlackGravel01`.
- Keep all 2048² source maps under an `Editor` folder; no source Texture2D becomes a direct runtime profile or Ground dependency.
- Preserve the existing 256² generated array tier, single-palette color contract, M2.1 periodic texture-form generation, and M2.4.1 binary substrate transition.
- Ignore metallic at runtime because the supplied metallic map is uniformly black and the current surface contract is dielectric.
- Initial normal-green inversion is disabled. Unity validation must toggle the existing library-entry option only if raised fragments visibly shade as recesses.
- Do not modify or replace Stone Ground 01, any existing material/profile/layer, the shader, HLSL, `GeneratedGround`, River code, vegetation, Painted Accents, scenes, prefabs, layers, tags, or components.
- Do not promote Black Gravel 01 over Stone Ground 01; it is an additional selectable material.
- Do not increase generated resolution before production-camera evidence shows that the 256² tier is insufficient.

### Initial authored values

The migration creates the reusable material with restrained defaults intended for first visual evaluation, not final acceptance:

```text
Palette: charcoal neutral-brown with near-black cavities
Texture Form Strength: 0.90
Scene Lighting Response: 0.70
Roughness Variation: 0.65
Macro Contrast: 0.35
Detail World Scale: 1.50 m per repeat
Detail Normal Strength: 0.90
Detail Cavity Strength: 1.10
Detail Cavity Bias: 0.10
Dry Smoothness: 0.18
Dry Specular Strength: 0.05
```

The larger repeat than the current Stone Ground 01 starting value is intentional because the source contains substantially smaller fragments; `PixelSurfaceGroundForwardPass.hlsl` multiplies world XZ by `1 / DetailWorldScale`, so a larger value produces physically larger rendered chips.

### Implementation sequence

1. Add the six source maps and explicit visible `.meta` files under the editor-only Black Gravel source folder.
2. Add an idempotent Editor migration that resolves the active detail library, validates all source assets and dimensions, and aborts before mutation if the stable ID or output paths conflict with unrelated assets.
3. Add or verify one authored-material-set entry with the five required source references, `flipAuthoredNormalGreen = false`, cavity weights `1 / 1`, and cavity floor `0.05`.
4. Create `SSMP_BlackGravel01` and `GSLP_BlackGravel01` only when absent. Set initial values only during creation; reruns must not overwrite user tuning.
5. Rebuild the affected library, verify generated detail and texture-form resolution for `black-gravel-01`, save, refresh, write/copy one complete report, and self-delete only after success.
6. Update canonical architecture/Inspector documentation and run the complete scope, reference, source-map, parser, and offline conversion audit.

### Validation gates

1. C# syntax/API review for the migration and exact source/meta inventory.
2. Verify the five assigned source maps are 2048×2048; verify metallic is uniformly zero and unassigned.
3. Verify no duplicate `black-gravel-01` entry or conflicting output asset exists; verify reruns do not reset existing material tuning.
4. Offline reproduce the current builder’s 256² texture-form and packed-detail conversion; emit source/generated 3×3 previews and edge/mip metrics.
5. Unity migration report must confirm source importer normalization, one library entry, created/verified material and layer, successful array rebuild, and runtime resolution.
6. Production-camera validation must check normal orientation, seam/repeat behavior, distant noise, and palette/scale controls before visual acceptance.

### Implementation result

- Added the six supplied 2048² JPEG maps under the editor-only `BlackGravel01` source folder with explicit visible `.meta` files. Base color imports as sRGB; normal, height, AO, roughness, and metallic import linearly, readable, uncompressed, repeat-wrapped, and without source mipmaps for editor-time conversion.
- Added an idempotent one-time Editor migration. It resolves the active detail library from Stone Ground 01 or the project’s sole library, validates map dimensions and metallic-black provenance, rejects duplicate stable IDs or conflicting output paths before mutation, creates one authored-material-set entry, creates `SSMP_BlackGravel01` and `GSLP_BlackGravel01`, rebuilds the library, verifies both generated slices, writes/copies one complete report, and self-deletes only after success.
- Existing material/layer assets are never replaced. A rerun preserves existing Black Gravel material tuning and only verifies its library/entry ownership.
- No runtime C#, shader, HLSL, Ground transport, River, transition, vegetation, Painted Accent, scene, prefab, tag, layer, or component changed.

### Post-change consistency and compliance audit

- Persistent patch scope: **PASS — exactly three Markdown documents, the temporary migration plus `.meta`, the Black Gravel source-folder `.meta`, and six source images plus six `.meta` files.**
- C# syntax parse: **PASS — Tree-sitter C# reports no error or missing nodes in `BlackGravel01ImportMigration.cs`.**
- Serialized-contract review: **PASS — every property written by the migration exists in the current `StylizedSurfaceDetailLibrary.Entry`, `StylizedSurfaceMaterialProfile`, and `GroundSurfaceLayerProfile` schemas.**
- Source inventory: **PASS — all six files are 2048×2048 RGB JPEGs; metallic maximum RGB byte is `0` and is intentionally unassigned.**
- Offline current-builder reproduction: **PASS — 256² grayscale texture form and packed normal/cavity/roughness outputs were generated; texture-form periodic repair activates on the failing axes/mips and leaves reported post-repair metrics within the current absolute-or-ratio acceptance contract.**
- Source seam risk: **RECORDED, not hidden — source normal, height, and AO opposite-edge differences are substantially larger than ordinary adjacent differences. The current builder repairs texture-form seams only; packed-detail seam visibility must be judged in Unity.**
- Runtime impact: **PASS by architecture — one additional generated library slice and one additional selectable profile/layer; no per-pixel sample, branch, draw call, geometry, runtime allocation, or runtime CPU path was added.**
- Source exclusion: **Architecturally satisfied by the `Editor` folder but player-build report proof remains pending.**
- Unity compilation, migration execution, generated-array verification, selector appearance, normal-green orientation, production-camera quality, repeat visibility, and distant stability: **unavailable here and pending, not passed.**

### Current status

- Read-only review: **Complete.**
- Canonical plan record: **Complete.**
- Source import and migration implementation: **Complete.**
- Static/offline audit: **Complete.**
- Unity compile, migration, generated-array rebuild, visual acceptance, and build-report validation: **Pending.**


## 2026-07-20 — GSU-M2.5: Stone Ground 01 Canonicalization and Fine Gravel Cleanup

**Status:** Implemented in the exact approved three-file persistent patch scope and post-change statically audited. The Unity-side asset move/delete/library-rebuild migration remains pending until the patch is imported into the live project. Read-only review used the current reconstructed GSU-M2.4.1 source, the current layer selector implementation, all four Fine Gravel layer/material assets, the detail-library schema and builder, and repository-wide references. The supplied source has no `.git` metadata, so branch, `HEAD`, history, and unrelated working-tree state remain unavailable.

### Objective

Promote the accepted imported stone material to the canonical human-facing identity **Stone Ground 01** and remove the obsolete Fine Gravel experiments from authoring. Preserve existing scene/style references to the accepted material by renaming assets through Unity `AssetDatabase.MoveAsset`, which moves their existing `.meta` files and GUIDs. Remove obsolete assets only after a reverse-dependency preflight proves that no external asset still references them.

### Reviewed evidence

- `GeneratedGroundEditor.DrawSurfaceLayerSelector` builds the dropdown from every `GroundSurfaceLayerProfile` returned by `AssetDatabase.FindAssets("t:GroundSurfaceLayerProfile")`; therefore obsolete layer assets remain visible until the assets themselves are deleted.
- The accepted assets are currently `GSLP_FineGravel_ImportedStoneGround01.asset` and `SSMP_FineGravel_ImportedStoneGround01.asset`, both with the display name `Fine Gravel — Imported Stone Ground 01`.
- The obsolete dropdown entries are backed by `GSLP_FineGravel.asset`, `GSLP_FineGravel_AB_A_Direct.asset`, and `GSLP_FineGravel_AB_B_Strong.asset`; each references its matching obsolete `SSMP_FineGravel*` profile.
- `StylizedSurfaceDetailLibrary` stores the obsolete stable IDs `fine-gravel`, `fine-gravel-ab-a-direct`, and `fine-gravel-ab-b-strong`, while the accepted material uses `fine-gravel-imported-stone-ground-01`. The library builder already supports an explicit rebuild after entry edits.
- The archive omits `.meta` files, so raw file renaming in the distributed patch would risk new GUIDs. Unity-side `AssetDatabase.MoveAsset` is required to preserve the live project GUIDs.

### Approved implementation scope

Persistent source/document changes:

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Game/Rendering/PixelSurface/Editor/StoneGround01CleanupMigration.cs
```

Unity migration outputs:

```text
MOVE   Assets/Game/Demo/Profiles/Ground/Layers/GSLP_FineGravel_ImportedStoneGround01.asset
    -> Assets/Game/Demo/Profiles/Ground/Layers/GSLP_StoneGround01.asset
MOVE   Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel_ImportedStoneGround01.asset
    -> Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_StoneGround01.asset
DELETE Assets/Game/Demo/Profiles/Ground/Layers/GSLP_FineGravel.asset
DELETE Assets/Game/Demo/Profiles/Ground/Layers/GSLP_FineGravel_AB_A_Direct.asset
DELETE Assets/Game/Demo/Profiles/Ground/Layers/GSLP_FineGravel_AB_B_Strong.asset
DELETE Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel.asset
DELETE Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel_AB_A_Direct.asset
DELETE Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel_AB_B_Strong.asset
```

The migration also renames the accepted detail entry to stable ID `stone-ground-01`, removes the three obsolete Fine Gravel detail entries, and rebuilds each affected `StylizedSurfaceDetailLibrary`. The temporary migration script deletes itself only after every operation and rebuild succeeds.

### Invariants and non-goals

- Preserve accepted layer and material GUIDs; existing scene, style, prefab, and object references must remain valid.
- Preserve all Stone Ground 01 tuning and source-map references.
- Do not edit scenes, prefabs, River files, shaders, Ground runtime code, vegetation, or Painted Accents.
- Do not delete purchased Stone Ground 01 source maps.
- Do not delete obsolete prepacked source textures in this pass; after their library entries and profiles are removed they are inert storage and can be audited separately if repository-size cleanup is desired.
- Historical document sections may retain old Fine Gravel names for traceability; current authoritative sections must identify Stone Ground 01 as canonical and the old assets as removed.

### Implementation sequence

1. Add an Editor-only one-time migration with automatic delayed execution and a manual rerun menu item.
2. Preflight old/new path state and reverse references for every deletion candidate. Abort before mutation if an obsolete asset has any referencer outside the deletion set.
3. Move the accepted layer/material assets, preserving their `.meta` files; set both Unity object names and display names to `GSLP_StoneGround01` / `SSMP_StoneGround01` and `Stone Ground 01`.
4. Update the accepted material `detailEntryId` to `stone-ground-01`; rename the matching library entry and remove the three obsolete entries.
5. Delete the six obsolete layer/material assets, rebuild affected libraries, save, refresh, write a local report, and self-delete the migration script.
6. Update canonical visual architecture and run the complete post-change scope/reference/static audit.

### Validation gates

1. C# parse and namespace/API review for the migration.
2. Verify every move/delete path and stable ID is declared exactly once.
3. Verify preflight occurs before the first mutating `AssetDatabase` call.
4. Verify the accepted asset move uses `AssetDatabase.MoveAsset` and no new GUID/meta is authored by the patch.
5. Unity migration report: accepted moves succeed, six obsolete assets delete, three obsolete library entries remove, `stone-ground-01` resolves, and every affected library rebuilds.
6. Inspector dropdown contains `Stone Ground 01 — GSLP_StoneGround01` and no Fine Gravel entries; existing Bank/Riverbed assignments remain intact.

### Implementation result

- Added a one-time Editor migration that runs after compilation, preserves the accepted layer/material GUIDs through `AssetDatabase.MoveAsset`, updates object/display names and the stable detail ID, removes the rejected layer/material assets and detail entries, rebuilds affected libraries, writes `Library/SurfaceMaterialDiagnostics/StoneGround01Cleanup.txt`, copies that report to the clipboard, and deletes itself only after verified success.
- Added a strict reverse-dependency preflight. Any obsolete layer/material reference outside the declared deletion set aborts the migration before the first move or deletion.
- Updated the canonical visual architecture so `Stone Ground 01`, `GSLP_StoneGround01`, `SSMP_StoneGround01`, and `stone-ground-01` are authoritative; old Fine Gravel names remain only in historical sections.

### Post-change consistency and compliance audit

- Persistent patch scope: **PASS — exactly the two declared Markdown documents plus the new temporary Editor migration; no existing C#, HLSL, shader, asset, scene, prefab, River, vegetation, or Painted Accent file changed.**
- C# syntax parse: **PASS — Tree-sitter C# reports no error or missing nodes.**
- Contract/API review: **PASS — current source exposes `GroundSurfaceLayerProfile.DisplayName`, `StylizedSurfaceMaterialProfile.DisplayName`, `StylizedSurfaceMaterialProfile.DetailEntryId`, serialized detail-library `entries/stableId/displayName`, and public `StylizedSurfaceDetailLibraryBuilder.Rebuild`.**
- Mutation ordering: **PASS — old/new path validation and reverse-dependency discovery execute before the first `AssetDatabase.MoveAsset` or `AssetDatabase.DeleteAsset` call.**
- GUID preservation: **PASS by implementation contract — the patch does not author replacement asset files or `.meta` files; live accepted assets are moved through Unity's AssetDatabase. Unity execution remains pending.**
- Cleanup completeness: **PASS by static declaration — six obsolete layer/material paths and three obsolete stable IDs are declared; final verification rejects any remainder and requires exactly one `stone-ground-01` entry. Unity execution remains pending.**
- Self-cleanup/reporting: **PASS — success schedules deletion of the temporary migration script; failure leaves it available through `Tools > PS3D > Run Stone Ground 01 Cleanup`; every run writes and copies one complete report.**
- Unity compilation, live reverse-reference result, asset moves/deletions, generated-array rebuild, Inspector dropdown result, and reference preservation: **unavailable here and pending, not passed**.

## 2026-07-19 — GSU-M2.4.1: Simple Binary Substrate Cut

**Status:** Implemented in the exact approved twelve-file scope and post-change statically audited. Read-only review used the current M2.4 reconstructed source and the exact pre-M2.4 state reconstructed from `Assets-Code-Archive(1).zip` plus the accepted M2.3, V1A.6, and V1A.6.1 patches. Unity compilation, source-library regeneration, production-camera visual acceptance, and GPU profiling remain pending. The supplied source has no `.git` metadata, so branch, `HEAD`, history, and unrelated working-tree state remain unavailable.

### Objective

Replace the failed M2.4 whole-stone scatter with the smallest high-confidence non-interpolating transition. Bank and Riverbed material support are combined first, then authored texture-form substrates use one binary material-ownership cut at the combined substrate boundary. The final contour receives only derivative-width antialiasing. There is no segmentation, scatter, per-stone retention, centroid reconstruction, cavity metadata, or runtime hashing.

### Proven evidence

- The M2.4 screenshot shows ordinary Ground leaking through the Bank/Riverbed boundary and pixel-scale fragmentation inside nominal stones.
- `PixelSurfaceGroundForwardPass.hlsl` currently runs `PS3D_ResolveStylizedSurfaceTransitionCoverage` independently for Bank and Riverbed before substrate composition. Independent rejection can expose ordinary Ground even when the two surfaces use the same material.
- `PixelSurfaceMaterialDetail.hlsl` currently reconstructs filtered centroid metadata and hashes it per fragment. Bilinear filtering and mip generation cannot preserve discrete element identity, so the implementation is not a high-confidence basis for a production transition.
- The accepted pre-M2.4 composition already combines Bank and Riverbed through `ResolveGroundSubstrateCompositionWeights`; restoring that order removes the false internal substrate holes.
- `PS3D_StylizedSurfaceDetail.textureFormPayload` already identifies imported authored texture-form materials in the shader. No new profile field, transport property, texture sample, or serialized asset is required.

### Approved files

The correction may modify only:

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
```

The eight C#/editor files outside the forward pass are in scope only to remove M2.4 transition controls, metadata generation, validation output, and transport while restoring their exact accepted pre-M2.4 behavior. No River file, scene, prefab, material asset, source texture, vegetation file, Painted Accent implementation, shader property declaration, or generated asset is approved.

### Runtime contract

1. Resolve the original continuous Bank and Riverbed material blends.
2. Sample Bank and Riverbed detail through the accepted M2.3 path.
3. Compute the ordinary/secondary substrate ownership from the **combined** Bank + Riverbed support.
4. When either active secondary layer has an authored texture-form payload, replace the combined secondary coverage with a fixed `0.5` binary cut. Use `fwidth` only to provide approximately one screen-pixel antialiasing at the contour.
5. Preserve the original Bank:Riverbed ratio inside the retained secondary coverage. Different wetness, smoothness, and lighting therefore remain possible, while ordinary Ground cannot appear between Bank and Riverbed merely because their individual weights cross.
6. Continuous/prepacked materials retain the accepted smooth substrate interpolation.

### Removed M2.4 behavior

- Remove `StylizedSurfaceTransitionStyle` and all four transition controls.
- Restore authored texture-form generation to normalized grayscale form only; generation algorithm version returns to the accepted M2.3 value and will force a signature mismatch/rebuild from the current M2.4 asset.
- Remove component segmentation, centroid/prominence payloads, metadata-aware mips, transition diagnostic images, runtime centroid reconstruction, hashing, scatter, and cavity-cut logic.
- Restore the hidden authored-tint transport vector to its neutral compatibility value.

### Invariants and performance

- No additional texture sample, property, texture, draw call, geometry, runtime allocation, CPU callback, or generated asset type.
- Texture-form and packed-detail visual evaluation remain M2.3.
- Bank/Riverbed hydrology and response remain independently weighted after material ownership is resolved.
- V1A.6 vegetation coverage, V1A.6.1 object search, Painted Accent, River masks, UV3, and geometry remain unchanged.
- The binary transition changes only authored texture-form substrate ownership at its boundary; continuous materials remain unchanged.

### Validation gates

1. Exact twelve-file scope and exact removal of all M2.4 transition symbols.
2. Parse every changed C# file and affected unchanged consumer; scan namespaces and method arity.
3. Verify HLSL delimiters, no remaining centroid/hash/scatter functions, and unchanged texture-sample counts.
4. Verify `GeneratedGround.cs` and `GeneratedGroundEditor.cs` retain the complete V1A.6 vegetation contract and no obsolete `FindObjectsSortMode` use.
5. Unity compilation and source-library rebuild.
6. Production-camera validation: no pixel scatter, no ordinary Ground between Bank and Riverbed, a single hard outer contour with only screen-width antialiasing, and unchanged continuous-material behavior.

### Implementation result

- Removed all M2.4 transition enums, four material controls, profile/layer accessors, Ground transport, editor UI, segmentation and metadata generation, whole-stone diagnostics, centroid reconstruction, runtime hashing, scatter, and cavity-cut logic.
- Restored the M2.3 grayscale texture-form payload and generation algorithm version `4`. A live M2.4-generated library signature therefore becomes stale and Unity must rebuild it through the existing repair path.
- Added `ResolveGroundSimpleBinarySubstrateWeights` in the Ground forward pass. It combines Bank and Riverbed support before one fixed `0.5` ownership cut for authored texture-form substrates, preserves their relative internal weights, and uses `fwidth` only for the exact contour.
- Preserved the current `CS0414` suppression around the hidden serialized `payloadMode` compatibility field.
- Preserved the complete V1A.6 vegetation coverage runtime/editor implementation and V1A.6.1 nonobsolete object-search overload.

### Post-change consistency and compliance audit

- Exact diff scope: **PASS — twelve declared files and no others** relative to the current M2.4 reconstructed source. Vegetation benchmarks and the serialized recovery scene are byte-identical.
- C# syntax parse: **PASS — seven changed C# files plus four direct unchanged consumers/contracts parsed with Tree-sitter C# without error or missing nodes**.
- HLSL structure: **PASS — delimiters and preprocessor balance are valid; the new helper has one definition and one call**.
- M2.4 removal: **PASS — no transition-style, thinning, cavity-attraction, retention, segmentation, centroid, prominence, or runtime scatter symbol remains under `Assets/Game`**.
- Accepted-code restoration: **PASS — builder, material editor, validation, material-detail HLSL, layer adapter, Ground runtime, and Ground editor are byte-identical to the reconstructed accepted pre-M2.4 state**. The material profile differs only by the retained `CS0414` warning suppression.
- Runtime sampling parity: **PASS — four texture-array samples and one ordinary texture sample, unchanged from M2.4/M2.3**. No new property, texture, draw call, geometry, allocation, CPU callback, or per-frame process was added.
- Numeric invariant test: **PASS — 100,000 random Bank/Riverbed pairs reproduce the original continuous weights exactly when texture-form hard cutting is disabled; hard-cut weights remain normalized and ordinary Ground is zero whenever combined support is at or above the fixed threshold**.
- Unity/Roslyn compilation, generated-subasset rebuild, production-camera edge appearance, and GPU timing: **unavailable here and pending, not passed**.

### Supersession

GSU-M2.4 whole-stone thinning is superseded by this patch because the runtime metadata representation produced false internal substrate holes and mip/filter-driven fragmentation. Its historical section remains below for traceability but is no longer authoritative.


## 2026-07-19 — GSU-M2.4: Discrete Whole-Stone Transition with Cavity-Locked Edges

**Status:** Implemented in the exact approved twelve-file source/document scope and statically/offline audited. Read-only review and implementation used `Assets-Code-Archive(1).zip` overlaid with the accepted V1A.6 and V1A.6.1 Ground vegetation-recovery patches. Unity compilation, source-driven array regeneration, copied live validation output, production-camera acceptance, and profiling remain pending. The supplied source has no `.git` metadata, so branch, `HEAD`, history, and unrelated working-tree state are unavailable.

### Objective

Replace broad opacity-like interpolation for discrete stone materials with a material-capability transition that preserves fully opaque complete stones. Inside the existing Bank/Riverbed transition field, whole stones progressively disappear according to deterministic per-stone retention; the final visible silhouette terminates at generated stone boundaries and the existing cavity field. Continuous materials retain the current smooth substrate interpolation.

### Read-only evidence

Reviewed before this plan edit:

- `StylizedSurfaceMaterialProfile.cs` and `GroundSurfaceLayerProfile.cs`: reusable material identity currently owns palette, grayscale texture form, packed detail, and finish, but has no transition capability contract.
- `StylizedSurfaceDetailLibraryBuilder.cs`: authored-material-set output is a 256² sRGB RGBA32 array. M2.3 writes normalized grayscale form redundantly to RGB and alpha `1`; therefore G/B/A can carry transition metadata without adding an array or sample. The packed-detail array already supplies cavity in B.
- `PixelSurfaceMaterialDetail.hlsl`: the texture-form sample currently consumes only R. G/B/A are unused. The packed sample already decodes broad cavity and cavity core.
- `PixelSurfaceGroundResponse.hlsl`: `ResolveGroundBankMaterialBlend` and `ResolveGroundRiverbedMaterialBlend` produce continuous scalar material weights. `ResolveGroundSubstrateCompositionWeights` then crossfades ordinary Ground, Bank, and Riverbed albedo/normal/finish.
- `PixelSurfaceGroundForwardPass.hlsl`: Bank and Riverbed each sample packed detail and optional texture form before substrate composition. The current order can be changed so those already-sampled details reshape the scalar material blend before composition; no extra sample is required.
- `GeneratedGround.ApplySurfaceLayerDetailProperties`: the legacy hidden authored-tint vector is still transported but intentionally unused after M2.3. It can carry four transition parameters without adding shader property IDs or changing serialized materials.
- `StylizedSurfaceMaterialProfileEditor.cs`, inline `GeneratedGroundEditor.cs`, and `StylizedSurfaceMaterialValidation.cs`: the reusable material editor and existing one-button report are the correct ownership and evidence surfaces. No new top-level Ground group or debug view is required.
- User screenshot: the continuous contour visibly crosses recognizable stones and produces a broad green/stone mix. The requested accepted direction is whole-stone thinning with cavity-locked termination.

### Approved files

The implementation may modify only:

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
```

Expected Unity-generated output after the generation-version increment:

```text
Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset
```

The generated asset must not be hand-edited. No River producer, corridor mask, UV3, hydrology, geometry, scene, prefab, material asset, source JPEG, vegetation benchmark, vegetation coverage, Painted Accent, layer, tag, renderer, draw call, or new texture is approved.

### Transition contract

Add a reusable material transition style with serialized compatibility:

- `Automatic`: authored material-set entries resolve to `Discrete Elements + Cavity Cut`; prepacked/legacy entries resolve to `Continuous`.
- `Continuous`: preserves the current scalar substrate interpolation.
- `Discrete Elements + Cavity Cut`: evaluates the existing spatial blend at a generated stone centroid, selects the complete stone through a deterministic retention threshold, and applies only derivative-width antialiasing at the final silhouette.

Material-owned controls:

- `Transition Style`.
- `Thinning Width`: fraction of the existing material-blend range used to remove stones.
- `Cavity Edge Attraction`: strength with which the final silhouette follows the packed cavity field.
- `Stone Retention Bias`: interpolation from deterministic random retention toward generated stone prominence, allowing larger/prominent stones to survive farther into ordinary Ground.

No River-application duplicate controls are added.

### Generated payload

For authored material-set entries, increment the generation algorithm version and repack the existing texture-form RGBA32 slice:

```text
R — normalized grayscale texture form
G — shortest toroidal U offset from the texel to its detected stone centroid
B — shortest toroidal V offset from the texel to its detected stone centroid
A — stone prominence and membership; zero denotes cavity/background
```

G/B are encoded from signed `[-0.5, +0.5]` offsets into `[0, 1]` using the existing sRGB-compatible raw upload. A remains linear. The builder must:

1. derive a deterministic stone/cavity domain from the already-generated packed cavity channel;
2. adapt the threshold when a merged component dominates the tile;
3. label components with toroidal eight-neighbour connectivity so stones crossing a repeat edge remain one element;
4. calculate circular/toroidal centroids and area-derived prominence;
5. produce explicit mips that preserve form in linear space and reconstruct centroid offsets relative to each destination texel;
6. retain M2.1 form-seam repair without modifying centroid/prominence channels;
7. keep the array 256² RGBA32, Repeat, Bilinear, complete-mip, and non-readable after upload.

### Runtime evaluation

1. Sample the same packed-detail and texture-form arrays already used by M2.3.
2. Decode centroid offset and prominence from the texture-form sample.
3. Convert centroid UV offset to world metres using the existing detail scale.
4. Estimate the existing Bank/Riverbed scalar blend at that centroid from screen derivatives of world XZ and the blend field. Pixels belonging to one stone therefore use one approximately common transition value rather than allowing the contour to cross the stone.
5. Generate deterministic retention from centroid world position and blend it toward prominence through `Stone Retention Bias`.
6. In the thinning band, retain or remove the stone as a unit. Multiply its final silhouette by generated membership and packed cavity according to `Cavity Edge Attraction`.
7. Preserve full substrate coverage in the interior core. Use `fwidth` only for screen-width edge antialiasing; do not restore a broad opacity blend.
8. Feed the adjusted Bank/Riverbed coverage into the existing substrate composition, normal weighting, cover retention, smoothness, wetness, and lighting paths.

### Inspector and validation

- Show the four transition controls only for detail-enabled authored material-set entries in the dedicated material editor and the existing inline `Shared Material Definition` foldout.
- Extend the existing one-button Surface Material Validation report with segmentation threshold, element count, coverage, area/prominence distribution, centroid reconstruction error, channel contract, runtime style resolution, and sample-count statement.
- Emit local-only PNG evidence under `Library/SurfaceMaterialDiagnostics`: component labels, prominence, and deterministic thinning previews at several transition positions. No new Inspector button or debug view.

### Invariants and performance

- Continuous materials are byte-for-byte behaviorally unchanged at runtime.
- Automatic mode changes authored material-set transitions only; palette, normal, cavity, roughness, wetness, and material identity remain M2.3.
- No extra texture-array sample, texture, draw call, geometry, runtime allocation, CPU callback, or per-frame field build.
- Added runtime cost is bounded fragment ALU and derivatives only where an authored material-set substrate is active.
- Stone segmentation and mip construction are editor rebuild work only.
- M2.1 periodic seam repair and M2.3 single-palette authority remain mandatory.
- The V1A.6 vegetation coverage contract and V1A.6.1 object-search repair must remain unchanged.

### Validation gates

1. Exact twelve-file source/document scope; `SSDL_DefaultSurfaceDetails.asset` may change only through Unity regeneration.
2. Parse every changed C# file and affected unchanged callers; scan new symbol references and required namespaces.
3. Verify shader delimiters, function arity, property parity, and unchanged texture-sample statement/count.
4. Synthetic builder tests: toroidal edge-crossing components, disconnected stones, merged-domain threshold fallback, centroid reconstruction, mip reconstruction, and no stone membership in cavity pixels.
5. Unity: compile, rebuild the library, run the existing one-button report, and inspect component/prominence/thinning PNGs.
6. Production camera: confirm no broad green/stone crossfade, no stones cut through by the transition, no repeat seam, no temporal crawl, and unchanged continuous-material behavior.

### Implementation result

- Added reusable `Automatic`, `Continuous`, and `Discrete Elements + Cavity Cut` transition styles. `Automatic` resolves to the discrete path only when the selected detail-library entry has an authored texture-form payload.
- Repacked the existing authored texture-form RGBA32 slice without changing its format or sample count: R remains normalized form, G/B carry shortest toroidal centroid offsets, and A carries element membership/prominence.
- Added editor-time adaptive cavity segmentation with toroidal eight-neighbour connectivity, circular centroids, area-derived prominence, explicit metadata-preserving mips, and R-only periodic form repair. The generation algorithm version is now `5`, forcing Unity to regenerate stale arrays.
- Added whole-element runtime retention at the estimated element-centroid blend, prominence-biased deterministic thinning, generated membership, and a derivative-width cavity silhouette. The full material remains intact in the interior core.
- Reused the hidden legacy authored-tint vector as the transition transport slot. Enable is encoded with sentinel value `2`, so the shader property's legacy white default (`x=1`) remains neutral. No shader property or material asset was added.
- Added the four material-owned transition controls to the dedicated material editor and existing inline Shared Material Definition only for enabled authored material sets.
- Extended the existing one-button validation report and local diagnostic output with element coverage, threshold, area, centroid-error, component-label, prominence, and 25/50/75-percent thinning evidence.
- Preserved the V1A.6 Ground vegetation-coverage runtime/editor contract and V1A.6.1 nonobsolete object-search overload.

### Static and offline audit result

- Exact modified scope: **PASS — the twelve declared files and no others** when compared with the latest archive plus V1A.6/V1A.6.1 overlays. No scene, prefab, material asset, source texture, River file, vegetation benchmark, or Painted Accent file changed.
- C# syntax parse: **PASS — seven changed C# files, zero error or missing nodes**. New references and required `System`, collection, UnityEngine, and UnityEditor imports are present.
- HLSL structural audit: **PASS** for delimiters, the four-argument texture-form assignment and transition-coverage callers, transition property declarations, and enable-sentinel transport.
- Runtime sampling parity: **PASS — the Ground forward pass remains four `SAMPLE_TEXTURE2D_ARRAY` calls and one `SAMPLE_TEXTURE2D` call**. No draw call, texture, geometry, allocation, CPU callback, or per-frame segmentation was added.
- Supplied serialized Fine Gravel evidence: adaptive p60 threshold `0.345098`; `210` detected elements; `59.9304%` element coverage; largest component `7.5517%` of element pixels; mean component area `187.0286` pixels; quantized centroid reconstruction max/p95 `0.00870086 / 0.00710501` UV; zero cavity pixels labelled as stones.
- Synthetic connectivity evidence: toroidal edge-crossing domain resolved as one component and two disconnected domains resolved as two. Deterministic thinning retained `129 / 45 / 15` complete elements at 25/50/75-percent progress.
- V1A.6 preservation audit: **PASS** for `VegetationCoverageInitialized`, `VegetationCoverageRevision`, `CalculateVegetationCoverageFraction`, `TrySampleVegetationCoverage`, Scene-view callback registration, and absence of `FindObjectsSortMode`.
- Full Unity/Roslyn compilation, generated-subasset inspection, live seam/transition report, camera stability, and GPU timing: **unavailable here and pending, not passed**.



## 2026-07-19 — GSU-M2.3: Single-Palette Surface Material Control Pass

**Status:** Implemented in the reconstructed supplied-source workspace and statically/offline audited. Exact fourteen-file scope, C# syntax parsing, HLSL delimiter/caller checks, serialized compatibility, runtime sample-count parity, real-array form normalization, palette-band coverage, and simulated periodic mips pass. Unity compilation, source-JPEG-driven form-array rebuild, production-camera colour/seam validation, copied control-integrity report, profiling, and user visual acceptance remain pending.

### Objective

Make `Base Color`, `Dark Color`, `Light Color`, and `Cavity Color` the single visible colour-authoring system for every reusable stylized surface material. Preserve the imported material-set texture only as a grayscale structural form input, not as a second colour source. Remove or hide controls whose resolved coefficient is zero, preserve serialized assets and the M2.1 periodic repair, and keep the existing runtime texture-sample count, array dimensions, draw calls, and River contracts.

### Read-only review and evidence

Current source was reconstructed from the supplied `Assets-Code-Archive.zip` plus the accepted GSU-M2.1 and GSU-M2.2 patches. The supplied source has no `.git` metadata; branch, `HEAD`, history, and unrelated working-tree changes are unavailable. The following current implementations and direct callers/consumers were reviewed before this plan edit:

- `StylizedSurfaceMaterialProfile.cs`: `payloadMode` manually selects `UsesAuthoredColor`; authored tint has a separate strength; `DetailValueStrength`, `DetailFormHighlightStrength`, and `FinishVariationStrength` are forced to zero in Authored Color mode.
- `StylizedSurfaceDetailLibrary.cs`: entry `sourceMode` already proves whether a material has a full authored material set, and generated slice mapping already resolves that source independently of the profile payload field.
- `StylizedSurfaceDetailLibraryBuilder.cs`: the generated sRGB array currently retains source RGB; M2.1 periodic repair and explicit mips are editor-only and must remain.
- `PixelSurfaceMaterialDetail.hlsl`: `PS3D_ResolveStylizedSurfaceAuthoredColor` uses `sqrt(sourceLuminance)`, retains 35% source chroma, weakens cavity shoulder/core response, and blends through a separate colour path. `PS3D_ResolveStylizedSurfaceDrySmoothness` can replace the profile baseline with `1 - roughness` at strength one.
- `PixelSurfaceGroundForwardPass.hlsl`: Bank and Riverbed are the only callers of the separate authored-colour evaluator. The extra array sample is already present and must not increase.
- `GroundSurfaceLayerProfile.cs`, `GroundMaterialControls.cs`, and `GeneratedGround.ApplySurfaceLayerDetailProperties`: transport exposes authored colour/tint/lighting/roughness semantics and applies multipliers to values that resolve to zero for the imported material.
- `StylizedSurfaceMaterialProfileEditor.cs` and `GeneratedGroundEditor.cs`: both expose Payload and Authored Color as a second colour system. Bank/Riverbed application panels always show Value/Form, Finish Variation, and Legacy Cell multipliers even when the selected material resolves those coefficients to zero.
- `StylizedSurfaceMaterialValidation.cs`: validates arrays and seams but has no control-integrity or palette-band-coverage report.

**Proven faults:**

1. `Authored Color Tint` is mathematically inert at tint strength zero, while the two controls are presented as one colour operation.
2. The imported RGB sample remains a colour/luminance authority after GSU-M2.2; palette colours grade it rather than owning final material colour.
3. `sqrt(sourceLuminance)` places nearly all measured source texels in the Base-to-Light branch, leaving Dark Color with negligible ordinary-surface coverage.
4. Imported-material cavity response uses lower coefficients than Palette Detail, so the same visible Cavity Color has materially different authority.
5. At authored roughness strength one, the profile Dry Smoothness baseline is replaced rather than modulated.
6. Bank/Riverbed application UI exposes multipliers whose profile-side coefficient is exactly zero.

Controls with valid current runtime paths and no proven fault remain unchanged unless their labels must reflect the unified contract: Detail World Scale, Detail Normal Strength, Detail Cavity Strength/Bias, Dry Specular Strength, material coverage/reach/transition, hydrology/wetness, and cover compatibility.

### Approved files

First plan update and implementation may modify only:

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs
Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
```

Expected Unity-generated output after the generation-version change:

```text
Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset
```

The generated asset is not to be hand-edited. No River source, geometry, hydrology, UV3, scene, prefab, material, purchased source texture, layer, tag, renderer, shader property name, or debug view may change.

### Implementation sequence

1. Preserve the serialized `payloadMode`, authored tint, and existing authored-value fields for backward compatibility, but hide the obsolete payload/tint controls and remove them from runtime colour authority. Determine texture-form capability automatically from the selected detail-library entry's `AuthoredMaterialSet` source mode.
2. Increment the editor generation algorithm version. Area-reduce source base colour as before, convert it to linear luminance, map the source 5th percentile / median / 95th percentile to form values `0 / 0.5 / 1`, encode the normalized grayscale form into the existing sRGB array, then retain M2.1 conditional periodic repair and explicit mips. Source hue must not enter the runtime array.
3. Replace the separate authored-colour evaluator with one shared palette resolver. Texture form contributes signed Dark/Base/Light variation through the same path as prepacked form; the common cavity shoulder and full cavity core apply identically to both source types.
4. Reinterpret the existing serialized authored controls as `Texture Form Strength`, `Scene Lighting Response`, and `Roughness Variation`. Dry Smoothness remains the baseline; roughness contributes bounded centred variation and cannot replace that baseline.
5. Update Ground transport without adding material properties or samples. Keep existing hidden shader property names for serialized/material compatibility while changing their documented semantics.
6. Make Bank and Riverbed application controls capability-aware. Show scale/form/lighting/normal/cavity/value-finish/legacy multipliers only when the selected resolved material has a nonzero coefficient for that control.
7. Update the dedicated and inline material editors so Palette is the only colour section. Update the preview to consume normalized texture form and the common palette/cavity/smoothness direction.
8. Extend the existing one-button validation report with control-integrity and form-distribution evidence, including missing form data, negligible Dark/Base/Light coverage, and active/inactive application-control capability.
9. Update canonical architecture and Inspector documents, then run exact-scope, parser/compiler, namespace/import, shader delimiter/caller, serialized-compatibility, generated-signature, algorithm, and performance audits.

### Invariants and non-goals

- Base, Dark, Light, and Cavity are the only visible colour pickers.
- The imported source contributes structural value/form only; source hue is discarded.
- Existing serialized assets retain their values; obsolete payload/tint fields remain hidden compatibility data.
- Palette Detail materials retain their existing packed-alpha value/form and finish behavior.
- M2.1 seam repair, complete mips, 256² RGBA32/sRGB arrays, Repeat/Bilinear sampling, and non-readable runtime arrays remain.
- Runtime remains one packed-detail sample plus one texture-form sample only for entries that contain an authored material set. No new sample, branch class, draw call, allocation, texture, geometry, or CPU callback is approved.
- No default profile asset or source JPEG is edited by the patch.
- No unrelated control formula is retuned without new concrete evidence.

### Implementation result

- `payloadMode`, authored tint, and their existing serialized names remain hidden compatibility data, but runtime capability now resolves automatically from the selected library entry's source mode.
- Imported base colour is converted editor-side to normalized grayscale texture form using linear-luminance p05 / median / p95 anchors. Source hue is absent from generated form data.
- The M2.1 repeat-edge repair remains conditional and narrow-band, but now averages and interpolates decoded linear form values before re-encoding, avoiding gamma-space form distortion.
- Prepacked and imported entries now use one `PS3D_ResolveStylizedSurfacePalette` path with common Dark/Base/Light mapping, common cavity shoulder, and full common Cavity Color core.
- `Texture Form Strength`, `Scene Lighting Response`, and `Roughness Variation` reuse existing serialized values as structural/finish controls. Dry Smoothness remains the additive baseline at every roughness-variation value.
- Both custom material editors expose one Palette section only. Detail-dependent variation controls are not shown when Structural Detail is disabled.
- Bank and Riverbed application panels resolve the selected material and display only multipliers whose material-side coefficient is nonzero; scene-lighting response is independently omitted when its base coefficient is zero.
- The existing one-button validation action now reports automatic source capability, grayscale channel integrity, p05/median/p95, Dark/Base/Light coverage, periodic mip evidence, and active control capability while preserving report-file and clipboard output.
- No generated array asset, imported material/profile asset, source art, shader declaration, scene, prefab, River source, hydrology, geometry, UV3, layer, tag, renderer, or debug view is included in the patch.

### Static and offline validation result

- Exact modified-file scope: **PASS — 14 declared files and no others**.
- Tree-sitter C# parse: **PASS — 9 changed C# files, zero error/missing nodes**.
- C#/HLSL lexical delimiter scan and changed caller arities: **PASS**.
- Runtime sampling parity: **PASS — `SAMPLE_TEXTURE2D_ARRAY` remains 4 and `SAMPLE_TEXTURE2D` remains 1 in the Ground forward pass**.
- Existing serialized M2.2 array simulation: p05 `0.079202`, median `0.159695`, p95 `0.353858`; normalized form coverage Dark `45.36%`, Base `15.84%`, Light `38.80%`; RGB channel delta `0`.
- Simulated texture-form mip 0 repeat repair: left/right mean and p95 ratios reduce to `0`; unaffected top/bottom ratios remain within M2.1 thresholds. Simulated mips 1–4 also pass.
- Full Unity/Roslyn and shader compilation: unavailable in this environment and therefore pending, not passed.

### Acceptance criteria

- Payload Mode and Authored Color colour controls are absent from both material authoring interfaces.
- Base, Dark, Light, and Cavity independently produce visible, appropriately localized changes on the imported material.
- Generated texture form contains grayscale only and reports meaningful Dark/Base/Light coverage.
- Cavity Color reaches the same deep-core authority for imported and prepacked materials.
- Dry Smoothness changes the baseline at every Roughness Variation value.
- No displayed Bank/Riverbed application multiplier resolves to zero for the selected material.
- The prior periodic seam remains within M2.1 limits.
- Runtime sample count, shader property count, array dimensions, draw calls, and memory layout do not increase.
- The existing comprehensive validation action copies one report containing seam and control-integrity evidence.

### Validation status

- Read-only review: **PASS** for the current reconstructed source and approved scope.
- Canonical plan persistence: **PASS** after this section is written.
- Implementation: **PASS — source work complete in exact approved scope**.
- Static/compile-oriented audit: **PASS for available parser, delimiter, caller, scope, compatibility, sample-count, and offline-array checks**.
- Unity compile, source rebuild, copied live report, visual acceptance, and profiling: **PENDING**.

## 2026-07-19 — GSU-M2.2: Authored-Color Palette Control and Encoded-Color Preservation

**Status:** Implemented in the supplied code workspace and statically audited. Exact eight-file scope passed. Unity compilation, source-driven array rebuild, production-camera colour/seam validation, and user visual acceptance remain pending.

### Objective

Restore the imported authored-colour material to an authorable brightness/hue range after GSU-M2.1 and make the existing reusable `Base Color`, `Dark Color`, `Light Color`, and `Cavity Color` controls materially affect Authored Color payloads. Preserve the seam correction, source artwork, runtime array size, sample count, River contracts, and reusable material ownership.

### Read-only evidence

- User screenshot after the M2.1 rebuild shows the Bank/Riverbed authored material rendered near-black while the visible palette controls cannot restore colour.
- `StylizedSurfaceDetailLibraryBuilder.CopyGeneratedMipChain` writes M2.1 authored colour through `Texture2DArray.SetPixels(Color[])`; `AreaResamplePixels` reads through `Texture2D.GetPixels`. The destination is an sRGB `Texture2DArray`. The current code does not explicitly preserve encoded source bytes across the CPU resample/upload path.
- `PS3D_ResolveStylizedSurfaceAuthoredColor` returns `lerp(paletteColor, authored, authoredColorStrength)`. At the candidate default strength `1`, `paletteColor` is removed entirely. `Base Color` and `Light Color` therefore have zero direct influence over ordinary authored texels; `Dark Color` only multiplies cavity shoulders and can only reduce values; `Cavity Color` affects deep gaps.
- `StylizedSurfaceMaterialProfileEditor` preview repeats the same bypass by blending directly from `BaseColor` to the authored sample, so preview and runtime both lack the requested four-colour authored grading contract.
- Repository search finds only two runtime calls to `PS3D_ResolveStylizedSurfaceAuthoredColor`, both in `PixelSurfaceGroundForwardPass.hlsl` for Bank and Riverbed. No River geometry, hydrology, UV3, scene, prefab, or material asset owns this behaviour.

### Approved implementation files

Modify only:

```text
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

Expected Unity-generated output after the generation-version change:

```text
Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset
```

No profile schema, source texture, River source, hydrology, geometry, UV3, material-property name, scene, prefab, material, layer, tag, renderer, or debug-view edit is approved.

### Implementation sequence

1. Increment the authored-colour generation algorithm version so existing arrays rebuild.
2. Read base-colour source pixels as `Color32`, perform the existing area reduction/seam repair in encoded colour values, and upload each RGBA32 mip with raw `SetPixelData(Color32[])`. This removes implicit `Color` format conversion from the sRGB array write while retaining M2.1 periodic generation and diagnostics.
3. Change the authored-colour evaluator so source luminance selects between the existing Dark/Base/Light palette colours. Preserve a restrained portion of source chroma, then apply existing cavity shoulder/core response. `Authored Color Strength` continues to blend between the ordinary palette result and the palette-graded authored result.
4. Mirror the runtime grading in the material Inspector preview.
5. Label and explain the existing palette controls in both the dedicated material Inspector and GeneratedGround inline editor so authors know they apply to Authored Color payloads.
6. Update canonical architecture/Inspector documents and run parser, delimiter, property/signature, caller, scope, and generated-signature audits.

### Implementation result

- `AuthoredColorGenerationAlgorithmVersion` is now `3`, invalidating the M2.1 generated array so Unity rebuilds it.
- Authored source pixels are read with `GetPixels32`; the existing area reduction, conditional periodic repair, and explicit mip construction continue in normalized encoded values. Each RGBA32 mip is quantized to `Color32` and uploaded with raw `Texture2DArray.SetPixelData`, avoiding implicit colour conversion during the sRGB array write.
- The authored evaluator converts sampled linear luminance to a perceptual value with `sqrt`, maps that value through Dark/Base/Light, retains 35% of the source chroma offset, and applies cavity shoulder/core through Dark and Cavity Color. All four visible palette fields therefore participate at Authored Color Strength `1`.
- The dedicated material preview mirrors the same value mapping and restrained chroma retention.
- Both material authoring surfaces label the fields `Palette (Applied to Authored Color)` and explain their ownership. No new serialized field was added.
- Static audit found exactly the eight declared modified files. `SAMPLE_TEXTURE` counts are unchanged; the shared evaluator has exactly two runtime callers and both pass the new Light Color argument.

### Static validation result

- Custom lexical syntax/delimiter scan: PASS for all changed C# and HLSL files.
- Function definition/caller arity audit: PASS (`5` arguments at both Bank and Riverbed calls).
- Namespace/import scan: PASS; new C# types are supplied by the existing `UnityEngine` import.
- Exact scope and line-ending audit: PASS; `GeneratedGroundEditor.cs` retains CRLF and no serialized asset/source file changed.
- Mathematical control-responsiveness cases: PASS for independent Base, Dark, Light, and Cavity changes.
- Runtime sample-count comparison against M2.1: unchanged.
- Full Unity/Roslyn compilation: unavailable in this environment and therefore pending, not passed.

### Invariants and performance

- Runtime remains one authored-colour sample per active authored layer; no added sample, texture, draw call, CPU callback, allocation, geometry, or material property.
- The new grading is fixed arithmetic on the already-sampled colour. It adds no branch beyond the existing authored path and no memory.
- Generated arrays remain 256² RGBA32/sRGB, complete-mip, Repeat/Bilinear, and non-readable.
- Palette Detail behaviour remains unchanged.
- M2.1 seam measurement, conditional edge repair, explicit mip generation, and diagnostics remain unchanged.
- Original purchased source bytes and GUIDs remain unchanged.

### Acceptance criteria

- Changing Base, Dark, Light, and Cavity Color visibly changes the imported surface at Authored Color Strength `1`.
- White/light palette values can restore a clearly visible mid/high value range; dark values can intentionally darken it.
- Source stone value structure and restrained per-stone chroma remain readable.
- The prior long repeat line does not return.
- Dedicated material preview and production shader respond in the same direction.
- No unrelated file changes and no runtime resource/sample-count change.

## 2026-07-19 — GSU-M2.1: Periodic Authored-Color Import and Seam Diagnostics

**Status:** Implemented in the supplied code workspace and statically audited. The current serialized authored-colour array is proven non-periodic at its horizontal repeat boundary. The editor-only generation/validation correction is complete; Unity compilation, source-driven array rebuild, production-camera visual acceptance, build exclusion, memory evidence, and GPU timing remain pending. No River, hydrology, geometry, normal, lighting, profile-schema, or runtime implementation change was made.

### Objective

Preserve the accepted visual quality of `Fine Gravel — Imported Stone Ground 01` while removing the repeating authored-colour line. Replace point-sampled authored-colour reduction and implicit temporary-texture mip generation with deterministic editor-only area reduction, bounded periodic edge repair when required, explicit mip construction, and one-button seam evidence. Runtime texture dimensions, formats, samples, profiles, shaders, Ground transport, River semantics, and source artwork remain unchanged.

### Read-only evidence

The current code archive contains the live generated `SSDL_DefaultSurfaceDetails_AuthoredColorArray` subasset in `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset`:

- 256×256, depth 1, RGBA32/sRGB, nine mips, non-readable;
- generated stable entry `fine-gravel-imported-stone-ground-01` resolves to colour slice 0;
- `StylizedSurfaceDetailLibraryBuilder.Rebuild` calls `ResamplePixels`, which takes one `GetPixelBilinear` sample per output texel, then `CopyGeneratedMipChain`, which delegates all mips to `Texture2D.Apply(true, false)`;
- the current validation report checks dimensions, references, and importer contracts but does not measure repeat boundaries or emit tiled previews.

The serialized array byte payload was decoded read-only and measured in normalized RGB. Boundary difference is compared with ordinary adjacent-pixel difference on the same axis:

| Mip | Size | Left/right mean ratio | Left/right p95 ratio | Top/bottom mean ratio | Top/bottom p95 ratio |
|---:|---:|---:|---:|---:|---:|
| 0 | 256 | 1.212 | 1.238 | 0.963 | 0.947 |
| 1 | 128 | 1.130 | 1.069 | 0.894 | 0.913 |
| 2 | 64 | 1.101 | 1.097 | 0.940 | 0.879 |
| 3 | 32 | 1.131 | 1.116 | 0.965 | 1.048 |
| 4 | 16 | 0.854 | 0.974 | 1.188 | 1.099 |

**Conclusion — proven:** the generated authored-colour payload already contains an excessive left/right repeat discontinuity at mip 0 and retains elevated horizontal boundary differences through mips 1–3. The shader samples this array with repeated world-XZ UVs, so the array can produce the observed recurring line without any River-domain defect.

**Unverified:** the code-only archive omits the purchased source JPEGs, so this review cannot separate source-edge mismatch from point-resampling loss. The implementation must therefore guarantee periodic generated output while leaving the original source unchanged.

### Approved implementation files

Modify only:

```text
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

Expected Unity-generated output after the revised signature forces a rebuild:

```text
Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset
```

Expected local-only diagnostics:

```text
Library/SurfaceMaterialDiagnostics/<Library>_<StableId>_SourceBaseColor_3x3.png
Library/SurfaceMaterialDiagnostics/<Library>_<StableId>_GeneratedColor_Mip0_3x3.png
Library/SurfaceMaterialDiagnostics/<Library>_<StableId>_GeneratedColor_Mip1_3x3.png
Library/SurfaceMaterialDiagnostics/<Library>_<StableId>_GeneratedColor_Mip2_3x3.png
Library/SurfaceMaterialDiagnostics/<Library>_<StableId>_GeneratedColor_Mip3_3x3.png
Library/SurfaceMaterialDiagnostics/<Ground>_SurfaceMaterialValidation.txt
```

No profile schema, serialized authoring field, shader, Ground runtime, River, scene, prefab, material, source texture, layer, tag, renderer, mesh, or debug-view edit is approved.

### Invariants and non-goals

- Preserve source JPEG bytes and GUIDs.
- Preserve `fine-gravel-imported-stone-ground-01`, array resolution 256, RGBA32 formats, complete mip chain, Repeat/Bilinear sampling, and non-readable generated arrays.
- Preserve the M2.0 runtime path and its already-approved optional authored-colour sample count.
- Preserve all `PaletteDetail` generation and copies byte-for-byte in behavior.
- Do not add stochastic tiling, texture bombing, triplanar sampling, UV offsets, runtime seam branches, runtime allocations, or extra texture samples.
- Do not alter normal, height, AO, roughness conversion in this correction.
- Do not hide the line by weakening authored colour, changing world scale, moving River boundaries, or tuning material response.

### File-by-file implementation sequence

1. In `StylizedSurfaceDetailLibraryBuilder.cs`, add an authored-colour generation algorithm version to the library signature so existing generated arrays rebuild automatically after the patch.
2. Replace authored-colour point reduction with exact weighted box-area reduction for downscales. The current 2048→256 source therefore contributes its complete 8×8 footprint to each output texel rather than one centre sample. Preserve bilinear fallback only for source dimensions smaller than the destination.
3. Measure horizontal and vertical boundary mean and p95 differences against ordinary same-axis neighbor differences. Apply no repair when both axes are within the accepted threshold.
4. When an axis exceeds the threshold, repair only a narrow eight-texel band at 256², scaled down with each mip. Blend symmetric samples across that repeat boundary with a smooth cubic falloff: exact averaging at the outermost pair, decreasing to zero at the inner edge of the band. This changes only the generated copy and avoids a full-tile colour ramp or central cross.
5. Construct every authored-colour mip explicitly from the preceding corrected mip by deterministic 2×2 box averaging, then evaluate and repair that mip independently. Write each mip directly to the destination array.
6. Extend `StylizedSurfaceMaterialValidation.cs` to use the same managed generation path, record pre/post seam metrics for each mip, fail generated mean ratio above 1.15 or p95 ratio above 1.25, and write the source/generated three-by-three diagnostic PNGs. Keep the existing report file and clipboard behavior.
7. Update the three canonical documents with the final implementation, runtime invariants, validation state, and remaining Unity/profile gates.

### Implementation result

- `AuthoredColorGenerationAlgorithmVersion = 2` participates in the generated-library signature, so the existing M2.0 array becomes stale and rebuilds in Unity.
- Authored base colour now uses exact weighted box-area reduction for downscales; the 2048→256 case consumes every source texel in its corresponding 8×8 footprint.
- Generated seam repair is conditional. It touches only an eight-texel band at 256², scales that band with each mip, averages the outermost repeat pair exactly, and feathers inward with a cubic falloff.
- Every authored-colour mip is built explicitly from the corrected preceding mip and is measured/repaired independently before direct `Texture2DArray.SetPixels` upload.
- The existing comprehensive validation action now caches one managed authored-colour build per shared library entry, reports pre/post mean and p95 ratios for every mip, emits one source-derived and mip 0–3 tiled diagnostic set, preserves report-file output, and copies the complete report to the clipboard.
- Static scope audit found exactly the five declared modified files and no River, shader, runtime transport, schema, serialized source, scene, prefab, layer, tag, or material edit.
- Synthetic periodic/horizontal/vertical/two-axis tests pass. A read-only simulation against the serialized M2.0 mip 0 changes the failing left/right mean ratio from `1.212` to `0.000` while the unaffected top/bottom mean ratio remains within threshold (`0.960`). This is algorithm evidence, not a substitute for the required Unity rebuild and production-camera result.

### Acceptance criteria

- Generated authored-colour mip 0–3 mean boundary ratios are at most 1.15 on both axes; p95 ratios are at most 1.25.
- Three-by-three previews show no long repeat line and no new broad cross, strip, or full-tile gradient.
- The purchased source hash remains unchanged.
- Existing `PaletteDetail` entries retain their existing source mip-copy path.
- Unity rebuilds the library because the generation algorithm version participates in `GeneratedSignature`.
- Generated arrays remain 256² RGBA32 with nine mips and `isReadable == false` after rebuild.
- The runtime shader sample count, draw calls, CPU work, GC, geometry, and memory dimensions are unchanged from M2.0.
- The same River scene remains line-free with authored colour strength 1 across the previously tested world-scale range.

### Performance

- **Active gameplay:** no change from M2.0. No new sample, branch, allocation, callback, draw call, texture, geometry, or memory dimension.
- **Dirty/editor CPU:** the current 2048→256 authored base colour performs approximately 4,194,304 weighted source-pixel contributions instead of 65,536 point samples, followed by 87,381 mip texels and narrow edge repair. This is rebuild-only work and is accepted under the project priority order.
- **Storage:** local PNG evidence is written under `Library`; no new runtime or repository asset is created.
- **Memory:** temporary managed colour arrays exist only during editor rebuild/validation. Final array memory remains approximately 0.333 MiB for one full-mip RGBA32 colour slice.

### Validation plan

- Parse every changed C# source and scan introduced symbols/usings.
- Audit exact modified-file scope and line endings.
- Verify the algorithm version participates in `CalculateSignature`.
- Run deterministic synthetic tests for already-periodic, horizontal-seam, vertical-seam, and two-axis-seam images; confirm repair is conditional and post-repair thresholds pass.
- Decode the existing serialized array as the pre-edit baseline and compare generated diagnostics after Unity rebuild.
- Unity compilation, actual source-driven rebuild, production-camera visual acceptance, Memory Profiler evidence, player-build source exclusion, and Ground-pass GPU timing remain explicit user-side gates.


## 2026-07-19 — GSU-M2.0: Optional Authored-Color Surface Materials

**Status:** Implemented and statically audited. Unity compilation, array rebuild, visual acceptance, Memory Profiler evidence, and GPU profiling remain pending.

### Objective

Add an optional authored-color payload to the existing reusable `StylizedSurfaceMaterialProfile` architecture, import the supplied `Stylized_StoneGround_01` material set as a temporary Fine Gravel candidate, and keep every current palette-detail material behaviorally unchanged. The original purchased maps are editor-only authoring inputs. Runtime output remains generated 256×256 texture-array slices.

### Reviewed evidence

The pre-edit review covered the current material profile, Ground adapter, library, builder, profile/library editors, Ground transport, Bank/Riverbed application controls, shader properties, packed-detail decoder, albedo/smoothness/specular evaluation, current serialized Fine Gravel assets, and the five canonical Ground documents. Primary reviewed paths:

```text
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryEditor.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs
Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
```

The supplied source maps are 2048×2048 JPEGs, not 4K. They remain development inputs only:

```text
Stylized_StoneGround_01_basecolor.jpg
Stylized_StoneGround_01_normal.jpg
Stylized_StoneGround_01_normalogl.jpg
Stylized_StoneGround_01_height.jpg
Stylized_StoneGround_01_ambientocclusion.jpg
Stylized_StoneGround_01_roughness.jpg
Stylized_StoneGround_01_metallic.jpg
```

`normal` and `normalogl` differ primarily by green-channel inversion. GSU-M2.0 uses `normal` as the DirectX-style source and records an explicit per-entry green-flip option. Metallic is ignored because the supplied map is effectively black and stone remains nonmetallic.

### Current architecture findings

- `StylizedSurfaceDetailLibrary.Entry` currently owns one editor-only prepacked source and resolves one generated linear RGBA detail array by stable ID.
- `StylizedSurfaceMaterialProfile` currently owns palette/detail/finish only.
- Ground Bank and Riverbed each sample at most one packed-detail array and apply application-specific scale, normal, cavity, form/value, finish, and legacy-cell multipliers.
- The current packed alpha channel is interpreted as signed form/value and signed finish variation.
- The current shader has no authored-color array and therefore cannot preserve the purchased pack's painted stone colour, worn edges, and broad internal form.
- Source references are already editor-only in the library schema; extending that editor-only entry recipe avoids runtime references to the purchased maps.

### Approved files

Implementation may modify only the following existing files:

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryEditor.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs
Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset
```

Approved additions:

```text
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs
Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel_ImportedStoneGround01.asset
Assets/Game/Demo/Profiles/Ground/Layers/GSLP_FineGravel_ImportedStoneGround01.asset
Assets/Game/ArtSources/Editor/SurfaceMaterials/StylizedStoneGround01/**
```

### Invariants and non-goals

- Existing palette-detail profiles must serialize and render identically when the new authored-color mode is disabled.
- No River source, River geometry, masks, UV3, hydrology, scene, prefab, layer, tag, renderer, mesh, or draw-call architecture changes.
- The original 2048 source maps must remain under an `Editor` folder and must not be referenced by runtime profile fields.
- Runtime material data remains 256×256 with mipmaps.
- Imported materials add one optional authored-color sample only where that payload is enabled.
- Metallic remains zero.
- Canonical `SSMP_FineGravel` is not overwritten before Unity comparison and explicit acceptance.
- Matching Bank/Riverbed sample reuse is deferred because their independent application scales and response multipliers prevent safe raw-sample reuse without a larger evaluator rewrite. Profiling will determine whether that optimization is required.

### File-by-file implementation sequence

1. Extend the library entry recipe and builder to generate a linear packed-detail array plus a compact sRGB authored-color array with per-entry colour-slice mapping.
2. Extend reusable material profiles and the Ground adapter with an explicit payload mode and authored colour, tint, scene-lighting, and roughness controls.
3. Extend Ground Bank/Riverbed application controls and transport with authored-colour and scene-lighting multipliers.
4. Extend shader properties, sampling, palette resolution, roughness evaluation, and final lighting blend while preserving the legacy path.
5. Add an editor-only comprehensive validation report that writes to `Library/SurfaceMaterialDiagnostics` and copies the report to the clipboard from one Inspector action.
6. Add the editor-only source maps, library material-set recipe, imported reusable profile, and Ground adapter.
7. Update all canonical documents, remove A5 from active guidance, and record it as rejected historical evidence.

### Acceptance criteria

- Existing palette-detail profiles resolve no authored-color array and retain their previous shader path.
- The imported candidate resolves one 256² colour slice and one 256² packed-detail slice from the same stable entry ID.
- Detail R/G derive from the supplied normal, B from height/AO contact recession, and A from roughness.
- The imported source files are all under an `Editor` folder; the validation report finds no runtime field referencing them.
- Bank and Riverbed expose authored-colour and scene-lighting application multipliers with neutral default `1`.
- The imported profile remains reusable outside River code.
- Static parsing and reference/import checks pass for every changed source file.
- Unity compilation, library rebuild, visual comparison, Memory Profiler evidence, and GPU profiling remain explicit pending gates.

### Risks

- Authored base colour contains painted form shading; excessive dynamic lighting can double-light it. The profile and per-application lighting controls must default to a restrained imported value while legacy materials remain at full lighting.
- Source normal convention may be inverted. The entry recipe stores an explicit green-channel flip, and Unity visual validation remains authoritative.
- JPEG source compression can introduce small data errors. The supplied pack is accepted as the source; the builder normalizes and downsamples once at editor time.
- A second sample increases texture bandwidth only for authored-colour materials. The quality trade is intentional and must be profiled in the production camera.

### Post-implementation consistency and compliance audit

**Result:** The final source/asset diff matches the approved GSU-M2.0 scope. No River implementation, River geometry, hydrology, scene, prefab, material, layer, tag, renderer, mesh, or unrelated subsystem file is included. The shared shader impact is limited to the Ground shader and its Ground-specific includes.

Implemented existing-file modifications:

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs
Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryEditor.cs
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs
Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset
```

Implemented additions:

```text
Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialValidation.cs (+ meta)
Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel_ImportedStoneGround01.asset (+ meta)
Assets/Game/Demo/Profiles/Ground/Layers/GSLP_FineGravel_ImportedStoneGround01.asset (+ meta)
Assets/Game/ArtSources.meta
Assets/Game/ArtSources/Editor.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/StylizedStoneGround01.meta
Assets/Game/ArtSources/Editor/SurfaceMaterials/StylizedStoneGround01/* (seven 2048² source JPEGs plus metas)
```

Static verification completed:

- Tree-sitter C# parsing reports no syntax-error or missing nodes in all ten changed/new C# files.
- Method-declaration/invocation arity matches for the changed Bank, Riverbed, and material-property transport functions.
- Shader/HLSL braces, parentheses, brackets, and preprocessor blocks balance in all four changed shader files.
- All six authored-colour shader property names match across `GeneratedGround`, ShaderLab properties/resources, the Ground material CBUFFER, and the Ground forward evaluator.
- New ScriptableObject YAML script GUIDs match their current `.cs.meta` files; the imported layer resolves the imported material GUID; the material resolves the existing detail-library GUID; and every source-map GUID resolves to the supplied editor-only map.
- All seven supplied maps are 2048×2048. Base colour is authored as sRGB; normal, height, AO, roughness, and metallic are linear; metallic is retained only as editor source evidence and is not consumed.
- The palette-detail path is neutral by construction: authored enable/strength defaults to zero, authored payload decode remains zero, the prior palette/cavity result is selected, the prior smoothness equation is selected, and the final authored-lighting blend has zero coverage.
- The imported material adds one 256² RGBA32 colour slice and one 256² RGBA32 packed slice with full mip chains after the Unity rebuild. The uncompressed upper-bound estimate is approximately 0.667 MiB for those two slices. Compression is not claimed or applied by this patch.
- A compile-risk correction discovered during audit replaced an invalid `List<int> ?? int[]` null-coalescing expression with an explicit `IReadOnlyList<int>` return path. The validation report also now requires `/Editor/` placement only for authored material-set sources, not legacy prepacked sources.

Unavailable checks are explicitly pending:

- No Unity executable is available in the reconstruction environment, so C# assembly compilation, ShaderLab/HLSL compilation, array sub-asset generation, and URP runtime rendering were not run.
- Git metadata is absent, so branch, `HEAD`, working-tree status, and commit-history comparison could not be verified. The supplied A5 file baseline was used for the scoped existing-file comparison instead.
- Production-camera visual acceptance, normal green-channel convention, wetness/submersion behavior, actual player-build exclusion, Memory Profiler evidence, and Ground-pass GPU timing remain Unity gates.


## Current authoritative status — 2026-07-18

The GeneratedGround Inspector and Painted Accent production workstream is complete, Unity-validated, and accepted through GI-A1–GI-A4 and PA-B1–PA-B4.1. **GeneratedGround and the broader Ground visual roadmap are not complete.**

The active mission is to finish the restrained-stylized static Ground stack before runtime surface simulation. **V3M — Broad Macro Patch Completion**, **V3R — Ground Elevation Readability**, and the complete **V3S-A4B.3 River-coupled appearance baseline** are Unity-validated and accepted. A2C.4 remains the frozen renderer-isolation baseline and A4B.3 remains the frozen River-coupled placement, hydrology, and highlight baseline. **GSU-M1 — Reusable Stylized Surface Material Foundation** is implemented and source-audited. GSU-M1.3.1 guards the transient missing-array state, GSU-M1.7 adds reusable shared-material editing and neutral Bank/Riverbed application multipliers, and GSU-M1.7.1 corrects the Unity 6.5 `EntityId` compile blocker. Unity evidence rejects the GSU-M1.6 and GSU-M1.7 Fine Gravel payloads. GSU-M1.8 restored the 256² runtime tier and improved rounded packing, but user evidence now rejects its texture as too edge-driven and too shallow: the packed map concentrates slope near stone rims, leaves broad interiors comparatively flat, and bakes inconsistent per-stone directional value that competes with scene lighting. **GSU-M1.9A — Fine Gravel Height-First Texture Reauthor** is visually rejected by Unity evidence: its generated stone bodies remain too flat, noisy, and materially unconvincing. **GSU-M1.9A.1 — Fine Gravel Packed-Source A/B Evaluation** is visually rejected: both image-generated candidates exposed non-periodic edge neighbourhoods and invalid/incoherent packed slope behaviour in Unity. **GSU-M1.9A.3 — Source-Preserved Integrable Stone Form** is visually rejected: Unity exposed tile-axis large/small segregation and insufficient stone-edge definition. **GSU-M1.9A.5 — Source-Art Packed Conversion, Macro Rebalance, and Worn Edge Accent** is visually rejected historical evidence: its packed-only conversion discarded the authored colour and form that made the source attractive. **GSU-M2.0 — Optional Authored-Color Surface Materials** is now the active material gate. It adds one optional sRGB colour-array sample beside the existing packed-detail sample, imports `Stylized_StoneGround_01` as a temporary reusable Fine Gravel candidate, and keeps all source maps editor-only while runtime slices remain 256². Reusable material identity belongs to a generic Pixel Surface profile and detail library, while Ground, River corridors, future roads, and future walls are consumers. Family-recipe tuning remains blocked until Fine Gravel and the required existing surface materials are accepted. **V4 — Contact / Edge Accents** remains queued afterward and excludes River sources.

The accepted current pipeline is:

```text
GeneratedGround authoring façade
→ deterministic mesh-free SurfaceStrokes and ProjectedGlyphs in Edit Mode
→ transient authoritative R8 preview coverage
→ one-button persistent R8 production bake
→ baked-only Play Mode and Player rendering
→ exact pre-build production validation
→ explicit project-wide generated-asset audit and confirmed-orphan cleanup
```

Key production rules:

- the old raised 3D Painted Accent ridge is retired;
- Ink Colour and Ink Opacity are Material-only and do not stale the bake;
- generation, geometry, modifier, River-exclusion, projection, cluster, or raster changes stale the bake;
- Player runtime performs no Painted Accent SurfaceStroke generation, ProjectedGlyph generation, companion solving, coverage rasterization, or procedural upload;
- a required Missing, Stale, Incompatible, duplicated, shared, or ownership-mismatched output blocks the build;
- generated assets are never deleted automatically during bake or build;
- obsolete outputs are removed only after the all-project audit classifies them as Confirmed orphan.

The detailed sections below are the historical implementation ledger. Their patch-local “pending” or “next” language records the state at the time of that patch and does not override this final status. The canonical final ownership and maintenance contract is recorded in `GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`.


### Frozen River-coupled baseline — V3S-A4B.3

A4B.3 is Unity-validated, visually accepted, and frozen. It replaces A4B.2's incorrect outward Riverbed wetness extension with an inward transition contained entirely inside `Ground Riverbed Support`, and restricts the stylized Shore highlight to an independently authored waterline band. The accepted four-component corridor contract is `TexCoord3.x = Riverbed Support`, `.y = outward Bank distance`, `.z = Bank-domain authorization`, and `.w = inward Riverbed distance`; `PixelSurfaceGroundForwardTypes.hlsl` must continue carrying all four components. The corrected River producer remains at the existing plural path `Assets/Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs`. The exact architecture and validation evidence are recorded in `Ground_River_Coupled_Surface_Response_Architecture.md`.

## Maintenance workflow

```text
Normal change affecting coverage
→ Bake Painted Accents

Ground or scene no longer needs its bake
→ Release Production Bake
→ save scene manually
→ Tools > Generated Ground > Audit and Clean Painted Accent Assets...
→ review and delete Confirmed orphan assets only
```

## Next work items

1. Let Unity import the editor-only `StylizedStoneGround01` source maps and rebuild `SSDL_DefaultSurfaceDetails`; confirm four packed-detail slices and one authored-colour slice at 256².
2. Select `Fine Gravel — Imported Stone Ground 01` for Bank and/or Riverbed and run **Run Surface Material Validation** once; paste the complete saved report if it fails.
3. Compare the imported candidate against canonical Fine Gravel from the same close and production cameras in dry, wet, and submerged states; verify normal orientation and adjust only the explicit green-flip setting if lighting is inverted.
4. Profile the Ground pass with canonical Fine Gravel and the imported authored-colour candidate; record draw count, GC, texture memory, and GPU timing.
5. After visual/performance acceptance, promote the imported material to canonical Fine Gravel, delete rejected A1–A5 temporary assets, and archive the 2048 source pack outside active project content if repository size becomes undesirable.

## GSU-M1.9A.1 — Fine Gravel Packed-Source A/B Evaluation — visually rejected; historical

**Status:** Rejected by Unity evidence; no longer actionable.

This temporary test installed `Fine Gravel A - Direct Normal` and `Fine Gravel B - Strong Form`, both produced from image-generated normal-style candidates. Neither candidate had genuinely periodic edge neighbourhoods, and their RGB fields did not constitute coherent packed slope data. Unity exposed visible repeat bands, malformed relief, flattening, and generally inadequate stone form. Do not validate, tune, or promote either A1 payload. GSU-M1.9A.3 overwrites those temporary payloads while retaining their serialized GUID/stable-ID plumbing only for safe migration.
---

## GSU-M1.9A.3 — Source-Preserved Integrable Stone Form — visually rejected; historical

**Status:** Superseded by GSU-M1.9A.4 after Unity exposed macro size segregation and insufficient contour definition.

GSU-M1.9A.1 is visually rejected. Its image-generated candidates were neither genuinely seamless nor valid coherent packed slope fields. GSU-M1.9A.2 remained an offline deterministic investigation only and proved periodic conversion, but its distance-cap reconstruction concentrated useful slope near stone rims and left large interiors too uniform. GSU-M1.9A.3 replaced the two temporary A/B texel payloads while deliberately retaining their existing GUIDs, library stable IDs, importer settings, and Ground-layer references so an installed A1 evaluation is upgraded without orphaning serialized selections. The legacy temporary filenames and stable IDs are cleanup debt only; they must be deleted when a winner replaces canonical `fine-gravel`.

The two historical A3 candidates were rebuilt deterministically from the user-supplied rounded-stone source rather than generated as final textures:

- **`Fine Gravel A3 - Source Preserved`** keeps restrained relief while preserving source-derived silhouettes, size distribution, neutralized internal stone-body cues, localized crowns, irregular shoulders, and hierarchical crevices.
- **`Fine Gravel A3 - Vertical Form`** uses the identical periodic stone layout, B cavity, and A variation, but increases coherent body slope and localized crown amplitude. It was the leading A3 candidate for stronger roundness and verticality before Unity rejected the shared layout and contour treatment.

The non-periodic source boundaries are moved to the centre one axis at a time; only stones intersecting each centre repair band are removed and repacked from extracted source silhouettes on a toroidal 1024² authoring canvas. This preserves most of the supplied layout while making opposite edge neighbourhoods continuous. Each stone uses a continuous side profile plus one or two localized crowns, source-body variation with its directional plane removed, and restrained microstructure. Broad whole-stone white plateaus are prohibited. The final 256² R/G channels are derived from one periodic height field, so the slopes are internally coherent and integrable; B contains a soft contact shoulder and narrower deep gap core; A contains non-directional per-stone and internal form variation.

Offline validation includes 3×3 shader-reference tiling, 256/128/64/32 mip tests, numerical wrap-to-adjacent ratios, per-stone height-distribution evidence, and a CPU reference that reproduces the current packed-detail decode, palette, cavity bands, flat-ground normal perturbation, and material values. The reference uses a simplified ambient/diffuse lighting term and is not claimed to reproduce the complete URP pass. Unity production-camera rendering remains authoritative.

Runtime architecture and cost do not change: three temporary 256² RGBA32 mipmapped slices remain during evaluation, only the selected substrate slice is sampled, and there is no new shader sample, ALU branch, draw call, renderer, mesh data, River data, or runtime CPU process. No C#, HLSL, ShaderLab, River source, scene, prefab, canonical Fine Gravel assignment, or unrelated material changes in this patch.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, historically required comparison of the two A3 choices with identical shared/application values from the same close and production cameras, include dry and wet views, and judge body roundness, internal variation, coherent common light direction, cavity width, repetition, seam visibility, and mip survival. Select a winner or reject both; do not tune the shader to hide a deficient packed source.

---

## GSU-M1 — Reusable Stylized Surface Material Foundation

**Status:** Implemented and source-audited through the rejected GSU-M1.9A.5 packed-only experiments. GSU-M1.3.1 adds null-safe transport, GSU-M1.7 adds generic shared/application authoring, and GSU-M1.7.1 repairs the Unity 6.5 `EntityId` compile blocker. GSU-M1.8 retains authority for the 256 runtime-tier restoration only. GSU-M2.0 now owns the active imported Fine Gravel evaluation path by adding optional authored colour beside the existing packed detail while preserving the frozen River contract. Unity array rebuild, production-camera visual acceptance, and GPU profiling remain pending user validation.

### Objective

Replace the current palette-plus-quantized-cell representation of secondary Ground layers with a reusable, generic stylized material identity that can be consumed by River banks and beds now and by roads, paths, walls, cliffs, and other Pixel Surface renderers later. Fine Gravel is the first proof material. Its acceptance target is a dense illustrated pebble field with readable stone silhouettes, dark inter-stone cavities, restrained per-stone value variation, and lighting-driven pseudo-volume instead of square pixel-cell breakup.

### Implemented result

- `StylizedSurfaceMaterialProfile` now owns reusable dry palette, cavity, natural scale, structural-detail strengths, legacy pixel-cell suppression, and dry finish without Ground, River, road, wall, hydrology, or placement ownership.
- `StylizedSurfaceDetailLibrary` stores stable entry IDs and one generated linear RGBA32 mipmapped `Texture2DArray`; source-texture references are editor-only, and the array is rebuilt only when missing/stale or explicitly requested.
- `GroundSurfaceLayerProfile` is now a Ground adapter. It resolves generic material identity when assigned, preserves every legacy serialized appearance field as a null-reference fallback, and continues to own only Ground cover compatibility.
- The Ground shader samples one packed detail slice for an active detailed Bank and one for an active detailed Riverbed, decodes RG slope, B cavity, and A form/finish, suppresses legacy square-cell contribution per material, perturbs the existing lighting normal, and preserves the accepted normalized substrate, cover, and hydrology order.
- `SSMP_FineGravel`, `SSDL_DefaultSurfaceDetails`, and `T_SurfaceDetail_FineGravel.png` provide the first dense stylized pebble material. Only `GSLP_FineGravel` is migrated; the other five `GSLP_*` assets remain untouched.
- The reusable material Inspector has cached horizontal/vertical previews and missing/stale library feedback. The detail-library Inspector validates IDs/import settings and rebuilds its array sub-asset through a delayed editor-only repair path.

### Reviewed evidence

- `Game/Procedural/Ground/GroundSurfaceLayerProfile.cs` stores palette, macro/pixel contrast, dry finish, and Ground cover retention in one Ground-specific asset. It has no structural-detail, cavity, or pseudo-normal source.
- `Game/Rendering/PixelSurface/Includes/PixelCellVariation.hlsl::PixelCellVariation_float` floors world position to cells and quantizes hashed values. This directly produces the current block/pixel read and cannot encode pebble boundaries.
- `Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl::ResolvePixelGroundSurfaceColor` applies the Bank and Riverbed profiles only through palette interpolation using broad, pixel, and vertex variation. `BuildSurfaceData` writes a flat tangent normal, so secondary materials have no authored local form.
- `Game/Procedural/Ground/GeneratedGround.cs::ApplySurfaceProfileMaterialProperties` is the accepted shared property-block transport used by ordinary Ground and `StylizedRiver` corridor renderers. It already resolves Bank/Riverbed profiles without changing geometry.
- `Game/Procedural/Rivers/StylizedRiver.cs` calls that same property transport with `GroundSurfaceRenderRole.RiverCorridor`; River is a placement consumer, not the owner of material identity.
- Six `GSLP_*` assets exist. Only `GSLP_FineGravel.asset` is authorized for migration in this update. All other assets must preserve current serialized values and output.
- Git metadata is absent from the supplied `Assets(70).zip`; branch, `HEAD`, history, and working-tree state are unavailable. Baseline file hashes were captured before the first documentation edit. No clone or destructive Git operation is permitted.

### Architecture

```text
StylizedSurfaceDetailLibrary
└── stable entry ID → source packed texture → generated Texture2DArray slice

StylizedSurfaceMaterialProfile
├── reusable palette and cavity identity
├── broad/detail value response
├── natural world scale
├── packed-detail normal/cavity/form controls
└── dry finish

GroundSurfaceLayerProfile
├── optional StylizedSurfaceMaterialProfile reference
├── legacy serialized appearance fallback
└── Ground-only vegetation/snow/frost/Painted Accent retention

Consumer projection and placement
├── Ground/River corridor: world XZ, existing Bank/Riverbed weights
├── future road/path: road mask plus world XZ or mesh UV
└── future wall/cliff: dominant-axis or mesh UV adapter

Environmental modifiers
└── hydrology, snow, frost, wear remain independent from dry material identity
```

The packed detail contract is:

```text
R/G = signed local form slope encoded in 0–1
B   = cavity / inter-element separation, 0 surface to 1 deepest gap
A   = authored form/value/finish variation
```

The library array is linear, repeat-wrapped, mipmapped, and 256×256 per slice for the initial implementation. The generated array is an editor-created sub-asset of the library; no runtime build, CPU upload, or per-frame scan is allowed.

### Invariants and non-goals

- Do not change River corridor geometry, `TexCoord3`, renderer authorization, Bank/Riverbed masks, hydrology, waterline highlight, or normalized substrate weights.
- Do not add a material-name enum or hardcoded Gravel/Mud/Sand/Rock branch.
- Do not add parallax, tessellation, runtime-generated stones, procedural Voronoi in the fragment shader, extra renderers, decals, or draw calls.
- Wetness remains in hydrology modifier profiles. Ground cover retention remains in `GroundSurfaceLayerProfile`.
- Existing `GroundSurfaceLayerProfile` serialized fields remain as a null-reference fallback. No destructive migration or asset-wide rewrite.
- Only Fine Gravel is assigned a generic material in GSU-M1.4.
- No scene, prefab, material, layer, tag, or renderer component edit.
- `GeneratedGroundEditor.cs` CRLF line endings must be preserved.


## GSU-M1.7 — Fine Gravel High-Definition Material and River Authoring Pass

**Status:** Inspector/transport architecture retained; the GSU-M1.7 512² Fine Gravel payload is visually rejected, GSU-M1.8 retains only the 256 runtime-tier correction, and GSU-M1.9A.3 owns the current temporary Fine Gravel A/B payload. GSU-M1.7.1 corrects the editor-only `EntityId` compile blocker. The shared-material and per-application authoring controls remain current; Fine Gravel remains explicitly unfrozen pending GSU-M1.9A.3 Unity validation.

> **Superseded payload record:** the 512-specific content, performance, and validation text in this M1.7 section is retained only as historical implementation evidence. Do not execute its 512 array gate. The current actionable payload and validation contract is GSU-M1.9A.3 at the 256² tier restored by M1.8; only the shared/application authoring and cavity-evaluator changes from M1.7 remain current.

### Objective and acceptance criteria

Replace the visually rejected GSU-M1.6 payload with a genuinely higher-definition stylized gravel material while preserving the reusable generic-material architecture. The result must provide substantially smaller and more varied stones than GSU-M1.6, irregular packing rather than a continuous uniform cellular network, narrow deep cavity cores with softer contact-shadow shoulders, readable local faces and chipped/faceted variation, stable definition at the production camera, and no dominant legacy square-cell response. The actual River/GeneratedGround authoring façade must expose the reusable material definition inline and provide neutral per-application Bank and Riverbed detail multipliers. Fine Gravel is accepted only after the user explicitly approves Unity corridor evidence; editor previews are not an acceptance gate.

### Reviewed evidence

- Unity screenshots supplied after GSU-M1.6 show the new payload at the River bank and bed. Relative to the prior material, the result has larger continuous cells, thick uniform seams, smooth inflated interiors, and lower apparent definition. This is direct visual rejection evidence.
- `Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png` is 256² and visually encodes a regular nearest-cell network. It contains no independent packed stones or multi-scale infill population.
- `Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset` fixes the generated array slice to 256². A true 512² payload therefore requires this asset and the texture importer metadata to change together.
- `Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl::PS3D_DecodeStylizedSurfaceDetail` currently resolves one thresholded cavity value, and `PS3D_ResolveStylizedSurfacePalette` lerps directly to the cavity colour. This makes the authored cavity channel behave as one uniformly strong outline instead of a soft contact shadow plus a narrower deep core.
- `Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs::DrawSurfaceLayerProfileEditor` exposes only Select/Ping buttons for a resolved `StylizedSurfaceMaterialProfile`. The shared material cannot be edited inline from the active River Bank/Riverbed authoring context.
- `Game/Procedural/Ground/GroundMaterialControls.cs` owns River Bank/Riverbed application data but has no neutral material-detail multipliers. `GeneratedGround.ApplySurfaceLayerDetailProperties` transports only shared profile values.
- `Game/Procedural/Rivers/StylizedRiver.cs::EnsureCorridorOutput` remains a consumer of `GeneratedGround.ApplySurfaceProfileMaterialProperties`; no River geometry or shader-mask change is required.
- Git metadata is absent. The comparison baseline is the reconstructed post-GSU-M1.6 source plus the accepted GSU-M1.3.1 null guard. No clone or destructive Git operation is permitted.

### Approved files

**Canonical documentation — modify**

- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md`

**Runtime/editor contracts — modify**

- `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`

**Fine Gravel content — modify**

- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel.asset`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png.meta`

No ShaderLab property, CBUFFER declaration, River source, River geometry, UV3 varying, scene, prefab, material asset, debug view, unrelated `GSLP_*`, or other surface-material asset is approved.

### Invariants and non-goals

- Reusable identity remains owned by `StylizedSurfaceMaterialProfile`; River/Ground application controls are neutral multipliers and do not duplicate palette or texture ownership.
- Bank and Riverbed application multipliers default to `1`, preserve shared-material output, serialize in both local controls and shared style variants, and are copied by `GroundMaterialControls.CopyFrom`.
- Riverbed inheriting the Bank layer may still use distinct Riverbed application multipliers.
- Wetness, cover compatibility, River masks, normalized substrate weights, waterline highlighting, and inward Riverbed wetness remain unchanged.
- Detail remains one packed texture-array sample per active detailed substrate; no parallax, tessellation, triplanar sampling, generated geometry, material-name branch, or per-frame CPU work is added.
- The 512² library change affects only the current single Fine Gravel slice. New materials remain content-driven and share the same library contract.
- The hidden material Preview pane is optional diagnostic assistance. Production-camera Unity rendering is authoritative.

### File-by-file implementation sequence

1. Add Bank and Riverbed detail-application multiplier fields, clamped accessors, null-reset defaults, and copy propagation in `GroundMaterialControls.cs`.
2. Pass the resolved application multipliers into `GeneratedGround.ApplySurfaceLayerDetailProperties` and multiply scale, normal, cavity, value/form, finish, and legacy-cell transport without adding shader properties.
3. Extend `GeneratedGroundEditor` local and shared Bank/Riverbed sections with a clearly labeled `This River Application` group and draw the selected reusable material inline under `Shared Material Definition`, including a warning that shared edits affect every consumer.
4. Update `StylizedSurfaceMaterialProfileEditor` so preview availability and orientation are clearly described as optional diagnostic support rather than a validation gate.
5. Refine the generic one-sample evaluator so the packed cavity channel produces a restrained soft contact-shadow shoulder plus a narrower deep cavity core, while preserving the existing packed contract and normal/smoothness consumers.
6. Replace Fine Gravel with a seamless 512² multi-population irregular-stone payload and retune only `SSMP_FineGravel`; raise the default detail-library slice and importer limits to 512.
7. Update all five relevant documents, remove stale GSU-M1.6 acceptance/preview language, and mark GSU-M1.6 visually rejected and superseded by M1.7.
8. Perform final scope, caller/consumer, serialized-default, parser/compiler, HLSL, texture-contract, GUID/reference, line-ending, and archive audits.

### Performance model

- Runtime CPU: no per-frame work; additional serialized multipliers are read only during existing material-property refresh.
- Dirty/editor CPU: the single Fine Gravel array slice rebuild increases editor-time pixel copy and mip generation from 256² to 512², approximately four times the pixels for that slice; this is event-triggered and accepted.
- GPU sampling: unchanged at one packed sample per active detailed Bank and one per active detailed Riverbed. The evaluator adds bounded scalar shaping only and no sample.
- GPU memory: the current uncompressed RGBA32 Fine Gravel slice with mips increases from approximately 0.33 MiB at 256² to approximately 1.33 MiB at 512². The library currently contains one slice. Future library memory must be reassessed before broad expansion.
- Storage: the source PNG grows; storage is lower priority than active runtime cost.
- **Performance exception:** none. Sample count, draw calls, renderers, mesh channels, and per-frame CPU work remain unchanged.

### Risks and mitigations

- **Serialized-default drift:** new multipliers must initialize and reset to one; static YAML defaults and `CopyFrom` are audited.
- **Shared material surprise:** inline editing must display an explicit all-consumers warning and trigger the existing `EditorProfileChanged` refresh path.
- **Inherited Riverbed ambiguity:** the Inspector must label Riverbed multipliers as Riverbed application values even when substrate identity inherits Bank.
- **Mip loss or shimmer:** the new payload uses 512² mipmaps and authoring that preserves larger structural faces while adding small infill; Unity production-camera review remains mandatory.
- **Overdark cavities:** the evaluator separates shoulder and core and Fine Gravel retunes cavity colour/strength; the user validates dry and wet views.
- **Scope drift:** no River source or shader-property contract change is allowed. Any such requirement stops implementation and updates this plan.

### Required validation

- Parse every changed C# file with an available real parser/compiler; scan namespace/imports and malformed multiline strings.
- Compile or parse the changed HLSL include through the available HLSL harness and audit all unchanged callers.
- Confirm `GroundMaterialControls.CopyFrom(null)` and `CopyFrom(source)` handle every new field and default to neutral values.
- Confirm local and shared style serialized-property paths resolve for all new controls.
- Confirm final texture/library/importer dimensions are 512², linear, readable, Repeat, mipmapped, GUID-stable, and reference-stable.
- Confirm no River file, ShaderLab property, scene, prefab, unrelated asset, or debug enum changes.
- Unity gate: River visible; array Current; no null exception; Fine Gravel quality/scale/cavity at production camera; Bank and Riverbed inline controls operate; no new draw or GC; GPU comparison captured if available.


### Implemented result and post-change compliance audit

- `GroundMaterialControls` now stores six Bank and six Riverbed application multipliers. All fields initialize and reset to `1`, expose clamped accessors, copy through `CopyFrom`, and are available in both local override and shared style-variant serialization paths.
- `GeneratedGround.ApplySurfaceLayerDetailProperties` applies those values during the existing property refresh: scale modifies the profile's world repeat, normal/cavity/value/form/finish strengths are multiplied, and retained legacy-cell influence is multiplied and saturated. No shader property, sample, renderer, draw, mesh, geometry, or per-frame CPU path is added.
- `GeneratedGroundEditor` preserves the two existing River groups. The selected layer's reusable material is editable inside `Shared Material Definition` with an all-consumers warning. `This River Application` exposes only neutral multipliers. Inherited Riverbed materials keep Riverbed-specific multipliers while shared identity remains editable from Bank.
- `PixelSurfaceMaterialDetail.hlsl` preserves the RG/B/A packed contract and one-sample path. B now resolves a broad contact-shadow shoulder and a narrower deep cavity core. Existing normal and smoothness consumers continue using the broad cavity signal.
- Fine Gravel now uses one 512² packed source and a 512² single-slice library. The profile disables legacy cell response, reduces broad macro dominance, and recalibrates normal, cavity, authored form, and dry finish.
- The optional asset Preview pane is now documented as diagnostic assistance only. Unity scene rendering from the production camera is authoritative.
- Final path comparison finds exactly the fourteen approved files. All River source files, `PixelSurfaceGroundMaterialProperties.hlsl`, `PixelSurfaceGroundForwardPass.hlsl`, and `SH_PixelGroundSurfaceLit.shader` are byte-identical to the reconstructed baseline.
- Four changed C# files pass Tree-sitter parsing with zero syntax/missing-node errors; introduced field/property/import references and malformed multiline strings pass static scans. Unity compilation is unavailable here and remains pending.
- The changed HLSL include compiles through a Clang 17 HLSL compute harness and emits LLVM. `dxv` is unavailable, so signed DXIL/Unity variant validation is not claimed.
- Texture/import/library validation passes: 512² RGBA, linear, readable, Repeat, mipmapped, 512 importer limits, stable `fine-gravel` entry, stable asset references, and zero duplicate GUIDs across 321 metadata files. Periodic boundary deltas are at most 1.22 times ordinary adjacent-pixel deltas.
- `GeneratedGroundEditor.cs` retains CRLF line endings. No scene, prefab, material, River source, debug enum, unrelated `GSLP_*`, or unrelated surface-material asset changed.

**Historical gate retired:** do not rebuild or validate the default array at 512². GSU-M1.8 owns only the 256² tier restoration; GSU-M1.9A.3 owns the current temporary Fine Gravel visual and profiling gate. Retain only the M1.7 shared/application-control validation.


## GSU-M1.7.1 — EntityId Foldout-Key Compile Repair

**Status:** Implemented and source-audited on 2026-07-18. Unity 6000.5.0f1 recompilation remains pending.

### Objective and acceptance criteria

Remove the Unity 6000.5 compile blocker introduced by the GSU-M1.7 inline shared-material foldout cache without changing Inspector behavior, serialized data, material transport, rendering, River behavior, or content. The fix is accepted when `GeneratedGroundEditor.cs` no longer performs an implicit `EntityId`-to-`int` conversion, every foldout lookup remains keyed by the selected material profile's current entity identity, all changed files pass static validation, and Unity recompiles without `CS0619` at the former line 5166.

### Reviewed evidence

- Unity reports `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs(5166,30): error CS0619: 'EntityId.implicit operator int(EntityId)' is obsolete`.
- The GSU-M1.7 source declares `SharedSurfaceMaterialFoldouts` as `Dictionary<int, bool>` and assigns `int foldoutKey = profile.GetEntityId();`. That assignment requires the obsolete implicit conversion named by the compiler.
- The project already uses `EntityId` directly as a dictionary key in River runtime code, including `StylizedRiverDisturbanceRuntime.Members.cs`; therefore `EntityId` is the established project key type for transient object identity.
- `GeneratedGroundEditor.cs` already imports `UnityEngine`, so no namespace or assembly dependency is required.
- Git metadata is absent from the supplied workspace. The comparison baseline is the delivered GSU-M1.7 package.

### Approved files

- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`

No runtime Ground file, shader, River source, serialized asset, texture, scene, prefab, material, or other editor file is approved.

### Implementation sequence

1. Change `SharedSurfaceMaterialFoldouts` from `Dictionary<int, bool>` to `Dictionary<EntityId, bool>`.
2. Change the local `foldoutKey` from `int` to `EntityId` while retaining `profile.GetEntityId()` as the source.
3. Scan the complete GSU-M1.7 changed C# set for any other newly introduced implicit `EntityId` conversions.
4. Preserve `GeneratedGroundEditor.cs` CRLF line endings and update the five canonical status records so the failed compile state is not left stale.
5. Run parser, reference, exact-scope, line-ending, and archive validation; leave Unity compilation explicitly pending.

### Invariants and performance

- Foldout expansion state remains editor-session-only and keyed by material-profile identity.
- No serialized value, rendering property, texture binding, Ground regeneration path, River corridor path, or runtime code changes.
- Runtime CPU/GPU, dirty-time generation, memory, draw calls, and storage are unchanged except for negligible documentation text.
- No performance exception.

### Implemented result and static validation

- The foldout cache and local key now use `EntityId` directly; no conversion to `int` remains.
- The complete GSU-M1.7 changed C# set was scanned for `GetEntityId()` assignments to `int`; no other newly introduced occurrence remains.
- `GeneratedGroundEditor.cs` passes Tree-sitter parsing, malformed multiline-string scanning, and CRLF preservation checks.
- Final patch scope is exactly the six approved files. No runtime, shader, River, serialized asset, texture, scene, prefab, or material file changed.

**Pending Unity gate:** recompile in Unity 6000.5.0f1 and confirm the `CS0619` error is gone before continuing visual validation.


## GSU-M1.9A — Fine Gravel Height-First Texture Reauthor — visually rejected; historical

**Status:** Implemented and source-audited on 2026-07-18. Unity array rebuild, production-camera acceptance, and profiling remain pending. Fine Gravel remains unfrozen.

### Objective

Replace the GSU-M1.8 Fine Gravel packed texture without changing the generic material schema or evaluator. The replacement must create convincing pseudo-volume from a continuous raised-stone height field rather than from rim-only slope rings. Fine Gravel remains a dense rounded-pebble material at the standard 256² runtime tier; it is not converted into the larger `Rounded River Rock` material.

### Reviewed evidence

- User Unity evidence shows GSU-M1.8 stones as separated but comparatively flat discs. The perceived depth comes primarily from dark gaps, while broad stone interiors carry little turning form.
- `T_SurfaceDetail_FineGravel.png` shows RG slope concentrated around boundaries and mostly neutral stone centres. This matches the observed rim-embossed result.
- `generate_fine_gravel_m18_final.py` built each stone from a high constant shoulder term (`0.68 + 0.28 * rounded`) and added random directional tone/face terms into A. The broad constant shoulder explains the flat interiors; the random directional A contribution explains highlights that do not consistently follow scene lighting.
- `PixelSurfaceMaterialDetail.hlsl::PS3D_DecodeStylizedSurfaceDetail` already interprets RG as signed slope, B as cavity shoulder/core, and A as palette/finish variation. The texture can therefore correct height and lighting coherence without a shader or profile change.

### Approved affected files

**Canonical documentation — modify**

- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md`

**Fine Gravel payload — modify**

- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png`

No code, shader, profile, library, importer, River source, scene, prefab, material, or other surface asset is in scope.

### Height-first construction contract

1. Build a seamless 1024² authoring height field from independently placed rounded ellipsoidal/superelliptic caps, then downsample height to the final 256² runtime field.
2. Use three deterministic size populations and restrained silhouette wobble; avoid polygonal cells, acute protrusions, uniform equal-width borders, and excessive tiny bubble fillers.
3. Give every stone a continuous raised body: full centre height, meaningful centre-to-edge falloff, rounded shoulder, and recessed surrounding gap. Do not begin from a near-constant interior plateau.
4. Derive RG signed slope from the final 256² height field after downsampling. The normal signal must cross broad stone interiors and preserve a common scene-light response; random baked highlight directions are prohibited.
5. Encode B as a soft contact shoulder plus a narrower deep gap core with variable width. Interior centres must remain below the current cavity bias.
6. Encode A as non-directional per-stone value identity with restrained height-correlated variation only. It must not encode a random light direction or competing per-stone highlight.
7. Preserve the existing 256² RGBA, linear, readable, Repeat, mipmapped, bilinear importer contract, source GUID, library stable ID, and one-sample runtime path.

### Implemented result

- Reused the accepted dense 682-stone multi-scale packing from GSU-M1.8—30 large anchors, 132 medium stones, and 520 small infill stones—so this correction isolates height/form rather than reopening placement and scale distribution.
- Replaced the near-constant shoulder profile with continuous sphere/broad-cap height. Each stone now rises from its contact edge through a full rounded body; height is authored at 1024² and downsampled before slope derivation.
- Derived RG from the final 256² height field and increased body-to-rim slope balance. Interior/rim mean encoded slope ratio rises from approximately `0.272` in GSU-M1.8 to `0.318` while retaining strong edge turn.
- Retained B cavity shoulder/core semantics and similar total coverage. New mask coverage is approximately `82.4%`; cavity mean remains approximately `0.205`, close to the prior `0.211`.
- Removed random directional face/tilt terms from A. Global A correlation with encoded X/Y slope falls from approximately `-0.036/+0.023` to approximately `-0.006/+0.003`, supporting scene-light ownership of the common bright side.
- Preserved 256² RGBA, source GUID, importer, library, profile, stable entry ID, sample count, and all runtime/editor code.

### Post-implementation source audit

- Final diff against the reconstructed GSU-M1.8 baseline contains exactly the five canonical documents plus `T_SurfaceDetail_FineGravel.png`.
- No C#, HLSL, ShaderLab, profile, library, importer, River source, scene, prefab, other surface asset, or metadata file differs.
- PNG validation passes: 256×256 RGBA, valid byte ranges, periodic first/last-edge deltas below ordinary adjacent-pixel deltas on both axes, and unchanged importer/GUID contract.
- Texture-only runtime cost is unchanged. Unity compilation is not required by the changed files, but Unity import/array rebuild and scene visual validation remain mandatory.

### Acceptance criteria

- Packed-map inspection shows meaningful RG gradients across stone bodies, not only rainbow rings at the perimeter.
- Height-only preview shows raised rounded pebbles with clear mass and recessed gaps before material response is applied.
- A-channel inspection contains no random directional highlight lobe.
- Downsampled 128² evidence retains large/medium/small separation and stone body without collapsing into dark outlines.
- Unity production-camera evidence shows a coherent common light-facing side, stronger rounded volume, no sharp protruding aggregate, and no regression to cellular/cobblestone packing.
- Fine Gravel remains unfrozen until explicit user acceptance. Material-response changes, if still required, must be a separately planned GSU-M1.9B update after the texture is accepted.

### Performance

Runtime sample count, shader ALU, draw calls, renderer count, mesh data, and CPU work are unchanged because only packed texel values change. The shipped source remains 256², so array memory remains approximately 0.333 MiB per uncompressed RGBA32 slice including mips. The 1024² construction field is an offline authoring intermediate and is not packaged.

## GSU-M1.8 — Rounded-Pebble Source Reauthor and 256 Runtime Restoration

**Status:** Implemented and source-audited on 2026-07-18, then visually rejected by user evidence. Its 256² runtime-tier correction remains accepted, but its packed texture is superseded by GSU-M1.9A because it reads as shallow rim embossing rather than raised rounded pebbles.

### Objective and acceptance criteria

Replace the GSU-M1.7 packed Fine Gravel payload rather than attempting another parameter-only correction. The new material must read as densely packed rounded pebbles with broad faces, soft chipped edges, three visible size populations, varied aspect/orientation/value, narrow non-uniform recessed gaps, and restrained internal planar/rounded form. It must not read as Voronoi cells, cracked mud, reptile skin, sharp aggregate, outlined cobblestone, or inflated uniform domes. The authoritative target is the actual isometric gameplay camera; asset previews and offline renders are supporting evidence only.

Restore the standard runtime detail library to 256×256. The texture may be generated internally at higher authoring resolution and downsampled before packaging, but the shipped source referenced by the current editor builder and the generated runtime array slice must remain 256×256. The one-packed-sample generic contract remains RG signed slope, B cavity/contact separation, and A authored form/value/finish variation.

### Completed read-only review and evidence

- The complete current `StylizedSurfaceDetailLibrary`, builder, reusable material profile, Ground adapter resolution, Ground property transport, and shared HLSL detail evaluator were reviewed. `StylizedSurfaceDetailLibraryBuilder.Validate` requires each source texture to match `SliceResolution`, and `NormalizeSourceImporters` clamps the importer to that resolution; therefore a 512 source plus 256 runtime array would require code changes and is not justified for this correction.
- `SSDL_DefaultSurfaceDetails.asset` currently sets `sliceResolution: 512`, and `T_SurfaceDetail_FineGravel.png.meta` clamps both default and Standalone imports to 512. Under the current uncompressed RGBA32 full-mip contract, one generated slice is approximately 1.333 MiB at 512 and 0.333 MiB at 256. Shader sample count and ALU are independent of this resolution.
- The current packed PNG directly shows a dense continuous cell network with many acute polygon corners and nearly uniform line ownership. Unity screenshots show the same structure as blocky protruding stones with excessive outline dominance. This is direct evidence that payload shape language, not only tuning or resolution, is the active defect.
- `PixelSurfaceMaterialDetail.hlsl` already separates a broad cavity shoulder and narrow cavity core and consumes authored slope/value from one sample. No shader change is justified until a substantially improved rounded-pebble payload is tested through the existing evaluator.
- `SSMP_FineGravel.asset` already disables legacy pixel-cell response and exposes shared and per-River application controls. The existing architecture and GSU-M1.7.1 Inspector compile correction are preserved.
- Git metadata is absent. The comparison baseline is the reconstructed post-GSU-M1.7 source with GSU-M1.7.1 overlaid. No clone or destructive Git operation is permitted.

### Approved exact affected files

**Canonical documentation — modify**

- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md`

**Fine Gravel content/runtime-library data — modify**

- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel.asset`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png.meta`

No C#, HLSL, ShaderLab, River source, scene, prefab, material, Ground layer adapter, unrelated profile, generated array sub-asset, debug view, folder, component, layer, or tag is authorized. If the existing one-sample evaluator proves incapable only after Unity evidence, that is a separate correction requiring a plan and explicit scope update.

### File-by-file implementation sequence

1. Record this persistent plan before any payload or serialized-asset edit.
2. Generate multiple seamless high-resolution authoring candidates offline using independently placed rounded superellipse/oval stones rather than polygonal region partitioning. Use large, medium, and small populations, conservative protrusion limits, varied near-contact gaps, and per-stone broad-top/side-plane form.
3. Compare candidates through a local decoder approximating the existing palette/cavity/normal evaluator. Reject candidates with acute-corner excess, single-scale packing, continuous equal-width seams, large empty pockets, visible periodic edge discontinuity, or weak mip survival.
4. Downsample the selected authored field to one 256×256 linear RGBA packed runtime source. Derive RG slopes from the final 256 height field, not from the higher-resolution field, so the shipped normal signal remains coherent after downsampling.
5. Restore `SSDL_DefaultSurfaceDetails.asset` to `sliceResolution: 256` and restore the Fine Gravel importer maximum sizes to 256 while preserving GUID, linear sampling, readability, Repeat wrap, mipmaps, bilinear filtering, and stable entry ID.
6. Retune only `SSMP_FineGravel.asset` for the selected payload. Preserve detail enablement, library reference, entry ID, zero legacy-cell influence, low dry gloss, and generic material ownership.
7. Update the four remaining canonical documents. Mark GSU-M1.7's 512 payload visually rejected/superseded, record 256 as the standard runtime tier, remove stale 512 rebuild instructions, and preserve GSU-M1.7 River-facing controls plus GSU-M1.7.1 compile correction.
8. Complete a post-implementation consistency/compliance audit and package only the nine approved files.

### Invariants and non-goals

- Preserve V3S-A4B.3 River renderer authorization, UV3 semantics, normalized substrate composition, hydrology, highlight, and geometry exactly.
- Preserve reusable material ownership. River/GeneratedGround remains a consumer with neutral per-application multipliers; no material definition moves into River code.
- Preserve one texture-array sample per active detailed substrate and at most two only where independently detailed Bank and Riverbed paths overlap.
- Do not add triplanar sampling, parallax, tessellation, generated stones, runtime texture construction, runtime CPU work, material-name branches, another packed channel, or another texture sample.
- Do not treat the reference as a texture to copy. Match its rounded, broad-faced, varied, recessed-gap shape language while retaining the project's own muted stylized palette and Fine Gravel scale.
- Do not freeze Fine Gravel without explicit Unity production-camera acceptance.

### Expected performance impact

- Active shader instruction count and sample count remain unchanged because no shader code or material enable path changes.
- Generated-array storage falls from approximately 1.333 MiB to 0.333 MiB per uncompressed RGBA32 full-mip slice, a 1 MiB reduction per slice and 75% reduction in slice memory.
- Source texture import/upload data also returns to the 256 class. Exact loaded duplication between readable source and generated array remains Unity Memory Profiler work, but every such copy is one quarter of the 512 pixel count.
- Editor array rebuild texel work falls by approximately 75% per slice.
- Different authored frequency can change cache/mip behavior, but the lower runtime resolution and unchanged sample count make a regression unlikely. This remains **Unverified, Medium confidence** until Unity GPU comparison.
- No performance exception.

### Required validation and pass criteria

- Packed source is exactly 256×256 RGBA, linear, readable, Repeat-wrapped, bilinear, mipmapped, and retains GUID `694620f5ff6c4b9589f5d6d4be929632`.
- Library remains one `fine-gravel` entry and resolves a 256×256 generated array after Unity rebuild.
- Periodic edge comparisons show no exceptional seam; RG encodes coherent finite slopes; B has distinct shoulder/core distribution rather than a binary thick outline; A contains bounded per-stone and internal-form variation.
- Final diff contains exactly the nine approved files, with all C#, HLSL, ShaderLab, River, scene, prefab, other material, and other layer files byte-identical to baseline.
- Unity compiles after the existing GSU-M1.7.1 correction, River remains visible, array status reaches Current at 256, and Fine Gravel reads as rounded varied pebbles in dry/wet Bank and Riverbed views.
- Fine Gravel remains pending until explicit user visual acceptance.

### Implemented result

- Reauthored the packed Fine Gravel source from scratch using 682 independently placed rounded superellipse/oval stones rather than polygonal region partitioning: 30 large anchors, 132 medium stones, and 520 small infill stones. Stone aspect, orientation, roundness, edge wobble, broad-top tilt, secondary face plane, and signed value vary deterministically.
- Generated the field internally at 1024², then downsampled coverage, height, and value to the shipped 256² source. RG slopes were derived from the final 256 height field so the packed normal signal matches runtime resolution instead of preserving aliased high-resolution gradients.
- Restored `SSDL_DefaultSurfaceDetails.sliceResolution` and all Fine Gravel importer maximum sizes from 512 to 256. Stable ID `fine-gravel`, source GUID `694620f5ff6c4b9589f5d6d4be929632`, linear/readable/Repeat/mipmap/bilinear import contract, and generated-array rebuild ownership remain unchanged.
- Retuned only `SSMP_FineGravel`: zero legacy cell influence is preserved; macro contrast is reduced; authored form/value and cavity response are strengthened; normal response is restrained; natural repeat remains approximately Fine Gravel scale; dry finish remains low-gloss.
- No C#, HLSL, ShaderLab, River, scene, prefab, other material, other layer, renderer, geometry, mask, hydrology, debug view, folder, component, layer, or tag changed.

### Static validation and post-implementation audit

- Final packed source is 256×256 RGBA. Channel ranges are finite and bounded. B classifies approximately 14.5% of texels as gap-side values above `0.54`, approximately 5.35% as deep-core values above `0.8`, and approximately 85.5% below the profile cavity threshold, avoiding a continuous thick-outline field.
- Periodic first/last-edge differences are within 0.94–1.14 times ordinary adjacent-pixel differences per channel, so no exceptional tile seam is present. The source keeps broad shoulder/core cavity separation and bounded A-channel form variation.
- Final runtime array memory returns from approximately 1.333 MiB to 0.333 MiB per uncompressed RGBA32 full-mip slice. Sample count, ALU, draw calls, renderer count, runtime CPU work, and material property transport are unchanged.
- Actual affected files match the approved scope exactly: five canonical documents plus `SSMP_FineGravel.asset`, `SSDL_DefaultSurfaceDetails.asset`, the packed PNG, and its `.meta` file.
- All C#, HLSL, ShaderLab, River source, scene, prefab, other profile, and other layer files are byte-identical to the reconstructed GSU-M1.7.1 baseline.
- The offline decoded preview demonstrates rounded, multi-scale pebble packing and variable recessed gaps, but it is supporting evidence only. Unity production-camera evidence remains the acceptance gate.

## GSU-M1.6 — Fine Gravel Visual Refinement and Acceptance

**Status:** Visually rejected by Unity production-scene evidence and superseded by GSU-M1.7. The provisional `GSU-M1.5` working label remains retired because `GSU-M1.5` already names the original foundation audit/validation step. Preserve this section only as historical evidence; do not treat its 256² payload, content-only limitation, or preview guidance as current acceptance criteria.

### Objective and acceptance criteria

Refine the first reusable material so Fine Gravel reads as a dense illustrated pebble field rather than quantized Ground or a uniform cellular shell. The production-camera result must show distinct small stones, irregular size and aspect distribution, narrow dark inter-stone separation, restrained per-stone value variation, and stable pseudo-normal lighting without photoreal noise, visible square-cell influence, excessive gloss, or obvious tile repetition. Close preview evidence supports authoring, but the gameplay camera is authoritative.

### Reviewed evidence and diagnosis

- `SSMP_FineGravel.asset` already reduces legacy pixel-cell influence to `0.06` and enables packed structural detail; therefore the remaining target is not solved by increasing the legacy Ground pixel controls.
- `T_SurfaceDetail_FineGravel.png` currently encodes a regular single-scale cellular field. The decoded material preview shows clear cavity and pseudo-normal response, proving that the shared evaluator can represent stone form, but the cells are comparatively uniform, smoothly inflated, and large for Fine Gravel. This is a content-authoring deficiency rather than evidence that the generic shader contract needs another channel or sample.
- `PixelSurfaceMaterialDetail.hlsl` already exposes slope, cavity, signed form value, and finish variation. `PixelSurfaceGroundForwardPass.hlsl` applies those results to palette, lighting normal, and dry smoothness. No shader change is authorized in this refinement unless new Unity evidence proves a structural evaluator defect.
- `detailWorldScale: 2.2` combined with the existing cell count produces coarse stone scale. The refinement will increase element density and reduce the profile's world repeat scale so the result reads as gravel rather than cobbles.
- Git metadata is absent from the supplied source. Comparison is against `Assets(70).zip`, the delivered GSU-M1 package, and GSU-M1.3.1.

### Approved affected files and sequence

1. **Plan first:** update this canonical section before content generation.
2. **Packed detail content:** replace `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png` with a seamless, higher-density, multi-scale irregular pebble field using the unchanged RG/B/A contract. Preserve its GUID and importer metadata.
3. **Material tuning:** modify `Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel.asset` only for Fine Gravel scale, cavity, normal, value, palette, and dry-finish calibration.
4. **Architecture/status documentation:** update `Ground_Visual_Design_and_Architecture.md`, `Ground_River_Coupled_Surface_Response_Architecture.md`, `GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`, and `Ground_Contact_Edge_Accent_Audit_and_Architecture.md` to record the refinement, preserve generic ownership, and remove stale statements that the unrefined proof is awaiting its first visual pass.
5. **Post-change audit:** verify exact file scope, texture dimensions/channels/seams/import contract, asset references, unchanged shader/C#/River files, and archive safety. Unity compilation, production-camera visual acceptance, and GPU profiling remain user-side gates.

### Invariants and non-goals

- No C#, HLSL, ShaderLab, River, scene, prefab, material, layer, tag, geometry, mask, UV3, hydrology, renderer, or debug-view change.
- No new texture sample, array slice, profile field, material-name branch, parallax, tessellation, generated geometry, or per-frame CPU process.
- Preserve `SSDL_DefaultSurfaceDetails.asset`, the Fine Gravel stable entry ID, all GUIDs, and the generated-array rebuild workflow.
- Do not modify any other `SSMP_*` or `GSLP_*` material.
- Fine Gravel is not frozen until Unity proves the production-camera target. Further content-only iterations remain permitted under a newly recorded correction step.

### Expected performance impact

The shader path and sample count are unchanged: one packed detail-array sample for each active detailed substrate, and at most two only where detailed Bank and Riverbed materials genuinely overlap. The new texture retains the existing 256×256 RGBA slice and mip/import contract, so runtime memory class, draw calls, CPU work, and shader instruction structure remain unchanged. Different authored frequency may alter texture-cache/mip behavior; this is **Unverified, Low confidence impact** and must be assessed in the same Unity GPU comparison already required for GSU-M1.

### Implemented result

- Replaced the regular inflated-cell source with a seamless 256×256 packed field containing 576 jittered, anisotropic pebble regions. Element density is even enough to avoid distinctive large clumps while size, aspect, orientation, local tilt, and per-stone value vary deterministically.
- Kept the packed contract unchanged: RG remains local slope, B remains seam/cavity, and A remains per-stone form/value/finish variation. The existing texture GUID, library entry ID, importer metadata, and array-rebuild workflow are preserved.
- Retuned `SSMP_FineGravel` from coarse proof values to a smaller-stone material: world repeat scale `2.0 m`, legacy pixel-cell influence `0.02`, lower macro contrast, restrained dry finish, stronger authored value range, and a dark but narrower cavity remap.
- No C#, HLSL, ShaderLab, library asset, Ground adapter, River file, or other surface asset changed.

### Static validation and remaining gate

- Texture is RGBA 256×256, retains the unchanged linear/repeat/mip importer contract, and preserves the original GUID. Channel ranges are valid and periodic edge discontinuity remains within the same class as ordinary one-pixel interior variation.
- The profile continues to reference `fine-gravel` in the unchanged default detail library. No generated array or sub-asset is packaged; Unity remains responsible for deterministic rebuild from the preserved source reference.
- Offline decoding through the same profile-preview equations shows smaller irregular stones, readable lighting form, and dark inter-stone separation. This is supporting evidence only; the production gameplay camera remains authoritative.
- Unity must still confirm River restoration, array rebuild, dry/wet Bank and Riverbed appearance, distance stability, tile visibility, and GPU cost. Fine Gravel is not frozen.

### Post-implementation consistency and compliance audit

- Actual affected files match the approved content-only scope exactly: five canonical Markdown documents, `SSMP_FineGravel.asset`, and `T_SurfaceDetail_FineGravel.png`.
- `GroundSurfaceLayerProfile`, `GeneratedGround`, the generic profiles/library, the detail-library builder, the shared HLSL evaluator, the Ground forward pass, ShaderLab declarations, and all River files are byte-identical to the GSU-M1.3.1 baseline.
- `SSDL_DefaultSurfaceDetails.asset` and the texture `.meta` are byte-identical to baseline; stable ID `fine-gravel`, source GUID `694620f5ff6c4b9589f5d6d4be929632`, profile GUID, and library GUID remain unchanged.
- The refined PNG is 256×256 RGBA and the unchanged importer remains linear, readable, Repeat-wrapped, and mipmapped. No generated `Texture2DArray` sub-asset is shipped in this patch.
- No scene, prefab, material, C#, HLSL, ShaderLab, River source, debug view, folder, component, layer, tag, renderer, or unrelated surface asset changed.
- Documentation now uses `GSU-M1.6` for Fine Gravel refinement; the temporary `GSU-M1.5` reuse is explicitly retired because that identifier already belongs to the foundation audit step.

### Approved expected affected files

**Canonical documentation — modify**

- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md`

**Generic runtime profiles — create**

- `Assets/Game/Rendering/PixelSurface/Profiles.meta`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceMaterialProfile.cs.meta`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs`
- `Assets/Game/Rendering/PixelSurface/Profiles/StylizedSurfaceDetailLibrary.cs.meta`

**Editor authoring/build/preview — create**

- `Assets/Game/Rendering/PixelSurface/Editor.meta`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs.meta`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryEditor.cs.meta`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceMaterialProfileEditor.cs.meta`

**Ground adapter and property transport — modify**

- `Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`

**Shared shader detail core — create/modify**

- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl.meta`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`

**Fine Gravel content — create/modify**

- `Assets/Game/Demo/Profiles/SurfaceMaterials.meta`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures.meta`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset.meta`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel.asset`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSMP_FineGravel.asset.meta`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png`
- `Assets/Game/Demo/Profiles/SurfaceMaterials/Textures/T_SurfaceDetail_FineGravel.png.meta`
- `Assets/Game/Demo/Profiles/Ground/Layers/GSLP_FineGravel.asset`

The editor builder is expected to add or replace one `Texture2DArray` sub-asset inside `SSDL_DefaultSurfaceDetails.asset` on import or explicit rebuild. That deterministic Unity-side rewrite is part of the approved scope.

### File-by-file implementation sequence

1. **GSU-M1.0 Documentation correction:** remove stale River-owned optional profile-detail instructions and record this material-first plan in all five relevant canonical documents.
2. **GSU-M1.1 Runtime profiles:** create the library and generic material profile; add the optional generic reference and legacy fallback to `GroundSurfaceLayerProfile`. No shader output changes at this step.
3. **GSU-M1.2 Editor workflow:** implement entry validation, deterministic array sub-asset rebuild, missing/stale repair scheduling, custom library Inspector, material Inspector, and cached horizontal/vertical preview. Modal asset creation is not added to live Ground IMGUI scopes.
4. **GSU-M1.3 Shader core and Ground adapter:** bind per-Bank and per-Riverbed array/slice/parameter sets, decode one packed sample per active detailed substrate, reduce legacy pixel-cell influence per material, apply cavity and form value to the layer palette, perturb the existing geometric normal, and apply bounded finish variation. No River producer or response-mask edit.
5. **GSU-M1.4 Fine Gravel content:** generate the linear packed Fine Gravel texture, create the default library and material profile, and assign only `GSLP_FineGravel` to `SSMP_FineGravel`.
6. **GSU-M1.5 Audit and validation:** reconcile exact files, reread final callers/consumers/contracts, parse/compile changed C#, run shader harness/static contract checks, verify texture packing/import data, and record Unity validation as pending where unavailable.

### Performance model

- **Runtime CPU:** no new per-frame work. Profile resolution and `MaterialPropertyBlock` writes occur only through existing material refresh paths. Library ID-to-slice lookup is bounded by library entry count at refresh time.
- **Dirty/editor CPU:** array rebuild is O(slice count × pixels × mip levels), editor-only, and runs only when the library is missing/stale or explicitly rebuilt. Preview generation is cached and editor-only.
- **GPU:** no-detail ordinary Ground performs no detail texture fetch through active-substrate uniform branches. A detailed Bank or Riverbed performs one packed array sample. A genuine Bank/Riverbed blend can perform two samples in the transition region. Bounded decode, cavity, normal, and finish ALU are added only to enabled detailed layers. Compiler branch elision is not assumed and must be profiled in Unity.
- **Memory:** one 256² RGBA32 mipmapped array slice is approximately 0.33 MiB. Thirty-two slices are approximately 10.7 MiB before platform-specific driver overhead. Source-texture references are compiled only into the Editor side of the library schema; the generated array is the intended player-side dependency. Player-build dependency and residency must still be verified in Unity.
- **Draw/geometry:** no new material, renderer, draw call, mesh channel, geometry, collider, or generated runtime texture.
- **Performance exception:** one optional packed sample per active detailed substrate is approved because the current palette/noise representation cannot encode stone silhouettes, cavities, or local form. More than two detail samples in the Ground pass is not approved.

### Acceptance criteria

1. Fine Gravel reads as dense stylized stones with dark gaps and lighting-driven pseudo-volume at production camera distance.
2. Square pixel-cell breakup is subordinate on Fine Gravel while legacy non-migrated layers remain unchanged.
3. Adding a later material requires a packed texture, library entry, generic material profile, and optional Ground adapter assignment; it does not require shader code.
4. The generic material profile contains no River placement or Ground cover-retention fields.
5. Fine Gravel works as Bank, Riverbed, and matching Bank/Riverbed; the existing Bank/Riverbed authoring sections expose the shared material definition and neutral per-application multipliers. The optional asset Preview pane is not acceptance evidence.
6. A4B.3 River appearance and ownership behavior remain unchanged except for the selected material's visual detail.
7. No scenes or prefabs change, no new debug view is added, and no additional draw call or runtime CPU process appears.
8. All changed C# files pass an available real parser/compiler and namespace/reference scan. Unity compilation and production-camera GPU profiling remain explicit user-side gates if Unity is unavailable.

### Validation matrix

| Check | Method | Current status |
|---|---|---|
| Approved file scope and baseline | final content comparison against reconstructed post-GSU-M1.7 plus GSU-M1.7.1 source | Passed for GSU-M1.8: exactly nine approved files differ; all C#, HLSL, ShaderLab, River, scene, prefab, other material, and other layer files are byte-identical |
| C# syntax and references | changed-file inventory plus baseline byte comparison | No C# file changes in GSU-M1.8; the GSU-M1.7.1 `EntityId` correction remains the current code baseline; Unity compilation remains pending |
| HLSL contract | changed-file inventory plus baseline byte comparison | No HLSL or ShaderLab changes in GSU-M1.8; the accepted one-sample RG/B/A evaluator and Ground callers are byte-identical to the GSU-M1.7.1 baseline |
| Texture contract | PIL/channel statistics, importer/library YAML audit, reference/GUID audit, periodic-edge comparison | Passed for GSU-M1.8: 256² RGBA, linear, readable, Repeat, mipmapped, three 256 importer limits, one 256 library slice, stable IDs/GUIDs, and first/last-edge deltas remain within 0.94–1.14× ordinary adjacent-pixel deltas |
| Application controls | baseline byte comparison and retained GSU-M1.7 audit | Unchanged in GSU-M1.8; Bank/Riverbed retain the six neutral multipliers and inline shared-material authoring from GSU-M1.7 |
| River invariants | byte comparison of all River files and frozen Ground shader contract files | Passed: every River, Ground shader-contract, and runtime/editor code file is byte-identical to the reconstructed GSU-M1.7.1 baseline |
| Line endings | changed-file inventory | No C# file changed in GSU-M1.8; existing `GeneratedGroundEditor.cs` line endings are untouched |
| Unity compile and array rebuild | Unity 6000.5.0f1 import/compile; `SSDL_DefaultSurfaceDetails` status | Pending user validation; must confirm the GSU-M1.7.1 compile fix, River visibility, no null exception, generated array Current at 256², and retained Inspector paths |
| Visual acceptance | production-camera and close-range dry/wet Bank/Riverbed evidence | Pending user validation; Fine Gravel remains unfrozen and the hidden asset Preview pane is not an acceptance gate |
| Performance | baseline/no-detail/Fine-Gravel/blend GPU captures; draw and GC comparison | Pending user validation; static audit proves no added sample, property, draw, renderer, geometry, or per-frame CPU path |


## GSU-M1.3.1 — Null Detail-Array Transport Guard

**Status:** Implemented and source-audited on 2026-07-17. Unity revalidation pending.

### Failure evidence

Unity reported `ArgumentNullException: Value cannot be null` from `MaterialPropertyBlock.SetTexture` in `GeneratedGround.ApplySurfaceLayerDetailProperties`, reached by both `StylizedRiver.EnsureCorridorOutput` and ordinary `GeneratedGround` refresh/regeneration callbacks. The first GSU-M1 package always executed `properties.SetTexture(textureArrayId, hasDetail ? textureArray : null)`. During script reload/import, `StylizedSurfaceDetailLibrary.GeneratedTextureArray` can legitimately remain null until the editor-only delayed repair pass rebuilds the array sub-asset. Unity rejects the null value before the method can write the remaining property block or assign it to the renderer, so River-corridor setup aborts and the River can disappear.

### Objective and acceptance criteria

- Never call `MaterialPropertyBlock.SetTexture` with a null texture.
- Preserve the editor-delayed detail-array rebuild workflow.
- Preserve zero-detail shader behavior by setting the detail-enable component to zero when resolution fails.
- Preserve any prior texture binding as inert while detail is disabled; a later successful resolution replaces it with the current array.
- Restore completion of ordinary Ground and River-corridor property-block assignment.
- Change no River geometry, UV3 data, renderer authorization, masks, hydrology, shader code, profile assets, scenes, or prefabs.

### Approved files

- `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
- `Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md`
- `Assets/Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`

### Reviewed callers, producers, and consumers

- Producer: `GroundSurfaceLayerProfile.TryResolveDetail` delegates to `StylizedSurfaceMaterialProfile.TryResolveDetail`; resolution returns false until `StylizedSurfaceDetailLibrary.GeneratedTextureArray` exists and the stable entry resolves to a valid slice.
- Editor producer lifecycle: `StylizedSurfaceDetailLibraryBuilder.Initialize` schedules delayed repair, so a transient null array during script reload is valid and must not break runtime/editor renderer setup.
- Shared transport: `GeneratedGround.ApplySurfaceProfileMaterialProperties` uses one `MaterialPropertyBlock` path for ordinary Ground and River-corridor renderers.
- River caller: `StylizedRiver.EnsureCorridorOutput` assigns the Ground material and invokes the shared transport with `GroundSurfaceRenderRole.RiverCorridor` during `OnEnable` and edit-mode `OnValidate`.
- Shader consumer: `ResolveGroundBankLayerDetail` and `ResolveGroundRiverbedLayerDetail` sample only when the corresponding `DetailA.x` enable value exceeds `0.5`; the no-detail vector remains zero.

### Implementation

`ApplySurfaceLayerDetailProperties` now calls `SetTexture` only when `TryResolveDetail` succeeds. When resolution fails, it still writes the zero detail-enable vector and the established fallback parameter vectors. No placeholder texture, allocation, renderer mutation, or editor repair is introduced.

### Performance impact

- Runtime/Editor CPU: one existing boolean branch controls whether the existing texture property write occurs. Failed detail resolution now performs one fewer property-block write.
- GPU: unchanged; the same zero detail-enable vector suppresses detail evaluation.
- Memory, storage, draws, geometry, and allocations: unchanged.

### Validation

| Check | Status |
|---|---|
| Stack-trace-to-source match | Passed: the reported line is the unconditional nullable `SetTexture` call |
| Null call removal | Passed statically: the only new detail-array `SetTexture` calls are guarded by successful resolution |
| No-detail shader gate | Passed statically: `DetailA.x` remains zero when resolution fails |
| River contract diff | Passed statically: no River source, shader, UV3, mask, or hydrology file changed |
| C# parser/compiler | Passed with Tree-sitter C# parsing: zero syntax errors in the changed `GeneratedGround.cs` and the reviewed detail producer, editor builder, and River caller files. Namespace/import and malformed multiline-string scans passed. Unity/Roslyn project compilation remains pending. |
| Unity compile and River restoration | Pending user validation |

### Post-implementation consistency and compliance audit

- Actual affected files match the six approved paths exactly: five canonical Markdown documents and `GeneratedGround.cs`.
- The final code diff contains one behavioral change: the new detail-array texture property is written only after successful non-null detail resolution.
- `GroundSurfaceLayerProfile`, `StylizedSurfaceMaterialProfile`, `StylizedSurfaceDetailLibrary`, the editor builder, `StylizedRiver.EnsureCorridorOutput`, and both Ground detail shader resolvers were reread after the edit; their producer/caller/consumer contracts remain consistent with the guard.
- No River source, shared shader, profile asset, detail texture, scene, prefab, material, meta file, or generated library asset changed.
- Tree-sitter C# parsing reports zero errors for the changed file and all reviewed related C# files. Namespace/import and malformed multiline-string scans pass.
- Unity project compilation and observed River restoration remain the only blockers to acceptance.

---

## V3S-A0/A1 — Canonical architecture and Riverbed Support proof

**Status:** Unity-validated and visually accepted.

The official River-coupled architecture and complete patch sequence are recorded in `Ground_River_Coupled_Surface_Response_Architecture.md`. V3S-A0/A1 corrects the canonical roadmap and consumes the River corridor's UV channel index `3` / HLSL `TEXCOORD3` contract in the Ground shader. `UV3.x` is forwarded as one `half` varying and exposed through debug mode `32`, `Ground Riverbed Support`. The shared `PixelSurfaceGroundResponse.hlsl` resolver returns zero for shader consumers that do not define Ground Riverbed Support, preserving `SH_PixelSurfaceLit` compatibility. This proof changes no normal lit material response.

V3S-A0/A1 validation gate:

1. Unity compiles both Ground and generic Pixel Surface shaders.
2. Ordinary GeneratedGround is zero in the new debug view.
3. Centre, FlatBedEdge, and BedSlope are one.
4. HiddenCover, OuterBlend, and BuriedApron are zero except expected interpolation at the first transition strip.
5. Existing Ground Shore output remains unchanged.
6. Normal lit rendering remains unchanged.

## V3S-A2A — Reusable surface-layer authoring foundation

**Status:** Unity-validated and accepted.

Adds `GroundSurfaceLayerProfile`, Bank and Riverbed profile references in `GroundMaterialControls`, six starter assets, automatic dropdown discovery, inline profile editing, and in-place create/duplicate actions under the main `GeneratedGround` Material Controls. Existing style assets remain unassigned and normal rendering is intentionally unchanged. The complete contract is in `Ground_River_Coupled_Surface_Response_Architecture.md`.


## V3S-A2B — Bank material-composition proof

**Status:** Unity-validated as the material-state and control proof. Its original spatial interpretation was incomplete because ordinary Ground could participate; V3S-A2C.1 restricts all Bank composition to the River corridor bank domain.

Adds the Ground-owned `Bank Material Strength`, `Core Bank Reach`, `Immediate-Bank Exposure`, `Waterline Material Strength`, and `Core Bank Transition Softness` controls directly beneath the main GeneratedGround Surface Layers authoring. The selected Bank profile's base/dark/light palette, macro/pixel contrast, dry smoothness, and dry specular character are carried through the existing `MaterialPropertyBlock` path to the River corridor renderer. Debug modes `33` and `34` prove the scalar Bank material field and selected layer identity. No wetness, cover retreat, or Riverbed rendering is added.

## V3S-A2C / A2C.1 / A2C.2 / A2C.3 / A2C.4 — Expanded Corridor-Owned Bank Composition Range

**Status:** A2C ordinary-Ground ownership rejected by visual validation. A2C.1 corridor-owned bank distance is visually validated. A2C.2 empty-stream cleanup failed visual validation. A2C.3 remains an ordinary-Ground mesh-layout integrity safeguard. On 2026-07-16 the user confirmed that A2C.4 solved Bank material spilling into ordinary Ground. Its explicit renderer authorization and ordinary-Ground River-data cleanup are accepted and frozen as the isolation baseline.

A2C.1 removes the generated Ground UV3 distance stream and extends the existing River corridor UV3 contract instead: X remains Riverbed Support, Y stores outward distance in metres from the support boundary, Z marks the corridor bank domain, and W remains zero. The main GeneratedGround Inspector retains the clear `Core Bank` and `Outer Bank Extension` groups. Core UV2.y weights and optional metre-based extension are both masked by corridor bank validity, so the selected layer starts at the bed edge, crosses `HiddenCover` and `OuterBlend`, and cannot create disconnected patches on ordinary Ground. Debug mode `35`, `Ground Outer Bank Extension`, isolates the optional corridor contribution. No wave analysis, generated texture, texture sample, new renderer, scene, prefab, or collider behavior is added.

A2C.2 attempted to fix the migration defect by assigning an empty UV channel `3`, but visual validation proved that this did not reliably target the mesh attached to the Ground renderer and did not reset the retained vertex layout. A2C.3 supersedes that attempt as a data-integrity safeguard. `GeneratedGround` inspects the actual `MeshFilter.sharedMesh`, treats any attached `TEXCOORD3` as missing/invalid Ground geometry, resets the generated mesh layout with `Mesh.Clear(false)` before normal mesh application, and asserts afterward that ordinary Ground contains no `TEXCOORD3`.

A2C.4 adds two independently justified corrections. Its exact thirteen-file scope, shared-include compatibility, central-resolver coverage, line endings, textual structure, and active-document consistency were audited, and on 2026-07-16 the user confirmed that the live Bank spill into ordinary Ground is solved. First, every Ground-profile property binding requires a mandatory `GroundSurfaceRenderRole`: the ordinary renderer writes `_GroundRiverCoupledEnabled = 0`, and all three River corridor bindings write `1`. The central shared response include gates Shore, Riverbed Support, Bank distance, and Bank domain while preserving generic Pixel Surface Shore behavior through preprocessor-safe fallback. Second, ordinary Ground no longer derives exposure, damp/deposit, vegetation, or `UV2.y` from River proximity; the corridor continues to publish its precise Shore signal and now asserts its four-component `TexCoord3` after writing the stream. Structural River concealment, handoff, regeneration orchestration, and explicit Painted Accent River exclusion remain unchanged. The explicit role is a containment and future-proofing contract, not a proven retrospective diagnosis. Complete evidence, scope, risks, and validation are recorded in `Ground_River_Coupled_Surface_Response_Architecture.md`.

## V3S-A3A — Bank Surface-Cover Retention and Retreat

**Status:** Unity-validated and accepted on 2026-07-16.

A3A adds four zero-default Material-only controls under `River-Coupled Ground Response — Surface-Cover Response`: Vegetation Retreat Strength, Snow Melt Strength, Frost Retreat Strength, and Painted Accent Retreat Strength. The selected Bank profile already stores the four compatibility fractions, and `GeneratedGround` already transports them as `_GroundBankLayerCoverRetention`. A3A packs the new master strengths into `_GroundBankCoverRetreatStrength`, resolves effective retention through the accepted Bank material blend, and applies the four channels to existing vegetation, snow, frost, and Painted Accent consumers. Debug mode `36` displays vegetation/snow/frost retreat in RGB and mode `37` displays Painted Accent retreat. Raw Painted Accent modes `28–29`, A2C.4 role gating, ordinary-Ground UV3 absence, corridor UV3 semantics, Bank spatial math, hydrology, and Riverbed rendering remain unchanged.

The user confirmed A3A. Its four independent cover-retention channels, debug modes `36–37`, ordinary-Ground exclusion, and Material-only persistence are accepted. The supplied archive still has no `.git` metadata, so branch/HEAD/working-tree comparisons remain an integration prerequisite.

## V3S-A3B — Independent Shore Hydrology Modifier

**Status:** A3B.1 and A3B.2 are Unity-validated and accepted. Existing modes `38–39` visually validate the independent Shore wetness field, and normal rendering now applies the accepted independent Hydrology Modifier response.

A3B adds a reusable hydrology modifier with independent metre-based Shore reach, removes Shore from generic damp/deposit and Pooled Wetness, applies bounded local/global wetness character after dry substrate and cover composition, and adds debug modes `38–39`. The approved sixteen-file implementation passed its source-level checks, but Unity exposed an editor-event integration defect not represented by those checks.

### V3S-A3B.1 — Inspector asset-creation GUI-scope repair

**Status:** Unity-validated and accepted. The user successfully created and selected a Hydrology Modifier, and subsequent spatial debug validation reached normal rendering without the prior layout exception.

The failure occurs because the Hydrology Modifier selector opens `SaveFilePanelInProject` and creates an asset while its `HorizontalScope` and nested `DisabledScope` are active. The Surface Layer selector has the same structure and is included as the directly related latent path. The bounded three-file repair defers both create/duplicate operations through `EditorApplication.delayCall`, captures stable target objects and serialized property paths instead of retaining `SerializedProperty`, reapplies local/shared ownership behavior after assignment, and preserves all runtime, shader, River, asset-schema, default, and folder behavior. Exact evidence, acceptance criteria, file scope, risks, and validation are canonical in `Ground_River_Coupled_Surface_Response_Architecture.md`.

The implementation now captures Create/Duplicate requests only during Inspector drawing and performs modal creation in a delayed callback outside all active IMGUI scopes. A fresh `SerializedObject` assigns the created asset to the captured property path, preserves Undo and local/shared ownership behavior, and refreshes affected Grounds. Exact three-file scope, CRLF/LF preservation, complete lexical C# structure checks, and static ownership/assignment contracts pass. Unity validation confirms the creation path now reaches modifier authoring and spatial debug without the prior layout exception.

### V3S-A3B.2 — Wet-response calibration and Hydrology Inspector consolidation

**Status:** Unity-validated and accepted.

The spatially correct local Shore wetness and effective wetness fields remain unchanged. A3B.2 preserves the existing global-weather coefficients, removes hidden `0.18` and `0.22` attenuation from local modifier darkening and smoothness, applies local specular as an additive absolute contribution after dry substrate resolution, and consolidates modifier definition plus spatial application under one Shore Hydrology foldout. Existing modes `38–39` remain byte-for-byte unchanged; no debug mode was added. The exact six-file implementation passed the canonical `44/44` source contract audit, C# and HLSL parser checks, line-ending checks, and isolated Clang HLSL expression checks. The user subsequently confirmed the calibrated normal-render response and consolidated Inspector behavior; A3B.2 is accepted.


## V3S-A4A / A4A.1 / A4B — Riverbed composition completion

**Status:** A4A remains a historical incomplete proof. A4A.1 normalized composition and A4B exact-support Riverbed hydrology were subsequently Unity-validated and accepted, then completed by the accepted A4B.1–A4B.3 finish, highlight, and inward-transition corrections. A4B.3 is the frozen final River-coupled baseline.

A4A proved exact role-gated `Ground Riverbed Support`, dry custom Riverbed profile transport, and submerged vegetation/snow/frost/Painted Accent exclusion. A4A.1 replaces its sequential Bank-then-Riverbed albedo/finish lerps with one normalized primary/Bank/Riverbed weight triplet, adds migration-safe `Primary Ground`, `Inherit Bank Surface Layer`, and `Custom Riverbed Surface Layer` ownership, and consolidates the main Inspector into `River-Coupled Ground Response — River Bank` and `River-Coupled Ground Response — Riverbed`. A4B adds exact-support Riverbed wetness with inherited/custom/disabled hydrology ownership and one `Riverbed Wetness Strength`; no reach/fade/depth mechanic or debug view is added. `Ground Effective Wetness` now includes global, Shore, and Riverbed wetness, while `Ground Local Shore Wetness` remains Bank-only. The exact implementation and validation contract is canonical in `Ground_River_Coupled_Surface_Response_Architecture.md`.

## V3R-A1 — Ground Elevation Readability proof

**Status:** Unity-validated and visually accepted.

Gentle generated Ground elevation can disappear from the intended elevated gameplay camera because the surface normals remain close to vertical and the calm material palette provides few broad form cues. V3R-A1 adds two independent, Material-only value responses under `Material Controls > Elevation Readability`:

- **Relief Shading Strength** exaggerates the signed horizontal slope relationship to the existing main-light direction. It uses the real final Ground normal but does not modify that normal, PBR lighting, geometry, shadows, collision, or River interaction.
- **Relative Height Contrast** applies a value shift per metre of generated object-space height relative to the undeformed local Ground plane. Positive generated height brightens and negative generated height darkens. It samples the existing vertex position directly; no height texture or generated mask is introduced.

Both controls default to zero so existing styles remain visually unchanged until authored. The response is evaluated in the existing Ground fragment path, uses the already-resolved main light, adds no noise sample, texture sample, CPU field generation, per-frame rebuild, scene object, or serialized component field, and remains a Material-only edit for both shared variants and local overrides.

V3R-A1 validation gate:

1. With both controls at zero, normal rendering remains unchanged.
2. Relief Shading Strength makes gentle rises and falls readable from the elevated gameplay camera without rings, outlines, or unstable camera-dependent shading.
3. Relative Height Contrast separates higher and lower terrain while remaining subordinate to macro patches, semantic response, Painted Accents, props, River, and lighting.
4. Shared and local controls persist through assembly reload and refresh only the Material stage.
5. No Ground geometry, mesh, collider, River handoff, Painted Accent output, or shader noise budget changes.

## PA-P4 — Deterministic External-Conflict Index and Incremental Reconciliation

**Status:** Unity-validated and accepted. Tightness `1.00` preserved the complete deterministic output baseline and reduced the complete ProjectedGlyphs/coverage pass to approximately `880.50 ms`, including approximately `2.67 ms` external-conflict validation and `38.23 ms` final reconciliation. This remains an execution-only optimization of the accepted PA-P3 projected-cluster path.

Authoritative cluster construction now maintains a deterministic fixed-grid index over the expanded projected bounds of glyphs already accepted before each candidate. A candidate member queries only the grid cells touched by its own expanded bounds, deduplicates returned glyph indices, sorts them back into original accepted-list order, and then runs the unchanged bounds and exact external-overlap authority. The grid can only omit glyphs whose expanded bounds occupy no common cell; any genuinely overlapping expanded bounds necessarily share at least one indexed cell.

Final reconciliation no longer rechecks cluster-to-cluster and cluster-to-initial-independent relationships already validated during authoritative construction. It tests accepted clusters only against participant independents committed after cluster allocation and against fallback independents introduced by an earlier reconciliation removal. Each surviving cluster records how many late independents it has already checked, so subsequent passes evaluate only newly introduced relationships while preserving the existing descending record order and removal/restart behavior.

Compact diagnostics report external spatial queries, visited cells, unique candidates, avoided full-list comparisons, actual bounds/detailed tests, reconciliation clusters examined, previously validated relationships skipped, new independent relationships tested, and legacy full-list relationships bypassed. The exact external-overlap predicate, cluster removal behavior, fallback glyphs, quotas, attempt budgets, and coverage remain authoritative. Projected-glyph generator revision advances from 18 to 19.

PA-P4 invariants:

- no candidate, donor, quota, layout, contact, attempt-budget, threshold, fallback, or coverage change;
- candidate conflict tests retain original accepted-glyph order after spatial filtering;
- the spatial index never declares validity or overlap; it only conservatively narrows the exact candidate set;
- initial independent and cluster-to-cluster relationships are omitted from reconciliation only because they were already checked during construction;
- newly committed and reconciliation-generated independent glyphs remain exact reconciliation authority;
- PA-P1 through PA-P3 execution paths, Ground/River startup coalescing, and River shader compile recovery remain untouched.

PA-P4 validation gate:

1. The Tightness `1.00` authoritative baseline remains identical: 318 projected marks, pair/triplet 78/48, nine pair shortfalls, 1,127 attempts, 126 accepted clusters, 300 clustered participants, 20,356 baked segments, and 5,707 covered texels.
2. Visual parity holds and final reconciliation removals remain zero for the current scene and seed.
3. External index diagnostics show a material reduction from full-list candidates to unique spatial candidates without changing external-conflict fallbacks.
4. Reconciliation tests only late/new independent relationships and materially reduces the accepted 665.07 ms reconciliation baseline.
5. External-conflict validation falls materially below the accepted 361.92 ms baseline and complete projected regeneration falls below the accepted 1,900.85 ms PA-P3 baseline.

## PA-P3 — Cheap Candidate Pruning and Precomputed Internal-Overlap Segments

**Status:** Unity-validated and accepted. Tightness `1.00` preserved the complete deterministic output baseline and reduced contact solving from 2,121.62 ms to 379.78 ms, near-parallel validation from 555.82 ms to 121.90 ms, candidate internal-overlap validation from 1,546.27 ms to 194.31 ms, and complete regeneration from 3,705.96 ms to 1,900.85 ms.

Candidate contact evaluation now performs deterministic scalar rejection before projected-polyline geometry checks. After terminal placement and the existing contact-side invariant, the solver resolves the translated centroid, pair-local or triplet step, near-collinear endpoint rule, and the exact existing candidate score. Candidates that fail step retention, fail the existing pair near-collinear rule, or cannot beat an already valid lower-scoring candidate are skipped before near-parallel and swept-width validation. `bestScore` is still updated only by a fully geometry-valid candidate, so a skipped noncompetitive candidate cannot become the selected result.

Internal swept-width validation now prepares reusable per-segment metadata once per method invocation in the existing per-build scratch object. Each segment caches endpoints, endpoint half-widths, maximum half-width, and unexpanded X/Y bounds. The Cartesian pair loop reuses those values for the accepted PA-P1 conservative bounds test and unchanged exact intersection/distance/width interpolation authority. No per-call heap allocation is introduced in the authoritative path.

Compact diagnostics report pre-geometry step-retention rejections, pre-geometry noncompetitive-score rejections, and candidates sent to geometric validation. The existing pair-step counter remains active but its classification point moves before geometry, so its numeric value may change even when output is identical. Projected-glyph generator revision advances from 17 to 18.

PA-P3 invariants:

- no quota, donor, layout, contact, threshold, attempt-budget, fallback, reconciliation, glyph, or coverage change;
- no accepted-candidate score or tie-breaking change;
- `bestScore` remains sourced only from candidates that pass all geometry checks;
- the PA-P1 exact swept-width authority and PA-P2 near-parallel authority remain unchanged;
- reusable segment metadata belongs to the existing per-build scratch object;
- Ground/River startup coalescing and River shader compile recovery remain untouched.

PA-P3 accepted evidence:

1. Tightness `1.00` preserved 318 projected marks, pair/triplet 78/48, nine pair shortfalls, 1,127 attempts, 20,356 baked segments, and 5,707 covered texels with visual parity.
2. Pre-geometry pruning rejected 4,102 candidates by step retention and 338 by noncompetitive score before only 1,242 candidates reached geometry.
3. Candidate internal-overlap validation fell from 1,546.27 ms to 194.31 ms and near-parallel validation fell from 555.82 ms to 121.90 ms.
4. Complete projected regeneration fell from 3,705.96 ms to 1,900.85 ms without shifting cost into contact placement, coverage, or surface validation.

## PA-P2 — Precomputed Near-Parallel Segments and Conservative Distance Broad Phase

**Status:** Unity-validated and accepted. Tightness `1.00` preserved the complete deterministic output baseline and reduced near-parallel validation from 5,144.74 ms to 555.82 ms, contact solving from 6,701.69 ms to 2,121.62 ms, and complete regeneration from 8,264.11 ms to 3,705.96 ms.

The current near-parallel validator remains authoritative. Each invocation now prepares reusable metadata for the right-hand projected segments once: endpoints, normalized direction, length, average half-width, and unexpanded bounds. The nested pair loop reuses this metadata instead of recalculating right-segment magnitude, normalization, width, and bounds for every left segment.

Before tangent alignment and exact segment-distance work, the validator compares unexpanded X/Y segment-bound gaps against the same exact near-parallel clearance derived from average half-widths and the existing 0.88 clearance fraction. A pair is skipped only when an axis gap is strictly greater than that clearance. Every uncertain pair retains the existing 22-degree alignment gate, exact segment distance, shared-axis interval overlap, 0.025 m / 14% accumulated blend threshold, and early-return behavior.

Compact diagnostics report method calls, metadata preparations, right segments prepared, segment pairs considered, axis-gap rejections, alignment rejections, exact distance tests and passes, interval-overlap evaluations, and detected blends. The projected-glyph generator revision advances from 16 to 17 for the execution and diagnostic payload change.

PA-P2 invariants:

- no quota, donor, layout, contact, threshold, fallback, reconciliation, glyph, or coverage change;
- no reduction of point count or quality authority;
- reusable metadata belongs to the existing per-build scratch object;
- the broad phase can only prove that a pair is too far apart and cannot accept a blend;
- the previous PA-P1 exact swept-width broad phase remains unchanged.

PA-P2 validation gate:

1. The Tightness `1.00` deterministic baseline remains identical: 318 projected marks, pair/triplet 78/48, nine pair shortfalls, 1,127 attempts, 536 near-parallel rejections, 3,891 swept-width rejections, 20,356 baked segments, and 5,707 covered texels.
2. Near-parallel exact distance tests fall materially below considered segment pairs while detected blends remain 536.
3. Near-parallel validation falls materially below the accepted 5,144.74 ms baseline; below 2,500 ms is the initial success threshold.
4. Visual parity holds at maximum participation, tightness, triplet share, and verticality.

## PA-P1 — Conservative Internal Segment-Pair Broad Phase

**Status:** Unity-validated and accepted. The deterministic Tightness `1.00` baseline remained identical. The conservative bound rejected 13,783,511 of 13,810,418 segment pairs (99.805%), reduced exact narrow-phase submissions to 26,907, and lowered the comparable complete regeneration from 9,969 ms to 8,264 ms.

The exact internal-overlap validator remains authoritative. Before each exact segment intersection and closest-distance calculation, the generator now derives a conservative maximum possible swept clearance from both segments' maximum endpoint half-widths and the existing 0.98 body / 0.94 exact-contact clearance fraction. Unexpanded segment-axis gaps may skip a pair only when X or Y separation is strictly greater than that maximum possible clearance. The bound does not subtract numerical tolerance and never declares an overlap; uncertain pairs continue through the unchanged exact narrow phase.

Compact workload diagnostics report internal-overlap method calls, final-silhouette overlap calls, segment pairs considered, conservative broad-phase skips, exact narrow-phase submissions, exact centreline intersections, and exact swept-clearance rejections. Timing further separates near-parallel validation, candidate internal-overlap validation, contact-placement/other work, and final-silhouette overlap validation. No per-segment timer or Console output is introduced.

PA-P1 invariants:

- no candidate, donor, quota, layout, contact, fallback, or reconciliation ordering changes;
- no clearance threshold, tolerance, glyph geometry, point count, or coverage change;
- the exact segment intersection/distance/width interpolation path remains the sole overlap authority;
- Ground/River startup coalescing and the accepted River shader baseline remain untouched;
- projected-glyph generator revision advances from 15 to 16 for the execution/diagnostic payload change.

PA-P1 validation gate:

1. The deterministic projected baseline remains identical, including 318 projected marks, pair/triplet 78/48, nine pair shortfalls, 1,127 attempts, 3,891 swept-width candidate rejections, 20,356 baked segments, and 5,707 covered texels.
2. Broad-phase rejected pairs are substantial and exact narrow-phase submissions fall materially below total considered pairs.
3. Projected cluster composition falls materially below the accepted 8.55–8.98 second baseline without shifting another Ground stage.
4. Visual parity holds at maximum participation, tightness, triplet share, and verticality.

## V3J.4F3.3A — Copyable Diagnostics and Projected Cluster Composition Audit

**Status:** Unity-validated and accepted. This patch is instrumentation-only: it does not change Painted Accent population, distribution, companion quotas, geometry, rejection thresholds, coverage, or rendering.

Implementation contract:

- Add `Copy Full Generation Report` under `Last Generation Diagnostics`. The button copies cached timing, surface-funnel, projected-companion, and coverage records through `EditorGUIUtility.systemCopyBuffer`; it does not regenerate or write to the Console.
- Display projected-glyph and companion diagnostics independently of the projected-glyph visual overlay toggle.
- Separate the current regeneration timing from retained timings for the last completed `SurfaceStrokes` and `ProjectedGlyphs` executions. A material-only or cache-hit update must state that those stages were not executed rather than presenting retained substage values as part of the current operation.
- Split authoritative projected-cluster allocation wall time into quota/spec preparation, participant scoring/partition, donor selection, prototype preparation, contact solving, internal silhouette/quality validation, cluster-attempt surface/domain validation, external accepted-glyph conflict validation, result commit/participant removal, final reconciliation, and residual loop overhead.
- Record bounded construction effort: requested/succeeded/failed clusters before reconciliation, total attempts, successful-attempt min/mean/max, success buckets `1`, `2–4`, `5–8`, `9–16`, `17–32`, `33–72`, and failures exhausting their dynamic attempt budget.
- Record external-conflict workload: glyph candidates examined, bounds tests, bounds-overlap passes, detailed overlap tests, and conflict rejections. Final accepted counts and reconciliation removals remain separate so construction success is not confused with final survival.
- Advance projected-glyph generator revision to 15 because the cached diagnostics payload changes; projected geometry semantics remain unchanged.

V3J.4F3.3A validation gate:

1. The copy button produces one complete report without regeneration, asset dirtiness, or Console spam.
2. Material-only and cache-hit operations identify unexecuted stages while retaining a clearly separated last-completed stage record.
3. A full 450–500 proposal regeneration reports cluster wall time, every measured substage, residual overhead, attempt distribution, and external-conflict workload.
4. Requested/achieved quotas, projected glyph output, coverage, and visual geometry remain unchanged relative to V3J.4F3.3 for the same seed and settings.

## V3J.4F3.3 — Count-Neutral Regional Selection and Generation Funnel Audit

**Status:** Unity-validated and accepted. This patch removes stochastic regional deletion without changing Stroke Density semantics, adding refill, or changing physical/projected validity rules.

The previous production path selected the weighted proposal budget and then discarded proposals through a second regional survival roll whose probability-weighted mean was approximately 45%. That made Distribution Contrast simultaneously control location and hidden population loss. The active contract is now count-neutral:

```text
candidate pool
→ one combined patch / semantic / regional weighted ranking
→ fixed selected proposal count
→ physical validation
→ projected validation
→ authoritative companion allocation
```

Implementation contract:

- Remove the production regional-thinning roll and its approximately 55% average deletion. Every selected proposal reaches physical construction and validation exactly once.
- Convert the quiet/supporting/accent weights into normalized ranking multipliers with probability-weighted mean `1.0`. Distribution Contrast therefore redistributes the fixed selected proposal budget instead of changing its size.
- Preserve the current Stroke Density proposal target and candidate-pool multiplier for the first measurement. No accepted-count refill, density rebasing, river prefilter, partial sort, or validation relaxation is included.
- Retire thinning-survival semantics from the composition overlay. Selected proposals are displayed by region mode without a survive/reject state.
- Add one compact funnel record covering candidate pool, selected/evaluated/accepted counts, surface and projected acceptance, quiet/supporting/accent selection plus surface/projected validity, and proposal-rank quartile surface/projected validity.
- Add source-stage timings for candidate construction/weighting, the regional-weighting subset, candidate ordering, composition setup, stroke setup, surface construction/validation, and placement diagnostics. Existing projected, coverage, and total timings remain authoritative.
- Advance the surface-stroke generator revision to 23.

V3J.4F3.3 validation gate:

1. With the same seed and unchanged Stroke Density, selected proposals equal physically evaluated proposals; no regional-thinning rejection exists.
2. Distribution Contrast still shifts selected marks among quiet/supporting/accent regions while the selected proposal count remains fixed.
3. The full funnel reports actual surface/projected losses and Q1–Q4 acceptance so the next efficiency patch is selected from measured evidence.
4. Companion quotas, projected contact geometry, cluster quality guards, coverage, and rendering remain unchanged.

## V3J.4F3.2B — Contact-Side and Swept-Width Correction

**Status:** Implemented; pending Unity compilation and same-seed visual validation. This is a geometric correctness correction over V3J.4F3.2A, not an additional aesthetic filter.

Audit of the overlap regression found that projected contact placement used the moving terminal's outward direction with the wrong sign. The endpoint centre was placed on the far side of the anchor centreline, allowing the rest of the moving body to fold back through or alongside the anchor. The point-sampled overlap validator then permitted substantial visible-width overlap and exempted a broad contact neighbourhood.

Implementation contract:

- Position the moving terminal on the near side of the anchor with `anchorContact - outward * endpointCentreSeparation`. The separation remains the projected anchor silhouette distance plus the moving terminal cap and any authored loose gap.
- Enforce a contact-side invariant after translation: the anchor must lie in the moving endpoint's outward direction and the moving body must lie inward from that endpoint.
- Replace internal point-sampled clearance with variable-radius segment-to-segment swept-width validation. Closest parameters interpolate each segment's visible half-width; non-contact body pairs require 98% of combined width clearance.
- Restrict the numerical contact tolerance to the actual moving terminal segment paired with the anchor segment containing the intended contact. That exact pair uses a 94% clearance tolerance; no multi-sample neighbourhood is exempt. Centreline crossings remain illegal everywhere.
- Keep external independent-mark conflict thresholds, authoritative quotas, distribution, family mix, angle limits, compactness, and F3.2/F3.2A aesthetic guards unchanged.
- Add compact cumulative counters for wrong-side terminal rejection and swept-width internal-overlap rejection.
- Advance projected-glyph generator revision to 14.

V3J.4F3.2B validation gate:

1. Same-seed clusters terminate on the approaching side of their anchors; folded arches, doubled contacts, and triangular pass-through forms are materially removed.
2. Visible member bodies no longer overlap between projected samples, including oblique and short terminal overlaps.
3. Legal endpoint-to-edge contacts remain available and authoritative pair/triplet quotas do not silently change.
4. Existing conservative unusual-shape policy remains active; no new generic fork, compactness, or branch-prominence rejection is introduced.

## V3J.4F3.2A — Conservative Triplet Pseudo-Contact Guards

**Status:** Implemented; pending Unity compilation and visual validation. V3J.4F3.2 remains the active broad cluster-quality baseline.

The remaining malformed minority does not justify a broad symbolic-shape filter. This follow-up deliberately avoids generic fork, chevron, same-side attachment, branch-prominence, or stronger compactness rejection because those rules would remove useful unusual compositions and drive the field toward repeated stock shapes.

Only two high-confidence post-composition failures are newly rejected:

- **Crowded dual junctions:** the two real final triplet junction loci may not collapse into the same tiny visible knot, even when their source anchor samples came from different members. The final-locus threshold is intentionally weaker than the existing candidate-time shared-locus rule and applies only to complete triplets.
- **Accidental free-end pseudo-contacts:** an unconnected terminal may not point directly into another unconnected terminal or another member body at almost-touching distance. This targets arrowhead/loop closures and tiny extra teeth, while close terminals that face away from one another remain legal.

The existing severe-compression guard remains unchanged and conservative. Both new failures return donors to the authoritative bounded retry search; they do not alter participation, triplet share, layout quotas, distribution, or total descriptor count. Compact generation telemetry records each rejection class without per-cluster Console output.

V3J.4F3.2A validation gate:

1. Existing pair and triplet quotas, distribution, layout weights, and accepted unusual compositions remain materially unchanged.
2. Final triplets no longer collapse both junctions into one tiny visible knot.
3. Unconnected terminal tips no longer almost meet one another or stab into another member body to form a second accidental junction.
4. The existing compactness counter and the two new counters are reviewed against quota shortfall; broad rejection is not accepted.

### 2026-07-14 — Patch V3J.4F3.2: Projected Cluster Quality Guards

**Status:** Implemented; pending Unity compilation and visual validation. Distribution control consolidation from V3J.4F3.1 and authoritative final companion quotas from V3J.4F3 remain active.

Unity inspection after V3J.4F3.1 accepted the three-control distribution scheme but exposed a minority of technically legal, visually malformed clusters: long near-parallel members blending into one thick band, multiple triplet tips competing for essentially the same attachment locus, and a smaller number of severely compressed triplet knots. V3J.4F3.2 adds projected-space quality rejection and bounded retry without changing quotas, distribution, family selection, rotation limits, or authored controls.

Implementation contract:

- Reject member pairs whose centreline segments remain within 88% of combined visible half-width while aligned within 22 degrees for at least `max(0.025 m, 14% of the shorter authored length)`. This targets sustained body blending, not ordinary terminal contact.
- For triplets, reserve attachment slots already consumed by an earlier contact. A later member cannot attach to the same or immediately adjacent projected sample on either side of the existing junction.
- Reject triplet contact loci that collapse within `max(10% of the shorter authored length, 2.25 times the candidate combined half-width)` of either side of an existing junction.
- Keep the compact-triplet guard deliberately conservative: reject only when all three member centroids fit inside 42% of the shortest authored member length. Interesting compact and irregular compositions remain eligible.
- Feed all quality failures back into the existing authoritative bounded donor/layout retry loop. Do not silently substitute layout quotas or reduce requested participation.
- Add compact cumulative telemetry for near-parallel body, occupied attachment slot, shared contact locus, and severe compactness candidate rejection. No per-cluster Console logging.
- Advance projected-glyph generator revision to 12.

V3J.4F3.2 validation gate:

1. Existing distribution controls and resolved pair/triplet/layout quotas remain unchanged.
2. Long near-parallel overlapping bands are materially reduced without eliminating ordinary shoulder or stepped contacts.
3. Triplets no longer place two tips into the same tiny junction or immediately adjacent anchor samples.
4. Only clearly collapsed knots are removed; unusual but readable compact clusters remain.
5. Achieved quota and shortfall telemetry is reviewed alongside the four new rejection counters so quality improvement is not bought through silent population loss.

### 2026-07-14 — Patch V3J.4F3.1: Distribution Control Consolidation

**Status:** Implemented; pending Unity compilation and visual validation. This patch changes Painted Accent distribution authoring only. The authoritative companion-quota architecture from V3J.4F3 remains active and unchanged.

The previous Inspector exposed five overlapping field controls—Distribution Patch Scale, Distribution Patchiness, Distribution Sparse Floor, Regional Zone Scale, and Regional Density Contrast—plus Companion Accent Bias. Although these values drove distinct implementation layers, they all changed related sparse/dense distribution behaviour and were not artistically separable enough to justify six normal authoring controls.

The active normal Inspector now exposes exactly three distribution controls:

```text
Distribution Scale
    size of spatial variation
    low = smaller, more frequent changes
    high = broader local patches and larger coherent regions

Distribution Contrast
    strength of sparse-versus-dense separation
    low = comparatively even field
    high = stronger local preference, stronger regional redistribution,
           and a lower protected sparse-region floor

Cluster Region Bias
    location of the fixed companion quota only
    low = clusters follow the overall field
    high = clusters concentrate in denser accent regions
```

The serialized `paintedAccentDistributionPatchScale`, `paintedAccentDistributionPatchiness`, and `paintedAccentCompanionAccentBias` fields are retained to avoid asset migration. Their displayed and runtime meanings are now Distribution Scale, Distribution Contrast, and Cluster Region Bias. The three former subordinate distribution fields remain serialized but hidden for compatibility and no longer independently control production output.

The production mapping is deterministic:

- Distribution Scale drives the continuous patch scale directly over `2–24 m` and derives the coherent regional scale over `1–13.5 m` from the same normalized value.
- Distribution Contrast directly drives patch preference and regional density redistribution. It also derives a protected sparse floor from `0.40` at zero contrast to `0.10` at full contrast.
- Cluster Region Bias is unchanged mathematically; only its authoring name and placement are clarified. It never changes Companion Participation, Triplet Share, or total mark count.

Only Distribution Scale and Distribution Contrast participate in the surface-stroke cache signature. Cluster Region Bias remains a projected-composition input. The old hidden values cannot cause invisible cache invalidation or alter output.

The next cluster-quality patch remains intentionally conservative for compact triplets. It may add a light degeneracy guard, but must not broadly reject the interesting irregular compositions already accepted. Stronger work is reserved for proven near-parallel body blending and multi-tip attachment convergence.

V3J.4F3.1 validation gate:

1. Unity compiles with zero C# and shader errors.
2. Both Painted Accent authoring inspectors show one Distribution group containing only Distribution Scale, Distribution Contrast, and Cluster Region Bias.
3. Distribution Scale visibly changes spatial feature size without changing the total proposal target or companion quota.
4. Distribution Contrast visibly changes sparse/dense separation while retaining non-zero sparse-area presence.
5. Cluster Region Bias changes where clusters concentrate without changing requested or achieved pair/triplet counts.
6. Existing style assets retain their serialized master scale, contrast, and cluster-bias values without a migration step.

### 2026-07-14 — Patch V3J.4F3: Authoritative Final Companion Quotas

**Status:** Active quota architecture; distribution authoring is superseded by V3J.4F3.1. This architecture supersedes V3J.4F2.3 population allocation while retaining its accepted pair-local shape grammar and exact terminal-contact validation.

The previous source-stage companion pass was best-effort: it calculated a participant target before final validation, randomly requested pairs or triplets, searched for conveniently positioned existing candidates, and silently accepted whatever survived later surface and projected fallback. Unity telemetry proved that raising its source target did not make the authored result authoritative. V3J.4F3 therefore removes companion ownership from surface placement and resolves composition only after every ordinary independent mark has passed projected prototype and final ground/domain validation.

The active authoring contract is now separated into independent responsibilities:

```text
Stroke Density
    total proposal population

Distribution Scale / Distribution Contrast
    size and strength of independent source-mark distribution

Companion Participation
    exact target share of final valid projected marks assigned to clusters

Triplet Share
    exact target share of clustered participants assigned to triplets

Cluster Region Bias
    where cluster anchors are preferred; never changes the global quota

Companion Tightness / Cluster Verticality
    junction spacing and translation-driven shape only

Advanced Pair/Triplet Layout Weights
    normalized exact whole-cluster type quotas
```

The serialized `paintedAccentHorizontalCompanionStrength` field is retained for asset compatibility, but its Inspector label and runtime meaning are now `Companion Participation`. The serialized triplet-verticality field is likewise retained, while its displayed meaning is `Cluster Verticality`. New serialized controls are `Triplet Share`, the backing field now displayed as `Cluster Region Bias`, four pair-layout weights, and four triplet-layout weights. Existing assets receive deterministic fallback values without a field rename or migration.

The production build sequence is:

1. Generate, thin, and physically validate the ordinary independent surface descriptors.
2. Build and finalize every valid independent projected prototype.
3. Solve the closest feasible integer counts `P`, `T`, and `S`, where `2P + 3T + S = N`, from Companion Participation and participant-based Triplet Share.
4. Normalize pair and triplet layout weights into exact whole-cluster counts by deterministic largest-remainder allocation.
5. Rank eligible donor marks by deterministic order blended with local projected density according to Cluster Region Bias.
6. Build requested triplets and pairs atomically from donor slots using the accepted projected contact, silhouette, ground/domain, and external-conflict validators. Donor family, seed, length, width, strength, and total population are preserved; only clustered projected placement changes.
7. Commit non-participants and any explicit unresolved donor shortfall as independent glyphs. Never silently substitute a different layout or pair/triplet type.
8. Report requested, achieved, and shortfall counts globally and per layout.

`Companion Participation = 0` is an exact independent compatibility path. At nonzero participation, no hidden 68%/94% multiplier, random triplet roll, same-region candidate dependency, source target-plan rejection, or silent triplet-to-pair downgrade controls the final population. Surface-stroke cache signatures no longer include companion composition controls; those controls invalidate projected glyphs, coverage, and material only.

Authoritative means the generator retries deterministic donor combinations within a bounded dirty-time budget and exposes any remaining geometrically impossible shortfall. It does not bypass river/modifier exclusion, ground sampling, projected silhouette safety, or exact terminal-contact rules. Total accepted projected mark count is preserved whether a requested cluster succeeds or falls back.

Compact projected diagnostics report:

- valid projected mark count and requested participation/triplet share;
- resolved participant, pair-cluster, and triplet-cluster counts;
- requested and achieved counts for every pair/triplet layout;
- bounded build attempts and explicit pair/triplet shortfall;
- final clustered participants, accepted glyphs, and achieved percentage;
- contact/surface/external-conflict fallback and final reconciliation removal counts.

**Canonical methods-tried ledger update:**

- raising best-effort participant ceilings and repairing source plans: **rejected as the population authority**; telemetry showed the requested and final results could diverge substantially;
- nearby pre-existing candidate search as a prerequisite for composition: **rejected**; descriptors now act as donor population slots;
- random pair/triplet and layout probabilities: **rejected**; exact integer quotas replace them;
- source-stage companion mutation: **retired from active production**; final valid projected prototypes own composition;
- bounded projected-space atomic construction, pair-local stepping, layout-aware contact, exact silhouette-edge termination, fixed descriptor population, and explicit fallback evidence: **retained**.

V3J.4F3 validation gate:

1. Unity compiles with zero C# and shader errors; Companion Participation `0` reproduces the independent baseline.
2. At `90%` participation and `45%` Triplet Share, diagnostics resolve whole-mark quotas against the final valid projected pool and the final clustered percentage closely matches the resolved target.
3. Requested versus achieved pair/triplet and per-layout counts agree, or every difference appears as an explicit shortfall rather than silent substitution.
4. Changing Participation or Triplet Share rebuilds projected glyphs/coverage without rebuilding surface placement; Tightness, Verticality, Accent Bias, and layout weights do not alter the total projected mark count.
5. Existing accepted pair/triplet shapes, bounded orientation, pair-local stepping, and exact no-pass-through contacts remain intact.
6. Record full projected quota diagnostics and regeneration timing before freezing shape/population and proceeding to the final spatial distribution pass.

### 2026-07-14 — Historical Patch V3J.4F2.3: Majority Companion Source Completion and Exact Contact Stop

**Status:** Superseded by V3J.4F3 for population allocation. Its pair-local geometry and exact contact-stop work remain active.

The F2.2 Unity sample established that the pair-local shape correction works, but also proved that the low clustered population is not caused by the coarse reservation gates. The authoritative source telemetry was:

```text
post-thinning companion target / formed / source-accepted: 264 / 206 / 182
primary attempts / target-plan rejected:                  311 / 301
missing secondary / tertiary / triplet downgrades:        122 / 9 / 226
reservation envelope / occupied cell:                     2 / 1
structure rejected pair / triplet:                        94 / 207
source-member fallback / incomplete clusters:             3 / 12
clusters composed pairs / triplets:                       100 / 2
```

This evidence changes the earlier provisional population plan. The one-cluster-per-cell and padded-envelope gates are retained because they rejected only three attempts in this generation. The confirmed dominant source losses were the 68% participant ceiling, single-region candidate search, and pair/triplet target plans that failed their own structure gates before candidate placement.

F2.3 therefore makes the following dirty-time-only changes:

- raises the maximum source participant target from `0.68` to `0.94` of the post-thinning candidate population at Companion Strength `1`;
- keeps same-region candidates preferred, then performs a bounded nearby cross-region fallback using the existing relocation radius when the same region contains no usable member;
- repairs only pair plans that would otherwise fail by moving the secondary along the already accepted pair-local shared normal until both the centre-step and pair-normal-span requirements are satisfied with an 8% margin;
- retries only failed structured triplet plans with `1.35x` and then `2.00x` translation scale, preserving the bounded angle contract and leaving already-valid triplets unchanged;
- retains the anti-chain envelope, occupied-cell reservation, physical validation, projected atomic commit, fixed descriptor population, maximum three members, independent fallback, R8 coverage, and G3 performance architecture;
- replaces negative projected centreline penetration with an exact silhouette-edge stop. Interior attachment distance accounts for the anchor half-width projected along the moving endpoint approach plus the moving cap half-width; near-tangent interior approaches are rejected;
- removes the broad segment-intersection exemption around intended contacts. Point-clearance relaxation remains local to the contact, but any actual centreline crossing is now illegal;
- adds final projected clustered-participant count and percentage plus the number of clusters removed during final independent reconciliation.

Compact source diagnostics additionally report cross-region candidate selections, pair structure repairs, and triplet structure repairs. No per-cluster Console logging is added.

**Canonical methods-tried ledger update:**

- removing the occupied-cell or envelope gates as the primary population fix: **rejected for this pass** because telemetry showed only `1` and `2` rejections respectively;
- raising the participant ceiling alone: **partially useful but insufficient**, because target-plan rejection and missing same-region candidates were dominant;
- pair-local target repair without extra rotation: **active implementation**;
- translation-only retry for failed triplet plans: **active implementation**;
- nearby cross-region candidate fallback within the existing relocation bound: **active implementation**;
- reversing projected independent/cluster priority or adding projected triplet-to-pair salvage: **deferred** until the new final clustered percentage and projected fallback telemetry prove that stage is dominant.

V3J.4F2.3 validation gate:

1. Unity compiles with zero C# and shader errors; Companion Strength `0` remains identical to the independent baseline.
2. At Strength/Tightness/Verticality `1 / 1 / 1`, source target participation rises to approximately 94% of post-thinning candidates and formed/source-complete participation increases materially.
3. Pair/triplet target-plan rejection drops sharply; the new repair counters explain how much was recovered without widening angle limits.
4. Most accepted projected glyphs belong to complete pairs or triplets. Use the new final clustered-participant percentage, not visual counting alone.
5. The isolated pass-through contact is absent: moving members stop at the visible edge of the anchor and do not cross its centreline.
6. Pair-local stepping, the 10/15-degree companion allowances, 42-degree cap, atomic fallback, and accepted F2.2 shape language remain intact.
7. Paste the complete Painted Accent Placement and Projected Glyph diagnostics, including final clustered percentage and reconciliation-removal count, before deciding whether projected conflict priority or triplet salvage needs one final targeted patch.

### 2026-07-13 — Historical Patch V3J.4F2.2: Pair-Local Stepping and Cluster Attrition Audit

**Status:** Implemented; pending Unity validation and telemetry review. This patch corrects the remaining pseudo-single-line pair geometry and adds cumulative evidence for the separate low-cluster-population problem. It does not yet relax the participant ceiling or any anti-overlap safety gate.

F2.1 proved that bounded orientation and stronger translation work, but its pair gate measured displacement along permanent visual vertical. Two nearly collinear members could therefore pass merely by sharing a shallow diagonal slope. F2.2 makes pair-local departure authoritative:

- source pair structure is measured perpendicular to the normalized average of the two member axes, so a straight horizontal or diagonal continuation measures approximately zero;
- projected pair step retention uses that same pair-local normal, while triplets retain their existing fixed-north vertical contract;
- maximum structured-pair prevalence rises from `0.82` to `0.94` at maximum Verticality; the remaining minority is renamed `Shallow Offset`, receives an explicit pair-local offset, and is no longer intended as a seamless axial continuation;
- pair-layout intent is carried internally from source descriptors into projected composition;
- structured projected pairs use layout-aware anchor samples: stepped and offset layouts prefer quarter/shoulder/interior contacts, shoulder layouts prefer centre/shoulder contacts, and routine structured endpoint-to-endpoint continuation is removed;
- shallow-offset endpoint contacts use a positive rendered gap instead of Tightness overlap;
- touching/overlapping endpoint candidates are rejected when their junction tangents are within 16 degrees and their pair-local step is below 12% of the shorter authored length;
- pair and triplet angle allowances remain frozen at authored jitter plus 10/15 degrees with the same 42-degree absolute cap;
- atomic cluster commit, projected silhouette checks, external conflict reconciliation, independent fallback, fixed population, R8 coverage, and G3 performance architecture remain unchanged.

The 68% maximum companion-participant budget remains unchanged for this evidence pass. Compact source diagnostics now report target versus formed participants, primary attempts, target-plan and pair/triplet structure rejection, missing secondary/tertiary candidates, triplet-to-pair downgrades, envelope/cell reservation rejection, source-member physical fallback, and source-incomplete clusters. Projected diagnostics additionally report near-collinear endpoint candidates rejected. All evidence remains one aggregate record per generation; no per-cluster Console spam is added.

**Canonical methods-tried ledger update:**

- more pair rotation or blind translation increases: **rejected** because the remaining defect was the reference frame, not insufficient magnitude;
- world/screen-vertical pair structure and retention: **rejected** because shallow collinear diagonals satisfy it;
- pair-local shared-normal structure and retention: **active implementation pending validation**;
- unrestricted projected endpoint-to-endpoint search for structured pairs: **rejected**;
- raising the 68% participant ceiling or weakening cell/envelope/conflict gates before knowing the dominant attrition stage: **deferred pending telemetry**.

V3J.4F2.2 validation gate:

1. Unity compiles with zero C# and shader errors, and Strength `0` preserves the independent baseline.
2. At Strength/Tightness/Verticality `1 / 1 / 1`, touching near-collinear horizontal and shallow-diagonal pseudo-single-lines are absent or materially reduced.
3. Structured pairs visibly depart from their shared axis; the shallow-offset minority retains a rendered break and does not read as one crooked line.
4. Triplets and the F2 bounded-angle result remain materially unchanged, with no return of hooks, chairs, trees, X/star crossings, or partial clusters.
5. Capture the complete placement and projected-generation telemetry, especially target/formed/source-accepted participants and every new attrition category.
6. Use that evidence to choose the final population/distribution patch; do not infer which safety gate to relax from screenshots alone.

### 2026-07-13 — Historical Patch V3J.4F2.1: Pair Verticality Completion

**Status:** Implemented; pending Unity validation. This is the final narrow shape-tuning pass on the accepted V3J.4F2 bounded-orientation architecture.

Unity validation of F2 confirmed that the extreme-rotation defect is solved and that the new triplet grammar produces useful contour fragments. The remaining weakness is isolated to two-member clusters: too many pair proposals remain explicitly flat, the source step is modest, and the projected solver may retain too little of that step for the result to read clearly at coverage resolution. F2.1 changes pair placement only:

- maximum structured-pair prevalence rises from `0.64` to `0.82` at maximum Verticality, leaving an intentional approximately 18% flat-proposal minority before later validation/fallback;
- structured pair translation rises from `0.18` to `0.24` of the shorter authored length and from `3.25` to `4.25` stroke widths;
- the gentlest `Offset Echo` translation scale rises from `0.82` to `0.90`;
- the source-stage minimum pair centre step rises from `0.11` to `0.15` of the shorter member length;
- final projected pairs must retain at least 65% of a meaningful requested step, while the validated triplet retention threshold remains unchanged at 42%;
- a requested triplet that cannot commit all three source members now recomputes an actual pair target from scratch instead of reusing a triplet fragment as a pair;
- pair and triplet orientation allowances remain unchanged at authored jitter plus 10/15 degrees with the same 42-degree absolute cap;
- triplet layout, cluster population, anti-chain envelopes, projected contact samples, silhouette/external-conflict validation, atomic fallback, physical validation, R8 coverage, and G3 performance architecture remain unchanged.

Compact cumulative diagnostics now report:

```text
source pair intent: flat / stepped / shoulder / offset
final committed pair vertical step: min / mean / max
final committed pair step fraction of shorter length: min / mean / max
pair contact candidates rejected by the 65% step-retention gate
pair fallback: incomplete / prototype / contact / surface / external
```

These are one aggregate record per generation. No per-pair Console logging is introduced.

**Canonical methods-tried ledger update:**

- increasing pair rotation again: **rejected**; F2's bounded-angle result is accepted and frozen;
- globally tightening projected step retention: **rejected** because the validated triplets do not need the stricter pair threshold;
- pair-only prevalence, translation, source gate, and projected retention tuning: **active implementation pending validation**.

V3J.4F2.1 validation gate:

1. Unity compiles with zero C# and shader errors.
2. `Horizontal Companion Strength = 0` remains visually identical to the accepted independent baseline.
3. At Strength/Tightness/Verticality `1 / 1 / 1`, the pair-intent diagnostic shows flat pairs as a clear minority and the final committed pair-step fraction has a visibly meaningful mean.
4. Most visible pairs have a clear vertical centre difference while some horizontal pairs remain; the accepted F2 triplet shapes are materially unchanged.
5. Companion angle extrema remain within the F2 bounds and no hook, chair, tree, near-vertical-member, X/star, deep-overlap, or partial-cluster regression appears.
6. Record total regeneration, projected cluster composition time, complete pairs/triplets, contact fallback, and pair step-retention rejection count before freezing shape work.

### 2026-07-13 — Historical Patch V3J.4F2: Bounded Orientation, Translation-Driven Steps

**Status:** Unity-validated and superseded by V3J.4F2.1 only for final pair-verticality tuning. F2 superseded V3J.4F as the companion-shape architecture while retaining F's projected-space atomic cluster representation.

Unity evidence after V3J.4F showed that projected contact quality improved, but the visible composition was still created primarily by rotating whole glyphs through the inherited E7/E8 64–84-degree ranges. The result was hooks, chairs, tree-like branches, near-vertical members, and approximately right-angle corners. F2 changes the division of responsibility:

```text
rotation = bounded local variation
translation = primary source of vertical stepping
projected contact search = clean endpoint/shoulder/interior joining
```

F2 changes dirty-time composition only:

- the primary member keeps its deterministic ordinary authored orientation;
- structured pair orientation is limited to authored `Angle Jitter Degrees` plus at most 10 degrees at maximum Verticality;
- structured triplet orientation is limited to authored jitter plus at most 15 degrees, with one absolute 42-degree safety cap for unusually high authored jitter;
- the former 22–58/64-degree pair path and 48–82/84-degree triplet path are removed;
- `Triplet Verticality` now creates explicit signed centre translation along fixed visual vertical and controls layout prevalence, step size, vertical span, and centre non-linearity rather than requiring a steep member or minimum angle spread;
- source-space straight-segment contact distance is no longer treated as authoritative for structured layouts. The source planner supplies a bounded-orientation stepped intent; the projected solver selects endpoint-to-endpoint, endpoint-to-shoulder, or endpoint-to-interior contacts from the actual family/profile polylines;
- projected contact selection records the original member-centre relationships, rejects candidates that reverse or collapse a meaningful requested vertical step, and scores remaining candidates by vertical-step preservation before relocation convenience;
- compact placement diagnostics now report accepted companion angle min/mean/max separately from the full accepted population, making the rotation cap directly auditable;
- complete-cluster atomic commit, projected silhouette checks, external-mark reconciliation, independent fallback, fixed descriptor population, the three-member cap, R8 coverage, and the closed G3 performance architecture remain unchanged.

**Canonical methods-tried ledger update:**

- E7/E8 extreme rotation as the source of verticality: **rejected**.
- V3J.4F actual projected contact and atomic fallback: **accepted and retained**.
- V3J.4F2 bounded orientation plus translation-driven stepped intent: **validated and retained as the base architecture**.

V3J.4F2 validation gate:

1. Unity compiles with zero C# and shader errors.
2. `Horizontal Companion Strength = 0` preserves the accepted V3J.4D3 independent baseline.
3. At Strength/Tightness/Verticality `1 / 1 / 1` with 18-degree authored jitter, accepted pair members remain at or below approximately 28 degrees and triplet members at or below approximately 33 degrees; no production path reaches the historical 64–84-degree extrema.
4. Strong pair/triplet vertical structure remains visible through translated centres, while hook, chair, tree, and near-90-degree motifs are absent or materially reduced.
5. Projected contacts may use endpoints, shoulders, or interiors, but do not reverse or flatten a meaningful requested step and do not create X/star crossings, body penetration, tapered needles, or partial clusters.
6. Companion changes still rebuild only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material` and remain within the accepted dirty-time performance envelope.

### 2026-07-13 — Historical Patch V3J.4F: Atomic Projected-Space Companion Composition

**Status:** Superseded by V3J.4F2 above. Its projected-space atomic cluster architecture remains active, but its inherited E7/E8 extreme-orientation grammar is rejected.

The E7/E8 audit proved that source-space straight-segment contact checks cannot guarantee the final mesh-free glyph silhouette. The visible mark is produced later from the actual wiggled source path, family-specific longitudinal profile, fixed-north projection, per-sample widths, and projected domain validation. E8 therefore validated a simplified skeleton before the actual family/profile geometry existed. It also allowed source-valid clusters to lose members during projected validation, leaving partial forks and orphan branches.

V3J.4F makes projected geometry authoritative without adding runtime objects, graph topology, connectors, proposals, or stroke population:

- every composed source stroke carries an internal cluster ID, member role, intended cluster size, and a complete independent fallback variant;
- if a composed source member fails physical validation, that member is restored immediately to its independent fallback rather than leaving a source-stage fragment;
- projected profiles are built for the actual selected members and their final family/profile seeds before contact placement;
- the primary projected glyph remains fixed, while secondary and tertiary glyphs are translated using contact samples selected from their real projected polylines and visible half-widths;
- contacts may use endpoint, shoulder, or interior samples on the anchor, while the moving member uses the first endpoint-adjacent sample with production-visible width; the sub-pixel tapered tail beyond that contact is removed rather than left as a needle;
- complete projected silhouettes are checked for unintended intersections, body penetration, non-contact overlap, and conflicts with independently accepted glyphs or other clusters; a final reconciliation pass also catches conflicts introduced by later cluster fallbacks;
- cluster commit is atomic: all members must build, compose, pass silhouette checks, and pass final ground/river/modifier/slope/grade validation; otherwise every available member is regenerated from its independent fallback;
- failed triplets never reuse a triplet fragment as a pair. A cluster either survives in full or returns to independent marks.

The projected diagnostics now report requested/accepted/fallback clusters, final complete pairs/triplets, and fallback reasons for incomplete membership, prototype failure, contact/silhouette rejection, final surface rejection, and external-mark conflict. Projection timing now includes a separate `projected cluster composition` aggregate.

**Canonical methods-tried ledger update:**

- E7 render-angle correction and stronger vertical grammar: **partially useful**; retained as input grammar, but not sufficient for contact quality.
- E8 source-space endpoint/junction gate: **rejected as the production contact validator**; it tested straight descriptor skeletons rather than final projected glyphs.
- V3J.4F projected-space atomic composition: **superseded as a complete patch; its projected contact and atomic-fallback architecture is retained by V3J.4F2**.

V3J.4F validation gate:

1. Unity compiles with zero C# and shader errors.
2. `Horizontal Companion Strength = 0` preserves the accepted independent baseline.
3. At maximum companion settings, malformed stars, dangling tapered needles, orphan branches, and partial triplets are absent or materially reduced.
4. `Projected companion clusters requested / accepted / fallback` is reported, and `Projected complete pairs / triplets` counts only clusters present in the final coverage input.
5. Every cluster fallback produces independent marks rather than retaining a composed fragment.
6. External independent marks do not intersect or extend a committed cluster silhouette.
7. Companion changes still execute only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material`.
8. The density-800 legitimate regeneration remains within the accepted dirty-time performance envelope; the new `projected cluster composition` timing is reported separately.

### 2026-07-13 — Historical Patch V3J.4E8: Source-Space Junction Quality and Structured Pair Expansion

**Status:** Superseded by V3J.4F after Unity validation proved that source-space straight-segment contacts did not represent the final projected family silhouette.

Unity validation of E7 proved the render-faithful steep-angle fix and interior contact modes, but exposed two production blockers: some target edges protruded through anchor bodies or formed star/X-like collisions, and dual pairs remained much flatter than the now-structured triplets. E8 addresses both without adding a control or changing population/distribution.

E8 changes dirty-time companion composition only:

- structured target contacts are endpoint-authoritative: the target contact remains within the final 1.5% of its span, while the anchor may contribute an endpoint, shoulder, or middle contact;
- contact separation is applied along the target member's outward axis instead of fixed visual horizontal;
- interior-anchor contacts receive essentially no deliberate penetration, while endpoint-to-endpoint contacts retain only small taper compensation;
- a final junction gate rejects intersections away from the intended join, acute interior contacts, deep free-end intrusion, multiple triplet joins collapsing onto one anchor point, and unintended non-adjacent segment intersections;
- `Triplet Verticality` now also controls a weaker structured-pair grammar: at maximum value, up to 64% of pair requests may use stepped continuation, shoulder contact, or offset echo, while the remaining pair requests retain flat continuation;
- structured pairs may reach 22–58-degree shape angles with a 64-degree hard limit and must prove a visible centre step, vertical span, and steep member before acceptance;
- triplet prevalence, E7 triplet angle limits, participant budget, Tightness, fixed population, three-member cap, anti-chain envelopes, physical validation, projection, R8 coverage, and G3 performance work are unchanged.

V3J.4E8 validation gate:

1. Unity compiles with zero C# and shader errors.
2. At Strength/Tightness/Verticality `1 / 1 / 1`, the prior star/X and protruding-stub examples are absent or materially rarer; contacts occur at a clean endpoint against an endpoint/shoulder/interior point.
3. Structured pairs are clearly present at maximum Verticality, but flat same-line pairs remain visible as a substantial minority.
4. Triplets still show strong E7 vertical structure and failed contact-quality cases downgrade rather than survive.
5. No cluster exceeds three members, separate clusters do not concatenate into four-or-more-mark chains, and Strength `0` remains count/coverage equivalent to V3J.4D3.
6. Companion changes execute only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material` and remain within the closed performance budget.

### 2026-07-13 — Historical Patch V3J.4E7: Render-Faithful Triplet Junctions

**Status:** Superseded by V3J.4E8 above after Unity validation exposed contact intrusions and underdeveloped pair grammar.

Unity validation of V3J.4E6 exposed a concrete solver/render mismatch. Structured triplet targets and the non-linearity gate were calculated from planned 30–62 degree angles, but final companion orientation was clamped back to ordinary `Angle Jitter Degrees` (18 degrees in the validation profile). The direct triplet offset also remained only 0.18 × Stroke Width, approximately 0.18 coverage texels in the measured setup. E6 therefore validated hypothetical steep geometry while baking shallow end-to-end runs.

V3J.4E7 makes one geometry contract authoritative from target solve through rendering:

- companion target angles are final angles; structured triplets may exceed ordinary Angle Jitter under the explicit `Triplet Verticality` control, with an 84-degree hard safety bound;
- maximum verticality raises the structured range to approximately 48–82 degrees and increases the Strength-eligible triplet probability from 56% to at most 72%;
- the flat-triplet exception falls from 12% at minimum verticality to approximately 1.5% at maximum;
- structured contacts are no longer endpoint-only: stepped, crown, and broken-terrace templates use deterministic endpoint-to-shoulder and endpoint-to-middle junctions while retaining a maximum of three independent descriptors;
- maximum Tightness permits controlled contact penetration so tapered endpoints visibly touch an interior portion of another mark instead of leaving a false gap;
- structured acceptance measures the final six segment endpoints, final member inclinations, centre non-linearity, total vertical span, and two layout-appropriate vertical steps; a failure downgrades to a pair.

The hard invariants remain fixed descriptor population, no connectors or recursive graph, three-member maximum, primary-centre preservation, cluster-to-cluster anti-chain envelopes, independent physical validation, `Horizontal Companion Strength = 0` compatibility, and the closed G3 performance architecture.

V3J.4E7 validation gate:

1. At Strength/Tightness/Verticality `1 / 1 / 1`, planned and rendered member angles match; no later Angle Jitter clamp flattens structured triplets.
2. Nearly all retained triplets show at least two clear vertical steps or a crown/junction structure; near-horizontal triplets are exceptional.
3. End-to-shoulder and end-to-middle contacts visibly touch or overlap slightly at maximum Tightness without creating four-or-more-mark chains.
4. Strength `0` remains byte/count/coverage equivalent to the accepted V3J.4D3 baseline.

# Ground Generation Surface Upgrade Plan

### 2026-07-13 — Historical Patch V3J.4E6: Guaranteed Triplet Verticality

**Status:** Superseded by V3J.4E7 and retained as the first explicit verticality-control experiment.

Unity validation of V3J.4E5 showed that angle progression alone did not guarantee visible stepping: most three-mark clusters could still resolve into a single shallow line. V3J.4E6 adds one explicit authoring control, `Triplet Verticality`, and makes structured-triplet acceptance depend on measurable geometry rather than template intent.

- `Triplet Verticality = 0` permits subtle, nearly linear structured triplets.
- `Triplet Verticality = 1` raises the structured-member angle range to approximately 30–62 degrees, with a hard member limit of 68 degrees.
- every non-flat triplet must clear both a perpendicular non-collinearity threshold and a visual-vertical-span threshold derived from Stroke Width; otherwise the requested triplet falls back to an ordinary pair;
- the existing 8% flat-triplet exception remains legal; therefore the system guarantees that retained structured triplets are visibly non-linear without banning every horizontal triplet;
- endpoint-local contact offsets, Tightness, fixed population, the three-member cap, anti-chain envelopes, physical validation, projection, and R8 coverage remain unchanged;
- `Horizontal Companion Strength = 0` remains the exact pre-composition compatibility state.

V3J.4E6 validation gate:

1. Existing assets initialize `Triplet Verticality` to `1.0`, and both Ground authoring Inspectors expose the control under Horizontal Companions.
2. At Strength/Tightness/Triplet Verticality `1 / 1 / 1`, at least 80–90% of visible triplets clearly depart from a straight line; the only intentional flat cases are the bounded flat-template minority.
3. Structured triplets that cannot meet the visible threshold become pairs rather than leaking shallow three-mark lines.
4. Near-contact spacing, maximum-strength population, anti-chain behavior, performance, and Strength-zero equivalence do not regress.


### 2026-07-13 — Patch V3J.4E5: Vertically Structured Triplet Grammar

**Status:** Superseded by V3J.4E6 above. Retained as the angle-progression experiment that did not guarantee visible non-linearity.

Unity validation of V3J.4E4 confirmed that the stronger two/three-mark population, near-contact spacing, and hard cluster cap work, but triplets still too often read as three glyphs assembled on one horizontal ruler. The final approved shape correction keeps pairs unchanged and makes vertical structure the normal triplet grammar.

V3J.4E5 changes only dirty-time triplet composition:

- 44% of triplets use a stepped run, 28% use a crown/arch run, 20% use a broken-terrace run, and only 8% retain the flatter continuation grammar;
- structured triplets are always placed sequentially from primary to secondary to tertiary, so the three marks describe one bounded run rather than two unrelated marks flanking a centre;
- vertical structure comes primarily from larger member tangent changes within the authored `Angle Jitter Degrees` limit, not from visibly disconnected centre offsets;
- stepped runs use a near-horizontal member, a strong rise/fall, and a shallower elevated continuation;
- crown runs rise, level near the crown, then fall;
- broken terraces use one sharp rise/fall and one subordinate continuation;
- endpoint-local vertical offsets are limited to a small fraction of Stroke Width, preserving the E4 touching or approximately one-to-two-pixel-gap target at maximum Tightness;
- pairs, maximum Strength, triplet frequency, fixed population, anti-chain envelopes, physical validation, projection, and R8 coverage remain unchanged.

The hard shape invariant remains a maximum of three independent marks. No connector, shared node, recursive extension, graph record, runtime object, new control, or additional stroke is introduced. `Horizontal Companion Strength = 0` still exits before candidate mutation and remains the exact V3J.4D3 compatibility state.

V3J.4E5 validation gate:

1. Unity compiles with zero C# and shader errors.
2. Strength `0` preserves the accepted independent-stroke counts and coverage output.
3. Pair appearance and counts remain consistent with V3J.4E4.
4. At maximum Strength/Tightness, most triplets contain a clear rise, fall, crown, or broken-terrace component rather than three vertically synchronized marks.
5. Flat triplets remain visible only as a minority exception.
6. Adjacent members still touch or leave only an approximately one-to-two-pixel break.
7. No cluster exceeds three intended members and separate clusters do not concatenate into a four-or-more-mark line.
8. Companion changes still execute only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material` and remain within the closed performance budget.

### 2026-07-13 — Patch V3J.4E4: Bounded Two/Three-Mark Companion Clusters

**Status:** Superseded by V3J.4E5 above. Retained as the stronger two/three-mark near-contact experiment whose triplets remained too horizontally synchronized.

V3J.4E3 removed accidental multi-pair chunk-wide chains, but Unity validation showed three remaining production blockers: paired members still aligned too exactly on one horizontal row, maximum Tightness left gaps that read as disconnected, and maximum Strength did not compose enough of the existing population. The accepted next experiment also expands the bounded motif from exactly two marks to either two or three marks.

V3J.4E4 retains the fixed-population, mesh-free architecture and the existing two controls. It changes only the dirty-time surface-stroke composition grammar:

- `Horizontal Companion Strength = 1` may compose at most 68% of post-thinning descriptors, up from 44%;
- cluster size is deterministically two or three, with triplet probability rising to 56% at maximum strength;
- the primary remains at its original centre and all companions still consume existing descriptors rather than adding strokes;
- cluster members follow one broad visual-horizontal trend but receive independent angle drift and non-zero vertical staggering, removing ruler-straight vertical synchronization;
- target centres are solved from the actual oriented endpoints of adjacent marks rather than centre-distance length estimates;
- the gap solver now compensates for the authored longitudinal End Taper before placing adjacent members;
- at maximum Tightness, continuation endpoints use a small controlled centreline overlap plus near-zero local endpoint stagger so the rendered marks touch or retain only an approximately one-to-two-pixel break, while independent tangent drift keeps their full centres off a ruler-straight row;
- Continuation triplets place companions on opposite sides of the primary, Staggered Echo may form a three-member stepped run, and Offset Shoulder uses an asymmetric split;
- one expanded cluster envelope and one deterministic composition cell remain reserved per motif, so a neighbouring cluster cannot concatenate into an open-ended chain;
- diagnostics now report total clusters, pair/triplet counts, composed/accepted participants, and fully accepted clusters.

The hard upper bound remains three independent marks. V3J.4E4 does not add connectors, shared nodes, graph traversal, recursive chaining, proposals, runtime objects, or a new Inspector control. Every member independently passes the existing ground, slope, river, modifier, and local-grade validation. `Horizontal Companion Strength = 0` still exits before candidate mutation and remains the exact V3J.4D3 compatibility state.

V3J.4E4 validation gate:

1. Unity compiles with zero C# and shader errors.
2. Strength `0` preserves the accepted independent-stroke counts and coverage output.
3. Strength/Tightness `1` produces clearly more companion participants than V3J.4E3 while remaining inside the fixed descriptor population.
4. Diagnostics contain both pair and triplet clusters; no cluster contains more than three intended members.
5. Members no longer sit on a repeated exact horizontal ruler line; centres and tangents show controlled local stagger.
6. At maximum Tightness, adjacent marks touch or leave only a visually tiny one-to-two-pixel break.
7. Separate clusters do not concatenate into a four-or-more-mark chunk-wide line.
8. Companion changes still execute only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material` and remain within the closed performance budget.

### 2026-07-13 — Patch V3J.4E3: Isolated Two-Mark Companion Motifs

**Status:** Superseded by V3J.4E4 above. Retained as the record of primary-fixed two-mark isolation and its insufficient strength/alignment result.

V3J.4E2 proved that dominant/subordinate pairing and same-facing profiles reduced the repeated bird/moustache symbol, but validation exposed two remaining failures: the effect was too weak at maximum strength, and multiple independent pairs could concatenate into accidental chunk-wide broken lines. The latter recreated the rejected network/terrace reading through coincidental pair alignment.

V3J.4E3 changes the composition algorithm rather than adding another tuning control:

- the primary descriptor keeps its original centre, role, length, width, and ordinary regional relationship;
- an intended companion target is solved from the primary before selecting a secondary descriptor;
- the secondary is selected by proximity to that target and may move only within a bounded relocation radius;
- every pair reserves a conservative horizontal corridor envelope, and any later pair whose envelope intersects it is rejected;
- a pair is also rejected when any third descriptor already occupies its expanded corridor, creating a hard two-mark anti-chain rule at composition time;
- one pair may occupy each deterministic companion-composition cell;
- companion visibility is restored to 72–95% of primary length and 90–100% strength;
- maximum participant fraction is 44%, but spatial quotas and anti-chain rejection determine the actual accepted population;
- continuation, staggered echo, and offset shoulder remain internal bounded arrangements;
- no descriptor, connector, node, graph, or runtime object is added.

`Horizontal Companion Strength = 0` remains a strict V3J.4D3 compatibility state. The pass exits before mutating any candidate. The existing two controls and all G3 projection/coverage/performance architecture remain unchanged.

V3J.4E3 validation gate:

1. Strength zero reproduces the accepted baseline counts and coverage.
2. Maximum strength produces clearly readable two-mark motifs rather than a barely visible population change.
3. No visible horizontal motif contains three or more marks, including concatenated independent pairs.
4. The primary remains at its original placement; only the selected secondary relocates.
5. Companion changes execute only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material`.
6. Density-800 regeneration remains within the closed Ground performance budget.

### 2026-07-13 — Patch V3J.4E2: Horizontal Companion Productionization Pass

**Status:** Superseded by V3J.4E3 above. Retained as the record of the hierarchy/same-facing experiment and its accidental multi-pair chain failure.

The validated V3J.4E evidence at density 800 was:

```text
strength / tightness:                    1.00 / 1.00
pairs / participants composed:           97 / 194
full pairs / participants accepted:      81 / 163
total regeneration:                      375.05 ms
```

The performance result remained effectively identical to the closed G3 baseline, so V3J.4E2 changes only pair grammar. It adds no control, proposal, stroke, connector, topology record, runtime object, or new validation path.

The active two controls remain:

```text
Horizontal Companion Strength
Companion Tightness
```

V3J.4E2 changes their internal interpretation as follows:

1. **Dominant/subordinate hierarchy.** The original primary retains its ordinary length and strength. Its companion is deterministically reduced to 55–85% of the primary length and 82–94% of its ordinary strength. Width remains controlled by the existing global Stroke Width path.
2. **Same-facing profile alignment.** Asymmetric, Shoulder, and Shallow companion profiles reuse their existing family formulas but choose a deterministic profile seed with the same mirror/facing sign as the primary whenever that family has a directional sign. Complete Mound remains symmetric.
3. **Same-side angle relationship.** The primary keeps one shared authored angle offset. The companion receives only a small additional offset on the same side of zero, rather than the former symmetric `-difference / +difference` split that encouraged mirrored wings.
4. **Three bounded arrangements.** Existing pair members are placed as approximately 58% Continuation, 30% Staggered Echo, and 12% Offset Shoulder. These are descriptor-placement templates only; both marks remain independent and unconnected.
5. **No routine negative gap.** Continuation pairs retain a positive authored-space break even at maximum Tightness. Staggered and shoulder arrangements may overlap in horizontal projection only when their vertical separation keeps them visibly independent.
6. **Reduced and nonlinear saturation.** Maximum participation falls from 50% to 38% of post-thinning survivors. Strength uses a `1.35` exponent, giving occasional composition near `0.35`, clearly present composition near `0.65`, and the bounded maximum only at `1.0`.

The pairing pass remains population-neutral and executes after regional thinning and ordinary role/family/geometry resolution. Each repositioned member still passes the unchanged ground, river, modifier, slope, and grade checks independently. Strength zero remains an exact V3J.4D3 compatibility path because the composition pass exits before mutating any candidate.

V3J.4E2 validation gate:

1. Unity compiles with no C# or shader errors.
2. Strength `0` reproduces the existing V3J.4D3/G3 counts and coverage exactly.
3. Test Strength `0.35`, `0.65`, and `1.0` at Tightness `0.65` in the production view without debug overlays.
4. At density 800 and Strength `1`, composed participants never exceed 38% of post-thinning survivors; with the previously observed 389 survivors this means no more than 147 composed participants before physical rejection.
5. The field no longer presents a dominant repeated bird/moustache motif. Most pairs read as one longer broken contour with one shorter subordinate mark.
6. Continuation pairs retain a visible break at Tightness `1`; there is no routine direct overlap.
7. Disabled family weights remain authoritative, and each member remains independently rejectable near rivers/modifiers.
8. Strength/Tightness changes still execute only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material`.
9. No-op, material-only, and G3 timing behavior remain unchanged within normal measurement noise.
10. Do not add further controls until the three prescribed strength levels have been compared.


### 2026-07-13 — Patch V3J.4E1: GeneratedGround Inspector Authoring Repair

**Status:** Implemented for Unity validation. No production generation or rendering semantics changed.

The initial V3J.4E delivery added the companion recipe fields and exposed them in the style-profile asset editor, but failed to wire those fields into the primary `GeneratedGround` Inspector. As a result, `Horizontal Companion Strength` and `Companion Tightness` were absent from the normal scene-authoring workflow even though generation supported them.

V3J.4E1 corrects the primary Inspector and reorganizes it around collapsed foldouts. The active Painted Accent authoring groups are now:

```text
Stroke Basics
Broad Distribution
Regional Composition
Horizontal Companions
Glyph Family Mix
Stroke Geometry
Projected Contour Profile
Authored Ink
```

`Horizontal Companions` is open by default inside the open Painted Accent section so the new controls are immediately visible. `Companion Tightness` remains disabled while strength is zero because it has no signature or generation effect in that state.

The formerly mandatory placement-debug controls and large diagnostic text blocks are moved under:

```text
Painted Accent Placement Debug
  Placement and Composition Overlays
  Projected Shape Overlay
  Last Generation Diagnostics
```

The debug section and diagnostics are collapsed by default. Ground Debug, Generation, Patch, Mountain Transition, Ground Shape, Surface, Modifiers, Advanced, surface-mask diagnostics, resolved-feature summaries, and each advanced material-control family are also foldouts. Foldout interaction is editor-only and must not dirty style data or trigger regeneration.

Validation gate:

1. Unity compiles with no C# errors.
2. The primary GeneratedGround Inspector displays `Horizontal Companion Strength` and `Companion Tightness` under `Painted Accent Strokes → Horizontal Companions`.
3. Opening or closing any foldout does not regenerate Ground or alter style assets.
4. Strength changes still execute only `SurfaceStrokes, ProjectedGlyphs, Coverage, Material`.
5. Tightness is disabled at strength zero and becomes editable above zero.
6. Large timing, placement, and coverage text appears only when `Last Generation Diagnostics` is expanded.
7. No production counts, signatures, defaults, family formulas, validation rules, or coverage output change from V3J.4E.


### 2026-07-13 — Patch V3J.4E: Bounded Horizontal Companion Composition

**Status:** Directional proof validated; superseded for production testing by V3J.4E2 above.

V3J.4E is the first post-performance visual composition experiment. It deliberately composes a bounded portion of the existing post-thinning Painted Accent population into independent two-mark arrangements along fixed visual horizontal. It does not create connectors, shared vertices, graph topology, new runtime objects, a separate network cache, or additional stroke proposals.

New authoring controls:

```text
Horizontal Companion Strength
  0 = exact V3J.4D3 independent-placement baseline
  1 = approximately half of surviving candidates may participate in pairs

Companion Tightness
  0 = broader end-to-end gaps and modest vertical offset
  1 = close end-to-end placement with rare slight overlap
```

The serialized default for `Horizontal Companion Strength` is `0`, so existing family/style assets preserve the validated V3J.4D3 field until the experiment is explicitly enabled. `Companion Tightness` defaults to `0.65` but has no generation effect while strength is zero.

Composition occurs after regional thinning and existing role/length/width/family resolution, but before physical stroke validation. Pair selection is deterministic, region-local, and population-neutral:

```text
regional survivors
→ deterministic bounded pair count
→ nearest available partner in the same composition region
→ two independent repositioned candidates
→ ordinary independent ground/river/modifier/slope/grade validation
→ ordinary projected glyphs and R8 coverage
```

Each pair uses the local projection of fixed world horizontal. Members are placed end-to-end with a small deterministic vertical offset and a restrained angle difference that remains inside the authored `Angle Jitter Degrees` absolute bound. Tightness controls the gap and offset; only the highest tightness range permits a low-probability slight overlap.

Family composition remains authored-weight-aware. The first member retains its ordinary selected family. The second member is selected from the same non-zero authored family weights with compatibility bias toward the approved combinations:

```text
Single Shoulder + Shallow Crest
Asymmetric Mound + Single Shoulder
Shallow Crest + Shallow Crest
Complete Mound + Single Shoulder
```

`Complete Mound + Complete Mound` is de-emphasized but remains a defensive fallback when authored family isolation leaves no compatible alternative. This preserves family-preview and zero-weight contracts.

Generation diagnostics now report:

```text
companion strength / tightness
pairs / participants composed
full pairs / participants accepted
```

A full accepted pair means both independently validated members survived. A one-member survivor remains a valid independent stroke; the system never bypasses physical validation to preserve a pair.

V3J.4E validation gate:

1. Unity compiles with no C# or shader errors.
2. At `Horizontal Companion Strength = 0`, all V3J.4D3 proposal, family, rejection, descriptor, projected-glyph, segment, and coverage results remain unchanged.
3. Raising only Horizontal Companion Strength executes `SurfaceStrokes, ProjectedGlyphs, Coverage, Material` and does not rebuild ground geometry, mesh, collider, or corridor ownership.
4. At strength `1`, diagnostics show a substantial but bounded paired population, never more than half of surviving candidates.
5. Pair members remain visibly separate marks with no connector or shared topology.
6. `Companion Tightness = 0` produces readable gaps; `1` produces close end-to-end pairs with only rare slight overlap.
7. River, modifier, slope, and grade rejection remain independently authoritative for each member.
8. Family-isolation tests do not introduce a family whose authored weight is zero.
9. No-op and material-only G2/G3 invalidation behavior remains unchanged.
10. The mixed production field is judged without debug overlays before deciding whether the companion fraction, gap range, or family compatibility needs a follow-up tuning patch.


### 2026-07-13 — Ground Performance G3: Exact Projected-Footprint Validation Optimization

**Status:** Unity-validated and closed. Applies after the Unity-validated G2 stage-invalidation baseline.

The decisive density-700 shape-only measurement was:

```text
projection total:              625.05 ms
surface/domain validation:    548.14 ms
coverage raster/upload:        26.46 ms
```

G3 optimizes that proven validation hotspot without changing Painted Accent placement, family construction, profile samples, rejection thresholds, rejection order, river/modifier semantics, or the 2048 × 2048 production coverage ceiling.

The principal correction is a projection-specific ground sampler. Projected validation consumes only height and visible render normal, but the former path constructed a complete `GroundSurfaceSample` independently for centre, left, and right footprint points. That complete contract repeatedly resolved and interpolated height, two normal fields, and all material/surface-mask channels. The new exact footprint sampler resolves one ground triangle per position and interpolates only height and render normal while preserving the former normal-normalization sequence.

The projected-glyph build now also:

```text
reuses one build-local scratch allocation for rejected candidates
precomputes centre/left/right footprint geometry once per glyph
copies permanent point/width arrays only for accepted glyphs
builds conservative per-glyph river and modifier candidate lists
runs the original exact river/modifier tests for every retained candidate
adds segment-AABB rejection before exact self-intersection tests
```

Ground-owned river broad-phase bounds are derived from the same active spline samples used to create the immutable ground snapshot and expanded by the snapshot maximum influence distance. A missing/unsafe bound remains unbounded and therefore cannot suppress an exact test. No river implementation file is modified. Modifier broad-phase bounds conservatively include authored shape and blend distance.

The live first-failure order remains authoritative and unchanged:

```text
sampling
broad slope
river exclusion
modifier exclusion
transverse grade
longitudinal grade
```

The regeneration timing panel now splits surface/domain validation into footprint preparation, ground sampling, broad slope, river exclusion, modifier exclusion, transverse grade, and longitudinal grade. Timing is aggregate only; no per-glyph or per-sample logging is added.

Coverage remains secondary. G3 reuses one exact-size CPU byte buffer and one compatible readable R8 texture, retaining bulk raw upload and one `Apply`. At 2048² this intentionally retains approximately 4 MB for the byte buffer and approximately 4 MB for the readable texture CPU copy to avoid repeated allocation and texture recreation. Coverage bytes, filtering, wrapping, dimensions, and shader contract remain unchanged.

G3 Unity validation passed:

```text
Density-700 shape-only total:        176.46 ms
Projection total:                    149.54 ms
Surface/domain validation:           111.32 ms
Topology + turn:                       6.46 ms
Coverage raster / upload:             26.41 / 0.46 ms

Unchanged regeneration:                0.35 ms
Material-only regeneration:            0.04 ms
Density-800 placement regeneration:   374.25 ms
```

The density-700 shape-only pass improved from `651.60 ms` to `176.46 ms`; surface/domain validation improved from `548.14 ms` to `111.32 ms`. The complete G2 invalidation matrix remained intact: Profile Height executes projection/coverage/material, Stroke Density executes surface strokes/projection/coverage/material, an unchanged request executes snapshots/material only, and Ink Color executes material only. G3 is therefore the closed final measured performance pass. Residual river exclusion cost is retained for correctness and is not a new optimization target without evidence of scaling failure in a future multi-river scene.


### 2026-07-13 — Ground Performance G2: Stage Invalidation and Projection Evidence

**Status:** Unity-validated. Applies after the validated G1 legacy-retirement baseline.

G1 reduced a density-700 full regeneration from more than 30 seconds to approximately `993 ms`. The measured remaining cost was dominated by projected-glyph generation (`724 ms`), followed by surface-stroke generation (`166 ms`). The 2048 × 2048 production coverage bake was already modest (`30.74 ms` raster, `1.53 ms` upload).

G2 keeps all visual and generation algorithms intact while making regeneration stage-aware:

```text
ground geometry signature unchanged
  skip GroundGenerator.Generate
  skip mesh upload
  skip MeshCollider recook
  skip corridor notification

surface-stroke inputs unchanged
  reuse accepted descriptors

projected-shape inputs unchanged
  reuse projected glyphs

coverage inputs unchanged
  reuse production coverage

material/debug change
  apply material properties only
```

GeneratedGround now maintains separate geometry, surface-stroke, projected-glyph, and coverage signatures. Missing outputs still force the required stage after domain restoration. The ground-side river notification path rebuilds current snapshots first; an identical editor river state therefore does not force a second geometry/projection/coverage pass. No river implementation file is modified. At runtime, explicit river notifications retain conservative invalidation.

Shape-only inputs (`Profile Height`, `Crest Crown Height`, `Profile Irregularity`, and `End Taper`) no longer contaminate the surface-stroke signature. They invalidate projection and coverage only. Placement inputs continue to invalidate descriptors, projection, and coverage. Ink colour and debug-view changes remain material-only.

The timing summary now reports executed stages and projected-glyph substages:

```text
profile construction
family-profile validation
projected-point construction
topology and turn validation
surface/domain validation
diagnostic accumulation
```

G2 validation gate:

1. A repeated unchanged regeneration reports snapshots/material only and does not run geometry, mesh, collider, descriptors, projection, coverage, or corridor notification.
2. Ink Color and debug-view changes report material only.
3. Profile Height rebuilds projection and coverage, but not surface strokes, ground geometry, mesh, collider, or corridor notification.
4. Stroke Density rebuilds surface strokes, projection, and coverage, but not unchanged ground geometry.
5. A legitimate ground/modifier/river structural change rebuilds all true dependants exactly once.
6. Scene-open and river-enable restoration do not perform a second identical full ground pass.
7. Ground mesh, collider, Painted Accent visuals, coverage dimensions, exclusions, counts, and deterministic results remain unchanged.
8. The projected-glyph substage timings identify the actual G3 optimization target.

G3 remains the final measured optimization pass. It must optimize the proven projection hotspot first; texture/buffer reuse remains a secondary allocation cleanup unless new timings contradict that priority.


### 2026-07-13 — Ground Performance G1: Legacy Field Retirement and Timing Evidence

**Status:** Implemented for Unity validation. This supersedes the earlier documentation-only pause checkpoint for the strictly GeneratedGround performance track.

The Painted Accent production path is now explicitly descriptor-first and coverage-only:

```text
GroundPaintedAccentSurfaceStrokeGenerator
→ accepted ground-following surface strokes
→ projected glyph families
→ generated R8 coverage
→ ordinary ground-albedo composition
```

The former `GroundPaintedAccentFoldFieldGenerator` file is renamed to `GroundPaintedAccentSurfaceStrokeGenerator` while preserving its `.meta` GUID. The legacy 256 × 256 RGBA fold field is deleted completely, including its all-pixel/all-stroke/all-segment rasterizer, generated texture, neutral texture, material properties, shader sampling, relief/signed-relief/final-prototype debug modes, and stale runtime fallback. Historical sections below may retain the former filename only as a record of the retired experiments.

Exact all-pairs nearest-stroke-distance statistics are also removed from ordinary descriptor generation. They never affected placement, family selection, physical validation, projection, coverage, or rendering and were not retained as a hidden approximation.

The obsolete Painted Accent scale, contrast, mask-influence, direction, seed, and unused coverage-texel-size shader properties are removed as well; only the active strength, coverage mapping, and Ink Color contract remains.

The surviving shader contract is:

```text
Ground Painted Accent Lines debug mode = production R8 coverage
normal render = production R8 coverage blended with Ink Color
minor smoothness/specular response = the same production coverage
no legacy relief body, signed side, or prototype field
```

GeneratedGround now records Profiler markers and one cached inspector summary for:

```text
total regeneration
snapshot collection
ground generation
mesh apply
collider cook
Painted Accent surface-stroke generation
Painted Accent projected-glyph generation
coverage CPU raster
coverage texture upload
material application
river-corridor notification invoked by GeneratedGround
```

This patch does not coalesce lifecycle requests, change river code, gate mesh/collider work, reuse coverage buffers, reduce density, lower coverage resolution, or alter accepted glyph-generation semantics. Those belong to the following strictly GeneratedGround passes:

```text
G2 — stage invalidation, mesh/collider gating, and material separation
G3 — coverage texture/buffer reuse and measured performance closure
```

G1 validation gate at Stroke Density 500:

1. Unity compiles with the renamed generator and no legacy shader property/debug-mode references.
2. Accepted stroke count, family counts, projected glyphs, exclusions, and normal Painted Accent appearance match the V3J.4D3 baseline.
3. No legacy RGBA fold texture is allocated or rasterized.
4. No exact nearest-stroke-distance sweep runs during regeneration.
5. The inspector timing summary identifies the actual remaining dominant stage.
6. V3J.4E companion composition remains paused until G2 and G3 are validated.

### 2026-07-13 — Painted Accent Checkpoint and Ground-Lifecycle Performance Handoff

**Status:** Historical handoff checkpoint. Superseded for active performance work by Ground Performance G1 above.

The current Painted Accent production path is accepted as the working visual baseline:

```text
independent placement descriptors
→ regional density/direction composition
→ four single-stroke glyph families
→ final-profile sanity and signed-angle decorrelation
→ projected mesh-free glyphs
→ generated R8 coverage
→ ordinary ground-albedo composition
```

Unity evidence after V3J.4D3 shows that the individual marks are not perfect but are sufficiently stable to stop shape-system iteration. Remaining rare compound-looking shapes are most interesting when two independent marks happen to sit roughly end-to-end or slightly overlap. The next visual experiment remains V3J.4E Implied Horizontal Companions: deliberately compose a bounded portion of the existing stroke population into independent two-mark arrangements without connectors, shared nodes, graph topology, or a separate network representation.

V3J.4E is paused because an independent editor-performance audit found a structurally duplicated GeneratedGround lifecycle path. `GeneratedGround.OnEnable` performs a complete regeneration, while `StylizedRiver.OnEnable → RegenerateAll → NotifyParentGround → Ground.NotifyRiverChanged` can request another complete ground regeneration during the same restoration sequence. A complete pass may regenerate the mesh, recook the collider, rebuild Painted Accent placement/projection, rebake the current `2048 × 2048` coverage texture, and rebuild the river corridor. This is a proven duplicate path and a credible contributor to editor stalls, although it is not proven to be the dominant source of the recorded 80–96 second freezes.

Performance remediation is explicitly delegated to a separate performance thread. This Painted Accent thread must not implement lifecycle coalescing, modify river files, or expand beyond the GeneratedGround visual feature scope. The live worktree reportedly contains substantial uncommitted ground work on branch `fufu` at audited HEAD `04dbc13`; any performance implementation must begin from the actual live files, read `AGENTS.md`, and preserve all current Painted Accent changes rather than reconstructing from clean HEAD or an older patch bundle.

Approved performance direction, owned elsewhere:

```text
P1 — editor-lifecycle ownership/coalescing
  merge enable/validate/river requests into one ground processing pass
  preserve ground/river notification semantics
  prevent synchronous feedback-style duplicate regeneration
  add profiler markers and concise request/pass counters

P2 — stage-level invalidation and reuse
  rebuild geometry, collider, fold field, projected glyphs, coverage, material,
  and river corridor only when their actual input signatures change
  do not rebake 2048² coverage for material-only or debug-only changes
  do not recook MeshCollider when generated geometry is unchanged
  reuse generated texture/buffers where safe
```

Non-negotiable performance constraint:

```text
no visual-resolution reduction
no style-default reduction
no generation-semantic change
```

Resume gate for V3J.4E:

1. A separate performance patch is applied and Unity-compiles against the live working tree.
2. Ordinary scene restoration, script reload, Play entry, and Play exit perform no duplicate full-ground pass or repeated identical coverage bake.
3. Legitimate geometry, modifier, river-structural, Painted Accent placement, Painted Accent shape, and material changes still invalidate the correct stages deterministically.
4. Ground mesh, collider, river cutout/corridor, Painted Accent coverage, family distribution, and approved visuals match the pre-performance baseline.
5. Only then resume Horizontal Companion Composition in the GeneratedGround/Painted Accent scope.

### 2026-07-13 — Patch V3J.4D3: Shape Sanity and Orientation Decorrelation

**Status:** Unity-validated as good enough; active baseline pending lifecycle-performance work.

Unity validation of V3J.4D2 confirmed that the family system is broadly usable, but exposed three residual defects: occasional sharp projected shoulders, rare double-peaked “cat-ear” silhouettes with a lower middle, and composition regions whose accepted marks too often shared the same signed tilt. V3J.4D3 is a contained final-sanity pass before implied horizontal companions. It does not change coverage, width, density, family weights, authored length bounds, or companion topology.

Final-profile sanity contract:

```text
all non-flat accepted families
  no significant interior valley between two separated higher sections
  minimum significant valley depth = max(0.001 m, 8% of profile peak)

Single Shoulder and ordinary Shallow Crest
  primary 5%-to-95% height transition spans at least 18% of the mark
```

The interior-valley audit operates on the final densely sampled combined profile, including crown response. It rejects the observed two-edge-peak / lower-middle silhouette across every family, including Complete Mound. Small sub-millimetre or sub-8% detail variation is ignored so ordinary organic irregularity does not become a false failure.

Family-specific final projected-turn limits replace the permissive shared `42°` limit:

```text
Complete Mound:     <= 32°
Asymmetric Mound:   <= 30°
Single Shoulder:    <= 27°
Shallow Crest:      <= 25°
```

Single Shoulder keeps a slightly longer high run and reduced plateau droop. Ordinary Shallow Crest shoulders now transition over approximately 34–44% of the span rather than compressing the bend into the first 12–22%. The existing `SharpTurn` rejection remains the evidence channel for failed final XZ geometry.

Orientation is restored to the authored signed-jitter contract:

```text
final offset remains inside [-Angle Jitter Degrees, +Angle Jitter Degrees]
regional mean contribution <= min(10°, 25% of authored jitter)
per-mark magnitude = 35–100% of authored jitter
accepted positive and negative signs are balanced per region as marks survive validation
```

The balancing is deterministic and acceptance-aware. Each region assigns the currently under-represented sign to the next candidate; failed physical candidates do not count, so later candidates continue trying to restore balance. A one-mark region remains deterministically random. This preserves broad regional relationship without allowing large map areas to lean almost entirely positive or negative. Existing placement diagnostics remain authoritative: accepted angle min/max should straddle zero in populated fields, while the mean should remain near zero rather than following the regional sign.

Modified files:

```text
Assets/Game/Procedural/Ground/GroundPaintedAccentFoldFieldGenerator.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentLongitudinalProfileGenerator.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentProjectedGlyphGenerator.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
```

Validation gate:

1. Unity compiles and regenerates placement, projected-glyph, and coverage caches.
2. Close inspection shows no cat-ear profile with a significant lower middle between two peaks.
3. Sharp shoulder examples are rejected or remain below their family-specific final-turn limit.
4. Single Shoulder and Shallow Crest transitions read as smooth bends rather than compressed elbows.
5. In any region with several accepted marks, positive and negative deviations both appear; large same-sign groups are no longer common.
6. Accepted angle min/max remain within the authored Angle Jitter bounds and normally straddle zero; the mean remains near zero.
7. Family identity, family weights, strict Stroke Length Min / Max, width, density, regional concentration, coverage, and all physical exclusions remain unchanged.
8. V3J.4E begins only after these independent-stroke sanity checks pass.

### 2026-07-13 — Patch V3J.4D2: Baseline Smoothness and Family Readability

**Status:** Unity-validated as a useful smoothness/readability pass; residual extrema and orientation defects are corrected by V3J.4D3.

Unity validation of V3J.4D1 showed that the family vocabulary is usable, but it also exposed two shared readability defects. Several families could acquire sharp angular shoulders because every `0.40–0.45 m` source stroke was stored with only five baseline points, and most Shallow Crests remained visually indistinguishable from straight dashes because their normalized shoulder could resolve to only one or two millimetres in world space.

V3J.4D2 is a corrective pass, not another family redesign.

Shared baseline contract:

```text
analytic deterministic centreline
→ spacing-driven source storage at approximately 0.03 m
→ 13–33 stored source points
→ existing 65–97 dense projected-profile samples
```

The old `ceil(length × 1.85)` source count is removed. A `0.40–0.45 m` mark now stores approximately 15–18 source points rather than five, including a small allowance for curved-path arc length. The projected generator measures the maximum final XZ turn angle; accepted glyphs report min/mean/max turn evidence, and only residual turns above `42°` are rejected as `SharpTurn` rather than allowing a severe elbow into coverage.

New authoring control:

```text
Stroke Path Wiggle: 0–1, default 0.35
```

This controls smooth lateral curvature of the source ground path only. It is independent from Profile Irregularity, generic feature Contrast, family selection, Profile Height, Stroke Length, and Stroke Width. `0` approaches a straight source baseline; `1` permits the strongest bounded non-looping bend. The control is included in placement-cache invalidation and is exposed in both Painted Accent authoring inspectors.

Shallow Crest correction:

```text
ordinary shallow shoulder: approximately 96%
rare near-straight quiet mark: approximately 4%
minimum ordinary endpoint displacement: 0.0035 m in projected world scale
ordinary normalized endpoint difference: >= 0.50
ordinary broad plateau fraction: >= 0.60
```

The ordinary variant now uses a deeper low endpoint, a quintic-smooth transition into the upper run, and a slightly larger but still low family height scale. Near-straight variants remain valid but are intentionally rare. The final sampled world-space endpoint difference is recorded in diagnostics and is authoritative; a mathematically shouldered profile that remains visually too small is rejected rather than counted as a successful Shallow Crest.

Single Shoulder transition regions also use quintic smootherstep interpolation so the high run and sustained descent join without a visible derivative corner. Complete Mound grammar, Asymmetric identity thresholds, family weights, density, regional distribution, strict Stroke Length Min / Max, width, R8 coverage, and every physical exclusion remain unchanged.

Modified files:

```text
Assets/Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentFoldFieldGenerator.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentLongitudinalProfileGenerator.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentProjectedGlyphGenerator.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
```

Validation gate:

1. Unity compiles and regenerates the placement, projected-glyph, and coverage caches.
2. Stroke Path Wiggle is present in both authoring inspectors and produces a clear smooth response between `0`, the default, and `1`.
3. Close inspection shows no five-point quarter-span elbows across any family.
4. Projected-turn diagnostics remain below `42°` for accepted glyphs; severe residual turns appear only in the dedicated rejection count.
5. Ordinary Shallow Crests show a visible small shoulder, and near-straight marks are only a small minority.
6. Shallow world endpoint-difference diagnostics remain at or above `0.0035 m` for ordinary accepted variants.
7. Complete, Asymmetric, and Single Shoulder family identity remains intact.
8. Stroke Length Min / Max, width, density, regional distribution, family weights, coverage, and physical exclusions remain unchanged.
9. V3J.4E does not begin until the corrected independent strokes are accepted.

### 2026-07-12 — Patch V3J.4D1: Perceptual Family Separation

**Status:** Unity-validated as a useful separation pass; source-baseline angularity and Shallow-Crest visibility are corrected by V3J.4D2.

Unity validation of V3J.4D confirmed that the four-family approach is useful but that three new families overlap too much perceptually. Some Asymmetric Mounds still resemble ordinary centred mounds, some Single Shoulders still resemble Asymmetric Mounds, and some Shallow Crests collapse into plain straight dashes. V3J.4D1 corrects those family identities without changing family weights, placement, density, coverage, authored length, authored width, shaders, or physical exclusions.

Final-curve family contracts:

```text
Asymmetric Mound:
  crest at approximately 19–28% or 72–81% of span
  long-side / short-side span ratio >= 2.0
  steep-leg / shallow-leg average slope ratio >= 1.5
  both endpoints descend meaningfully from the crest

Single Shoulder:
  peak remains at the high endpoint band
  high run occupies approximately 30–55% of final span
  high endpoint loses no more than 20% of peak height
  one sustained descent loses at least 65% of peak height
  no reverse rise beyond numerical tolerance

Shallow Crest:
  approximately 88% use a one-sided shallow-shoulder form
  shoulder form has a broad plateau and a measurable endpoint step
  approximately 12% remain deliberately near-straight quiet marks
  intermediate accidental almost-straight forms are rejected
```

Validation is measured from the final densely sampled profile rather than only from seeded construction parameters. Diagnostics report Asymmetric crest position, leg-span ratio, and leg-slope ratio; Shoulder upper-run, upper-end-drop, and descending-drop fractions; and Shallow plateau, vertical-range, endpoint-difference, and near-straight counts. The projected-glyph revision is included in the cache signature so the corrected families rebuild automatically.

V3J.4D1 does not implement companion placement. Unity screenshots revealed promising larger implied shapes when two independent marks happened to align approximately horizontally. That evidence is recorded as the next experiment rather than folded into this family correction.

Modified files:

```text
Assets/Game/Procedural/Ground/GroundPaintedAccentLongitudinalProfileGenerator.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentProjectedGlyphGenerator.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
```

Validation gate:

1. Unity compiles with no C# errors and regenerates the projected coverage automatically.
2. Complete Mound remains visually unchanged.
3. Isolated Asymmetric Mounds all show unmistakably unequal leg spans and slopes.
4. Isolated Single Shoulders show a sustained high run and one descent, never a second mound leg.
5. Isolated Shallow Crests normally show a small one-sided shoulder; near-straight marks remain rare.
6. Projected diagnostics remain inside every documented final-curve threshold.
7. Stroke Length Min / Max, Stroke Width, family weights, density, regional distribution, coverage, and physical exclusions remain unchanged.
8. The mixed field is judged without debug overlays before V3J.4E begins.

### Planned V3J.4E — Horizontal Companion Composition

**Status:** Historical design candidate. Implemented by the active V3J.4E section at the top of this document.

The next composition experiment should encourage implied connectivity without returning to literal graph topology. A controlled fraction of the existing proposal budget may form pairs of independent strokes arranged approximately along fixed visual horizontal, with a small gap or rare slight overlap, modest vertical offset, restrained orientation difference, and family-aware pair preferences. The strokes remain separate descriptors, separate projected glyphs, and separate R8 coverage marks.

Authoring premise:

```text
Horizontal Companion Strength = 0
  all marks remain independently placed

Horizontal Companion Strength = 1
  a substantial but bounded fraction of the existing population participates
  in two-mark horizontal compositions
```

The control must redistribute the existing requested population rather than add unbounded extra marks. Preferred first-proof pairings are Shoulder + Shallow, Asymmetric + Shoulder, Shallow + Shallow, and Complete + Shoulder. Complete + Complete should not be favoured because it risks recreating repeated paired arches. No shared topology, junction object, connecting segment, graph, or network cache is permitted. V3J.4E proceeds only after isolated V3J.4D1 families are accepted.


### 2026-07-12 — Patch V3J.4D: Glyph Family Expansion and Spacing Cleanup

**Status:** Unity-validated as a useful vocabulary proof; family overlap corrected by V3J.4D1.

V3J.4D removes the Unity-validated dead `Local Spacing Strength` control and replaces the single repeated mound silhouette with four independently authorable projected-glyph families. It keeps the V3J.4C2 width, density, broad-distribution, regional-concentration, strict-length, physical-validation, shader, and R8 coverage systems intact.

Removed completely:

```text
paintedAccentCompositionSpacingStrength serialized field and property
both inspector controls
cache-signature contribution
role-aware pairwise spacing rejection
spacing survival debug state and statistics
```

There is no hidden fixed-spacing fallback. After regional thinning, every surviving descriptor proceeds directly to the existing all-or-nothing physical-domain validation.

New relative family weights:

```text
Complete Mound Weight:    0–1, default 0.20
Asymmetric Mound Weight:  0–1, default 0.30
Single Shoulder Weight:   0–1, default 0.30
Shallow Crest Weight:     0–1, default 0.20
```

Weights are normalized internally. Selection uses only the stable proposal seed and authored weights; region mode and dominant/standard/support role do not alter the requested mixture. All-zero weights explicitly fall back to Complete Mound. Family selection happens before final projected-footprint validation, and a failed family is rejected rather than converted to another family.

Family grammar:

```text
Complete Mound:
  unchanged A6/A7 continuous two-sided mound

Asymmetric Mound:
  deterministic mirrored crest at approximately 25–38% or 62–75%
  one long shallow leg and one short steeper leg
  approximately 70–100% of the existing mound-height target

Single Shoulder:
  deterministic mirrored high run followed by one smooth descent
  no second complete leg
  approximately 50–85% of the existing mound-height target

Shallow Crest:
  broad predominantly lateral upper band with restrained end change
  approximately 20–50% of the existing mound-height target
```

Every family preserves the authored descriptor length and width. No family-specific world-length multiplier or width multiplier exists. The projected generator validates family shape, non-zero segments, self-intersection, complete width footprint, river/modifier exclusion, broad slope, and local grade before acceptance.

Diagnostics add selected and accepted descriptor counts per family, projected attempted/accepted/rejected counts, per-family accepted length and peak-height ranges, family-coloured composition markers, and an editor-only Family Preview filter. The filter affects Scene diagnostics only and never changes production coverage.

Expected modified files:

```text
Assets/Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentFoldFieldGenerator.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentProjectedGlyphGenerator.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentLongitudinalProfileGenerator.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
```

Validation gate:

1. Unity compiles with no C# or shader errors.
2. Local Spacing Strength is absent from both authoring inspectors and placement diagnostics.
3. Raising Stroke Density can produce denser local populations without hidden pairwise suppression.
4. Each family can be isolated by setting its weight to `1` and the others to `0`.
5. Complete Mound remains the accepted A6/A7 baseline.
6. Asymmetric Mound has materially unequal legs without elbows.
7. Single Shoulder has one high side and exactly one meaningful descending side.
8. Shallow Crest remains low and predominantly lateral rather than reading as a miniature complete arch.
9. Mixed accepted counts broadly follow normalized weights before family-specific physical rejection.
10. Every reported family length remains inside Stroke Length Min / Max, and width remains family-independent.
11. River, modifier, slope, grade, and invalid-ground exclusions remain correct.
12. The final mixed field is judged without Scene overlays.

### 2026-07-12 — Patch V3J.4C2: Density, Regional-Control, and Final Width Tuning

**Status:** Unity-validated for thinner width, higher population, Regional Zone Scale, and Regional Density Contrast. Local Spacing Strength was visually dead and is removed by V3J.4D.

V3J.4C2 is the final composition-tuning pass before glyph-family expansion. It preserves the accepted A6/A7 geometry, strict V3J.4C1 stroke-length bounds, physical exclusions, and generated R8 architecture while addressing three validated authoring needs: thinner ink, substantially larger baked populations, and direct control over local density concentration independently from total requested density.

Authoring ranges and controls:

```text
Stroke Width:                0.002–0.20 m
Stroke Density:              0–2000 proposals per standard 40x40 patch
Regional Zone Scale:         1–16 m
Regional Density Contrast:   0–1
```

`Regional Density Contrast` redistributes a fixed average regional survival rate rather than multiplying the total population. At zero, quiet, supporting, and accent regions use equal survival. At one, survival is approximately `0.10 / 0.44 / 0.99` while the probability-weighted mean remains `0.45`. Artists can therefore make accent zones denser without increasing Stroke Density. `Regional Zone Scale` controls the size of the jittered composition zones. The separate Local Spacing Strength proof produced no useful visible response in Unity and is not part of the active architecture.

The R8 baker is tightened again:

```text
minimum raster half-width: 0.30 → 0.08 texels
minimum edge feather:      0.35 → 0.10 texels
relative edge feather:     0.28 → 0.12 × core half-width
```

The authored Stroke Width remains authoritative; the rasterizer now adds only enough support for stable sub-texel coverage. The generator target cap rises from `240` to `2000` strokes per standard patch. This is dirty-time work and creates no runtime objects or per-frame processing.

Validation gate:

1. Unity compiles without C# or shader errors.
2. Existing style/profile assets expose Regional Zone Scale and Regional Density Contrast in both authoring inspectors.
3. A `0.02 m` authored stroke renders materially thinner than V3J.4C1; reducing Stroke Width below `0.01 m` continues to affect coverage rather than hitting the old clamp.
4. Stroke Density accepts values above `240` and placement diagnostics reflect the requested target up to `2000` on a standard patch.
5. With Stroke Density fixed, increasing Regional Density Contrast visibly concentrates marks into accent zones without a comparable increase in the overall pre-physical survival expectation.
6. Regional Zone Scale changes the size of coherent density/direction zones.
7. Unity evidence confirms the former Local Spacing Strength control has no useful visible effect; V3J.4D removes it rather than tuning it.
8. Stroke Length Min / Max remain strict bounds.
9. River, modifier, slope, grade, and invalid-ground exclusions remain unchanged.
10. The field is judged without debug overlays before proceeding to glyph-family expansion.


### 2026-07-12 — Patch V3J.4C1: Strict Authored Stroke-Length Bounds

**Status:** Unity-validated; active strict-length contract.

Unity validation of V3J.4C confirmed that regional composition improved field distribution, but also exposed an unacceptable contract violation: dominant, standard, and support role multipliers were applied after selecting a value from `Stroke Length Min / Max`, so generated marks could extend far outside the authored interval.

V3J.4C1 makes the authoring controls hard physical bounds. Composition roles now choose different normalized subranges *inside* the authored interval:

```text
support:  lower 0–38% of the authored interval
standard: middle 28–78% of the authored interval
dominant: upper 72–100% of the authored interval
```

The stable regional scale bias now shifts that normalized choice by at most `±0.07` before clamping to `[0, 1]`; it no longer multiplies world-space length. Final stroke length is constructed only as:

```text
lerp(Stroke Length Min, Stroke Length Max, bounded normalized role length)
```

Therefore every generated descriptor satisfies:

```text
Stroke Length Min <= generated planar stroke length <= Stroke Length Max
```

A narrow authored interval intentionally limits visible role hierarchy. Artists must widen Min / Max when they want larger length contrast; the composition system may never override those controls. The generator revision is incremented so existing placement and coverage caches rebuild automatically. No distribution, direction, spacing, width, A6/A7 profile, projection, shader, or coverage behavior changes in this correction.

Validation gate:

1. With Stroke Length Min `0.40 m` and Max `0.45 m`, placement diagnostics report accepted minimum and maximum inside `0.40–0.45 m`.
2. No visible mark has a planar endpoint span outside the authored interval.
3. Changing only Min / Max immediately constrains the complete generated population after regeneration.
4. The improved V3J.4C regional distribution remains otherwise unchanged.


### 2026-07-12 — Patch V3J.4C: Width Fidelity and Regional Composition

**Status:** Unity-validated for distribution and width; original length-scaling defect superseded by V3J.4C1.

V3J.4C keeps the accepted A6/A7 projected-glyph and R8 ground-albedo architecture, but corrects the provisional coverage width and replaces democratic per-mark placement with deterministic regional composition. It does not add connected networks, fragmentation, glyph families, shader controls, runtime objects, or per-frame work.

Coverage correction:

```text
R8 edge feather: 1.15 texels → 0.35 texels
minimum raster half-width: 0.45 texels → 0.30 texels
```

Coverage diagnostics now distinguish the narrowest authored full width, effective raster core width, edge feather per side, and estimated visible full width. The diagnostic core width is measured from each valid glyph's maximum authored half-width rather than its deliberately tapered endpoints.

Regional composition occurs before the existing physical stroke validation and before A6/A7 projection:

```text
weighted proposal
→ deterministic jittered regional assignment
→ quiet / supporting / accent regional thinning
→ dominant / standard / support role assignment
→ regional direction + restrained per-mark jitter
→ role-specific length and width scaling
→ role-aware spacing suppression (historical V3J.4C stage; removed by V3J.4D)
→ existing complete footprint validation
→ unchanged A6/A7 projected-glyph generation
→ corrected R8 coverage bake
```

Experimental fixed composition constants:

```text
region scale: clamp(mean authored length × 7, 3.0 m, 4.5 m)
region modes: 30% quiet / 50% supporting / 20% accent
acceptance ranges: 0.05–0.15 / 0.35–0.55 / 0.80–1.00
regional direction offset: up to ±30°
per-mark jitter: up to ±7°

dominant: at most first surviving mark of an accent region
          upper 72–100% of authored Min / Max interval
          width 0.90–1.05 × authored variation
standard: middle 28–78% of authored Min / Max interval
          width 0.90–1.00 × authored variation
support:  lower 0–38% of authored Min / Max interval
          width 0.80–0.95 × authored variation

regional length bias: at most ±0.07 normalized interval position
final world-space length: always inside authored Stroke Length Min / Max
```

V3J.4C originally added deterministic role-aware spacing. Unity later showed that its exposed strength control had no visible artistic value, so V3J.4D removes the complete pairwise stage. Every regionally surviving stroke still undergoes the existing all-or-nothing sampling, river, modifier, broad-slope, and local-grade validation. A rejected stroke remains absent.

The editor-only **Show Painted Accent Composition Debug** overlay exposes proposal region mode, thinning survival, occupied-region direction, and accepted dominant/standard/support roles. V3J.4D extends those accepted markers with family colour and removes obsolete spacing survival/statistics.

Expected modified files:

```text
Assets/Game/Procedural/Ground/GroundPaintedAccentCoverageBaker.cs
Assets/Game/Procedural/Ground/GroundPaintedAccentFoldFieldGenerator.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
```

The accepted projected-glyph generator, longitudinal-profile generator, shader/HLSL path, recipe/profile assets, scenes, prefabs, rivers, modifiers, materials, layers, and tags remain unchanged.

Validation gate:

1. Unity compiles with no C# or shader errors.
2. Production ink is materially thinner and crisper than V3J.4B.
3. Coverage diagnostics report core and feather contributions separately.
4. Real quiet regions appear rather than slightly reduced density everywhere.
5. Occupied regions show coherent local direction while adjacent regions differ.
6. Accent regions contain at most one dominant mark.
7. Role hierarchy remains inside the authored Stroke Length Min / Max interval.
8. Near-identical adjacent pairs are substantially reduced.
9. Physical exclusions remain correct.
10. The field is judged without debug overlays before glyph-family expansion is considered.


### 2026-07-12 — Patch V3J.4B: Accepted Projected Coverage Proof

**Status:** Unity-validated as a functional representation proof; superseded by V3J.4C for width fidelity and composition.

V3J.4B keeps the accepted A6/A7 projected glyph data unchanged and supplies its first production-style renderer. `GroundPaintedAccentCoverageBaker` rasterizes accepted projected polylines and their per-sample half-widths into a generated linear `R8` texture at generation/dirty time. Rasterization is segment-bounded rather than a full texture-by-segment scan. Resolution targets approximately `0.0125 m` per texel, aligns dimensions to eight texels, and caps each axis at `2048`. A sub-texel minimum raster half-width and bilinear filtering prevent accepted thin lines from disappearing at the proof resolution.

Implemented files:

```text
add:
  Assets/Game/Procedural/Ground/GroundPaintedAccentCoverageBaker.cs
  Assets/Game/Procedural/Ground/GroundPaintedAccentCoverageBaker.cs.meta

modify:
  Assets/Game/Procedural/Ground/GeneratedGround.cs
  Assets/Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs
  Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
  Assets/Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs
  Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaskDebug.hlsl
  Assets/Game/Rendering/PixelSurface/PixelSurfaceMaskDebugMode.cs
  Assets/Docs/Ground_Visual_Design_and_Architecture.md
  Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
```

Runtime/dirty-time contract:

```text
accepted projected glyph snapshot
→ R8 coverage texture owned by GeneratedGround
→ MaterialPropertyBlock texture/origin/size/world-to-ground matrix/Ink Color
→ coverage sampled in GeneratedGround-local XZ
→ lerp existing ground albedo toward opaque authored Ink Color
→ existing URP ground lighting
```

No scene, prefab, style asset, recipe value, material asset, layer, tag, river code, or accepted glyph point is changed. Dependent river corridor renderers receive the same ground-owned texture and world-to-ground matrix through the existing `ApplySurfaceProfileMaterialProperties(Renderer)` path. Glyph footprints already reject visible river, hidden bed/handoff, modifier, slope, grade, and invalid-surface regions before the coverage bake.

The proof deliberately does not add authoring controls for feather, opacity, resolution, breakup, or texture quality. `Strength` remains the existing layer blend scalar and `Ink Color` is the existing family/variant-authored opaque colour. `Ground Painted Accent Lines` debug mode displays the production coverage mask when it is enabled. The Inspector reports resolution, glyph/segment counts, coverage fraction, world texel size, and authored/effective minimum half-width.

Validation gate:

1. Unity compiles with no shader or C# errors.
2. The normal ground render shows dark accepted A6/A7 marks without Scene Handles.
3. **Ground Painted Accent Lines** shows the same silhouettes as a monochrome coverage mask.
4. **Show Accepted Projected Debug** aligns with the rendered ink centreline and width boundaries.
5. Changing only Ink Color updates the rendered ink without changing coverage.
6. Changing profile controls regenerates coverage and preserves A6/A7 geometry semantics.
7. Marks remain absent from river/handoff and Painted Accent modifier exclusions.
8. No child object, mesh, separate renderer, or material instance is created.
9. The user judges the production result before any placement-composition or glyph-family patch is approved.


### 2026-07-12 — Patch V3J.4A10R: Retire Rejected Regional-Network Experiments

**Status:** Confirmed in Unity; cleanup accepted.

Unity validation rejected both regional-network proofs.

A10A produced only three tiny one-trunk/one-branch survivors. Its diagnostics showed that restrictive descriptor grouping and generic rooted-tree extraction discarded almost the entire descriptor population before physical validation.

A10B successfully formed larger connected regions, but its explicit upper-run-plus-two-descending-terraces grammar produced a repeated table/π-symbol family rather than terrain-integrated contour marks. The result was structurally connected and deterministic, but artistically wrong. More branches, randomization, or fragmentation would only decorate the rejected grammar.

The complete A9A/A10A/A10B candidate line is therefore retired. Cleanup removes:

```text
GroundPaintedAccentRegionalNetworkCandidateGenerator.cs and .meta
regional-network cache, signatures, snapshots, and diagnostics
regional-network Scene overlay and rejection markers
accepted-versus-network paired comparison
regional-network Inspector controls and statistics
```

The accepted A6/A7 projected glyph path is unchanged and is now the sole Painted Accent shape implementation and Scene-view shape overlay.

Current shape architecture:

```text
GroundPaintedAccentSurfaceStroke
→ A6 continuous longitudinal profile
→ fixed world +Z projection
→ complete mesh-free A6/A7 projected glyph
```

No network, tendril, terrace, echo, or fragmentation candidate remains in code. No scene, prefab, style asset, recipe, shader, material, layer, or tag is changed by the cleanup.

Future work must not restart the rejected graph/network progression without a materially different artistic premise. The next useful investigation should begin from the accepted A6/A7 glyphs and concern placement composition, coverage baking, ink integration, or another explicitly approved representation—not additional procedural branches or network fragmentation.

#### Rejected-method ledger

```text
A9A descriptor-local cluster:
    rejected — ornate islands/tendrils, not a network

A10A small regional rooted tree:
    rejected — only three tiny surviving Y-like structures

A10B macro-regional contour terrace:
    rejected — repeated table/π-symbol grammar
```

#### Unity cleanup validation

1. Unity compiles without references to regional-network types.
2. The Painted Accent Inspector exposes only the accepted projected shape overlay.
3. No A9A/A10A/A10B network toggle, paired comparison, statistics panel, or Scene legend remains.
4. **Show Accepted Projected Debug** still displays the unchanged A6/A7 baseline.
5. Placement distribution, proposal, and accepted-position overlays remain available.
6. No regional-network generator file remains under `Assets/Game/Procedural/Ground/`.


### 2026-07-12 — Patch V3J.4A9AR: A9A Rejection and Independent Overlay Controls

**Historical status:** superseded; candidate controls and implementation removed by V3J.4A10R.

Unity validation rejected V3J.4A9A as a network solution. Its individual chains obey the downward-only rule, but its generation unit remains one accepted descriptor at a time:

```text
accepted descriptor
→ one local high root
→ two local arms
→ optional local branches and echo
→ one isolated decorated island
```

Adding arms, tendrils, and echoes inside each descriptor-local result did not create regional connectivity. The field still reads as separate symbols distributed across the ground. A9A was therefore retained temporarily as rejected comparison evidence by A9AR. V3J.4A10A removes that provisional implementation and replaces its candidate slot with the regional network; A9A must not be restored or advanced to fragmentation or production coverage.

A9AR changed no generated points, validation, caches, or candidate mathematics. It corrected only the then-current Scene-view comparison workflow and recorded the next architecture.

#### Independent and additive overlay contract

At that historical point, the three shape controls had independent meanings:

```text
Show Accepted Projected Debug
    accepted A6/A7 glyphs at true positions only

Show Rejected A9A Candidate Debug
    rejected A9A candidates at true positions only

Show Paired Comparison Preview
    additional editor-offset comparison copies
    accepted copy on visual left
    rejected A9A copy on visual right
```

Historical valid combinations were:

```text
all off                    → no shape overlay
accepted only              → accepted true-position result only
A9A only                   → rejected candidate true-position result only
accepted + A9A             → both true-position fields together
paired only                → offset comparison pairs only
any true-position toggles
+ paired                   → requested true-position fields plus pairs
```

The paired preview no longer substitutes for the true-position toggles. Both comparison copies are moved away from the real anchor in Scene Handles, so enabling a true-position result together with the paired preview does not draw a duplicate directly on top of it. Serialized field names were retained for compatibility at that stage; A10R later removed the candidate fields and controls completely.

#### Historical V3J.4A10A proposal — regional downward-only network (rejected and removed)

The next experiment must change the unit of generation:

```text
rejected A9A:
    one descriptor → one decorated island

then-planned A10A:
    nearby compatible descriptor group → one shared regional network
```

A10A was not implemented by A9AR. Its then-approved design target was:

```text
3–7 nearby accepted descriptors grouped by distance and broad direction
→ one regional high spine or shared root structure
→ connected downward-only primary chains across the group
→ restrained child branches and related lower echoes
→ complete unbroken regional network
→ fresh full-width validation of the whole network
→ separate candidate data, cache, diagnostics, and toggle
```

Descriptors become regional anchors and constraints rather than each demanding their own visible mound. Some descriptors may influence a shared trunk, junction, branch target, or echo without producing a complete local symbol.

Every directed chain must still remain level or descend away from its high/root point. Cups, valleys, upward hooks, and drop-then-rise chains remain forbidden. The first A10A proof is complete and unbroken; fragmentation remains forbidden until a regional network itself reads coherently.

This historical acceptance gate was not met. A10A and its A10B replacement were later rejected and removed by A10R.


### 2026-07-12 — Patch V3J.4A9A: Separate Downward-Only Contour-Cluster Candidate

**Status:** Rejected as a network solution by Unity visual validation; implementation removed by V3J.4A10A.

V3J.4A9A adds a new Scene-view-only native-2D candidate beside the accepted A6/A7 projected glyph. It does not modify `GroundPaintedAccentProjectedGlyph`, its generator, its cache, its diagnostics, or the meaning of **Show Projected Glyph Debug**.

The candidate has its own data-only generator:

```text
GroundPaintedAccentContourClusterCandidateGenerator
```

Candidate construction is:

```text
accepted placement descriptor
→ deterministic A9A density selection
→ one shared high/root point
→ independent left and right directed primary arms
→ zero to two smoothly attached directed branches
→ optional lower parallel echo
→ monotone cubic-Hermite evaluation
→ dense downward-height audit
→ full centre/left/right footprint validation
→ separate candidate snapshot and diagnostics
```

The accepted A6 mound profile is not used as the candidate shape foundation. The descriptor supplies only the placement anchor, seed, source scale, and visible width convention. Fixed world `+Z`, converted to GeneratedGround local X/Z, remains visual up.

#### Directed-height contract

Every candidate chain is stored from its high/root end toward its free end. In that direction, projected visual height may remain level or decrease but may not increase:

```text
north[i + 1] <= north[i] + 0.0005 m
```

The rule applies independently to both primary arms, every branch, and every echo. The generator uses monotone scalar Hermite interpolation for outward distance and accumulated drop, densely samples the result, and rejects the complete cluster if any sampled chain exceeds the upward tolerance. Catmull-Rom interpolation and unconstrained Bezier overshoot are not used.

A branch begins at an internal primary-arm sample, initially inherits the parent's outgoing direction, and then separates while continuing level or downward. An echo copies a substantial primary-arm interval and moves it uniformly lower, so it cannot curl upward toward the parent.

#### First-proof population

A9A intentionally remains a narrow experiment:

```text
candidate density selection: 64% of valid descriptors
primary controls:             5–8 per arm
primary final samples:        17–49 per arm
branches:                     0–2 per cluster
optional lower echo:          42% probability
fragmentation:                none
production rendering:         none
```

Both primary arms share the same high root but vary independently in horizontal reach, total drop, and internal spacing. The arms form one complete mound-directed parent gesture. Branches and echoes provide local connected/family structure without introducing cups, valleys, or upward hooks.

#### Independent presentation

The editor now exposes three independent concepts:

```text
Show Projected Glyph Debug
    accepted complete A6/A7 baseline at its true position

Show Cluster Candidate Debug
    complete unbroken A9A clusters at their true positions

Compare Accepted and Candidate
    accepted glyph at its true position
    matching candidate shifted only in Scene Handles to visual right
    dotted editor connector between the pair
```

Comparison offsets are not stored and do not participate in candidate generation or validation. A candidate is paired to an accepted glyph by the unchanged descriptor seed. Candidate rendering uses cyan primary arms, green branches, blue echoes, an orange high-root marker, and white branch-junction markers. Baseline rendering retains the validated red/purple/yellow palette.

#### Validation inheritance and scope

A9A performs a fresh full-width validation for every generated chain against:

```text
base-surface sampling
broad slope
transverse and longitudinal grade
river surface/bed/handoff plus safety clearance
GroundModifier Painted Accent exclusion
```

The whole candidate is rejected if any chain fails. This is necessary because the candidate extends beyond the accepted A6 glyph footprint. No clipping, relocation, backfill, or partial cluster acceptance is performed.

A9A changes only:

```text
new data-only contour-cluster candidate generator
GeneratedGround candidate cache, snapshot access, and two editor-only toggles
GeneratedGroundEditor candidate and comparison Scene Handles
canonical ground documents
```

It adds no mesh, renderer, child object, collider, material, shader, texture, layer, tag, component, per-frame rebuild, or production representation. Fragmentation remains forbidden until the complete unbroken cluster is visually accepted.

A9A validation gate:

1. Unity compiles without errors.
2. With candidate controls disabled, the accepted A7 overlay is visually unchanged.
3. **Show Projected Glyph Debug** still shows complete accepted glyphs only.
4. **Show Cluster Candidate Debug** shows complete multi-chain candidates at true positions.
5. **Compare Accepted and Candidate** shows separate paired outputs; neither representation replaces the other.
6. Every primary arm, branch, and echo remains level or descends away from its root; no cup, valley, upward hook, or drop-then-rise chain appears.
7. Candidate statistics report `Accepted upward violations: 0` and maximum accepted upward excursion no greater than `0.0005 m`.
8. Branches join smoothly enough to read as related terrain contours rather than random crossing scratches.
9. The complete, unbroken clusters read materially more connected and network-like than the accepted isolated mound symbols.
10. Fragmentation is not considered unless this complete-cluster gate passes.



### 2026-07-12 — Patch V3J.4A8R: Accepted Baseline Restoration and Downward-Only Cluster Plan

**Status:** Implemented; awaiting Unity regression validation.

V3J.4A8 is rejected. It preserved the A6 parent arrays internally, but replaced the only visible accepted overlay with one or two clipped windows from each already-small isolated mound. Unity validation showed the predictable failure: complete mound symbols became smaller disconnected line scraps, while the accepted A7 result could no longer be judged through its original debug presentation.

V3J.4A8R restores the exact validated A7 data and presentation contract:

```text
GroundPaintedAccentProjectedGlyph again contains only the complete A6 contour
Show Projected Glyph Debug again draws the complete accepted contour
full parent centreline, width boundaries, and visible crest marker are restored
visibility-window data, generation, drawing, taper, and diagnostics are removed
fragment statistics no longer alter the accepted projected-glyph summary
```

The restoration is a direct reversal of A8 in the three implementation files. It does not approximate the old result with a full-range window and does not retain dormant fragment behavior inside the accepted glyph. The A6 longitudinal generator, descriptor placement, fixed world-`+Z` embedding, width, full-footprint validation, river/modifier/slope/grade rejection, cache inputs, style assets, and ground rendering remain unchanged.

#### V3J.4A8 rejection record

The rejected premise was:

```text
small isolated A6 mound
→ hide one or more internal portions
→ expect terrain-network character
```

This cannot supply the missing large-scale coherence. The accepted marks already read as separate islands; subdividing them removes context rather than creating a larger implied landform. A8 is retained only as decision history. Its visibility-window structs, candidate ranges, cut taper, fragment drawing helpers, and fragment-population diagnostics are not part of the active code.

#### V3J.4A9A candidate — separate unbroken downward-only contour cluster

The next experiment must be a genuinely new native-2D candidate beside the accepted A6/A7 baseline. It must not use the left-leg/crest/right-leg mound profile as its shape foundation and must not mutate or replace accepted glyph data, diagnostics, cache behavior, or debug meaning.

Candidate generation sequence:

```text
accepted descriptor or candidate placement seed
→ native-2D cluster control graph
→ one long primary contour/spine
→ zero to two related downward-only arms or branches
→ optional lower parallel echo
→ monotone smooth spline evaluation
→ full candidate-footprint validation
→ separate Scene-view candidate presentation
```

The first A9A proof is complete and unbroken. Fragmentation is forbidden until the full cluster itself reads as one coherent mound-contour family.

The cluster is height-directed using fixed world `+Z` as visual up. Every arm has a defined high/root end. Moving away from that root, its projected visual height may remain approximately level or decrease, but may never rise again:

```text
height[i + 1] <= height[i] + small numerical tolerance
```

This rule applies to primary arms, branches, free ends, and parallel echoes. Invalid outcomes include cups, valleys, upward hooks, a low branch curling back toward a higher parent, or a chain that drops and later climbs. A complete two-sided mound is built as two independently directed arms leaving a high crest/spine, not as an unconstrained left-to-right spline.

Spline interpolation must preserve the directed-height rule between control nodes. An unconstrained Catmull-Rom curve is not acceptable because it may overshoot upward. Use monotone cubic Hermite segments or cubic Bezier handles clamped to the endpoint height interval, then densely sample and reject any upward excursion.

A9A presentation and data must remain separate:

```text
accepted full projected contour: unchanged A7 toggle and diagnostics
cluster candidate: separate structure, toggle, cache, and diagnostics
comparison: optional editor-only side-by-side display without moving stored data
```

No fragmentation, gaps, displaced scraps, or candidate production rendering belong in A9A. A later A9B may test zero to two restrained gaps per successful large cluster, preserving one long dominant component and never cutting a junction or destroying the downward-only mound read.

A8R validation gate:

1. Unity compiles without errors.
2. **Show Projected Glyph Debug** once again displays the exact complete A7 contours.
3. No visibility-window clipping, internal gaps, cut-width taper, or fragment-only crest hiding remains.
4. Project search finds no `GroundPaintedAccentVisibilityWindow`, `VisibilityWindows`, fragment drawing helper, or fragment-window diagnostic identifier.
5. Accepted projected-glyph counts and A6 projection/rejection diagnostics match the validated A7 behavior.
6. No candidate cluster is implemented by this recovery patch.
7. Both canonical documents mark A8 rejected and define A9A as a separate, unbroken, downward-only native-2D experiment.


### 2026-07-12 — Patch V3J.4A7: Raised Ribbon Retirement and Fragment-Proof Preparation

**Status:** Validated. The projected A6 result remained unchanged after retirement of the 3D branch.

The secondary 3D Painted Accent representation is retired. The mesh-free projected contour has succeeded as the sole active representation and is visually preferable to the raised crowned-ribbon experiment. V3J.4A7 removes the rejected comparison branch instead of carrying dead mesh, renderer, material, shader, and editor lifecycle code into the next 2D experiment.

Removed implementation:

```text
secondary crowned-ribbon mesh generation
preview child GameObject creation
MeshFilter and MeshRenderer preview setup
ground-normal profile displacement
three-vertex cross-section and crown topology
preview material/property-block configuration
preview mesh, memory, lighting, and silhouette diagnostics
legacy build/clear inspector actions
dedicated Painted Accent preview shader and meta file
preview cleanup and stale-name compatibility helpers
```

Retained 2D architecture:

```text
accepted ground-surface stroke descriptors
A6 continuous longitudinal profile
fixed world +Z projected embedding
visible-width taper
full transformed-footprint validation
river, modifier, sampling, slope, and grade rejection
Scene-view projected contour diagnostics
authored Ink Color reserved for future coverage rendering
existing 256 RGBA field/debug support until the coverage path replaces it
```

The internal serialized names `paintedAccentFoldHeight`, `paintedAccentCrestCrownHeight`, `paintedAccentFoldIrregularity`, and `paintedAccentFoldEndTaper` remain unchanged to preserve existing style assets. Their current authoring meanings are exclusively 2D:

```text
Profile Height:       primary fixed-+Z contour amplitude
Crest Crown Height:  additional projected crest/cap amplitude
Profile Irregularity: seeded longitudinal contour variation
End Taper:           contour and visible-width endpoint envelope
Stroke Width:        visible projected-contour width
Ink Color:           future ground-albedo coverage colour
```

A7 originally documented fragment windows as the next narrow experiment. V3J.4A8 implemented that idea and Unity validation rejected it because it chopped already-isolated mound glyphs into disconnected scraps while replacing the accepted overlay. V3J.4A8R removes that experiment and restores A7 exactly. The corrected next candidate is the separate native-2D, unbroken, downward-only contour-cluster proof defined at the top of this document.

A7 validation gate:

1. Unity compiles without errors.
2. No legacy raised-comparison heading or build/clear buttons remain.
3. No secondary Painted Accent preview mesh, renderer, material, child object, or dedicated preview shader can be generated.
4. The projected Scene overlay and A6 contour output remain unchanged.
5. Existing style assets deserialize without migration.
6. The project code and assets contain no retired preview method, child-name, or shader identifier.
7. Documentation identifies mesh-free projection as the sole active representation.
8. Fragment emission is documented only as the next experiment and is not presented as accepted or already implemented.

## Historical implementation record — superseded by V3J.4A7

All sections below this heading are retained only as decision history. Any wording below that calls a raised ribbon, secondary mesh, comparison shader, or raised/projected dual path “active,” “current,” or “required” is superseded by V3J.4A7 and must not be followed as implementation guidance.


### 2026-07-12 — Patch V3J.4A6: Continuous Spline Profile Reset

**Status:** Implemented; awaiting Unity visual validation.

V3J.4A5 failed both visual gates. Unity still showed lower-leg elbows, most applied endpoints remained visually shallow, and parameter variation continued to collapse into a small family of arch silhouettes. A5 is rejected as the active shaping architecture. Its normalized takeoff slope, coordinate warp, positive-bell detail grammar, isolated-turn detector, and lower-leg reconstruction have been removed rather than tuned further.

V3J.4A6 returns to the useful legacy solved profile knots and treats them as shape evidence rather than final line segments. The final shared scalar profile is now constructed as:

```text
17–25 legacy solved source knots
→ independent physical endpoint-angle requests
→ positive asymmetric Hermite mound guide
→ retained legacy signed residual
→ smooth signed multi-scale control signal
→ positive mound safety floor
→ one shape-preserving C1 cubic spline
→ 65–97 final samples
```

Each endpoint requests a continuous physical profile-space angle in `12–68 degrees`. The deterministic distribution intentionally includes soft, moderate, and steep entries, with left and right legs resolved independently. Requested angle is converted through actual stroke planar length and physical Profile Height into a normalized derivative, then bounded only by the monotonic cubic limit. Diagnostics report both requested angles and the final sampled endpoint angles after crest and crown composition; a population dominated by shallow applied angles fails validation.

The broad guide is one piecewise cubic-Hermite mound with the selected endpoint derivatives and zero derivative at the dominant crest. The legacy profile contributes a signed residual at `0.45–0.90` retention. Each leg also receives three to five signed interior detail controls, interpolated smoothly and faded to zero at the grounded endpoint and inside the crest protection zone. Signed detail may create a swell followed by a small release, unlike the rejected positive-only bell stack, but the final result is constrained above a seeded positive mound floor at `0.82–0.92` of the guide. This permits real directional rhythm without restoring the broad inward sags rejected in A2.

The final source knots use shape-preserving cubic tangents. Endpoint tangents are explicit, the dominant crest tangent is exactly zero, and every interior knot shares one tangent between adjacent segments. The final output is sampled at `(sourceCount - 1) × 4 + 1`, giving 65–97 points without adding a mesh or runtime object to projected mode. No post-profile corner detector or local reconstruction pass remains.

The raised-preview summary now reports:

```text
sourceKnotsMinMeanMax
longitudinalSamplesMinMeanMax
endpointAngleRequestedMinMeanMax
endpointAngleAppliedMinMeanMax
endpointLegsSoftModerateSteep
signedDetailControlsMinMeanMax
negativeDetailControls
floorCorrectionSamples
minimumProfileMinusFloor
samplesBelowPositiveFloor
dominantPeakViolations
maximumSplineTangentDiscontinuity
maximumSampledTurnDegrees
nearestSilhouetteRmsMinMean
nearDuplicateSilhouettePairs
largestNearDuplicateSilhouetteCluster
```

Validation gate:

1. Unity compiles without errors.
2. Reuse the exact A5 seed and screenshots containing the returned lower-leg elbows.
3. No visible elbow may remain at either endpoint or at an interior source-knot location.
4. The dense field must contain a clear mixture of soft, moderate, and steep applied endpoint angles; most endpoints may not remain nearly parallel to the baseline.
5. Steep endpoints must transition continuously into their legs rather than forming a short straight segment followed by a knee.
6. Signed variation must produce visibly different directional rhythm, not merely thicker versions of the same arch.
7. `samplesBelowPositiveFloor=0`, `dominantPeakViolations=0`, and `maximumSplineTangentDiscontinuity≈0` are mandatory.
8. Silhouette RMS diagnostics, not internal parameter bins, are the population-diversity evidence.
9. Fixed world-`+Z` projection, width, placement, exclusions, shaders, and style assets remain unchanged.


### 2026-07-12 — Patch V3J.4A5: Diverse Endpoint Takeoff and Non-Template Leg Detail

**Status:** Rejected by Unity validation; superseded by V3J.4A6.

V3J.4A4 removed the reported lower-leg elbows, but Unity validation showed that it over-normalized the profile population. Its smootherstep mound guide forced zero slope at every grounded endpoint, and its absolute-turn repair rebuilt roughly four fifths of the synthetic validation legs through the same zero-slope cubic takeoff. Combined with a universal `one broad shoulder + one-to-three fine bells` recipe and a fixed `0.20–0.38` detail-release window, numerically distinct profiles collapsed into a few repeated visual patterns.

V3J.4A5 replaces the universal endpoint basis with an independently seeded monotone cubic-Hermite takeoff for each leg:

```text
B(u, m) = (-2u^3 + 3u^2) + m(u^3 - 2u^2 + u)

B(0)  = 0
B(1)  = 1
B'(0) = m
B'(1) = 0
```

`m` is a continuous seeded value in `0.10–2.80`. It supports soft stitching, moderate entry, and a meaningful minority of steep ground-stabbing entries without creating discrete serialized archetypes. Left and right legs resolve independently. A second independent seeded parameter warps progress as `w(u)=u+b*u*(1-u)`, with `b` in approximately `-0.45…+0.45`, moving each leg's curvature earlier or later while preserving a monotonic endpoint-to-crest foundation and zero crest slope.

The fixed lower-detail window is replaced by per-leg seeded ranges:

```text
protected endpoint region: 0.06–0.18
full detail release:       0.18–0.32
```

Steeper takeoffs bias toward earlier release. The previous mandatory broad-plus-fine grammar is removed. Every sufficiently long leg instead requests two to six continuously parameterized positive events, with independent amplitude `1.5–13%`, centre `0.14–0.88`, and width `0.05–0.36`. Feature centres use a seeded low-discrepancy sequence with jitter so events neither collapse into the same structural template nor cluster at one location. Detail remains additive-only, fades at endpoints and near the dominant crest, and stays below `0.88 ×` the dominant height.

The A4 repair is retained only as an outlier safety pass. A turn is repaired only when it exceeds both an absolute `34°` threshold and the neighbouring-turn median by `1.8× + 7°`; an early height reversal remains a hard fault. The repair anchor begins two samples beyond the outlier instead of rebuilding the complete inspected lower leg. Its full Hermite reconstruction now includes the selected endpoint derivative, so a repaired steep leg remains steep rather than reverting to a horizontal stitch. Monotone derivative limiting prevents overshoot. The target correction rate is substantially below `25%`; a majority correction rate is a failure because the pass would again be functioning as a profile normalizer.

The existing single raised-preview summary now reports:

```text
takeoffSlopeMinMeanMax
takeoffLegsSoftModerateSteep
curvatureBiasMinMeanMax
detailProtectionMinMax
detailFullReleaseMinMax
detailFeaturesMinMeanMax
detailFeaturesSmallMediumBroad
lowerLegCornerCorrections
lowerLegCorrectionRate
lowerLegIsolatedTurnBeforeAfter
```

Validation gate:

1. Unity compiles with no Console errors.
2. Reuse the exact A4 seed and view containing the circled repeated profiles.
3. The field must visibly include soft, moderate, and steep endpoint entries.
4. A steep entry must remain continuously curved and may not become a pipe-like elbow.
5. Left and right takeoff strength and curvature must vary independently.
6. The field must no longer read as two or three recycled macro-patterns.
7. Positive detail should appear at varied scales and positions without restoring negative sag or competing dominant peaks.
8. `lowerLegCorrectionRate` must remain well below `0.25`; a majority correction rate fails the patch.
9. `samplesBelowPositiveGuide=0` and `negativeDetailSamples=0` remain mandatory.
10. Fixed world-`+Z` projection, width, placement, exclusions, raised topology, and shaders remain unchanged.


### 2026-07-12 — Patch V3J.4A4: Lower-Leg Takeoff and Corner Suppression

**Status:** Superseded as a population-wide takeoff strategy by V3J.4A5; its smooth envelope crossover remains active.

V3J.4A3 established the accepted positive mound grammar, but Unity validation exposed a separate lower-leg failure: one or both grounded legs could contain a hard elbow close to the endpoint. The audit found three concrete causes in the shared profile code:

```text
hard min:
  guide = min(smootherstep mound, end envelope)

hard max:
  crown envelope = max(fold envelope, short crown support)

positive detail:
  raw residuals and seeded bells could become visible too close to the endpoint
```

The hard min/max selectors are continuous in value but not in slope at their crossover, so they can create a visible polyline knee even when both source envelopes are individually smooth. V3J.4A4 replaces them with smooth selectors across a normalized `0.08` transition band. Endpoint and crest values remain unchanged.

The first part of every endpoint-to-crest leg is now a protected takeoff zone:

```text
no retained raw or seeded detail through first 20% of a leg
smooth detail release from 20% to 38%

broad shoulder centre:
  moved from 0.32–0.68 to 0.42–0.72

fine feature centre:
  moved from 0.16–0.82 to 0.30–0.82
```

After all positive detail is applied, each leg inspects the first `40%` for either:

```text
normalized turn angle > 26 degrees
or
an early endpoint-to-crest height reversal > 0.008 of peak height
```

A flagged leg rebuilds only its lower takeoff. The endpoint remains exactly zero, the repair anchor and all samples above it remain fixed, and a cubic Hermite segment matches the anchor value and local derivative. The positive mound guide and the positive residual above it are rebuilt separately, preserving the invariant `profile >= guide`. Up to three guarded passes may extend the anchor, between 3 and 12 samples, only while the lower corner remains outside the gate.

No general smoothing is applied to accepted legs. Crest grammar, positive-only multi-scale detail, fixed world-`+Z` projection, width, placement, river/modifier exclusion, and raised topology remain unchanged.

The existing single raised-preview summary adds only:

```text
lowerLegCornerCorrections = left/right/both stroke counts
lowerLegTurnBeforeAfter    = maximum detected degrees before/after repair
lowerLegDetailProtectionRelease = 0.20–0.38
lowerLegCornerMaximumTurn  = 26 degrees
lowerLegEnvelopeTransition = 0.08
```

Validation gate:

1. Unity compiles with no Console errors.
2. Reuse the exact four lower-leg elbow examples that failed V3J.4A3.
3. Neither leg may contain a visible pipe-like knee near its grounded endpoint.
4. The first takeoff must remain smooth without becoming a long sterile straight segment.
5. Broad shoulders and fine positive variation must remain visible above the protected zone.
6. The dominant crest and A3 positive mound floor must remain unchanged in character.
7. `lowerLegTurnBeforeAfter` must show a lower post-repair maximum for corrected strokes.
8. Endpoints, fixed `+Z` orientation, width, placement, and all exclusion rules remain unchanged.

### 2026-07-12 — Patch V3J.4A3: Crest-Smooth Positive Mound and Multi-Scale Detail

**Status:** Accepted positive-profile foundation; lower-leg edge-case polish superseded by V3J.4A4.

V3J.4A2 did not pass its visual gate. Although it softened some isolated one-leg cases, the underlying power-law guide still joined the two legs at a non-zero slope and therefore continued to produce sharp `^` crests. Its signed articulation pass also deliberately permitted broad negative sags, and its absolute chord-deviation diagnostic treated outward shoulders and inward dents as equivalent improvements. V3J.4A3 replaces that rejected repair model rather than retuning it.

The shared longitudinal profile now uses a crest-flat positive mound foundation. For normalized endpoint-to-crest progress `u`, the guide is:

```text
S(u) = 6u^5 - 15u^4 + 10u^3
G(u) = S(u)^p
```

`S(0)=0`, `S(1)=1`, `S'(0)=0`, and `S'(1)=0`. Both legs therefore leave their endpoints smoothly and meet the dominant crest with zero analytic slope. Existing seeded left/right sharpness remains, so the mound can still be asymmetric without creating a geometric cusp. The existing rounded-crest pass and guarded sharp-tip safety pass remain as secondary protection, not as the primary source of crest shape.

The raw profile is no longer blended symmetrically toward the guide. Only positive detail above the guide is retained:

```text
R+(t) = max(0, Raw(t) - G(t))
H(t)  = G(t) + retention × R+(t)

retention = 0.25–0.65, scaled by Profile Irregularity
```

Broad raw plateaus reduce retention by at most `20%`. Negative raw dents are discarded. After crest rounding and detail application, every sample is clamped back to the positive mound floor, so the final invariant is:

```text
H(t) >= G(t)
```

The rejected A2 straight-leg detector and signed single bell are removed. Every sufficiently long leg now receives positive-only multi-scale contour detail:

```text
one broad shoulder per leg:
  centre    = seeded 0.32–0.68 of the leg
  width     = seeded 0.18–0.30
  amplitude = seeded 5–11% of dominant height

one to three fine positive events per leg:
  centre    = seeded 0.16–0.82
  width     = seeded 0.06–0.14
  amplitude = seeded 1.5–4.5% of dominant height
```

The event count and amplitude scale with existing **Profile Irregularity**. Every event is strictly additive, fades to zero at the endpoint, and is suppressed within the final `14%` before the dominant crest. Non-crest samples are capped at `0.88 ×` the dominant height, preserving one primary mound while allowing shoulders and small secondary contour events. No new authoring control is added.

Raised-preview diagnostics now report:

```text
positiveRawRetentionMin/Mean/Max
positiveRawDetailSamples
left/right/both broad-shoulder stroke counts
finePositiveFeatures
crestSafetyCorrectedStrokes
minimumProfileMinusGuide
samplesBelowPositiveGuide
negativeDetailSamples
maximumNormalizedCrestSlope
```

Required invariants are:

```text
samplesBelowPositiveGuide = 0
negativeDetailSamples     = 0
minimumProfileMinusGuide >= -0.00001
```

Validation gate:

1. Unity compiles with no Console errors.
2. Reuse the exact seed and examples that failed V3J.4A2.
3. The clustered `^` profiles must become compact rounded mounds rather than one-sample corners.
4. No leg may form the broad inward sag seen in the rejected A2 screenshot.
5. Long legs must contain visible positive shoulders or smaller contour events rather than smooth ruler-like ramps.
6. Fine events must remain subordinate; no secondary peak may approach the dominant crest.
7. Short strokes must remain readable rather than becoming high-frequency scribbles.
8. Projected and raised representations must change together because they consume the same profile.
9. Endpoint grounding, fixed world-`+Z` projection, width, placement, and all exclusion rules remain unchanged.

### 2026-07-12 — Patch V3J.4A2: Rejected Signed Articulation Experiment

**Status:** Rejected and superseded by V3J.4A3.

A2 added post-process crest smoothing and one signed broad articulation bell to legs classified as straight. Unity validation showed that the underlying power-law guide still produced triangular crests, the one-bell correction remained visually bland, and the permitted negative sign created inward sags. Its absolute chord-deviation metric also could not distinguish a desired outward shoulder from an undesired inward dent. Do not restore or tune the A2 signed articulation path; V3J.4A3 is the authoritative shared-profile correction.

### 2026-07-12 — Patch V3J.4A1: Shared Legacy Profile Transfer

**Status:** Implemented; awaiting Unity equivalence and shape validation.

V3J.4A correctly proved the mesh-free descriptor/validation/debug architecture, but its independently invented sine-envelope projection failed the visual gate. It produced overly uniform one-sided arcs, weak crests, and seed-random inversions. The accepted raised crowned ribbon already contained the required longitudinal silhouette mathematics; V3J.4A1 therefore removes the duplicate projected shape model and shares the solved scalar profile between both representations.

Shared data flow:

```text
accepted GroundPaintedAccentSurfaceStroke
→ shared longitudinal sampling and profile solve
→ scalar silhouette H(t)
      ├─ legacy comparison: baseline + local ground normal × H(t)
      └─ projected glyph:   baseline X/Z + local(world +Z) × H(t)
```

The new representation-independent evaluator owns the exact existing raised-profile calculations:

```text
sample count: clamp(max(17, source points, ceil(planarLength / 0.09) + 1), 17, 25)
descriptor-index point/normal interpolation
five-sample cross-profile crest search
seeded positive/negative profile bases
fold end envelope
single-mound shaping
plateau suppression
isolated-valley repair
rounded-crest treatment
peak normalization
crown end envelope
visible-width end taper with 0.12 endpoint scale
```

The physical scalar applied by both consumers is:

```text
crestHeight(t) =
  ProfileHeight
  × lerp(0.94, 1.0, stroke.Strength)
  × normalizedCrestHeight(t)

crownHeight(t) =
  CrestCrownHeight
  × crownEndEnvelope(t)

H(t) = crestHeight(t) + crownHeight(t)
```

The fixed gameplay camera always treats world `+Z` as screen-up. Projected glyphs therefore apply `H(t)` along world `+Z`, transformed into the GeneratedGround local X/Z plane. There is no seed-random bend sign, local-stroke-side choice, camera lookup, or per-frame camera dependency.

V3J.4A1 removes the failed projected-only machinery:

```text
Projected Profile Spread
9–25 point / 0.05 m arc-length resampler
independent crest selection
sine mound envelope
low-frequency modulation
random bend side
independent endpoint-width formula
```

The existing authored values are now explicitly shared:

```text
Profile Height
Crest Crown Height
Profile Irregularity
End Taper
Stroke Width
```

The legacy raised preview consumes the same profile samples and preserves its existing cross-section, lateral surface sampling, normal lift, topology, renderer, and diagnostics. The projected path remains descriptor-only and creates no mesh, renderer, child object, collider, material, or shader draw.

Transformed projected footprints continue to validate the centre and tapered left/right edges against valid ground sampling, broad slope, local transverse/longitudinal grade, visible/hidden river and handoff clearance, and `GroundModifier` Painted Accent exclusion. One invalid sample rejects the complete glyph; no clipping, fitting, relocation, or backfilling is introduced.

New or revised diagnostics:

```text
projectionWorldDirection = +Z
projectionLocalDirectionXZ
profilePointCountMin/Mean/Max
crestTMin/Mean/Max
crestPeakHeightMin/Mean/Max
crownPeakHeightMin/Mean/Max
combinedPeakHeightMin/Mean/Max
maximumNorthDisplacementError
maximumCrossAxisDrift
projected rejection counters by reason
```

The two projection invariants are:

```text
abs(dot(projectedXZ - baselineXZ, localNorth) - H(t)) <= 0.00001 m
abs(dot(projectedXZ - baselineXZ, perpendicular(localNorth))) <= 0.00001 m
```

Unity validation procedure:

1. Compile with no Console errors.
2. Regenerate the same ground, seed, family, variant, placement controls, and fixed gameplay camera used for the V3J.4A comparison.
3. Enable Scene-view Gizmos and **Show Projected Glyph Debug**.
4. Build **Legacy Raised Ribbon** without changing any profile control.
5. Confirm every cyan projected profile rises toward screen-up/world `+Z`; no glyph may invert by seed.
6. Compare each cyan curve to the brown legacy upper silhouette: endpoints, crest position, relative peak, asymmetry, shoulders, rounded crest, and rise/fall lengths must agree.
7. Change only **Profile Height**; both representations must scale proportionally without moving the crest.
8. Change only **Crest Crown Height**; both must update immediately, proving corrected cache invalidation.
9. Change only **Profile Irregularity** and then **End Taper**; both representations must retain matching seeded profile and endpoint behavior.
10. Require north-displacement error and cross-axis drift to remain at or below `0.00001 m`.
11. Recheck river, modifier, slope, grade, and full-width rejection.
12. Clear the raised preview and disable the overlay; confirm no projected scene object or mesh exists.

Acceptance gate:

- Raised and projected consumers use the same profile samples.
- The legacy raised output remains visually and numerically unchanged.
- Every projected profile points toward fixed world `+Z`.
- The projected contour reproduces the useful raised upper-silhouette grammar rather than the rejected V3J.4A banana arcs.
- Complete transformed-footprint validation remains correct.
- No production texture bake, shader composition, pooling, or quality-tier work is included.

Implemented by V3J.4B: accepted projected polylines are rasterized into single-channel coverage and the family/variant Ink Color is blended into final ground albedo before ordinary lighting. The proof uses bounded CPU segment rasterization at dirty time; a GPU bake is not required unless later profiling justifies it.

### 2026-07-11 — Patch V3J.3D5: Environment-Integrated Flat Ink

D5 is the active Painted Accent material proof. It preserves the flat graphic Ink Color and all D4 geometry/placement behavior, but replaces the completely unlit output with a normal-independent environmental exposure scalar.

Changed files:

```text
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentInk.shader
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
```

Proof response:

```text
ambient=0.75
direct=0.80
shadow=0.70
local lights=0.25
minimum visibility=0.14
maximum exposure=1.0
```

The shader samples ambient SH, main-light attenuation/shadows, and restrained additional lights without using mesh normals. The renderer receives shadows but continues to cast none. Validate open daylight, partial/deep shadow, dusk/night, and nearby local lights. The D4 pointed-apex question remains separate and unresolved.

### 2026-07-11 — Patch V3J.3C8: Flat Ink Surface Baseline

C7 proved full double-sided shape visibility, but its lit response was rejected because it made the accepted geometry read as a physically shaded mound. C8 locks geometry and replaces the entire C7 response with a uniform graphic ink treatment inspired by drawn ground-outline strokes.

Implemented file operations:

```text
modify:
  Assets/Game/Procedural/Ground/GeneratedGround.cs
  Assets/Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs
  Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
  Assets/Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs
  Assets/Docs/Ground_Visual_Design_and_Architecture.md
  Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md

remove:
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentLit.shader
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentLit.shader.meta

add:
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentInk.shader
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentInk.shader.meta
```

Shader contract:

```text
shader: PS3D/Ground Painted Accent Ink
pass: SRPDefaultUnlit
queue/render type: opaque geometry
Cull: Off
ZWrite: On
ZTest: LEqual
fragment result: opaque _InkColor only
lighting/shadows/GI/AO/probes/fog/specular/emission/textures: none
```

Authoring contract:

```text
control: Ink Color
serialized field: paintedAccentInkColor
default: (0.12, 0.10, 0.08, 1)
alpha: fixed opaque
scope: one uniform colour across every vertex and both sides of a stroke
```

`GeneratedGround` resolves the selected Painted Accent recipe colour and applies it through a renderer `MaterialPropertyBlock`. The shared material keeps the default ink colour only as a fallback, allowing multiple generated-ground previews to retain different variant colours without creating per-object materials. The renderer explicitly keeps shadow casting off, shadow receiving off, light probes off, and reflection probes off.

C7 cleanup:

```text
remove UV1 generation and SetUVs(1)
remove seed-derived material variation and statistics
remove crown/edge/endpoint/per-stroke/smoothness constants
remove lit shader fallback preference
remove normal-based lighting and back-face normal correction
remove C7 surface-response diagnostics
```

UV0 and recalculated normals remain in the mesh for compatibility, although the C8 shader does not consume them. Expected 36-stroke storage returns to the C5/C6 estimate:

```text
vertices=1404
triangles=1728
estimatedVertexBufferBytes=44928
estimatedIndexBufferBytes=10368
estimatedRawMeshBytes=55296
```

Required diagnostic state:

```text
materialShader=PS3D/Ground Painted Accent Ink
inkColor=(0.120,0.100,0.080,1.000)  # default, unless authored otherwise
surfaceMode=FlatUnlitInk
materialCull=Off
doubleSidedGI=False
shadowCasting=Off
receiveShadows=False
lightProbes=Off
reflectionProbes=Off
```

Validation keeps the accepted geometry settings and compares the same line under different scene-light directions, brightness levels, and both viewing sides. Accept only if the entire stroke remains one visually uniform dark colour with no lighting gradient, crown highlight, edge response, endpoint response, seed variation, cast shadow, received shadow, or probe-driven change. After acceptance, stop individual-line geometry/material work and proceed to distribution.

### 2026-07-11 — Patch V3J.3C7: Painted Accent Surface-Response Proof

C6 validation proved that double-sided rasterization solves the interior-face disappearance. C5 geometry plus C6 visibility is now the locked representation baseline. C7 begins final per-line visual treatment without changing geometry, placement, or authoring controls.

Implemented scope:

```text
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentLit.shader
Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentLit.shader.meta
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
```

Dedicated shader contract:

```text
shader: PS3D/Ground Painted Accent Lit
render type: opaque
culling: off
back-face lighting: flip world normal for reverse-facing fragments
metallic: 0
specular: 0
smoothness: 0.08
emission: none
textures/noise maps: none
preview shadow casting: off
shadow receiving: retained
```

Mesh-channel contract:

```text
UV0.x: existing normalized position along stroke
UV0.y: existing normalized crown cross coordinate
UV1.x: deterministic 0–1 value derived once from stroke.Seed
UV1.y: zero/reserved
```

Surface proof values:

```text
Base Color                 = (0.50, 0.46, 0.40, 1)
Crown Brightness Lift      = 0.10
Outer Edge Darken          = 0.08
Endpoint Softening Span    = 0.12
Endpoint Contrast Scale    = 0.55
Per-Stroke Variation       = 0.05
Smoothness                 = 0.08
```

Response behavior:

- use `UV0.y` for smooth centre-crown lightening and outer-edge darkening;
- use `UV0.x` to reduce cross-sectional contrast and slightly desaturate the terminal span;
- do not use alpha, transparency, clipping, emission, Fresnel, or texture sampling;
- use `UV1.x` only for restrained stable per-stroke brightness difference;
- retain one combined mesh, one shared material, and no per-stroke objects;
- resolve the dedicated shader first, but keep the prior URP fallback chain for import/compile failure;
- upgrade a cached shared proof material to the dedicated shader when available.

Topology remains 1,404 vertices and 1,728 triangles for 36 strokes. UV1 increases estimated vertex stride to 40 bytes and expected raw storage to 66,528 bytes:

```text
vertex buffer: 56,160 bytes
index buffer:  10,368 bytes
raw total:     66,528 bytes
```

Required log additions:

```text
materialShader=PS3D/Ground Painted Accent Lit
strokeVariationUv1Min/Mean/Max
surfaceCrownLift=0.100
surfaceEdgeDarken=0.080
surfaceEndpointSpan=0.120
surfaceEndpointContrast=0.550
surfacePerStrokeVariation=0.050
surfaceSmoothness=0.080
materialCull=Off
doubleSidedGI=True
```

Focused validation keeps `Stroke Width = 0.02 m`, `Fold Height = 0.25 m`, and `Crest Crown Height = 0.02 m`. Capture normal gameplay, close, low-angle, exterior-side, and interior-side views. Accept only if the silhouette remains unchanged, both sides remain consistently lit, the crown is readable but restrained, edges do not become outlines, terminals feel grounded without transparent fading, and per-stroke variation is subtle. Reject glow, noisy bands, texture soup, obvious stamped colour steps, metallic highlights, or rope/rail/root emphasis.

No recipe, inspector, style asset, descriptor generator, distribution, scene, base mesh, collider, River, Generated Mass, or GroundModifier change is part of C7. Distribution work remains deferred until this surface proof passes.

### 2026-07-11 — Patch V3J.3C6: Double-Sided Interior-Face Visibility Validation Result

C6 was validated successfully. The same leg that previously lost its interior-facing surface remained visible after the shared material changed to `_Cull = CullMode.Off`. The defect was back-face culling on the accepted open crowned ribbon, not a missing shell or deficient height profile.

Retained baseline:

```text
C5 longitudinal and crown geometry: accepted
Cull Off: accepted and permanent
Material.doubleSidedGI: accepted
shallow open side shell: not required
further geometry shaping: paused
```

C6 adds no mesh channels and preserves 1,404 vertices / 1,728 triangles for 36 strokes. Its successful diagnostic state is `materialCull=Off` and `doubleSidedGI=True`. C7 now owns individual-line surface response while preserving this representation exactly.

### 2026-07-11 — Patch V3J.3C5: Valley-Suppressed Crowned Ribbon Refinement

C4 validation isolated two implementation-level overcorrections. The strict `0.70` single-crest blend plus monotonic guards removed the unwanted `M` profiles but made most strokes read as sterile `^` shapes. Shoulder contribution increased to `0.35`, yet the crown still used the slow macro end envelope, leaving first/last interior rows with too little cross-sectional body. C5 keeps the accepted crowned-ribbon representation and corrects only those two points.

Implemented code scope:

```text
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
```

No recipe, inspector, style asset, descriptor generator, shader, scene, base mesh, collider, River, Generated Mass, or GroundModifier change is part of C5.

Longitudinal shaping contract:

```text
raw source:
  unchanged five-position stochastic crest search at every row

broad guide:
  choose the highest interior row
  build the existing smooth rise/fall target
  blend raw -> target at 0.35

variation preservation:
  remove C4's monotonic rise/fall passes
  preserve asymmetry, shelves, minor bumps, and local slope changes

valley suppression:
  inspect the blended profile from an unmodified copy
  a valley is actionable only when below both neighbours
  required depth = more than 0.08 of the stroke peak
  repair = lift 60% toward the lower neighbour
  one targeted pass; no global flattening
```

Crown and leg-support contract:

```text
old cross-section factors: [0.35, 1.00, 0.35]
new cross-section factors: [0.50, 1.00, 0.50]

macro Fold Height and width:
  continue to use the original deterministic Fold End Taper envelope

crown-only short envelope:
  ramp fraction = 0.12 of stroke length at each end
  leg support multiplier = 0.45
  effective crown envelope = max(original fold envelope,
                                 short envelope × 0.45)

terminal guarantee:
  exact first/final rows remain zero-crown
  existing 0.002 m terminal embed remains
```

No side walls, underside, caps, bottom, collider, or base-ground mutation are introduced. Topology remains three vertices across and thirteen rows minimum: 1,404 vertices and 1,728 triangles for 36 strokes, with estimated raw storage of 55,296 bytes.

The log must include:

```text
singleCrestBlend=0.350
valleyThresholdFraction=0.080
valleyRepairStrength=0.600
shoulderCrownFraction=0.500
legCrownSupport=0.450
crownEndRampFraction=0.120
ribbonVerticesAcross=3
longitudinalSamplesMin/Mean/Max
crestPeakHeightMin/Mean/Max
crownPeakHeightMin/Mean/Max
combinedPeakHeightMin/Mean/Max
effectiveWidthMin/Mean/Max
vertices / triangles / memory / buildMs
```

Focused validation baseline:

```text
Stroke Width       = 0.02 m
Fold Height        = 0.25 m
Crest Crown Height = 0.02 m
```

Validation gate:

- confirm compilation and one combined preview child with no collider;
- confirm topology remains 1,404 vertices and 1,728 triangles for 36 strokes;
- confirm more longitudinal variation than C4 and fewer simple `^` profiles;
- confirm pronounced centre valleys remain uncommon without erasing small irregularity;
- confirm first/last interior leg rows retain more visible body than C4;
- confirm exact endpoints still meet the ground with no upright wedge;
- reject rails, bars, roofs, blades, roots, wires, walls, obstacles, or a return to frequent two-hill profiles;
- continue to defer final shader and distribution work until this shape gate passes.

Descriptor/texture separation remains deferred to V3J.3D.

### 2026-07-11 — Patch V3J.3C4: Single-Crest Crowned Ribbon Validation Result

C4 retained the correct open crowned-ribbon architecture and removed the repeated `M` silhouette, but visual validation rejected its final shaping. The `0.70` guide blend plus hard monotonic rise/fall guards made most strokes look like simple `^` contours and suppressed too much seeded irregularity. The `[0.35, 1.00, 0.35]` crown factors added limited shoulder body, but the crown still followed the long macro Fold End Taper envelope, so the legs remained liable to disappear from side-biased views. C5 replaces only those over-strong/under-supported details.

### 2026-07-11 — Patch V3J.3C3: Grounded Crowned Crest Ribbon Validation Result

V3J.3C3 was validated as the retained representation direction after C2 removed the filled hill but exposed a flat-sheet limitation. Its three-vertex crown produced real cross-sectional body, stronger lighting variation, and much better gameplay readability. The validation did not yet close the shape gate because raw longitudinal variation frequently produced a centre dip between two higher regions, while the zero-crown shoulders allowed the start/finish legs to disappear from side views. C4 first refined those two points without changing the accepted open crowned-ribbon architecture; C5 now corrects C4's over-strong profile shaping and weak terminal support.

Implemented geometry contract:

```text
longitudinal crest source:
  unchanged 5-position stochastic profile search

longitudinal resolution:
  unchanged minimum 13 rows

visible cross-section:
  3 vertices across
  left and right shoulders use the generated macro crest height
  centre uses macro crest height + Crest Crown Height
  2 sloped faces per row span

ground connection:
  existing end envelope retained
  crown height multiplied by the same end envelope
  only first and final rows embed by 0.002 m

excluded:
  no side walls
  no underside
  no caps
  no collider
  no base-ground mutation
```

New/extended authoring contract:

```text
Stroke Width range:       0.01–0.35 m
Fold Height range:        0–0.50 m
Crest Crown Height range: 0–0.05 m
Crest Crown initial value: 0.02 m
```

The Fold Height serialized default remains `0.018 m`. The style asset is not part of the patch. `0.25 m` is the focused Fold Height proof baseline, not an automatic asset migration.

For 36 strokes, expected topology is 1,404 vertices and 1,728 triangles. Estimated raw storage is 55,296 bytes with the existing 32-byte vertex estimate and 16-bit indices. The build log must report `requestedCrownHeight`, `ribbonVerticesAcross=3`, macro crest peak min/mean/max, crown peak min/mean/max, combined peak min/mean/max, effective width, material state, topology, memory, and build time.

Focused validation keeps shape variables isolated:

```text
Stroke Width = 0.02 m
Fold Height  = 0.25 m

Test A: Crest Crown Height = 0.01 m
Test B: Crest Crown Height = 0.02 m
Test C: Crest Crown Height = 0.03 m
```

Validation result:

- one combined preview child, visual-only topology, and no base mesh/collider mutation remained intact;
- the crown faded at the endpoints and materially improved visible form over C2;
- the geometry direction was retained;
- multiple strokes showed an unwanted two-hill `M` silhouette;
- shoulders and legs remained too thin because only the centre received crown offset;
- distribution and final shader work remain deferred while C5 completes the isolated profile-variety and leg-support corrections.

Descriptor/texture separation remains deferred to V3J.3D.

### 2026-07-11 — Patch V3J.3C2: Grounded Open Crest Ribbon Validation Result

V3J.3C2 was validated as an open-under-crest secondary-geometry proof. The patch changed only the preview representation and its authoring/documentation contract. Descriptor generation, projected/debug rasterization, the style asset, shaders, base ground mesh, base collider, scenes, and unrelated systems remained unchanged.

Implemented geometry contract:

```text
crest source:
  evaluate the existing stochastic profile at 5 cross positions
  keep the maximum normalized height for each longitudinal row

visible ribbon:
  2 vertices across
  width remains stroke.Width
  each side independently samples ground height and render normal
  both sides receive the same row crest height

longitudinal resolution:
  minimum 13 rows

ground connection:
  existing deterministic end envelope retained
  width retains the non-zero terminal scale
  only first and final vertex pairs embed by 0.002 m

removed filled representation:
  no cross-width surface grid
  no side-boundary embedding
  no underside
  no collider
```

For 36 accepted strokes at thirteen rows, the proof must report 936 vertices and 864 triangles. Estimated raw mesh storage is 35,136 bytes using the existing 32-byte vertex estimate and 16-bit indices. The diagnostic fields are `crestSearchSamples=5`, `ribbonVerticesAcross=2`, `longitudinalSamplesMin/Mean/Max`, `crestPeakHeightMin/Mean/Max`, effective width, material state, topology, memory, and build time.

The Fold Height authoring range is temporarily `0–0.25 m`; serialized defaults and `GSSP_Snowfield.asset` are unchanged. Controlled validation heights are:

```text
0.12 m
0.18 m
0.24 m
```

Validation result:

- stroke placement, length, facing, signed jitter, and width semantics remained stable;
- each mark started on the ground, rose through intermediate rows, and returned to the ground;
- no Painted Accent surface filled the area underneath;
- one child mesh, one renderer, no collider, and no base-ground/collider mutation were retained;
- `0.12 m` and `0.18 m` logs reported 936 vertices, 864 triangles, and mean macro peaks `0.1140 m` and `0.1709 m`;
- visual tests at `0.12`, `0.18`, and `0.24 m` rejected the final representation because two equal-height vertices across form a flat raised sheet with weak lighting variation and distance visibility.

V3J.3C3 adds only the missing narrow cross-sectional crown. V3J.3D descriptor/texture separation remains deferred until the representation decision is made.

### 2026-07-11 — Patch V3J.3C1: Ridge Readability Calibration Result

C1 validation proved monotonic height response and stable topology but rejected the filled five-wide surface. Requested heights `0.02`, `0.06`, and `0.12 m` produced mean generated peaks `0.0184`, `0.0552`, and `0.1105 m` while remaining at 36 strokes, 1,260 vertices, 1,728 triangles, and stable effective width. At `0.12 m` the height was clearly visible, but the fully triangulated cross-width surface read as a hill. The open crest ribbon in V3J.3C2 addresses that exact representation failure rather than adding more height or changing descriptor generation.

### 2026-07-10 — Patch V3J.3C: Narrow Static Secondary Ridge Reconciliation

V3J.3C is the active implementation patch. It keeps the validated V3J.3A4 stroke distribution and orientation unchanged and replaces only the V3J.3B broad visible fold footprint.

Implemented geometry contract:

```text
visible half width:
  stroke.Width * 0.5

visible BodyWidth contribution:
  none

cross-section samples:
  7

surface:
  narrow open ridge
  no underside
  no end caps

boundary treatment:
  side boundaries embedded 0.002 m below sampled ground
  start/end rings embedded 0.002 m below sampled ground
  width tapers toward a small non-zero terminal scale
  height tapers to zero through the existing deterministic end envelope
```

The existing stochastic profile is retained but is evaluated only inside the narrow visible width. `Fold Broadness` is removed from author-facing controls, ridge calculations, signatures, serialized recipe data, and current style assets because the audit found no remaining consumer.

Renderer and mesh contract:

```text
one child per GeneratedGround:
  __PaintedAccentRidgePreview_Debug

one MeshFilter
one MeshRenderer
one combined mesh containing every accepted stroke
one shared neutral/dark lit material
no collider
ShadowCastingMode.Off
receiveShadows = true
motion vectors set to camera-only
```

The proof vertex layout is position + normal + UV0, with no tangents or vertex colours. The mesh uses 16-bit indices unless vertex count exceeds 65,535. Every explicit build logs actual stroke, vertex, triangle, estimated vertex-buffer, estimated index-buffer, total raw mesh bytes, and elapsed milliseconds.

The base `GeneratedGround` mesh and `MeshCollider` are never changed by this feature. The editor preview remains CPU-readable. Production integration is deferred to the camp/run-loading phase, where static meshes may call `UploadMeshData(true)` after upload. There must be no calls from `Update`, `LateUpdate`, `FixedUpdate`, render callbacks, or other gameplay-time polling paths.

Known V3J.3C limitation: the explicit ridge build still obtains stroke descriptors through `EnsurePaintedAccentFoldFieldCurrent()`, whose current generator also allocates/bakes the projected fold-field texture. V3J.3C does not perform a risky generator split. V3J.3D must separate descriptor generation from optional texture rasterization before production camp/loading integration so geometry-only chunks do not retain an unnecessary production texture.

Current decision:

```text
active candidate:
  narrow static secondary ridge geometry

fallback:
  projected/baked shader response from the same stroke descriptors

rejected default:
  broad ground apron
  full closed tube
  per-line GameObjects/renderers
  gameplay-time generation
```

Focused validation:

1. Unity compiles.
2. `Build 3D Ridge Preview` creates exactly one separate visual child.
3. The base mesh and collider remain unchanged.
4. The broad `BodyWidth` apron is gone.
5. Width responds only to `Stroke Width`.
6. Height and irregularity still change real 3D form.
7. Side and end boundaries disappear into the ground without caps.
8. Distribution, length, facing direction, signed jitter, and seed determinism are unchanged.
9. Console diagnostics report actual geometry and build cost.
10. From the gameplay camera, marks read as tiny terrain accents rather than miniature hills, roots, wires, worms, or pipes.

### 2026-07-10 — Patch V3J.3B: Stochastic 3D Fold Surface Preview

Patch V3J.3B converts the validated flat 3D stroke preview into the first actual raised-form proof. Stroke placement, density, length, facing-direction perpendicularity, and signed angle jitter remain unchanged from V3J.3A4. The patch changes only the preview surface and its explicit profile controls.

The implementation deliberately rejects a fixed semantic cross-section such as “outer edge / shoulder / crest / shoulder / outer edge.” Instead, every preview vertex samples one generic parametric surface:

```text
t = normalized distance along the accepted 3D stroke
u = normalized distance across the fold, -1..1

P(t, u) =
    GroundPoint(t)
  + SurfaceAcross(t) * u * HalfWidth
  + SurfaceNormal(t) * Height(t, u)
```

`Height(t, u)` is generated from a deterministic smooth Gaussian-basis mixture. Each stroke seed produces one to four broad bases with independent center, width, amplitude, phase, and slow along-stroke evolution. The sum is normalized and multiplied by:

```text
EdgeEnvelope(u)           -> forces both side edges back to ground height
AlongStrokeVariation(t)   -> creates slow height/profile undulation
EndEnvelope(t)            -> blends both ends back to ground height
Fold Height               -> normal-space height scale in metres
```

No basis is named or treated as a crest or shoulder. The same formula can yield one broad offset rise, a flatter plateau, overlapping low rises, uneven slopes, or subtle local dips. `Fold Irregularity` controls basis count and parameter variation; `Fold End Taper` controls the length of the start/end blend. V3J.3C removes `Fold Broadness` from ridge authoring and ridge calculations because the broad-body footprint was the failure being corrected.

Code changes:

```text
GroundSurfaceFeatureRecipe
  historically added Fold Height, Fold Irregularity, Fold Broadness, Fold End Taper
  V3J.3C retains Height/Irregularity/End Taper and retires Broadness from ridge authoring
  corrects the generic Direction tooltip: Painted Accent orientation uses Facing Direction Degrees

GeneratedGround
  replaced the flat two-vertex ribbon preview with an 11-sample broad proof surface
  V3J.3C replaces that with a 7-sample narrow open ridge
  projects stroke tangents onto the sampled ground-normal plane
  generates per-stroke deterministic Gaussian profile bases
  re-samples ground height/normal at every lateral profile vertex
  samples stochastic height across and along every stroke
  recalculates mesh normals/tangents
  uses a lit debug material for readable 3D form
  replaced __FoldFieldLinePreview_Debug with __PaintedAccentFoldSurfacePreview_Debug at the V3J.3B stage
  V3J.3C now uses __PaintedAccentRidgePreview_Debug and clears both legacy preview names

GeneratedGroundEditor / GroundSurfaceStyleProfileEditor
  expose the four profile controls
  replace Build/Clear 3D Line Preview with Build/Clear 3D Fold Preview

GeneratedGround signature
  includes all four profile controls so editor changes invalidate the generated Painted Accent data deterministically
```

V3J.3B remains historical editor/debug proof context. V3J.3C resolves its decision gate: the base mesh/collider remain immutable, narrow secondary geometry is the active candidate, and baked projection is fallback only.

### 2026-07-10 — Patch V3J.3A4: Perpendicular Facing-Direction Fix

The validated orientation rule is:

```text
finalStrokeAngle =
    Facing Direction Degrees
  + 90 degrees
  + random(-Angle Jitter Degrees, +Angle Jitter Degrees)
```

The generic `Direction` vector is not used for Painted Accent stroke orientation. `Facing Direction Degrees` represents player/camera facing in local X/Z; the line is perpendicular to that direction; the jitter roll is deterministic and signed per stroke.

### 2026-07-10 — Patch V3J.3A3: Explicit Base-Angle Audit

V3J.3A3 exposed that V3J.3A2 still jittered around a hidden diagonal feature direction. It added an explicit authored angle, which V3J.3A4 immediately redefined correctly as facing direction plus a perpendicular conversion. This is historical context only; V3J.3A4 is the active contract.

### 2026-07-10 — Patch V3J.3A2: Explicit Signed Angle Jitter Degrees

Patch V3J.3A2 replaced normalized angle variety/orientation families with a direct signed degree control. Validation then proved that the roll was still centered around a hidden legacy direction. V3J.3A3 and V3J.3A4 supersede its base-angle semantics; only the deterministic signed-degree-roll requirement remains current.

### 2026-07-10 — Patch V3J.3A1: 3D Stroke Distribution Fix

Patch V3J.3A1 is a corrective patch for the first V3J.3A validation. The user confirmed density and length controls were useful, but the preview exposed two bugs: accepted strokes concentrated on one side of the chunk, and `Angle Variety` rotated strokes in unwanted orientation families instead of applying small symmetric variation around the preferred direction.

Code changes are intentionally narrow:

```text
GroundPaintedAccentFoldFieldGenerator
  replaces random-start row-major cell traversal with globally shuffled deterministic candidate cells
  accepts candidates from the shuffled whole-chunk list so target strokes are not filled by one contiguous region
  removes slash/vertical/backslash orientation families from ResolveStrokeAxis
  interprets the existing angle control as symmetric +/- jitter around feature.Direction

GroundSurfaceFeatureRecipe / Editors
  keep the serialized field for compatibility
  replace normalized angle-variety UI with explicit Angle Jitter Degrees semantics
```

No height-profile, raised fold body, lateral squiggle, shader response, or final-material work is part of this patch. Validation should check only whole-chunk distribution and symmetric angle behavior.

### 2026-07-10 — Patch V3J.3A: 3D Stroke Distribution Controls

Patch V3J.3A tuned the first 3D stroke preview without adding raised fold height. V3J.3R proved the code baseline could generate ground-following 3D stroke ribbons, but validation showed three layout problems: too few lines, strokes were too long, and orientation was too uniform. Lateral curvature was intentionally not treated as a bug; V3J.3B now owns the missing height/profile proof.

The patch adds explicit Painted Accent 3D stroke controls to `GroundSurfaceFeatureRecipe` and exposes them both in `GroundSurfaceStyleProfileEditor` and directly on the selected `GeneratedGround` inspector:

```text
Painted Accent Stroke Width
Painted Accent Stroke Density
Painted Accent Stroke Length Min
Painted Accent Stroke Length Max
Painted Accent Stroke Angle Jitter Degrees
```

The fold-field signature includes the layout controls, so changing density, length, facing direction, or angle jitter invalidates and rebuilds the generated stroke/fold-field data. The generator no longer relies on `Strength` for approximate count or generic `Scale` for stroke length. Density is an approximate target count per 40x40 patch and length min/max are direct metre controls. V3J.3A's preferred-direction wording is historical; V3J.3A4 established the active facing-direction-plus-perpendicular angle contract.

Generation also uses an expanded deterministic placement-attempt grid. This keeps placement stratified while making density more reliable after support-based rejection. At the V3J.3A stage the validation target was the temporary `Build 3D Line Preview`; V3J.3B historically superseded it with `Build 3D Fold Preview`, and V3J.3C now supersedes that broad proof with `Build 3D Ridge Preview`.

### 2026-07-10 — Patch V3J.3R: Painted Accent 3D Stroke Baseline Reconciliation

Patch V3J.3R is a reconciliation patch after V3J.0-V3J.2 proved that body/noise-first line inference is the wrong active baseline. The current healthy baseline is now 3D-line-first: generated Painted Accent strokes are short local-space surface curves sampled against `GroundHeightFieldSnapshot`; the fold-field texture is derived from those strokes instead of being the source from which lines are inferred.

Code reconciliation:

```text
GroundPaintedAccentFoldFieldGenerator
  removed the active continuous-noise / threshold / connected-component crest extraction path
  now generates stratified 3D surface stroke descriptors
  rasterizes R/G/B/A from those strokes

GeneratedGround
  stores generated `GroundPaintedAccentSurfaceStroke[]`
  originally replaced Build/Clear Height Preview with a temporary flat 3D line preview
  V3J.3B now replaces that temporary ribbon with the raised stochastic fold-surface preview

GroundSurfaceFeatureRecipe
  removes peak-threshold / min-area / crest-width proof controls
  keeps one active Painted Accent stroke width control

GroundSurfaceStyleProfileEditor / GeneratedGroundEditor
  remove obsolete threshold-region tuning UI
  expose only 3D stroke width in feature assets and on GeneratedGround for preview tuning
  originally exposed Build/Clear 3D Line Preview; V3J.3B now exposes Build/Clear 3D Fold Preview

SH_GroundFoldFieldHeightPreview
  removed as obsolete because the G-height grid preview is no longer the active diagnostic
```

Texture contract after V3J.3R:

```text
R = baked stroke-line coverage from generated 3D surface strokes
G = soft body/support around the strokes
B = stroke-relative signed side encoded 0..1
A = semantic support / reserved
```

The historical validation sequence first proved line density, length, and orientation through the temporary ribbon preview. V3J.3B now supplies the raised stochastic fold-surface preview required before deciding whether to promote the 3D result toward final rendering.

## Purpose

Define the implementation plan, patch history, and active technical roadmap for generated-ground surface work. The ground visual/design baseline is owned by `Ground_Visual_Design_and_Architecture.md`; this document implements that baseline and records how the code/assets are brought into alignment.

The current visual north star, defined in `Ground_Visual_Design_and_Architecture.md`, is:

```text
Restrained stylized terrain:
BOTW/TOTK-like base-material restraint
+ Hades-1-like painted ground accents
+ mostly 3D procedural geometry
+ reusable procedural masks and style layers instead of fully hand-painted floor art.
```

This does not mean copying any reference literally. It means borrowing the useful production grammar:

- from BOTW/TOTK: calm matte base ground, restrained noise, broad readable material regions, and scene complexity carried by geometry, lighting, props, vegetation, rocks, rivers, and atmosphere;
- from Hades 1: sparse authored-looking surface accents, short dark mound/crease lines, contact emphasis, decorative rhythm, and deliberate value grouping;
- from the existing PS3D framework: procedural masks, component-owned style authoring, shared material/property-block contracts, debugable semantic channels, and deterministic generated geometry.

The ground should remain mostly flat and combat-friendly. It should not become interesting through constant height noise, texture soup, or feature-by-feature simulation before the art language is proven. Instead, it separates and layers:

- playable shape;
- calm family/variant base material;
- broad macro patch composition;
- static semantic masks;
- reusable painted accent layers;
- contact and edge accent layers;
- sparse motif/stamp layers;
- runtime surface state later;
- future grass, snow, rain, mud, footprints, puddles, and material blending.

The desired result is a broad, readable stage floor whose surface feels designed: simple at rest, but enriched by meaningful patches, subtle hand-painted-looking accents, shore/contact response, compacted paths, damp low areas, snow or mud identity, and later runtime footprints or weather response.

## Current State

### Current Implementation Status After Patch V3E

The ground upgrade has moved beyond the original single snow-material improvement pass. The current system now has a real surface-style framework, and the design direction has pivoted from feature accumulation to a shared visual doctrine.

| Area | Status | Notes |
| --- | --- | --- |
| Ground visual doctrine | Canonical in `Ground_Visual_Design_and_Architecture.md` | The ground target is restrained stylized terrain: calm BOTW/TOTK-like base surfaces plus Hades-1-like painted accents, implemented through reusable procedural style layers. |
| Dedicated ground shader | Implemented | `SH_PixelGroundSurfaceLit.shader` owns ground rendering separately from generated masses. |
| Static semantic masks | Implemented baseline | Vertex color and UV2 carry tonal, exposure, damp/deposit, vegetation, compaction, shore, rocky/dry, and authored standing-water/puddle-potential data. |
| Ground/corridor material contract | Implemented | `GeneratedGround` resolves visual state and applies it by `MaterialPropertyBlock`; river corridors remain dependent renderers and must remain style-agnostic. |
| Component-owned surface authoring | Implemented | `GeneratedGround` exposes top-level Surface Family and Surface Variant controls. |
| Asset-backed visual families | Implemented baseline | `GroundSurfaceStyleProfile` assets own visual families such as Snowfield, Wet Mudflat, and Grassland. Families define surface identity; they do not define the global art language alone. |
| Asset-backed variants | Implemented baseline | `GroundSurfaceVariantRecipe` stores stable ids, display names, material controls, and feature recipes. Variants tune the shared style stack. |
| Feature-module recipe layer | Implemented stack baseline in Patch V3 | `GroundSurfaceFeatureRecipe` supports explicit cost classes. `GeneratedGround` now resolves the first enabled ShaderOnly recipe of each supported kind and writes explicit shader-property blocks, so variants can combine supported features. |
| Snowfield family | Implemented baseline | `GSSP_Snowfield` and `GSP_Snowfield` exist. Variants are calm baseline snow floors under the new doctrine. |
| Wet Mudflat family | Implemented baseline | `GSSP_WetMudflat` and `GSP_WetMudflat` exist. Patch Q reset the family to matte earth until explicit puddle/rut/debris features exist. |
| Grassland family | Implemented baseline in Patch V2B | `GSSP_Grassland` and `GSP_Grassland` add the missing living-ground baseline for shared feature validation. No vegetation rendering is included. |
| Style profile editor | Implemented in Patch R | Style assets have a readable custom editor with variant cards, feature summaries, duplicate support, and validation warnings. |
| Style asset live refresh | Implemented in Patch S | Editing a style asset can refresh open `GeneratedGround` users without manual scene rebuilds for material/property-block changes. |
| Ground modifier surface/height contract | Implemented in Patch T | `GroundModifier` can affect height, authored surface masks, or both; legacy Flatten compaction behavior is preserved. |
| TrampledWear proof feature | Implemented/prototyped in Patch U | `TrampledWear` reads `UV2.x` compaction/path. It is now considered an experiment/proof of the mask-to-feature route, not the active art-direction priority. |
| Runtime surface state | Deferred | Wetness, snow depth, compression, footprints, and trample maps remain future work. Runtime work must wait until the static visual language is validated. |
| Painted accent lines | Reconciled to 3D surface-stroke baseline in Patch V3J.3R | `PaintedAccentLines` remains the first stackable doctrine layer, but the active source of truth is now generated 3D ground-following surface strokes. Prior curve-distance, candidate-stamp, continuous-noise, shader-contour, and thresholded-region crest extraction paths are retired as active directions. |
| GeneratedGround debug views | Implemented in Patch V3B; dropdown cleanup in Patch V3C | Generated-ground debug selection is now exposed on the `GeneratedGround` component and written through renderer-local material property blocks. Material asset debug controls are fallback/internal only. |

Current conceptual split:

```text
GroundSurfaceProfile
  semantic / mask-generation profile

GroundSurfaceStyleProfile
  visual family asset

GroundSurfaceVariantRecipe
  variant recipe inside a visual family

GroundMaterialControls
  material / shader response recipe

GroundSurfaceFeatureRecipe
  optional feature-module recipe with explicit cost class

GeneratedGround
  resolver, top-level authoring surface, and per-object override owner
```

Future terrain families must be added as style/profile assets, not as new hardcoded `GeneratedGround` enum branches.

Primary implementation files:

- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/GroundGenerator.cs`
- `Assets/Game/Procedural/Ground/GroundModifier.cs`
- `Assets/Game/Procedural/Ground/GroundHeightFieldSnapshot.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceProfile.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceStyleProfile.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceVariantRecipe.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs`
- `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
- `Assets/Game/Procedural/Core/MeshData.cs`
- `Assets/Game/Procedural/Core/MeshBuilder.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverGroundSnapshot.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelCellVariation.hlsl`
- `Assets/Game/Demo/Materials/Ground/M_PixelFrozenDirt.mat`

Related art and system documents:

- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
- `Assets/Docs/Rock_Generated_Mass_Upgrade_Plan.md`
- `Assets/Docs/Proof of Concept/01_Visual_Language_and_Rendering.md`
- `Assets/Docs/Proof of Concept/05_Project_Application_Norse_Game.md`
- `Assets/Docs/Proof of Concept/06_Proof_of_Concept.md`

The original ground implementation already had useful foundations, and these remain relevant:

- `GroundRecipe` controls patch size, resolution, patch coordinate, transition slope, broad shape, roughness, surface detail, edge blending, and material variation.
- `GroundModifier` supports deterministic flatten, raise, and lower regions for authored traversal and scene composition.
- `StylizedRiverGroundSnapshot` lets rivers conceal broad ground below the dedicated river corridor.
- `GroundHeightFieldSnapshot` lets other systems sample pre-river height, normals, render normals, surface variation, and reserved material classification.
- `MeshData` supports vertex colors and optional UV2 data.
- `SH_PixelSurfaceLit.shader` already has generic pixel surface features such as broad variation, warped cell lookup, profile contrast, wetness, frost, semantic brightening/darkening, and material profile controls.

Original limitations that motivated this upgrade. Items marked as implemented are kept here as historical context rather than active blockers:

- [~] Ground shape and ground surface began coupled inside `GroundRecipe`; semantic and visual style ownership are now split, but future path/compaction work still needs clearer authored modifier rules.
- [x] `GroundProfile` only describes the heightfield family, not the material family. Material family ownership now lives in `GroundSurfaceStyleProfile`.
- [x] `BuildSurfaceMetadata` originally wrote one broad variation value and left material classification at `0`; it now writes semantic masks.
- [x] `BuildMeshData` originally wrote neutral vertex color channels; it now writes the documented vertex color/UV2 surface contract.
- [x] The ground originally had no object-owned material property block equivalent to `GeneratedMass`; `GeneratedGround` now applies resolved material controls through `MaterialPropertyBlock`.
- [x] The ground originally had no authored surface profile asset; `GSP_Snowfield`, `GSP_WetMudflat`, and `GSP_Grassland` now exist.
- [x] The ground originally had no static mask contract for snow potential, wetness potential, dirt/deposit, vegetation suitability, or terrain type blending; the baseline semantic contract now exists.
- [ ] The ground still has no runtime surface state texture for rain, footprints, snow compression, grass trampling, or mud/water accumulation; this is now deliberately deferred until the static visual language is proven.
- [~] Early material output read as pale, low-contrast procedural fuzz. Baseline Snowfield, Wet Mudflat, and Grassland now exist, but final detail should come from the shared visual stack: calm base, macro patches, painted accent lines, contact accents, and sparse motifs before niche runtime features.

## Design Constraints

The upgrade must:

- preserve mostly flat, combat-stable gameplay terrain;
- avoid camera/player bobbing caused by excessive height variation;
- keep `GroundProfile` useful for broad physical shape only;
- add a separate surface/material profile system;
- keep existing generated ground scenes valid;
- keep river handoff behavior intact;
- keep ground modifier behavior intact;
- support future grass, wind response, rain response, snow accumulation, player footprints, and terrain type selection;
- prefer deterministic generated masks for static terrain identity;
- reserve runtime maps for changing state such as wetness, snow depth, footprints, and grass compression;
- keep shader contracts explicit and documented;
- avoid a large biome/world streaming system in the first pass;
- make the first visible improvement possible without authored texture assets;
- preserve the new ground doctrine: calm base surfaces plus selective painted accents;
- use family/variant assets to tune the shared style stack rather than creating unrelated one-off feature silos;
- avoid high-frequency procedural noise as the primary source of visual interest;
- keep runtime surface state deferred until the static style pillars are proven.

The upgrade should not:

- turn the prototype ground into high-relief terrain;
- solve production terrain streaming;
- introduce destructible terrain;
- require a full vegetation system before surface profiles are useful;
- require final weather simulation before rain/snow channels are reserved;
- bake footprints into the generated mesh;
- treat every terrain type as a separate duplicated material;
- turn the generic pixel surface shader into an unreadable all-purpose monolith without contracts;
- chase Hades 2-level hand-painted floor production;
- rely on Tunic-like block/voxel simplicity as the main style target;
- make every ground family visually unrelated;
- build footprints, puddles, rain, grass trampling, or runtime wetness before the static ground language works.

## Ground Visual Doctrine - Restrained Stylized Terrain

The canonical generated-ground design baseline now lives in:

```text
Assets/Docs/Ground_Visual_Design_and_Architecture.md
```

That document is authoritative for:

- the BOTW/TOTK-like base + Hades-1-like accent direction;
- ground style pillars;
- reference interpretation;
- non-goals;
- the shared ground composition stack;
- family/variant interpretation;
- reusable style-layer architecture;
- static surface-mask contracts;
- the paused runtime-state policy;
- acceptance criteria and drift-prevention rules.

This implementation plan should not duplicate the full doctrine. It should reference the ground design document and focus on concrete implementation state, patch sequencing, known limitations, and validation.

Implementation shorthand retained here:

```text
Restrained stylized terrain
= calm base surfaces
+ broad macro patch composition
+ semantic mask response
+ Hades-1-like painted accent lines
+ contact / edge accents
+ sparse motifs
+ runtime state later.
```

Patch work that changes the visual doctrine, static mask contract, family/variant meaning, feature-layer taxonomy, or runtime-state priority must update both documents together.

## Active Roadmap After Style Doctrine Pivot

The old Patch V-Z runtime roadmap is paused. It was coherent technically, but it is now the wrong priority because the ground art direction must be proven before more niche simulation/features are added.

Patch T and Patch U remain useful:

- Patch T established the authored surface-mask contract.
- Patch U proved that a feature can consume `UV2.x` compaction/path in the shader.

However, `TrampledWear` is now classified as a proof/experiment, not the next visual cornerstone. The active roadmap is now style calibration and shared doctrine layers.

| Priority | Patch | Concrete goal |
| --- | --- | --- |
| 1 | Patch V0 — Ground Visual Doctrine Documentation | Completed. `Ground_Visual_Design_and_Architecture.md` now owns the sacred ground design baseline; this implementation plan records technical alignment. |
| 2 | Patch V1 — Style Calibration Setup | Completed as a temporary `Style Calibration` surface family with four comparison variants: Calm Base, Hades Accent Proxy, Hybrid Target Proxy, and Pixel-Faceted. |
| 3 | Patch V2 — Base Ground Simplification | Implemented as an asset/docs retune. Snowfield and Wet Mudflat now use calmer matte bases with lower pixel variation, lower patch contrast, and reduced broad noise so future accents can sit on top. |
| 4 | Patch V2B — Grassland Baseline Family | Implemented as a production `Grassland` family with Clean, Patchy, Damp, and Worn Meadow variants. Establishes the canonical three-family test set. |
| 5 | Patch V3 — Shader Feature Stack + Painted Accent Lines | Implemented. Variants now use a real shader feature stack, and Painted Accent Lines are the first stackable doctrine layer. |
| 6 | Patch V3M — Broad Macro Patch Completion | **Accepted through V3M-A1.3.4.** See `Ground_Macro_Patch_Audit_and_Architecture.md`. |
| 7 | Patch V3R — Ground Elevation Readability | **Accepted through V3R-A1.** Cheap value-only relief and relative-height cues; no geometry or lighting-normal change. |
| 8 | Patch V3S — River-Coupled Ground Response | **Accepted through A4B.3.** Riverbed Support, Bank/Riverbed composition, cover retreat, hydrology, submerged finish, and waterline transition are frozen. Reusable material detail moved to generic GSU-M1; family acceptance follows material expansion. |
| 9 | Patch V4 — Contact / Edge Accent Layer | **Queued after V3S.** GeneratedMass grounding plus explicitly participating GroundModifier boundaries only. River is excluded. See `Ground_Contact_Edge_Accent_Audit_and_Architecture.md`. |
| 10 | Patch V5 — Sparse Motif Layer | Add reusable sparse marks such as chips, cracks, scuffs, stains, snow scratches, stones, or debris hints. Avoid stamp spam. |
| 11 | Patch V6 — Feature Stack Authoring Polish | Add richer warnings, cost summaries, duplicate/combination guidance, and editor UX after more stack layers exist. |
| 12 | Later | Ground Surface Runtime State Stub | Revisit runtime wetness, snow depth, compression, footprints, and disturbance after the static visual stack is accepted. |
| 13 | Later | Footprints / Rain / Puddles / Grass Integration | Build on the runtime state contract only after the visual doctrine is stable. |
| 14 | Future | Mixed Terrain / Profile Blending | Add explicit support for blended surface families such as snow over mud, rocky scrub over soil, or worn path through snow. |

### Paused runtime roadmap

The following patches are no longer the immediate queue:

```text
Old Patch V — Ground Surface Runtime State Stub
Old Patch W — Footprint / Compression Prototype
Old Patch X — Rain / Wetness Prototype
Old Patch Y — Style/Feature Authoring Polish
Old Patch Z — Grass Integration Contract
```

They are not rejected. They are deferred because building them before the static style works invites drift and overengineering.

### Surface modifier note

- Surface-only masks are preferred when the same visual effect can be achieved without changing playable height.
- Small denivelations are acceptable for roads, wagon tracks, camp pads, puddle basins, and other authored terrain features when they remain combat-safe and camera-stable.
- Snow paths and grass paths should eventually come from snow/grass accumulation and runtime interaction systems, not be hard-baked into the base ground as final content.
- Patch T inspected the current `GroundModifier` and ground mask code before implementing the path.

## Superseded Implementation Plan Notes

The original Patch 1-12 implementation plan has been superseded by the completed Patch J-V0 work and the active doctrine roadmap above. It is no longer the active queue and should not be used to decide next work.

Historical mapping:

| Old concern | Current status |
| --- | --- |
| Separate physical shape from surface identity | Implemented through `GroundSurfaceProfile`, `GroundSurfaceStyleProfile`, variants, and `GroundModifier` surface/height split. |
| Static surface mask contract | Implemented baseline through vertex color and UV2 channels. |
| Ground material property block | Implemented through `GeneratedGround` material/property-block resolver. |
| Dedicated ground shader | Implemented as `SH_PixelGroundSurfaceLit.shader`. |
| Terrain profile asset set | Implemented baseline with Snowfield, Wet Mudflat, and Grassland. |
| Runtime state design | Deferred after doctrine pivot. Contract remains documented, but implementation is no longer the immediate milestone. |
| Footprints / rain / grass | Deferred until the static visual doctrine stack works. |
| Mixed terrain/profile blending | Future work. |

Active implementation work must follow `Active Roadmap After Style Doctrine Pivot`, not the old Patch 1-12 list.

## Patch V1 - Style Calibration Setup

Patch V1 creates a temporary development surface family for screenshot-based style comparison.

Changed assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_StyleCalibration.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_StyleCalibration.asset
```

`GSP_StyleCalibration` is a neutral semantic/mask-generation profile. It provides a common mask baseline for all calibration variants so the comparison is mostly about visible material/style tuning.

`GSSP_StyleCalibration` is a `GroundSurfaceStyleProfile` discovered by the existing `GeneratedGround` style-family dropdown because the editor searches:

```text
Assets/Game/Demo/Profiles/Ground/Styles
```

The family contains four variants:

| Variant id | Display name | Intent |
| --- | --- | --- |
| `calibration.calm_base` | Calm Base | Restrained BOTW/TOTK-like base-material lane. Low noise, matte finish, broad soft patches, no feature recipe. |
| `calibration.hades_accent_proxy` | Hades Accent Proxy | Stronger Hades-1-like surface rhythm using the existing `DirectionalStreaks` shader-only feature as a temporary proxy. |
| `calibration.hybrid_target_proxy` | Hybrid Target Proxy | Likely doctrine target: calm base plus restrained accent rhythm. Uses a weaker `DirectionalStreaks` proxy. |
| `calibration.pixel_faceted` | Pixel-Faceted | Pushes existing PS3D pixel/faceted material identity harder for comparison. No new feature recipe. |

Implementation boundaries:

- No new code.
- No shader changes.
- No scene changes.
- No runtime state.
- No new materials.
- No river code changes.
- No final painted accent line implementation yet.

Patch V1 is intentionally a calibration patch. It gives the project a controlled way to choose the next visual lane before Patch V2 base simplification and Patch V3 painted accent lines.


## Patch V2 - Base Ground Simplification and Calibration Cleanup

Patch V2 applies the first screenshot-driven doctrine correction after the V1 calibration pass.

Calibration findings recorded by this patch:

- `Calm Base` is the strongest foundation: readable, restrained, and appropriate as the stage floor.
- `Hades Accent Proxy` and `Hybrid Target Proxy` support the direction philosophically, but the existing `DirectionalStreaks` proxy does not create convincing Hades-1-like painted crease lines. Real accent lines remain Patch V3.
- `Pixel-Faceted` is useful as an anti-reference for the default ground style. Global pixel/faceted noise becomes too busy and should not be the primary ground read.

Patch V2 therefore retunes the real production families toward the accepted base doctrine:

```text
calm matte base
+ restrained broad patches
+ lower pixel/faceted noise
+ subtle feature response
+ no final painted accents yet
```

Changed assets:

```text
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_StyleCalibration.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Snowfield.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_WetMudflat.asset
```

Concrete asset changes:

- Renamed the calibration display label from `Pixel / Faceted` to `Pixel-Faceted` so Unity does not treat the slash as a submenu path.
- Kept the stable variant id `calibration.pixel_faceted` unchanged.
- Reduced Snowfield pixel variation, pixel effect strength, cell warp, patch blend, and overly strong directional streaks.
- Reduced Wet Mudflat pixel variation, pixel effect strength, cell warp, damp darkening, patch blend, and pooled/trampled feature intensity.
- Kept Wet Mudflat matte; no glossy puddle or water material behavior was added.
- Kept all feature work static and shader/material-control driven; no runtime state was introduced.

Patch V2 does not implement:

- real `PaintedAccentLines`;
- contact/edge accents;
- sparse motifs;
- feature-stack aggregation;
- runtime wetness, snow compression, footprints, rain, puddles, grass suppression, roads, or wagon tracks;
- new shader properties, components, scene changes, or river logic.

The success condition is not that the ground already looks like Hades. The success condition is that Snowfield and Wet Mudflat become calm, readable stage floors that can accept future painted accents without fighting base noise.

## Patch V2B - Grassland Baseline Family

Patch V2B adds a real production `Grassland` family before Patch V3 painted accent lines.

Reason:

```text
Snowfield   = pale, cold, soft, low-value ground
Wet Mudflat = dark, earthy, damp, matte ground
Grassland   = green/olive, living, medium-value ground
```

Future shared doctrine layers need this three-family test set. Testing only snow and mud biases the system toward extreme pale/dark materials and leaves no medium-value living-ground baseline.

Changed assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_Grassland.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Grassland.asset
```

`GSP_Grassland` semantic intent:

- high vegetation suitability;
- moderate damp/deposit potential;
- low snow eligibility;
- moderate footprint visibility;
- soft broad tonal patches;
- moderate rain absorption;
- high grass recovery speed for later vegetation/runtime systems.

`GSSP_Grassland` variants:

| Variant id | Display name | Intent |
| --- | --- | --- |
| `grassland.clean_meadow` | Clean Meadow | Calm muted olive meadow baseline. Low noise and broad soft variation. |
| `grassland.patchy_meadow` | Patchy Meadow | Slightly more exposed earth/olive patching. Still calm and non-speckled. |
| `grassland.damp_meadow` | Damp Meadow | Cooler/darker green for river-adjacent or damp living ground. Uses a tiny `PooledWetness` proof response, not real puddles. |
| `grassland.worn_meadow` | Worn Meadow | Browner, compressed/path-capable meadow. Uses restrained `TrampledWear` so authored compaction can be tested on grassland. |

`Style Calibration` remains a temporary development family. It is not Grassland and should not be treated as production content.

Patch V2B does not implement:

- grass blades or vegetation placement;
- foliage density maps, wind animation, physics, or trampling;
- painted accent lines;
- contact/edge accents;
- sparse motifs;
- feature-stack aggregation;
- runtime wetness, snow compression, footprints, rain, puddles, roads, or wagon tracks;
- new shader properties, components, scene changes, or river logic.

The success condition is that Grassland becomes a calm third baseline for upcoming shared feature work, not that it already looks like finished grass or foliage.

## Patch V3 - Shader Feature Stack and Painted Accent Lines

Patch V3 corrects the feature architecture and adds the first real doctrine-layer visual feature.

### Feature stack contract

The serialized asset model was already list-based:

```text
GroundSurfaceVariantRecipe.features
```

Patch V3 makes the renderer honor that list as a stack:

```text
variant feature list
  -> first enabled ShaderOnly recipe per supported kind
  -> explicit MaterialPropertyBlock block per feature kind
  -> shader applies all supported layers in stable renderer-defined order
```

Supported ShaderOnly feature kinds after Patch V3:

```text
DirectionalStreaks
PooledWetness
TrampledWear
PaintedAccentLines
```

Rules:

- features are not mutually exclusive by default;
- first enabled recipe of each kind wins;
- duplicate enabled same-kind recipes are authoring mistakes;
- unsupported feature kinds may remain serialized but do not render;
- non-ShaderOnly cost classes remain reserved until their renderer path exists;
- shader composition order is not controlled by the asset list order;
- `_GroundFeatureMode` is a deprecated proof-feature compatibility property and must not be extended with new modes.

### Painted Accent Lines

`PaintedAccentLines` is a shader-only, visual-only layer for Hades-1-like ground accents.

It creates short, broken, slightly curved, dark/value-shifted stroke masks from world-space procedural cells and semantic mask gating. Patch V3D refines the raw mask after validation showed the first implementation produced large isolated strips. Patch V3E then changes the primitive from straight/micro-bar strokes into short curved terrain-fold strokes with a soft signed relief body. Scale controls accent spacing/grouping rather than raw stroke length; stroke length and thickness are capped in world units. It is intended to suggest:

```text
grass folds
mud creases
snow wrinkles
small mound lines
soft contour breaks
surface age
```

It explicitly does not add:

```text
textures
decals
height deformation
mesh changes
runtime state
footprints
puddles
grass blades
contact accents
sparse motif stamps
```

### Style asset usage

Patch V3 adds `PaintedAccentLines` recipes to the canonical families:

```text
Snowfield
Wet Mudflat
Grassland
```

It also updates Style Calibration's Hades/Hybrid lanes to use real Painted Accent Lines instead of relying only on DirectionalStreaks as a proxy.

This enables combinations such as:

```text
grassland.damp_meadow
  PooledWetness
  PaintedAccentLines

grassland.worn_meadow
  TrampledWear
  PaintedAccentLines

mudflat.trampled
  TrampledWear
  PaintedAccentLines

snowfield.wind_scoured
  DirectionalStreaks
  PaintedAccentLines
```

## Validation Plan

Validation must happen from the actual isometric/gameplay camera first. Close editor inspection is secondary. The ground is successful only if it reads as a coherent stage for characters, rivers, rocks, props, combat telegraphs, and atmosphere.

### Style Calibration Validation

Checklist:

- [ ] Select `GeneratedGround` -> `Surface Family = Style Calibration`.
- [ ] Test `Calm Base`, `Hades Accent Proxy`, `Hybrid Target Proxy`, and `Pixel-Faceted` from the same camera.
- [ ] Confirm the calm base surface is readable without looking empty.
- [ ] Confirm broad macro patches are visible but not noisy.
- [ ] Confirm the Hades and Hybrid proxy accents suggest useful authored ground rhythm without becoming procedural hatching.
- [ ] Do not expect final painted accent lines, contact accents, or sparse motifs in V1; those remain queued for V3-V5.
- [ ] Confirm pixel/faceted variation is clearly visible only in the Pixel-Faceted lane.
- [ ] Confirm ground detail does not compete with characters, VFX, hazards, dialogue presentation, or river foam.


### Base Simplification Validation

Checklist:

- [ ] Confirm the calibration variant appears as `Pixel-Faceted`, not as a nested dropdown.
- [ ] Test `Snowfield` variants from the gameplay camera and confirm they are calmer and less noisy than before.
- [ ] Test `Wet Mudflat` variants from the gameplay camera and confirm they remain matte, broad, and low-noise.
- [ ] Confirm `Wet Mudflat -> Trampled` still responds to compaction/path masks, but does not make trampled wear the main visual foundation.
- [ ] Confirm `Wind-Scoured` remains directional but no longer dominates as a fake final accent-line solution.
- [ ] Confirm the base ground may look plain; that is acceptable until Patch V3 adds real painted accent lines.

### Grassland Baseline Validation

- [ ] Confirm `GeneratedGround` exposes `Surface Family = Grassland`.
- [ ] Test `Clean Meadow`, `Patchy Meadow`, `Damp Meadow`, and `Worn Meadow` from the gameplay camera.
- [ ] Confirm Grassland reads as calm muted living ground, not a grass-blade/foliage system.
- [ ] Confirm `Damp Meadow` remains matte and does not look like puddles or glossy wet grass.
- [ ] Confirm `Worn Meadow` can be used with compaction/path masks without becoming the main style foundation.
- [ ] Confirm the river corridor still follows the selected Grassland style.
- [ ] Confirm river corridor material sync still follows the selected ground style.

### Painted Accent Lines / Feature Stack Validation

- [ ] Confirm Unity compiles after Patch V3.
- [ ] Confirm style assets can contain multiple enabled ShaderOnly features without being treated as invalid.
- [ ] Confirm `Snowfield -> Wind-Scoured` still shows DirectionalStreaks and also receives Painted Accent Lines.
- [ ] Confirm `Wet Mudflat -> Trampled` still responds to compaction/path masks and also receives Painted Accent Lines.
- [ ] Confirm `Grassland -> Damp Meadow` can show PooledWetness and Painted Accent Lines together.
- [ ] Confirm `Grassland -> Worn Meadow` can show TrampledWear and Painted Accent Lines together.
- [ ] Select `GeneratedGround -> Ground Debug -> Debug View -> Ground Painted Accent Lines` and confirm the raw line mask is visible.
- [ ] Confirm the raw mask uses small clustered strokes, not large isolated bars/crescents.
- [ ] Confirm line scale changes spacing/grouping without creating huge strokes.
- [ ] Confirm Snowfield is not scratched everywhere.
- [ ] Confirm Wet Mudflat does not turn into crack/noise texture.
- [ ] Confirm Grassland does not turn into grass-blade hair.

### Object-Level Ground Debug Validation

- [ ] Confirm `GeneratedGround` Inspector shows `Ground Debug -> Debug View`.
- [ ] Switch to `Ground Compaction Path` from the `GeneratedGround` Inspector and confirm the mask appears without editing the material asset.
- [ ] Switch to `Ground Painted Accent Lines` from the `GeneratedGround` Inspector and confirm the raw accent-line mask appears.
- [ ] Press `Clear Debug View` and confirm normal rendering returns.
- [ ] Confirm changing debug views refreshes material properties only and does not regenerate the mesh.


### Gameplay Validation

Checklist:

- [ ] Walk across the patch without distracting vertical bob.
- [ ] Fight or simulate combat movement on the patch.
- [ ] Verify hit/telegraph readability over the ground.
- [ ] Verify bridge and river crossing remain clear.
- [ ] Verify camera does not need to chase tiny height changes.
- [ ] Verify flatten/lower/raise modifiers still preserve playable spaces.
- [ ] Verify surface-only modifiers can change masks without changing height.

### Technical Validation

Checklist:

- [ ] Regenerate ground in edit mode.
- [ ] Change surface family and variant and verify material updates.
- [ ] Edit a style profile asset and verify open generated grounds refresh as expected.
- [ ] Change shape seed and verify selected style state persists.
- [ ] Verify `MeshData.Validate` passes.
- [ ] Verify UV2 count matches vertex count when used.
- [ ] Verify material property blocks do not instantiate materials.
- [ ] Verify no river corridor material-sync regressions.
- [ ] Verify shader compiles in URP.

### Debug Validation

Checklist:

- [ ] Inspect tonal patch mask.
- [ ] Inspect exposure/snow-hold mask.
- [ ] Inspect damp/deposit mask.
- [ ] Inspect vegetation suitability mask.
- [ ] Inspect shore influence mask.
- [ ] Inspect compaction/path influence mask.
- [ ] Inspect standing-water/puddle-potential mask.
- [ ] Inspect painted accent line mask from `GeneratedGround -> Ground Debug`.
- [ ] Confirm debug view changes do not regenerate terrain or require opening material assets.
- [ ] Inspect runtime wetness/snow/compression only after runtime state exists.

## Suggested Initial Tuning

The first style-calibration goal is not final snow, mud, grass, or path quality. The goal is to find the correct balance between calm base ground and selective authored-looking accents.

For the current prototype clearing:

- lower physical height detail before increasing shader detail;
- make base material response matte and restrained;
- keep smoothness/specular conservative, especially for mud;
- make material patch scale larger than individual mesh cells;
- reduce reliance on tiny pixel/noise variation;
- use broad cold/warm or pale/damp land patches for composition;
- keep accent marks sparse enough that some ground remains quiet;
- add stronger detail near shore/contact/path boundaries before distributing detail everywhere;
- test from game camera before judging close-up editor screenshots.

Possible starting values for a calm base pass:

```text
Ground shape
BroadForm: 0.25 to 0.95 for combat-safe uneven fields
Roughness: 0.15 to 0.40
SurfaceDetail: 0.02 to 0.12

Base surface response
PatchScale: 7 m to 18 m
PatchContrast: 0.10 to 0.28
PatchEdgeSoftness: 0.40 to 0.75
PixelVariation: 0.00 to 0.04 unless testing Pixel/Faceted lane
BroadVariation: 0.03 to 0.10
Smoothness: low unless the feature is explicit water/ice/wet stone
SpecularStrength: low for mud/soil/snow baselines

Painted accent line first target
Density: low
Contrast: low-to-medium
Length: short
Distribution: clustered
Curvature: subtle
Masking: biased by macro patches, shore/contact, and family tuning
```

These are only starting ranges. The real test is whether the ground looks deliberately simple rather than unfinished, and accented rather than noisy.

## Open Questions

Resolved by current architecture:

- Ground now has a dedicated shader path.
- Surface family/variant authoring now lives on `GeneratedGround` with style assets.
- Surface-only modifier authoring now belongs in `GroundModifier` through `GroundModifierSurfaceEffectMode`.
- The first semantic mesh channel contract is established.

Still open after the doctrine pivot:

- Should style calibration use a temporary `GSSP_Calibration` family, or should Snowfield/Wet Mudflat receive explicit calibration variants?
- What is the minimum shader/control change needed for `PaintedAccentLines` to feel hand-authored rather than procedural?
- Should painted accent lines be generated entirely in shader from world-space noise, from a baked/generated mask, or from a cheap hybrid?
- Which existing masks should bias accent-line density first: tonal patch, damp/deposit, shore, compaction, or modifier priority?
- How should contact/edge accents be sourced for generated masses and props: existing placement data, ground modifiers, object stamps, or a later contact-mask bake?
- How much additional editor UX is needed now that Patch V3 implements the first shader feature stack?
- How many doctrine-layer controls should be exposed in `GroundSurfaceStyleProfileEditor` before the UI becomes cluttered?
- What debug views are needed for contact accents and sparse motifs after `GroundPaintedAccentLines`?
- How much pixel/faceted breakup should remain in the final style, if any?
- What runtime state resolution is needed later for footprints from the game camera, after the static style is validated?

## Risks

### Doctrine Drift

Risk:

- new work slides back into niche features, runtime systems, or one-off material tricks before the visual language is proven.

Mitigation:

- keep this document as the canonical baseline;
- require new ground features to state which doctrine pillar they serve;
- pause work that does not improve calm base, macro patches, painted accent lines, contact accents, sparse motifs, or the feature-stack resolver.

### Too Much Height Detail

Risk:

- terrain becomes visually richer but damages camera/player comfort.

Mitigation:

- keep physical height detail low;
- put most variety in static masks and shader response;
- validate from gameplay camera first.

### Procedural Noise Masquerading As Style

Risk:

- the ground looks busy but not authored.

Mitigation:

- lower noise frequency and contrast;
- prefer broad patches and sparse accents;
- cluster marks instead of distributing them uniformly;
- add debug views for doctrine layers.

### Hades Reference Overreach

Risk:

- the project tries to match Supergiant-level hand-painted terrain production.

Mitigation:

- copy Hades 1 ground grammar, not its full authored finish;
- implement reusable procedural accent layers;
- keep base ground simple and let geometry, lighting, props, rivers, rocks, and atmosphere carry the scene.

### Tunic Reference Misuse

Risk:

- the ground becomes too primitive because simple Tunic-like surfaces are treated as the target without the rest of Tunic's block/toy-world simplification.

Mitigation:

- use Tunic only for readability lessons;
- keep the main target as restrained stylized 3D terrain with higher organic/geometric complexity.

### Shader Becomes Too Broad

Risk:

- the ground shader accumulates unrelated rock, ground, weather, vegetation, and feature assumptions.

Mitigation:

- keep a dedicated ground shader path;
- document property contracts;
- organize shader code into doctrine-layer functions;
- expose debug modes.

### Feature Silo Accumulation

Risk:

- `DirectionalStreaks`, `PooledWetness`, `TrampledWear`, and future features become mutually exclusive one-off modes.

Mitigation:

- keep using the Patch V3 shader feature stack as the canonical composition path;
- require each feature recipe to map to a doctrine layer;
- do not reintroduce mutually exclusive feature modes that cannot coexist.

### Profiles Become Premature Biome System

Risk:

- too many terrain families are added before one looks good.

Mitigation:

- calibrate the doctrine on existing Snowfield, Wet Mudflat, and Grassland first;
- add new families only to test a specific style-layer need;
- defer production biome/world assembly.

### Runtime State Overbuild

Risk:

- footprint/weather infrastructure is built before the static visual stack is proven.

Mitigation:

- keep runtime state contract documented;
- implement texture allocation only after calm base, accent lines, contact accents, and feature-stack resolving are validated.

### Mask Ambiguity

Risk:

- channels mean different things to different systems.

Mitigation:

- keep a channel contract in code comments and this document;
- expose debug views;
- avoid reusing a channel for incompatible meanings.

## Deferred Work

Defer until the static doctrine stack is proven:

- ground runtime state component;
- detailed boot-shape footprints;
- rain/wetness accumulation and drying;
- snow compression runtime;
- puddle fluid simulation or puddle rendering;
- grass rendering implementation and trampling;
- roads/wagon-track spline system;
- mixed terrain/profile blending;
- production terrain streaming;
- destructible terrain;
- full biome graph;
- authored texture painting UI;
- erosion simulation;
- persistent save/load of runtime footprint and weather state;
- large-scale weather manager;
- snow depth geometry displacement;
- triplanar authored texture sets;
- terrain LOD system.

Do not treat deferral as rejection. These systems remain useful later, but they should inherit a proven ground language rather than define it prematurely.

## Definition of Done for First Doctrine Milestone

The first doctrine milestone is complete when:

- the docs define restrained stylized terrain as the canonical target;
- `GeneratedGround` still exposes family/variant authoring;
- existing Snowfield, Wet Mudflat, and Grassland variants are retuned or calibrated under the doctrine;
- the same clearing can demonstrate at least two style-calibration lanes, including the preferred hybrid target;
- the calm base reads as intentional from the game camera;
- broad macro patches are visible but not noisy;
- a first painted accent-line prototype creates sparse Hades-1-like crease/mound marks;
- contact/edge accents are either prototyped or explicitly queued as the next doctrine layer;
- ground remains combat-safe and camera-stable;
- river corridor material sync still works;
- semantic debug views still show the mesh channel contract;
- no runtime surface state has been added merely to compensate for an undecided static style.

## Working Checklist Summary

Active checklist after the doctrine pivot:

- [x] Patch T - establish surface/height modifier contract.
- [x] Patch U - prove compaction/path mask can feed a shader feature.
- [x] Patch V0 - document and lock the new ground visual doctrine.
- [x] Patch V1 - create style calibration setup.
- [x] Patch V2 - simplify and retune calm base ground.
- [x] Patch V2B - add Grassland baseline family and establish the three-family test set.
- [x] Patch V3 - implement shader feature stack and painted accent lines.
- [x] Patch V3C - fix object-level debug dropdown labels and Unity 6.5 obsolete editor refresh warning.
- [x] Patch V3D - refine Painted Accent Lines raw mask from large strips into smaller clustered micro-strokes.
- [x] Patch V3E - replace straight line stamps with curved visual-relief terrain-fold strokes.
- [x] Patch V3F - expose painted-accent relief channels and strengthen visual relief.
- [x] Patch V3F.1 - make relief continuity and signed-side debug readable; validation still rejects the curve-distance source model.
- [x] Patch V3G - retire the curve-distance stroke model and document the generated fold-field direction.
- [x] Patch V3H - add generated fold-field data skeleton and shader sampling fallback plumbing.
- [x] Patch V3I - prototype local-space 256x256 fold-field generation at ground regeneration/dirty time.
- [x] Patch V3I.1 - correct fold body shapes from oval stamps into curved tapered ridge/fold bodies.
- [x] Patch V3I.2A - retire candidate-stamp generator and document the continuous field plan.
- [x] Patch V3I.2 - implement continuous domain-warped fold height field generation.
- [x] Patch V3I.3 - add editor/debug-only fold-field height preview mesh.
- [x] Patch V3I.3A - isolate fold-field debug data from final render and improve preview readability.
- [x] Patch V3J.0 - add a debug-only Painted Accent final visual-response proof view.
- [x] Patch V3J.1 - replace the failed broad-mask prototype with shader-side contour extraction from the G/body field.
- [x] Patch V3J.2 - reconcile the failed shader-side approach by precomputing selected crest lines from thresholded G peak regions at generation/dirty time; validation still rejected the body/noise-first source model.
- [x] Patch V3J.3R - retire the active body/noise-first line-inference path and establish generated 3D surface strokes as the Painted Accent source of truth.
- [x] Patch V3J.3A - expose and implement 3D stroke density, length, width, and angle controls for the 3D line preview.
- [x] Patch V3J.3A1 - globally shuffle candidate cells so accepted strokes populate the whole chunk; remove orientation families.
- [x] Patch V3J.3A2 - expose explicit signed angle jitter in degrees; later superseded for base-angle semantics.
- [x] Patch V3J.3A3 - audit and expose the hidden base angle.
- [x] Patch V3J.3A4 - define the active facing-direction + 90 degrees + signed-jitter orientation contract.
- [x] Patch V3J.3B - replace flat line ribbons with deterministic stochastic raised 3D fold-surface previews.
- [ ] Patch V3K - decide whether the accepted raised 3D strokes become final geometry, baked shader response, or both.
- [ ] Patch V3L - tune Painted Accent Lines per production family after the 3D stroke baseline is accepted.
- [ ] Patch V4 - prototype contact/edge accents.
- [ ] Patch V5 - prototype sparse motif/stamp layer.
- [ ] Patch V6 - feature-stack authoring polish, warnings, and per-kind drawers.
- [ ] Later - resume runtime state design only after static doctrine validation.

Historical patch notes remain below for context.

### 2026-07-10 — Patch V3J.3R: Painted Accent 3D Stroke Baseline Reconciliation

Patch V3J.3R supersedes the V3J.2 active path. V3J.2 proved that threshold controls could affect the selected region set, but it also proved the wrong thing was being selected: the source G regions were noisy/blobby, and the inferred crest output read as fat ribbons rather than clean 2-3 pixel/line-like strokes.

The active implementation now generates the intended line first as 3D surface data:

```text
stratified stroke placement over the local ground bounds
  -> deterministic direction jitter around the feature direction
  -> ground-surface walking using GroundHeightFieldSnapshot
  -> sampled local 3D points and render normals
  -> editor/debug mesh ribbon preview
  -> baked R/G/B/A fold-field texture derived from those strokes
```

The removed active code includes the continuous fractal body field, percentile body shaping, thresholded peak mask, connected-component labeling, boundary-distance crest scoring, and soft crest rasterizer. Those ideas remain documented as failed experiments only.

The old height-preview workflow was retired in V3J.3R. Its temporary flat line-ribbon preview was then used through V3J.3A4 to validate placement, distribution, length, and orientation. V3J.3B introduced a broad raised proof surface; V3J.3C supersedes it with:

```text
GeneratedGround Inspector:
  Build 3D Ridge Preview
  Clear 3D Ridge Preview

Preview child object:
  __PaintedAccentRidgePreview_Debug
```

The active preview is a narrow open stochastic ridge sampled around the accepted 3D strokes. Field/noise-first line discovery remains retired, and `BodyWidth` no longer contributes to visible secondary geometry.

### 2026-07-10 — Patch V3J.2: Precomputed Peak-Region Crest Lines

Patch V3J.2 replaces the failed shader-side contour extraction model. Validation of V3J.1 showed that local G contour bands create embossed topo-map soup: many rings and smudges, not one chosen painted fold line per meaningful region. The accepted correction is to move regional reasoning to generation/dirty time.

The fold-field texture contract after V3J.2 is:

```text
R/line field      -> precomputed selected crest/accent line mask
G/body field      -> continuous fold body / peak source / weak context only
B/signed field    -> one-sided dark/light polarity
A/support field   -> semantic support / reserved validity
normal render     -> still isolated/unchanged
```

The generator now thresholds the smoothed G/body field, labels connected peak regions, rejects regions below `Painted Accent Minimum Peak Area`, computes a major axis per region, chooses one internal crest path using boundary distance plus body height, soft-rasterizes that path into R, and leaves the shader to shade R cheaply. The Final Prototype shader no longer samples neighboring G texels or extracts contour bands at runtime.

Temporary proof controls were added to `GroundSurfaceFeatureRecipe` and displayed for `PaintedAccentLines` in the style-profile editor:

```text
Painted Accent Peak Threshold
Painted Accent Minimum Peak Area
Painted Accent Crest Width Texels
```

Validation should focus on `Ground Painted Accent Lines` and `Ground Painted Accent Final Prototype`: lowering the threshold should admit more selected peak regions, raising it should reduce them, and the final prototype should shade only the selected R lines rather than the whole G field.

### 2026-07-10 — Patch V3J.1: Painted Accent Prototype Contour Extraction

Patch V3J.1 replaces the failed V3J.0 final-prototype response. V3J.0 compiled after its hotfix, but visually it only reused the existing R channel as if R were already a narrow crease. Validation showed that R/G/B existed, but the final prototype was faint, blocky, and did not demonstrate narrow painted crease response.

V3J.1 keeps the same debug view name and enum value:

```text
Ground Painted Accent Final Prototype
```

The shader now derives the prototype crease from local structure in the G/body channel instead of trusting R directly. It samples neighboring G texels, computes a local gradient magnitude, combines that with narrow contour bands through the body value, and uses R only as an activity/support gate. B remains the signed side/polarity input for dark-side versus light-side response.

The proof contract after V3J.1 is:

```text
G/body field      -> contour + edge extraction source
R/line field      -> activity gate only, not the final crease shape
B/signed field    -> one-sided dark/light polarity
normal render     -> still isolated/unchanged
generator tuning  -> still deferred
```

Validation showed this was still the wrong operation. Shader-side local contour extraction created broad embossed contour/topography response instead of selecting one line per meaningful peak. V3J.2 supersedes this approach by moving thresholding, connected-region identification, and crest-line selection into the generator.

### 2026-07-10 — Patch V3J.0: Painted Accent Final Visual-Response Proof

Patch V3J.0 adds `Ground Painted Accent Final Prototype`, a debug-only view that tests whether the existing generated fold-field channels can produce the desired painted fold/crease response before spending time tuning the continuous generator.

This patch deliberately keeps the V3I.3A normal-render isolation in place. The new prototype view is only selected through the object/material debug mode; it does not reactivate Painted Accent contribution in the normal final ground render.

Prototype response contract:

```text
R / line channel      -> narrow selected contour/crease visibility
G / body channel      -> local context gate only, not broad visible albedo noise
B / signed side       -> side polarity for crease/highlight balance
normal final render   -> unchanged/clean while generated fold field is active
```

The key validation question is not whether the current field is already well placed. The current generator may still be blocky/noisy. The question is whether the field-first representation can be shaded as narrow painted terrain folds rather than broad stains. If the prototype response is promising, continue with field-shape correction/tuning. If it still reads as stains even with G restricted to context, revise the response model/channel contract before tuning the generator.

Validation after this patch should confirm:

```text
normal final render remains clean
Ground Painted Accent Relief / Signed Relief / Lines still expose raw channels
Ground Painted Accent Final Prototype appears in the debug dropdown
the prototype emphasizes narrow crease/highlight response instead of broad G stains
Build Height Preview and Clear Height Preview still work
```


### 2026-07-09 — Patch V3I.3A: Fold Field Debug Isolation + Preview Color Readability

Patch V3I.3A fixes the issues found during V3I.3 validation.

Observed validation result:

```text
The height preview mesh was geometrically displaced and useful from a side/profile view.
The preview material was nearly one color from top view.
The normal final ground render showed fold-field-correlated noise even after clearing the preview mesh.
```

Fixes:

- The final forward pass now zeros Painted Accent final-render contribution while `_GroundPaintedAccentFoldFieldEnabled` is active. Generated fold-field data remains available to debug views and the height preview mesh, but no longer contaminates the normal final render.
- Added `Hidden/PS3D/Ground Fold Field Height Preview`, a small debug shader that reads preview mesh vertex color/body values and maps them to a visible low/mid/high height gradient.
- `GeneratedGround` now prefers this debug shader for the preview mesh material.
- The preview renderer disables shadow casting and shadow receiving.
- `ClearPaintedAccentFoldFieldHeightPreview()` now removes all child preview objects whose names begin with `__FoldFieldHeightPreview_Debug`.

V3I.3A is diagnostic/pipeline correctness only. It does not tune the field generator, alter production mesh geometry, change collision, perform final line extraction, tune families, or add runtime displacement.

Validation after this patch should confirm:

```text
normal final render stays clean after building and clearing preview
height preview is readable from top view and side view
Projected Painted Accent debug views still show the generated field
```


### 2026-07-09 — Patch V3I.3: Fold Field Height Preview Debug Mesh

Patch V3I.3 implements the planned Option B preview for the generated Painted Accent fold field. The existing Painted Accent debug views are projected channel views; this patch adds an editor/debug-only mesh that shows the G/body field as actual relief.

Implementation:

```text
GeneratedGround inspector:
  Build Height Preview
  Clear Height Preview

GeneratedGround:
  stores the latest generated G/body values returned by the fold-field generator
  builds a temporary child mesh named __FoldFieldHeightPreview_Debug
  samples the existing GroundHeightFieldSnapshot for base height
  offsets preview vertices by G * debug height scale
  clears preview mesh/material/object on request

GroundPaintedAccentFoldFieldGenerator:
  keeps the same texture generation path
  additionally returns the same smoothed body array used for the G channel
```

The preview is intentionally diagnostic-only:

```text
no production mesh displacement
no collision change
no gameplay terrain deformation
no generator tuning
no final contour extraction
no new layer or tag dependency
```

Validation should use both the projected `Ground Painted Accent Relief` view and the height preview mesh. If the preview shows blocky value-noise terraces, the next patch should tune the continuous generator before V3J. If the preview shows useful broad terrain forms, V3J line extraction can proceed.


### 2026-07-09 — Patch V3I.2: Continuous Domain-Warped Fold Height Field Prototype

Patch V3I.2 replaces the V3I.2A neutral placeholder with a continuous domain-warped scalar field generator. This is the first active generator that follows the accepted field-first model instead of a candidate/stamp model.

Implemented generator stages:

```text
GenerateRawContinuousField(...)
  local-space texel coordinates
  deterministic domain warp
  broad fractal value field
  medium fractal value field
  ridge-like fractal component
  directional continuity component

ApplySemanticSupport(...)
  multiplies the raw field by existing generated ground mask support

ShapeBodyField(...)
  computes a percentile coverage threshold from the supported field
  soft-thresholds the field into the G/body channel

SmoothBodyField(...)
  applies one light smoothing pass

BuildPixelsFromContinuousField(...)
  writes:
    R = rough edge/contour candidate from G
    G = continuous body field
    B = signed gradient polarity from G
    A = semantic support / reserved
```

The generator intentionally does not reintroduce:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
discrete mark spawning
ellipse stamps
curved ridge stamps
```

Validation target:

```text
Ground Painted Accent Relief
```

The relief view should now show a continuous terrain-like secondary fold field with organic raised regions and quiet negative space. `Ground Painted Accent Lines` remains rough and should not be judged as final art until V3J.

Debug tooling plan:

```text
Patch V3I.3 - Fold Field Height Preview Debug Mesh
```

V3I.3 will use Option B: an editor/debug-only preview mesh generated from the fold-field texture and displaced by the G channel. This is planned because projected texture-channel debug views are useful but not sufficient for reading the actual shape of a height-field generator. The preview must remain debug-only and must not affect mesh geometry, collision, or gameplay.

V3I.2 adds no shader rewrite, final line extraction, fake normal, mesh displacement, 3D preview implementation, family tuning, chunk bake system, or resolution tiering.


### 2026-07-09 — Patch V3I.2A: Candidate-Stamp Generator Retirement + Continuous Field Plan

Patch V3I.2A is a cleanup/redirection patch after V3I.1 validation showed that the second generator prototype was still the wrong model. V3I proved the generated local-space texture path, and V3I.1 reduced the large oval/leaf stamp issue, but the generator remained candidate/stamp based and produced sparse brush-like marks instead of a natural secondary height layer.

This patch intentionally removes the active candidate-stamp generator internals from `GroundPaintedAccentFoldFieldGenerator.cs`:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
candidate cell spawning
candidate curvature/asymmetry/side-lobe stamp model
```

The generator now returns a neutral 256x256 RGBA32 placeholder:

```text
R = 0
G = 0
B = 128 / neutral signed side
A = 0
```

The neutral placeholder preserves the `GeneratedGround` -> material property block -> shader data path while preventing further tuning of the rejected model. It is expected that Painted Accent debug views show no generated fold marks until V3I.2 implements the continuous field generator.

Retained architecture:

```text
GeneratedGround-owned fold texture lifecycle
local/object-space sampling
256x256 active-chunk policy
R/G/B/A texture contract
shader router and debug views
```

Next implementation target:

```text
Patch V3I.2 - continuous domain-warped scalar field generation
  GenerateBaseNoiseField(...)
  GenerateDomainWarp(...)
  ShapeContinuousBodyField(...)
  ApplySemanticSupport(...)
  SmoothBodyField(...)
  BuildPixelsFromContinuousField(...)
```

V3I.2A adds no continuous field implementation, final line extraction, shader rewrite, mesh displacement, fake normal, 3D preview, family tuning, chunk bake system, or resolution tiering.


### 2026-07-09 — Patch V3I.1: Fold Body Shape Correction

Patch V3I.1 corrects the first generated fold-field body model after validation showed that the V3I relief channel successfully came from generated local-space field data but still read as large soft oval/leaf stamps. This patch changes only the generator and docs. It does not change the shader router, material property path, debug enums, mesh generation, river code, or surface family assets.

Generator changes:

- Replaced the single-ellipse fold candidate with a short curved tapered ridge/fold primitive.
- Reduced fold candidate density for more quiet negative space.
- Increased candidate cell spacing.
- Added candidate curvature, width jitter, asymmetry, and deterministic local warp.
- Added a small optional side lobe so bodies can break away from perfect capsules.
- Kept max-composition into the body field so overlapping forms do not become noisy additive mush.
- Reduced the smoothing blur from `0.65 / 0.35` to `0.74 / 0.26`.
- Kept the line channel rough; final line extraction remains Patch V3J.

V3I.1 validation still focuses on `Ground Painted Accent Relief`. Success means the body field no longer reads as repeated smooth oval stamps and starts reading as short irregular low terrain folds/ridges. `Ground Painted Accent Lines` may remain rough.


### 2026-07-09 — Patch V3I: Local-Space 256x256 Fold Field Generator Prototype

Patch V3I is the first active implementation of the generated visual fold-field direction. It replaces the active Painted Accent Lines data source, when the feature is enabled, with a generated local-space texture owned by `GeneratedGround`.

Implemented data policy:

```text
visible authored chunk with PaintedAccentLines active:
  generate one 256x256 RGBA32 fold-field texture

hidden/offscreen/background chunks:
  disable PaintedAccentLines entirely
  no low-resolution fallback texture
```

Budget:

```text
256x256 RGBA32 = 256 KiB per active chunk
10 chunks  = 2.5 MiB
50 chunks  = 12.5 MiB
100 chunks = 25 MiB
200 chunks = 50 MiB
```

Runtime/chunk-library policy:

```text
Chunks are authored/generated in editor or at load/camp rebuild time.
The runtime map builder places, rotates, and connects reusable authored chunks.
Fold-field sampling is local/object-space so the field rotates with the chunk.
The feature is not a per-frame CPU simulation.
```

Implementation details:

- Added `GroundPaintedAccentFoldFieldGenerator`.
- The generator builds deterministic soft fold candidates in local chunk space.
- Candidates are rasterized into a scalar body field instead of deriving a body from procedural curve-distance tubes.
- The body field is semantically supported by existing ground masks.
- The body field is lightly smoothed.
- The signed channel is derived from the body-field gradient.
- The line channel is a rough temporary edge/gradient candidate and is not final line-art polish.
- The generated texture is uploaded with no mipmaps and the CPU texture copy is discarded.
- The retired V3D-V3F.1 curve-distance path remains as fallback when no active `PaintedAccentLines` feature exists.

V3I validation should judge `Ground Painted Accent Relief` first. Success means the relief/body debug view reads as terrain-field-like soft folds rather than fat tubes, scratches, or side rails. V3J will refine line extraction after the body field is accepted.

### 2026-07-09 — Patch V3H: Generated Fold Field Data Skeleton

Patch V3H adds the inactive data path for the accepted fold-field model without changing visible output. `GeneratedGround` now owns a neutral generated fold-field texture placeholder and pushes it through the same material-property-block path used by the ground renderer and river corridor renderer. The ground shader declares the fold-field texture and parameters, adds a non-retired fold-field resolver, and routes Painted Accent debug/final sampling through a new feature router.

V3H data contract:

```text
R = selected accent line mask
G = relief/body/fold-height channel
B = signed side encoded 0..1, where 0.5 is neutral
A = reserved / validity / future support
```

V3H intentionally keeps `_GroundPaintedAccentFoldFieldEnabled = 0`, so the retired curve-distance shader path remains the runtime fallback until V3I generates real fold-field data. This patch adds no noise generation, no edge extraction, no family tuning, no fake normals, no mesh channels, no material assets, and no runtime state.

### 2026-07-09 — Patch V3G: Painted Accent Direction Reset / Fold-Field Plan

Patch V3G is a redirection patch. It does not delete or disable the V3D-V3F.1 shader path, but it clearly retires that curve-distance stroke model as the final solution. The old model is kept only as fallback/comparison code until the generated fold-field replacement exists. It must not be tuned further as the chosen direction.

Rejected source model:

```text
procedural curve stroke
  -> distance-to-curve contour
  -> inflated tube-like relief body
  -> side rails derived from curve side
```

Chosen source model:

```text
generated visual fold field F(x,z)
  -> fold-height/body channel
  -> selected contour/ridge/edge line channel
  -> gradient/polarity signed-side channel
```

Retained architecture:

```text
PaintedAccentLines feature kind
GroundSurfaceVariantRecipe feature stack
GeneratedGround material-property-block ownership
object-level Ground Debug dropdown
Ground Painted Accent Lines / Relief / Signed Relief debug modes
three-channel line/body/signed-side validation contract
```

Planned implementation sequence after V3G:

```text
Patch V3H - Generated Fold Field Data Skeleton
Patch V3I - Fold Field Generator Prototype
Patch V3J - Edge/Contour Extraction
Patch V3K - Final Render Fold Response
Patch V3L - Production Family Tuning
```

This remains visual-only. No physical terrain mesh deformation, collision changes, runtime footprints/wetness, decals, contact accents, sparse motifs, or family tuning are part of V3G.

### 2026-07-09 — Patch V3F.1: Per-Stroke Relief Correction

Patch V3F.1 made the current three-channel debug contract more useful by keeping the relief body continuous instead of fragmenting it with the line breakup mask, and by making the signed-side debug view display visible polarity colors. Validation after V3F.1 showed the debug channels were useful but the underlying source model was still wrong: the line remained a procedural stroke, the relief body read as a fat distance tube, and the signed side read as parallel rails. This validation directly motivates Patch V3G's fold-field direction reset.

### 2026-07-09 — Patch V3F: Painted Accent Relief Debug + Visual Relief Strengthening

Validation after V3E showed the curved marks were directionally better but still too wide and still read as 2D painted stamps. Patch V3F keeps the feature shader-only and visual-only, but exposes the internal three-channel model directly in object-level debug:

```text
Ground Painted Accent Lines
  thin line contour / crease mask

Ground Painted Accent Relief
  broader soft relief body around the contour

Ground Painted Accent Signed Relief
  signed side field remapped from [-1, 1] to [0, 1] for debug
```

The shader now thins the contour independently from the wider relief body and uses the signed relief side more deliberately in normal rendering: one side receives painted shadow, the opposite side receives painted highlight, and the narrow contour remains the dark/tinted crease. No mesh displacement, collision, decals, textures, generated atlases, runtime state, new mesh channels, new components, contact accents, sparse motifs, or family asset retuning are included in this patch.

### 2026-07-09 — Patch V3E: Painted Accent Lines Curved Relief Model

Validation after V3D showed the raw mask was thinner but still fundamentally read as straight 2D bars/line stamps rather than the Hades-1-like curved mound/crease marks defined by the ground doctrine. Patch V3E keeps the shader feature stack and object-level debug workflow unchanged, but replaces the straight micro-stroke primitive with short irregular curved stroke paths built from several local control points. The feature now also outputs a soft signed relief body used for subtle painted shadow/highlight value shaping. This is visual relief only: no terrain mesh height, collision, decals, textures, runtime state, or generated atlases are added.

### 2026-07-09 — Patch V3D: Painted Accent Lines Mask Refinement

Refined the raw `GroundPaintedAccentLines` mask after object-level debug validation showed the first V3 generator was drawing oversized isolated strips/crescents. Patch V3D keeps the feature stack architecture unchanged and updates only the procedural mask primitive: scale now controls group spacing, stroke length/thickness are capped in world units, accents are generated as smaller micro-strokes, and a broad cluster gate prevents uniform hatching. No color-response tuning, style asset retuning, runtime state, decals, textures, mesh changes, or contact accents were added.

### 2026-07-09 — Patch V3C: GeneratedGround Debug UX Hotfix

Fixed two validation blockers from the V3/V3B workflow: object-level `GeneratedGround` debug labels no longer use slash characters that Unity treats as submenu separators, and `GroundSurfaceStyleProfileEditor` no longer uses the obsolete `FindObjectsByType` overload with `FindObjectsSortMode`. This patch does not change shader logic, style recipes, mask generation, or Painted Accent Lines behavior.

### 2026-07-09 — Patch V3B: GeneratedGround Object-Level Debug Views

Exposed ground debug selection directly on `GeneratedGround` under `Ground Debug`. The component now writes `_MaskDebugMode` through its `MaterialPropertyBlock`, so authors can validate ground masks and doctrine-layer debug views from the generated-ground object without opening shared material assets. Debug changes refresh material properties only and do not regenerate terrain.

### 2026-07-09 — Patch V3A: Generated-Mass Shader Compile Hotfix

Fixed compile isolation after Patch V3 by guarding the ground-only painted-accent-line resolver so `PS3D/Pixel Surface Lit` / generated-mass shader paths no longer compile references to ground-only uniforms.

### 2026-07-09 — Patch V3: Shader Feature Stack and Painted Accent Lines

Changed the ground feature renderer from the old single `_GroundFeatureMode` proof slot to explicit stackable shader-property blocks per supported feature kind. Added `PaintedAccentLines = 20` as the first doctrine-layer feature. Updated Snowfield, Wet Mudflat, Grassland, and Style Calibration assets so Painted Accent Lines can coexist with DirectionalStreaks, PooledWetness, and TrampledWear. Added `GroundPaintedAccentLines = 28` debug mode.

### 2026-07-09 — Patch V2B: Grassland Baseline Family

Implemented as an asset/docs patch after validation confirmed Snowfield and Wet Mudflat baselines but identified the need for a living-ground family before shared feature work.

- Added `GSP_Grassland.asset` as a semantic profile with high vegetation suitability, moderate damp/deposit response, low snow eligibility, and soft broad patches.
- Added `GSSP_Grassland.asset` as a production surface family.
- Added `Clean Meadow`, `Patchy Meadow`, `Damp Meadow`, and `Worn Meadow` variants.
- Kept the patch asset-only; no grass blades, vegetation rendering, runtime state, shader changes, river logic, or scene edits were added.
- Established Snowfield, Wet Mudflat, and Grassland as the canonical three-family test set for Patch V3 and later shared style layers.

### 2026-07-09 — Patch V2: Base Ground Simplification and Calibration Cleanup

Implemented as an asset/docs retune after the first Style Calibration screenshots.

- Renamed `Pixel / Faceted` to `Pixel-Faceted` while preserving stable id `calibration.pixel_faceted`.
- Recorded the calibration outcome: Calm Base is the best foundation; Hybrid remains the target philosophy; Pixel-Faceted should not be the default ground style; the Hades proxy is not a substitute for real painted accent lines.
- Retuned `GSSP_Snowfield.asset` toward calmer, matte, lower-noise snow variants.
- Retuned `GSSP_WetMudflat.asset` toward matte, broad, lower-noise mud variants.
- Reduced excessive pixel variation, pixel effect strength, cell warp, patch blend, damp darkening, and overly strong feature response where it fought the doctrine.
- Added no code, shader changes, runtime state, scene edits, materials, or river changes.

### 2026-07-09 — Patch V1: Style Calibration Setup

Implemented as an asset-only calibration patch after the ground doctrine was accepted.

- Added `GSP_StyleCalibration.asset` as a neutral semantic profile for visual-lane comparisons.
- Added `GSSP_StyleCalibration.asset` as a temporary style family with `Calm Base`, `Hades Accent Proxy`, `Hybrid Target Proxy`, and `Pixel-Faceted` variants.
- Used existing `GroundSurfaceStyleProfile`, `GroundSurfaceVariantRecipe`, `GroundMaterialControls`, and `GroundSurfaceFeatureRecipe` architecture.
- Used `DirectionalStreaks` only as a temporary proxy for Hades-like accent rhythm in the Hades and Hybrid variants.
- Added no code, shader changes, runtime state, scene edits, materials, or river changes.

### 2026-07-08 — Patch I: Ground Visual Scale Cleanup

Implemented after the first snowfield visual-response pass made the final ground read as too granular from the isometric camera. The ground masks were kept unchanged; the fix is limited to the dedicated ground shader/material response.

- Added `_GroundMacroPatchScale` to `PS3D/Pixel Ground Surface Lit` so macro snowfield variation is measured in terrain metres instead of deriving from `_PixelCellSize * 8`.
- Reduced `M_PixelFrozenDirt` fine pixel variation/warp to avoid repeated mottling across the ground plane.
- Reworked snow response so `_GroundSnowBrightness` handles value lift and `_GroundSnowTintStrength` controls value-preserving hue shift toward `_FrostColor`.
- No generated-ground mask generation, river corridor, water, foam, or generated-mass shader code changed.

### 2026-07-08 — Patch J: Ground Visual Presets and Component-Owned Material Controls

Implemented after the snowfield baseline became visually acceptable but too difficult to author through the shared material asset. The ground material asset remains a shared shader backend; per-ground visual response is now owned by `GeneratedGround` and pushed through renderer `MaterialPropertyBlock`s.

- Added the first `GroundVisualPreset` implementation with `Clean Snowfield`, `Patchy Snowfield`, `Dirty / Thawing Snowfield`, and `Wind-Scoured Snowfield` options.
- Added serialized `GroundMaterialControls` on `GeneratedGround` for pixel variation, broad variation, vertex variation, cell warp, patch blend, macro patch scale, snow tint, snow brightness, damp darkening, and frost colour.
- Extended `GeneratedGround.ApplySurfaceProfileMaterialProperties(Renderer)` so these visual controls are applied per renderer through property IDs for `_PixelVariation`, `_PixelBroadVariation`, `_PixelVertexVariation`, `_PixelWarpStrength`, `_GroundPatchBlendStrength`, `_GroundMacroPatchScale`, `_GroundSnowTintStrength`, `_GroundSnowBrightness`, `_GroundDampDarkenStrength`, and `_FrostColor`.
- Added a `GeneratedGroundEditor` preset dropdown and compact `Advanced Material Controls` foldout under the existing Surface section. Changing presets writes the bundled values into the serialized controls; manually editing a control marks the preset as `Custom`.
- Added a generation-signature guard in `GeneratedGround.OnValidate()` so material-only control edits refresh material property blocks instead of forcing a ground/corridor geometry regeneration.
- Added `StylizedRiver.RefreshCorridorMaterialProperties()` so ground visual changes can resync the existing river corridor renderer without rebuilding corridor meshes.
- No material duplication, mask generation changes, generated-mass shader changes, or river geometry changes were introduced.

### 2026-07-08 — Patch K: Surface Type / Surface Variant Architecture

Implemented after visual validation showed that the Patch J presets were too similar and that `Dirty / Thawing Snowfield` appeared as a nested Unity menu item because `/` was interpreted as a submenu separator. Patch K starts the long-term surface-style architecture while keeping the current implementation cheap and reversible.

Current authoring model:

```text
GeneratedGround
  Surface Profile        -> semantic/mask-generation asset
  Surface Type           -> visual family, currently Snowfield
  Snowfield Variant      -> Clean / Patchy / Dirty Thawing / Wind-Scoured / Custom
  Advanced Material Controls -> per-object visual recipe overrides
```

Important architecture decisions:

- `Surface Type` is intentionally not called biome. A biome is a world/ecology concept; this control is a renderer/terrain-surface family.
- `GroundSurfaceProfile` remains the source for generated mask tendencies such as exposure, damp/deposit, vegetation suitability, rocky/dry suitability, snow eligibility, and rain absorption.
- `GroundSurfaceType` and the per-type variant select a final visual recipe that interprets those masks.
- Variant edits are visual-only and must continue to refresh material property blocks without rebuilding ground or river-corridor geometry.
- The current enum-backed implementation is a stepping stone. The expected final form is asset-backed `GroundSurfaceStyleProfile` / variant assets once more than one surface type exists and once the required feature vocabulary is known.

Patch K changes:

- Replaced the flat `Ground Visual Preset` authoring concept with `Surface Type` plus `Snowfield Variant`.
- Renamed `Dirty / Thawing Snowfield` to `Dirty Thawing`, removing the slash that caused Unity to display a nested dropdown submenu.
- Expanded `GroundMaterialControls` so variants now drive a full visual recipe instead of only ten mild material values.
- Added per-ground control over base colour, frost colour, damp/rocky/vegetation tint colours and tint strengths, pixel cell size, tone count, cluster strength, pixel effect strength, profile contrast scales, semantic response scales, wetness/finish controls, frost response, monolithic flattening, smoothness, and specular strength.
- Added ground shader properties for `_GroundDampTint`, `_GroundDampTintStrength`, `_GroundRockyDryTint`, `_GroundRockyDryTintStrength`, `_GroundVegetationTint`, and `_GroundVegetationTintStrength`.
- Updated `PixelSurfaceGroundForwardPass.hlsl` so damp, rocky/dry, and vegetation responses can shift hue through value-preserving tint targets instead of being fixed hard-coded colour multipliers.
- Strengthened the four snowfield recipes so they are intentionally more distinct at game-camera distance: clean is quiet/cold, patchy increases rocky/dry and macro contrast, dirty thawing increases warm damp/shore/wet response and lowers snow purity, and wind-scoured suppresses dirt/detail while flattening into larger cold plates.

Near-term limitation:

- Wind-scoured ground still lacks true directional streak geometry/noise. The current recipe can make it cleaner, colder, flatter, and broader, but a convincing scoured/swept snowfield will need a directional surface-feature module later.

Future architectural target:

```text
GeneratedGround
  GroundSurfaceProfile         // mask generation / terrain semantic tendencies
  GroundSurfaceStyleProfile    // visual surface family, e.g. Snowfield, Mudflat, Rocky Ground
  Style Variant                // Clean, Patchy, Dirty Thawing, Wind-Scoured, etc.
  Advanced Overrides           // local per-object deviation from the selected variant
```

Do not add dozens of hardcoded surface types indefinitely. When the second or third surface type is introduced, move from enum recipes to style-profile assets so new ground families can be authored without expanding `GeneratedGround.cs` into a preset registry.

---

## Patch J-L Implementation Update: Ground Visual Authoring and Style Profiles

### Patch J — GeneratedGround material controls

Patch J moved the normal ground visual authoring path from material-asset editing to the `GeneratedGround` component.

Implemented direction:

- `GeneratedGround` owns per-ground visual material controls.
- The shared ground material remains a backend/default asset.
- Ground visual values are applied through `MaterialPropertyBlock`.
- River corridor renderers receive the resolved parent-ground property block instead of owning a separate ground style.
- Visual-only control changes refresh material properties without requiring ground or corridor mesh regeneration.

This established the correct renderer path:

```text
GeneratedGround resolves visual controls
→ applies MaterialPropertyBlock to its renderer
→ refreshes child StylizedRiver corridor material properties
```

Material duplication is intentionally avoided.

### Patch K — Surface Type / Snowfield Variant bridge

Patch K replaced the flat temporary `Ground Visual Preset` concept with an explicit hierarchy:

```text
Surface Type: Snowfield
Snowfield Variant: Clean / Patchy / Dirty Thawing / Wind-Scoured / Custom
```

It also expanded snowfield variants from small value tweaks into fuller visual recipes controlling palette, semantic response, pixel/macro variation, wetness, frost, smoothness, and specular response.

Patch K was a bridge, not the final architecture. Its enums made the hierarchy clearer, but hardcoded terrain families and hardcoded recipe switches would not scale to muddy, rocky, waterlogged, desert, or future feature-heavy surface families.

### Patch L — Ground Surface Style Profile architecture

Patch L introduces the asset-backed architecture that future ground families should use.

The conceptual split is now:

```text
GroundSurfaceProfile
  Semantic/mask-generation profile.
  Controls generated surface-mask tendencies such as exposure,
  damp/deposit, vegetation suitability, rocky/dry suitability,
  snow eligibility, and rain absorption.

GroundSurfaceStyleProfile
  Visual surface family asset.
  Owns a default GroundSurfaceProfile and a list of variant recipes.
  Example: Snowfield.

GroundSurfaceVariantRecipe
  One named visual recipe inside a style profile.
  Uses a stable id such as snowfield.clean or snowfield.dirty_thawing.
  Owns GroundMaterialControls.

GroundMaterialControls
  Renderer/material response recipe.
  Contains palette, pixel/macro variation, semantic response,
  weather/finish, and shader response values.

GeneratedGround
  Resolver and per-object override owner.
  Selects a GroundSurfaceStyleProfile and variant id, optionally overrides
  the semantic profile and/or material controls, then pushes the resolved
  result through MaterialPropertyBlock.
```

Current asset path:

```text
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Snowfield.asset
```

Current Snowfield variant ids:

```text
snowfield.clean
snowfield.patchy
snowfield.dirty_thawing
snowfield.wind_scoured
```

The Inspector now treats style data as asset-owned by default:

```text
Surface Style Profile: Snowfield
Surface Variant: Clean / Patchy / Dirty Thawing / Wind-Scoured
Override Surface Profile: optional
Advanced Material Overrides: optional local custom copy
```

Important behavior:

- Selecting a variant uses the recipe from the style asset.
- Advanced material overrides are local to the selected `GeneratedGround` object.
- Enabling material override copies the currently resolved recipe first, so local edits start from the selected variant.
- Existing Patch K enum data is retained only for migration and compatibility.
- The active material recipe should no longer be hardcoded inside `GeneratedGround` for future styles.

### Rules for future ground families

Do not add future terrain families as hardcoded enums in `GeneratedGround`.

Do not add large `switch` blocks for Mudflat, Rocky Ground, Desert, Waterlogged Ground, and similar families.

Do not duplicate material assets per variant.

Do not make river corridors own ground style state. They should continue to receive the resolved parent-ground renderer contract through material property blocks.

Do not merge `GroundSurfaceProfile` and `GroundSurfaceStyleProfile` yet. The semantic mask-generation profile and the visual style family are related but not the same layer.

The expected path for a new visual family is:

```text
Create a GroundSurfaceStyleProfile asset
→ assign or create its default GroundSurfaceProfile
→ add variant recipes
→ only add code if a truly new shader/feature module is needed
```

### Next architecture step after Patch L

The next scalable addition should be feature-module support inside style variants, not another hardcoded terrain-family branch.

Potential future variant feature modules:

- directional snow streaks;
- melt patches;
- pebble or scree scatter;
- mud crust cracks;
- wet pooled lowlands;
- trampled path wear;
- frosted rock dust.

Each future feature should declare whether it is shader-only, mesh-mask driven, texture/atlas driven, or runtime-state driven, so styles only pay for the features they actually use.

### Patch M — Surface Variant Feature Module Foundation

Patch M adds the first feature-module layer inside the asset-backed ground style architecture.

The important architectural change is that a `GroundSurfaceVariantRecipe` is no longer only a material-control preset. It can now own optional `GroundSurfaceFeatureRecipe` entries. This lets a variant define a small feature vocabulary without adding terrain-family branches to `GeneratedGround`.

The new feature data types are:

```text
GroundSurfaceFeatureKind
  Names reusable feature modules such as Directional Streaks, Melt Patches,
  Pooled Wetness, Pebble Scatter, Mud Crust Cracks, Trampled Wear, and
  Frosted Rock Dust.

GroundSurfaceFeatureCostClass
  Declares the broad cost bucket: Shader Only, Mesh Mask Driven,
  Generated Texture, or Runtime State.

GroundSurfaceFeatureRecipe
  A per-variant feature entry containing kind, enabled state, cost class,
  strength, scale, contrast, mask influence, direction, and seed offset.
```

Patch M intentionally implements only one renderable proof feature:

```text
Directional Streaks
  Cost class: Shader Only
  Owner: GroundSurfaceVariantRecipe
  Resolver: GeneratedGround
  Renderer path: MaterialPropertyBlock
  Shader path: Pixel Ground Surface Lit
```

Directional Streaks exists because wind-scoured snow, sand, ash, and dust cannot be represented convincingly by colour and macro-noise sliders alone. The first implementation is deliberately cheap: it uses world-position noise, a stable direction vector, the existing pixel seed, and the selected variant's feature recipe. It does not allocate textures, add atlases, change generated mesh data, or create runtime state.

Historical Patch M renderer contract:

```text
_GroundFeatureMode
_GroundFeatureStrength
_GroundFeatureScale
_GroundFeatureContrast
_GroundFeatureMaskInfluence
_GroundFeatureDirection
_GroundFeatureSeed
```

This single-slot contract is superseded by Patch V3. Current ground rendering resolves the feature list as a stack and writes explicit property blocks per supported feature kind. `_GroundFeatureMode` remains only as a hidden compatibility property and must not receive new feature modes. River corridor renderers remain style-agnostic and continue to receive the resolved parent-ground material contract through the same property block refresh path.

Current Snowfield feature usage:

```text
Clean
  Weak Directional Streaks, mostly masked to snow/exposure.

Patchy
  Mild Directional Streaks, still secondary to patch variation.

Dirty Thawing
  No directional streak feature in Patch M; its identity remains damp/melt-biased.

Wind-Scoured
  Strong Directional Streaks, broad scale, lower semantic masking.
```

Patch M does not implement melt patches, pebble scatter, mud cracks, trampled wear, or frosted rock dust yet. Pooled Wetness is implemented in Patch N as the second shader-only proof feature. Remaining kinds are valid feature kinds in the asset contract, but each should only become renderable when it has a concrete cost model and visual need.

Rules after Patch M:

- Do not add a new hardcoded enum branch to `GeneratedGround` for every future terrain family.
- Do not add all features to every style at full runtime cost.
- Do not add generated textures or atlases until a feature demonstrably needs them.
- Do not make river corridors understand style names or feature kinds.
- Keep feature recipes variant-owned and renderer application resolved by `GeneratedGround`.
- Keep the material-property-block path as the final per-renderer contract.

The next architectural proof should be a second style family or a second cheap feature, not a large feature explosion. A good next candidate is either a minimal Mudflat/Waterlogged style using existing material controls, or a shader-only Pooled Wetness feature if the snowfield/river-adjacent ground needs more expressive thaw/melt response.

### Patch N — Second Surface Family Proof and Pooled Wetness

Patch N proves that the Patch L/M architecture can add a second visual ground family without adding a hardcoded terrain-family branch to `GeneratedGround`.

New assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_WetMudflat.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_WetMudflat.asset
```

`GSP_WetMudflat` is the semantic/mask-generation profile for wet mud: high damp/deposit tendency, high rain absorption, low snow eligibility, and high footprint visibility. It reuses the existing generated ground vertex/UV2 mask contract; no new mesh channels are added.

`GSSP_WetMudflat` is the visual style profile. Its variants are:

```text
mudflat.damp_mud
  balanced damp mud, moderate pooled wetness.

mudflat.waterlogged
  darker, wetter, smoother, strongest pooled wetness.

mudflat.trampled
  higher contrast, compacted-looking mud response, moderate pooled wetness.

mudflat.frozen_thaw
  colder thawing mud, partial frost response, lighter pooled wetness.
```

Patch N also made `Pooled Wetness` a renderable shader-only proof feature. Historically it used the single `_GroundFeatureMode` contract added in Patch M.

That path is superseded by Patch V3. `PooledWetness` is now one supported layer in the shader feature stack and can coexist with other supported ShaderOnly features such as PaintedAccentLines.

Pooled Wetness is deliberately cheap. It uses world-position procedural noise, damp/deposit mask, shore mask, rocky/dry suppression, the feature recipe seed, and the selected variant's strength/scale/contrast/mask influence. It darkens and damp-tints local pools and adds local smoothness/specular response in the ground shader. It does not allocate textures, add atlases, generate new mesh data, or create runtime state.

The important architectural result is this workflow:

```text
new style family
→ new GroundSurfaceProfile asset
→ new GroundSurfaceStyleProfile asset
→ variant recipes with material controls and feature recipes
→ GeneratedGround resolves selected style/variant generically
→ MaterialPropertyBlock pushes the resolved contract
```

No terrain-family switch was added to `GeneratedGround`. The river corridor remains style-agnostic and continues to receive the parent ground's resolved material-property block.

Rules after Patch N:

- Add future ground families as `GroundSurfaceStyleProfile` assets, not `GeneratedGround` enum branches.
- Add future visual vocabulary as `GroundSurfaceFeatureRecipe` entries, with explicit cost class.
- Keep shader-only features cheap and procedural until a feature proves it needs texture/atlas/state support.
- Do not make river corridor code understand surface style names.
- Do not polish every variant before proving the architecture; visual tuning belongs after the contract is stable.

The next recommended step is authoring UX: create a compact custom editor for `GroundSurfaceStyleProfile` assets so variant IDs, material controls, and feature recipes are easier to edit and validate before many more styles are added.


### Patch O — Generated Ground Surface Authoring UX

Patch O moves the normal surface-family workflow to the top of the `GeneratedGround` Inspector.

Patch L and Patch M made ground styles asset-backed, but Patch N exposed an authoring problem: users had to manually drag `GroundSurfaceStyleProfile` assets onto the generated ground object and scroll down to find the style and variant controls. That is acceptable for a technical proof, but not for regular level-authoring.

Patch O keeps the asset-backed architecture and changes only the authoring path.

The top of the `GeneratedGround` Inspector now begins with:

```text
Ground Surface
  Surface Family
  Surface Variant
  Override Surface Profile
  Resolved Surface Profile
  Feature Summary
  Advanced Style Asset
```

`Surface Family` is an editor-populated dropdown. The editor discovers `GroundSurfaceStyleProfile` assets from:

```text
Assets/Game/Demo/Profiles/Ground/Styles
```

and falls back to all project `GroundSurfaceStyleProfile` assets if none are found in that folder. This means normal authoring can switch between families such as `Snowfield` and `Wet Mudflat` without manually dragging assets.

`Surface Variant` is populated from the selected style profile's variant recipes. Switching family assigns the chosen style asset, validates the stored variant id, and falls back to the first valid variant if the previous id does not exist in the new family.

Patch O also adds top-level authoring validation warnings for:

- missing style profile;
- missing default surface profile on a style;
- missing or empty variant lists;
- stored variant id not present in the selected style;
- duplicate variant ids inside a style asset.

The raw style asset reference still exists under `Advanced Style Asset` for custom or externally stored profiles, but it is no longer the primary workflow.

Patch O does not change rendering, shader behavior, feature recipes, material controls, river corridor logic, or generated mesh data. The architecture remains:

```text
GeneratedGround top-level authoring selection
→ GroundSurfaceStyleProfile asset
→ GroundSurfaceVariantRecipe
→ optional local overrides
→ MaterialPropertyBlock
→ ground renderer and child river corridor renderers
```

Rules after Patch O:

- Surface family and variant selection should remain at the top of `GeneratedGround`.
- Do not make normal users manually drag style assets for common families.
- Keep the raw style asset field as an advanced escape hatch.
- Add new surface families as style assets discoverable by the editor dropdown.
- Keep river corridors style-agnostic.
- Keep visual tuning separate from authoring UX patches.

The next recommended step is a dedicated `GroundSurfaceStyleProfile` editor if nested variant/material/feature editing remains awkward after the number of style assets grows. That editor should improve authoring of style assets themselves, not move style ownership back into `GeneratedGround`.


### Patch P — Wet Mudflat Material Sanity Pass

Patch P is a small visual sanity pass for the first non-snowfield style family created by Patch N.

Patch N intentionally proved that `GroundSurfaceStyleProfile` assets can define a second surface family and that `Pooled Wetness` can run as a shader-only feature without textures, atlases, runtime state, or new mesh channels. The first values were deliberately broad proof values, and validation showed that Wet Mudflat was much too glossy: the darker variants read closer to oil, tar, polished plastic, or wet metal than mud.

Patch P keeps the same architecture and changes only wet mud material response and Wet Mudflat recipe values.

Changed response rules:

```text
Pooled Wetness shape contrast:
  reduced from 1.0–4.25 to 0.85–3.10

Pooled Wetness albedo darkening:
  reduced from 0.20 + Strength × 0.28
  to           0.12 + Strength × 0.18

Pooled Wetness damp tint addition:
  reduced from pooled × 0.58
  to           pooled × 0.32

Pooled Wetness albedo blend:
  reduced from pooled × 0.88
  to           pooled × 0.62

Global wetness darkening:
  reduced from Wetness × WetDarkenStrength × 0.36
  to           Wetness × WetDarkenStrength × 0.26

Smoothness contribution:
  reduced from Smoothness + Wetness × WetSmoothnessBoost + PooledWetness × 0.24
  to           Smoothness + Wetness × WetSmoothnessBoost × 0.55 + PooledWetness × 0.10

Specular wetness multiplier:
  reduced from 1.25 at full Wetness
  to           1.08 at full Wetness

Specular pooled-wetness multiplier:
  reduced from 1.38 at full Pooled Wetness
  to           1.12 at full Pooled Wetness
```

Wet Mudflat recipe values were also pulled back:

```text
Damp Mud:
  lower Wetness, WetSmoothnessBoost, Smoothness, Specular Strength, and Pooled Wetness strength.

Waterlogged:
  remains the wettest mudflat variant, but no longer uses extreme global smoothness/specular values.

Trampled:
  remains higher-contrast and compacted, but its wet finish is reduced so it reads more like walked mud than oil.

Frozen Thaw:
  remains colder and partially frosted, with the weakest pooled-wetness finish among the wet variants.
```

Patch P does not change style-family discovery, variant selection, `GeneratedGround` authoring UX, river corridor refresh logic, mesh generation, semantic mask generation, material property names, feature asset contracts, textures, atlases, or runtime state.

Rules after Patch P:

- Wet Mudflat may still need major future shader/features work, but baseline variants should not be mirror-glossy.
- Keep wet ground response mostly matte unless a specific feature intentionally requests stronger shine.
- Do not solve future mud quality by raising global smoothness/specular back to extreme values.
- Prefer local pooled-wetness breakup and semantic masks over full-surface reflectivity.

The next recommended step is either a dedicated `GroundSurfaceStyleProfile` editor, if asset editing remains painful, or a focused new feature/family proof once Wet Mudflat is visually stable enough to stop distracting from architecture validation.



### Patch Q — Wet Mudflat Matte Baseline Reset

Patch Q follows Patch P after validation showed the opposite failure: after reducing the mirror/oil response, the Wet Mudflat variants still read like smooth plastic or playdough because the style was still trying to imply an entire muddy scene through full-surface colour, smoothness, and wetness.

The architectural decision after this validation is important:

```text
Mud ground should not be globally reflective.
The earth body should be mostly matte.
Future reflectivity should come from explicit local features such as puddles, wet stones, water-filled ruts, potholes, and standing-water patches, not from making the whole terrain surface shiny.
```

Patch Q therefore resets Wet Mudflat to a conservative matte-earth baseline. The four variants are allowed to be somewhat samey for now. Their names describe future feature targets, not fully delivered final art.

Changed recipe direction:

```text
Damp Mud:
  ordinary damp brown earth, low wetness, very low specular.

Waterlogged:
  darker and more moisture-biased, but still mostly matte earth until explicit puddle/standing-water features exist.

Trampled:
  slightly darker, higher variation and contrast, but not glossy.

Frozen Thaw:
  colder and paler, with restrained frost and low wet finish.
```

Changed shader response:

```text
Pooled Wetness is now treated as a matte damp-earth breakup cue, not as a water/puddle substitute.
Its smoothness and specular contributions are reduced to minimal values.
```

Rules after Patch Q:

- Do not attempt to make final mud variants using only full-surface colour/smoothness/specular controls.
- Keep baseline earth surfaces matte unless an explicit feature owns the local reflective surface.
- Future waterlogged quality should come from features such as `StandingWaterPuddles`, water-filled ruts, potholes, debris scatter, and terrain/prop context.
- It is acceptable for early Wet Mudflat variants to look similar if they remain plausible ground.


### Patch R — Ground Plan Reconciliation and Style Profile Editor

Patch R reconciles the ground roadmap with the architecture that now exists after Patches J through Q and adds a custom editor for `GroundSurfaceStyleProfile` assets.

The documentation update records the split between semantic surface profiles, visual style profiles, variant recipes, material controls, feature recipes, and the `GeneratedGround` resolver. Its roadmap was later superseded by Patch V0, which paused the immediate runtime-state queue and made style calibration, painted accent lines, contact accents, sparse motifs, and feature-stack aggregation the active direction. Patch V3 later implemented the first shader feature stack baseline.

The `GroundSurfaceStyleProfile` editor makes style assets practical to edit before more surface families are added. It adds:

- readable variant cards instead of a raw variant array as the primary editing view;
- stable ID and display-name editing per variant;
- compact feature summaries per variant;
- material-control and feature foldouts;
- Add Variant, Duplicate Variant, Remove Variant, and Add Feature actions;
- warnings for missing default surface profiles;
- warnings for empty or duplicate variant IDs;
- warnings for enabled `None` features;
- informational warnings for reserved feature kinds or cost classes that do not currently render.

Patch R does not change visuals, shader behavior, generated mesh data, river corridor logic, material values, style-family discovery, textures, atlases, or runtime state.

Rules after Patch R:

- Keep `GeneratedGround` as the top-level level-authoring surface.
- Keep `GroundSurfaceStyleProfile` as the style-family asset, edited through its custom editor.
- Do not add new terrain families as `GeneratedGround` enum branches.
- Do not infer final muddy/snowy/rocky detail from global material controls alone; add explicit feature modules when needed.
- For path/compaction work, prefer visual-only masks where equally effective, but allow small safe height changes where the terrain feature justifies them.


### Patch S — Ground Style Asset Live Refresh

Patch S fixes an authoring gap introduced by the asset-backed style workflow. After Patch R, `GroundSurfaceStyleProfile` assets were much easier to edit, but editing a style asset did not immediately update open `GeneratedGround` instances that referenced that asset.

The intended authoring behavior is now:

```text
Edit GSSP_Snowfield or GSSP_WetMudflat
→ open GeneratedGround objects using that style refresh their resolved style state
→ material and shader-only feature edits reapply MaterialPropertyBlock values
→ child river corridors receive the same refreshed ground material contract
```

Patch S adds automatic delayed refresh from `GroundSurfaceStyleProfileEditor` whenever serialized style data changes, plus an explicit `Apply To Open Generated Grounds` button for manual refresh.

The refresh path intentionally calls `GeneratedGround.RefreshSurfaceStyleState()` rather than rebuilding unconditionally. Material-control and shader-only feature edits should remain material-property-block updates. If the resolved semantic `GroundSurfaceProfile` changes and the generated ground is configured to regenerate on validation, the existing generation-signature path performs the necessary regeneration.

Patch S does not change visuals, style assets, shader code, mesh data, river corridor code, textures, atlases, runtime state, or modifier behavior.

### Patch T — Ground Modifier Surface/Height Contract

Patch T separates two concepts that were previously coupled inside `GroundModifier`:

```text
Does this modifier change playable terrain height?
Does this modifier write authored ground-surface meaning?
```

Before Patch T, `Flatten` was the only modifier mode that wrote the `UV2.x` compaction/path mask, and there was no way to author a path, damp/deposit boost, or standing-water/puddle potential without using an ordinary height modifier.

Patch T adds:

```text
GroundModifierMode.None
GroundModifierSurfaceEffectMode.AutoFromHeight
GroundModifierSurfaceEffectMode.None
GroundModifierSurfaceEffectMode.Custom
Surface Compaction Strength
Surface Damp/Deposit Strength
Surface Standing Water Strength
```

The generated ground mask contract is now:

```text
Vertex Color R = tonal surface variation
Vertex Color G = exposure / accumulation eligibility
Vertex Color B = damp/deposit potential, including authored modifier boost
Vertex Color A = vegetation suitability
UV2.x = compaction/path/flatten influence
UV2.y = shore influence at Patch T; reserved zero on ordinary Ground after V3S-A2C.4
UV2.z = rocky/dry patch
UV2.w = authored standing-water / puddle potential
```

Legacy behavior is preserved: existing `Flatten` modifiers using `AutoFromHeight` continue to write compaction/path influence. `Raise` and `Lower` keep their height behavior.

Authoring rules after Patch T:

- Use `Mode = None` with `Surface Effect Mode = Custom` for pure visual/path/damp/standing-water masks.
- Use `Flatten`, `Lower`, or `Raise` with `Surface Effect Mode = Custom` when a road, wagon rut, camp pad, drainage dip, or puddle basin needs both a small height change and explicit surface metadata.
- Use `Surface Effect Mode = None` for physical height edits that should not imply path, damp, or standing-water surface meaning.
- Keep denivelations small and combat-safe unless a later gameplay/navigation pass explicitly approves stronger terrain deformation.

Patch T does not add final trampled rendering, puddle rendering, splines, footprints, runtime wetness, atlases, textures, or new mesh channels. It only establishes the static authored modifier contract that future features can read.


### Patch U — Trampled Wear Feature / Compaction Feature Proof

Patch U is the first feature that consumes the Patch T authored surface-mask contract directly in the ground shader.

Concrete flow:

```text
GroundModifier surface mask
→ GroundGenerator metadata pass
→ UV2.x compaction/path/flatten influence
→ GroundSurfaceFeatureKind.TrampledWear
→ shader-only feature response
```

Patch U proves the data path but does not define the final ground direction. After the doctrine pivot, `TrampledWear` is classified as a useful proof and future compaction-response layer, not the current foundation. Do not keep polishing trampled mud while the overall static style remains undecided.

Patch U intentionally does not solve final footprints, snow compression, grass suppression, puddles, runtime wetness, roads, wagon tracks, or painted accent-line language.

### Patch V0 — Ground Visual Doctrine Documentation

Patch V0 locks the new ground baseline in `Assets/Docs/Ground_Visual_Design_and_Architecture.md`:

```text
Restrained stylized terrain
= BOTW/TOTK-like base-material restraint
+ Hades-1-like painted ground accents
+ procedural masks and reusable style layers
+ family/variant tuning.
```

This patch also changes this implementation plan so it no longer acts as the sole home for ground design doctrine. It pauses the old immediate runtime-state roadmap and makes style calibration the next milestone.

Rules after Patch V0:

- family/variant architecture stays;
- families define material identity;
- variants tune the shared visual stack;
- `GroundSurfaceFeatureRecipe` entries should evolve toward reusable doctrine layers;
- painted accent lines are the first new foundational visual feature;
- contact/edge accents are the next major grounding layer;
- sparse motifs come after accent lines/contact response;
- runtime state resumes only after the static visual language is validated;
- no new niche terrain features should be prioritized until the doctrine stack is working.

Patch V0 changes documentation only.



### Patch V3J.3A3 correction — explicit base angle plus signed jitter

V3J.3A2 still produced visually same-angle strokes because the signed jitter was applied around a hidden legacy `direction` vector. The default Painted Accent direction was already biased diagonally, and the GeneratedGround validation UI exposed only jitter, not the base angle. The corrected contract is now explicit:

```text
finalStrokeAngle = Facing Direction Degrees + 90° + random(-Angle Jitter Degrees, +Angle Jitter Degrees)
```

Painted Accent 3D strokes no longer use the generic feature `Direction` vector as their active orientation source. The GeneratedGround and style-profile editors expose `Facing Direction Degrees` and `Angle Jitter Degrees`. The facing direction represents the player/camera-facing direction; generated strokes are perpendicular to it, then each stroke rolls a signed jitter around that perpendicular line angle.


### Patch V3J.3A4 — Perpendicular Facing Direction Angle Fix

The V3J.3A3 control still described the line angle directly. Validation showed the authored angle should instead describe the player/camera-facing direction, with generated strokes perpendicular to that direction. The active rule is now:

```text
finalStrokeAngle = Facing Direction Degrees + 90° + random(-Angle Jitter Degrees, +Angle Jitter Degrees)
```

This keeps the generator deterministic and keeps angle variation as simple signed jitter, but removes the 90-degree semantic mismatch from the authoring UI.

### Patch V3J.3D — Painted Accent Placement Foundation

Status: implemented; awaiting Unity validation.

V3J.3D locks the V3J.3C5 geometry, C6 double-sided visibility, and C8 flat unlit ink shader. It changes descriptor placement only.

Implemented scope:

```text
GroundPaintedAccentFoldFieldGenerator.cs
GeneratedGround.cs
GroundSurfaceFeatureRecipe.cs
GroundModifier.cs
GroundModifierEditor.cs
GeneratedGroundEditor.cs
GroundSurfaceStyleProfileEditor.cs
Ground_Visual_Design_and_Architecture.md
Ground_Generation_Surface_Upgrade_Plan.md
```

Implementation contract:

1. `GenerateSurfaceStrokes(...)` now owns deterministic descriptor proposal, weighted patch selection, and full-stroke validity checks.
2. `RasterizeSurfaceStrokes(...)` consumes already accepted descriptors for the legacy/debug fold field.
3. Candidate capacity is approximately target proposals x16, using continuous two-scale value noise and weighted random priority. Patch-coordinate offsets are included in both density-noise sampling and candidate hashing so adjacent patches share a continuous broad field without repeating identical local stroke layouts.
4. `Stroke Density` remains a pre-rejection proposal target. Rejected strokes are not replaced.
5. New per-feature controls:
   - `Distribution Patch Scale`, 2-24 m, default 9 m;
   - `Distribution Patchiness`, 0-1, default 0.70.
6. Both controls are editable in the style profile and from the selected `GeneratedGround` component's consolidated Painted Accent section.
7. Complete strokes are validated at at least 13 samples and at no more than 0.25 m spacing, across left/centre/right footprint samples.
8. River exclusion uses `StylizedRiverGroundSnapshot.TryEvaluate(...)`, `ResolveHandoffHalfWidth(...)`, stroke footprint, and 0.15 m proof clearance.
9. Terrain proof limits are 45-degree broad slope and 40-degree local longitudinal/transverse grade.
10. `GroundModifier` adds flags-based feature exclusions. `Painted Accent Lines` can be blocked with Circle or Box regions including Blend Distance, without requiring height or surface-mask effects.
11. Ordinary collider scans, new layers, new tags, new standalone exclusion components, scene edits, style-asset edits, geometry changes, and shader changes are excluded from this patch.

Required validation:

1. Confirm Unity compiles without C# errors.
2. Keep the accepted Painted Accent width, height, crown, taper, and ink settings unchanged.
3. Begin with Patch Scale 9 m and Patchiness 0.70.
4. Regenerate twice with one seed and confirm identical proposals, acceptances, and positions.
5. Confirm visibly sparse regions and denser soft patches exist without hard island borders.
6. Confirm touching or near-touching strokes are possible.
7. Confirm no accepted stroke overlaps visible water, hidden river bed, or the river handoff footprint.
8. Add a GroundModifier with Mode None, Surface Effect Mode None, and Painted Accent Lines excluded; verify its full shape plus Blend Distance remains clear.
9. Move and resize the modifier, regenerate, and confirm deterministic updates.
10. Test an abrupt elevation transition and confirm no stroke bridges it.
11. Confirm ordinary gentle slopes remain eligible.
12. Confirm rejected proposals are not backfilled into remaining valid land.
13. Confirm the crowned-ribbon topology and flat unlit ink output are visually unchanged.
14. Capture the complete placement diagnostic log.

Acceptance gate:

- distribution reads as continuous noisy patches rather than even cells or binary islands;
- rivers and river beds remain completely clean;
- explicit exclusion zones reliably clear structures and rocks;
- cliff/height-transition crossings are absent;
- placement remains deterministic;
- final accepted count may be below the target proposal count;
- existing individual-line geometry and appearance do not regress.

After validation, tune family/variant-specific density, patch scale, patchiness, length, orientation, width, height, irregularity, taper, and ink colour. Distance raster stability remains deferred renderer/LOD polish.


### Patch V3J.3D1 — Distribution Debug and Density Headroom

**Status:** Implemented, awaiting Unity validation.

V3J.3D1 is a narrow authoring patch following the successful initial V3J.3D distribution/exclusion test. It changes no production placement formula and does not modify the accepted ribbon geometry or ink shader.

Implementation contract:

- Extend `Painted Accent Stroke Density` from `0–80` to `0–240` in `GroundSurfaceFeatureRecipe`, the consolidated `GeneratedGround` controls, and `GroundSurfaceStyleProfileEditor`.
- Extend the descriptor generator target cap to 240 so the new range is effective.
- Keep Stroke Density semantics as pre-rejection weighted proposals; never backfill rejected proposals.
- Add per-GeneratedGround serialized debug toggles for live distribution, live weighted proposals, and last accepted positions.
- Generate the live distribution/proposal snapshot through the exact production candidate/noise/priority helpers rather than a visually similar duplicate formula.
- Render a 21x21 Scene view point heatmap without textures or scene objects.
- Display compact last-generation proposal and rejection totals directly in the GeneratedGround inspector.
- Preserve rivers, modifiers, terrain continuity validation, descriptor topology, ribbon geometry, flat ink, base mesh, collider, and scenes unchanged.

Validation procedure:

1. Enable **Show Distribution Overlay** and vary Patch Scale/Patchiness; confirm the point field updates without building the 3D preview.
2. Enable **Show Weighted Proposals** and vary Shape Seed/Stroke Density; confirm deterministic live proposal changes.
3. Build/regenerate and enable **Show Last Accepted Positions**; compare with visible lines and diagnostics.
4. Confirm **Last Generated** counts equal the console build diagnostic.
5. Test density 80, 120, 160, and 240. Confirm target/proposed count increases and accepted count responds subject to exclusions.
6. Confirm rejected proposals are still not backfilled.
7. Confirm the accepted placement formula and all geometry/rendering output remain unchanged apart from having more available proposals.

### Patch V3J.3D1a — Distribution Debug Visibility Correction

**Status:** Implemented, awaiting Unity validation.

The first V3J.3D1 Unity test proved that density values above 80 and the placement/rejection statistics were functioning, but none of the three Scene-view overlays was practically visible. This is a debug-presentation defect, not a placement-generation defect.

Implementation contract:

- Keep the exact production 21x21 distribution sample grid and proposal-selection helpers.
- Preserve a fixed `resolution × resolution` sample array, including explicit invalid sample entries, so heatmap cell topology remains stable.
- Replace the tiny point field with 20x20 translucent surface-following filled cells coloured blue-to-red by patch weight.
- Draw proposal centres as large screen-stable cyan/yellow crosses with dark under-strokes.
- Draw last accepted centres as larger solid green discs with dark outlines.
- Use `Handles.zTest = CompareFunction.Always` for all placement-debug layers.
- Add a Scene-view legend showing layer meanings plus sample, proposal, and accepted counts.
- Treat an empty or malformed live snapshot as invalid and display a visible warning in both the legend and GeneratedGround inspector.
- Do not alter density ranges, patch weighting, candidate generation, proposal selection, river/modifier/slope/grade rejection, descriptor geometry, ink rendering, base mesh, collider, or scenes.

Validation procedure:

1. Select one GeneratedGround and enable **Show Distribution Overlay**; confirm a filled blue-to-red field is immediately visible without zooming into individual points.
2. Enable **Show Weighted Proposals**; confirm large crosses remain clear over both pale ground and scene objects.
3. Enable **Show Last Accepted Positions**; confirm green discs align with generated stroke centres and remain visible through geometry.
4. Confirm the legend reports `Samples: 441/441` on a complete surface and proposal/accepted counts matching the active snapshot and last generation.
5. Toggle each layer independently and confirm only that layer disappears.
6. Confirm proposal/rejection diagnostics and generated placement are unchanged from V3J.3D1.

### Patch V3J.3D2 — Effective Weight Debug and Sparse-Area Control

**Status:** Implemented, awaiting Unity validation.

The V3J.3D1a heatmap proved the patch field is active, but validation showed three authoring ambiguities: the overlay displayed patch noise rather than the full proposal weight, the fixed `0.18` sparse floor limited concentration strength, and filled accepted markers obscured proposal crosses. Unity also reported repeated `Editor.targets` misuse from `OnSceneGUI`.

Implementation contract:

- Add `paintedAccentDistributionSparseFloor` to `GroundSurfaceFeatureRecipe` with range `0.02–0.40` and default `0.18`.
- Expose `Distribution Sparse Floor` in `GroundSurfaceStyleProfileEditor` and in the selected `GeneratedGround` Painted Accent controls.
- Feed the sparse-floor value through `FieldSettings`, patch-weight evaluation, placement signatures, diagnostics, and live debug generation.
- Add a per-GeneratedGround editor-only `Overlay Weight` enum with `Patch Preference` and `Effective Proposal Weight` modes.
- Store both patch weight and effective proposal weight in distribution samples and proposal debug points.
- Define effective proposal weight as patch weight multiplied by the same semantic weight used by production weighted selection.
- Display selected-mode minimum/mean/maximum values in the Scene-view legend.
- Colour proposal crosses by effective proposal weight.
- Replace filled accepted discs with green rings so proposal crosses remain visible; crosses without rings identify rejected proposals.
- Remove all `targets` array access from `GeneratedGroundEditor.OnSceneGUI`; use the singular `target` property.
- Preserve candidate-pool size, weighted-random algorithm, no-backfill semantics, exclusions, geometry, flat ink, base mesh, collider, scenes, and authored style assets.

Recommended proof settings:

```text
Stroke Density = 180
Distribution Patch Scale = 11 m
Distribution Patchiness = 0.92
Distribution Sparse Floor = 0.05
```

Validation procedure:

1. Select one `GeneratedGround`, enable Gizmos and all placement layers, and confirm no `targets array should not be used inside OnSceneGUI` warning is emitted.
2. In `Patch Preference` mode, observe the pure blue-to-red noise preference field.
3. Switch to `Effective Proposal Weight`; confirm semantic support can cool or warm areas relative to the patch-only view.
4. Confirm the legend updates its mode label and min/mean/max values.
5. Confirm every accepted position is a green ring around a still-visible proposal cross; rejected proposals remain crosses without rings.
6. Compare Sparse Floor `0.18` with `0.05` at the same seed and settings. The lower value should produce stronger broad sparse/dense contrast while retaining occasional proposals outside warm patches.
7. Regenerate twice and confirm deterministic proposal, acceptance, and rejection results.
8. Confirm river/modifier/slope/grade rejection, crowned geometry, and flat-ink rendering are unchanged.

Acceptance requires stronger controllable patch contrast, truthful effective-weight visualization, unobscured proposal/acceptance comparison, and removal of the Unity editor warning without any production-placement regression.

### Patch V3J.3D3 — Single-Mound Bias Refinement

**Status:** Unity-validated as the plateau correction; superseded by V3J.3D4 for apex softening.

V3J.3D2 made patch placement controllable and truthful, but gameplay-camera inspection exposed one remaining geometry polish issue: many longer Painted Accent strokes still read as shallow long plateaus even with Fold Irregularity near 0.8. The defect is longitudinal profile shaping, not placement, exclusions, crown topology, or the flat-ink shader.

Implementation contract:

- Keep descriptor placement, centreline curvature, patch weighting, proposal selection, rivers, modifiers, slope/grade rejection, cross-sectional crown geometry, and ink rendering unchanged.
- Increase minimum ribbon longitudinal rows from 13 to 17.
- Permit length-driven resolution up to 25 rows with an approximate 0.09 m target spacing.
- Narrow per-stroke Fold Height scaling from `0.82–1.00` to `0.94–1.00`.
- Normalize each shaped profile toward a deterministic `0.90–1.10` peak target.
- Promote the peak target and mound guide according to actual stroke length and generated width, weighted 65% length / 35% width.
- Detect the contiguous raw high span at 86% of raw peak.
- Increase mound-guide blend and sharpness for broad high spans so long plateaus collapse into one dominant crest.
- Resolve the preferred crest near the weighted centre of an almost-equal raw high region rather than selecting an arbitrary edge sample.
- Use deterministic asymmetric left/right mound powers so irregular strokes retain unequal shoulders.
- Retain targeted valley repair and do not restore hard monotonic rise/fall enforcement.
- Add mound target, guide blend, raw plateau span, and plateau-suppressed stroke counts to the build diagnostic.
- Add no new inspector control and modify no style asset.

Expected current-range topology:

```text
17 rows × 3 vertices = 51 vertices per stroke
16 spans × 2 crown strips × 2 triangles = 64 triangles per stroke
```

At 220 fully accepted strokes:

```text
11,220 vertices
14,080 triangles
```

Validation procedure:

1. Keep the accepted D2 placement seed, density, Patch Scale, Patchiness, Sparse Floor, exclusions, width, crown, and flat-ink settings.
2. Use Fold Height `0.20`, Fold Irregularity `0.80`, Fold End Taper `0.40`, and Stroke Length `0.50–0.90` for the first comparison.
3. Rebuild the same seed before and after the patch from the gameplay camera.
4. Confirm long nearly level crest shelves are materially less common.
5. Confirm most longer/wider marks form one readable mound rather than a flat bar.
6. Confirm actual crest-height diagnostics cluster near requested Fold Height rather than substantially below it.
7. Confirm left/right shoulder asymmetry and smaller seeded profile changes remain visible.
8. Confirm double-hill/M profiles do not return.
9. Confirm placement positions, proposal/rejection diagnostics, river/modifier clearance, crown cross-section, and flat-ink rendering are unchanged.
10. Capture the complete build log, including the new mound and plateau diagnostics.

Acceptance requires a stronger gameplay-camera mound read without replacing the accepted distribution/rendering baseline or turning the collection into repeated identical `^` shapes.


### Patch V3J.3D4 — Rounded Crest Apex Refinement

**Status:** Unity-validated; apex softening did not materially improve the gameplay-camera result.

V3J.3D3 successfully removed the frequent long, weak plateaus, but gameplay-camera validation showed that the strengthened guide and sharpening produced too many narrow `^` apexes. V3J.3D4 is a local crest-shape correction only.

Implementation contract:

- Keep V3J.3D3 requested-height promotion, plateau detection, asymmetric mound guide, targeted valley repair, and 17–25 longitudinal rows.
- Keep descriptor placement, patch distribution, Sparse Floor, weighted selection, rivers, modifiers, slope/grade rejection, cross-sectional crown, flat ink, base mesh, collider, scenes, and style assets unchanged.
- Reduce plateau-guide contribution from `0.42` to `0.34`.
- Reduce mound sharpness constants from `1.35 / 0.65 / 0.85` to `1.15 / 0.50 / 0.50` for base/span/plateau response.
- After mound shaping and valley repair, redetect the actual shaped peak.
- Create a deterministic rounded crest cap with a 10%–16% half-span, small left/right asymmetry, and at least two rows per side where available.
- Use a SmoothStep interpolation from the peak to each side's existing cap-boundary height.
- Blend low neighbours toward the cap at `0.72`; use a `1.65` falloff power to keep the immediate apex curved; reduce already-high neighbours only at 35% of that strength to preserve irregularity.
- Preserve the peak sample, cap-boundary samples, exact endpoints, end envelopes, and final peak normalization.
- Add rounded-crest span and apex-softened stroke counts to diagnostics.
- Add no new inspector control and modify no style asset.

Validation procedure:

1. Use the same seed and accepted D2/D3 placement, width, length, Fold Height, Crown Height, irregularity, taper, exclusions, and ink settings.
2. Rebuild from the gameplay camera.
3. Confirm the D3 plateau fix remains effective.
4. Confirm sharp roof-like `^` peaks are substantially reduced.
5. Confirm the new top reads as a short curved apex, not a broad flat shelf.
6. Confirm asymmetrical rises/descents and smaller seeded irregularities remain.
7. Confirm double-hill/M shapes do not return.
8. Confirm placement positions, proposal/rejection diagnostics, river/modifier clearance, topology, crown cross-section, and flat-ink rendering are unchanged.
9. Capture the full log including `roundedCrestSpanMin/Mean/Max`, `roundedCrestBlend`, `roundedCrestFalloffPower`, and `apexSoftenedStrokes`.

Acceptance requires the middle ground between the rejected extremes: a clear single mound with a visibly curved crest, neither a long plateau nor a pointed roof.


### Patch V3J.3D5 — Environment-Integrated Flat Ink

**Status:** Implemented, awaiting Unity validation.

The flat-unlit ink baseline made every point of the ribbon visually uniform, but Unity validation under cast shadows and night lighting showed that this was too disconnected from the terrain. Painted Accent marks preserved their daytime colour while the ground darkened, producing bright floating tracers.

Implementation contract:

- Keep the accepted double-sided crowned ribbon, placement, patch distribution, Sparse Floor, exclusions, topology, Ink Color control, and no-collider architecture unchanged.
- Keep one combined mesh and one shared material.
- Keep `Cull Off`, opaque depth writing, and shadow casting disabled.
- Change the Painted Accent shader pass from `SRPDefaultUnlit` to `UniversalForward`.
- Include URP ambient, main-light, main-shadow, additional-light, and additional-light-shadow variants.
- Pass world position to the fragment shader.
- Compute one illumination scalar from ambient spherical-harmonic luminance, main-light luminance/attenuation/shadow attenuation, and restrained additional-light luminance/attenuation.
- Do not use mesh normals or any `dot(normal, lightDirection)` term.
- Convert light colour to luminance so lighting changes value but not the authored ink hue.
- Clamp final exposure between `0.14` and `1.0`.
- Use proof responses `0.75 ambient`, `0.80 direct`, `0.70 shadow`, and `0.25 local light`.
- Set the preview renderer to receive shadows while preserving `ShadowCastingMode.Off`.
- Add the lighting policy and response constants to the build diagnostic.
- Add no new inspector control and modify no style asset.

Validation procedure:

1. Use the same seed, geometry, distribution, exclusions, and Ink Color before and after the patch.
2. Compare one stroke in open daylight and beneath a strong cast shadow.
3. Confirm the shadowed portion darkens without any crown/shoulder or front/back normal gradient.
4. Evaluate dusk and full night; confirm marks no longer remain bright while the ground approaches darkness.
5. Move a point or spot light near the marks; confirm local light restores visibility but never makes the ink brighter than its authored colour.
6. Confirm coloured lights do not shift the authored ink hue.
7. Confirm the ribbon still casts no shadow and remains double-sided.
8. Confirm topology, placement counts, rejection counts, geometry diagnostics, and authoring controls are unchanged.
9. Confirm the log reports `surfaceMode=EnvironmentIntegratedFlatInk`, all five response constants, `receiveShadows=True`, and `shadowCasting=Off`.

Acceptance requires terrain-integrated illumination and shadow response without returning to physically shaded ridge faces, highlights, material gloss, glow, or emission.

## GSU-M1.9A.4 — Balanced Toroidal Mix and Hard Rock Contour

**Status:** Visually superseded by GSU-M1.9A.5 after Unity exposed persistent macro cross bias, excessive micro-fillers, and insufficient authored worn-edge definition.

GSU-M1.9A.3 is visually rejected. Its source-preserved reconstruction improved interior verticality, but Unity evidence exposed two remaining packed-source defects: the repaired source layout segregated large and small stones into repeatable macro regions that formed visible cross/square patterns when tiled, and the stone-to-gap transition remained too gradual, causing individual forms to read as soft dirt mounds rather than hard rocks. A4 replaces only the two temporary A/B packed payloads while retaining their GUIDs, stable IDs, importer settings, Ground adapters, and serialized selections. The temporary legacy filenames remain cleanup debt until one candidate replaces canonical `fine-gravel`.

A4 starts from the coherent periodic A3 vertical height/form data and applies only deterministic, periodic operations. Two independently phase-warped copies of the same source layout are combined without alpha-blended ghosting: the second copy contributes only substantial stone bodies inside genuinely low regions of the first. This breaks the previous tile-axis size bands and interleaves large, medium, and small forms more chaotically while preserving a single coherent height field. Both active candidates use exactly the same redistributed layout, coverage, cavity topology, and non-directional form variation:

- **`Fine Gravel A4 - Balanced Mix`** uses a moderately compressed contact wall and restrained slope amplitude.
- **`Fine Gravel A4 - Hard Rock Contour`** uses a narrower contact wall, stronger edge-normal energy, stronger stone-side cavity shoulder, and stronger neutral edge/body separation. It is the leading candidate for the requested hard-rock delimitation, but no winner is declared before Unity evidence.

The mixed layout covers approximately `58.6%` of the tile. High regions remain localized rather than broad plateaus: Balanced Mix places about `2.43%` of stone pixels above `0.90` height and `9.31%` above `0.75`; Hard Rock Contour places about `2.75%` above `0.90` and `11.38%` above `0.75`. Mean edge-gradient energy is approximately `1.94×` the inner-body gradient for Balanced Mix and `2.37×` for Hard Rock Contour. The final R/G slopes are re-derived from each periodic height field; B remains a hierarchical deep-gap core plus narrow stone-side contact shoulder; A remains lighting-neutral.

Runtime architecture and cost do not change: three temporary 256² RGBA32 mipmapped slices remain during evaluation, only the selected substrate slice is sampled, and there is no new shader sample, ALU branch, draw call, renderer, mesh data, River data, or runtime CPU process. No C#, HLSL, ShaderLab, River source, scene, prefab, canonical Fine Gravel assignment, or unrelated material changes in this patch.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, compare the two A4 choices with identical shared/application values from the same close and production cameras, include dry and wet views, and judge local size mixing, absence of the prior cross/square macro pattern, hard contour readability, internal form, cavity width, repetition, seam visibility, and mip survival. Select a winner or reject both; do not tune the shader to conceal a deficient packed source.


## GSU-M1.9A.5 — Source-Art Packed Conversion, Macro Rebalance, and Worn Edge Accent

**Status:** Implemented and source-audited; Unity comparison pending.

### Objective

Replace the visually rejected A4 temporary Fine Gravel payloads with two controlled 256² candidates derived from the user-approved worn-rock source image. Preserve the reusable one-sample packed-detail architecture while correcting the three Unity-observed defects: repeated cross/square macro size segregation, excessive tiny-stone noise at gameplay distance, and insufficient hard-rock edge definition.

### Reviewed evidence

- Unity A4 repeat evidence shows a stable cross-like macro region where small stones concentrate through the tile centre while larger stones dominate surrounding regions.
- Unity close and production-camera evidence shows improved internal verticality but weak stone delimitation; rocks read as soft mounds because the packed source lacks an explicit bright worn-rim signal.
- The approved source image contains a better large/medium/small hierarchy, fewer micro-fillers, dark crevices, hard contours, and visible worn edge highlights. It is a beauty source only and must not be sampled directly as packed material data.
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl` already maps positive A-channel variation toward the material light colour and maps B to contact/deep-cavity bands; therefore A can carry a lighting-neutral worn-rim accent without adding a shader sample or material-name branch.

### Approved files

- the five canonical Ground documents;
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset`;
- the two existing temporary `SSMP_FineGravel_AB_*` assets;
- the two existing temporary `GSLP_FineGravel_AB_*` assets;
- the two existing temporary packed PNG payloads.

Importer metadata, GUIDs, stable IDs, C#, HLSL, ShaderLab, River source, scenes, prefabs, canonical `fine-gravel`, and unrelated materials are outside scope.

### Implementation sequence

1. Treat the approved image as authoring source only. Extract stone silhouettes, neutralized surface character, crevice structure, and worn-edge cues.
2. Repack extracted source stones on a 1024² toroidal authoring canvas with local size-class balancing. Medium stones dominate; large stones remain distributed; the smallest filler class is capped and used only where necessary.
3. Construct coherent per-stone height with localized crowns, irregular internal form, and compressed contact walls. Derive final R/G slopes only after area-downsampling to 256².
4. Derive B as hierarchical contact shoulder plus deep crevice core. Derive A as neutral body variation plus an explicit narrow positive worn-rim band; no directional sunlight or cast shadow is baked.
5. Produce two candidates from the identical periodic layout: a restrained worn-edge candidate and a stronger worn-edge/contour candidate. Retain the temporary A/B GUID and stable-ID plumbing for safe serialized migration.
6. Validate 3×3 repeat, 256/128/64/32 mip survival, edge-neighbourhood continuity, size-density balance, small-stone share, packed-channel ranges, and a CPU reference matching current packed-detail decode. Package only after those checks pass.

### Invariants and non-goals

- Runtime remains one packed sample per active detailed substrate.
- Runtime resolution remains 256² RGBA32 with the existing mip/import contract.
- No geometry, parallax, displacement, extra draw call, renderer, runtime CPU process, or River contract change.
- The explicit rim is an authored material-form cue in A, not a world-light direction and not a replacement for URP lighting.
- A5 is rejected historical evidence. GSU-M2.0 is the active material gate; canonical Fine Gravel remains unfrozen until the imported authored-colour candidate passes Unity comparison and explicit user acceptance.

### Acceptance criteria

- No visible cross, square, quadrant, or axis-aligned size-density pattern in 3×3 repeats or Unity gameplay views.
- Medium stones dominate and tiny filler stones no longer create distant visual noise.
- Individual stones show a clearly delimited hard contour and restrained bright worn rim.
- Internal stone form and verticality remain at least as strong as the accepted part of A4.
- No broad seam band at full resolution or lower mips.
- No runtime architecture or cost regression.

### Implementation result

Historical A5 replaced the two temporary A4 texel payloads in place and retained their existing GUIDs, stable IDs, importer settings, Ground adapters, and serialized selections. Its choices, **`Fine Gravel A5 - Worn Edge`** and **`Fine Gravel A5 - Strong Rim`**, are visually rejected because the packed-only conversion discarded the authored colour and broad form that made the source attractive. They remain cleanup debt only and are not active validation candidates. GSU-M2.0 supersedes them with **`Fine Gravel — Imported Stone Ground 01`**.

The source is cropped to a low-discontinuity 1024² region, projected to a periodic luminance field, segmented into stone bodies, stripped of broad directional lighting planes per stone, and converted into coherent height, slope, cavity, neutral source character, and worn-rim data. The two candidates share one layout, height field, cavity topology, and source character; only packed slope amplitude, A-channel rim strength, and matching generic profile strengths differ.

The runtime tile contains `109` recognized stones after removal of sub-runtime fragments: `31` large, `41` medium, and `37` small by count. Small stones occupy about `0.94%` of runtime texels, medium stones about `9.78%`, and large stones about `51.10%`; total stone coverage is about `61.82%`. The smallest class therefore remains available as sparse filler without recreating the distant micro-pebble carpet. R/G are derived after final 256² downsampling, B remains the hierarchical crevice/contact signal, and A contains neutralized source texture plus a narrow positive worn-rim cue.

Static 256/128/64/32 packed and shader-reference tests report a worst wrap-to-ordinary-adjacency ratio of approximately `1.29`; no exact Unity seam, mip, lighting, dry/wet, or production-camera acceptance is claimed until the project test. Runtime architecture and nominal cost remain unchanged: three temporary 256² slices during evaluation, one packed sample for the selected substrate, no new shader branch, draw call, geometry, renderer, or runtime CPU process.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, compare **Worn Edge** and **Strong Rim** with identical shared/application values from the same close and production cameras, include dry and wet views, and judge macro repetition, distant noise, worn-rim readability, internal form, cavity width, and mip stability. Promote neither candidate until explicit visual acceptance.
