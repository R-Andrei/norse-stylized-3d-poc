# Continuation Handoff

Use a handoff only when work must continue in another chat or agent thread. Keep it concise and describe the current repository state only.

## Required content

- Objective and acceptance criteria.
- Current branch, commit, pull request, and CI state.
- Approved scope and actual changed files.
- Current implementation behavior and important invariants.
- Validation completed, exact failures, and validation still pending.
- Active blockers, unresolved decisions, and required approvals.
- Next action in executable order.

## Source of truth

- The current Git working tree and repository instructions are authoritative.
- Start or resume the receiving thread from the latest `origin/fufu` on its own branch.
- Preserve unrelated local changes.
- Use Git commits and pull requests for delivery.
- If an external file is supplied as task input, compare it with the current branch and obtain direction before allowing it to replace repository state.

## Writing rules

- State current facts, not a chronological history.
- Include only information needed to continue safely.
- Cite current repository evidence for technical claims.
- Mark unknown or pending validation explicitly.
- Do not duplicate repository-wide rules; reference the applicable `AGENTS.md`.
- Do not include secrets, transient logs, obsolete instructions, or speculative future work.
