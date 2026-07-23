# Weather Cloud-Shadow Architecture and Continuation Handoff

## A. Handoff identity

**Stable architecture identifier:** `WEATHER-CLOUD-SHADOW-V0`

**Documentation update identifier:** `WEATHER-CLOUD-SHADOW-DOC-V0.2`

**Architecture locked:** 2026-07-23. Universal gameplay-world coverage remains mandatory, and the user selected the URP main directional-light cookie as the first complete implementation. The hybrid optimized receiver system is deferred unless measured cookie cost is unacceptable.

**Current implementation status:** `WEATHER-CLOUD-SHADOW-V0.1` receiver-audit source patch prepared against the supplied `Assets-Code-Archive(16).zip`. The audit utility is editor-triggered only and does not implement visible cloud shading. Unity execution against the live project remains pending.

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

---

## B. Immediate continuation brief

Universal receiver coverage remains mandatory: Ground, Bank, Riverbed, Generated Mass, Vegetation, River, actors, buildings, future houses, roads, walls, snow, ice, and other sun-responsive world materials must share one coherent moving cloud boundary.

The **selected first complete implementation is a URP main directional-light cookie on the authoritative sun**. This decision is now locked for `WEATHER-CLOUD-SHADOW-V0` because it provides the highest immediate visual consistency, naturally attenuates the main directional light, handles coarse and low-poly geometry without interpolation faceting, and gives standard URP Lit and compatible Shader Graph materials a durable future-content path.

Performance remains a hard acceptance concern, but it will be measured after the complete cookie implementation exists. The cookie is not pre-approved as permanently cheap: matched baseline/candidate profiling at the target camera, resolution, dense Vegetation load, River load, and realistic whole-scene coverage is mandatory. If measured cost is unacceptable, the previously investigated hybrid shared-mask system becomes the explicit fallback workstream. V0 must not pre-implement or partially mix that fallback.

The field remains Weather-owned and must move with authoritative Weather wind direction or a deterministic bounded angular offset. No visible cloud mesh, cloud plane, volumetric system, projector, decal, fullscreen pass, per-object cloud state, or custom vertex cloud field is part of the selected V0.

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

### Visual field requirements

- Openings are broad stylized regions, not small speckled noise.
- The analytic spectrum or authored cookie must avoid intentionally producing features below the `5–7 m` target. This is a frequency/authoring constraint, not a mathematical guarantee that every connected threshold island has a minimum diameter.
- Initial edge softness is approximately `1.5 m`, subject to gameplay-camera validation.
- Coverage, shade transmission, scale, warp, speed, and deterministic seed remain authorable Weather controls.
- The field must tile or extend without visible seams across the active gameplay region.

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

No implementation may begin until the live workspace, current diffs, active scene materials, renderer shaders, and installed URP version are reread and reconciled with this plan.

---

## E. Documentation update scope and source provenance

### Supplied sources reviewed

- `/mnt/data/Weather_Cloud_Shadow_Handoff.md` — preceding Ground-only canonical plan.
- `/mnt/data/Assets-Code-Archive(16).zip` — supplied project source snapshot.
- `Assets/AGENTS.md` — mandatory repository workflow.
- Current Weather, Ground, Generated Mass, Vegetation, River documents and the authored shaders/includes named in sections G and M.

No source implementation was edited. No Git retrieval, clone, reset, restore, scene edit, package edit, or generated-asset operation was performed.

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

WeatherCloudShadowController [planned]
  -> resolves one stable cloud travel direction from Weather wind
  -> owns cookie seed, coverage, scale, softness, transmission, speed, and phase
  -> creates or assigns one low-resolution single-channel cookie representation
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

- Use one broad, low-resolution, single-channel cookie field. Exact asset/runtime representation is selected after inspecting live Unity 6000.5 / URP 17.5 cookie APIs and import requirements.
- Pattern generation may occur in the Editor or only when authored settings become dirty. It must not regenerate every rendered frame.
- Steady-state movement changes only the supported directional-cookie transform/offset state and small controller values.
- The authoritative sun remains owned by `TimeOfDayController`; the cloud controller must not create a second sun or change sun colour, intensity, rotation, shadows, or time-of-day behavior.
- If the live URP API exposes only transform-based cookie placement, the implementation may move the directional light's otherwise lighting-irrelevant positional/offset state while preserving its rotation and the celestial-rig contract. This must be proven against the live package source before code.
- Cookie scale, repetition, and movement must be world-coherent and must not swim independently per receiver.

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

| Path | Relevant symbols | Current role | Planned cloud action |
|---|---|---|---|
| `Assets/Game/Procedural/Weather/WeatherWindDomain.cs` | `SampleWindXZ`, `TrySampleWindXZ`, `FieldAnchor` | Authoritative wind | Read-only dependency unless a proven missing contract exists |
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayController.cs` | `ApplySun`, `RenderSettings.sun` | Sun ownership | Read-only dependency |
| `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs` | Weather-owned unlit trails | Exempt receiver | No cloud shading while intentionally unlit |

### Current authored visible shader families

| Feature | Shader/include files | Receiver status before implementation |
|---|---|---|
| Generated Ground | `SH_PixelGroundSurfaceLit.shader`, `PixelSurfaceGroundForwardTypes.hlsl`, `PixelSurfaceGroundForwardPass.hlsl` | Required receiver |
| Generated Mass / Pixel Surface | `SH_PixelSurfaceLit.shader`, `PixelSurfaceForwardTypes.hlsl`, `PixelSurfaceForwardPass.hlsl` | Required receiver |
| Vegetation | `SH_StylizedVegetationBenchmark.shader`, `VegetationLighting.hlsl` | Required receiver; highest vertex-cost risk |
| River | `SH_CleanStylizedRiver.shader`, `RiverWaterLighting.hlsl` | Required transparent receiver |
| Shader Graph Pixel Surface | `SG_PixelSurfaceLit.shadergraph`, `SGS_PixelSurfaceCore.shadersubgraph` | Must be inventoried for active usage and either integrated, migrated, or explicitly retired |
| Weather wind trails | `SH_WeatherWindTrails.shader` | Explicitly exempt while unlit |

### Receiver families not proven by the archive

The supplied archive does not prove the complete active scene material set for actors, buildings, future houses, wood, thatch, snow, particles, decals, or standard URP Lit materials. The live pre-implementation audit must enumerate them. The absence of a custom shader file from this archive is not evidence that the receiver does not exist.

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

## I. Completed documentation work

### `WEATHER-CLOUD-SHADOW-DOC-V0.1`

The documentation patch:

- changes cloud shadow from a Ground-only effect to a universal Weather lighting contract;
- defines mandatory and exempt receiver classes;
- records future-house and future-shader compatibility requirements;
- introduces an explicit receiver-compliance audit requirement;
- separates Weather field ownership from receiver-local lighting integration;
- records the vertex and cookie candidates without silently selecting a mixed production path;
- adds system-specific invariants to Ground, Generated Mass, Vegetation, and River canonical documents;
- records Vegetation as the primary aggregate performance risk;
- leaves every runtime and shader implementation pending.

### `WEATHER-CLOUD-SHADOW-DOC-V0.2`

This decision-lock patch selects the directional-light cookie as V0, removes the unresolved vertex-versus-cookie gate, moves hybrid optimization behind measured failure, updates every receiver document to the cookie contract, and rewrites continuation phases around complete cookie implementation followed by benchmarking.

No Unity validation is required for this Markdown-only patch. Markdown scope, cross-document terminology, exact changed-file declaration, and contradiction checks are required and completed in section N.

---

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

### Verified from supplied sources

- No cloud-shadow runtime implementation exists in the supplied archive.
- `WeatherWindDomain` exposes CPU sampling through `SampleWindXZ` and `TrySampleWindXZ`.
- `TimeOfDayController` publishes the active sun through `RenderSettings.sun`.
- Current handwritten receiver families exist for Ground, Pixel Surface/Generated Mass, Vegetation, and River.
- A Lit Pixel Surface Shader Graph and subgraph also exist and require active-usage audit.
- Current Vegetation lighting separates ambient, sun, and local-light terms.
- Current River lighting separates ambient, sun, unshadowed sun, local lights, and main-shadow attenuation.
- Current Ground and Pixel Surface forward passes call `UniversalFragmentPBR`; their exact cookie variant support must be confirmed in the live shaders and URP 17.5 path.

### User-approved

- no visible clouds are required;
- all relevant world receivers must shade coherently;
- broad openings should be approximately `5–7 m` or larger by authored scale;
- approximately `1.5 m` softness is a valid initial test;
- movement should follow Weather wind;
- the directional-light cookie is the selected first complete implementation;
- performance must be benchmarked only after complete receiver coverage;
- the hybrid shared-mask design remains fallback only if measured cost is unacceptable.

### Unverified

- complete active scene receiver/material inventory;
- exact Shader Graph and standard URP material usage;
- current live workspace drift from the archive and prior handoff;
- directional-cookie measured cost at the highest Vegetation density and target low-end PC;
- exact URP 17.5 main-light/cookie helper signatures in the live project;
- whether every transparent and custom receiver correctly compiles and consumes the main-light cookie path;
- final defaults for coverage, transmission, scale, warp, speed, and divergence.

---

## L. Remaining work and gap analysis

### `WEATHER-CLOUD-SHADOW-V0.1` — live inventory and cookie compatibility audit

**Status:** source implemented in the supplied archive patch; live Unity execution pending.

Archive evidence recorded before implementation:

- `Assets/Settings/PC_RPAsset.asset:75` contains `m_SupportsLightCookies: 1`.
- `Assets/Settings/Mobile_RPAsset.asset:75` contains `m_SupportsLightCookies: 1`.
- `SH_PixelGroundSurfaceLit.shader` and `SH_PixelSurfaceLit.shader` call `UniversalFragmentPBR` through their forward includes but do not declare `_LIGHT_COOKIES`.
- `SH_CleanStylizedRiver.shader` calls the cookie-aware three-argument `GetMainLight` through `RiverWaterLighting.hlsl` but does not declare `_LIGHT_COOKIES`.
- `SH_StylizedVegetationBenchmark.shader` uses custom main-light evaluation through zero-argument `GetMainLight`, and does not declare or sample `_LIGHT_COOKIES`.
- `SH_WeatherWindTrails.shader` is the documented intentionally unlit Weather exemption.

Required end state:

- current workspace, packages, render-pipeline assets, scenes, and diffs reconciled;
- active sun and cookie support confirmed in PC and Mobile URP assets;
- every active gameplay-world renderer/material/shader classified as cookie-supported, exempt, or unsupported;
- exact custom shader edits and exact cookie texture/controller files declared before code;
- copyable receiver-compliance report designed.

### `WEATHER-CLOUD-SHADOW-V0.2` — complete directional-cookie implementation

**Status:** approved architecture; implementation not started.

Required end state:

- one Weather-owned controller assigns, animates, disables, and restores the authoritative sun cookie;
- broad deterministic mask controls satisfy scale, softness, coverage, shade, speed, and wind direction requirements;
- standard URP Lit, compatible Shader Graph, Ground, Generated Mass, Vegetation, River, actors, buildings, and other active receivers consume the cookie exactly once;
- no custom cloud field or hybrid optimization exists;
- lifecycle, night gating, scene integration, and diagnostics are complete.

### `WEATHER-CLOUD-SHADOW-V0.3` — visual closure and receiver repair

**Status:** blocked by V0.2.

Required end state:

- visual continuity across opaque, transparent, animated, instanced, and Shader Graph receivers;
- unsupported active shaders corrected or explicitly exempted;
- no direct-light double attenuation;
- future-content cookie compatibility rule documented and auditable.

### `WEATHER-CLOUD-SHADOW-V0.4` — performance decision

**Status:** pending complete implementation.

Required end state:

- matched no-cookie/cookie profiling at target resolution and realistic maximum scene load;
- zero recurring GC and no unexpected draw/dispatch or per-renderer CPU work;
- cookie retained if measured cost is acceptable;
- hybrid fallback reopened only if measured cost is materially unacceptable, with a separate approved plan.

---

## M. Exact continuation procedure and proposed implementation scope

### `WEATHER-CLOUD-SHADOW-V0.1` exact patch declaration

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
  Assets/Docs/Ground_Visual_Design_and_Architecture.md
  Assets/Docs/Generated_Mass_Framework.md
  Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
  Assets/Docs/River_Rendering_Roadmap.md

Create:
  Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowReceiverAudit.cs
  Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowReceiverAudit.cs.meta
```

No runtime controller, shader, scene, material, texture, compute, Shader Graph, layer, or tag is modified by V0.1. The editor utility performs an on-demand loaded-scene audit, verifies all discovered URP pipeline assets, logs the complete report, and copies it to the clipboard.

### Step 1 — live-state and URP cookie audit

1. Read repository instructions, this handoff, parent Weather/Wind documents, receiver documents, active shaders, active scene materials, and installed Unity 6000.5 / URP 17.5 cookie source.
2. Inspect live branch, `HEAD`, status, diffs, scene ownership, PC/Mobile RP assets, and current sun configuration without replacing supplied work.
3. Confirm how directional-cookie texture, size, offset/transform, variants, and custom-shader sampling work in the installed package.
4. Run or implement an editor-triggered inventory of every active world renderer/material/shader and classify cookie support.
5. Update this document with the exact runtime file declaration before edits.

### Step 2 — expected core Weather files

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
  Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity  [through Unity only]

Create:
  Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
  Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs
  Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowReceiverAudit.cs

Metadata/Companion:
  corresponding Visible Meta Files for every created file

Conditional after live audit:
  one approved cookie texture/profile asset and metadata, or one dirty-triggered generation owner;
  PC_RPAsset.asset / Mobile_RPAsset.asset only if light-cookie support is not already enabled.
```

The controller owns cookie assignment, authored controls, wind-aligned movement, night gating, lifecycle restoration, and diagnostics. It must not alter Weather wind generation or time-of-day sun ownership.

### Step 3 — expected handwritten receiver verification

The live audit must verify at least:

```text
Generated Ground:
  Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl

Generated Mass / Pixel Surface:
  Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl

Vegetation:
  Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
  Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl

River:
  Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
  Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterLighting.hlsl
```

Only files proven to lack cookie variant or cookie-aware main-light sampling should be edited. No vertex cloud varying, analytic cloud include, custom cloud texture sample, compute field, or simulation change is permitted.

### Step 4 — Shader Graph, standard URP, actor, building, and transparent closure

- Confirm standard URP Lit and active Lit Shader Graph receivers inherit the main-light cookie automatically under the active pipeline settings.
- Inspect every active custom actor/building/particle/transparent shader and add only the minimum cookie-compatible main-light path when required.
- Shader Graph assets must be edited through Unity only if live validation proves they do not already receive the cookie.
- The receiver audit must fail on unsupported future houses or props rather than silently accepting them.

### Step 5 — complete visual implementation before optimization

Validate one complete cookie implementation across the normal gameplay camera, broad movement cycle, wind-direction changes, zero wind, sunrise, midday, sunset, night, opaque/transparent boundaries, local lights, emission, fog, and ordinary geometric shadows. Do not add the hybrid fallback during this phase.

### Step 6 — benchmark and retain-or-reopen decision

Use matched no-cookie and cookie captures with identical scene, camera, time, resolution, quality, Vegetation density, River load, and receiver load. Record CPU main/render thread, GPU frame time, draw/dispatch count, GC, shader variants, and visual receiver coverage.

- If cost is acceptable, freeze the cookie architecture and write the concise per-feature handoff.
- If cost is materially unacceptable, record evidence and request approval for a separate hybrid fallback plan. Do not silently optimize individual receivers.

---

## N. Validation and evidence ledger

| ID | Procedure | Current result | Proves | Does not prove |
|---|---|---|---|---|
| `CSH-DOC-01` | Inspect supplied handoff and archive | Passed | Ground-only plan and current source snapshot were reviewed | Live workspace state |
| `CSH-DOC-02` | Enumerate authored shaders/graphs | Passed | Current archive contains Ground, Pixel Surface, Vegetation, River, Weather trails, and Pixel Surface Shader Graph families | Complete active material usage |
| `CSH-DOC-03` | Cross-document contradiction audit | Passed for documentation patch | Receiver scope and ownership agree across six changed documents | Runtime correctness |
| `CSH-DOC-04` | Exact documentation file-scope reconciliation | Passed | Six declared Markdown files are the only patch files | Live Meta File state |
| `CSH-UNITY-01` | C# compile and shader import | Pending | Runtime source correctness | Visual/performance acceptance |
| `CSH-VIS-01` | Gameplay view across Ground, grass, rocks, River, actors, buildings | Pending | Spatial receiver continuity | Aggregate performance |
| `CSH-AUDIT-01` | Copyable receiver-compliance audit | Source implemented; live Unity execution pending | The utility can enumerate loaded-scene renderers, materials, shaders, RP assets, sun state, and source/keyword support without per-frame work | Actual receiver coverage until run in Unity |
| `CSH-PERF-01` | Matched baseline/candidate 300-frame capture | Pending | CPU/GPU/GC delta | Other scenes/platforms |
| `CSH-PERF-02` | Highest Vegetation density and realistic maximum visible receiver load | Pending | Directional-cookie aggregate fragment cost | Future content |
| `CSH-TIME-01` | Sunrise, midday, sunset, night | Pending | Sun gating and low-elevation stability | Weather transitions not tested |

### Documentation consistency audit

- The cloud handoff no longer describes Ground as the sole receiver.
- The parent Weather document no longer lists cloud-shadow ownership as completely undefined.
- Ground, Generated Mass, Vegetation, and River documents identify the authoritative directional-light cookie as an external Weather lighting input and preserve local subsystem ownership.
- The hybrid path is consistently deferred behind measured cookie performance failure.
- Runtime architecture remains unimplemented and unverified.

---

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

## P. Risks, blockers, unknowns, and decisions

### Active blockers

- **Live receiver inventory unavailable.** Impact: exact complete shader/material scope cannot yet be frozen. Resolution: run Step 1 against the live workspace and active gameplay scenes.
- **Exact live cookie compatibility scope unavailable.** Impact: custom shader and RP-asset edits cannot be frozen from the archive alone. Resolution: complete the live URP/receiver audit before implementation.

### Known risks

- **Directional-cookie fragment cost — High importance, unmeasured.** The world fills much of the isometric screen and dense transparent Vegetation can increase overdraw. Mitigation: complete the implementation, profile the highest density and target resolution, and reopen hybrid optimization only if the measured regression is material.
- **Unsupported custom materials — High importance.** Standard URP Lit is expected to use the cookie path, but handwritten and transparent shaders may omit variants or cookie-aware main-light calls. Mitigation: live receiver audit and minimum compatibility edits.
- **Direct-light divergence — Medium probability.** Ground/Pixel Surface use `UniversalFragmentPBR`, Vegetation and River use custom separated lighting. Mitigation: inspect exact URP path and compare open/cloud response across receivers.
- **Transparent coverage — Medium probability.** River and future transparent effects may differ from opaque cookie/custom behavior. Mitigation: explicit transparent receiver tests.
- **Accelerated time-of-day motion — Medium probability if sun-plane projection is used.** Initial V0 should prefer world-XZ field movement and use sun only for gating unless a separate projection decision is approved.

### Unresolved questions

- Exact active actor/building/prop/material shader families.
- Numeric CPU/GPU acceptance threshold beyond no recurring GC and no unexpected draw/dispatch.
- Final authored defaults.
- Whether future global overcast should also modulate ambient/sky independently of the moving mask.

### Decisions already made

- Universal receiver coverage is mandatory.
- UI, sky, diagnostics, and intentionally unlit/emissive effects may be exempt.
- Field ownership belongs to Weather.
- Receiver simulation/material identity remains local and unchanged.
- The directional-light cookie is the selected V0 production path.
- Hybrid optimization is deferred until complete cookie profiling proves it necessary.
- Final implementation must be auditable for future houses and materials.

---

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
| Expand universal receiver scope | `WEATHER-CLOUD-SHADOW-DOC-V0.1` | Six canonical Markdown files | Exact match | None | Complete | Markdown/scope consistency passed | Superseded by architecture lock |
| Lock directional-cookie V0 | `WEATHER-CLOUD-SHADOW-DOC-V0.2` | Same six canonical Markdown files | Exact match | None | Complete | Markdown/scope consistency required | Apply docs to live project |
| Live receiver and URP cookie audit | `WEATHER-CLOUD-SHADOW-V0.1` | Six canonical docs + audit source/meta | Six canonical docs + audit source/meta | Editor-triggered only | Source complete; Unity run pending | Static source/scope checks passed; live report pending | Run `Tools > PS3D > Weather > Run & Copy Cloud-Shadow Receiver Audit` |
| Complete cookie implementation | `WEATHER-CLOUD-SHADOW-V0.2` | Weather controller/scene/assets + proven receiver compatibility files | None | One main-light cookie sample per affected lit fragment | Approved architecture | Pending | Implement after audit |
| Visual receiver closure | `WEATHER-CLOUD-SHADOW-V0.3` | Approved fixes only | None | Same cookie path | Blocked | Pending | Validate every receiver |
| Performance retain/reopen decision | `WEATHER-CLOUD-SHADOW-V0.4` | Plan/evidence plus approved fixes | None | Must be measured | Pending | Pending | Retain cookie or request hybrid fallback |
| Non-cloud feature handoff | `WEATHER-CLOUD-SHADOW-HANDOFF-V0.5` | Concise Markdown handoff | None | Documentation only | Deferred | Pending | Write after implementation with one section per affected feature |

---

## T. Receiving-model startup checklist

1. Read repository instructions and all six documentation files changed by `WEATHER-CLOUD-SHADOW-DOC-V0.1`.
2. Treat the live supplied workspace as authoritative; inspect Git state and preserve unrelated River, Ground, Vegetation, and Generated Mass work.
3. Inventory every active gameplay-world renderer, material, shader, Shader Graph, transparent receiver, and intentional exemption.
4. Update this plan with the exact runtime file declaration and live cookie-compatibility evidence before code.
5. Announce the next stable update identifier and exact expected files, then implement only that update.
6. Reconcile actual files, reread complete modified implementations and dependencies, run receiver/visual/performance validation, and update this plan before any later patch.

## V0.1 source-patch compliance note

The V0.1 implementation is deliberately limited to the audit gate. It introduces no visible effect and makes no speculative receiver edits before the live scene report exists. The next patch may edit only the controller, scene, generated cookie representation, and receiver shaders proven necessary by the copied report.

## Final completeness audit

- A–T sections are present.
- The user’s universal receiver requirement is explicit.
- Ground-only scope is superseded without erasing its decision history.
- Current and future receiver classes, exemptions, and audit requirements are explicit.
- The directional-light cookie is explicitly selected as V0.
- Hybrid optimization is explicitly deferred behind measured cookie cost.
- Directional-cookie fragment cost and custom-shader compatibility are visible validation risks.
- Exact documentation patch files are declared and reconciled.
- Proposed runtime files are separated from approved documentation files.
- No runtime implementation or Unity validation is claimed.
- The requested future concise non-cloud feature handoff is recorded as a required closure deliverable.
