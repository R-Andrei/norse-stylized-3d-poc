# Repository Agent Instructions

## Authority and instruction reload

Every instruction is mandatory. For every user message, first re-read from disk the complete root `AGENTS.md` and every applicable nested `AGENTS.md`, even when already in context. Before this reload, perform no substantive repository work except locating and reading those files. Never substitute memory, prior reads, copies, or summaries. If a file cannot be read completely or followed exactly, stop and report the blocker; subsequent work is invalid until compliance is restored.

## Exact project locations — mandatory fail-closed gate

Before writing any project location anywhere—including analysis, plans, validation, `Next work items`, documentation, or responses—perform a fresh, distinct check of that exact location against the current project. This covers every file, asset, scene, Hierarchy, component, Inspector, control, menu, and navigation chain. Verify the full route through every intermediate segment and copy its exact spelling and characters from authoritative current project evidence. Memory, documentation, naming patterns, inference, partial checks, and prior verification do not count. Each occurrence requires its own check: twenty location references require twenty distinct checks; bulk or reused searches are invalid. If verification fails, omit the location and every instruction dependent on it.

Any invented, stale, partial, unchecked, or inaccurate location is a project failure. Stop immediately, disclose the breach, invalidate all work since the last verified checkpoint, reload the instructions, and reverify before continuing. No urgency, familiarity, or apparent obviousness permits bypassing this gate.

## Scope and authorization

- Treat diagnosis as authorization for read-only inspection, not implementation. Obtain explicit user approval before implementing.
- Modify only approved files. Obtain approval before adding layers, tags, components, renamed assets, folders, architectural dependencies, or materially different defaults; disclose every proposed default change.
- Preserve unrelated user and repository changes.

## Git workflow and delivery

- Treat `fufu` as the integration and user-testing branch. The user should only need to pull `fufu` to test the latest accepted agent work in Unity.
- Use one dedicated Git branch per active chat or agent thread. At the start of a new thread, begin from the latest `origin/fufu`, create a uniquely named thread branch, and keep all work for that thread on that branch. Reuse that branch when the same thread resumes; never share one working branch across concurrent threads.
- Before editing, fetch the trusted remote and reconcile the thread branch with the latest `origin/fufu`. Preserve local or user-owned changes and stop for direction if a safe update is not possible.
- Commit repository changes on the thread branch. Do not exchange patch files as the normal delivery mechanism; use a patch only when Git is unavailable or the user explicitly requests one.
- Before delivery, run the required local validation, update the thread branch against the latest `origin/fufu`, push the thread branch, and open or update its pull request targeting `fufu`. Merge only after the required GitHub Actions checks pass. After the merge, synchronize local `fufu` with `origin/fufu` and verify the published integration commit so the user can pull and test it. Do not bypass the pull request with an untested local merge.
- Publishing remains subject to explicit authorization: staging, committing, pushing, pull-request creation, and merging must be covered by the user's request or separately approved. Report the thread branch, pull request, integration commit, push result, and any conflict or CI blocker. Do not force-push `fufu` or rewrite published history unless the user explicitly authorizes that exact operation.
- Changes intended for `fufu` must pass the repository's required GitHub Actions checks before merge. If branch protection is unavailable or not yet configured, wait for the checks and verify their successful result manually before merging; report that enforcement limitation and do not claim CI is enforced.

## Implementation gates

Apply all four gates to every implementation change. Never skip, perform mentally, or complete one retroactively. If a gate cannot be completed, stop and report the blocker; work performed in breach is unverified and incomplete.

1. **Review before editing.** Read the review surface: the complete implementation to change and its direct callers, consumers, producers, shared contracts, and related modules. Read the latest canonical architecture, active plans, handoffs, and validation requirements. Repository evidence overrides remembered context. Capture the reviewed files, documents, commits, findings, and constraints in the working plan before modifying code, shaders, serialized assets, generated inputs, or running any formatter, generator, autofix, or other modifying tool.
2. **Plan before implementation.** Formulate a plan after review and before the first implementation change. The plan must cover the objective, acceptance criteria, approved files, reviewed evidence, invariants, non-goals, file-by-file sequence, affected modules, risks, validation/compliance checks, and each item's status. The plan may live in the agent's working context, task tracker, or a repository document; it does not have to be persisted or use Markdown unless the user or task explicitly requires a durable plan. Update the plan and obtain required approval before any material deviation, design change, or scope expansion.
3. **Implement the plan only.** Trace every edit to an active plan item within scope. Never add speculative changes, unrelated cleanup, refactors, renames, dependencies, or architecture changes. If evidence invalidates a step or assumption, stop, record it, update the plan, and resolve approval before resuming. Complete an item only after its verification passes.
4. **Audit after implementation.** Compare the final diff with the approved scope and plan. Re-read complete final versions of the Gate 1 review surface. Compare behavior with the pre-edit state, `HEAD`, and relevant accepted or superseded versions. Record every intentional difference and confirm all behavior intended to remain unchanged. Verify canonical documents, durable plans when present, handoffs, repository rules, budgets, performance constraints, and Unity requirements. Run all available compilation, tests, static checks, and Unity validation; capture all evidence, results, deviations, and pending checks in the working plan or final handoff.

Do not describe a patch as complete, compliant, ready, or successful until all gates pass. Treat missing comparisons, undocumented deviations, unresolved inconsistencies, failed checks, and unverified items as blockers. Mark unavailable validation pending with a concrete next action; never represent it as passed.

## Technical communication and evidence

State outcomes first. Use strict, direct, concise technical language without storytelling, rhetorical filler, euphemism, or decoration. Keep humor, jokes, analogies, metaphors, and descriptive prose outside technical content.

Attach evidence beside every technical claim:

- **Repository:** exact path plus the relevant symbol, line, excerpt, diff, or commit.
- **Observed:** complete relevant logs, reproducible output, test results, screenshots, or measurements.
- **Mathematical or algorithmic:** the applicable equation, derivation, proof, or worked calculation.
- **External:** a direct link to the primary or most authoritative available source, such as official documentation, a specification, paper, study, or original publication.
- **Historical:** the relevant commit, diff, release, or accepted or superseded implementation record.

Citations must support their claims directly. Never invent sources, present memory as evidence, or cite only general background. Use original and primary sources; identify and use secondary sources only when no suitable primary source exists. One source may support adjacent claims only when the mapping is unambiguous.

Never state incomplete or unavailable proof as fact. Label it **Inference**, **Opinion**, **Hypothesis**, or **Unverified**; assign **High**, **Medium**, or **Low** confidence; cite evidence; and state what would verify or falsify it. Separate proven premises from recommendation judgment. Make no unsupported claim. Answer the primary question before secondary diagnostics.

## Validation responses

- Provide 1-6 concise, numbered, fully actionable steps that only test the current patch. Include every required action, value, input, exact full menu, Hierarchy, Inspector component/property, asset, or file path, expected result, and failure-evidence request; require no reader inference.
- Order applicable phases as Inspector action/path, Play Mode visual/runtime checks, then submission of the complete validation report.
- Exclude obvious setup such as closing Unity, copying files, reopening the project, or selecting a known test object unless necessary. Request the complete relevant log or screenshot once, request a specific diagnostic section only when uniquely required, and never request unrelated telemetry.

## Unity and architecture constraints

- Use Unity 6000.5.0f1 and URP.
- Use `GetEntityId()` instead of obsolete instance-ID APIs where applicable.
- Use unsorted `FindObjectsByType` unless behavior explicitly requires ordering.
- Never call `DestroyImmediate` from `OnValidate`.
- Preserve Force Text serialization and Visible Meta Files.
- Raw-edit scenes, prefabs, materials, or other serialized Unity assets only when the user explicitly approves the necessary edit.
- Never change layers or tags without approval.
- Keep generated geometry within the accepted vertex budgets in canonical subsystem documents.
- Minimize runtime cost. Permit expensive deterministic build-time or dirty-time work only when justified.
- Never add a per-frame full-field rebuild without explicit performance justification.
- Perform an explicit cross-subsystem impact audit for every shared shader or include change.

## Git delivery and future work

Implementation responses must briefly:

- State outcome and changed files.
- Describe material changes.
- Report the formulated plan and consistency/compliance audit result.
- Provide 1-6 concise, fully actionable validation steps in the required order, or state that no Unity validation is required.
- State each blocker or limitation and its concrete next action.

End with `Next work items` only for firmly future work. List unresolved work beginning after current completion. Exclude completed work and implementation, documentation freeze, validation, evidence collection, compliance audit, or handoff needed to finish the current patch.

## Documentation

- Keep this file at invariant level. Put formulas, tuning values, current patch identifiers, validation counters, and active implementation steps in canonical subsystem documents.
- Update canonical architecture documents when accepted decisions change them; record each material design or architecture decision and its rationale.
- Write project documentation in Markdown. Do not append temporary logs indefinitely.
- Remove or clearly mark superseded instructions. Historical documents must identify their authoritative replacement.
