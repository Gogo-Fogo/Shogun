# Shogun Agent Rules

## Scope

- Treat `G:\Workspace\Unity\Projects\Shogun` as the maximum allowed write scope.
- Do not create, modify, move, or delete anything outside this repository unless the user explicitly asks for that exact path.
- Never run commands against drive roots such as `G:\`, `C:\`, or `G:\Workspace`.

## Destructive Operations

- Never run recursive delete commands outside a clearly named repo-local target.
- Never run `git clean`, `git reset --hard`, `Remove-Item -Recurse -Force`, `del /s`, `rmdir /s`, or similar destructive commands unless the user explicitly requests them.
- If a task appears to require deleting many files, ask first and name the exact folder that would be affected.

## Backup Safety

- Before large refactors, reorganizations, or generated bulk edits, recommend or create a commit first when the repo is in a reasonable state.
- Prefer additive changes and targeted edits over broad file rewrites.
- Treat generated folders like `/_Generated/` as disposable, but still keep deletions scoped to the exact folder requested.

## Unity

- Do not delete scenes, prefabs, ScriptableObjects, or project settings as part of cleanup unless the user explicitly names them.
- Prefer validation, inspection, and narrow fixes over broad automated cleanup.


## Documentation Routing (Codex and Claude)

- For new features, behavior changes, or architecture changes, start with `docs/PROJECT_CONTEXT_INDEX.md` and read the smallest relevant subset before implementation.
- For active implementation work, route through `docs/ops/doc-ops-010-claude-code-read-first-handoff.md`, `docs/ops/doc-ops-008-short-term-implementation-todo.md`, `docs/design/design-008-active-vertical-slice-definition.md`, and `docs/design/design-009-first-vertical-slice-roster-and-encounter-plan.md` unless the user explicitly requests out-of-slice roadmap work.
- In implementation summaries, cite the docs that drove decisions using concrete file references.
- If implementation decisions materially change behavior or scope, update the relevant docs in the same change or explicitly record the mismatch.
