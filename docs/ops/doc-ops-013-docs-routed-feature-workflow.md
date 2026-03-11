# DOC-OPS-013 Docs-Routed Feature Workflow

> Status date: March 10, 2026  
> Purpose: enforce a consistent docs-first implementation workflow for feature and behavior changes in `Shogun`.

## Why this exists

`Shogun` has high design and ops surface area relative to current live code scope.

Without a strict docs-first gate, implementation can drift from slice scope, miss key constraints, or ship behavior that no longer matches the written source of truth.

This policy is the enforcement layer for feature work.

## When this rule applies

Apply this workflow for:

- new features
- behavior changes
- architecture changes
- cross-system refactors
- scope-affecting UI or combat changes

## Limited exceptions

You may skip the full protocol only for narrow non-behavioral fixes, such as:

- typo-only edits
- no-behavior null checks
- compile-error cleanup that does not alter feature behavior

If behavior changes in practice, this exception no longer applies.

## Required read set before coding

Read in this order before implementation:

1. [`docs/PROJECT_CONTEXT_INDEX.md`](../PROJECT_CONTEXT_INDEX.md)
2. [`DOC-OPS-010`](./doc-ops-010-claude-code-read-first-handoff.md)
3. [`DOC-OPS-008`](./doc-ops-008-short-term-implementation-todo.md)
4. [`DESIGN-008`](../design/design-008-active-vertical-slice-definition.md)
5. [`DESIGN-009`](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md)

Add task-specific docs only after this minimum set.

For UI-heavy work, also include:

- [`DOC-OPS-011`](./doc-ops-011-ui-implementation-todo.md)
- [`DESIGN-010`](../design/design-010-combat-hud-and-battle-ui-specification.md)

If the task appears out of active-slice scope, check:

- [`DOC-OPS-009`](./doc-ops-009-long-term-roadmap-todo.md)

## Pre-code checklist

Before editing code, explicitly confirm:

1. The task is in-slice or intentionally deferred.
2. The docs that constrain the task are identified.
3. The non-goals are identified so scope does not drift.

## During-implementation checklist

While editing:

1. Keep changes aligned with the active slice and encounter plan.
2. Follow the source-of-truth hierarchy in `PROJECT_CONTEXT_INDEX.md` when docs conflict.
3. Track which docs must be updated if behavior or scope changes.

## Pre-ship checklist

Before finalizing:

1. Include concrete doc file-path citations in the implementation summary.
2. If behavior or scope changed, update the corresponding docs in the same change.
3. If docs cannot be updated in the same change, add an explicit mismatch note.

## Required mismatch-note format

Use this exact structure in the most relevant ops/design doc when needed:

```text
Mismatch note
- Date:
- Code change:
- Affected docs:
- Why docs were not updated in this change:
- Follow-up owner:
- Follow-up target date:
```

## Enforcement rule

Feature implementation is not complete unless one of the following is true:

- doc citations are included and docs are aligned
- doc citations are included and a mismatch note is recorded

## Relationship to other docs

- `DOC-OPS-010` is the lean handoff entrypoint.
- `DOC-OPS-013` is the enforcement workflow.
- `DOC-OPS-008` and `DOC-OPS-011` define active implementation scope.
- `DESIGN-008` and `DESIGN-009` define the current slice target.
