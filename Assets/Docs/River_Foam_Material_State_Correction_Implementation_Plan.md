# River Foam Material-State Correction Implementation Plan

## Document Status

**Status:** Canonical implementation plan. Patch 4.11C.3 is implemented in code and awaits focused Unity validation; later correction patches remain unimplemented.

**Supersedes:** the former monolithic proposal named `Patch 4.11C.3 — Minimal Material State, Lifetime Authority, and Topology Aging Correction`.

**Approved replacement sequence:**

1. `4.11C.3 — Source Quantity and Birth-Merge Correction`;
2. `4.11C.4 — Persistent Material-State Migration`;
3. `4.11C.5 — Transport and Valid-Fluid Correction`;
4. `4.11C.6 — Lifetime Authority and Presentation`;
5. `4.11C.7 — Validation, Regression Audit, and Documentation Closure`.

**Current gate:** validate `4.11C.3` in Unity. After user acceptance, the next implementation patch is `4.11C.4`.

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
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/Includes/*.hlsl`;
- `Game/Rendering/Water/Shaders/Includes/RiverWaterFoam.hlsl`;
- `Game/Rendering/Water/Shaders/SH_CleanStylizedRiver.shader`.

**Related canonical documents:**

- `River_Foam_Stage6_Architecture.md` — owns the permanent Stage 6 behavioural contract;
- `River_Rendering_Roadmap.md` — owns the concise milestone sequence;
- `River_Foam_Topology_Implementation_Plan.md` — owns topology only and records that topology generation remains closed;
- `River_Progressive_Initialization_and_Work_Scheduling_Plan.md` — owns initialization, scheduling, and performance safeguards.

No implementation patch may silently absorb behaviour assigned to a later patch. The only unavoidable atomic boundary is `4.11C.4`, where every producer and consumer of the persistent material texture must migrate together.

---

## 1. Why This Correction Exists

Patch 4.11C.1 proved that the progressive event trajectory and per-update source rasterization are correct. Patch 4.11C.2 proved that the source reaches the persistent material handoff. Unity testing then showed that changing Neutral Lifetime, Supported Aging Rate, and Negative Aging Rate produced little or no meaningful change in visible Foam survival.

The current persistent state is:

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
- is displayed diagnostically;
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

**Implementation status:** Implemented in code; focused Unity validation pending. The old persistent Amount/Life/Integrity/Phase packing is intentionally still active until 4.11C.4.

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

Compatibility may retain an old public `freshness` parameter or `FoamTestFreshness` alias when removing it would break callers, but its canonical meaning remains Initial Remaining Life.

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

Disturbance-driven motion remains. Current uniforms named as Wake/Impact Reinforcement also influence `CS_RiverFoam.Motion.hlsl`; remove only their material-growth use. Preserve or rename the motion influence after verifying all call sites.

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

Append new serialized debug values; do not renumber existing values.

Add:

- `Material Presence`;
- corrected `Material Remaining Life`, masked by Presence;
- `Material Pattern`.

The existing progressive source and transfer diagnostics remain.

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
- no final contour stabilization tuning;
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

Known limitation: transport and boundary operations may still reduce Presence incorrectly until 4.11C.5. This patch proves the state contract, not final survival.

---

## 7. Patch 4.11C.5 — Transport and Valid-Fluid Correction

### 7.1 Purpose

Ensure movement and partial river boundaries reshape Presence without becoming independent material-death clocks.

### 7.2 Forward and Reverse Advection

Both advection directions must:

1. backtrace/forward-trace through the existing motion field;
2. bilinearly sample premultiplied Presence/LifeMoment/PatternMoment;
3. write the sampled packed state;
4. avoid repeated multiplicative boundary attenuation;
5. clear only when the destination is fully invalid or outside the simulation domain.

Forward and reverse paths must use identical packed-state semantics so MacCormack error estimation does not contain an extra material-loss operation.

### 7.3 MacCormack Correction

Retain the existing correction structure where valid:

```text
corrected = forward + 0.5 × (original - reverse)
```

Then clamp:

```text
Presence       to [0, 1]
Life moment    to [0, Presence]
Pattern moment to [0, Presence]
A              to 0
```

### 7.4 Presence Contour Stabilization

Apply the parameter-free smooth remap after corrected advection:

```text
P = P × P × (3 - 2 × P)
```

This is a numerical reconstruction of geometric coverage. It preserves `0`, `0.5`, and `1`, pushes low values toward empty, and high values toward occupied.

It is not age, dissolution, or quantity conservation. Sub-resolution features may still disappear. Source widths must therefore produce a meaningful resolved core at every supported quality tier.

### 7.5 Canonical Valid Fluid

Use:

```text
Valid Fluid = Boundary Coverage × (1 - Canonical Obstacle Footprint)
```

If Valid Fluid is effectively zero, clear the state.

For partial coverage:

```text
Clipped Presence = min(Presence, Valid Fluid)
```

Decode the original normalized Life and Pattern, replace only Presence, and re-encode the same attributes with the clipped Presence.

The `min` operation is idempotent: repeated application at the same bank does not produce exponential decay.

`_FoamObstacleExclusion` is authoritative. `_FoamTopology.a` remains only a compatibility/debug copy and must not be multiplied as a second exclusion.

### 7.6 Sampling Cleanup

`SampleStateBilinear` and equivalent helpers return ordinary interpolation of the premultiplied channels. Remove circular Phase numerator logic permanently.

### 7.7 Expected Files

- `StylizedRiverFoamRuntime.Compute.cs`;
- `StylizedRiverFoamRuntime.Binding.cs` if obstacle binding is required for boundary kernels;
- `CS_RiverFoam.Resources.hlsl`;
- `CS_RiverFoam.Sampling.hlsl`;
- `CS_RiverFoam.Simulation.hlsl`;
- `CS_RiverFoam.compute`;
- relevant diagnostics/documentation.

### 7.8 Explicit Exclusions

- no change to the approved topology aging formula;
- no final-life dissolve presentation;
- no CPU reservation rewrite;
- no automatic births;
- no breakup.

### 7.9 Acceptance Gate

Using a long or temporarily neutralized lifetime:

- resolved material moves without becoming an Amount-like fading haze;
- Remaining Life does not change merely because the shape crosses texels;
- near-bank material clips once rather than shrinking every update;
- obstacle interiors remain empty;
- reverse flow remains correct;
- Low, Medium, and High quality preserve time-based life even though edge precision differs;
- no invalid packed moments appear after correction;
- limitations of sub-resolution geometry are documented rather than misclassified as lifecycle behaviour.

---

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
Presence contour
× Lifecycle Visibility
× unfrozen factor
× Foam Colour alpha
```

Hidden proof properties may remain serialized for compatibility but must not control survival or material identity.

### 8.4 CPU Activity Reservations

Remove reservation `RemainingAmount` and all Amount-decay reservation logic.

A reservation retains only:

- centre global distance;
- along-flow extent;
- elapsed time;
- conservative maximum active duration.

For initial normalized Remaining Life `L0`:

```text
Maximum duration = L0 × Neutral Lifetime / Supported Aging Rate
```

This uses the slowest selected approved aging rate. The existing global safety cap may remain. No separate dissolve seconds are added because the fade occurs inside Remaining Life.

### 8.5 Metrics

Topology/material coverage metrics must count Presence rather than Amount. Rename the internal visible-material threshold to a Presence metric threshold and use a fixed core-coverage contour suitable for diagnostics.

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
- `StylizedRiverFoamRuntime.Injection.cs`;
- `StylizedRiverFoamRuntime.State.cs`;
- `StylizedRiverFoamRuntime.Constants.cs`;
- `StylizedRiverFoamRuntime.Compute.cs`;
- `StylizedRiverFoamRuntime.Binding.cs`;
- `StylizedRiverFoamRuntime.Topology.cs`;
- `StylizedRiverFoamRuntime.PublicSurface.cs`;
- `CS_RiverFoam.Resources.hlsl`;
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

This diagnostic is not a permanent per-frame production cost. It exists to detect forgotten old-format writes, MacCormack overshoot, stale Phase data, or invalid moments.

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
4. update conservative activity reservations;
5. build guidance when scheduled;
6. forward advection;
7. reverse advection;
8. simulate corrected material:
   - decode corrected state;
   - stabilize Presence contour;
   - apply idempotent valid-fluid clipping;
   - calculate topology age rate;
   - reduce Remaining Life;
   - clear expired material;
   - merge per-step source without rejuvenating occupied material;
   - encode Presence/Life/Pattern;
9. swap temporal state textures;
10. bind previous/current state and diagnostics to the water material.

Source merge occurs after aging so material born during the current dispatch begins with its complete selected Initial Remaining Life and starts aging on the next material update.

---

## 11. Resource and Performance Contract

No extra persistent material texture is required.

Retained resources remain `ARGBHalf`:

- state A;
- state B;
- forward advection;
- reverse advection;
- per-step progressive source;
- transfer diagnostic.

The correction should reduce hot full-field cost by removing:

- eight-direction Amount/Phase neighbourhood sampling;
- broad Amount neighbourhood work;
- circular Phase trigonometry and mixing;
- disturbance material reinforcement;
- Integrity calculations;
- independent Amount exponential decay.

It adds:

- semantic decode/encode divisions;
- one inexpensive cubic Presence reconstruction;
- source-local Pattern and birth-fill generation;
- optional development diagnostics.

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
- Old hidden shader properties may remain declared for serialized material compatibility, but they become inert where they conflict with the corrected contract.
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
