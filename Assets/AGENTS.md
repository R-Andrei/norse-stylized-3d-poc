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
