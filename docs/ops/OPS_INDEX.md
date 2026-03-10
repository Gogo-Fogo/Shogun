# Operations Documentation Index

> Purpose: route repo workflow, AI tooling, Unity MCP, backup safety, artifact-inventory, and operational retrospective questions.

## Use this folder when the question is:

- how Codex, Claude, or Unity MCP should be configured and used
- how repo safety, backup cadence, or recovery workflow should work
- how Unity editor baseline and upgrade policy should be handled for this repo
- what the current Unity project actually contains versus what the docs imply
- whether PixelLab belongs in the production toolchain
- what implementation artifacts or generated deliverables should exist
- what changed in a tooling or repo-modernization batch and why
- how Claude Code should start without scanning the entire docs tree

## Lean handoff pack

Start here for next-stage implementation or Claude Code onboarding:

1. [`doc-ops-010-claude-code-read-first-handoff.md`](./doc-ops-010-claude-code-read-first-handoff.md)
   - low-token Claude entrypoint, read order, and repo assumptions
2. [`doc-ops-008-short-term-implementation-todo.md`](./doc-ops-008-short-term-implementation-todo.md)
   - strict near-term backlog for the active vertical slice only
3. [`doc-ops-011-ui-implementation-todo.md`](./doc-ops-011-ui-implementation-todo.md)
   - UI-specific slice backlog for battle readability, mobile portrait layout, and result-state clarity
4. [`../design/design-008-active-vertical-slice-definition.md`](../design/design-008-active-vertical-slice-definition.md)
   - what the current implementation slice must prove
5. [`../design/design-009-first-vertical-slice-roster-and-encounter-plan.md`](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md)
   - the first trustworthy team/enemy/encounter target
6. [`doc-ops-009-long-term-roadmap-todo.md`](./doc-ops-009-long-term-roadmap-todo.md)
   - deferred roadmap so the slice backlog stays clean

## Document order

1. [`doc-ops-010-claude-code-read-first-handoff.md`](./doc-ops-010-claude-code-read-first-handoff.md)
   - low-token entrypoint for Claude Code and next-stage implementation work
2. [`doc-ops-008-short-term-implementation-todo.md`](./doc-ops-008-short-term-implementation-todo.md)
   - strict near-term vertical-slice backlog
3. [`doc-ops-011-ui-implementation-todo.md`](./doc-ops-011-ui-implementation-todo.md)
   - UI-specific implementation backlog for the active slice
4. [`doc-ops-009-long-term-roadmap-todo.md`](./doc-ops-009-long-term-roadmap-todo.md)
   - deferred roadmap lanes kept out of the active slice backlog
5. [`doc-ops-002-unity-mcp-bridge-setup-and-usage.md`](./doc-ops-002-unity-mcp-bridge-setup-and-usage.md)
   - Unity MCP bridge setup, live-editor access, and safe usage rules
6. [`doc-ops-003-ai-workspace-safety-and-backup-plan.md`](./doc-ops-003-ai-workspace-safety-and-backup-plan.md)
   - repo-scope rules, backup cadence, restore drill, and off-site backup workflow
7. [`doc-ops-006-unity-editor-version-policy-and-upgrade-checklist.md`](./doc-ops-006-unity-editor-version-policy-and-upgrade-checklist.md)
   - project-specific Unity editor baseline, stable upgrade target, and safe upgrade checklist
8. [`doc-ops-007-stage-1-unity-project-reality-audit.md`](./doc-ops-007-stage-1-unity-project-reality-audit.md)
   - stage-1 reality check for what the current Unity project actually is, what is salvageable, and what needs rebuilding
9. [`doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md`](./doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md)
   - PixelLab fit, subscription vs API, and AI-assisted sprite workflow boundaries
10. [`doc-ops-001-project-document-and-telemetry-index.md`](./doc-ops-001-project-document-and-telemetry-index.md)
   - implementation artifact inventory and output manifest
11. [`doc-ops-005-march-2026-repo-modernization-retrospective.md`](./doc-ops-005-march-2026-repo-modernization-retrospective.md)
   - retrospective explanation for the March 2026 repo/tooling cleanup batch
12. [`doc-ops-012-2026-03-10-combat-ui-and-auto-positioning-retrospective.md`](./doc-ops-012-2026-03-10-combat-ui-and-auto-positioning-retrospective.md)
   - retrospective explanation for the March 10, 2026 combat HUD and auto-positioning batch

## Companion docs outside this folder

- [`../design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](../design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
  - use for product intent before changing tools or workflows that affect scope
- [`../legal/LEGAL_INDEX.md`](../legal/LEGAL_INDEX.md)
  - use when tooling questions become legal, privacy, or policy questions
- [`../research/RESEARCH_INDEX.md`](../research/RESEARCH_INDEX.md)
  - use when the question shifts from operations to engineering or comparative research
- [`../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md`](../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md)
  - use when the question is a broad production recommendation for new Unity versions rather than the repo-specific upgrade rule
- [`../art/ART_INDEX.md`](../art/ART_INDEX.md)
  - use when the question becomes art-production specific rather than tool/process specific

## Default order for ops/tooling questions

1. `DOC-OPS-010`
2. `DOC-OPS-008`
3. `DOC-OPS-011`
4. `DOC-OPS-009`
5. `DOC-OPS-002`
6. `DOC-OPS-003`
7. `DOC-OPS-006`
8. `DOC-OPS-007`
9. `DOC-OPS-004`
10. `DOC-OPS-001`
11. `DOC-OPS-005`
12. `DOC-OPS-012`
13. `DOC-LEGAL-001`

## Rule for future updates

Add new ops docs here when:

- a tooling workflow becomes stable enough to enforce
- a repo-safety or backup procedure materially changes
- the Unity editor baseline or upgrade policy materially changes
- a handoff/startup flow becomes stable enough to recommend before broad scanning
- a new operational retrospective or inventory note is needed
- a UI implementation backlog becomes stable enough to route active-slice work
