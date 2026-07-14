# Current continuation — `4.11C.5.17D.0` Final-Edge Fray Retirement

Gameplay-camera validation rejected Fray as a useful independent morphology category. Centimetre-scale teeth were not readable from the production isometric camera, while enlarged teeth became indistinguishable from Chipping. C.1, C.1A, and C.1B improved ordering and control ownership but did not establish a stable visual category worth its shader, Inspector, migration, or diagnostic cost.

Fray is therefore retired completely. The active Layer E order is now:

```text
coherent Foam
→ analytical Chipping
→ structural Strands
→ composition
```

Removed production ownership:

```text
final-boundary Fray helper and result structure;
cluster/tooth procedural fields and boundary reconstruction;
Fray shader properties and runtime bindings;
Fray serialized authoring fields, Inspector controls, and tooltips;
Fray diagnostics and debug categories;
dead transient breakupField carrier and runtime _FoamBreakupScale path.

No scene, prefab, material, or other serialized Unity asset is modified. Existing stale Fray values may remain in serialized assets and are ignored because their active fields and bindings no longer exist; any manual asset cleanup is left to the project owner.
```

Serialized debug values `22`, `23`, and `27` are retired and resolve safely to Final. A hidden `legacyFoamBreakupScale` field remains only as a `FormerlySerializedAs` migration source for historical Chip tuning; it has no shader property, runtime binding, Inspector control, or production effect. The historical tuning-version alias containing `ChipFray` is also retained only so old Chip migration state deserializes correctly.

The settled readable morphology ownership is:

```text
Chipping = medium/large subtractive bites and holes
Strands  = elongated anisotropic cuts and remnants
Base lifecycle morphology = broad coherent deterioration
Fray = retired
```

The next morphology task is `4.11C.5.17D.1 — Chipping Readability Audit` from the gameplay camera. Remaining-Life morphology integration stays blocked until the surviving Chip and Strand vocabularies are accepted.

## Methods-tried decision — Fray

| Method | Status | Reason |
| --- | --- | --- |
| Legacy pre-Strand Fray | Rejected | Could not see Chip- or Strand-created final boundaries. |
| C.1 post-Strand hybrid-boundary Fray | Structurally useful, artistically rejected | Correct order, but weak scale readability and control authority. |
| C.1A monotonic Coverage | Rejected | High coverage became broad silhouette recession. |
| C.1B master authority and intermittent teeth | Rejected and retired | Small teeth vanished at production distance; large teeth duplicated Chipping. |
| Final decision | Accepted | Remove Fray and invest in camera-readable Chipping. |

> **Supersession note:** Later sections retain historical Fray experiments and terminology as the canonical methods-tried record. Any statement below that describes Fray as active, pending, or planned is superseded by `4.11C.5.17D.0` above.

## Decoupled Chip shape cadence and transition — `4.11C.5.17B.2D2B-B.2L` — implemented, Unity validation pending

B.2K established meaningful multi-axis contour geometry, but its single cycles-per-second control simultaneously determined when the trajectory advanced and how fast the geometry crossed between configurations. Runtime evidence showed short, visually abrupt contour switches. B.2L keeps the B.2K Fourier geometry and separates the two timing responsibilities:

```text
Shape Change Cadence (changes/s)
= how often a candidate selects its next deterministic contour target

Shape Transition Time (seconds)
= how long the actual geometry takes to move from the previous target to the next
```

Consecutive targets are fixed golden-angle steps through each candidate's accepted two-axis morph plane. The target distance is therefore constant instead of varying randomly. A quintic transition moves between targets, then holds the target for the remainder of the cadence interval. Candidate-specific phase offsets stagger target events across the field. If Transition Time exceeds the cadence interval, the effective transition uses the complete interval and remains continuously in motion.

The timing equation is:

```text
interval = 1 / cadence
effective transition = min(authored transition time, interval)
hold = interval - effective transition
```

The average coefficient-plane angular speed during a transition is the fixed golden-angle step divided by effective transition time. Shape Change Amount remains the sole geometric-excursion control. Candidate Radius, Size Pulse, lifecycle scale, movement, rotation, projected-size LOD, permission, and Strands are unchanged. Fray was later retired by D.0.

# River Foam Stage 6 Canonical Architecture


## Multi-axis Chip contour geometry — `4.11C.5.17B.2D2B-B.2K` — geometry accepted; timing superseded by B.2L

B.2J permission behavior is Unity-validated and accepted. B.2K changes only the analytical candidate silhouette because the previous temporal contour could not produce meaningful geometry evolution: it used three cosine harmonics, stayed mirror-symmetric, interpolated along one A-to-B coefficient line, and multiplied all temporal change by Shape Irregularity.

The static B.2J contour remains the cosine-harmonic baseline:

```text
S(theta) = ShapeIrregularity × [a1 cos(theta) + a2 cos(2theta) + a3 cos(3theta)]
```

Temporal Shape Change now occupies the independent sine-harmonic subspace:

```text
T(theta,t) = b1(t) sin(theta) + b2(t) sin(2theta) + b3(t) sin(3theta)
```

Two deterministic, candidate-specific sine-coefficient directions are orthonormalized. A normalized quarter-cycle trajectory moves through that two-dimensional plane. The temporal coefficient direction is normalized to a constant L1 budget of `0.55`, so its raw radial target remains positive in `[0.45, 1.55]` before normalization.

The temporal target is normalized in squared-radius space. Fourier orthogonality gives its exact angular mean, and the final contour blends static and temporal squared radii:

```text
temporalArea = constant for every trajectory phase
finalRadiusSquared = lerp(staticRadiusSquared, temporalRadiusSquared, ShapeChangeAmount)
```

This preserves temporal radial area exactly instead of turning Shape Change into another Size Pulse. Shape Irregularity owns static cosine asymmetry, while Shape Change independently owns temporal sine geometry; either may be zero without disabling the other. Candidate Radius remains the area-equivalent authored scale. Redistributed lobes can reach at most `1.52x` that radius, so B.2K extends only the adaptive lateral search ceiling from `5x9` to `5x11`; no texture, sample, buffer, compute, persistent-state, lifecycle, motion, rotation, view-readability, eligibility, Strand, or Fray behavior changes.

## Simplified orthogonal Chip permission — `4.11C.5.17B.2D2B-B.2J` — Unity-validated and accepted

Unity validation rejected B.2I. Its local visible-support/material-depth proxy contained no reliable established-body distance information, compressed useful response into the top of the slider, and was multiplied after both Edge Coverage and Interior Access. `Chip Inward Admission` therefore duplicated and serially gated the two existing permissions instead of owning an independent domain.

B.2J removes that third gate entirely. Production Chipping now has two parallel, truthful permissions:

```text
Chip Edge Coverage
  owns only the weak Presence-transition fringe;

Chip Interior Access
  deterministically grants a fraction of activated candidate identities
  full permission over established pre-Chip visible Foam.
```

For pre-Chip visible support `V`, edge eligibility `E`, activated candidate field `C`, and candidate-level interior admission `I_c`:

```text
edgeSelection = C × E
interiorSelection = I_c
productionSelection = V × max(edgeSelection, interiorSelection)
```

There is no Material Pattern gate, inward-depth proxy, or final serial admission multiplier. At `Interior Access = 0`, Edge Coverage remains fully operational. At `Edge Coverage` minimum, Interior Access remains fully operational. `Chip Activation` controls the participating candidate population; `Interior Access` controls the fraction of that population permitted to cut established body.

Candidate-independent diagnostics use:

```text
edgeAuthority = V × E
interiorAuthority = V × InteriorAccess
potentialEligibility = max(edgeAuthority, interiorAuthority)
```

The dedicated debug slot formerly used by Inward Admission now shows `Chip Interior Access` authority. No candidate lifecycle, rigid motion, projected-size LOD, search bounds, Strand logic, Fray logic, texture, sample, buffer, dispatch, or persistent state changes are included. Removing the failed gate also removes its square root, derivative, smoothstep, shader property, binding, and serialized Inspector control.


## Extended rigid lateral Chip travel — `4.11C.5.17B.2D2B-B.2H` — implemented, Unity validation pending

B.2F/B.2G are Unity-validated and accepted for lifecycle, rigid evolution, and bounded projected-size readability. B.2H changes only the authority and search completeness of the existing rigid lateral centre motion.

### Control and physical meaning

```text
Lateral Motion Amount (spacing)
  maximum plus/minus candidate-centre excursion measured in Candidate Spacing;
  range 0–2.5.

Lateral Motion Speed (cycles/s)
  independent frequency of the existing smooth periodic lateral wave.
```

For Candidate Spacing `S`, amount `A`, frequency `f`, candidate phase `phi`, and the existing signed smooth periodic wave `W`:

```text
centreY(t) = staticCentreY + S × A × W(f × t + phi)
excursion = ±S × A
peak-to-peak travel = 2 × S × A
peak centre speed = 6 × f × S × A
```

The factor `6` is the exact maximum derivative per cycle of `RiverWaterFoamResolveChipSignedWave`; the Inspector exposes resolved excursion and peak physical speed. Translation remains rigid, so the larger range cannot shear, rotate, or stretch an individual contour.

### Exact adaptive rectangular search

The previous `0.25 × spacing` maximum fit inside the B.2F `3×3`/`5×5` search. At the new `2.5 × spacing` maximum, candidate source cells can originate farther laterally. Let:

```text
R = maximum candidate radius in spacings after bounded view scale,
    size irregularity, and size pulse
A = Lateral Motion Amount
J = 0.39 × Distribution Irregularity
C = 0.5 + J

required downstream offset = floor(R + C), clamped to 1–2
required lateral offset    = floor(R + A + C), clamped to 1–4
```

The shader evaluates the smallest enclosing rectangle from `3×3` through `5×9`. At the complete authored maxima:

```text
R <= 0.65 × 1.42 × 1.45 = 1.338 spacings
A <= 2.5 spacings
C <= 0.89 spacings

required downstream offset = floor(1.338 + 0.89) = 2
required lateral offset    = floor(1.338 + 2.5 + 0.89) = 4
maximum search             = 5×9
```

Lower settings retain smaller rectangles, including `3×3`, `3×5`, `3×7`, `3×9`, `5×3`, `5×5`, `5×7`, and `5×9` as required. No texture, sample, buffer, compute dispatch, candidate identity, lifecycle, eligibility, Strand, Fray, Layer C/D, or production-order change is added.


## Bounded projected-size Chip LOD — `4.11C.5.17B.2D2B-B.2G` — Unity-validated and accepted

B.2F remains authoritative for candidate identity, lifecycle, rigid motion, pulse, and shape change. B.2G changes only how each static candidate radius is rendered when projection would make stable Chips unreadably small.

### Controls and ownership

```text
Minimum Stable Radius (px)
  target screen radius for each fully formed candidate;
  zero preserves pure world-space sizing.

Maximum View Scale
  upper bound on readability enlargement;
  one disables enlargement.
```

Candidate positions, hashes, activation, eligibility, lifecycle phase, motion, rotation, pulse, and contour coefficients remain River-space systems. No candidate is generated or anchored in screen space.

### Metric-correct projected radius

For River coordinate `p = (distance, lateral)`, the fragment shader forms the screen-to-River Jacobian from `ddx(p)` and `ddy(p)`. `metresPerPixel` is the largest singular value of that Jacobian: the square root of the largest eigenvalue of its Gram matrix. It therefore measures the most compressed projected direction and automatically accounts for camera distance, field of view, resolution, and surface foreshortening.

For one candidate’s static authored radius `R`, projected radius `r`, target `P`, and maximum scale `S`:

```text
r = R / metresPerPixel
w = 1 - smoothstep(0.75 P, P, r)
required = clamp(P / max(r, epsilon), 1, S)
viewScale = lerp(1, required, w)
stabilizedStaticRadius = min(
    R × viewScale,
    0.65 × CandidateSpacing × candidateSizeMultiplier)
```

The scale is never below one, so close Chips are never shrunk. The target is a readability floor rather than exact billboard locking. `Maximum View Scale` and the existing `0.65 × spacing × sizeMultiplier` static-radius cap prevent unbounded world-space growth.

The target is evaluated after deterministic size irregularity, so each candidate receives its own readability decision. The spacing-relative cap is scaled by that same size multiplier, preserving the accepted authored maximum. Size pulse and lifecycle are applied last:

```text
finalRadius = stabilizedStaticRadius
              × sizePulse
              × lifecycleScale
```

Formation and Dissolve therefore still reach exact zero, and Dormant Time remains exact zero coverage.

### Subpixel tail and search completeness

The old whole-candidate attenuation between approximately `0.75–2.0 px` radius is removed. Only the genuinely unresolved lifecycle tail is faded:

```text
subpixelVisibility = smoothstep(0.25, 0.75, finalRadius / metresPerPixel)
```

The same singular-value metric is used as conservative world-space contour antialias width. Candidate-search selection and the Inspector readout use the conservative `Maximum View Scale` ceiling. The existing `3×3`/`5×5` search and `1.34 × spacing` final-radius cap remain sufficient.

Serialized tuning version 8 assigns `Minimum Stable Radius = 2 px` and `Maximum View Scale = 1.75`. No texture, texture sample, buffer, compute dispatch, persistent state, candidate identity change, density LOD, Strand change, Fray change, or production-order change is added.


## Independent Chip lifecycle and rigid evolution — `4.11C.5.17B.2D2B-B.2F` — Unity-validated and accepted

B.2F supersedes the coupled B.2E Evolution Amount/Rate model. B.2D production eligibility and B.2E candidate-independent permission diagnostics remain unchanged.

### Control ownership

```text
Lifecycle — always active for every candidate retained by Chip Activation
  Formation Time
  Stable Time
  Dissolve Time
  Dormant Time

Rigid Motion
  Downstream Speed
  Lateral Motion Amount / Speed
  Rotation Amount / Speed

Living Variation — established stage only
  Size Pulse Amount / Speed
  Shape Change Amount / Cadence / Transition Time
```

The retired hidden `foamChipEvolutionAmount` and `foamChipEvolutionRate` fields remain serialized only for one-way tuning-version-7 migration. They are no longer bound to the shader.

### Four-stage lifecycle

For authored durations `F`, `S`, `D`, and `Q`:

```text
T = F + S + D + Q
τ = frac(time / T + candidatePhase) × T
smooth5(x) = 6x^5 - 15x^4 + 10x^3

formation = smooth5(τ / F)
dissolution = 1 - smooth5((τ - F - S) / D)
lifeRadiusScale = min(formation, dissolution)
```

This guarantees monotonic zero-to-one Formation, exact one during Stable Time, monotonic one-to-zero Dissolve, and exact zero throughout Dormant Time. Candidate phase is deterministic per lattice cell, so the same identity returns asynchronously on the next cycle. Lifecycle does not depend on any motion, pulse, rotation, or shape amount.

Size pulse and shape change are eased in and out inside Stable Time. Formation and Dissolve therefore return to the accepted authored base contour and cannot be reversed by a pulse wave.

### Rigid evolution and smear removal

The nonlinear value-noise coordinate warp, derivative-basis inversion, and local metric correction are removed. Candidate-local position is now:

```text
Delta_i(p,t) = p - Centre_i(t)
Centre_i.y(t) = StaticCentre_i.y
                + CandidateSpacing × LateralAmount × wave_i(t)
```

The spatial Jacobian of translation is the identity. Rotation uses an orthonormal axis transform. Neither operation can shear, fold, or stretch a candidate. Downstream Speed remains a uniform River-space translation of the complete field.

### Independent living variation

```text
livingRadius = authoredRadius
               × lifeRadiusScale
               × (1 + SizePulseAmount × pulseWave × stableAuthority)
```

`Size Pulse Amount` is a direct fractional radius excursion: `0.20` means `80%–120%` while established. It never controls death or Dormant Time.

B.2K supersedes the old single-axis A-to-B contour interpolation. Static Shape Irregularity remains the accepted cosine-harmonic contour. Shape Change independently adds a candidate-specific sine-harmonic target moving through an orthonormal two-axis coefficient plane. The target uses a positive fixed coefficient budget and squared-radius blending with a phase-invariant angular mean, so the silhouette redistributes area without becoming a Size Pulse. Candidate Radius remains the area-equivalent scale; the bounded `1.52x` lobe reach is handled by the adaptive search.

### Candidate-search completeness

B.2F originally used an adaptive `3×3`/`5×5` search for the then-approved `0.25 × spacing` lateral excursion. B.2H expanded rigid travel to an adaptive `3×3` through `5×9` contract. B.2K retains the same rectangular derivation but multiplies radius reach by the conservative multi-axis lobe factor:

```text
shapeReach = sqrt(lerp(1, 1.52^2, ShapeChangeAmount))
```

The maximum lateral offset therefore extends to five source cells, producing a `5×11` maximum only when the combined radius, morph, jitter, and lateral settings require it. The area-equivalent candidate-radius base remains:

```text
0.65 Radius Ratio × 1.42 Size Irregularity × 1.45 Size Pulse
= 1.338 × Candidate Spacing
```

No texture, texture sample, compute dispatch, persistent field, Layer C/D write, Strand change, Fray change, or production-order change is added.

### Migration

Serialized tuning version 7 assigns the deliberate slow lifecycle defaults `2.5 / 5 / 2.5 / 4 seconds`. The rejected nonlinear warp is not preserved. Legacy Evolution Amount migrates only to the bounded authorities that have a truthful equivalent:

```text
Lateral Motion Amount = 0
Rotation Amount       = 0
Size Pulse Amount     = old amount × 0.08 radius
Shape Change Amount   = old amount
```

The rejected nonlinear warp is not reinterpreted as rigid movement. Lateral and rotation therefore start neutral and require deliberate authoring.

All independent speeds receive slow defaults. Existing Edge Coverage, Interior Access, candidate construction, Strands, and temporary legacy Fray remain unchanged.


## Superseded Chip turnover and warp-safety model — `4.11C.5.17B.2D2B-B.2E`

B.2D production selection is retained. B.2E changes exactly three remaining responsibilities without adding a resource or changing Strands, Fray, Layer C, or Layer D.

### Candidate-independent eligibility domain

`Chip Eligibility Composite` no longer consumes current candidate contours, Activation, lifecycle phase, or current Evolution position. It now displays the potential permission field:

```text
preChipVisibleSupport
× binaryAntialiasedMaterialAdmission
× max(edgeEligibility, Chip Interior Access)
```

`Chip Interior Access` is a probability over deterministic candidate cells, so its candidate-independent representation is expected permission authority across the visible body. Fractional Interior Access therefore changes yellow intensity rather than drawing current candidate blobs. `Chip Final Selection` and `Production Chip Mask` remain the current-candidate diagnostics.

### Complete candidate turnover

At full Evolution Amount, each participating candidate now has exact lifecycle endpoints:

```text
birth:  radius = 0, visibility = 0
mature: radius reaches the authored maximum, visibility = 1
release: radius = 0, visibility = 0
next cycle: deterministic asynchronous return through frac(candidate clock)
```

The old `0.13×` minimum radius is removed. A pixel-footprint gate suppresses unresolved birth/death remnants; Evolution Amount `0` remains on the unchanged static path.

### Bounded spatial evolution

Spatial coordinate warp authority is capped at `0.20`. Values above `0.20` continue increasing lifecycle, pulse, and contour-morph authority but cannot increase coordinate displacement or fold severity. The derivative-basis correction now:

```text
rejects reversed or near-collapsed bases through a smooth stability weight;
limits corrected local-vector magnitude to 0.67×–1.50×;
blends to the uncorrected evolved-space delta when the basis is unstable.
```

This replaces the former inverse correction that permitted up to `6×` local-vector amplification. No texture, sample, kernel, dispatch, buffer, persistent write, serialized control, migration, or candidate-loop expansion is added.


## Visible Chip domain and binary admission — `4.11C.5.17B.2D2B-B.2D` — implemented, Unity validation pending

The B.2C composite exposed a real production limitation, not merely a diagnostic mismatch. Production `Chip Selection Depth` was evaluated against `materialEdgeDepth = baseMask`, which is a local Presence-derived coverage value rather than geometric distance from the Foam boundary. Established material saturates that scalar to `1`, while the serialized selector stops at `0.95`; established Foam was therefore mathematically unavailable to the Chip path. The same path multiplied `materialBody` twice and used a smooth Material Pattern value as cut strength, allowing hardening to erase many partial cuts.

B.2D makes the zero-resource contract explicit instead of pretending that local Presence amplitude contains interior distance:

```text
Chip Edge Coverage
  retains the old low-cost Presence-transition selector;
  low values permit thin weak-material strips;
  high values permit all non-saturated edge material;
  it does not claim geometric distance through an established body.

Chip Interior Access
  candidate-level deterministic permission for established visible Foam;
  0 keeps every activated candidate edge-only;
  1 grants every activated candidate full visible-body permission;
  intermediate values admit complete connected candidates, not pixel noise.

```

The production formula is now:

```text
preChipVisibleSupport = ResolveBaseCoverage(preChipMask)

edgeSelection =
    activatedCandidates
    × materialBody
    × edgeBand(Chip Edge Coverage)

interiorSelection =
    activated candidates admitted by Chip Interior Access

productionChipSelection =
    preChipVisibleSupport
    × max(edgeSelection, interiorSelection)
```

`materialBody` is applied once. At `Chip Activation = 1` and `Chip Interior Access = 1`, every activated candidate can reach selection value `1` anywhere beneath the pre-Chip visible Foam body. Candidate spacing, radius ratio, irregularity, lifecycle, motion, and contour coverage are then the only remaining spatial limits. B.2J adds no texture, texture sample, compute kernel, dispatch, buffer, Layer C write, Layer D write, or candidate-search expansion; the existing adaptive B.2H rectangular search remains unchanged.

Serialized tuning version 6 still initializes existing Rivers to `Interior Access = 0`. The removed historical admission value is no longer read, bound, or clamped.

At B.2D, the Chip composite was corrected only to compare pre-material Chip Final Selection against pre-Chip rendered coverage rather than a post-breakup mask. B.2E then made the permission diagnostic candidate-independent. B.2J preserves that contract while reducing the domain to the independent Edge Coverage and Interior Access authorities.

### Canonical methods-tried ledger

| Method | Status | Reason |
| --- | --- | --- |
| Treat Presence-derived `baseMask` as geometric Chip depth | Rejected | Saturates throughout established Foam and cannot encode inward distance. |
| Raise the existing threshold alone | Rejected | Cannot recover information absent from a flat saturated scalar; even `1` remains derivative-band dependent. |
| Smooth Material Pattern multiplied as Chip strength | Rejected | Partial values are frequently swallowed by the later hardening thresholds. |
| New Chip distance texture or compute pass | Rejected by current constraint | Would provide real distance but violates the approved zero-resource architecture. |
| Separate edge coverage from candidate-level interior permission | Implemented in B.2D | Gives truthful edge-only control and guaranteed whole-body endpoint without new resources. |
| Binary antialiased Material Pattern admission | Rejected after Unity evidence | Produced disconnected transported ranking islands and duplicated Activation/Interior Access ownership. |
| Threshold visible-support/material-depth geometric mean | Rejected after B.2I validation | Local coverage saturated through established bodies, useful response collapsed near one, and the serial multiplier made it dependent on Interior Access. |
| Remove the third gate; keep Edge Coverage and Interior Access as parallel permissions | Implemented in B.2J | Restores independent control ownership and removes unnecessary per-fragment admission work. |
| Single soft Fray threshold from `0.88` to `0.30` with unbounded `fwidth` | Rejected after C.1 Unity evidence | Maximum Coverage never admitted the complete perimeter, while wide threshold antialiasing changed partial displacement across mostly the same locations. |
| `1 - Coverage` admission plus a `0.55–1.0` continuous serration profile | Rejected after C.1A Unity evidence | High Coverage admitted broad edge areas but never restored untouched gaps, so Fray read as smooth large-area recession rather than teeth. |
| Independent Strand-side Fray cutter | Rejected after C.1A Unity evidence | Ignored all Fray controls and produced visible tooth-like cuts when Fray Amount and Tooth Depth were zero. |
| Fray master-off contract plus separate cluster/tooth patterns | Implemented in C.1B, validation pending | Makes zero authoritative across final and Strand boundaries, retains untouched gaps at maximum Amount, and uses camera-readable tooth sizes without new resources. |
| Three cosine harmonics interpolated along one A-to-B line | Rejected after B.2J validation | Stayed mirror-symmetric, depended on Shape Irregularity, and read as one dragged blob with weak pulse-like flexing. |
| Independent sine-harmonic two-axis temporal geometry | Accepted in B.2K; timing decoupled in B.2L | Decouples temporal shape from static irregularity, keeps coefficient authority constant, and redistributes lobes without changing radius controls. |
| One cycles-per-second control for both target cadence and transition speed | Rejected after B.2K validation | Correct geometry could still switch abruptly because event spacing and interpolation duration were inseparable. |
| Shape Change Cadence plus Shape Transition Time | Implemented in B.2L, validation pending | Uses constant-distance deterministic targets and quintic transitions so event frequency and actual geometric speed are independently authored. |
| Candidate-shaped Eligibility Composite | Rejected after B.2D validation | Showed where current chips landed, not the candidate-independent area where Chipping is permitted. |
| Expected permission field for fractional Interior Access | Implemented in B.2E | Represents candidate-level admission probability without changing production behavior. |
| Lifecycle radius floor of `0.13×` | Rejected | Prevented true growth from and return to zero. |
| Spatial warp authority across the full Evolution Amount range | Rejected | Increased coordinate fold and smear risk beyond the visually usable range. |
| Warp capped at `0.20`, full-range lifecycle/morph authority retained | Implemented in B.2E | Separates useful turnover from unsafe coordinate deformation. |
| Up to `6×` inverse-basis correction | Rejected | Allowed near-fold metric reconstruction to amplify local deltas into streaks. |
| Stability-gated `0.67×–1.50×` local correction | Implemented in B.2E | Bounds correction and falls back smoothly near collapsed or reversed bases. |
| Coupled Evolution Amount/Rate for lifecycle, motion, pulse, and morph | Rejected after B.2E validation | Could not provide slow death/dormancy and independent tuning. |
| Zero-duration dormant phase | Rejected | Candidate immediately re-entered formation and read as twinkling. |
| Nonlinear coordinate warp plus inverse metric correction | Rejected and removed in B.2F | Continued to permit smeared or elongated candidates. |
| Explicit four-stage lifecycle independent of variation | Implemented in B.2F | Guarantees formation, stable dwell, dissolve, and exact dormant dwell. |
| Rigid translation/rotation with bounded pulse/morph pairs | Implemented in B.2F | Gives independent physical ownership and removes motion-induced stretching. |
| Adaptive `3×3`/`5×5` candidate search | Implemented in B.2F | Preserves complete coverage when pulse or lateral travel exceeds compact reach. |
| Whole-candidate fade from `0.75–2.0 px` projected radius | Rejected after B.2F validation | Combined with Foam hardening, it erased stable distant Chips rather than merely antialiasing them. |
| Increase all world-space Chip radii for distant readability | Rejected | Would make close Chips oversized and would not account for projection angle or resolution. |
| Unbounded exact screen-size locking | Rejected | Would create arbitrarily large world-space cuts and excessive overlap at extreme distance. |
| Bounded singular-value projected-size LOD | Implemented in B.2G | Preserves world-space identity, enlarges only undersized Chips, keeps lifecycle zero endpoints, and caps world growth. |
| Raise Lateral Motion Amount without expanding source-cell search | Rejected | Candidates can clip or disappear when translated beyond the searched lattice rows. |
| Rigid `0–2.5 spacing` lateral travel with exact adaptive rectangular search | Implemented in B.2H | Provides tenfold travel authority while preserving complete candidate contours and smaller search tiers at lower settings. |


## Purpose

This is the canonical architecture contract for Stage 6 river Foam.

This document is the active source of truth for how the Foam system is allowed to work. It supersedes older persistent-morph, lateral-row-commit, pocket/entity, shader-macro-stretch, local-edge-fray, and one-off coherent-warp plans wherever they conflict with this contract.

The goal is to reproduce the broad behavior of the visual inspiration river: stylized pale surface-film sheets, connected ribbons, bank and obstacle skirts, temporary bridges, pinches, fractures, edge chipping, small fragments, and thin bright surface streaks, while preserving a performance-safe field-based architecture.

The target is not a physically exact fluid solver and not a foam entity database. The target is a fixed-grid mathematical field system with strict ownership boundaries and no circular dependencies.


## Historical evidence gate — eligibility composites and shape-preserving Chip advection — `4.11C.5.17B.2D2B-B.2C`

Unity evidence from B.2B showed that the large coordinate warp could stretch otherwise valid Chip blobs into long ribbons. The cause was architectural: the warped coordinate controlled both candidate movement and local contour distance. B.2C keeps the same large animated lookup field, but converts each candidate-local delta back into the unwarped River metric through screen-space derivative bases before evaluating the connected contour. The field still controls candidate translation, compression, turnover, and clustering; it no longer owns local chip aspect ratio. Near coordinate folds, the inverse is bounded to prevent unbounded correction.

B.2C also adds exactly two comparison diagnostics using existing fragment data and no new texture:

```text
Chip Eligibility Composite
Fray Eligibility Composite

Dark gray   exact current Final Foam mask;
Cyan        eligibility outside the rendered mask;
Bright yellow rendered Foam overlapping eligibility.
```

At B.2C time these views compared the post-breakup rendered silhouette against the then-current Chip Edge Eligibility and Fray Permitted Band. B.2C deliberately did not alter either formula. Its Chip comparison contract and production eligibility are superseded by B.2D above; the Fray evidence remains preliminary until `2D2B-C`. No texture, compute dispatch, buffer, persistent state, authoring control, candidate-loop expansion, Strand change, or Fray production change was introduced.

## Chaotic Chip advection and geometric turnover — `4.11C.5.17B.2D2B-B.2B`

Unity rejected B.2A because its candidate-local centre motion was capped to `0.10 × Candidate Spacing`, radius pulse was only `±3.5%`, and animated orientation could rotate by roughly `±66°`. The visible result was therefore nearly stationary chips rotating in place, with no meaningful travel or growth/shrink.

B.2B replaces that local-wave model with a continuous animated coordinate field evaluated before lattice lookup:

```text
base River coordinate
→ Field Speed downstream translation
→ broad animated coordinate warp
→ finer independently moving coordinate warp
→ candidate lattice lookup
```

At full Evolution Amount the combined field can displace the analytical pattern by several candidate spacings, with materially stronger lateral than downstream authority. Because lookup occurs after the coordinate warp, the existing fixed `3×3` search remains complete regardless of total field displacement. Spatially varying compression and release allow temporary concentrations to form and dissolve without persistent candidate pairs.

Candidate turnover now changes geometry as well as opacity. Each stable cell hash owns an independent lifecycle rate and phase; a candidate grows from approximately `0.13×`, reaches approximately `1.08×`, then shrinks before disappearing. A separate bounded radius pulse adds visible scale variation. Animated rigid rotation is removed: contour orientation remains hash-authored and fixed, while coefficients morph between two candidate-specific connected contours.

The controls remain:

```text
Chip Field Speed (m/s)  global downstream field translation;
Chip Evolution Rate     coordinate-warp, lifecycle, and morph time scale;
Chip Evolution Amount   multi-spacing advection, geometric turnover,
                        radius pulse, and contour-morph authority.
```

No texture, compute dispatch, Motion Field sample, persistent state, candidate-loop expansion, control, or debug view is added. `Evolution Amount = 0` disables the coordinate warp and geometric turnover while preserving Field Speed.

## Superseded Chip evolution attempts

`B.2` is rejected because one candidate was a rigid three-circle union. `B.2A` correctly replaced that union with one connected contour, but its movement and scale formulas were visually ineffective and rotation-dominant. B.2B retains the accepted single-contour representation and static authoring controls while replacing only the evolution model.

## Production Chip control model — `4.11C.5.17B.2D2B-B.1A`

B.1A corrects three source-proven formula defects without adding textures, compute work, candidate samples, or time input.

```text
Candidate Spacing (m)       average lattice density; minimum 0.10 m;
Distribution Irregularity   centre jitter only;
Candidate Radius Ratio      mean radius / spacing, range 0.05–0.65;
Size Irregularity           centred candidate scale range;
Shape Irregularity          fixed-extent silhouette redistribution.
```

Absolute mean radius is now truthful and explicit:

```text
mean radius = Candidate Spacing × Candidate Radius Ratio
```

The ratio is bounded so the existing fixed 3×3 analytical candidate search remains complete at maximum centre jitter and maximum size variation. The old metre-radius authoring was dishonest because shader evaluation silently capped it to `spacing × 0.46`; serialized tuning version 4 migrates the previously visible effective radius into the new ratio.

At Size Irregularity `0`, every candidate uses the mean radius. At `1`, deterministic candidate scale spans approximately `0.58×–1.42×`, twice the former range and centred on the authored mean.

At Shape Irregularity `0`, one circle occupies the candidate outer radius. B.1A originally redistributed that extent across three displaced lobes; Unity later rejected that construction because it read as a rigid cluster. B.2A supersedes only the silhouette construction with one connected harmonic contour while preserving Radius Ratio and Size Irregularity authority.

Distribution, size, and shape hashes remain stable and independent. B.2B adds coordinate advection, geometric turnover, and contour morphing without changing those authoring meanings.

## Superseded production Chip handoff — `4.11C.5.17B.2D2B-B`

This section records the original B production contract and is superseded for Chip domain/admission mathematics by B.2D above. C.1 later inserted final-boundary Fray, and D.0 retired that feature. Current production order is:

```text
Layer C persistent material and transported Material Pattern
→ analytical production Chip selection
→ accepted Layer E Strands
→ final composition
```

Production Chipping uses no new texture, compute pass, persistent field, or Layer C mutation. The sparse, jittered single-contour world-space candidates are filtered in four explicit stages:

```text
Chip Activation
× Chip Selection Depth
× transported Material Pattern gate
→ soft-body Chip cut before Strands.
```

The transported gate is a fixed smooth function of Layer C `Material Pattern`. It has no wall-clock phase and no random reseeding. River-space candidates therefore do not remain permanently open: eligible material moving through a candidate controls whether the cut is active. The final cut is reconstructed from the pre-hardening soft body, then applied to the accepted hardened mask before Strands. Legacy `Chip Strength` is serialized only for migration and is no longer bound to the shader, preventing a hidden second Chip pass.

Production controls are mathematically separate:

```text
Chip Activation
  fraction of analytical candidates retained;
Candidate Spacing (m)
  average world-space candidate density;
Distribution Irregularity
  deterministic centre-position jitter only;
Candidate Radius Ratio
  mean candidate radius divided by Candidate Spacing;
Size Irregularity
  candidate-to-candidate radius variation only;
Shape Irregularity
  circle to asymmetric connected-contour distortion only;
Chip Edge Coverage
  edge-only Presence-transition territory eligible for removal;
Chip Interior Access
  deterministic fraction of candidate identities allowed beyond the edge band.
```

Chip diagnostics are ordered and authoritative:

```text
Chip Candidate Field
Chip Activated Candidates
Chip Edge Coverage
Chip Eligibility Composite candidate-independent permission domain
Chip Final Selection       current candidate intersections after both permissions
Chip Interior Access       candidate-independent established-body authority
Production Chip Mask       exact hardened coverage removed
```

`Production Chip Mask` must agree with Final Foam. `Chip Activation = 0` must reproduce the pre-B result exactly. Strands remain unchanged and are clipped only because the already-chipped body reaches them first.

Historical C.1 superseded the diagnostic-only Fray prototype, but D.0 later retired the entire Fray production, authoring, binding, and diagnostic path. No Fray resource, control, helper, or debug view remains active.


## Play Mode topology startup policy — `4.11C.5.17B.P1`

Topology generation is explicit dirty-time Editor work. Ordinary Play startup is cache-only and must never call obstacle cache generation, `BuildMajorTopology`, `BuildConnectorTopology`, `BuildPocketTopology`, delayed topology replacement, cache-asset mutation, or `AssetDatabase.SaveAssets`.

Startup outcomes are authoritative:

```text
Exact
  validate and install the assigned payload;
  continue without generation or persistence.

Stale-compatible
  install the structurally compatible payload for this session;
  report exact obstacle/settings mismatch reasons;
  do not rebuild or save during Play.

Missing / incompatible
  leave topology-dependent Foam disabled in a stable Preparation Required state;
  report one actionable diagnostic;
  do not retry, generate, read back, or save during Play.
```

The authoring workflow is **Actions → Foam Layer A Cache Tools → Prepare / Rebuild Foam Topology Cache**. It runs outside Play, reuses the existing obstacle/Major/Connector/Pocket generators, builds one payload, stores the assigned cache once, and calls `SaveAssets` once. A matching next Play entry must install the exact cache with zero topology builds.

One compact `[River Foam P1] Startup` summary is emitted per startup outcome. It records total wall time, field dimensions, cache outcome/reason flags, phase counts/times, topology-build counts, registry event counts, dirty cycles, restarts, and Play cache-write attempts. Detailed per-event success logging remains prohibited.

The River-to-Ground restoration cascade is deliberately not corrected here. That is cross-feature work for a separate thread; this River-only patch neither changes Ground code nor suppresses River/Ground structural ownership.

## Explicit cache-build efficiency contract — `4.11C.5.17B.P3`

P1 defines when topology work is allowed. P3 defines the normal cost contract for that explicit Edit Mode work without changing topology results.

The ordinary **Prepare / Rebuild Foam Topology Cache** transaction must:

```text
wait for one ready River domain and settled obstacle registry;
build obstacle exclusion once;
build Major, Connector, and Pocket CPU topology once each;
publish generated topology to the GPU once, after the complete graph exists;
validate package dimensions, fingerprints, collections, and checksum contract;
serialize the normal payload once;
clone payload bytes once into the persistent cache asset;
verify stored metadata and bytes directly;
call SaveAssets once.
```

Incomplete Major-only and Major-plus-Connector GPU publications are prohibited during explicit preparation. They remain available only where an actual live runtime stage requires partial visual publication.

The exhaustive deterministic proof is not part of the normal build. **Run Exhaustive Cache Integrity Proof** explicitly performs repeated serialization, deserialization, exact byte reproduction, generated-channel parity, and deliberate corruption rejection. Strict release validation remains a separate non-mutating action that verifies the stored payload against current domain, obstacle, and generation fingerprints.

Runtime cache readers may use the cache asset's internal read-only payload reference because the codec never mutates source bytes. Public callers continue to receive defensive copies.

GeneratedGeometryRegistry callbacks are observations, not river-boundary invalidations. A restoration wave is coalesced into one pending notification burst and resolved only after the disturbance runtime reports its obstacle registry ready. The final `ObstacleGeometryVersion` decides whether the active cached obstacle field/topology is stale. The geometric river boundary texture is not rebuilt for obstacle-source notifications because it contains shoreline coverage only.

P3 acceptance requires one generated-topology GPU publication and one normal serialization, an unchanged payload size/hash for unchanged inputs, successful strict cache validation and exhaustive proof, and an exact-cache Play launch with zero generation or persistence.

## Steady-state work accounting contract — `4.11C.5.17B.P4`

P4 observes operational Foam work without becoming a scheduler. The accounting window remains inactive by default, begins only through the explicit Start / Reset action after Play startup resources are ready, and never gates dispatch, material lifetime, topology evolution, debug evaluation, readback, rendering, or resource ownership.

The canonical categories are:

```text
total work
  compute dispatch count and logical cell-iteration count;

Layer C material work
  fixed material commits, CFL transport substeps, maximum per-substep CFL,
  material dispatch/cell counts, and CPU command-submission time;

topology work
  dirty evaluations, positive queued-work observations, evolution checks,
  composite topology refreshes, topology dispatch/cell counts, and CPU
  command-submission time;

Layer D diagnostic work
  visual occupancy/shape evaluations, dispatch/cell counts, and CPU
  command-submission time;

diagnostic readbacks
  topology and transport metric requests, completions, errors, and topology
  request timeouts;

empty-field evidence
  material commits that continued while the latest fresh asynchronous metrics
  sample reported zero integrated Presence and zero visible Presence core;

visibility evidence
  accounted active frames split by `surfaceRenderer.isVisible`.
```

CPU measurements cover main-thread setup and GPU command submission, not GPU execution. Visibility is the renderer visibility signal, not a gameplay-distance policy. Empty-field qualification is asynchronous evidence and must not itself stop lifetime aging. Logging is explicit and compact through **Start / Reset P4 Accounting** and **Log P4 Work Summary** under Actions; Runtime Diagnostics remain read-only; no periodic Console output is permitted.

## Current implementation status — `4.11C.5.17B.2D2A`

Unity presentation audit is conclusive: normal Final Foam stuttered beside rocks and banks, while both `Foam Committed Final Preview` and `Foam Evaluated Final Preview` remained stable on the same material population. The committed simulation therefore was not oscillating. The rejected point-velocity residual predictor was the presentation fault.

`4.11C.5.16E.2 — Committed Final Foam Promotion + Residual Prediction Retirement` promotes the proven committed Layer C presentation to normal Final Foam and removes residual prediction from active ownership:

```text
Final Foam
  = current committed Layer C packed state
  + selected Concentration + Lifetime or Lifecycle-Faithful policy
  + existing surface coupling, colour, lighting, opacity, and fog;

removed
  = _FoamRenderAdvectionSeconds
  + render-side canonical velocity reconstruction
  + obstacle-influence prediction fade
  + downstream/lateral point backtrace.
```

`Foam Committed Final Preview` is removed because it is now identical to production. Serialized debug value `16` is reserved and resolves safely to Final Foam. `Foam Evaluated Final Preview` remains diagnostic-only for the later Layer D production decision. No Layer D promotion occurs in this patch.

The two Layer C ARGBHalf textures remain mandatory read/write ping-pong storage for conservative transport; they are not render-history textures and are not removed. The patch adds no texture, field, channel, buffer, kernel, dispatch, or memory allocation. It reduces fragment work by removing production Motion Lane, Obstacle Routing, and Obstacle Exclusion reads that existed only for residual prediction.

Both accepted visibility policies remain available. Supported Aging Rate remains `0.05–1.00`, and lifecycle aging remains one complete material-tick decrement on the final CFL substep. The grey-body/white-border appearance exists in committed and evaluated presentation alike and is therefore recorded as a later Layer E shader-composition issue, not a transport or lifecycle fault.

Status: `5.16E.2` is Unity-validated and accepted. Normal Final Foam matches the former committed preview, the rock/bank stutter is gone in both visibility policies, `Foam Evaluated Final Preview` remains available, and serialized debug value `16` resolves safely to Final Foam. `5.16E.3` has since attributed the remaining transport capacity loss, and `5.16E.3C` records the consciously deferred sub-1% PoC limitation without changing the solver.

The River Inspector and diagnostics redesign R1–R5 is also Unity-validated and accepted. It changes only Editor organization and presentation: all sections are collapsed by default, authoring follows feature ownership and Foam Layers A–E, one exclusive debug hub controls the existing serialized debug fields, runtime telemetry is read-only and stable-height, mutating tools live under Actions, and constant repaint is limited to visible live diagnostic leaves.

`4.11C.5.17A.1 — Interior Composition Authority Correction` is accepted. `5.17B` and `5.17B.1` are rejected; `5.17B.2` established usable pre-hardening authority and `5.17B.2C` provides state-preserving Inspector tuning and Hold Foam State. Same-state evidence rejected `5.17B.2B`, the periodic `5.17B.2A` Strand path, reconstructed D1, and the D1B/D1C shaping models. `5.17B.2D1A` proved exact lineification extraction. `5.17B.2D1D` is Unity-validated and accepted: Strength, Scale, Density, and Reach now produce viable Strands without excessive visual artefacting. Unity visually rejected `5.17B.2D2`: although it changed patterns and scalar thresholds, Chip and Fray still cut iso-contours from a procedurally eroded visibility field and therefore remained visually similar to the former lineification behavior. `5.17B.2D2A` is partially useful but not accepted. Unity evidence shows Chip is more recognizably chip-like, but still needs a stronger medium-to-large bias; Fray remains visually ineffective, and Chip/Fray/Strands still compete for substantially the same weak-presence territory. It preserves accepted D1D Strands and reconstructs Chip/Fray from a transient material-edge-depth field derived from the pre-morphology base material footprint.


## Approved Layer E finishing contract — `4.11C.5.17P`

The inspiration comparison is refreshed before final rendering work. The production river is not expected to copy the reference one-to-one. Its accepted macro result already contains the required family resemblance: broad predominantly horizontal bands, lateral travel, split/merge behavior, obstacle-driven convergence, and stronger shore accumulation. Slightly fatter ribbons and greater bank accumulation are acceptable consequences of the current field resolution and source grammar. Layer C and the existing motion system remain the macro authority.

The remaining target is local rendered character: foam should read as pale, substantially opaque, chipped, frayed, and energetic while preserving the accepted macro ribbons.

The approved Layer E order is:

```text
committed Layer C material and selected visibility policy
  -> local rendered morphology
  -> interior colour / opacity composition
  -> post-breakup edge contrast
  -> final water lighting, fog, and reflection/refraction composition
```

Layer E reads upstream data and writes screen pixels only. It adds no persistent damage state, transported channel, compute pass, or feedback into Layers C or D.

### `5.17A / 5.17A.1 — Layer E Interior Composition`

`5.17A` failed visual validation. The controls were bound correctly, but `Interior Fill Strength` operated after the visible mask had already been hardened near full coverage, `Interior Opacity Floor` was incorrectly capped by `Foam Colour` alpha, and `Edge Emphasis Strength` added only a weak second whitening treatment instead of controlling the existing rim produced by the edge-versus-interior lighting transition.

`4.11C.5.17A.1 — Interior Composition Authority Correction` replaces that failed control model. Its authoring surface is:

```text
Foam Colour                 base RGB/tint and base opacity before the interior floor
Interior Opacity Floor      absolute minimum opacity for established Foam body
Edge Contrast               signed control over the existing edge-versus-interior lighting contrast
```

`Interior Fill Strength` is removed. The current visibility path already supplies a hardened established body, so a second fill remap had no useful authority and risked duplicating later morphology. `Interior Opacity Floor` may now exceed `Foam Colour` alpha, but it is gated by `smoothstep(0.42, 0.82, incoming mask)` and therefore cannot create Foam in weak fringe or outside the incoming silhouette. `Edge Contrast` ranges from `-1` to `+1`: negative values suppress the existing bright rim toward filtered interior lighting, zero preserves the accepted pre-5.17A lighting exactly, and positive values intensify the existing edge.

Production Final Foam and Foam Evaluated Final Preview share the corrected composition helper. Neutral values are:

```text
Interior Opacity Floor = 0
Edge Contrast           = 0
```

At neutral values, opacity reduces exactly to `smoothstep(0.08, 0.46, mask) × Foam Colour alpha` and lighting reduces exactly to the pre-5.17A edge/interior transition. The correction adds arithmetic only: no texture sample, branch, loop, neighbourhood stencil, persistent resource, compute dispatch, readback, material-state change, or silhouette expansion.

### `5.17B / 5.17B.1 — Rejected Hardened-Mask Breakup`

`4.11C.5.17B` and `4.11C.5.17B.1` are both visually rejected. The decisive blind comparison used Breakup Scale `0` with Chip/Fray `0` versus `1`; the difference was weak enough that the images were identified backwards. Stronger threshold constants did not solve the ownership error.

Both patches applied breakup after the visibility signal had already been hardened:

```hlsl
float hardVisible = smoothstep(0.22, 0.58, softVisibility);
float fringe = smoothstep(0.06, 0.34, softVisibility) * 0.34;
float hardenedMask = saturate(max(hardVisible, fringe));
```

Most visible body pixels therefore entered the breakup helper near `1.0`. The old equations altered mainly the narrow antialiased transition, while `5.17A.1` Interior Opacity Floor concealed partial erosion that did not reach zero. Breakup Scale also weakened the result at broader settings because the broad/diagonal composite had a compressed centre-weighted distribution. No further hardened-mask threshold recalibration is allowed.

### Historical `5.17B.2 — Pre-Hardening Binary Edge Cuts`

> Superseded for active authoring and future implementation by the D2A findings and `2D2B-A` control-divorce/selection-diagnostic contract above. The text below records what that historical patch did; it is not the current Chip/Fray plan.

Historical status: Unity-validated for visible authority at that time. Their neutral-versus-maximum comparison is unmistakable and both controls produce the intended zero-coverage bites and serration. Breakup Scale visibly modifies the result and is provisionally accepted with the rest of this feature family; no separate Scale-coherence correction blocks current work.

The public authoring surface is unchanged:

```text
Chip Strength   0–1; default 0
Fray Strength   0–1; default 0
Breakup Scale   0–1; default 0.5
```

`RiverWaterFoamPatternedMask` now preserves its continuous pre-hardening `softVisibility` transiently while leaving the accepted hardening equation numerically unchanged. `RiverWaterFoamResult` carries that scalar through the existing visual warp, stretch, surface-break, stored-retention, and freeze coupling beside the existing hardened mask. Layer E then performs antialiased binary survival tests against `softVisibility` and multiplies the result into the hardened mask. Selected chips and short cuts may therefore reach true zero coverage; fray uses a shallower threshold. Exact saturated soft cores remain protected. The post-breakup result is removal-only and always satisfies `postBreakupMask <= hardenedMask`, so Interior Opacity Floor cannot refill a removed pixel.

The stable pattern path still evaluates exactly the same broad, diagonal, mid, and fine noise calls. Only the transient Chip/Fray outputs are normalized before Scale interpolation: mid and broad chip fields receive separate contrast normalization, as do fine and mid fray fields. Static distribution analysis shows the selected-field means remain effectively matched across Scale endpoints, so Scale changes feature size/frequency without silently collapsing authority. The accepted combined visibility pattern is unchanged.

Production Final Foam, Foam Evaluated Final Preview, Foam Shader Detail Probe, and Foam Shader Detail Difference continue to use the same breakup helper. The Probe shows the exact production post-breakup silhouette. Difference remains removal-only: black is unchanged and magenta/red is removed coverage; green remains zero. The evaluated preview supplies its evaluated shape to the same binary helper without promoting Layer D to production.

Neutral Chip and Fray values return the exact accepted hardened mask, and Breakup Scale alone does nothing. The fixed proof still reads no Remaining Life, Support, Negative Topology, surface-energy multiplier, river-location multiplier, or additional time input. It adds no texture sample, procedural-noise call, texture, buffer, persistent field, compute kernel, dispatch, readback, shader property, or C# binding. Incremental cost is fragment arithmetic plus one transient scalar and possible register pressure.

Unity validation passed the authority requirement: Chip and Fray are clearly visible at maximum strength and behave as described. The original short-cut contribution is separated into optional `5.17B.2A` Foam Strands. Breakup Scale visibly modifies the accepted Chip/Fray result and is provisionally accepted; no separate coherence patch is active.


### Superseded `5.17B.2A` / rejected `5.17B.2B`

The periodic-lane Strand implementation and the partial-presence Edge Fragmentation model are no longer part of the active architecture. The Strand control group is retained, but D1 remaps it to the actual hidden Chip/Fray lineification family. The Fragmentation controls and shader properties are removed.

### `5.17B.2C — State-Preserving Foam Authoring`

Status: Unity-validated and accepted.

The River Inspector no longer treats every serialized edit as a structural river change. `OnValidate()` still clamps settings, keeps required outputs/runtimes present, and applies live material values, but it no longer queues `RegenerateAll()`. The custom Inspector now requests the existing debounced full rebuild only when one of the structural authoring sections changes:

```text
Setup
River Domain
Channel Shape
Shoreline Safety
Natural Variation
Surface Mesh
```

Spline edits remain structural through the existing spline-change callback. Water rendering, surface motion, refraction, runtime-disturbance tuning, Foam Layers A–E, debug selection, and diagnostics no longer rebuild the River domain merely because a value changed. Layer E rendering values continue through the existing per-frame property binding, so they preserve the active Layer C textures.

`Runtime & Quality` also exposes the non-persistent Play Mode diagnostic **Hold Foam State**. While held, the runtime preserves the allocated Layer C material and existing Layer D products, skips topology evolution, births, aging, transport, and Layer D temporal advancement, discards elapsed wall time, and continues binding the current textures plus live Layer E properties. Pending manual/automatic work remains queued and resumes without catch-up when the hold is released. The toggle is not serialized authoring data and resets when the component is enabled or disabled.

This patch adds no texture, buffer, shader property, compute kernel, dispatch, readback, or persistent simulation field. Structural or resource-allocation changes may still legitimately rebuild and clear state; Hold Foam State is for same-domain rendering comparisons, not for preserving material across incompatible domain changes.

### `5.17B.2D1 / D1A — Lineification Extraction`

`5.17B.2D1` is visually rejected. It reconstructed Strands from a coherent-to-lineified delta, a fixed canonical Chip/Fray pattern pair, broad group suppression, and a derivative threshold over the already-cut removal mask. Unity close-ups showed that this was not the current Chip-plus-Fray morphology: it produced short screen-aligned dashes, square steps, discontinuous groups, and severe pixel quantisation. The failed reconstruction and its transient Strand field are removed.

`4.11C.5.17B.2D1A — Exact Lineification Extraction` is Unity-validated for exact same-state Chip-plus-Fray versus Strand equivalence. The neutral body and separate stable lineified soft signal from D1 are retained. Current Chip and Fray remain unchanged as the reference. Strand Strength now literally calls the same current Chip and Fray survival helpers with:

```text
same lineified soft visibility
same Breakup-Scale-selected Chip pattern
same Breakup-Scale-selected Fray pattern
same fwidth antialiasing footprint
same exact-core protection
```

Therefore, at the same Breakup Scale, these configurations are intended to be mathematically equivalent:

```text
A: Chip Strength = 1, Fray Strength = 1, Strand Strength = 0
B: Chip Strength = 0, Fray Strength = 0, Strand Strength = 1
```

The proof deliberately adds no spacing reinterpretation, width reinterpretation, curvature warp, grouping mask, delta reconstruction, or post-threshold screen-space culling. Strand Spacing, Strand Width, and Strand Curvature remain serialized for the later shaping step but are visibly disabled in the Inspector and have no shader authority during D1A. Production Final Foam and Foam Evaluated Final Preview use the same exact helper.

D1A removes the failed four-component transient Strand field and its propagation through stored, warped, lead, and trail evaluations. It adds no texture sample, procedural-noise evaluation, hash evaluation, texture, buffer, persistent field, compute kernel, dispatch, readback, Layer C mutation, or Layer D mutation. Unity same-state validation confirmed exact visual equivalence.

### `5.17B.2D1B — Strand Shaping and Projected Detail Floor`

`4.11C.5.17B.2D1B` is visually rejected as a Strand-control solution. It deliberately combines the shaping controls with artifact suppression because both operate on the same continuous source family:

```text
Strand Spacing
  biases continuous Chip/Fray selection density;
  higher values retain fewer, more separated structures.

Strand Width
  changes continuous selection breadth/depth;
  reference 0.50 preserves the D1A threshold model at resolved distances.

Strand Curvature
  broad-modulates the existing stable anisotropic band field;
  reference 0.55 preserves the D1A source and no time/seed replacement is introduced.
```

The shader now carries a dedicated transient `strandSoftVisibility` beside the coherent and legacy lineified signals. At the serialized reference values (`Spacing 0.55`, `Width 0.50`, `Curvature 0.55`) and when source bands are resolved, the Strand path reduces to D1A. Projected river-space footprint is measured once before wake/lee branching. The Strand-only pattern pair then falls back continuously from fine to medium and from medium to broad existing bands as source density becomes unresolved; the anisotropic band breaker similarly falls back to the existing broad/diagonal field. No finished removal mask is re-thresholded, grouped, dithered, or culled per pixel.

D1B adds no texture sample, procedural-noise/hash call, texture, buffer, persistent field, compute kernel, dispatch, readback, Layer C mutation, or Layer D mutation. It adds transient arithmetic/register pressure only. Current Chip and Fray remain unchanged until D1B proves that controlled Strands retain the desired visual family without distant square/dash noise.


### `5.17B.2D1C — Strand Spatial Controls and Resolution Cutoff`

`4.11C.5.17B.2D1C` is visually rejected. Static mathematics assigned distinct coordinate-frequency, band-width, and coordinate-warp operations, but Unity evidence showed that all three controls were effectively inert. The audit proved why: the visible extracted lineification is an intersection of the anisotropic soft band with separate Chip and Fray candidate patterns. D1C shaped only the anisotropic band while the decisive candidate topology remained fixed. Its projected-detail estimate also omitted the transported Material Pattern phase that participates in every procedural coordinate. No further constant calibration is allowed on that control model.

### `5.17B.2D1D — Strand Control Model Reset and Coherent Pattern Transport`

`4.11C.5.17B.2D1D` is Unity-validated and accepted. Its Strength, Scale, Density, and Reach controls can produce viable controlled lineification without excessive visual artefacting. This accepted Strand path is the reference that D2 must preserve.

The independent Strand authoring surface is now:

```text
Strand Strength
  overall Strand contribution;

Strand Scale
  hierarchical broad/medium/fine source ownership;
  zero retains finer subdivision, one keeps broader structure;

Strand Density
  candidate selection prevalence only;
  zero is sparse, one is dense;

Strand Reach
  anisotropic attenuation, presence eligibility, and cut depth only;
  zero remains shallow, one permits deeper weak-to-medium-body channels.
```

The old serialized Spacing, Width, and Curvature values migrate into Scale, Density, and Reach through `FormerlySerializedAs`; the obsolete names are removed from bindings, shader properties, Inspector labels, and active documentation.

D1D gives Strands a dedicated candidate-pattern pair without modifying the legacy Chip/Fray pair. Strand Scale builds that pair hierarchically from the existing broad, medium, and fine bands. Fine and medium contribution disappear first as their projected footprint becomes unresolved; if even the broad organization is unresolved, Strand authority returns to the coherent Foam body. Candidate thresholds receive derivative-aware antialiasing.

Projected resolution now includes the transported Material Pattern derivative multiplied by the same seed factors used by the broad, diagonal, medium, fine, and anisotropic sources. The derivative is resolved outside wake/lee branching. Stored, warped, lead, and trail Strand patterns and resolution authority are transported with the soft shape that owns them; `max` paths choose the winning sample's pattern rather than applying the stored pattern to a different visible shape. At D1D time, the legacy Chip/Fray `breakupField` remained stored-authority; D.0 later removed that dead carrier after Fray retirement.

D1D adds no texture sample, texture, buffer, persistent field, compute kernel, dispatch, readback, Layer C mutation, or Layer D mutation. It removes the extra D1C shaped-coordinate noise call and replaces it with arithmetic, transient Strand pattern/resolution values, and derivative-aware candidate selection.

### `5.17B.2D2 — Rejected Visibility-Contour Role Separation`

Unity visually rejected D2. The implementation moved Chip and Fray away from the explicit Strand signal, but still thresholded `coherentSoftVisibility`. That scalar contains procedural noise, time-driven morphology, wake/stretch composition, and internal valleys; it is not monotonic distance from the Foam silhouette. Spatially varying thresholds therefore exposed nested iso-contours and elongated channels rather than edge-connected bites and shallow perimeter roughness. Sequential multiplication did not create geometric boundary awareness. No further threshold calibration of that model is allowed.

### `5.17B.2D2A — Presence-Space Chip and Fray Reconstruction`

`4.11C.5.17B.2D2A` is implemented with Unity validation pending. It keeps the public controls unchanged and preserves the accepted D1D Strand source and survival equations.

`RiverWaterFoamPatternedMask` now exposes a transient `materialEdgeDepth` sourced from `baseMask`, before procedural coherent morphology, anisotropic lineification, surface-break modulation, and final hardening. That depth travels with the same stored, warped, lead, trail, and stored-retention coherent-shape owner as the Chip/Fray pattern pair. It adds no texture, buffer, persistent channel, compute work, dispatch, or readback.

Chip and Fray now build one material-depth requirement:

```text
Chip
  broad/medium candidate pattern
  medium inward material-depth requirement

Fray
  medium/fine candidate pattern
  shallow perturbation added to the same requirement
```

The combined requirement is compared against `materialEdgeDepth`. Fully established material remains protected. The resulting edge keep is applied to coherent soft visibility before the accepted hardening equation is reconstructed. A reconstructed-mask ratio preserves the existing coupled/wake-shaped production mask exactly when Chip and Fray are neutral while applying only removal when they are active. Accepted Strands remain a final independent removal stage.

D2A must be rejected if Chip still forms long internal contours, Fray still removes medium body regions, or Strand-only output changes. If the presence-space model fails, the no-new-sample scalar route is exhausted and the next escalation requires neighbour-derived edge geometry or a small edge-distance resource.

### Lifetime and topology rule

Direct Support or Negative Topology breakup multipliers are explicitly excluded from `5.17A` and the first `5.17B` proof. Layer C already converts topology into Remaining Life through the configured aging rates. Supported foam therefore remains visually younger for much longer, neutral foam follows normal lifetime, and negative foam expires rapidly. Current proof tuning is Neutral Lifetime `7.5 s`, Supported Aging Rate `0.08×`, and Negative Aging Rate `7.5×`; these values describe the validation setup and are not silently promoted to new project defaults. The first `5.17B` proof does not read Remaining Life at all; it isolates the fixed morphology vocabulary before temporal progression is introduced. `5.17C` will then use Remaining Life as the initial and sole temporal fragility signal, allowing the existing lifecycle system to prove whether it supplies enough differentiation without hidden additional help.

Do not sample support/negative topology directly for breakup unless later Unity evidence shows that Remaining Life alone is insufficient and the user separately approves that coupling. Layer E must never modify Remaining Life.

### Later polish

`5.17C` is planned Remaining-Life progression for the accepted deterioration vocabulary: chips, fray, extracted strands, detached flecks, broken streak remnants, and sparse old-foam remnants should advance as Layer C Remaining Life falls. Supported and Negative Topology must initially influence that progression only through their existing Layer C aging rates; direct topology, support, negative-pressure, or river-location breakup multipliers remain deferred unless Remaining Life demonstrably proves insufficient. `5.17D` is planned fine-fragment and final-energy work rather than an optional bucket to omit: tiny detached flecks, small fragments, thin streak remnants, micro-bubbles, and selective bright glints are part of the intended final Foam pass. Their underlying Foam availability and deterioration follow the lifecycle-derived mask; optical glints may additionally respond to lighting.

### Performance contract

```text
new persistent textures / fields / channels = 0
new compute kernels / dispatches / readbacks = 0
cost location = fragment shader only
wide neighbourhood sampling = rejected by default
```

Reuse the existing shader-detail probe and available samples where practical. Profile before accepting any broad sampling stencil.


---

# 0. Non-negotiable design goals

## 0.1 Visual target decomposition

The reference river is not one effect. It is a stack of visual phenomena:

1. **Broad pale surface film**
   Large white/pale sheets sit on the water surface. They read as continuous film rather than as discrete particles.

2. **Connected ribbons and current seams**
   Foam forms long broken bands along flow lanes, banks, rocks, and darker water pockets.

3. **Emergent split / merge / pinch / reunite appearance**
   Visible film narrows, separates, rejoins, and creates temporary necks through material advection and temporal sheet evolution. A separate persistent macro-fracture state is explicitly rejected.

4. **Chipping and edge chaos**
   Edges chip, fray, crack, and flutter. This can be largely procedural and local.

5. **Thin bright streaks**
   Narrow fast white scratches/streaks in the reference are not the same layer as broad film. They should be shader-side detail or a separate lightweight detail layer.

6. **Bank / rock / obstacle contact foam**
   Pale film gathers around banks and obstacles. This is not merely transported material; the visual system needs external contact/support fields.

## 0.2 Performance target

The solution must remain viable for desktop PC first, including low-to-medium hardware. Mobile is not a target, but the game must not rely on high-end GPU headroom.

The architecture must scale with:

```text
river field cells × update rate × active visible river chunks
```

not with:

```text
number of foam islands / pockets / entities
```

and not primarily with:

```text
screen pixels × frame rate × wide neighbourhood shader samples
```

## 0.3 Data authority rule

Every data product has exactly one writer.

```text
Layer A writes/owns River Domain data.
Layer B writes/owns External Influence Fields.
Layer C writes/owns Persistent Foam Material.
Layer D writes/owns Visual Foam/Film products such as _FoamShapeMask.
Layer E writes only final rendered pixels.
Layer F schedules/binds/debugs; it does not own foam behavior.
```

If Foam looks wrong, diagnose the owner of the wrong product instead of adding another hidden authority.

## 0.4 No circular dependencies

The dependency graph must be acyclic.

Allowed flow:

```text
Layer A — River Domain
        ↓
Layer B — External Influence Fields
        ↓
Layer C — Persistent Foam Material
        ↓
Layer D — Visual Foam / Film Evaluation
        ↓
Layer E — Shader Composition
```

Layer A may also be read directly by B, C, D, and E. Layer B may be read by C, D, and E. Layer C may be read by D and E. Layer D may be read by E.

Forbidden flow:

```text
Layer D → Layer C
Layer D → Layer B
Layer C → Layer B
Layer E → any compute/simulation layer
Layer E → Layer C
Layer E → Layer D
```

Foam-derived visual helper fields belong inside Layer D. They are not External Influence Fields and must never feed Layer C.

## 0.5 No entity database by default

Do not introduce foam pocket IDs, connected-component tracking, per-pocket state, or a foam island database unless field methods fail and the user explicitly approves that architectural pivot.

The default solution is field math:

```text
for each cell/pixel, compute output from upstream fields and local/limited-neighbour information
```

not:

```text
for each foam island, track identity, split history, merge history, velocity, and shape state
```

---

# 1. Canonical layer stack

The previous three-stage summary remains useful, but the complete architecture is six named layers. The letters are intentional: do not use `Stage 1.5` because it implies an arbitrary half-stage and caused confusion.

```text
Layer A — River Domain
Layer B — External Influence Fields
Layer C — Persistent Foam Material
Layer D — Visual Foam / Film Evaluation
Layer E — Shader Composition
Layer F — Scheduling, Quality, Debug
```

The condensed user-facing summary is:

```text
Stage 1 = Layer C: persistent material simulation.
Stage 2 = Layer D: visual film/shape compute evaluation.
Stage 3 = Layer E: shader composition and local polish.
```

Layer A and Layer B are upstream foundations. Layer F is orchestration.

---

# 2. Layer A — River Domain

## 2.1 Abstract responsibility

Layer A defines the coordinate system and river-space truth.

It answers:

```text
Where is the river?
What direction is downstream?
What is across-river coordinate here?
Which cells are valid water?
Where are banks and boundaries?
How do world/surface pixels map into foam/material textures?
```

Layer A does not know or care whether Foam exists.

## 2.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/RiverDomainSnapshot.cs
Assets/Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Important current symbols/functions include:

```text
RiverDomainSnapshot
BuildSharedSplineSamples(...)
_FoamDimensions
_FoamValidLength
_FoamSimulationLength
_FoamGlobalStart
_FoamFieldLength
_FoamBoundary
_FoamCurrentShoreEdgesRead / _FoamCurrentShoreEdgesWrite
FoamValidFluidAt(int2 coordinate)
LoadBoundaryCoverage(...)
SampleBoundaryCoverageBilinear(...)
FoamUVToTexelCoordinate(...)
RiverWaterFoamResult.materialUV
```

## 2.3 Owned data

Layer A owns or defines:

```text
river-space coordinate convention
full-resolution foam grid dimensions
valid/simulation length
boundary/coverage texture
shore edge texture
river/global start offset
field length
material-space UV mapping
flow direction conventions
```

## 2.4 Allowed reads

Layer A may read:

```text
river spline/domain data
river width and shape settings
corridor/water mesh settings
terrain/corridor geometry where required for masks
```

Layer A must not read:

```text
Persistent Foam State
_FoamShapeMask
Stage D visual helper textures
shader output
```

## 2.5 Writers and consumers

Layer A writes domain data. It may be consumed by:

```text
Layer B — to place external influence fields in river space.
Layer C — to transport and clip persistent material.
Layer D — to evaluate visible shape in the same coordinate system.
Layer E — to sample foam and render debug/final output.
```

## 2.6 Connectivity invariant

All directional concepts must use this layer's coordinate basis.

If one field says “left,” “right,” “upstream,” “downstream,” “across,” or “cell,” it must mean the same thing to every consumer. Any disagreement here is a Layer A bug, not a Foam artistic issue.

---

# 3. Layer B — External Influence Fields

## 3.1 Abstract responsibility

Layer B contains foam-agnostic environmental influence. It answers:

```text
Where is foam encouraged?
Where is foam suppressed?
Where is material or visual film shaped by banks, rocks, wakes, pressure, or motion intent?
```

Layer B does not mean “foam exists here.” It means the environment provides a reason for foam to be born, preserved, suppressed, bent, visually supported, or locally agitated.

## 3.2 Critical correction

Layer B must not read Foam data.

This is the correction to the earlier ambiguous `Stage 1.5` wording.

Allowed:

```text
River Domain → External Influence Fields → Persistent Foam Material
River Domain → External Influence Fields → Visual Foam/Film Evaluation
```

Forbidden:

```text
Persistent Foam Material → External Influence Fields → Persistent Foam Material
Visual Foam/Film → External Influence Fields
```

Foam-derived sheet-support fields are Layer D internal fields, not Layer B fields.

## 3.3 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs
Assets/Game/Procedural/Rivers/FoamTopology/*
Assets/Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.*.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Support.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Evolution.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.TopologyTransition.hlsl
```

Important current texture/symbol names include:

```text
_FoamTopologyRead / _FoamTopologyWrite
_FoamTopologySourcesRead / _FoamTopologySourcesWrite
_FoamTopologyGeneratedRead
_FoamTopologyTransitionFromRead
_FoamObstacleExclusionRead / _FoamObstacleExclusionWrite
_FoamMotionLaneRead
_FoamObstacleRoutingRead
_FoamRippleField
_FoamWakeField
_FoamStaticWakeField
_FoamStaticPressureField
_FoamEvolvingMajorRead / Write
_FoamEvolvingHostedNegativeRead / Write
_FoamEvolvingFreeWaterNegativeRead / Write
_FoamEvolvingConnectorRead / Write
_FoamCurrentShoreEdgesRead / Write
FoamResolveMotionFieldSample(...)
FoamLoadObstacleRoutingCell(...)
FoamSampleMotionLaneSmooth(...)
FoamValidFluidAt(...)
```

## 3.4 Owned data

Layer B owns external influence textures and fields, including current or future versions of:

```text
valid-fluid support context
obstacle/solid exclusion
positive support/topology
negative/free-water suppression or aging pressure
bank/shore/contact support
rock/contact support
wake support and lee context
pressure/ripple disturbance influence
motion/lateral intent
birth support
lifetime support
visual contact support
```

## 3.5 Allowed reads

Layer B may read:

```text
Layer A river domain and coordinate data
rocks/obstacles/banks/collider-derived river interactions
static disturbance emitters
dynamic disturbance emitters
time
its own previous influence texture when a field is intentionally persistent, such as wake decay
```

Layer B must not read:

```text
_FoamStateRead
_FoamStateWrite
_FoamShapeMask
Layer D visual helper textures
Final shader output
```

## 3.6 Writers and consumers

Layer B writes External Influence Fields.

Layer B may be consumed by:

```text
Layer C — birth, lifetime, exclusion, future real material transport.
Layer D — visual support, visual deformation, bridge/pinch context, contact film.
Layer E — debug or local polish if useful.
```

## 3.7 Resolver requirement

Layer B should resolve raw contradictory inputs before consumers read them.

Do not expose several raw fields that can mean different directions to different layers. Instead, raw inputs should be combined into canonical resolved fields with fixed meanings.

Raw inputs may include:

```text
bank contact
rock contact
obstacle exclusion
lane motion
obstacle routing
pressure/wake/ripple
negative zones
dynamic emitter influence
```

Canonical resolved outputs should eventually include explicit meanings such as:

```text
birthSupport
lifetimeSupport
exclusion
resolvedFoamVelocity
visualContactSupport
breakupAgitation
```

If two raw influences conflict, Layer B resolves the conflict once. Layer C and Layer D then read the same resolved intent instead of inventing separate interpretations.

## 3.8 Connectivity invariant

Layer B is upstream context. It is not Foam state.

Correct language:

```text
The motion field says the environment would prefer visual/material lateral influence here.
The obstacle routing field says this area has local obstacle-driven routing intent.
The support field says foam should survive or appear more strongly here.
```

Incorrect language:

```text
The motion field moved this foam cell.
The support field created persistent foam by itself.
The visual sheet field caused Stage 1 material to merge.
```

Only Layer C can move persistent material. Only Layer D can create visual-only broad film shape.

## 3.9 Unified Foam velocity contract

Patch `4.11C.5.16A` makes resolved Foam velocity the canonical Layer B motion output.

Current raw inputs:

```text
Motion Lane texture: signed lateral route preference; sampled at a physically advected X phase.
Obstacle Routing texture R: signed route-around-obstacle preference; fixed in river space.
Obstacle Routing texture G: obstacle influence / collision-shadow strength; fixed in river space.
River Flow Speed × Liquid Factor × Downstream Speed Ratio: base Foam speed.
```

Canonical pure resolver:

```hlsl
lateralIntent = clamp(
    lerp(laneIntent, obstacleIntent, obstacleInfluence),
    -1,
    1);

slowdown = saturate(
    obstacleInfluence * ObstacleSlowdownStrength);

downstreamFactor = lerp(
    1,
    ObstacleMinimumDownstreamFactor,
    slowdown);

vDownstream = max(0, baseFoamSpeed * downstreamFactor);
vLateral = lateralIntent * baseFoamSpeed * MaximumLateralSpeedRatio;
```

Invariant:

```text
vDownstream >= 0
```

Therefore the final movement system may move material left or right, slow it, or temporarily stop downstream movement, but it may never move material upstream.

The shared implementation lives in:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl
```

Compute raw-field sampling lives in:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
```

The existing Motion Field debug modes use the same pure contract. Hue encodes route meaning: neutral gray is straight motion, red/blue encode signed lateral velocity, and yellow indicates obstacle influence. Brightness is applied only after hue composition and represents `downstreamSpeedFactor`: bright is full-speed, dark is slowed, and near-black is near-stagnation. White uses the shared meaningful-Presence visibility gate derived from committed material. Raw Material Presence remains available in its dedicated amplitude-faithful view.

Motion Lane authoring now has independent shape controls:

```text
Direction Change Frequency:
  controls how often left/right intent changes sign downstream;

Across-River Coherence:
  controls how broadly neighbouring lateral rows share an instruction;

Low Lateral Motion Coverage:
  compresses a selected fraction of the field toward low lateral magnitude;

Lane Advection Ratio:
  controls how quickly the authored route pattern moves downstream in sample space.
```

At defaults `Direction Change Frequency = 1` and `Across-River Coherence = 1`, the generated field preserves the pre-split baseline. Higher direction frequency changes every downstream octave, breaker, cross-cut, and warp frequency without increasing across-river frequency. Higher coherence lowers across-river noise frequency while the existing two-pass across-width smoothing remains the final anti-checkerboard guarantee.

Lane phase is now advanced in physical metres:

```text
laneScrollMetres = baseFoamSpeed × LaneAdvectionRatio × deltaTime
laneScrollCells = laneScrollMetres / longitudinalCellSpacing
```

The old wraps-per-second formula, whose physical speed scaled with total river length, is retired.

---

# 4. Layer C — Persistent Foam Material

## 4.1 Abstract responsibility

Layer C is the durable Foam simulation.

It answers:

```text
Where does actual foam material exist?
How old is it?
What stable material pattern does it carry?
How does it move downstream?
Where is it born, preserved, clipped, or killed?
```

Layer C is the only writer of Persistent Foam State.

## 4.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl
```

Important current kernels include:

```text
InjectFoam
CommitPhaseTransport
SimulateFoam
ApplyBoundary
ClearRange
```

Important current texture/symbol names include:

```text
_FoamStateRead
_FoamStateWrite
currentState
previousState
writeState
DispatchPhaseCommit(...)
DispatchSimulationRange(...)
DispatchApplyBoundary(...)
FoamDecodeMaterialState(...)
FoamEncodeMaterialState(...)
FoamMergeBornPresence(...)
FoamClipPackedToValidFluid(...)
FoamApplyPersistentMaterialMorph(...)
```

## 4.3 Persistent state packing

Canonical packed state:

```text
R = Presence
G = Presence × normalized Remaining Life
B = Presence × normalized Material Pattern
A = reserved / future use
```

Decoded state:

```hlsl
struct FoamMaterialState
{
    float presence;
    float remainingLife;
    float materialPattern;
};
```

## 4.4 Meaning of fields

### Presence

Persistent material coverage in a foam simulation cell.

It is not:

```text
final opacity
visual support
topology pressure
shader streak strength
shape-mask brightness
```

### Remaining Life

The actual durable survival clock.

Only Layer C may change it.

Layer D and Layer E may read it as metadata for visual fragility, but they must not write or reinterpret it as a visual mask owner.

### Material Pattern

Stable material identity/pattern data that travels with persistent foam.

Layer D and Layer E may use it for deterministic procedural variation.

## 4.5 Allowed reads

Layer C may read:

```text
previous Persistent Foam State
Layer A River Domain data
Layer B External Influence Fields
time/update delta
source injection/birth events
```

Layer C must not read:

```text
_FoamShapeMask
Layer D visual helper textures
shader local noise result
final rendered pixels
```

## 4.6 Owned behavior

Layer C owns:

```text
foam birth/source-to-persistent merge
persistent presence
downstream material transport
future real lateral material transport if approved
future real obstacle-guided material transport if approved
Remaining Life aging
support/negative aging response
valid-fluid clipping
obstacle/solid exclusion clipping
real material merge rules if added later
```

Layer C must not own:

```text
temporary visual chipping
temporary visual bending
temporary visual bridge/pinch behavior
shader-local streaks
final color/opacity
large hidden neighbour-sampled morphology that writes back to FoamState
```

## 4.7 Source population contract

Source population is Layer C birth preparation. It may read Layer A/B context, choose where real material should be born, and queue birth through the same persistent material injection path used by manual sources.

Source population must obey this rule:

```text
support/context may choose birth candidates;
only Layer C birth creates material;
support/context must not render as foam by itself.
```

The intended route for shore, rock, wake, and current-seam foam is therefore:

```text
Layer B environmental support/contact/wake context
  -> Layer C automatic source population creates real FoamState material
  -> Layer C support/negative aging captures or kills that material
  -> Layer D derives broad visual film from the material
  -> Layer E adds pixel-scale breakup/streaks/polish
```

This replaces the earlier temptation to add a separate visual-only environmental film authority. Such a visual-only product is postponed and should not be introduced until source population has been tested and found insufficient.

Patch `4.11C.5.14A` added the first automatic source class: conservative shore/contact birth. Validation proved the plumbing but showed the first control design was too crude. Patches `5.14B–5.14H` established source-class-specific authoring and a dedicated typed source-event rasterizer. `5.15A–5.15A.4` added static object/contact arcs, semi-arcs, and flecks. `5.15B–5.15B.3.1` added free-water lace connectors, cross-lace connectors, and progressively revealed torn fragments. These source families are not final-quality, but they now provide sufficiently varied real `FoamState` material to unblock evolution work. Spawning is parked unless a concrete regression blocks evolution validation.

## 4.8 Current status

Active/trusted:

```text
manual/source birth
automatic shore birth
automatic static object/contact birth
automatic free-water lace/cross-lace/fragment birth
source-to-persistent merge
conservative local 2D Layer C material transport
lifecycle aging
support/negative aging influence
valid-fluid and obstacle clipping
unified physical Foam velocity consumed by Layer C and Layer D
half-resolution advected Layer D temporal occupancy
```

Rejected/superseded:

```text
global downstream phase transport
persistent stored-state morph as visual breakup
fractional lateral row weighting
per-cell stochastic lateral row commit
hidden neighbour-resampling morphology that writes persistent state
```

Actual lateral material transport is active in Layer C. Temporal occupancy may move visual sheet structure in Layer D, but it remains a separate visual product and must never be described as additional durable material.

## 4.9 Connectivity invariant

Layer C is the only layer that can truthfully say:

```text
actual foam material moved
actual foam material was born
actual foam material died
actual Remaining Life changed
```

If Layer D displays foam slightly offset from the material, that is visual interpretation only. It must never be described as actual material movement.

---

# 5. Layer D — Visual Foam / Film Evaluation

## 5.1 Abstract responsibility

Layer D is runtime GPU compute that derives visible broad foam/film shape from upstream data.

It answers:

```text
Given persistent foam material plus river/support/motion context,
what should the broad visible foam film look like right now?
```

Layer D is the only writer of Evaluated Foam Shape products.

## 5.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
```

Current important symbols:

```text
EvaluateFoamShape kernel
DispatchEvaluateShape()
evaluateShapeKernel
shapeMaskTexture
_FoamShapeMaskWrite
_FoamShapeMask
FoamEvaluatedShape = 7
FoamShapeDifference = 8
IsShapeProductDebugActive
FoamEvaluateIntrinsicShapeMask(...)
BindField()
```

## 5.3 Current implementation state after 4.11C.5.10B

The current code has a Layer D output slot, `_FoamShapeMask`, and two Layer D debug views:

```text
Foam Evaluated Shape = displays _FoamShapeMask directly.
Foam Shape Difference = compares _FoamShapeMask against raw persistent Presence.
```

After validation of 5.10, the 5.9z coordinate-warp prototype was retired and `EvaluateFoamShape` was reset to a clean pass-through baseline:

```hlsl
float FoamEvaluateIntrinsicShapeMask(
    FoamMaterialState material,
    float validFluid)
{
    return saturate(material.presence * validFluid);
}

_FoamShapeMaskWrite[coordinate] =
    FoamEvaluateIntrinsicShapeMask(material, validFluid);
```

The 5.10 validation screenshots showed clear green/magenta signed values in `Foam Shape Difference`, proving the 5.9z product was numerically changing `_FoamShapeMask`. However, `Material Presence` and `Foam Evaluated Shape` still looked and behaved basically identical in normal mask display. The conclusion is precise:

```text
5.9z did work at the value/difference level.
5.9z did not work at the visible structural-shape level.
```

The failure reason remains architectural, not merely amplitude tuning:

```text
- one-to-two-cell coordinate displacement affects mostly contours;
- broad solid masks remain broad and solid after nearby sampling;
- blend-to-base damped visible differences;
- the operation redistributed coverage inside the same overall ribbon/blob;
- coordinate warp cannot create visual bridge/pinch/sheet/contact support by itself.
```

Therefore 5.9z is no longer present as active code in Layer D. Its lesson is retained in documentation as a rejected/superseded prototype. Layer D now starts from a truthful baseline where `Foam Shape Difference` should be mostly black until a new Layer D component is intentionally added.

`DispatchEvaluateShape()` remains gated behind Layer D debug use because Final Foam still does not consume `_FoamShapeMask`.

## 5.3.1 4.11C.5.10 compliance audit result

The first source audit after the architecture lock found the current code broadly compatible with the acyclic Layer A-F graph:

```text
Layer B external influence generation was not found reading FoamState or _FoamShapeMask.
Layer C persistent material kernels were not found reading _FoamShapeMask or Layer D helper products.
Layer D writes _FoamShapeMask only, not FoamState.
Layer E debug/final paths render pixels only and do not feed compute.
Final Foam remains disconnected from _FoamShapeMask.
```

Cleanups made from that audit:

```text
Added Foam Shape Difference debug.
Corrected stale Foam Evaluated Shape descriptions.
Corrected Water Body help text so persistent Foam is described as downstream material transport, not active lateral disturbance transport.
Removed unused wake/pressure disturbance-transport constants left from abandoned material-motion experiments.
Gated DispatchEvaluateShape to Layer D debug use until Final Foam actually consumes the product.
```

Known non-urgent caveats:

```text
The transition-hold fallback may still bind persistent state where the shape mask is expected; evaluated-shape debug during topology transition should be treated cautiously until a dedicated transition ShapeMask snapshot exists.
Shader-side Final Foam still owns legacy macro shaping until the accepted Layer D film/shape product replaces it.
Low-res Layer D Film Source and Film Support helpers exist after `4.11C.5.13`; coordinate-space and support-source semantics were corrected and validated through `4.11C.5.13B` and `4.11C.5.13C`, and spread was tuned in `4.11C.5.13D`. The latest architectural correction is that Layer D should not be asked to invent shore/rock/contact film from a single central manual ribbon. Source placement belongs in Layer C source population first; Layer D then spreads material-derived products.
```

## 5.3.2 4.11C.5.10B validation response and reset

The first validation after `Foam Shape Difference` showed the exact problem:

```text
Foam Shape Difference: clearly non-black, with green/magenta bands.
Material Presence: visually broad white ribbon/blob.
Foam Evaluated Shape: visually broad white ribbon/blob, effectively the same silhouette and behavior.
Final Foam: unchanged, as intended.
```

Interpretation:

```text
The 5.9z coordinate warp changed values but not useful visible structure.
A debug difference view can look dramatic while the actual shape remains player-useless.
Future Layer D work must prove visible structural benefit, not just nonzero numeric difference.
```

5.10B therefore removes the warp helpers and Motion Field/routing bindings from `DispatchEvaluateShape()`. The pass-through baseline is deliberately boring so future probes have a clean comparison target:

```text
Material Presence ~= Foam Evaluated Shape
Foam Shape Difference ~= black
```

4.11C.5.11 tested a deliberately isolated local procedural breakup probe on top of this clean baseline. Validation proved the probe was active, but it produced cell/ribbon-shaped removals because `_FoamShapeMask` is too coarse for atomic fine breakup. 4.11C.5.11B retires that probe and returns Layer D to pass-through. Fine breakup now belongs in Layer E shader composition; Layer D remains the future macro film-structure layer.

## 5.4 Allowed reads

Layer D may read:

```text
Layer A River Domain data
Layer B External Influence Fields
Layer C Persistent Foam State
time
read-only previous Layer D temporal occupancy
```

Layer D may not read:

```text
final shader output
screen-space result
any future downstream product that would create a cycle
```

## 5.5 Owned products

Current product:

```text
_FoamShapeMask
R = evaluated broad visible foam/film mask
```

Current helper products:

```text
_FoamFilmSource
_FoamFilmSupport
_FoamVisualOccupancyA / _FoamVisualOccupancyB
```

Future possible products, if justified:

```text
_FoamDamageMask
_FoamBreakMask
_FoamEdgeMask
```

Any foam-derived helper field belongs here, not in Layer B.

## 5.6 Allowed behavior

Layer D may visually:

```text
widen foam
connect nearby foam
bridge small gaps
pinch weak links
soften contours
bend/ripple broad film using motion fields
increase old-foam fragility based on Remaining Life
use contact/support fields to create bank/rock film support
use low-res helper fields for broad sheet behavior
advect temporal visual occupancy so broad sheets can persist, pinch, tear, split, and rejoin over time
```

## 5.7 Forbidden behavior

Layer D must not:

```text
write _FoamStateWrite
modify Presence
modify Remaining Life
modify Material Pattern
move durable material
spawn durable material
kill durable material
feed back into Layer B
feed back into Layer C
track pocket IDs
own connected-component identity
hide broken Stage C transport with visual-only macro movement
```

## 5.8 Correct meaning of visual offset

Incorrect:

```text
Stage D moved this foam cell right.
```

Correct:

```text
Stage D displayed the broad visible film slightly right of durable material, within bounded visual-shape rules.
```

Layer D may lie visually. It may not corrupt material truth.

## 5.9 Required internal structure

Layer D should become a small fixed-grid pipeline, not one all-powerful pass.

### D1 — Visual Film Source

Build a low-res material-derived source field. This field may read upstream support/contact data as bias or suppression, but support must not create Film Source by itself.

Inputs:

```text
Persistent Presence
Remaining Life
Material Pattern
valid fluid
exclusion
bank/contact support as bias only
rock/contact support as bias only
wake/pressure/ripple support as bias only
motion intent as future bias only
negative suppression
```

Output:

```text
_FoamFilmSourceHalf
```

Meaning:

```text
Where does material-derived broad visible film begin?
```

This is not persistent material. It is also not raw Layer B support. Persistent material creates this source; Layer B support/contact fields can only bias or suppress it.

### D2 — Visual Sheet Support

Create broad field support for reference-like sheets/ribbons.

Inputs:

```text
_FoamFilmSourceHalf
Layer B visual/contact support
flow direction / river basis
optional disturbance agitation
```

Operations should be cheap field operations such as:

```text
directional spread along flow
weaker spread across flow
separable blur/spread
small-gap closing approximation
thresholded sheet support
contact/bank support expansion
```

Output:

```text
_FoamFilmSupportHalf
```

Meaning:

```text
Where can broad visible surface film structurally exist?
```

### D3 — Full-resolution Evaluated Shape

Create `_FoamShapeMask`.

Inputs:

```text
Persistent Foam State
_FoamFilmSourceHalf
_FoamFilmSupportHalf
valid fluid
obstacle exclusion
Remaining Life
Material Pattern
Motion Field / obstacle routing
optional local procedural breakup seed data
```

Output:

```text
_FoamShapeMask
```

This pass combines durable material truth with broad visual support, while preserving the rule that only Layer C owns real material.

### D4 — Advected Temporal Visual Occupancy

Implemented in `4.11C.5.16C` as the structural moving visual sheet used by Final Foam integration and shader-local detail.

Inputs:

```text
previous half-resolution temporal occupancy
current Film Source
current Film Support
canonical local 2D velocity
valid fluid and obstacle exclusion
Layer C CFL substep count
```

Output:

```text
current half-resolution temporal occupancy
```

Rules:

```text
history is transported through the same face-velocity and closed-boundary contract as Layer C;
history builds quickly toward newly supported film and releases more slowly when support disappears;
history may preserve a visual sheet, bridge, pinch, or tear over time;
history must not feed Layer C or Layer B;
history must not become material lifetime or material quantity;
Final Foam remains disconnected until the coordinate-consistent evaluated-shape A/B path is accepted.
```

## 5.10 Local-only breakup limit

A local function can produce convincing chipping and semi-organized chaos when it runs at the right visual scale:

```text
visible = broadMask * proceduralPattern(position, riverUV, time, life, materialPattern)
```

This can create:

```text
edge chipping
animated breakup
small cuts
apparent fragments
thin cracks
life-based fragility
```

But a purely local function cannot reliably bridge based on nearby foam, because an empty cell between two foam patches and an empty cell in open water have identical local foam presence.

Therefore:

```text
local procedural math = decorative breakup/detail
low-res support field = structural bridge/sheet/contact behavior
```

The `4.11C.5.11` validation adds a second, stricter lesson:

```text
local procedural breakup should not be baked into _FoamShapeMask when the desired detail is finer than a foam field cell.
```

Layer D writes a foam-field texture. Removing cells inside `_FoamShapeMask` exposes simulation-cell scale and creates long ribbon/cell-shaped holes. The inspiration reference's fine breakup is much more granular, closer to rendered-pixel/sub-cell detail.

Corrected ownership:

```text
Layer D owns macro visual structure: broad film, sheet support, bridge/pinch/split, bank/rock/contact film, smooth mask foundation.
Layer E owns micro visual detail: granular edge breakup, tiny cuts, thin streaks, highlight scratches, final polish.
```

## 5.11 Layer D local procedural breakup probe — validation and rejection

`4.11C.5.11` implemented the first cheap local-only Layer D breakup test. Its purpose was to test the best-case version of the no-neighbour "magical" approach before paying for low-resolution structural support fields.

Implemented code path in 5.11:

```text
CS_RiverFoam.compute
  EvaluateFoamShape(...)
    FoamClipPackedToValidFluid(...)
    FoamDecodeMaterialState(...)
    FoamEvaluateLocalProceduralBreakupShape(...)
      FoamResolveMaterialPhysicalPosition(...)
      FoamEvaluateLocalBreakupField(...)
      FoamSourceFillValueNoise(...) / EvaluateFoamSourceFillField(...)
    _FoamShapeMaskWrite[coordinate] = result

StylizedRiverFoamRuntime.Compute.cs
  DispatchEvaluateShape()
    bound _FoamBoundary
    bound _FoamObstacleExclusionRead
    bound _FoamStateRead
    bound _FoamShapeMaskWrite
    bound _FoamTime / _FoamSeed
    bound _FoamGlobalStart / _FoamFieldLength
    bound _FoamMetricRows
```

The probe correctly obeyed the dependency rules:

```text
no neighbouring FoamState sampling
no Motion Field lane
no Obstacle Routing field
no Topology support fields
no low-res Film Source / Film Support
no Final Foam shader mask
no entity or pocket identity
no persistent FoamState mutation
```

Validation result:

```text
Foam Shape Difference became clearly non-black, mostly magenta/removal.
The removals were long cell/ribbon-shaped gaps.
The result exposed _FoamShapeMask cell scale.
It did not resemble the granular, almost atomic breakup in the inspiration river.
Final Foam remained unchanged, as intended.
```

Conclusion:

```text
5.11 proved that Layer D local-only breakup can produce difference values, but it is rejected as the fine-fragmentation solution.
The issue is layer/resolution mismatch, not inactivity.
Do not tune this Layer D breakup probe further.
Fine fragmentation must be tested in Layer E shader composition at rendered-pixel scale.
Layer D should stay focused on macro film structure.
```

`4.11C.5.11B` retires the 5.11 probe as active code. The baseline shape path is again:

```text
CS_RiverFoam.compute
  EvaluateFoamShape(...)
    FoamClipPackedToValidFluid(...)
    FoamDecodeMaterialState(...)
    FoamEvaluateIntrinsicShapeMask(...)
    _FoamShapeMaskWrite[coordinate] = result

StylizedRiverFoamRuntime.Compute.cs
  DispatchEvaluateShape()
    binds _FoamBoundary
    binds _FoamObstacleExclusionRead
    binds _FoamStateRead
    binds _FoamShapeMaskWrite
```

Expected baseline after 5.11B:

```text
Material Presence ~= Foam Evaluated Shape
Foam Shape Difference = black or effectively black
Final Foam unchanged
```

## Layer D structural performance target

For a High 32 m chunk:

```text
Full field: 128×128 = 16,384 cells
Half field: 64×64 = 4,096 cells
```

Proposed core Layer D cost target:

```text
D1 Film Source:     ~32k reads/update,  ~4k writes/update
D2 Sheet Support:   ~40k–60k reads/update, ~8k writes/update
D3 Full Shape:      ~100k–150k reads/update, ~16k writes/update
Total:              ~175k–240k reads/update/chunk, ~28k writes/update/chunk
```

Recommended update rates:

```text
Low:    8 Hz
Medium: 12–16 Hz
High:   16–24 Hz
```

Avoid default full-res wide neighbourhood classifiers. Radius 1/3/5 box sampling costs:

```text
3×3 + 7×7 + 11×11 = 9 + 49 + 121 = 179 samples/cell
128×128×179 ≈ 2.93M samples/update/chunk
```

That is not the default architecture.

---

# 6. Layer E — Shader Composition

## 6.1 Abstract responsibility

Layer E is the water render shader. It turns upstream textures into final pixels.

It answers:

```text
How should this water pixel look right now?
```

It does not own simulation or broad structure.

## 6.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Important current symbols include:

```text
_FoamShapeMask
_FoamDebugView
_FoamMotionLane
_FoamObstacleRouting
RiverWaterEvaluateFoam(...)
RiverWaterFoamResult
RiverWaterFoamResult.materialUV
foam.presence
foam.remainingLife
foam.mask
Foam Evaluated Shape debug branch
Foam Motion Field debug branch
Foam Motion Field + Cell Grid debug branch
```

## 6.3 Allowed reads

Layer E may read:

```text
Layer A coordinate/material UV data
Layer B influence fields if needed for debug or local polish
Layer C Persistent Foam State if needed
Layer D _FoamShapeMask and visual helper products
local procedural noise
time
```

Layer E writes:

```text
screen pixels only
```

Layer E must not feed back into any compute texture or simulation state.

## 6.4 Owned behavior

Layer E owns:

```text
final foam color
opacity
edge softness
small local chipping
small local fray
thin bright streaks
sparkle/highlights
reflection/refraction blending
water lighting/composition
debug visualization
```

Layer E should not own:

```text
broad sheet creation
macro split/join decisions
bank/rock film support as structure
wide neighbourhood bridge logic
persistent material movement
```

## 6.5 Broad structure vs local detail rule

Use this rule:

```text
If the effect needs context/nearby foam/support, do it in Layer D compute.
If the effect is local per-pixel polish, do it in Layer E shader.
```

More precise correction:

```text
Decorative breakup should try local procedural math first.
Structural film connectivity should use cheap Layer D field support.
```

This prevents the shader from doing expensive wide neighbourhood searches per visible screen pixel, while still allowing rich local chaos cheaply.

## 6.6 Final Foam switch rule

Final Foam must not consume `_FoamShapeMask` as the production shape until Layer D is visibly better than current final foam.

Before the switch, debug views compare:

```text
Material Presence
Foam Evaluated Shape
Foam Shape Difference
```

After the switch, shader-side legacy macro foam shaping should be reduced or demoted so there is only one broad-structure authority.

---

# 7. Layer F — Scheduling, Quality, Debug

## 7.1 Abstract responsibility

Layer F is orchestration. It controls when layers update, which textures are allocated, what debug view is shown, and which quality tier is active.

It does not own foam behavior.

## 7.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Disturbances.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.UI.cs
```

Important current symbols include:

```text
StylizedRiverFoamDebugView
ResolveFoamDebugView(...)
IsMotionFieldDebugActive(...)
BindField()
BindDisabled()
DispatchEvaluateShape()
EnsureResources(...)
shapeMaskTexture
motionLaneTexture
obstacleRoutingTexture
FoamEvaluatedShape = 7
```

## 7.3 Owned behavior

Layer F owns:

```text
update cadence
quality tiers
active chunk culling/freezing
debug view selection
texture allocation/release
compute kernel lookup
compute texture binding
Inspector display and labels
profiling marker placement
```

Layer F must not own:

```text
foam birth logic
foam material motion math
visual bridge/break math
shader breakup math
```

## 7.4 Recommended quality/update targets

For broad foam/film:

```text
Layer C Persistent Material:
Low    ~8 Hz
Medium ~12 Hz
High   ~16 Hz

Layer D Visual Film:
Low    ~8 Hz helper/support, 8–16 Hz shape
Medium ~12–16 Hz helper/support, 16–24 Hz shape
High   ~16–24 Hz helper/support, 24 Hz shape unless profiling proves 60 is cheap

Layer E Shader:
Every rendered frame, local-only detail.
```

Distant, frozen, or offscreen chunks should reduce or skip Layer C and Layer D updates where safe.

## 7.5 Required debug views

Existing useful debug views:

```text
Final Foam
Foam + Aging Topology
Progressive Birth Source
Material Presence
Material Remaining Life
Foam Motion Field
Foam Motion Field + Cell Grid
Foam Evaluated Shape
Foam Shape Difference
```

Additional existing debug views:

```text
Foam Film Source
Foam Film Support
```

Each debug view must explicitly state what product it displays:

```text
Persistent material truth
External influence field
Layer D visual helper
Evaluated shape
Final shader output
```

Do not reuse a final-render mask in a raw material debug view.

## 7.6 Accepted Inspector and diagnostics contract

The accepted River tooling contract is:

```text
all top-level and nested foldouts
  collapsed by default;

production authoring
  grouped by River feature;

Foam production authoring
  grouped by Layers A–E plus Runtime & Quality;

Debug Views
  one central exclusive controller over the existing serialized debug enums;

Runtime Diagnostics
  labelled, selectable, read-only, stable-height rows;

Generated Status
  read-only generated and compatibility results;

Actions
  all generation, validation, cache, test-source, probe, clear, and reset operations.
```

The debug hub preserves the existing runtime enum fields and shader behavior. Selecting a new non-Final view clears the other debug systems. Existing scenes with multiple active debug fields are reported without silent modification; the rendered result follows the established shader priority:

```text
Foam > Disturbances > Refraction > Surface Motion > Water Body
```

`Normalize to Rendered View` preserves the winning view and clears hidden lower-priority selections. `Reset All Debug Views` returns every system to Final.

Runtime diagnostics must not own or mutate Foam behavior. Diagnostic rows remain present while values transition through Edit Mode, unavailable runtime, pending asynchronous readback, and live states. Ordinary authoring, Debug Views, Generated Status, and Actions do not request constant repaint. Constant repaint is permitted only in Play Mode, with one selected River, while a visible live Disturbance or Foam diagnostic leaf requires updates.

The planned Presence capacity-loss attribution belongs only at:

```text
Runtime Diagnostics
  Foam
    Layer C — Persistent Material
      Transport Accounting
```

It must not restore ad-hoc warning boxes, duplicate selectors, or editable controls inside diagnostics.

---

# 8. Canonical connectivity table

| Layer | May read | Must not read | Writes | Consumers |
|---|---|---|---|---|
| A — River Domain | River geometry, spline/domain, corridor/terrain geometry as needed | FoamState, ShapeMask, shader output | domain snapshots, boundary/coverage, coordinate mapping | B, C, D, E |
| B — External Influence | A, obstacles, banks, emitters, time, own previous influence history | FoamState, ShapeMask, D helper fields, shader output | topology/support/exclusion/motion/wake/pressure influence fields | C, D, E/debug |
| C — Persistent Material | A, B, previous FoamState, source events, time | ShapeMask, D helpers, shader output | FoamState | D, E/debug |
| D — Visual Foam/Film | A, B, C, time, optional previous visual shape | shader output, future downstream products | ShapeMask, visual helper fields | E |
| E — Shader Composition | A, B, C, D, local noise, time | nothing downstream; no feedback | final pixels | screen only |
| F — Scheduling/Debug | settings, runtime state, visibility | behavior internals as authority | dispatch/binding/debug decisions | all layers indirectly |

If a proposed feature violates this table, stop and redesign before coding.

---

# 9. Conflict examples and resolutions

## 9.1 “Layer C says left, Layer D says right”

This is only a contradiction if both layers claim to move material.

Canonical resolution:

```text
Layer C owns material movement.
Layer D owns visual interpretation only.
```

If Layer D's visible offset fights the material so strongly that foam appears to move the wrong way, the fix is to bound or retune Layer D. Do not let Layer D write material state.

## 9.2 “Motion Field moves foam”

Incorrect.

Canonical language:

```text
Motion Field is an external influence/input field.
Layer C may use it later for approved real material transport.
Layer D may use it for visible film deformation.
The Motion Field itself does not move foam.
```

## 9.3 “Support created foam”

Incorrect unless Layer C used support during an approved birth rule.

Canonical language:

```text
Support encourages birth/survival/visual film.
Persistent material only exists if Layer C writes it.
Visual film only appears if Layer D writes it.
```

## 9.4 “Stage D bridge is a material merge”

Incorrect.

Canonical language:

```text
Layer D bridge is visual-only surface film connectivity.
Real material merge, if ever needed, belongs to Layer C.
```

## 9.5 “Shader breakup split the foam”

Incorrect.

Canonical language:

```text
Shader breakup split the rendered appearance at this pixel.
It did not split Persistent Foam State.
```

---

# 10. Accepted and rejected techniques

## 10.1 Accepted primary techniques

Accepted as architectural direction:

```text
fixed-grid field math
read/write ownership per layer
External Influence Fields upstream of Persistent Material and Visual Film
Persistent Foam State as durable material truth
Visual Foam/Film Evaluation as broad structural interpretation
shader-side local procedural breakup and thin streaks
low-res Layer D helper fields for sheet support/bridging
bounded visual-only offsets
quality-tiered update cadence
explicit debug views for every product
```

## 10.2 Rejected as primary techniques

Rejected/superseded as primary architecture:

```text
foam pocket IDs
connected-component foam islands
per-pocket entity database
persistent stored-state morph as visual breakup
neighbour-resampled morphology that writes FoamState
fractional lateral row weighting
per-cell stochastic lateral row commit
dense interior hole cutting as the main look
tiny local edge-fray as the main look
5.9z coordinate warp as the final shape solution
naive full-res 179-sample wide-neighbour classifiers as default
shader-side wide-neighbour structural foam search
using final shader masks as raw material debug truth
```

## 10.3 Techniques allowed only with caution

Allowed but not as first resort:

```text
mip/pyramid helper fields
jump-flood/distance fields
real material merge rules beyond the existing source merge contract
additional temporal damage/history products beyond the accepted occupancy pair
```

These require separate approved plans and validation.

---

# 11. Performance model

## 11.1 Resolution assumptions

Typical per 32 m river chunk:

```text
Low:    full 64×64  = 4,096 cells;  half 32×32 = 1,024 cells
Medium: full 96×96  = 9,216 cells;  half 48×48 = 2,304 cells
High:   full 128×128 = 16,384 cells; half 64×64 = 4,096 cells
```

## 11.2 Proposed Layer D cost target

For High, per 32 m chunk:

```text
D1 Film Source:
~32k reads/update
~4k writes/update

D2 Sheet Support:
~40k–60k reads/update
~8k writes/update

D3 Full Shape:
~100k–150k reads/update
~16k writes/update

Core total:
~175k–240k reads/update/chunk
~28k writes/update/chunk
```

At 16 Hz:

```text
~3–4M reads/sec/chunk
```

For 3 active High chunks:

```text
~9–12M reads/sec
```

This is acceptable as a target for low-end desktop-class GPUs if shader work remains local and chunk/update scheduling is respected.

## 11.3 Why not shader-side structural search

At 1080p, if water covers 25% of the screen:

```text
1920×1080×0.25 ≈ 518k water pixels
```

A shader-side 8-sample structural neighbourhood effect at 60 FPS costs roughly:

```text
518k×8×60 ≈ 249M samples/sec
```

This scales with screen coverage and frame rate. Broad structure should therefore be computed into compact fields, not rediscovered per rendered pixel.

## 11.4 Why not naive wide full-res classifiers

A radius 1/3/5 full-res classifier costs:

```text
3×3 + 7×7 + 11×11 = 179 samples/cell
128×128×179 ≈ 2.93M samples/update/chunk
```

At 60 FPS:

```text
~176M samples/sec/chunk
```

That is too expensive as a default and still not as clean as low-res sheet support for broad film.

## 11.5 Memory target

High chunk approximate additional Layer D memory:

```text
_FoamShapeMask RHalf 128×128 ≈ 32 KB
Two half-res RHalf helpers 64×64 ≈ 16 KB total
Optional previous shape RHalf ≈ 32 KB
```

Expected added memory:

```text
~16–48 KB/chunk without richer RG helpers
```

This is a reasonable memory trade for lower runtime cost.

---

# 12. Implementation roadmap

## Phase 1 — Documentation lock

Status: this document.

Purpose:

```text
make dependencies, ownership, rejected paths, and target layers canonical
```

## Phase 2 — Compliance/debug visibility

Status after 4.11C.5.13C: complete for current Layer D debug/product pipeline.

Completed:

```text
Foam Shape Difference debug.
A/B/C/D/E/F source-level compliance audit.
Stale editor/help text cleanup.
Layer D dispatch gating while Final Foam does not consume _FoamShapeMask.
```

Completed later by `4.11C.5.13` and follow-up corrections:

```text
Foam Film Source debug.
Foam Film Support debug.
Domain-space Layer D sampling fix in 5.13B.
Material-gated Film Source semantic fix in 5.13C.
Verification that new Layer D helpers write only Layer D products and do not feed Layer B or C.
```

## Phase 3 — Layer E shader-side local detail probe

Status: implemented and validated as a technical proof in `4.11C.5.12`.

Purpose:

```text
determine how much reference-like fine chaos can be achieved with local shader math before/alongside the structural Layer D film-support system
```

Implemented diagnostic scope:

```text
Foam Shader Detail Probe debug view
Foam Shader Detail Difference debug view
shader-side local procedural chipping/fray/cuts based on river metres, material UV, material pattern, time, Remaining Life, and surface energy
sub-cell granular edge breakup at rendered-pixel scale
no neighbourhood search
no persistent mutation
no _FoamShapeMask mutation
no broad bridge support
no Final Foam change
```

Validation rule:

```text
Accept this as Layer E detail only if the result reads as pixel/sub-cell edge detail and not as cell/ribbon holes, dirty static noise, or broad structural breakup.
```

## Phase 4 — Low-res Visual Film Source and Sheet Support

Add Layer D low-res helper textures.

Scope:

```text
_FoamFilmSourceHalf
_FoamFilmSupportHalf
fixed-tap/separable directional spread
small-gap visual bridging support
bank/rock/contact film support
flow-aware sheet elongation
```

Purpose:

```text
create broad film/sheet behavior that local noise cannot know
```

## Phase 5 — Advected Temporal Visual Occupancy

Status: accepted through `4.11C.5.16C.1`.

Scope:

```text
two half-resolution RHalf ping-pong textures;
canonical-velocity donor-cell advection;
closed shore/obstacle/invalid/lateral exterior faces;
flow-aware endpoint outflow;
asymmetric exponential build/release toward Film Source + Film Support;
full-resolution _FoamShapeMask combines Presence and temporal occupancy;
no feedback into Layer C;
debug-gated while Final Foam remains disconnected.
```

Accepted evidence:

```text
occupancy follows the Layer C route;
stationary target/occupancy difference converges to black;
no permanent coordinate offset was found;
comparative material diagnostics use a shared meaningful-Presence gate;
Final Foam remains unchanged.
```

## Phase 6 — Occupancy-Native Macro Breakup Experiment

Status: rejected and retired through `4.11C.5.16D.R`.

The zero-extra-field experiment attempted to infer strain, weakness, connected cut evidence, slow healing, and local shape authority directly from temporal occupancy. It added no textures, channels, buffers, kernels, or dispatches, but Unity validation did not produce convincing macro tears. It mainly suppressed weak film and exaggerated existing lane transitions.

Permanent rule:

```text
do not restore 5.16D–5.16D.2 breakup code;
do not add a persistent damage field or packed damage channel;
use emergent lateral advection/temporal occupancy for macro separation;
use Layer E for micro-tearing and edge erosion.
```

## Phase 7 — Coordinate-Consistent Final Foam Integration

Next active phase.

Scope:

```text
add a reversible A/B path in which Final Foam consumes _FoamShapeMask;
sample evaluated shape and material attributes through one consistent presentation coordinate contract;
avoid the rejected preview mismatch between committed shape and residual-shifted life/pattern;
preserve Layer C truth and all transport/lifetime ownership;
retain the legacy Final Foam path until the evaluated path is explicitly accepted.
```

Acceptance:

```text
no upstream-looking expiry illusion;
no double movement or obstacle snap-back;
evaluated broad film is visibly preferable to the legacy macro shape;
normal material lifetime, pattern, and spawning remain unchanged;
performance cost is measured and bounded.
```

## Phase 8 — Shader-Local Micro-Tearing and Edge Erosion

Scope:

```text
small edge notches and short cracks;
thin strand separation and chipped weak fringes;
age- and disturbance-sensitive local erosion;
rendered-pixel procedural math only;
zero new persistent textures, fields, channels, or compute dispatches.
```

## Phase 9 — Thin bright streak and local polish layer

Scope:

```text
fast narrow white scratches/streaks;
glints, bubbles, edge lighting, and foam colour polish;
local/no-neighbour procedural math;
separate from broad film structure.
```

## Phase 10 — Performance tiers and chunk scheduling

Formalize:

```text
resolution per quality tier;
Layer C update rate;
Layer D helper/shape update rate;
active chunk caps;
culling/freezing rules;
debug/profiling counters.
```

---

# 13. Current active conclusion

The active architecture is now:

```text
Layer C owns durable material birth, transport, lifetime, and death;
Layer D owns material-derived Film Source / Film Support;
Layer D owns the accepted advected temporal visual occupancy sheet;
Layer D does not own a separate persistent fracture or damage state;
_FoamShapeMask is the full-resolution diagnostic product;
Layer E owns rendered-pixel micro-tearing, streaks, colour, and lighting;
Final Foam consumes committed Layer C state directly and remains disconnected from `_FoamShapeMask` until a separate Layer D production decision is accepted.
```

`4.11C.5.16E.2` is the active architecture state and is Unity-validated. Point-velocity residual prediction is retired after the committed/evaluated previews proved stable while residual-predicted Final Foam stuttered. Final Foam keeps both reversible visibility policies, the supported-aging minimum is `0.05`, and lifecycle aging is quantized once per complete material tick rather than once per CFL substep. `_FoamShapeMask` remains diagnostic-only.

The accepted R1–R5 Inspector redesign is the current Layer F tooling contract. `4.11C.5.16E.3 — Transport Presence Capacity-Loss Attribution Audit` is Unity-validated and accepted. It uses `Runtime Diagnostics > Foam > Layer C — Material & Lifecycle > Transport Accounting` and changes no transport, lifecycle, source, Layer D, or rendering behavior.

## `4.11C.5.16E.3` transport Presence attribution contract

The existing total Presence clamp/capacity loss remains the authoritative before-versus-stored transport measurement. The audit observes the same raw transported and final packed state and divides Presence loss sequentially into:

```text
raw positive Presence -> unit [0,1] capacity;
unit-limited Presence -> fractional boundary coverage;
boundary-limited Presence -> boundary × (1 - obstacle exclusion);
remaining capacity-limited Presence -> state-validity rejection;
remaining tiny Presence -> final minimum-state cutoff.
```

The audit additionally stores:

```text
maximum raw transported Presence;
maximum raw excess above local valid-fluid capacity;
any/unit/boundary/obstacle capacity-hit cell-substep counts;
signed CPU attribution residual against the existing total.
```

Counts are cell-substep samples, not unique cells. The same cell may contribute more than once when CFL safety requires multiple substeps. Loss values remain area-weighted fixed-point material units. Peaks use the same `4096` fixed-point scale without area weighting.

The metric buffer contract is:

```text
12 -> 23 raw uint entries;
48 -> 92 bytes;
+44 bytes per active Foam runtime.
```

The existing reset kernel, simulation kernel, capture cadence, and asynchronous readback are reused. New arithmetic and atomics execute only while `_FoamTransportMetricsEnabled != 0`. No new texture, persistent field, channel, kernel, dispatch, or readback is permitted.

The category values are fixed-point partitioned against the existing total whenever their float stages reconcile. This avoids manufacturing a residual solely through independent per-category rounding. If the float stages do not reconcile, categories are accumulated independently so the signed residual exposes the unmatched path rather than hiding it.

No limiter, rejected-inflow retention, redistribution, epsilon change, source change, or gate change is part of `5.16E.3`.

## `4.11C.5.16E.3C` accepted closure and deferral contract

Unity validation used only the corresponding automatic source class in each controlled case:

| Test | Total loss | Unit | Boundary | Obstacle | Peak raw | Peak local excess |
|---|---:|---:|---:|---:|---:|---:|
| Open water | 0.067% | 0.000% | 0.067% | 0.000% | 0.7727 | 0.0215 |
| Shoreline | 0.484% | 0.222% | 0.262% | 0.000% | 1.2046 | 0.2046 |
| Obstacle | 0.747% | 0.479% | 0.267% | 0.000% | 1.2246 | 0.2246 |

Every capture reconciled with zero signed residual and reported zero obstacle-capacity, state-validity, and minimum-cutoff loss. The object-source case therefore compresses Foam into neighbouring valid cells; it does not lose Foam by storing it inside the obstacle footprint. Boundary and unit-capacity losses are both real, while unconstrained open-water transport remains below the original target in the tested population.

The engineering target remains `0.10%`. `5.16E.3C` introduces an Editor-only temporary `1.00%` review threshold for the accepted PoC deferral. A result between those values is displayed as `Deferred known limitation`; a result above `1.00%` is again reported as an active review failure. This status does not redefine the solver as corrected and does not alter any runtime threshold, transport equation, state, source, or rendering path.

Exact receiver-acceptance transport was deferred because the audited multi-pass solution would cost approximately `2–3×` the current Layer C transport work. The cheaper single-pass directional vacancy cap was also deferred because its estimated `10–25%` Layer C transport cost is not justified by the current sub-1% and visually tolerated loss. Fractional shoreline storage relaxation remains a future zero-dispatch visual A/B candidate, but it is explicitly partial and may create hidden shoreline reservoirs.

Capacity-hit category counters can overlap; `Total` is the union of hit samples, while unit and boundary counts may both include the same cell-substep. The next active work is Layer E interior composition, followed by shader-local micro tearing. The numerical issue must be reopened if the `1.00%` review threshold is exceeded, visible loss appears, or materially different content invalidates the controlled evidence.



---

# Addendum — 4.11C.5.13 Low-Resolution Layer D Film Source / Film Support

`4.11C.5.13` implements the first real structural Layer D helper system. This is not a foam entity database and not a pocket tracker. It is a fixed-size field pipeline:

```text
Layer C FoamState + Layer B external support/contact fields
    -> half-resolution Film Source
    -> half-resolution Film Support directional spread
    -> full-resolution _FoamShapeMask
    -> Layer E debug/render sampling
```

Ownership remains acyclic:

```text
Layer B does not read FoamState, Film Source, Film Support, or _FoamShapeMask.
Layer C does not read Film Source, Film Support, or _FoamShapeMask.
Layer D reads Layer B and Layer C and writes only visual products.
Layer E reads visual products and writes screen pixels only.
```

New Layer D products:

```text
_FoamFilmSource  — half-resolution RHalf visual-film permission/source field.
_FoamFilmSupport — half-resolution RHalf broad sheet/contact/bridge support field.
```

`BuildFoamFilmSource` is material-gated after `4.11C.5.13C`. Persistent material creates Film Source. Layer B topology, pressure, lee, shore, and contact support may bias or suppress that material-derived source, but they must not seed Film Source from zero. The result is clipped by valid fluid and obstacle exclusion and suppressed by negative-aging pressure.

`BuildFoamFilmSupport` performs a cheap fixed-tap directional spread over the half-resolution Film Source. It favours along-flow continuity, applies weaker across-flow widening, and includes small diagonal support for bridge/cohesion. After `4.11C.5.13C`, Layer B support/contact can bias or suppress that spread, but cannot create spread without material-derived Film Source. This is the intended low-cost alternative to wide full-resolution neighbourhood classifiers.

`EvaluateFoamShape` now combines clipped persistent material with the film source/support product. This is allowed because `_FoamShapeMask` is visual interpretation, not durable material truth. Fine sub-cell detail still belongs in Layer E shader composition.

New debug views:

```text
Foam Film Source  — samples _FoamFilmSource.
Foam Film Support — samples _FoamFilmSupport.
```

Final Foam remains disconnected from `_FoamShapeMask` until the Layer D output is validated.

# Addendum — 4.11C.5.13B Layer D Domain-Space Film Sampling Fix

`4.11C.5.13B` corrects the coordinate ownership of the Layer D film pipeline.

The fixed contract is:

```text
Layer C FoamState:
  material-space persistent storage.
  Rendering may use residual material travel to display it smoothly.

Layer B external support/contact fields:
  domain-space river support fields.
  These do not follow material residual phase.

Layer D Film Source / Film Support / _FoamShapeMask:
  domain-space current visual products.
  They may read phase-corrected material, but the products themselves are anchored to the river domain.

Layer E shader debug/render sampling:
  Layer C material views use materialUV.
  Layer D visual products use fieldUV.
```

The bug fixed by `5.13B` was that `_FoamFilmSource`, `_FoamFilmSupport`, and `_FoamShapeMask` were sampled through `foam.materialUV`. Because `foam.materialUV` includes residual phase travel and snaps back after integer material commits, domain-anchored film/support products appeared to slide and then snap with the cell grid. The fix is not a tuning change; it is a coordinate-space ownership correction.

Implementation details:

```text
CS_RiverFoam.compute:
  - added FoamResolveMaterialPhaseOffsetUV;
  - added FoamResolveMaterialUVForDomainUV;
  - added FoamSampleMaterialStateForDomainUV;
  - BuildFoamFilmSource samples support/contact fields at domainUV and material state at phase-corrected materialUV;
  - EvaluateFoamShape samples material at phase-corrected materialUV but writes domain-space _FoamShapeMask.

StylizedRiverFoamRuntime.Compute.cs:
  - DispatchEvaluateShape explicitly binds _FoamPhaseTransportMetres before all Layer D kernels.

SH_CleanStylizedRiver.shader:
  - Layer D debug views sample _FoamShapeMask, _FoamFilmSource, and _FoamFilmSupport with foam.fieldUV.

RiverWaterFoam.hlsl:
  - Layer E shader-detail probe uses stable river-space diagnostic coordinates instead of inheriting the residual material phase.
```

Do not reverse this split. If a future effect needs durable material motion, it belongs in Layer C. If a future effect needs broad visual film support, it belongs in Layer D and writes domain-space visual products. If a future effect needs pixel-scale local polish, it belongs in Layer E and must not feed back into compute state.


# Addendum — 4.11C.5.13C Material-Gated Layer D Film Source

`4.11C.5.13C` fixes a semantic Layer D bug exposed after the domain-space sampling fix. The bug was not a coordinate issue. The film products were stable, but `Foam Film Source`, `Foam Film Support`, `Foam Evaluated Shape`, `Foam Shape Difference`, and the shader-detail probe inherited shapes from Layer B support topology because Film Source allowed support to become source directly.

Rejected behaviour:

```text
Layer B topology/support
    -> Film Source
    -> Film Support
    -> _FoamShapeMask
```

Canonical behaviour after this patch:

```text
Layer C material, phase-corrected into domain space
    -> Film Source

Film Source + Layer B support/contact bias/suppression
    -> Film Support

Layer C material + Film Source + Film Support
    -> _FoamShapeMask
```

Hard rule:

```text
Generic Layer B support/contact/topology cannot create visual film by itself.
It can bias, preserve, widen, or suppress material-derived film.
If the project later needs environmental foam/film that appears without spawned material, it must be a separately named and documented product, not accidental generic topology support.
```

Debug-view audit from the 5.13B baseline:

```text
0 Final Foam — clean from the Layer D support-source bug; still uses legacy Final Foam.
1 Foam And Aging Topology — intentionally topology/support-based.
2 Progressive Birth Source — clean from topology-support film contamination.
3 Material Presence — clean Layer C material truth.
4 Material Remaining Life — clean Layer C material-life truth.
5 Foam Motion Field — external motion/routing debug, not topology support.
6 Foam Motion Field + Cell Grid — external motion plus intentional material-space cell grid.
7 Foam Evaluated Shape — contaminated before 5.13C through _FoamShapeMask.
8 Foam Shape Difference — truthful comparison, but its evaluated-shape input was contaminated before 5.13C.
9 Foam Shader Detail Probe — inherited contaminated _FoamShapeMask before 5.13C.
10 Foam Shader Detail Difference — inherited contaminated _FoamShapeMask before 5.13C.
11 Foam Film Source — direct root of the support-source contamination before 5.13C.
12 Foam Film Support — inherited contaminated Film Source before 5.13C.
```

Implementation details:

```text
CS_RiverFoam.compute:
  - added FoamVisualFilmInfluence;
  - added FoamResolveVisualFilmInfluenceAtDomainUV;
  - Film Source now uses materialBody * supportBias * negativeSuppression;
  - supportBias is a multiplier only and cannot seed source from zero;
  - Film Support still spreads Film Source, but Layer B support only biases/suppresses that spread.

StylizedRiverFoamRuntime.Compute.cs:
  - BuildFoamFilmSupport now binds _FoamTopologyRead and _FoamTopologySourcesRead because the support pass needs bias/suppression data.

StylizedRiverEditor.cs:
  - Film Source / Film Support debug descriptions now state that support cannot create visual film from zero.
```

Expected validation:

```text
Foam Film Source should follow spawned/material foam instead of generic support topology shapes.
Foam Film Support may be broader than Film Source but should not appear where no material-derived Film Source exists nearby.
Foam Shape Difference should show additions caused by material-derived film spread, not raw support topology.
Foam And Aging Topology remains the explicit view for support topology.
Final Foam remains unchanged.
```

---

# Addendum — 4.11C.5.13C Unity Validation and 5.13D Gold-Standard Next Target

Unity validation after `4.11C.5.13C` confirmed the semantic correction worked:

```text
Foam Film Source no longer displays raw support topology where no material-derived foam exists.
Foam Film Support now expands material-derived Film Source instead of topology support.
Foam Evaluated Shape and Foam Shape Difference no longer inherit generic support shapes.
Final Foam remains unchanged.
```

This means the active issue has moved from architecture/semantics to visual spread quality. The current Layer D film system is clean enough to tune, but not visually final.

## Current meaning of Layer D debug views

```text
Foam Film Source
  Half-resolution material-derived visual source.
  It should answer: where is real persistent material feeding possible visual film?
  It is not final foam and not support topology.

Foam Film Support
  Half-resolution spread/support field fed by Film Source.
  It should answer: where can material-derived film broaden, connect, or preserve macro continuity?
  It may be broader than source, but it must not appear from generic support alone.

Foam Evaluated Shape
  Full-resolution domain-space visual mask.
  It combines phase-corrected persistent material with Film Source/Support.
  It is visual interpretation, not durable material truth.

Foam Shape Difference
  Signed comparison against raw material presence.
  Green now means material-derived Layer D addition.
  Magenta means Layer D/Layer E removal.
  It must no longer be interpreted as automatic foam generation.
```

## Why 5.13D was needed

The latest screenshots showed Film Support behaving like a thick, uniform low-resolution dilation around the spawned material ribbon. This is expected from the current first-pass spread formula, but it is not the desired final film shape.

The issue is now:

```text
semantically correct source/support;
visually primitive spread/threshold behavior.
```

The patch tunes spread shape without adding new architecture.

## 4.11C.5.13D — Layer D Film Spread Shape Tune

Status: implemented as a compute-only tuning pass; pending Unity validation.

Target:

```text
Make Film Support less like a uniform capsule dilation and more like controlled surface-film support.
```

Concrete tuning responsibilities:

```text
Film Source:
  keep close to material-derived truth;
  avoid over-thickening the source at half resolution;
  keep support as a small multiplier/suppression only.

Film Support:
  preserve along-flow continuity;
  weaken and condition cross-flow widening;
  tighten bridge/fill thresholds;
  reduce uniform spread around simple ribbons;
  keep support/contact as bias/suppression, not source.

EvaluateFoamShape:
  make supportShape more conservative;
  reduce support dominance over base/material source;
  keep additions visible but selective.
```


Implementation completed in this patch:

```text
FoamResolveVisualFilmInfluenceAtDomainUV:
  supportBias is now 0.94-1.08 instead of 0.90-1.18.

BuildFoamFilmSupport:
  along-flow taps remain the dominant continuity path;
  cross-flow taps are reduced and gated by source/evidence;
  diagonal spread is reduced and tied to the same cross-flow gate;
  bridge thresholds are stricter;
  bridge contribution is reduced to 0.42.

EvaluateFoamShape:
  supportShape threshold is stricter;
  supportShape no longer dominates visualFilm;
  sourceShape remains visible but slightly more conservative.
```

Files intentionally not changed:

```text
StylizedRiverFoamRuntime.*.cs
SH_CleanStylizedRiver.shader
RiverWaterFoam.hlsl
StylizedRiver.cs
StylizedRiverEditor.cs
```

No-touch rules:

```text
Do not switch Final Foam to _FoamShapeMask.
Do not reintroduce support-only source.
Do not add environmental contact film yet.
Do not add entities, pocket IDs, or connected-component foam tracking.
Do not mutate FoamState, Remaining Life, or Material Pattern from Layer D.
Do not tune Layer E shader detail as part of this patch.
Do not expose Inspector controls yet.
```

Primary code file to inspect first:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

Specific functions to inspect:

```text
FoamResolveVisualFilmInfluenceAtDomainUV(...)
FoamResolveVisualFilmSourceAtDomainUV(...)
BuildFoamFilmSource
FoamLoadFilmSource(...)
BuildFoamFilmSupport
EvaluateFoamShape
```

Acceptance criteria:

```text
Foam Film Source still follows material only.
Foam Film Support remains broader than source but less uniformly inflated.
Foam Shape Difference shows smaller/more selective green additions.
No support-topology shapes return.
No phase/cell-grid stutter returns.
Final Foam remains unchanged.
```



### 2026-07-09 — River Foam 4.11C.5.14A Layer C Automatic Shore/Contact Source Population

Audited the current birth architecture after 5.13D validation. The audit found that manual/progressive birth, support/lifetime capture, topology/contact fields, and Layer D material-derived spread exist, but automatic birth near specific environmental locations was missing. The correct next step is therefore Layer C source population, not a new Layer D environmental-film authority.

Implemented the first conservative source class: disabled-by-default automatic shore/contact birth. The runtime scans sparse shore-support-band candidates at a low fixed cadence, accepts a bounded subset based on the river seed and amount, then queues real persistent material through `PendingInjection`, `QueueMaterialBirth`, and the existing `InjectFoam` compute kernel. The material then lives or dies under the existing support/negative aging system.

This patch initially added Inspector controls under `Source Population`: `Automatic Birth Enabled` and `Shore Contact Birth Amount`, plus runtime counters/status. Validation showed that the single amount slider was overloaded and could create large shore chunks. Patch `4.11C.5.14B` then overcorrected by exposing too many implementation controls. Patch `4.11C.5.14C` simplified the control surface, but validation showed the hidden implementation was too starved. Patch `4.11C.5.14D` keeps the Layer C source-population route and uses deterministic full-strength source events controlled by Coverage, Activity, Patch Size, and Pattern. It does not switch Final Foam, does not create support-only Film Source, does not add a visual-only environmental film texture, and does not create entities or pocket IDs.


### 2026-07-09 — River Foam 4.11C.5.14B Source Population Controls / Shore Birth Profile

Validated `4.11C.5.14A` enough to confirm automatic birth works, but the old `Shore Contact Birth Amount` created large blocky chunks because it controlled density, footprint, initial amount, initial life, elongation, and compound shape together. This was a source-profile design problem, not a Layer C architecture problem.

`4.11C.5.14B` correctly defined source-class-specific spawning as the contract for future automatic birth, but its Inspector exposed too many implementation controls and was not suitable for authoring or validation.

### 2026-07-09 — River Foam 4.11C.5.14C Simplified Shore Spawn Controls

`4.11C.5.14C` keeps the source-class-specific spawning contract but removes the low-level shore controls from the Inspector. Shore Contact Birth is now a deterministic sparse shoreline-stroke recipe controlled by four intent-level values: Coverage, Size, Strength, and Persistence.

The shore recipe no longer exposes per-tick budget, support threshold, inward band, radius, elongation, stroke length, initial amount, initial life, jitter, or shape mode. Internally, Coverage maps to candidate spacing/acceptance and budget; Size maps to conservative radius/stroke length; Strength maps to initial material presence; Persistence maps to initial Remaining Life. Shore birth always uses small deterministic strokes, never compound blobs.

### 2026-07-09 — River Foam 4.11C.5.14D Deterministic Shore Source Events

`4.11C.5.14D` replaces the `5.14C` one-shot sparse shore stroke recipe with deterministic shore source events. The architectural rule is unchanged: automatic shore birth creates real persistent Layer C material through the existing progressive composition / material injection path; support/lifetime capture decides survival; Layer D may only spread material-derived film; Final Foam remains unchanged.

The patch rejects faint-deposit accumulation as the shore strategy. Automatic shore events now spawn normal-strength material and reveal their area spatially over the event duration. This is intended to reduce the visual read of a completed patch teleporting into existence without paying for many weak births.

The Source Population UI now exposes only:

```text
Coverage
Activity
Patch Size
Pattern: Mixed / Shore Ribbons / Inward Wash
```

Two recipes are implemented: `Shore Ribbon`, a bank-parallel opaque ribbon source event, and `Inward Wash`, a shore-attached event that drifts inward/downstream from the bank contact band. Both are scheduled through deterministic slots distributed along both banks, bounded by a maximum number of starts and scans per update.


### 2026-07-09 — River Foam 4.11C.5.14E Automatic Source Event Rasterizer

Runtime validation of `4.11C.5.14D` showed that deterministic source-event scheduling alone was not enough. Both `Shore Ribbons` and `Inward Wash` still flowed through the generic progressive composition / `PendingInjection` / `InjectFoam` segment path, so the GPU only received capsule-like stamps. The result was predictable near-shore rectangles/bars, insufficient coverage at max settings, and no strong visual difference between patterns.

`4.11C.5.14E` keeps the Layer C material-birth contract but separates final automatic source generation from the manual/debug injection primitive path. Automatic shore slots now create typed source events and a dedicated `RasterizeFoamSourceEvent` compute kernel evaluates shore-local analytic masks against `_FoamCurrentShoreEdgesRead`. The kernel writes real persistent material through `FoamMergeBornPresence`; support/lifetime capture, Layer D Film Source/Support, and Final Foam integration remain unchanged.

Implemented event types:

```text
ShoreRibbon
  live-shore-following ribbon band with deterministic breakup and tapered ends.

InwardWash
  shore-attached inward/downstream tongue with progressive area reveal and curvature.
```

The current UI remains Coverage, Activity, Patch Size, and Pattern. The old generic `PendingInjection` path remains available for manual/debug/simple births only.

### 4.11C.5.14F source formation rule

Automatic Layer C source events now separate three concepts that were previously coupled:

1. **Coverage** — which eligible shoreline slots can participate over time.
2. **Activity** — how often new source events start.
3. **Formation Speed** — how quickly a single source event forms along its path, in metres per second.

This keeps source density/frequency independent from source kinematics. The user-facing problem was that source events appeared as if a mask popped on in about one second. The fix is distance-based formation: a longer source path takes longer to form at the same formation speed.

Inward Wash also changes from a filled reveal mask to a moving stroke-head. The source rasterizer now writes a short curved head/trailing segment per update, while persistent FoamState preserves the trail. This preserves the Layer C rule: the rasterizer writes real material, not visual-only film, and Layer D only interprets that material afterward.

### 4.11C.5.14G shore wash refinement rule

`4.11C.5.14G` keeps the 5.14E/5.14F automatic source-event rasterizer architecture but tightens the `Inward Wash` source class. The scope is still shore-related Layer C spawning only.

The refined rule is:

- `Shore Ribbon` remains the primary validated shore source.
- `Inward Wash` is not a large filled tongue and not a broad moving body. It is a small detaching stroke that starts by following the shore and then peels inward.
- Wash events use separate, shorter head-trail limits from ribbons.
- Wash fill noise is low so shape is controlled by stroke geometry rather than chunky source-fill cells.
- `Mixed` is protected from bad wash dominance by greatly reducing Inward Wash weighting.

This patch still writes real persistent FoamState material through the Layer C rasterizer. It does not alter Layer D visual-film evaluation or Final Foam.


### 4.11C.5.14H foam birth authoring framework

`4.11C.5.14H` does not alter the Layer C source-event rasterizer contract. It changes authoring: shore source recipes are no longer hardcoded experimental constants. They are controlled by a source-category inspector framework.

Current source categories:

```text
Shore Foam      implemented source category
Object Foam     staged placeholder for later static-object/contact spawning
Free Water Foam staged placeholder for later open-water source spawning
```

The implemented Shore Foam category keeps Coverage and Activity as category-level density/rate controls. Pattern composition is controlled by normalized pattern shares whose sum is always one. This means changing `Shore Ribbons` versus `Inward Wash` changes which source type is selected in Mixed mode, not the total source rate.

Each implemented shore pattern now owns its own source-authoring controls: Formation Speed multiplier, dimensions, Initial Life, and Breakup Strength. `Initial Life` is the normalized Remaining Life written into newly born persistent material; it is not event duration. Event duration still derives from source path distance divided by formation speed.

Dimension selection now uses a correlated event scale plus small per-axis jitter and aspect guards. This preserves deterministic variety without allowing short/fat or reach/width-incoherent shore wash events.

### 4.11C.5.15A Object Foam source category

Object Foam extends the Layer C source-event rasterizer with static-object source events. The anchor list is exported from the existing disturbance runtime static source registry, keeping scheduling deterministic and bounded on CPU. GPU rasterization evaluates object-local contact arc/fleck masks, gates them by valid fluid, obstacle exclusion, and static pressure contact evidence, then merges births into persistent material state. This remains a spawning feature only; it does not change transport/evolution or Final Foam composition.

### 4.11C.5.15A.1 Object Foam activation correction

Object Foam activation is category-driven. `Spawn Preset` no longer silently disables Shore or Object source categories except when set to `Off`. The intended hierarchy is: `Automatic Foam Birth` global master switch, `Spawn Preset = Off` global disable, and per-category `Enabled` toggles for Shore/Object/Free Water. Object Foam runtime diagnostics include copied static source anchor count before events are scheduled.

### 4.11C.5.15A.2 Object Contact Edge Field

Object Foam now uses a local contact-edge field for final source shape authority. CPU static source snapshots still schedule bounded object events; the GPU contact field supplies per-cell contact confidence, object-to-water normal, and upstream/front-side relevance derived from obstacle exclusion plus static pressure/contact context.

This preserves the selected performance model: no GPU readback, no texture-wide source spawning, no particle system, and no connected-component event generation. Object extents remain as coarse bounds only. Contact Arc and Contact Fleck masks now use field normal/tangent space so they can follow actual contact edges rather than object half-extent rectangles.

## Addendum — 4.11C.5.15A.4 Object Contact Semi-Arcs

Object Foam now has three Layer C source recipes: full Contact Arcs, lopsided Contact Semi-Arcs, and Contact Flecks. This is still persistent material birth, not Layer D shape evaluation and not Final Foam rendering.

The reason for the additional recipe is mathematical: full Contact Arcs use a tangent-space mask centred by `abs(tangentDistance)`, which is stable but inherently symmetric. Contact Semi-Arcs use the same object-contact field and coarse object bounds, but carry deterministic signed lopsidedness through the existing source-event `Curvature` / GPU `variation.w` channel. The semi-arc evaluator projects into contact tangent space, multiplies by the signed side, and gates the source with a one-sided interval:

```text
-backReach < tangentDistance * side < revealedForwardReach
```

This keeps the selected performance model intact: no GPU readback, no connected-component extraction, no new textures or buffers, and no new object-contact resource binding. The object-contact field remains the 5.15A.2/5.15A.3.4 stable broad contact authority. Any future sharper edge-distance field correction must be a separate resource-audited patch.

### Layer C Free Water Birth — 4.11C.5.15B

Free Water Foam is now a Layer C source category alongside Shore Foam and Object Foam. It writes persistent material state through the same automatic source-event rasterizer instead of inserting final visual foam.

Implemented source grammars:

- **Lace Connector**: head+stroke emission along a curved open-water path. Earlier samples persist in FoamState while the head advances.
- **Torn Fragment**: asymmetric local fragment shape revealed by a timed sweep. It is patch-shaped, but not instant.

Bright glints/scratches from the visual reference remain out of Layer C. They belong to later shader/rendering work, not persistent material birth.

The source-event dispatch path now supports an optional Y range. Existing shore/object events dispatch the full field height; free-water events dispatch only the lateral band required by their local shape.

#### 4.11C.5.15B.2 Cross-Lace Connectors

Free Water Foam now has a third source grammar: **Cross-Lace Connector**. The original Lace Connector is flow-aligned because its sampled path runs along global distance. Cross-Lace swaps that path basis so the source head travels across the river laterally while the ribbon only bends slightly along flow. This is intended to supply the horizontal/cross-current pale ribbons visible in the visual target without increasing global spawn density or inserting final foam art.

Cross-Lace remains Layer C material birth only. It writes persistent FoamState through the existing automatic source-event rasterizer, is clipped by river boundary and obstacle exclusion, and uses the existing local X/Y dispatch bounds.

