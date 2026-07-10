# Repository Agent Instructions

## Branch ownership

- `main` is owned and maintained by Fufu.
- Agents must not commit, push, merge, rebase, or open pull requests against `main` unless Fufu explicitly requests it.
- Fufu decides when validated milestones from `fufu` are merged into `main`.

## Working branches

- `fufu` contains validated work only. Treat it as the stable agent baseline.
- `fufu-test` contains work currently being implemented or validated.
- All agent implementation commits and pushes go to `fufu-test`.
- Never push unvalidated work directly to `fufu`.

## Starting and continuing work

1. Fetch the remote state.
2. Use `fufu` as the source of validated work before beginning the next task.
3. Perform implementation work on `fufu-test`.
4. Before pushing, confirm that the commit contains only the intended task changes and does not modify unrelated user work.
5. Push the completed implementation to `fufu-test`.

## Validation promotion

- Fufu validates work from `fufu-test` locally.
- When Fufu explicitly confirms that a change is validated, successful, or error-free, merge the validated `fufu-test` state into `fufu` before beginning further implementation.
- Push the resulting validated state to `fufu`.
- Do not promote failed, incomplete, or unconfirmed work.
- After promotion, continue new implementation on `fufu-test`, using `fufu` as the validated baseline.

## Safety rules

- Preserve unrelated changes already present in either branch.
- Do not force-push or rewrite shared branch history unless Fufu explicitly requests it.
- If branch histories conflict or it is unclear which commits were validated, stop and ask Fufu rather than guessing.
- Report the branch name and commit SHA after every push.
