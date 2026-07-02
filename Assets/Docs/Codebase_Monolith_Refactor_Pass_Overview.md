# Codebase Monolith Refactor Pass Overview

## Purpose

Define how a refactor pass should approach oversized source, shader, and compute modules without prematurely deciding the final split map.

The goal is not to make files smaller for its own sake. The goal is to reduce maintenance cost, improve reviewability, make future feature work safer, and expose natural ownership boundaries while preserving the current runtime behaviour.

This document is intentionally content-agnostic. The first implementation step is a research pass that identifies the concrete modules, responsibilities, coupling points, and risk areas. Only after that research should the team approve a specific split plan.

## Document Status

**Status:** Initial overview.

**Scope:** Project-owned monolithic modules, including C# gameplay/editor/runtime code and large HLSL, shader, and compute assets.

**Out of scope for this overview:** Naming the exact final files, moving code, changing public behaviour, retuning features, or redesigning systems.

## Core Principle

A successful refactor pass should preserve behaviour first and improve structure second.

The safest version of this work is staged:

1. discover and classify the current monoliths;
2. identify ownership boundaries and risk;
3. choose the least disruptive split strategy for each category;
4. make narrow mechanical changes first;
5. extract deeper responsibilities only where the research proves they are stable;
6. validate after every slice.

The refactor should not become a feature pass. Any behavioural improvement discovered during research should be recorded separately unless it is required to make the split safe.

## Why This Is Worth Doing

Very large modules tend to hide several different problems:

- unrelated responsibilities living in one file;
- long methods that are difficult to review or reason about;
- implicit ordering requirements;
- duplicated constants or data layouts across CPU and GPU code;
- editor UI mixed with runtime behaviour;
- resource allocation mixed with simulation logic;
- shader helper code mixed with kernel or pass entry points;
- debug, diagnostics, and production paths interleaved;
- fragile changes because small edits require touching a high-conflict file.

Splitting can help, but only if it follows real boundaries. A bad split can make navigation worse by spreading one tightly coupled responsibility across many files. The research pass must distinguish between "large but coherent" and "large because it contains multiple systems."

## Research Pass

The first concrete phase should produce a refactor map before any major code movement.

### Inventory

Build an inventory of oversized modules across source and rendering assets. The inventory should capture:

- path;
- file type;
- line count;
- main declared types or shader entry points;
- public API surface;
- serialized fields or asset-facing properties;
- generated or editor-only status;
- known owner subsystem;
- direct dependencies;
- test or validation coverage.

The inventory should include large files below the obvious threshold if they are frequently edited, hard to review, or sit on an important boundary.

### Responsibility Mapping

For each candidate, identify what responsibilities it currently combines. Examples include:

- configuration and validation;
- lifecycle and initialization;
- resource allocation and release;
- CPU simulation or topology construction;
- GPU dispatch and binding;
- editor UI;
- debug drawing and diagnostics;
- asset serialization;
- data conversion and cache encoding;
- shader property contracts;
- shared math and sampling helpers;
- kernel or pass entry points.

The output should be a responsibility map, not a final file list.

### Coupling Review

Before splitting, identify coupling that may constrain the refactor:

- Unity serialization and Inspector field names;
- prefab and scene references;
- component identity and `RequireComponent` relationships;
- public methods used by editor tooling or other systems;
- shader property names and buffer layouts;
- compute kernel names and dispatch order;
- include order and macro dependencies;
- runtime initialization order;
- editor-only compilation boundaries;
- generated asset import behaviour.

This review should call out which dependencies are intentional contracts and which are accidental implementation coupling.

### Change Frequency Review

Large files should be prioritized when they also change often. The research pass should look for:

- files that are repeatedly touched by unrelated changes;
- files that frequently cause merge conflicts;
- files where bug fixes require broad scrolling and context gathering;
- files where new feature work tends to append code rather than extend a clear abstraction.

This helps separate urgent refactor targets from merely large but stable modules.

### Method and Function Shape

File size is not enough. The research pass should measure whether the problem is:

- many small helpers in one file;
- a few extremely long methods;
- deeply nested control flow;
- large switch or phase machines;
- repeated binding or validation patterns;
- repeated shader sampling patterns;
- mixed debug and production logic.

Each shape wants a different treatment. Moving a 500-line method into another file does not solve the underlying readability problem.

## Refactor Strategies

The chosen strategy should depend on risk and module type.

### Mechanical File Splitting

Use this when a large module already has clear internal sections and the main risk is file size or review conflict.

Typical moves:

- split one class into partial files by responsibility;
- move editor UI sections into editor-only partials;
- move constants or property IDs into a dedicated contract file;
- move local data structs into a nearby data file when they are stable;
- keep serialized fields in place until a later phase proves they can move safely.

This is usually the safest first step for Unity component classes because it preserves type identity and serialized data.

### Helper Extraction

Use this when a long method or function contains separable operations with clear inputs and outputs.

Typical moves:

- extract validation helpers;
- extract metric or diagnostic formatting;
- extract resource creation helpers;
- extract pure math;
- extract repeated binding sequences;
- extract small state transition helpers.

This should be done after tests or manual validation points are known, because helper extraction can accidentally alter order-sensitive behaviour.

### Service or Collaborator Extraction

Use this when a responsibility has a stable lifecycle and a meaningful boundary.

Typical examples:

- runtime resource owner;
- topology or geometry builder;
- cache codec;
- diagnostic snapshot builder;
- editor preview generator;
- shader binding facade;
- scheduling coordinator.

This is more invasive than partial splitting. It should be reserved for responsibilities that are truly separable and are likely to remain separate.

### Shader and Compute Include Splitting

Large shader and compute assets should be handled differently from C# files.

The safest initial strategy is usually:

- keep the main shader or compute file as the pass or kernel manifest;
- move shared structs into includes;
- move shared constants into includes;
- move sampling helpers into includes;
- move pure math helpers into includes;
- move domain-specific helper groups into includes;
- preserve kernel names, pass names, property names, and resource bindings.

Splitting one compute asset into multiple compute assets should be treated as a later, higher-risk step because it can affect kernel lookup, binding setup, dispatch sequencing, and asset loading.

### Contract Extraction

Some monoliths become safer when shared contracts are explicit.

Examples of contracts that may deserve their own small files:

- shader property IDs;
- CPU/GPU buffer layouts;
- serialized preset data;
- debug enum definitions;
- cache version constants;
- coordinate-space conventions;
- feature readiness states.

Contract extraction should be conservative. A contract file is only useful if it makes a real boundary clearer.

## Recommended Pass Structure

### Pass 0: Baseline and Guardrails

Record the current state before refactoring:

- current dirty worktree status;
- known manual validation scenes or workflows;
- expected runtime behaviour;
- relevant visual debug views;
- compile status;
- obvious performance-sensitive paths.

No behaviour should change in this pass.

### Pass 1: Research and Split Plan

Produce a concrete refactor plan with:

- ranked target modules;
- reason for each target;
- proposed split strategy;
- expected risk level;
- validation method;
- rollback notes;
- dependencies between splits.

The plan should distinguish between:

- mechanical splits that can be done immediately;
- helper extraction that needs targeted validation;
- deeper architecture work that should wait;
- files that should stay large for now.

### Pass 2: Low-Risk Mechanical Splits

Start with changes that preserve type identity and behaviour:

- partial class splits;
- editor-only partials;
- shader include extraction for pure helpers;
- small contract files for stable constants;
- no serialized field renames;
- no public API removals;
- no feature tuning.

Each change should compile independently.

### Pass 3: Long Method Reduction

After mechanical splits, reduce the worst methods and functions:

- extract named phases;
- flatten duplicated validation;
- isolate debug-only branches;
- turn repeated setup blocks into helpers;
- preserve call order.

This pass should be driven by the research findings rather than a generic line-count target.

### Pass 4: True Responsibility Extraction

Only after the earlier passes expose stable boundaries should the team extract larger collaborators.

This pass may create new classes or modules, but only where ownership is clear. It should avoid moving serialized state unless there is an approved migration path.

### Pass 5: Follow-Up Cleanup

Once the structure is stable:

- remove dead helper wrappers;
- tighten visibility;
- normalize naming;
- update architecture docs;
- add or adjust focused tests;
- update manual validation notes.

This pass should remain separate from the initial split work so that behaviour-preserving changes are easy to review.

## Validation Gates

Every refactor slice should have an explicit validation gate.

Minimum gates:

- project compiles;
- relevant scenes load;
- affected components retain serialized values;
- shader and compute assets compile;
- key runtime workflows still run;
- debug views still bind and display;
- no expected asset GUIDs changed;
- no unrelated files were reformatted.

Additional gates for higher-risk rendering work:

- shader pass output matches the baseline;
- compute kernels are found by name;
- all buffers and textures bind with the expected layout;
- representative quality tiers still run;
- resource allocation and release still work across enable/disable and domain changes.

Additional gates for editor work:

- Inspector sections still display;
- multi-object editing still behaves as expected where supported;
- undo still records changes;
- generated assets still save and reload;
- preview or diagnostic controls still refresh.

## Review Rules

The refactor should be easy to review in small pieces.

Preferred review shape:

- one responsibility per change;
- mechanical moves separated from logic edits;
- generated or formatting-only changes avoided unless necessary;
- exact behaviour changes called out explicitly;
- old and new ownership documented near the change;
- screenshots or validation notes attached for visual systems.

If a change both moves code and alters behaviour, it should be split unless the behaviour change is required for compilation.

## Risk Areas

The pass should treat the following as high risk:

- serialized field moves or renames;
- changing MonoBehaviour or ScriptableObject type identity;
- changing asset GUIDs;
- changing shader property names;
- changing compute kernel names;
- changing buffer struct layout;
- changing dispatch order;
- changing initialization timing;
- changing editor undo or asset creation paths;
- extracting code that depends on hidden side effects.

High-risk work is not forbidden, but it should require a dedicated plan and validation gate.

## Success Criteria

The refactor pass is successful when:

- the largest modules have clear ownership boundaries;
- future contributors can find the relevant responsibility quickly;
- risky contracts are explicit;
- reviews become smaller and less conflict-prone;
- long methods have been reduced where that improves clarity;
- shader and compute helper code is easier to navigate;
- public behaviour remains stable unless separately approved;
- the docs describe the new structure well enough to maintain it.

The desired outcome is not a codebase made of tiny files. The desired outcome is a codebase where each file has a reason to exist, each boundary reflects a real concept, and future feature work has somewhere obvious to go.

## Next Step

Expand this overview into a concrete research pass. That pass should inventory the actual modules, rank them by risk and value, propose candidate split boundaries, and define validation gates before implementation begins.
