# River Foam Material-State Correction Implementation Plan

## Document Status

**Status:** Canonical implementation plan. Patch 4.11C.3 is Unity-validated. Patch 4.11C.4 installed the atomic Presence/Life/Pattern state but failed visual acceptance because obsolete autonomous material guidance, non-conservative footprint transport, repeated boundary attenuation, and unhelpful raw-state diagnostics still controlled the visible result. Patch 4.11C.5 replaces those systems and has now passed the major lifetime/footprint validation gate, but Unity testing exposed transport-quality issues. Patch 4.11C.5.1 added Material Flow Speed and first-pass visual residue cleanup. Unity testing after C.5.1 showed that motion could still appear stepwise/laggy, so Patch 4.11C.5.2 raises the authoritative material cadence, tightens the transport Courant target, and adds temporal diagnostics before any topology or lateral-drift work proceeds. C.6–C.7 remain blocked.

**Supersedes:** the former monolithic proposal named `Patch 4.11C.3 — Minimal Material State, Lifetime Authority, and Topology Aging Correction`.

**Approved replacement sequence:**

1. `4.11C.3 — Source Quantity and Birth-Merge Correction`;
2. `4.11C.4 — Persistent Material-State Migration`;
3. `4.11C.5 — Material Footprint Preservation and Unified Lifecycle Diagnostics`;
4. `4.11C.5.1 — Material Flow Speed and Visual Residue Cleanup`;
5. `4.11C.5.2 — Transport Temporal Continuity`;
6. `4.11C.6 — Lifetime Authority and Presentation`;
7. `4.11C.7 — Validation, Regression Audit, and Documentation Closure`.

**Current gate:** validate `4.11C.5.2 — Transport Temporal Continuity` in Unity. Do not proceed to C.6 unless Foam motion is visually continuous at Material Flow Speed 1 and at faster settings, the Inspector temporal readouts show a changing interpolation alpha and stable material step cadence, source footprints remain materially distinct, and no obsolete guidance/spreading/reinforcement code has returned.

**Blocked until 4.11C.7 passes:**

- `4.11D` Anchored Birth Events;
- `4.11E` Open-Water Births and Spatial Fairness;
- `4.11F` Integrated Birth Population;
- all fracture, shredding, mature rendering, and final performance work.

**Primary implementation targets:**

- `Game/Procedural/Rivers/StylizedRiver.cs`;
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs`;
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs`;
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`;
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/*.hlsl`;
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`;
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`.

**Related canonical documents:**

- `River_Foam_Stage6_Architecture.md` — owns the permanent Stage 6 behavioural contract;
- `River_Rendering_Roadmap.md` — owns the concise milestone sequence;
- `River_Foam_Topology_Implementation_Plan.md` — owns topology only and records that topology generation remains closed;
- `River_Progressive_Initialization_and_Work_Scheduling_Plan.md` — owns initialization, scheduling, and performance safeguards.

No implementation patch may silently absorb behaviour assigned to a later patch. The only unavoidable atomic boundary is `4.11C.4`, where every producer and consumer of the persistent material texture must migrate together.

---

## 1. Why This Correction Exists

Patch 4.11C.1 proved that the progressive event trajectory and per-update source rasterization are correct. Patch 4.11C.2 proved that the source reaches the persistent material handoff. Unity testing then showed that changing Neutral Lifetime, Supported Aging Rate, and Negative Aging Rate produced little or no meaningful change in visible Foam survival.

The superseded pre-4.11C.4 persistent state was:

| Channel | Current meaning |
|---:|---|
| R | Amount |
| G | Amount × Remaining Life |
| B | Amount × Integrity |
| A | Phase / provenance |

The current solver and renderer give Amount several unrelated responsibilities:

- source intensity;
- persistent occupancy;
- render visibility;
- merge weight;
- disturbance reinforcement capacity;
- boundary attenuation;
- end-of-life decay;
- denominator for Remaining Life and Integrity.

Consequently, material may disappear while Remaining Life is still non-zero. It can also be rejuvenated by disturbance reinforcement or repeated source overlap. The lifetime controls may therefore be mathematically evaluated while remaining visually irrelevant.

This patch sequence separates four concepts that must not be conflated:

1. **Emitter Amount** — how much of a candidate source shape is born;
2. **Presence** — where persistent Foam occupies the simulation field;
3. **Remaining Life** — the sole ordinary survival clock;
4. **Material Pattern** — stable transported local variation reserved for future breakup and rendering.

---

## 2. Canonical Definitions

### 2.1 Emitter Amount

Emitter Amount is a temporary source coefficient in the normalized range `0–1`.

It answers:

> What spatial portion of the candidate birth shape is allowed to become Foam?

It does not answer:

- how bright the resulting Foam is;
- how durable it is;
- how quickly it ages;
- how much Remaining Life it receives;
- whether it survives transport;
- how strongly topology affects it.

Required endpoint behaviour:

- `0` creates no material;
- `1` makes the complete valid candidate shape eligible;
- intermediate values accept deterministic coherent subsets;
- a higher Amount produces a nested superset of every lower Amount for the same event, seed, and source geometry.

After source conversion, Amount is discarded. There is no persistent Amount channel.

### 2.2 Presence

Presence is a normalized `0–1` geometric-coverage field.

- `0` means no Foam occupies the texel;
- `1` means the texel is fully occupied by the Foam shape;
- intermediate values represent anti-aliased or sub-texel edge coverage.

Presence is used for:

- the persistent Foam silhouette;
- edge interpolation;
- transport of occupied material;
- valid-fluid clipping;
- rendering the base shape;
- future fracture geometry.

Presence is not lifetime, density, durability, or emitter strength.

### 2.3 Remaining Life

Remaining Life is a normalized material attribute in the range `1–0`.

- `1` means the complete selected life budget remains;
- `0.5` means half of the normalized life budget remains;
- `0` means the material has expired and its Presence is cleared.

Remaining Life is the sole ordinary survival clock. It is modified only by the approved topology aging equation and explicit future lifecycle systems.

### 2.4 Material Pattern

Material Pattern is a deterministic, smoothly varying `0–1` value generated when material is born and transported with it.

It exists so future breakup can vary locally without inventing unstable frame-to-frame noise. A later fracture calculation may combine:

```text
Age-driven breakup readiness
× Material-Pattern local variation
× approved flow/topology stress
```

In the 4.11C correction sequence Material Pattern:

- is generated;
- is packed;
- is transported;
- is preserved on overlap;
- remains internal after the C.4 channel proof;
- has no normal user-facing debug view until fracture gives it an approved visible job;
- does not affect survival or Final Foam appearance.

Material Pattern is linear. It is not circular Phase, a hard event ID, provenance ownership, or Integrity.

### 2.5 Reserved A Channel

The alpha channel has no proven persistent owner. Every source, clear, transport, simulation, and encode path must write it as zero.

This prevents stale Phase data from surviving the migration and keeps one channel available for a future property only after that property has a demonstrated requirement.

---

## 3. Final Persistent Material Contract

Beginning with Patch 4.11C.4, the persistent and per-step source textures use:

| Channel | Canonical meaning |
|---:|---|
| R | Presence |
| G | Presence × Remaining Life |
| B | Presence × Material Pattern |
| A | reserved zero |

### Decode

```text
Presence = saturate(R)

if Presence > epsilon:
    Remaining Life = saturate(G / Presence)
    Material Pattern = saturate(B / Presence)
else:
    Remaining Life = 0
    Material Pattern = 0
```

### Encode

```text
if Presence <= epsilon or Remaining Life <= 0:
    packed state = 0
else:
    packed state = float4(
        Presence,
        Presence × Remaining Life,
        Presence × Material Pattern,
        0)
```

### Why Life and Pattern are Presence-weighted

Premultiplication keeps attributes attached to occupied material during bilinear interpolation.

An interpolation between fully young occupied material `(R=1, G=1)` and empty water `(R=0, G=0)` produces `(R=0.5, G=0.5)`. Decoding gives `Remaining Life = 1`, meaning half coverage of fully young Foam. Directly storing unweighted Remaining Life would incorrectly produce a half-aged edge merely because the shape moved between texels.

The same rule applies to Material Pattern.

---

## 4. Non-Negotiable Ownership Rules

1. Amount is source-only and is discarded after birth conversion.
2. Presence owns shape occupancy, not lifespan.
3. Remaining Life owns ordinary survival.
4. Material Pattern has no lifecycle authority.
5. Ordinary transport does not age material.
6. Boundary coverage does not provide an exponential decay clock.
7. Positive support and Negative Aging Pressure modify only the Remaining Life clock.
8. Disturbance fields may influence motion and future event selection, but may not continuously add Presence or rejuvenate Remaining Life.
9. Source overlap may add newly occupied area but may not refresh already occupied material.
10. Final-life fading occurs inside Remaining Life and adds no post-lifetime tail.
11. Exact obstacle and outside-domain invalidation remain legal geometric removal.
12. Sub-resolution geometry may still be lost numerically; that is a resolution limitation, not an approved lifecycle mechanism.

Explicit legal removal paths are:

- Remaining Life reaches zero;
- the material leaves the simulated river domain;
- the material enters canonical solid Obstacle Footprint;
- the user clears Foam;
- existing freeze/disable/resource-lifecycle policy explicitly clears material;
- a later approved fracture/dissolution system removes material.

---

## 5. Patch 4.11C.3 — Source Quantity and Birth-Merge Correction

**Implementation status:** Unity-validated and accepted. Its temporary old persistent packing was removed by 4.11C.4.

### 5.1 Purpose

Correct the meaning of emitter Amount and all source-local merge behaviour while leaving the old persistent texture packing temporarily intact.

This patch proves that the source creates the right spatial quantity before the more invasive state migration.

### 5.2 Included Behaviour

#### Deterministic Amount-to-area conversion

For every candidate source texel:

1. evaluate capsule/ellipse geometric coverage;
2. evaluate canonical valid fluid;
3. evaluate a deterministic coherent birth-fill field from physical river coordinates and a dedicated event seed;
4. accept the location when the fixed birth-fill value is below the selected Amount;
5. retain fractional coverage only at anti-aliased source and fill boundaries.

The birth-fill pattern must:

- be deterministic for the same event and source coordinates;
- remain stable across updates;
- use physical river distance and lateral metres rather than raw texel indices;
- contain broad coherent features rather than checkerboard noise;
- preserve nested subsets as Amount increases;
- have a minimum physical feature scale large enough to survive Low-quality transport.

Birth-fill noise and Material Pattern are separate concepts and use separate salts/seeds.

#### Source-to-source union

Multiple progressive segments and multiple active events may write the same per-step source texture. The source texture must use geometric union rather than addition or last-writer replacement.

For existing source Presence `Pe` and incoming source Presence `Pi`:

```text
Added Presence = max(0, Pi - Pe)
Combined Presence = max(Pe, Pi)
```

The incoming source attributes apply only to `Added Presence`. This prevents update cadence, capsule overlap, event order, or simultaneous emitters from repeatedly strengthening the same source area.

#### Manual and progressive consistency

Manual one-frame injection and progressive ribbon source generation must share:

- Amount-to-area semantics;
- valid-fluid evaluation;
- deterministic source fill;
- source union rules;
- initial Remaining Life semantics.

Manual injection must bind and respect the canonical Obstacle Footprint, not only bank coverage.

#### Diagnostic fragment-chain correction

The manual fragment-chain helper currently gives lower-Amount fragments shorter Remaining Life. Remove that coupling. Amount scaling may change born area only; every accepted fragment receives the selected Initial Remaining Life.

#### Naming cleanup

Rename ambiguous source-only fields such as `PreviousAmount` to `PreviousEmissionAmount` or `PreviousSourceAmount`.

Do not change the proven trajectory calculation. The supplied baseline already adds `StartAcrossNormalized` exactly once; the previously proposed doubled-start correction is invalid and must not be applied.

### 5.3 Temporary Persistent-State Rule

During 4.11C.3 only, the persistent texture still uses the old Amount/Life/Integrity/Phase format. Accepted source regions may write full local Amount into that old state; the public Amount slider determines which regions are accepted rather than reducing the amplitude of every accepted region.

This temporary compatibility ends in 4.11C.4.

### 5.4 Expected Files

- `StylizedRiver.cs` — remove fragment-chain Amount-to-life coupling; update source-only tooltip text if safe in this patch;
- `StylizedRiverEditor.cs` — update source diagnostic help where necessary;
- `StylizedRiverFoamRuntime.State.cs` — rename source-only fields without changing persistent packing;
- `StylizedRiverFoamRuntime.BirthEvents.cs` — retain trajectory, provide dedicated source-fill seed;
- `StylizedRiverFoamRuntime.BirthTransfer.cs` — bind source-fill parameters;
- `StylizedRiverFoamRuntime.BirthDiagnostics.cs` — use identical source semantics;
- `StylizedRiverFoamRuntime.Injection.cs` — bind canonical obstacle exclusion and source-fill seed;
- `CS_RiverFoam.Noise.hlsl` — add deterministic coherent source-fill helpers;
- `CS_RiverFoam.Resources.hlsl` — add source-fill uniforms only;
- `CS_RiverFoam.compute` — rewrite manual/progressive source acceptance and source-to-source union;
- relevant canonical docs and status text.

### 5.5 Explicit Exclusions

- no new persistent channel contract;
- no Material Pattern channel;
- no transport correction;
- no boundary-survival correction;
- no renderer rewrite;
- no lifetime-authority claim;
- no automatic births;
- no trajectory redesign.

### 5.6 Acceptance Gate

Pass only if:

- Amount `0` creates no source;
- Amount `1` creates the complete valid candidate source;
- Amount `0.2`, `0.5`, and `1.0` produce nested coherent spatial subsets;
- accepted material receives identical Initial Remaining Life regardless of Amount;
- repeated segment overlap does not strengthen already accepted source area;
- source behaviour is independent of material update cadence;
- manual injection cannot place material inside canonical obstacles;
- the progressive source trajectory, counters, and transfer diagnostic remain correct;
- no automatic material population returns.

---

## 6. Patch 4.11C.4 — Persistent Material-State Migration

**Implementation status:** Atomic channel migration completed, but visual acceptance failed. The channel contract itself remains authoritative; C.5 must remove the obsolete systems that expanded and erased the material carrying that state.

### 6.1 Purpose

Atomically replace persistent Amount, Integrity, and material Phase with Presence, Remaining Life, and Material Pattern.

Every producer and consumer of the material texture must migrate in this patch. A partial delivery is invalid.

### 6.2 Semantic Helper Module

Repurpose `CS_RiverFoam.Simulation.hlsl` from obsolete Amount/Phase neighbourhood logic into the canonical material-state helper module.

Define a semantic structure equivalent to:

```hlsl
struct FoamMaterialState
{
    float presence;
    float remainingLife;
    float materialPattern;
};
```

Provide central helpers for:

- decoding packed state;
- encoding packed state;
- clamping moments;
- clearing expired/empty state;
- merging newly born Presence;
- later valid-fluid clipping and contour stabilization.

Compute kernels and rendering code should stop manually repeating raw channel interpretation wherever practical.

### 6.3 CPU State Migration

Remove persistent material uses of:

- Integrity;
- Phase;
- Amount-decaying reservation state.

Progressive event data retains source-only Amount and adds a dedicated PatternSeed. Manual injection receives a corresponding pattern seed.

The obsolete public `freshness` parameter name and `FoamTestFreshness` alias are removed. Manual birth APIs use Initial Remaining Life terminology only.

### 6.4 Pattern Generation

Each event derives a PatternSeed from:

- river visual seed;
- event sequence identity;
- a dedicated pattern salt independent of trajectory and source-fill salts.

The source rasterization kernel generates a broad, coherent pattern in physical river coordinates. Pattern frequency is internal during this proof and is not exposed as an artistic control until a later system visibly uses it.

### 6.5 Persistent Merge Rule

For existing material `(Pe, Le, Me)` and source `(Ps, Ls, Ms)`:

```text
Added Presence = max(0, Ps - Pe)
Combined Presence = max(Pe, Ps)

Combined life moment = Pe × Le + Added Presence × Ls
Combined pattern moment = Pe × Me + Added Presence × Ms
```

Consequences:

- if existing Presence is already greater than or equal to source Presence, no rejuvenation occurs;
- if the source expands a partially covered texel, only the newly occupied fraction receives fresh life and pattern;
- existing Material Pattern is preserved on already occupied material;
- repeated source passes cannot make old material immortal.

The per-step source texture uses the same union rule internally.

### 6.6 Remove Old Material Logic

Remove active material uses of:

- Integrity;
- material Phase/provenance;
- circular Phase interpolation and mixing;
- Amount/Phase neighbourhood cohesion logic;
- persistent Amount reinforcement;
- disturbance-based life rejuvenation;
- reinforcement-based Integrity mixing;
- independent Amount end-of-life decay;
- old Amount visibility thresholds.

Disturbance-driven motion remains. C.5 completes the ownership cleanup by retaining only explicitly named Wake/Lee and Pressure motion influences; no reinforcement-named material uniform remains.

### 6.7 Minimal Renderer Migration

The renderer decodes:

- Presence;
- Remaining Life;
- Material Pattern.

During this patch:

- Presence becomes the base Foam shape;
- Integrity and Phase no longer affect mask, colour, edge noise, or brightness;
- Material Pattern remains visually inert except in its dedicated diagnostic;
- expiry may be a simple clear at Remaining Life zero until 4.11C.6 adds the approved life-derived dissolve presentation.

### 6.8 Diagnostics

C.4 temporarily added raw Presence, Remaining Life, and Material Pattern diagnostics to verify the atomic channel migration. Unity validation showed that these were not useful primary workflow views. C.5 removes them and leaves only Final Foam, Foam + Aging Topology, Progressive Birth Source, and Progressive Birth Transfer with compact current enum values.

### 6.9 Expected Files

- `StylizedRiver.cs`;
- `StylizedRiverEditor.cs`;
- `StylizedRiverFoamRuntime.State.cs`;
- `StylizedRiverFoamRuntime.BirthEvents.cs`;
- `StylizedRiverFoamRuntime.BirthTransfer.cs`;
- `StylizedRiverFoamRuntime.BirthDiagnostics.cs`;
- `StylizedRiverFoamRuntime.Injection.cs`;
- `StylizedRiverFoamRuntime.Compute.cs`;
- `StylizedRiverFoamRuntime.Constants.cs`;
- `StylizedRiverFoamRuntime.Binding.cs`;
- `StylizedRiverFoamRuntime.PublicSurface.cs`;
- `CS_RiverFoam.Resources.hlsl`;
- `CS_RiverFoam.Sampling.hlsl`;
- `CS_RiverFoam.Simulation.hlsl`;
- `CS_RiverFoam.Noise.hlsl`;
- `CS_RiverFoam.Motion.hlsl` for audit/preserved motion use;
- `CS_RiverFoam.compute`;
- `RiverWaterFoam.hlsl`;
- `SH_CleanStylizedRiver.shader`;
- canonical documents.

### 6.10 Explicit Exclusions

- no final boundary clipping correction;
- no final presentation tuning beyond the C.4 proof renderer;
- no Local Aging Response diagnostic;
- no mature dissolution presentation;
- no automatic births;
- no fracture or shredding;
- no use of Pattern in normal rendering.

### 6.11 Acceptance Gate

Pass only if:

- R means Presence everywhere;
- G means Presence × Remaining Life everywhere;
- B means Presence × Material Pattern everywhere;
- A is zero everywhere;
- manual and progressive births share the same format;
- source overlap does not rejuvenate existing material;
- disturbance fields no longer add Presence or Remaining Life;
- Material Pattern is coherent, moves with material, and does not flicker;
- Final Foam renders from Presence;
- all Integrity- and material-Phase-based rendering is inactive;
- the progressive source and transfer diagnostics remain functional.

Failure found during Unity validation: tiny and wide sources converged toward the same river-wide footprint, and visibly supported Foam disappeared after only several seconds despite much longer calculated life. The causes were obsolete procedural material guidance, hidden lateral attraction/spread, non-footprint-preserving transport, repeated boundary attenuation, and a renderer contour that could hide live material. C.4 is therefore not accepted as a behavioural patch; its state format is retained as the corrected foundation for C.5.

---

## 7. Patch 4.11C.5 — Material Footprint Preservation and Unified Lifecycle Diagnostics

**Implementation status:** Implemented in code; focused Unity validation pending.

### 7.1 Purpose

Ensure that a birth event establishes the material footprint and that later motion deforms and transports that footprint without replacing it with an autonomous river-wide population. Remove every obsolete material-guidance and spreading path, make transport conservative and interface-preserving, apply valid-fluid clipping idempotently, and provide two primary views that show the exact final Foam and its complete aging context.

The patch contract is:

> Existing Foam may move and deform, but it may not autonomously grow into a materially larger patch; topology changes the Remaining Life clock, not material velocity or Presence.

### 7.2 Damage Confirmed After C.4

Unity validation proved that the C.4 state migration alone was insufficient:

- a `0.2 m × 0.2 m` source expanded toward a river-wide sheet;
- a tiny instant patch and a broad arc converged toward nearly the same final footprint;
- material configured for very long supported life disappeared visibly after only several seconds;
- raw Presence and Pattern views did not show the final player-facing result or the topology responsible for aging it.

The identified causes were:

1. an inherited procedural lane/network field continuously steering all material;
2. hidden boundary attraction, generic lateral spread, and wandering/evolution velocity;
3. backward scalar advection that could duplicate occupancy across divergent destination samples;
4. repeated multiplication by fractional bank coverage;
5. a final `Presence ≈ 0.5` contour that could hide material while Remaining Life was still high;
6. diagnostics organised around internal channels rather than the actual visible material/topology interaction;
7. live Pressure, Lee, and Shore composition was incorrectly tied to an active topology debug view, allowing Final Foam to age against stale dynamic topology.

These systems are obsolete and are removed rather than disabled or retained as compatibility code.

### 7.3 Exact Motion Ownership

Persistent material velocity may contain only:

1. canonical signed downstream river flow;
2. accepted physical Wake/Lee disturbance motion;
3. accepted physical Pressure disturbance motion;
4. later explicitly approved fragment motion after fracture exists.

Persistent material velocity must not contain:

- procedural lane or Voronoi-network attraction;
- tangent wandering around an autonomous network;
- Major, Connector, Shore, or Negative topology steering;
- generic bank attraction or shore suction;
- generic lateral spread/evolution coefficients;
- hidden material reinforcement or rejuvenation;
- Impact Ripple motion unless separately approved as material motion.

Positive Support and Negative Aging Pressure affect only the Remaining Life calculation. Shore Support may preserve material near a bank, but it does not pull material toward the bank.

Dynamic topology ownership is independent of diagnostics. Every active material step refreshes and composes current Pressure, Lee, Shore, and evolving generated topology before the material solver consumes it, including while **Final Foam** is selected. The combined view displays that authoritative state; it does not activate or alter it. Low-rate metric readback remains limited to the combined view, explicit profiling, or an active manual footprint proof.

### 7.4 Mandatory Physical Deletions

The following obsolete modules are deleted from the project, including their Unity metadata:

- `StylizedRiverFoamRuntime.Guidance.cs`;
- `StylizedRiverFoamRuntime.Guidance.cs.meta`;
- `CS_RiverFoam.Network.hlsl`;
- `CS_RiverFoam.Network.hlsl.meta`.

All guidance textures, kernels, allocations, initialization phases, update cadence, bindings, memory accounting, transition ownership, profiling labels, serialized shader properties, and runtime status text are removed. No inert compatibility declaration remains for these material systems.

The old `AdvectForward`/`AdvectReverse` MacCormack path, old forward/reverse resource names, cubic post-advection sharpening proof, boundary-attraction controls, generic material spread/evolution constants, disturbance reinforcement, old raw material debug modes, obsolete Freshness API naming, and the unused boundary-attraction payload are also removed. The boundary texture now has one semantic value only: valid-water coverage in R; G/B/A are always zero.

### 7.5 Conservative Sharp-Interface Transport

The replacement is a two-stage conservative finite-volume update:

1. conservative predictor;
2. conservative SSP-RK2 corrector;
3. conservative pairwise interface compression along disjoint horizontal and vertical pairs;
4. lifecycle and source merge.

Presence is reconstructed at transport faces with a bounded THINC sharp-interface profile when a resolved monotone interface exists. Uniform regions, plateaus, and local extrema use a bounded monotonized-central reconstruction. This combination keeps real material boundaries sharp without applying a non-conservative post-process that manufactures or destroys Presence.

At every face:

- the upwind Presence determines the material flux;
- normalized Remaining Life and Material Pattern from the same upwind material travel with that Presence flux;
- the packed invariants remain `0 ≤ G ≤ R`, `0 ≤ B ≤ R`, and `A = 0`;
- the same face flux is shared by adjacent cells, so one cell's loss is the neighbour's gain.

The transport Courant number is bounded per axis and the runtime raises update cadence when authored downstream speed requires it. Reverse-flow rivers use signed downstream velocity rather than a hard-coded positive direction.

Chunk scheduling keeps one downstream chunk as a **transport work halo** beyond each known CPU reservation. Conservative face flux must update both donor and receiver cells when material crosses a chunk boundary; the halo prevents the receiver half from being skipped. It changes dispatch coverage only. It does not paint, spread, reserve extra lifetime for, or otherwise alter Foam material.

There is no post-transport cubic Presence sharpening. The former local `3×3` sharpening/renormalisation proof was rejected because it could destroy integrated material even though it appeared locally sharper.

The retained compression step is conservative and pairwise: it moves packed Presence/Life/Pattern moment from the lower-Presence member of one disjoint adjacent pair toward the higher-Presence member, clamped by donor availability and receiver capacity. Each pass preserves the pair's total Presence, Remaining-Life moment, and Material-Pattern moment. Horizontal even, horizontal odd, vertical even, and vertical odd passes prevent one diffuse interface from becoming a broad invisible haze without creating new material or restoring any deleted autonomous network.

### 7.6 Footprint Preservation Contract

For an isolated resolved source in neutral unobstructed water with no new births and no active disturbance:

- integrated Presence must remain approximately constant;
- the final visible core must remain recognisable;
- a tiny source must not expand to the river width;
- a tiny patch and a wide arc must remain materially distinguishable;
- transport may translate the shape and introduce bounded sub-texel edge interpolation;
- sub-resolution details may still be lost and are a resolution limit, not a lifecycle rule.

Wake or Pressure motion may bend, stretch, or separate a footprint locally. Because the update is conservative, an enlarged geometric extent must be accompanied by thinning or separation rather than a newly created solid sheet.

The runtime exposes development metrics:

```text
Integrated Presence Area
Final Foam Core Area
Manual Proof Presence Ratio = current integrated area / captured post-birth area
```

The ratio is diagnostic only. It is not a population controller and never feeds simulation behaviour. Once a manual proof captures its post-birth reference, its low-rate area readback continues in Final Foam as well as the composite view so the displayed ratio is current rather than frozen. Normal Final Foam with no active manual proof does not request diagnostic readback.

### 7.7 Canonical Valid Fluid

Use one authoritative mask:

```text
Valid Fluid = Boundary Coverage × (1 - Canonical Obstacle Footprint)
```

If Valid Fluid is effectively zero, clear the state.

For partial coverage:

```text
Clipped Presence = min(Presence, Valid Fluid)
```

Decode Life and Pattern, replace only Presence, and re-encode the same normalized attributes. Repeating the clip at the same bank is idempotent and cannot create exponential decay.

`_FoamObstacleExclusion` is the only obstacle source. `_FoamTopology.a` is reserved zero; the former obstacle compatibility copy is deleted from producers, consumers, renderer bindings, and documentation.

### 7.8 Primary Diagnostics

The normal Inspector exposes only four Foam views with compact current enum values `0–3`; obsolete raw-state/debug values are not retained:

1. **Final Foam** — exact player-facing Foam after transport, life, clipping, temporal interpolation, lighting, and transported Presence coverage;
2. **Foam + Aging Topology** — one combined lifecycle-validation view;
3. **Progressive Birth Source** — source rasterisation proof;
4. **Progressive Birth Transfer** — source/persistent merge proof.

`Foam + Aging Topology` uses:

| Information | Colour |
|---|---|
| neutral valid water | near-black |
| maximum positive support | green |
| Negative Aging Pressure | red |
| positive/negative overlap | additive yellow |
| canonical Obstacle Footprint | blue |
| exact final visible Foam coverage | bright cyan/white |

The Foam overlay uses the same final transported-coverage `foam.mask` as normal rendering. Its brightness decreases with Remaining Life so visible material, age, and current topology context can be judged together.

Raw Presence, Remaining Life, Material Pattern, and individual topology class views are removed from the normal workflow. Material Pattern remains valid internal state for later breakup but has no user-facing view until it receives an approved visible job.

### 7.9 Expected Files

- `StylizedRiver.cs`;
- `Editor/StylizedRiverEditor.cs`;
- `StylizedRiverFoamRuntime.*.cs` resource, lifecycle, compute, binding, diagnostics, and scheduling modules;
- deletion of the old guidance runtime module;
- `CS_RiverFoam.compute`;
- `CS_RiverFoam.Motion.hlsl`;
- new `CS_RiverFoam.Transport.hlsl`;
- `CS_RiverFoam.Resources.hlsl`;
- `CS_RiverFoam.Sampling.hlsl`;
- `CS_RiverFoam.Noise.hlsl`;
- `CS_RiverFoam.Simulation.hlsl`;
- `CS_RiverFoam.Topology.hlsl`;
- deletion of `CS_RiverFoam.Network.hlsl`;
- `RiverWaterFoam.hlsl`;
- `SH_CleanStylizedRiver.shader`;
- all five canonical river/Foam documents;
- the historical compute-refactor baseline receives an explicit superseded-contract warning only.

### 7.10 Explicit Exclusions

- no automatic anchored or open-water births;
- no fracture, holes, edge shredding, or fragments;
- no topology generation/cache redesign;
- no global target-coverage controller;
- no post-life dissolution tail;
- no new artistic spread or guidance controls;
- no attempt to make sub-resolution geometry permanent.

### 7.11 Acceptance Gate

#### Tiny neutral patch

Inject a resolved `0.2 m × 0.2 m` patch into neutral open water.

Expected:

- it moves downstream;
- it does not expand to the river width;
- it does not converge toward a hidden lane/network shape;
- Integrated Presence Area and Manual Proof Presence Ratio remain close to their captured post-birth values until aging or geometric exit legitimately removes material.

#### Small-versus-large distinction

Test a tiny patch and a broad arc in separate clean runs. Their later footprints must remain recognisably different.

#### Lifetime visibility

With Neutral Lifetime `10` in verified neutral water, the exact Final Foam should remain visible close to ten seconds unless it exits the valid simulation region. With full support and Supported Aging Rate `0.1`, the life slope must predict approximately `100 s` and the material must plainly survive beyond neutral timing.

#### Topology interaction

The combined view must show exact final Foam travelling across green support, red negative pressure, yellow overlap, neutral water, and blue obstacles. Support and negative fields modify Remaining Life but never directly create, steer, or erase Presence.

#### Boundaries and obstacles

Near-bank material clips once rather than shrinking every update. Obstacle interiors remain empty. Reverse flow remains correct.

#### Obsolete-code audit

No deleted guidance/network file, kernel, texture, allocation, field, property, profiler label, status text, or old advection/sharpening path may remain in the active project.

---



## 7.1 Patch 4.11C.5.1 — Material Flow Speed and Visual Residue Cleanup

Unity validation of C.5 confirmed that the catastrophic lifetime failure is gone: long lifetime settings now visibly preserve Foam. The remaining issues are transport-quality controls rather than state-contract failures. Existing Foam can feel too slow relative to the visible water, and finite-volume transport can leave very low-coverage visual crumbs behind the main moving footprint.

C.5.1 therefore adds exactly two corrections:

1. **Material Flow Speed** — a serialized Foam material movement multiplier relative to the authored river Flow Speed. The value affects persistent-material downstream transport, CPU material reservations, and conservative transport update-rate stability. It does not change source Amount, Remaining Life, topology aging, water-surface motion, or automatic birth scheduling. A value of `1` follows the river Flow Speed; values above `1` move existing Foam faster downstream; `0` freezes ordinary downstream material drift while still allowing explicit birth, topology aging, disturbances, and valid-fluid clipping.
2. **Visual residue cleanup** — final Foam rendering suppresses very low-coverage numerical crumbs left by conservative transport, while conservative interface compression is made slightly stronger so the main footprint reabsorbs edge residue instead of leaving a long faint tail. This cleanup does not reintroduce an Amount threshold, hidden death clock, network attraction, bank suction, source replenishment, or disturbance reinforcement. Stored Remaining Life remains authoritative; the cleanup targets sub-useful coverage crumbs that should not be presented as player-facing Foam.

C.5.1 explicitly does **not** restore any removed C.5 system. The procedural material network, guidance texture, lane capture, shore suction, generic spread/evolution steering, material reinforcement, material rejuvenation, old forward/reverse advection, and raw-state primary diagnostics remain deleted.

Acceptance for C.5.1:

- `Material Flow Speed = 1` behaves like C.5 baseline transport.
- Increasing Material Flow Speed visibly accelerates downstream Foam motion.
- Fast values remain stable because the runtime update rate accounts for the multiplier.
- Reservations follow the same multiplied material speed so moving Foam does not lose active chunks.
- The main footprint remains recognizable and does not inflate into a river-wide sheet.
- Low-coverage crumbs behind the main footprint are not visible in Final Foam or Foam + Aging Topology.
- Lifetime validation from C.5 remains intact.

## 7.2 Patch 4.11C.5.2 — Transport Temporal Continuity

Unity validation after C.5.1 showed that Foam lifetime is materially improved, but persistent Foam can still appear to travel in visible fixed-step jumps. This makes every later support, negative-pressure, residue, and obstacle test harder to trust. C.5.2 therefore addresses only the temporal transport clock and its diagnostics. It does not attempt residue classification, topology calibration, lateral drift, or obstacle sliding.

C.5.2 changes the material transport cadence from the old low-frequency proof values to internal temporal-continuity rates:

- Low quality: `45 Hz`;
- Medium quality: `60 Hz`;
- High quality: `90 Hz`;
- the stability resolver may still raise the cadence further when Flow Speed and Material Flow Speed require it.

The internal transport Courant target is tightened from `0.45` to `0.28` cells per material step. This reduces visible grid jumps and gives the conservative transport/compression pair a smaller, more stable displacement to process. The patch deliberately raises cadence instead of splitting source/lifetime work into hidden substeps because naïvely substepping the complete solver would reapply birth transfer and aging in ways that change event semantics.

C.5.2 also adds Inspector diagnostics for:

- authoritative material update rate;
- material step duration;
- material steps run during the most recent Unity frame;
- render interpolation alpha bound to the water shader;
- estimated downstream cells per material step;
- transport substeps used;
- compression passes run during the most recent frame.

The diagnostics are not artistic controls. They exist to prove whether visible stepping is caused by low cadence, catch-up bursts, stuck interpolation alpha, excessive cell displacement per step, or compression pulsing.

C.5.2 explicitly keeps all C.5 deletions in force. It does not restore the procedural material network, lane attraction, tangent wandering, shore suction, generic spread/evolution steering, topology-as-motion, material reinforcement, material rejuvenation, or old MacCormack advection. Material motion remains downstream flow plus accepted wake/lee and pressure disturbance motion only.

Acceptance for C.5.2:

- Material Flow Speed `1` moves smoothly rather than in obvious bursts.
- Material Flow Speed `2–4` moves faster without becoming chunkier.
- Render Interpolation Alpha changes continuously between steps rather than sticking at `0` or `1`.
- Material Steps Last Frame remains stable; repeated catch-up bursts indicate failure.
- Estimated Cells / Step stays below the internal transport limit.
- Compression Passes Last Frame is a balanced multiple of four when transport runs.
- No obsolete transport/guidance/spread/reinforcement system returns.


## 8. Patch 4.11C.6 — Lifetime Authority and Presentation

### 8.1 Purpose

Make Remaining Life the sole ordinary survival clock throughout simulation, CPU activity reservation, diagnostics, and rendering.

### 8.2 Exact Topology Aging Equation

Positive Influence is:

```text
max(Major, Connector, Pressure, Lee, Shore)
```

The first material implementation consumes the aggregate Negative Aging Pressure field.

```text
Positive Factor = lerp(1, Supported Aging Rate, Positive Influence)
Negative Factor = lerp(1, Negative Aging Rate, Negative Aging Pressure)
Local Aging Rate = Positive Factor × Negative Factor

Remaining Life = max(
    0,
    Remaining Life
    - Delta Time × Local Aging Rate / Neutral Lifetime)
```

No Amount, Presence, or Pattern term appears in this equation.

The multiplicative overlap is permanent unless later visual evidence explicitly reopens it.

Default arithmetic:

- neutral: `4 s`;
- full support at `0.2×`: `20 s`;
- full negative at `4×`: `1 s`;
- full support plus full negative at `0.8×`: `5 s`.

### 8.3 End-of-Life Presentation

Remove every remaining post-life Amount tail.

Use a normalized renderer lifecycle factor:

```text
Lifecycle Visibility = smoothstep(0, 0.35, Remaining Life)
```

- above `0.35`: full lifecycle visibility;
- from `0.35` to `0`: gradual fade/dissolution presentation;
- at `0`: persistent state is cleared.

The `0.35` proof band is the existing documented normalized final-life band. It is internal and may be revisited with mature fracture/dissolution, but it adds no extra time beyond the selected lifetime.

Final Foam should be approximately:

```text
Transported Presence coverage
× Lifecycle Visibility
× unfrozen factor
× Foam Colour alpha
```

No obsolete hidden proof property may remain in the active shader or binding surface. Compatibility is not a justification for retaining dead Foam-material behaviour.

### 8.4 CPU Activity Reservation Validation

C.5 already removed reservation `RemainingAmount`, Amount decay, spread-radius growth, and the separate post-life tail. C.6 must retain and validate the resulting reservation contract:

- centre global distance follows signed downstream flow;
- along-flow extent remains conservative but does not grow from a spread estimate;
- elapsed time is compared with one conservative maximum active duration;
- one downstream chunk remains a transport work halo only.

For initial normalized Remaining Life `L0`:

```text
Maximum duration = L0 × Neutral Lifetime / Supported Aging Rate
```

This uses the slowest selected approved aging rate. The existing global safety cap may remain. No separate dissolve seconds are added because the fade occurs inside Remaining Life. C.6 acceptance must prove that supported living material is never cleared early by CPU scheduling.

### 8.5 Metrics and Timing Evidence

C.5 already converted material/topology coverage metrics to Presence and added Integrated Presence Area, Final Foam Core Area, and Manual Proof Presence Ratio. C.6 retains those definitions and uses them alongside Local Aging Response and visible timing; it does not reintroduce an Amount threshold or another population controller.

### 8.6 Local Aging Response Diagnostic

Add an always-available view showing the exact Local Aging Rate calculated from the same topology and Inspector values used by the simulation:

- blue/cyan: slower than neutral;
- neutral gray/white: `1×`;
- orange/red: faster than neutral;
- black: invalid fluid or obstacle.

Bind Supported Aging Rate and Negative Aging Rate to the water material for this diagnostic. Bind one authoritative dissolution-start value rather than duplicating unrelated literals.

### 8.7 Inspector Semantics

Required tooltip meanings:

- **Amount:** source-only coefficient controlling how much candidate birth area becomes Foam; discarded after birth; no lifetime or durability effect;
- **Neutral Lifetime:** approximate total ordinary life of newly born Foam in neutral water; final fade occurs inside this time;
- **Supported Aging Rate:** full-support multiplier applied only to the Remaining Life clock;
- **Negative Aging Rate:** full-negative multiplier applied only to the Remaining Life clock.

Remove descriptions promising Integrity weakening or Phase evolution.

Correct the transfer diagnostic legend:

- red: current Source Presence;
- green contribution: newly accepted Presence;
- blue: pre-existing Presence;
- yellow: source that created new Presence;
- magenta: source overlapping existing material;
- white: source, newly accepted partial Presence, and existing Presence overlap.

### 8.8 Expected Files

- `StylizedRiver.cs`;
- `StylizedRiverEditor.cs`;
- `StylizedRiverFoamRuntime.Injection.cs` only if validation exposes a reservation defect;
- `StylizedRiverFoamRuntime.Constants.cs`;
- `StylizedRiverFoamRuntime.Compute.cs`;
- `StylizedRiverFoamRuntime.Binding.cs`;
- `StylizedRiverFoamRuntime.Topology.cs`;
- `StylizedRiverFoamRuntime.PublicSurface.cs`;
- `CS_RiverFoam.Resources.hlsl`;
- `CS_RiverFoam.Sampling.hlsl`;
- `CS_RiverFoam.Noise.hlsl`;
- `CS_RiverFoam.Simulation.hlsl`;
- `CS_RiverFoam.compute`;
- `RiverWaterFoam.hlsl`;
- `SH_CleanStylizedRiver.shader`;
- canonical documents.

### 8.9 Explicit Exclusions

- no automatic anchored/open-water births;
- no use of Pattern in normal rendering;
- no fracture masks, holes, strips, fragments, or shredding;
- no final artistic reference matching;
- no broad performance architecture changes.

### 8.10 Acceptance Gate

At controlled locations where the Local Aging Response is known:

- Neutral Lifetime `1`, `4`, and `10` produces approximately `1`, `4`, and `10` seconds respectively;
- full support at defaults produces approximately `20` seconds while the material remains in support;
- full negative produces approximately `1` second;
- full positive/negative overlap produces approximately `5` seconds;
- negative pressure accelerates the clock rather than erasing Presence instantly;
- Amount changes born area but not the life of accepted material;
- the renderer and Remaining Life diagnostic reach zero together;
- no approximately fixed old death time remains;
- the final-life fade ends at zero and adds no post-life tail;
- CPU reservations cannot clear a supported living chunk early.

---

## 9. Patch 4.11C.7 — Validation, Regression Audit, and Documentation Closure

### 9.1 Purpose

Prove that C.3–C.6 form one coherent system and close the manual progressive-birth/lifecycle correction before automatic population begins.

This patch contains no new visual feature beyond diagnostics required for verification.

### 9.2 Development Packed-State Invariant Check

Add an Editor/development-only counter or bounded readback verifying:

```text
0 <= R <= 1
0 <= G <= R
0 <= B <= R
abs(A) <= epsilon
```

This diagnostic is not a permanent per-frame production cost. It exists to detect forgotten old-format writes, conservative-transport overshoot, stale reserved-channel data, or invalid moments.

### 9.3 Semantic Static Audit

Search specifically for old material meanings, not every use of common words elsewhere.

Old material logic that must be absent:

- persistent material Amount;
- material Integrity;
- material Phase/provenance;
- `_FoamInjectionIntegrity`;
- `_FoamInjectionPhase`;
- Amount visibility thresholds;
- post-lifetime Amount decay;
- disturbance material reinforcement;
- disturbance life rejuvenation;
- circular material Phase mixing;
- old Amount/Phase neighbourhood cohesion logic;
- reservation RemainingAmount decay.

Legitimate unrelated uses remain, including topology Amount controls, initialization/evolution phases, wave phase, and source-only Amount.

### 9.4 Full Unity Validation Matrix

#### Empty baseline

Clear Foam and wait at least ten seconds. No material appears and Presence remains black.

#### Amount independence

Run clean tests at Amount `0.2`, `0.5`, and `1`. Confirm nested area changes and identical life for matching accepted locations.

#### Neutral lifetime

At a confirmed `1×` Local Aging Response, test Neutral Lifetime `1`, `4`, and `10`.

#### Full support

At confirmed full support with defaults, verify approximately `0.2×` and `20 s` while the material remains in that region.

#### Full negative

At confirmed full negative, verify approximately `4×` and `1 s`.

#### Positive/negative overlap

At confirmed full overlap, verify approximately `0.8×` and `5 s`.

#### Progressive timing

With a multi-second event, confirm earlier sections are older than later sections and the final section receives one full local lifetime after its own birth rather than after button press.

#### Overlap without rejuvenation

Cross old material with a second event. Existing occupied material retains its age and Pattern; only newly expanded Presence receives fresh attributes.

#### Pattern persistence

Pattern moves with material, remains stable as life changes, and is not overwritten on occupied overlap.

#### Boundary and obstacles

Near-bank Presence is clipped idempotently; obstacle interiors remain empty; valid material still obeys Remaining Life.

#### Quality and cadence

Repeat equivalent tests at Low, Medium, and High quality/update cadence. Lifetime in seconds remains stable even though edge precision differs.

#### Runtime lifecycle

Verify reverse flow, freeze/thaw, disable/re-enable, quality/domain reallocation, component disable, destruction, and Clear Foam. No stale old-format state appears.

### 9.5 Documentation Closure

Update the four canonical river documents and this plan so all agree on:

- the final Presence/Life/Pattern contract;
- source-only Amount;
- Remaining Life authority;
- Material Pattern’s future role and current visual inactivity;
- no active generic Integrity;
- no active material Phase;
- disturbance motion versus material ownership;
- idempotent valid-fluid clipping;
- actual C.3–C.7 validation results;
- 4.11D remaining blocked or becoming unblocked based on evidence.

Mark the old monolithic C.3 document superseded rather than retaining it as a competing current plan.

### 9.6 Acceptance Gate

C.7 passes only if:

1. all packed-state invariants pass;
2. the full Unity matrix passes;
3. every producer and consumer uses the final contract;
4. no hidden ordinary survival or rejuvenation mechanism remains;
5. the four canonical docs agree;
6. topology generation and caches remain unchanged;
7. source and transfer diagnostics remain correct;
8. Final Foam visibly grows along the progressive source;
9. lifetime controls produce the predicted visible timing;
10. 4.11D is explicitly unblocked only after user acceptance.

---

## 10. Final Dispatch Order After C.6

The fixed material update order is:

1. clear the current per-step birth source;
2. advance active events;
3. rasterize source Presence/Life/Pattern;
4. update conservative time-based activity reservations;
5. run conservative sharp-interface transport predictor;
6. run conservative SSP-RK2 transport corrector;
7. simulate material:
   - decode transported state;
   - apply idempotent canonical valid-fluid clipping;
   - calculate topology age rate;
   - reduce Remaining Life;
   - clear expired material;
   - merge per-step source without rejuvenating occupied material;
   - encode Presence/Life/Pattern;
8. swap temporal state textures;
9. bind previous/current state and the selected high-level diagnostic to the water material.

Source merge occurs after aging so material born during the current dispatch begins with its complete selected Initial Remaining Life and starts aging on the next material update.

There is no guidance-build phase, forward/reverse backtrace pair, MacCormack estimate, post-advection cubic sharpening, continuous material reinforcement, or bank-coverage multiplication.

---

## 11. Resource and Performance Contract

No extra persistent material texture is required.

Retained `ARGBHalf` resources are:

- state A;
- state B;
- conservative transport predictor;
- conservative transport corrected result;
- per-step progressive source;
- transfer diagnostic.

C.5 physically removes:

- guidance texture and all guidance rebuild work;
- procedural network evaluation;
- forward/reverse advection resources and kernels;
- full-field circular Phase work;
- material-neighbour reinforcement;
- non-conservative local Presence sharpening;
- raw-state user-facing diagnostic branches.

C.5 adds bounded sharp-interface face reconstruction and four conservative face fluxes in each of two transport stages. The cost is fixed by active material-field cells and quality-scaled field resolution; it does not scale with event count or create per-event objects.

Pattern and source-fill noise are evaluated only where sources are rasterized, not for every field cell every material update.

No patch in C.3–C.7 may introduce:

- per-event GameObjects;
- steady-state managed allocations;
- unbounded candidate or path search;
- GPU readback for production scheduling;
- a continuously maintained graph;
- a global target-coverage controller.

---

## 12. Serialization and Compatibility

- Topology cache payloads, fingerprints, versions, and assets do not change.
- Material textures are runtime-only, so no saved material-state migration is required.
- Existing lifecycle and manual proof controls remain serialized.
- Public compatibility aliases may remain where harmless.
- New debug enum values are appended rather than inserted.
- Obsolete Foam-material shader properties are removed rather than retained inertly. Existing serialized materials fall back to the remaining canonical properties; no old guidance/spread/Integrity/Phase property may continue to affect runtime behaviour.
- A new documentation file requires its own Unity `.meta`; existing documentation GUIDs remain unchanged.

---

## 13. Explicit Non-Goals Across C.3–C.7

Do not add:

- automatic anchored selection;
- automatic open-water selection;
- regional fairness or cooldown fields beyond documenting later requirements;
- upstream ingress;
- V/U paired archetypes;
- branching events;
- a replacement generic Integrity scalar;
- persistent crack state;
- holes, strips, fragments, or shredding;
- Pattern-driven normal rendering;
- mature internal Foam texture;
- final reference matching;
- new topology generation;
- topology cache changes;
- unrelated performance scheduling architecture.

---

## 14. Roadmap After Closure

After C.7 is accepted:

- `4.11D — Anchored Birth Events`;
- `4.11E — Open-Water Births and Spatial Fairness`;
- `4.11F — Integrated Birth Population`;
- `4.12A — Age- and Pattern-Driven Breakup Readiness`;
- `4.12B — Persistent Fracture and Separation`, only if exact cracks require explicit state;
- `4.12C — Edge Shredding and Dissolution Motion`;
- `4.13 — Mature Foam Rendering and Reference Matching`;
- `4.14 — Performance and Regression Closure`.

No generic Integrity property is presumed. Initial breakup readiness derives from:

```text
Age = 1 - Remaining Life
```

combined with Material Pattern and later approved stress inputs. Explicit fracture state is added only if the visible requirement proves that exact cracks or separations must persist.

---

# Patch 4.11C.5.2b — Foam Debug Layer Reorganization

**Status:** implemented; Unity validation pending.

C.5.2b is a non-behavioural Inspector/debug-layer patch. It exists because the flat Foam diagnostics list became unusable during live validation: transport timing, topology interaction, lifetime, source, residue, and resource metrics were mixed into one long sequence.

The Foam Inspector now groups validation data into foldouts:

- Foam Validation Overview;
- Foam View Modes;
- Transport / Motion;
- Material Lifetime;
- Topology Interaction;
- Birth / Source Debugging;
- Shape Conservation / Residue;
- Runtime State and Resources;
- Advanced Internal Diagnostics.

Each foldout begins with an explanation. The active recovery workflow should use only the first few sections unless a specific issue points elsewhere. Transport timing values are visible in the Transport / Motion section near the top and must not be buried under whole-river topology coverage again.

This patch intentionally does not change transport, lifetime, topology, source, residue, lateral motion, or rendering behaviour. It only reorganizes the debug layer and clarifies which values are implemented now versus assigned to future proof patches.


## 15. Patch 4.11C.5.4e–5.4h — Lifetime Probe Findings and Lifecycle Commit Repair

### 15.1 Why these patches existed

After the material-state migration and lifecycle consolidation, user validation still showed Foam living far longer than the visible lifetime settings. With `Neutral Lifetime = 1`, `Supported Aging Rate = 1`, and `Negative Aging Rate = 1`, visible Foam survived well beyond one second. Several earlier theories were possible: stale Inspector repaint, birth refresh, synchronized material cells, topology interaction, renderer masking, or a broken lifecycle commit.

### 15.2 Evidence gathered

The minimal truth probes deliberately bypassed production shape/topology ambiguity. They showed that:

- raw material patches could be written into the persistent material state;
- the isolated probe did not depend on topology;
- birth activity was idle during the relevant tests;
- Inspector, runtime, and GPU lifetime values agreed at `1.00s`;
- both configured and debug-only absolute 1-second probe modes failed to reduce Remaining Life.

This made parameter value mismatch, topology support, and birth refresh unlikely. The failing absolute probe pointed at lifecycle dispatch/commit state instead.

### 15.3 Root cause

`_FoamDeltaTime` is shared between material lifecycle and topology maintenance kernels. In the lifecycle update loop, material code configured shared parameters with the correct step duration, but topology refresh could then call `ConfigureTopologyParameters(0f)` before `SimulateFoam`. That overwrote `_FoamDeltaTime` with zero. The simulation pass still dispatched, and CPU telemetry still counted a step, but the shader subtracted zero from Remaining Life.

### 15.4 Implemented repair

`SimulateFullField(deltaTime)` now calls `ConfigureSharedComputeParameters(deltaTime)` immediately before `DispatchSimulation(...)`. This is a narrow ordering repair, not a new architecture. It keeps topology generation untouched and ensures the material lifecycle pass binds its own current delta and authoritative read/write textures at the point of dispatch.

### 15.5 Validation gate

Before resuming shape, breakup, drift, or obstacle-flow work, validate:

```text
Neutral Lifetime = 1
Supported Aging Rate = 1
Negative Aging Rate = 1
Debug View = Material Remaining Life
Clear + Emit Absolute 1s Probe
```

Expected: `0.33` dies first, `0.66` second, `1.00` last, and the raw Remaining Life debug view is empty after approximately 1.1 seconds. If the absolute probe passes, also confirm the configured probe behaves the same under the same lifetime/rate settings.
