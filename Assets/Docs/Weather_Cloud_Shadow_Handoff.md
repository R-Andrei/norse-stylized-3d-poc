# Weather Cloud-Shadow Architecture and Continuation Handoff

## A. Handoff identity

**Stable architecture identifier:** `WEATHER-CLOUD-SHADOW-V0`

**Documentation update identifier:** `WEATHER-CLOUD-SHADOW-V0.4-FREEZE`

**Architecture locked:** 2026-07-23. Universal gameplay-world coverage remains mandatory, and the user selected the URP main directional-light cookie as the first complete implementation. The hybrid optimized receiver system is deferred unless measured cookie cost is unacceptable.

**Current implementation status:** `WEATHER-CLOUD-SHADOW-V0.4` is accepted and frozen as of 2026-07-23. The receiver audit, directional-cookie runtime, Ground/Generated Mass/Vegetation/River compatibility, debug visualization, debug-focus cleanup, low-frequency seed evolution, automated benchmark suite, coroutine correction, and execution-order evidence correction all compiled and were runtime-validated by the user. The final 2560 × 1440 Direct3D12 Editor Play Mode stress-view report completed with restoration `PASS`, explicit alternating pair order, mean paired GPU median deltas of `+0.016 ms` for the static cookie and `+0.011 ms` for the moving cookie, no SetPass regression, and no post-evolution residual cost. The native URP directional-cookie architecture is retained. Standalone Player and low-end-PC confirmation are deferred to the future project-wide testing sprint and are not blockers to the V0 freeze.

**Canonical source:** this document is the authoritative plan and continuation handoff for the first cloud-shadow implementation. `Assets/Docs/Weather_System_Architecture_Provisional.md` remains the parent Weather architecture. Receiver-specific canonical documents record only their local integration invariants and defer field ownership to this document.

**Historical repository state:** the preceding Ground-only handoff recorded branch `fufu` at `9d69a18c7b9ba1e42e58577a8832aeff94723cc8`. The supplied `Assets-Code-Archive(16).zip` contains no `.git` metadata, so that branch, revision, current working-tree state, and later drift are not independently verified by this documentation update. The next implementation thread must inspect the live workspace before any edit.

### Terminology

- **Cloud shadow** means a moving stylized reduction of environmental illumination across gameplay-world receivers. It does not require visible cloud geometry.
- **Transmission** means retained direct sunlight at a world position. `1` is open sun; a lower positive value is cloud shade.
- **Receiver** means a gameplay-world renderer whose visible surface responds to environmental sunlight.
- **Universal receiver coverage** means every relevant visible world receiver uses the same authoritative cloud field or is explicitly exempted for a documented visual reason.
- **Primary V0 path** means one URP main-directional-light cookie assigned to the authoritative sun and consumed through each compatible receiver's normal main-light cookie path.
- **Hybrid fallback** means a later performance optimization that may replace cookie sampling on selected high-cost custom receivers while preserving the same field and visual alignment. It is not part of V0 and requires measured justification and separate approval.
- **Double application** means a receiver applies both the directional cookie and any custom cloud attenuation. Double application is prohibited.
- **Debug focus** means the Transform used only to position the finite cloud diagnostic overlay. It does not define, crop, or move the production cloud field, because the directional cookie tiles globally.

---

## B. Immediate continuation brief

Universal receiver coverage remains mandatory: Ground, Bank, Riverbed, Generated Mass, Vegetation, River, actors, buildings, future houses, roads, walls, snow, ice, and other sun-responsive world materials must share one coherent moving cloud boundary.

The **selected first complete implementation is a URP main directional-light cookie on the authoritative sun**. This decision is now locked for `WEATHER-CLOUD-SHADOW-V0` because it provides the highest immediate visual consistency, naturally attenuates the main directional light, handles coarse and low-poly geometry without interpolation faceting, and gives standard URP Lit and compatible Shader Graph materials a durable future-content path.

The performance acceptance gate is complete for the current V0 stress view. The final corrected 2560 × 1440 suite measured `+0.016 ms` mean paired GPU median delta for a static cookie and `+0.011 ms` for the normal moving cookie, with unchanged SetPass count and restoration `PASS`. These deltas are below ordinary run-to-run variation and do not justify a hybrid receiver system. The hybrid shared-mask design remains a deferred contingency only if later Player-build or low-end-hardware evidence demonstrates a material regression.

The field remains Weather-owned and moves with authoritative Weather wind direction or a deterministic bounded angular offset. No visible cloud mesh, cloud plane, volumetric system, projector, decal, fullscreen pass, per-object cloud state, or custom vertex cloud field is part of the frozen cloud-shadow V0. The adjacent Weather LightRay architecture is defined in `Assets/Docs/Weather_Light_Ray_Architecture.md`: it may query the same cloud transmission and evolution state, but it remains a separate hybrid presentation and gameplay-zone subsystem and does not modify the frozen receiver-cookie path. Its current AF4 renderer direction is a dense-overlap bundle of separate continuous parallel beam ribbons plus one pooled shadowless URP Spot Light per active zone for receiver-material lighting, with the prior depth-aware screen-space circular lift retained only as an optional zero-default complement. The rejected sampled-volume renderer is not a cloud-system dependency.

The accepted visual starting points remain:

- broad clouded regions with broad sun openings;
- intentionally authored opening features normally no smaller than approximately `5–7 m`;
- initial transition softness around `1.5 m`;
- continuous movement with Weather wind;
- no cloud darkening at night or when the sun has no useful contribution.

---

## C. User intent, scope, and acceptance criteria

### Primary objective

Create a coherent moving cloud-illumination layer for the top-down isometric game. The effect is judged across the complete visible scene, not by one isolated material.

### Mandatory receiver scope

The following receiver classes are in scope whenever they are present in the active gameplay view:

- Generated Ground, Bank, Riverbed, roads, paths, terrain-like surfaces, and ground modifiers;
- Generated Mass rocks and other Pixel Surface props;
- dense Vegetation, future trees, shrubs, and other sun-lit foliage;
- River water, ice, foam, shore response, and other sun-lit water presentation;
- player characters, enemies, NPCs, creatures, equipment, and animated props;
- buildings, houses, walls, roofs, bridges, fences, and constructed props;
- snow, mud, stone, wood, thatch, decals, and future reusable world materials;
- lit particles or VFX that are intended to sit physically inside world illumination.

### Explicit exemptions

The following do not require cloud attenuation unless a later feature explicitly changes their visual role:

- UI and screen-space overlays;
- skybox and any future visible-cloud rendering itself;
- unlit debug views, diagnostic overlays, editor handles, and gizmos;
- intentionally emissive or additive effects whose brightness is self-generated;
- Weather wind trails while they remain an intentionally unlit atmospheric indicator;
- shadow-caster, depth-only, motion-vector, and selection passes that do not output visible lighting.

An exemption must be explicit. “Not yet integrated” is not an exemption.

### Universal acceptance criteria

1. One authoritative Weather cloud field, movement phase, and transmission convention drives every receiver.
2. Adjacent Ground, grass, rocks, River, actors, and buildings show the same cloud boundary at the same world position.
3. New gameplay-world shaders and Shader Graphs must adopt the cloud receiver contract before they are considered production-compatible.
4. An editor-triggered receiver audit must identify active renderers whose shaders do not declare cloud support and must provide a clipboard-copyable report.
5. No active sun-responsive renderer may be silently omitted from the final validation scene.
6. The effect must attenuate environmental sun response. Ambient, local lights, emission, fog, and material identity must not be indiscriminately multiplied unless a receiver-specific approximation is explicitly approved and documented.
7. Open sunlight must match the no-cloud baseline within ordinary numerical tolerance.
8. Cloud shade must remain bounded and readable; it must not black out materials or remove their local form.
9. Cloud motion must remain world-space coherent across separate meshes, chunks, instances, and object transforms.
10. The effect must disable at night, below the sun horizon, or when the active sun is absent or negligible.
11. No cloud implementation may modify generation topology, collision, hydrology, River simulation, foam lifecycle, Vegetation interaction fields, actor gameplay logic, or material ownership.
12. No per-frame CPU traversal of every renderer, material, grass instance, vertex, or River cell is permitted.
13. No recurring managed allocation is permitted after warm-up.
14. Every supported receiver must use the authoritative main-light cookie path exactly once; no custom vertex/fragment cloud attenuation is permitted in V0.
15. Unity compilation, gameplay-camera visual review, receiver-compliance audit, day/night validation, wind validation, and matched CPU/GPU profiling must pass before V0 is accepted.
16. Before cloud-pattern placement, coverage, or spawning behavior is tuned, a dedicated debug visualization must show the exact active cookie projection independently of receiver lighting strength.
17. Directional-cookie world coverage must remain independent of player position, camera position, map-chunk count, and total map dimensions. The cookie repeat period controls pattern repetition, not a finite coverage boundary.

### Visual field requirements

- Openings are broad stylized regions, not small speckled noise.
- The analytic spectrum or authored cookie must avoid intentionally producing features below the `5–7 m` target. This is a frequency/authoring constraint, not a mathematical guarantee that every connected threshold island has a minimum diameter.
- Initial edge softness is approximately `1.5 m`, subject to gameplay-camera validation.
- Coverage, shade transmission, scale, warp, speed, and deterministic seed remain authorable Weather controls.
- The field must tile or extend without visible seams across the active gameplay region.

### Mandatory debug visualization

`WEATHER-CLOUD-SHADOW-V0.3A` introduces one Weather-owned diagnostic overlay with three modes:

- `Off`;
- `CloudAreas`, which tints clouded regions while leaving openings transparent;
- `CloudAndOpenings`, which displays the complete cloud/opening map using two distinct colours.

The overlay is an unlit, nonpersistent, programmatically submitted horizontal quad. Its shader samples the exact active URP main-light cookie at world positions, so it displays the same projected field receivers are expected to consume. It renders above ordinary scene depth for diagnosis, casts no shadow, receives no shadow, uses no probes, creates no hierarchy object, and does not modify cloud generation or lighting. The controller also exposes the generated cookie as an Inspector preview.

The debug overlay is explicitly diagnostic and exempt from cloud attenuation. Its presence must not be interpreted as visible cloud geometry or a production renderer. The overlay resolves a generic debug focus in this order: runtime override, Inspector override, assigned fallback camera, `Camera.main`, then controller transform. By default it follows that focus and may match one full cookie repeat period. This focus affects only where the finite overlay is drawn; production cloud shading continues to tile globally and works at remote cutscene locations without moving the player.

---

## D. Governing instructions and source hierarchy

1. `AGENTS.md` and `Assets/AGENTS.md` govern review, planning, approved scope, implementation, validation, and evidence.
2. This document governs cloud-field ownership, receiver coverage, the selected cookie architecture, fallback boundaries, and implementation sequencing.
3. `Assets/Docs/Weather_System_Architecture_Provisional.md` governs parent Weather ownership.
4. `Assets/Docs/Weather_Wind_Architecture.md` governs current authoritative wind and its CPU/GPU contracts.
5. Receiver documents govern local material, rendering, simulation, and performance invariants:
   - `Assets/Docs/Ground_Visual_Design_and_Architecture.md`;
   - `Assets/Docs/Generated_Mass_Framework.md`;
   - `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`;
   - `Assets/Docs/River_Rendering_Roadmap.md`.
6. Installed Unity 6000.5 / URP 17.5 source in the live workspace governs exact cookie, main-light, Shader Graph, and lighting-helper behavior.
7. The live supplied workspace overrides archive snapshots and this handoff when later code drift is proven. Drift must be recorded before implementation.

V0.2 source implementation began only after the user-supplied live receiver report was reconciled with the supplied source snapshot and this plan was updated. The live project must still reconcile patch conflicts, Git state, and unrelated working-tree drift before applying the files.

---

## E. Source provenance and update scope

### Supplied sources reviewed

- `/mnt/data/Weather_Cloud_Shadow_Handoff.md` — preceding Ground-only canonical plan.
- `/mnt/data/Assets-Code-Archive(16).zip` — supplied project source snapshot.
- `Assets/AGENTS.md` — mandatory repository workflow.
- Current Weather, Ground, Generated Mass, Vegetation, River documents and the authored shaders/includes named in sections G and M.

The documentation-only V0.1/V0.2 decision patches edited no source. The current V0.2 implementation edits only the files declared in section M. No Git retrieval, clone, reset, restore, raw scene edit, package edit, material edit, pipeline-asset edit, or persistent generated-asset operation was performed.

### `WEATHER-CLOUD-SHADOW-DOC-V0.1` expected and actual files

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
  Assets/Docs/Ground_Visual_Design_and_Architecture.md
  Assets/Docs/Generated_Mass_Framework.md
  Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
  Assets/Docs/River_Rendering_Roadmap.md
```

Actual `WEATHER-CLOUD-SHADOW-DOC-V0.1` scope matched the declaration.

### `WEATHER-CLOUD-SHADOW-DOC-V0.2` expected and actual files

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
  Assets/Docs/Ground_Visual_Design_and_Architecture.md
  Assets/Docs/Generated_Mass_Framework.md
  Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
  Assets/Docs/River_Rendering_Roadmap.md
```

Actual V0.2 documentation-lock scope matches the declaration. Metadata files are not supplied in `Assets-Code-Archive(16).zip`; the live project must preserve its existing Visible Meta Files and must not regenerate or replace an existing handoff GUID unnecessarily.

---

## F. System architecture

### Locked V0 producer flow

```text
WeatherWindDomain
  -> publishes authoritative world-space XZ wind
  -> exposes SampleWindXZ / TrySampleWindXZ

TimeOfDayController / RenderSettings.sun
  -> owns and publishes the authoritative directional sun

WeatherCloudShadowController + WeatherCloudShadowCookieGenerator [source implemented]
  -> resolves one stable cloud travel direction from Weather wind
  -> owns cookie seed, coverage, scale, softness, minimum-opening cleanup, transmission, speed, and phase
  -> dirty-generates one low-resolution seamless linear R8 cookie
  -> animates the directional-cookie placement without changing sun direction/intensity ownership
  -> assigns and clears the cookie on the authoritative sun
  -> provides diagnostics and lifecycle restoration

URP main-light cookie path
  -> projects one coherent field through the sun
  -> attenuates direct main-light contribution on every compatible receiver
  -> preserves ambient, local lights, emission, fog, material identity, and subsystem simulation
```

### Receiver contract

Every production-visible world shader must be one of:

```text
Cookie Supported
  receives the authoritative main directional-light cookie through URP or an equivalent custom main-light call;

Explicitly Exempt
  is UI, sky, diagnostic, intentionally unlit, or intentionally self-emissive;

Unsupported
  is sun-responsive gameplay-world rendering that does not receive the cookie.
```

`Unsupported` blocks V0 acceptance. An editor-triggered receiver audit must scan active scene renderers only on request, list renderer/material/shader ownership, classify cookie support, and copy the complete report. It must not scan every frame.

### Cookie representation and motion

- Use one broad, low-resolution, nonpersistent linear `TextureFormat.R8` cookie with bilinear filtering, repeat wrap, no mipmaps, and `HideFlags.HideAndDontSave`.
- Pattern generation occurs only when authored settings become dirty or `Rebuild Cloud Cookie` is invoked. It does not regenerate during steady movement.
- The generator uses periodic value-noise lattices and a dirty-time periodic connected-component cleanup that removes isolated opening regions below the authored approximate minimum-opening diameter. This cleanup is a visual constraint, not a mathematical guarantee that every irregular retained opening has the same diameter.
- Steady-state movement changes only `UniversalAdditionalLightData.lightCookieOffset` plus small controller state. The offset is derived from the integrated world-XZ Weather-wind phase transformed into the current sun cookie plane and wrapped by one seamless cookie period.
- The authoritative sun remains owned by `TimeOfDayController`; the cloud controller creates no second sun and does not change sun colour, intensity, rotation, shadows, or time-of-day behavior.
- The controller captures and restores the previous `Light.cookie`, `lightCookieSize`, and `lightCookieOffset` on disable, destruction, publisher handoff, missing useful sun, or resolved-sun change.
- Cookie scale, repetition, and movement remain world-coherent and do not swim independently per receiver.

### Shader compatibility rule

Standard URP Lit and compatible Lit Shader Graph receivers should use URP's existing cookie path. Handwritten Ground, Pixel Surface, Vegetation, River, actor, building, particle, or transparent shaders must compile the relevant main-light cookie variant and call a cookie-aware main-light path using world position. The exact edit may be as small as variant/overload support, but every active shader must be audited in the live project.

No custom Weather cloud field, cloud varying, procedural fragment noise, custom final-colour multiply, or duplicated direct-light reconstruction is part of V0.

### Performance contract

Expected steady-state costs are:

- one main-light cookie sample per affected lit fragment;
- no new draw call, fullscreen pass, compute dispatch, mesh update, per-renderer update, or recurring managed allocation;
- bounded `O(1)` CPU controller work per frame plus bounded Weather wind sampling;
- dirty-triggered cookie generation/import work only.

The invocation count is not treated as proof of failure. V0 is accepted or rejected by matched profiling on target-class hardware. Required evidence includes GPU frame time, opaque/transparent and Vegetation cost where tooling permits, CPU main/render thread, GC allocation, draw/dispatch count, shader variants, and receiver coverage.

### Hybrid fallback boundary

The hybrid shared-mask architecture is deferred. It may be reopened only if the complete cookie implementation causes a material measured regression or a proven receiver incompatibility that cannot be corrected reasonably. Any fallback must preserve one field and must explicitly prevent double application. It requires a plan update, exact file declaration, benchmark comparison, and user approval.

---

## G. Reviewed file and receiver inventory

### Existing authoritative producers

| Path | Relevant symbols | Current role | V0.2 cloud action |
|---|---|---|---|
| `Assets/Game/Procedural/Weather/WeatherWindDomain.cs` | `SampleWindXZ`, `TrySampleWindXZ`, `FieldAnchor` | Authoritative wind | Read-only dependency unless a proven missing contract exists |
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayController.cs` | `ApplySun`, `RenderSettings.sun` | Sun ownership | Read-only dependency |
| `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs` | Weather-owned unlit trails | Exempt receiver | No cloud shading while intentionally unlit |

### Current authored visible shader families

| Feature | Shader/include files | V0.2 receiver status |
|---|---|---|
| Generated Ground | `SH_PixelGroundSurfaceLit.shader`, `PixelSurfaceGroundForwardTypes.hlsl`, `PixelSurfaceGroundForwardPass.hlsl` | Source integrated by ForwardLit cookie pragma; includes unchanged |
| Generated Mass / Pixel Surface | `SH_PixelSurfaceLit.shader`, `PixelSurfaceForwardTypes.hlsl`, `PixelSurfaceForwardPass.hlsl` | Source integrated by ForwardLit cookie pragma; includes/generation unchanged |
| Vegetation | `SH_StylizedVegetationBenchmark.shader`, `VegetationLighting.hlsl` | Source integrated by cookie pragma and world-position-aware main light; highest fragment-overdraw performance risk |
| River | `SH_CleanStylizedRiver.shader`, `RiverWaterLighting.hlsl` | Source integrated by ForwardLit cookie pragma; lighting include unchanged |
| Shader Graph Pixel Surface | `SG_PixelSurfaceLit.shadergraph`, `SGS_PixelSurfaceCore.shadersubgraph` | Live audit reports native cookie support; source unchanged; specific asset-by-asset visual spot checks remain part of future regression testing |
| Weather wind trails | `SH_WeatherWindTrails.shader` | Explicitly exempt while unlit |

### Live receiver families proven by the V0.1 audit

The user-supplied report inventories standard URP Lit actors and hearths, Lit Shader Graph wood/thatch/stone buildings and props, custom Ground, custom River, and custom Generated Mass. Standard URP Lit and Lit Shader Graph sources are not edited. Future shaders remain subject to the same compliance audit; absence from the current scene is not an exemption.

---

## H. Chronological decision history

1. The user requested broad moving cloud shade for an isometric top-down game and explicitly did not require visible clouds.
2. A URP directional-light cookie was first proposed because it naturally projects through the sun and reaches compatible lit receivers.
3. Per-fragment sample count motivated investigation of a cheaper Ground-only approach.
4. The active demo Ground was reported as `40 m`, `33 × 33`, `1,089` vertices with `1.25 m` spacing. Broad `5–7 m` openings and approximately `1.5 m` softness made vertex interpolation plausible.
5. The user approved a vertex-evaluated Ground prototype first and retained the cookie as fallback.
6. The preceding handoff therefore limited V0 to Ground and explicitly excluded River, Vegetation, rocks, actors, and other materials.
7. The user then rejected that receiver scope as visually incomplete: cloud shade must sit over the complete scene, including grass, River, future houses, and other world systems.
8. `WEATHER-CLOUD-SHADOW-DOC-V0.1` superseded the Ground-only receiver scope and reopened architecture selection.
9. After comparing cookie, vertex, hybrid, shadow-plane, and fullscreen approaches, the user selected the URP directional-light cookie everywhere as the first complete implementation because visual quality and universal consistency are immediately strongest.
10. Performance remains mandatory but will be judged from the finished cookie implementation. The hybrid shared-mask architecture is retained only as a measured fallback.

---

## I. Completed work

### `WEATHER-CLOUD-SHADOW-DOC-V0.1`

The documentation patch changed cloud shadow from a Ground-only effect to a universal Weather lighting contract, defined mandatory/exempt receiver classes, required future-shader compatibility auditing, and added local invariants to Ground, Generated Mass, Vegetation, and River documents.

### `WEATHER-CLOUD-SHADOW-DOC-V0.2`

The decision-lock patch selected the URP main directional-light cookie for V0 and deferred hybrid optimization until matched profiling proves it necessary.

### `WEATHER-CLOUD-SHADOW-V0.1`

The editor-triggered receiver audit compiled after Unity 6.5 API corrections and produced the user-supplied live report. It verified both URP assets have cookies enabled, identified the authoritative directional sun, inventoried 64 active renderer/material records, and exposed the real custom-receiver gaps.

### `WEATHER-CLOUD-SHADOW-V0.2` — source implementation and Unity compatibility

The source patch now:

- creates `WeatherCloudShadowController`, which resolves and preserves the authoritative sun, consumes bounded-cadence Weather wind direction, integrates a world-space movement phase, assigns one generated cookie, updates only the URP cookie offset during steady movement, gates the effect by useful sun contribution, and restores the prior sun state on disable or ownership change;
- creates `WeatherCloudShadowCookieGenerator`, which builds a deterministic seamless linear `R8` cookie only when pattern settings change or an explicit rebuild is requested;
- creates a custom Inspector with rebuild, motion reset, copyable comprehensive report, active-publisher diagnostics, and a 30 Hz bounded edit-preview tick while selected;
- adds the native `_LIGHT_COOKIES` fragment variant to Ground, Generated Mass Pixel Surface, Vegetation, and River ForwardLit passes;
- changes only Vegetation's main-light lookup to the world-position-aware cookie-compatible overload while retaining its no-geometric-main-shadow policy;
- corrects the receiver audit so package URP Lit is not falsely rejected and custom authored shaders must prove a real pragma declaration rather than an inherited fallback keyword-space entry;
- leaves materials, Shader Graph assets, pipeline assets, Weather wind generation, Time of Day, Ground generation, Generated Mass generation, Vegetation simulation, and River simulation unchanged.

The controller was subsequently attached through Unity to `Systems/Weather`. The user supplied a post-fix audit showing controller gate `PASS`, receiver gate `PASS`, 64/64 loaded-scene receiver records supported, all mandatory authored receiver shaders supported, and both discovered URP assets cookie-enabled. The user then validated the projected debug field, ordinary receiver response, global tiled coverage, debug-focus behavior, low-frequency seed evolution, and the complete V0.3E2 benchmark suite. Visual and performance acceptance are complete for the frozen V0 baseline.

## J. Rejected and prohibited shortcuts

### Ground-only implementation

Rejected by expanded user requirement. It creates visible lighting discontinuities between Ground and adjacent grass, rocks, water, actors, or buildings.

### Untracked per-system cloud controls

Rejected. Each subsystem must not invent its own seed, phase, direction, speed, threshold, or coverage. That would produce drifting boundaries and prevent coherent Weather transitions.

### Final-colour multiplication everywhere

Rejected as the default. It darkens ambient, local lights, emission, and receiver-specific highlights indiscriminately. A receiver-specific temporary approximation requires explicit plan status and visual evidence.

### Per-frame CPU renderer/material updates

Rejected. Cloud parameters are global. Per-renderer property blocks, material clones, and renderer traversal are unnecessary for the steady-state effect.

### Cookie plus custom cloud attenuation in V0

Rejected. V0 uses the directional cookie exactly once on each supported receiver. Custom vertex or fragment cloud attenuation is not implemented alongside it. The hybrid path may be introduced only later as an exclusive measured optimization.

### Automatic acceptance of standard or future materials

Rejected. Future content compatibility must be detected by the receiver audit or guaranteed by the selected universal path.

### Visible cloud geometry as a coverage solution

Rejected for V0. A cloud plane or shadow-caster geometry introduces renderer, shadow-map, culling, altitude, cascade, and scene-ownership costs without solving all receiver contracts more cleanly than the selected candidates.

---

## K. Current verified and unverified state

### Verified from the supplied source patch

- `WeatherCloudShadowController.cs` and `WeatherCloudShadowCookieGenerator.cs` implement the approved Weather-owned cookie lifecycle without a per-frame field rebuild.
- The controller uses `WeatherWindDomain.TrySampleWindXZ`, preserves the previous sun cookie/size/offset, and publishes through one active controller.
- The generated cookie is linear `TextureFormat.R8`, bilinear, seamless, nonpersistent, and dirty-generated.
- Ground, Generated Mass Pixel Surface, Vegetation, and River ForwardLit passes now contain an authored `_LIGHT_COOKIES` fragment pragma.
- Ground and Generated Mass continue through `UniversalFragmentPBR`.
- River continues through its existing three-argument `GetMainLight` call.
- Vegetation now calls the three-argument `GetMainLight` overload with `inputData.positionWS` and a neutral shadow mask.
- The audit now checks custom authored pragma declarations directly, avoiding the Pixel Surface fallback-keyword false positive, and treats package-owned URP Lit as authoritative.
- Static delimiter, obsolete-API, exact-scope, metadata, encoding, and source-diff checks are recorded in section N.

### Verified by the user-supplied live audit before V0.2

- PC and Mobile URP assets have Light Cookies enabled.
- `RenderSettings.sun` resolves to the active directional `Lighting/CelestialRig/SunLight`.
- The loaded demo scene contains 64 renderer/material records using five unique shaders.
- Standard URP Lit and the Lit Pixel Surface Shader Graph are intended native cookie receivers; package URP Lit must not be edited.

### User-approved and runtime-validated

- universal world-receiver coverage;
- no visible clouds required;
- broad clouded regions with authored openings normally around `5–7 m` or larger;
- approximately `1.5 m` initial transition softness;
- movement aligned with Weather wind;
- one native URP main directional-light cookie as the complete V0 receiver path;
- globally tiled coverage independent of player, camera, chunk count, and map dimensions;
- `256²` linear `R8` cookie with a `128 m` world-space repeat period;
- automatic deterministic seed evolution at randomized `90–180 s` intervals;
- `10 s` smooth crossfade at `6 Hz` using one retained GPU cookie;
- debug overlay and receiver-audit tooling retained for diagnostics;
- automated benchmark suite retained for future hardware and quality-tier checks;
- hybrid optimization only after measured unacceptable cost.

### Deferred validation, not a V0 blocker

- standalone Player confirmation at 2560 × 1440;
- representative low-end-PC confirmation against the project-wide 60 FPS target;
- shader-variant stripping inspection in a release-oriented build;
- broader day/night, scene-transition, and long-duration soak coverage during the future Weather testing sprint.

## L. Remaining work and gap analysis

### `WEATHER-CLOUD-SHADOW-V0.1` — receiver audit

**Status:** complete in Unity. The warning-only API drift was corrected with `cookieSize2D` and the sort-free `FindObjectsByType` overload.

### `WEATHER-CLOUD-SHADOW-V0.2` — complete directional-cookie source implementation

**Status:** Unity compatibility complete. The user supplied a post-fix receiver audit with both controller and V0 receiver gates at `PASS`, 64/64 loaded-scene renderer/material records supported, all four mandatory authored receiver shaders supported, and both discovered URP assets cookie-enabled.

The implementation resolves every real authored-receiver incompatibility found by the audit. Source review additionally found that `SH_PixelSurfaceLit.shader` inherited `_LIGHT_COOKIES` in `Shader.keywordSpace` through its fallback without compiling the custom ForwardLit variant; V0.2 adds that pragma and hardens the audit against the same false positive.

### `WEATHER-CLOUD-SHADOW-V0.3A` — cloud-area debug visualization

**Status:** complete in Unity. The overlay visibly showed the projected moving cloud regions. The user confirmed that receiver response became visible after lowering shaded transmission; no receiver-path defect remained.

Required end state:

- show the generated cookie directly in the Weather controller Inspector;
- show a world overlay in both Scene and Game views;
- make cloud areas and open-sun areas immediately distinguishable even when receiver lighting contrast is weak or the visible region is uniformly shaded;
- sample the exact active directional-cookie projection rather than duplicating the generator or inventing a second world mapping;
- create no persistent renderer, hierarchy object, material asset, layer, tag, receiver edit, or cloud-generation change;
- keep the visualization disabled or removable after diagnosis.

### `WEATHER-CLOUD-SHADOW-V0.3B` — global tiled coverage and diagnostic focus

**Status:** complete in Unity. The user confirmed that production cloud shading remains globally tiled while the finite debug overlay follows the resolved diagnostic focus.

Required end state:

- preserve the globally tiled directional-cookie field; do not introduce a finite player-, camera-, or chunk-centred cloud simulation window;
- clarify that `Cookie World Size Metres` is one world-space repeat period rather than total cloud coverage;
- expose a generic coverage-focus resolution order: runtime override, Inspector override, assigned fallback camera, `Camera.main`, controller fallback;
- provide public set/clear methods so remote cutscenes may direct diagnostics without moving the player;
- make the debug overlay follow the resolved focus by default and optionally match one complete cookie repeat period;
- record the focus source, position, repeat period, and global-tiling model in the Inspector and copied report;
- leave cloud generation, receiver shaders, scene ownership, and the actual lighting projection unchanged.

### `WEATHER-CLOUD-SHADOW-V0.3C` — debug-focus terminology and Inspector clarity

**Status:** complete in Unity. The user confirmed that automatic `Camera.main` resolution, explicit Transform/camera focus, and the diagnostic-only Inspector terminology work as intended.

Required end state:

- rename serialized diagnostic controls from coverage-focus terminology to debug-focus terminology without losing existing scene values;
- rename the public diagnostic-focus API and report labels so no API suggests that cloud coverage follows a player, camera, or Transform;
- show the live resolved debug focus, resolution source, and world position immediately in the custom Inspector even when both serialized reference fields are `None`;
- warn when an Inspector debug-focus override masks the fallback camera, and separately identify any runtime override that currently wins;
- rename the manual refresh and runtime-clear actions to debug-focus terminology;
- preserve the globally tiled cookie, cloud movement, generation, receiver lighting, scene state, and runtime performance.

### `WEATHER-CLOUD-SHADOW-V0.3D` — low-frequency cookie evolution

**Status:** complete in Unity. The user confirmed that the low-frequency seeded crossfade works exactly as planned.

Objective and acceptance criteria:

- periodically replace the current deterministic cloud seed without an abrupt visible pattern swap;
- retain one authoritative directional cookie and one receiver sample exactly as in V0.2/V0.3;
- perform no automatic evolution work outside Play Mode; edit-mode evolution is manual only;
- while idle, add only an `O(1)` timer/state check and no texture upload, generation, allocation, draw, dispatch, or receiver traversal;
- when evolution begins, generate the next seed once, then blend current/next `R8` pixel buffers into the existing GPU texture at a configurable bounded cadence;
- preserve the existing world movement phase and URP cookie offset throughout the transition so cloud travel does not restart or jump;
- complete the blend by promoting the next seed and returning to zero transition work;
- expose randomized minimum/maximum intervals, transition duration, transition update rate, current/next seed, progress, `Evolve Now`, and `Complete Evolution Immediately` controls and diagnostics;
- retain the current cookie whenever next-seed generation fails, report the complete error, and schedule no corrupted transition;
- preserve sun ownership/restoration, global tiling, debug focus/overlay, Weather wind sampling, receiver shaders, and all Ground/Generated Mass/Vegetation/River systems.

Approved defaults from the accepted design discussion:

- automatic evolution enabled;
- randomized interval: `90–180 s`;
- transition duration: `10 s`;
- blend/upload cadence: `6 Hz`.

Reviewed evidence and direct dependencies:

- `WeatherCloudShadowController.EnsureCookie` currently creates and replaces one unreadable generated texture; it must be refactored to retain CPU pixel data and update the same readable texture during a transition;
- `WeatherCloudShadowCookieGenerator.Generate` currently owns both deterministic pixel generation and texture creation; it must expose reusable pixel generation/upload helpers while preserving its existing field algorithm and `R8`/bilinear/repeat/no-mipmap contract;
- `WeatherCloudShadowControllerEditor` already owns all manual Weather cloud actions and copied reporting, so evolution controls belong there rather than in a new editor module;
- `WeatherCloudShadowReceiverAudit` consumes only `GeneratedCookie` and controller readiness; its public contract does not need to change;
- Unity's texture update contract requires `SetPixelData` followed by `Apply` while the texture remains readable; the active cookie must therefore stop using `Apply(..., makeNoLongerReadable: true)`.

Risks and mitigations:

- **Visible ghosting during morph:** use a smoothstep blend curve and preserve movement phase; validate with the cloud/opening debug overlay and ordinary lighting;
- **runtime upload spikes:** clamp update rate to a bounded range and reuse three byte arrays; no allocation is permitted at each blend step;
- **Inspector edits during a transition:** any dirty pattern setting cancels the pending evolution, rebuilds the authored seed, and reschedules cleanly;
- **automatic editor churn:** randomized scheduling runs only in Play Mode; manual evolution remains available in edit preview;
- **seed repetition:** derive each next seed with a deterministic integer hash and prevent equality with the current seed.

Validation requirements:

1. Unity C# compilation and cookie assignment remain clean.
2. `Evolve Now` visibly morphs the debug cookie and world lighting without an abrupt swap or movement reset.
3. During idle state, the report shows no blending/upload work; during transition, uploads occur at the configured rate only.
4. `Complete Evolution Immediately` ends the transition on the target seed and returns to idle.
5. Changing any pattern setting during a transition rebuilds safely and leaves one valid assigned cookie.
6. Disable/destroy/ownership-change paths release all CPU buffers/texture state without stale evolution state or recurring GC allocation.

Implementation outcome:

- `WeatherCloudShadowCookieGenerator` now separates deterministic pixel generation, readable texture creation, and pixel upload while preserving the existing field algorithm and texture format/filter/wrap contract;
- one retained generator workspace reuses lattice, field, visited, and queue arrays after capacity warm-up;
- `WeatherCloudShadowController` owns reusable current/next/blended byte arrays, one readable active cookie, deterministic next-seed and interval hashing, Play-Mode-only automatic scheduling, smoothstep low-cadence blending, safe dirty rebuild cancellation, manual evolve/complete actions, and lifecycle cleanup;
- `WeatherCloudShadowControllerEditor` exposes live state, seeds, progress, schedule, upload counters, estimated transition bytes, and manual actions;
- the receiver audit, receiver shaders, debug shader, scene, materials, URP assets, Weather wind, Time of Day, Ground, Generated Mass, Vegetation, and River remain unchanged;
- static exact-scope, delimiter, bracket, UTF-8/NUL, obsolete-API, and allocation-location scans passed. The user confirmed that the evolution works exactly as planned; profiler and performance evidence move to V0.3E.


### `WEATHER-CLOUD-SHADOW-V0.3E` — automated runtime benchmark suite

**Status:** compiled and executed successfully after `WEATHER-CLOUD-SHADOW-V0.3E1`; two complete Editor Play Mode reports captured. Reporting clarity is corrected by `WEATHER-CLOUD-SHADOW-V0.3E2`.

Objective:

- measure the persistent directional-cookie cost in the current worst-realistic gameplay view without manually toggling controller settings;
- measure static-cookie and normal moving-cookie cases against adjacent cloud-cookie-disabled baselines;
- measure one complete seed-evolution transition separately from persistent cost;
- verify that post-evolution performance returns to the normal moving-cookie state;
- collect frame-level CPU total, active main-thread, active render-thread, GPU, managed-allocation, draw-call, batch, and SetPass evidence where the runtime exposes valid counters;
- restore the exact captured cloud enabled state, movement speed, evolution setting, debug mode, seed, movement phase, and automatic-evolution schedule after completion, cancellation, disable, destruction, or Play Mode exit.

Exact approved scope:

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
  Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
  Assets/Game/Procedural/Weather/WeatherCloudShadowCookieGenerator.cs
  Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs

Create:
  Assets/Game/Procedural/Weather/WeatherCloudShadowBenchmark.cs
  Assets/Game/Procedural/Weather/WeatherCloudShadowBenchmark.cs.meta
```

Actual affected files:

```text
Modified:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
  Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
  Assets/Game/Procedural/Weather/WeatherCloudShadowCookieGenerator.cs
  Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs

Created:
  Assets/Game/Procedural/Weather/WeatherCloudShadowBenchmark.cs
  Assets/Game/Procedural/Weather/WeatherCloudShadowBenchmark.cs.meta
```

Expected and actual scope match exactly. No scene, receiver shader, material, compute shader, River, Vegetation, Ground, Generated Mass, camera, quality, or pipeline file changed.

Implementation sequence:

1. Add benchmark-safe controller state capture, case application, evolution timing counters, and exact restoration. Do not change ordinary cloud behavior.
2. Add dirty-generation and texture-upload profiler markers in the cookie generator; keep existing generation and upload code paths unchanged.
3. Add one transient runtime benchmark runner. The runner must create no persistent scene object and must preallocate measurement storage before timed windows.
4. Run paired persistent cases with alternating order across two repetitions: cloud cookie disabled versus static cookie, then cloud cookie disabled versus moving cookie. Use 120 warm-up frames and 600 measured frames per window by default.
5. Run one forced evolution case after a short steady-state warm-up, record the complete active transition, then run one post-evolution moving-cookie control window.
6. Save one report automatically under `Library/WeatherCloudShadowBenchmarkDiagnostics` in the Editor or `Application.persistentDataPath` in a Player, retain it in memory, and expose start, cancel/restore, and copy-report actions in the existing Weather cloud Inspector.

Invariants and non-goals:

- the benchmark does not change the scene, camera, Grass density, River, rocks, time of day, quality settings, VSync, target frame rate, or time scale;
- the debug overlay is forced off only during the suite and restored afterward;
- automatic evolution is disabled during persistent windows and restored afterward;
- no receiver shader, material, compute shader, render pass, draw path, cookie representation, pattern default, or evolution cadence is changed;
- benchmark allocations and report construction occur outside measured windows; per-frame sample storage is preallocated;
- unavailable GPU or Profiler counters are reported as unavailable, never as zero;
- whole-frame A/B deltas are evidence for the complete cloud path, not a claim that one shader sample was isolated directly.

Pass criteria:

1. One Inspector action runs the complete suite unattended in Play Mode.
2. One second Inspector action copies the complete retained report after completion.
3. Completion, cancellation, component disable, destruction, or Play Mode exit restores captured cloud state.
4. Persistent cases report mean, median, minimum, maximum, p95, p99, and standard deviation for valid timing samples.
5. Evolution reports preparation time, upload count, raw bytes, measured transition-frame statistics, and the worst frame.
6. No recurring managed allocation is attributable to the benchmark sampling loop after its preallocation warm-up.

#### `WEATHER-CLOUD-SHADOW-V0.3E1` — coroutine compile correction

**Status:** compiled and runtime-validated; superseded only by the V0.3E2 report-integrity clarification.

Observed evidence:

- Unity reports `CS1626` at `WeatherCloudShadowBenchmark.cs` lines 538, 544, 548, 554, and 555: C# forbids `yield return` inside a `try` block that has a `catch` clause.
- The affected symbol is `WeatherCloudShadowBenchmark.RunCompleteSuite`. Its direct nested producers are `RunPersistentPair`, `RunMeasurementWindow`, `RunEvolutionWindow`, `RunPostEvolutionWindow`, and `RunPostEvolutionMeasurement`. The custom Inspector starts and cancels the suite only through the benchmark's public static API.
- The accepted safety invariant remains unchanged: an exception in any nested benchmark iterator must produce an incomplete report and restore the captured controller state.

Exact correction scope:

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Game/Procedural/Weather/WeatherCloudShadowBenchmark.cs
```

Implementation sequence:

1. Keep `RunCompleteSuite` as the Unity-facing coroutine but remove the illegal `try`/`catch` around yielded values.
2. Move the existing ordered suite body into a separate iterator without changing case order, warm-up counts, measurement counts, or controller state transitions.
3. Drive that iterator and every directly yielded nested iterator through an explicit stack. Advance each iterator inside a non-iterator helper that catches exceptions before `RunCompleteSuite` yields to Unity.
4. On success, finish once with `completed = true`. On any captured exception, dispose remaining iterators, finish once with the full exception text, build an incomplete report, and restore captured cloud state.
5. Preserve existing cancellation behavior: `CancelAndRestore` stops the Unity-facing coroutine and calls `FinishSuite` exactly once.

Acceptance criteria:

- no `yield return` remains inside any `try` block with a `catch` clause;
- the benchmark source passes lexical/delimiter checks;
- success, exception, cancellation, disable, and destruction paths retain single-finalization and state-restoration behavior;
- no benchmark case ordering, timing default, report field, controller contract, shader, scene, or subsystem behavior changes.

Implementation outcome:

- `RunCompleteSuite` now owns only initialization, an explicit `Stack<IEnumerator>` driver, Unity-facing yielded values, and one final `FinishSuite` call;
- `RunCompleteSuiteBody` preserves the accepted static-pair, moving-pair, evolution, and post-evolution order exactly;
- `TryAdvanceRoutine` catches exceptions from every manually driven nested iterator before the Unity-facing coroutine yields again;
- remaining iterators are disposed on captured failure, and disposal errors are appended to the same incomplete-report failure text;
- `TryInitializeSuite` captures initialization failures without entering the iterator driver;
- cancellation still stops the Unity-facing coroutine and invokes the existing single `FinishSuite` restoration path;
- the hotfix changes exactly the canonical handoff and `WeatherCloudShadowBenchmark.cs`. The custom Inspector, controller, generator, shaders, scene, and all non-Weather systems are byte-identical to the delivered V0.3E patch.

Post-change validation:

- lexical/delimiter scan: passed;
- UTF-8 and NUL scan: passed;
- illegal iterator form scan: passed; `RunCompleteSuite` contains no `catch` clause and its only `yield return` is outside all `try` blocks;
- nested-driver contract scan: passed; suite body, advance helper, failure disposal, and single finalization are present;
- exact hotfix scope comparison against the delivered V0.3E files: passed, two changed paths only;
- Unity compilation: passed in the live Unity project; two complete benchmark suites ran with restoration PASS.

### `WEATHER-CLOUD-SHADOW-V0.3E2` — benchmark execution-order evidence

**Status:** complete in Unity. The final 2560 × 1440 run reports explicit baseline-first and candidate-first execution order, actual execution indices, start offsets, pair elapsed times, complete timing capture, and restoration `PASS`.

Observed evidence:

- `RunCompleteSuiteBody` already alternates persistent execution order by repetition. Repetition 1 runs baseline then candidate; repetition 2 runs candidate then baseline.
- `CreatePersistentWindow` constructs and appends the baseline record before the candidate record regardless of execution order. The former `[All Measurement Windows]` section iterated that creation-order list, making even repetitions appear baseline-first despite correct runtime execution.
- Paired deltas were still calculated from the correct baseline and candidate records, so the existing GPU verdict remains valid; the ambiguity affected evidence presentation, not cloud behavior or pair arithmetic.

Exact correction scope:

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Game/Procedural/Weather/WeatherCloudShadowBenchmark.cs
```

Implementation sequence:

1. Assign every measurement window a monotonically increasing execution index at the moment it actually starts.
2. Record its suite-relative start offset and elapsed time including warm-up.
3. Preserve baseline and candidate records for paired arithmetic, while retaining a separate actual-execution list for detailed reporting.
4. Print each pair's actual order, execution indices, and total pair elapsed time.
5. Print detailed windows in actual execution order rather than construction order.
6. Version the generated report and filename as V0.3E2 so old and corrected reports cannot be confused.

Invariants:

- warm-up counts, measured-frame counts, repetition count, candidate case settings, controller state transitions, evolution measurement, report statistics, cancellation, and restoration are unchanged;
- no controller, generator, Inspector, shader, scene, receiver, Ground, Generated Mass, Vegetation, River, material, quality, or pipeline file changes;
- no persistent or timed-window runtime work is added beyond writing a few scalar timestamps and one list reference when each window begins or ends.

Acceptance criteria:

- the paired summary explicitly reports `baseline → candidate` for repetition 1 and `candidate → baseline` for repetition 2;
- execution indices in each pair match the detailed actual-execution list;
- every detailed window prints a suite-relative start offset and elapsed time;
- the cloud-shadow benchmark still completes and restores captured state;
- the measured cloud system remains behaviorally byte-identical to V0.3E1.

### `WEATHER-CLOUD-SHADOW-V0.4` — accepted performance freeze

**Status:** complete and frozen on 2026-07-23.

Objective:

- close the cloud-shadow V0 implementation after corrected benchmark evidence;
- retain the existing native directional-cookie runtime without source or tuning changes;
- record the accepted visual, architecture, evolution, tooling, and performance baseline;
- defer Player-build and low-end-hardware confirmation to the future project-wide testing sprint without reopening the architecture pre-emptively.

Exact freeze scope:

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
```

No C#, shader, scene, material, prefab, Shader Graph, compute shader, pipeline asset, Ground, Generated Mass, Vegetation, River, Time of Day, or Weather-wind source changes are part of V0.4.

Final benchmark evidence:

| Evidence | Result |
|---|---|
| Runtime | Unity Editor Play Mode, Unity 6000.5.0f1, Direct3D12 |
| View | Current worst-realistic gameplay view with dense grass, a large River, and many rocks |
| Resolution / quality | 2560 × 1440, `PC`, `PC_RPAsset` |
| GPU | NVIDIA GeForce RTX 3080 Ti |
| Static cookie mean paired GPU median delta | `+0.016 ms` |
| Moving cookie mean paired GPU median delta | `+0.011 ms` |
| Static cookie mean paired CPU median delta | `-0.192 ms`, inconclusive within Editor noise |
| Moving cookie mean paired CPU median delta | `-0.486 ms`, inconclusive within Editor noise |
| SetPass calls | No material regression; baseline/candidate windows remained approximately `116–117` |
| Evolution preparation | `14.586 ms` once per transition |
| Evolution blend/upload | `60` updates, `73.323 ms` total, `1.466 ms` maximum update |
| Evolution upload volume | `3,932,160` raw texel bytes over the complete transition |
| Evolution GPU median | `0.631 ms` |
| Post-evolution GPU median | `0.633 ms` |
| Benchmark restoration | `PASS` |

Accepted frozen baseline:

- one Weather-owned native URP main directional-light cookie;
- one cookie-aware main-light sample per supported receiver;
- universal world-receiver coverage or explicit exemption;
- global tiled world coverage with no player-, camera-, chunk-, or map-sized cloud simulation;
- `256²` linear `R8` texture, bilinear filtering, repeat wrap, no mipmaps;
- `128 m` world-space repeat period;
- broad cloud regions, authored clearances normally around `5–7 m` or larger, and approximately `1.5 m` transition softness as the current visual baseline;
- Weather-wind-driven movement with the accepted ordinary speed currently captured as `0.7 m/s` by the benchmark;
- randomized automatic evolution every `90–180 s`;
- `10 s` smooth seed crossfade at `6 Hz` with one retained GPU cookie and reusable CPU buffers;
- diagnostic overlay, receiver audit, copied comprehensive reports, and automated benchmark suite retained;
- debug visualization and benchmark runner disabled outside explicit diagnostic use.

Decision:

- retain the directional-cookie architecture;
- do not reduce resolution, remove receivers, remove evolution, add a finite coverage window, or introduce the hybrid shared-mask fallback based on current evidence;
- reopen optimization only after a representative Player or low-end-hardware capture demonstrates a material regression;
- treat Editor GC counters as contaminated by Editor and benchmark-runner activity rather than evidence of cloud-system allocation;
- perform no additional cloud-shadow implementation work during the current feature sequence unless a concrete defect appears.

### `WEATHER-CLOUD-SHADOW-HANDOFF-V0.5` — godrays exploration handoff

**Status:** complete and superseded as the active LightRay design authority.

The concise handoff initiated the architecture discussion and remains useful as historical cloud-system orientation. Its “undefined godrays” status is superseded by `Assets/Docs/Weather_Light_Ray_Architecture.md`.

### `WEATHER-CLOUD-SHADOW-HANDOFF-V0.6` — LightRay architecture cross-reference

**Status:** synchronized through `WEATHER-LIGHT-RAY-V1.1D-AF4`; the cloud-shadow implementation remains frozen and unchanged.

Weather LightRay remains the approved adjacent subsystem. The canonical document records:

- shared Sun/Moon source abstraction with mutually exclusive day/night source groups;
- cloud-respecting and cloud-ignoring policies;
- graceful transition suspension for cloud-respecting rays and continued operation for ignore-cloud rays;
- timed, permanent, and externally controlled lifetimes;
- authored and gameplay-requested divine overrides;
- analytical gameplay influence independent of rendering;
- mandatory hybrid rendering;
- several separate continuous parallel beam ribbons per LightRay zone;
- full-resolution atmospheric masks and composite, one real shadowless per-zone Spot Light for material response, and an optional zero-default screen-space complement;
- explicit rejection of the sampled frustum/tube/ellipse renderer and broad visible envelope;
- performance, diagnostics, implementation order, and validation gates.

The cloud-shadow implementation remains frozen. The existing LightRay CPU query projects controlled world positions into the directional-cookie plane and samples the retained readable `R8` cookie with bilinear repeat filtering. It does not alter the installed cookie, receiver integration, generation, movement, evolution cadence, restoration, benchmark state, or cloud visual baseline.

### `WEATHER-CLOUD-SHADOW-HANDOFF-V0.7` — LightRay CPU query contract

**Status:** accepted. Unity compilation passed and the CPU projection markers were visually validated against the Cloud + Sun Openings overlay after the V1.0B visibility and V1.0C shared-focus corrections.

Changed cloud source:

```text
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
```

Added public query contracts:

```text
TrySampleCloudTransmission(worldPosition, directionalLight, out sample)
TryProjectCloudCookieUv(worldPosition, directionalLight, out uv, out offset, out error)
ShadedTransmission
```

The query returns explicit clear-sky, stable, evolution-unstable, unavailable, or error status. It samples `GeneratedCookie.GetPixelBilinear`; it does not execute `WeatherCloudShadowCookieGenerator`, allocate another cloud field, or read back the GPU. When the queried source is the active captured Sun, the computed source offset is required to match `CurrentCookieOffset` while the Sun gate is active. The LightRay Scene-view diagnostic overlays controlled CPU sample markers on the existing shader-sampled Cloud / Opening Map for live projection validation.

## M. Exact implementation scope and sequence

### `WEATHER-CLOUD-SHADOW-V0.3A` debug-visualization scope

Modified:

```text
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs
```

Created:

```text
Assets/Game/Rendering/Weather/Shaders/SH_WeatherCloudShadowDebugOverlay.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherCloudShadowDebugOverlay.shader.meta
```

The patch adds a transient programmatic draw only. It does not edit the cookie generator, receiver shaders, receiver audit, scene, materials, pipeline assets, Weather wind, Ground, Generated Mass, Vegetation simulation, or River simulation.


### `WEATHER-CLOUD-SHADOW-V0.3B` global-coverage clarification scope

Modified:

```text
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs
```

No generator, shader, receiver, scene, material, pipeline, compute, Ground, Generated Mass, Vegetation, or River file changes are required. The initially proposed finite-window regeneration work is intentionally not implemented because the active directional cookie already repeats across world space.

### `WEATHER-CLOUD-SHADOW-V0.3C` debug-focus terminology scope

Modified:

```text
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs
```

No new file is required. Existing serialized Inspector references must be preserved with `FormerlySerializedAs`. No cookie generator, debug shader, receiver audit, receiver shader, scene, material, pipeline, compute, Ground, Generated Mass, Vegetation, or River file may change.

### `WEATHER-CLOUD-SHADOW-V0.3D` cookie-evolution scope

Approved modified files:

```text
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowCookieGenerator.cs
Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs
```

No new file is approved. No debug shader, receiver audit, receiver shader, scene, material, pipeline, compute, Ground, Generated Mass, Vegetation, or River file may change.

Implementation sequence:

1. Refactor the generator into deterministic pixel generation, texture creation, and pixel-upload helpers without changing pattern output.
2. Refactor the controller's initial/dirty rebuild to own reusable current, next, and blended byte buffers plus one readable active cookie texture.
3. Add the idle/preparation/blending/completion state machine, deterministic next-seed selection, randomized schedule, low-cadence smoothstep blending, safe cancellation, and lifecycle reset.
4. Add serialized evolution controls, public manual actions, status properties, and comprehensive report evidence.
5. Extend the custom Inspector with manual evolution actions and live state/progress/cost reporting.
6. Update the parent Weather architecture only after source implementation matches this plan.
7. Reconcile the exact final diff, reread all five files and the receiver-audit dependency, and record static/Unity validation honestly.

Performance budget:

- idle: timer/state comparison only;
- preparation: one dirty `O(R²)` next-seed generation;
- transition: one `O(R²)` byte blend and one `R²` texture upload at the configured cadence, with no per-step allocation;
- default 256²/6 Hz/10 s transition: 60 uploads of 65,536 texel bytes, approximately 3,932,160 raw texel bytes per complete transition before engine/driver overhead;
- receiver GPU path: unchanged single URP directional-cookie sample.

### Earlier V0.2 implementation scope

### Approved modified files

```text
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Generated_Mass_Framework.md
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/River_Rendering_Roadmap.md

Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowReceiverAudit.cs

Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
```

### Approved created files

```text
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs.meta
Assets/Game/Procedural/Weather/WeatherCloudShadowCookieGenerator.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowCookieGenerator.cs.meta
Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs.meta
```

### Scene integration boundary

`Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` was never raw-edited by the source patches. The controller was attached through Unity to the existing `Systems/Weather` object and subsequently passed the live controller and receiver audits. V0.4 changes documentation only and preserves the validated scene ownership.

### Historical V0.2 file-by-file sequence — completed

1. Created the dirty-triggered seamless cookie generator using linear `TextureFormat.R8`, bilinear filtering, repeat wrap, `HideFlags.HideAndDontSave`, and no mipmaps.
2. Created the ExecuteAlways controller with authoritative-sun preservation/restoration, bounded Weather-wind sampling, world-space movement phase, sun-cookie-plane projection, and steady-state offset updates.
3. Created the custom Inspector with grouped controls, rebuild/reset actions, copied diagnostics, and bounded edit-preview ticking.
4. Added `_LIGHT_COOKIES` to Ground, Generated Mass Pixel Surface, and River ForwardLit passes without changing their lighting includes.
5. Added `_LIGHT_COOKIES` to Vegetation ForwardLit and replaced only its main-light query with the position-aware overload.
6. Corrected the receiver audit's package-shader classification and custom authored pragma verification.
7. Updated parent and receiver documents with actual local changes and preserved behavior.
8. Reconciled exact scope, reread final sources and direct dependencies, ran static checks, and then completed Unity compilation, runtime, visual, and profiling validation through V0.3E2.

### Performance model

- steady CPU: `O(1)` movement integration plus bounded wind sampling; no renderer traversal or managed allocation;
- dirty CPU: `O(R^2)` cookie generation only when pattern settings change or rebuild is explicitly requested;
- GPU: URP's native main-light cookie sample on compatible lit fragments; no additional draw, pass, compute dispatch, or custom receiver sample;
- V0.2 base memory: one generated `R8` texture (`R^2` texel bytes before engine-side overhead), default 256 × 256 = 65,536 texel bytes;
- V0.3D evolution memory: three reusable `R8` CPU byte buffers plus one retained generator workspace; the active texture remains single and readable for bounded updates;
- V0.3D idle CPU: one timer/state check; no generation, upload, or allocation;
- V0.3D transition CPU/upload: one next-seed generation, then one `R²` byte blend and texture upload at the configured cadence; default raw upload estimate is 3,932,160 texel bytes per transition;
- shader variants: one `_LIGHT_COOKIES` fragment variant added to Ground, Generated Mass Pixel Surface, Vegetation, and River.

### Prohibited scope

Do not edit Generated Mass geometry, generation, editor, feature-atlas, or lighting-include code beyond the declared Pixel Surface ShaderLab pragma; do not edit URP package shaders, Shader Graph assets, actor/hearth materials, Weather wind generation, Time of Day ownership, River simulation/foam/compute files, Vegetation compute/interaction/trample files, Ground generation/hydrology files, pipeline assets, layers, tags, materials, prefabs, or scene YAML.

---

## N. Validation and evidence ledger

| ID | Procedure | Result | Proves | Does not prove |
|---|---|---|---|---|
| `CSH-LIVE-01` | User ran `Tools > PS3D > Weather > Run & Copy Cloud-Shadow Receiver Audit` | Completed: 64 records, five shaders, both RP assets supported, authoritative directional sun resolved | Live pre-V0.2 receiver and infrastructure inventory | Post-V0.2 visual behavior |
| `CSH-SRC-01` | Complete reread of new controller, generator, Inspector, audit, changed shader passes, Vegetation lighting, and Weather wind public contract | Passed | Source changes match the recorded architecture and direct dependencies | Unity compilation |
| `CSH-SRC-02` | C# lexical delimiter/string/comment balance over all four cloud-shadow C# files | Passed | No unmatched braces/brackets/parentheses or unterminated lexical state | Type/API correctness |
| `CSH-SRC-03` | Obsolete-API scan | Passed: no `Light.cookieSize`, `FindObjectsSortMode`, sorted `FindObjectsByType`, or instance-ID API; `DestroyImmediate` occurs only in explicit edit-time resource cleanup, not `OnValidate` | Unity 6.5 API warnings previously observed are removed | Other compiler warnings |
| `CSH-SRC-04` | Receiver pragma and lighting-path inspection | Passed: Ground/Pixel Surface/Vegetation/River authored pragmas present; Ground/Pixel Surface use PBR; River/Vegetation use position-aware main light | Every mandatory custom receiver has the intended source path | Shader import and runtime variant retention |
| `CSH-SRC-05` | Exact before/after scope comparison | Passed: six docs, five existing source/shader files plus Pixel Surface and three new source/meta pairs; no scene/material/pipeline/compute changes | Final source patch remains inside updated approved scope | Live workspace conflict state |
| `CSH-SRC-06` | Meta GUID, UTF-8, NUL, and line-ending checks | Passed | New Unity source companions are structurally present and patch files are text-safe | Unity asset-database import |
| `CSH-UNITY-01` | Unity C# compile and shader import | Passed by user execution; no implementation compiler or shader error was reported before the successful runtime audit | Runtime type/API and shader import correctness | Visual/performance acceptance |
| `CSH-AUDIT-02` | Post-V0.2 receiver audit after scene attachment | Passed: controller gate `PASS`; V0 receiver gate `PASS`; 64/64 loaded-scene records and all mandatory authored receivers supported | Controller, cookie, RP assets, loaded scene, and mandatory receivers are all ready | Visual quality |
| `CSH-DBG-01` | Cloud-area overlay and generated-cookie Inspector preview | Passed in Unity; user confirmed projected field movement and receiver response | Exact projected cookie regions can be inspected independently of lighting contrast | Future visual-regression checks |
| `CSH-VIS-01` | Gameplay-camera continuity across all receiver families | Passed for the active stress view; receiver audit also reports 64/64 supported records | Universal world-space visual coverage | Other scenes and future shaders |
| `CSH-TIME-01` | Sunrise, midday, sunset, night, disable/enable | Deferred to the future Weather testing sprint | Broader lifecycle and low-elevation coverage | Not a V0 freeze blocker |
| `CSH-BENCH-SRC-01` | V0.3E exact-scope and static source audit | Passed: five modified files plus benchmark source/meta; unique metadata GUID; C# delimiter/lexical balance; UTF-8/NUL; obsolete API; no scene/receiver/system drift | Source structure and scope compliance | Unity compile and runtime measurement correctness |
| `CSH-BENCH-SRC-02` | V0.3E1 coroutine compile correction audit | Passed: exact two-file hotfix scope; no yielded value inside a `try`/`catch`; nested iterator exceptions are captured by the explicit driver; lexical/delimiter and UTF-8/NUL checks pass | The reported `CS1626` source form is removed while restoration/finalization contracts remain present | Unity type/API compilation and runtime suite completion |
| `CSH-BENCH-SRC-03` | V0.3E2 execution-order evidence audit | Passed in source and Unity; final report shows repetition 2 `candidate → baseline`, actual execution indices, timings, and restoration `PASS` | Correct report-order evidence without changing cloud behavior or pair arithmetic | Future Player/hardware runs |
| `CSH-PERF-01` | Matched warmed baseline/candidate capture | Passed for the 2560 × 1440 Editor stress view: static `+0.016 ms` and moving `+0.011 ms` mean paired GPU median deltas; no SetPass regression | Persistent native cookie cost is negligible in the measured view | Player and low-end-PC confirmation during the future testing sprint |

### Post-change consistency result

- Weather owns the only cloud pattern, phase, direction, speed, transmission, and sun gate.
- Receivers use URP's native main-light cookie exactly once.
- Ground, Generated Mass, Vegetation, and River simulation/generation ownership is unchanged.
- No parallel vertex field, custom fragment cloud sample, fullscreen pass, renderer feature, compute field, cloud geometry, material clone, or per-renderer update was introduced.
- Runtime, receiver compatibility, cloud-pattern readability, seed evolution, benchmark restoration, and persistent stress-view performance are verified in Unity. V0.4 changes documentation only.

## O. Constraints and invariants

- One authoritative cloud field and phase only.
- Universal world-receiver coverage or explicit exemption.
- No visible clouds required for V0.
- No separate cloud seeds, phases, directions, or speeds per subsystem.
- No cloud influence on Ground ownership, River hydrology/simulation, Foam lifecycle, Vegetation wind/trample, Generated Mass geometry, actors, collision, or gameplay state.
- No per-frame CPU material/renderer traversal or mesh/instance rebuild.
- No recurring managed allocation.
- No raw scene, material, prefab, or Shader Graph JSON edits.
- No new layer or tag.
- No final-colour blanket darkening without an explicit approved approximation.
- No double shading from cookie plus any custom cloud field.
- No unsupported gameplay-world shader after final receiver audit.
- No claim of performance based only on Ground topology.
- No automatic cookie rejection based only on theoretical sample count; measure on target hardware.
- No hybrid optimization before complete cookie profiling proves it necessary.
- Any additional representation, custom receiver field, buffer, renderer feature, compute field, or hybrid path requires a plan update and approval.

---

## P. Frozen decisions, deferred validation, and residual risks

### Frozen decisions

- Universal receiver coverage is mandatory.
- Weather owns the cloud-shadow system.
- The native main directional-light cookie is the only V0 receiver path.
- Standard URP Lit and compatible Lit Shader Graphs use native package behavior.
- Ground, Generated Mass, Vegetation, and River retain only the minimum cookie-compatibility edits already validated.
- The field tiles globally; map dimensions and loaded chunk count do not create additional cloud state.
- `256² R8`, `128 m` repeat period, wind movement, and low-frequency seed evolution are accepted.
- Hybrid optimization is deferred until measured Player or low-end-hardware failure.
- No scene YAML, package shader, material, pipeline asset, layer, or tag is changed by the V0.4 freeze.

### Deferred validation

- standalone Player benchmark at the target resolution and quality;
- representative low-end-PC benchmark against the project-wide 60 FPS target;
- release-oriented shader-variant stripping inspection;
- broad sunrise/sunset/night, scene-transition, disable/enable, and long-duration soak tests during the future Weather testing sprint.

These are future validation tasks, not current architecture or implementation blockers.

### Residual risks

- **Low-end fragment scaling — Low to medium, unverified outside the RTX 3080 Ti Editor run.** Current evidence shows only `+0.011–0.016 ms` paired GPU median cost at 2560 × 1440. Verification: repeat V0.3E2 in a representative Player build and low-end target.
- **Evolution preparation hitch — Low, not demonstrated visually.** The measured one-time preparation cost is `14.586 ms`; Editor frames already contain unrelated larger spikes. Verification: inspect a Player capture and visible gameplay during forced evolution.
- **Low-sun projection behavior — Medium.** Cookie-plane projection may become visually less intuitive at shallow sun angles. Mitigation: retain useful-sun gating and include sunrise/sunset validation in the Weather testing sprint.
- **Shader stripping — Low, unverified.** Authored cookie variants passed Editor runtime validation but release-oriented stripping remains unchecked. Verification: inspect a representative Player build.
- **Future receiver drift — Medium over project lifetime.** New sun-responsive shaders can omit cookie compatibility. Mitigation: retain and rerun `WeatherCloudShadowReceiverAudit` whenever new receiver families are introduced.

## Q. Recommended reading order

1. `AGENTS.md` and `Assets/AGENTS.md` — read completely; extract mandatory gates and Unity constraints.
2. This document — read completely; extract universal receiver requirements, candidate gates, exact planned phases, and blockers.
3. `Assets/Docs/Weather_System_Architecture_Provisional.md` — read completely; confirm parent ownership.
4. `Assets/Docs/Weather_Wind_Architecture.md` and `WeatherWindDomain.cs` — read current architecture and complete implementation; confirm sampling, units, cadence, and lifecycle.
5. `TimeOfDayController.cs`, `TimeOfDayProfile.cs`, and `LightingModifierProfile.cs` — read completely; confirm sun and global environment ownership.
6. Ground, Generated Mass, Vegetation, and River canonical documents changed by this update — read their new cloud sections and current local lighting invariants.
7. All current receiver shaders/includes in section M — read completely with direct dependencies.
8. Active Shader Graphs, renderer materials, scene renderers, and installed URP lighting/cookie source — determine complete coverage and exact integration feasibility.
9. Live scene and Git diffs — preserve unrelated changes, especially paused River/Ground work.

---

## R. Commands and reproduction reference

Run from the live project root.

```powershell
git status --short --branch
git diff --name-only
git diff -- Assets/Docs/Weather_Cloud_Shadow_Handoff.md Assets/Docs/Weather_System_Architecture_Provisional.md
```

Use these to establish branch state and protect unrelated work. Do not reset or restore supplied changes.

```powershell
rg --files Assets/Game -g '*.shader' -g '*.shadergraph' -g '*.shadersubgraph' -g '*.mat'
rg -n "SampleWindXZ|TrySampleWindXZ|FieldAnchor|RenderSettings.sun|ApplySun" Assets/Game
rg -n "UniversalFragmentPBR|GetMainLight|VegetationEvaluateLighting|RiverWaterEvaluateLighting" Assets/Game/Rendering
```

Use these to recover receiver families, producer contracts, and lighting integration points.

The editor receiver audit must later provide a scene-specific material/shader list because source-file enumeration alone does not prove active usage.

---

## S. Final state matrix

| Objective | Stable ID | Expected files | Actual files | Runtime cost | Status | Validation | Next action |
|---|---|---|---|---|---|---|---|
| Universal receiver scope | `WEATHER-CLOUD-SHADOW-DOC-V0.1` | Six canonical docs | Exact match | None | Complete | Documentation checks passed | Retained |
| Directional-cookie lock | `WEATHER-CLOUD-SHADOW-DOC-V0.2` | Six canonical docs | Exact match | None | Complete | Documentation checks passed | Retained |
| Live receiver audit | `WEATHER-CLOUD-SHADOW-V0.1` | Audit source/meta + docs | Implemented and run | Editor-triggered only | Complete | User supplied live report | Superseded by post-V0.2 audit |
| Complete cookie runtime and receiver compatibility | `WEATHER-CLOUD-SHADOW-V0.2` | Exact files in section M plus Unity scene attachment | Implemented and attached | `O(1)` CPU steady state, dirty `O(R²)` generation, one native cookie sample per affected lit fragment | Complete | Controller and receiver audits passed | Retain |
| Cloud-area debug visualization | `WEATHER-CLOUD-SHADOW-V0.3A` | Four modified files and one shader/meta pair | Implemented | One diagnostic transparent draw while enabled | Complete | User confirmed projected field and lighting response | Retain |
| Global tiled coverage and diagnostic focus | `WEATHER-CLOUD-SHADOW-V0.3B` | Four modified files | Exact match | Same cookie path; no recenter or map simulation | Complete | User confirmed overlay focus behavior and global-coverage model | Retain |
| Debug-focus terminology and Inspector clarity | `WEATHER-CLOUD-SHADOW-V0.3C` | Four modified files | Exact match | No runtime behavior or cost change | Complete | User confirmed the cleanup works | Retain |
| Low-frequency cookie evolution | `WEATHER-CLOUD-SHADOW-V0.3D` | Five modified files | Exact match | Idle `O(1)` check; dirty `O(R²)` generation; bounded `O(R²)` blend/upload only during transition; receiver GPU unchanged | Complete | User confirmed the evolution works exactly as planned | Retain |
| Automated runtime benchmark suite | `WEATHER-CLOUD-SHADOW-V0.3E` + `V0.3E1` + `V0.3E2` | Original seven-file suite plus two two-file corrections | Complete and live validated | No idle cost; transient hidden runner only while suite runs; preallocated timed-window storage; optional profiler counters; one forced evolution; scalar execution metadata only | Final 2560 × 1440 report completed with actual alternating order and restoration `PASS` | Static `+0.016 ms`; moving `+0.011 ms` mean paired GPU median deltas | Retain for future Player/hardware checks |
| Performance retain/reopen decision | `WEATHER-CLOUD-SHADOW-V0.4` | Two Weather documents | Exact docs-only freeze scope | Documentation only | Complete and frozen | Final corrected benchmark evidence recorded | Native cookie retained; hybrid fallback deferred | No current cloud implementation work |
| Godrays exploration handoff | `WEATHER-CLOUD-SHADOW-HANDOFF-V0.5` | Separate concise Markdown handoff | Delivered with V0.4 | Documentation only | Complete, historical | Points to relevant source and frozen decisions | Superseded by LightRay canonical architecture | Retain as historical orientation |
| LightRay architecture cross-reference | `WEATHER-CLOUD-SHADOW-HANDOFF-V0.6` | Three Weather Markdown documents | Exact docs-only scope | Documentation only | Complete | Confirms LightRay consumes cloud state without modifying receiver-cookie V0.4 | LightRay runtime not started | Follow `Weather_Light_Ray_Architecture.md` |
| LightRay CPU cloud-query contract | `WEATHER-CLOUD-SHADOW-HANDOFF-V0.7` | Cloud controller plus LightRay source/docs | Query-only cloud modification | No cloud steady-state work; samples occur only when called | Source prepared; Unity pending | Query samples retained cookie and exposes evolution stability | Cloud V0.4 generation/receiver path unchanged | Validate CPU markers against shader overlay |
| LightRay future-transmission query | `WEATHER-CLOUD-SHADOW-HANDOFF-V0.8` | Cloud controller + three Weather documents + LightRay population source scope | Query-only cloud source change; no generator/receiver change | No idle cloud work; one CPU projection + bilinear sample per LightRay request | Source prepared; Unity pending | Static exact-scope and query-contract audit passed | Validate through V1.2D population |

## T. Receiving-model startup checklist

1. Treat `WEATHER-CLOUD-SHADOW-V0.4` as the frozen cloud-shadow baseline; do not redesign or optimize it without a concrete defect or new benchmark evidence.
2. Read `Assets/Docs/Weather_Light_Ray_Architecture.md` as the canonical adjacent-system plan.
3. Read the current cloud controller, cookie generator, Time of Day sun owner, Weather wind owner, PC Renderer/RP assets, custom Inspector, debug overlay shader, receiver audit, and benchmark runner identified by the LightRay review evidence.
4. Preserve the mandatory hybrid LightRay target, source exclusivity, cloud policies, transition suspension, permanence, and analytical gameplay-query contracts.
5. Add only the minimal reviewed cloud-transmission query required by LightRay; keep the installed cookie, receiver contract, generation, movement, and V0.4 tuning unchanged.

## V0.4 freeze compliance note

- Review evidence: the complete V0.3E2 report supplied by the user records actual alternating order, complete timing capture, and restoration `PASS` at 2560 × 1440.
- Plan-first rule: the V0.4 objective, evidence, exact two-document scope, invariants, accepted defaults, deferred gates, and decision are recorded in section L before the parent Weather document is updated.
- Final diff: exactly `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` and `Assets/Docs/Weather_System_Architecture_Provisional.md` change.
- Runtime comparison: no C#, shader, scene, serialized asset, material, pipeline, or receiver file differs from the user-validated V0.3E2 implementation.
- Validation: Markdown/text-safety, exact-scope, stale-status, and benchmark-value consistency checks pass. Unity validation is not required for the documentation-only freeze.

## V0.2 source-patch compliance note

The source patch implements the approved cookie architecture and all mandatory custom receiver compatibility changes. It intentionally does not raw-edit `VisualFrameworkDemo.unity`; scene attachment remains a Unity-only action. No unresolved source receiver incompatibility is knowingly left in scope.

## Final completeness audit

- A–T sections are present.
- Universal receiver coverage and explicit exemptions remain authoritative.
- The directional-light cookie remains the sole V0 path.
- Weather controller, dirty cookie generation, Inspector diagnostics, audit hardening, and all mandatory custom receiver source edits are implemented.
- Pixel Surface fallback-keyword inheritance is explicitly corrected in both source and audit logic.
- Exact source files are declared and reconciled.
- Static source, metadata, text-safety, and scope checks are recorded.
- Unity runtime/receiver compatibility, cloud-area visualization, global coverage, seed evolution, benchmark restoration, and stress-view performance are recorded as passed.
- Directional-cookie repeat wrapping is now the authoritative scalability model; no finite coverage window or recenter system is required.
- V0.3B actual scope is exactly two Weather documents plus the controller and custom Inspector; the generator and all receiver systems remain unchanged.
- V0.3C actual scope is exactly the same two Weather documents plus the controller and custom Inspector. `FormerlySerializedAs` preserves all three renamed serialized fields; no generator, shader, receiver, scene, material, or production-lighting file changed.
- V0.3C static checks and live Inspector validation passed.
- V0.3D actual scope is exactly two Weather documents plus the controller, cookie generator, and custom Inspector; no new file or receiver/system edit was introduced.
- V0.3D retains one receiver cookie sample and one GPU cookie texture, adds reusable CPU buffers/workspace, and performs no automatic evolution outside Play Mode.
- V0.3D exact-scope and static checks passed; the user confirmed the runtime evolution works as planned. Allocation and performance evidence move to V0.3E.
- Static scope, delimiter, NUL/encoding, and obsolete-API scans passed; Unity compilation and remote-focus visualization were subsequently validated by the user.
- V0.3E actual scope matches its exact seven-file plan. It adds a transient hidden runtime benchmark runner, benchmark-safe controller state capture/restoration, evolution CPU timing, generator/upload profiler markers, an Inspector start/cancel/copy workflow, and automatic report saving. No scene or receiver-system file changed.
- V0.3E static exact-scope, unique-GUID, delimiter/lexical, UTF-8/NUL, obsolete-API, and subsystem-drift checks passed. The first Unity compile exposed `CS1626` in the top-level iterator.
- V0.3E1 changes only the canonical handoff and benchmark source, removes the illegal iterator form, preserves case ordering and single restoration/finalization behavior, and passes exact-scope, lexical/delimiter, UTF-8/NUL, and nested-driver contract checks. It compiled and completed two live Editor Play Mode suites with restoration PASS.
- Live-source inspection confirmed that persistent execution already alternated correctly; only the creation-order detailed report was misleading. V0.3E2 changes exactly the handoff and benchmark source, records actual execution indices/start offsets/elapsed times, prints pair order and actual detailed order, versions the report as V0.3E2, and leaves all measurement and cloud behavior unchanged.
- V0.8 adds only a query-time future offset to the existing CPU cookie projection; the cookie generator, installed directional cookie, receiver shaders, and V0.4 benchmark path remain unchanged.
- Hybrid optimization remains deferred behind measured Player or low-end-hardware failure.
- The concise godrays exploration handoff remains historical orientation. `Assets/Docs/Weather_Light_Ray_Architecture.md` is now the authoritative adjacent-system plan and still does not authorize runtime edits without a separately approved patch.

## U. LightRay future-transmission query boundary — `WEATHER-CLOUD-SHADOW-HANDOFF-V0.8`

### Status

Source implementation prepared as part of `WEATHER-LIGHT-RAY-V1.2D`; Unity validation is pending.

### Contract

`WeatherCloudShadowController` now exposes a query-only future-time transmission path for LightRay candidate forecasting:

```text
TrySampleCloudTransmissionAtTimeOffset(
    world position,
    celestial directional light,
    future seconds,
    out transmission sample)
```

The query reuses the existing readable generated cookie and the exact source-local directional-cookie projection. Future offset is derived analytically from the controller-owned current world phase plus resolved wind direction multiplied by movement speed and requested future time. It does not regenerate cookie pixels, scan the cookie, read back the GPU, alter the installed directional cookie, change receiver shader work, or create a second cloud field.

The frozen V0.4 cloud producer remains authoritative for generation, movement, evolution, cookie assignment, and universal receiver shading. Automatic LightRay candidates, world-cell identity, ground validation, footprint sampling, budgets, hysteresis, handles, and lifecycle remain owned by `WeatherLightRayController` and its non-component population runtime.

During seed evolution the query reports `EvolutionUnstable` exactly as the present-time query does. LightRay population owns the established suspension rule and does not request dual outgoing/incoming cloud evaluation in V1.2D.

### Exact source impact

Modified cloud source:

- `Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs`

Unchanged cloud sources and assets:

- `WeatherCloudShadowCookieGenerator.cs`
- cloud debug overlay shader;
- cloud benchmark runner;
- receiver shaders and includes;
- scene, material, renderer, light, layer, tag, and project settings.

### Performance

Each requested future sample performs one CPU cookie projection and one `Texture2D.GetPixelBilinear` read against the existing readable texture. There is no idle cloud cost when LightRay automatic population is disabled. Population cadence and sample count are bounded by the LightRay controller; the cloud controller performs no autonomous forecast loop.


## V. LightRay measured-cover and population-policy consumer boundary — `WEATHER-CLOUD-SHADOW-HANDOFF-V0.9`

`WEATHER-LIGHT-RAY-V1.2E` consumes the frozen cloud field through two independent contracts:

1. existing current/future world-position transmission queries for bounded `Clear Footprint` and `Distinct Cloud Opening` candidate qualification;
2. one cached normalized global cloud-cover value for population-rule activation curves.

`WeatherCloudShadowController.MeasuredCloudCover` is computed only from the already generated CPU R8 transmission pixels when the current or next cookie is generated. During seed evolution, the cached current/next measurements are interpolated with the same eased evolution progress. Steady reads are `O(1)`. The LightRay selector/population system must not trigger cookie generation, scan pixels per frame, read back GPU state, or alter receiver shading.

Cloud-data requirement belongs to each LightRay population rule:

- `Ignored`: no cloud query and no evolution dependency; only unrestricted placement is valid.
- `Optional`: a genuinely absent or disabled cloud producer is clear sky, but an enabled producer that is unready or invalid suspends the rule.
- `Required`: a published, enabled, ready cloud field is mandatory.

Spatial policy is also consumer-owned:

- `Any Position`: no per-candidate transmission samples;
- `Clear Footprint`: the existing bounded 13-position by 4-time forecast contract;
- `Distinct Cloud Opening`: clear footprint plus a bounded surrounding-cloud contrast ring, with no connected-component extraction or full-cookie search.

The common sunlight contract is `Optional + Clear Footprint`, so one rule supports both cloudless skies and clear openings when clouds exist. This consumer extension does not reopen the frozen cloud generator, directional-cookie assignment, world movement, evolution, receiver compatibility, benchmark baseline, or native URP cookie sample.
