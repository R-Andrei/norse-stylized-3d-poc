# Weather LightRay and Cloud Inspector Cleanup Plan

## Status

**Current patch:** `WEATHER-LIGHT-RAY-CLEANUP-V1.3A5-AUTHORITY-CLOSURE`

**Current state:** V1.3A/A1/A2/A3/A4 are runtime-accepted. V1.3A5 is the approved authority-closure patch: it removes source-level serialized appearance/evolution fallbacks that A4 no longer consumes, replaces the last global transition-getter side effect with explicit tick-time cleanup, and freezes the generic per-ray preset architecture. No serialized scene/prefab/preset asset is edited because authoritative live serialization is not present in the supplied archive.

## Objective

Clean the current Weather LightRay and cloud authoring surfaces without changing the frozen cloud-cookie producer or the indexed vegetation-accent sidecar.

The patch will:

1. make LightRays source-agnostic at the core architecture level;
2. remove the unused preset catalog and disconnected preset-selection infrastructure;
3. remove obsolete non-shader LightRay diagnostics and Editor smoke tests;
4. separate automatic-population dependency suspension from cloud-transition spawn pausing;
5. preserve existing automatic rays through cloud-pattern transitions;
6. remove obsolete or derived automatic-population controls;
7. make the camera-ground footprint the only automatic-population region shape;
8. make the LightRay and Cloud Inspectors production authoring surfaces with one telemetry root each;
9. replace stale Weather documentation with the current agreed architecture;
10. preserve `_TEST.asset` as an intentional high-strength testing preset that future production orchestration must ignore.

## V1.3A5 — LightRay authority closure

### Status

**Patch identifier:** `WEATHER-LIGHT-RAY-CLEANUP-V1.3A5-AUTHORITY-CLOSURE`

**Approval state:** approved for implementation after a second A4 code-consumer audit. This section is the canonical implementation plan and is written before source edits. Unity validation remains pending until the resulting overlay is compiled and exercised in Unity 6000.5.0f1.

### Objective

Remove the remaining serialized appearance/evolution fallback authority and migration baggage that A4 no longer consumes, while preserving every validated A4 runtime contract. The closure must leave one appearance authority per ray: Preset Override first, Controller Default Preset second, and no legacy appearance fallback.

### Approved files

Modify exactly:

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`
- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`

No renderer, population, cloud, shader/HLSL, preset asset, scene, prefab, material, `.meta`, layer, tag, package, or project-setting modification is approved. The intentional `_TEST.asset` remains untouched.

### Reviewed evidence

- A4 `WeatherLightRayAnchor.BuildLocalDescriptor()` constructs only request-local source, lifecycle, placement, spacing, seed, and local-intensity state. It deliberately supplies neutral placeholder presentation values; `WeatherLightRayController.UpdateAuthoredSlot()` must apply the resolved preset before the descriptor becomes active. Therefore the old Anchor appearance/evolution serialization is no longer runtime authority.
- A4 procedural slots already require a resolved override/default preset and use the same `WeatherLightRayPreset.ApplyTo()` boundary.
- The vegetation sidecar publisher reads intensity, coverage, and softness from each active ray descriptor. No Controller fallback vegetation field participates in publication.
- Beam evolution reads `descriptor.EvolutionStrength` and `descriptor.EvolutionSpeed`; Controller fallback evolution fields are not part of active slot construction.
- The four Anchor convenience properties `AreaLayout`, `BeamCount`, `BeamPitchMetres`, and `FootprintRadiusMetres` calculate from local serialized spacing even when preset spacing is authoritative. They have no project consumer and can misreport resolved runtime layout.
- `PresetPresentationBlend` is no longer a valid global presentation API, but its getter currently clears completed Controller-default transition bookkeeping. The property may be deleted only after that side effect is replaced by explicit tick-time transition finalization.
- `previousPresentationPreset`, `presetTransitionStartedAt`, and `presetTransitionDurationSeconds` remain required because inherited rays created or updated during an active Controller-default transition must join that transition.
- The supplied archive still does not contain authoritative live scene/prefab LightRay serialization. A5 therefore proves runtime-safety, not recoverability of historical values stored in fields that A4 already ignores. No serialized scene/prefab asset is edited by this patch.

### Anchor deletion contract

Retain only genuine instance/request serialization: Controller Override, Preset Override, Edit Mode Preview, Source Kind, Cloud Policy, Source Gate Policy, Lifetime Policy, fade/hold durations, external visibility, Height, Maximum Visual Lean, Area Diameter, Beam Spacing, Variation Seed, Local Intensity Multiplier, and Override Preset Beam Spacing. Preserve the `FormerlySerializedAs("legacyBeamSpacingMetres")` compatibility attribute on Beam Spacing.

Delete all legacy Anchor presentation/migration serialization and its support code: version counters, legacy beam-count/width/packing data, preset-owned beam appearance, colour/warmth/atmospheric presentation, surface-response fallbacks, hidden historical surface/cloud values, footprint migration values, Anchor-local evolution override/preset/strength/speed, migration constants/methods, presentation clamps, and obsolete appearance accessors.

Delete the unused/misleading Anchor convenience layout properties `AreaLayout`, `BeamCount`, `BeamPitchMetres`, and `FootprintRadiusMetres`; Inspector/runtime telemetry must continue deriving layout from the resolved descriptor or from explicit resolved preset/local-spacing authoring logic.

### Controller deletion contract

Delete the five serialized fallback-authority fields for Controller vegetation/evolution presentation and all fallback-only clamps/helpers/properties that depend on them. Delete the redundant default-preset vegetation summary from the comprehensive report; per-ray descriptor vegetation telemetry remains.

Rename private `SharedAccentLine...` mapping identifiers to vegetation-specific terminology without changing their numeric mapping. The mapping remains a pure per-ray conversion from descriptor vegetation intensity to sidecar scale.

Delete the global `PresetPresentationBlend` property only after adding explicit default-preset transition finalization to the regular Controller tick. The transition finalizer must clear only completed Controller-default transition bookkeeping and must not alter any per-slot transition state.

### Invariants / non-goals

1. No A4 population, A1 camera-footprint, A2 turnover, cloud-transition, cloud-query, renderer grouping, source-resolution, surface Spot, vegetation sidecar, shader, or beam-evolution calculus change.
2. No change to preset artistic values or preset assets. `_TEST.asset` remains intentionally retained.
3. No deletion of generic gameplay/request contracts such as Spawn Priority, Movement Policy, Gameplay Channel, or GameplayRequested origin merely because current producers use only a subset.
4. Keep historical public Controller `ActivePreset` / `TrySetActivePreset` API names for compatibility while authoring continues to present Default Preset.
5. Keep the D3D12-safe renderer vegetation fallback buffer; it is unrelated to deleted appearance fallback authority.
6. No raw serialized scene/prefab/material/preset edits. Historical ignored field values may cease to deserialize after source-field deletion; A4 runtime behavior must remain unchanged.

### File-by-file implementation sequence

1. Update this canonical plan first and record the second audit evidence and exact deletion contract.
2. Simplify the Anchor serialization/public surface/validation to the retained request-local state only.
3. Simplify Controller fallback presentation state, add explicit default-transition finalization, remove redundant report telemetry, and rename only private vegetation accent mapping identifiers.
4. Update the canonical LightRay and Weather current-state documents to freeze the closed authority contract and distinguish historical A4 migration notes from current state.
5. Re-run repository-wide consumer scans for every deleted symbol, verify Editor serialized-property bindings, compare protected A4 modules byte-for-byte, and package only the exact approved changed files.

### Acceptance criteria

- Deleted Anchor/Controller fallback symbols have zero live C# consumers.
- Anchor retains exactly the approved request-local serialized state and no hidden presentation authority.
- Every active authored/procedural ray still requires a resolved override/default preset.
- Controller Default Preset transitions still allow inherited slots to join an in-progress transition; completed Controller-level transition bookkeeping is explicitly cleared without the removed getter side effect.
- Per-ray vegetation descriptor publication, renderer presentation grouping, source resolution, automatic population, A1 footprint recovery, A2 stateless turnover, and A3 sidecar contract are unchanged.
- Current Inspectors resolve every serialized property they draw.
- No approved-external project file changes.
- Available static/contract/scope checks pass; Unity compilation/runtime checks are reported pending until executed in the project.

### Implementation status

- [x] Gate 1 review and second deletion-consumer audit complete.
- [x] Canonical A5 plan recorded before source edits.
- [x] Anchor authority cleanup.
- [x] Controller authority/transition cleanup.
- [x] Canonical architecture/current-state update.
- [x] Final scope/consumer/protected-contract audit.
- [x] Package overlay and provide Unity validation contract.

### A5 final audit evidence

The final A5 source/scope audit passed 107 checks with zero failures. It verified the exact 18-field Anchor serialization, removal of every approved legacy Anchor/Controller authority symbol, explicit Controller-default transition finalization, continued authored/procedural preset requirements, descriptor-owned vegetation publication, current Inspector serialized-property resolution, lexical/delimiter integrity of both modified C# files, exact five-file project scope, and byte identity of the A4 renderer, population, preset/types, vegetation shader/include, Editors, and `_TEST.asset` protected surfaces. Unity compilation and Play Mode validation remain pending because Unity is unavailable in the implementation environment.

### A5 implementation evidence

- Anchor serialization is reduced to the 18 approved request-local fields; all 30 legacy presentation/migration fields and their helper/accessor code are removed.
- Controller fallback vegetation/evolution serialization and fallback-only public properties are removed.
- `PresetPresentationBlend` is removed; completed Controller-default transitions are finalized explicitly from the Controller tick while per-slot transition state remains untouched.
- Vegetation accent mapping remains numerically identical but uses vegetation-specific private naming and still consumes descriptor values per ray.
- No renderer, population, cloud, shader/HLSL, preset asset, scene, prefab, material, or project-setting implementation change is part of A5.

## V1.3A4 — Per-ray preset authority and presentation grouping

### Status

**Patch identifier:** `WEATHER-LIGHT-RAY-CLEANUP-V1.3A4-PER-RAY-PRESET-AUTHORITY`

**Approval state:** approved and implemented. This section was the canonical implementation plan and was written before source edits. Unity validation remains pending.

### Objective

Make a LightRay preset resolve per ray rather than globally while preserving the Controller preset as the inherited default. Complete the downstream authority change so every rendered/receiver-facing value comes from the resolved ray descriptor/snapshot rather than the Controller default. Support simultaneous weather, quest, nighttime, storytelling, and other LightRay uses with different presets and source policies.

The authoring contract is:

- the Controller field remains serialized as `activePreset` for compatibility but is presented to users as **Default Preset**;
- an authored Anchor may assign **Preset Override**; `None` inherits the Controller Default Preset;
- a procedural/gameplay `WeatherLightRaySpawnRequest` may supply an optional preset override; `null` inherits the Controller Default Preset;
- every active ray must resolve either an override or the Controller default; no renderer or receiver may silently consult legacy appearance fallbacks after slot resolution;
- vegetation accent intensity, coverage, and softness remain authored only on `WeatherLightRayPreset` under **Surface Response**; they are not duplicated on Controllers or Anchors;
- authored `Source Kind` remains request-local policy and is exposed in the Anchor Inspector; preset source metadata is not runtime authority.

### Approved files

Modify:

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`
- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayPresetEditor.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs`

No scene, prefab, preset asset, material, renderer asset, `.meta`, layer, tag, package, project-setting, cloud-runtime, cloud-shader, vegetation-HLSL, or vegetation-shader modification is approved. `_TEST.asset` remains present and unchanged.

### Reviewed evidence

- `WeatherLightRayDescriptor` already carries per-zone geometry, beam appearance, atmospheric appearance, surface response, evolution, lifecycle, source policy, and variation state. `RuntimeSlot` already owns per-ray lifecycle/evolution state. This is the correct resolution boundary for per-ray preset application.
- `WeatherLightRayPreset.ApplyTo` already converts local request state plus preset presentation into a final descriptor. The Controller is the current global caller; the method itself does not require global preset ownership.
- `WeatherLightRaySpawnRequest` currently has no preset reference. Adding one optional reference is CPU-only and does not affect GPU buffer layouts or Burst/job contracts.
- `WeatherLightRayAnchor` still contains serialized legacy appearance/evolution fields, but the custom Inspector already treats shared presentation as preset-owned. Those legacy fields remain serialized in A4 because the supplied archive does not contain authoritative live scene/prefab serialization for destructive migration. A4 stops using them as runtime appearance authority without deleting their serialized data.
- The indexed vegetation sidecar is already one record per URP additional light. Its protected two-`float4` layout can carry different parameters for every LightRay Spot without any HLSL or renderer-buffer-layout change. The current CPU publisher is the remaining global-authority defect because it builds one Controller-preset accent record and assigns it to every LightRay Spot.
- The atmospheric mask draw is already per ray, but the current render pass accumulates every selected zone into one scalar R16 mask and performs one softening/composite using the first visible zone's parameters. It also filters every zone to the first selected `SourceKind`. These are the two renderer-global assumptions that block correct simultaneous per-ray presets/source policies.
- The final composite uses shared colour and softening parameters. Screen-space surface influence also consumes per-zone centre/bounds. Therefore batching must be based on resolved final-pass compatibility rather than only preset object identity. Rays with active screen-space surface response require isolated groups unless their complete surface state is identical.
- Source colour/intensity and presentation direction are currently resolvable by the Controller before snapshots are published. Moving the render-facing numeric source presentation into each snapshot removes the renderer's need to choose or query one source family.

### Invariants

1. **One appearance authority per ray.** A ray resolves exactly one preset: explicit override first, Controller Default Preset second. Missing both is an actionable configuration/spawn error.
2. **Preset remains appearance-only.** No preset source-family, Weather eligibility, time-of-day, cloud, quest, or gameplay ownership contract is introduced.
3. **Anchor/request retain local policy.** Source kind/direction, cloud policy, source gate, lifecycle, placement, geometry overrides, seed, and local intensity remain per ray.
4. **Descriptor/snapshot are downstream authority.** After slot resolution, atmospheric rendering, surface Spots, vegetation publication, and reporting consume resolved per-ray descriptor/snapshot data. They do not ask which Controller preset is active.
5. **Vegetation authoring stays on the preset.** `Vegetation Accent Intensity`, `Vegetation Accent Coverage`, and `Vegetation Accent Softness` are authored under the preset's Surface Response section only. The protected indexed sidecar remains exactly two `float4`s and `parameters.w = 1` remains independent of intensity.
6. **Common weather path remains one presentation group.** Multiple rays with compatible final-pass presentation continue through one mask/soften/composite sequence. Additional full-screen sequences occur only for genuinely incompatible simultaneous presentation groups.
7. **No per-frame managed allocation.** Group storage and draw metadata are preallocated/reused and bounded by LightRay slot capacity. No LINQ, dictionary-based grouping, recent-history cache, or unbounded collection is introduced.
8. **Source semantics are resolved before rendering.** The render pass consumes resolved colour, intensity, direction, and visibility state. It does not filter the ray set by a global `SourceKind`.
9. **Existing validated population remains inherited-default behavior.** The current atmospheric automatic producer uses the Controller Default Preset unless a future Weather orchestrator explicitly supplies another preset. Population, cloud qualification, A1 footprint recovery, and A2 stateless turnover calculus are otherwise unchanged.
10. **Serialized destructive cleanup remains deferred.** A4 may remove the obsolete C# preset `SourceKind` member because it has no production consumer, but it does not raw-edit preset assets or delete legacy serialized Anchor/Controller appearance keys from scenes/prefabs/assets.

### Presentation-group contract

The renderer partitions visible rays by the resolved parameters that must be shared after individual beam-mask drawing. The group signature includes the final atmospheric colour, softening strength/radius/direction, and any active screen-space surface presentation state required by the composite. Per-ray values already consumed during mask drawing—position, area, beam layout, current intensity, lifecycle, seed, beam evolution, camera fade, and local intensity—remain per draw and do not force a new group unless they alter a shared final-pass value.

For the normal automatic-weather case, rays use the same default preset, source presentation, area/layout, and softening contract, so the expected group count remains one. A distinct quest/story preset or incompatible resolved source presentation creates an additional group.

The RenderGraph executes groups sequentially:

```text
Group A rays -> mask A -> soften A -> composite A -> colour A
Group B rays -> mask B -> soften B -> composite B -> colour B
...
```

Transient mask/softened/destination textures are logical RenderGraph resources with bounded group count. The implementation must not permanently enlarge the atmospheric mask format or sidecar format. Debug views may retain a single combined diagnostic group where required to preserve existing debug semantics; Final Composite must use full production grouping.

### Per-ray preset transition contract

- Controller `TrySetActivePreset` continues to transition the Controller default. Only rays inheriting that default participate. Explicit override rays do not transition because the default changed.
- Runtime slots retain the resolved target preset and enough local transition state to evaluate the correct previous preset/blend.
- A new inherited ray spawned while a Controller-default transition is active joins that transition.
- Changing an Anchor/request override without a dedicated per-ray transition-duration API is immediate in A4. A4 does not invent a new authoring duration control.
- Existing beam-layout evolution is independent and remains slot-local.

### File-by-file sequence

1. Record this plan and lock the approved scope before code edits.
2. Extend shared request/descriptor/snapshot contracts for optional preset override, resolved vegetation values, resolved preset telemetry, and render-facing source presentation.
3. Make preset application populate vegetation values and remove the obsolete runtime preset `SourceKind` member without editing serialized preset assets.
4. Add Anchor Preset Override and expose Source Kind; stop using legacy Anchor appearance/evolution fields to build active presentation while leaving their serialization intact for the later migration audit.
5. Resolve preset authority per runtime slot for authored and procedural rays; preserve inherited Controller-default transitions and make override changes immediate.
6. Build vegetation sidecar records from each ray descriptor rather than Controller-global accent values; keep the A3 GPU sidecar contract unchanged.
7. Make surface-light and atmospheric source presentation consume resolved snapshot source colour/intensity rather than one renderer-selected source state.
8. Remove SourceKind-family filtering from the renderer feature/pass and partition Final Composite draws into compatible presentation groups using fixed reusable CPU storage.
9. Keep the current automatic atmospheric population on inherited Controller Default Preset semantics and update internal terminology only; do not change A1/A2 population calculus.
10. Update Controller, Anchor, and preset authoring labels/help/status/reporting for Default Preset, Preset Override, resolved preset, source policy, and per-ray vegetation authority.
11. Reconcile canonical Weather and vegetation current-state documentation while preserving historical records as explicitly historical.
12. Run final scope/diff audit, symbol/consumer audit, descriptor-constructor audit, sidecar-layout equality check, renderer/shader boundary audit, lexical/delimiter checks, and available C# compilation/static checks. Record Unity validation as pending with exact final Inspector routes verified from final source.

### Risks and safeguards

- **False per-ray support:** fail the patch if any production renderer/surface/vegetation path still derives appearance from Controller `activePreset` after a descriptor/snapshot exists.
- **Vegetation cross-ray leakage:** sidecar parameters must be derived from the matching slot descriptor for each Spot Light; preserve indexed identity and source direction.
- **Renderer group explosion:** cost scales with distinct incompatible presentation groups, not ray count. Common identical weather rays must resolve to one group. Group storage is fixed/reused; no memory-backed anti-repeat or general cache is added.
- **Screen-space surface mismatch:** because surface influence requires per-zone centre/bounds, an active screen-space surface response may not be batched with another ray unless the complete shared composite state is compatible.
- **Source-family omission:** final production preparation must scan all visible rays and must not discard Sun/Moon/Independent rays because another source kind was encountered first.
- **Source-colour regression:** preserve the existing source colour/intensity and Sun warmth calculation, but resolve its numeric output before rendering.
- **Default-transition bleed:** explicit override rays must not interpolate when only the Controller default changes.
- **Serialized data loss:** do not delete legacy Anchor/Controller serialized appearance fields or raw-edit scenes/prefabs/preset assets in A4.
- **A1/A2 population regression:** keep footprint projection, candidate randomization, activation identity, lifetime, cloud qualification, and no-history policy unchanged.
- **A3 sidecar regression:** no HLSL, sidecar-record-layout, stride, URP-ordering, or camera-binding change.

### Acceptance criteria

- Controller Inspector presents `activePreset` as **Default Preset**.
- Anchor Inspector exposes **Preset Override** and **Source Kind**.
- Preset Inspector explicitly exposes Surface Response with **Vegetation Accent Intensity**, **Vegetation Accent Coverage**, and **Vegetation Accent Softness**; no preset source-kind authoring is shown.
- authored and procedural rays resolve override-first/default-second preset authority; missing both fails clearly.
- two simultaneous rays using different presets retain different descriptor colour/softening/vegetation values.
- automatic atmospheric rays with no override continue inheriting the Controller default and preserve current population behavior.
- vegetation sidecar records differ per LightRay Spot when resolved preset accent values differ; the two-`float4` GPU layout is unchanged.
- setting one preset's vegetation accent intensity to zero keeps sidecar override `w = 1` for that ray while its Weather-specific accent strength is zero.
- Final Composite renders all visible source kinds and does not select one global source family.
- compatible weather rays produce one presentation group; incompatible preset/source presentation produces multiple groups.
- no per-frame managed allocation is introduced by grouping after warm-up/capacity growth.
- default-preset transitions affect inherited rays only; explicit override rays remain on their override.
- Renderer Feature/Render Pass do not consult Controller Default Preset for per-ray appearance.
- cloud producer, cloud shaders, population qualification/footprint/turnover, beam-evolution calculus, surface Spot geometry, vegetation HLSL, and vegetation shader behavior remain unchanged except for receiving the correct per-ray sidecar values.
- C# source and shaders compile with zero Unity errors; runtime validation confirms simultaneous inherited and override rays, vegetation response, automatic population, and no missing-SRV warning.

### V1.3A4 implementation status

- [x] Complete source/consumer/producer/shared-contract review.
- [x] Record approved scope, invariants, authoring contract, risks, and acceptance gates before code edits.
- [x] Shared per-ray contracts implemented.
- [x] Preset/Anchor authoring implemented.
- [x] Runtime per-slot preset resolution implemented.
- [x] Per-ray vegetation sidecar publication implemented.
- [x] Per-ray source presentation implemented.
- [x] Presentation-group renderer implemented.
- [x] Documentation reconciliation completed.
- [x] Final static/cross-subsystem audit completed: 93/93 available checks passed before packaging.
- [ ] Unity compilation and runtime validation completed. Unity 6000.5.0f1 is unavailable in the implementation environment; validate the delivered overlay in the live project.

### V1.3A4 implementation audit

- The final source diff is restricted to the 15 approved files. No scene, prefab, preset asset, material, renderer asset, `.meta`, layer, tag, package, or project-setting file changed.
- The automatic-population implementation is behaviorally unchanged from the accepted A3 baseline except for `ActivePreset` → `DefaultPreset` terminology and forwarding the optional request preset override through the existing cloud-aware spawn helper. A1 footprint recovery and A2 stateless turnover logic are unchanged.
- The protected vegetation sidecar C# record/stride and the vegetation HLSL/shader consumers are byte-identical to A3. `_TEST.asset`, cloud runtime/cookie generation, LightRay common HLSL, and LightRay mask/scatter/composite shaders are byte-identical to A3.
- Runtime preset authority is now override-first/default-second per slot. Legacy serialized Anchor/Controller appearance fields remain physically present for deferred migration safety but are not used to construct an active ray descriptor.
- Vegetation accent intensity/coverage/softness are resolved into each ray descriptor and published independently per LightRay Spot. The sidecar override flag remains `1` even when the ray's vegetation accent intensity resolves to zero.
- The renderer no longer asks the Controller for a single renderable source family or Controller preset. Resolved source colour/intensity/direction arrive on each snapshot. Final Composite partitions compatible rays into deterministic reusable presentation groups; debug views intentionally retain the previous single combined diagnostic presentation.
- Group storage is array-backed and reused. No LINQ, dictionary grouping, recent-history cache, or unbounded per-frame collection was introduced. The common compatible-weather case remains one mask/soften/composite sequence.
- RenderGraph group sequencing explicitly reads the prior group's camera-colour result before publishing the next group's reused mask global, preventing later group mask/softening globals from being reordered ahead of an earlier composite.
- No material deviation from the approved architecture was required. The only implementation clarification is that active screen-space surface response is conservatively isolated into its own presentation group because its final composite uses per-ray screen bounds/centre state. Current curated LightRay presets have that response disabled, so this does not alter the normal weather-group count.
- Available static closure result before packaging: 93 checks passed, 0 failed. Unity C# import, shader compilation, and Play Mode behavior remain unverified until live-project validation.

## V1.3A3 — Vegetation sidecar closure plan

### Objective

Finish the already-completed migration from legacy LightRay vegetation globals/geometric diagnostics to the indexed per-additional-light sidecar without changing the protected sidecar layout, renderer ordering, ordinary vegetation lighting, cloud behavior, automatic population, surface Spot geometry, or serialized LightRay authoring.

V1.3A3 must also correct the remaining production intensity-authority defect: the indexed sidecar strength must be derived from the preset-resolved `AccentLineIntensity` value, including preset-transition interpolation, rather than directly from the legacy Controller fallback field.

### Approved V1.3A3 files

Modify:

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`
- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Protected and reviewed but byte-identical in this patch:

- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`

No scene, prefab, material, renderer asset, preset asset, `.meta`, layer, tag, package, project-setting, cloud-runtime, population-runtime, Anchor, or serialized-asset change is approved.

### Reviewed evidence

- The current Controller still declares and republishes six legacy Weather vegetation shader globals, an inactive published Spot position/direction state, diagnostic-mode state, and two unused legacy direction helpers. Production per-Light metadata is already supplied through the Light-to-sidecar dictionaries consumed by the renderer feature.
- The current vegetation include retains the protected two-`float4` sidecar plus legacy globals, diagnostic-only result fields, diagnostic-mode coverage bypass, diagnostic bookkeeping in both additional-light loops, and a false-colour diagnostic resolver.
- The benchmark vegetation shader has one remaining diagnostic-mode fragment return before the normal production lighting resolve.
- Repository-wide consumer search found no supported activation path for the legacy vegetation diagnostic mode and no production consumer of the legacy global Spot/direction/intensity/coverage bridge beyond the obsolete HLSL branches being removed.
- The renderer feature publishes one camera-local sidecar record per URP additional light and is the protected producer. Its record stride, ordering, zero fallback buffer, binding pass, Forward/Forward+ index alignment, and publication telemetry are not modified.
- The canonical vegetation architecture already defines the indexed sidecar as the active contract but retains historical sections that describe geometric matching as if current. Those sections must remain as history while current-state wording is corrected.

### Invariants

- `VegetationAdditionalLightAccentData` remains exactly two `float4` values in the same order and semantics.
- `parameters.w` remains the explicit Weather-LightRay identity/override flag independently of artistic intensity. Intensity `0` must therefore still publish an active override record when the Spot is a LightRay so ordinary punctual edge-accent fallback cannot reappear.
- URP `Light.direction`, attenuation, colour, cone, range, and rendering-layer filtering remain authoritative for ordinary vegetation body lighting.
- Only indexed `sourceDirectionWS` may replace the radial Spot direction for the Weather-specific stylized blade-edge side selector.
- Ordinary point/spot lights receive zero sidecar records and retain the generic punctual edge path.
- Coverage remains a stable whole-card threshold and never scales surviving radiance.
- Softness shapes only the selected Weather blade-edge profile.
- The current surface Spot Light, atmospheric renderer, cloud producer, automatic population, beam evolution, lifecycle, and storage behavior remain unchanged.

### Non-goals

- no sidecar layout or stride change;
- no renderer-feature modification;
- no new diagnostic suite or false-colour replacement;
- no vegetation material recalibration;
- no coverage, softness, attenuation, or edge-profile redesign;
- no cloud, automatic-population, source-orchestration, Anchor, preset-serialization, or `_TEST.asset` change;
- no mandatory-preset migration.

### File-by-file sequence

1. Update this canonical plan and lock the V1.3A3 scope before implementation.
2. Remove Controller legacy global-publication IDs/state/helpers/calls while preserving sidecar dictionaries, publication telemetry, surface Spots, and cached exponential scale.
3. Change the cached exponential input from the serialized fallback field to the preset-resolved `AccentLineIntensity` property.
4. Reduce the vegetation include to production body/edge result state plus the protected indexed sidecar; remove legacy globals, diagnostic bookkeeping, coverage diagnostic bypass, and false-colour resolver.
5. Remove only the obsolete diagnostic fragment-return branch from the benchmark shader.
6. Update current Weather and vegetation documentation while preserving clearly historical patch records.
7. Run cross-subsystem shader/include audit, exact diff/scope audit, dead-symbol search, sidecar-layout comparison, syntax/static checks, and protected-renderer byte comparison.
8. Record Unity compilation/runtime validation as pending with exact current Inspector routes verified from final source.

### Risks and safeguards

- **Sidecar layout regression:** compare the final HLSL record declaration with the protected C# record and require the renderer feature to remain byte-identical.
- **Zero-intensity semantic regression:** keep `parameters.w = 1` for LightRay Spots even when the resolved strength is zero; only `parameters.x` becomes zero.
- **Preset-transition regression:** resolve cache input through `AccentLineIntensity` so the cached exponential mapping follows current preset interpolation each controller tick when the resolved input changes.
- **Ordinary-light regression:** remove only Weather-diagnostic branches and retain the generic punctual-light edge path byte-for-byte where practical.
- **Shared-include regression:** repository-wide include search confirms the benchmark vegetation shader is the sole current shader consumer; final audit must repeat this search.
- **D3D12 missing-SRV regression:** the renderer feature's per-camera zero fallback binding and sidecar binding pass are protected and unchanged.

### Acceptance criteria

- all six legacy Weather vegetation shader-global names are absent from production C#/HLSL source;
- no legacy Spot position/direction publication, diagnostic-mode publication, diagnostic colour resolver, or diagnostic shader branch remains;
- no `ProductionVegetationAccentMatchingEnabled`, `SupportedVegetationAccentSpots`, or legacy vegetation direction helper remains without a live consumer;
- the sidecar remains two `float4` values with unchanged C#/HLSL semantics and renderer stride;
- the renderer feature is byte-identical to the V1.3A2 baseline;
- sidecar strength is calculated from preset-resolved `AccentLineIntensity`, not directly from the fallback field;
- intensity `0` keeps the indexed override flag active for LightRay Spots while producing zero Weather-specific edge strength;
- coverage and softness behavior are unchanged;
- ordinary punctual-light body and edge response are unchanged;
- Game and Scene View sidecar publication remains camera-local and the valid zero fallback remains bound for other cameras;
- no missing sidecar SRV warning appears in Unity;
- two or more simultaneous LightRay Spots retain vegetation accents in Unity;
- Unity 6000.5.0f1 compiles C# and vegetation shaders with zero errors.

### V1.3A3 status

- [x] Complete review surface and repository-wide consumer scan.
- [x] Record approved scope, invariants, risks, and acceptance gates in the canonical plan.
- [x] Controller legacy bridge removal and intensity-authority correction.
- [x] HLSL diagnostic/global cleanup.
- [x] Benchmark shader diagnostic-branch removal.
- [x] Current-state documentation reconciliation.
- [x] Cross-subsystem final audit and static validation (`62 / 62`).
- [ ] Unity compilation and runtime validation.

## V1.3A approved files (historical completed scope)

### Modify

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`
- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs`

### Delete

- `Assets/Game/Procedural/Weather/WeatherLightRayPresetCatalog.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionProfile.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationProfile.cs`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPresetCatalog.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRaySelection_DefaultCycle.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_Daylight.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_IndependentNight.asset`

For V1.3A itself, no scene, prefab, material, renderer asset, shader, HLSL include, layer, tag, package, or project-setting change was approved. V1.3A3 supersedes only that earlier shader/HLSL exclusion with the explicitly bounded shared-vegetation scope above.

## Reviewed evidence

The complete current source and relevant consumers/producers were reviewed before implementation:

- LightRay controller storage, source resolution, lifecycle, cloud response, population integration, reports, diagnostics, and vegetation publication;
- automatic population settings, camera-footprint resolution, candidate lifecycle, cloud sampling, spawn/update, retirement, reporting, and debug records;
- LightRay Controller Inspector authoring, telemetry, Scene diagnostics, smoke tests, and reports;
- authored Anchor and preset descriptor contracts;
- shared LightRay types and renderer source-kind consumers;
- Cloud Controller query, evolution-stability, debug, report, and benchmark contracts;
- Cloud Controller Inspector authoring, telemetry, preview, actions, and benchmark UI;
- renderer and vegetation sidecar boundaries;
- current Weather architecture, LightRay architecture, cloud handoff, and prior Inspector plan;
- catalog, selection profile/runtime, and selection-only population profile code and assets;
- repository-wide consumers of selection-only shared types.

## Settled ownership

### Core LightRay system

The core LightRay system is generic. It owns rendering, storage, lifecycle, appearance application, beam evolution, source-policy evaluation, cloud-policy evaluation, authored registration, and procedural request execution.

It does not own daylight, the Sun, Weather eligibility, quest eligibility, storytelling eligibility, or automatic preset selection.

### Preset

A preset owns appearance and shared presentation only. It must not determine source ownership, automatic eligibility, or Weather dependencies.

Preset `SourceKind` is not runtime authority. A4 removed the C# preset source-kind member; inert legacy YAML keys may remain in existing preset assets until Unity naturally reserializes them. A5 does not raw-edit working preset assets solely to remove ignored keys.

### Runtime request

A ray request may carry a source kind, explicit direction, source-gate policy, cloud policy, movement policy, lifecycle, and placement. Those values describe that request; they do not redefine global LightRay ownership.

### Weather orchestration

A future Weather orchestration layer will decide which atmospheric population is active, which preset is eligible, which environmental dependencies matter, and which directional source is supplied at runtime. This cleanup does not implement that orchestrator and must not hardcode its future decisions into the core system.

### Current automatic population

The current automatic producer is a cloud-opening atmospheric population. It may continue receiving the currently resolved daylight directional source for cloud projection and visual direction, but its contracts, labels, preset checks, stable identity, and documentation must not define all automatic LightRays as Sun-owned.

### Cloud producer

The cloud-shadow subsystem remains the frozen producer of the globally tiled directional cookie and present/future CPU transmission queries. Cookie generation, projection, movement, receiver shading, benchmark implementation, and debug overlay behavior are non-goals.

## Required behavior changes

### Cloud transition policy

During cloud-pattern evolution below `Cloud Transition Spawn Resume`:

- existing automatic rays continue their assigned lifetimes and normal fades;
- authored and caller-created rays continue normally;
- beam evolution continues;
- automatic active-ray lifecycle, camera-region retirement, budget retirement, cooldown, and reporting continue;
- pending and new automatic candidate evaluation pauses;
- no full automatic-population retirement occurs merely because the cloud pattern is unstable;
- cloud transmission marked `EvolutionUnstable` remains usable for existing `RespectClouds` rays.

Runtime state must distinguish `Disabled`, `Suspended`, `SpawningPaused`, and `Running`.

### Automatic population region

- obtain one representative Ground Mask hit to establish a horizontal reference-plane height;
- project the viewport centre and all eight perimeter rays mathematically onto that plane;
- do not require every viewport sample to hit a finite Ground collider;
- expand the resulting polygon by Camera Margin;
- when a focus override is assigned, translate the resolved footprint in XZ so its centre follows the override;
- preserve the ground-reference height for candidate raycast origins;
- raycast each actual candidate downward against the configured Ground Mask before spawning;
- never restore a circular fallback;
- suspend only when no usable ground reference or valid camera-plane footprint can be resolved.

### Derived controls

Delete serialized authoring for:

- Fallback Active Radius;
- Qualification Duration;
- Candidate Checks Per Tick;
- Ground Search Distance.

Derive:

```text
candidate checks per update = clamp(Ray Budget × 2, 4, 64)
ground raycast distance = max(100 m, resolved camera far clipping distance)
```

The first valid six-sample evaluation may spawn a new candidate during the same population update. No hidden second evaluation remains.

### Inspector contract

The LightRay Controller Inspector roots are:

1. Core Setup
2. Source & Rendering
3. Automatic Population
4. Advanced System
5. Diagnostics
6. Runtime Status
7. Report

Only Runtime Status contains live or resolved telemetry. Diagnostics contains editable debug toggles only. Report contains one copy action.

The Cloud Controller Inspector retains its production controls, preview, actions, receiver audit entry point, and benchmark, but all ordinary live telemetry moves to its sole Runtime Status root.

## Deleted infrastructure

Delete:

- unused preset catalog code and asset;
- disconnected selection profile/runtime;
- selector-only population profiles and assets;
- Controller selection-dependency resolver;
- selection-only shared enums and structures with no remaining consumer;
- LightRay-owned CPU cloud projection probe;
- Procedural Test Pair Editor scaffolding;
- Cloud-Aware Test Ray Editor scaffolding;
- separate Beam Evolution Runtime Audit;
- Controller vegetation diagnostic suite state and Inspector actions only where removal does not touch shared shader/HLSL code.

Historical V1.3A stopped at the Controller/Inspector boundary and deliberately left the legacy vegetation shader globals and false-colour branches for a separately approved shared-shader cleanup. V1.3A3 is that bounded cleanup and preserves the indexed sidecar layout exactly.

## Preserved behavior and invariants

- `_TEST.asset` remains unchanged and present.
- All curated LightRay preset assets remain.
- manual Active Preset selection remains authoritative for now.
- runtime preset switching API remains.
- authored and procedural multi-ray rendering remains.
- beam evolution remains active throughout each slot lifecycle.
- current surface Spot Light behavior remains.
- indexed vegetation accent sidecar publication remains byte-compatible.
- cloud cookie generation, query, movement, debug overlay, receiver audit, and benchmark remain.
- automatic population remains disabled by default and still requires an explicit Ground Mask.
- no automatic time-of-day, Sun, Moon, quest, or gameplay orchestration is introduced.
- no mandatory-preset Anchor migration or serialized appearance-field deletion occurs in this patch.

## File-by-file sequence

1. Rewrite this canonical plan and lock scope.
2. Delete disconnected catalog/selection code and assets.
3. Remove selection-only shared contracts and Controller consumers.
4. Refactor automatic population state, transition policy, footprint resolution, derived work budgets, and immediate candidate spawning.
5. Remove obsolete Controller non-shader diagnostic state and update reporting.
6. Rebuild the LightRay Controller Inspector.
7. Consolidate Cloud Inspector telemetry without changing cloud runtime behavior.
8. Rewrite the LightRay and provisional Weather architecture documents.
9. Run repository-wide dead-reference, source-ownership, serialized-field, and scope audits.
10. Run available static syntax checks and prepare exact Unity validation.

## Risks and safeguards

### Serialized migration

At the historical V1.3A boundary, legacy Controller and Anchor appearance fields and preset `SourceKind` remained. V1.3A4 removed runtime preset source authority and stopped consuming the legacy Anchor appearance path. V1.3A5 closes that debt by deleting the obsolete source-level Anchor/Controller fallback and migration fields without raw-editing scenes, prefabs, or preset assets.

### Source coupling

The current controller still resolves a daylight directional source through legacy serialized fields. The automatic producer may consume that runtime value, but no preset eligibility check, automatic-population label, identity hash, or system-level statement may treat it as inherent ownership.

### Transition regression

Spawn pausing must not bypass normal lifecycle and retirement work. The runtime status enum and report must expose the distinction.

### Footprint regression

A focus override translates the complete camera footprint. It must not create a circle or use the override Transform height as terrain height.

### Shared shader boundary

No shader or HLSL modification is allowed. Diagnostic CPU publication that is still referenced by the unchanged shader path may remain temporarily, but no obsolete Inspector action or report may expose it as a supported production diagnostic.

## Acceptance criteria

- no catalog, selector, selector profile, or selector-only population profile reference remains;
- `_TEST.asset` is present and unmodified;
- production automatic population does not read preset `SourceKind`;
- automatic-population labels and reports contain no Sun-owned claim;
- cloud evolution below threshold produces `SpawningPaused`, not full suspension;
- existing automatic rays are not retired by the transition pause;
- existing cloud-response rays do not receive zero openness solely because data is evolution-unstable;
- the focus override translates a complete camera footprint;
- no circular population fallback remains;
- candidate and ground-raycast budgets are derived exactly as specified;
- a new valid candidate can spawn on its first six-sample evaluation;
- the LightRay Inspector has exactly the seven approved roots and only one telemetry root;
- the Cloud Inspector has one telemetry root;
- obsolete LightRay projection/test/audit actions are absent;
- the comprehensive report remains available;
- the frozen cloud producer, renderer path, surface Spots, and vegetation sidecar are unchanged;
- all modified C# files pass available lexical, brace, and dead-reference checks;
- Unity compilation and runtime validation are explicitly reported as pending until run in Unity 6000.5.0f1.

## Patch status

- [x] Review surface completed.
- [x] Scope and ownership decisions recorded.
- [x] Catalog/selection deletion completed.
- [x] Population runtime refactor completed.
- [x] Controller cleanup completed.
- [x] LightRay Inspector cleanup completed.
- [x] Cloud Inspector telemetry cleanup completed.
- [x] Canonical architecture documents updated.
- [x] Final diff and consistency audit completed.
- [x] Available static validation completed.
- [ ] Unity compilation and runtime validation completed.

## Static closure evidence

The Archive 48 source was audited after implementation with the following results:

- all six modified C# files passed lexical string/comment and delimiter-balance checks;
- all serialized property names drawn by the rebuilt Inspectors resolve to current Controller fields;
- all Controller properties and methods used by both custom Inspectors resolve to current source members;
- all Controller calls into `WeatherLightRayPopulationRuntime` resolve to current runtime members;
- no live source reference remains to the deleted catalog, selector, selector-only population profiles, or selection-only shared types;
- no source reference remains to the removed LightRay projection probe, test-pair, cloud-aware test-ray, separate evolution audit, or vegetation diagnostic Inspector suite;
- no production automatic-population read of preset `SourceKind` remains;
- no circular population fallback or obsolete serialized population control remains;
- the cloud Controller, cloud generator, LightRay renderer feature, LightRay Anchor, shared vegetation HLSL, vegetation shader, and Anchor Inspector are byte-identical to Archive 48;
- `_TEST.asset` is byte-identical to Archive 48 with SHA-256 `e25c225dd700dc3db10cb32fab5ce8b3d97d5c93da70e3a9a892a867b1c6842d`;
- the eight approved deleted files are absent from the implementation tree.

Unity Editor compilation and runtime behavior cannot be executed in this environment and remain mandatory acceptance gates.

## V1.3A2 turnover-randomization closure

The automatic population follow-up is intentionally stateless:

- each completed cell sweep resolves a new deterministic permutation from Population Seed and turnover epoch;
- stable cell identity remains responsible for occupancy, cooldown, and duplicate prevention;
- a separate activation identity varies exact within-cell position, lifetime, beam seed, and external opening identity;
- active rays remain static;
- no recent-cell history, queue, cache, or additional Inspector control is introduced.

Acceptance requires varied replacement locations during a mostly clear-sky multi-minute test, while the same Population Seed remains reproducible for the same runtime sequence.
