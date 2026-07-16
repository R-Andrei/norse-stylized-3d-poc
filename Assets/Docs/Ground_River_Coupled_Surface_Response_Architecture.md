# Ground River-Coupled Surface Response Architecture and Implementation Plan

## Status — 2026-07-16

**Architecture is implemented and source-audited through V3S-A4B.2; Unity compilation and visual validation of A4B.2 are pending. Unity has already validated normalized Bank/Riverbed composition, explicit Riverbed substrate ownership, exact-support Riverbed hydrology, region-oriented Inspector authoring, and submerged Riverbed finish decoupling. A4B.2 transfers most Shore-added smoothness/specular response from the broad URP PBR lobe into a narrow camera-readable stylized Shore highlight, and adds a short wetness-only Riverbed-to-Bank transition using the existing corridor bank distance. It adds no debug view and does not modify River geometry, water rendering, profile schemas/assets, textures, scenes, prefabs, materials, or semantic channels.**

This document is the canonical authority for River-coupled Ground appearance. It supersedes every earlier proposal that placed River banks or riverbeds inside the generic V4 Contact / Edge Accent field.

The latest project source overrides this document when they conflict.

## Mission

Complete the static Ground appearance stack in this order:

```text
Ground Material Response
├── Ordinary Ground Response
│   ├── family palette
│   ├── macro patches
│   ├── elevation readability
│   └── static semantic responses
│
├── River-Coupled Ground Response
│   ├── Spatial Classification
│   │   ├── broad bank
│   │   ├── immediate bank
│   │   ├── waterline core
│   │   └── exact riverbed support
│   │
│   ├── Surface Composition
│   │   ├── primary Ground surface
│   │   ├── reusable Bank Surface Layer
│   │   └── reusable Riverbed Surface Layer
│   │
│   ├── Surface-Cover Response
│   │   ├── vegetation retreat
│   │   ├── snow melt / retention
│   │   ├── frost retention
│   │   └── Painted Accent retention
│   │
│   └── Independent Hydrology Modifier
│       ├── reusable wetness character
│       ├── independent metre-based Shore reach
│       ├── wet tint / darkening
│       ├── pixel-pattern softening
│       └── smoothness / specular response
│
└── Contact / Edge Accents
    ├── GeneratedMass grounding
    └── selected GroundModifier boundaries
```

The River-coupled system resolves material identity before wetness:

```text
choose substrate
→ remove or retain compatible surface cover
→ apply bank / waterline / submerged wetness
→ light the resulting material
```

A wetness-only bank is explicitly insufficient. Grass may retreat, snow may melt, and a bank may expose soil, sand, gravel, mud, rock, or any future authored layer.

River banks and riverbeds never participate in the Contact Accent field. River-coupled response is a direct material interpretation of River-owned semantic channels on geometry rendered by the Ground shader.

## Proven ownership

```text
River corridor geometry
→ owns visible bed, bank, cover, and terrain-handoff geometry

River corridor semantic channels
→ own exact spatial classification

GroundSurfaceStyleProfile or GeneratedGround local override
→ owns Ground-family appearance

SH_PixelGroundSurfaceLit
→ renders ordinary GeneratedGround and the River corridor

River water shader
→ owns refraction, absorption, optical depth, foam, motion, and water-surface appearance
```

River code does not own bank tint, bed tint, wetness, smoothness, substrate style, or Ground-family interpretation. Ground code does not infer riverbed identity from world height, water depth, centreline distance, or ambiguous low shore values. River corridor generation publishes the exact metre distance from the Riverbed Support boundary; the Ground shader interprets that semantic distance using Ground-owned authoring controls.

## Spatial contracts

### Existing Ground semantic stream

```text
Vertex Color R = tonal variation
Vertex Color G = exposure
Vertex Color B = damp/deposit
Vertex Color A = vegetation suitability
UV2.x = compaction/path
UV2.y = shore/waterline influence
UV2.z = rocky/dry
UV2.w = standing-water potential
```

`UV2.y` is a River shore and waterline signal only on explicitly River-coupled Ground-shader draws. Ordinary GeneratedGround must publish zero in this component. The River corridor publishes its precise shore signal and uses it for broad bank wetness and the narrow waterline core. It is not bed identity.

### River Corridor Material Masks

The visible River corridor render mesh exposes one corridor-owned packed stream:

```text
Unity UV channel index 3
HLSL vertex semantic TEXCOORD3

X = Riverbed Support
Y = outward distance in metres from the Riverbed Support boundary
Z = River corridor bank-domain validity
W = reserved zero
```

Ordinary GeneratedGround publishes no River-coupled UV3 stream. An attached ordinary Ground mesh that still exposes `TEXCOORD3` is treated as invalid geometry. `GeneratedGround` forces a Ground-only rebuild, resets the mesh vertex layout with `Mesh.Clear(false)` before applying ordinary Ground data, and asserts that `TEXCOORD3` is absent afterward. The River corridor asserts that `TEXCOORD3` exists after its material-mask stream is written. These layout checks validate data integrity; they do not authorize River shading. Authorization is the explicit per-renderer `OrdinaryGround` or `RiverCorridor` role written through the existing `MaterialPropertyBlock`. Bank composition is interpreted only on the dedicated River corridor draw even though every authoring control remains on the main `GeneratedGround` component.

The authoritative X mapping remains:

```text
Centre       = 1
FlatBedEdge  = 1
BedSlope     = 1
HiddenCover  = 0
OuterBlend   = 0
BuriedApron  = 0
```

The encoded Z domain begins on the final BedSlope boundary vertex and remains valid through `HiddenCover`, `OuterBlend`, and `BuriedApron`. Y is zero at that exact Riverbed Support boundary and increases outward across the corridor toward its terrain handoff. Shader interpretation multiplies Z by `(1 - Riverbed Support)`, so the shared boundary vertex provides continuous interpolation without applying Bank material inside the supported bed. These channels are semantic geometry data, not pre-shaped artistic falloffs.

## Existing shader behavior that must be refactored

The current Ground forward pass combines shore with generic damp/deposit response:

```hlsl
float groundDampVisual = saturate(
    (groundDampDeposit * 0.84 +
     groundShore * 0.34 * max(0.0, _GroundShoreDampStrength)) *
    max(0.0, _GroundDampResponse));
```

V3S must separate these responsibilities:

```text
groundDampDeposit
→ generic semantic damp/deposit response

groundShore
→ River-coupled Shore / Bank response

riverbedSupport
→ Riverbed response
```

The current global `_Wetness` value independently influences colour, pixel softening, smoothness, and specular response. V3S must resolve one bounded effective wetness and use it consistently instead of stacking a second unrelated River formula.

## Shared-shader safety

`PixelSurfaceGroundResponse.hlsl` is consumed by both:

```text
SH_PixelGroundSurfaceLit.shader
SH_PixelSurfaceLit.shader
```

Only the Ground shader carries Riverbed Support. Any shared resolver must therefore compile to zero when `PS3D_GROUND_HAS_RIVERBED_SUPPORT` is not defined. Shared include changes require both shaders to compile before a patch passes.

# Implementation sequence

## V3S-A0/A1 — Canonical architecture and Riverbed Support proof

**Purpose:** make the architecture authoritative and prove the new River semantic reaches the Ground fragment path before changing normal rendering.

### Documentation

Update:

```text
Ground_Generation_Surface_Upgrade_Plan.md
Ground_Visual_Design_and_Architecture.md
Ground_Contact_Edge_Accent_Audit_and_Architecture.md
Ground_River_Regeneration_Orchestration_Manual.md
Ground_Macro_Patch_Audit_and_Architecture.md
GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md
```

Required corrections:

- V3M Macro Patch Composition is accepted.
- V3R Elevation Readability is accepted.
- V3S River-Coupled Ground Response is active.
- V4 Contact / Edge Accents is queued after V3S.
- V4 sources are GeneratedMass and explicit GroundModifier boundaries only.
- River structural changes refresh River-owned geometry and semantic channels; they do not stale Contact coverage.

### Shader input

Add to Ground `Attributes`:

```hlsl
float4 riverMaterialMasks : TEXCOORD3;
```

Forward only the required scalar:

```hlsl
half riverbedSupport : TEXCOORD7;
```

Resolve through the shared include:

```hlsl
float ResolveGroundRiverbedSupportMask(Varyings input)
{
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
    return saturate((float)input.riverbedSupport);
#else
    return 0.0;
#endif
}
```

### Diagnostics

Add one debug view:

```text
Ground Riverbed Support = 32
```

Do not add source-isolated River debug modes or telemetry.

### Acceptance

1. Unity compiles `SH_PixelGroundSurfaceLit` and `SH_PixelSurfaceLit`.
2. Ordinary GeneratedGround is zero in `Ground Riverbed Support`.
3. Corridor Centre, FlatBedEdge, and BedSlope are one.
4. HiddenCover, OuterBlend, and BuriedApron are zero except expected interpolation across the first transition strip.
5. Existing `Ground Shore` output is unchanged.
6. Normal lit output is unchanged.

### Non-scope

No controls, tint, smoothness, substrate texture, style-asset edit, material edit, scene/prefab edit, or Contact Accent implementation.

## V3S-A2A — Reusable surface-layer library and main-Inspector authoring

**Purpose:** establish reusable secondary substrate definitions while keeping routine selection, creation, duplication, and editing inside the main `GeneratedGround` Inspector.

### Storage architecture

Add the reusable asset type:

```text
GroundSurfaceLayerProfile
```

A profile defines dry material identity and cover compatibility only:

```text
Identity
Palette
    Base Colour
    Dark Colour
    Light Colour
Surface Character
    Macro Contrast
    Pixel Contrast
    Dry Smoothness
    Dry Specular Strength
Cover Compatibility
    Vegetation Retention
    Snow Retention
    Frost Retention
    Painted Accent Retention
```

The serialized wet-colour and wet-finish fields created during A2A are retained only as hidden backward-compatible data. They are not part of the active authoring contract and are not consumed by rendering. Hydrological character belongs to the separate `GroundHydrologyModifierProfile` introduced by A3B. A surface-layer profile does not own River geometry, spatial reach, waterline thresholds, layer blend strength, runtime weather state, or world placement.

`GroundMaterialControls` stores two optional references:

```text
Bank Surface Layer
Riverbed Surface Layer
```

A null reference means `Inherit Primary Ground`. Shared style mode stores the selection in the selected `GroundSurfaceStyleProfile` variant. Local override mode stores it on the `GeneratedGround` component.

### Main Inspector contract

Normal authoring occurs at:

```text
Generated Ground
→ Surface Appearance
  → Material Controls
    → River-Coupled Ground Response — Surface Layers
```

The section provides:

```text
Bank Surface Layer dropdown
Bank Layer Settings inline editor
Riverbed Surface Layer dropdown
Riverbed Layer Settings inline editor
Create New Layer…
Duplicate Selected Layer…
```

The dropdown discovers all `GroundSurfaceLayerProfile` assets automatically. Adding a new material type requires no enum or shader branch. The inline editor edits the selected reusable asset directly and displays both selection ownership and layer-definition ownership. Shared layer changes are marked dirty, saved after a short inactivity delay, and flushed before assembly reload or editor exit.

### Initial profile library

Create:

```text
GSLP_ExposedSoil
GSLP_PaleRiverSand
GSLP_DarkRiverMud
GSLP_FineGravel
GSLP_RoundedRiverRock
GSLP_CompactedSnowSoil
```

These are deliberately distinct starting presets, not final family tuning. Existing Ground style assets remain unassigned so the patch cannot change normal rendering.

### Acceptance

- both dropdowns are available from the main `GeneratedGround` component;
- all six starter assets appear automatically;
- selecting `Inherit Primary Ground` stores a null reference;
- selected assets can be edited inline without Project-window navigation;
- Create and Duplicate assign the resulting asset immediately;
- shared/local selection ownership is explicit;
- layer-definition ownership is explicit;
- edits survive assembly reload;
- normal rendering is unchanged;
- no scene, prefab, material, or existing Ground style asset is modified.

## V3S-A2B — Bank material-composition proof

**Status:** Unity-validated as the material-state and control proof. Subsequent validation exposed that the spatial blend must be restricted to the River corridor bank domain rather than ordinary GeneratedGround; V3S-A2C.1 corrects that ownership.

**Purpose:** prove that a selected Bank Surface Layer can replace the corridor bank substrate across the existing shore field before cover retreat or wetness is added.

Ground-owned controls are authored at:

```text
Generated Ground
→ Surface Appearance
  → Material Controls
    → River-Coupled Ground Response — Bank Composition
```

Controls and compatibility defaults:

```text
Bank Material Strength       1.00
Core Bank Reach                 0.65
Immediate-Bank Exposure         0.55
Waterline Material Strength     1.00
Core Bank Transition Softness   0.55
```

The controls are disabled while Bank Surface Layer is `Inherit Primary Ground`. Null layer selection also writes `_GroundBankLayerEnabled = 0`, so existing styles and Grounds remain visually unchanged.

The Ground shader resolves three nested weights from the verified `UV2.y` contract:

```text
broad bank
→ threshold moves across low-to-mid shore support through Core Bank Reach

immediate bank
→ fixed stronger shore-support band, scaled by Immediate-Bank Exposure

waterline material peak
→ highest corridor-only shore support, scaled by Waterline Material Strength
```

The broad-bank weight contributes up to `0.65` replacement on its own; Immediate-Bank Exposure and Waterline Material Strength then add independent bounded replacement through union. The composed result is multiplied by Bank Material Strength. Therefore the immediate-bank and waterline controls remain perceptible instead of being hidden behind an already saturated broad band, Bank Material Strength remains the master zero-preservation control, and the final result cannot exceed one.

The selected profile is transported through the existing Ground/corridor `MaterialPropertyBlock` path:

```text
base / dark / light palette
macro contrast
pixel contrast
dry smoothness
dry specular strength
vegetation / snow / frost / Painted Accent retention vector reserved for A3
```

Normal rendering blends a complete dry Bank material state rather than multiplying one tint. The Bank palette receives the existing world-stable macro field plus the existing fine pixel and generated-vertex tonal signals. Dry smoothness and specular character blend toward the selected profile using the exact same Bank material weight.

Debug modes:

```text
33 — Ground Bank Material Blend
34 — Ground Bank Layer Identity
```

Mode 33 proves spatial composition as a scalar field. Mode 34 displays the selected profile base colour only where that field participates, separating a wrong layer selection/property binding from a wrong spatial blend.

The selected Riverbed Surface Layer remains authorable but is not rendered yet. Wet Colour, wet finish, cover-retention behavior, snow melt, vegetation retreat, Painted Accent suppression, and Riverbed composition remain outside A2B.

### Acceptance

- `Inherit Primary Ground` or zero Bank Material Strength preserves ordinary Ground;
- Core Bank Reach controls broad-bank participation inside the precise existing shore field;
- Immediate-Bank Exposure increases substrate replacement near water;
- Waterline Material Strength changes material identity independently from wetness;
- Core Bank Transition Softness changes only the core material handoff softness;
- Snowfield, Grassland, and Wet Mudflat can each reveal a visibly distinct selected substrate;
- ordinary Ground away from Rivers remains unchanged;
- Material-only control or profile edits do not rebuild Ground geometry, collider, River geometry, or Painted Accent coverage.

## V3S-A2C / A2C.1 / A2C.2 / A2C.3 — Expanded corridor-owned bank range

**Status:** A2C ordinary-Ground distance ownership was rejected by visual validation. A2C.1 replaces it with the corridor-owned contract and its bank-distance behavior is visually validated. A2C.2 attempted an empty-stream purge but failed visual validation. A2C.3 remains the authoritative ordinary-Ground layout-reset and fail-fast integrity invariant. A2C.4 is Unity-accepted after the user confirmed on 2026-07-16 that Bank material spilling into ordinary Ground is solved; its role gate and data-isolation behavior are the frozen baseline.

**Purpose:** preserve the precise core bank while adding a separately authored outward material zone that starts exactly where Riverbed Support ends and travels across the generated River corridor toward its terrain handoff. Dynamic waves are not analysed and River geometry is not widened.

The Inspector keeps one master control and separates the two spatial jobs:

```text
River-Coupled Ground Response — Bank Composition
  Bank Material Strength

  Core Bank
    Core Bank Reach
    Immediate-Bank Exposure
    Waterline Material Strength
    Core Bank Transition Softness

  Outer Bank Extension
    Outer Bank Extension       metres, 0–20
    Outer Bank Strength        0–1
    Outer Bank Fade            metres, 0.05–10
```

The extension distance starts at the exact `UV3.x` Riverbed Support boundary. `UV3.y` increases outward through the corridor bank and encoded `UV3.z` proves corridor-bank membership. The shader removes any remaining Riverbed Support from that domain, making the two responses complementary across the shared interpolated edge. Ordinary GeneratedGround remains zero and cannot receive Bank Surface Layer composition. The selected profile therefore begins immediately outside the bed and continues through `HiddenCover`, `OuterBlend`, and the visible part of `BuriedApron`, fading naturally before or at the corridor/terrain handoff.

`Outer Bank Extension = 0` disables only the optional metre-distance contribution. Core broad-bank, immediate-bank, and waterline weights still use `UV2.y`, but are now multiplied by the corridor bank-domain flag so they cannot create disconnected patches on ordinary Ground. Outer Bank Strength controls only the optional broader zone; Bank Material Strength remains the master multiplier.

The final Bank blend is a bounded union of:

```text
corridor-domain UV2.y core broad-bank contribution
+ corridor-domain immediate-bank contribution
+ corridor-domain waterline contribution
+ optional corridor UV3.y metre-distance contribution
```

Debug mode `35 — Ground Outer Bank Extension` isolates only the new corridor-owned outer contribution. Existing modes `33` and `34` show the final combined Bank blend and selected layer identity.

### A2C.2 rejected empty-stream purge

The rejected A2C implementation temporarily wrote a River-distance payload into ordinary GeneratedGround UV channel index `3`. A2C.2 attempted to remove it by assigning an empty UV3 stream through the private `generatedMesh` reference during reload/validation and after mesh application. Visual validation proved that this was not a reliable migration:

- the private non-serialized reference was not guaranteed to be the mesh currently attached to the `MeshFilter`;
- the ordinary mesh application still used the shared layout-preserving clear path;
- there was no post-build invariant proving that `TEXCOORD3` was absent.

A2C.2 is superseded and must not be used as the maintenance contract.

### A2C.3 authoritative ordinary-Ground layout reset

A2C.3 establishes the actual Ground invariant:

```text
inspect MeshFilter.sharedMesh
→ if the attached ordinary Ground mesh exposes TEXCOORD3, mark geometry output missing
→ force an ordinary Ground geometry rebuild

before MeshBuilder.ApplyToMesh
→ generatedMesh.Clear(false)
→ remove the retained vertex layout, not merely its values

after mesh application
→ assert that GeneratedGround has no TEXCOORD3 attribute
→ fail immediately if any future Ground path reintroduces River corridor data
```

This correction changes only `GeneratedGround`. It does not inspect, modify, rebuild, or reinterpret any River corridor mesh. The corridor UV3 contract remains unchanged.

### A2C.4 explicit renderer authorization and ordinary-Ground data isolation

**Status checklist**

- [x] Read-only source, caller, producer, consumer, shared-shader, document, and supplied-file baseline review completed.
- [x] Concrete implementation and validation plan recorded before code edits.
- [x] Explicit renderer role implemented at every Ground-profile binding call.
- [x] Ordinary-Ground River-derived metadata removed without changing structural River integration.
- [x] Complementary Ground/corridor UV3 invariants implemented.
- [x] Static/parser checks and final consistency audit completed.
- [x] Unity compilation and visual/runtime acceptance completed; the user confirmed on 2026-07-16 that the Bank spill into ordinary Ground is solved.

#### Objective

A2C.4 contains two independently justified corrections:

```text
A. Renderer authorization
   Ordinary GeneratedGround draw = OrdinaryGround
   River corridor draw           = RiverCorridor
   role gates Shore, Riverbed Support, Bank distance, and Bank domain

B. Ordinary-Ground data cleanup
   remove River influence from exposure, damp/deposit, and vegetation metadata
   write zero to ordinary Ground UV2.y
   preserve concealment geometry, corridor handoff, regeneration orchestration,
   and explicit Painted Accent River exclusion
```

The explicit role is the authoritative ownership and future-proofing contract. It is not recorded as the proven retrospective cause of the post-A2C.3 screenshot. Current Bank composition already requires a nonzero corridor domain; if the unwanted area still responds after role gating, implementation stops and investigates corridor geometry extent, renderer assignment, and handoff ownership.

#### Read-only gate evidence — 2026-07-16

- The user-supplied `Assets(66).zip` was extracted without alteration to `/mnt/data/assets66` and copied to `/mnt/data/a2c4_work` for implementation. The supplied archive contains no `.git` directory or Unity `Library`/Editor log state, so archive-local dirty-tree and Unity compilation evidence are unavailable.
- The connected GitHub repository was inspected read-only to identify the committed baseline. `R-Andrei/norse-stylized-3d-poc` branches `fufu` and `fufu-test` are identical at commit `a4f23b0dffaf3e6a08755d06877316b2ad979e45`. Relevant supplied files were compared against their `fufu-test` blobs before edits; the archive already contained substantial uncommitted Ground, River, shader, document, and GeneratedMass work.
- Before A2C.4 edits, the supplied baseline was preserved in `/mnt/data/assets66`. The active working copy is compared directly against that baseline. The exact A2C.4 scope is the two canonical documents and seven runtime/shader files listed below; every unrelated supplied file remains byte-for-byte unchanged.
- `GeneratedGround.ApplySurfaceProfileMaterialProperties(Renderer)` is called for its own renderer and at all three corridor synchronization paths. The same style transport is correct, but there is no explicit River-capability value in the supplied baseline.
- `PixelSurfaceGroundResponse.hlsl` resolves Shore directly from `materialMasks.y` and resolves Riverbed/Bank data from the corridor varying. It is shared by both Ground and generic Pixel Surface shaders, so the role-property reference must be preprocessor-protected and generic Shore behavior must remain unchanged.
- In the supplied baseline, `GroundGenerator.BuildSurfaceMetadata` applies River shore influence to ordinary-Ground exposure, damp/deposit, vegetation, and `UV2.y`. `StylizedRiverCorridorGeometry` independently publishes a precise corridor Shore signal and copies generic Ground metadata into the corridor.
- The corridor validates material-mask count before `SetUVs(3, ...)` but has no post-write attribute invariant.

#### Approved implementation files

```text
Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Docs/Ground_Generation_Surface_Upgrade_Plan.md
Docs/Ground_Visual_Design_and_Architecture.md
Docs/Ground_Contact_Edge_Accent_Audit_and_Architecture.md
Docs/River_Rendering_Roadmap.md
Docs/Ground_River_Regeneration_Orchestration_Manual.md
Game/Procedural/Ground/GeneratedGround.cs
Game/Procedural/Ground/GroundGenerator.cs
Game/Procedural/Rivers/StylizedRiver.cs
Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs
Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
```

Mandatory read/compile audit targets that require no edit unless direct access bypassing the central resolvers is found:

```text
Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaskDebug.hlsl
Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl
Game/Rendering/PixelSurface/Includes/PixelSurfaceMaskDebug.hlsl
Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader
```

#### File-by-file implementation sequence

1. Add mandatory `GroundSurfaceRenderRole` values `OrdinaryGround` and `RiverCorridor`; add `_GroundRiverCoupledEnabled` property binding; require every `ApplySurfaceProfileMaterialProperties` caller to pass a role; always write `0` or `1` to the target property block.
2. Update all three corridor binding paths to pass `RiverCorridor`; the private GeneratedGround wrapper passes `OrdinaryGround`.
3. Add the hidden shader property and Ground material CBUFFER field with default zero.
4. Add a preprocessor-safe capability resolver before Shore resolution. In the Ground shader it reads `_GroundRiverCoupledEnabled`; in generic Pixel Surface it preserves existing Shore behavior and returns zero for unavailable UV3 River resolvers.
5. Gate Shore, Riverbed Support, Bank distance, and Bank domain centrally. Existing debug and forward consumers inherit the contract without scattered edits.
6. Remove `EvaluateShoreInfluence` from ordinary-Ground metadata construction, remove its exposure/damp/vegetation contributions, write zero to ordinary Ground `UV2.y`, and retain the structural River consumers outside this visual metadata path.
7. After corridor `SetUVs(3, riverMaterialMasks)`, assert `TexCoord3` exists and has four dimensions. Retain the ordinary-Ground post-build absence invariant.
8. Reconcile the active Ground visual, Contact/Edge Accent, River rendering, and Ground/River regeneration documents so `UV2.y` is corridor-only, ordinary Ground publishes zero, and material-only refresh preserves explicit renderer roles.
9. Run the cross-subsystem shared-include audit, parser/static checks, exact scope diff, and final document/status update.

#### Invariants and non-goals

- `GeneratedGround` remains the sole Bank/Riverbed artistic authoring façade.
- No scene, prefab, material, profile, layer, tag, collider, texture, shader keyword, shader variant, renderer, draw call, or generated field is added.
- No renderer is identified by name, hierarchy, material identity, UV presence, or reference comparison.
- The role argument has no default and is always explicit.
- Generic Pixel Surface Shore semantics remain unchanged.
- Painted Accent River exclusion remains authoritative; its candidate weighting may shift slightly because ordinary-Ground dampness and vegetation no longer contain broad River bias.
- Corridor metadata intentionally inherits generic Ground state once and adds its own precise Shore signal once.
- The patch adds one per-renderer float and negligible scalar shader work; it does not promise compiler-elided River arithmetic.


#### Post-change consistency and compliance audit — 2026-07-16

- Exact comparison against the untouched user-supplied Assets snapshot reports only the thirteen approved files changed: six active documents and seven runtime/shader files. No scene, prefab, material, profile, layer, tag, collider, generated texture, or unrelated source file changed.
- Every `ApplySurfaceProfileMaterialProperties` target now resolves through an explicit role: the private GeneratedGround wrapper passes `OrdinaryGround`, and all three River synchronization paths pass `RiverCorridor`. The role argument has no default, and every property-block write publishes `_GroundRiverCoupledEnabled` explicitly.
- Direct reads of `materialMasks.y` and `riverCoupledMasks.x/y/z` remain centralized in `PixelSurfaceGroundResponse.hlsl`. Existing forward and debug paths call those resolvers, so no scattered bypass required edits.
- Ordinary-Ground `BuildSurfaceMetadata` no longer accepts River snapshots, no `EvaluateShoreInfluence` implementation remains, and `UV2.y` is written as zero. Structural `ApplyRivers` concealment remains before metadata construction, and explicit Painted Accent River rejection remains present.
- Corridor generation still publishes its precise `UV2.y` Shore signal and now rejects a post-write render mesh that lacks a four-component `TexCoord3`. The existing ordinary-Ground post-build absence invariant remains unchanged.
- `Ground_Visual_Design_and_Architecture.md`, `Ground_Contact_Edge_Accent_Audit_and_Architecture.md`, `River_Rendering_Roadmap.md`, and `Ground_River_Regeneration_Orchestration_Manual.md` now match the canonical contract: ordinary Ground writes zero Shore data, the corridor owns River channels, and material-only refresh preserves explicit renderer roles.
- Clang 17 HLSL syntax/type compilation passed for the complete changed material-property include and response include in both Ground-role and generic Pixel Surface configurations. The generic configuration compiled without declaring `_GroundRiverCoupledEnabled` or a River varying, proving the shared-include fallback is preprocessor-safe.
- All four changed C# files passed a tree-sitter C# parse with no error or missing nodes and no multiline string literals. All changed C#, HLSL, and ShaderLab files passed comment/string-aware delimiter, preprocessor-balance, trailing-whitespace, final-newline, and line-ending checks. No Unity executable or project package set is present in the supplied files, so this audit does not claim Unity compilation or runtime validation; that remains the first live-project gate.

#### A2C.4 acceptance

- Ordinary Ground modes `12` and `32–35` remain at the zero-mask background colour `(0.025, 0.025, 0.035)` with no selected-layer response.
- The corridor retains Shore, Riverbed Support, Bank blend, Bank layer identity, and Outer Bank Extension.
- Exaggerating River-coupled controls changes only the corridor draw.
- Ordinary Ground contains no `TexCoord3`; corridor render geometry contains a four-component `TexCoord3` after build.
- Ground regeneration, River regeneration, material-only refresh, assembly reload, and scene reload preserve the role isolation.
- Painted Accent River exclusion and corridor handoff remain correct.
- If a role-gated unwanted response remains, no fallback weakens the gate; corridor extent/ownership becomes the next diagnostic.

### Acceptance

- no selected Bank Surface Layer appears on ordinary GeneratedGround;
- the Bank blend starts directly at the Riverbed Support boundary with no untreated corridor strip;
- Extension zero preserves the corrected core/waterline corridor response;
- a small Extension covers local water pushout while remaining inside the corridor;
- a large Extension can fill the available corridor bank up to its terrain handoff;
- Outer Bank Strength changes only the optional broader contribution;
- Outer Bank Fade changes only the outer corridor handoff;
- an attached ordinary-Ground mesh carrying obsolete UV3 data forces a Ground-only rebuild; the rebuilt mesh has a completely reset vertex layout and must contain no `TEXCOORD3`;
- no wave analysis, generated texture, texture sample, renderer, scene, prefab, or collider change is introduced.

## V3S-A3A — Bank surface-cover retention and retreat

**Status:** Unity-validated and accepted on 2026-07-16. Its renderer isolation, four independent retention channels, debug modes `36–37`, and Material-only persistence are frozen as the A3B baseline.

### Objective

Consume the selected Bank Surface Layer's existing vegetation, snow, frost, and Painted Accent retention values in the final Ground shader while preserving the accepted A2C.4 renderer-role and corridor-domain isolation baseline. This patch changes cover compatibility only. Shore hydrology remains a separate A3B patch.

### Acceptance criteria

- all four master controls default to zero and therefore preserve the current accepted rendering exactly;
- a selected Bank Surface Layer can independently retreat vegetation, snow, frost, and Painted Accents across the existing Bank material blend;
- `Bank Material Strength = 0`, no selected Bank Surface Layer, ordinary Ground renderer role, or zero Bank domain produces full retention and zero retreat;
- raw Painted Accent coverage debug modes remain raw and unchanged;
- debug mode `36 — Ground Bank Cover Retreat` displays vegetation, snow, and frost retreat in RGB;
- debug mode `37 — Ground Bank Painted Accent Retreat` displays Painted Accent retreat as a scalar field;
- the patch adds no River edit, generated data, texture sample, geometry, renderer, draw call, collider work, per-frame CPU work, or asset assignment.

### Read-only gate evidence — 2026-07-16

The supplied `Assets(67).zip` contains no `.git` directory. Branch, `HEAD`, staged state, working-tree state, and history comparison are unavailable in this environment and remain a delivery limitation. The complete affected implementations and direct contracts were reviewed in the supplied snapshot:

- `GroundMaterialControls.cs`: Bank profile references and spatial controls exist; no cover-retreat master controls exist; `CopyFrom` owns both null-reset defaults and shared/local value copying.
- `GroundSurfaceLayerProfile.cs`: `VegetationRetention`, `SnowRetention`, `FrostRetention`, and `PaintedAccentRetention` already exist and clamp to `0–1`; no profile schema edit is required.
- `GeneratedGround.cs`: `_GroundBankLayerCoverRetention` is already packed as XYZW and sent to every ordinary/corridor property block; no shader consumer currently uses it. Debug values `32–35` and the A2C.4 `GroundSurfaceRenderRole` binding are present.
- `GeneratedGroundEditor.cs`: local and shared material controls use the same subsection methods; Bank composition is already separately grouped; the new cover controls must be exposed through both storage paths.
- `PixelSurfaceGroundResponse.hlsl`: `ResolveGroundBankCoverRetention` already resolves profile retention through `ResolveGroundBankMaterialBlend`, but has no final-render consumer. The include is shared with `SH_PixelSurfaceLit.shader`, so new Ground-material-property helpers must remain inside `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES` guards.
- `PixelSurfaceGroundForwardPass.hlsl`: vegetation and snow are local scalar visuals; frost is currently a global material strength used in pixel/profile contrast and smoothness; Painted Accent coverage is consumed directly for albedo and through `ResolveGroundPaintedAccentLinesFeature` for finish response.
- `PixelSurfaceGroundMaskDebug.hlsl`: modes `32–35` are the current River-coupled proofs; raw Painted Accent modes `28–29` must remain unchanged.
- `SH_PixelGroundSurfaceLit.shader` and `PixelSurfaceGroundMaterialProperties.hlsl`: all Ground property declarations are explicit and must receive one packed cover-retreat-strength vector.
- `StylizedRiver.cs`: all corridor property-block call sites pass `GroundSurfaceRenderRole.RiverCorridor`; no River edit is required.
- `SH_PixelSurfaceLit.shader`: includes `PixelSurfaceGroundResponse.hlsl` without Ground material properties; shared-include zero/compile safety must remain intact.

### Approved implementation files

```text
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaskDebug.hlsl  [approved read/compatibility scope; unchanged]
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
```

No River file, scene, prefab, material, style asset, starter layer asset, shared mesh builder, or generic Pixel Surface shader file is approved for modification.

### Authoring controls and defaults

```text
River-Coupled Ground Response — Surface-Cover Response
  Vegetation Retreat Strength      0–1, default 0
  Snow Melt Strength               0–1, default 0
  Frost Retreat Strength           0–1, default 0
  Painted Accent Retreat Strength  0–1, default 0
```

The four serialized controls are packed into one shader vector:

```text
_GroundBankCoverRetreatStrength.x = vegetation
_GroundBankCoverRetreatStrength.y = snow
_GroundBankCoverRetreatStrength.z = frost
_GroundBankCoverRetreatStrength.w = Painted Accent
```

### Composition contract

```hlsl
float4 spatialRetention = lerp(
    float4(1.0, 1.0, 1.0, 1.0),
    saturate(_GroundBankLayerCoverRetention),
    bankMaterialBlend);

float4 effectiveRetention = lerp(
    float4(1.0, 1.0, 1.0, 1.0),
    spatialRetention,
    saturate(_GroundBankCoverRetreatStrength));

float4 retreat = 1.0 - effectiveRetention;
```

Consumers:

```text
X multiplies the existing vegetation visual response
Y multiplies the existing snow visual response
Z multiplies the local effective frost strength
W multiplies rendered Painted Accent coverage
```

Raw Painted Accent coverage sampling and debug modes `28–29` remain unchanged. Normal rendering and finish response use retained coverage.

### File-by-file implementation sequence

1. Add four zero-default serialized controls, clamped accessors, and complete `CopyFrom` behavior in `GroundMaterialControls.cs`.
2. Add one packed shader property ID and property-block write plus debug enum values `36–37` in `GeneratedGround.cs`.
3. Bind both shared-style and local-override controls in a dedicated Inspector subsection in `GeneratedGroundEditor.cs`.
4. Declare the packed vector in the Ground shader property block and shader properties.
5. Resolve effective retention/retreat in `PixelSurfaceGroundResponse.hlsl` without weakening shared-include guards.
6. Apply the four channels to vegetation, snow, frost, and Painted Accent consumers in `PixelSurfaceGroundForwardPass.hlsl`, reusing each function's existing Bank blend to avoid extra Bank-field evaluations.
7. Add debug modes `36–37` in `PixelSurfaceGroundMaskDebug.hlsl`.
8. Run the post-change scope, line-ending, parser, contract, shared-include, and source-consistency audit; record Unity compile/visual checks as pending because Unity is unavailable here.

### Invariants and non-goals

- A2C.4 renderer authorization, ordinary-Ground UV3 absence, corridor UV3 contract, and Bank spatial math remain unchanged.
- The profile remains the owner of compatibility fractions; Ground controls only scale whether those fractions are enforced.
- No hydrology, wet colour, wet darkening, wet smoothness, shore refactor, Riverbed composition, or submerged-cover exclusion is included.
- No starter profile value or existing shared style/local scene value is modified.
- No raw Painted Accent coverage data or production bake is regenerated or altered.
- Material edits remain Material-only and use the existing property-block refresh path.

### Risks and required checks

- Frost is a global scalar in the existing shader. A3A must derive a local effective frost strength wherever that scalar affects visible Ground response; missing one consumer would make retreat inconsistent.
- Painted Accent has separate albedo and finish consumers. Both must use retained coverage while raw debug modes continue to show source coverage.
- `PixelSurfaceGroundResponse.hlsl` is shared. Generic Pixel Surface compilation must remain valid through existing preprocessor guards.
- `GeneratedGroundEditor.cs` uses CRLF in the supplied source; all other approved files use LF. Original line endings must be preserved.

### Unity validation gate

1. With all four controls at zero, compare normal rendering and modes `28–35`; they must remain unchanged.
2. Set one retreat control to one at a time and confirm only its intended response changes inside the accepted Bank blend.
3. Verify mode `36` uses RGB for vegetation/snow/frost retreat and mode `37` isolates Painted Accent retreat; ordinary Ground remains at the zero-mask background.
4. Confirm `Bank Material Strength = 0` and `Inherit Primary Ground` each restore full retention.
5. Repeat after material-only refresh, assembly reload, Ground regeneration, and River regeneration; no cover field may spill onto ordinary Ground.
6. Compile all changed C# plus Ground and generic Pixel Surface shaders; report the complete first relevant error if any check fails.

### A3A post-change source audit — 2026-07-16

**Source status:** implementation complete in the recorded ten-file scope; Unity compilation and visual validation remain pending because the supplied archive contains no Unity Editor state or `.git` metadata.

- Exact scope comparison against the preserved `/mnt/data/a3a_baseline` snapshot reports only the two canonical documents and eight approved Ground/shader files changed; no River, scene, prefab, material, style-profile, starter-layer, shared mesh-core, or unrelated file changed.
- Tree-sitter C# parsing reports no syntax-error or missing nodes in all three changed C# files. The malformed multiline-string scan passes.
- Clang 17 HLSL library-mode syntax checks pass for the changed material-property, response, mask-debug, and forward-composition includes. The forward include was checked through the end of `BuildSurfaceData`; the unchanged Unity fragment entry point was excluded because bare Clang lacks Unity's semantic/header environment. DXIL signing/validation is unavailable because `dxv` is not installed.
- The property contract is complete across C# property ID and `MaterialPropertyBlock` write, HLSL CBUFFER declaration, and hidden ShaderLab property. Debug values `36–37` are present in both C# and HLSL dispatch.
- All four serialized controls default to zero, clamp to `0–1`, participate in null reset and `CopyFrom`, appear in both local and shared-style Inspector paths, and are packed as XYZW in `_GroundBankCoverRetreatStrength`.
- Cover retention is resolved through the existing Bank material blend. Vegetation, snow, frost, and both Painted Accent composition paths consume their respective retained values. Raw Painted Accent debug modes `28–29`, A2C.4 role gating, ordinary-Ground UV3 invariants, corridor UV3 semantics, Bank spatial math, hydrology, and Riverbed rendering remain unchanged.
- No new texture sample, generated field, noise evaluation, geometry, renderer, material, keyword, shader variant, draw call, collider work, or per-frame CPU process is introduced. A3A adds one packed property vector and bounded scalar/vector arithmetic in existing material evaluation paths.

## V3S-A3B — Independent Shore hydrology modifier and legacy wetness refactor

**Status:** implementation installed; Unity compilation reached the main Inspector, where creating a Hydrology Modifier exposed an IMGUI layout-scope defect. A3B.1 below is the active corrective patch.

### V3S-A3B.1 — Inspector asset-creation GUI-scope repair

**Status:** Unity-validated and accepted on 2026-07-16. The user successfully created and selected `GHMP_NewGroundHydrologyModifier` without the prior IMGUI layout-scope exception. Duplicate and Cancel remain regression checks rather than blockers.

#### Observed failure

Creating `GHMP_NewGroundHydrologyModifier` from the main `GeneratedGround` Inspector reports:

```text
EndLayoutGroup: BeginLayoutGroup must be called first.
UnityEngine.GUI/Scope:Dispose ()
GeneratedGroundEditor.DrawHydrologyModifierSelector (...)
GeneratedGroundEditor.cs:4410
```

The current selector calls `CreateHydrologyModifierAsset`, including `EditorUtility.SaveFilePanelInProject` and `AssetDatabase.CreateAsset`, while `EditorGUILayout.HorizontalScope` and its nested `EditorGUI.DisabledScope` are still active. The existing Surface Layer selector uses the same modal-creation-inside-layout-scope pattern and is included in the correction as the directly related latent path.

#### Objective and acceptance criteria

- Asset dialogs and `AssetDatabase.CreateAsset` must run after the current Inspector GUI event and outside all active IMGUI layout/disabled scopes.
- Create and Duplicate must assign the new asset back to the exact serialized local or shared-style property that requested it.
- Local Ground assignments must preserve the existing custom-material-control state and refresh material properties.
- Shared style assignments must remain dirty/save-queued and refresh loaded Grounds that resolve the new asset.
- Canceling the save dialog must leave the selection unchanged and produce no GUI exception.
- Hydrology and Surface Layer selectors must retain their current dropdowns, inline editing, caches, defaults, asset types, and storage locations.

#### Approved corrective scope

```text
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
```

No runtime Ground code, shader, River file, scene, prefab, material, existing profile asset, folder, tag, layer, or serialized default may change.

#### Implementation sequence

1. Capture Create/Duplicate button requests inside the existing scopes without opening a dialog or creating an asset.
2. Schedule the requested asset operation through `EditorApplication.delayCall`, capturing only stable target objects, the serialized property path, and the optional source asset.
3. In the delayed callback, create/duplicate the asset, rebuild a fresh `SerializedObject`, assign the created asset to the captured property, apply it, preserve local/shared ownership behavior, invalidate discovery caches, and refresh affected loaded Grounds.
4. Apply the same correction to both Hydrology Modifier and Surface Layer selectors because they share the proven failure pattern.
5. Reread the complete final editor file, compare against the A3B package and A3A predecessor, preserve CRLF, parse the changed C# with an available parser, scan malformed multiline strings, and verify exact three-file scope.

#### Risks and non-goals

- The delayed callback must not retain a `SerializedProperty` across GUI events; only target objects and `propertyPath` may be captured.
- No general Inspector refactor or asset-workflow redesign is authorized.
- No automatic folder relocation or creation of a new Hydrology subfolder is part of this repair.
- Unity remains the authoritative validation for modal editor behavior.

#### Implementation result and post-change audit

- Both selectors now record only `createRequested` / `duplicateRequested` while their `HorizontalScope` and `DisabledScope` are active. No modal dialog or asset creation occurs inside those scopes.
- `ScheduleSurfaceLayerAssetCreation` and `ScheduleHydrologyModifierAssetCreation` capture cloned target-object arrays, the stable serialized `propertyPath`, and the optional source asset, then invoke the existing create/duplicate method through `EditorApplication.delayCall`. No `SerializedProperty` survives the GUI event.
- `AssignCreatedProfileAsset` constructs a fresh `SerializedObject`, records Undo, assigns the created asset, applies the property, preserves local `MarkGroundVisualControlsCustom` behavior, queues shared-style saves, and refreshes loaded Grounds through the existing type-specific refresh methods. Canceling returns before assignment.
- Exact diff against the installed A3B package changes only the two canonical Markdown documents and `GeneratedGroundEditor.cs`. The editor file retains CRLF line endings; both documents retain LF.
- A complete lexical C# delimiter/comment/string scan reports no mismatched delimiters, unterminated comments/strings, or malformed regular multiline string literals. Static contract checks confirm that neither selector directly invokes its modal creation method, both delayed paths capture target objects plus `propertyPath`, and local/shared ownership hooks remain present.
- No standalone C# compiler or Unity Editor is available in the package environment. Unity compilation and live modal-workflow validation are mandatory before A3B.1 can be accepted.

### V3S-A3B.2 — Wet-response calibration and Hydrology Inspector consolidation

**Status:** implemented and source-audited on 2026-07-16 in the exact approved six-file scope. Unity compilation and visual validation are pending.

#### Observed evidence

- The user confirmed that A3B.1 removed the creation-time `EndLayoutGroup` failure by creating and selecting `GHMP_NewGroundHydrologyModifier` from the main Inspector.
- Existing debug mode `38 — Ground Local Shore Wetness` shows the intended corridor Shore bands, and mode `39 — Ground Effective Wetness` shows the same field while global `_Wetness` is effectively zero. This validates renderer authorization, corridor bank distance/domain, profile enablement, spatial control transport, and the bounded local/global mask union.
- Normal rendering shows no perceptible wet response with approximately `Shore Wetness Strength = 0.509`, `Wet Darkening = 0.428`, `Smoothness Boost = 0.420`, and `Specular Boost = 0.0854`.
- `PixelSurfaceGroundResponse.hlsl` currently scales both global and local darkening by `0.18`, both global and local smoothness by `0.22`, and returns local specular as a multiplier contribution. At the observed strongest local mask (`~0.509`), these formulas yield approximately `3.9%` darkening, `+0.047` smoothness, and only a `4.3%` relative specular increase. The local profile controls therefore do not express their authored values directly.
- `GeneratedGroundEditor.DrawLocalMaterialControlGroups` and `DrawSharedMaterialControlGroups` draw the Hydrology Modifier foldout before Bank Composition, then draw the Shore Hydrology spatial foldout after Surface-Cover Response. Modifier definition and application controls are separated by unrelated Bank sections.
- The supplied source snapshot contains no `.git` metadata. Branch, `HEAD`, staged state, unstaged state, and comparison with the actual repository remain unavailable and must be re-established during integration.

#### Objective

Make the already validated local Shore wetness field visibly affect the resolved substrate while preserving the established global-weather wetness response. Present the reusable modifier and its spatial application as one coherent authoring feature.

#### Approved files

```text
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md

Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
```

No `GeneratedGround`, `GroundMaterialControls`, profile schema, shader-property declaration, mask-debug include, River file, mesh channel, scene, prefab, material, existing asset, default, texture, renderer, or shared mesh-core change is approved.

#### Response contract

Preserve the existing global-weather coefficients and apply local modifier values directly:

```hlsl
globalDarkening =
    globalWetness * globalDarkeningStrength * 0.18;
localDarkening =
    localShoreWetness * modifierDarkening;
combinedDarkening =
    BoundedUnion(globalDarkening, localDarkening);

globalSmoothnessBoost =
    globalWetness * globalSmoothnessStrength * 0.22;
localSmoothnessBoost =
    localShoreWetness * modifierSmoothnessBoost;
combinedSmoothnessBoost =
    BoundedUnion(globalSmoothnessBoost, localSmoothnessBoost);

globalSpecularMultiplier =
    1 + globalWetness * 0.025;
localSpecularBoost =
    localShoreWetness * modifierSpecularBoost;
finalSpecular = saturate(
    resolvedDrySpecular * globalSpecularMultiplier +
    localSpecularBoost);

finalAlbedo = PS3D_ApplyValuePreservingTint(
    darkenedResolvedAlbedo,
    modifierWetTintColor,
    localShoreWetness * modifierWetTintStrength);
```

Pixel-pattern softening and snow/frost melt retain their existing direct local profile semantics. Bank material reach, Bank cover retention, local Shore wetness mask generation, and effective wetness mask generation remain unchanged.

#### Inspector contract

Use one top-level foldout in this order:

```text
River-Coupled Ground Response — Surface Layers
River-Coupled Ground Response — Bank Composition
River-Coupled Ground Response — Surface-Cover Response
River-Coupled Ground Response — Shore Hydrology
    Hydrology Modifier
        selector
        Create / Duplicate
        storage owner
        inline Wetness Character settings
    Spatial Application
        Shore Wetness Strength
        Shore Wetness Reach
        Shore Wetness Fade
        Broad-Bank Saturation
        Immediate-Bank Saturation
        Waterline Saturation
Macro Patch Composition
```

The A3B.1 delayed Create/Duplicate path, shared/local ownership behavior, save queue, profile cache invalidation, and material refresh must remain unchanged.

#### Debug policy

- Add no debug view, enum value, ShaderLab property, or mask-debug branch.
- Existing modes `38–39` remain unchanged because they already prove the spatial mask and effective union.
- Debug-view removal or consolidation requires a separate audit and explicit authorization; it is not part of this response-calibration patch.

#### File-by-file implementation sequence

1. Record this plan in the three approved canonical documents before code edits.
2. Recalibrate local darkening and local smoothness in `PixelSurfaceGroundResponse.hlsl` while preserving the current global coefficients. Split local additive specular from the existing global multiplicative response.
3. Update `PixelSurfaceGroundForwardPass.hlsl` to apply the direct local tint strength and compose global multiplicative plus local additive specular after dry Bank finish.
4. Consolidate Hydrology Modifier selection/editing and spatial controls inside one Shore Hydrology foldout in both local and shared-style authoring paths. Preserve delayed asset creation and all ownership refresh behavior.
5. Perform exact-scope comparison, complete-file reread, C# parser validation, malformed multiline-string scan, HLSL syntax/shared-consumer checks, line-ending preservation, performance audit, and documentation consistency audit.

#### Acceptance criteria

- Existing modes `38–39` remain byte-for-byte unchanged and continue to show the validated spatial fields.
- The user's current modifier values produce a clearly visible wet band in normal rendering without replacing the underlying gravel or primary-Ground identity.
- Local darkening and smoothness use the modifier values directly; global wetness retains the previous `0.18`, `0.22`, and `0.025` response scales.
- Local specular is an additive absolute contribution after dry substrate resolution; global wetness remains a restrained multiplicative response.
- Bank reach and Shore wetness reach remain independent.
- Modifier authoring and spatial controls are adjacent under one Shore Hydrology foldout for both local overrides and shared styles.
- No new debug view, texture/noise sample, material property, per-frame work, mesh/renderer change, River change, or serialized asset migration is introduced.
- Unity compiles with no C# or Ground/generic Pixel Surface shader errors, and the solved A2C.4 ordinary-Ground spill does not return.

#### Risks and required checks

- Direct local darkening and smoothness are intentionally much stronger than A3B. Validate at full profile values to ensure saturation remains bounded and the surface does not become black or uniformly mirror-like.
- Additive local specular must be saturated after addition and applied equally to RGB to preserve neutral nonmetallic response.
- The shared response include is consumed by `SH_PixelSurfaceLit.shader`; all property-dependent functions must remain inside the existing Ground-material-properties guard.
- `GeneratedGroundEditor.cs` uses CRLF and must retain it. The two shader includes and all documents use LF.

#### A3B.2 post-change source audit — 2026-07-16

- Exact comparison against the preserved current-source snapshot reports only the three approved canonical documents, `GeneratedGroundEditor.cs`, `PixelSurfaceGroundResponse.hlsl`, and `PixelSurfaceGroundForwardPass.hlsl` changed. No runtime data schema, shader property, debug dispatch, River file, scene, prefab, material, existing asset, or unrelated file changed.
- The complete contract audit passes `44/44` checks. Tree-sitter parsing reports no syntax-error or missing nodes in the changed C# and HLSL files; the malformed multiline-string scan passes; CRLF/LF ownership is preserved.
- Local darkening now resolves as `localShoreWetness * modifierDarkening`; local smoothness resolves as `localShoreWetness * modifierSmoothnessBoost`; local specular is an additive absolute neutral contribution after dry substrate resolution. Global wetness retains the established `0.18`, `0.22`, and `0.025` coefficients.
- With the user's observed strongest local mask (`~0.509`) and modifier values, the corrected local contributions are approximately `0.218` darkening, `+0.214` smoothness, and `+0.043` specular before final saturation, rather than the previous approximately `0.039`, `+0.047`, and negligible absolute specular response.
- Local and shared-style Inspector paths now draw Surface Layers, Bank Composition, Surface-Cover Response, then one Shore Hydrology foldout containing modifier selection/creation/duplication, inline Wetness Character authoring, and Spatial Application. The A3B.1 delayed asset-creation and ownership-refresh paths remain intact.
- `GeneratedGround.cs`, `PixelSurfaceGroundMaskDebug.hlsl`, shader properties, and debug modes `38–39` are byte-for-byte unchanged. No debug view was added.
- No texture sample, noise evaluation, derivative, generated field, mesh channel, renderer, draw call, per-frame process, or River rebuild was introduced. Clang 17 library-mode checks pass for isolated changed HLSL response and forward-composition expressions; `dxv` and the Unity Editor are unavailable, so Unity shader compilation and visual acceptance remain authoritative.

### Objective

Add a reusable Shore hydrology modifier whose material character and spatial reach are independent from the selected Bank Surface Layer. The dry substrate may extend farther than wetness, wetness may affect inherited primary Ground with no Bank layer selected, and changing hydrology must remain Material-only. Remove Shore from the legacy generic damp/deposit and Pooled Wetness semantic paths before introducing the new local Shore wetness authority.

### Concrete authoring contract

Add the reusable asset type:

```text
GroundHydrologyModifierProfile
```

It stores only wetness character:

```text
Identity
    Display Name
Wet Colour Response
    Wet Tint Colour
    Wet Tint Strength
    Wet Darkening
Surface Finish
    Pixel Pattern Softening
    Smoothness Boost
    Specular Boost
Cover Interaction
    Snow Melt Influence
    Frost Melt Influence
```

Spatial placement remains in `GroundMaterialControls`:

```text
Shore Hydrology Modifier       null = Disabled
Shore Wetness Strength         0–1, default 0.00
Shore Wetness Reach            0–20 m, default 0.50 m
Shore Wetness Fade             0.05–10 m, default 0.25 m
Broad-Bank Saturation          0–1, default 0.45
Immediate-Bank Saturation      0–1, default 0.80
Waterline Saturation           0–1, default 1.00
```

Routine selection, creation, duplication, and inline profile editing remain under the main `GeneratedGround` Inspector. No starter hydrology asset is added; `Create New Hydrology Modifier…` creates a neutral reusable asset with the profile defaults and assigns it immediately. No scene, prefab, material, existing style asset, or existing surface-layer asset is edited.

### Spatial contract

The local Shore wetness mask uses only the accepted corridor role and River-owned semantic channels:

```hlsl
bankDomain = ResolveGroundRiverBankDomain(input);
distance = ResolveGroundRiverBankDistance(input);
shore = ResolveGroundShoreMask(input);

distanceWeight =
    (1 - smoothstep(reach, reach + fade, distance)) * bankDomain;

broadContribution = distanceWeight * broadBankSaturation;
immediateContribution =
    smoothstep(0.17, 0.30, shore) * immediateBankSaturation * bankDomain;
waterlineContribution =
    smoothstep(0.31, 0.40, shore) * waterlineSaturation * bankDomain;

localShoreWetness =
    profileEnabled * shoreWetnessStrength *
    (1 -
        (1 - broadContribution) *
        (1 - immediateContribution) *
        (1 - waterlineContribution));
```

The mask must not read or multiply `ResolveGroundBankMaterialBlend`, `_GroundBankLayerEnabled`, Bank reach, Bank extension, or Bank profile data. A 2 m gravel layer and 0.5 m wetness reach therefore produce wet gravel near the support boundary and dry gravel farther outward. The same hydrology mask can operate over inherited primary Ground when no Bank Surface Layer is selected.

### Wetness-composition contract

Use bounded union for wetness masks, pixel softening, darkening, and smoothness. A3B.2 supersedes the original symmetric hidden attenuation of local darkening/smoothness and the original multiplicative local-specular interpretation:

```hlsl
BoundedUnion(a, b) = 1 - (1 - saturate(a)) * (1 - saturate(b));

effectiveWetness =
    BoundedUnion(globalWetness, localShoreWetness);

combinedPixelSoftening = BoundedUnion(
    globalWetness * globalPixelSoftening,
    localShoreWetness * modifierPixelSoftening);

combinedDarkening = BoundedUnion(
    globalWetness * globalDarkening * 0.18,
    localShoreWetness * modifierDarkening);

combinedSmoothnessBoost = BoundedUnion(
    globalWetness * globalSmoothnessBoost * 0.22,
    localShoreWetness * modifierSmoothnessBoost);

globalSpecularMultiplier = 1 + globalWetness * 0.025;
localSpecularBoost = localShoreWetness * modifierSpecularBoost;
```

Local wet tint is applied to the already resolved dry primary/Bank surface after cover composition. Painted Accent ink remains a cover layer and is moved before the final wetness modifier so Shore and global wetness can affect the resulting covered surface consistently. The raw Painted Accent coverage field and debug modes `28–29` remain unchanged.

Local hydrology independently reduces retained snow and frost:

```hlsl
snowHydrologyRetention =
    1 - localShoreWetness * modifierSnowMeltInfluence;

frostHydrologyRetention =
    1 - localShoreWetness * modifierFrostMeltInfluence;
```

These multiply the already accepted A3A Bank cover-retention results; they do not replace or spatially couple to the Bank material blend.

### Legacy-path removal

- `groundDampVisual` uses `groundDampDeposit` only; Shore no longer contributes.
- `ResolveGroundPooledWetnessFeature` no longer accepts or weights Shore. Pooled Wetness remains an independent ordinary-ground feature driven by damp/deposit and rocky/dry semantics.
- `_GroundShoreDampStrength` and the corresponding Inspector control are retired from active transport and authoring. Their serialized C# field is retained hidden for backward compatibility with existing style assets.
- legacy wet fields in `GroundSurfaceLayerProfile` remain serialized but hidden and unused; the new hydrology profile is the sole Shore wet-character owner.

### Existing debug proof

```text
38 — Ground Local Shore Wetness
     raw independent local Shore wetness mask

39 — Ground Effective Wetness
     bounded union of global and local wetness masks
```

Mode `38` must be black on ordinary Ground and must remain independent from Bank material reach. Mode `39` may show ordinary Ground when global `_Wetness` is nonzero.

### Implementation result — 2026-07-16

- Added `GroundHydrologyModifierProfile` with wet tint, darkening, pixel softening, smoothness, specular, snow-melt, and frost-melt character. No starter modifier asset was created.
- Added main-Inspector discovery, selection, inline editing, creation, duplication, delayed saving, shared/local ownership, and independent Shore spatial controls.
- Packed modifier character and spatial values through the existing role-aware `MaterialPropertyBlock`; ordinary Ground and corridor renderers retain their accepted A2C.4 authorization roles.
- Added one shared per-fragment local Shore-wetness evaluation and one shared Bank evaluation used by colour, smoothness, and specular composition. No texture or noise evaluation was added.
- Removed Shore from generic damp/deposit and Pooled Wetness, retired `_GroundShoreDampStrength` from active Ground authoring/transport, and retained its serialized C# field only for compatibility.
- Applied bounded global/local wetness after dry primary/Bank substrate and Painted Accent cover composition. Wet smoothness and specular are applied after dry Bank finish so the Bank layer cannot overwrite hydrology.
- Added independent snow/frost hydrology retention and debug modes `38–39`; raw Painted Accent modes `28–29` remain unchanged.
- Post-change audit passed `171/171` contract checks. Tree-sitter parsed all five changed C# files with zero syntax errors. Clang 17 HLSL library-mode harnesses passed for the material-property, shared-response, debug, and forward-composition includes; the harness removes only Unity-owned aggregate-zero casts and the fragment semantic that bare Clang cannot model. `dxv` is unavailable, so Unity remains authoritative for compiled shader validation.
- The source archive contains no `.git` metadata, so branch, `HEAD`, and working-tree integration state remain unverified.

### Reviewed evidence — 2026-07-16

- `GroundMaterialControls.cs` stores Bank composition, A3A cover response, legacy global wetness, and the obsolete Shore Damp scale. It has no independent hydrology profile or metre-based wetness reach.
- `GroundSurfaceLayerProfile.cs` still exposes wet colour and finish fields even though no current renderer consumes them; this conflicts with the accepted independent-modifier ownership and requires hidden backward-compatible retirement.
- `GeneratedGround.cs` already applies one MaterialPropertyBlock to ordinary Ground and corridor renderers with explicit roles. It transports Bank profile and global wetness values but no independent Shore hydrology profile.
- `PixelSurfaceGroundResponse.hlsl` exposes role-gated Shore, corridor-bank distance, and corridor-bank domain. Those channels are sufficient for an independent metre-based wetness mask without River changes.
- `PixelSurfaceGroundForwardPass.hlsl` currently mixes Shore into `groundDampVisual`, passes Shore into Pooled Wetness, applies global wetness before Painted Accent ink, and evaluates global wetness separately in colour, pixel contrast, smoothness, and specular.
- `StylizedRiver.cs` calls `ApplySurfaceProfileMaterialProperties(..., RiverCorridor)` from all corridor binding/refresh paths. `StylizedRiverCorridorGeometry.cs` already publishes the required UV3 X/Y/Z contract. Both are read-only and outside this patch.
- `PixelSurfaceGroundResponse.hlsl` is shared by `SH_PixelGroundSurfaceLit.shader` and `SH_PixelSurfaceLit.shader`; all new property-dependent resolvers must remain inside `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`, and generic-shader fallbacks must compile without Ground material properties or River masks.
- The supplied source archive has no `.git` metadata. Branch, `HEAD`, staged state, unstaged state, and comparison with the actual repository cannot be verified here and remain an integration prerequisite.

### Approved implementation files

```text
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md

Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GroundSurfaceLayerProfile.cs
Assets/Game/Procedural/Ground/GroundHydrologyModifierProfile.cs
Assets/Game/Procedural/Ground/GroundHydrologyModifierProfile.cs.meta
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs

Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaskDebug.hlsl
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
```

No River file, shared mesh core, scene, prefab, material, existing style asset, existing surface-layer asset, generated texture, or starter modifier asset is approved.

### File-by-file implementation sequence

1. Update this canonical plan and the four related Ground authoring/lifecycle documents before implementation.
2. Add `GroundHydrologyModifierProfile` with clamped character values and hidden-compatible separation from surface-layer profiles.
3. Add the modifier reference and seven zero-compatible spatial controls to `GroundMaterialControls`, including null reset, copy, and public accessors.
4. Extend `GeneratedGroundEditor` with profile discovery, selector, inline editing, create/duplicate, delayed save, shared/local storage display, and the Shore Hydrology control foldout. Remove the legacy Shore Damp field and surface-layer hydrological fields from active authoring.
5. Extend `GeneratedGround` property IDs, public profile exposure, and MaterialPropertyBlock transport using packed character and spatial vectors. Preserve explicit renderer roles.
6. Add shared-safe HLSL resolvers for local Shore wetness, effective wetness, and bounded response contributions. Remove Shore from generic damp and Pooled Wetness.
7. Apply independent local/global wetness to pixel contrast, albedo/tint, snow/frost retention, smoothness, specular, and Painted Accent ordering.
8. Add debug modes `38–39` and matching hidden ShaderLab properties.
9. Complete scope, parser/compiler, shared-shader, line-ending, property-contract, performance, and documentation audits.

### Invariants and non-goals

- A3A Bank cover retention remains unchanged and accepted.
- Bank substrate reach and Shore wetness reach remain independent in both data and shader math.
- No River file or corridor semantic changes.
- No new texture sample, noise evaluation, generated field, geometry, renderer, material, keyword, shader variant, draw call, collider work, or per-frame CPU process.
- Hydrology changes are Material-only and refresh both ordinary Ground and corridor property blocks through existing role-aware paths.
- No scene, prefab, material, existing style asset, existing layer asset, or starter modifier asset edit.
- Riverbed composition remains A4.
- Pooled Wetness remains an independent style feature and no longer treats Shore as an input.

### Acceptance criteria

- with no selected modifier or `Shore Wetness Strength = 0`, normal rendering matches the accepted A3A baseline except for the intentional removal of the obsolete Shore contribution from generic damp/Pooled Wetness;
- a selected modifier can wet inherited primary Ground or a selected Bank substrate without requiring the other;
- changing Bank reach does not change debug mode `38`; changing Shore Wetness Reach does not change modes `33` or `35`;
- a 2 m Bank extension and 0.5 m Shore Wetness Reach visibly produce wet substrate near the support boundary and dry substrate farther outward;
- local wet tint, darkening, pixel softening, smoothness, and specular character respond independently to the modifier profile;
- local snow and frost melt multiply A3A retention without changing Bank blend;
- ordinary Ground is black in mode `38`; mode `39` equals bounded local/global union;
- global wetness remains visually compatible when no local modifier is active;
- raw Painted Accent coverage and modes `28–29` are unchanged;
- the solved Bank-spill defect does not return;
- Unity compiles the Ground and generic Pixel Surface shaders and reports no C# errors.

### Risks and required checks

- Moving wetness after Painted Accent intentionally changes global wetness interaction with ink. Validate that wet ink remains readable and does not become glossy or over-dark.
- Existing style assets serialize the retired Shore Damp scale. The field must remain hidden and harmless rather than forcing asset migration.
- Existing surface-layer assets serialize legacy wet fields. They must remain loadable without editing or requiring reserialization.
- Normal rendering evaluates local Shore wetness and Bank composition once per fragment and shares the results across colour, smoothness, and specular. Debug rendering evaluates only the selected proof path.
- `PixelSurfaceGroundResponse.hlsl` shared-shader compilation is mandatory.

### Unity validation gate

1. Confirm C# and both Ground/generic Pixel Surface shaders compile with no errors.
2. With no hydrology modifier or Shore Wetness Strength at zero, verify the accepted A3A Bank and cover behavior remains stable and the solved ordinary-Ground spill does not return.
3. Create/select one modifier, use a 2 m Bank extension and 0.5 m Shore Wetness Reach, and verify wet substrate transitions to dry substrate before the Bank layer ends.
4. Compare debug modes `33`, `35`, and `38`: Bank controls must not move mode `38`, and Shore Wetness Reach must not move Bank modes. Ordinary Ground must remain black in mode `38`.
5. Validate tint, darkening, pixel softening, smoothness/specular, snow melt, and frost melt one at a time; verify mode `39` correctly unions local wetness with nonzero global Wetness.
6. Refresh material properties and regenerate Ground once; selections and controls must persist, raw Painted Accent modes `28–29` must remain unchanged, and no River rebuild must be required.

## V3S-A4A — Riverbed dry substrate and submerged-cover exclusion

**Status:** installed and Unity-observed. Exact `Ground Riverbed Support`, dry Riverbed profile transport, and submerged-cover exclusion work. A4A is not accepted as a final composition baseline because Unity screenshots show a primary-Ground boundary strip between matching Bank and Riverbed substrates, and the Riverbed remains dry while the adjacent Bank receives Shore hydrology.

### Proven A4A behavior retained

- `Ground Riverbed Support` exactly identifies Centre, FlatBedEdge, and BedSlope on the explicitly authorized River corridor renderer.
- A selected custom Riverbed Surface Layer supplies its dry palette and finish only on support.
- Vegetation, snow, frost, and rendered Painted Accent cover are excluded by support independently from the selected dry layer and material strength.
- River geometry, UV3 X/Y/Z semantics, renderer-role authorization, ordinary-Ground UV3 exclusion, raw Painted Accent coverage, and existing debug-view names remain unchanged.

### Blocking Unity findings — 2026-07-16

1. The forward pass first lerps primary Ground toward Bank and then lerps that result toward Riverbed. When both Bank and Riverbed blends are fractional at the BedSlope/HiddenCover interpolation edge, residual primary Ground remains. For representative `Bank = 0.5` and `Riverbed = 0.5`, sequential blending retains `(1 - 0.5) × (1 - 0.5) = 0.25` primary Ground. This is the observed green transition strip.
2. `ResolveGroundLocalShoreWetness` multiplies every Shore contribution by `ResolveGroundRiverBankDomain`. That domain explicitly multiplies encoded Bank validity by `(1 - Riverbed Support)`, so exact Riverbed Support receives zero local hydrology. The observed Bank is darker than the matching Riverbed because the Bank is wet and the Riverbed is dry.
3. The current Inspector separates Surface Layers, Bank Composition, Riverbed Composition, Surface-Cover Response, and Shore Hydrology into five top-level foldouts. The Riverbed selector is distant from its strength and offers only null-as-primary/custom-profile semantics. The user approved two region-oriented foldouts and explicit Riverbed inheritance choices.

## V3S-A4A.1 — Normalized Bank/Riverbed composition and region-oriented authoring

**Status:** implemented exactly from the pre-recorded plan and source/compliance audited. Unity compilation and visual validation are pending.

### Objective

Remove residual primary Ground from the shared Bank/Riverbed transition, add explicit Riverbed substrate-source ownership, and consolidate all River Bank and Riverbed controls into two coherent top-level foldouts. Do not add hydrology behavior in A4A.1; A4B below owns Riverbed wetness.

### Approved serialized authoring

Add an explicit Riverbed substrate source:

```text
Riverbed Surface Source
    Primary Ground
    Inherit Bank Surface Layer
    Custom Riverbed Surface Layer
```

Existing serialized data must migrate without scene, prefab, style, or profile-asset edits:

- legacy null Riverbed profile resolves to `Primary Ground`;
- legacy non-null Riverbed profile resolves to `Custom Riverbed Surface Layer`;
- new controls use explicit source values after the user changes them;
- `Inherit Bank Surface Layer` resolves the currently selected Bank profile; if the Bank itself inherits primary Ground, the Riverbed also resolves to primary Ground.

`Riverbed Material Strength` remains `0–1`, default `1.00`, and applies only when the effective source resolves a secondary surface profile. Submerged-cover exclusion remains independent from source and strength.

### Normalized substrate-composition contract

Evaluate the existing raw Bank and Riverbed material blends once, then resolve shared weights:

```hlsl
rawBank = saturate(bankMaterialBlend);
rawRiverbed = saturate(riverbedMaterialBlend);
rawTotal = rawBank + rawRiverbed;
secondaryCoverage = saturate(rawTotal);
normalization = rawTotal > 0.0001
    ? secondaryCoverage / rawTotal
    : 0;

primaryWeight = 1 - secondaryCoverage;
bankWeight = rawBank * normalization;
riverbedWeight = rawRiverbed * normalization;
```

Use the same `primary / bank / riverbed` weights for:

- dry albedo;
- dry smoothness;
- dry specular strength.

Do not add a gap-fill slider, boundary-width slider, River geometry change, or new spatial semantic. When Bank and Riverbed use the same profile and both strengths are `1`, the shared edge must contain no primary-Ground colour or finish. Different profiles must transition directly Bank-to-Riverbed.

### Approved Inspector structure

```text
River-Coupled Ground Response — River Bank
    Substrate
        Bank Surface Layer
        Create / Duplicate
        collapsed inline Layer Settings

    Material Coverage
        Bank Material Strength
        Core Bank
            Bank Material Reach
            Immediate Bank Exposure
            Waterline Material Strength
            Bank Transition Softness
        Outer Bank Extension
            Extension
            Strength
            Fade

    Surface Cover
        Vegetation Retreat Strength
        Snow Melt Strength
        Frost Retreat Strength
        Painted Accent Retreat Strength

    Shore Wetness
        Shore Hydrology Modifier
        Create / Duplicate
        collapsed inline Wetness Character
        Spatial Application
            Shore Wetness Strength
            Shore Wetness Reach
            Shore Wetness Fade
            Broad Bank Saturation
            Immediate Bank Saturation
            Waterline Saturation

River-Coupled Ground Response — Riverbed
    Substrate
        Riverbed Surface Source
        Custom Riverbed Surface Layer [Custom only]
        Create / Duplicate [Custom only]
        collapsed inline Layer Settings [Custom only]
        Riverbed Material Strength [secondary source only]

    Submerged Cover
        Vegetation: Excluded
        Snow: Excluded
        Frost: Excluded
        Painted Accents: Excluded

    Wetness
        implemented by V3S-A4B below
```

The A3B.1 delayed asset-creation workflow is mandatory. No modal dialog may open inside an active IMGUI layout scope, and no `SerializedProperty` may be retained across editor callbacks.

### A4A.1 reviewed source evidence

- `PixelSurfaceGroundForwardPass.hlsl`, `ResolvePixelGroundSurfaceColor`, currently performs `albedo = lerp(albedo, bankLayerAlbedo, bankMaterialBlend)` followed by `albedo = lerp(albedo, riverbedLayerAlbedo, riverbedMaterialBlend)`. `ResolveGroundProfileSmoothness` and `BuildSurfaceData` repeat the same sequential order for finish.
- `PixelSurfaceGroundResponse.hlsl`, `ResolveGroundRiverBankDomain`, already provides the complementary support/domain semantics required for normalized composition. No River producer change is needed.
- `GroundMaterialControls.cs` already stores `bankSurfaceLayer`, `riverbedSurfaceLayer`, and `riverbedMaterialStrength`. It lacks explicit Riverbed source ownership.
- `GeneratedGround.cs`, `ApplySurfaceProfileMaterialProperties`, already resolves and transports Bank and custom Riverbed profiles through one role-aware property block.
- `GeneratedGroundEditor.cs` currently draws five separate River-coupled foldouts and disables Riverbed strength when the custom Riverbed profile is null.
- `StylizedRiverCorridorGeometry.cs` and `StylizedRiver.cs` remain read-only. Their exact UV3 contract and `RiverCorridor` property-block role are sufficient.

### A4A.1 acceptance criteria

- matching Bank and Riverbed profiles show no primary-Ground strip at their shared edge;
- different profiles transition directly without an unrelated third substrate;
- albedo, smoothness, and specular use identical normalized weights;
- `Primary Ground`, `Inherit Bank Surface Layer`, and `Custom Riverbed Surface Layer` resolve exactly as labeled;
- legacy null/non-null selections preserve their pre-patch behavior without asset migration;
- `Ground Riverbed Support`, `Ground Bank Material Blend`, `Ground Bank Layer Identity`, and `Ground Outer Bank Extension` remain unchanged;
- no River file, UV stream, debug view, texture, geometry, renderer, draw call, or per-frame work is added.

### A4A.1 implementation result — 2026-07-16

- `GroundRiverbedSurfaceSource` provides migration-safe `Primary Ground`, `Inherit Bank Surface Layer`, and `Custom Riverbed Surface Layer` ownership. Legacy null/non-null profile data resolves to Primary/Custom without modifying any scene, prefab, style, or profile asset.
- `ResolveGroundSubstrateCompositionWeights` normalizes existing Bank and Riverbed blends into one primary/Bank/Riverbed weight triplet. The same triplet now composes dry albedo, smoothness, and specular. Sequential Bank-then-Riverbed lerps are removed.
- The main Inspector now exposes exactly two River-coupled top-level groups: `River-Coupled Ground Response — River Bank` and `River-Coupled Ground Response — Riverbed`. Surface and hydrology profile editors remain collapsed by default, and the A3B.1 delayed create/duplicate path is preserved.
- No River file, support/domain resolver, UV stream, texture, geometry, renderer, draw call, debug enum, or debug dispatch changed.

## V3S-A4B — Exact-support Riverbed hydrology

**Status:** implemented after the A4A.1 normalized-composition consistency check and source/compliance audited. Unity compilation and visual validation are pending.

### Objective

Apply an independent wetness modifier to exact Riverbed Support after dry Riverbed substrate composition. Riverbed wetness must be at least independently controllable and may inherit the accepted Shore Hydrology Modifier or use a separate existing `GroundHydrologyModifierProfile`.

### Approved serialized authoring

```text
Riverbed Hydrology Source
    Inherit Shore Hydrology Modifier
    Custom Hydrology Modifier
    Disabled

Custom Riverbed Hydrology Modifier [Custom only]
Riverbed Wetness Strength          0–1, default 1.00
```

`Inherit Shore Hydrology Modifier` is the default. If no Shore modifier is selected, inherited Riverbed hydrology resolves disabled. `Custom Hydrology Modifier` uses an independently selected reusable profile. `Disabled` produces no local Riverbed wetness.

No Riverbed reach, fade, broad-bank, immediate-bank, or waterline controls are allowed. Placement is exact existing `Ground Riverbed Support`:

```hlsl
riverbedWetness =
    riverbedHydrologyEnabled *
    saturate(riverbedWetnessStrength) *
    saturate(riverbedSupport);
```

### Wetness-composition contract

Keep `Ground Local Shore Wetness` as the Bank-only Shore mask. Update `Ground Effective Wetness` to display the bounded union of:

- global weather Wetness;
- local Shore wetness;
- local Riverbed wetness.

Normal rendering must apply Shore and Riverbed modifier character independently after dry substrate and cover composition:

- pixel-pattern softening uses bounded union of global, Shore, and Riverbed contributions;
- darkening uses bounded union of global, Shore, and Riverbed contributions;
- smoothness uses bounded union of global, Shore, and Riverbed contributions;
- specular preserves the existing global multiplier and adds saturated Shore and Riverbed boosts;
- Shore tint applies from the Shore profile and Riverbed tint applies from the Riverbed profile, with Riverbed applied last as support rises;
- snow/frost hydrology retention combines both local modifiers, although exact submerged-cover exclusion already removes those covers on full support.

Riverbed wetness is independent from Bank and Riverbed substrate identity. Primary-Ground Riverbed source can still be wet; a custom dry Riverbed layer can be dry only when hydrology is disabled or strength is zero.

### A4B reviewed source evidence

- `ResolveGroundLocalShoreWetness` is correctly Bank-domain-only and must remain so.
- `ResolveGroundEffectiveWetness`, combined wet response helpers, and final forward composition currently accept only `localShoreWetness`.
- `GroundHydrologyModifierProfile` already contains every approved wet-character field and requires no schema or asset edit.
- `GeneratedGround` already transports the Shore modifier character. A4B requires a second resolved modifier transport and one strength property only.
- Existing `Ground Effective Wetness` can be updated; no new debug mode is required or approved.

### A4B acceptance criteria

- full Riverbed Support receives wetness equal to the selected/inherited modifier multiplied by Riverbed Wetness Strength;
- the Riverbed is visibly at least as wet as the adjacent matching Bank when Riverbed strength is `1` and Shore wetness is partial;
- Riverbed wetness works with Primary Ground, inherited Bank, and custom Riverbed substrate sources;
- Shore Wetness Reach does not move Riverbed wetness, and Riverbed Wetness Strength does not move `Ground Local Shore Wetness`;
- `Ground Effective Wetness` includes global, Shore, and Riverbed wetness without a new debug view;
- disabled or missing modifier state produces zero Riverbed wetness;
- no River file, spatial channel, reach/fade control, texture, geometry, scene, prefab, material, or profile-schema change occurs.

### A4B implementation result — 2026-07-16

- `GroundRiverbedHydrologySource` resolves `Inherit Shore Hydrology Modifier`, `Custom Hydrology Modifier`, or `Disabled`; `Riverbed Wetness Strength` defaults to `1.00`. No Riverbed distance, reach, fade, depth, or additional shape control exists.
- Riverbed wetness is exact existing `Ground Riverbed Support × modifier enabled × strength`. It is independent from Bank/Riverbed substrate source and Shore Wetness Reach.
- Global, Shore, and Riverbed wetness now feed the existing bounded darkening, pixel-softening, smoothness, specular, snow-retention, frost-retention, and `Ground Effective Wetness` paths. `Ground Local Shore Wetness` remains Bank-only.
- The existing `Ground Effective Wetness` debug dispatch consumes the updated resolver; `GeneratedGroundDebugView` and `PixelSurfaceGroundMaskDebug.hlsl` remain byte-identical and no debug view was added.

## V3S-A4B.1 — Submerged Riverbed finish decoupling

**Status:** implemented exactly from the pre-recorded plan and final source/compliance audited. Unity compilation and visual validation are pending.

### Observed Unity evidence

- Unity validation confirms A4A.1/A4B otherwise works as intended: normalized Bank/Riverbed substrate composition, explicit Riverbed source selection, exact-support wetness, and Inspector grouping are accepted.
- At high Riverbed wetness, the supplied screenshot shows localized bright white highlights on the visible bed beneath the water. The same wetness character is intended to keep tint, darkening, pattern softening, snow melt, and frost melt, but the submerged Ground should not act as the primary reflective interface.
- Current source applies Riverbed smoothness as `riverbedWetness × _GroundRiverbedHydrologyCharacterA.w` inside `ResolveGroundCombinedWetSmoothnessBoost`, and Riverbed specular as `riverbedWetness × _GroundRiverbedHydrologyCharacterB.x` inside `ResolveGroundRiverbedWetSpecularBoost`. `PixelSurfaceGroundForwardPass.hlsl` adds both results after dry substrate resolution. There is no independent submerged-finish attenuation.
- `GroundHydrologyModifierProfile` already owns the reusable wetness character. It remains read-only. The correction belongs to Riverbed spatial/application controls, not to the reusable profile schema.
- The supplied source snapshot contains no `.git` metadata. Branch, `HEAD`, staged/unstaged state, and unrelated repository changes cannot be inspected here and remain an integration prerequisite in the real repository.

### Objective

Preserve exact-support Riverbed hydrology colour, darkening, pixel-pattern softening, and cover interaction while independently controlling the amount of modifier smoothness and specular finish permitted on submerged Ground. Default both new response multipliers to zero so water-surface optics remain the primary reflection owner. Shore hydrology remains unchanged.

### Approved authoring contract

Under the existing Riverbed foldout:

```text
River-Coupled Ground Response — Riverbed
└── Wetness
    ├── Riverbed Hydrology Source
    ├── Riverbed Wetness Strength
    └── Submerged Finish
        ├── Smoothness Response   0–1, default 0
        └── Specular Response     0–1, default 0
```

The controls scale only the Riverbed modifier's finish channels:

```hlsl
riverbedSmoothnessBoost =
    riverbedWetness *
    modifierSmoothnessBoost *
    riverbedSmoothnessResponse;

riverbedSpecularBoost =
    riverbedWetness *
    modifierSpecularBoost *
    riverbedSpecularResponse;
```

They do not change Riverbed wetness, tint, darkening, pixel softening, snow/frost melt, substrate identity, exact support, Shore wetness, global wetness, or water rendering.

### Approved file scope

```text
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md

Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs

Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
```

`PixelSurfaceGroundForwardPass.hlsl` is reviewed but should remain unchanged because it already calls the two Riverbed finish resolvers. `GroundHydrologyModifierProfile.cs`, all River/water files, `PixelSurfaceGroundMaskDebug.hlsl`, debug enums, profile assets, scenes, prefabs, materials, geometry, UV streams, textures, and shared mesh utilities are read-only.

### File-by-file implementation sequence

1. Record this plan before implementation; update the Ground roadmap and Inspector plan after the canonical plan exists.
2. Add two serialized zero-default response multipliers, clamped accessors, null-reset values, and copy behavior to `GroundMaterialControls`.
3. Add matching Shader property IDs and role-aware MaterialPropertyBlock transport in `GeneratedGround`.
4. Add both fields to local/shared Inspector property binding and draw them under a compact `Submerged Finish` subsection inside Riverbed Wetness. Preserve the A3B.1 delayed asset-creation workflow and existing region grouping.
5. Add matching hidden ShaderLab and Ground CBUFFER properties.
6. Multiply only Riverbed smoothness/specular resolver contributions by the new response values. Preserve tint, darkening, softening, snow/frost retention, exact-support wetness, Shore hydrology, and global wetness formulas byte-for-byte.
7. Complete exact-scope, full modified-file reread, direct-caller/consumer, C# parser/compiler, malformed-string, line-ending, property-parity, shared-shader, HLSL syntax, no-debug-growth, no-River/water-change, default/migration, performance, documentation, and final baseline-comparison checks.

### Invariants and non-goals

- No new debug view. Existing `Ground Riverbed Support`, `Ground Local Shore Wetness`, and `Ground Effective Wetness` remain unchanged.
- No River or water shader changes. This patch does not implement water reflection, depth suppression, Fresnel, planar reflection, refraction, or emergence classification.
- No change to Riverbed wetness placement or strength. Exact `Ground Riverbed Support` remains the only Riverbed wetness field.
- No change to Shore hydrology smoothness/specular behavior.
- No `GroundHydrologyModifierProfile` schema or asset edit. The reusable modifier continues to define wetness character; Riverbed application controls define how much finish survives while submerged.
- No scene, prefab, material, profile asset, texture, geometry, renderer, mesh channel, shader keyword/variant, generated field, collider, draw call, River rebuild, Ground geometry rebuild, or per-frame CPU work.
- Both controls are Material-only and use the existing role-aware property-block refresh.
- Existing serialized objects receive zero defaults without automatic scene/prefab/style/profile reserialization by this patch.

### Risks and required checks

- Zero smoothness/specular response must not disable Riverbed tint, darkening, softening, snow melt, or frost melt.
- Raising either response must restore only its corresponding finish channel and remain saturated by the existing final Ground surface path.
- `PixelSurfaceGroundResponse.hlsl` is also included by `SH_PixelSurfaceLit.shader`; all new property references must remain inside `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`.
- `GeneratedGroundEditor.cs` uses CRLF and must retain CRLF. Other approved source/doc files retain their existing line endings.
- Property names must match exactly across C#, CBUFFER, and ShaderLab.

### Acceptance criteria

- At defaults (`Smoothness Response = 0`, `Specular Response = 0`), the bright submerged Ground highlights disappear.
- Riverbed wet tint, darkening, pixel softening, snow melt, frost melt, exact support, and `Ground Effective Wetness` remain unchanged.
- Shore wetness keeps its current smoothness and specular response.
- Raising only Smoothness Response restores only Riverbed smoothness; raising only Specular Response restores only Riverbed specular.
- No new debug view, River/water change, rebuild, geometry change, or runtime field is introduced.
- Unity compiles the C# code and both Ground/generic Pixel Surface shaders without errors.

### Unity validation gate

1. Confirm there are no C# errors and both Ground/generic Pixel Surface shaders compile.
2. With Riverbed wetness enabled and both Submerged Finish responses at `0`, confirm the bright white bed highlights disappear while wet colour/darkening remain visible.
3. Raise Smoothness Response alone, then Specular Response alone; verify each restores only its named Riverbed finish response.
4. Confirm Shore wetness appearance is unchanged and `Ground Local Shore Wetness` remains Bank-only.
5. Confirm `Ground Effective Wetness` and `Ground Riverbed Support` look unchanged from the accepted A4B result.
6. Refresh material properties once; settings must persist and no River or Ground geometry rebuild should occur.

### A4B.1 implementation and post-change audit — 2026-07-16

- Exactly the nine approved files changed: three canonical documents, `GroundMaterialControls.cs`, `GeneratedGround.cs`, `GeneratedGroundEditor.cs`, `PixelSurfaceGroundMaterialProperties.hlsl`, `PixelSurfaceGroundResponse.hlsl`, and `SH_PixelGroundSurfaceLit.shader`. No file was added or removed.
- `GroundMaterialControls` adds only `riverbedWetSmoothnessResponse` and `riverbedWetSpecularResponse`, both implicit-zero serialized floats with clamped accessors, null-reset values, and copy behavior. No enum, source mode, profile field, or asset schema changed.
- `GeneratedGround` transports both values through the existing explicit-role MaterialPropertyBlock path. C#, Ground CBUFFER, and hidden ShaderLab property names match exactly.
- `GeneratedGroundEditor` exposes the two controls under one `Submerged Finish` subsection inside Riverbed Wetness for local and shared-style authoring. The two existing region foldouts and A3B.1 delayed profile-creation paths remain unchanged. CRLF line endings are preserved.
- `ResolveGroundCombinedWetSmoothnessBoost` multiplies only the Riverbed smoothness contribution by Smoothness Response. `ResolveGroundRiverbedWetSpecularBoost` multiplies only the Riverbed specular contribution by Specular Response. Riverbed wetness, effective wetness, tint, darkening, pixel softening, snow/frost retention, Shore finish, and global wetness functions remain unchanged. `PixelSurfaceGroundForwardPass.hlsl` is byte-identical.
- `GroundHydrologyModifierProfile.cs`, `GroundSurfaceLayerProfile.cs`, River code, the Ground debug include and enum, scenes, prefabs, materials, assets, geometry, textures, and shared mesh utilities are unchanged. No debug view was added.
- The automated contract audit passes `107/107` checks. Tree-sitter parses all three changed C# files without syntax errors and parses the changed response include; malformed multiline-string and line-ending checks pass. Clang 17 library-mode HLSL harnesses for the new CBUFFER and finish expressions emit LLVM IR successfully. `dxv` is unavailable, so DXIL validation/signing and Unity shader compilation are not claimed.
- Added shader work is two saturated scalar multipliers. No texture/noise sample, derivative, trigonometric operation, keyword/variant, generated field, draw call, renderer, geometry, rebuild, or per-frame process was introduced.


## V3S-A4B.2 — Stylized Shore wet-finish shaping and Riverbed/Bank wetness transition

**Status:** implemented exactly from the pre-recorded plan and source/compliance audited. Unity compilation and visual validation are pending.

### Observed Unity evidence

- The supplied editor/game-camera comparison shows the current wet Shore finish becoming broad and nearly white from a near-perpendicular editor view, while the production isometric camera shows only a weak diffuse highlight. The existing Shore wetness mask itself is already validated; the defect is presentation, not spatial classification.
- The supplied water-hidden screenshot shows exact-support Riverbed wetness meeting lower Bank wetness at a visible hard line. `ResolveGroundRiverbedWetness` currently returns `enabled × strength × Ground Riverbed Support` with no Bank-side transition.
- `ResolveGroundCombinedWetSmoothnessBoost` and `ResolveGroundLocalShoreWetSpecularBoost` currently apply the Shore modifier's full smoothness/specular contributions to URP PBR. `UniversalFragmentPBR` therefore owns the complete lobe shape, which varies strongly with camera angle and gives no production-camera-centered art direction.
- `StylizedRiverCorridorGeometry` already publishes `TEXCOORD3.y` as metres outward from the Ground Riverbed Support boundary and `TEXCOORD3.z` as corridor Bank-domain validity. This existing data is sufficient for a short wetness-only transition; no River edit or new field is required.
- The supplied source snapshot contains no `.git` metadata. Branch, `HEAD`, staged/unstaged state, and unrelated repository changes cannot be inspected here and remain an integration prerequisite in the real repository.

### Objective

Implement both approved corrections in one Material-only Ground patch:

1. Replace most of the broad Shore-added PBR smoothness/specular response with a narrow stylized highlight that remains readable from the production isometric camera and fades vertically away from screen centre.
2. Extend Riverbed hydrology a short authored distance onto the corridor Bank and smoothly reduce it to zero, removing the hard wetness seam without changing Riverbed substrate/support ownership.

### Approved authoring contract

Under the existing region-oriented Inspector:

```text
River-Coupled Ground Response — River Bank
└── Shore Wetness
    ├── existing modifier and spatial application
    └── Wet Highlight Shaping
        ├── Wet Highlight Strength          0–2, default 0.35
        ├── Wet Highlight Tightness         0–1, default 0.80
        ├── Camera-Centred Bias             0–1, default 0.85
        └── Vertical Falloff                 0–1, default 0.60

River-Coupled Ground Response — Riverbed
└── Wetness
    ├── existing source / strength
    ├── Wetness Transition
    │   ├── Riverbed-to-Bank Blend Distance 0–2 m, default 0.20 m
    │   └── Riverbed-to-Bank Blend Softness 0–1, default 0.75
    └── existing Submerged Finish
```

The Shore controls have these exact meanings:

- `Camera-Centred Bias = 0` preserves the ordinary physical half-vector highlight and the existing Shore-added PBR finish.
- Increasing Camera-Centred Bias transfers Shore wet smoothness/specular out of the broad PBR path and into a screen-centred stylized highlight. At the default `0.85`, fifteen percent of the old broad Shore finish remains.
- Wet Highlight Tightness controls the physical half-vector exponent. Vertical Falloff controls how quickly the camera-centred band fades toward the top and bottom of the active camera view.
- The stylized highlight is multiplied by existing local Shore wetness and current lit visibility. It does not apply to ordinary Ground or Riverbed wetness. Riverbed reflective finish remains owned by the existing zero-default A4B.1 Submerged Finish controls.

The Riverbed transition has this exact contract:

```hlsl
transition01 = saturate(bankDistance / max(0.0001, blendDistance));
transitionWeight = lerp(
    1.0 - transition01,
    1.0 - transition01 * transition01 * (3.0 - 2.0 * transition01),
    blendSoftness);

riverbedWetness =
    modifierEnabled *
    wetnessStrength *
    saturate(riverbedSupport + bankDomain * transitionWeight);
```

A zero Blend Distance preserves exact-support A4B.1 behavior. The transition changes hydrology only; Riverbed substrate, support, cover exclusion, geometry, and Bank material fields remain unchanged.

### Approved file scope

```text
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md

Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs

Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
```

Read-only related contracts are `GroundHydrologyModifierProfile.cs`, `GroundSurfaceLayerProfile.cs`, `StylizedRiverCorridorGeometry.cs`, `StylizedRiver.cs`, the water shaders/includes, `PixelSurfaceGroundMaskDebug.hlsl`, and `SH_PixelSurfaceLit.shader`.

### File-by-file implementation sequence

1. Record this canonical plan before implementation; then update the Ground roadmap and Inspector plan to reference the same bounded patch.
2. Add six serialized controls, exact defaults, clamped accessors, null-reset values, and copy behavior to `GroundMaterialControls`.
3. Pack Shore highlight shaping into one MaterialPropertyBlock vector and Riverbed transition into one vector in `GeneratedGround`; add exact matching property IDs.
4. Bind both local and shared-style Inspector paths. Draw the four Shore controls under `Wet Highlight Shaping` and the two Riverbed controls under `Wetness Transition`. Preserve the two region foldouts and delayed profile-creation workflow.
5. Add matching hidden ShaderLab and Ground CBUFFER vectors.
6. In `PixelSurfaceGroundResponse.hlsl`, scale only Shore-added PBR wet smoothness/specular by `1 - Camera-Centred Bias`, and extend only Riverbed wetness onto the existing Bank domain using the recorded distance/softness formula.
7. In `PixelSurfaceGroundForwardPass.hlsl`, add one stylized Shore highlight after the existing neutral/tinted PBR lighting composition and before fog. Use existing Shore wetness, the main-light direction/colour, the active camera's normalized screen-space Y, the physical half-vector lobe, and current lighting visibility. Do not modify Riverbed highlight behavior.
8. Complete exact-scope, full modified-file reread, direct producer/consumer, property-parity, C# parser/compiler, malformed-string, line-ending, HLSL syntax/shared-include, no-debug-growth, no-River/water-change, no-profile-schema/asset-change, default/migration, performance, documentation, and final baseline-comparison checks.

### Invariants and non-goals

- No new debug view. Existing exact Inspector names remain `Ground Riverbed Support`, `Ground Bank Material Blend`, `Ground Local Shore Wetness`, and `Ground Effective Wetness`.
- No River or water file changes. This patch does not implement water reflection, Fresnel, depth-based suppression, refraction, planar reflection, emergence classification, or player tracking.
- No player/component/camera reference is introduced. Camera-centred shaping uses the active rendering camera's existing normalized screen-space position only.
- No Riverbed highlight is added. A4B.1 Submerged Finish remains the sole control over Riverbed smoothness/specular response.
- No change to Bank/Riverbed substrate weights, Riverbed support, submerged-cover exclusion, Shore wetness placement, global wetness, hydrology profile schemas, or profile assets.
- No scene, prefab, material, texture, geometry, renderer, mesh channel, shader keyword/variant, generated field, collider, draw call, River rebuild, Ground geometry rebuild, or per-frame CPU work.
- Added fragment work is bounded to scalar/vector arithmetic, one physical-lobe `pow`, one screen-band `pow`, and one main-light query on River-coupled Ground-shader draws. No texture/noise sample or derivative is added.
- All six controls are Material-only and use the existing explicit-role MaterialPropertyBlock refresh.

### Risks and required checks

- Camera-centred shaping must not highlight dry Bank, ordinary Ground, or Riverbed; local Shore wetness is the mandatory mask.
- High Camera-Centred Bias must reduce the previous broad white Shore response rather than stacking another highlight on top of it.
- `Camera-Centred Bias = 0` must restore the existing broad PBR finish and physical lobe path.
- The screen-centred highlight must use the active Game camera in Game view and the Scene camera in Scene view without storing a camera reference.
- Riverbed transition must use existing metre distance and Bank domain only, stop at the authored distance, and become zero when Blend Distance is zero or Riverbed hydrology is disabled.
- `PixelSurfaceGroundResponse.hlsl` is shared by the generic Pixel Surface shader; all new property references must remain inside the existing Ground-material-properties guard.
- `GeneratedGroundEditor.cs` uses CRLF and must retain CRLF. Property names must match exactly across C#, CBUFFER, and ShaderLab.

### Acceptance criteria

- The near-top-down editor view no longer turns most of both Shores white at the default shaping values.
- The production isometric camera shows a clear stronger Shore highlight around the vertical middle of the camera and a smooth reduction toward the top and bottom.
- Tightness, strength, camera bias, and vertical falloff each change only their named highlight characteristic.
- Existing Shore wet tint, darkening, softening, spatial reach, Bank substrate, and cover response remain unchanged.
- Riverbed wetness remains full on Ground Riverbed Support and fades smoothly into Bank wetness over the authored short transition without changing substrate composition.
- Zero Blend Distance restores the previous hard exact-support wetness boundary. `Ground Local Shore Wetness` remains unchanged; `Ground Effective Wetness` includes the transition through its existing resolver.
- No new debug view, River/water change, profile-schema/asset change, geometry/rebuild change, or texture sample is introduced.
- Unity compiles all changed C# and both Ground/generic Pixel Surface shaders without errors.

### Unity validation gate

1. Confirm there are no C# errors and both Ground/generic Pixel Surface shaders compile.
2. With the default shaping values, compare the same Scene and Game camera positions: the broad white Shore washout must be reduced, while the Game camera shows a stronger camera-centred band.
3. Test Wet Highlight Strength, Tightness, Camera-Centred Bias, and Vertical Falloff separately; confirm each affects only the Shore wet highlight and no Riverbed/ordinary Ground pixels.
4. Set Riverbed-to-Bank Blend Distance to `0`, `0.20`, and `0.50 m`; with the water surface hidden, confirm only the wetness handoff changes and no substrate boundary moves.
5. Confirm exact debug names `Ground Local Shore Wetness` is unchanged and `Ground Effective Wetness` shows the short Riverbed transition without new debug entries.
6. Refresh material properties once; settings must persist and no Ground or River geometry regeneration should occur.

### Implementation and final compliance result

- Six serialized controls, defaults, clamped accessors, reset/copy behavior, local/shared Inspector bindings, matching MaterialPropertyBlock vectors, CBUFFER declarations, and hidden ShaderLab properties are present.
- Shore-added PBR smoothness and specular are scaled only by `1 - Camera-Centred Bias`. The replacement post-lighting lobe is Shore-wetness-only, uses the main-light half-vector, active-camera normalized screen-space Y, one physical-lobe exponent and one vertical-band exponent, and is applied before fog. It exits before the main-light query and exponent work when Shore wetness, highlight strength, or camera bias is zero, so ordinary GeneratedGround does not pay the shaping cost.
- Riverbed wetness remains full on exact `Ground Riverbed Support` and gains only the approved short Bank-domain transition from existing corridor distance/domain data. Riverbed substrate, cover exclusion, A4B.1 submerged-finish controls, Shore wet tint/darkening/softening, and global wetness are unchanged.
- The exact ten-file scope passed a `124/124` automated contract audit. Tree-sitter parsed all three changed C# files without syntax errors; malformed-string, NUL, line-ending, property-parity, local/shared binding, no-debug-growth, unchanged River/profile/debug files, and no-new-sample/noise checks passed.
- Two Clang 17 HLSL library-mode harnesses emitted LLVM IR for the changed Riverbed transition/PBR-transfer expressions and the stylized Shore highlight. This is parser/expression validation, not Unity shader compilation or DXIL signing. `dxv` and the Unity Editor were unavailable.
- The supplied source snapshot has no `.git` metadata, so branch, `HEAD`, staged/unstaged state, and unrelated repository changes remain unverified until integration into the real repository.



## Combined approved implementation scope

```text
Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md
Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md
Assets/Docs/Ground_Visual_Design_and_Architecture.md
Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md
Assets/Docs/GeneratedGround_Inspector_Audit_and_Overhaul_Plan.md

Assets/Game/Procedural/Ground/GroundMaterialControls.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs

Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaskDebug.hlsl
Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader
```

No other file is approved. `GroundSurfaceLayerProfile`, `GroundHydrologyModifierProfile`, all River files, shared mesh utilities, scenes, prefabs, materials, existing assets, textures, and debug enums are read-only.

## Combined file-by-file implementation sequence

1. Record this plan before implementation and update the four related canonical Ground documents with the same ownership and non-goals.
2. Add migration-safe Riverbed surface-source and Riverbed hydrology-source serialization, custom Riverbed hydrology reference, strength, accessors, reset, and copy behavior to `GroundMaterialControls`.
3. Resolve effective Riverbed surface and hydrology profiles in `GeneratedGround`; transport existing dry/wet profile data through the current role-aware MaterialPropertyBlock.
4. Replace five River-coupled top-level Inspector foldouts with the approved River Bank and Riverbed foldouts for both local and shared-style authoring. Preserve delayed create/duplicate callbacks and asset ownership refresh.
5. Add matching hidden ShaderLab/CBUFFER properties and shared-safe A4B resolvers. Keep all property-dependent shared-include code inside the Ground material-property guard.
6. Resolve normalized substrate weights once per fragment and use them for albedo, smoothness, and specular.
7. Resolve Shore and Riverbed wetness once per fragment and share them across cover, colour, smoothness, specular, and `Ground Effective Wetness`.
8. Run exact-scope, line-ending, C# parser/compiler, malformed-string, serialized migration, property parity, shared-shader, HLSL syntax, no-debug-growth, no-River-change, performance, documentation, and final baseline-comparison audits.

## Combined invariants and non-goals

- A2C.4 renderer authorization and ordinary-Ground UV3 integrity remain unchanged.
- A3A Bank cover retention and A3B Shore mask/character remain unchanged except where combined with the new Riverbed wetness source.
- Exact Riverbed Support remains the only Riverbed placement field.
- No new debug view. Use exact Inspector names: `Ground Riverbed Support`, `Ground Bank Material Blend`, `Ground Bank Layer Identity`, `Ground Outer Bank Extension`, `Ground Local Shore Wetness`, and `Ground Effective Wetness`.
- No profile-schema or existing asset edits.
- No textures, noise samples, geometry, renderers, draw calls, shader keywords/variants, generated fields, collider work, River rebuild, Ground geometry rebuild, Painted Accent rebake, or per-frame CPU process.
- Every new control is Material-only and refreshes existing property blocks.
- The supplied source snapshot has no `.git` metadata; branch, `HEAD`, staged/unstaged state, and unrelated repository changes remain unverified integration prerequisites.

## Combined Unity validation gate

1. Confirm C# and both Ground/generic Pixel Surface shaders compile without errors.
2. Set Bank and Riverbed to the same profile and verify no primary-Ground strip remains at the shared edge; repeat with different profiles and verify a direct Bank-to-Riverbed transition.
3. Validate `Primary Ground`, `Inherit Bank Surface Layer`, and `Custom Riverbed Surface Layer`, including an inherited-primary Bank and one legacy custom Riverbed selection.
4. Validate `Inherit Shore Hydrology Modifier`, `Custom Hydrology Modifier`, and `Disabled`; verify full `Ground Riverbed Support` is visibly wet at strength `1` and `Ground Local Shore Wetness` remains Bank-only.
5. Verify `Ground Effective Wetness` includes global, Shore, and Riverbed wetness; confirm Bank controls do not move Riverbed support/wetness and Riverbed controls do not move Bank or Shore masks.
6. Refresh material properties and regenerate Ground once; confirm persistence, unchanged raw Painted Accent coverage, no ordinary-Ground spill, and no River rebuild requirement.

## V3S-A5 — Optional profile detail extension

After base composition is accepted, extend `GroundSurfaceLayerProfile` with optional packed detail:

```text
Detail Texture / Texture2DArray slice
Detail Scale
Normal Strength
Cavity Strength
Finish Variation
```

Use stable world-space XZ sampling. One packed sample should provide normal XY, cavity/value response, and finish variation. Profiles without detail perform no sample. Do not hardcode Sand/Mud/Gravel/Rock shader cases, add parallax, tessellation, generated stones, extra renderers, or procedural Voronoi.

## V3S-A6 — Family tuning and production acceptance

Only after A2B–A5 behavior is accepted, assign and tune shared defaults for:

```text
Snowfield
Grassland
Wet Mudflat
```

Example targets:

```text
Grassland
→ Pale River Sand or Exposed Soil bank
→ Fine Gravel or Dark River Mud bed
→ strong vegetation retreat

Snowfield
→ Compacted Snow Soil bank
→ Rounded River Rock bed
→ strong waterline snow melt

Wet Mudflat
→ inherited or Dark River Mud bank / bed
→ material identity changes restrained
→ wet finish dominant
```

Verify shared/local ownership, inline asset persistence, Material-only refresh, corridor property propagation, and no geometry, collider, River domain, corridor geometry, macro, elevation, semantic, or Painted Accent regression.

# Corrected V4 Contact / Edge Accent plan

V4 begins only after V3S-A6.

## Source contract

```text
GeneratedMass grounding
selected GroundModifier boundaries
```

There is no River source, River snapshot dependency, River contribution channel, or River invalidation path.

## V4-A1 — Source contract and transient coverage proof

Add a dedicated Generated Texture feature kind and a Ground-local R8 Contact coverage field.

### GroundModifier participation

Participation must be explicit:

```text
Contact Accent Mode: None / Boundary
Contact Accent Strength
```

Default is `None`. Ordinary flattening volumes do not automatically produce rings.

### GeneratedMass discovery

Use `GeneratedGeometryRegistry`; accept active solid static GeneratedMass sources with valid final meshes and overlapping Ground bounds. Do not scan arbitrary scene renderers or colliders.

### Contact contour

For GeneratedMass:

1. transform final mesh vertices into Ground-local space;
2. sample Ground base height;
3. collect contact-height vertices and triangle-edge crossings;
4. project to XZ;
5. merge near duplicates;
6. build a deterministic convex hull;
7. reject negligible contours;
8. use an explicitly reported conservative fallback only when exact extraction fails.

For GroundModifier, use the analytical circle or oriented-box core boundary.

### Coverage

```text
one Ground-local R8 field
world-texel target approximately 0.04 m
power-of-two resolution clamped 512–2048
maximum combination, never addition
```

Breakup is deterministic bake/preview-time shaping, not per-frame shader noise.

A1 adds one raw coverage debug view, one SceneView contour overlay, compact diagnostics, signatures, and no normal-lit response.

## V4-A2 — Contact visual response

Interpret the R8 field through existing Ground semantic context and palette:

```text
damp / standing-water context
→ restrained Damp Tint and wet response

compaction context
→ worn or compressed boundary response

high exposure / Snowfield context
→ cool compressed rim

otherwise
→ restrained GeneratedMass grounding response
```

The effect is not a complete ring, black outline, drop shadow, or decal stripe. Add a dedicated Contact Tint only if family validation proves it necessary.

## V4-A3 — Persistent production output

After visual acceptance:

- persist one Contact R8 asset and Ground-local mapping;
- validate signatures and ownership;
- bind persistent data only in Play Mode and Player;
- extend build validation and generated-asset cleanup;
- generalize authoring toward `Bake Ground Surface Outputs` without breaking existing Painted Accent assets.

Player runtime never scans sources, resolves contours, rasterizes coverage, or uploads generated fields.

## V4-A4 — Explicit ordinary-prop sources, deferred

Arbitrary huts, bridge supports, and ordinary meshes remain excluded until a separately approved opt-in source contract is required. Never scan all scene renderers.

# Invalidation and regeneration

## Material-only

The following refresh Ground and corridor `MaterialPropertyBlock` values only:

```text
shore/waterline strengths and finish
riverbed response, tint strength, and finish
Contact overall visible intensity
debug view
family palette and material finish
```

## River structural

```text
River spline
corridor width/profile
bed/bank geometry
corridor semantic channels
```

These rebuild River-owned outputs and refresh Ground interpretation. They never stale Contact coverage.

## Contact coverage

```text
GeneratedMass final geometry or transform
explicit modifier participation, shape, or transform
Contact width, softness, breakup, source weights, or seed
Ground mapping or bounds
Contact baker revision
```

These invalidate Contact source preparation and coverage only.

## Unrelated

River foam, transport, disturbances, pressure/wakes, water material, Painted Accent ink colour/opacity, macro intensity, and elevation readability do not rebuild Contact coverage.

# Performance contract

Through V3S-A4:

```text
new per-frame CPU work         none
new generated textures         none
new geometry                    none
new renderers/draw calls        none
new collider work               none
```

V3S-A2A is authoring/storage only and changes no shader output. V3S-A2B–A4 add bounded scalar shader work and profile property transport. V3S-A2C.1 packs corridor-bank distance and validity into the River corridor's already-existing `Vector4` UV3 stream, so ordinary Ground gains no new mesh memory and no extra channel. V3S-A2C.4 adds one explicit per-renderer float in the existing property block and a bounded uniform authorization test; it adds no texture sample, keyword, variant, material, renderer, draw call, generated texture, or per-frame CPU process. Multiplication guarantees zero output but does not claim that the shader compiler skips every River arithmetic operation. V3S-A5 adds at most one optional packed detail sample for profiles that enable it.

V4 adds one R8 texture sample and one bounded static texture per Ground; all source work remains Edit Mode or production-bake time.

# Patch order

```text
Patch 1 — V3S-A0/A1
    canonical docs
    TEXCOORD3.x consumption
    one Riverbed Support debug view
    no lit response

Patch 2 — V3S-A2A
    reusable GroundSurfaceLayerProfile asset type
    six starter layer assets
    main-Inspector dropdown selection
    inline editing, creation, duplication, and persistence
    no lit response

Patch 3 — V3S-A2B
    Bank Surface Layer material-composition proof
    bank reach / immediate-bank / waterline material weights
    no wetness or cover retreat

Patch 4 — V3S-A2C.1
    remove rejected ordinary-Ground bank distance
    corridor UV3.y distance from Riverbed Support boundary
    corridor UV3.z bank-domain validity
    explicit Core Bank and Outer Bank Extension groups

Patch 5 — V3S-A2C.4
    explicit OrdinaryGround / RiverCorridor renderer role
    role-gated Shore, Riverbed Support, Bank distance, and Bank domain
    ordinary-Ground River-derived metadata cleanup
    complementary Ground/corridor TEXCOORD3 invariants

Patch 6 — V3S-A3A
    vegetation / snow / frost / Painted Accent retention
    four zero-default Material-only master strengths
    debug modes 36–37

Patch 7 — V3S-A3B
    reusable independent Shore hydrology modifier
    metre-based wetness reach independent from Bank reach
    legacy Shore damp/Pooled Wetness refactor
    bounded local/global wetness and debug modes 38–39

Patch 8 — V3S-A4A
    Riverbed dry Surface Layer composition through UV3.x
    submerged vegetation / snow / frost / Painted Accent exclusion
    no Riverbed hydrology and no new debug view

Patch 9 — V3S-A4A.1
    normalized primary / Bank / Riverbed dry composition
    explicit Riverbed substrate source ownership
    two region-oriented River-coupled Inspector groups

Patch 10 — V3S-A4B
    independent exact-support Riverbed hydrology
    inherited / custom / disabled modifier ownership
    no reach/fade field and no new debug view

Patch 11 — V3S-A5
    optional packed profile detail


Patch 12 — V3S-A6
    family tuning and production acceptance

Patch 13 — V4-A1
    explicit Contact sources and transient R8 proof

Patch 14 — V4-A2
    Contact visual response

Patch 15 — V4-A3
    persistent output and production integration
```

# Rejected architecture

- River banks or riverbeds inside the Contact Accent field.
- River-owned Ground style or substrate controls.
- Bed identity inferred from low `UV2.y`, world height, depth, or centreline distance.
- A second shore-darkening layer stacked on the existing combined damp formula.
- Wetness character stored in or spatially multiplied by the Bank Surface Layer profile.
- Per-River duplicated Ground materials.
- Per-frame mask generation, contour solving, registry scans, or texture uploads.
- Universal object rings or scene-wide renderer/collider scanning.
- Extra bed geometry, decals, child renderers, or mesh strips.
- Substrate textures before the semantic mask and wet baseline are proven.

# Immediate next work item

Unity-validate the implemented V3S-A4B.1 package exactly through the six-step gate above. A4A.1/A4B substrate ownership, normalized composition, exact-support wetness, and Inspector grouping are accepted and must not be reopened. Do not begin V3S-A5 or water-reflection work until the submerged Ground finish is accepted.
