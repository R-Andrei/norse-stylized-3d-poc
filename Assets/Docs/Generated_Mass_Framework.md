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

No production material value, mesh generation rule, normal-generation rule, scene/prefab/profile state, or ordinary shader variant was changed by 5Q. The shared forward include additions exist only inside the local surface-causality audit variant. The 2026-08-07 Unity run completed the full 3,677-case decision matrix and all 3,468 Stage E orientation cases with no completeness failure; that evidence is now the causal basis for 5R.

## GM-SURFACE.5R orientation-coherent material-response baseline

GM-SURFACE.5Q completed the exhaustive orientation-stage capture and changed the ownership conclusion from a generic lighting problem to a pre-light material-value problem. The direct-light product itself is validated: captured albedo × captured `NdotL` × attenuation reproduces every cumulative direct checkpoint exactly, while the Lambert preflight agrees with the GPU stored-normal field to approximately `1e-7` normalized RMSE.

The first real source-face inversions appear when Generated Mass exposure/upwardness/height is allowed to scale pre-light value. Crevice and base/contact darkening add large additional inversion batches. Tonal breakup leaves source faces ordered but creates the dominant new bevel-only error family. Disabling all pre-light value authorities removes every source-face inversion and returns bevel error to the known constant-base classifier floor; disabling specular changes nothing.

The framework rule is therefore explicit: **Generated Mass semantic masks are material descriptors, not illumination.** Exposure no longer owns Generated Mass pre-light luminance; crevice and base/contact no longer darken direct-light albedo toward fixed targets; convex-bevel tonal breakup suppresses interpolated vertex-R, broad, and warp authority and retains topology-independent world-position pixel-cell variation. Ground semantic rendering is unchanged.

The cold-grey parity baseline uses the legacy pixel amplitudes and disables HLSL-only broad/warp tonal values. F0 `0.04` and the temporary cold-grey generated-normal bypass remain present from the earlier trial but are not considered causal closure for the active orientation defect.

5R is visually accepted as of 2026-08-09. Preserve the 5Q evidence and the semantic-mask/illumination separation as permanent architecture; do not return to whole-object brightness or specular tuning as the former defect definition.


## GM-SURFACE.5S low-light directional form readability

5S is a separate illumination-layer feature for the case where weak or cloud-attenuated direct Sun leaves correct geometry insufficiently readable under the existing low-frequency indirect response. It does not reopen 5R.

Generated Mass may shape its existing baked-GI contribution by the resolved fragment normal's signed facing to the actual URP main-light direction. The shaping is bounded, multiplicative, and non-emissive. It uses the existing `_ShadowAmbientStrength` and `_DiffuseWrap` material values; zero strength is exact 5R parity. Ground does not consume the helper.

The permanent rule remains: semantic masks and geometry classifications describe material state, not incident illumination. Exposure/upwardness/height, crevice, base/contact, dirt, and bevel identity cannot become substitute lighting fields. Weather remains responsible for the real Sun cookie/shadow attenuation; 5S uses base Sun direction only as an indirect form cue and does not sample or alter Weather data.

The runtime addition is fixed fragment ALU with no new texture fetch, buffer, pass, draw, dispatch, CPU update, generated data, or persistent memory. GPU profiling and Unity visual validation remain pending before acceptance. Source implementation is complete and the offline static audit passes `58 / 58`; Unity compilation, visual validation, exact local-package verification, and GPU profiling remain pending.

Generated Mass now owns the authoring location for 5S strength. The component publishes **Low-Light Form Strength** through its existing renderer `MaterialPropertyBlock` path to `_ShadowAmbientStrength`, so a mass keeps one object-level readability-strength choice across compatible stone materials. This patch does not add a corresponding object-level Wrap control; `_DiffuseWrap` remains material-owned unless a future approved patch proves separate wrap authoring is necessary. The source implementation is complete for this authoring handoff; Unity compile and scene validation remain pending.

## GM-SURFACE.5S2 extended low-light form strength

User validation confirms that GM-SURFACE.5S1 is wired correctly but that the former maximum strength `1.0` remains too weak in very uniform low-light conditions. 5S2 preserves the same illumination-derived architecture and extends the Generated-Mass-owned strength range to `0..2`.

The forward response remains baked-GI-only and continues to use the bare main-light direction. Values in the original `0..1` interval are algebraically unchanged. The wider range is implemented by replacing the former unit saturation of `_ShadowAmbientStrength` with a clamp to `0..2`; the directional coefficient and Wrap behavior do not change.

This patch intentionally does not add a position-aware shadow/cookie query solely to detect local darkness. URP's position-aware main-light path performs shadow evaluation and can sample the main-light cookie; duplicating that work before the existing PBR call would add active-gameplay fragment cost. Because 5S shapes only indirect GI, its relative visual contribution already increases naturally as genuine direct lighting becomes weaker.

With the unchanged directional target bounded to `0.60..1.40`, maximum strength `2.0` keeps the final GI multiplier bounded to `0.20..1.80`. The response therefore remains multiplicative, non-emissive, and positive across the approved range. If `2.0` is still insufficient, a further response-curve redesign requires a separate decision rather than silently extending the current signed multiplier beyond its safe range.

5S2 source implementation is complete and the offline static/cross-subsystem audit passes `35 / 35`. Unity compilation and same-scene visual validation at strengths `0`, `1`, and `2` remain pending and are the next authoritative gates.

## GM-SURFACE.5S3 Sun-orthogonal face separation

5S2 user validation shows that the available bright-versus-dark amplitude is now sufficient and can become excessive, while several differently oriented faces still collapse to nearly the same low-light value. The remaining limitation is dimensional rather than scalar magnitude: the primary helper maps each resolved normal to one Sun-projection value, so equal or near-equal Sun projections remain equal or near-equal regardless of Strength.

5S3 adds one independent Generated-Mass-owned **Low-Light Face Separation** control. The default is `0`, which preserves exact 5S2 behavior. The secondary cue uses the stable mesh geometric face normal and a view-to-camera axis projected perpendicular to the real main-light direction. It is applied only to baked GI and only as a bounded tie-breaker for faces whose current wrapped Sun-facing response has headroom. Real direct lighting remains entirely Sun-authoritative.

The two controls have distinct jobs: **Low-Light Form Strength** controls the primary Sun-facing amplitude, while **Low-Light Face Separation** distributes similarly Sun-facing faces within the same established response envelope. Authors can therefore lower Strength to reduce extreme bright/dark contrast while raising Face Separation to increase the number of visibly distinct planes.

The camera-relative axis is deliberately view-rotation-dependent but view-position-independent. This is a stylized readability layer, not a claim about additional physical incident light. Semantic masks, feature identity, random face values, height, world-up, crevice/base state, and generated topology remain prohibited as fake illumination authorities.

The new term is fixed fragment ALU only. It adds no new main-light lookup, SH sample, texture/cookie/shadow sample, mesh stream, buffer, pass, draw, dispatch, CPU-per-frame update, allocation, or generated storage. The larger Generated Mass Inspector redesign is deferred; 5S3 adds no custom-Inspector restructuring. Source implementation is complete and the offline exact-scope/cross-subsystem audit passes `40 / 40`; Unity compilation, visual tuning, camera-rotation validation, and GPU profiling remain pending.

## GM-SURFACE.5S4 generation-time face-tone separation

User validation rejects the 5S3 camera-axis tie-breaker. Its object-level control plumbing remains valid, but its runtime equation does not create enough independent face identities: bright faces are suppressed by the 5S3 headroom term and darker visible faces tend to move together. 5S4 therefore retires camera-relative face separation rather than increasing that response.

Low-Light Face Separation now scales a deterministic generation-time tonal identity carried by each logical planar face. Final triangles are grouped by shared quantized edges and near-equal flat render normals. Non-convex source/cap faces receive a five-level symmetric base palette chosen to maximize tonal distance from effective neighbours; source faces separated by an ordinary convex transition are treated as neighbours for this assignment. ConvexBoundary transition/bevel groups do not receive independent palette identities: their raw tone is derived from adjacent non-convex tones so the bevel remains inside its parent/neighbor tonal envelope. The completed rock is area-weight recentered to approximately zero mean and scaled down only if required to stay inside signed `[-1,+1]`.

Generated Mass stores the signed result in its previously unused `UV2.w` component. Ground retains its separate standing-water-potential meaning for its own `UV2.w`; the face-tone interpretation is Generated-Mass-only. Material-mask inheritance does not modify this component. No new mesh stream or material property is introduced.
The production-generation contract version advances with 5S4 so existing Generated Mass instances invalidate pre-5S4 generated meshes and rebuild the signed face-tone component through the normal generation/synchronization path.

The shader applies at most `±0.16` indirect-GI multiplier at Face Separation `1`, then clamps the combined low-light multiplier to the existing `0.20..1.80` envelope. Face Separation `0` is exact 5S2 parity. The layer is intentionally stylized but remains bounded, zero-mean at generation time, indirect-only, and independent of material profile.

Runtime fragment cost is lower than 5S3 because camera/view-axis construction and normalization are removed. The additional intelligence runs only during mesh generation and its temporary graph data is discarded immediately after face-tone compilation. Source implementation is complete and the offline static/cross-subsystem audit passes `58 / 58`; Unity compilation and visual tuning remain pending.

## GM-SURFACE.6A structural material-response baseline

GM-SURFACE.6A activates the existing packed `ConvexBoundary` and `CornerChipCap` semantic contributions as two independent material-response modules. This is deliberately **not** a structural-normal update: whole-rock normals, geometric normals, chip-cap faceting, and all normal blending remain exactly as in the accepted pre-6A baseline.

The convex module reuses the existing Generated Mass edge-response strength and softness authoring, but its old UV2-driven albedo lift/tint behavior remains quarantined. The semantic `ConvexBoundary` weight instead modestly reduces the amplitude of the already-evaluated world-position pixel variation and adds a small smoothness increase. The module therefore reads as a cleaner/worn structural transition without becoming an independently lit or painted bright strip.

The chip module adds one Generated-Mass-owned `Chip Interior Response`, range `0..1`, default `0.60`. The semantic `CornerChipCap` weight modestly increases the amplitude of the existing pixel variation and reduces smoothness so exposed cap faces can read as rougher fractured interior stone. It introduces no fixed color, darkening, brightening, emission, or normal perturbation.

Both modules evaluate only the existing primary/secondary structural contribution slots. Runtime work is fixed scalar ALU and does not scale with total feature count. No new texture/noise evaluation, mesh stream, buffer, pass, draw call, light query, per-frame CPU update, or persistent generated data is introduced.

The permanent lighting rule remains unchanged: semantic feature identity may alter material character, but it does not decide which surface is illuminated. Real geometric/resolved normals and URP lighting remain the illumination authority.

Source implementation completed for 6A and its offline static/cross-subsystem audit passed `68 / 68`, but Unity visual validation then found no observable response at either master-control extreme. The weak fixed 6A coefficients are therefore superseded by GM-SURFACE.6A.1; the packed semantic architecture remains retained.


## GM-SURFACE.6A.1 structural response visibility correction

The initial GM-SURFACE.6A visual trial produced no observable difference at either master response extreme. 6A.1 keeps the structural semantic architecture but replaces the weak fixed coefficients with explicitly authored response profiles.

`Convex Surface Response` and `Chip Interior Response` remain the `0..1` semantic/intensity gates. Generated Mass now also owns four cross-material profile values: **Convex Variation Multiplier** (`0..2`, default `0.10`), **Convex Smoothness Offset** (`-0.40..+0.40`, default `+0.20`), **Chip Variation Multiplier** (`0..3`, default `2.00`), and **Chip Smoothness Offset** (`-0.40..+0.40`, default `-0.20`). A multiplier of `1` and an offset of `0` are neutral, allowing tonal variation and smoothness to be tested independently.

The structural variation multiplier now scales the complete existing tonal offset after the accepted 5R bevel restrictions. It does not restore vertex/broad breakup on convex faces; it only scales whatever tonal response is already valid at that fragment. Smoothness receives the authored signed semantic offsets directly and is saturated at the final PBR value.

This remains a material-response-only feature. Geometry, normals, generated semantics, lighting direction, low-light face-tone response, Ground, Weather, and the historical bevel albedo-lift/tint quarantine are unchanged. Runtime remains fixed scalar ALU with no additional sampling, stream, pass, draw, loop, buffer, or per-frame CPU work.

The deliberately strong defaults are also a diagnostic boundary: if Unity still shows no response at full master values, future work must investigate semantic transport/shader consumption rather than increasing material-response magnitude again.

Source implementation is complete for 6A.1 and the offline static/cross-subsystem audit passes `106 / 106`. Unity compilation, visual module-independence validation, and target-GPU timing remain pending authoritative runtime gates.

## GM-SURFACE.6A.2 structural semantic transport diagnostics

Unity validation of 6A.1 produced literally zero visible change across every structural-response control, including after regeneration. This activates the 6A.1 diagnostic boundary: response strength is no longer to be increased until the semantic path is proven.

6A.2 adds no new material behavior. It extends the existing live Render Mesh Audit to inspect the final render mesh's structural UV4/TEXCOORD4 channel and current renderer property-block values. The audit reports encoded type histograms, ConvexBoundary and CornerChipCap vertex/triangle counts and strength ranges, plus both master responses and all four 6A.1 profile values. An all-zero channel is called out explicitly because it makes the entire structural response inert while still satisfying the old packed-vector validity test.

Two temporary Generated Mass Surface Debug modes directly visualize the active forward-fragment data: **Structural Semantics** shows raw packed semantic strength, while **Structural Resolved Response** shows the same data after the two master response gates. Convex is yellow, chip cap is cyan, and zero is black. These views return before normal/material/PBR work, making them binary transport tests rather than artistic diagnostics.

The decision boundary is strict: zero semantics in the mesh audit means producer/packing diagnosis; non-zero mesh semantics plus a black raw shader view means TEXCOORD4/variant diagnosis; a working raw view plus black resolved view at master `1` means property-block/uniform diagnosis; working raw and resolved views prove semantic/master plumbing and move the fault downstream to material application.

The patch reuses the existing mask-debug uniform and structural stream. It adds no shader property/CBUFFER field, varying, stream, texture, buffer, pass, draw, allocation, geometry, normal, or response-coefficient change. Offline exact-scope/static/cross-subsystem audit passes `97 / 97`. Unity 6000.5.0f1 compilation and live diagnostic validation remain pending authoritative gates because Unity is unavailable in this environment.

## GM-SURFACE.6A.3 structural material-response membership correction

The 6A.2 live mesh audit showed that structural type transport can be present while packed feature strengths are much smaller than the intended artistic response range. GM-SURFACE.6A.3 therefore separates **semantic membership** from **material-response magnitude**.

For the structural material module, a non-zero `ConvexBoundary` or `CornerChipCap` slot above the structural epsilon is treated as membership in that class. The corresponding Generated Mass master control then owns the response magnitude. Packed feature strength is preserved unchanged for raw diagnostics and future systems that intentionally need continuous structural intensity.

The existing 6A.1 variation/smoothness profile controls, semantic packing, geometry, normals, lighting, materials, Ground, Weather, and low-light face-tone behavior are unchanged. The live render-mesh audit now warns when a non-zero master response cannot affect the audited mesh because that semantic type is absent.

Unity compilation and live visual validation remain pending authoritative gates.


## GM-SURFACE.6A.4 absolute structural variation authoring

6A.3 proves semantic membership and master-response plumbing can drive the structural material path, but post-patch visual validation shows the 6A.1 variation-multiplier model remains poorly suited to convex surfaces. A multiplier of `1` is neutral by definition, and 5R already suppresses broad/vertex breakup on convex transitions before the multiplier sees the tonal offset.

6A.4 therefore replaces **Convex/Chip Variation Multiplier** authoring with **Convex/Chip Variation Strength**. The stored numeric values/ranges remain `0..2` for convex and `0..3` for chip through serialization migration, but the numbers now describe absolute structural tonal amplitude: one strength unit contributes up to approximately `±0.10` before the existing Pixel Effect Strength.

The shader reuses the already-computed signed pixel-cell variation and adds it as an independent structural tonal term after the unchanged base tonal offset. This means convex/chip material breakup no longer depends on how much ordinary tonal variation survived previous bevel restrictions. If both semantics overlap at a fragment, the larger structural amplitude wins rather than summing both.

Master response controls and signed smoothness offsets remain independent. Setting Variation Strength to `0` disables only structural tonal breakup; setting Smoothness Offset to `0` disables only smoothness response. No fixed brightening/darkening, tint, emission, normal change, geometry change, semantic change, new sampling, stream, buffer, pass, draw, or per-frame CPU work is introduced.

Source implementation is present; offline validation status is recorded in the canonical implementation checklist. Unity compilation and one direct visual `0 -> max` variation-strength comparison remain pending authoritative gates.
