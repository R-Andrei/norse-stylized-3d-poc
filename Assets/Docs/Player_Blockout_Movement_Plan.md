# Player Blockout Movement Plan

## Status

- Patch ID: `PBM-1`
- Objective: add minimal Rigidbody-driven XZ movement to `Player_Blockout` with independent cursor-derived facing.
- Authorization: approved by the user on 2026-07-21.
- Current phase: `PBM-3` implementation, runtime-source compilation, and static consistency audit complete; Unity Play Mode feel/collision validation remains pending. `PBM-2` camera Play Mode behavior and the shared Profiler validation also remain pending.

## Acceptance criteria

- The existing `Player/Move` action drives planar movement with `W`, `A`, `S`, and `D`.
- Simultaneous orthogonal inputs produce diagonal movement.
- Input magnitude is clamped to one before speed is applied, so a digital diagonal has the same maximum planar speed as a cardinal input.
- Movement is applied to a dynamic `Rigidbody` during `FixedUpdate`; gravity and vertical velocity remain owned by physics.
- Starting movement and changing direction select the commanded planar velocity immediately; release deceleration remains bounded and frame-rate independent through `Time.fixedDeltaTime`.
- The existing `UI/Point` screen position drives yaw independently from movement by intersecting the camera ray with a horizontal plane through the Rigidbody position.
- The player can face one world direction while moving in any other world direction.
- Pitch and roll remain constrained while yaw remains available for cursor facing.
- The existing capsule collider, player transform, camera target, aura, weather-field anchor, layers, tags, input bindings, and unrelated scene content remain unchanged.
- Unity compiles the final runtime implementation without errors.
- Scene serialization retains valid references to the input action asset, main camera, movement component, and Rigidbody.

## Approved files

### Create

- `Assets/Docs/Player_Blockout_Movement_Plan.md`
- `Assets/Game/Input/PlayerBlockoutMovement.cs`

### Metadata/companion

- `Assets/Docs/Player_Blockout_Movement_Plan.md.meta`
- `Assets/Game/Input.meta`
- `Assets/Game/Input/PlayerBlockoutMovement.cs.meta`

### Modify

- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`

### Explicitly unchanged

- `Assets/InputSystem_Actions.inputactions`
- `Assets/InputSystem_Actions.inputactions.meta`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/TimeManager.asset`
- `ProjectSettings/DynamicsManager.asset`
- all layers, tags, prefabs, materials, shaders, generated geometry, camera components, weather code, and ground code

## Reviewed evidence

- `AGENTS.md` and `Assets/AGENTS.md`: implementation requires a recorded plan before code, strict scope control, final consistency audit, and Unity validation.
- `Assets/Docs/Proof of Concept/07_Implementation_Guide_for_Proof_of_Concept.md`, section 8: the prototype requires a very simple new-Input-System controller moving on XZ and facing movement or cursor direction.
- `Assets/Docs/Proof of Concept/06_Proof_of_Concept.md`, project setup and variation boundaries: the prototype uses the new Input System exclusively, needs a minimal controller, and retains a fixed camera contract.
- `Assets/Docs/Proof of Concept/08_Proof_of_Concept_Implementation_Log.md`, initial implementation and current limitations: the scene has a player capsule, Cinemachine follow camera, player-centred aura, and no movement controller.
- `Assets/Docs/Proof of Concept/10_Project_Architecture_and_Asset_Organisation_Rules.md`: reusable input behaviour belongs under `Game/Input/`; C# filenames match their principal public type; reusable namespaces remain project-generic.
- `Assets/Docs/handoff.md`: active-gameplay runtime cost has highest performance priority; every update requires exact expected and actual file reconciliation.
- `Assets/InputSystem_Actions.inputactions`, complete asset: `Player/Move` is a `Vector2` value action with WASD and other device bindings; `UI/Point` is a `Vector2` pass-through action with mouse, pen, and touch position bindings. The working asset is identical to `HEAD`.
- `Assets/InputSystem_Actions.inputactions.meta`: wrapper generation is disabled, so the controller will use the existing `InputActionAsset` directly rather than add a generated wrapper.
- `Packages/manifest.json`: Input System `1.19.0`, Cinemachine `3.1.7`, and Unity physics are already present; no dependency change is required.
- `ProjectSettings/ProjectSettings.asset`: `activeInputHandler: 1` selects the new Input System.
- `ProjectSettings/TimeManager.asset`: fixed timestep is `0.02` seconds, producing a normal target of 50 physics ticks per second.
- `ProjectSettings/DynamicsManager.asset`: gravity is `(0, -9.81, 0)`, automatic simulation is enabled, and the default collision matrix permits the current layer-zero player and environment to collide.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, complete relevant object records: `Player_Blockout` currently has Transform, MeshFilter, MeshRenderer, and an enabled non-trigger CapsuleCollider, but no Rigidbody or movement component. Its current transform is user-owned working-tree state and must not be replaced.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, direct dependents: child `CameraTarget` is the Cinemachine tracking target; child `Aura` is a point light; `WeatherWindDomain.fieldAnchor` references the player Transform; the main camera is tagged `MainCamera` and uses perspective projection.
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`, `ComputeDesiredOriginCell` and `ResolveAnchorPosition`: the wind field reads the player anchor position and recentres on cell changes. Player movement therefore activates existing recenter work without changing weather code.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` and `Assets/Game/Procedural/Ground/GeneratedGround.cs`: the enabled ground MeshCollider receives the generated ground mesh and is the existing ground collision producer.
- Repository search found no current C# movement controller, `PlayerInput`, `InputActionReference`, runtime Rigidbody consumer, or automatic Collider/Rigidbody scan in the river, vegetation, or weather modules.
- Git history: commit `74a7ed1` introduced the existing input actions and player blockout; current `HEAD` is `0122633ca1057bb3270e462f93142ab4effb09c3`. The scene has extensive unrelated working-tree changes. At final review before planning, its SHA-256 was `C740F29A83C4C977C8C814F1A94574CB6FDA48B5D933FC6BAC67BF01A29F2C6A`; the current player position was `(-0.15, 0.63, -11.12)`, while `HEAD` contains `(-8.45, 0.82, -11.73)`. Only additive player components and their serialized fields may be introduced.

## Design and invariants

- Runtime type: `ProgrammaticStylized3D.Input.PlayerBlockoutMovement`.
- The component requires and caches one `Rigidbody`; it does not search the scene per frame.
- Serialized references: the existing `InputSystem_Actions` asset and existing main camera.
- Action lookup occurs once during initialization using `Player/Move` and `UI/Point`; no per-tick string lookup occurs.
- The movement action is clamped with `Vector2.ClampMagnitude(value, 1f)`. For raw digital diagonal `(1, 1)`, the resulting value is `(1/sqrt(2), 1/sqrt(2))`; multiplying by speed therefore retains the same magnitude as a cardinal vector.
- Desired world movement is `(input.x, 0, input.y) * maximumSpeed` and does not depend on facing.
- Planar velocity selects desired velocity directly while input is above the drift epsilon and approaches zero using deceleration while input is absent. The current Y velocity is preserved.
- Cursor facing uses `Camera.ScreenPointToRay` and `Plane.Raycast` against a horizontal plane through `Rigidbody.position`; it performs no physics raycast, layer query, tag query, allocation, or scene scan.
- Cursor yaw uses `Rigidbody.MoveRotation` and `Quaternion.RotateTowards`; movement never supplies the facing direction.
- Current tuning after `PBM-2`/`PBM-3`: maximum speed `5 m/s`, immediate nonzero-input response, release deceleration `40 m/s^2`, and yaw speed `720 degrees/s`.
- Rigidbody configuration after `PBM-2`: mass `3`, gravity enabled, non-kinematic, interpolation enabled, discrete collision detection, and X/Z rotation frozen. Discrete collision is the lowest-cost suitable current mode at `5 m/s` and `0.02 s` fixed timestep; the maximum commanded displacement is `0.1 m` per tick against a radius-`0.5 m` capsule. Tunnelling validation remains required before considering a higher-cost continuous mode.
- No jump, sprint, dash, combat, slope solver, ground probe, camera orbit, layer, tag, prefab, or input-binding work is in scope.

## Performance analysis

### Baseline

- Measured baseline timing is unavailable. Current movement-controller CPU cost and dynamic-player physics cost are zero because no controller or player Rigidbody exists.
- The stationary player currently leaves the existing weather anchor stationary after initialization.

### Added active-gameplay cost

- `Awake`/enable: two action lookups and two action enables execute once per component lifecycle. Lookup time is `O(A)` over the small existing action asset; retained state is two action references and cached component/camera references.
- `FixedUpdate`: one player executes two `Vector2` reads, constant-count vector arithmetic, at most one square root for clamping, one ray-plane intersection, and bounded quaternion arithmetic. Time and space are `O(1)` per physics tick and `O(P)` across `P` players; this patch configures `P = 1` at 50 ticks/second.
- Managed allocations: analytically expected zero per physics tick because all values are structs and no collections, LINQ, strings, delegates, or object creation occur in the hot path. This is unmeasured and must be checked in the Unity Profiler.
- Physics: one dynamic capsule is added. Broadphase and solver cost scale with nearby overlaps/contacts; this scene adds one body and normally one ground contact. CPU cost is unmeasured.
- GPU/rendering: the controller adds no draw, shader, texture, upload, or direct GPU work.
- Existing weather consequence: movement can change the anchor cell. With `0.5 m` weather cells and `5 m/s` maximum movement, axial travel can cross at most about 10 cells/second; `WeatherWindDomain` may issue its existing recenter dispatch when those changes are observed. No weather allocation or texture count changes. GPU cost is unmeasured and must be profiled while moving continuously.
- Storage: one small C# source, metadata, this Markdown plan, and additive scene serialization. No build-time asset, texture, mesh, or cache is added.

### Alternatives considered

- Direct Transform translation was rejected because it bypasses the requested Rigidbody collision/gravity path.
- Per-frame physics raycasts for cursor facing were rejected because a mathematical plane supplies the requested cursor direction without collision-query cost or layer semantics.
- `ContinuousDynamic` collision was deferred because it costs more than discrete collision and no current tunnelling evidence requires it.
- A generated input wrapper was rejected because wrapper generation is currently disabled and the existing asset can be referenced directly.
- A `PlayerInput` component was rejected because the controller needs only two existing actions and can own their lifecycle without an additional messaging/configuration layer.
- Direct keyboard polling was rejected because it would duplicate the existing action contract and discard its current device bindings.

### Measurement plan

- In the Unity Profiler, compare stationary Play Mode with continuous cardinal and diagonal movement after warm-up. Record `PlayerBlockoutMovement.FixedUpdate`, `Physics.Simulate`, GC Alloc, frame time, and weather recenter dispatch counters over at least 10 seconds.
- Pass criteria: zero GC allocation attributed to the controller after initialization; no sustained frame-time regression visible above profiler noise for one player; no unexpected weather resource rebuild; recenter dispatches occur only when anchor cells change.
- `PERFORMANCE EXCEPTION`: none selected. The dynamic body and fixed-tick controller are necessary for the authorized physics-based movement. Existing weather recenter work is an integration consequence of the already-authored player anchor, not a new algorithm or resource.

## File-by-file implementation sequence

| Step | Status | File | Change and verification |
|---|---|---|---|
| PBM-1P | Completed | `Assets/Docs/Player_Blockout_Movement_Plan.md` and `.meta` | Review, scope, design, risks, validation requirements, and initial status were recorded before runtime or scene edits. |
| PBM-1A | Completed | `Assets/Game/Input.meta`, `Assets/Game/Input/PlayerBlockoutMovement.cs`, `.meta` | Added the runtime component with cached action/body references, bounded planar velocity, diagonal clamping, and cursor-plane yaw. |
| PBM-1B | Completed; Unity load pending | `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Added/configured one Rigidbody and one movement component and assigned the existing action asset and main camera. The pre-edit player Transform and all existing component references were retained. Unity batch serialization was unavailable due licensing, so the additive YAML records were written directly and statically verified; licensed Unity loading remains required. |
| PBM-1C | Completed | `Assets/Game/Input/PlayerBlockoutMovement.cs` | Final runtime source contains no editor namespace, installer, setup method, or conditional editor block. |
| PBM-1V | Partially completed | all approved files | Full runtime-source compilation, serialized-reference inspection, final diff inspection, complete modified-file reread, and static consistency checks passed. Unity import, Play Mode behavior, and Profiler checks remain pending. |

## Risks and required responses

- The scene is already heavily modified. Before scene setup, recheck its timestamp, hash, and player transform. If it changed after this plan, use the newest state and record the drift before mutation; never restore the prior snapshot.
- Unity serialization may touch undeclared source assets. If any source file outside the approved list changes, stop, preserve the evidence, update this plan, and resolve scope before continuing.
- Missing input asset or camera references must disable the controller with a clear error; they must not fall back to legacy input or a scene-wide repeated search.
- If final Unity compilation is unavailable, mark compilation pending and do not report the patch complete.
- Runtime feel, collision, cursor mapping, and performance require Play Mode observation. If interactive Play Mode cannot be executed automatically, mark those checks pending with concrete user steps.

## Validation ledger

| Check | Status | Evidence / pass condition |
|---|---|---|
| Scope and dirty-tree baseline | Passed | Git status/diff/history inspected; unrelated changes recorded; relevant input asset matches `HEAD`. |
| Canonical architecture and contracts | Passed | Applicable instructions, proof-of-concept guidance, architecture rules, handoff performance rules, relevant scene records, input maps, physics settings, ground collision, camera, aura, and weather anchor inspected. |
| Plan gate | Passed | This file and its metadata existed before `PBM-1A`; implementation followed its approved file scope. |
| Runtime-source compilation | Passed | Unity 6000.5.0f1's bundled .NET 8 Roslyn compiler compiled the complete current `Assembly-CSharp.rsp` source/reference set plus `PlayerBlockoutMovement.cs` with exit code `0`; outputs were `Temp/PBM-1A-Runtime.dll` (2,527,744 bytes) and `Temp/PBM-1A-Runtime.ref.dll` (529,408 bytes). |
| Unity compile/import | Pending — licensing blocker | `Logs/PBM-1A-Compile.log` records a licensing timeout, missing `com.unity.editor.headless`, loss of the Unity Licensing Client connection, and reconnect attempts. The editor never reached asset import or Unity-managed compilation. |
| Scene serialization | Static checks passed; Unity load pending | Static assertions found exactly one new Rigidbody record and one new movement-component record; the script GUID, input-asset GUID/file ID, camera file ID, Rigidbody settings, and component list match this plan. The player Transform remains `(-0.15, 0.63, -11.12)`, identity rotation, unit scale. Final scene SHA-256 is `BB98978BDCC706015201B064146B5CA0817714845079BA6DE694B6428C48F4D8`. |
| Static consistency audit | Passed | The audit returned `StaticAudit=PASS` and `DiagonalSpeed=5`. Final source uses `Vector2.ClampMagnitude`, preserves Y velocity, maps X/Y input to world X/Z independently of yaw, uses a horizontal cursor plane and `Rigidbody.MoveRotation`, and contains no editor installer. `Assets/InputSystem_Actions.inputactions` remains unchanged. |
| Play Mode behavior | Pending | Cardinal/diagonal movement, independent cursor facing, gravity, collision, camera/aura/weather following, and no unintended pitch/roll observed. |
| Performance | Pending | Unity Profiler procedure above; no measured result is claimed before capture. |

## Post-change audit record

### Actual affected source files

- Created `Assets/Docs/Player_Blockout_Movement_Plan.md` and `.meta` for `PBM-1P` and this audit record.
- Created `Assets/Game/Input.meta`, `Assets/Game/Input/PlayerBlockoutMovement.cs`, and `.meta` for `PBM-1A` and `PBM-1C`.
- Modified `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` for `PBM-1B`; no other source asset was changed by this patch.
- Compiler outputs under `Temp/` and the batch-editor log under `Logs/` are validation artifacts, not source changes.

### Intentional differences

- `PBM-1` initially added one dynamic Rigidbody with mass `1`; `PBM-2` superseded that tuning with current mass `3`. Gravity, interpolation, discrete collision detection, and X/Z rotation constraints remain enabled.
- `Player_Blockout` now has one `PlayerBlockoutMovement` component referencing the existing `InputSystem_Actions` asset and existing main camera.
- The runtime component owns only planar movement and yaw: WASD input is clamped to unit magnitude, `PBM-3` makes nonzero-input start/direction changes immediate, release deceleration remains bounded using the fixed timestep, vertical velocity is preserved, and cursor position determines yaw independently through a horizontal mathematical plane.

### Preserved state and cross-module consistency

- The input action asset, packages, project settings, layers, tags, collider, player Transform, children, camera configuration, aura, weather code/anchor reference, ground code/collider, shaders, materials, and generated geometry are unchanged by this patch.
- Comparison with the captured pre-edit scene state confirms that the player position remains `(-0.15, 0.63, -11.12)` and that the only intended player-object additions are component file IDs `2100000001` and `2100000002` plus their records.
- Comparison with `HEAD` confirms that extensive unrelated scene and repository changes predate this patch. They were retained and are outside `PBM-1`.
- The scene's five current `git diff --check` trailing-whitespace reports are pre-existing unrelated records at diff lines 1982, 5461, 8223, 8623, and 14382. The added controller record has no whitespace warning.

### Validation result and deviations

- The complete current runtime response set plus the new controller compiled with Unity's bundled Roslyn compiler at exit code `0`. This proves C# source/reference compatibility for the current runtime assembly; it does not prove Unity asset import, scene deserialization, or Play Mode behavior.
- Static scene and controller assertions passed, including exact component multiplicity, serialized references, Rigidbody settings, preserved player Transform, diagonal-speed calculation, movement/facing separation, and absence of editor-only setup code.
- The planned Unity-driven scene serialization path could not run because Unity batch mode did not obtain a licensing connection. The scene addition was therefore applied as a narrowly scoped raw serialization edit using the existing imported action artifact's file ID and inspected scene file IDs. This is a documented implementation deviation. Licensed Unity scene loading is still required to validate deserialization.

### Remaining checks and concrete next actions

- Open the project in a normally licensed Unity 6000.5.0f1 editor, allow import/compilation to finish, and verify that the Console contains no compiler, missing-script, or deserialization errors.
- Run Play Mode checks for cardinal and diagonal speed, movement independent from cursor yaw, gravity/ground collision, rotation constraints, and continued camera/aura/weather-anchor following.
- Capture the Profiler measurements defined above while stationary and during sustained cardinal and diagonal movement. Until these checks are recorded, `PBM-1` is implemented and statically/source-validated but is not reported as fully validated or complete.

## PBM-2 — Player mass and fixed-angle follow camera

### Status and authorization

- Objective: increase the player's Rigidbody mass slightly and remove movement-induced camera rotation while preserving the existing smooth positional follow.
- Authorization: approved by the user on 2026-07-22.
- Current status: scene implementation and static consistency audit complete; Unity Play Mode observation pending.

### Acceptance criteria

- `Player_Blockout` Rigidbody mass changes from `1` to `3`.
- `CM_PlayerFollow` retains its existing Cinemachine Camera, world-space Follow component, tracking target, follow offset, positional damping, lens, and Transform rotation.
- `CM_PlayerFollow` no longer runs its Rotation Composer, so player translation cannot cause that component to pan or tilt the camera.
- The virtual-camera Transform remains at its authored rotation `{x: 0.407275, y: -0.000000013609314, z: 0.0000000060688703, w: 0.9133056}` and supplies the fixed camera orientation when no valid Aim component runs.
- Player movement, cursor-facing yaw, gravity, collider, Rigidbody interpolation/constraints/collision mode, camera position damping, and all unrelated scene state remain unchanged.
- Static serialization checks pass; licensed Unity Play Mode observation remains required for camera comfort and collider traversal.

### Approved patch scope

- Modify `Assets/Docs/Player_Blockout_Movement_Plan.md` to record `PBM-2` review, implementation status, and audit evidence.
- Modify `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` only at `Player_Blockout` Rigidbody `m_Mass` and `CM_PlayerFollow` Rotation Composer `m_Enabled`.
- Explicitly unchanged: `Assets/Game/Input/PlayerBlockoutMovement.cs`, input assets, camera Brain, Cinemachine Camera, Cinemachine Follow settings, camera/player Transforms, colliders, layers, tags, project settings, packages, and every unrelated scene record.

### Reviewed evidence and conclusion

- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, `Player_Blockout` file IDs `1611485499`/`2100000001`: the current dynamic Rigidbody has mass `1`, gravity enabled, interpolation enabled, discrete collision detection, and X/Z rotation constrained. The player Transform remains `(-0.15, 0.63, -11.12)`.
- `Assets/Game/Input/PlayerBlockoutMovement.cs`, `ApplyMovement`: the controller writes planar velocity and preserves the Rigidbody's current Y velocity. It contains no step-height or slope-rejection solver.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, `CM_PlayerFollow` file IDs `1533091818`–`1533091822`: the camera has an enabled Rotation Composer, a Follow component with `BindingMode: 4`, position damping `(0.35, 0.45, 0.35)`, follow offset `(0, 20, -20)`, and tracking target `CameraTarget` file ID `1578696517`.
- `Library/PackageCache/com.unity.cinemachine@f3f96bcb59af/Runtime/Core/TargetTracking.cs`, `BindingMode`: serialized value `4` is `WorldSpace`; `GetReferenceOrientation` returns identity for that mode. The Follow offset therefore does not inherit player yaw.
- `Library/PackageCache/com.unity.cinemachine@f3f96bcb59af/Runtime/Components/CinemachineRotationComposer.cs`, `MutateCameraState`: this Aim-stage component pans/tilts the camera to keep its LookAt target framed. `CameraTarget.CustomLookAtTarget` is false, so the Tracking target is also the effective LookAt target under `CameraTarget.cs`.
- `Library/PackageCache/com.unity.cinemachine@f3f96bcb59af/Runtime/Core/CinemachineVirtualCameraBase.cs`, `PullStateFromVirtualCamera`, and `Runtime/Behaviours/CinemachineCamera.cs`, `InternalUpdateCameraState`: camera state starts from the virtual-camera Transform orientation and only valid pipeline components mutate it. Disabling the Rotation Composer while retaining Follow therefore keeps positional tracking but stops Aim-stage rotation.
- `Assets/Docs/Proof of Concept/07_Implementation_Guide_for_Proof_of_Concept.md`, camera rules: keep camera movement simple, fixed if possible, follow gently, and do not add free rotation. A fixed orientation with damped positional follow conforms to that contract.
- Historical comparison: commit `74a7ed1` and current `HEAD` `0122633ca1057bb3270e462f93142ab4effb09c3` contain the same enabled Rotation Composer and world-space Follow structure. The movement patch exposed the pre-existing aim/follow interaction once the tracking target began translating.
- User observation on 2026-07-22: pressing `D` produces a slight opposite camera rotation and the ongoing rotation causes discomfort. This is the required behavioral validation evidence for removing the Aim stage.
- Physics limitation: mass affects response to forces and collisions with other dynamic bodies, but not gravitational acceleration (`F = m g`, `a = F / m = g`) and it does not prevent a capsule contact normal from redirecting motion upward against a static collider. Raising mass to `3` is the requested slight weight increase, not a proven step-blocking solution.
- Baseline scene evidence before `PBM-2`: SHA-256 `BB98978BDCC706015201B064146B5CA0817714845079BA6DE694B6428C48F4D8`, length `470885` bytes, last write `2026-07-21 20:35:08 UTC`. The scene contains extensive unrelated user-owned changes relative to `HEAD`; they must be preserved.

### Invariants, non-goals, risks, and performance

- No controller logic, step solver, slope limit, additional gravity, Physics Material, collider shape, or camera script is added.
- Disabling the Rotation Composer removes its per-frame Aim-stage work and cannot add runtime CPU, allocation, physics, GPU, or storage cost. Follow position damping remains unchanged.
- Increasing mass changes collision impulse distribution only when the player interacts with other non-kinematic bodies. Static-world traversal may remain unchanged; Play Mode will verify or falsify improvement.
- The fixed orientation means the player can move away from exact screen-center temporarily while positional damping catches up. The existing `(0.35, 0.45, 0.35)` damping is intentionally retained.
- Raw scene serialization is necessary because the prior batch-editor attempt could not establish licensing. Only the two planned scalar fields may change.

### File-by-file implementation sequence

| Step | Status | File | Change and verification |
|---|---|---|---|
| PBM-2P | Completed | `Assets/Docs/Player_Blockout_Movement_Plan.md` | Record review, scope, behavior, physics limitation, risks, and validation before scene editing. |
| PBM-2A | Completed; Play Mode pending | `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Changed `Player_Blockout` Rigidbody `m_Mass: 1` to `3`; static assertions confirm every other audited Rigidbody field is unchanged. |
| PBM-2B | Completed; Play Mode pending | `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Changed `CM_PlayerFollow` Rotation Composer `m_Enabled: 1` to `0`; static assertions confirm Follow, Camera target, damping, offset, and Transform rotation are preserved. |
| PBM-2V | Partially completed | both approved files | Final scene records and plan reread, exact edit scope reconciled, static serialization assertions passed, and performance impact audited. Interactive Unity observation remains pending. |

### PBM-2 validation ledger

| Check | Status | Evidence / pass condition |
|---|---|---|
| Review and plan gate | Passed | Current scene/controller, direct camera package contracts, canonical camera guidance, Git status/diff/history, prior plan, and pre-edit scene identity reviewed and recorded before scene modification. |
| Scene scope | Passed | The `PBM-2` scene patch changed only Rotation Composer `m_Enabled: 1` to `0` and Rigidbody `m_Mass: 1` to `3`. The canonical plan is the only other modified source file. |
| Camera serialization | Passed statically | `PBM2StaticAudit=PASS`: Rotation Composer file ID `1533091819` exists once and is disabled; Follow file ID `1533091820` exists once and remains enabled with binding `4`, damping `(0.35, 0.45, 0.35)`, offset `(0, 20, -20)`, tracking target `1578696517`, and the original virtual-camera Transform rotation. |
| Rigidbody serialization | Passed statically | `PBM2StaticAudit=PASS`: Rigidbody file ID `2100000001` exists once with mass `3`; gravity `1`, interpolation `1`, constraints `80`, collision mode `0`, and player position `(-0.15, 0.63, -11.12)` remain unchanged. |
| Unity Play Mode | Pending | Camera translates without yaw/pitch response to WASD, retains the authored framing angle, and movement/cursor-facing remain functional. |
| Collider traversal | Pending | User checks the reported short-collider case; if climbing persists, plan an explicit step/slope-blocking patch rather than further mass tuning. |
| Performance | Static audit passed; measurement pending | No code or component was added, and one per-frame Cinemachine Aim component was disabled. This patch cannot add controller/Aim execution work; no new Profiler measurement is claimed. |

### PBM-2 post-change audit

#### Actual files and intentional differences

- Modified `Assets/Docs/Player_Blockout_Movement_Plan.md` to record the required `PBM-2` review, scope, implementation state, validation evidence, and limitations.
- Modified `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` at exactly two planned scalar fields: Rotation Composer `m_Enabled` is now `0`, and player Rigidbody `m_Mass` is now `3`.
- No C# source, metadata, input asset, package, project setting, layer, tag, collider, Transform, material, shader, or generated asset was changed by `PBM-2`. `PlayerBlockoutMovement.cs` retains length `6419` bytes and last-write time `2026-07-21 20:33:26 UTC`.

#### Final consistency and compliance result

- Static assertions passed for exact component multiplicity and all planned/preserved camera and Rigidbody fields; output was `PBM2StaticAudit=PASS`.
- Final scene SHA-256 is `59943C251E2554E15C3C418ED8F314FFF02E66A8AD493F79871EFD54A10AFE10`; file length remains `470885` bytes. The unchanged length is consistent with two single-character scalar replacements and is not standalone proof of scope.
- Final `git diff --check` reports the same five unrelated pre-existing scene whitespace warnings at diff lines 1982, 5461, 8223, 8623, and 14382. Neither `PBM-2` field has a whitespace warning.
- The final Rotation Composer, Follow, Cinemachine Camera, virtual-camera Transform, player Transform, Rigidbody, movement component, and package contracts were reread after editing. The final fields match the `PBM-2` plan.
- No compilation was run because `PBM-2` changes only serialized scalar values and contains no source change. Two Unity 6000.5.0f1 processes were active during final audit, so a second batch editor was not launched against the project.

#### Pending behavior and limitation

- Interactive validation remains required: confirm that WASD produces camera translation without yaw/pitch response and that the fixed authored angle remains comfortable.
- Increased mass is implemented but remains unverified for the reported collider case. If the capsule still climbs a short static collider, mass tuning is exhausted as the proposed mechanism; the concrete next action is a separately planned step/slope-blocking movement change.

## PBM-3 — Immediate movement start and direction changes

### Status and authorization

- Objective: remove acceleration delay whenever movement input is nonzero while retaining bounded deceleration after movement input is released.
- Authorization: approved by the user's explicit movement-response requirement on 2026-07-22.
- Current status: implementation, runtime-source compilation, and static consistency audit complete; Unity Play Mode observation pending.

### Acceptance criteria

- Any digital WASD input, or analog `Player/Move` input above the existing drift epsilon, becomes the commanded planar velocity during the next `FixedUpdate`; there is no acceleration ramp for starting movement or changing direction.
- Reversing from one full cardinal direction to its opposite assigns the opposite `5 m/s` planar velocity in one physics update.
- Releasing all movement input retains the current `40 m/s²` deceleration. From `5 m/s`, the analytical stop time remains `5 / 40 = 0.125 s` before collision response.
- Digital diagonals remain clamped to unit magnitude and therefore remain capped at the same `5 m/s` speed as cardinal movement.
- Current Y velocity, Rigidbody collision response, cursor-facing behavior, camera configuration, input bindings, and all unrelated scene/controller behavior remain unchanged.
- The obsolete acceleration serialized field is removed from the component source and scene record so the Inspector does not expose an ineffective control.

### Approved patch scope

- Modify `Assets/Docs/Player_Blockout_Movement_Plan.md` for the required `PBM-3` review, plan, and audit.
- Modify `Assets/Game/Input/PlayerBlockoutMovement.cs` only in the movement configuration validation and `ApplyMovement` velocity-selection path.
- Modify `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` only to remove the obsolete `acceleration: 30` field from `PlayerBlockoutMovement` file ID `2100000002`.
- Explicitly unchanged: maximum speed, deceleration, rotation speed, Rigidbody settings, Cinemachine settings, input assets, camera/player Transforms, colliders, layers, tags, packages, project settings, and unrelated scene content.

### Reviewed evidence and design

- `Assets/Game/Input/PlayerBlockoutMovement.cs`, complete current type and `ApplyMovement`: nonzero input and zero input currently share `Vector3.MoveTowards`; only `changeRate` differs between acceleration `30 m/s²` and deceleration `40 m/s²`.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, `PlayerBlockoutMovement` file ID `2100000002`: serialized values are maximum speed `5`, acceleration `30`, deceleration `40`, and rotation speed `720`.
- Current start timing is `Δv / a = 5 / 30 = 0.1667 s`; a full reversal changes velocity by `10 m/s` and therefore takes `10 / 30 = 0.3333 s`. At fixed timestep `0.02 s`, these are approximately 8.33 and 16.67 physics ticks.
- Required algorithm: after clamping input, select `desiredVelocity` directly whenever `input.sqrMagnitude > InputEpsilon`; only the at-or-below-epsilon branch uses `Vector3.MoveTowards(currentPlanarVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime)`.
- Directly assigning a Rigidbody's planar velocity preserves physics ownership of integration, contacts, gravity, and Y velocity. It removes only the controller-authored planar acceleration ramp.
- User observation on 2026-07-22: the current sub-second windup is perceptible and unacceptable for planned precise combat. Starting and changing movement must be immediate; end-of-movement deceleration is acceptable.
- Git status confirms the controller and plan are existing untracked `PBM` files and the scene contains extensive user-owned changes. `PBM-3` must preserve all unrelated state.

### Invariants, risks, and performance

- No input buffering, animation, root motion, combat system, dash, jump, grounded solver, force-based acceleration, new component, or new serialized control is added.
- Immediate reversals intentionally create a discontinuous planar velocity change of up to `10 m/s`. This provides the requested responsiveness and may produce stronger collision impulses than the acceleration-limited version; Play Mode collision checks remain required.
- Runtime work is reduced slightly for nonzero input because the movement path skips `Vector3.MoveTowards`. Complexity remains `O(1)` with no expected managed allocation.
- No shader, rendering, weather, geometry, or GPU contract changes.

### File-by-file implementation sequence

| Step | Status | File | Change and verification |
|---|---|---|---|
| PBM-3P | Completed | `Assets/Docs/Player_Blockout_Movement_Plan.md` | Record exact timing, approved scope, algorithm, invariants, risks, and validation before implementation. |
| PBM-3A | Completed; Play Mode pending | `Assets/Game/Input/PlayerBlockoutMovement.cs` | Removed the acceleration field/clamp; nonzero input directly selects desired planar velocity and only zero input uses bounded deceleration. |
| PBM-3B | Completed | `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Removed only serialized `acceleration: 30` from component file ID `2100000002`. |
| PBM-3V | Partially completed | all approved files | Current runtime source compiled at exit `0`; static algorithm/serialization checks passed; final scope/diffs and affected files/contracts were reread. Interactive Unity validation remains pending. |

### PBM-3 validation ledger

| Check | Status | Evidence / pass condition |
|---|---|---|
| Review and plan gate | Passed | Complete controller, serialized component, current plan, input/physics contracts, timing math, Git state, and user observation reviewed before implementation. |
| Source implementation | Passed | Final `ApplyMovement` selects `desiredVelocity` directly for nonzero input; only zero input uses `Vector3.MoveTowards(..., Vector3.zero, deceleration * Time.fixedDeltaTime)`; final assignment preserves `currentVelocity.y`. |
| Serialization | Passed statically | Component file ID `2100000002` has no acceleration field; maximum speed `5`, deceleration `40`, rotation speed `720`, input GUID, and camera file ID remain unchanged. PBM-2 camera Aim disablement and mass `3` also remain present. |
| Runtime-source compilation | Passed | Unity 6000.5.0f1 bundled .NET 8 Roslyn compiled the current `Assembly-CSharp.rsp` runtime set with exit `0`; outputs are `Temp/PBM-3-Runtime.dll` (2,533,376 bytes) and `Temp/PBM-3-Runtime.ref.dll` (530,944 bytes), timestamp `2026-07-22 08:58:35 UTC`. |
| Static behavior | Passed | `PBM3StaticAudit=PASS`: start speed `5`, full reverse velocity `-5`, diagonal speed `5`, analytical release stop time `0.125 s`, and first release-tick speed `4.2 m/s`. |
| Unity Play Mode | Pending | Starting and direction changes feel immediate; release decelerates; diagonal speed, facing, collision, and camera remain correct. |
| Performance | Static audit passed; measurement pending | Nonzero movement replaces one `Vector3.MoveTowards` call with direct selection of an already-computed struct value. No new branch category, allocation, collection, query, component, or resource was added. |

### PBM-3 post-change audit

#### Actual files and intentional differences

- Modified `Assets/Docs/Player_Blockout_Movement_Plan.md` to record `PBM-3` review, plan, evidence, and audit.
- Modified `Assets/Game/Input/PlayerBlockoutMovement.cs`: removed the serialized acceleration field and its `OnValidate` clamp; changed the nonzero-input planar-velocity branch from bounded interpolation to direct target selection. The zero-input deceleration path and vertical-velocity preservation remain.
- Modified `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`: removed only `acceleration: 30` from movement component file ID `2100000002` for `PBM-3`.

#### Final consistency and compliance result

- Final controller SHA-256 is `402FC76125ED1701CDDE084B72AC25617F67E80AEFBF6FD5A03386BE0C977192`; final scene SHA-256 is `A9764584226228EA76CBA9D53C2320E70FF7662D5BABA0366F49BF6168EFC599`.
- The scene length changed from the `PBM-2` baseline `470885` bytes to `470866` bytes, exactly `19` bytes, matching removal of the LF-terminated line `  acceleration: 30\n`. This supports, but does not independently prove, the recorded scene edit scope.
- Complete final controller and relevant final movement/camera/Rigidbody scene records were reread. Input clamping, world-XZ mapping, Y preservation, cursor yaw, maximum speed, deceleration, rotation speed, input/camera references, disabled Rotation Composer, and mass `3` remain consistent with `PBM-1` through `PBM-3`.
- Unity-bundled Roslyn compilation passed at exit `0`. Static source/serialization and numerical assertions passed with `PBM3StaticAudit=PASS`.
- Final `git diff --check` retains only the five pre-existing unrelated scene whitespace warnings. Removal of the acceleration line introduced no whitespace issue.
- No source or scene file outside the three approved `PBM-3` files was changed by this patch. Existing unrelated working-tree changes remain untouched.

#### Pending behavior and concrete next action

- Unity Play Mode must verify perceived start/reversal response, release deceleration, collisions under instantaneous reversal, diagonal speed, cursor-facing independence, and fixed-angle camera behavior.
- If movement still feels delayed after this change, capture the Game view and Inspector once while reproducing it; the next investigation should distinguish physics/update delay from animation or presentation delay rather than reintroducing acceleration tuning.
