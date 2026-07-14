## 2026-07-14 — Active PA-P4 External-Conflict and Reconciliation Contract

Authoritative projected-cluster construction maintains a deterministic 0.50 m fixed-grid index over each already accepted glyph's external-conflict-expanded bounds. Candidate members query their own touched cells, deduplicate glyph indices, sort them into original accepted-list order, and then retain the exact bounds, segment-intersection, and point-clearance authority. The grid never declares validity; it only proves that glyphs occupying no shared indexed cell cannot have overlapping expanded bounds.

Construction order establishes a reusable validation history. Initial independents exist before every cluster; each accepted cluster is checked against them and all earlier clusters; every later cluster is checked against all prior accepted glyphs. Because external overlap is symmetric, final reconciliation only needs relationships introduced after construction: unused participant independents and independent fallbacks created by a reconciliation removal. Surviving cluster records retain the late-independent count already checked, so subsequent passes test only newly introduced relationships while preserving descending cluster order and one-removal-per-pass behavior.

Diagnostics expose spatial queries, cells, unique candidate glyphs, full-list comparisons avoided, exact external workload, reconciliation clusters examined, previously validated relationships skipped, new independent relationships tested, and legacy full-list work bypassed. Candidate ordering, cluster geometry, quotas, attempts, exact overlap thresholds, fallback content, and coverage are unchanged.

## 2026-07-14 — Accepted PA-P3 Contact-Candidate Execution Contract

PA-P3 was Unity-validated at Tightness `1.00` with the complete authoritative baseline unchanged: 318 projected marks, pair/triplet 78/48, nine pair shortfalls, 1,127 attempts, 20,356 baked segments, and 5,707 covered texels. It reduced complete regeneration from 3,705.96 ms to 1,900.85 ms and contact solving from 2,121.62 ms to 379.78 ms.

Projected contact selection preserves the authoritative V3J.4F3 score, pair/triplet step-retention rules, near-collinear endpoint rule, near-parallel body-blend authority, and swept-width overlap authority. PA-P3 changes evaluation order only. Once a candidate terminal is placed on the valid contact side, step retention, pair-local anti-collinearity, and the exact existing score are resolved before polyline interaction checks. A candidate whose score cannot beat the current best fully valid candidate is not geometrically evaluated because it cannot become the selected result. The current best score is never updated by an unvalidated candidate.

Internal swept-width validation reuses per-segment endpoints, widths, maximum width, and unexpanded bounds from the per-build scratch object. The accepted PA-P1 conservative bounds decision and exact intersection / closest-distance / interpolated-width authority are unchanged. Candidate ordering, quotas, donor selection, contact geometry, attempt budgets, fallbacks, reconciliation, point counts, and coverage remain unchanged.

Diagnostics distinguish pre-geometry step rejection, pre-geometry noncompetitive-score rejection, and geometric-validation submission. Because step classification now precedes geometry, rejection-category counts may shift while accepted output remains identical; authoritative parity is judged by projected marks, cluster/layout quotas, attempts, geometry, and coverage.

## 2026-07-14 — Accepted PA-P2 Near-Parallel Execution Contract

Near-parallel body-blend authority remains the exact V3J.4F3.2 segment rule: at most 22 degrees of tangent difference, centreline distance inside 88% of combined average half-width, and accumulated shared-axis overlap of at least 0.025 m or 14% of the shorter authored member. PA-P2 changes execution only.

The per-build projected-glyph scratch now reuses right-segment metadata containing endpoints, normalized direction, length, average half-width, and unexpanded bounds. Before alignment and exact distance work, an axis-gap test skips a segment pair only when its X or Y bounds separation is strictly greater than the same exact near-parallel clearance. All uncertain pairs continue through the existing alignment, exact distance, interval-overlap, accumulation, and early-return logic. Candidate ordering, shape rules, quotas, contact placement, fallbacks, reconciliation, and coverage remain unchanged.

Compact diagnostics expose method calls, metadata preparation, segment-pair funnel counts, exact distance passes, interval evaluations, and detected blends without per-segment timing or Console output.

Unity validation at Tightness `1.00` preserved 318 projected marks, pair/triplet 78/48, 1,127 attempts, 536 near-parallel rejections, 3,891 swept-width rejections, 20,356 baked segments, and 5,707 covered texels while reducing near-parallel validation by 89.2% and complete regeneration by 55.2% relative to PA-P1.

## 2026-07-14 — Accepted PA-P1 Conservative Internal-Overlap Broad Phase

Projected cluster overlap authority remains the exact V3J.4F3.2B variable-radius segment test. PA-P1 adds only a conservative per-segment rejection before that narrow phase. Maximum possible clearance is bounded by the maximum endpoint half-width on each segment, the existing body/contact clearance fraction, and the existing minimum clearance. A segment pair is skipped only when an unexpanded X or Y bounds gap is strictly greater than that maximum; all uncertain pairs retain the exact centreline-intersection, closest-segment, interpolated-width, and tolerated-clearance test.

The broad phase cannot accept geometry and cannot loosen contact safety. It only proves that a segment pair is too far apart to reach the current swept-width threshold. Candidate ordering, authoritative quotas, layout mix, contact placement, quality guards, surface/domain validation, fallback, reconciliation, glyph geometry, and coverage remain unchanged. Compact counters and method-level timings expose broad-phase effectiveness without per-segment timing or Console spam.

## 2026-07-14 — V3J.4F3.3A Copyable Cluster Diagnostics

Painted Accent diagnostics now have one cache-only `Copy Full Generation Report` action. It combines current regeneration timing, the latest completed Painted Accent stage timings, the surface proposal/validity funnel, authoritative projected-companion diagnostics, and coverage statistics without triggering generation or Console output. Projected audit text is always available in the Inspector and is no longer coupled to the visual projected-glyph overlay.

Timing ownership is explicit. The current operation reports detail only for stages actually executed; retained `SurfaceStrokes` and `ProjectedGlyphs` timings are shown in a separate last-completed section. Projected cluster allocation is instrumented as a wall-time domain with measured quota/spec, participant-selection, donor-selection, prototype, contact, internal-quality, cluster-surface, external-conflict, commit, and reconciliation substages plus residual loop overhead.

The compact canonical record also preserves bounded construction effort and external-conflict workload: construction success before reconciliation, attempt-count distribution, exhausted budgets, glyph candidates examined, bounds tests/passes, detailed overlap tests, and conflict rejections. Final cluster survival and reconciliation removal remain separate. This patch changes diagnostics only; the V3J.4F3.3 count-neutral proposal selection and all V3J.4F3.2B geometry/quality rules remain authoritative.

### 2026-07-14 — Active V3J.4F3.3 Count-Neutral Distribution Contract

Painted Accent distribution no longer deletes a probability-weighted average of roughly 55% of the selected proposal budget. Patch preference, semantic support, and quiet/supporting/accent preference are combined once into deterministic candidate ranking. `Distribution Contrast` changes where the fixed proposal population is selected, not how many proposals survive.

`Stroke Density` remains a finite pre-physical proposal target in this audit patch. Every selected proposal is physically evaluated once; invalid river, modifier, sampling, slope, or grade locations remain invalid and are not backfilled. The patch intentionally does not yet add accepted-count refill or rebase density. Those decisions follow the measured funnel.

The canonical generation sequence is:

```text
finite candidate pool
→ count-neutral weighted ranking
→ selected proposals
→ physical validation
→ projected validation
→ authoritative pair/triplet allocation
→ coverage
```

Compact diagnostics expose candidate pool/selected/evaluated/surface accepted/projected valid/final counts; regional and proposal-rank surface/projected acceptance; and source, projected, coverage, and total stage timings. The former thinning-survival overlay and active thinning counter are retired.

### 2026-07-14 — Active V3J.4F3.2B Contact Geometry Contract

Projected companion contact is a near-side silhouette termination. `movingOutward` points from the moving body through its terminal; therefore the terminal centre is placed at `anchorContact - movingOutward * endpointCentreSeparation`. A post-translation invariant requires the anchor to remain outward from the terminal and the moving body inward. Far-side placement is invalid regardless of later silhouette checks.

Internal cluster clearance uses variable-radius swept segments rather than point samples. Closest points and interpolation parameters are resolved for every projected segment pair, and visible half-widths are interpolated at those parameters. Ordinary internal body pairs require 98% of combined visible half-width clearance. Only the exact moving terminal segment and anchor segment containing the intended junction receive a narrow 94% numerical tolerance; no broad contact neighbourhood is exempt, and centreline crossings are always rejected.

This correction does not add a broader aesthetic grammar filter. Authoritative participation, triplet share, layout quotas, distribution controls, bounded orientation, F3.2 body/locus guards, and F3.2A conservative pseudo-contact guards remain unchanged. Diagnostics add wrong-side terminal and swept-width internal-overlap rejection counts.

### 2026-07-14 — Active V3J.4F3.2A Conservative Triplet Quality Boundary

Projected triplet validation remains intentionally selective. In addition to the active F3.2 body-blend, attachment-locus, and severe-compression checks, complete triplets reject only two further clear failures: both real junctions collapsing into one tiny final locus, and an unconnected terminal pointing into another terminal or member body at almost-touching distance.

Production still does **not** apply a generic symbol-like silhouette, fork-angle, same-side-attachment, branch-prominence, or broader compactness rule. Those more subjective filters remain rejected because they would homogenize the mark language and remove useful irregular compositions. The authoritative allocator retries rejected donors and reports explicit shortfall rather than weakening the authored quota silently.

### 2026-07-14 — Active V3J.4F3.2 Projected Cluster Quality Contract

Authoritative quota satisfaction does not make every geometrically legal composition visually acceptable. Final cluster construction therefore applies three projected-space quality rules before a candidate may consume donor slots.

1. **Sustained body separation.** Near-parallel members may touch at a deliberate junction, but their visible bodies may not run blended together for a meaningful distance. The validator measures continuous segment clearance and longitudinal overlap rather than relying only on sampled point distance.
2. **Distinct triplet attachment loci.** A triplet's second junction must occupy a meaningfully different sample region and projected locus from the first junction. Both sides of an existing contact are treated as occupied, preventing star-like multi-tip convergence.
3. **Conservative severe-compression guard.** Compact irregular triplets remain part of the accepted visual language. Rejection is limited to the extreme case where all three member centroids collapse within 42% of the shortest authored member length.

Quality rejection returns the candidate donors to the existing bounded authoritative search. It does not change Companion Participation, Triplet Share, layout quotas, Cluster Region Bias, distribution, family weights, or total projected population. Any resulting unresolved quota remains explicit in the existing shortfall diagnostics.

The projected diagnostic record reports candidate rejection counts for sustained near-parallel body blending, occupied triplet attachment slots, shared triplet contact loci, and severely compressed triplets.

### 2026-07-14 — Active V3J.4F3.1 Distribution Authoring Architecture

Painted Accent distribution has three normal authoring controls. `Distribution Scale` owns the size of sparse/dense spatial structure. It drives both the continuous patch field and the coherent regional field through one deterministic coupled mapping. `Distribution Contrast` owns how strongly the fixed proposal population separates into populated and quiet areas. It drives patch preference, fixed-average regional redistribution, and a protected non-zero sparse floor. `Cluster Region Bias` owns only where the already resolved companion quota is concentrated.

The three responsibilities are strict:

```text
Distribution Scale
    spatial size only

Distribution Contrast
    sparse-versus-dense strength only

Cluster Region Bias
    cluster-anchor location only
```

The former independently authored Sparse Floor, Regional Zone Scale, and Regional Density Contrast values remain hidden serialized compatibility data. Production output no longer reads them independently. Distribution Scale derives regional scale over `1–13.5 m`; Distribution Contrast derives sparse floor over `0.40–0.10` and directly supplies both patch and regional contrast. This removes overlapping artistic controls without collapsing the underlying two-scale distribution implementation.

Cluster Region Bias does not alter final cluster count, pair/triplet share, layout quotas, or total glyph population. At zero, cluster anchors follow the eligible field broadly. At one, the same authoritative quota is preferentially assigned to denser accent regions.

The next projected-cluster quality pass must preserve unusual useful forms. Compact-triplet protection is intentionally low-strength: reject only clearly degenerate concentration, while stronger guards target proven long near-parallel body blending and multiple terminal tips converging on effectively the same attachment locus.

### 2026-07-14 — Active V3J.4F3 Authoritative Companion-Quota Architecture (distribution controls superseded by V3J.4F3.1)

Painted Accent population, distribution, composition, and shape are separate authoring domains. `Stroke Density` controls the selected proposal budget. Distribution Scale and Distribution Contrast redistribute that fixed budget spatially through one combined ranking pass; they do not delete proposals after selection. `Companion Participation` controls the exact target share of final valid projected marks assigned to complete two- or three-member clusters. `Triplet Share` divides clustered participants between pair and triplet quotas. Neither Tightness, Cluster Verticality, Angle Jitter, layout weights, nor distribution controls may silently change those resolved counts.

Companion allocation occurs after ordinary independent projected prototypes pass family, turn, ground, river, modifier, slope, and grade validation. The resulting final valid pool size `N` is converted to deterministic integers `P`, `T`, and `S` satisfying `2P + 3T + S = N`. Participation error is minimized first, then participant-based triplet-share error. The Inspector and compact diagnostics expose the resolved whole-mark result and any explicit geometric shortfall.

Existing descriptors are donor population slots rather than pre-positioned companion requirements. One donor remains the fixed anchor; other donors retain their family, seed, dimensions, strength, and population identity while their projected prototypes are translated into the requested atomic composition. No descriptors, connectors, shared vertices, graph records, child objects, or runtime topology are created.

`Cluster Region Bias` changes only anchor preference. At zero, deterministic cluster selection is broadly distributed. At one, cluster anchors strongly favour locally dense accent areas. The global pair/triplet quota remains unchanged. The advanced layout weights independently resolve exact requested counts for Stepped, Shoulder, Offset, and Shallow Offset pairs and Stepped Run, Crown Run, Broken Terrace, and Shallow Run triplets. A failed requested type is reported as that type's shortfall; it is not silently replaced.

`Companion Tightness` controls the visible junction gap and retains the exact silhouette-edge stop: maximum tightness permits terminal touching, never centreline penetration or pass-through. `Cluster Verticality` controls translation-driven pair-local/triplet stepping. `Angle Jitter Degrees` remains the orientation control. These shape controls do not own population.

The active build order is independent validation, exact quota resolution, authoritative atomic cluster construction, then independent remainder commit. Complete clusters are preferred; unresolved clusters return all donor marks to independent output so total projected mark count remains fixed. Bounded deterministic retries and explicit shortfall preserve safety without pretending an impossible layout succeeded.

Production invariants remain mesh-free projected glyphs, 2048² R8 coverage, ordinary ground-material integration, maximum three members, fixed descriptor population, no graph/network architecture, bounded orientation, pair-local anti-collinearity, exact terminal contact, atomic complete-cluster validation, compact cumulative telemetry, and the closed G3 performance architecture.

**Methods tried:** best-effort source participant ceilings, random triplet probabilities, same-region candidate dependency, and silent underfilled targets are rejected. Final-valid-pool integer quotas and donor-slot projected construction are authoritative.

### 2026-07-14 — Historical V3J.4F2.3 Majority-Companion Architecture

Painted Accent companions remain fixed-population atomic groups of two or three existing descriptors. Maximum Companion Strength now targets 94% of the post-thinning source population for cluster participation. This is a target rather than permission to bypass terrain, river, modifier, slope, grade, silhouette, external-conflict, or atomic-completeness validation.

Source selection keeps composition-region locality as a preference, not a hard boundary. It searches the primary region first and then nearby candidates from adjacent regions within the same existing relocation limit. A pair plan that misses the accepted pair-local structure threshold is repaired only by translation along its shared normal. A failed structured triplet receives two stronger translation-only attempts. Member angles remain bounded by authored jitter plus 10 degrees for pairs and plus 15 degrees for triplets, with the unchanged 42-degree absolute cap.

Projected contact is now an edge-termination operation rather than centreline penetration. For an interior anchor contact, the solver projects the anchor half-width along the moving endpoint approach, adds the moving cap half-width, and positions the endpoint at that silhouette boundary. Near-tangent approaches that cannot produce a clean terminal contact are rejected. Intended-contact point clearance remains locally relaxed, but segment intersections are never exempt; a moving stroke may end at another stroke but may not pass through it.

The anti-chain envelope and occupied-cell reservation remain active because F2.2 telemetry showed they were not the population bottleneck. The projected independent-first conflict policy and all-cluster atomic fallback also remain unchanged pending direct evidence from the new final clustered-participant percentage and reconciliation-removal count.

Production invariants remain: no added descriptors, no connector geometry, no graph/network cache, maximum three members, exact Strength `0` compatibility, independent stored fallback, physical and projected validation, 2048² R8 coverage, ordinary material integration, and the closed G3 performance architecture.

Compact diagnostics now distinguish source target/formed/accepted participation, cross-region selections, pair/triplet structure repairs, projected complete pairs/triplets, final clustered participants as a percentage of accepted glyphs, and clusters removed during final independent reconciliation.

### 2026-07-13 — Historical V3J.4F2.2 Pair-Local Companion Architecture

Painted Accent companion composition remains bounded-orientation, translation-driven, projected-space, and atomic. The authoritative two-member shape metric is now departure from the pair's own shared axis, not displacement along world or screen vertical. The shared axis is the normalized average of both final member axes; the pair-local step is the centre displacement projected onto its perpendicular. Consequently, a straight shallow diagonal continuation has approximately zero structure and cannot masquerade as a stepped pair.

At maximum Verticality, up to 94% of pair proposals use stepped-continuation, shoulder-contact, or offset-echo intent. The remaining minority is a `Shallow Offset`, not a flat continuation: it receives a smaller pair-local displacement and, when projected through endpoint contact, keeps a positive rendered gap. Pair-layout intent is internal descriptor metadata carried into projection. Stepped/offset pairs prefer quarter, shoulder, or interior anchor samples; shoulder pairs prefer centre/shoulder samples. Routine endpoint-to-endpoint joining is not available to structured pairs.

Projected pair retention uses the same shared-normal reference as source validation. A touching or overlapping endpoint candidate is additionally rejected when its tangents differ by no more than 16 degrees and its local step is below 12% of the shorter authored length. This specifically removes the one-long-slightly-crooked-line appearance without reopening the rejected steep-angle solution. Triplets retain fixed-north step retention and their accepted contact grammar.

Rotation remains frozen: pair members may use at most 10 degrees beyond authored `Angle Jitter Degrees`, triplets at most 15 degrees beyond it, and the 42-degree absolute cap remains authoritative. The fixed descriptor population, no-connectors rule, maximum three members, fixed primary centre, anti-chain envelopes, physical and projected validation, external-conflict checks, complete-cluster fallback, Strength-zero compatibility, R8 coverage, and G3 performance architecture are unchanged.

Population tuning is explicitly evidence-gated. The 68% participant ceiling and all envelope, cell, surface, contact, and conflict protections remain unchanged until cumulative telemetry identifies the dominant attrition stage. The canonical compact record now separates participant target, formed and source-accepted totals; target-plan, candidate-search, downgrade, reservation, source-fallback and incomplete-cluster losses; projected near-collinear rejection; and existing projected fallback categories. No per-mark or per-cluster Console output is permitted.

**Methods tried:** world-vertical pair stepping is rejected; blind angle/translation escalation is rejected; pair-local shared-normal stepping and layout-aware projected contact are active. Population relaxation is deferred until the complete generation telemetry is reviewed.

### 2026-07-13 — Historical V3J.4F2.1 Pair-Verticality Completion Architecture

The accepted F2 companion language remains bounded-orientation, translation-driven, projected-space, and atomic. F2.1 freezes triplet geometry and changes only the pair contract so a maximum-Verticality field contains mostly readable stepped pairs rather than mostly same-level continuations.

At maximum Verticality, up to 82% of pair proposals use the stepped-continuation, shoulder-contact, or offset-echo grammar; approximately 18% remain deliberately flat before physical/projected fallback. Structured pair centres request at least 15% of the shorter authored length as visible vertical separation, use a 24%-of-length translation target with a 4.25-width fallback, and must preserve at least 65% of any meaningful requested step during projected contact solving. Triplets retain their accepted 42% retention contract.

Rotation remains frozen at F2 values: pair members may use at most 10 degrees beyond authored `Angle Jitter Degrees`, triplets at most 15 degrees beyond it, and no member may exceed the 42-degree absolute safety cap. Pair verticality therefore comes from centre translation and contact topology, never from reopening the rejected steep-angle path.

A failed three-member source composition must resolve a fresh two-member target before committing as a pair. Reusing one triplet branch as an accidental pair is forbidden. Final pair statistics are measured from complete projected clusters that survive reconciliation and enter coverage, not from provisional source proposals.

The invariant set remains fixed descriptor population, no connectors or graph topology, maximum three members, fixed primary centre, anti-chain envelopes, actual projected endpoint/shoulder/interior contact search, full silhouette and unrelated-mark conflict checks, complete-cluster fallback, independent final ground validation, exact Strength-zero compatibility, R8 coverage, and the closed G3 performance architecture.

Compact diagnostics include source pair-intent counts, final committed pair-step ranges in metres and as a fraction of shorter authored length, pair contact-candidate rejections caused by the pair-only retention gate, and pair-specific fallback counts for incomplete, prototype, contact, surface, and external-conflict failures. No per-result Console spam is permitted.

**Methods tried:** more pair rotation is rejected; global triplet tightening is rejected; pair-only prevalence/translation/retention tuning is the active final shape pass.

### 2026-07-13 — Historical V3J.4F2 Translation-Driven Companion Architecture

**Status:** Unity-validated and superseded by V3J.4F2.1 only for pair-verticality completion.

Painted Accent companion clusters remain atomic groups of two or three existing independent descriptors, but vertical structure is no longer authored through extreme member rotation. The primary member retains its deterministic ordinary angle. A structured pair may use at most 10 degrees beyond authored `Angle Jitter Degrees`; a structured triplet may use at most 15 degrees beyond it; one 42-degree absolute safety cap prevents unusual authored settings from reintroducing near-vertical glyphs.

`Triplet Verticality` is now principally a placement control. It increases structured-layout prevalence, signed centre stepping along permanent visual vertical, required vertical span, and centre non-linearity. It does not require a steep member or a minimum member-angle spread. Flat continuation remains a legal minority.

The source planner supplies a bounded-orientation stepped layout intent. Final contact is solved only after actual wiggled family profiles and visible widths exist. The projected solver may join a moving endpoint to an anchor endpoint, shoulder, or interior sample, but it must preserve the sign and a substantial fraction of any meaningful requested centre step. Candidates that flatten or reverse the requested arrangement are rejected before atomic cluster commit.

The production invariants remain fixed descriptor population, no connector geometry, no graph/network cache, maximum three members, fixed primary centre, cluster anti-chain envelopes, independent final surface validation, projected silhouette and external-conflict validation, complete-cluster fallback, exact `Horizontal Companion Strength = 0` compatibility, ordinary R8 coverage, and the closed G3 performance architecture.

Compact placement diagnostics expose accepted companion angle min/mean/max separately from the overall accepted-angle range. Projected diagnostics retain requested/accepted/fallback cluster totals and contact/silhouette fallback counts.

**Methods tried:** E7/E8 extreme rotation is rejected. V3J.4F projected contact geometry and atomic fallback are retained. F2 translation-driven stepping is the validated base architecture retained by F2.1.

### 2026-07-13 — Historical V3J.4F Atomic Projected-Cluster Architecture

**Status:** Superseded by V3J.4F2 above. Its projected-space contact and atomic-fallback architecture remains active; its inherited extreme-orientation grammar does not.

Companion composition is now finalized in the same projected representation that feeds the R8 coverage baker. Surface placement still selects and seeds a bounded fixed-population cluster, but source-space contact geometry is provisional only. Each composed stroke carries internal cluster identity, member role, intended size, and an independent fallback stroke.

The projected stage builds the real wiggled/family-shaped prototype for each actual selected member before solving contact. The primary remains fixed. Secondary and tertiary prototypes are translated using samples from their real projected polylines and production half-widths. Moving-member contacts use endpoint-adjacent samples with visible width, and the tapered tail beyond the chosen contact is removed, avoiding the sub-pixel needles produced by mathematical endpoints. Anchor contacts may be at an endpoint, shoulder, or interior sample.

A cluster is an atomic projected transaction:

```text
actual selected source members
→ actual longitudinal profiles
→ actual fixed-north projected prototypes
→ projected contact placement
→ complete silhouette and external-mark checks
→ final ground/domain sampling for every member
→ commit all members
```

If any step fails, all available members return to their stored independent variants. After all initial cluster decisions, a deterministic reconciliation pass rechecks committed clusters against the final independent/fallback population and atomically restores any newly conflicting cluster. Partial pairs/triplets, orphan steep branches, and triplet-fragment pair fallthrough are forbidden. Cluster identity is preserved into final projected glyphs so diagnostics count only complete clusters that actually reach coverage.

The active diagnostics contract includes requested/accepted/fallback clusters, complete final pairs/triplets, fallback reason categories, and a separate projected-cluster composition timing. These are compact aggregate diagnostics; no per-cluster Console logging is introduced.

The production invariants remain fixed descriptor population, no connector geometry, no graph/network cache, maximum three members, independent `Horizontal Companion Strength = 0` compatibility, ordinary final ground/river/modifier/slope/grade validation, ordinary R8 coverage, and the closed G3 performance architecture.

**Methods tried:** E7's render-angle correction remains useful grammar input; E8's source-space straight-segment junction gate is rejected as a production validator because it ignored path wiggle, family projection, width taper, later projected rejection, and unrelated final glyphs.

### 2026-07-13 — Historical V3J.4E8 Source-Space Junction Architecture

V3J.4E8 keeps the bounded fixed-population companion architecture but adds an explicit final-junction quality gate. Structured contacts now place the target member by its true endpoint against an endpoint, shoulder, or interior point on the anchor. Target endpoint fractions remain within the final 1.5% of the member span, eliminating the visible free stubs that appeared when E7 allowed shoulder-depth target contacts.

Every structured junction is validated against the same final straight-segment geometry used by target placement. The gate rejects intersections away from the intended contact, excessive contact separation, acute interior junctions, free endpoints that remain inside another member's body corridor, multiple triplet junctions collapsing onto nearly the same anchor point, and unintended non-adjacent member intersections. Failed triplets continue to downgrade to pairs; rejected structured pairs simply remain unavailable to that composition attempt.

`Triplet Verticality` remains the single vertical-composition control, but it now gives pairs a deliberately weaker subset of the structured grammar. At maximum Verticality, up to 64% of pair requests may use stepped-continuation, shoulder-contact, or offset-echo layouts; flat pair continuation remains legal and common enough to preserve visual variety. Structured pairs may exceed ordinary `Angle Jitter Degrees` up to a 64-degree safety bound, while triplets retain the E7 84-degree limit.

Structured contact separation is solved along the target member's outward axis rather than fixed visual horizontal. Interior contacts do not receive routine deep penetration; endpoint-to-endpoint contacts retain only small taper compensation. The three-member hard cap, fixed descriptor population, primary-centre preservation, cluster anti-chain envelopes, independent physical validation, G3 performance architecture, and exact `Horizontal Companion Strength = 0` compatibility remain unchanged.

**V3J.4E7 is superseded by this architecture.** Its render-faithful angle fix and stronger junction grammar remain foundational, but it lacked final contact-quality rejection and left pair composition comparatively flat.

### 2026-07-13 — Historical V3J.4E7 Render-Faithful Triplet Junction Architecture

Structured triplet geometry now uses the same final angles for contact solving, geometric acceptance, descriptor orientation, and rendering. Ordinary `Angle Jitter Degrees` remains authoritative for independent marks and pair grammar, but the explicit `Triplet Verticality` control may exceed that bound for structured triplets up to an 84-degree safety limit. This removes the E6 mismatch in which steep targets were validated and then flattened during final orientation.

Three-mark clusters use bounded contact topology rather than endpoint-only chains. Depending on deterministic layout, an endpoint may contact another member's shoulder or middle, and crown motifs may place two steep members against opposite interior portions of the primary. These are still three independent descriptors with no connector, shared topology record, recursive extension, or additional population.

At high Verticality, the control also increases eligible triplet prevalence, makes the flat exception rare, strengthens final member inclination, and raises mandatory endpoint-span, centre non-linearity, and two-step thresholds. Acceptance is evaluated from the final six segment endpoints and the same final axes later used by surface-stroke generation. A structured proposal that cannot prove the required geometry downgrades to a pair.

Maximum Tightness permits small deterministic penetration at triplet contacts to compensate for tapered endpoints and produce visible contact. Cluster envelopes continue to reserve the complete final motif and prevent cluster-to-cluster concatenation. `Horizontal Companion Strength = 0` remains the exact pre-composition compatibility state.

**V3J.4E6 is superseded by this architecture.** Its explicit control and geometric-gate direction were correct, but its target solver and final renderer used different angle bounds and its contacts remained endpoint-only.

---
document_id: PS3D-GROUND-01
title: "Ground Visual Design and Architecture"
version: 0.1
status: active-baseline
scope: generated-ground-style-and-architecture
authoritative_for: "generated ground visual doctrine, style pillars, family/variant interpretation, shared ground style layers, static surface-mask contracts, and ground roadmap priority"
related_documents: [PS3D-00, PS3D-01, PS3D-02, PS3D-04, PS3D-06]
implementation_documents:
  - Ground_Generation_Surface_Upgrade_Plan.md
---


### 2026-07-13 — Historical V3J.4E6 Guaranteed Triplet Verticality Architecture

Three-mark companion clusters are no longer accepted merely because they were assigned a stepped or arched template. `Triplet Verticality` is an explicit style input controlling both the triplet-only orientation range and the minimum visible departure from a line. It belongs to the surface-stroke signature whenever Horizontal Companion Strength is active.

At maximum verticality, structured triplets may use a steep 30–62-degree member while retaining a hard 68-degree limit. After endpoint-connected centres are solved, the generator measures (1) the middle centre's perpendicular distance from the line joining the outer centres and (2) total span along fixed visual vertical. Both thresholds scale with Stroke Width. A structured triplet that fails either threshold is discarded as a triplet and may still compose as a pair.

The 8% flat-continuation template remains a deliberate exception. Therefore horizontal triplets are legal but uncommon, while every retained non-flat triplet is geometrically proven to have a visible two-dimensional component. Tightness continues to govern endpoint contact; verticality does not add strokes, connectors, shared topology, recursive extension, or runtime objects.

The active Horizontal Companion controls are:

```text
Horizontal Companion Strength
Companion Tightness
Triplet Verticality
```

`Horizontal Companion Strength = 0` still exits before mutation. Pair grammar, fixed descriptor population, anti-chain isolation, three-member hard cap, independent physical validation, projection, coverage, and the closed G3 performance architecture remain invariants.

### 2026-07-13 — Historical V3J.4E5 Vertically Structured Triplet Architecture

**Status:** Superseded by the active V3J.4E6 architecture above. Retained as the angle-progression grammar that did not guarantee visible non-linearity.

The production doctrine remains bounded implied connectivity between independent Painted Accent descriptors. Pairs retain the accepted E4 grammar. Triplets remain capped at exactly three existing descriptors, but their default composition is now a vertically structured run rather than a flat horizontal assembly.

```text
fixed primary
→ endpoint-adjacent secondary
→ endpoint-adjacent tertiary
→ one stepped, crowned, or broken-terrace trend
→ one reserved cluster envelope and composition cell
→ independent physical validation
→ unchanged projected glyph and R8 coverage paths
```

Structured triplets use the authored `Angle Jitter Degrees` range as their absolute orientation bound. Their vertical component is created primarily by tangent progression across the three members, allowing adjacent endpoints to stay touching or nearly touching. Small endpoint-local offsets prevent mechanical perfect joins but remain only a fraction of Stroke Width.

The deterministic internal distribution is:

```text
Stepped Run:       44%
Crown Run:         28%
Broken Terrace:    20%
Flat Continuation:  8%
```

Flat triplets remain legal as an exception, not the dominant motif. Strength and Tightness retain their E4 meaning: Strength controls the bounded participant budget and pair/triplet frequency; Tightness controls endpoint separation. No new authoring control is added.

Anti-chain isolation, the three-member hard cap, fixed population, primary-centre preservation, and independent ground/slope/river/modifier/grade validation remain architectural invariants. At `Horizontal Companion Strength = 0`, the pass exits before mutation and preserves V3J.4D3 exactly.


### 2026-07-13 — Active V3J.4E4 Bounded Companion-Cluster Architecture

**Status:** Superseded by the active V3J.4E5 architecture above. Retained as the stronger bounded-cluster architecture whose triplet layout remained too horizontally synchronized.

The production doctrine remains implied connectivity between independent Painted Accent descriptors, but the bounded local artistic unit is now a **cluster of two or three marks**. Three is a hard cap, not the start of graph topology.

```text
post-thinning descriptors
→ choose one fixed primary
→ resolve one or two oriented endpoint-adjacent targets
→ relocate existing secondary/tertiary descriptors
→ reserve one expanded cluster envelope and composition cell
→ validate each descriptor independently
→ project and bake through the unchanged R8 coverage path
```

The primary centre is never translated. Secondary and tertiary members are selected from the existing same-region population within a bounded relocation radius. No proposal or stroke is created. Maximum Strength raises the participant budget to 68% so the authoring control has a clearly strong diagnostic endpoint; the remainder stays independently placed.

Cluster layout follows fixed visual horizontal only as a broad trend. Each member has independent bounded tangent drift and deliberate vertical stagger. Endpoint placement uses the actual oriented member axes and compensates for the authored longitudinal End Taper. At maximum Tightness, adjacent source centreline spans may overlap slightly so the tapered rendered marks touch or retain only an approximately one-to-two-pixel break; tangent drift still prevents the complete member centres from snapping onto one ruler-straight row. Pair and triplet templates remain deterministic internal grammar, not new authoring controls.

Anti-chain isolation remains architectural. Each complete two/three-mark motif reserves one expanded envelope and one deterministic composition cell. A later motif whose envelope intersects it is rejected. Recursive extension, a fourth member, connectors, junctions, shared vertices, graph records, and network caches remain forbidden.

At `Horizontal Companion Strength = 0`, the pass exits before mutation and preserves V3J.4D3 exactly. With companions active, only the existing surface-stroke signature and downstream projected glyph, coverage, and material stages invalidate.

### 2026-07-13 — Historical V3J.4E3 Isolated Companion Architecture

**Status:** Superseded by the active V3J.4E4 architecture above. Retained as the exactly-two-mark isolation experiment.

The production doctrine remains implied connectivity between independent marks, but one local motif must contain exactly two marks. V3J.4E3 therefore treats anti-chain isolation as an architectural invariant rather than a probability preference.

The active pipeline is:

```text
regional surviving descriptors
→ choose an unchanged primary
→ solve one deterministic companion target
→ select one nearby available secondary
→ reject when the pair corridor intersects another pair or any third mark
→ independently validate both descriptors
→ ordinary projected glyph and R8 coverage paths
```

The primary descriptor is never translated by companion composition. The secondary remains an existing descriptor from the fixed stroke population; it is repositioned only when its original centre lies within the bounded target-selection radius. No new stroke is created.

Each accepted pair reserves both a deterministic composition cell and an expanded visual-horizontal corridor. This prevents two pairs from joining end-to-end and prevents an unpaired third mark from extending the motif. These are conservative composition checks only; both members still pass the exact ground, slope, river, modifier, and grade validation independently afterward.

Companions are intentionally readable again: 72–95% of primary length and 90–100% strength, with maximum authored participation capped at 44% before spatial and anti-chain rejection. Strength controls frequency; Tightness controls target gap and vertical relationship. Neither control creates graph topology or changes total descriptor count.

The invariant remains:

```text
one implied companion motif = exactly two independent marks
```

At `Horizontal Companion Strength = 0`, the composition pass exits before any candidate mutation and preserves the accepted V3J.4D3 baseline exactly.

### 2026-07-13 — Historical V3J.4E2 Horizontal Companion Grammar

**Status:** Superseded by the active V3J.4E3 architecture above. Retained as the record of the dominant/subordinate and same-facing experiment.

The production doctrine remains implied connectivity between independent marks. V3J.4E proved that horizontal companionship is the correct direction, but its equal-length, symmetrically angled, highly saturated pairs created a repeated bird/moustache symbol at maximum strength. V3J.4E2 replaces that grammar without adding controls or representation layers.

Each composed pair now has a clear hierarchy:

```text
primary mark
  ordinary authored length and strength

subordinate companion
  55–85% of primary length
  82–94% of ordinary strength
  same authored Stroke Width system
```

Directional families align their existing mirrored profile variant to the primary member. Pair orientation no longer straddles a shared angle with equal positive and negative offsets; the companion receives a restrained deviation on the same side. This removes the strongest source of wing-like symmetry without modifying any projected-family formula.

The pair grammar contains three deterministic internal arrangements:

```text
Continuation     58%  end-to-end broken contour with a positive gap
Staggered Echo   30%  shorter companion offset vertically and toward one end
Offset Shoulder  12%  uncommon tucked companion implying a broken larger silhouette
```

These are not new authoring modes and do not appear as Inspector controls. They are bounded variation inside the existing Horizontal Companion Strength and Companion Tightness contract.

Population participation is now nonlinear and capped at 38% of post-thinning survivors at Strength `1`. Strength `0.35` is intended to be occasional, `0.65` clearly present, and `1.0` a strong diagnostic state that still leaves most of the field independently placed. Tightness controls separation inside each arrangement, but Continuation never receives a negative gap.

All architecture invariants remain:

```text
no added proposals or strokes
no connectors or shared nodes
no graph/network cache
no runtime objects
same independent physical validation
same projected families and R8 coverage
strength zero preserves the accepted independent-placement baseline
```


### 2026-07-13 — Active GeneratedGround Inspector Contract after V3J.4E1

**Status:** Implemented for Unity validation. This is an editor-authoring contract only.

The primary `GeneratedGround` Inspector must expose every active Painted Accent control used by the selected style variant. Horizontal companion controls are part of that primary contract; exposing them only in the underlying style-profile asset editor is insufficient.

The Inspector is organized as nested foldouts rather than one continuous control and diagnostic stream. Painted Accent authoring is divided into coherent groups for basics, distribution, regional composition, companions, family mix, geometry, projected profile, and ink. Debug overlays and read-only diagnostic blocks are separate collapsed groups. Other GeneratedGround systems and advanced material families are likewise collapsible.

Foldout state is transient editor UI state. Opening or closing a foldout must not modify serialized recipes, invalidate generation signatures, mark assets dirty, or schedule regeneration. Only edits to the actual serialized controls may invoke the existing GeneratedGround refresh path.

The `Horizontal Companions` foldout is open by default during V3J.4E validation. Its controls remain:

```text
Horizontal Companion Strength
Companion Tightness
```

Tightness is visually disabled while strength is zero, matching the generation-signature contract that inactive tightness has no effect.


### 2026-07-13 — Active V3J.4E Horizontal Companion Architecture

**Status:** Historical V3J.4E proof. Superseded by the active V3J.4E2 grammar above; V3J.4D3 remains the strength-zero visual baseline.

The active production representation remains mesh-free and population-bounded:

```text
independent regional surface-stroke candidates
→ optional bounded two-mark horizontal composition
→ independent physical validation
→ four projected glyph families
→ shared R8 coverage
→ Ink Color albedo composition
```

Horizontal companions are a composition rule, not a new representation. They consume existing post-thinning candidates and never add proposal budget, connector segments, shared nodes, graph records, child objects, meshes, materials, or runtime update work. At maximum strength, approximately half of surviving candidates may participate; the remaining population stays independently placed.

Pairing is deterministic and local to an existing composition region. A candidate is paired with the nearest available regional survivor, and the pair is repositioned end-to-end along the GeneratedGround-local projection of fixed world horizontal. Small vertical separation and restrained orientation difference prevent mechanical duplication while retaining the intended larger implied contour. Both members remain ordinary `GroundPaintedAccentSurfaceStroke` candidates and pass sampling, slope, river, modifier, and grade rejection independently.

`Horizontal Companion Strength = 0` is a strict compatibility contract: it preserves the accepted V3J.4D3 placement and family behavior. `Companion Tightness` changes only pair spacing and has no signature effect while strength is zero. With companions active, both controls belong to the surface-stroke signature and therefore invalidate only descriptors and their downstream projection/coverage/material stages.

Family-aware pairing biases the second member toward Shoulder + Shallow, Asymmetric + Shoulder, Shallow + Shallow, and Complete + Shoulder arrangements while respecting authored zero weights. The system does not force a hidden family into an isolated-family preview. Complete + Complete is de-emphasized, not categorically forbidden, because a single-family authored configuration must remain valid.

The accepted doctrine remains implied connectivity:

```text
separate marks placed in relation
→ eye reads a larger contour
```

and explicitly rejects:

```text
connector geometry
shared topology
procedural graph/network storage
branch or tendril generation
population growth outside Stroke Density
```


### 2026-07-13 — Active Ground Performance Architecture after G3

**Status:** G1, G2, and G3 are Unity-validated and closed; V3J.4D3 remains the accepted strength-zero visual baseline.

The active Painted Accent production architecture remains:

```text
accepted surface-stroke descriptors
→ four projected glyph families
→ exact ground/domain validation
→ generated R8 coverage
→ flat Ink Color albedo composition under ordinary ground lighting
```

G2 established stable stage signatures and made unchanged regeneration effectively free. G3 does not alter those dependencies. It optimizes only the legitimate projected-footprint work measured after a shape-only change.

Projected validation now uses a narrow immutable ground-sampling contract containing only height and visible render normal. Centre, left, and right samples share the snapshot/grid context and each resolve the checkerboard triangle once. The complete `GroundSurfaceSample` contract remains authoritative for consumers that require material and semantic masks; it is no longer constructed merely to validate Painted Accent slope and grade.

One reusable build-local scratch set owns candidate projected points, widths, footprint boundaries, sampled centres, and broad-phase indices. Rejected glyphs do not allocate permanent point arrays. Accepted glyphs still receive independent owned arrays, so cached production data never aliases mutable scratch storage.

River and modifier broad phases are conservative filters only. Ground-owned river bounds are built from active spline samples and expanded by maximum snapshot influence; unsafe or unavailable bounds remain unbounded. Modifier bounds include complete authored shapes and blend distance. Exact snapshot evaluation remains the final authority, and the existing first-failure rejection order is preserved. No river implementation file is changed.

The topology audit retains all exact segment-intersection decisions but rejects segment pairs with disjoint AABBs before cross-product testing.

Coverage remains a single GeneratedGround-owned R8 texture. G3 retains a compatible readable texture plus exact-size CPU byte buffer across coverage rebuilds, using raw bulk upload and one controlled `Apply`. The deliberate maximum additional retained CPU memory is approximately 8 MB at 2048²; visual resolution and coverage bytes are unchanged.

The timing contract now reports:

```text
profile build
family validation
point construction
topology + turn
surface/domain validation
  footprint preparation
  ground sampling
  broad slope
  river exclusion
  modifier exclusion
  transverse grade
  longitudinal grade
diagnostics
coverage raster
coverage upload
```

G3 passed exact timing and invalidation validation. V3J.4E is now the active visual experiment described above.


### 2026-07-13 — Active Ground Performance Architecture after G1

**Status:** Historical G1 architecture checkpoint. Superseded by the active G3 section above; V3J.4D3 remains the accepted visual baseline.

Painted Accents no longer own or sample a legacy fold-field representation. The sole active representation is:

```text
independent accepted surface-stroke descriptors
→ four projected glyph families
→ generated R8 coverage
→ flat Ink Color albedo composition under ordinary ground lighting
```

`GroundPaintedAccentSurfaceStrokeGenerator` is the current descriptor/placement generator. The old `GroundPaintedAccentFoldFieldGenerator` name, RGBA texture, relief body, signed-side field, final-prototype debug view, and brute-force diagnostic rasterizer are retired. Historical sections may mention those items only to document rejected development stages; they are not valid implementation guidance.

The production `Ground Painted Accent Lines` shader debug mode now displays only the same R8 coverage used by the accepted render. The obsolete Painted Accent scale, contrast, mask-influence, direction, seed, and unused coverage-texel-size shader properties are removed; GeneratedGround now writes only the active Painted Accent strength plus coverage/Ink Color properties. Minor material response that previously sampled the legacy selected-line channel is driven by production coverage instead. There is no second Painted Accent texture representation and no legacy shader fallback.

Exact nearest-stroke-distance statistics are not part of generation semantics and are removed. Performance evidence is supplied through GeneratedGround Profiler markers and a compact cached inspector timing summary rather than expensive diagnostic geometry sweeps or console spam.

G1 intentionally preserves every approved visual parameter and output-quality choice. It does not reduce the 2048² coverage ceiling, density capability, family weights, length/width contracts, regional distribution, path wiggle, physical exclusions, or deterministic seeds.

The remaining strictly GeneratedGround performance roadmap is:

```text
G2
  stable stage signatures
  no unconditional downstream invalidation
  material-only changes remain material-only
  Painted Accent-only changes do not rebuild ground mesh/collider
  unchanged geometry does not recook MeshCollider

G3
  reuse compatible R8 Texture2D instances
  reuse the CPU byte buffer
  retain bulk upload
  optimize the measured legitimate raster hotspot only if still required
```

No river implementation file is part of this roadmap. GeneratedGround may continue to report the time spent in its own corridor-notification call, but this track does not alter river lifecycle ownership or river generation semantics. V3J.4E implied horizontal companions remain paused until G2 and G3 pass timing and visual-equivalence validation.

### 2026-07-13 — Active Painted Accent Baseline, Pause Boundary, and Performance Ownership

**Status:** Historical pause boundary. G1 is now active above; V3J.4D3 remains the accepted visual baseline.

The active Painted Accent doctrine is now:

```text
independent strokes, not explicit networks
regional concentration and bounded signed orientation variation
four deterministic glyph families
strict final-profile sanity
mesh-free projected coverage baked into ground albedo
```

The current shapes are accepted as good enough to compose. Future visual work should not reopen explicit contour graphs, tendril clusters, shared-node networks, or π/terrace topology. The next permitted network-adjacent mechanism is implied connectivity through nearby independent marks.

The performance boundary is architectural, not artistic. Editor restoration currently has a structurally duplicated full-ground path: GeneratedGround enables and regenerates, then river enablement can notify the parent and request another complete ground regeneration. Because a pass may include collider recooking, Painted Accent placement/projection, and a `2048 × 2048` R8 coverage bake, companion work is paused until unnecessary duplicate execution is removed.

Performance work is owned outside this Painted Accent thread. It may not be implemented here, and this thread may not modify `StylizedRiver.cs` or other river files. Any separate performance patch must preserve this document's visual doctrine and must not reduce coverage resolution, density capability, family behavior, or accepted style defaults as a substitute for lifecycle correctness.

Required performance semantics:

```text
one explicit owner for each lifecycle processing pass
coalesced edit-mode requests rather than synchronous duplicate regeneration
stage signatures remain authoritative
material/debug changes do not rebuild geometry or coverage
shape changes rebuild projection/coverage but not ground geometry
placement changes rebuild placement/projection/coverage but not unchanged geometry
geometry or structural river/modifier changes rebuild all true dependants exactly once
unchanged geometry does not recook the collider
```

After that work is validated visually and by profiling, V3J.4E may introduce bounded horizontal companion compositions. Pair members must remain independent descriptors/glyphs/coverage strokes, consume the existing population budget, validate independently, and create implied rather than literal connectivity.

### 2026-07-13 — Patch V3J.4D3: Final-Profile Sanity and Signed Orientation Balance

**Status:** Unity-validated as good enough; active baseline pending lifecycle-performance work.

Painted Accent family identity is now subject to a common final-profile sanity invariant:

```text
no accepted non-flat mark may contain a significant interior valley
with separated higher profile sections on both sides
```

This is the explicit prohibition against the observed “cat-ear” silhouette. The rule is evaluated from the final dense combined-height samples, not from source parameters. A valley counts only when both sides exceed it by at least the greater of `0.001 m` or `8%` of the profile peak; minor organic detail remains legal. Complete Mound is no longer exempt from this cross-family sanity rule.

Final projected smoothness is family-sensitive. Complete and Asymmetric mounds may turn slightly more strongly than one-sided quiet marks, but the former shared `42°` ceiling is retired. Active limits are `32°`, `30°`, `27°`, and `25°` for Complete, Asymmetric, Single Shoulder, and Shallow Crest respectively. Single Shoulder and ordinary Shallow Crest must also devote at least 18% of their span to the primary 5%-to-95% height transition. Construction ranges are adjusted to give Shoulder a stable high run and Shallow Crest a 34–44% smooth shoulder transition envelope.

Orientation composition must preserve both local relationship and authored signed randomness. `Angle Jitter Degrees` is again an absolute final bound around the perpendicular base direction. Regional mean bias is limited to at most 25% of that authored range and never more than `10°`; each accepted mark receives a larger deterministic per-mark deviation. Within a composition region, sign choice is acceptance-aware: the next candidate uses the currently under-represented positive or negative sign, and only successful physical placements update the balance. Therefore a failed mark cannot leave the rest of the region permanently biased to one side.

This balance is not a new control and does not force a global checkerboard. Single-mark regions remain random, magnitudes remain varied, and neighboring marks still share the weaker regional mean. The existing accepted angle min/mean/max diagnostic is the evidence: populated fields should normally straddle zero and keep the mean near zero while respecting the authored absolute jitter limit.

Coverage, density, regional density contrast, zone scale, family weights, Stroke Path Wiggle, strict length bounds, width, and every physical-domain exclusion remain unchanged. V3J.4E companions may only compose marks that pass these final-profile and orientation contracts.

### 2026-07-13 — Patch V3J.4D2: Smooth Source Paths and Visible Shallow Shoulders

**Status:** Unity-validated as a useful smoothness/readability pass; residual extrema and orientation defects are corrected by V3J.4D3.

Painted Accent shape quality is governed by two independent curves:

```text
source ground path
  controls lateral bend and regional flow

projected family profile
  controls displacement toward fixed world +Z
```

Both must be smooth and artistically legible. A dense projected profile cannot repair an angular source path. The active source-path doctrine therefore stores the analytic deterministic centreline at approximately `0.03 m` spacing with `13–33` points, replacing the former five-point minimum that created visible quarter-span elbows across multiple families.

`Stroke Path Wiggle` is the sole authoring control for source-path lateral curvature. It is not an alias for Profile Irregularity or feature Contrast and cannot change family identity, length, width, height, density, or region membership. The baseline remains monotonic along its authored stroke axis, so added wiggle may bend but cannot loop or create graph-like topology.

Final projected XZ geometry is now audited directly. Maximum per-sample turn is reported, and turns above `42°` are rejected as severe residual kinks. The existing profile-space turn metric remains useful for scalar-profile analysis but is not accepted as evidence that final projected geometry is smooth.

Shallow Crest is now defined by visible projected displacement, not normalized ratios alone:

```text
ordinary variant
  broad upper run
  one small but visible endpoint shoulder
  at least 0.0035 m endpoint displacement

rare quiet variant
  intentionally near-straight
  approximately 4% of Shallow Crest selections
```

This preserves occasional calm dashes while preventing the family from being dominated by visually straight lines. Single Shoulder uses quintic-smooth plateau/descent transitions. The four-family architecture, authored family weights, independent-mark representation, R8 coverage, and all physical-domain exclusions remain active.

The next architectural candidate remains V3J.4E implied horizontal companions. It may only compose the corrected independent strokes; it must not hide unresolved kinks or invisible family shoulders.



### 2026-07-12 — Patch V3J.4D1: Distinct Final-Curve Family Identities

**Status:** Unity-validated as a useful separation pass; source-baseline angularity and Shallow-Crest visibility are corrected by V3J.4D2.

The four-family architecture remains accepted, but family identity is now defined by measurable final-curve structure rather than by loose seeded intent. This prevents the vocabulary from collapsing back into one continuous arch-to-line spectrum.

Active identity doctrine:

```text
Complete Mound
  preserved A6/A7 baseline; two meaningful sides

Asymmetric Mound
  unmistakably off-centre crest; >= 2:1 leg-span ratio; >= 1.5:1 slope ratio

Single Shoulder
  high plateau/run followed by exactly one sustained descent

Shallow Crest
  normally one small shoulder leading into a broad shallow run
  rare near-straight quiet subvariant only
```

The final densely sampled profile is the authority. Parameter values alone cannot prove family membership. A generated candidate that fails its perceptual family contract is rejected rather than relabelled or converted into Complete Mound. Authored Stroke Length Min / Max and Stroke Width remain absolute and family-independent. Regional density, direction, family weights, R8 coverage, and all physical exclusions remain unchanged.

Unity also exposed a valuable composition behavior: two independent marks placed roughly end-to-end along visual horizontal can imply a larger contour formation without literal connectivity. This is now the preferred network-adjacent direction. It preserves independent strokes and lets the eye complete the relationship, avoiding the rejected tendril-tree and π-terrace failures.

### Planned V3J.4E — Implied Horizontal Companions

**Status:** Historical design checkpoint. Implemented by the active V3J.4E architecture section above after G3 validation.

The future companion system may pair a bounded portion of the existing mark population along the axis perpendicular to fixed world `+Z`. Pair members remain fully independent geometry and coverage. Small end-to-end gaps, rare slight overlaps, restrained vertical offsets, and compatible family combinations may create emergent larger shapes. The system must never create explicit connectors, shared nodes, graph topology, or a separate network representation.

### 2026-07-12 — Patch V3J.4D: Four-Family Glyph Vocabulary and Spacing Cleanup

**Status:** Unity-validated as a useful vocabulary proof; family overlap corrected by V3J.4D1.

V3J.4D expands the sole accepted mesh-free Painted Accent shape path from one repeated complete mound into four deterministic single-stroke families. The production architecture remains independent marks baked into the existing R8 coverage texture; no graph, shared topology, fragmentation, runtime object, mesh, or per-frame generation is introduced.

Active glyph vocabulary:

```text
Complete Mound     → accepted A6/A7 rounded two-sided mound
Asymmetric Mound   → off-centre crest with materially unequal legs
Single Shoulder    → one high run and one meaningful descending side
Shallow Crest      → low predominantly lateral contour with restrained ends
```

Family choice is made once from the proposal seed and four artist-authored relative weights. The weights are normalized internally, do not depend on regional mode or composition role, and do not alter length or width. If all four weights are zero, Complete Mound is the explicit safe fallback. Each family is selected before final physical validation and rejected in place when its own shape or footprint is invalid; there is no silent conversion to another family.

Authoring contract:

```text
Stroke Length Min <= every family descriptor length <= Stroke Length Max
Stroke Width remains authoritative and family-independent
Profile Height remains the primary projected-height response
Crest Crown Height remains an additive crest-cap response
regional density/direction composition remains independent from family mix
```

The `Local Spacing Strength` control and its pairwise spacing-suppression stage are removed completely. Unity validation showed no visible artistic response at the active sparse/dense field scales, and retaining a dead control would obstruct deliberately dense baked populations. There is no hidden fixed spacing replacement. At that historical V3J.4D stage, population was governed by Stroke Density, broad distribution, regional thinning, and all-or-nothing physical-domain validation. The active V3J.4F3.3 contract at the top of this document supersedes regional thinning with count-neutral regional ranking.

The editor-only Family Preview filter can isolate Complete Mound, Asymmetric Mound, Single Shoulder, or Shallow Crest in Scene diagnostics without changing generated coverage. Composition role remains encoded by marker size while family is encoded by marker colour. Production rendering always contains the complete accepted family mixture.


### 2026-07-12 — Patch V3J.4C2: Author-Controlled Population and Regional Concentration

**Status:** Unity-validated for width, high population, Regional Zone Scale, and Regional Density Contrast. The ineffective Local Spacing Strength experiment is removed by V3J.4D.

Painted Accent population and regional concentration are separate authoring dimensions. `Stroke Density` controls the requested proposal population and supports up to `2000` proposals per standard `40 × 40 m` patch. `Regional Density Contrast` redistributes a fixed average survival rate among quiet, supporting, and accent regions, so denser local zones do not require a higher total density. `Regional Zone Scale` controls the size of those coherent zones.

The active distribution doctrine is:

```text
Stroke Density            → requested population
Distribution Patch Scale  → broad continuous density-field scale
Distribution Patchiness   → continuous field preference strength
Distribution Sparse Floor → broad-field cold-region floor
Regional Zone Scale       → coherent regional density/direction size
Regional Density Contrast → fixed-average redistribution into accent zones
```

These layers must not silently redefine one another. Increasing regional contrast may redistribute marks but does not change Stroke Length Min / Max or authored width. Increasing Stroke Density creates more proposals but does not force uniform distribution. No secondary pairwise spacing stage remains after regional selection.

Coverage remains a generated R8 field. The minimum raster support and feather are reduced to `0.08` and `0.10` texels, with relative feather reduced to `0.12 ×` core half-width. Authored Stroke Width now supports `0.002–0.20 m` and remains the primary physical line-width control. No connected network, fragmentation, new glyph family, runtime object, or per-frame generation is introduced.


### 2026-07-12 — Patch V3J.4C1: Stroke Length Min / Max Are Hard Bounds

**Status:** Unity-validated active contract.

`Stroke Length Min` and `Stroke Length Max` are authoritative physical limits in metres. Regional composition may decide where inside that interval a support, standard, or dominant mark belongs, but it may never multiply a selected length beyond either bound.

The active role contract is:

```text
support  → lower portion of authored interval
standard → middle portion of authored interval
dominant → upper portion of authored interval
```

Regional scale variation is a bounded normalized offset inside that same interval. A narrow artist-authored interval deliberately produces a narrow role hierarchy. This preserves direct author control and prevents composition logic from silently redefining length semantics.


### 2026-07-12 — Patch V3J.4C: Independent Marks with Regional Composition

**Status:** Unity-validated for regional distribution; length-bound defect corrected by V3J.4C1.

V3J.4B proved that accepted A6/A7 projected glyphs can be rendered as mesh-free ground-integrated ink, but Unity evidence exposed two problems: the provisional raster feather made a nominally thin line visibly too broad, and the population still read as many equally important copies of one small arch. V3J.4C addresses both without reopening network topology or changing the accepted profile generator.

Active architecture:

```text
weighted independent proposals
→ stable jittered macro-regions
→ region mode: quiet / supporting / accent
→ deterministic thinning and role assignment
→ local regional direction and scale hierarchy
→ existing all-or-nothing physical validation
→ unchanged A6/A7 continuous projected glyph
→ narrow corrected R8 coverage
→ family/variant Ink Color in ordinary ground lighting
```

The artistic unit remains an independent terrain mark. Regional composition relates nearby marks through shared density, direction, and scale; it never joins them into a graph. Quiet regions may contain no accepted marks. Supporting regions contain restrained standard/support marks. Accent regions may contain at most one longer dominant mark plus smaller independent companions. No rejected candidate is backfilled.

The regional direction doctrine is:

```text
authored global facing
+ stable regional offset
+ small per-mark jitter
```

This replaces near-identical global orientation without becoming random scratch noise. Length varies by role only within the authored Stroke Length Min / Max interval, while width scales mildly. Dominant marks occupy the upper part of that interval rather than overriding it. The original role-aware spacing proof was later removed by V3J.4D after Unity showed that its exposed strength control had no useful visible effect. Dense local packing is now allowed to follow the authored population and regional controls directly.

The V3J.4B provisional raster expansion is narrowed from `1.15` to `0.35` texels of minimum feather and from `0.45` to `0.30` texels of minimum half-width. The coverage diagnostic contract now reports authored core, effective raster core, feather per side, and estimated visible full width separately.

Authoritative boundaries:

```text
accepted A6/A7 profile and projection: unchanged
connected network or shared topology: prohibited
fragmentation and geometry clipping: absent
new glyph families: deferred
new artist controls: absent during proof
runtime objects or per-frame generation: absent
```

The temporary composition overlay is editor-only and independently controllable. It shows proposal region modes, thinning survival, one direction bar per occupied region, and role/family markers for accepted marks. Production rendering remains solely the generated R8 coverage sampled by the existing ground shader.

Decision gate: if regional density, direction, hierarchy, and spacing succeed but the field still reads as one repeated arch family, the next justified step is a restrained V3J.4D glyph-family expansion. Breakup and fragmentation remain later rendering concerns and must not be used to disguise a failed composition.


### 2026-07-12 — Patch V3J.4B: Accepted Projected Coverage and Ground-Albedo Ink Proof

**Status:** Unity-validated as a functional representation proof; superseded by V3J.4C for width fidelity and regional composition.

V3J.4B preserves the accepted A6/A7 projected glyph geometry and changes only its production representation. Accepted glyph polylines and tapered half-widths are rasterized at generation/dirty time into a `GeneratedGround`-owned single-channel `R8` coverage texture. The texture is sampled in fixed GeneratedGround-local XZ space by means of a per-renderer world-to-ground matrix, so the same property block remains valid for the ground mesh and dependent river-corridor renderers.

Active representation:

```text
accepted placement descriptor
→ A6 continuous spline profile
→ fixed world +Z native-2D projection
→ complete A6/A7 projected glyph
→ bounded segment rasterization
→ generated R8 coverage
→ family/variant Ink Color blended into ground albedo
→ ordinary ground lighting
```

The proof introduces no mesh, child object, collider, separate renderer, normal displacement, relief response, emission, or special lighting pass. Coverage uses bilinear filtering, fixed soft-edge feathering, tapered authored widths, and a very short endpoint opacity envelope. The shader composes the opaque authored Ink Color at the end of ground-albedo construction and then continues through the existing URP lighting path.

The existing `Ground Painted Accent Lines` mask debug mode now displays the production projected coverage when available. Older fold-body and signed-relief diagnostics remain available as historical/debug infrastructure and do not drive the V3J.4B final render.

This patch is a representation proof, not a shape or placement redesign. The accepted projected points, placement descriptors, river/modifier exclusions, and A6/A7 profile generator remain unchanged. Unity validation must decide whether production-style ink materially reduces the apparent island/symbol problem before regional composition or glyph-family expansion is attempted.


### 2026-07-12 — Patch V3J.4A10R: Accepted Projected Glyphs Become the Sole Shape Architecture

**Status:** Regional-network architecture rejected and removed.

Unity evidence closed the regional-network investigation.

A10A failed structurally at population scale: its compatibility graph and rooted-tree extraction represented only 11 of 169 considered descriptors and produced three tiny Y-like candidates. A10B corrected the population problem, but its deliberate connected terrace grammar generated a repeated table/π-symbol family. That output was coherent as a graph and wrong as terrain contour language.

The failure is artistic grammar, not sampling, river exclusion, slope validation, connectivity, or rendering. Fragmentation and additional procedural branches are not valid rescue strategies.

The active Painted Accent shape architecture is again singular:

```text
accepted descriptor
→ A6 continuous spline profile
→ fixed world +Z native-2D projection
→ complete A6/A7 projected glyph
→ generated R8 projected coverage
→ ground-albedo Ink Color integration
```

The following candidate architecture is absent from production and editor code:

```text
descriptor-local clusters
regional compatibility graphs
rooted trees
macro-regional terrace networks
candidate caches and diagnostics
candidate Scene overlays
accepted-versus-candidate paired previews
```

`GroundPaintedAccentProjectedGlyphGenerator.cs` and `GroundPaintedAccentLongitudinalProfileGenerator.cs` remain authoritative for shape generation. The accepted result remains data-only and mesh-free. Scene Handles remain an editor proof tool, not the final renderer.

A9A, A10A, and A10B remain documented only as rejected-method history so future work does not repeat them. A materially different future representation requires a new explicit design discussion and approval; the existing network family must not be silently revived.


### 2026-07-12 — Patch V3J.4A9AR: Regional Network Boundary and Debug Independence

**Historical status:** superseded; all candidate controls were removed by V3J.4A10R.

V3J.4A9A is rejected as a successful contour-network architecture. Its downward-only chains are mathematically valid, but the generator creates one complete local construction per accepted descriptor. Branches and echoes therefore decorate isolated symbols rather than connecting the field into shared terrain structure.

Authoritative visual finding:

```text
more chains inside one descriptor-local result
is not equivalent to
one network shared by a local region
```

The A9A generator was temporarily retained as rejected comparison evidence by A9AR, but A10A removes that provisional code path. A9A remains only as historical evidence and must not be restored as a production or comparison representation.

At that historical stage, Scene-view presentation was independent and additive:

```text
accepted true-position overlay
rejected A9A true-position overlay
editor-offset paired comparison copies
```

Any subset could be enabled. The paired preview did not force either true-position overlay. In paired mode, both copies are offset around the source anchor—accepted to visual left and A9A to visual right—without mutating stored geometry or validation footprints.

The then-proposed candidate, V3J.4A10A, was intended to use a regional generation unit:

```text
nearby directionally compatible descriptor group
→ one shared high spine/root system
→ connected downward-only chains spanning the group
→ optional restrained branches and lower related echoes
→ one complete unbroken regional network
```

Descriptors are inputs to grouping and network constraints, not one-to-one visible symbols. A regional candidate may use some descriptors as trunk anchors, some as branch targets, and some only as spacing or validation support.

The downward-only doctrine remains mandatory for every directed path:

```text
next visual height <= current visual height + numerical tolerance
```

This historical design gate was not met. A10A produced only sparse Y-like survivors, and A10B produced repeated table/π-symbol formations. A10R subsequently removed the network implementation.

A9AR added no new shape-generation behavior. It historically established independent comparison controls before the later A10A/A10B experiments; those candidate controls were removed by A10R.


### 2026-07-12 — Patch V3J.4A9A: Separate Downward-Only Cluster Candidate

**Historical status:** rejected as a network solution; implementation removed by V3J.4A10A.

The sole accepted Painted Accent representation remains the complete mesh-free A6/A7 projected glyph. V3J.4A9A introduced a separate unaccepted native-2D contour-cluster candidate for Scene-view evaluation. The candidate may share placement descriptors and validation infrastructure, but it may not alter the accepted glyph's arrays, cache signature, diagnostics, controls, or debug meaning.

Authoritative representation boundary:

```text
accepted path
    GroundPaintedAccentSurfaceStroke
    → A6 longitudinal profile
    → fixed world +Z projection
    → complete accepted projected glyph

experimental A9A path
    GroundPaintedAccentSurfaceStroke
    → separate directed 2D control graph
    → complete downward-only contour cluster
    → separate candidate snapshot and Scene Handles
```

The candidate's shape grammar is a directed mound-contour family, not a freeform crack or contour graph. It owns one shared high/root point, two independently shaped outward primary arms, zero to two child branches, and an optional lower echo. Every chain is directed from high/root to free end. In that direction its world-`+Z` visual coordinate may remain level or decrease but may never rise.

Required invariant:

```text
next visual height <= previous visual height + 0.0005 m
```

This forbids:

```text
cups
valleys
upward hooks
free ends curling toward a higher parent
a chain that drops and later climbs
an echo that rises back toward its source arm
```

It permits:

```text
near-horizontal crest shoulders
firm downward legs
long shallow descending runs
smooth downward forks
lower parallel echoes that preserve the parent's descent
```

The interpolation contract is monotone cubic Hermite evaluation of outward distance and accumulated drop, followed by dense sampled auditing. Unconstrained Catmull-Rom and overshooting spline handles are prohibited.

The candidate is complete and unbroken in A9A. No visibility windows, gaps, clipped parent pieces, or fragmentation logic may be applied. A later experiment may remove restrained sections only after the full cluster proves coherent and only while preserving one dominant readable component, every junction, and the downward-only mound interpretation.

Debug and data separation is mandatory:

```text
accepted toggle: complete red/purple/yellow A6/A7 glyph
candidate toggle: complete cyan/green/blue A9A cluster
comparison toggle: editor-offset pair, baseline left and candidate right
```

The comparison offset exists only in Scene Handles. Stored positions, validation footprints, caches, and future production data remain at their true coordinates.

Candidate chains undergo fresh centreline and full-width sampling, broad-slope, local-grade, river, and modifier validation. The entire cluster is rejected if one chain fails. Partial acceptance would silently change the designed graph and is not allowed in this proof.

A9A remains data-only and editor-only. It creates no secondary geometry, renderer, child object, collider, material, shader pass, texture, or runtime update. The accepted future production destination remains an R8 coverage bake blended into ground albedo, but no candidate advances to that stage until its complete shape language is explicitly accepted.


### 2026-07-12 — Patch V3J.4A8R: Baseline Restored; New Candidate Must Be a Downward-Only Cluster

V3J.4A8 is rejected as both a visual premise and an integration strategy. It exposed windows from each small accepted A6 mound and replaced the accepted overlay with those clipped pieces. The result was a field of disconnected random-looking strokes, not a larger implied terrain structure.

V3J.4A8R restores the authoritative A7 representation without fragment metadata or clipping:

```text
accepted representation: complete A6 projected contour
accepted debug meaning: full contour, full width boundaries, visible crest marker
accepted data: complete LocalSurfacePoints and HalfWidths only
rejected: visibility windows cut inside accepted mound glyphs
```

The next candidate is not a modified A6 glyph. It is a separate native-2D **downward-only contour cluster** evaluated beside the accepted baseline.

Authoritative candidate visual grammar:

```text
one coherent high crest or primary spine
longer native-2D primary contour than the accepted mound glyph
arms generated outward from their high/root point
zero to two related branches that inherit the parent tangent
optional lower parallel echo with related curvature
complete unbroken cluster during the first proof
no upward hook, cup, valley, or drop-then-rise sequence
```

Fixed world `+Z` remains permanent screen-up. Along every directed chain moving away from its high/root point, projected height may remain level or decrease only. The candidate must satisfy:

```text
nextHeight <= currentHeight + numericalTolerance
```

A complete mound family may contain left and right arms, but each arm is generated independently from the high region toward a lower free end. Branches attach to an internal parent point and may continue sideways or downward; they may never begin low and curl upward. Parallel echoes must obey the same rule and must not turn upward toward the primary chain.

Interpolation must preserve this order between controls. Monotone cubic Hermite interpolation or height-bounded cubic Bezier handles are acceptable. Unconstrained Catmull-Rom interpolation is not acceptable because it can overshoot into an upward hook even when its controls are ordered correctly. Dense post-sampling must report and reject any upward excursion.

The A9A experiment is intentionally unbroken. It first asks whether a larger connected or strongly related contour family reads as mound-like terrain notation. Only after that complete structure succeeds may A9B consider a very small number of gaps. Any later gap logic must preserve one long dominant component, avoid junctions, and retain the downward-only read.

Architectural separation is mandatory:

```text
accepted A6/A7 glyph generator, cache, diagnostics, and toggle remain unchanged
candidate cluster owns separate data, cache, diagnostics, and Scene-view toggle
candidate generation cannot mutate or replace accepted glyph arrays
comparison may offset Scene Handles only; stored geometry remains at true positions
```

The candidate remains mesh-free and data-only. It adds no `Mesh`, `MeshFilter`, `MeshRenderer`, child `GameObject`, collider, material, shader pass, texture, or per-frame camera work during the proof stage. Any displaced candidate chain requires its own full-width ground, river, modifier, slope, and grade validation; it cannot inherit parent validity merely because it shares a seed or origin.


### 2026-07-12 — Patch V3J.4A7: Mesh-Free Projected Contours Are the Sole Active Representation

The Painted Accent 3D crowned-ribbon experiment is retired. It proved useful longitudinal shape mathematics, but the secondary geometry representation was less successful than the mesh-free projected contour and is no longer part of the architecture. There is no active Painted Accent child mesh, renderer, material, collider, ground-normal displacement path, comparison action, or dedicated preview shader.

Authoritative representation contract:

```text
accepted ground-surface descriptor
→ A6 continuous scalar contour profile
→ fixed world +Z embedding
→ tapered visible width
→ transformed-footprint validation
→ current Scene-view diagnostics
→ future R8 coverage bake
→ future ground-albedo composition
```

The fixed gameplay camera makes world `+Z` permanent screen-up. Painted Accent profile controls describe this 2D projected contour only. Historical serialized field names are retained for asset compatibility and do not imply an active fold mesh or raised representation.

A7 originally proposed fragmenting the complete A6 contour as the next experiment. V3J.4A8 tested that premise and was rejected: cutting already-isolated mound glyphs produced disconnected scraps and incorrectly replaced the accepted overlay. V3J.4A8R restores the complete A7 contour as the sole active representation. The corrected experimental direction is the separate, unbroken, downward-only native-2D cluster described above. Production remains a projected coverage field blended into final ground albedo after family/variant surface composition.

## Historical architecture record — superseded by V3J.4A7

All sections below this heading are retained as design history. Any older statement that treats the raised ribbon, secondary Painted Accent geometry, comparison shader, or dual raised/projected path as active is superseded by the sole mesh-free projected-contour contract above.

### 2026-07-12 — Patch V3J.4A6: Continuous Profile and Endpoint-Angle Contract

Painted Accent profiles are continuous curves, not corrected polylines. The authoritative shared profile starts from the legacy solved profile knots, retains their seeded character, adds smooth signed detail, constrains the result above a positive mound floor, and evaluates one shape-preserving cubic spline at 65–97 samples. Adjacent spline segments share one tangent at every source knot; lower-leg elbow detection and reconstruction are no longer part of the architecture.

Ground entry is authored in physical profile-space angles. Each endpoint independently requests a continuous angle between `12` and `68 degrees`, converted using actual stroke length and Profile Height. Soft, moderate, and steep entries must all remain materially represented after monotonic derivative limiting. A dense population in which most applied endpoints are nearly parallel to the baseline is not accepted.

Visual variation may be signed, provided the final contour remains above its positive mound floor. This permits a shoulder to rise, release slightly, and rise again without producing the broad inward collapse previously rejected. Positive-only bell stacks are no longer authoritative because they changed fullness more readily than directional rhythm and made distinct parameters look like the same arch.

Population diversity is judged from normalized sampled silhouettes. Nearest-neighbour RMS distance, near-duplicate pair count, and largest near-duplicate cluster are the evidence; internal seed or parameter differences are not sufficient.

The contract remains:

```text
exact grounded endpoints
+ materially varied endpoint angles
+ C1-continuous legs and crest
+ smooth signed multi-scale character
+ positive mound safety floor
+ one unique dominant crest
+ fixed world +Z projected embedding
```

Projection, width, descriptor placement, river/modifier exclusion, shaders, and family/variant assets remain representation-independent and unchanged.

### 2026-07-12 — Patch V3J.4A5: Endpoint Diversity and Profile-Population Contract

Painted Accent legs must not all stitch into the ground with the same horizontal tangent. The fixed camera and world-`+Z` projection make endpoint angle a major part of the visible glyph language, so each endpoint-to-crest leg now owns independent continuous takeoff and curvature parameters.

The positive mound foundation uses a monotone cubic-Hermite leg with a seeded endpoint derivative in `0.10–2.80` and a monotonic curvature warp in approximately `-0.45…+0.45`. Every leg still reaches the dominant crest with zero slope, but grounded entries may be soft, moderate, or steep. Steep entries are an intentional part of the family population, not errors to be normalized away. Left and right legs vary independently.

Profile diversity must come from continuous parameter variation rather than a small named archetype set. The former mandatory broad-shoulder-plus-fine-bells recipe is replaced by two to six positive events per sufficiently long leg, continuously varying in amplitude, centre, and width. Endpoint protection and detail release are also independently seeded per leg. Variation remains positive-only and subordinate to the unique dominant crest.

Lower-leg corner suppression is an exceptional safety repair, not a general smoothing stage. It may repair an early height reversal or an isolated turn exceeding both `34°` and the neighbouring median by `1.8× + 7°`. It must preserve the selected endpoint derivative. A correction rate near or above `25%` is evidence that the guard has become a population normalizer and must not be accepted.

Visual acceptance requires all of the following in one dense sample:

- clearly represented soft, moderate, and steep ground entries;
- no hard elbows on steep entries;
- independent left/right leg character;
- no obvious two-or-three-template repetition;
- positive multi-scale contour variation without inward sag;
- one rounded dominant crest;
- exact endpoint grounding and unchanged world-`+Z` projection.

### 2026-07-12 — Patch V3J.4A4: Smooth Grounded Takeoff Contract

The A3 positive mound grammar remains authoritative, but positive variation may not begin with a visible mechanical elbow. Every Painted Accent leg now has a protected grounded takeoff before broader shoulders and small contour events become fully active.

The shared profile uses smooth envelope selection rather than hard `min`/`max` crossover points, suppresses retained raw and seeded detail through the first `20%` of each leg, and releases that detail smoothly by `38%`. Broad shoulders are centred in the upper `42–72%` of a leg; fine positive events begin no lower than `30%`.

A final guarded lower-leg audit measures the first `40%` in normalized endpoint-to-crest coordinates. A turn above `26 degrees` or an early height reversal triggers a local cubic-Hermite takeoff rebuild. The endpoint, upper leg, crest, and all accepted positive detail above the repair anchor remain fixed. The guide and positive residual are rebuilt separately so no correction can violate the A3 positive mound floor.

This is not a general smoothing pass. Accepted legs retain their chaotic positive shoulders and mini-events. The contract is simply:

```text
grounded endpoint
→ smooth takeoff
→ positive irregular contour detail
→ one dominant rounded crest
```

Hard lower-leg elbows, negative sags, random projection sides, and long globally smoothed strokes are not accepted. Projection, width, exclusions, shaders, and raised topology are unchanged.

### 2026-07-12 — Patch V3J.4A3: Positive Crest-Smooth Contour Grammar

The accepted V3J.4A1 architecture remains unchanged: one mesh-free scalar profile feeds the fixed-world-`+Z` projected glyph and the legacy raised comparison. V3J.4A3 replaces the rejected A2 signed-articulation experiment with a stricter visual grammar:

```text
crest-flat asymmetric positive mound
+ retained positive raw detail
+ positive broad shoulders
+ positive fine contour events
- negative sags
- cusp-producing power-law joins
```

The mound foundation uses a smootherstep basis raised by the existing seeded side sharpness. It has zero analytic slope at both the grounded endpoint and dominant crest. Every final sample must remain on or above this foundation. Raw variation contributes only where it rises above the guide, and all newly seeded contour events are additive. No negative detail term exists.

Every leg long enough to support detail receives one broad positive shoulder and one to three finer positive events, scaled by **Profile Irregularity**. Detail fades out near endpoints and inside the protected crest zone, and non-crest samples remain below `0.88 ×` the dominant peak. This keeps one dominant rounded mound while avoiding long sterile ramps.

A2 is historical only. Its signed broad bell and absolute chord-deviation success metric are rejected because they allowed inward sags and counted them as useful variation. The authoritative diagnostics now require zero negative detail samples and zero samples below the positive mound guide. Projection direction, descriptor placement, full-width validation, river/modifier exclusion, width taper, shaders, and raised topology remain unchanged.

### 2026-07-12 — Patch V3J.4A1: One Shared Profile, Two Embeddings

The accepted architectural distinction is between **shape mathematics** and **representation**. The raised crowned-ribbon experiment is rejected as the final material representation, but its longitudinal silhouette mathematics is accepted. V3J.4A failed because it correctly removed the mesh while also discarding that solved profile and inventing a weaker sine-based plan curve.

V3J.4A1 establishes one representation-independent scalar profile:

```text
accepted placement descriptor
→ shared longitudinal profile evaluator
→ H(t): solved crest + crown height
      ├─ legacy raised comparison: local ground normal × H(t)
      └─ projected ground glyph:   fixed world +Z × H(t)
```

The evaluator reproduces the legacy raised calculation without changing arithmetic order: 17–25 descriptor-index samples at the existing `0.09 m` target spacing, five cross-profile crest probes, seeded profile bases, asymmetric single-mound shaping, plateau suppression, valley repair, rounded-crest shaping, peak normalization, end envelopes, and the existing `0.12` endpoint width floor.

The shared physical profile is:

```text
H(t) =
  ProfileHeight
  × lerp(0.94, 1.0, stroke.Strength)
  × normalizedCrestHeight(t)
  + CrestCrownHeight
  × crownEndEnvelope(t)
```

The gameplay camera is fixed permanently. World `+Z` is the authored screen-up direction and is converted once into the GeneratedGround local X/Z plane. This removes all left/right ambiguity: projected profiles never choose a random bend side and never use the local stroke perpendicular as their height direction. Only visible line width uses a perpendicular to the final projected tangent.

The V3J.4A-only `Projected Profile Spread` control and its independent resampler, crest selector, sine envelope, modulation, random sign, and width taper are removed. The shared family/variant controls are now **Profile Height**, **Crest Crown Height**, **Profile Irregularity**, **End Taper**, and **Stroke Width**. Existing serialized field names for the legacy controls remain intact to avoid style-asset migration.

The projected representation is still pure data:

```text
Vector3[] local surface points
float[] half widths
seed
crest T
crest/crown/combined peak heights
projection-invariant errors
```

It creates no `Mesh`, `MeshFilter`, `MeshRenderer`, child object, collider, material, shader pass, or per-frame camera work. The raised preview remains an explicit editor comparison and consumes the same profile samples before applying its unchanged crowned cross-section and ground-normal lift.

After `+Z` transfer, centre and tapered left/right footprint samples are revalidated against valid ground sampling, broad slope, transverse/longitudinal grade, river surface/bed/handoff plus safety clearance, and `GroundModifier` Painted Accent exclusion. Any invalid transformed sample rejects the whole glyph. Clipping, spread fitting, relocation, and proposal backfill remain excluded.

The later V3J.4A1a visibility correction supersedes the initial cyan-on-turquoise palette. Scene diagnostics now use a black-outlined red projected centreline, black-outlined dark-purple width boundaries, a black-outlined yellow crest marker, and reason-coloured rejection crosses. Added diagnostics prove that the displacement contains only the shared height in the authored north direction:

```text
maximumNorthDisplacementError <= 0.00001 m
maximumCrossAxisDrift         <= 0.00001 m
```

V3J.4A1 remains a shape-equivalence proof, not production rendering. The old 256 RGBA diagnostic field, final ground shader, and raised comparison shader remain unchanged. V3J.4B may begin only after Unity confirms that the projected cyan curve matches the useful brown upper-silhouette grammar while the legacy raised output remains unchanged.

### 2026-07-11 — Patch V3J.3D5: Environment-Integrated Flat Ink

The active Painted Accent material proof is no longer completely unlit. Unity validation showed that uniform C8 ink remained visibly bright beneath cast shadows and at night, separating the marks from the ground. V3J.3D5 retains the flat graphic colour model but modulates that colour by local environmental illumination and shadow attenuation.

Current material contract:

```text
shader: PS3D/Ground Painted Accent Ink
pass: UniversalForward
culling: Off / double-sided
normal-based diffuse: none
specular/metallic/emission/textures: none
ambient response: 0.75
main-light response: 0.80
main/additional shadow response: 0.70
additional-light response: 0.25
minimum visibility: 0.14
maximum exposure: authored Ink Color
cast shadows: Off
receive shadows: True
```

Light colour is converted to luminance before modulation, preserving the family/variant-authored Ink Color hue. The shader uses world position, ambient spherical harmonics, main-light shadow attenuation, and restrained additional-light attenuation, but never mesh normals. Geometry, placement, exclusions, distribution, topology, and authoring controls are unchanged.

The D4 apex-softening result was not materially better in the gameplay camera and remains a separate unresolved geometry-polish question. D5 validation must judge only terrain integration under daylight, cast shadows, night, and local lights.

### 2026-07-11 — Patch V3J.3C8: Flat Ink Surface Baseline

C7 validation confirmed that the accepted C5 geometry plus C6 double-sided rasterization renders the entire crowned ribbon consistently from all tested viewpoints. Geometry is therefore locked for the current Painted Accent milestone. The C7 lit response was rejected as a final visual direction: normal lighting, crown lift, edge darkening, endpoint softening, per-stroke brightness variation, saturation changes, and shadow reception made the marks read as small physical brown ridges rather than graphic outline strokes.

V3J.3C8 replaces that material model with one opaque, uniform, double-sided ink colour. The geometry supplies only silhouette, overlap, perspective, and parallax; the shader supplies no geometric or environmental shading.

Asset replacement:

```text
remove:
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentLit.shader
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentLit.shader.meta

add:
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentInk.shader
  Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentInk.shader.meta

shader name:
  PS3D/Ground Painted Accent Ink
```

Final flat-ink contract:

```text
rendering: opaque unlit
colour: one uniform artist-authored Ink Color
culling: off / both sides visible
ZWrite: on
ZTest: LEqual
scene lights: ignored
main/additional-light shadows: ignored
ambient and baked GI: ignored
screen-space ambient occlusion: ignored
reflection probes: disabled
light probes: disabled
fog: omitted from this baseline
metallic/specular/smoothness/emission: not present
textures/noise maps: not present
cast shadows: off
receive shadows: off
```

`GroundSurfaceFeatureRecipe` now owns one `PaintedAccentInkColor` value, exposed as **Ink Color** in both Painted Accent authoring surfaces. The default is a dark warm neutral `(0.12, 0.10, 0.08, 1)`. Alpha is not authored because the shader is intentionally opaque. A renderer property block supplies the colour per GeneratedGround preview while retaining one shared material and one combined mesh, so different style variants are not forced to share one static colour.

C8 removes the C7 UV1 seed stream and all corresponding generation, statistics, constants, and diagnostics. UV0 remains available but the flat shader does not consume it. With position, normal, and UV0 retained, estimated stride returns from 40 to 32 bytes. For the demonstrated 36-stroke topology:

```text
vertices:               1,404
triangles:              1,728
estimated vertex bytes: 44,928
estimated index bytes:  10,368
estimated raw mesh:     55,296
```

The accepted C5 shape constants, C6 `Cull Off` visibility, 13-row minimum, three vertices across, endpoint embed, open underside, no collider, one-child/one-renderer architecture, descriptor generation, placement, orientation, and immutable base ground remain unchanged.

C8 acceptance requires every point and both sides of a stroke to render the same authored ink colour under different scene-light positions and intensities. There must be no crown highlight, edge darkening, endpoint fade, stroke-seed brightness change, shadow band, probe response, specular response, or saturation shift. After this baseline passes, individual-line appearance is locked and work moves to distribution and placement rather than further geometry or material complexity.

### 2026-07-11 — Patch V3J.3C7: Painted Accent Surface-Response Proof

C6 validation closed the Painted Accent geometry and visibility gate. With culling disabled, the same open crowned ribbon remained visible from both exterior and interior viewpoints; the apparent disappearing-leg defect was therefore back-face culling, not insufficient leg height, missing side-shell geometry, or unstable longitudinal generation. The retained baseline is now C5 geometry plus permanent double-sided rendering. Do not resume crest, shoulder, Fold Height, end-envelope, or topology tuning unless later evidence identifies a new geometric defect.

V3J.3C7 was implemented and validated as a rendering experiment, but its lit/materially shaded direction was rejected in favour of the C8 flat-ink baseline. It replaces the generic URP Lit proof material with a dedicated lightweight shader:

```text
Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundPaintedAccentLit.shader
shader name: PS3D/Ground Painted Accent Lit
```

The shader remains opaque, rough, non-metallic, non-emissive, double-sided, and shadow-receiving while the preview renderer continues to cast no shadows. Back-facing fragments flip the interpolated world normal before lighting, so interior and exterior views retain comparable diffuse response instead of rendering the reverse side with an inverted normal.

The accepted geometry already writes:

```text
UV0.x = normalized position along the stroke
UV0.y = normalized position across the three-vertex crown
```

C7 adds one deterministic secondary channel:

```text
UV1.x = one 0–1 seed value shared by every vertex in one stroke
UV1.y = reserved / zero
```

The dedicated shader uses those channels for a restrained no-texture finish:

```text
base colour:                 (0.50, 0.46, 0.40, 1)
crown brightness lift:       0.10
outer-edge darkening:        0.08
endpoint softening span:     0.12 of each end
endpoint contrast scale:     0.55
per-stroke brightness range: ±0.05
smoothness:                  0.08
```

`UV0.y` produces a smooth centre-crown lift and outer-edge darkening rather than three hard bands. `UV0.x` reduces cross-sectional contrast and slightly desaturates only the terminal span; it does not use alpha, transparency, clipping, or sorting-dependent fading. `UV1.x` creates a small stable brightness difference between strokes without adding along-stroke noise, textures, atlases, per-frame state, per-stroke objects, or extra materials.

The material resolver requests the dedicated shader first and retains the previous URP Lit/Simple Lit/Unlit chain only as a compile/import fallback. Cached proof materials are upgraded to the dedicated shader when it becomes available. The existing `_Cull = Off` and `doubleSidedGI = true` state remains permanent.

Topology remains 1,404 vertices and 1,728 triangles for the demonstrated 36-stroke case. Adding UV1 changes the estimated vertex stride from 32 to 40 bytes:

```text
estimated vertex buffer: 1,404 × 40 = 56,160 bytes
estimated index buffer:  1,728 × 3 × 2 = 10,368 bytes
estimated raw mesh:                    66,528 bytes
```

The build diagnostic now reports the dedicated shader, UV1 seed range, and all proof response values. C7 does not change descriptors, distribution, width, Fold Height, Crown Height, C5 shaping, C6 visibility, normals, winding, collider behavior, base ground, recipes, inspectors, style assets, scenes, River, Generated Mass, or GroundModifier systems.

Acceptance requires the normal gameplay camera to retain the accepted silhouette while gaining a restrained readable crown, slightly darker outer edges, softer grounded terminals, stable two-sided lighting, and subtle stroke-to-stroke material variation. Reject emission, glow, Fresnel outlines, obvious bands, transparent tips, noisy texturing, metallic/specular emphasis, rope/rail/root readings, or any response that competes with characters and gameplay. Distribution remains deferred until individual-line surface response is accepted.

### 2026-07-11 — Patch V3J.3C6: Double-Sided Interior-Face Visibility Validation Result

C6 was validated successfully. The same asymmetric stroke was inspected from exterior, profile, and interior-facing viewpoints. With `_Cull = CullMode.Off`, the previously missing interior side remained rendered and the leg stopped appearing/disappearing as the camera crossed the ribbon. This proves the unresolved C5 leg defect was ordinary back-face culling on the open crown surfaces rather than insufficient Fold Height, Crown Height, shoulder support, longitudinal sampling, or missing physical shell depth.

The accepted representation baseline is therefore:

```text
C5 valley-suppressed three-vertex crowned ribbon
+ C6 double-sided rasterization
+ Material.doubleSidedGI = true
```

The open underside, 13-row minimum, three vertices across, endpoint embed, no-collider contract, one combined child mesh, and immutable base ground remain accepted. A shallow side shell is not required. Geometry/topology shaping is now locked while individual-line surface response is developed.

For 36 strokes, the accepted pre-C7 topology remains 1,404 vertices and 1,728 triangles. C6 itself adds no mesh channels or storage. The successful diagnostic log reports `materialCull=Off` and `doubleSidedGI=True`.

### 2026-07-11 — Patch V3J.3C5: Valley-Suppressed Crowned Ribbon Refinement

V3J.3C4 validation confirmed that strict single-crest shaping removed the repeated `M` silhouette, but it overcorrected the longitudinal character: a `0.70` guide blend plus hard monotonic rise/fall guards made most marks read as simple `^` profiles with too little seeded variation. C4 also raised the shoulder crown factor to `0.35`, but the entire crown still used the longer Fold End Taper envelope. Crown body therefore remained near zero along the first and final interior rows, so side-biased views could still lose the legs.

V3J.3C5 is the retained longitudinal-shape baseline validated before the C6 material diagnostic. It preserves the accepted three-vertex open crowned ribbon, five-position stochastic crest search, thirteen-row minimum, descriptor generation, whole-chunk placement, facing rule, signed jitter, authoring controls, endpoint embed, one combined preview mesh, no-collider contract, material, and immutable base ground. It changes only longitudinal profile correction and crown support near the stroke ends.

Longitudinal shaping now uses restrained guidance rather than a hard unimodal constraint:

```text
1. evaluate the unchanged raw crest samples;
2. select the highest interior sample and build the same broad rise/fall guide;
3. blend raw height toward that guide at 0.35 rather than 0.70;
4. do not enforce monotonic rise or monotonic fall;
5. inspect the blended samples for substantial one-row valleys;
6. when a valley is deeper than 0.08 of the stroke peak, lift it 60% toward the lower neighbour.
```

This keeps the dominant-hill bias while restoring asymmetry, shelves, minor bumps, local slope changes, and other seeded irregularity. The valley pass targets only pronounced local dips; it does not flatten every variation or guarantee a mathematically monotonic profile.

Cross-sectional shoulder support changes from `[0.35, 1.00, 0.35]` to `[0.50, 1.00, 0.50]`. At the focused `0.02 m` Crown Height, each shoulder can therefore receive up to `0.01 m` while the centre can receive `0.02 m`.

The crown now has a separate short terminal support envelope. The original Fold End Taper remains authoritative for macro Fold Height and width. Crown Height uses the greater of that original envelope and a `0.12`-of-stroke short ramp multiplied by `0.45` leg support. Both envelopes are exactly zero at `t = 0` and `t = 1`, so the terminal rows remain grounded and embedded by `0.002 m`; only the interior leg rows gain earlier and later cross-sectional body. No side walls, bottom, caps, collider, underside fill, or closed solid are introduced.

Topology and storage remain unchanged from C3/C4. For 36 strokes:

```text
vertices:  36 × 13 × 3 = 1,404
triangles: 36 × 12 × 2 × 2 = 1,728
estimated raw mesh: 55,296 bytes
```

The build diagnostic identifies:

```text
singleCrestBlend=0.350
valleyThresholdFraction=0.080
valleyRepairStrength=0.600
shoulderCrownFraction=0.500
legCrownSupport=0.450
crownEndRampFraction=0.120
```

Focused validation remains `Stroke Width = 0.02 m`, `Fold Height = 0.25 m`, and `Crest Crown Height = 0.02 m`. Acceptance requires visibly more longitudinal variety than C4, substantially fewer deep two-hill valleys than C3, stronger leg body in side-biased views, exact grounded endpoints, and no rail, bar, roof, blade, root, wire, wall, or obstacle reading. Shader finish and distribution remain deferred until this shape gate passes.

### 2026-07-11 — Patch V3J.3C4: Single-Crest Crowned Ribbon Validation Result

C4 proved that a dominant-crest guide can remove the common two-hill `M` silhouette, but its `0.70` guide blend and hard monotonic guards were rejected as final shaping because most strokes collapsed toward simple `^` contours with insufficient irregularity. Its `[0.35, 1.00, 0.35]` crown distribution added some shoulder body, but did not solve leg disappearance: the shoulder and centre crown contributions still followed the same long Fold End Taper envelope and therefore approached zero together along the terminal legs.

C4 retained the correct open crowned-ribbon architecture and unchanged 1,404-vertex / 1,728-triangle topology for 36 strokes. C5 supersedes only the over-strong profile correction and weak terminal crown support; C4 remains useful evidence that strict monotonic shaping is unnecessary and visually sterile for this feature.

### 2026-07-11 — Patch V3J.3C3: Grounded Crowned Crest Ribbon Validation Result

V3J.3C3 was visually validated and retained as the geometry direction. V3J.3C2 had proved that the open-under-crest topology removes the filled-hill failure, but its two equal-height vertices formed a curved sheet with no cross-sectional body. C3 added a three-vertex crown and, at narrow width with a pronounced macro Fold Height, produced a substantially more readable and intentional terrain accent in gameplay and close side views. Validation then isolated two refinement defects: repeated two-high-region `M` silhouettes along the stroke and shoulder/leg disappearance because the centre alone received Crown Height. V3J.3C4 first addressed those defects but overconstrained the profile; V3J.3C5 keeps the C3 representation while refining the correction.

V3J.3C3 preserves the validated descriptor generation, whole-chunk placement, facing rule, signed jitter, five-sample longitudinal crest search, thirteen-row minimum, endpoint grounding, one combined child mesh, visual-only/no-collider contract, material, and base-ground immutability. It changes only the ribbon cross-section and authoring range:

```text
three vertices across each longitudinal row:
  left shoulder  = generated longitudinal crest height
  centre crown   = generated longitudinal crest height + Crest Crown Height
  right shoulder = generated longitudinal crest height

two sloped crown faces:
  left shoulder -> centre crown
  centre crown -> right shoulder

underside:
  empty
```

`Crest Crown Height` is a new explicit geometry control with range `0–0.05 m` and initial proof value `0.02 m`. It is multiplied by the existing deterministic end envelope, so the crown fades out with the same start/finish grounding as Fold Height. The shoulders continue to use independently sampled ground height and render normals. The centre also samples the underlying surface independently before receiving its local crown offset. No side walls, underside, caps, collider, or closed solid are added.

The `Stroke Width` authoring minimum is reduced from `0.04 m` to `0.01 m` so the approved `0.02 m` narrow-line proof is representable. `Fold Height` is extended from `0–0.25 m` to `0–0.50 m`; `0.25 m` is the focused normal proof value rather than the slider ceiling. The existing Fold Height serialized default remains `0.018 m`, and `GSSP_Snowfield.asset` is not explicitly modified by the patch.

For the demonstrated 36-stroke case, the expected proof topology is:

```text
36 strokes × 13 longitudinal rows × 3 crown vertices = 1,404 vertices
36 strokes × 12 longitudinal spans × 2 cross spans × 2 triangles = 1,728 triangles

estimated vertex buffer: 44,928 bytes
estimated index buffer:  10,368 bytes
estimated raw mesh:      55,296 bytes
```

The focused validation holds Fold Height and Stroke Width constant while changing only crown height:

```text
Stroke Width: 0.02 m
Fold Height:  0.25 m

A: Crest Crown Height 0.01 m
B: Crest Crown Height 0.02 m
C: Crest Crown Height 0.03 m
```

C3 passed the direction gate for gameplay readability and real cross-sectional lighting, but it did not complete the final-shape gate. The diagnostic reports requested crown height, three vertices across, macro crest peaks, effective crown peaks, combined peaks, effective width, topology, memory estimate, material state, and build time. Distribution and final shader response remain deferred while C4 corrects the longitudinal `M` profile and weak shoulder/leg body.

### 2026-07-11 — Patch V3J.3C2: Grounded Open Crest Ribbon Validation Result

V3J.3C2 was validated at requested Fold Heights `0.12 m`, `0.18 m`, and `0.24 m`. It preserved the validated deterministic `GroundPaintedAccentSurfaceStroke` descriptors, whole-chunk placement, facing rule, signed jitter, visible `Stroke Width`, and optional projected/debug field while changing only the visual-only secondary mesh representation.

At each longitudinal row, the existing stochastic fold profile is still evaluated at five cross positions (`u = -1, -0.5, 0, 0.5, 1`). The maximum normalized result becomes one crest height. The mesh then generates exactly two vertices across the stroke at `±Stroke Width / 2`, with both vertices lifted to that crest height from independently sampled ground positions and normals. Consecutive rows form one quad. There are no cross-width triangles descending from the crest to the ground, no underside, and no collider.

Ground connection is longitudinal only: the existing deterministic end envelope drives height and width toward the first and final row, and only those terminal pairs receive the `0.002 m` embed. Side-edge embedding is removed because every generated vertex is now a ribbon edge. The minimum longitudinal resolution is thirteen rows so the start and finish ramps contain enough intermediate segments to read as rising from and returning to the terrain rather than abruptly appearing above it.

For the demonstrated 36-stroke case, the expected proof topology is:

```text
36 strokes × 13 longitudinal rows × 2 ribbon vertices = 936 vertices
36 strokes × 12 longitudinal spans × 2 triangles = 864 triangles

estimated vertex buffer: 29,952 bytes
estimated index buffer:   5,184 bytes
estimated raw mesh:      35,136 bytes
```

The calibration-only Fold Height range was extended to `0–0.25 m` without changing the serialized default or `GSSP_Snowfield.asset`. The `0.12 m` and `0.18 m` logs proved stable thirteen-row/two-wide topology at 936 vertices and 864 triangles, with mean generated macro peaks `0.1140 m` and `0.1709 m`. Visual validation at `0.12`, `0.18`, and `0.24 m` proved the area underneath remained empty and the longitudinal rise was clearly present. The representation was nevertheless rejected as final because both cross-width vertices shared one height: the mark remained a flat raised sheet with negligible across-width lighting variation and increasing perspective loss at distance. V3J.3C3 addresses that exact missing cross-sectional form.

The explicit build diagnostic now reports `crestSearchSamples=5`, `ribbonVerticesAcross=2`, longitudinal sample counts, generated crest-peak heights, effective widths, material state, topology, memory estimate, and build time. The descriptor/texture generation coupling remains deferred to V3J.3D and must not be widened into this visual proof.

### 2026-07-11 — Patch V3J.3C1: Ridge Readability Calibration Result

V3J.3C1 was validated at requested Fold Heights `0.02 m`, `0.06 m`, and `0.12 m` using the same 36-stroke layout. The generated mean peak response was nearly linear (`0.0184 m`, `0.0552 m`, and `0.1105 m` respectively), while topology and width remained fixed at 1,260 vertices, 1,728 triangles, and approximately `0.0051–0.0590 m` effective width. This proved that Fold Height was functioning correctly.

The representation nevertheless failed visually. `0.02 m` was effectively invisible, `0.06 m` only became observable, and `0.12 m` was clearly raised but read as a hill because every interval across the five-sample profile was triangulated into one opaque continuous upper surface. The failure was therefore not insufficient height, hidden width change, or unstable topology; it was the filled cross-width representation itself. V3J.3C2 supersedes that representation while preserving its validated profile and descriptor inputs.

### 2026-07-10 — Patch V3J.3C: Narrow Static Secondary Ridge Reconciliation

V3J.3C resolves the V3J.3B decision gate. The active Painted Accent representation candidate is now a **separate, visual-only, narrow open ridge mesh** generated from the validated deterministic 3D surface-stroke descriptors. The existing `GeneratedGround` mesh and `MeshCollider` are immutable for this feature.

Canonical ownership and lifecycle:

```text
base GeneratedGround mesh:
  never displaced, retopologized, or replaced by Painted Accent work

base ground collider:
  never modified by Painted Accent work

secondary Painted Accent ridge mesh:
  one combined child mesh per ground chunk
  visual only
  no collider
  no end caps or underside
  shadow casting disabled
  one shared material
  generated only during editor authoring or camp/run loading
  static during gameplay

projected fold-field texture/shader path:
  retained as debug/support and as fallback only
  must become optional if secondary geometry is accepted
```

The visible ridge footprint comes only from `Painted Accent Stroke Width`. `GroundPaintedAccentSurfaceStroke.BodyWidth` remains available for the optional projected/debug fold-field texture, but it does not widen the secondary mesh. The obsolete `Fold Broadness` field/control is removed entirely because the audit found no remaining texture/debug consumer and its only active effect was to widen the rejected preview footprint.

The ridge uses seven generic samples across `u = -1..1`. Its stochastic Gaussian-basis profile, asymmetric cross-section, slow along-stroke variation, and height end taper remain intact. V3J.3C additionally tapers width at both ends, keeps a small non-zero terminal width to avoid degenerate rings, and embeds the side boundaries plus start/end rings slightly below the sampled ground surface. This creates a narrow open surface rather than a broad terrain apron, closed tube, root, wire, or worm.

The proof renderer contract is:

```text
child name:
  __PaintedAccentRidgePreview_Debug

renderer count:
  one per chunk

material:
  shared neutral/dark lit debug material

shadows:
  casts none
  receives ground/world shadows for form readability

motion vectors:
  camera-only; no per-object motion tracking
```

V3J.3C emits one compact explicit-build diagnostic containing stroke count, vertex count, triangle count, estimated vertex/index/raw mesh bytes, and build time. The estimate matches the proof layout: position + normal + UV0, with 16-bit indices unless the mesh exceeds 65,535 vertices. Preview meshes remain CPU-readable because editor rebuild/inspection still needs them; production camp/loading meshes may call `UploadMeshData(true)` after final upload.

V3J.3C deliberately leaves one cleanup boundary: descriptor generation and projected texture rasterization are still coupled inside the current fold-field generator call. V3J.3D must split those paths before production adoption so the secondary-mesh mode can generate descriptors/geometry without allocating or retaining the fallback texture.

Provisional budget:

```text
per chunk target:
  <= 1,500 vertices
  <= 2,000 triangles
  1 renderer
  1 shared material
  0 colliders
  0 runtime generation
  0 shadow-caster draw

visibility model:
  usually 1 visible chunk
  sometimes 2
  rarely 3
  almost never 4
  never 100 simultaneously visible
```

A full closed tube is not the default because it adds hidden lower-shell geometry, cylindrical lighting, cap/intersection problems, and root/wire/worm silhouette risk. Projection remains the fallback only if the narrow open ridge fails the gameplay-camera visual test or measured cost becomes unjustifiable.

### 2026-07-10 — Patch V3J.3B: Stochastic 3D Fold Surface Preview

Patch V3J.3B is the first height/form proof for the accepted 3D-stroke source model. The validated V3J.3A4 layout remains unchanged: strokes are distributed across the whole chunk, their length/density/width are explicit controls, and each stroke is perpendicular to `Facing Direction Degrees` plus a deterministic signed `Angle Jitter Degrees` roll.

The active preview is no longer a flat two-vertex ribbon. Each generated ground-following stroke is treated as the centerline of a sampled 3D surface:

```text
t = normalized position along the stroke, 0..1
u = normalized position across the fold, -1..1

surface(t, u) =
    groundPoint(t)
  + surfaceAcross(t) * u * foldHalfWidth
  + surfaceNormal(t) * height(t, u)
```

`height(t, u)` is not a fixed U, triangle, shoulder/crest template, or hand-authored list of semantic profile points. It is a deterministic smooth stochastic function. Every stroke receives one to four broad Gaussian basis functions. Their centers, widths, amplitudes, phases, and slow along-stroke evolution are derived from the stroke seed. The summed basis field is multiplied by a smooth edge envelope so both side edges return to the ground, a separate asymmetric end envelope so both stroke ends blend back into the ground, and low-frequency along-stroke modulation so height and profile fluctuate without adding lateral centerline squiggle.

The resulting profile can naturally form one broad rise, an offset rise, a shallow plateau, overlapping low rises, uneven slopes, or a locally flatter section. No code concept such as “left shoulder,” “crest count,” or “right shoulder” exists. Mesh vertices are generic samples of the same formula.

V3J.3B originally added four proof controls. V3J.3C retains `Fold Height`, `Fold Irregularity`, and `Fold End Taper`; it removes `Fold Broadness` from author-facing ridge controls because broad-body footprint scaling contradicts the narrow-ridge target. The obsolete broadness field is removed entirely because no remaining generator or shader path consumes it.

The preview uses 11 evenly spaced cross-stroke samples per existing ground-following stroke point, re-samples the base ground height/normal at every lateral vertex, recalculates mesh normals/tangents, and uses a lit debug material so the 3D form can be judged from lighting rather than read as a flat color strip. It remains editor/debug-only: it does not alter the production ground mesh, collision, or gameplay surface.

Patch V3J.3B deliberately did not add lateral stroke deviation, family-specific material integration, or production ownership. V3J.3C resolves that historical decision gate: narrow secondary geometry is the active candidate and projection is fallback only.

### 2026-07-10 — Patch V3J.3A4: Perpendicular Facing-Direction Contract

V3J.3A4 established the validated orientation contract:

```text
finalStrokeAngle =
    Facing Direction Degrees
  + 90 degrees
  + random(-Angle Jitter Degrees, +Angle Jitter Degrees)
```

`Facing Direction Degrees` represents the player/camera-facing direction in local X/Z. Painted Accent strokes are perpendicular to that direction. `Angle Jitter Degrees` is a per-stroke deterministic signed roll around the perpendicular result. The generic feature `Direction` vector is not the active Painted Accent orientation source.

### 2026-07-10 — Patch V3J.3A3: Explicit Base-Angle Audit

V3J.3A3 exposed the hidden-base-angle error in V3J.3A2 and made the authored angle explicit. V3J.3A4 immediately refined that control’s meaning from direct line angle to facing direction plus a perpendicular conversion. V3J.3A3 is therefore historical context, not the current authoring contract.

### 2026-07-10 — Patch V3J.3A2: Explicit Signed Angle Jitter Degrees

Patch V3J.3A2 kept the V3J.3A1 whole-chunk distribution fix and replaced orientation families with an explicit signed degree roll. Validation then proved that the roll was still centered around a hidden legacy direction. V3J.3A3 and V3J.3A4 supersede its orientation semantics. The retained lesson is only that angle variation must be a deterministic signed degree offset, not normalized “variety” or discrete orientation families.

### 2026-07-10 — Patch V3J.3A1: 3D Stroke Distribution Fix

Patch V3J.3A1 corrects two V3J.3A layout bugs reported from the 3D line preview. First, stroke placement no longer walks cells sequentially from a random offset and stops as soon as enough strokes are accepted. That row-major traversal could populate only one side of the chunk because accepted strokes filled the target count before the traversal reached the rest of the patch. The generator now builds the full candidate-cell set, assigns each candidate a deterministic random sort key, globally sorts that set, and accepts from the shuffled order. This preserves deterministic generation while spreading accepted strokes across the whole chunk.

Second, `Angle Variety` was replaced by explicit signed degree jitter rather than slash/vertical/backslash orientation families. V3J.3A3 and V3J.3A4 later corrected the hidden base direction and established the current facing-direction-plus-perpendicular contract.

This patch did not add raised fold height or lateral squiggle. V3J.3B now owns the first raised stochastic fold-surface proof.

### 2026-07-10 — Patch V3J.3A: 3D Stroke Distribution Controls

Patch V3J.3A kept the V3J.3R source-of-truth correction and fixed the first preview's layout problems: too few generated lines, overly long strokes, and overly uniform slash-like orientation. It deliberately did not solve raised fold height or lateral squiggle. Its historical validation target was the line layout itself; V3J.3B now supplies the raised-form proof needed before lateral deviation is judged.

The active Painted Accent controls are now explicit for 3D surface-stroke layout:

```text
Stroke Width         -> source-line/core width in metres
Stroke Density       -> approximate stroke count per standard 40x40 patch
Stroke Length Min    -> lower length bound in metres
Stroke Length Max    -> upper length bound in metres
Facing Direction Degrees -> player/camera-facing direction in local X/Z
Angle Jitter Degrees -> signed +/- offset around the perpendicular stroke angle
```

Generation no longer derives line count mainly from `Strength` or line length mainly from generic `Scale`. `Strength` remains feature intensity, while the explicit stroke controls own the visible 3D distribution. Stroke placement uses a larger deterministic attempt grid so density changes produce more reliable preview changes after support rejection. V3J.3A originally centered signed jitter around a preferred direction; V3J.3A4 superseded that detail with the active `Facing Direction Degrees + 90 degrees + signed jitter` contract.

V3J.3A remains only the historical layout-control patch. V3J.3B now sweeps the stochastic raised profile along the accepted surface strokes and owns the active height/form proof.

### 2026-07-10 — Patch V3J.3R: Painted Accent 3D Stroke Baseline Reconciliation

Patch V3J.3R resets the active Painted Accent baseline after the V3J.0-V3J.2 experiments proved the wrong source model. The prior active path generated a broad/noisy fold body field, tried to threshold or contour it, and then attempted to infer one useful line. Validation showed predictable failures: shader contour extraction produced embossed topographic soup, connected-region crest extraction produced fat blobs/ribbons, and threshold controls could not compensate for a bad body-first source.

The active source of truth is now a generated 3D surface stroke. Each stroke is a short ground-following local-space curve sampled against `GroundHeightFieldSnapshot`; its points and normals are real 3D surface data. The fold-field texture remains available, but it is now derived from those 3D strokes: `R` is baked line coverage, `G` is the optional body/support around that line, `B` is stroke-relative side polarity, and `A` remains semantic support/reserved. Runtime shader work must consume this baked data; it must not rediscover regions or contours from noise.

The old height-field preview has been removed from the active workflow and replaced by a 3D line/ribbon preview. The preview builds actual temporary mesh ribbons from the generated stroke points so the next validation answers the important question first: do the generated 3D surface lines themselves look promising enough to become the effect?

# Ground Visual Design and Architecture


### 2026-07-13 — GeneratedGround Stage Ownership and Invalidation

**Status:** G2 Unity-validated. Its stage ownership remains active under the G3 optimization above.

The active GeneratedGround pipeline is no longer treated as one indivisible regeneration operation. Its owned outputs are:

```text
ground geometry / base surface
mesh assignment
collider assignment
Painted Accent surface descriptors
Painted Accent projected glyphs
Painted Accent R8 coverage
material property state
river-corridor notification issued after changed ground geometry
```

Each output is governed by a stable input signature plus a missing-output check. A downstream stage depends on the upstream output revision rather than forcing every earlier stage to run again.

Active dependency doctrine:

```text
geometry change
  → mesh → collider → descriptors → projection → coverage → material

placement/domain change
  → descriptors → projection → coverage → material

shape-profile change
  → projection → coverage → material

ink/debug/material change
  → material only
```

GeneratedGround remains the only modified runtime owner in this performance pass. River source state is inspected from the ground side solely to distinguish an identical enable-time notification from a real structural change; no river lifecycle or generation implementation is altered. Corridor notification is issued only after changed ground geometry, never after Painted Accent-only updates.

The regeneration diagnostic must state which stages actually executed. Projected-glyph generation also reports coarse substage timings so the final G3 patch is driven by measured profile, topology, domain-validation, or diagnostic cost rather than by speculation.

Visual doctrine is unchanged: four independent glyph families, strict authored length/width contracts, final-profile sanity, regional composition, production R8 coverage, and no explicit graph/network representation.

## Purpose

This document is the canonical generated-ground design document.

It exists so ground direction does not have to be reconstructed from the generic visual-language docs or from an implementation patch plan. It defines the ground's visual philosophy, style target, design pillars, surface-family architecture, shader/data contracts, feature-layer strategy, and active implementation priority.

The short version is:

```text
Restrained stylized terrain:
BOTW/TOTK-like base-material restraint
+ Hades-1-like painted ground accents
+ mostly 3D procedural geometry
+ reusable procedural masks and style layers
+ family/variant tuning.
```

This is now the baseline. Future ground work must either serve this direction or explicitly document why the direction is being changed.

## Authority and Document Boundaries

This document owns the durable generated-ground design baseline.

Use it for questions such as:

- What should generated ground look like?
- How simple or noisy should ground be?
- How should BOTW/TOTK and Hades 1 influence the ground?
- How do ground families and variants relate to the shared style doctrine?
- Which ground layers are foundational and which are niche?
- What should be built before runtime footprints, puddles, grass suppression, or rain?
- How should ground features avoid becoming one-off silos?

Use `Ground_Generation_Surface_Upgrade_Plan.md` for implementation history, patch notes, exact current status, and concrete patch sequencing.

Use `Proof of Concept/01_Visual_Language_and_Rendering.md` for broader project rendering principles, palette, lighting, camera, snow, fog, and general stylized 3D language.

Use `Proof of Concept/06_Proof_of_Concept.md` for the clearing prototype scope and validation goals.

If these documents conflict on generated-ground direction, this document is the ground-specific source of truth and the other document should be patched.

## Current Decision

The ground direction is not:

```text
Tunic block-world terrain
Hades 2 hand-painted production density
realistic ARPG terrain material complexity
high-frequency procedural noise
runtime simulation first
feature-by-feature terrain special cases
```

The ground direction is:

```text
A calm, readable, mostly matte 3D stage floor,
made rich through broad patch composition,
selective painted-looking accents,
contact/edge response,
sparse reusable motifs,
and later runtime/weather/interaction state.
```

The ground should support the scene rather than become the scene's loudest element. It should look intentional from an isometric/action camera while leaving characters, hazards, VFX, rivers, rocks, structures, silhouettes, fog, and lighting room to read.

## Reference Interpretation

References are production grammar, not literal style mandates.

### BOTW/TOTK: base-material restraint

The useful lesson is that base terrain can be visually quiet. Large ground areas can rely on:

- broad color/value regions;
- low-frequency material variation;
- readable terrain forms;
- slope and shoreline relationships;
- vegetation, rocks, cliffs, props, and paths;
- lighting and atmosphere;
- composition and silhouette hierarchy.

The ground material does not need to impersonate every blade of grass, pebble, mud wrinkle, snow grain, or stain. A simple base can work if the scene around it carries enough form and meaning.

### Hades 1: authored-looking accent grammar

The useful lesson is not exact brushwork. The useful lesson is floor grammar:

- broad readable regions;
- strong value grouping;
- sparse decorative marks;
- short dark mound/crease lines;
- cracks, chips, stains, scuffs, trim, and rhythm;
- contact emphasis around walls, props, edges, and boundaries;
- ground detail that supports gameplay readability.

The small dark ground lines visible in Hades 1 are especially relevant. They read as shallow mounds, grass folds, soft contour breaks, mud creases, or hand-painted surface rhythm. They imply form without needing real height deformation.

### Tunic: readability reference only

Tunic remains useful for high-angle readability, clean silhouettes, and compositional economy.

It is not the generated-ground style target. Tunic's basic ground works because the whole world is reduced to chunky toy/block geometry. This project is allowed to use more organic rocks, rivers, generated masses, snow, mud, shorelines, foam, and terrain detail. In that context, Tunic-simple ground can look underdeveloped rather than elegant.

### Hades 2: ambition reference, not production target

Hades 2 is not a feasible generated-ground baseline. Its floor art is extremely authored and dense. It can inspire taste, but it should not define implementation expectations.

### Realistic dark ARPGs: caution reference

Realistic or high-detail ARPG ground implies heavier authored textures, scans, decals, lighting, material response, grime, and asset production. That is not the current project direction.

## Non-Goals

Generated ground must not drift into the following unless a future design decision explicitly replaces this document:

- full hand-painted terrain everywhere;
- Hades 2-level authored floor detail;
- Tunic/voxel/block terrain as the primary visual target;
- realistic scanned terrain materials;
- noisy procedural texture soup;
- constant high-frequency variation to hide weak composition;
- every ground family using unrelated shader branches;
- one-off feature silos that cannot serve multiple families;
- runtime footprints, puddles, rain, wetness, or grass trampling before the static visual language works;
- heavy terrain height noise that threatens combat readability;
- river-specific or family-specific hacks inside unrelated systems.

## Sacred Design Rule

The ground is a shared visual stack tuned by families and variants.

It is not a collection of unrelated special-case effects.

```text
Shared doctrine stack
  calm base
  broad patches
  semantic mask response
  painted accent lines
  contact / edge accents
  sparse motifs
  runtime state later

Family / variant tuning
  decides how a specific surface expresses that stack.
```

A new ground feature is valid only if it can name which layer it belongs to and how it remains reusable or deliberately scoped.

## Style Pillars

### Pillar 1 - Calm Base Surfaces

Base ground is the stage floor.

It should be:

- matte or mostly matte;
- broad;
- low-noise;
- low-to-moderate contrast;
- readable from the game camera;
- controlled by family/variant material values;
- subordinate to characters, hazards, VFX, rivers, rocks, props, silhouettes, and lighting.

The base material should not carry all terrain identity by itself. Earlier wet mud tuning showed the failure mode: broad smoothness/color changes can become plastic or playdough. The solution is not endless global material tweaking; it is a restrained base plus explicit style layers.

### Pillar 2 - Broad Macro Patch Composition

The first visible variation layer should be large and deliberate.

Good macro patches:

- use low-frequency variation;
- create broad value/color islands;
- respect the family palette;
- read from the isometric camera;
- avoid checkerboard noise;
- can be posterized or softened;
- can be biased by exposure, damp/deposit, shore, vegetation, and compaction masks;
- support composition instead of visual mush.

Bad macro patches:

- look like arbitrary Perlin noise;
- create equal activity everywhere;
- fight player/foe silhouettes;
- hide path readability;
- overpower rivers, shorelines, or combat telegraphs.

### Pillar 3 - Painted Accent Lines

Painted accent lines are the first foundational new style layer after this doctrine.

They are short, broken, Hades-1-like dark/value-shifted surface strokes. They should suggest:

- small mounds;
- grass folds;
- mud creases;
- snow wrinkles;
- soft contour breaks;
- surface age;
- hand-authored rhythm.

They are visual only. They do not require terrain deformation.

#### Chosen Implementation Direction - Generated 3D Surface Strokes and Narrow Secondary Ridge

Patch V3J.3R established deterministic short 3D surface strokes as the source of truth. Patch V3J.3C establishes the active representation candidate built from those descriptors:

```text
generate short deterministic 3D surface-stroke descriptors
  -> sample them against GroundHeightFieldSnapshot
  -> build one combined narrow open ridge mesh per chunk
  -> render that static visual-only mesh during gameplay
```

The base ground mesh and collider are never modified. The secondary ridge has finite width and very small height so it can carry real normals, lighting, parallax, and grazing-angle silhouette, but it has no underside, end caps, collider, per-line GameObjects, or per-line renderers.

The stroke descriptor retains:

```text
local 3D points on the ground
sampled render normals
tangent and across-line directions
Stroke Width
BodyWidth for optional texture/debug support only
strength
seed
```

`Stroke Width` is the sole visible width source for secondary geometry. `BodyWidth` must never widen the ridge mesh. The stochastic profile provides cross-section asymmetry and along-stroke variation; no lateral centerline squiggle is introduced.

Generation is permitted only during editor authoring or camp/run loading. During gameplay the mesh is static and only participates in ordinary rendering and chunk culling. Shader projection remains available as a fallback/debug representation, not an equally active direction.

#### Retired Painted Accent Experiments

The following experiments are retained only as history and should not be tuned as active direction:

```text
V3D-V3F.1 curve-distance strokes:
  line first -> inflated 2D relief tube -> side rails
  failed as scratches/capsules/rails

V3I/V3I.1 candidate stamps:
  discrete oval/ridge stamps -> body/line channels
  failed as leaf/brush stamps

V3I.2 continuous body field:
  domain-warped value field -> G body -> rough R/B
  failed as blocky/noisy field placement for this line target

V3J.0 final prototype:
  trusted existing R as if it were already the selected line
  failed as faint/blocky smudging

V3J.1 shader contour extraction:
  sampled neighboring G and drew local contour bands
  failed as embossed topographic soup

V3J.2 peak-region crest extraction:
  thresholded G, labeled regions, inferred internal crest lines
  threshold worked, but the source regions remained bad and selected lines read as fat blobs/ribbons
```

The lesson is now part of the baseline: **generate the 3D line intentionally, then derive any supporting body/texture response from that line.**

#### Fold-Field Texture Contract

The texture contract survives, but its source changes. Patch V3J.3R derives the generated fold-field texture from 3D surface strokes instead of from noise/body-first inference.

```text
R = baked selected stroke-line coverage from generated 3D surface strokes
G = soft body/support around those strokes, for context or later shading
B = stroke-relative signed side encoded 0..1, with 0.5 as neutral
A = semantic support / reserved future validity channel
```

The shader decodes the channels as:

```text
selectedLine = R
bodyContext  = G
signed       = B * 2 - 1
```

The debug views keep their existing names but now mean:

```text
Ground Painted Accent Lines
  shows R: baked line coverage from generated 3D surface strokes

Ground Painted Accent Relief
  shows G: soft support/body around those 3D strokes

Ground Painted Accent Signed Relief
  shows B decoded as stroke-relative side polarity

Ground Painted Accent Final Prototype
  shades the baked R line with B polarity and weak G context
```

Runtime shader policy is strict: the shader samples baked channels and shades them. It does not perform connected-component labeling, contour extraction, body-field thresholding, or line discovery.

#### Narrow Static Secondary Ridge Policy

The active explicit preview controls are:

```text
GeneratedGround inspector:
  Stroke Width
  Stroke Density
  Stroke Length Min / Max
  Facing Direction Degrees
  Angle Jitter Degrees
  Fold Height
  Fold Irregularity
  Fold End Taper
  Build 3D Ridge Preview
  Clear 3D Ridge Preview

GeneratedGround child object:
  __PaintedAccentRidgePreview_Debug
```

`Fold Broadness` is no longer author-facing and does not affect ridge geometry. The ridge uses seven evenly spaced generic `u` samples across the exact visible `Stroke Width`:

```text
height(t, u) =
    FoldHeight
  * StochasticProfile(strokeSeed, t, u)
  * EdgeEnvelope(strokeSeed, u)
  * AlongStrokeVariation(strokeSeed, t)
  * EndEnvelope(strokeSeed, t)

halfWidth(t) =
    StrokeWidth * 0.5
  * lerp(nonZeroTerminalScale, 1, EndEnvelope(strokeSeed, t))
```

Every lateral vertex re-samples ground height and render normal. The two side boundaries and both terminal rings are embedded slightly below the sampled ground. The interior rises along the sampled ground normal. There are no end caps and no lower shell.

The preview is one combined child mesh per chunk. It uses one shared neutral/dark lit material, casts no shadows, receives shadows, has motion vectors disabled, and has no collider. It does not modify the generated ground mesh or collider.

The current proof remains editor/debug generated. Production adoption must move the same build to camp/run loading and keep gameplay free of generation, rebuilding, polling, and texture-field regeneration. Production static meshes may discard CPU data after upload; editor previews must retain it for rebuild/inspection.

#### Chunk-Library and Runtime Policy

The intended game workflow is a library of reusable authored chunks:

```text
editor:
  generate/author chunks
  validate the fold-field result
  save the chunk as reusable runtime content

runtime map builder:
  choose authored chunks from the library
  rotate/place/connect them in new arrangements
  generate run-specific minutiae such as unit placement and doodads
```

The outside world may be rebuilt while the player is in camp, but the fold-field feature is not a per-frame simulation. It is generated at edit time or load/camp rebuild time before gameplay resumes.

Because chunks can be rotated and reused, fold-field sampling must be chunk-local rather than world-locked. The shader samples with object-space X/Z (`positionOS.xz`) so the painted fold field rotates with the chunk when the runtime map builder rotates that chunk.

#### Resolution and Performance Policy

There is no low/background/hero/special resolution tier for this feature.

The policy is:

```text
visible authored gameplay chunk with PaintedAccentLines active:
  generate one 256x256 RGBA32 fold-field texture

hidden/offscreen/background chunk:
  disable PaintedAccentLines entirely
  do not generate a lower-resolution fold-field texture
```

Memory budget:

```text
256x256 RGBA32 = 262,144 bytes = 256 KiB per active chunk

1 chunk    = 256 KiB
10 chunks  = 2.5 MiB
50 chunks  = 12.5 MiB
100 chunks = 25 MiB
200 chunks = 50 MiB
```

This is acceptable for the projected game scale. A visual style demo validates one chunk. A vertical slice may use roughly ten chunks. A beta-like version may use around fifty. A full game might use around one hundred active selected chunks, with two hundred treated as a remote upper bound. If chunks are meant to be fully hidden by walls, relief, fog, or camera framing, they should disable the feature rather than use a reduced texture.

Patch V3I generation is bounded:

```text
fixed 256x256 texture
RGBA32
no mipmaps
CPU texture copy discarded after upload
candidate rasterization instead of broad per-pixel cell searching
hard candidate cap
```

The production cleanup target is to sample the fold field once per visible ground fragment and reuse that result through albedo/smoothness/surface response. Prototype patches may temporarily sample more than once while the data model is being validated.


They should be:

- short;
- broken;
- slightly curved;
- clustered rather than uniform;
- low-to-medium contrast;
- darkened or value-shifted rather than pure black;
- sparse enough that quiet ground remains quiet;
- tuned per family/variant;
- reusable across snow, mud, grass, rocky dirt, shore, and path surfaces.

They must not become:

- uniform hatching;
- full-screen scribble noise;
- equal-density procedural cracks everywhere;
- hard black outlines unrelated to lighting/material;
- a mud-only or grass-only feature silo.

#### Patch V3I Validation and V3I.1 Body-Shape Correction

Patch V3I validated the new data path: the ground is now reading generated local-space fold-field texture data instead of the retired curve-distance stroke fallback when `PaintedAccentLines` is active. The three debug channels changed exactly as expected for the first prototype:

```text
Ground Painted Accent Relief:
  showed the generated G/body field as soft pale fold bodies

Ground Painted Accent Signed Relief:
  showed gradient polarity around those bodies

Ground Painted Accent Lines:
  showed rough edge/crescent candidates derived from the bodies
```

The result proved the architecture but not the final body shape. The V3I generator used one soft elongated ellipse per candidate, so the relief field read as repeated large oval/leaf stamps. That is still the wrong art read for the final target.

Patch V3I.1 corrects the body generator before any line-art polish. The candidate primitive changes from:

```text
one soft ellipse
```

to:

```text
one short curved tapered ridge/fold body
  with variable width
  asymmetric side weight
  deterministic local warp
  optional small side lobe
  lower density and more negative space
```

This keeps the accepted field-first architecture:

```text
fold body first
then signed polarity
then rough contour/line candidate
```

It does not return to line-first curve strokes. It also does not introduce mesh displacement, collision, fake normals, 3D preview tooling, family tuning, or final line extraction. The validation target remains `Ground Painted Accent Relief`: it should read less like large oval stamps and more like short irregular low terrain folds/ridges. Final contour extraction remains Patch V3J.


#### Patch V3I.2A - Candidate-Stamp Generator Retirement

Patch V3I.2A is a cleanup/redirection patch before the next generator implementation. It removes the V3I/V3I.1 candidate-stamp generator internals from the active code path and replaces them with a neutral 256x256 placeholder texture.

The V3I/V3I.1 validation outcome is now locked:

```text
V3I proved:
  GeneratedGround-owned fold-field texture plumbing works.
  Local-space sampling works.
  The R/G/B/A debug contract is active.
  New shape / seed changes update generated data.

V3I failed visually because:
  the generator still created discrete procedural shapes.
  the relief channel read as large oval/leaf stamps.

V3I.1 improved:
  ellipses became smaller curved tapered forms.
  density was reduced.
  bodies were less uniformly oval.

V3I.1 still failed visually because:
  the model remained candidate/stamp based.
  the output read as sparse brush marks, not a natural secondary height layer.
```

Therefore, the following systems are retired as an active direction:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
candidate density/cell spawning
candidate curvature/asymmetry/side-lobe stamp model
ellipse/ridge stamp language as the source of the body field
```

The retained systems are:

```text
GeneratedGround-owned fold texture lifecycle
local/object-space sampling
256x256 RGBA32 active-chunk policy
R/G/B/A texture contract
debug views
shader router
retired curve-distance fallback for inactive/missing generated data
```

The next accepted implementation is continuous field generation:

```text
continuous domain-warped scalar field F(local x, local z)
  -> shaped visual height/body G
  -> gradient polarity B
  -> rough selected contour/edge R
```

Patch V3I.2A intentionally produces a blank/neutral generated fold texture for active `PaintedAccentLines` chunks. This prevents further visual tuning of the rejected stamp generator while keeping compile/runtime plumbing intact for Patch V3I.2. It is expected that Painted Accent Relief / Signed Relief / Lines debug views will show no generated fold marks during this transition.

Patch V3I.2 will replace the neutral placeholder with:

```text
GenerateBaseNoiseField(...)
GenerateDomainWarp(...)
ShapeContinuousBodyField(...)
ApplySemanticSupport(...)
SmoothBodyField(...)
BuildPixelsFromContinuousField(...)
```

No candidate spawning, no stamp rasterization, no mesh displacement, no 3D preview tooling, no final line extraction, and no family tuning are part of V3I.2A.


#### Patch V3I.2 - Continuous Domain-Warped Fold Height Field

Patch V3I.2 replaces the neutral V3I.2A placeholder with the first continuous scalar-field implementation. This is the first generator that matches the accepted field-first direction instead of the rejected candidate/stamp direction.

The active model is now:

```text
continuous domain-warped scalar field F(local x, local z)
  -> shaped visual height/body G
  -> gradient polarity B
  -> rough selected contour/edge R
```

The generator no longer uses:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
discrete mark spawning
ellipse stamps
curved ridge stamps
```

The V3I.2 field generation pipeline is:

```text
local chunk coordinate
  -> deterministic domain warp
  -> broad fractal value field
  -> medium fractal value field
  -> ridge-like fractal component
  -> directional continuity component
  -> semantic support from the existing generated ground masks
  -> percentile-based coverage threshold
  -> soft body shaping
  -> light smoothing
  -> R/G/B/A texture write
```

The coverage normalization is important. The generator does not use one hard global threshold; it resolves a percentile threshold from the generated field so the active body coverage remains bounded by feature strength:

```text
low strength  -> lower active coverage
high strength -> higher active coverage
```

This is intended to prevent both failure extremes:

```text
full-screen cloudy mush
isolated procedural stamps
```

The texture contract remains unchanged:

```text
R = rough contour/edge candidate from G
G = continuous visual fold-height/body field
B = signed side from the gradient of G, encoded 0..1
A = semantic support / reserved
```

The primary validation target remains `Ground Painted Accent Relief`, which displays the G/body field projected on the current ground surface. `Ground Painted Accent Lines` remains a rough derivation and is not final line extraction. V3J remains the line extraction polish patch.

This implementation is still visual-only:

```text
no mesh displacement
no collision change
no fake normal
no production terrain deformation
```

A true debug height preview is explicitly planned as the next diagnostic tooling layer:

```text
Patch V3I.3 - Fold Field Height Preview Debug Mesh
```

The chosen preview approach is Option B: generate an editor/debug-only preview mesh from the fold-field texture and displace that preview by the G channel. This will show the field honestly at preview resolution instead of relying on the existing ground mesh density. It must remain debug-only and must not imply gameplay displacement.



#### Patch V3I.3 - Fold Field Height Preview Debug Mesh

Patch V3I.3 implements the planned Option B diagnostic preview. The existing `Ground Painted Accent Relief`, `Ground Painted Accent Signed Relief`, and `Ground Painted Accent Lines` views remain projected texture-channel debug modes. They are useful, but they do not show the field as actual relief.

The new preview is editor/debug-only:

```text
Generated fold-field G channel
  -> temporary child preview mesh
  -> vertex height = sampled ground height + G * debug height scale + small lift
```

The preview mesh is intentionally separate from the production ground mesh:

```text
does not modify the generated ground mesh
does not modify collision
does not change gameplay terrain
does not imply production displacement
does not require new layers or tags
```

The preview mesh is created as a child object named:

```text
__FoldFieldHeightPreview_Debug
```

The `GeneratedGround` inspector exposes:

```text
Build Height Preview
Clear Height Preview
```

This preview uses the same generated G/body values that are written into the fold-field texture, not a GPU readback or a second approximation. `GroundPaintedAccentFoldFieldGenerator.Generate(...)` now returns the generated body array alongside the uploaded texture so the debug mesh can be built from the exact same scalar field.

This patch is diagnostic only. It does not tune the continuous field generator and does not perform final contour extraction. The next decision should be based on inspecting the projected G channel and the height preview mesh together.


#### Patch V3I.3A - Debug Isolation and Preview Color Readability

Patch V3I.3 validation exposed two pipeline issues:

```text
1. The generated fold field could still influence the normal final ground render.
2. The preview mesh had real displacement, but its material did not visualize height values from top view.
```

Patch V3I.3A fixes those issues before any generator tuning. The generated fold field remains diagnostic-only until final response is deliberately rebuilt in V3J/V3K.

Final render isolation rule:

```text
Generated fold-field data may feed:
  Ground Painted Accent Relief
  Ground Painted Accent Signed Relief
  Ground Painted Accent Lines
  Ground Painted Accent Final Prototype
  Fold Field Height Preview mesh

Generated fold-field data must not feed:
  normal final albedo
  normal final smoothness
  normal final specular
  production material response
```

The final forward pass now zeros Painted Accent final-render contribution while `_GroundPaintedAccentFoldFieldEnabled` is active. This preserves the debug data path while preventing the experimental G field from appearing as noise in the normal game render.

The height preview mesh now uses an explicit hidden debug shader:

```text
Hidden/PS3D/Ground Fold Field Height Preview
```

The shader reads the preview mesh vertex color/body value and maps it to a visible low/mid/high debug gradient. This makes the height field readable from top view as well as from profile. The preview renderer also disables shadow casting and shadow receiving so the preview does not contaminate lighting diagnostics.

Preview cleanup was also made more robust: clearing the preview removes all child objects whose names begin with `__FoldFieldHeightPreview_Debug`, rather than relying on only one exact child lookup.

V3I.3A is a correctness patch only:

```text
no generator tuning
no final line extraction
no production displacement
no collision changes
```


#### Patch V3J.0 - Painted Accent Final Visual-Response Proof

Patch V3J.0 adds one debug-only view:

```text
Ground Painted Accent Final Prototype
```

This view exists to answer a specific architecture question before generator tuning continues: can the generated fold-field contract be turned into the intended painted fold/crease visual language? It is not a production render path and does not remove the V3I.3A final-render isolation rule.

The prototype response treats the generated channels as follows:

```text
R = selected contour / narrow visible crease source
G = soft fold body / context gate only
B = signed side / crease-highlight polarity
A = support / still reserved for field semantics
```

The important rule is that `G` must not directly become broad albedo darkening. The previous V3I.3 validation proved that broad body modulation reads as noisy stains. In V3J.0, `G` only gates where the narrow `R` contour is allowed to become visible; the prototype color is driven by a crease mask, a smaller side highlight mask, and a very low context term.

This patch intentionally does not tune the field generator. A bad field can still make bad shapes. The proof target is narrower: if the current ugly field can still produce crease-like marks when only the contour/signed channels are emphasized, the architecture is viable and the next patch should improve field shape/placement. If the prototype still reads as stains despite the restricted response, the response model or channel contract needs revision before generator tuning.

V3J.0 is limited to:

```text
debug-mode enum plumbing
shader-only prototype visualization
documentation of the proof contract
```

It does not add mesh displacement, collision changes, production normal perturbation, family tuning, generator tuning, new components, decals, or runtime state.

V3J.1 correction also failed the actual target. It moved extraction into the shader, but the shader produced embossed contour soup: many local G level sets, not one selected line per meaningful fold. V3J.2 reconciles the architecture: region selection belongs to generation/dirty time, not the runtime fragment shader. The generator now thresholds G, labels connected peak regions, rejects small junk regions, extracts one representative internal crest line per accepted region, and writes that selected line to R. The Final Prototype shader consumes R directly and only uses G as weak context plus B for one-sided dark/light polarity.

#### Patch V3J.2 - Precomputed Peak-Region Crest Lines

Patch V3J.2 is the reconciliation patch after the failed V3J.0/V3J.1 shader-only proof attempts. The accepted division of responsibility is now:

```text
generation / dirty time:
  generate continuous G/body field
  apply a temporary peak threshold
  identify connected peak regions
  discard tiny regions
  select one internal crest/accent line per accepted region
  write that selected line to R

runtime shader:
  sample R/G/B/A
  shade the already-selected R line
  do not perform connected-component or contour-band extraction
```

Temporary authoring controls live on `GroundSurfaceFeatureRecipe` for `PaintedAccentLines` only:

```text
Painted Accent Peak Threshold
Painted Accent Minimum Peak Area
Painted Accent Crest Width Texels
```

These controls are proof/tuning controls, not final family art direction. The important contract is that `R` is now line-selection data, not a broad activity field and not a shader-derived contour approximation.


### Pillar 4 - Contact and Edge Accents

Ground should visually respond around meaningful geometry and semantic boundaries. This is one of the main ways an isometric scene looks authored instead of assembled.

Contact/edge accent candidates:

- rock bases;
- standing stones;
- cliffs and banks;
- river shorelines;
- bridge or crossing contact;
- path boundaries;
- modifier boundaries;
- raised/lowered terrain edges;
- structure foundations;
- camp pads and authored clearings;
- damp deposits near water;
- snow buildup near wind-protected edges.

This layer may add local darkening, dampness, deposit hints, outline-like value shifts, accent-line density changes, or surface-wear emphasis. It must not turn every object into a heavy decal blob.

### Pillar 5 - Sparse Motifs and Stamps

After accent lines and contact accents, the next detail tier is sparse motif/stamp content.

Examples:

- chips;
- cracks;
- small stones;
- dirt strokes;
- dry scuffs;
- mud stains;
- snow scrape marks;
- tiny tuft-like marks;
- leaf/debris hints;
- frost specks;
- ash specks;
- broken trim marks.

Rules:

- sparse beats dense;
- clusters beat uniform distribution;
- motifs should respond to semantic masks;
- motifs should not tile obviously;
- motifs should not be required for the base ground to look acceptable;
- each family/variant should be able to reduce or disable motif density.

### Pillar 6 - Runtime State Later

Runtime surface state remains valuable, but it is not the foundation right now.

Deferred runtime state includes:

- rain wetness;
- drying;
- snow depth;
- snow compression;
- footprints;
- grass trampling;
- mud disturbance;
- puddle growth;
- standing-water evolution;
- disturbance age.

These systems should wait until the static visual language works. Runtime state is expensive in complexity even if the texture memory is manageable. It should not be used to compensate for unresolved art direction.

### Pillar 7 - Geometry Still Matters

The ground is not only a shader plane.

The scene should also be carried by:

- terrain silhouette;
- banks and slopes;
- rivers and shore corridors;
- generated rocks/masses;
- structures and ruins;
- grass/vegetation later;
- snow banks later;
- fog and lighting;
- prop placement;
- manual composition.

A calm base material works only if these scene layers participate.

## Ground Composition Stack

The intended render/meaning stack is:

```text
Playable terrain shape
  ↓
Calm family base material
  ↓
Broad macro patch composition
  ↓
Static semantic mask response
  exposure / damp / deposit / shore / vegetation / compaction / rocky-dry
  ↓
Painted accent lines
  ↓
Contact / edge accents
  ↓
Sparse motifs and stamps
  ↓
Runtime surface state later
  wetness / snow depth / compression / footprints / mud / puddles
  ↓
Debug override
```

The ordering matters:

- the base must remain readable without detail layers;
- macro patches define the broad composition;
- semantic masks make the ground respond to generated meaning;
- accent lines add the Hades-1-like authored rhythm;
- contact accents glue objects and terrain together;
- sparse motifs add identity without noise;
- runtime state modifies the surface only after the static language is proven.

## Family and Variant Architecture

The existing family/variant architecture remains correct. It now has clearer meaning.

```text
GroundSurfaceProfile
  semantic / mask-generation profile

GroundSurfaceStyleProfile
  visual surface family

GroundSurfaceVariantRecipe
  family-local recipe

GroundMaterialControls
  calm base-material response

GroundSurfaceFeatureRecipe
  reusable style-layer tuning

GeneratedGround
  resolver, top-level authoring surface, per-object override owner
```

The family decides what kind of surface this is.

The doctrine decides how all surfaces speak visually.

The variant tunes how much of each shared style layer appears.

### GroundSurfaceProfile

`GroundSurfaceProfile` owns semantic/mask-generation intent. It should describe what the generated surface means and what static masks are produced, not the entire visual material response.

Examples:

- snowfield semantic profile;
- wet mudflat semantic profile;
- grassland semantic profile;
- future rocky scrub profile;
- future ash or frost profile.

### GroundSurfaceStyleProfile

`GroundSurfaceStyleProfile` owns a visual family.

Examples:

- Snowfield;
- Wet Mudflat;
- Grassland;
- future Rocky Scrub;
- future Ash/Frost/Corruption surface.

A family should not require bespoke shader logic for basic ground language. It should tune the shared doctrine stack.

### GroundSurfaceVariantRecipe

`GroundSurfaceVariantRecipe` owns a family-local recipe.

Examples:

```text
Snowfield.clean
Snowfield.patchy
Snowfield.dirty_thawing
Snowfield.wind_scoured

WetMudflat.damp_mud
WetMudflat.waterlogged
WetMudflat.trampled
WetMudflat.frozen_thaw

Grassland.clean_meadow
Grassland.patchy_meadow
Grassland.damp_meadow
Grassland.worn_meadow
```

Variants should tune:

- base color/value;
- snow/mud/damp/shore response;
- macro patch scale/contrast;
- painted accent-line density/contrast;
- contact accent strength;
- sparse motif density;
- specific mask response such as compaction or wetness.

Variants should not create unrelated one-off rendering pipelines.

### GroundMaterialControls

`GroundMaterialControls` should remain the calm base material and broad response recipe.

It is appropriate for:

- base color;
- secondary color;
- brightness/value bias;
- tint strength;
- damp darkening;
- snow tinting;
- smoothness/specular baseline;
- patch scale/contrast;
- broad material response.

It is not the right place to fake every terrain detail. If visual richness requires small strokes, contact accents, motif stamps, or runtime state, those should be explicit layers.

### GroundSurfaceFeatureRecipe

`GroundSurfaceFeatureRecipe` should evolve from one-off features into reusable style-layer tuning.

Good feature kinds are doctrine layers or reusable semantic responses:

- PaintedAccentLines;
- ContactEdgeAccents;
- SparseMotifs;
- DirectionalSurfaceMarks;
- CompactionResponse;
- PooledWetnessResponse;
- ShoreDepositResponse.

Bad feature kinds are overly narrow unless deliberately scoped:

- one exact mud decal;
- one exact snow footprint look;
- one exact puddle shape hardcoded into a family;
- a feature that only works because a single current asset happens to need it.

## Current Feature Interpretation

Existing feature work should be reclassified under the doctrine rather than discarded.

### DirectionalStreaks

Keep, but reinterpret as an early directional surface-mark proof.

It may eventually fold into Painted Accent Lines or Directional Surface Marks.

### PooledWetness

Keep as a wetness response proof, but do not treat it as final puddles.

Real puddles/standing water are later explicit features or runtime state. Wet mud base should remain mostly matte.

### TrampledWear

Keep as a compaction/path response proof.

Patch U proved this flow:

```text
GroundModifier authored surface mask
→ generated metadata
→ UV2.x compaction/path
→ shader feature response
```

After the doctrine pivot, `TrampledWear` is no longer the active cornerstone. It should be used as one stackable compaction response layer, not polished as a bespoke terrain direction.

### PaintedAccentLines

This is the first real doctrine-layer feature.

It creates short, broken, slightly curved, dark/value-shifted surface strokes that suggest grass folds, mud creases, snow wrinkles, small mounds, and surface age. It is visual only: no decals, textures, height deformation, mesh edits, or runtime state.

It must remain sparse and authored-looking. Failure modes are global hatching, scratch noise, grass-blade hair, mud-crack networks, or black outline marks.

## Feature Stack Direction

Patch V3 makes the variant feature list the canonical composition model.

```text
GroundSurfaceVariantRecipe.features
  -> first enabled recipe of each supported ShaderOnly kind wins
  -> GeneratedGround writes explicit shader-property blocks per kind
  -> shader applies all supported layers in a stable renderer-defined order
```

This replaces the earlier proof-feature shortcut where `_GroundFeatureMode` selected one mutually exclusive feature. `_GroundFeatureMode` may remain as a hidden serialized compatibility property, but it must not be extended as the long-term feature architecture.

A variant may now combine shader-only feature recipes, for example:

```text
grassland.damp_meadow
  PaintedAccentLines
  PooledWetness

grassland.worn_meadow
  PaintedAccentLines
  TrampledWear

snowfield.wind_scoured
  DirectionalStreaks
  PaintedAccentLines
```

Composition rules:

- feature recipes are a list, not a dropdown choice;
- features are not mutually exclusive by default;
- first enabled recipe of a given kind wins;
- duplicate enabled recipes of the same kind should be treated as authoring mistakes;
- unsupported or non-ShaderOnly recipes may remain in the asset contract but do not render until implemented;
- shader composition order is stable and renderer-defined, not asset-list-order-defined;
- style authors are responsible for choosing coherent combinations.

Current supported shader stack layers:

```text
DirectionalStreaks
PooledWetness
TrampledWear
PaintedAccentLines
```

Future layers such as ContactAccents and SparseMotifs should follow the same stack model instead of adding one-off bespoke plumbing.

## Static Surface Data Contract

Generated ground uses vertex colors and UV2 as static semantic masks.

Current intended contract:

```text
Vertex Color R = tonal patch variation
Vertex Color G = exposure / snow-hold potential
Vertex Color B = damp / deposit potential
Vertex Color A = vegetation suitability

UV2.x = compaction / path / flatten influence
UV2.y = river / shore influence
UV2.z = rocky / dry secondary patch
UV2.w = authored standing-water / puddle potential
```

Design rules:

- these are semantic masks, not final visual effects;
- shader/style layers interpret them;
- debug modes must remain available and trustworthy;
- do not repurpose channels silently;
- if a channel meaning changes, update this document and the implementation plan together.

## GroundModifier Contract

Ground modifiers can now affect height, authored surface masks, or both.

The accepted design is:

```text
Mode = None + Surface Effect Mode = Custom
  → surface masks only, no height change

Mode = Flatten/Lower/Raise + Surface Effect Mode = Custom
  → height change + authored surface masks

Mode = Flatten + Surface Effect Mode = AutoFromHeight
  → legacy flatten writes compaction

Surface Effect Mode = None
  → height-only modifier, no authored surface meaning
```

Design rules:

- prefer surface-only masks when visual response is enough;
- allow small height denivelations for roads, wagon tracks, camp pads, puddle basins, and intentional terrain shaping;
- keep height changes combat-safe and camera-stable;
- do not bake final snow paths or grass paths into base ground; future snow/grass systems should interpret masks/runtime state.

## River Corridor Contract

River corridors must remain style-agnostic.

Correct relationship:

```text
GeneratedGround resolves family/variant/material/feature state
→ applies MaterialPropertyBlock to ground renderer
→ refreshes dependent river corridor renderer
```

Wrong relationship:

```text
StylizedRiver knows Snowfield, WetMudflat, TrampledWear, PaintedAccentLines, etc.
```

The river may consume ground snapshots, shore masks, and material-property refreshes. It must not own ground style-family logic.

## Shader and Material Contract

Generated ground uses a dedicated ground shader path, separate from generated masses.

Important rules:

- keep ground-specific contracts explicit;
- use `MaterialPropertyBlock` for object/variant-resolved values;
- avoid duplicating materials for every variant;
- keep debug modes stable;
- do not shift existing debug enum values casually;
- avoid turning the shader into a monolith of unrelated family branches;
- aggregate shared style-layer controls rather than hardcoding family-specific features.

The shader stack should eventually expose or receive controls for:

- base material response;
- macro patch scale/contrast;
- semantic mask responses;
- painted accent lines;
- contact/edge accents;
- sparse motif density;
- wetness/compaction/shore responses;
- runtime state later.

## Cost Classes

Ground features should declare or imply cost class.

Preferred progression:

```text
ShaderOnly
  first pass for broad visual layers

MeshMaskDriven
  when generated static masks are needed

GeneratedTexture
  only when the visual cannot be achieved cleanly from mesh masks/world-space shader logic

RuntimeState
  only after static style language is accepted
```

Do not make every style pay for every feature.

## Active Roadmap

The old runtime-first roadmap is paused.

Patch T and Patch U remain useful, but they are not the active direction:

- Patch T established the authored surface-mask contract.
- Patch U proved a shader feature can consume `UV2.x` compaction/path.

The active direction is now style calibration and shared doctrine layers.

| Priority | Patch | Goal |
| --- | --- | --- |
| 1 | V0 — Ground Visual Doctrine Documentation | Completed by documentation. Establish the canonical doctrine and this design doc. |
| 2 | V1 — Style Calibration Setup | Completed as a temporary `Style Calibration` surface family with four comparison variants. |
| 3 | V2 — Base Ground Simplification | Implemented as an asset/docs retune: Snowfield and Wet Mudflat now target calmer matte, lower-noise base surfaces. |
| 4 | V2B — Grassland Baseline Family | Implemented as a real production `Grassland` surface family so shared style layers can be validated across snow, mud, and living ground. |
| 5 | V3 — Shader Feature Stack + Painted Accent Lines | Implemented as the first stackable doctrine layer and as the migration away from the old single `_GroundFeatureMode` proof-feature slot; V3D refines the raw accent-line mask from large strips into smaller clustered micro-strokes, and V3E upgrades those strokes into curved visual-relief terrain folds. |
| 6 | V4 — Contact / Edge Accent Layer | Add localized response near shores, rocks, modifier boundaries, paths, banks, and object contact zones. |
| 7 | V5 — Sparse Motif Layer | Add reusable sparse chips, cracks, scuffs, stains, snow scratches, stones, and debris hints. |
| 8 | V6 — Feature Stack Authoring Polish | Add richer editor warnings, cost summaries, and feature-combination guidance after more stack layers exist. |
| 9 | Later | Runtime Surface State Stub | Revisit wetness, snow depth, compression, footprints, and disturbance after static style acceptance. |
| 10 | Later | Footprints / Rain / Puddles / Grass Integration | Build on runtime state only after the visual doctrine is proven. |
| 11 | Future | Mixed Terrain / Profile Blending | Blend surface families such as snow over mud, rocky scrub over soil, or worn path through snow. |

## Style Calibration Requirements

Patch V1 should not add final features. It should create a comparison setup.

The same clearing should be tested under several lanes:

```text
Calm BOTW-like base
  simple, matte, low-noise, broad color/value regions

Hades-accent lane
  same base plus stronger painted accent lines and contact marks

Hybrid target lane
  restrained base plus selective accent lines/contact response/sparse motifs

Pixel/faceted lane
  current PS3D material-space pixel identity pushed harder
```

The goal is to decide the visible lane before deeper implementation.

The likely target is the hybrid lane.

### Patch V1 Implementation

Patch V1 implements the calibration setup as assets, not as new shader code or runtime systems.

Canonical calibration assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_StyleCalibration.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_StyleCalibration.asset
```

`GSP_StyleCalibration` is a neutral semantic/mask-generation profile. Its purpose is to keep the generated static masks steady while the visible style variants change. It is not a production biome or final terrain identity.

`GSSP_StyleCalibration` is a temporary development surface family discovered by the existing `GeneratedGround` family dropdown. It contains four comparison variants:

| Variant id | Display name | Purpose |
| --- | --- | --- |
| `calibration.calm_base` | Calm Base | BOTW/TOTK-like restraint test: matte, quiet, broad, low-noise ground. |
| `calibration.hades_accent_proxy` | Hades Accent Proxy | Current-tool approximation of stronger Hades-1-like ground mark rhythm. Uses `DirectionalStreaks` as a proxy, not as the final accent-line implementation. |
| `calibration.hybrid_target_proxy` | Hybrid Target Proxy | Expected target lane: calm base plus restrained accent rhythm. |
| `calibration.pixel_faceted` | Pixel-Faceted | Stress test for the existing PS3D material-space pixel/faceted identity. |

The Hades and Hybrid variants originally used `DirectionalStreaks` as a calibration stand-in. Patch V3 replaces that proxy with real stackable `PaintedAccentLines` recipes while keeping DirectionalStreaks available as a separate surface-mark layer.

Patch V1 does not add:

- `PaintedAccentLines` shader logic;
- contact/edge accent logic;
- sparse motifs;
- shader feature stack migration;
- runtime state;
- footprints, puddles, rain, grass suppression, roads, or wagon tracks;
- scene edits or new components.

The purpose is screenshot comparison from the same clearing and camera, so the next implementation patch is guided by evidence instead of taste drift.


### Patch V2 Implementation

Patch V2 records the first calibration conclusion and retunes production families accordingly.

Calibration conclusion:

```text
Use Calm Base as the foundation.
Keep Hybrid as the target philosophy.
Do not use Pixel-Faceted as the default ground read.
Do not mistake DirectionalStreaks for real Hades-1-like painted accent lines.
```

Implemented adjustments:

- `Pixel-Faceted` is now a flat display label. The stable id remains `calibration.pixel_faceted`.
- Snowfield variants reduce fine pixel noise, patch contrast, warp, and over-strong directional streaks.
- Wet Mudflat variants reduce fine pixel noise, damp darkening, patch contrast, and feature intensity while staying matte.
- `TrampledWear` remains a useful compaction-response proof, but it is not treated as the primary ground-style foundation.

Patch V2 is still not final ground art. It is a base-floor cleanup so later doctrine layers have room to work.

Patch V2 does not add painted accent lines, contact accents, sparse motifs, runtime state, new shader properties, scene edits, river logic, or shader feature stack migration.

### Patch V2B Implementation

Patch V2B adds the missing third production baseline family: `Grassland`.

This is not a vegetation-rendering patch. It does not add grass blades, foliage placement, grass physics, wind animation, density maps, or grass suppression. It adds a calm living-ground surface family so future shared doctrine layers can be tested against three different material/value regimes instead of only snow and mud.

Canonical three-family test set after V2B:

```text
Snowfield
  pale, cold, soft, low-value surface

Wet Mudflat
  dark, earthy, damp, matte surface

Grassland
  muted green/olive, living-ground, medium-value surface
```

Shared ground features must prove themselves across this set before they are treated as part of the baseline visual language. A feature that only works on snow or only works on mud should remain a family-specific response, not a doctrine pillar.

Implemented assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_Grassland.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Grassland.asset
```

`GSP_Grassland` is a semantic/mask-generation profile with high vegetation suitability, moderate damp/deposit response, moderate footprint visibility, low snow eligibility, and soft broad patches.

`GSSP_Grassland` is a production visual family with four baseline variants:

| Variant id | Display name | Purpose |
| --- | --- | --- |
| `grassland.clean_meadow` | Clean Meadow | Calm matte meadow baseline. Muted olive-green, low noise, broad soft variation. |
| `grassland.patchy_meadow` | Patchy Meadow | Slightly more exposed-earth/olive patching while staying restrained. |
| `grassland.damp_meadow` | Damp Meadow | Cooler, darker, river-adjacent living ground. Uses a very subtle `PooledWetness` proof response only; not real puddles. |
| `grassland.worn_meadow` | Worn Meadow | Browner, compressed/path-capable meadow ground. Uses a restrained `TrampledWear` proof response so compaction masks can be tested on grassland. |

Patch V2B deliberately keeps `Style Calibration` as a development-only comparison family. It does not convert calibration into production grassland. Grassland is a real family; Style Calibration remains a temporary visual lane tester.

Patch V2B does not add painted accent lines, contact accents, sparse motifs, shader feature stack migration, runtime state, vegetation rendering, scene edits, river logic, new shader properties, or new components.

### Patch V3 Implementation

Patch V3 implements `Shader Feature Stack + Painted Accent Lines`. Patch V3A fixes shader include isolation after the first V3 compile issue. Patch V3B moves ground debug selection onto the `GeneratedGround` component so validation no longer requires opening shared material assets. Patch V3C cleans up that object-level debug UX by removing slash characters from debug labels and removing the obsolete Unity 6.5 editor-refresh overload. Patch V3D refines the raw Painted Accent Lines mask after validation showed the first line generator produced large isolated strips rather than small broken ground creases. Patch V3E then replaces the remaining straight/bar-like primitive with curved visual-relief terrain-fold strokes. Patch V3F exposes the three painted-accent channels separately and strengthens the final side-dependent value relief. Patch V3F.1 makes the relief body more continuous and the signed-side channel readable, but validation shows the model still reads as curve tubes and side rails. Patch V3G retires the curve-distance stroke model as the chosen direction and redirects the feature toward generated visual fold-field data.

Technical contract:

```text
GroundSurfaceVariantRecipe.features
  list of feature recipes

GeneratedGround
  resolves first enabled ShaderOnly recipe of each supported kind
  writes explicit MaterialPropertyBlock properties per feature kind

Shader
  evaluates DirectionalStreaks, PooledWetness, TrampledWear, and PaintedAccentLines as stackable layers
```

The old generic `_GroundFeatureMode` slot is now a deprecated compatibility property. It must not receive new modes.

`PaintedAccentLines` is the first Hades-1-like doctrine layer, but its V3D-V3F.1 curve-distance implementation is retired as the final visual model. That implementation remains in code temporarily as a fallback/comparison path only. It must not be tuned, extended, or used as the basis for family polish.

Retired V3D-V3F.1 model:

```text
world-space procedural curve strokes
  -> distance-to-curve line mask
  -> inflated distance-to-curve relief body
  -> side bands derived from curve side
```

Reason for retirement:

```text
It produces scratches, fat tubes, and rail-like signed-side bands. The target requires an underlying visual fold/height field whose selected edges produce accent lines.
```

Accepted V3G direction:

```text
generated visual fold field F(x,z)
  -> relief/body channel from F
  -> precomputed selected crest line from thresholded peak regions
  -> signed side from fold-field gradient/polarity
```

The useful parts of V3 are retained: `PaintedAccentLines` feature kind, feature-stack authoring, material-property plumbing, object-level `GeneratedGround` debug selection, and the three-channel debug contract. The implementation source changes from direct shader curve strokes to generated fold-field data. The layer remains visual-only: no mesh displacement, no collision change, no terrain height edit, no runtime footprints/wetness state, and no decal system. Generated/cached texture data is allowed for the fold field if it is produced at ground regeneration/dirty time and sampled cheaply at runtime.

Canonical validation set after V3:

```text
Snowfield
Wet Mudflat
Grassland
```

Each shared feature must be judged across all three before it is accepted as part of the baseline ground language.

Patch V3G fold-field validation rule for Painted Accent Lines:

```text
Generated field debug first, extracted line second, final color last.
```

`Ground Painted Accent Lines` debug should show the selected crest/accent strokes extracted from thresholded peak regions of the fold field. `Ground Painted Accent Relief` should show the underlying visual fold-height/body field, not a widened line tube. `Ground Painted Accent Signed Relief` should show gradient/polarity information that can drive shadow/highlight side selection, not decorative parallel rails. The selected line should not show straight bars, giant crescent strips, continuous worms, full-screen hatching, dense hair-like noise, crack networks, many contour rings around one bump, or full closed outlines around every bump. Normal rendering should eventually read as subtle visual mound/crease relief through painted shadow/highlight or, if later accepted, a tiny shader-only normal cue. Final family tuning must wait until the fold field, selected line, and signed side read are all directionally correct.


### 2026-07-09 — Patch V3G: Painted Accent Direction Reset / Fold-Field Plan

Patch V3G retires the V3D-V3F.1 curve-distance stroke model as the final Painted Accent Lines direction. The previous path proved the feature stack, object-level debug workflow, shader property plumbing, and the three-channel diagnostic contract, but validation showed that the source representation is wrong: it starts from a curve, inflates the curve into a tube-like body, and derives rail-like side bands.

The chosen direction is now a generated visual fold field. The ground generator will eventually create persistent/cached visual fold data at regeneration or dirty time. The shader will sample that generated data and use it to render accent lines and visual relief. The expected channel meaning is:

```text
line contour
  selected contour/ridge/edge strokes extracted from the fold field

relief body
  underlying visual fold-height/body influence

signed relief side
  gradient/polarity field for painted shadow/highlight side selection
```

The old shader curve-stroke code remains temporarily as a runtime fallback/comparison path until the fold-field replacement is implemented. It is explicitly retired and must not be tuned as the final solution.

### 2026-07-09 — Patch V3E: Painted Accent Lines Curved Relief Model

V3E redefines the active Painted Accent Lines primitive as visual terrain-fold strokes, not 2D line stamps. The shader now builds each stroke from several local control points to produce irregular curved marks, then derives both the line mask and a soft signed relief body from that same curve. The relief is used only for subtle painted value shaping: it is not mesh displacement, collision, terrain height, decals, textures, runtime state, or generated atlas data.

### 2026-07-09 — Patch V3F: Painted Accent Relief Debug + Visual Relief Strengthening

V3F separates the Painted Accent Lines visual model into three debuggable channels:

```text
line contour
  thin dark/painted crease

relief body
  broader soft fold area around the contour

signed relief side
  side-dependent field used for painted shadow/highlight
```

The object-level ground debug dropdown now exposes `Ground Painted Accent Relief` and `Ground Painted Accent Signed Relief` in addition to `Ground Painted Accent Lines`. Normal rendering uses the narrow contour for crease/tint response and the signed side field for stronger value-side shadow/highlight. This remains visual-only: no mesh deformation, collision change, decals, textures, generated atlases, runtime state, new mesh channels, or new components are introduced.

## Acceptance Criteria

Ground work is successful when:

- a quiet ground area still looks intentional, not empty;
- broad patches read from the game camera;
- accent lines feel authored, not procedurally sprayed;
- contact accents make rocks/rivers/paths feel integrated;
- variants feel related by one visual language;
- snow, mud, and grassland differ through tuning, not unrelated pipelines;
- the ground does not fight characters, VFX, hazards, or UI;
- debug masks correspond to visible responses;
- feature cost remains opt-in and understandable;
- screenshots make the style direction easier to choose, not harder.

Ground work is failing when:

- visual richness comes mostly from noise;
- every surface has equal detail density;
- the shader becomes a list of hardcoded family branches;
- feature work cannot explain which doctrine layer it improves;
- a quiet area looks unfinished;
- a detailed area looks like texture soup;
- runtime simulation is used to hide an unresolved static style.

## Authoring Workflow Target

The authoring flow should remain object/profile driven:

```text
Select GeneratedGround
→ choose Surface Family
→ choose Surface Variant
→ optionally override material/style controls
→ place GroundModifiers for semantic/height intent
→ regenerate
→ inspect debug masks
→ inspect final ground
```

Style-profile authoring should remain asset-backed:

```text
Open GroundSurfaceStyleProfile
→ inspect variants
→ tune material controls
→ tune feature-layer recipes
→ apply to open generated grounds
```

The UI should expose design concepts, not low-level noise clutter.

## Debug and Validation Rules

Ground debugging should remain compact, trustworthy, and object-owned.

Normal validation path:

```text
Select GeneratedGround
→ Ground Debug
→ Debug View
→ choose the needed ground debug view
```

GeneratedGround writes the selected debug mode through its renderer-local `MaterialPropertyBlock` using `_MaskDebugMode`. Authors should not need to open or edit shared material assets to validate generated-ground masks or doctrine-layer debug views. Material asset debug controls are fallback/internal only.

Ground debug changes are visual/material-property-block changes only. They must not regenerate terrain, change mesh data, instantiate materials, or require a style/profile asset edit. River corridor renderers may receive the same parent-ground debug view through the existing ground material-property refresh path, but river code must remain style-agnostic.

Debug view labels must avoid slash characters because Unity enum dropdowns treat slashes as submenu separators. Use flat labels such as `Ground Compaction Path`, `Ground Damp Deposit`, and `Ground Rocky Dry`.

Required mask/debug concepts:

- tonal patch;
- exposure/snow-hold;
- damp deposit;
- vegetation suitability;
- compaction path;
- shore;
- rocky dry;
- standing-water/puddle potential;
- painted accent lines;
- combined ground mask where useful.

Validation should always distinguish:

```text
mask exists and is correct
shader interprets mask correctly
style tuning looks good
```

Do not diagnose visual tuning before confirming the mask path.

## Runtime State Contract - Deferred

Runtime state remains a future layer. The current likely channel contract is:

```text
R = wetness
G = snow depth / snow amount
B = compression / footprint / trample
A = mud / standing water / disturbance age
```

This contract is not active implementation priority. It should be revisited after the static stack is accepted.

## Future Family Examples

Future ground families should be added through profiles/style assets and should obey the same doctrine.

Possible families:

- Rocky Scrub;
- Frozen Dirt;
- Ash Field;
- Corrupted Ground;
- Worn Road;
- Snow-over-Mud blend;
- Riverbank/Silt;
- Sacred/Ritual Ground.

Each family should tune:

- calm base material;
- macro patch behavior;
- accent-line behavior;
- contact/edge response;
- sparse motif identity;
- semantic mask interpretation;
- runtime response later.

## Maintenance Rules

1. This document owns generated-ground visual doctrine.
2. The implementation plan records patch history and concrete work sequencing.
3. Generic visual-language docs may summarize this doctrine but should link here instead of duplicating every detail.
4. When implementation changes a channel contract, feature-layer meaning, or roadmap priority, update this document and the implementation plan together.
5. Do not allow examples to become accidental requirements. Mark them as examples if they are not committed.
6. Do not introduce a new ground feature unless it identifies its doctrine layer.
7. Do not resume runtime surface-state work until the static style calibration is accepted or the pause is explicitly lifted.

## Final Baseline Statement

Generated ground should be a restrained stylized stage: simple enough to preserve readability, rich enough to feel designed, and structured enough that every future family can share one visual language.

The base is calm. The interest comes from broad patches, meaningful masks, selective painted accents, contact response, sparse motifs, and later runtime state.

That is the ground baseline until deliberately changed.


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

## Patch V3J.3D — Painted Accent Placement Foundation (Implemented, Awaiting Unity Validation)

The accepted Painted Accent geometry and flat unlit ink surface remain unchanged. V3J.3D changes only where deterministic stroke descriptors may be proposed and accepted.

### Distribution contract

Painted Accent placement now uses a dense stratified candidate pool and continuous weighted two-scale value noise rather than selecting one candidate from a sparse evenly distributed grid.

```text
Stroke Density
→ approximate proposal count before hard placement rejection

Distribution Patch Scale
→ world-space size of broad sparse/dense regions

Distribution Patchiness
→ strength of weighted preference for high-noise regions
```

The density field is continuous and retains a non-zero low-density floor. It must be able to produce empty-looking regions, dense regions, isolated marks, and touching or near-touching marks without binary island boundaries. Rejected proposals are not backfilled elsewhere, so rivers and exclusions do not artificially compress the original target density onto the remaining ground. Patch noise and candidate hashes are sampled in patch-coordinate ground space, so neighbouring patches using the same seed and distribution settings do not repeat the same local motif and the broad density field remains continuous across their shared boundary.

`Distribution Patch Scale` and `Distribution Patchiness` are stored per `GroundSurfaceFeatureRecipe`. They are exposed both in the style-profile feature editor and directly in the selected `GeneratedGround` component's Painted Accent controls, so normal variant tuning can remain a one-component workflow.

### Placement-validity contract

Every proposed stroke is validated as a complete curved footprint before it becomes an authoritative descriptor. Validation uses at least thirteen longitudinal samples, never more than 0.25 m apart, and checks the left shoulder, centre, and right shoulder.

A complete stroke is rejected when any required sample:

- cannot sample the base surface;
- overlaps a river handoff corridor or river-bed concealment region;
- lies inside a `GroundModifier` Painted Accent exclusion shape or its blend distance;
- exceeds the proof broad-slope limit;
- crosses a steep longitudinal or transverse local grade.

The initial proof constants are:

```text
river safety clearance = 0.15 m
maximum broad slope = 45 degrees
maximum local grade = 40 degrees
```

The existing positive Painted Accent contribution from `ShoreInfluence` is removed. Rivers are now hard exclusions rather than preferred semantic support.

### General feature exclusions

`GroundModifier` now owns a flags-based `Feature Exclusions` contract. The first flag is `Painted Accent Lines`.

A pure exclusion zone is authored as:

```text
Mode = None
Surface Effect Mode = None
Feature Exclusions = Painted Accent Lines
Shape = Circle or Box
Blend Distance = desired clearance
```

This does not change height or surface masks. It provides deterministic explicit clearances for structures, rocks, encounter spaces, roads, future world-assembly outputs, and other authored areas. Arbitrary physics colliders are intentionally not scanned.

Feature-exclusion snapshots remain available to Painted Accent placement even when the parent ground recipe disables ordinary height modifiers. The `Use Modifiers` switch continues to control height/surface generation only.

### Descriptor/texture ownership

Stroke descriptor generation and legacy 256x256 fold-field rasterization are now separate generator entry points. The accepted descriptors are generated, validated, and stored first. The old projected/debug texture is then rasterized from those accepted descriptors when the existing shader/debug contract requests it.

### Diagnostics

The Painted Accent preview build log now includes:

```text
targetProposals
candidatePool
proposed
accepted
distributionPatchScale
distributionPatchiness
proposalPatchWeightMin/Mean/Max
rejectedSampling
rejectedRiver
rejectedModifierExclusion
rejectedBroadSlope
rejectedLocalGrade
nearestStrokeDistanceMin/Mean/Max
```

V3J.3D is implemented but not accepted until Unity validation proves soft patch distribution, complete river clearance, explicit exclusion zones, terrain-edge rejection, deterministic regeneration, and unchanged crowned-ribbon/flat-ink visuals.


## Patch V3J.3D1 — Painted Accent Distribution Debug and Density Headroom (Implemented, Awaiting Unity Validation)

Unity validation of V3J.3D established that soft patch distribution is a clear visual improvement and that explicit exclusion is functioning. The accepted crowned-ribbon geometry and flat unlit ink remain locked. V3J.3D1 adds authoring visibility and density headroom only; it does not alter patch weighting, proposal selection, exclusion rules, geometry, or rendering.

### Live GeneratedGround placement debug

The selected `GeneratedGround` component now exposes three editor-only Scene view overlays:

- **Show Distribution Overlay** samples the exact production continuous patch-weight function on a 21x21 point grid. Cool/small points represent low placement preference; warm/larger points represent high placement preference.
- **Show Weighted Proposals** displays the exact deterministic proposal centres selected from the production candidate pool before river, modifier, sampling, broad-slope, and local-grade rejection.
- **Show Last Accepted Positions** displays accepted descriptor centres from the most recent placement generation.

The live distribution/proposal snapshot is generated directly from `GroundPaintedAccentFoldFieldGenerator` using the same candidate construction, semantic weighting, noise, weighted random priority, shape seed, patch coordinate, and selected variant values as production placement. It does not rebuild the 3D ribbon preview, change the base mesh/collider, allocate runtime objects, or introduce a debug texture.

The GeneratedGround inspector also displays a compact **Last Generated** statistics block containing target proposals, candidate pool, proposed, accepted, and each rejection category. This distinguishes genuinely sparse patch regions from proposals removed by placement validity.

### Density headroom

`Painted Accent Stroke Density` remains the single authoring control for proposal count. Its range is extended from `0–80` to `0–240`, and the generator target cap is extended to 240. Serialized defaults and authored style assets are unchanged. Density continues to mean approximate weighted proposals per standard 40x40 patch before rejection; rejected proposals remain unfilled so exclusions cannot crowd surviving terrain.

At the maximum 240 accepted strokes, the locked geometry would produce approximately 9,360 vertices and 11,520 triangles. This is authoring headroom rather than a recommended default, and candidate evaluation remains regeneration/editor work rather than per-frame simulation.

### Validation gate

V3J.3D1 is accepted only after Unity confirms:

1. Distribution points update live for seed, density, Patch Scale, and Patchiness changes without rebuilding the 3D preview.
2. Weighted proposal centres match the next generated placement before exclusions.
3. Last accepted points and inspector rejection totals match the build diagnostic.
4. Density values above 80 can produce materially higher proposal/accepted counts.
5. Patch weighting, exclusion behavior, crowned geometry, and flat ink remain visually unchanged.

## Patch V3J.3D1a — Painted Accent Placement Overlay Visibility Correction (Implemented, Awaiting Unity Validation)

Unity validation confirmed that the V3J.3D1 placement controls, proposal counts, density headroom, and rejection statistics were active, but the Scene-view visualization failed its usability requirement. The original debug marks were sub-pixel to only a few pixels at normal Scene-view zoom, low-weight samples were low-alpha against pale ground, and `Handles.zTest = LessEqual` allowed terrain and generated geometry to occlude the diagnostics. The empty debug snapshot also reported itself as valid because it contained non-null empty arrays.

V3J.3D1a changes debug presentation only. Production patch weighting, proposal selection, exclusions, accepted descriptors, geometry, and flat-ink rendering remain unchanged.

### Filled patch heatmap

- The exact production 21x21 distribution sample grid is retained.
- Samples now preserve their fixed grid index and validity state.
- Adjacent samples render as 20x20 surface-following translucent cells rather than tiny points.
- Colour runs from cool blue through cyan to warm red according to the exact production patch weight.
- Invalid surface cells are omitted rather than corrupting grid indexing.

### Clear proposal and accepted markers

- Weighted proposals render as large screen-stable cyan-to-yellow crosses with dark under-strokes.
- Last accepted positions render as larger solid green discs with dark outlines.
- All placement diagnostics use `CompareFunction.Always`, so terrain, rivers, rocks, and ribbon geometry cannot hide them.

### Status and failure reporting

A Scene-view legend explains the active layers and reports valid samples, proposal count, and accepted count. If the live snapshot cannot be built, the legend and GeneratedGround inspector report that failure instead of silently accepting an empty snapshot.

`GroundPaintedAccentPlacementDebugSnapshot.IsValid` now requires a resolution of at least two and exactly `resolution × resolution` distribution samples. `Empty` is therefore correctly invalid.

### Validation gate

V3J.3D1a is accepted only after Unity confirms:

1. The filled blue-to-red distribution field is immediately visible at ordinary Scene-view zoom.
2. Proposal crosses and accepted discs remain unmistakable over pale ground and through scene geometry.
3. The legend reports 441 total samples for a complete 21x21 field and counts consistent with the active proposal/accepted sets.
4. Disabling each toggle removes only its corresponding layer.
5. Production placement, exclusion counts, generated line topology, and rendering remain unchanged.

## Patch V3J.3D2 — Effective Placement Weight Debug and Sparse-Area Control (Implemented, Awaiting Unity Validation)

V3J.3D1a made the placement field visible and confirmed that warm patch regions are preferred, but Unity validation also showed that small local red/cold comparisons can be misleading. Production proposal selection multiplies the noise-driven patch preference by semantic support and then performs a deterministic weighted random draw. A red patch is therefore not a per-cell quota, accepted markers can differ after river/sampling rejection, and the fixed `0.18` sparse floor limited the strongest possible warm-to-cold contrast.

V3J.3D2 preserves the weighted-random placement architecture and adds the controls and diagnostics needed to tune it directly.

### Family/variant sparse-floor control

Each Painted Accent `GroundSurfaceFeatureRecipe` now owns:

```text
Distribution Sparse Floor
range: 0.02–0.40
default: 0.18
```

The value is exposed both in `GroundSurfaceStyleProfileEditor` and in the selected `GeneratedGround` component's consolidated Painted Accent controls. It is the minimum noise-driven patch preference before semantic weighting. Lower values allow cold regions to become substantially quieter while retaining a non-zero proposal chance, so the field remains soft rather than becoming a binary exclusion mask.

The patch-weight contract is now:

```text
patchWeight = lerp(
    1,
    lerp(Distribution Sparse Floor, 1, smoothPatchNoise),
    Distribution Patchiness)
```

The default `0.18` preserves existing authored behavior. A proof value near `0.05` is intended for stronger patch concentration tests.

### Patch versus effective-weight debug

The `GeneratedGround` placement debug section now includes an `Overlay Weight` mode:

```text
Patch Preference
Effective Proposal Weight
```

`Patch Preference` displays the continuous noise-driven patch weight only. `Effective Proposal Weight` displays the actual pre-lottery selection weight:

```text
effectiveProposalWeight = patchWeight × lerp(0.45, 1, semanticSupport)
```

The live debug snapshot stores both values for every distribution sample and proposal. The Scene-view legend reports the selected mode plus minimum, mean, and maximum valid sample weight. This distinguishes weak patch preference from semantic suppression without changing production placement.

Proposal crosses are coloured by effective proposal weight. Last accepted positions are now green rings rather than filled discs, so a proposal cross remains visible inside an accepted marker. A cross without a green ring represents a proposal rejected by sampling, river, modifier, slope, or grade validation.

### Editor API correction

`GeneratedGroundEditor.OnSceneGUI()` now uses the singular `target` property and no longer reads Unity's `targets` array. Unity 6 explicitly forbids `targets` access inside `OnSceneGUI` and `OnPreviewGUI`; the previous implementation produced a warning on every Scene-view repaint.

### Diagnostics and unchanged contracts

The Painted Accent build diagnostic now includes `distributionSparseFloor`. The placement signature includes the new value so live debug and generated descriptors refresh deterministically when it changes.

V3J.3D2 does not alter candidate-pool size, weighted-random priority, no-backfill behavior, river/modifier/slope/grade rejection, accepted crowned-ribbon geometry, flat-ink rendering, base ground mesh, collider, scenes, or style assets.

### Validation gate

1. Confirm the `targets array should not be used inside OnSceneGUI` warning no longer appears.
2. Confirm `Distribution Sparse Floor` is editable from both the style profile and selected `GeneratedGround` component.
3. Compare `Patch Preference` and `Effective Proposal Weight`; verify semantic influence can visibly change the effective field.
4. Enable proposals and accepted positions together; confirm crosses remain visible inside green accepted rings.
5. Confirm the legend reports correct mode-specific weight min/mean/max values.
6. Test Patchiness `0.92`, Sparse Floor `0.18`, then Sparse Floor `0.05`; confirm the lower floor produces materially quieter cold areas without hard island boundaries.
7. Confirm same seed/settings remain deterministic and all placement rejection totals, geometry, and flat-ink visuals remain correct.

## Patch V3J.3D3 — Painted Accent Single-Mound Bias Refinement (Unity-Validated Plateau Fix, Superseded by V3J.3D4 Apex Refinement)

Unity validation of V3J.3D2 established that the patch distribution, sparse-floor control, exclusions, double-sided visibility, and flat-ink rendering are all directionally successful. The remaining individual-line defect is narrower: from the gameplay camera, a significant subset of longer strokes reads as a low, smooth plateau with a long nearly level top rather than as one clear raised mound. High Fold Irregularity does not reliably correct this because most of the existing variation is vertical and is compressed by the elevated camera angle.

V3J.3D3 reopens only the longitudinal crest profile. Placement, exclusions, descriptor centreline paths, width authoring, crown cross-section, flat ink, and family/variant controls remain unchanged.

### Requested-height bias

The generated crest now treats Fold Height as the normal artistic magnitude rather than a loose upper bound:

- per-stroke Fold Height scaling is narrowed from `0.82–1.00` to `0.94–1.00`;
- every shaped longitudinal profile is normalized toward a deterministic target in the `0.90–1.10` range;
- longer and wider descriptor strokes receive a stronger target promotion;
- small seeded variation remains so all strokes do not reach exactly the same height.

The promotion factor uses actual planar stroke length at 65% weight and generated stroke-width position within the existing `0.84–1.18` width variation at 35% weight. This makes the visually riskier long/wide marks more likely to form a decisive crest.

### Plateau-aware single-mound guide

The raw profile is inspected before shaping. Samples at or above 86% of the raw peak define the contiguous high-crest span. Broad high spans increase the guide strength and mound sharpness instead of surviving as long shelves.

The mound guide:

- chooses the dominant crest near the weighted centre of an almost-equal raw high region;
- uses separate deterministic left/right powers so one side may rise or fall more steeply;
- blends more strongly only when the raw profile is broad or the stroke span is large;
- retains more raw variation at high Fold Irregularity;
- preserves the existing targeted one-row valley repair so the earlier double-hill failure does not return.

No monotonic hard constraint is reintroduced. Minor shelves, asymmetry, and small seeded changes remain valid, but long nearly level high sections should become uncommon.

### Longitudinal resolution

The crowned ribbon now uses:

```text
minimum longitudinal rows: 17
maximum longitudinal rows: 25
target spacing: approximately 0.09 m
```

At the current `0.50–0.90 m` authoring range, strokes normally use 17 rows. Longer future strokes can increase resolution up to 25 rows without changing descriptor generation.

With three vertices across, a 17-row stroke contributes:

```text
51 vertices
64 triangles
```

At 220 fully accepted strokes this is approximately 11,220 vertices and 14,080 triangles. The preview remains one combined mesh, one material, no collider, no per-stroke objects, and regeneration/editor work only.

### Diagnostics

The build log now adds:

```text
moundPeakTargetMin/Mean/Max
moundGuideBlendMin/Mean/Max
rawPlateauSpanMin/Mean/Max
plateauSuppressedStrokes
```

Existing actual crest, crown, combined-height, topology, placement, exclusion, and material diagnostics remain authoritative.

### Validation gate

Use the accepted distribution and rendering baseline, with the current proof settings around:

```text
Stroke Width = 0.02 m
Stroke Length = 0.50–0.90 m
Fold Height = 0.20 m
Crest Crown Height = 0.02 m
Fold Irregularity = 0.80
Fold End Taper = 0.40
```

V3J.3D3 is accepted only if the gameplay camera shows materially fewer long flat-topped marks, longer/wider strokes commonly develop a clear dominant mound, actual crest peaks remain near the requested Fold Height, high irregularity retains visible asymmetry, and double hills do not return. Distribution, exclusions, river clearance, crown geometry, and flat-ink output must remain unchanged.


## Patch V3J.3D4 — Rounded Crest Apex Refinement (Unity-Validated, Apex Softening Not Accepted)

Unity validation of V3J.3D3 confirmed that the long-plateau defect was solved, but the plateau-aware mound guide overcompensated: too many marks now terminate in a narrow, roof-like `^` apex. V3J.3D4 keeps the accepted stronger mound, near-requested peak height, asymmetric shoulders, valley repair, distribution, exclusions, crown cross-section, and flat-ink rendering. It changes only the local longitudinal shape around the dominant crest.

### Local rounded crest cap

After mound shaping and targeted valley repair, the dominant shaped peak is detected again. A short deterministic cap is then built around that peak:

- crest half-span varies between approximately 10% and 16% of the longitudinal sample span;
- left and right cap radii receive a small deterministic irregularity-driven asymmetry;
- at least two rows per side are used where the peak location permits it;
- each side blends from the peak toward its own existing boundary height with `SmoothStep`;
- low neighbouring samples are lifted strongly toward the rounded cap, while already-high samples are only reduced lightly;
- the exact peak, cap boundaries, endpoints, end envelopes, and post-shape peak normalization remain intact.

This creates a curved apex with immediate but smooth falloff. It does not clip the crest into a flat table and does not impose a symmetric or monotonic profile.

### Relaxed anti-plateau sharpening

The D3 plateau response remains active but is reduced so it no longer converts most valid mounds into pointed roofs:

```text
plateau guide contribution: 0.42 -> 0.34
base mound sharpness:       1.35 -> 1.15
span sharpness:             0.65 -> 0.50
plateau sharpness:          0.85 -> 0.50
rounded crest blend:        0.72
rounded crest falloff power: 1.65
```

Requested-height promotion, raw plateau detection, asymmetric left/right shaping, targeted valley repair, 17–25 longitudinal rows, and the accepted three-vertex crown remain unchanged.

### Diagnostics

The build log now also reports:

```text
roundedCrestSpanMin/Mean/Max
roundedCrestBlend
roundedCrestFalloffPower
apexSoftenedStrokes
```

The existing mound target, guide blend, raw plateau span, crest height, crown height, topology, placement, exclusion, and material diagnostics remain authoritative.

### Validation gate

Use the same seed and accepted D3/D2 settings. V3J.3D4 passes only if:

1. Long flat plateaus remain materially less common than before D3.
2. Most marks no longer end in a sharp `^` or roof peak.
3. Crests read as short curved caps rather than clipped flat shelves.
4. Left/right shoulder asymmetry and seeded irregularity remain visible.
5. Double-hill/M profiles do not return.
6. Requested Fold Height response, placement positions, exclusions, crown cross-section, flat ink, and topology remain unchanged.

The gameplay camera is the primary acceptance view; close profile views are supporting evidence only.


## Patch V3J.3D5 — Environment-Integrated Flat Ink (Implemented, Awaiting Unity Validation)

Unity validation after V3J.3D4 exposed a separate material-integration defect. The completely unlit Painted Accent ink retained the same value beneath strong cast shadows and at night, while the surrounding ground responded normally. The marks therefore read as bright tracers floating over the terrain rather than as graphic contours belonging to it.

V3J.3D5 does not reopen geometry, placement, exclusions, distribution, crown construction, or Ink Color authoring. It replaces only the flat-unlit output policy with a restrained environment-integrated flat-ink policy.

### Lighting contract

The Painted Accent shader remains graphic rather than physically shaded:

- no normal-dot-light term;
- no face-direction shading;
- no crown/shoulder gradient;
- no metallic, smoothness, specular, emission, texture, or reflection response;
- no self-cast shadow pass;
- double-sided rendering remains enabled.

Every fragment samples environmental illumination at its world position and multiplies the authored Ink Color by one scalar exposure. The scalar uses:

```text
ambient spherical-harmonic luminance
+ main-light luminance and attenuation
+ main-light shadow attenuation
+ restrained additional-light luminance and attenuation
```

Lighting colour is reduced to luminance so the authored Ink Color hue remains stable. Illumination may darken the mark or restore it toward the authored value, but exposure is capped at `1.0`; lights cannot make the ink brighter than its authored colour.

Initial proof constants are:

```text
Ambient Response:       0.75
Direct Response:        0.80
Shadow Response:        0.70
Local Light Response:   0.25
Minimum Visibility:     0.14
```

The minimum visibility floor prevents complete disappearance in darkness while remaining low enough to remove the previous bright nighttime-tracer effect.

### Renderer contract

The preview renderer now reports `receiveShadows=True` and the shader explicitly samples URP main-light and additional-light shadow attenuation. Shadow casting remains `Off`; the visual-only ribbon does not add its own miniature cast shadow to the terrain.

Light probes and reflection probes remain disabled. Ambient response uses the scene environment spherical harmonics, including the project's flat ambient colour/intensity changes driven by the time-of-day system.

### Diagnostics

The Painted Accent build log now reports:

```text
surfaceMode=EnvironmentIntegratedFlatInk
inkAmbientResponse
inkDirectResponse
inkShadowResponse
inkLocalLightResponse
inkMinimumVisibility
receiveShadows=True
shadowCasting=Off
```

### Validation gate

V3J.3D5 passes only if:

1. The same stroke is visibly darker inside a cast shadow than in open daylight.
2. Night no longer produces bright floating outlines over a dark ground.
3. Main and local lights can restore visibility without exceeding the authored Ink Color.
4. Opposite faces, crown, and shoulders remain free of normal-based gradients.
5. The ink hue remains the authored family/variant colour.
6. The ribbon continues to cast no shadow.
7. Geometry, placement, exclusions, distribution, topology, and authoring controls remain unchanged.

The unresolved V3J.3D4 apex-shape question remains separate from this material proof and must not be judged as part of the D5 lighting validation.
