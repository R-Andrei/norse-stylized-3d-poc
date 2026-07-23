# Weather Cloud-Shadow Handoff — Vertex-Evaluated Ground Prototype

## A. Handoff identity

**Stable planning identifier:** `WEATHER-CLOUD-SHADOW-V0`

**Handoff update identifier:** `WEATHER-CLOUD-SHADOW-HANDOFF-V0`

**Created:** 2026-07-23, after read-only architecture and performance review and before any cloud-shadow implementation.

**Workspace root:** `F:\Unity\Projects\Norse Stylized 3D PoC`

**Branch and reviewed revision:** `fufu`, `9d69a18c7b9ba1e42e58577a8832aeff94723cc8` (`Add river velocity and weather refinements`).

**Task type:** diagnosis, architecture selection, performance comparison, planning, and continuation handoff. No cloud-shadow runtime or shader implementation exists at the time of this handoff.

**Authoritative current-status source:** this document is the canonical plan and continuation handoff for `WEATHER-CLOUD-SHADOW-V0`. `Assets/Docs/Weather_System_Architecture_Provisional.md` remains the authoritative parent Weather document until an approved implementation update changes it. `Assets/Docs/Weather_Wind_Architecture.md` remains authoritative for the implemented wind subsystem.

**Source provenance:** the user-provided active Unity workspace at the path above was used directly. A workspace search found no `.zip`, `.7z`, `.rar`, `.tar`, `.tar.gz`, `.tgz`, or `.unitypackage` archives under the workspace root. No clone, fetch, pull, reset, checkout, restore, or remote replacement was performed.

**Terminology:**

- **Cloud shadow** means a moving stylized illumination mask rendered on the ground. It does not mean visible cloud geometry, a sky layer, volumetrics, or a cloud renderer.
- **Opening** means a sunlit region in the otherwise cloud-shaded ground mask.
- **Transmission** means the scalar amount of sunlight retained at a ground point: `1` is fully open sunlight and a lower positive value is cloud shade.
- **Vertex-evaluated** means the cloud field is calculated in the ground vertex shader and one scalar result is interpolated across ground triangles.
- **Cookie fallback** means the previously reviewed URP directional-light-cookie approach. It remains deferred unless the vertex prototype fails its visual or lighting acceptance criteria.

## B. Immediate continuation brief

The user wants broad, moving, stylized cloud shadows because the isometric top-down camera sees the ground rather than visible clouds. The accepted first experiment is a ground-only vertex-evaluated cloud mask. No visible clouds are required or desired. The smallest sunlit opening should be approximately `5–7 m` across or larger, and an initial transition softness of approximately `1.5 m` is acceptable. The mask should move at a constant speed in the Weather wind direction or within a bounded angle of it. Later Weather state may control speed, coverage, and other parameters.

The current demo ground is a `40 m` patch at `33 × 33` vertices, or `1,089` vertices with `1.25 m` grid spacing. Repository evidence is `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, `GeneratedGround.recipe.patchSize = 1` and `resolution = 1`, plus `GroundGenerator.ResolvePatchSize` and `ResolveResolution`, where enum value `1` resolves to `40 m` and `33`. This topology provides approximately four to six vertex intervals across a `5–7 m` opening. The result may be adequate because the requested shapes are broad and the transition is soft, but Unity visual validation is mandatory; the interpolation quality is not yet observed.

The next model must not implement immediately. The user authorized creation of this handoff document only. Runtime, shader, scene, metadata, and architecture changes for `WEATHER-CLOUD-SHADOW-V0` require a new explicit approval after the receiving model refreshes repository state and declares exact affected files. The highest-risk mistake is treating the selected prototype as a fragment texture or per-frame CPU field. The selected design uses no cloud texture sample and no per-frame mesh or vertex-buffer rebuild.

The recommended implementation has `O(V)` vertex field evaluation per visible ground draw, one additional interpolated scalar, and `O(P)` low-cost fragment arithmetic to apply the mask, where `V` is visible ground vertices and `P` is shaded ground fragments. At the reviewed demo scale, field generation is `1,089` vertex evaluations per ground chunk instead of one cookie sample for every visible ground fragment. Exact GPU time is unmeasured. There is no approved `PERFORMANCE EXCEPTION` in the selected prototype. The directional-cookie fallback would be a `PERFORMANCE EXCEPTION` relative to this prototype because it adds one sample per affected fragment for higher projection consistency.

**Next update:** none is authorized. After approval, the expected first implementation update is `WEATHER-CLOUD-SHADOW-V0.1`, with the exact file declaration recorded in section M.

## C. User intent, scope, and acceptance criteria

### Primary objective

Create the initial cloud-shadow presentation for the top-down isometric game without rendering clouds. The ground should read as mostly clouded while broad sun openings move across it. Clouded and sunlit regions must remain clearly distinguishable, with controlled soft boundaries rather than tiny noisy spots or hard binary edges.

### User-approved visual direction

- No visible clouds are needed. The player primarily sees the ground.
- The first approach is a vertex-evaluated ground mask.
- The smallest opening should normally be at least `5–7 m`.
- An initial softness near `1.5 m` is acceptable.
- The field should move with the Weather wind direction or a bounded nearby direction.
- Constant movement speed is acceptable for V0.
- The URP directional-light-cookie solution is a fallback only if the vertex approach does not work.

### Acceptance criteria for the first prototype

1. No cloud mesh, sky cloud layer, volumetric effect, projector, decal, fullscreen pass, runtime cloud texture, or shadow-caster cloud plane is created.
2. The normal gameplay camera shows broad moving cloud-shaded regions and broad sun openings on the ground.
3. No intentionally generated opening is smaller than the configured minimum target; the initial target range is `5–7 m`.
4. The cloud-to-sun transition is visibly softened; the initial authored target is approximately `1.5 m`.
5. The field motion is continuous and follows the authoritative Weather wind direction or an explicit deterministic angular offset from it.
6. The cloud field is evaluated in the vertex shader. The fragment stage receives one interpolated scalar and does not sample a cloud texture.
7. No per-frame CPU mesh change, vertex-buffer upload, texture rebuild, compute dispatch, managed allocation, or full-field CPU rebuild is introduced.
8. Wind strength is not silently interpreted as metres per second. `WeatherWindDomain` documents wind magnitude as dimensionless Weather strength, so V0 cloud speed remains an explicit independent metres-per-second control.
9. The effect deactivates when the sun has no useful contribution, preventing cloud shade from darkening night lighting.
10. Existing ground material composition, hydrology, detail arrays, painted accents, ordinary shadows, fog, geometry, collision, and generation remain unchanged except for the final approved cloud-light modulation.
11. The first prototype affects the Ground shader only. Rocks, water, vegetation, actors, VFX, and other materials are outside V0 unless the user separately expands scope.
12. Unity compilation, gameplay-camera visual inspection, day/night inspection, wind-direction inspection, and matched baseline/candidate profiling must complete before the patch is described as verified.

### Explicitly outside the approved handoff update

The current document-only update does not authorize changes to any C#, HLSL, shader, scene, profile, material, pipeline asset, layer, tag, prefab, generated asset, or existing architecture document. It also does not authorize implementing the fallback cookie, changing vegetation shadow policy, adding visible clouds, or changing the Weather wind model.

### Performance priority

The governing priority is:

`active-gameplay runtime compute > dirty-triggered runtime compute > memory usage >> storage space`

The selected vertex field minimizes the highest-priority cost. It moves cloud-pattern evaluation from per-fragment texture sampling to per-vertex arithmetic while retaining only inexpensive per-fragment scalar application. Exact pass thresholds are defined in sections M and N because no current baseline or candidate GPU measurement exists.

## D. Governing instructions and source-of-truth hierarchy

1. `AGENTS.md` and `Assets/AGENTS.md` are the highest-priority repository instructions. They require a complete read-only review, a persistent canonical plan before implementation, implementation strictly from that plan, and a post-implementation consistency/compliance audit. They prohibit unapproved layers, tags, components, assets, folders, dependencies, raw scene edits, and speculative scope.
2. This file, `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`, is the canonical plan for `WEATHER-CLOUD-SHADOW-V0`. Its implementation statuses must be updated before any material deviation.
3. `Assets/Docs/Weather_System_Architecture_Provisional.md` is the provisional Weather parent. Its section 12 currently lists cloud systems as explicitly undefined, and section 14 prohibits speculative cloud modules without a dedicated review. This handoff supplies that review but does not alter the parent until implementation is approved.
4. `Assets/Docs/Weather_Wind_Architecture.md` is authoritative for current wind ownership, CPU sampling, units, cadence, and performance. Cloud motion may consume its public CPU contract but must not alter wind generation in V0.
5. `Assets/Docs/Ground_Visual_Design_and_Architecture.md` governs Ground rendering, material composition, visual hierarchy, and performance. Cloud shade must remain subordinate to gameplay, characters, VFX, rivers, rocks, and lighting.
6. `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` governs vegetation lighting and currently excludes real-time shadow receiving and fragment texture samples. V0 does not edit vegetation.
7. `Assets/Docs/handoff.md` governs the structure and evidence depth of this document.
8. `Packages/manifest.json` records URP `17.5.0`; installed package source under `Library/PackageCache/com.unity.render-pipelines.universal@0c18adc4ff89` is authoritative for local URP shader behavior when the fallback cookie is considered.
9. Local Git metadata may be used for status, diff, and history. It does not override the active workspace files. No remote retrieval is authorized.
10. Direct user decisions in this task override provisional preferences in project documents where they do not violate repository invariants. The user selected the vertex prototype, `5–7 m` minimum openings, approximately `1.5 m` softness, and cookie fallback only after failure.

Conflicts are resolved in the order above. If current repository state has drifted from this handoff, stop, record the drift in this plan, and request approval for any expanded file scope.

## E. Repository and working-tree state

### Reviewed state

At the final pre-write status inspection:

```text
branch: fufu
HEAD: 9d69a18c7b9ba1e42e58577a8832aeff94723cc8
tracking: origin/fufu
local branch: ahead by 26 commits
```

The working tree contained unrelated modified River documents, River runtime/editor/compute files, and `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`. These changes pre-existed this document update and must be preserved. The exact observed paths were:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md
Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

Do not use `git reset`, `git checkout`, `git restore`, or broad cleanup commands against these paths. The scene is user-modified and must not be raw-edited.

The post-write audit observed these additional unrelated modified paths, which were not present in the initial status snapshot:

```text
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssembler.cs
Assets/Game/Rendering/PixelSurface/Editor/GeneratedMassSparseRiverbedTileAssemblyValidation.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl
```

These paths were not changed by this handoff update. Because `PixelSurfaceGroundForwardPass.hlsl` is a proposed future integration point, the receiving model must reread its complete current content, inspect its working-tree diff, and record the resulting baseline before requesting or starting runtime implementation.

### Supplied-file and archive inventory

The workspace itself is the supplied authoritative game-file set. `rg --files` with archive extensions found no supplied archives under the root. No extraction was required. No external mounted game-file set was selected. No source-provenance conflict remains known.

### Update ledger

#### `WEATHER-CLOUD-SHADOW-HANDOFF-V0`

Expected affected files declared before writing:

```text
Create:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md

Metadata/Companion:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md.meta
```

The post-write audit confirmed that the two declared handoff files are the only files created by this update. No implementation file or undeclared file was changed by this update. Unrelated working-tree drift observed during the audit is listed above and must be preserved.

### Relevant history

- `5944f56 Add time-of-day lighting setup` introduced the current time-of-day controller and lighting modifier structure.
- `68225e2 Add vegetation trample and weather trails` added the implemented wind-trail consumer and related Weather contracts.
- `9d69a18 Add river velocity and weather refinements` is the reviewed current `HEAD` and most recent relevant Weather documentation change.

## F. System and architecture explanation

### Existing producer and consumer flow

```text
TimeOfDayController
  -> rotates CelestialRig
  -> sets directional SunLight color/intensity
  -> publishes RenderSettings.sun and ambient state

WeatherWindDomain
  -> owns authoritative world-space XZ wind
  -> exposes SampleWindXZ / TrySampleWindXZ
  -> publishes GPU wind fields for existing visual consumers

Proposed WeatherCloudShadowController
  -> samples Weather wind direction at a bounded cadence
  -> integrates one world-space displacement phase per frame
  -> publishes scalar/vector shader globals only

Ground vertex shader
  -> evaluates broad cloud field at each ground world position
  -> applies wind displacement and optional sun-aware projection
  -> outputs one interpolated transmission scalar

Ground fragment shader
  -> applies bounded cloud shading to the approved lighting term
  -> retains existing material, ordinary shadow, fog, and surface logic
```

### Time-of-day ownership

`TimeOfDayController.ApplySun` calculates an artistic orbit from current hour, rotates `celestialRig`, writes `sunLight.color` and `sunLight.intensity`, and assigns `RenderSettings.sun`. Cloud V0 must consume the published sun/light state rather than own a second sun or modify the time-of-day controller.

The controller already has a global `weatherModifier` slot. That slot is suitable for later global overcast changes to sun intensity, ambient, sky, reflections, and fog. It is not a spatial cloud mask. V0 must not repurpose it for moving local openings.

### Wind ownership

`WeatherWindDomain.SampleWindXZ(Vector3)` evaluates the authoritative CPU wind function at the current simulation time. `TrySampleWindXZ` resolves the published active domain. The cloud controller should sample at the Weather field anchor when available, or at its own transform/ground anchor fallback when not. It should normalize the result for direction and use an explicit cloud speed for displacement. A near-zero wind sample should preserve the last stable direction or use an explicitly serialized fallback direction; it must not normalize zero.

The controller should avoid resampling complex wind every rendered fragment or vertex. It publishes a resolved direction/displacement global from CPU. Sampling wind at approximately the existing Weather cadence and integrating phase smoothly every frame keeps movement continuous while bounding CPU work.

### Proposed field model

The selected field is analytic and deterministic. It should combine a small number of broad, tileable or effectively non-repeating low-frequency components. The first implementation should prefer two value-noise octaves or another bounded deterministic field over many sinusoids that can form obvious stripes. Field evaluation must use world-space XZ coordinates plus a shared displacement so adjacent Ground chunks agree at their borders.

The first profile must expose or encode:

- deterministic seed;
- broad feature scale;
- secondary shape-warp scale and strength;
- cloud coverage threshold;
- minimum opening scale target;
- transition softness in metres;
- shaded transmission or shade strength;
- constant movement speed in metres per second;
- wind-direction response;
- optional bounded angular divergence;
- optional projection distance or mathematical cloud-plane height if sun-relative displacement is enabled.

No exact artistic defaults beyond the user-approved `5–7 m` minimum opening target and approximately `1.5 m` softness are accepted yet. Other default values require visual calibration and must not be presented as user-approved.

### Sun and ground position

If V0 uses sun-aware projection, each vertex may calculate the XZ point where the ray toward the directional sun reaches a mathematical horizontal projection plane:

```text
t = (projectionPlaneY - groundPositionWS.y) / max(epsilon, sunDirectionWS.y)
sampleXZ = groundPositionWS.xz + sunDirectionWS.xz * t - windDisplacementXZ
```

This plane is mathematical only. It does not create or imply visible clouds. The implementation must handle low or negative `sunDirectionWS.y` explicitly by disabling the effect or using a bounded fallback; division by a near-zero elevation is prohibited.

If visual validation shows that sun-relative projection creates excessive movement during the accelerated day cycle, the plan must be updated before switching to pure world-XZ sampling. That would be a material behavior decision, not a silent tuning change.

### Ground shading integration

The ground ForwardLit pass currently builds material albedo, stylized value shaping, `InputData`, `SurfaceData`, and `UniversalFragmentPBR`, then derives a lighting luminance ratio and final stylized color before fog. The cloud scalar should be added without changing surface generation or geometry.

The unresolved technical decision is how strictly V0 isolates direct sun:

- **Preferred correctness target:** attenuate only the main directional-light contribution while retaining ambient, local lights, emission-equivalent terms, wet highlights, and fog.
- **Lower-change visual prototype:** apply a bounded shade modulation to the final pre-fog Ground color, gated by sun availability. This is cheaper to integrate but can also reduce ambient or local-light contribution.

The receiving model must inspect the complete current URP PBR call path and Ground post-light composition before choosing. If exact direct-only isolation requires copying or replacing substantial URP lighting code, stop and request a user decision between the bounded stylized approximation and the cookie fallback. Do not silently replace `UniversalFragmentPBR` or create a project-wide lighting fork.

### Runtime and performance model

Let:

- `C` = visible Ground chunks;
- `V` = vertices per visible chunk;
- `P` = visible Ground fragments;
- `O` = analytic field operations per vertex.

The selected field cost is `O(C × V × O)` in the vertex stage plus `O(P)` interpolation/application arithmetic. In the reviewed scene, `V = 1,089`. Ten equally configured visible chunks produce `10,890` vertex evaluations. The per-fragment cloud work should be limited to scalar saturation/lerp/multiply operations and must contain no cloud texture sample or procedural noise.

CPU active cost is `O(1)` per frame for displacement integration and `O(1)` per bounded wind-sample update. Persistent memory is one component state plus shader globals; no field texture or buffer is required. No managed allocation is permitted in `Update`. Dirty-trigger work is limited to validation and parameter publication.

These costs are analytically derived from the selected architecture, not measured. GPU instruction count and frame time remain pending.

## G. File and symbol inventory

### Governing documents

| Path | Relevant section | Review depth | Current status | Next action |
|---|---|---:|---|---|
| `AGENTS.md` | Mandatory four-gate workflow; Unity and evidence rules | Read completely from supplied instructions | Unchanged | Re-read before implementation |
| `Assets/AGENTS.md` | Same repository rules within Assets | Read completely | Unchanged | Re-read before implementation |
| `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` | Complete document | Created by handoff update | Canonical plan | Update statuses before and after implementation |
| `Assets/Docs/Weather_System_Architecture_Provisional.md` | Sections 2, 11–14 | Read completely | Clouds currently undefined | Modify only after approval |
| `Assets/Docs/Weather_Wind_Architecture.md` | Ownership, CPU sampling, units, cadence, budgets | Relevant architecture and active/frozen sections inspected; large historical trail record searched | Wind authoritative | No V0 runtime edit planned |
| `Assets/Docs/Ground_Visual_Design_and_Architecture.md` | Lighting, runtime cost, Ground hierarchy | Relevant sections searched and inspected | Ground canonical | Update only if accepted decision materially changes it |
| `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` | Shadow and texture-sample policy | Relevant sections inspected | Vegetation excluded from V0 | No edit planned |
| `Assets/Docs/handoff.md` | Required A–T structure and performance ledger | Read completely | Governs this file | No edit planned |

### Existing runtime producers

| Path | Symbols | Review depth | Role and risk | Next action |
|---|---|---:|---|---|
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayController.cs` | `ApplyCurrentState`, `ApplySun`, `weatherModifier` | Read completely | Owns sun rotation/intensity and global lighting publication | Read-only dependency; do not edit in V0 |
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayProfile.cs` | `Evaluate`, checkpoints | Read completely | Supplies sun and environment states | No edit planned |
| `Assets/Game/Scripts/Environment/Lighting/LightingModifierProfile.cs` | `Apply` | Read completely | Later global overcast extension | No edit planned |
| `Assets/Game/Procedural/Weather/WeatherWindDomain.cs` | `PublishedDomain`, `FieldAnchor`, `SampleWindXZ`, `TrySampleWindXZ`, `Update` | Public lifecycle, sampling, globals, anchor, evaluation, configuration inspected | Authoritative wind producer; units are not m/s | No edit planned unless a missing public contract is proven |

### Ground producer and renderer

| Path | Symbols | Review depth | Role and risk | Next action |
|---|---|---:|---|---|
| `Assets/Game/Procedural/Ground/GeneratedGround.cs` | `GroundResolution`, `recipe`, `GridSpacing` | Relevant topology/configuration sections inspected | Scene Ground component and grid-spacing source | Read direct final version before implementation |
| `Assets/Game/Procedural/Ground/GroundGenerator.cs` | `ResolvePatchSize`, `ResolveResolution`, mesh loops | Relevant generator/topology sections inspected | Proves 40 m / 33² reviewed topology | No edit planned |
| `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader` | `ForwardLit` includes/pragmas | Forward pass and complete pass structure inspected | Integration root for Weather include | Proposed edit after approval |
| `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardTypes.hlsl` | `Varyings`, `Vert` | Read completely | Add one scalar varying and vertex field evaluation | Proposed edit after approval |
| `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl` | `ApplyGroundStylizedValueShaping`, `BuildInputData`, `Frag` | Relevant complete lighting sections inspected | Apply interpolated transmission without disrupting Ground composition | Proposed edit after approval |
| `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl` | Ground masks and material response | Relevant searches and dependencies inspected | Must remain unchanged unless direct need is proven | No planned edit |
| `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl` | material CBUFFER | Inspected | Cloud globals should not be per-material if Weather-owned | No planned edit |

### Proposed Weather implementation

| Path | Proposed symbol | Current status | Role |
|---|---|---|---|
| `Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs` | `WeatherCloudShadowController` | Does not exist | Resolve wind, integrate displacement, publish globals, lifecycle cleanup, diagnostics |
| `Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs.meta` | Unity metadata | Does not exist | Visible Meta Files companion |
| `Assets/Game/Rendering/Weather/Includes/WeatherCloudShadowField.hlsl` | analytic field and projection helpers | Does not exist | Shared vertex-evaluable field contract |
| `Assets/Game/Rendering/Weather/Includes/WeatherCloudShadowField.hlsl.meta` | Unity metadata | Does not exist | Visible Meta Files companion |

### Serialized scene and assets

`Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` contains the active `Weather` object with `WeatherWindDomain`, the `TimeOfDaySystem`, the directional `SunLight`, and the reviewed Ground recipe. It was already modified by unrelated work at handoff creation. Any future component addition must be performed through Unity with explicit scene scope; raw YAML editing is prohibited.

No cloud texture, material, compute shader, RenderTexture, renderer feature, layer, or tag is planned.

## H. Chronological history of the work

1. The user requested a cloud system for an isometric top-down game. Visible clouds were explicitly unnecessary; the desired output was moving, stylized cloud shadows on the ground, mostly clouded with sunlit openings and moderately soft boundaries.
2. Read-only review found the provisional Weather parent, the implemented Weather wind domain, the time-of-day controller, the custom Ground/Generated Mass/Vegetation/River shaders, URP assets, scene ownership, Git status, and relevant history.
3. The first recommendation was a directional-light cookie because URP natively projects a mask through the sun onto world positions and attenuates direct light. The project enables light cookies in both PC and mobile pipeline assets.
4. The user corrected an unnecessary discussion of potentially visible clouds. The requirement was narrowed explicitly: only ground-visible cloud shade matters.
5. Performance analysis identified one additional cookie sample per affected lit fragment. At `2560 × 1440`, a fullscreen ground surface represents approximately `3.69 million` fragment invocations; ultrawide resolutions scale linearly.
6. The user asked whether the sample count was excessive and requested a cheaper approach.
7. Ground topology inspection found the active demo Ground is `40 m`, `33 × 33`, and `1,089` vertices, with `1.25 m` spacing. This made vertex-evaluated coverage viable for broad forms.
8. Alternatives were compared. Fragment procedural noise was rejected because it replaces one cache-friendly sample with more per-fragment arithmetic. Fullscreen, decal/projector, and shadow-only geometry alternatives add passes, overdraw, depth reconstruction, or cascade work. CPU vertex-buffer updates were rejected because they create per-frame field rebuilds/uploads. Vertex shader evaluation was selected as the lowest practical active-runtime cost.
9. The user accepted the vertex approach, established a `5–7 m` minimum opening scale, accepted approximately `1.5 m` softness, and reserved the cookie approach as fallback only.
10. The user authorized creation of this handoff in `Assets/Docs`. No runtime implementation was authorized or performed.

## I. Completed work, with per-file deltas

### `WEATHER-CLOUD-SHADOW-HANDOFF-V0`

**Expected affected files before write**

```text
Create:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md

Metadata/Companion:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md.meta
```

**Actual affected files**

```text
Created:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md

Metadata/Companion:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md.meta
```

Reconciliation result: expected and actual task scope match exactly. Both files are new and untracked. No undeclared file was changed by this update. Additional unrelated working-tree modifications appeared during the documentation task; section E records and protects them.

**Previous state:** no dedicated cloud-shadow architecture, plan, or handoff existed. The provisional Weather parent explicitly listed cloud systems as undefined.

**Current state:** this document records the accepted vertex prototype, constraints, evidence, performance model, proposed implementation scope, validation plan, fallback criteria, and approval boundary.

**Intentional differences:** documentation now defines the selected first experiment without claiming implementation or changing the provisional parent’s current-runtime status.

**Preserved state:** all runtime code, shaders, scene data, materials, profiles, URP assets, wind behavior, time-of-day behavior, Ground generation, and unrelated user changes remain untouched.

**Performance:** storage-only Markdown and metadata. No active-gameplay, dirty-triggered runtime, CPU memory, GPU memory, draw, dispatch, shader, or build behavior changes. Exact file sizes are recorded by the final audit.

**Validation:** Markdown structure, required-section presence, task scope, metadata format, encoding, Git status, and complete final-content reread passed. Unity runtime validation is not applicable to this documentation-only update.

## J. Investigations, failed approaches, and lessons

### Directional-light cookie

The cookie remains technically valid and visually robust. Installed URP source binds the main light cookie directly and samples it from fragment world position. It applies consistent direct-light modulation across compatible shaders.

It was not selected first because it adds one texture sample per affected fragment. The count scales with rendered resolution and overdraw. At `2560 × 1440`, fullscreen ground means approximately `3.69 million` cookie sample invocations per frame; `3440 × 1440` means `4.95 million`; `5120 × 1440` means `7.37 million`. Cache reuse makes those counts different from uncached memory reads, but exact GPU cost is unmeasured.

**Fallback condition:** use the cookie only if the vertex result visibly facets, cannot preserve opening shapes in motion, cannot isolate acceptable lighting, or must shade multiple heterogeneous receiver shaders consistently.

**PERFORMANCE EXCEPTION:** selecting the cookie after vertex failure accepts higher per-fragment runtime cost for visual consistency. It requires user approval, matched profiling, `_LIGHT_COOKIES` shader-variant review, and a cross-subsystem receiver audit.

### Per-fragment procedural noise

Rejected as the default. It removes a texture sample but performs multiple noise/hash/interpolation operations per visible fragment. It scales with resolution like the cookie and is likely more arithmetic-intensive. It becomes valid only if a measured target platform is texture-bandwidth-bound and the chosen analytic field is demonstrably cheaper.

### Fullscreen low-resolution mask

Rejected. A low-resolution generation pass still needs a fullscreen composite, depth/world-position reconstruction or screen-space approximation, render-target storage, and color-buffer traffic. It also affects non-Ground pixels unless additional classification is added. It may become valid only if a later global atmosphere stack already owns an appropriate pass and receiver classification.

### Projector, decal, or transparent overlay

Rejected. These add draw calls and overdraw and risk incorrect interaction with terrain height, transparency, fog, and material ordering. They do not reduce the full-screen fragment relationship.

### Invisible shadow-casting cloud geometry

Rejected. A shadow-only plane or mesh would rasterize into directional shadow cascades, require alpha sampling in the shadow pass, consume shadow-map resolution, and introduce cascade, bias, culling, and altitude issues. It also creates a new scene renderer and geometry dependency.

### CPU-updated vertex colors or mesh data

Rejected. Although only `1,089` vertices exist in the reviewed chunk, updating mesh channels or vertex buffers every frame moves deterministic GPU work to per-frame CPU rebuild/upload work and violates the repository prohibition on unjustified per-frame full-field rebuilds.

### Key lesson

The field evaluation count is not the only cost. The selected design still applies a few operations per fragment, but expensive pattern construction moves to the much smaller vertex population. The mask must remain broad enough for interpolation; the user’s `5–7 m` minimum openings make that trade viable.

## K. Current behavior and verified state

### Verified by repository inspection

- No cloud system or cloud-shadow component currently exists.
- `Weather_System_Architecture_Provisional.md` explicitly lists cloud systems as undefined.
- `WeatherWindDomain` exposes CPU wind sampling and field-anchor access.
- Weather wind strength is dimensionless rather than metres per second.
- `TimeOfDayController` rotates the sun rig and publishes sun/ambient state.
- The demo Ground recipe uses `Standard40` and `Medium33`, resolving to `40 m`, `33 × 33`, `1,089` vertices, and `1.25 m` spacing.
- The custom Ground ForwardLit pass already carries world position and performs its final lighting and fog composition in `PixelSurfaceGroundForwardPass.hlsl`.
- No cloud implementation file was modified during the read-only design investigation.

### User-reported or user-approved

- The camera is isometric top-down and the visible presentation target is the ground.
- Openings should not be tiny; the smallest should likely be at least `5–7 m`.
- Approximately `1.5 m` smoothing is acceptable for the first test.
- The vertex approach should be attempted first.
- The directional cookie is fallback only if the vertex approach fails.

### Unverified

- Whether `33 × 33` interpolation preserves acceptable `5–7 m` opening silhouettes in motion.
- Whether `1.5 m` softness is sufficient to hide triangle interpolation at the gameplay camera.
- Exact cloud coverage, shade strength, feature scale, warp, wind speed, and angular divergence defaults.
- Whether strict direct-sun-only attenuation can be integrated without a disproportionate Ground lighting fork.
- CPU and GPU timings on PC and mobile.
- Multi-chunk continuity in a representative future scene.

## L. Remaining work and gap analysis

### Mandatory

#### `WEATHER-CLOUD-SHADOW-V0.1` — architecture record and runtime producer

**Status:** not started; awaiting explicit implementation approval.

**Desired end state:** Weather owns a controller that resolves wind direction, integrates cloud displacement, publishes bounded globals, and reports its state without allocations or field rebuilds.

**Gap:** class, metadata, parent architecture update, diagnostics, lifecycle cleanup, and scene component do not exist.

**Risk:** scene is already modified; adding a component without preserving user changes would be destructive. Scene change must occur through Unity.

#### `WEATHER-CLOUD-SHADOW-V0.2` — analytic vertex field

**Status:** not started; awaiting approval.

**Desired end state:** Ground vertices evaluate a deterministic broad field with minimum-opening and softness controls, then interpolate transmission.

**Gap:** Weather HLSL include, shader inclusion, varying, vertex evaluation, and debug mode are absent.

**Risk:** four to six grid intervals across the smallest opening may be visually marginal. Failure is determined in the gameplay camera, not from source inspection.

#### `WEATHER-CLOUD-SHADOW-V0.3` — Ground lighting application

**Status:** not started; design detail unresolved.

**Desired end state:** cloud transmission darkens the intended sun-lit Ground contribution without crushing night, ambient, local lights, wet highlights, or fog.

**Gap:** exact integration point requires a complete current URP/Ground lighting audit immediately before implementation.

**Decision request:** if strict main-light isolation requires a substantial custom PBR fork, user approval is required for either a bounded stylized final-color approximation or the cookie fallback.

#### `WEATHER-CLOUD-SHADOW-V0.4` — Unity validation and performance evidence

**Status:** pending implementation.

**Desired end state:** compiled, visually accepted, continuous under wind/time changes, and measured against the same scene without cloud shading.

**Gap:** no runtime candidate exists.

### Optional and deferred

- Apply cloud shade to rocks, water, vegetation, characters, or VFX. Outside V0.
- Drive coverage, speed, shade, or transitions from a complete Weather state. The Weather state architecture remains provisional.
- Introduce regional weather, multiple cloud layers, storms, precipitation, fog coupling, save/load, or network determinism. Outside V0.
- Implement the directional cookie. Deferred fallback only.

## M. Exact continuation procedure

### Step 1 — refresh and approval gate

1. Inventory chat attachments, workspace inputs, and archives before Git retrieval.
2. Read `AGENTS.md`, `Assets/AGENTS.md`, this handoff, `Weather_System_Architecture_Provisional.md`, relevant current Wind sections, the complete proposed implementation files and direct dependencies.
3. Run `git status --short --branch`, `git diff --name-only`, and targeted diffs. Record drift and preserve unrelated changes.
4. Confirm whether the user authorizes `WEATHER-CLOUD-SHADOW-V0.1–V0.4`.
5. Do not modify anything if approval does not name the canonical plan and exact implementation scope.

### Step 2 — `WEATHER-CLOUD-SHADOW-V0.1` plan and producer

**Expected affected files, proposed but not yet approved:**

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Docs/Weather_System_Architecture_Provisional.md

Create:
  Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs

Metadata/Companion:
  Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs.meta
```

Record reviewed evidence and active statuses in this plan before code. Implement a single Weather-owned component with serialized controls, validation, wind sampling, displacement integration, shader-global publication, cleanup, and a comprehensive report. Use existing `WeatherWindDomain.TrySampleWindXZ`; do not change the wind domain unless a proven missing contract blocks implementation.

**Performance:** `O(1)` CPU per frame, bounded wind sampling, no allocations, no textures/buffers. Validate zero GC allocations after warm-up. Failure response: disable publication and preserve current rendering.

**Intermediate pass criteria:** C# compiles; one domain publishes stable globals; zero-wind fallback is finite; enable/disable clears globals; no per-frame allocation.

Immediately after the update, report `Actually affected files`, reconcile the declaration, update this plan, and stop on discrepancies.

### Step 3 — `WEATHER-CLOUD-SHADOW-V0.2` vertex field

**Expected affected files, proposed but not yet approved:**

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardTypes.hlsl

Create:
  Assets/Game/Rendering/Weather/Includes/WeatherCloudShadowField.hlsl

Metadata/Companion:
  Assets/Game/Rendering/Weather/Includes/WeatherCloudShadowField.hlsl.meta
```

Implement deterministic world-XZ analytic evaluation in the vertex path and add one `half` transmission varying. Keep all noise out of the fragment path. Preserve existing semantics and confirm interpolator availability for the target shader model.

**Performance:** `O(V × O)` vertex arithmetic and one scalar interpolator; no texture sample, allocation, dispatch, or draw. Profile realistic visible chunk counts. If visual quality fails, do not increase Ground topology or move noise to fragments without updating the plan and approval.

**Pass criteria:** compilation succeeds; no seams between matching adjacent chunks; no NaN/Inf at low sun; feature motion is continuous; debug visualization shows openings no smaller than the configured target.

### Step 4 — `WEATHER-CLOUD-SHADOW-V0.3` Ground shading

**Expected affected files, proposed but not yet approved:**

```text
Modify:
  Assets/Docs/Weather_Cloud_Shadow_Handoff.md
  Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
```

Before editing, reread the complete final Ground shader/includes and installed URP functions reached by `UniversalFragmentPBR`. Record whether direct-only attenuation can be added locally. Apply the mask before fog and preserve debug modes. Do not duplicate or fork broad URP lighting without a documented decision.

**Performance:** `O(P)` scalar arithmetic. No cloud sample or noise is permitted in the fragment path. Exact added instruction count must be inspected from compiled shader or captured through available tooling.

**Pass criteria:** open regions match baseline sun; cloud regions darken clearly but retain readable ambient; night is unchanged; local lights remain acceptable; wet highlights and fog remain coherent.

### Step 5 — scene integration

The scene is currently modified by unrelated work. Before any scene write, declare:

```text
Modify through Unity only:
  Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity
```

Attach the approved controller to the existing `Systems/Weather` object unless the accepted architecture specifies another existing owner. Do not add a layer, tag, folder, child hierarchy, duplicate Weather domain, or new sun. Reconcile the scene diff structurally and preserve unrelated River/user changes.

### Step 6 — validation and closure

Run compilation, shader import, source consistency checks, gameplay visual checks, time-of-day checks, wind checks, and matched performance captures. Update this handoff with every result and pending item. Reread complete modified files and affected dependencies, compare final diff to scope and `HEAD`, and record every intentional difference.

If the vertex prototype fails, do not implement the cookie automatically. Record the failure evidence, update the plan, declare the fallback’s exact files and `PERFORMANCE EXCEPTION`, and request approval.

## N. Validation and evidence ledger

| ID | Procedure | Result | Proves | Does not prove | Repeat |
|---|---|---|---|---|---|
| `CSH-DOC-01` | `git status --short --branch` before handoff | Branch `fufu`, `HEAD` lineage current, unrelated River/scene modifications present | Working-tree ownership at planning time | Later state | Yes |
| `CSH-DOC-02` | Archive extension inventory under workspace | No archives found | Workspace is the only discovered source set | Inputs outside workspace unavailable to search | At next chat start |
| `CSH-DOC-03` | Source inspection of Ground recipe and enum resolution | `40 m`, `33²`, `1,089` vertices, `1.25 m` spacing | Vertex-evaluation scale model | Visual interpolation quality | If scene recipe changes |
| `CSH-DOC-04` | Inspect Weather CPU sampling and unit documentation | Sampling exists; units are dimensionless Weather strength | Direction can be consumed; speed requires separate mapping | Runtime behavior of future controller | After implementation |
| `CSH-DOC-05` | Final Markdown A–T and scope audit | Passed: all 20 ordered A–T sections are present; expected and actual scope is exactly the handoff plus its metadata; the metadata GUID is unique; no NUL bytes were found | Documentation completeness and exact scope | Runtime correctness | No |
| `CSH-UNITY-01` | Unity compile and shader import | Pending; no implementation | Nothing yet | All runtime behavior | Required |
| `CSH-VIS-01` | Gameplay camera at midday; stationary wind phase | Pending | Opening scale, softness, interpolation | Motion continuity | Required |
| `CSH-VIS-02` | Move cloud phase through at least one complete broad feature | Pending | Temporal stability and seam behavior | Performance | Required |
| `CSH-VIS-03` | Sunrise, midday, sunset, night | Pending | Sun projection bounds and night gating | All weather transitions | Required |
| `CSH-PERF-01` | Unity Profiler/GPU capture, baseline vs candidate, same camera/scene/settings, 300 warmed frames | Pending | CPU/GPU/GC delta at reviewed scale | Other platforms/scales | Required |
| `CSH-PERF-02` | Repeat with realistic maximum visible Ground chunks and target ultrawide resolution | Pending | Aggregate scaling | Mobile | Required |
| `CSH-PERF-03` | Repeat using Mobile RP configuration on target-class hardware | Pending | Mobile impact | PC | Required if mobile is a target |

Performance pass criteria must compare matched captures. Required minimum criteria are: zero recurring GC allocation attributable to the controller; no new draw call or dispatch; no cloud texture sample in the Ground fragment shader; no per-frame mesh upload; and no material frame-time regression outside the project’s accepted measurement noise. A numeric GPU threshold is not yet approved and must be established with the user or existing project performance budget before final acceptance.

## O. Constraints, invariants, and do-not-do list

- Do not render visible clouds.
- Do not add cloud geometry, a fullscreen pass, a renderer feature, projector, decal, shadow-only plane, or volumetric system in V0.
- Do not add a cloud texture sample or procedural noise to the Ground fragment shader.
- Do not rebuild or upload a cloud field, mesh, colors, or vertex data per frame.
- Do not interpret Weather strength as physical speed. Keep explicit cloud speed until Weather defines a mapping.
- Do not edit `WeatherWindDomain` merely to expose data already available through its public contract.
- Do not edit `TimeOfDayController`; consume its published sun state.
- Do not expand the effect to vegetation, rocks, water, actors, or VFX without approval and an explicit cross-subsystem audit.
- Do not increase Ground resolution to hide interpolation without a geometry-budget and performance decision.
- Do not raw-edit `VisualFrameworkDemo.unity`; use Unity and preserve current unrelated changes.
- Do not add layers, tags, folders, duplicate components, new hierarchy modules, renamed assets, or architectural dependencies without approval.
- Do not change shared shaders/includes without the required cross-subsystem impact audit.
- Do not describe source-complete work as compiled, visually accepted, performant, or complete until matching evidence exists.
- Stop and update this plan before changing the field representation, lighting isolation strategy, receiver scope, default scale/coverage, or fallback.

## P. Risks, blockers, unknowns, and decision requests

### Active blockers

- Runtime implementation lacks explicit approval. Resolution: user approves the exact proposed implementation file scope after current-state refresh.
- Ground-light isolation strategy is not finalized. Resolution: complete pre-edit URP/Ground audit and obtain a user decision if strict direct-only attenuation requires a broad lighting fork.

### Known risks

- **Interpolation faceting:** Medium probability. Evidence: only `4–6` vertex intervals span the smallest approved openings. Mitigation: broad field, `1.5 m` softness, gameplay-camera validation; fallback cookie if unacceptable.
- **Accelerated sun motion:** Medium probability. The time-of-day cycle can move rapidly. Sun-aware projection may slide shadows faster than wind. Mitigation: bound projection distance and validate sunrise/sunset; do not silently remove sun projection.
- **Chunk seams:** Low-to-medium probability. World-XZ sampling should align, but inconsistent object transforms or precision can expose boundaries. Mitigation: use world position and shared globals; validate adjacent chunks.
- **Local-light darkening:** Medium probability if final-color modulation is used. Mitigation: prefer main-light isolation; test hearth/aura lighting; request decision if correctness requires a lighting fork.
- **Shader interpolator/variant impact:** Low probability but unverified. Mitigation: inspect compiled shader and target platforms.

### Unresolved questions

- Exact cloud coverage and shaded transmission.
- Exact broad and secondary feature scales.
- Exact constant speed and divergence range.
- Whether sun-aware mathematical projection is visually preferable to pure world-XZ motion under the accelerated day cycle.
- Numeric frame-time acceptance budget.

### Assumptions currently in force

- V0 targets Ground only.
- Openings smaller than `5 m` are undesirable.
- Approximately `1.5 m` softness is a starting point, not a frozen shipped default.
- The reviewed `40 m / 33²` topology is representative enough for the first test.

### Decisions requiring user approval

- Exact implementation scope.
- Any broad Ground lighting rewrite.
- Cookie fallback and its `PERFORMANCE EXCEPTION`.
- Any receiver expansion beyond Ground.
- Any scene hierarchy, component-owner, default-value, or performance-budget decision not already explicit.

### External dependencies or unavailable evidence

No external package is required. Unity runtime compilation, visual evidence, compiled shader statistics, and GPU/CPU profiles are unavailable because implementation has not begun.

## Q. Recommended reading order for the next model

1. Read `AGENTS.md` and `Assets/AGENTS.md` completely. Extract workflow gates, approval boundaries, Unity constraints, and evidence requirements.
2. Read this document completely. Treat it as the active plan and record any drift before implementation.
3. Read `Assets/Docs/Weather_System_Architecture_Provisional.md` completely. Confirm current Weather ownership and undefined cloud status.
4. Read the current architecture and active/frozen sections of `Assets/Docs/Weather_Wind_Architecture.md`, then inspect the complete `WeatherWindDomain.cs`. Extract public sampling, units, lifecycle, and anchor behavior.
5. Read `TimeOfDayController.cs`, `TimeOfDayProfile.cs`, and `LightingModifierProfile.cs` completely. Confirm sun and global weather-lighting ownership.
6. Read `GeneratedGround.cs` topology/configuration portions and `GroundGenerator.cs` generation/resolution paths. Confirm the active scene recipe has not changed.
7. Read the complete current `SH_PixelGroundSurfaceLit.shader`, `PixelSurfaceGroundForwardTypes.hlsl`, and `PixelSurfaceGroundForwardPass.hlsl`, plus direct includes reached by the intended edit.
8. Read relevant lighting and performance sections of `Ground_Visual_Design_and_Architecture.md`.
9. If and only if considering the cookie fallback, read installed URP `LightCookieManager.cs`, `LightCookie.hlsl`, `RealtimeLights.hlsl`, all affected receiver shaders, and vegetation’s complete shadow/lighting contract.
10. Inspect `VisualFrameworkDemo.unity` only through Unity or targeted read-only YAML/source queries; do not raw-edit it.

## R. Commands and reproduction reference

Run from `F:\Unity\Projects\Norse Stylized 3D PoC`.

```powershell
git status --short --branch
git diff --name-only
git diff -- Assets/Docs/Weather_Cloud_Shadow_Handoff.md
```

These commands establish branch state, preserve unrelated changes, and isolate the handoff delta.

```powershell
rg --files -g '*.zip' -g '*.7z' -g '*.rar' -g '*.tar' -g '*.tar.gz' -g '*.tgz' -g '*.unitypackage'
```

This inventories supplied archives before any Git retrieval. Do not clone or replace the supplied workspace.

```powershell
rg -n "SampleWindXZ|TrySampleWindXZ|FieldAnchor|ApplySun|RenderSettings.sun" Assets/Game
rg -n "GroundResolution|ResolveResolution|ResolvePatchSize|PixelSurfaceGroundForward" Assets/Game
rg -n "_LIGHT_COOKIES|SampleMainLightCookie|GetMainLight" Assets/Game/Rendering Library/PackageCache/com.unity.render-pipelines.universal@0c18adc4ff89
```

These recover wind, sun, Ground topology, and fallback-cookie contracts.

Unity validation procedure after implementation:

1. Compile and confirm no Console errors or shader warnings attributable to the patch.
2. Use the normal gameplay camera at midday and inspect minimum opening size, softness, and triangle faceting.
3. Run long enough for one broad feature to cross the visible Ground; then reverse or rotate the prevailing wind and confirm continuous response.
4. Scrub sunrise, midday, sunset, and night; capture one complete relevant log or screenshot set if behavior fails.
5. Capture matched baseline/candidate CPU and GPU profiles after warm-up at ordinary and maximum visible Ground scale.
6. Inspect Frame Debugger/compiled shader evidence to confirm no new draw, dispatch, Ground fragment cloud sample, or unintended receiver.

## S. Final state matrix

| Objective | Stable ID | Expected files | Actual files | Performance | Scope | Implementation | Validation | Next action |
|---|---|---|---|---|---|---|---|---|
| Preserve design and continuation state | `WEATHER-CLOUD-SHADOW-HANDOFF-V0` | Create handoff + meta | Exact match: handoff + meta | Storage-only; no runtime cost | Approved | Complete | A–T, scope, metadata, encoding, and final-content audits passed | Request separate approval for V0.1 |
| Weather controller and architecture | `WEATHER-CLOUD-SHADOW-V0.1` | Exact files in M.2 | None | `O(1)` CPU/frame; unmeasured | Awaiting approval | Not started | Pending | Request approval |
| Vertex field | `WEATHER-CLOUD-SHADOW-V0.2` | Exact files in M.3 | None | `O(C×V×O)` vertex; one varying | Awaiting approval | Not started | Pending | Implement only after V0.1 |
| Ground lighting application | `WEATHER-CLOUD-SHADOW-V0.3` | Exact files in M.4 | None | `O(P)` scalar ALU; no cloud sample | Awaiting approval; lighting decision unresolved | Not started | Pending | Audit direct-light integration |
| Scene integration | `WEATHER-CLOUD-SHADOW-V0.3S` | Demo scene only, through Unity | None | One component; no draw/dispatch | Awaiting approval | Not started | Pending | Preserve user scene diff |
| Unity/performance closure | `WEATHER-CLOUD-SHADOW-V0.4` | Plan plus any approved evidence outputs | None | Must be measured | Awaiting implementation | Not started | Pending | Run ledger N |
| Cookie fallback | `WEATHER-CLOUD-SHADOW-FALLBACK-C1` | Undeclared until failure | None | `O(P)` texture sample; `PERFORMANCE EXCEPTION` | Not approved | Deferred | Not applicable | Use only after recorded vertex failure |

## T. Receiving-model startup checklist

1. Read `AGENTS.md`, `Assets/AGENTS.md`, this handoff, and the named canonical Weather/Ground sources.
2. Inventory chat attachments, mounted inputs, workspace files, and archives before Git retrieval.
3. Treat the supplied workspace as authoritative; preserve it and do not clone, pull, reset, checkout, restore, or replace it.
4. Confirm branch, `HEAD`, status, diffs, and relevant file contents; record drift from this handoff.
5. Preserve all unrelated River and scene changes.
6. Confirm authorization. Before any write, announce the stable update identifier and exact expected affected files.
7. Recheck the selected design’s local and aggregate performance model and any unresolved lighting decision.
8. Implement only the approved plan item, reconcile exact actual files immediately, validate it, and update this plan before continuing.

## Final completeness audit

- A–T sections: present.
- User objective, accepted scale/softness, fallback rule, and no-visible-cloud requirement: recorded.
- Current authorization: document-only; runtime implementation awaiting approval.
- Expected affected files: recorded for the completed handoff update and each proposed implementation update.
- Pre-existing working-tree changes: recorded and protected.
- Source provenance and archive inventory: recorded; no clone or replacement used.
- Architecture producers, consumers, dependencies, and proposed files: inventoried.
- Selected and rejected solutions: recorded with local and aggregate performance models.
- `PERFORMANCE EXCEPTION`: cookie fallback explicitly identified and unapproved.
- Validation: source evidence separated from pending Unity, visual, and performance evidence.
- Risks, blockers, unknowns, reading order, commands, state matrix, and startup checklist: present.
- Post-write actual-file reconciliation, file-size inspection, metadata validation, encoding checks, full final-content reread, and final scope audit: completed. The exact two-file task scope matches the declaration; unrelated working-tree drift remains untouched and recorded.
