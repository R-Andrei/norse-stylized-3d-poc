# Repository Agent Instructions

## Start of every turn

- Re-read the complete root `AGENTS.md` and every applicable nested `AGENTS.md` before repository work.
- Verify every referenced project path, asset, scene object, component, property, or menu against the current project before presenting it as fact.
- Stop if required instructions or evidence cannot be read completely.

## Scope

- Diagnosis authorizes read-only inspection only. Implement only with user approval.
- Change only approved files and behavior. Preserve unrelated work.
- Obtain approval before adding or renaming folders, assets, components, dependencies, layers, tags, or materially different defaults.

## Plan and implementation

1. Review the complete change surface: implementation, direct dependencies, contracts, current architecture, validation requirements, and working-tree state.
2. Inspect relevant Git history for files and symbols being changed when prior intent, regressions, compatibility, or rejected approaches may affect the solution. Use targeted logs, blame, and diffs; do not review unrelated history or let historical behavior override current requirements and source.
3. Formulate a plan before editing. Include objective, acceptance criteria, scope, evidence, invariants, sequence, risks, and validation. The plan may remain in working context; a persisted Markdown plan is required only when the user or task requests one.
4. Implement only the approved plan. Stop and update the plan before any material deviation or scope expansion.
5. Audit the final diff against the plan. Re-read changed files and direct dependencies, confirm preserved behavior, and run all available validation.

## Git delivery

- `fufu` is the integration and user-testing branch.
- Use one dedicated branch per active chat or agent thread. Start it from the latest `origin/fufu` and reuse it when that thread resumes.
- Fetch `origin` when starting or resuming a thread, before the first edit, at major work boundaries, before final validation, and immediately before pushing or merging.
- Reconcile the thread branch with the latest `origin/fufu` before editing and again before final validation. Do not interrupt unsafe or incomplete working-tree changes merely to synchronize; first reach a safe checkpoint.
- If `origin/fufu` advances after validation, reconcile again and rerun every affected check before delivery.
- Never use a bare `git pull` as a substitute for identifying which remote branch is being incorporated.
- Use Git commits and pull requests for delivery from the thread branch.
- After final validation, push the thread branch and open or update a pull request targeting `fufu`.
- Merge only after required GitHub Actions checks pass. Never bypass the pull request with a local merge.
- After merge, synchronize local `fufu` with `origin/fufu` and verify the published commit.
- Staging, committing, pushing, pull-request creation, and merging require user authorization. Never force-push `fufu` or rewrite published history without explicit authorization.
- Report the thread branch, pull request, CI result, integration commit, push result, conflicts, and blockers.

## Evidence and communication

- State outcomes first. Be concise, direct, and concrete.
- Support repository claims with current file content, diffs, commits, command output, tests, logs, or screenshots.
- Label unverified conclusions and state how to verify them. Never report pending validation as passed.
- Keep handoffs focused on current state, active constraints, validation, blockers, and next actions. Do not add migration logs or archived instructions to this file.

## Unity constraints

- Use Unity 6000.5.0f1 and URP.
- Use `GetEntityId()` instead of obsolete instance-ID APIs where applicable.
- Use unsorted `FindObjectsByType` unless ordering is required.
- Never call `DestroyImmediate` from `OnValidate`.
- Preserve Force Text serialization and Visible Meta Files.
- Raw-edit serialized Unity assets only with explicit approval.
- Never change layers or tags without approval.
- Respect canonical geometry budgets and architecture contracts.
- Minimize runtime cost. Do not add per-frame full-field rebuilds without explicit justification.
- Audit every shared shader or include change across affected subsystems.

## Validation and delivery response

- Run all available compilation, tests, static checks, Unity validation, and relevant performance checks.
- For user validation, provide 1-6 ordered steps limited to the current change. Include exact inputs, expected results, and the evidence to return on failure.
- An implementation response must state the outcome, changed files, material changes, plan/audit result, validation result, and blockers with concrete next actions.
- Add `Next work items` only for firmly future work; never use it for work still required to complete the current change.

## Documentation

- Keep this file limited to current repository-wide invariants.
- Keep subsystem design, tuning, budgets, and active implementation detail in their canonical documents.
- Write project documentation in Markdown and update canonical architecture when accepted decisions change it.
- Remove obsolete instructions instead of archiving them in active policy files.
