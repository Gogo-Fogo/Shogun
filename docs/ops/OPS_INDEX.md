# Operations Documentation Index

> Purpose: route repo workflow, AI tooling, Unity MCP, backup safety, artifact-inventory, and operational retrospective questions.

## Use this folder when the question is:

- how Codex, Claude, or Unity MCP should be configured and used
- how repo safety, backup cadence, or recovery workflow should work
- whether PixelLab belongs in the production toolchain
- what implementation artifacts or generated deliverables should exist
- what changed in a tooling or repo-modernization batch and why

## Document order

1. [`doc-ops-002-unity-mcp-bridge-setup-and-usage.md`](./doc-ops-002-unity-mcp-bridge-setup-and-usage.md)
   - Unity MCP bridge setup, live-editor access, and safe usage rules
2. [`doc-ops-003-ai-workspace-safety-and-backup-plan.md`](./doc-ops-003-ai-workspace-safety-and-backup-plan.md)
   - repo-scope rules, backup cadence, restore drill, and off-site backup workflow
3. [`doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md`](./doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md)
   - PixelLab fit, subscription vs API, and AI-assisted sprite workflow boundaries
4. [`doc-ops-001-project-document-and-telemetry-index.md`](./doc-ops-001-project-document-and-telemetry-index.md)
   - implementation artifact inventory and output manifest
5. [`doc-ops-005-march-2026-repo-modernization-retrospective.md`](./doc-ops-005-march-2026-repo-modernization-retrospective.md)
   - retrospective explanation for the March 2026 repo/tooling cleanup batch

## Companion docs outside this folder

- [`../design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](../design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
  - use for product intent before changing tools or workflows that affect scope
- [`../legal/LEGAL_INDEX.md`](../legal/LEGAL_INDEX.md)
  - use when tooling questions become legal, privacy, or policy questions
- [`../research/RESEARCH_INDEX.md`](../research/RESEARCH_INDEX.md)
  - use when the question shifts from operations to engineering or comparative research
- [`../art/ART_INDEX.md`](../art/ART_INDEX.md)
  - use when the question becomes art-production specific rather than tool/process specific

## Default order for ops/tooling questions

1. `DOC-OPS-002`
2. `DOC-OPS-003`
3. `DOC-OPS-004`
4. `DOC-OPS-001`
5. `DOC-OPS-005`
6. `DOC-LEGAL-001`

## Rule for future updates

Add new ops docs here when:

- a tooling workflow becomes stable enough to enforce
- a repo-safety or backup procedure materially changes
- a new operational retrospective or inventory note is needed
