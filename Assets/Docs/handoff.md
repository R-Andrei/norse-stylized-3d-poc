# Produce an Exhaustive Continuation Handoff

Write a self-contained handoff document that will allow a new model in a new chat, with no access to the current conversation, to continue the work correctly and efficiently.

This is **not** a request for a normal chat summary, a concise status update, release notes, or a short list of next steps. Produce a **substantially larger, deeper, and more explicit document than a conventional handoff**. The document must preserve the reasoning, evidence, constraints, implementation state, and exact continuation procedure that would otherwise be lost with this chat.

The receiving model must not need to reconstruct the project history from scattered files, infer why a change exists, guess which instructions remain active, or repeat investigations already completed. Eliminate every **avoidable** follow-up question. If a question cannot be resolved from available evidence, record it as an explicit unknown with its impact and a concrete verification method. Never hide uncertainty or invent an answer merely to make the handoff appear complete.

## 1. Non-negotiable depth and size

The output must be long enough to transfer operational understanding, not merely topical awareness.

- For any non-trivial implementation, investigation, review, or multi-file task, treat **3,000 words as a floor, not a target**. A complex or long-running task will commonly require **5,000–8,000 words or more**.
- Exceed those ranges whenever necessary to document the evidence, per-file state, decisions, validation, and continuation steps without compression.
- Do not shorten the document because the subject seems familiar, because source files are available, or because some details appear “obvious.” The next model does not share the current chat's implicit context.
- Do not satisfy the size requirement with repetition, generic advice, motivational language, restated headings, or filler. Every paragraph must preserve actionable context, evidence, rationale, state, or procedure.
- Prefer explicit redundancy only where it prevents a high-risk mistake. When repeating a critical constraint, connect it to the specific step or file where it applies.
- Use full explanations rather than terse labels. A bullet such as “fix validation” is insufficient; identify the validation, current result, cause, relevant symbols, required change, expected result, and how to verify it.
- Do not impose a self-selected brevity limit. If the response limit is genuinely insufficient, provide the handoff in clearly numbered consecutive parts and state that all parts form one document. Do not omit later sections silently.

Before finalizing, compare the document against every mandatory section below. A handoff that omits a required section, uses vague placeholders, or delegates essential reconstruction to the next model is incomplete.

## 2. Operating rules while producing the handoff

The handoff must reflect the current verified repository state, not memory alone.

1. Read the applicable repository instructions and identify their scope and precedence.
2. Review the complete current conversation, including user approvals, corrections, rejected approaches, validation evidence, and unresolved requests.
3. Before attempting any Git retrieval, inspect the chat attachments, mounted input locations, workspace, and other user-provided inputs for supplied game files or archives containing game files.
4. Inspect the current working tree, when one exists, and distinguish committed state, pre-existing user changes, changes made during this task, staged changes, unstaged changes, untracked files, and deleted files.
5. Read the complete current versions of every file materially involved in the task, plus direct callers, consumers, producers, shared contracts, tests, plans, architecture documents, and validation instructions needed to understand the work.
6. Compare relevant working files with `HEAD` and with accepted or superseded historical versions when history influenced the current design and local Git history is available.
7. Use repository search and locally available history where needed to identify references, dependencies, renamed concepts, compatibility paths, and stale documentation.
8. Do not modify implementation, documentation, assets, Git state, external systems, or task scope merely to produce the handoff unless the user separately authorized that change.

### Mandatory source precedence for supplied game files

If the user has provided game files directly, whether as loose files, a directory, a workspace snapshot, a `.zip`, `.7z`, `.rar`, `.tar`, `.tar.gz`, `.tgz`, `.unitypackage`, or another archive or package format, those supplied files are the **mandatory authoritative starting point**.

- Use the supplied files. Do not ignore them in favor of a remote repository, a familiar upstream project, or a cleaner checkout.
- **Do not run `git clone` when supplied game files exist.** Do not create a parallel clone, download a replacement repository snapshot, or substitute remote files for the supplied contents.
- Do not use `git pull`, `git reset`, `git checkout`, restore operations, or another remote-based operation to overwrite or reconstruct supplied files. Such operations require separate explicit authorization even when the supplied files contain a `.git` directory.
- Git metadata already included with the supplied files may be inspected locally for status, diffs, and history. That permission does not authorize cloning, fetching replacement content, or changing the supplied state.
- If the supplied files have no `.git` directory, work from them as provided. Mark branch, `HEAD`, remote, and Git-history fields as unavailable or not applicable. Never clone merely to manufacture missing repository metadata.
- Preserve the original supplied archive unchanged. Before extraction, inspect its entry paths for absolute paths or parent-directory traversal. Extract non-destructively into a dedicated local directory, verify the extraction destination, and record the archive name, location, size, hash when available, extraction method, destination, and result.
- If more than one supplied archive or game-file set exists, inventory all of them. When their precedence or relationship is ambiguous, disclose the conflict and request user direction; do not silently choose one and do not use Git as a fallback.
- If a supplied archive is corrupt, encrypted without an available password, incomplete, or missing required files, record the exact failure and request the missing input or authorization. Do not replace it by cloning from Git.
- A Git clone is permissible only when no supplied game files or game-file archive exists and the user has separately authorized obtaining the project from Git.

The generated handoff must state whether supplied game files were searched for, what was found, which source set was used, and whether any source-provenance question remains unresolved.

### Mandatory affected-file declaration for every update

For **every** implementation, documentation, configuration, asset, migration, generation, formatting, autofix, move, rename, or deletion update, the acting agent must state exactly which files the update is expected to affect **before** performing any write or running any modifying tool. This requirement applies regardless of update size.

- Assign the update a stable identifier or specific title so its declaration, implementation, validation, and handoff record can be traced to the same unit of work.
- State `Expected affected files` and list every exact path grouped by intended operation: `Modify`, `Create`, `Delete`, `Move/Rename` with old and new paths, `Generate`, or `Metadata/Companion`.
- Include generated outputs, import metadata, lockfiles, manifests, snapshots, caches committed to the project, and other companion files when the update can affect them. Do not list only the primary source file when a tool or engine can also change dependent files.
- Name known exact paths. A wildcard, directory name, subsystem label, file type, “related files,” “generated files,” or “and similar files” is not an acceptable substitute.
- If the update will affect no files, state `Expected affected files: None` and explain why the update is read-only or otherwise produces no persistent file change.
- The declaration is a scope statement, not authorization. Every path must still be inside the approved scope and comply with repository instructions.
- If the exact affected paths cannot yet be determined, perform only read-only discovery or a non-writing dry run until they can be named. Do not begin the modifying operation with an open-ended file scope.
- If new evidence requires an undeclared file to be affected, stop before modifying it, amend the affected-file declaration and canonical plan, explain why the scope changed, and obtain any required approval. Do not silently add the file mid-update.
- After each update, state `Actually affected files` with every exact path and actual operation. Reconcile this list against the pre-update declaration, including declared files that remained unchanged and any unexpected tool-created or tool-modified files.
- If a tool unexpectedly affects an undeclared file, stop further implementation, preserve the evidence, report the file and operation, update the plan, and resolve scope or approval requirements before continuing. Do not silently keep, delete, restore, or overwrite the unexpected change.

The generated handoff must preserve both the expected and actual affected-file lists for every completed, active, or planned update. A combined file list for the whole task is not sufficient when the task contains multiple updates.

If a required inspection is unavailable, say exactly what could not be inspected, why it was unavailable, which conclusions are therefore limited, and what the next model must do to close the gap.

## 3. Evidence and certainty standard

Attach evidence to technical claims at the point where the claim is made.

- For repository facts, cite an exact path and the relevant symbol, section, line, diff, command output, or commit when available.
- For observed behavior, include the relevant test name, command, result, log, screenshot description, measurement, or reproducible procedure.
- For historical claims, cite the commit, diff, prior document, or previous implementation that establishes the history.
- For external constraints, cite the primary specification or official documentation when it was actually consulted.
- Clearly distinguish facts observed in the current repository from statements reported by the user.
- Label incomplete conclusions as **Inference**, **Hypothesis**, **Opinion**, or **Unverified**, include **High**, **Medium**, or **Low** confidence, state the evidence supporting the label, and specify what would confirm or falsify it.
- Never fabricate file contents, line numbers, test results, commits, runtime behavior, approvals, or completion status.
- Do not say that something is “working,” “safe,” “complete,” “validated,” or “ready” unless the evidence and applicable acceptance criteria support that exact claim.

Use short excerpts only when the precise text matters. Prefer exact references plus explanation over pasting large source files into the handoff.

## 4. Performance-first solution requirements

Performance is a **top-priority design and implementation criterion**. The default recommendation must be the correct solution with the lowest practical performance cost that satisfies approved behavior, quality, scope, architecture, safety, and acceptance criteria. Do not treat performance as optional polish to assess after selecting an implementation.

Correctness and explicit user requirements remain hard constraints. When the lowest-cost option cannot satisfy them, recommend the lowest-cost option that can and identify the additional cost explicitly. Any selected solution that is materially more expensive than a viable alternative must be presented as a performance exception under the protocol below.

### Performance priority order

Use this order when evaluating and trading costs:

1. **Active-gameplay runtime compute — highest priority.** This includes CPU and GPU work performed continuously or frequently during active play: per-frame, per-render, per-physics-tick, per-AI-tick, per-input, per-visible-object, per-particle, per-pixel, per-vertex, hot-loop, synchronization, dispatch, draw, culling, animation, and other latency-sensitive work. Frame-time spikes, stalls, garbage collection, synchronization points, memory-bandwidth pressure, and work multiplied by active instance count belong here when they affect gameplay execution.
2. **Dirty-triggered or change-triggered runtime compute.** This is runtime work performed only when inputs, configuration, content, topology, state, or cached data becomes dirty and must be rebuilt, regenerated, uploaded, rebound, or recomputed. Assess trigger frequency, burst size, worst-case spike, amortization, invalidation breadth, and whether the work can occur during active play. Work that becomes frequent enough to behave like continuous gameplay work must be reclassified into the first category.
3. **Memory usage.** This includes persistent and peak CPU memory, GPU memory, managed and native allocations, transient allocations, retained caches, duplicated data, buffer capacity, fragmentation risk, allocation churn, and garbage-collection pressure not already counted as active runtime compute.
4. **Storage space — lowest priority and substantially below memory.** This includes repository size, source-asset size, generated-data size, cache size on disk, build size, installed size, and saved-data size. Runtime I/O latency, decompression work, streaming stalls, and upload work are not merely storage-space costs; classify them under active or dirty-triggered runtime compute as appropriate.

The intended weighting is:

`active-gameplay runtime compute > dirty-triggered runtime compute > memory usage >> storage space`

Apply the hierarchy directly:

- Do not save storage space by increasing active-gameplay compute unless a hard storage constraint requires it and the tradeoff is explicitly approved.
- Prefer precomputation, caching, baked data, and additional storage when they materially reduce higher-priority runtime work, provided their memory, invalidation, loading, correctness, and workflow costs remain acceptable.
- Do not reduce memory by moving equivalent or greater work into a frequent active-gameplay path without explicit evidence that the tradeoff improves the stated priorities.
- A lower-priority improvement does not compensate automatically for a higher-priority regression. State both costs separately rather than combining them into an unsupported overall claim.
- Consider the aggregate cost at realistic and worst-case scale. A small per-instance, per-frame, per-pixel, or per-event cost may dominate when multiplied across the game.

### Mandatory performance analysis for every offered solution

Every offered solution, including the recommended solution and each serious alternative, must contain a performance-impact analysis. The analysis must be proportional to the solution's complexity but may not be omitted because a change appears small, editor-only, data-only, or obviously inexpensive.

Analyze performance at every level that materially exists:

- **Per step or phase:** discovery, generation, build, initialization, loading, dirty rebuild, active update, rendering, teardown, serialization, and validation phases.
- **Per function or execution path:** relevant methods, loops, callbacks, jobs, compute kernels, shader passes, allocations, uploads, synchronization points, and event handlers.
- **Per file or asset:** the performance role of each affected implementation file, shader, include, configuration, serialized asset, generated artifact, and cache.
- **Per change:** the incremental cost introduced, removed, moved, cached, amortized, or transferred by that exact change.
- **Overall:** combined end-to-end cost across all changed paths, realistic instance counts, affected subsystems, platforms, and expected usage patterns.

For each solution, state all applicable items:

1. current baseline behavior and cost, with evidence or an explicit statement that the baseline is unmeasured;
2. when the cost executes: editor-time, build-time, load-time, initialization, dirty-triggered runtime, active gameplay, rendering, teardown, or storage-only;
3. execution frequency and trigger: per frame, tick, camera, object, element, vertex, pixel, dispatch, event, dirty transition, load, save, build, or session;
4. CPU cost, GPU cost, synchronization or stall risk, memory-bandwidth impact, and I/O or upload work;
5. algorithmic time and space complexity using the actual scaling variables, plus expected typical and worst-case scale;
6. managed, native, and GPU allocations; persistent, peak, and transient memory; cache size; allocation lifetime; garbage-collection implications;
7. dirty-state invalidation breadth, rebuild frequency, burst cost, whether work is amortized, and whether it can interrupt active gameplay;
8. storage impact, including source, generated, build, installed, cache, and saved-data size where applicable;
9. local impact for each relevant step, phase, function, file, and change;
10. aggregate impact after multiplying recurring costs by realistic and worst-case counts and composing all affected paths;
11. costs transferred between priority categories, such as exchanging storage or memory for lower runtime compute;
12. performance risks, regressions, platform sensitivities, scalability limits, and failure modes;
13. lower-cost alternatives considered and the evidence-based reason the recommendation is preferable;
14. measurement status: measured, analytically derived, estimated, inferred, or unknown;
15. exact profiling, benchmark, instrumentation, or stress-test procedure required to validate the claim, including baseline, workload, metrics, pass thresholds, and comparison method.

Use quantitative evidence when available: CPU time, GPU time, frame time, call count, dispatch count, draw count, allocation bytes, garbage-collection allocations, resident memory, peak memory, buffer size, bandwidth, load time, rebuild time, file size, build size, or another directly relevant metric. Identify the environment, platform, scene, content scale, configuration, warm-up, sample duration, and tool used. Do not present numbers from a different workload as proof for the current one.

When measurements are unavailable, do not invent precision. Label the statement as an estimate, inference, or unknown; show the cost model or reasoning; identify the variables most likely to change the result; and give the concrete measurement that would verify or falsify it. Terms such as “free,” “zero cost,” “negligible,” “cheap,” “lightweight,” “optimized,” or “no performance impact” require supporting evidence and a defined scale.

### Performance exception protocol

Clearly mark any solution or change that does not minimize practical performance cost, regresses a higher-priority category, or accepts a material cost for correctness, visual quality, maintainability, compatibility, schedule, workflow, or another requirement.

Use the explicit label `PERFORMANCE EXCEPTION` and state:

- the exact solution, step, phase, function, file, and change affected;
- which priority category regresses;
- measured or estimated magnitude, frequency, scaling behavior, and worst-case impact;
- why the higher cost is necessary;
- the lower-cost alternatives considered and why they are not acceptable;
- mitigation, containment, budgets, feature gates, caching, amortization, quality tiers, or fallback behavior;
- the evidence and validation required;
- whether the exception has user or architecture approval, or whether approval remains required.

Do not hide an exception in a general tradeoff paragraph. Do not call a higher-cost solution “preferred” without identifying the performance exception. A handoff must make every active and proposed performance exception easy for the receiving model to find.

### Required performance conclusion

For every solution comparison and every implementation update, end the performance analysis with:

- the recommended option under the stated priority hierarchy;
- the local performance effect per relevant unit of work;
- the overall performance effect at realistic and worst-case scale;
- the highest-priority cost changed;
- the evidence confidence and outstanding measurements;
- any `PERFORMANCE EXCEPTION`;
- the exact performance validation that must pass before the work can be considered verified.

## 5. Required handoff structure

Use the following top-level sections in this order. Preserve all sections. If a section is genuinely not applicable, write `Not applicable`, explain why, and provide the evidence supporting that conclusion.

### A. Handoff identity

State:

- a specific title that names the subsystem and current objective;
- the date and the point in the work at which the handoff was created;
- the repository or workspace root, current branch, and relevant revision identifiers;
- the exact supplied game-file or archive source, when present, including its original location and the workspace or extraction directory actually used;
- whether the handoff covers implementation, diagnosis, review, planning, validation, or a combination;
- the authoritative source of truth for current status;
- any terminology needed to interpret the document.

### B. Immediate continuation brief

Start the substantive handoff with a dense but complete operational summary. In several paragraphs, explain:

- the user's actual goal and the current desired outcome;
- the present state of the work;
- the most important completed result;
- the exact next action the receiving model should take;
- the highest-risk constraint or mistake to avoid;
- whether implementation is authorized, partially authorized, awaiting approval, or prohibited;
- any current blocker that changes what can happen next.
- whether supplied game files or an archive exist, whether they are the active source, and confirmation that no replacement Git clone was used.
- the stable identifier and exact expected affected-file list for the next update, or `None` with a reason when the next action is read-only.
- the recommended solution's highest-priority performance impact, overall expected impact, evidence status, and any `PERFORMANCE EXCEPTION`.

This section is an orientation layer, not a substitute for the detailed sections that follow.

### C. User intent, scope, and acceptance criteria

Record the user's request in operational terms. Include:

- the primary objective and the practical reason for it;
- explicit acceptance criteria stated by the user;
- acceptance criteria established by repository rules, plans, architecture, or validated decisions;
- files, systems, behaviors, and deliverables inside the approved scope;
- files, systems, behaviors, and deliverables explicitly outside scope;
- actions that require additional approval;
- assumptions that were allowed and assumptions that must not be made;
- any user preferences about design, behavior, performance, visuals, workflow, or communication;
- explicit performance acceptance criteria, budgets, target platforms, workloads, scaling assumptions, and the active-gameplay > dirty-triggered > memory >> storage priority order;
- requests that were superseded, rejected, narrowed, or deferred, with the reason and evidence.

Quote the user's exact wording only when precision is necessary. Otherwise provide a faithful, detailed paraphrase.

### D. Governing instructions and source-of-truth hierarchy

List every instruction, plan, architecture document, checklist, handoff, specification, and validation requirement that governs the task. For each one, state:

- exact path or source;
- which part of the task it governs;
- whether it is current, historical, superseded, or partially authoritative;
- relevant constraints and mandatory gates;
- how conflicts are resolved;
- where user-provided game files, archive contents, repository instructions, plans, local Git metadata, and remote sources sit in the source-of-truth hierarchy, with supplied game files taking precedence over Git retrieval;
- which document is the canonical plan and which document records final validation.

Do not merely provide a reading list. Explain why each source matters and what the next model must extract from it.

### E. Repository and working-tree state

Provide a precise snapshot of the state the next model will inherit:

- all supplied game-file sets and archives inspected, including exact names, locations, sizes, hashes when available, extraction destinations, and extraction results;
- the authoritative source set actually used and why it was selected;
- explicit confirmation that no Git clone or remote replacement was used when supplied files existed;
- workspace root and active branch;
- relevant `HEAD` commit and any important historical commits;
- staged, unstaged, untracked, and deleted files relevant to this task;
- unrelated pre-existing changes that must be preserved;
- files modified during the current task;
- an update-by-update ledger mapping each stable update identifier to its predeclared expected files, actual affected files, operations, discrepancies, and approval status;
- generated files, serialized assets, metadata, caches, or local-only outputs that require special handling;
- whether the repository was clean before the task, if known;
- commands used to obtain the state and the time or stage at which it was observed.

For every changed file in scope, distinguish the task's intentional delta from user-owned or pre-existing edits. Explicitly warn against destructive Git operations when they could erase unrelated work.

If the supplied files contain no Git metadata, say so and mark Git-only fields as unavailable or not applicable. Do not weaken or omit the repository-state section and do not obtain a clone to populate those fields.

### F. System and architecture explanation

Explain the relevant system deeply enough that the next model can reason about changes without rediscovering its structure. Cover, as applicable:

- responsibilities of each component;
- runtime, editor-time, build-time, or generation-time flow;
- data ownership and data flow from producers through transformations to consumers;
- public and internal contracts;
- serialization and migration behavior;
- lifecycle ordering and event sequencing;
- shader, rendering, geometry, asset, scene, prefab, or profile interactions;
- performance-sensitive paths and accepted budgets;
- current performance baselines, execution frequencies, scaling variables, allocation behavior, dirty-trigger boundaries, aggregate instance counts, and unmeasured performance assumptions;
- failure modes, fallbacks, compatibility paths, and diagnostics;
- cross-subsystem dependencies and shared resources;
- architecture invariants that the pending work must preserve.

Use a compact diagram, ordered flow, or table when it materially clarifies relationships. Define all project-specific terms and acronyms on first use.

### G. File and symbol inventory

Provide an annotated inventory, grouped by role rather than as one flat list. Include all relevant:

- governing documents and plans;
- supplied archives, packages, workspace snapshots, and extracted source roots;
- implementation files;
- direct callers and entry points;
- producers, consumers, and shared contracts;
- editor or tooling files;
- shaders and includes;
- tests and validation utilities;
- serialized assets and metadata;
- historical or superseded files that explain current decisions.

For each file, state:

1. exact path;
2. relevant classes, methods, fields, shader functions, document sections, or assets;
3. its role in the system;
4. why it matters to the current task;
5. whether it was read completely or only inspected in part;
6. current change status;
7. required next action, or explicitly state that no edit is planned;
8. dependencies or risks associated with editing it.

For every supplied archive or game-file set, also record its provenance, integrity or extraction status, whether it is authoritative, and how it relates to any other supplied set. Do not treat a remote clone as an inventory substitute.

Do not write “read the relevant files” or “inspect related code.” Name them. If discovery is still required, provide exact search commands, patterns, or dependency paths and state that the inventory is incomplete.

### H. Chronological history of the work

Reconstruct the task in order. Include:

- initial problem or request;
- investigations performed and evidence collected;
- competing explanations or approaches considered;
- decisions made and who or what authorized them;
- implementation steps completed;
- user feedback and resulting course corrections;
- validation cycles and what each one proved or failed to prove;
- reversions, abandoned experiments, and superseded plans;
- current stopping point.

For every material decision, explain **why** it was made, which alternatives were rejected, the tradeoffs, and the evidence. Do not collapse the history into only the final state when earlier attempts contain information that prevents repeated mistakes.

### I. Completed work, with per-file deltas

Describe every completed change in enough detail for review and debugging. For each changed file or coherent change set, include:

- the stable update identifier or title;
- the exact pre-update `Expected affected files` declaration, grouped by intended operation;
- the exact post-update `Actually affected files` reconciliation, grouped by actual operation;
- any difference between the expected and actual lists, why it occurred, whether implementation stopped, and how scope or approval was resolved;
- previous behavior or content;
- current behavior or content;
- exact symbols or sections changed;
- reason for the change;
- how it connects to the objective and plan item;
- intentional behavior differences;
- behavior intended to remain unchanged;
- compatibility, migration, serialization, performance, or visual consequences;
- detailed local performance impact per relevant step, phase, function, file, and change, plus the combined overall impact under realistic and worst-case scale;
- the measurement status, evidence, comparison with baseline, and any `PERFORMANCE EXCEPTION`;
- validation already performed and its result;
- any residual concern.

Do not equate “code was written” with “work is complete.” Distinguish implemented, reviewed, compiled, tested, visually validated, accepted, and documented states.

### J. Investigations, failed approaches, and lessons

Record work that did not become part of the final implementation. Include:

- hypotheses tested;
- commands, probes, temporary instrumentation, or experiments used;
- results and interpretation;
- approaches rejected and the exact reason;
- misleading symptoms or dead ends;
- temporary changes that were removed or must still be removed;
- conditions under which a rejected approach might become valid later.

This section must prevent the receiving model from repeating expensive failed work. Do not omit an attempt merely because it did not produce a diff.

### K. Current behavior and verified state

State what the system or document does **now**, based on current evidence. Separate:

- behavior verified by tests or direct observation;
- behavior established by code inspection only;
- user-reported behavior not independently reproduced;
- expected behavior not yet verified;
- known regressions or inconsistencies;
- platform, scene, asset, configuration, or environment conditions affecting the result.

Where visual quality or interactive behavior matters, describe the exact observed state and reference the available screenshot, capture, scene, object, profile, or reproduction setup.

### L. Remaining work and gap analysis

Enumerate all unfinished work. For every item, provide:

- stable identifier and priority;
- current status: not started, in progress, implemented but unverified, blocked, deferred, or awaiting approval;
- exact desired end state;
- gap between current and desired state;
- files and symbols likely involved;
- dependencies and prerequisites;
- acceptance criteria;
- validation method;
- risk and likely failure modes;
- expected local and aggregate performance impact under the required hierarchy, including measurement gaps and exception status;
- whether the item is in the approved scope;
- reason it remains unfinished.

Separate mandatory work from optional follow-up, cleanup, and future ideas. Do not disguise required work as optional and do not inflate scope with speculative improvements.

### M. Exact continuation procedure

Write a step-by-step execution plan detailed enough for the next model to follow without designing a new plan from scratch. Each step must include:

1. stable update identifier and objective;
2. prerequisites and required authorization;
3. exact files and symbols to read before editing;
4. exact `Expected affected files`, grouped as modify, create, delete, move/rename, generate, or metadata/companion operations;
5. intended change and rationale;
6. invariants and behaviors to preserve;
7. dependencies and sequencing constraints;
8. detailed expected performance impact per relevant step, phase, function, file, and change, plus overall impact and exception status;
9. expected intermediate result;
10. validation command or manual check, including performance measurement when applicable;
11. functional and performance pass criteria;
12. failure response or rollback strategy;
13. required post-update `Actually affected files` reconciliation;
14. plan/documentation update required before moving on.

Include concrete commands when known and safe. Mark example commands as examples. Do not prescribe destructive commands, broad resets, blind formatting, raw serialized-asset edits, scope expansion, or external side effects unless explicitly authorized and required.

The first steps must re-establish repository state and confirm that the handoff still matches reality. The plan must say where the next model may continue directly and where it must stop for user approval.

Immediately before each modifying step, the receiving agent must repeat the stable update identifier and exact expected affected-file declaration to the user. It must not rely on a declaration buried elsewhere in the handoff. Immediately after the step, it must report and reconcile the exact actual affected files before starting another update.

The first continuation step must inventory supplied game files and archives before any Git retrieval. If supplied files exist, the plan must use them, prohibit cloning or replacing them from Git, verify any extraction directory, and treat absent Git metadata as unavailable rather than as a reason to clone.

### N. Validation and evidence ledger

Create a table or structured list for every validation activity. Include:

- validation identifier;
- command or procedure;
- environment and relevant configuration;
- time or task stage performed;
- exact result, including warnings and failures;
- what the result proves;
- what it does **not** prove;
- evidence location;
- whether repetition is required after remaining changes.

Cover all applicable compilation, automated tests, static checks, formatting checks, diff checks, editor validation, runtime validation, visual review, performance measurements, serialization checks, and documentation consistency checks.

For performance validation, record the baseline and candidate under the same workload, environment, platform, configuration, scale, warm-up, sampling method, and profiler or measurement tool. Include the metrics and thresholds associated with active-gameplay compute, dirty-triggered compute, memory, and storage. State which priority categories remain unmeasured and the exact next measurement required.

For unavailable validation, use `Pending` rather than `Passed`. State why it is unavailable and give the concrete next action. For failed validation, preserve the failure rather than summarizing it away.

### O. Constraints, invariants, and do-not-do list

Collect the constraints that are most likely to be violated during continuation. Tie each constraint to its source and affected files or steps. Include, where applicable:

- scope and approval boundaries;
- repository workflow gates;
- user-owned changes to preserve;
- supplied game-file and archive precedence, including the prohibition on cloning or remotely replacing those files;
- the mandatory pre-update expected-file declaration and post-update actual-file reconciliation for every update;
- performance as a top-priority criterion, the required priority hierarchy, mandatory detailed impact analysis for every solution, and explicit `PERFORMANCE EXCEPTION` handling;
- architecture and API contracts;
- serialization and metadata requirements;
- performance and geometry budgets;
- lifecycle or ordering requirements;
- rendering, shader, platform, and engine constraints;
- naming, folder, layer, tag, component, and dependency restrictions;
- prohibited cleanup, refactors, compatibility breaks, or speculative changes;
- conditions that require stopping and updating the plan.

Write explicit consequences. “Preserve compatibility” is insufficient; identify which compatibility path, who consumes it, and what failure would occur if it changed.

### P. Risks, blockers, unknowns, and decision requests

Maintain separate subsections for:

- active blockers;
- known risks;
- unresolved questions;
- assumptions currently in force;
- decisions that require user approval;
- external dependencies or unavailable evidence.

For each entry, state impact, probability where meaningful, evidence, mitigation, owner or decision-maker, and the exact condition that resolves it. If there are no entries in a subsection, state `None found after [specific inspection]`; do not simply write `None`.

### Q. Recommended reading order for the next model

Give an ordered reading list optimized for safe continuation. For each item, state:

- exact path or source;
- whether to read it completely;
- the specific information to extract;
- why it comes at that point in the order;
- documents or implementations it supersedes;
- inconsistencies to watch for.

The list must include applicable instructions first, then canonical plans and architecture, then implementation and direct dependencies, then tests and historical references. Do not use the reading list as a substitute for summarizing critical facts in the handoff itself.

### R. Commands and reproduction reference

Collect the safe, relevant commands and procedures needed to recover context and validate continuation. Include:

- commands or procedures used to locate, inspect, hash, and safely extract supplied archives without overwriting their originals;
- repository status and diff commands;
- targeted search commands;
- build, compile, and test commands;
- reproduction procedure;
- log or artifact locations;
- environment prerequisites;
- expected success indicators.

Explain what each command is for. Preserve exact quoting and working directories when that matters. Do not include secrets, credentials, machine-specific private data, or a command that changes external state unless the action is already authorized and clearly labeled.

When supplied game files exist, do not include `git clone` as a continuation command and do not present cloning as a fallback. If no Git metadata is present, state that Git-specific commands are not applicable. Any command that fetches, pulls, restores, or replaces supplied content must be excluded unless the user explicitly authorized that exact operation.

### S. Final state matrix

End the technical body with a compact matrix mapping each objective or plan item to:

- stable update identifier;
- exact expected affected files and operations;
- exact actual affected files and operations;
- file-list discrepancy status;
- local performance impact by relevant unit of work;
- overall performance impact at realistic and worst-case scale;
- highest-priority cost affected, evidence status, performance-validation status, and exception status;
- scope status;
- implementation status;
- review status;
- validation status;
- documentation status;
- evidence;
- next action;
- owner or approval requirement.

This matrix must agree with the detailed narrative. Resolve contradictions before finalizing the handoff.

### T. Receiving-model startup checklist

Provide a short ordered checklist that the receiving model can execute at the beginning of the next chat. It must include:

1. read applicable instructions and the named canonical sources;
2. inventory chat attachments, mounted inputs, the workspace, and supplied game-file archives before any Git retrieval;
3. if supplied game files exist, confirm they are the active source, preserve the originals, and do not clone or replace them from Git;
4. confirm branch, `HEAD`, working tree, and relevant file contents have not drifted when local Git metadata exists; otherwise mark those Git fields unavailable;
5. preserve unrelated changes and compare current reality with the handoff, recording deviations;
6. confirm authorization and scope, announce the next update's stable identifier and exact expected affected files, review its detailed local and overall performance impact under the required hierarchy, then continue and reconcile the actual affected files and measured performance before any later update.

This checklist is an entry point, not a replacement for the exact continuation procedure.

## 6. Required writing quality

- State outcomes before background while retaining the full background later.
- Use direct, technically precise language.
- Keep paths, symbol names, commands, identifiers, values, and statuses exact.
- Explain causal relationships; do not provide disconnected inventories.
- Use tables where they improve comparison or state tracking, but do not compress necessary explanations into unreadable cells.
- Use numbered procedures for dependent steps and bullets for non-sequential inventories.
- Define status terms and use them consistently.
- Make contradictions visible and resolve them where evidence permits.
- Avoid statements such as “as discussed above,” “the usual process,” “etc.,” “and related files,” or “continue as needed” when they conceal required detail.
- Do not assume that the next model can access this chat, tool outputs that are not saved, transient screenshots, or unstated user intent.
- Do not paste massive logs or complete source files when a precise excerpt and durable location are sufficient. Preserve complete logs only when the exact output is material and no durable reference exists.

## 7. Anti-compression rules

The following are specifically prohibited because they create handoffs that look complete but are not operationally complete:

- replacing per-file analysis with a list of filenames;
- replacing history with only the final decision;
- replacing validation evidence with “tests pass”;
- replacing next steps with task names lacking procedure and pass criteria;
- replacing rationale with “for maintainability,” “for performance,” or another generic benefit;
- describing a solution as performant, cheap, negligible, optimized, or free without local and aggregate analysis, scale, evidence status, and a validation method;
- omitting a material performance regression or failing to label a higher-cost selection as `PERFORMANCE EXCEPTION`;
- reporting only overall performance while hiding per-step, per-phase, per-function, per-file, or per-change costs, or reporting only local costs while omitting their aggregate effect;
- omitting pre-existing working-tree changes;
- omitting supplied-file provenance or silently substituting a Git clone for provided game files;
- providing only a task-wide, directory-level, wildcard, or “related files” list instead of exact expected and actual paths for every update;
- modifying an undeclared file or continuing after an unexpected file change without updating the declaration, plan, and approval state;
- treating an unverified implementation as completed work;
- collapsing multiple unresolved items into “minor cleanup remains”;
- omitting rejected approaches and thereby causing repeated work;
- asserting certainty to avoid documenting an unknown;
- telling the next model to “review the codebase” without exact starting points;
- relying on the receiving model to infer scope or authorization;
- reducing detail merely to fit a normal response length.

## 8. Final completeness audit

Before sending the handoff, perform and include a brief self-audit. Confirm each item with evidence or mark the handoff incomplete:

- all mandatory sections A–T are present;
- the document meets the substantial-task depth expectation without filler;
- the current user objective, scope, authorization state, and acceptance criteria are explicit;
- every in-scope changed file has a per-file delta and status;
- every completed, active, and planned update has a stable identifier, an exact expected affected-file declaration, an exact actual-file reconciliation where applicable, and a resolved explanation for every discrepancy;
- every offered solution and update includes detailed performance impact per relevant step, phase, function, file, and change plus overall realistic and worst-case impact;
- performance recommendations follow `active-gameplay runtime compute > dirty-triggered runtime compute > memory usage >> storage space`, with baselines, frequency, scaling, allocations, evidence status, measurements, and validation criteria stated;
- every material higher-cost choice is visibly labeled `PERFORMANCE EXCEPTION`, justified against lower-cost alternatives, mitigated where possible, and assigned an approval status;
- relevant callers, consumers, producers, contracts, tests, documents, and history are covered;
- working-tree ownership and unrelated changes are distinguishable;
- supplied game files and archives were inventoried, their provenance and extraction state are recorded, the supplied source was used, and no prohibited Git clone or remote replacement occurred;
- decisions include rationale, alternatives, tradeoffs, and evidence;
- unfinished work includes exact procedures and pass criteria;
- validations distinguish passed, failed, unavailable, and pending states;
- facts, user reports, inferences, hypotheses, opinions, and unknowns are distinguishable;
- blockers and approval needs are explicit;
- the reading order and startup checklist are actionable;
- the state matrix agrees with the narrative;
- no essential detail depends on access to the current chat.

If any audit item fails, expand or correct the handoff before sending it. The final output should be the handoff document itself, not an explanation of how you wrote it.
