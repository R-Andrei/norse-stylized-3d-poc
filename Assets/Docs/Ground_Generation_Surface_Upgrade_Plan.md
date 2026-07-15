# Ground Generation and Surface Upgrade Plan

## Current authoritative status — 2026-07-15

The GeneratedGround Inspector and Painted Accent production workstream is complete, Unity-validated, and accepted through GI-A1–GI-A4 and PA-B1–PA-B4.1. **GeneratedGround and the broader Ground visual roadmap are not complete.**

The active mission is to finish the restrained-stylized static Ground stack before runtime surface simulation. The active milestone is **V3M — Broad Macro Patch Completion**, audited in `Ground_Macro_Patch_Audit_and_Architecture.md`. **V4 — Contact / Edge Accents** remains architecturally accepted but is queued until V3M passes gameplay-camera visual acceptance.

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

1. Treat only the Inspector and Painted Accent production slice as closed.
2. Unity-validate V3M-A0's slim macro diagnostics without changing normal lit Ground output.
3. Implement V3M-A1 as a replacement shader macro-region proof only after the raw field and weighted influence are captured for Snowfield and Grassland.
4. Resume V4 Contact / Edge Accents only after broad macro composition is visually accepted from the gameplay camera.

---

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
| 6 | Patch V3M — Broad Macro Patch Completion | **Active milestone.** Replace visually insufficient broad noise with deliberate, readable, restrained macro-region composition. See `Ground_Macro_Patch_Audit_and_Architecture.md`. |
| 7 | Patch V4 — Contact / Edge Accent Layer | **Queued after V3M.** Add localized accent response near shores, rocks, modifier boundaries, paths, banks, and object contact zones. Existing semantic masks provide style context; the audited production geometry is a separate generated contact field. See `Ground_Contact_Edge_Accent_Audit_and_Architecture.md`. |
| 8 | Patch V5 — Sparse Motif Layer | Add reusable sparse marks such as chips, cracks, scuffs, stains, snow scratches, stones, or debris hints. Avoid stamp spam. |
| 9 | Patch V6 — Feature Stack Authoring Polish | Add richer warnings, cost summaries, duplicate/combination guidance, and editor UX after more stack layers exist. |
| 10 | Later | Ground Surface Runtime State Stub | Revisit runtime wetness, snow depth, compression, footprints, and disturbance after the static visual stack is accepted. |
| 11 | Later | Footprints / Rain / Puddles / Grass Integration | Build on the runtime state contract only after the visual doctrine is stable. |
| 12 | Future | Mixed Terrain / Profile Blending | Add explicit support for blended surface families such as snow over mud, rocky scrub over soil, or worn path through snow. |

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
UV2.y = shore influence
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
