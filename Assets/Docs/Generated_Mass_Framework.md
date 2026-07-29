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

The permanent bevel-shading evidence suite keeps capture, geometry-fingerprint, source-face preservation, degenerate parity, logical mapping, shared-edge continuity, normal, parent-cone, and upload checks. Clean passes use a compact report; failures retain full per-bevel evidence.

## Residual bevel-shading status — GM-SURFACE.5G-H2

GM-SURFACE.5G remains a partial production correction: shared-edge generated-mask values are continuous and geometry is preserved, but low-light visual faceting remains on a subset of bevels. The active comprehensive audit evaluates value and structural gradients, triangle quality, geometric and render normals, tangents, parent envelopes, direct-light sensitivity, upload parity, and immutable geometry in one incremental run. Shader-side procedural noise, feature-atlas filtering, ambient/SH, shadows, SSAO, PBR specular and post-processing remain explicitly identified as visual-isolation follow-ups when CPU evidence is insufficient.

## GM-SURFACE.5I — certified bevel triangulation quality

Finalized one-surface polygons no longer accept the first stable boundary fan. Production evaluates the best stable fan and the complete certified general triangulation, then chooses deterministically by maximum aspect ratio, minimum internal angle, minimum area, authored-normal agreement, and stable tie-breaking. Existing tolerance-collinear reinsertion remains a last fallback only when no complete certified candidate exists. Polygon boundaries, selected bevels, render normals, provenance, surface groups, material channels, and shaders are unchanged. This is geometry-quality cleanup and does not close the separate material/shader-dependent residual shading investigation.

### GM-SURFACE.5I-H1 final-emission safety gate

Quality-aware bevel retriangulation is fail-closed. Every selected triangle must pass final-position duplicate, coincidence, finite-value, scale-relative area, winding, and authored-normal checks. A lower aspect ratio can never justify an invalid emitted triangle. Runtime validation reports uploaded-mesh and logical-bevel degenerate counts from the same accepted build.
