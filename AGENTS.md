# Repository Agent Instructions

## Repository authority

- Treat the checked-out repository and its canonical documents as authoritative.
- Inspect relevant code and documentation before diagnosing, proposing, or implementing.
- Do not rely on conversation summaries when repository evidence differs.
- Read the subsystem documents identified by the repository README or documentation index before modifying that subsystem.
- If no documentation index exists or the authoritative subsystem documents are ambiguous, identify the relevant documents from the repository and ask before making an architectural change.

## Scope and authorization

- Diagnosis requests authorize read-only inspection, not implementation.
- Implement only after explicit user approval.
- Modify only files placed within the approved patch scope.
- Never introduce new layers, tags, components, renamed assets, folders, or architectural dependencies without approval.
- Never silently choose materially different defaults.
- Preserve unrelated user and repository changes.

## Evidence requirements

- Support technical findings with exact file paths, code excerpts, logs, or measured output.
- State confidence levels for diagnoses.
- Distinguish proven facts, inferences, and unverified hypotheses.
- Address the user's primary question before secondary diagnostics.
- Do not propose likely fixes before reading the actual implementation.

## Unity constraints

- The project uses Unity 6000.5.0f1 and URP.
- Use `GetEntityId()` rather than obsolete instance-ID APIs where applicable.
- Use `FindObjectsByType` without sorting unless ordering is explicitly required.
- Avoid `DestroyImmediate` from `OnValidate`.
- Preserve Force Text serialization and Visible Meta Files.
- Never commit `Library`, `Temp`, `Logs`, `obj`, builds, or `UserSettings`.
- New files inside `Assets` require matching `.meta` files.
- Do not raw-edit scenes, prefabs, materials, or other serialized Unity assets unless explicitly approved and necessary.
- Do not change layers or tags without approval.
- Generated geometry must respect the accepted vertex budgets recorded in the canonical subsystem documents.
- Prefer low runtime cost. Expensive deterministic build-time or dirty-time work is acceptable when justified.
- Never add per-frame full-field rebuilds without explicit performance justification.

## Project architecture invariants

Keep this file at the invariant level. Detailed formulas, tuning values, current patch identifiers, validation counters, and active implementation steps belong in the canonical subsystem documents.

- Stage ownership must remain explicit.
- River Foam persistent material may be moved only by the persistent-state transport stage.
- Visual shape evaluation must not mutate persistent material or Remaining Life.
- Remaining Life may be changed only by the accepted lifecycle controls and support/negative fields.
- Edge-wear final rendering must not silently fall back to obsolete atlas behavior.
- Ground, river, and mass changes must remain non-destructive to unrelated mesh systems.
- Changes to shared shaders or includes require an explicit cross-subsystem impact audit.

## Branch contract

| Branch | Meaning | Authority |
| --- | --- | --- |
| `main` | Milestone and release history | User-owned |
| `fufu` | Latest locally validated development state | Agents may promote into it only after explicit user validation |
| `fufu-test` | Current unvalidated implementation candidate | The only branch agents modify directly |

### `main`

- Agents must never commit, push, merge, rebase, or open pull requests against `main`.
- The user alone decides when `fufu` represents a milestone and merges `fufu` into `main`.
- `main` should normally remain downstream of `fufu`, not become a separate development line.
- If the user applies an independent hotfix to `main`, the user must bring it back into `fufu` before agents synchronize `fufu-test`.

### `fufu`

- `fufu` is the canonical validated development baseline.
- Agents inspect it before beginning a new implementation candidate.
- Nothing enters `fufu` without explicit user validation or acceptance.
- Agents do not commit directly to `fufu`.
- After explicit validation, agents may merge the reviewed `fufu-test` candidate into `fufu` through the promotion procedure below.

### `fufu-test`

- `fufu-test` is the only branch agents modify directly.
- The user remains locally checked out on `fufu-test` and pulls it for Unity validation.
- Agents commit and push approved implementations to `fufu-test`.
- Failed attempts and corrections remain isolated from `fufu`.
- Corrections continue on `fufu-test` until the candidate is accepted or explicitly abandoned.

## Serialized implementation rule

- Multiple threads may investigate, inspect, diagnose, and plan concurrently.
- Only one approved implementation candidate may occupy `fufu-test` at a time.
- Other workstreams remain read-only while that candidate is being implemented or validated.
- Do not mix unrelated workstreams in one commit or validation candidate.

Before beginning a new implementation candidate, confirm all three conditions:

1. `fufu-test` contains no unvalidated work from a previous candidate.
2. `fufu-test` is synchronized with `fufu`.
3. The expected file scope is explicitly confirmed.

Before editing, inspect the current `fufu` and `fufu-test` state and recent changes to every file in scope. If another workstream changed an overlapping file, stop and reconcile against the current development branches before proceeding.

During an active candidate, continue from the current `fufu-test` head. Do not reset or repeatedly synchronize it from `fufu` while unvalidated commits are present.

## Validation promotion

When the user explicitly reports that the current candidate passed:

1. Confirm the exact `fufu-test` head commit validated by the user.
2. Compare `fufu-test` against `fufu`.
3. Confirm that only the validated candidate is present.
4. Open a pull request from `fufu-test` into `fufu`.
5. Review the complete pull-request diff and report its scope.
6. Merge it normally only after the user's validation instruction; do not squash merge it.
7. Synchronize `fufu-test` to the resulting `fufu` state.
8. Confirm that both development branches are aligned before beginning the next implementation.

- Do not enable auto-merge.
- Do not merge or promote an incomplete, failed, unconfirmed, or mixed candidate.
- Report the branch name and commit SHA after every push or promotion.

## Failed or abandoned validation

- If validation fails, leave `fufu` untouched.
- Continue corrections on `fufu-test`, then ask the user to pull and validate again.
- Keep other implementation workstreams read-only until the candidate passes or is abandoned.
- Restoring `fufu-test` to `fufu` after abandoning a candidate may rewrite or discard published work. Do this only with explicit user approval.
- Never rewrite published history or force-push without explicit user instruction.

## Patch delivery

Every implementation response must include:

- Outcome.
- Changed files.
- A short explanation of the implementation.
- Evidence that the approved scope was respected.
- Concise numbered Unity validation steps.
- Known limitations.
- Next work items.

If no Unity validation is required, state that explicitly.

## Documentation

- Update canonical architecture or plan documents when an accepted decision changes them.
- Use Markdown only for project documentation.
- Do not append temporary logs indefinitely.
- Remove or clearly mark superseded instructions.
- Historical documents must identify their authoritative replacement.
