# River Editor Loading Performance: Full Diagnosis

**Investigation date:** 2026-07-14  
**Project:** Norse Stylized 3D PoC  
**Unity:** 6000.5.0f1, URP  
**Scope:** Editor stalls after river code or shader changes, entering Play mode, and selecting the river in the Inspector  
**Change status:** Diagnosis and remediation proposal only. No implementation is authorized by this document.

## 1. Executive conclusion

The reported delay is not one operation and is not adequately explained by ground or river geometry regeneration alone. It is a sequence of at least three independent costs:

1. **A broad C# compilation and ordinary managed-domain restoration cost** occurs after script edits. The project has 147 C# files and no assembly definitions under `Assets`, so Unity recompiles the monolithic runtime and Editor assemblies. The latest captured river edit spent **28.266 seconds** in `CompileScripts`, followed by a **9.613-second** domain reload whose largest ordinary component was **6.830 seconds** in `AwakeInstancesAfterBackupRestoration`.
2. **An extreme river shader-variant compilation cost** occurs when the Scene view or Play mode first needs a variant after the river shader/include changes. The strongest captured Play transition spent **242.547 seconds** in domain reload, of which **234.344 seconds** was attributed by Unity to `CreateAndSetChildDomain`. During that interval, a newly launched `UnityShaderCompiler` process accumulated approximately **256 CPU-seconds**. Shader-cache output appeared at the end of the stall. This is strong evidence that the main thread was waiting on shader compilation or a graphics/shader synchronization lock even though the managed profiler bucket was named `CreateAndSetChildDomain`.
3. **A deferred Inspector/preview shader-variant compilation cost** occurs only when the river is selected with an Inspector/preview capable of rendering its generated mesh and temporary material. New shader-cache artifacts were written during the selection freeze, without another assembly reload or Asset Pipeline refresh. This explains why a different selected object is fast and why selecting the river can freeze for minutes after Play has already started and the visible shader progress bar has completed.

The highest-impact code-level regression is in `RiverWaterFoam.hlsl`. The current fragment path unconditionally calls the selection-diagnostics candidate search. That search has two explicitly unrolled loops covering **5 × 11 = 55 candidates per fragment**, compared with **3 × 3 = 9 candidates** in the committed baseline. The current search therefore exposes the shader compiler to approximately **6.1 times as many copies of an already enlarged candidate body**. The body now contains additional hashes, lifetime calculations, trigonometric motion, rotation, size pulsing, morphing, derivatives, and anti-aliasing. Runtime material values can skip candidates while executing, but they cannot prevent the compiler from expanding and optimizing an explicitly unrolled, uniform-dependent search.

**Primary diagnosis:** the multi-minute stalls are dominated by pathological compilation of the current river fragment shader in several on-demand variants and rendering contexts. **Confidence: 97%.**

**Secondary diagnosis:** monolithic C# assemblies and duplicated Ground/River restoration requests account for much of the repeatable tens-of-seconds overhead around the shader stall, but do not explain the three-to-four-minute Inspector-specific freezes. **Confidence: 95% for monolithic compilation; 80% that duplicated lifecycle work materially contributes to ordinary restoration.**

The project should not attempt to solve this by merely prewarming every variant. That would move the multi-minute cost earlier and could multiply it. The durable fix is to reduce the fragment shader's compiler complexity, isolate diagnostic-only shader work, constrain the variant surface, and then address C# assembly boundaries and Ground/River orchestration as separate workstreams.

## 2. Confidence scale and evidence labels

This report uses the following terms:

| Label | Meaning |
|---|---|
| **Proven fact** | Directly visible in current source, serialized project state, `Editor.log`, or measured process/cache output. |
| **Strong inference** | Multiple independent facts fit one causal explanation, with no observed contradictory evidence. |
| **Hypothesis** | Plausible but not yet isolated by a controlled measurement. |

Confidence percentages express confidence in the stated causal claim, not certainty that a proposed fix will preserve every visual detail.

| Confidence | Interpretation |
|---|---|
| 95–100% | Directly demonstrated or overwhelmingly supported. |
| 80–94% | Strong evidence; one important link is inferred. |
| 60–79% | Likely contributor, but not isolated. |
| 30–59% | Possible amplifier or secondary condition. |
| Below 30% | Weak candidate or substantially contradicted by evidence. |

## 3. Exact behavior being explained

The observations form a consistent, reproducible sequence:

| Stage | User-visible behavior | Evidence-backed interpretation |
|---|---|---|
| River script/include changed | Unity compiles for a noticeable period. | C# changes rebuild monolithic assemblies; shader/include changes invalidate dependent river variants. |
| Scene view returns | River may be cyan and show `Compiling shader`. | Async shader compilation is enabled. Cyan is Unity's temporary replacement while the requested variant is not ready. It does not prove missing geometry or a failed river cache. |
| Play is pressed | Domain reload can take several minutes and shader compilation appears to restart. | Play requests a different keyword/context variant. Unity waits during domain creation while shader compiler work is active. The progress is a new compile batch, not necessarily the original variant restarting. |
| Play eventually begins | River renders and normal runtime proceeds. | The Play-context variant completed. River foam startup telemetry shows little actual CPU work in its measured phases. |
| Inspector is hidden or another object is selected | Interaction is comparatively fast. | Nothing has requested the river Inspector/preview rendering context yet. |
| River is selected with Inspector open | Inspector/UI freezes for minutes, with no new domain reload. | The selection causes Unity's object/material/mesh preview path to request another uncached river shader variant. Shader-cache artifacts are produced during this freeze. |
| Same source remains unchanged afterward | Later interactions may become faster. | The specific variants now exist in the Library shader cache. A later shader source edit invalidates dependent compiled output and repeats the sequence. |

This behavior is characteristic of **deferred compilation across different consumers**, not a single procedural generator repeatedly rebuilding the same CPU data.

## 4. Captured timing evidence

### 4.1 Latest river-edit Asset Pipeline refresh

The latest captured forced synchronous recompile in `Logs/Editor.log` produced:

| Operation | Time | Source |
|---|---:|---|
| `CompileScripts` | **28,265.530 ms** | `Logs/Editor.log:3505` |
| `ShaderAssetModifiedCallback` | **369.094 ms** | `Logs/Editor.log:3516` |
| Domain reload | **9,613 ms** | `Logs/Editor.log:3452` |
| `CreateAndSetChildDomain` | **397 ms** | `Logs/Editor.log:3455` |
| `AwakeInstancesAfterBackupRestoration` | **6,830 ms** | `Logs/Editor.log:3471` |
| Total Asset Pipeline refresh | **39.486 s** | Summary surrounding `Logs/Editor.log:3488–3517` |

The shader assets themselves imported quickly:

- `RiverWaterFoam.hlsl`: approximately **0.011 s**.
- `SH_CleanStylizedRiver.shader`: approximately **0.385 s**.

That distinction is important. **Shader source import is not shader variant compilation.** Unity can parse/import the shader asset quickly and defer expensive GPU-program variants until a rendering context asks for them.

### 4.2 Play-mode transition after the edit

The same session then recorded:

| Operation | Time | Source |
|---|---:|---|
| `Reloading assemblies for play mode` | Event begins | `Logs/Editor.log:3652` |
| Domain reload total | **242,547 ms** | `Logs/Editor.log:3675` |
| `CreateAndSetChildDomain` | **234,344 ms** | `Logs/Editor.log:3678` |
| `AwakeInstancesAfterBackupRestoration` | **6,491 ms** | `Logs/Editor.log:3692` |
| `CompileScripts` | **1.695 ms** | `Logs/Editor.log:3709` |
| Backup-scene integration | **21,875.535 ms** | `Logs/Editor.log:3759` |

`CompileScripts` was effectively zero on this Play transition. The 234-second interval therefore was not a second C# compile.

During the exact long `CreateAndSetChildDomain` interval:

- Unity launched a new `UnityShaderCompiler` process.
- That process accumulated approximately **256 CPU-seconds** by the time the domain transition completed.
- Shader-cache outputs for `SH_CleanStylizedRiver` appeared at the end of the interval.

Unity's timing label identifies where the main thread waited, not necessarily what the native shader worker was doing. The combined process and cache evidence makes the shader compiler/global shader synchronization path the most likely owner of that wait.

### 4.3 Deferred shader-cache sequence

The river shader cache directory contained this sequence after one source update:

| Time | Cache output size | Recorded keywords | Likely trigger |
|---|---:|---|---|
| 12:22:49.704 | 32,236 B | none | Initial small-stage output |
| 12:27:15.924 | 799,756 B | none | Initial Scene/default heavy variant completes |
| 12:27:16.389 | 32,308 B | `_ADDITIONAL_LIGHT_SHADOWS`, `_FORWARD_PLUS`, `_MAIN_LIGHT_SHADOWS_CASCADE` | Play variant small-stage output |
| 12:31:12.756 | 850,332 B | same plus `_SHADOWS_SOFT` | Play heavy variant completes |
| 12:33:06.688 | 32,300 B | `_ADDITIONAL_LIGHT_SHADOWS`, `_FORWARD_PLUS`, `_MAIN_LIGHT_SHADOWS` | Inspector selection small-stage output |
| 12:37:12.431 | 831,776 B | same keywords | Inspector selection heavy variant completes |

`Library/ShaderCache/EditorEncounteredVariants` was then updated at 12:37:15.

The cache timing maps closely to the user-observed phases:

- roughly **4 minutes 26 seconds** for the first heavy no-keyword context;
- roughly **3 minutes 56 seconds** for the Play-context heavy variant;
- roughly **4 minutes 6 seconds** for the Inspector-selection heavy variant.

No new domain reload or Asset Pipeline refresh was logged during the Inspector selection interval. This rules out C# compilation, domain restoration, and ordinary asset import as the direct cause of that specific freeze.

The cache contained approximately **1,657 files / 29.12 MB**, with older files still present. That contradicts the idea that the entire global shader cache is being wiped on every operation. What is invalidated is the compiled output dependent on the edited river shader/include, which is normal Unity behavior after a source change.

**Evidence caveat:** `Library` contents and process IDs are ephemeral diagnostic artifacts, not source-controlled project truth. Their timing is useful for this captured incident, but should not be treated as a stable architecture contract.

## 5. Ranked findings

### Finding 1 — Explicitly unrolled 55-candidate fragment search is the principal compile-time regression

**Impact:** Critical  
**Confidence:** 98% that it is the dominant per-variant compile-cost regression  
**Classification:** Proven source expansion plus strong causal inference

### Evidence

`Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl:512–516` contains:

```hlsl
[unroll]
for (int offsetX = -2; offsetX <= 2; offsetX++)
{
    [unroll]
    for (int offsetY = -5; offsetY <= 5; offsetY++)
```

This is **5 × 11 = 55 candidate iterations**. In the committed baseline, the corresponding ranges were `-1..1` on both axes: **3 × 3 = 9 candidates**. The new candidate count is therefore **55 / 9 = 6.11 times** the prior count.

The current include differs from the committed version by **+779 / -576 lines**, while the owning shader differs by **+114 / -26 lines**. File size alone is not proof of compile cost, but the changed loop and enlarged body are directly relevant to compiler work.

The candidate evaluation now includes, among other work:

- multiple hash/random values;
- lifetime and phase calculations;
- sine/cosine wave motion;
- downstream and lateral displacement;
- rotation;
- size pulsing;
- multi-axis shape/morph bases;
- derivative operations such as `fwidth`;
- anti-aliasing and selection diagnostics.

Because `[unroll]` is explicit, the compiler is asked to materialize and optimize many copies of this expression graph. The generated heavy cache entries are approximately **0.8 MB per observed keyword context**, which is consistent with a very large compiled program/intermediate representation.

### Why the dynamic `continue` does not solve compilation

The code calculates:

- `requiredDownstreamOffset` at `RiverWaterFoam.hlsl:500–504`;
- `requiredLateralOffset` at `RiverWaterFoam.hlsl:505–509`;
- a uniform-dependent skip at `RiverWaterFoam.hlsl:518–522`.

Those values depend on material uniforms. A runtime value such as zero may cause the GPU to skip work while rendering, but it is not a compile-time constant. The compiler must retain code for every possible legal material value and optimize the unrolled control flow. Consequently:

- lowering the property in the material may improve runtime shader execution;
- it does **not** reliably reduce cold shader compile time;
- the large compile cost can be caused indirectly by a property whose maximum range forced larger static loop bounds.

### Most suspicious indirect property

`_FoamChipLateralMotionAmount` is declared with a range of `0..2.5` in `SH_CleanStylizedRiver.shader:126`. It feeds lateral motion in the candidate evaluation and the maximum-reach calculation. The required lateral offset is allowed to reach five cells, matching the `-5..5` loop.

This makes it a strong example of “one property causing the problem indirectly”: supporting its maximum authoring range expanded the compiler-visible search domain for **every** material value, including zero.

**Confidence that this property/range materially forced the 11-wide lateral search:** 92%.

### Suggested fixes

#### Fix 1A — Immediate containment: reduce compile-visible search bounds

Restore a much smaller static candidate set, or redesign maximum motion/shape reach so it does not require `5 × 11` fragment candidates.

- **Expected impact:** Very high.
- **Confidence of compile-time improvement:** 95%.
- **Risk:** High visual risk if performed mechanically. Chips at extreme lateral motion, large shape size, or edge cases may disappear, pop, or clip because their source cell is no longer searched.
- **Requirement:** Derive the smallest bounds from the actual maximum footprint and motion contract, then validate every extreme authoring combination.

This is appropriate as an emergency recovery patch, but it is not the best final architecture if the full motion envelope must be retained.

#### Fix 1B — Long-term solution: move candidate-field construction out of the surface fragment shader

Build the sparse/dynamic foam-chip field once per relevant update in a compute shader, structured buffer, or texture, then sample the result from the water surface shader.

The surface fragment shader should answer a small, bounded question such as “what is the chip mask/selection value here?” rather than procedurally reconstructing and selecting 55 possible chips for every pixel and every compiled lighting variant.

- **Expected impact:** Highest sustainable reduction in compile complexity and likely runtime cost.
- **Confidence:** 90%.
- **Risk:** Medium-to-high implementation risk. It changes data ownership, update timing, texture/buffer lifetime, and synchronization. Visual equivalence must be tested.
- **Preservation strategy:** Keep the same deterministic cell/hash rules in the field producer, publish a versioned field snapshot, and compare GPU captures or reference screenshots before removing the fragment implementation.

#### Fix 1C — Remove explicit unrolling or restructure into compiler-bounded phases

Test whether the target platforms accept a dynamic/rolled loop without catastrophic runtime cost, or split broad selection into a cheap coarse lookup followed by a very small exact candidate set.

- **Expected impact:** Potentially high.
- **Confidence:** 75% because HLSL compiler behavior and target GPU loop handling must be measured.
- **Risk:** A dynamic loop may trade Editor compile time for runtime GPU cost, divergent branching, or unsupported behavior on lower targets.

### Finding 2 — The shader has a broad variant surface, so the pathological body is compiled repeatedly

**Impact:** Critical multiplier  
**Confidence:** 96% that variants multiply the primary cost; 85% that the current surface contains materially unused variants  
**Classification:** Proven declarations plus observed variant cache outputs

### Evidence

`SH_CleanStylizedRiver.shader:185–193` declares:

```hlsl
#pragma target 3.5
#pragma vertex Vert
#pragma fragment Frag
#pragma multi_compile_fog
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
#pragma multi_compile _ _FORWARD_PLUS
#pragma multi_compile_fragment _ _SHADOWS_SOFT
```

The nominal keyword cross-product represented here is:

`4 fog states × 4 main-light shadow states × 2 additional-light states × 2 additional-shadow states × 2 Forward+ states × 2 soft-shadow states = 256 combinations`

Unity/URP strips invalid and unused combinations, and not all 256 are necessarily compiled in this Editor session. The observed cache nevertheless proves that at least these distinct contexts were requested:

- no recorded keywords;
- Forward+ plus cascaded main shadows and additional-light shadows;
- the above plus soft shadows;
- Forward+ plus non-cascade main shadows and additional-light shadows.

Each requested context recompiles the same extremely expensive fragment body.

### Why shader progress appears to restart

Async shader compilation is enabled at `ProjectSettings/EditorSettings.asset:25`:

```yaml
m_AsyncShaderCompilation: 1
```

Unity can therefore return control using a cyan placeholder for one context while other contexts remain uncompiled. Entering Play changes cameras, render targets, shadow modes, and/or active URP keywords. Inspector preview rendering can request another context again. The progress bar moving back or reappearing is consistent with a **new variant batch** being requested, not proof that the same completed binary was discarded.

### Suggested fixes

#### Fix 2A — Audit and strip impossible project variants

Create a deliberate matrix of which river shader keyword states the project can actually use in Scene, Game, preview, and builds. Strip combinations that are impossible under the project's URP configuration.

Possible mechanisms include shader-source `skip_variants`, a controlled shader preprocessing rule, and removal of `multi_compile` dimensions that are not required by this shader.

- **Expected impact:** High as a multiplier reduction.
- **Confidence:** 90% if the active rendering matrix is accurately documented.
- **Risk:** High if guessed. Incorrect stripping can produce a pink/missing shader or silently select a fallback in a camera, preview, quality level, or player build.
- **Required audit:** The shared-shader/cross-subsystem impact audit required by repository invariants must precede implementation.

Variant stripping alone is not sufficient while a single needed variant still takes approximately four minutes.

#### Fix 2B — Isolate diagnostic-only code from the production fragment path

`RiverWaterFoamEvaluateSelectionDiagnostics` is called unconditionally from `SH_CleanStylizedRiver.shader:933`. Its authoring controls are runtime uniforms, so the compiler retains the diagnostic body even when diagnostics are visually disabled.

Move diagnostic-only rendering to one of:

- a separate diagnostic shader/pass;
- a separate material used only while the diagnostic view is active;
- a compile-time local feature with strict controls and a verified small variant impact;
- an Editor visualization that consumes prepared diagnostic data without expanding the production surface fragment.

- **Expected impact:** High if the candidate search is diagnostic-only or can be excluded from normal rendering.
- **Confidence:** 92% that unconditional inclusion is materially harmful.
- **Risk:** Medium. A new keyword can itself create more variants if introduced carelessly. A separate shader/pass can drift from production logic unless both consume the same field data.

The preferred long-term combination is shared field-generation logic plus thin production and diagnostic consumers, not two independent copies of the procedural algorithm.

#### Fix 2C — Add cold-compile budgets to shader feature acceptance

Every shader feature that expands a loop, branch graph, or variant dimension should record:

- cold compile time for a representative required variant;
- output/intermediate size where available;
- number of actually encountered Editor variants;
- Scene, Play, and Inspector first-use timings.

Recommended acceptance targets for this project are:

- first required cold variant under **10 seconds**;
- subsequent common variants under **5 seconds**;
- river selection with an already open Inspector under **2 seconds**;
- no repeated multi-minute work without a shader source/configuration change.

These are proposed performance budgets, not existing canonical requirements.

### Finding 3 — River selection triggers a deferred built-in preview variant, not expensive custom Inspector code

**Impact:** Critical user-facing stall  
**Confidence:** 98% for the immediate selection-freeze cause  
**Classification:** Proven cache timing and source inspection

### Evidence

The user isolated the behavior:

- another object can remain selected through Play;
- opening the Inspector for that object does not freeze;
- selecting the river after the Inspector is open causes a large freeze;
- this happens after Play begins and after the earlier shader work appears complete.

During the selection freeze, new `SH_CleanStylizedRiver` cache artifacts were written. No new domain reload or Asset Pipeline refresh appeared in `Editor.log`.

The river's custom inspector is not a strong direct candidate:

- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs:111` only requests constant repaint in Play mode when one target is selected and the Runtime Diagnostics section and relevant subpanels are open.
- `OnInspectorGUI` begins at line 193 and gates detailed sections behind foldouts.
- The editor does not implement `HasPreviewGUI`, `OnPreviewGUI`, or `RenderStaticPreview`.
- Its in-memory open-section set begins empty after a reload.

By contrast, the river supplies a generated mesh and material to normal Unity components:

- `StylizedRiver.cs:5272–5285` creates/assigns a temporary generated surface mesh with `HideFlags.DontSave`.
- `StylizedRiver.cs:5290` assigns `meshRenderer.sharedMaterial = ResolveBodyMaterial()`.
- `StylizedRiver.cs:5532–5535` creates a temporary `Material` with `HideFlags.DontSave` if no body material asset is assigned.
- In `VisualFrameworkDemo.unity`, the river's serialized MeshFilter mesh is null at line 10422 and `bodyMaterial` is null at line 10797, so both are runtime/editor-generated rather than persistent scene asset references.
- The saved Inspector layout contains an expanded preview preference (`Preview_InspectorPreview`).

The likely sequence is:

1. Selection causes Unity to inspect the GameObject's MeshRenderer/MeshFilter and previewable material.
2. The built-in preview asks the shader for a preview-camera keyword combination.
3. That exact combination has not been compiled since the source edit.
4. The preview repaint waits on the pathological fragment compilation.
5. Windows reports `InspectorWindow.Repaint`, because repaint is the caller blocked on the shader, not because repaint bookkeeping itself costs four minutes.

### Suggested fixes

#### Fix 3A — Fix the shader body first

This is the only solution that addresses Scene, Play, and Inspector contexts together. Inspector workarounds cannot make a four-minute required Play variant acceptable.

- **Expected impact:** Critical.
- **Confidence:** 98%.

#### Fix 3B — Prevent or cheapen river preview rendering

As an Editor-only containment measure, provide a lightweight custom preview or explicitly suppress an expensive preview for the river's generated renderer/material when appropriate.

- **Expected impact:** High for the selection freeze only.
- **Confidence:** 85%.
- **Risk:** Low-to-medium. It may remove a useful material/mesh preview or require a carefully scoped custom editor. It does not help Scene/Game compilation.

Collapsing the Inspector preview is a valid temporary workflow workaround and diagnostic control, but not the final fix.

#### Fix 3C — Evaluate a persistent material asset

Assigning a persistent body material asset may improve preview identity, serialization stability, and cache reuse compared with recreating a `DontSave` material after reload.

- **Expected impact:** Low-to-medium.
- **Confidence:** 55% for reducing repeated preview requests.
- **Important limitation:** A persistent material cannot reuse a compiled shader binary after the shader source it depends on changes. It will not solve the cold compile regression.

### Finding 4 — Monolithic C# assemblies cause broad, avoidable script compilation after river code edits

**Impact:** High but not multi-minute shader-scale  
**Confidence:** 95% that it causes broad invalidation; 90% that it materially contributes to the 28-second compile  
**Classification:** Proven project layout and compile timing

### Evidence

- There are **147 C# files** under `Assets`.
- There are **zero `.asmdef` files** under `Assets`.
- Unity therefore compiles project runtime scripts into `Assembly-CSharp.dll` and Editor scripts into `Assembly-CSharp-Editor.dll`.
- The captured compiler invocation referenced hundreds of assemblies: approximately 341 references for the runtime assembly and 356 for the Editor assembly.
- Captured `CompileScripts` durations varied substantially:

| Log line | Time |
|---|---:|
| `Editor.log:330` | 7.639 s |
| `Editor.log:662` | 13.807 s |
| `Editor.log:1915` | 2.955 s |
| `Editor.log:2110` | 11.487 s |
| `Editor.log:2359` | 4.108 s |
| `Editor.log:3194` | 6.265 s |
| `Editor.log:3505` | **28.266 s** |

Any change to a river runtime file invalidates the entire runtime assembly and then its dependent Editor assembly. This is structurally broader than necessary.

### Suggested fix

Introduce assembly definitions in a staged architecture, for example:

- a small shared procedural contracts/runtime assembly;
- Generated Ground runtime;
- Stylized River runtime;
- Generated Mass runtime;
- corresponding Editor-only assemblies referencing their runtime assembly;
- any genuinely shared rendering/runtime support in a narrowly defined shared assembly.

This is a proposal only. Repository invariants prohibit introducing architectural dependencies without approval.

The main design hazard is the current two-way Ground/River knowledge. Creating `Ground -> River` and `River -> Ground` assembly references would form a forbidden cycle. Before assembly splitting, shared interfaces/snapshots and the transaction coordinator described in `Ground_River_Regeneration_Orchestration_Manual.md` should establish a one-directional dependency structure.

- **Expected impact:** High for script-edit iteration.
- **Confidence:** 90%.
- **Risk:** Medium. Assembly boundaries expose hidden dependencies, Editor/runtime namespace mistakes, reflection assumptions, and cyclic subsystem knowledge.
- **Non-goal:** This will not materially reduce cold river shader variant compilation.

### Finding 5 — Ground and River both initiate restoration-time generation, producing duplicate request waves

**Impact:** High ordinary reload/Play overhead; negligible against the four-minute shader compile  
**Confidence:** 100% that duplicate request paths exist; 80% that they materially contribute to ordinary restoration; below 5% that they cause the multi-minute selection freeze  
**Classification:** Proven call graph; contribution inferred

### Evidence

`Assets/Game/Procedural/Ground/GeneratedGround.cs`:

- `OnEnable` begins at line 627 and calls `Regenerate()` after cache/normalization/modifier refresh.
- `Regenerate()` begins at line 659 and owns geometry, mesh application, collider work, painted accents, material work, and corridor publication.
- `NotifyRiverChanged` begins at line 875, refreshes modifiers, and requests another `Regenerate()`.
- after ground corridor data changes, the ground calls `river.RebuildCorridorFromGround()` at line 1837.

`Assets/Game/Procedural/Rivers/StylizedRiver.cs`:

- `OnEnable` begins at line 3339 and calls `RegenerateAll()` at line 3358.
- `RegenerateAll()` begins at line 3739, builds river domain/surface work, and calls `NotifyParentGround()` at line 3751.
- `NotifyParentGround()` begins at line 6037 and calls `ground.NotifyRiverChanged(this)` at line 6046.

One restoration wave can therefore contain:

```text
Ground.OnEnable
  -> Ground.Regenerate

River.OnEnable
  -> River.RegenerateAll
  -> Ground.NotifyRiverChanged
  -> Ground.Regenerate/request
  -> River.RebuildCorridorFromGround
```

Signature checks and dirty-state guards may skip some stages, so this is not proof that every path performs two full geometry rebuilds. It is proof that two components independently claim authority to initiate the same dependency wave.

### Timing correlation

Ordinary reloads repeatedly show:

- `AwakeInstancesAfterBackupRestoration`: approximately **5.3–9.9 seconds**;
- backup-scene integration: approximately **17–22 seconds**.

Examples of integration time include 17.076 s, 17.953 s, 17.461 s, 20.247 s, 20.832 s, 20.248 s, 21.314 s, and 21.876 s.

There is no per-generator timing trace in the captured log that attributes these entire buckets to Ground/River. The call graph makes them credible contributors, but scene deserialization, other components, and Unity integration work are also inside those buckets.

### Suggested fix

Implement the accepted Ground-owned generation transaction described in `Ground_River_Regeneration_Orchestration_Manual.md`:

1. Change callbacks only mark versioned inputs dirty and submit a request.
2. One coordinator coalesces all requests in the current restoration/edit wave.
3. Ground establishes canonical geometry once.
4. Ground publishes immutable/versioned geometry and surface snapshots.
5. River builds its surface/domain after ground geometry exists.
6. River publishes bank/corridor influence data without directly invoking a fresh upstream ground transaction.
7. Ground-dependent features consume the appropriate snapshot in a defined phase.
8. A bounded correction/finalization phase applies river-bank-dependent ground features without restarting the entire pipeline.

Future features should declare inputs and outputs against these snapshots rather than call `Regenerate()` on their provider. That preserves the required ordering—river after ground geometry, bank-aware ground features after river data—without an unbounded regeneration loop.

- **Expected impact:** High for predictable dirty-time and reload cost.
- **Confidence:** 85% for eliminating duplicated work once correctly implemented.
- **Risk:** Medium-to-high. Ordering, version ownership, Undo, prefab/scene restoration, and edit-mode callbacks can regress if partially migrated.
- **Authoritative design manual:** `Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md`.

This work should proceed independently of the shader emergency. It will not make a pathological fragment variant compile faster.

### Finding 6 — Foam startup telemetry is measuring wall-clock starvation, not doing 75 seconds of foam-cache work

**Impact:** Diagnostic confusion; low direct cost  
**Confidence:** 97% that the foam cache/build is not the multi-minute root cause  
**Classification:** Proven phase telemetry

### Evidence

`Editor.log:3848` reports:

- total startup elapsed: **75,705.652 ms**;
- outcome: `StaleCompatible`;
- reasons: `ObstacleMismatch, CombinedMismatch`;
- cache: attempt 1, hit 0, miss 1, install 1, build 0;
- cache load: **61.247 ms**;
- all topology build counters: **0**;
- slowest measured phase: `InstallCachedTopology`, **35.515 ms**;
- `WaitForObstacleStability`: 15 calls totaling only **0.176 ms** of measured method work.

Earlier runs report the same pattern with total wall times of 1.2–4.3 seconds but measured cache/phase work in tens of milliseconds.

The 75-second total spans multiple frames while Unity's main thread is occupied or starved by other Editor/render work. The state machine's own CPU phase totals do not approach 75 seconds. It is therefore misleading to interpret the total as “foam generation took 75 seconds.”

### Suggested fix

Treat `StaleCompatible` mismatch as a correctness and cache-predictability issue after the primary stalls are controlled:

- determine why obstacle and combined signatures differ immediately after restoration;
- log or compare the specific versioned inputs responsible for the mismatch;
- ensure the orchestration transaction publishes stable obstacle/corridor versions before foam resolves its cache;
- retain separate active-CPU time and wall-clock-between-ticks telemetry.

- **Expected performance impact:** Low for the current multi-minute incidents.
- **Confidence:** 90% that resolving the mismatch will improve predictability but not eliminate the shader freezes.
- **Risk:** Medium if cache validation is weakened. Never convert a mismatch into a blind hit merely to improve timing.

## 6. Secondary contributors and ruled-down suspects

### 6.1 Search indexing

**Current status:** not a credible root cause for the latest incident.  
**Confidence:** 95% that it did not cause the captured Play/Inspector stalls.

Evidence:

- `UserSettings/Search.settings` currently has `indexOnEditorStartup=false`.
- The latest log records `IndexOnStartup:false`.
- `trackSelection=true` remains enabled.
- The Inspector freeze coincides with a new river shader-cache artifact, not a Search indexing event.

Search had produced a large indexing event in earlier investigation and can absolutely amplify Editor stalls. Keeping startup indexing disabled is sensible while diagnosing. “Track current selection” may add modest selection work, but it cannot explain an 831 KB river shader binary appearing after a four-minute shader worker interval.

### 6.2 Profiler recording re-enabling itself

**Current status:** possible amplifier, not the root cause.  
**Confidence:** above 95% that it does not explain the shader-cache sequence; 40–60% that it adds noticeable overhead to ordinary Editor callbacks.

Evidence:

- The user observed the Profiler recording after believing it had been stopped.
- The current saved layout does not contain a `ProfilerWindow` or saved `m_Recording` flag, so this layout is not proven to restore recording.
- Shader compiler CPU and cache output occur independently of Profiler recording.

Recommended follow-up is to set the Profiler's default recording state to Disabled and call-stack capture to None, then verify after one restart. If recording still returns, capture the exact preference state once and treat it as a separate Unity Editor configuration/bug investigation.

### 6.3 URP additional-light shadow atlas warnings

The log repeatedly reports reduced punctual-light shadow resolution, with approximately six shadow maps competing in a 2048 atlas. Repeated warnings and stack capture can add noise and small rendering overhead.

- **Confidence as a multi-minute cause:** below 10%.
- **Confidence as a small Editor amplifier:** 25%.

Resolve the lighting/atlas configuration after the principal issue, both to reduce log noise and to make future traces clearer.

### 6.4 Custom river Inspector constant repaint

The custom editor can request constant repaint only under a specific Play-mode Runtime Diagnostics state. This could amplify cost once an expensive preview exists, but it does not explain the first selection compile and is not active for ordinary collapsed sections after reload.

- **Confidence as direct root:** below 10%.
- **Confidence as an amplifier when diagnostics are open:** 45%.

### 6.5 Full shader-cache corruption or unconditional purge

The presence of 1,657 cache files spanning older timestamps contradicts a full purge on every interaction. Editing an included shader file correctly invalidates dependent river variants.

- **Confidence that the global cache is being purged each time:** below 5%.

A clean Library test would force all work cold and is not recommended until the shader has been simplified; it would likely create a worse one-time stall without answering the current causal question.

## 7. Unified causal model

```text
Edit river C# and/or shader include
    |
    +-- C# changed?
    |     -> Rebuild Assembly-CSharp + Assembly-CSharp-Editor
    |     -> ordinary domain restoration
    |     -> Ground/River OnEnable request wave
    |
    +-- Shader/include changed?
          -> invalidate dependent SH_CleanStylizedRiver variants
          -> import shader source quickly
          |
          +-- Scene view requests context A
          |     -> compile huge 55-candidate fragment body
          |     -> cyan async placeholder while pending
          |
          +-- Enter Play requests context B
          |     -> compile same huge body under Play/URP keywords
          |     -> main thread waits during domain creation
          |
          +-- Select River with Inspector preview requests context C
                -> compile same huge body under preview keywords
                -> InspectorWindow.Repaint blocks until ready
```

The key architectural lesson is that **the labels on the frozen Unity window name the waiting caller, not always the expensive producer**:

- `SceneHierarchyWindow.Repaint` means a hierarchy repaint was waiting on Unity's main thread.
- `InspectorWindow.Repaint` means an Inspector repaint triggered or waited on rendering/preview work.
- `CreateAndSetChildDomain` can contain native waits that are not managed-domain construction CPU.

This is why the screenshots initially appeared to implicate generic UI repaint or domain reload while the shader process/cache evidence points to the river shader compiler.

## 8. Remediation roadmap

### Phase 0 — Preserve a repeatable benchmark

Before changing code, retain a small timing matrix for one deliberate river shader edit:

| Checkpoint | Metric |
|---|---|
| Source save to scripts reloaded | C# compile + Asset Pipeline refresh |
| First Scene river render | cold context-A shader time |
| Press Play to first responsive frame | domain + context-B shader time |
| Select river with Inspector already open | context-C preview time |
| Second selection without editing shader | warm reuse time |

Use `Editor.log`, process CPU, and shader-cache timestamps. Do not rely only on the wording of the modal progress window.

### Phase 1 — Recover shader iteration time

Recommended order:

1. Establish which parts of `RiverWaterFoamEvaluateSelectionDiagnostics` are production requirements versus diagnostics.
2. Reduce the compiler-visible candidate count while preserving a visual reference set.
3. Remove diagnostic-only candidate work from the normal fragment path.
4. Measure one cold no-keyword, Play, and Inspector variant after every structural change.
5. Audit the URP keyword matrix and strip only proven-impossible variants.
6. Design the field/buffer/texture producer that removes broad candidate construction from the surface fragment permanently.

Do not start by creating a large Shader Variant Collection. Prewarming the current body can front-load several four-minute variants and make Editor startup worse.

### Phase 2 — Reduce script-edit recompilation scope

After dependency direction is documented:

1. introduce the smallest shared contracts/runtime assembly;
2. separate subsystem runtime assemblies;
3. move Editor code into Editor-only assemblies;
4. resolve Ground/River communication through shared snapshots/contracts rather than cyclic assembly references;
5. compare river-only edit compile time before and after.

### Phase 3 — Implement the Ground-owned transaction coordinator

Follow `Ground_River_Regeneration_Orchestration_Manual.md`. The objective is one coalesced generation transaction per dirty/restoration wave, with versioned publications for future ground-dependent features.

This phase should specifically remove direct re-entrant “feature changed, therefore regenerate provider now” behavior. Features submit dirtiness and consume snapshots; the coordinator owns order and bounded iteration.

### Phase 4 — Resolve cache mismatch and Editor amplifiers

After the dominant stalls are gone:

- stabilize obstacle/combined foam cache signatures;
- verify Profiler default recording behavior;
- keep Search startup indexing off until normal timings are established;
- address URP shadow-atlas warnings;
- decide whether the river Inspector should use a lightweight preview.

## 9. Fix candidate comparison

| Candidate | Targets | Expected impact | Confidence | Main risk |
|---|---|---:|---:|---|
| Reduce 55-candidate unrolled search | Scene, Play, Inspector shader compilation | Critical | 95% | Loss of extreme chip coverage/motion |
| Move chip field to compute/texture/buffer | Compile time and likely runtime GPU cost | Critical, durable | 90% | Larger architecture/data-lifetime change |
| Isolate diagnostic shader path | All normal river rendering variants | Very high | 92% | New keyword/pass can create drift or variants |
| Strip impossible URP variants | Repeated context compilation/build size | High multiplier | 90% after audit | Missing variants/pink shader if wrong |
| Lightweight/suppressed Inspector preview | Inspector selection only | High locally | 85% | Less useful preview; does not help Play |
| Persistent river material asset | Preview identity/cache stability | Low–medium | 55% | Does not survive shader source invalidation |
| Assembly definitions | Script-edit compile time | High | 90% | Dependency cycles and hidden references |
| Ground transaction coordinator | Ordinary restoration and dirty-time work | High | 85% | Ordering/Undo/edit-mode regressions |
| Foam cache signature stabilization | Startup predictability/correctness | Low for current minutes | 90% | Unsafe hits if validation weakened |
| Search/Profiler setting changes | Environmental amplification | Low–medium | 40–60% | Masks symptoms without fixing shader |

## 10. Functionality risk assessment

There is a real chance of breaking existing river visuals if the primary shader fix is done by simply reducing bounds. The current search expansion appears designed to preserve chips that move laterally, rotate, pulse, or morph beyond their source cell. A safe implementation must distinguish:

- **visual reach:** the maximum screen/world footprint a candidate can occupy;
- **motion reach:** how far a candidate can move from its deterministic source cell;
- **selection reach:** how many source cells a fragment must consider to find every candidate that could cover it;
- **diagnostic reach:** additional candidates required only to visualize why a candidate was selected/rejected.

The best low-risk long-term design does not pretend the reach is smaller. It changes where the broad search occurs. A prepared field can retain the full reach while preventing every lighting variant and every fragment from embedding 55 procedural evaluations.

Variant stripping also has meaningful functional risk. Scene view, preview cameras, and player cameras do not always share the same keyword set. Stripping must be based on captured/declared rendering configurations, not on the one currently visible Game camera.

Assembly definitions and orchestration changes can break edit-time generation even when Play mode appears correct. Validation must include code reload, Undo/Redo, scene restoration, river changes, ground changes, and saved/reopened scene state.

## 11. Validation plan for an eventual patch

1. Make one controlled river shader/include edit and record cold Scene, Play, and Inspector-selection times plus corresponding shader-cache outputs; each must meet the agreed budget and the second warm selection must not recompile.
2. Compare river reference views at minimum/default settings and maximum chip lateral motion, shape, size, rotation, pulse, bank interaction, fog, main-light shadows, additional-light shadows, Forward+, and soft shadows; verify no missing, clipped, popping, or pink output.
3. Edit one river-only C# file and verify which assemblies rebuild and the total `CompileScripts` time; confirm unrelated Ground and Mass assemblies remain untouched once assembly boundaries exist.
4. Trigger Ground-only, River-only, and combined dirty events in Edit mode and Play startup; verify one coalesced Ground transaction, one river surface build after ground geometry, one bank/corridor publication, and no re-entrant full regeneration.
5. Validate future-feature ordering with a bank-dependent ground consumer: it must read version-matched ground geometry and river-bank data without requesting an upstream regeneration itself.
6. Save/reload the scene and exercise Undo/Redo, prefab/scene restoration, Inspector selection, and two consecutive Play entries; compare geometry, collider, material, foam cache signature, and total restoration timings against the baseline.

## 12. Evidence index

### Project source

| Evidence | Location |
|---|---|
| Shader properties and variant pragmas | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader:126,185–193` |
| Unconditional diagnostic evaluation call | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader:933–958` |
| Selection diagnostic implementation | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl:355–784` |
| Dynamic reach calculations | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl:500–509` |
| Explicit 5 × 11 unrolled candidate loops | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl:512–516` |
| Ground OnEnable/regeneration | `Assets/Game/Procedural/Ground/GeneratedGround.cs:627–632,659 onward` |
| Ground river-change request | `Assets/Game/Procedural/Ground/GeneratedGround.cs:875–889` |
| Ground-to-river corridor notification | `Assets/Game/Procedural/Ground/GeneratedGround.cs:1820–1838` |
| River OnEnable/regeneration | `Assets/Game/Procedural/Rivers/StylizedRiver.cs:3339–3358,3739–3758` |
| River-to-ground notification | `Assets/Game/Procedural/Rivers/StylizedRiver.cs:6037–6047` |
| Generated mesh and material assignment | `Assets/Game/Procedural/Rivers/StylizedRiver.cs:5272–5290` |
| Temporary material creation | `Assets/Game/Procedural/Rivers/StylizedRiver.cs:5482–5538` |
| Custom Inspector repaint rule | `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs:111 onward` |
| River scene mesh/material null references | `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity:10422,10797` |

### Project settings

| Evidence | Location |
|---|---|
| Async shader compilation enabled | `ProjectSettings/EditorSettings.asset:25` |
| Shader compilation logging disabled | `ProjectSettings/GraphicsSettings.asset:64` |
| Strict variant matching disabled | `ProjectSettings/ProjectSettings.asset:196` |
| Search startup indexing disabled; selection tracking enabled | `UserSettings/Search.settings` |
| Inspector preview saved expanded | `UserSettings/Layouts/default-6000.dwlt` |

### Session log

| Evidence | Location |
|---|---|
| Latest ordinary domain reload | `Logs/Editor.log:3452–3471` |
| Latest script compile | `Logs/Editor.log:3505` |
| Shader modified callback | `Logs/Editor.log:3516` |
| Play reload begins | `Logs/Editor.log:3652` |
| 242.547-second Play domain reload | `Logs/Editor.log:3675` |
| 234.344-second child-domain wait | `Logs/Editor.log:3678` |
| Backup-scene integration | `Logs/Editor.log:3759` |
| Foam startup/cache telemetry | `Logs/Editor.log:3848` |

## 13. Final priority order

1. **Recover the river shader from the 55-candidate unrolled fragment design.** This is the only finding that explains all three shader-related contexts and the measured four-minute binaries.
2. **Separate production and diagnostic shader responsibilities, then constrain variants.** This prevents the fixed body from being multiplied unnecessarily.
3. **Add assembly boundaries after resolving dependency direction.** This addresses the independent 3–28-second script compilation cost.
4. **Implement the Ground-owned generation transaction.** This addresses repeatable restoration/dirty-time duplication and provides the extensible dependency system needed by future ground features.
5. **Stabilize foam cache signatures and remove Editor amplifiers.** These improve predictability and observability after the critical paths are fixed.

The decisive point is that the river's procedural CPU caches are not restarting four minutes of generation on every context. The current shader is being compiled on demand multiple times, and each required variant has become pathologically expensive. Ground/River regeneration and monolithic C# compilation remain real problems, but they are separate costs and should be fixed without obscuring the shader root cause.
