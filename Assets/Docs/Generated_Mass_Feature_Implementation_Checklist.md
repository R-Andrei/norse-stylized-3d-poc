# Generated Mass Feature Implementation Checklist

Status: active concise checklist

## Completed and frozen

- [x] Base Generated Mass deterministic construction.
- [x] Ordinary bevel candidate generation and bounded construction.
- [x] Corner-chip transaction geometry.
- [x] Post-chip ordinary candidate discovery.
- [x] Ranked one-loser-per-retry incompatibility reduction.
- [x] Retirement of combinatorial conflict-frontier search.
- [x] Focused pathological regression suite.
- [x] Final full incremental-selection suite pass.
- [x] GM-SURFACE.1 documentation consolidation.
- [x] Explicit `BaseGeometryOnly` / `ProductionSurfaceFeatures` routing boundary.

## GM-SURFACE.2 — production geometry promotion

- [x] Share the certified construction path between production and diagnostic callers.
- [x] Implement player-safe `ProductionSurfaceFeatures` behavior.
- [x] Route enabled ordinary generation through certified chips/bevels.
- [x] Preserve cheap deterministic base-only output when disabled.
- [x] Preserve certified chips with zero bevels.
- [x] Preserve deterministic base fallback.
- [x] Increment the production-generation contract through existing invalidation ownership.
- [ ] Confirm in Unity that existing scene rocks regenerate without scene edits.

## GM-SURFACE.3 — semantic contributions

- [x] Define semantic feature type and response role.
- [x] Emit `ConvexBoundary` and `CornerChipCap` generation-time contributions.
- [x] Reserve `ConcaveBoundary`, `Fracture`, `ImpactDent`, and `MaterialSeam` semantics.
- [x] Resolve deterministic primary and secondary contributions.
- [x] Define deterministic overflow/overlap policy.
- [x] Keep contribution storage generation-local in `TriangleSoup`; no persistent feature database or runtime list was introduced.

## GM-SURFACE.4 — packed mesh channels

- [x] Audit colour and UV stream ownership.
- [x] Preserve existing colour and UV2 material masks.
- [x] Avoid `TEXCOORD3`, which remains occupied by the retired diagnostic feature-atlas path.
- [x] Store primary/secondary type and strength in one `TEXCOORD4` `Vector4`.
- [x] Hold the production payload to exactly 16 bytes per final render vertex.
- [x] Derive role and current direction instead of persisting them.
- [x] Add finite/range/type/disabled-slot validation before mesh upload.
- [x] Validate the channel again after Unity mesh upload.
- [x] Include the stream in exact deterministic mesh-data comparisons.
- [ ] Confirm in Unity that base, bevel, and chip meshes expose complete `TEXCOORD4` data.

## GM-SURFACE.5 — whole-rock normals

- [x] Add shared textureless whole-rock normal response.
- [x] GM-SURFACE.5A: make strength true slope amplitude and suppress legacy broad masks on generated bevel/chip surfaces.
- [ ] GM-SURFACE.6: consume semantic stream for explicit convex accents and exposed-chip response.
- [ ] Add optional shared micro detail.
- [ ] Add deterministic per-rock transform/seed variation without unique materials.
- [ ] Provide low and standard cost tiers.
- [ ] Use correct normal blending.

## GM-SURFACE.6 — first structural responses

- [ ] Implement convex-boundary response.
- [ ] Implement corner-chip-cap response.
- [ ] Evaluate at most two structural slots.
- [ ] Confirm one module can be disabled without affecting the other or whole-rock detail.
- [ ] Confirm cost does not scale with total source feature count.

## GM-SURFACE.7 — concave creases

- [ ] Define geometry-versus-surface threshold.
- [ ] Detect or author concave boundaries.
- [ ] Compile into existing contribution slots.
- [ ] Implement concave normal/darkening/roughness response.
- [ ] Avoid new streams unless the existing contract is proven insufficient.

## Permanently rejected

- [x] Per-rock feature atlases.
- [x] Generated per-rock normal maps.
- [x] Permanent array slice per rock.
- [x] Unbounded per-pixel feature loops.
- [x] One renderer/material pass per feature.
- [x] Edge-wear-only normal architecture.
- [x] Persistent per-rock feature database.

## GM-SURFACE.5B — completed pending Unity validation

- [x] Preserve faint whole-rock normal readability outside direct sunlight.
- [x] Keep the shadow-side response multiplicative and non-emissive.
- [x] Apply a deterministic rounded-normal proxy only to convex contributions.
- [x] Keep source faces planar and corner-chip caps faceted.
- [x] Add a minimal light-dependent convex lift without a new control row.
- [x] Add no texture, mesh stream, draw call or variable feature loop.
- [ ] Confirm the marked vertical bevel no longer reads as one uniform dark band.
- [ ] Confirm `Normal Strength = 0` remains exact visual parity.
- [ ] Confirm shaded response is visible but does not flatten form or glow.


## GM-SURFACE.5C — Generated-face material-mask inheritance

The dark-band bevel defect was confirmed to be a pre-light material-mask ownership defect, not a mesh-normal defect. Generated bevel and chip faces must not recompute Exposure, Crevice/Base, or DirtDeposit from their own new face normal, and the shader must not suppress those masks as a compensation. During final mesh emission, generated triangles now inherit the source-face material-mask samples present on their shared boundaries. Shader-side convex/chip mask suppression and the GM-SURFACE.5B convex lighting compensation are retired. Whole-rock normal response remains independent.

## GM-SURFACE.5D — completed generated-face material-mask correction

- [x] Remove triangle-wide averaging of all coincident source samples.
- [x] Resolve inherited masks per generated boundary vertex.
- [x] Interpolate unresolved interior generated vertices from their own resolved triangle boundary.
- [x] Make Generated Mass rendering consume compiled `Color.g`, `Color.b`, and `UV2.y` masks.
- [x] Stop recomputing Generated Mass crevice/base and dirt from generated-face orientation.
- [x] Preserve whole-rock normal response as an independent layer.
- [ ] Validate the previously bright lower bevel and darker upper bevel under a rotating directional light.

## GM-SURFACE.5E — raw lighting parity

- [x] Disable production bevel albedo lift and tint without deleting serialized authoring values.
- [x] Bypass normal-dependent pre-PBR value shaping for Generated Mass fragments.
- [x] Return raw `UniversalFragmentPBR` output for Generated Mass fragments.
- [x] Quarantine post-PBR light-colour reconstruction and shadow-side normal readability.
- [ ] Confirm bevel response tracks light direction comparably to `M_PixelStone` / `SG_PixelSurfaceLit` with whole-rock normal strength zero.
- [ ] Reintroduce stylization only after parity is visually confirmed.

## GM-SURFACE.5G — Logical-bevel material-mask continuity

- [x] Reconcile duplicated ordinary-bevel vertices by provenance and position.
- [x] Restrict production writes to `Color.g`, `Color.b`, and `UV2.y`.
- [x] Preserve source-face masks.
- [x] Add pre/post immutable-channel fingerprints during the bevel-shading diagnostic run.
- [x] Add pre/post degenerate-triangle identity parity and exclude matched degenerates from angular mismatch decisions.
- [x] Tighten logical-bevel shared-edge mask continuity threshold to `0.00001`.
- [x] Validate the complete evidence suite on two distinct accepted production meshes with exact geometry parity and zero internal mask jumps.
- [ ] Preserve visual acceptance as a scene-level check when the affected material or lighting baseline changes.

## GM-SURFACE.5G-H1 — Validation closure and diagnostic cleanup

- [x] Record two-rock validation evidence in the canonical architecture.
- [x] Replace the generic clean verdict with `LOGICAL_BEVEL_MASK_CONTINUITY_VALIDATED`.
- [x] Keep successful reports compact.
- [x] Retain complete per-bevel evidence for every failed invariant.
- [x] Preserve all permanent geometry, mapping, mask, normal, degenerate, and upload regression checks.
- [x] Confirm no separate obsolete DIAG1 implementation file remains.
- [ ] Validate Unity compilation and one compact-success report in Unity 6000.5.0f1.

## GM-SURFACE.5G-H2 — Comprehensive residual bevel-shading audit

- [x] Preserve accepted-build capture and geometry regression guards.
- [x] Add piecewise-linear gradient analysis for surface variation, exposure, crevice, dirt and structural channels.
- [x] Add triangle area/aspect, geometric-facet, render-normal, tangent and parent-envelope checks.
- [x] Add nominal/high direct-response sensitivity evidence and material-property inventory.
- [x] Rank per-bevel and per-edge evidence in one cancellable suite.
- [ ] Compile in Unity 6000.5.0f1.
- [ ] Run on a visually failing rock under the low-light condition and submit the complete report.
- [ ] Use existing mask-debug modes to isolate shader-side paths only when the CPU report does not identify a sufficient cause.

## GM-SURFACE.5I — Certified bevel triangulation quality

- [x] Evaluate all stable boundary-fan anchors.
- [x] Evaluate the complete general triangulation even when a stable fan exists.
- [x] Rank complete candidates by maximum aspect ratio, minimum angle, minimum area, authored-normal agreement, and deterministic tie-breaking.
- [x] Preserve exact polygon boundaries, provenance, surface groups, render normals, and material/structural vertex values.
- [x] Keep tolerance-collinear reinsertion as last fallback only.
- [ ] Compile in Unity 6000.5.0f1.
- [ ] Re-run the comprehensive suite on the two known stones and compare degenerate, sliver, aspect-ratio, and geometric-facet-risk results.
- [ ] Confirm no open-edge, non-manifold, T-junction, mapping, upload, source-mask, or visual silhouette regression.

- [x] GM-SURFACE.5I-H1: certify exact emitted bevel triangles fail-closed; reject duplicate/coincident/scale-degenerate candidates.
- [x] GM-SURFACE.5I-H1: add uploaded-mesh, captured ordinary-bevel, and per-logical-bevel degenerate accounting with terminal mismatch verdicts.
- [ ] Validate H1 on the same two stones: zero uploaded degenerates, zero logical-bevel degenerates, zero accounting mismatch, no 180-degree zero-normal artifact.
