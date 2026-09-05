# Repository Agent Instructions

## Start of every turn

- Re-read the complete root `AGENTS.md` and every applicable nested `AGENTS.md` before repository work.
- Verify every referenced project path, asset, scene object, component, property, menu, issue, branch, or pull request against the current repository/project before presenting it as fact.
- Stop if required instructions or evidence cannot be read completely.
- Classify the turn as read-only diagnosis/research or implementation. Read-only diagnosis may inspect GitHub without creating an issue or branch. Repository mutation requires the issue/thread workflow below.
- For implementation on an existing issue, read the issue and all current claim/status comments before any write and verify that the current Thread-ID, ACTIVE claim, and branch still agree.

## Scope and authorization

- Diagnosis authorizes read-only inspection only. Implement only with user approval.
- Change only approved files and behavior. Preserve unrelated work.
- Obtain approval before adding or renaming folders, assets, components, dependencies, layers, tags, or materially different defaults.
- A user approval to proceed with a scoped implementation may authorize the normal issue claim, branch creation, edits, commits, pushes, pull request creation, CI-fix iterations, and integration when that approval clearly covers completing/integrating the task. Do not infer authorization for a material scope expansion or destructive history rewrite.
- Never force-push `fufu` or rewrite published history without explicit authorization.

## Issue and thread identity

- Every implementation task must have an open GitHub issue before repository files are mutated. The issue is the canonical task, scope, acceptance, and ownership record.
- GitHub assignees are optional metadata and are not proof of thread ownership.
- Every active agent/chat thread uses one immutable Thread-ID for its lifetime. Generate it once in the form `t-YYYYMMDD-HHMM-xxxxxx`, where the final six characters are lowercase letters/digits, and reuse it whenever the same conversation resumes.
- A thread may have at most one active implementation branch at a time. After one issue is completed, the same conversation may claim a different issue and create a new issue-specific branch while keeping the same Thread-ID.
- Before creating a branch, post a structured claim comment on the task issue using this exact field set:

```text
<!-- agent-thread-claim:v1 -->
Thread-ID: t-YYYYMMDD-HHMM-xxxxxx
Branch: issue-<issue>/t-YYYYMMDD-HHMM-xxxxxx-<short-slug>
Base: fufu@<full-or-abbreviated-commit-sha>
Status: ACTIVE
Scope: <short description of the approved work>
```

- Valid claim statuses are `ACTIVE`, `INTEGRATED`, and `COMPLETED`. Post a later structured claim comment with the same Thread-ID to update status; do not erase historical claim comments.
- Only one effective ACTIVE thread claim may exist for an issue. If the user explicitly authorizes a takeover/migration, the new ACTIVE claim must add `Supersedes: <old-thread-id>` (comma-separated for more than one). Superseded claims never become active again automatically.
- Do not supersede a claim that owns an open pull request unless that PR is first closed/merged or the user explicitly authorizes the takeover with awareness of the open PR.
- If the issue's effective ACTIVE claim, Thread-ID, or recorded Branch does not match the current thread/repository branch, stop all writes. Do not guess which branch is yours and do not silently repair another thread's claim.

## Plan and implementation

1. Review the complete change surface: issue objective, acceptance criteria, implementation, direct dependencies, contracts, current architecture, validation requirements, and current `fufu` state.
2. Formulate a plan before editing. Include objective, acceptance criteria, approved scope, evidence, invariants, sequence, risks, and validation. The GitHub issue is the canonical persisted task plan unless the user requests another document.
3. Confirm or create the structured ACTIVE thread claim, then create the issue-specific thread branch from the latest `origin/fufu`.
4. Implement only the approved plan. Stop and update the issue/plan before any material deviation or scope expansion.
5. Audit the final diff against the issue and plan. Re-read changed files and direct dependencies, confirm preserved behavior, and run all relevant automated validation.

## Git delivery

- `fufu` is the integration and user-testing branch.
- Implementation branches must use `issue-<issue>/t-YYYYMMDD-HHMM-xxxxxx-<short-slug>` and must exactly match the Branch recorded in the effective ACTIVE issue claim.
- Create each implementation branch from the latest trusted `origin/fufu`. Record that base commit in the claim before branch creation.
- Before editing on a resumed thread, fetch/re-read the current remote state and reconcile the thread branch with `origin/fufu`. Stop if existing work prevents a safe update.
- Use Git commits and pull requests for delivery. Never bypass the pull request with a local merge into `fufu`.
- Every implementation PR must target `fufu` and contain these exact metadata fields matching the issue claim and PR head branch:

```text
Issue: #<issue-number>
Thread-ID: <thread-id>
Branch: <exact-branch-name>
```

- Before final merge, reconcile the branch with the latest `origin/fufu` and rerun required checks. A stale branch is not merge-ready.
- Treat `Repository Policy` and `Unity Compile and Edit Mode Tests` as required merge gates for implementation PRs even if repository branch-protection settings are temporarily misconfigured. A red, cancelled, missing, or materially skipped required check is a blocker.
- Merge only after the required checks pass and all issue-specific automated gates are green. Manual Unity validation is required before issue closure only when acceptance depends on evidence that cannot be reliably automated.
- After merge, verify the published `fufu` commit. Post a structured status comment for the thread: `INTEGRATED` if manual acceptance remains, or `COMPLETED` when all acceptance criteria are satisfied. Close the issue only when acceptance is complete.
- If manual validation rejects an integrated change, reopen/reactivate work on the same issue with a valid ACTIVE claim, reconcile the thread branch from current `fufu`, and deliver a new PR rather than bypassing the workflow.
- Report the issue, Thread-ID, branch, pull request, CI result, integration commit, conflicts, and blockers.

## Issue worklog convention

- Use the task issue thread as the durable, concise engineering history for non-trivial implementation work. A future agent should be able to recover the important reasoning without replaying the full chat or reconstructing every intermediate diff.
- Post a short worklog comment when there is durable information worth preserving: a meaningful implementation milestone, an accepted or rejected approach whose reason matters, a diagnostic/root-cause conclusion, a material plan or scope change, a blocker, or a validation/acceptance result.
- Record failed approaches for the reusable reason they failed, not as exhaustive command-by-command or edit-by-edit transcripts. Routine progress chatter, repeated status, and implementation narration that is already obvious from the diff should not be logged.
- Point worklog entries to the relevant commit SHA(s), pull request, CI run, report, screenshot, or other evidence when available. Keep implementation detail in commits/diffs/tests; use the issue comment to explain what changed in understanding and why the referenced evidence matters.
- Keep each entry compact and self-contained. Prefer a few high-information bullets or a short paragraph over long chronological logs.
- When a commit changes the engineering conclusion, add or update the issue history with that conclusion rather than merely posting that a commit exists.

## Automated validation policy

- Prefer deterministic automated validation over manual Unity testing whenever the contract can be tested reliably and at proportionate cost.
- Compilation, deterministic geometry/math contracts, topology, serialization/data invariants, editor tooling contracts, and repeatability belong in automated Edit Mode tests or inexpensive repository checks when practical.
- Runtime state transitions that can run headlessly should use automated Play Mode tests when such tests provide meaningful coverage. Do not add ceremonial Play Mode jobs that test nothing useful.
- Appearance, composition, artistic quality, input feel, camera feel, or other evidence that cannot yet be represented by a trustworthy deterministic test remains manual Unity validation.
- When implementing a durable deterministic behavior contract, prefer adding or extending an automated regression test rather than requiring the user to repeat the same manual check indefinitely.
- Run the smallest relevant automated suites that answer the current uncertainty. Do not run broad test matrices merely for ceremony.
- CI artifacts/logs are evidence. Never report pending, skipped, or unavailable validation as passed.

## Repository policy invariants

- The root `AGENTS.md` and `Assets/AGENTS.md` are intentional byte-identical mirrors. Keep them synchronized; repository CI enforces this.
- Keep Unity pinned to `6000.5.0f1` unless a separately approved engine-upgrade issue changes the repository contract.
- Do not track generated/local Unity directories such as `Library`, `Temp`, `Logs`, `obj`, `Build`, `Builds`, or `UserSettings`.
- Repository-policy automation should remain fast and run before expensive Unity validation. Do not move slow gameplay/visual suites into the policy preflight.
- If GitHub branch/ruleset configuration does not mechanically require the documented checks, treat that configuration drift as a repository-governance defect; agents must still obey the documented merge gates.

## Evidence and communication

- State outcomes first. Be concise, direct, and concrete.
- Support repository claims with current file content, diffs, commits, issue/PR state, Actions results, logs, tests, or screenshots.
- Label unverified conclusions and state how to verify them.
- Keep handoffs focused on current state, active issue/claim/branch identity, constraints, validation, blockers, and next actions. Do not add migration logs or archived instructions to this file.

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

- Run all available relevant compilation, tests, static checks, Unity validation, and performance checks required by the issue. Prefer automation as defined above.
- For required manual user validation, provide 1-6 ordered steps limited to the current change. Include exact inputs, expected results, and the evidence to return on failure.
- An implementation response must state the outcome, issue/Thread-ID/branch/PR, changed files, material changes, plan/audit result, automated validation result, required manual validation if any, and blockers with concrete next actions.
- Add `Next work items` only for firmly future work; never use it for work still required to complete the current issue.

## Documentation

- Keep this file limited to current repository-wide invariants and workflow.
- Keep subsystem design, tuning, budgets, and active implementation detail in their canonical documents and task issues.
- Write project documentation in Markdown and update canonical architecture when accepted decisions change it.
- Remove obsolete instructions instead of archiving them in active policy files.
