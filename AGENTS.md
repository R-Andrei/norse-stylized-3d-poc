# Repository Agent Instructions

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


## Response and validation style

- Be concise, direct, and action-oriented by default. Do not restate obvious setup steps or narrate routine actions.
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
- Up to fix validation steps, or a statement that no Unity validation is required.
- Known blocker or limitation and CONCRETE next actions.

## Documentation

- Update canonical architecture or plan documents when an accepted decision changes them.
- Use Markdown only for project documentation.
- Do not append temporary logs indefinitely.
- Remove or clearly mark superseded instructions.
- Historical documents must identify their authoritative replacement.
