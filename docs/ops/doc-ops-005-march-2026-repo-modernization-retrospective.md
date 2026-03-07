# March 2026 Repo Modernization Retrospective

**Summary:** This note retroactively documents the intent behind the March 7, 2026 repository modernization, safety/tooling rollout, and immediate art-strategy follow-up. It exists because several pushed commits landed with short subjects only and no explanatory body.

## Purpose

Use this document when the question is:

- what changed during the March 7, 2026 cleanup and tooling pass
- what changed in the immediate March 7 art-strategy follow-up
- why those commits were separated the way they were
- which changes were infrastructure/safety work versus Unity project alignment
- where to look for the resulting canonical workflows

## Why this note exists

The following commits were pushed to `main` with short one-line subjects and without a descriptive body:

- `60454b4` `docs: consolidate canonical project guidance`
- `64a7957` `feat: add unity mcp integration and diagnostics`
- `e293790` `refactor: modernize project export workflow`
- `712cab6` `fix: repair urp default renderer configuration`
- `a23a7b4` `docs: add character collection and fantasy strategy`

Rather than rewriting already-pushed `main` history, this retrospective records the missing rationale in one canonical place.

Later follow-up commits in the same period *do* contain bodies:

- `11b6b75` `docs: finalize AI safety workflow and backup tooling`
- `a9843c3` `fix: align project settings with Unity 6000.0 baseline`

## Batch overview

The March 7 work had six distinct goals:

1. clean up and canonicalize project documentation
2. add live Unity-editor access for Codex and Claude through MCP
3. demote the old exporter from primary workflow to fallback snapshot tool
4. repair the broken URP default-renderer state in the repo
5. harden AI workspace safety and add repo-local backup tooling
6. align committed Unity metadata with the actual supported Unity `6000.0.69f1` baseline

The immediate follow-up work added a seventh goal:

7. turn the PixelLab and gacha-art discussion into a production-facing character-collection strategy instead of leaving it as chat-only advice

## Commit-by-commit rationale

### `60454b4` `docs: consolidate canonical project guidance`

This commit normalized the project documentation layout so the repo had one canonical `docs/` tree instead of a split/legacy structure.

Main outcomes:

- flattened the canonical docs into `docs/`
- removed `docs/old` as a live source of truth
- updated the routing layer in [`PROJECT_CONTEXT_INDEX.md`](../PROJECT_CONTEXT_INDEX.md)
- updated the repo-level [`README.md`](../../README.md) to point at canonical docs instead of stale legacy paths
- added repo guidance files like [`AGENTS.md`](../../AGENTS.md), [`CLAUDE.md`](../../CLAUDE.md), and [`.claude/settings.json`](../../.claude/settings.json)

Why it was separate:

- this was repository information architecture work, not engine/tooling work
- keeping it separate made it easy to review the documentation and guardrail changes without mixing them with Unity asset churn

## `64a7957` `feat: add unity mcp integration and diagnostics`

This commit introduced the live Unity MCP workflow so Codex and Claude could inspect the running Unity editor instead of relying only on static exports.

Main outcomes:

- added repo MCP config through [`.mcp.json`](../../.mcp.json)
- added a stable launcher at [`tools/mcp-unity/launch-mcp-unity.cjs`](../../tools/mcp-unity/launch-mcp-unity.cjs)
- added Unity-side diagnostic/editor commands in [`ShogunMcpDiagnosticTools.cs`](../../Assets/_Project/Scripts/Editor/ShogunMcpDiagnosticTools.cs)
- updated package manifests for the Unity MCP bridge
- documented the workflow in [`doc-ops-002-unity-mcp-bridge-setup-and-usage.md`](./doc-ops-002-unity-mcp-bridge-setup-and-usage.md)

Why it was separate:

- MCP is a meaningful workflow boundary by itself
- it touched both repo tooling and Unity editor code, so isolating it made later debugging and review easier

## `e293790` `refactor: modernize project export workflow`

This commit changed the old exporter from a destructive one-shot dump into a fallback snapshot tool.

Main outcomes:

- moved the export output model to timestamped snapshots under `/_Generated/ProjectExport/`
- split the workflow into smaller export actions instead of one broad destructive action
- positioned MCP as the primary live-editor workflow
- kept the exporter for offline snapshots, handoff artifacts, and audit-friendly captures

Primary file:

- [`ProjectExportTool.cs`](../../Assets/_Project/Scripts/Core/Editor/ProjectExportTool.cs)

Why it was separate:

- exporter changes are operational workflow changes, not gameplay/system changes
- they needed independent review because they affected how AI context is gathered from Unity

## `712cab6` `fix: repair urp default renderer configuration`

This commit fixed a broken URP default-renderer state that was spamming Unity errors and interfering with normal editor use.

Main outcomes:

- repaired the URP asset configuration
- ensured a valid default renderer asset existed
- updated graphics/project settings to point at the correct URP asset
- paired the fix with validation/repair commands exposed through the Unity MCP diagnostics

Key files:

- [`UniversalRP.asset`](../../Assets/Settings/UniversalRP.asset)
- [`Renderer2D_Repaired.asset`](../../Assets/Settings/Renderer2D_Repaired.asset)
- [`GraphicsSettings.asset`](../../ProjectSettings/GraphicsSettings.asset)

Why it was separate:

- this was a concrete engine-state fix, not just documentation or tooling
- isolating it made rollback or review much easier if the repair caused regressions

## `11b6b75` `docs: finalize AI safety workflow and backup tooling`

This follow-up commit documented the final safety posture after the earlier work was complete.

Main outcomes:

- recorded the final Codex sandbox posture
- documented the point-in-time safety branch
- added the repo-local backup helper

Key files:

- [`doc-ops-003-ai-workspace-safety-and-backup-plan.md`](./doc-ops-003-ai-workspace-safety-and-backup-plan.md)
- [`TODO_AI_SAFETY_AND_BACKUP.md`](../../TODO_AI_SAFETY_AND_BACKUP.md)
- [`Backup-Shogun.ps1`](../../tools/backup/Backup-Shogun.ps1)

## `a9843c3` `fix: align project settings with Unity 6000.0 baseline`

This follow-up commit cleaned up the remaining local Unity/editor churn after review and promoted it to canonical history because it matched the actual supported baseline already referenced in the docs and MCP notes.

Main outcomes:

- set the committed project version to Unity `6000.0.69f1`
- kept URP project settings aligned with that baseline
- committed the matching importer metadata cleanup so the working tree stayed clean after opening the project in the supported editor version

## `a23a7b4` `docs: add character collection and fantasy strategy`

This follow-up commit translated the PixelLab, gacha-art, and collection-value discussion into a stable production note under `docs/art/` rather than leaving the reasoning trapped in chat history.

Main outcomes:

- added what is now [`design-001-character-collection-and-fantasy-strategy.md`](../design/design-001-character-collection-and-fantasy-strategy.md)
- formalized the split between gameplay-facing battle art and collection-facing presentation art
- documented roster-pillar planning, collectible fantasy, and variant potential as explicit design rules
- updated the art router in [`ART_INDEX.md`](../art/ART_INDEX.md)
- updated the global router in [`PROJECT_CONTEXT_INDEX.md`](../PROJECT_CONTEXT_INDEX.md)
- updated the repo-level [`README.md`](../../README.md) so the new strategy note is discoverable

Why it was separate:

- this was not a minor addition to the existing PixelLab note
- it changed how roster planning should be structured at the art/design level
- keeping it separate made the distinction clear between tool evaluation and long-term character strategy

## Resulting operating model

After this batch, the intended workflow is:

- canonical documentation under `docs/`
- live Unity inspection through MCP first
- exporter used only as fallback snapshot tooling
- repo-scoped AI safety rules committed in the repo
- local backup helper plus GitHub plus off-site/external backup strategy
- Unity baseline treated as `6000.0.69f1` unless deliberately upgraded
- character production planned through two lanes: battle readability and collection-facing presentation

## Related documents

- [`doc-ops-002-unity-mcp-bridge-setup-and-usage.md`](./doc-ops-002-unity-mcp-bridge-setup-and-usage.md)
- [`doc-ops-003-ai-workspace-safety-and-backup-plan.md`](./doc-ops-003-ai-workspace-safety-and-backup-plan.md)
- [`doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md`](./doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md)
- [`ART_INDEX.md`](../art/ART_INDEX.md)
- [`design-001-character-collection-and-fantasy-strategy.md`](../design/design-001-character-collection-and-fantasy-strategy.md)
- [`PROJECT_CONTEXT_INDEX.md`](../PROJECT_CONTEXT_INDEX.md)
- [`README.md`](../../README.md)

## Decision for future batches

For future pushes, prefer one of these:

- descriptive commit body at commit time, or
- a same-day retrospective note like this one if history has already been pushed and should not be rewritten

Do not force-push `main` just to improve old commit descriptions unless there is a much stronger reason than documentation quality alone.
