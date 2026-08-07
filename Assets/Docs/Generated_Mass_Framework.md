# Generated Mass Framework

Status: canonical overview

## Purpose

Generated Mass creates deterministic stylized rock geometry from recipes and surface-feature settings. The module owns base shape generation, structural feature geometry, final mesh channels, and the bridge to shared rock materials.

## Canonical documents

- `Generated_Mass_Surface_Response_Architecture.md` — production geometry, normals, feature responses, performance limits.
- `Generated_Mass_Incremental_Selection_Architecture.md` — frozen corner/bevel selection and ranked-discard contract.
- `Generated_Mass_Incremental_Selection_Implementation_Plan.md` — closure record and remaining selection-related maintenance.
- `Generated_Mass_Edge_Wear_Code_Inventory.md` — current code ownership map.
- `Generated_Mass_Edge_Wear_Recovery_Architecture.md` — retained historical recovery conclusions only.
- `Generated_Mass_Feature_Implementation_Checklist.md` — concise completed/active work list.

## Active lighting defect invariant

The unresolved Generated Mass lighting defect is **surface-orientation response incoherence**, not general darkness and not specular magnitude. Under the same directional light, individual source faces and bevel faces in the HLSL material can be brighter or darker in an order that contradicts their orientation to that light. A bevel geometrically between two parent faces can render darker than both or brighter than both; source faces can likewise invert the expected bright-to-dark progression.

The legacy `M_PixelStone` / `SG_PixelSurfaceLit` behavior is the visual reference: surface response changes coherently as face orientation changes relative to the light, and bevel response bridges its parent surfaces instead of randomly escaping their directional-light ordering.

Whole-object luminance, ambient strength, F0/specular strength, exposure, or average BRDF residual are secondary measurements. They cannot close this defect unless **per-surface and parent–bevel–parent orientation ordering is correct**. Future diagnostics and fixes must evaluate that ordering directly.

## Current production state

GM-SURFACE.2 makes certified bevel and corner-chip geometry ordinary Generated Mass output whenever the serialized structural settings enable it. `BaseGeometryOnly` is retained for disabled features and deterministic safe fallback. Diagnostic preview actions evaluate the same construction system but do not own production output.

## Final framework direction

```text
recipe + surface settings
→ base geometry
→ certified chips and bevels
→ final topology and geometric normals
→ compact primary/secondary feature fields
→ shared whole-rock triplanar normal response
→ bounded feature-localized material responses
```

No per-rock atlases or generated normal maps are allowed. Runtime shaders evaluate at most two structural contributions and do not iterate arbitrary feature lists.

## Frozen selection policy

- corner chips outrank ordinary bevel preservation;
- ordinary bevels are opportunistic;
- incompatible ordinary candidates are reduced deterministically by ranking;
- zero surviving ordinary bevels is valid;
- every accepted result requires complete geometry, topology, render-channel, and performance certification;
- no combinatorial global subset optimization is active.

## Geometry and material ownership

Geometry owns silhouette and macro structural shape. Shared material response owns whole-rock grain and sub-triangle detail. Mesh normals describe geometry only.

## Runtime constraints

- no per-frame mesh reconstruction;
- no unbounded shader loops;
- no one-renderer-per-feature architecture;
- deterministic outputs;
- no scene/prefab/material mutation during source-only patches;
- diagnostics longer than a brief moment must remain incremental, cancellable, responsive, and checkpointed.

## Next work items

1. GM-SURFACE.2 productionize certified bevel/chip geometry. **Completed.**
2. Pack the completed semantic primary/secondary contribution model into final mesh channels.
3. Add shared triplanar whole-rock normal detail.
4. Add convex/chip responses, then concave creases through the same contract.


## Structural surface-response stream

GM-SURFACE.4 compiles the generation-time primary and secondary semantic contributions into `TEXCOORD4` on every final Generated Mass vertex. The 16-byte layout stores primary type/strength and secondary type/strength. The current response role and direction are derived later by the shared shader; feature identity and generation-only provenance are deliberately discarded. `TEXCOORD3` is not reused because the old diagnostic feature-atlas path still occupies it.


## Current surface-lighting status

GM-SURFACE.5A makes whole-rock normal strength visibly meaningful and corrects generated-face mask ownership. Convex bevels and chip caps are lit by their geometric normals while legacy broad exposure/crevice/dirt masks are suppressed on those generated surfaces. Explicit convex accents and exposed-chip material response remain GM-SURFACE.6 work.

### Current generated-face lighting correction — GM-SURFACE.5B

Ordinary convex bevels use a bounded object-space rounded-normal proxy because
the current fixed 16-byte structural stream does not carry adjacent source-face
normals. Shaded whole-rock normal relief receives a weak non-emissive
hemispherical readability term, and convex faces receive a restrained
light-dependent lift. Source faces and chip caps retain their distinct normal
policies. This correction adds no texture, draw call, variable feature loop or
mesh stream.


## GM-SURFACE.5C — Generated-face material-mask inheritance

The dark-band bevel defect was confirmed to be a pre-light material-mask ownership defect, not a mesh-normal defect. Generated bevel and chip faces must not recompute Exposure, Crevice/Base, or DirtDeposit from their own new face normal, and the shader must not suppress those masks as a compensation. During final mesh emission, generated triangles now inherit the source-face material-mask samples present on their shared boundaries. Shader-side convex/chip mask suppression and the GM-SURFACE.5B convex lighting compensation are retired. Whole-rock normal response remains independent.

## GM-SURFACE.5D generated-face material-channel correction

The final Generated Mass mesh owns its pre-light exposure, crevice/base, and dirt masks. The Pixel Surface renderer reads `Color.g`, `Color.b`, and `UV2.y` directly for Generated Masses and does not reclassify bevel or chip faces from their new normals. Generated boundary vertices retain their local source-boundary samples; generated interior vertices interpolate those samples instead of receiving one triangle-wide average. This keeps material treatment continuous while leaving geometric normals solely responsible for light orientation.

## Current Generated Mass lighting baseline — GM-SURFACE.5E

Generated Masses currently return raw URP `UniversalFragmentPBR` lighting. Bevel-specific pre-light colour painting, directional pre-PBR value shaping, post-PBR light-colour reconstruction and shadow-side normal readability are quarantined during parity validation against `SG_PixelSurfaceLit`. Geometry and compiled material masks are unchanged.

## Validated generated-face material-mask continuity

Committed ordinary bevel polygons compile exposure (`Color.g`), crevice/base (`Color.b`), and dirt-deposit (`UV2.y`) values during dirty-triggered mesh construction. After the existing source-boundary/interior inheritance pass, duplicated render vertices are reconciled by emitted bevel provenance and quantized local position. Internal triangulation therefore cannot assign different material-mask values to the same logical bevel point.

The contract is validated on two distinct accepted production meshes: 204 triangles / 35 logical bevels and 188 triangles / 34 logical bevels. Both retained exact immutable geometry fingerprints, complete bevel mapping, zero source-face mask changes, zero internal mask jumps, zero upload mismatches, and zero degenerate-triangle regression. The reconciliation modifies only the three generated-face mask channels and does not modify positions, indices, normals, structural feature values, UV0, provenance, source-face masks, or shader behavior.

The permanent surface-causality suite keeps accepted-build capture, immutable fingerprints, source-face preservation, canonical triangle accounting, logical mapping, mask continuity, normal/gradient evidence, frozen-mesh material parity, shader contribution isolation, and upload checks. Reports remain comprehensive until the visible defect is causally closed.

## Residual bevel-shading status — GM-SURFACE.5G-H2

GM-SURFACE.5G remains a partial production correction: shared-edge generated-mask values are continuous and geometry is preserved, but low-light visual faceting remains on a subset of bevels. The active comprehensive audit evaluates value and structural gradients, triangle quality, geometric and render normals, tangents, parent envelopes, direct-light sensitivity, upload parity, and immutable geometry in one incremental run. Shader-side procedural noise, feature-atlas filtering, ambient/SH, shadows, SSAO, PBR specular and post-processing remain explicitly identified as visual-isolation follow-ups when CPU evidence is insufficient.

## Integrated surface-causality baseline — GM-SURFACE.5J

The GM-SURFACE.5I/5I-H1 production retriangulation experiment is rejected and rolled back. Production bevel triangulation is restored to the exact pre-5I source; GM-SURFACE.5G duplicate-position mask reconciliation remains as a partial, independently validated correction.

Surface investigation now uses one integrated, incremental and cancellable suite. A canonical double-precision final-triangle kernel separates structural invalidity, numerical under-resolution, extreme slivers and valid conditioned geometry, and the same kernel feeds capture, uploaded-mesh accounting, logical-bevel analysis and audit-only triangulation scoring.

With two selected Generated Masses, the active selection is the visually suspect subject and the second selection is the reference. Each mesh is regenerated/captured once and then frozen while temporary renderers cross mesh, material asset and renderer `MaterialPropertyBlock` state. Audit-only shader variants isolate pre-light fields, direct/ambient response, stored/final/triangle normals and major material/light contribution families. Ordinary materials remain on the production shader variant because audit branches are compiled only under the local `_SURFACE_CAUSALITY_AUDIT` keyword.

Screen-space evidence is measured across projected, front-facing internal logical-bevel edges using asynchronous GPU readback. The terminal report names both geometry condition and causal ownership. The suite does not author a production correction.


## GM-SURFACE.5J-H1 compile-integration correction

The first GM-SURFACE.5J package was rejected by Unity compilation because `MassGenerator.TriangleQuality.cs` declared `IsFinite(Vector3)` inside the partial `MassGenerator` type while `MassGenerator.EdgeWear.Graph.cs` already owned the same exact private signature. The original static audit checked the changed implementation surface but failed to scan declarations across the complete partial type.

The triangle-quality helpers are now subsystem-scoped as `IsTriangleQualityFinite(Vector3)` and `IsTriangleQualityFinite(float)`. This is a private-name correction only: canonicalized token comparison against the rejected source confirms no classifier behavior change, and every public triangle-quality API and direct caller remains unchanged. A complete cross-partial scan now covers all `280` C# files under `Assets` (`6150` method declarations) and the `45`-file Generated Mass subtree (`1801` declarations), with `0` duplicate exact signatures in both scopes. The complete GM-SURFACE.5J-H1 static audit passes `41 / 41`; Unity 6000.5.0f1 compilation remains pending and is not claimed offline.

## GM-SURFACE.5J-H2 warning-free Unity 6.5 integration

The 5J render audit now uses Unity 6.5's unsorted `FindObjectsByType<T>(FindObjectsInactive)` overload and `Shader` instance property-introspection APIs. This removes the obsolete-API warnings without changing tournament cases, object inclusion, material-property reporting, scene state, production geometry, or ordinary shader behavior.

The first completed runtime tournament was single-subject only: the suspect mesh was valid but poorly conditioned, audit-only retriangulation found no improving candidate, and disabling all pixel variation reduced the measured internal-edge facet score by approximately 62.85%. Because no reference subject was supplied, causal ownership remains explicitly inconclusive and no production fix is selected.

## GM-SURFACE.5K same-mesh legacy lighting decomposition

The active surface investigation no longer assumes that the ordinary bevel is the faulty class. The same frozen Generated Mass mesh is rendered with the current HLSL material and the known-good legacy `Assets/Game/Demo/Materials/Stone/M_PixelStone.mat`, then partitioned by captured final-triangle provenance into source faces, ordinary bevels, junction/endpoint caps, corner-damage caps, and unclassified output.

Every case reports class luminance distributions, class-to-whole relative response, and source-parent-A / bevel / source-parent-B sample triplets. Matched main-direct and indirect environments distinguish source-face under-response from bevel over-response, while audit-only pre-light checkpoints isolate pixel variation, exposure semantic scaling, mottle, generated crevice/base/dirt layers, and final pre-light albedo. Area-weighted stored-normal main-light prediction and observed direct-to-prelight class response expose whether the relative source/bevel result contradicts the frozen mesh normals. Geometry, mesh channels, production shader variants, and serialized materials remain unchanged.

Diagnostic capture may invoke the existing named Stone Surface Profile and temporarily reassert its HLSL material during `GeneratedMass.Regenerate()`. GM-SURFACE.5K therefore snapshots every source renderer material slot plus global and per-slot `MaterialPropertyBlock` state before capture, restores it in a nested `finally` immediately after capture and again during finalization/cancellation/exception handling, and reports a terminal failure if the final fingerprint differs. Temporary parity materials and class-mask meshes are never assigned to the source renderer. A run started while the suspect already uses the legacy control terminates explicitly instead of producing a meaningless same-material comparison.

## GM-SURFACE.5K-H1 compile correction

`GeneratedMassSurfaceCausalityRenderAudit.BuildEnvironmentReport()` now formats the predicted source-face and ordinary-bevel main-diffuse values through its class-owned invariant-culture `Format(float)` helper. The rejected GM-SURFACE.5K source referenced the private `F(float)` helper owned by the separate diagnostic-suite class, causing two `CS0103` errors. This is a compile-only correction; the reported values, renderer material-state restoration, tournament cases, production geometry, and shader behavior are unchanged.


## GM-SURFACE.5L-DIAG bidirectional BRDF workflow parity

The completed GM-SURFACE.5K-H1 Unity run rendered `71` cases at `384×384` in `68.60918` seconds and verified same-mesh legacy/HLSL comparison plus complete source renderer material/property-block restoration. It did not causally close the defect: the terminal specular label had zero confidence, used one lighting direction, and reduced signed local behaviour to class aggregates. The user has observed both bevel over-response and bevel under-response, so one-way under-lighting is not an accepted interpretation.

GM-SURFACE.5L replaces isolated class-only mask renders with one depth-tested full-mesh triangle-identity pass using exact uploaded-index parity and a checksummed 16-bit triangle ID plus 8-bit checksum. Eight deterministic object-relative directional-light cases compare the legacy metallic-zero workflow, current HLSL specular F0 `0.16`, and temporary HLSL F0 `0.04` on the same frozen mesh. The two directions with the largest signed current-HLSL residual are rerun with actual material response and with a keyword-synchronized, diffuse-energy-matched no-highlight pair. Terminal ownership requires the complete eight-direction and two-direction adaptive evidence matrix; missing or corrupted triangle identity fails closed.

The update is editor-only. Production geometry, mesh streams, material assets, scenes, prefabs, named profile publication, and ordinary Pixel Surface shader behaviour remain unchanged. The only shared-shader addition is audit mode `25`, compiled exclusively under `_SURFACE_CAUSALITY_AUDIT`, which returns temporary per-triangle vertex-colour identity before material or lighting evaluation.

### GM-SURFACE.5M diagnostic boundary

The active surface-lighting investigation uses an audit-only, non-interpolated per-triangle identity stream on a frozen uploaded mesh. It performs deterministic directional legacy/current BRDF comparisons and may not alter production geometry, saved materials, shader defaults, scenes, or runtime rendering behaviour. A diagnostic verdict is invalid unless triangle identity, provenance, complete case accounting, finite outputs, keyword state, and source-renderer restoration all pass.

## GM-SURFACE.5N diagnostic ownership boundary

The final surface-lighting ownership audit uses a dedicated editor-only identity shader rather than extending the shared production varying interface. Lighting measurements are floating-point, per-triangle, same-mesh captures. The diagnostic retains one in-flight render resource set until asynchronous readback completes and has no active-gameplay execution, production shader variant, serialized material write, or persistent runtime allocation.

A terminal ownership label requires complete case accounting, validated identity for every tested view, finite floating-point data, keyword parity, and bounded visible-triangle coverage. Failure of any contract produces an explicit execution failure rather than a lighting conclusion.


## GM-SURFACE.5N dedicated ownership capture

The active ownership audit isolates visible triangle attribution in a dedicated editor-only shader rendered through a direct command buffer. It does not add a varying, keyword, or mode to the shared Pixel Surface production shader. Identity uses nonzero base-255 RGB digits in a linear, single-sample target; zero is background and any partially zero code is invalid. Lighting cases use sequential linear floating-point readback, retain their temporary resources until the request completes, and write both a terminal text report and per-triangle CSV. The 190 decision cases plus two auxiliary view-identity passes remain editor-only, incremental, cancellable, and fail closed. No production surface correction is selected until the complete matrix is reviewed.


## GM-SURFACE.5N-H1 compile correction

The 5N floating-point readback guard now tests each `Color` component directly with `float.IsNaN` and `float.IsInfinity`. The rejected source referenced a nonexistent `IsFinite(float)` helper and produced four Unity `CS0103` errors. This is a compile-only correction: identity rendering, the 190-case ownership matrix, CSV/report output, production geometry, materials, and runtime shader behaviour are unchanged.


## GM-SURFACE.5N-H2 diagnostic contract

The complete Generated Mass lighting-ownership audit no longer depends on `_SPECULARHIGHLIGHTS_OFF`. Its dedicated identity pass is followed by a controlled Lambert preflight that validates the relationship between identity pixels, floating-point lighting pixels, the published directional light, and stored triangle normals. Each lighting readback determines its identity-buffer vertical orientation by foreground intersection-over-union and fails closed below 0.995 IoU or above one-percent foreground-size difference.

Stage A uses black-albedo specular-only captures for the legacy material and both HLSL F0 values. Diffuse is derived per triangle as full response minus specular-only response, preserving signed RGB and luminance evidence without requiring a Shader Graph keyword. Stage B compares actual material response and generated/stored normals in the four worst directions; Stage C checks the two worst directions from two additional camera azimuths; Stage D closes indirect and actual-scene response. The editor-only contract is 209 decision cases plus three identity passes, 212 render passes total, with zero gameplay cost and no production asset mutation.

The 5N-H2 offline audit passes `85 / 85`. The implementation delta is limited to the two editor diagnostic classes and three canonical documents; the dedicated identity shader and shared production forward includes remain byte-identical to 5N-H1. Unity compilation and runtime execution remain pending.

## GM-SURFACE.5N-H3 pixelwise Lambert preflight

The 5N-H2 identity and lighting readback infrastructure is retained. Runtime evidence showed the current-view identity/light buffers align at 0.9999759 IoU with zero invalid identity pixels, while the Lambert gate failed because it compared per-pixel shader lighting against one CPU-averaged normal per triangle.

H3 replaces that approximation with one auxiliary audit-mode-14 stored-normal capture. The preflight compares the exact interpolated GPU normal field pixel-by-pixel against the controlled mode-12 Lambert response, evaluates configured and opposite light directions plus a best scalar fit, and only queues the existing ownership matrix after this exact contract passes. The 209 decision cases remain unchanged; the added normal capture raises auxiliary validation passes to four and total render/readback passes to 213.

This remains editor-only, incremental, cancellable, and asynchronous. Production materials, shared shaders, geometry, scenes, prefabs, profiles, layers, tags, and runtime rendering cost are unchanged.


## GM-SURFACE.5O cold-grey production parity trial — visually insufficient

The completed 5N-H3 matrix showed that F0 `0.04` matches the legacy neutral stored-normal BRDF and that generated whole-surface normal perturbation changes parity metrics. Those are valid measurements, but they do **not** define the active defect.

The 2026-08-07 side-by-side visual validation after 5O still shows the actual failure: individual HLSL source faces and bevels do not remain coherently ordered by surface orientation to the same light. Marked bevels remain darker than both parents where an intermediate response is expected, while other bevel/surface regions become brighter or darker in the wrong orientation order. The legacy material remains coherent in the same comparison.

Therefore 5O is **rejected as a root-cause correction for the active surface-orientation defect**. Its F0 and generated-normal changes may alter magnitude, but neither general darkness nor specular parity may be used as the problem definition or as closure evidence. Subsequent work must directly trace the value used for each fragment's directional response back to that fragment's actual surface orientation and compare parent–bevel–parent ordering.

## GM-SURFACE.5Q exhaustive orientation-stage attribution

The active surface-causality suite now preserves the completed H3 ownership matrix and adds a deliberately heavy Stage E whose primary evidence is measured surface orientation versus measured response, not whole-rock brightness. The Stage E basis uses the three already-validated camera views and all 27 controlled light directions.

For each view, the HLSL audit captures triangle/current/stored normal fields, raw generated-mass material channels, dirt/height/upwardness evidence, resolved exposure/crevice/base/dirt fields, nonlinear visual masks, response scalars, and every cumulative pre-light albedo checkpoint. For each light direction it captures the exact GPU stored-normal NdotL/attenuation/light vector, direct response after every cumulative pre-light stage, legacy actual-material PBR, HLSL production PBR, HLSL stored-normal PBR, and direct plus PBR one-layer ablations for tonal breakup, exposure, mottle, crevice, base, dirt, wet, frost, monolithic closure, overall tint, specular-zero, and all pre-light value authorities together.

The direct checkpoints are additionally validated pixel-by-pixel against the separately captured cumulative albedo and GPU NdotL/attenuation fields. Cross-case multiplication is rejected unless all three captures agree on identity-relative readback orientation and foreground pixel count. Source-face inversions are counted only when two measured NdotL values differ materially. Parent-bevel-parent envelope violations are counted only when the measured bevel NdotL itself lies between the two measured parent NdotL values within tolerance; a bevel that genuinely faces the light more directly than both parents is not an envelope failure.

The run intentionally favors evidence completeness over speed. It contains 3,677 counted decision cases plus four existing auxiliary validation/identity passes, for 3,681 GPU render/readback passes. GPU readback remains asynchronous and cancellable. Render checkpoints are persisted periodically rather than rebuilding the growing full text report after every Stage E pass. The dedicated orientation CSV is streamed to disk at finalization and preserves per-case/per-triangle provenance, geometry quality, CPU mask endpoints, luminance distribution, stored/authored/geometric normals, raw/resolved HLSL fields, cumulative albedo values, GPU NdotL/attenuation/light-vector evidence, and every Stage E result.

No production material value, mesh generation rule, normal-generation rule, scene/prefab/profile state, or ordinary shader variant is changed by 5Q. The shared forward include additions exist only inside the local surface-causality audit variant. Unity compilation and the full GPU run remain authoritative validation gates.
