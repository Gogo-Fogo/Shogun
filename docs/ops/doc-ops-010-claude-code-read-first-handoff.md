# DOC-OPS-010 Claude Code Read-First Handoff

> Purpose: give the next-stage Claude Code session a low-token, decision-complete starting point.

## Current project status

`Shogun` is currently in cleanup, staging, and active vertical-slice setup.

It is not in broad feature implementation mode.

Current repo truth:

- `Assets/_Project/Scenes/Dev/Dev_Sandbox.unity` is the only meaningful gameplay scene.
- `MainMenu`, `Battle_Prototype`, and `UI_Demo` are shell/support scenes.
- `Characters` and `Combat` are real but prototype-grade.
- `Gacha`, broader `UI`, `Networking`, and broader `Input` are planned architecture, not trustworthy implementation surfaces.
- New work should stay centered on the first authored slice: `Courtyard Ambush`.
- Slice player trio: `Ryoma`, `Kuro`, `Tsukiko`.
- Slice enemy trio: `Ronin Footman`, `Oni Brute`, `Yurei Caster`.
- Old imported-heavy art is legacy by default, not canonical production art.
- New art/source work belongs in the newer `Source / Production` lanes.

## Read these first

Read in this exact order before broad scanning:

1. [`docs/PROJECT_CONTEXT_INDEX.md`](../PROJECT_CONTEXT_INDEX.md)
2. [`doc-ops-010-claude-code-read-first-handoff.md`](./doc-ops-010-claude-code-read-first-handoff.md)
3. [`doc-ops-013-docs-routed-feature-workflow.md`](./doc-ops-013-docs-routed-feature-workflow.md)
4. [`doc-ops-008-short-term-implementation-todo.md`](./doc-ops-008-short-term-implementation-todo.md)
5. [`../design/design-008-active-vertical-slice-definition.md`](../design/design-008-active-vertical-slice-definition.md)
6. [`../design/design-009-first-vertical-slice-roster-and-encounter-plan.md`](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md)

Only widen the read set after those files if the current task truly needs more context.

## Mandatory docs-reference workflow (Claude and Codex)

For feature implementation work (not pure bug triage or tiny hotfixes):

1. Run the checklist in [`DOC-OPS-013`](./doc-ops-013-docs-routed-feature-workflow.md) before coding.
2. Confirm the source-of-truth docs before coding.
3. Keep implementation notes and final summaries doc-referenced with concrete file paths.
4. If shipped behavior changes scope, update the matching docs in the same change or leave an explicit mismatch note.

## Do not assume

- Do not assume broad system maturity.
- Do not assume shell scenes are live implementation targets.
- Do not assume imported legacy art is canonical production content.
- Do not assume broad cleanup is still the current goal.

## Model guidance

- Use **Sonnet** by default for day-to-day coding, refactors, scene/system implementation, and most repo work.
- Switch to **Opus** only for harder architecture passes, design synthesis, large reviews, or when Sonnet gets stuck on a complex cross-doc reasoning task.

## Repo rules summary

- No broad cleanup passes unless a narrow cleanup is directly required.
- No destructive deletes.
- Treat old art as legacy by default.
- Keep work centered on the active slice.
- Prefer additive or narrowly scoped changes over big rewrites.

## Operational posture

- The docs tree is the source-of-truth router.
- This handoff note is intentionally thin and should reduce token waste.
- If the task is active implementation, prefer the short-term TODO and slice docs before reading broader roadmap material.
