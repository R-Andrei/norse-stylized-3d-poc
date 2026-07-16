# Repository Agent Instructions

## Scope and authorization

- Diagnosis requests authorize read-only inspection, not implementation.
- Implement only after explicit user approval.
- Modify only files placed within the approved patch scope.
- Never introduce new layers, tags, components, renamed assets, folders, or architectural dependencies without approval.
- Never silently choose materially different defaults.
- Preserve unrelated user and repository changes.

## Mandatory implementation workflow — non-bypassable gates

These gates are mandatory and project-critical for every implementation change, regardless of size, urgency, or apparent simplicity. An agent has no discretion to skip them, perform them only mentally, or complete them retroactively. If any gate cannot be completed, stop and report the blocker before proceeding. Work performed in breach of a gate is unverified and incomplete.

1. **Complete a read-only review before editing.** Read the complete current implementation expected to change, its direct callers, consumers, producers, shared contracts, and related modules. Read the canonical subsystem architecture, active plans, handoffs, and validation requirements. Inspect relevant Git status, diffs, and history; distinguish pre-existing working-tree changes; and compare the working implementation with `HEAD` and relevant accepted or superseded versions. Record the exact files, documents, commits, findings, and constraints in the canonical plan. Until that evidence is recorded, modifying code, shaders, serialized assets, generated inputs, or running any formatter, generator, autofix, or other modifying tool is prohibited.
2. **Create a concrete, persistent plan before implementation.** Updating the canonical Markdown plan is the first permitted change after review and must occur before any implementation edit. The plan must define the objective and acceptance criteria, approved files, reviewed evidence, invariants and non-goals, file-by-file implementation sequence, affected related modules, risks, and validation/compliance checks. Every item must have a current status. If no canonical plan exists or the required document is outside the approved scope, stop and request approval for the specific document. Update the plan before every material deviation, design change, or scope expansion, and obtain any required approval before continuing.
3. **Implement strictly from the recorded plan.** Every edit must be traceable to an active plan item and remain inside the approved scope. Do not make speculative edits or introduce unrelated cleanup, refactors, renames, dependencies, or architecture changes. If new evidence invalidates any step or assumption, stop implementation, update the evidence and plan, and resolve any approval requirement before resuming. Writing code does not complete a plan item; its required verification must also pass.
4. **Complete a post-implementation consistency and compliance audit.** Before reporting completion, compare the final diff with the approved scope and every plan item. Reread the complete final versions of all modified files and the affected current callers, consumers, producers, shared contracts, and related modules. Compare final behavior with the captured pre-edit state, `HEAD`, and relevant accepted or superseded historical versions; record every intentional difference and confirm all behavior intended to remain unchanged. Verify the result against canonical docs, active plans, handoffs, repository rules, budgets, performance constraints, and applicable Unity requirements. Run all available compilation, tests, static checks, and Unity validation, then record the evidence, results, deviations, and pending checks in the plan.

No patch may be described as complete, compliant, ready, or successful until all four gates are satisfied. Missing comparisons, undocumented deviations, unresolved inconsistencies, failed checks, or unverified plan items are blockers. Unavailable validation must be marked pending with a concrete next action and must never be represented as passed.

## Technical communication and evidence — non-bypassable

Humor, jokes, analogies, metaphors, and descriptive prose are permitted only outside technical content. Every solution, conclusion, diagnosis, technical opinion, direction, recommendation, review finding, process description, or claim about behavior, cause, correctness, safety, performance, or compliance must use strict, direct, concise language. State the outcome first. Do not dilute technical content with storytelling, rhetorical filler, euphemism, or decorative phrasing.

Every technical claim must have concrete evidence attached at the point where the claim is made. Acceptable evidence is:

- repository evidence: exact file path plus relevant symbol, line, excerpt, diff, or commit;
- observed behavior: complete relevant logs, reproducible output, test results, screenshots, or measurements;
- mathematical or algorithmic evidence: the applicable equation, derivation, proof, or worked calculation;
- external evidence: a direct link to the primary or most authoritative available source, such as official documentation, a specification, paper, study, or original publication;
- historical evidence: the relevant commit, diff, release, or accepted/superseded implementation record.

A citation must directly support the claim beside it. Never invent a source, cite from memory as if verified, or cite a source that only discusses the general topic. Prefer original and primary sources; use secondary sources only when no suitable primary source is available and identify them as secondary. One source may support multiple adjacent claims only when the mapping is unambiguous.

If proof is incomplete or unavailable, the statement must not be presented as fact. Label it explicitly as **Inference**, **Opinion**, **Hypothesis**, or **Unverified**, add a **High**, **Medium**, or **Low** confidence level, state the concrete evidence used to form it, and identify what would verify or falsify it. Recommendations must separate proven premises from judgment. Unsupported claims are prohibited.

Address the user's primary question before secondary diagnostics.

## Validation response style

- Validation instructions must contain **at most** six numbered items and only include steps that materially test the patch. Omit instructions such as closing Unity, copying files, reopening the project, or selecting the already-known test object unless unusually necessary.
- When evidence is needed, ask for the complete relevant log or screenshot once. Do not enumerate every telemetry field the user must copy unless a specific field is uniquely required.

## Unity constraints

- The project uses Unity 6000.5.0f1 and URP.
- Use `GetEntityId()` rather than obsolete instance-ID APIs where applicable.
- Use `FindObjectsByType` without sorting unless ordering is explicitly required.
- Avoid `DestroyImmediate` from `OnValidate`.
- Preserve Force Text serialization and Visible Meta Files.
- Do not raw-edit scenes, prefabs, materials, or other serialized Unity assets unless explicitly approved and necessary.
- Do not change layers or tags without approval.
- Generated geometry must respect the accepted vertex budgets recorded in the canonical subsystem documents.
- Prefer low runtime cost. Expensive deterministic build-time or dirty-time work is acceptable when justified.
- Never add per-frame full-field rebuilds without explicit performance justification.

## Project architecture invariants

Keep this file at the invariant level. Detailed formulas, tuning values, current patch identifiers, validation counters, and active implementation steps belong in the canonical subsystem documents.
- Changes to shared shaders or includes require an explicit cross-subsystem impact audit.

## Patch delivery

Every implementation response must include, briefly:
- Outcome and changed files.
- What materially changed.
- The canonical plan and the post-change consistency/compliance result.
- Up to six validation steps, or a statement that no Unity validation is required.
- Known blocker or limitation and CONCRETE next actions.

## Documentation

- Update canonical architecture documents when an accepted decision changes them.
- Use Markdown only for project documentation.
- Do not append temporary logs indefinitely.
- Remove or clearly mark superseded instructions.
- Historical documents must identify their authoritative replacement.
