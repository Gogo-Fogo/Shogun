# Project Context Index

> Purpose: this is the AI routing layer for the `docs` Shogun project documents. Read this file first, then open the smallest relevant subset of docs.

## Folder layout

```text
docs/
  PROJECT_CONTEXT_INDEX.md
  README.md
  ai/
    doc-data-001-unity-mobile-gacha-rpg-data-collection-and-compliance.md
    doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md
    doc-eng-002-unity-project-runtime-architecture-patterns.md
    doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md
    doc-legal-001-ai-generated-assets-legal-considerations.md
    doc-legal-002-ai-assets-legal-and-compliance-roadmap.md
    doc-legal-003-tos-eula-and-user-facing-legal-documents.md
    doc-ops-001-project-document-and-telemetry-index.md
    doc-ops-002-unity-mcp-bridge-setup-and-usage.md
    doc-ops-003-ai-workspace-safety-and-backup-plan.md
    doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md
    doc-ops-005-march-2026-repo-modernization-retrospective.md
    doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md
    doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md
    doc-ref-003-one-piece-treasure-cruise-analysis.md
    doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md
    doc-ref-004-fire-emblem-heroes-success-and-drawbacks.docx
  design/
    DESIGN_INDEX.md
    design-001-character-collection-and-fantasy-strategy.md
    design-002-world-pillars-and-combat-identity-framework.md
  art/
    ART_INDEX.md
    art-001-style-bible-and-visual-targets.md
    art-002-sprite-production-pipeline.md
    art-003-unity-2d-import-and-animation-standards.md
    art-004-asset-provenance-and-source-tracking.md
    art-006-sex-appeal-and-damage-art-policy.md
  recruiter/
    doc-data-001-unity-mobile-gacha-rpg-data-collection-and-compliance.pdf
    doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.pdf
    doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.pdf
    doc-legal-001-ai-generated-assets-legal-considerations.pdf
    doc-legal-002-ai-assets-legal-and-compliance-roadmap.pdf
    doc-legal-003-tos-eula-and-user-facing-legal-documents.pdf
    doc-ops-001-project-document-and-telemetry-index.pdf
    doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.pdf
    doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.pdf
    doc-ref-003-one-piece-treasure-cruise-analysis.pdf
```

## Default operating rule

Do not scan the whole folder by default.

For most tasks:

1. Start with this index.
2. Prefer `ai/` documents first unless the task is specifically roster/world design, art direction, sprite production, Unity import standards, or asset provenance.
3. Open the matching PDF in `recruiter/` only if formatting, page fidelity, or missing detail matters.

## Canonical document catalog

| ID | Topic | Primary AI path | Recruiter PDF path | Authority | Notes |
| --- | --- | --- | --- | --- | --- |
| `DOC-GDD-001` | Shogun product design | `ai/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md` | `recruiter/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.pdf` | Primary design truth | Use first for gameplay, UX, progression, monetization, and product scope. |
| `DOC-ENG-001` | Unity engineering and production architecture | `ai/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md` | `recruiter/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.pdf` | Primary engineering truth | Use first for backend, Unity structure, build pipeline, delivery, and scale-path decisions. |
| `DOC-ENG-002` | Unity runtime architecture patterns | `ai/doc-eng-002-unity-project-runtime-architecture-patterns.md` | No recruiter PDF in this folder | Supporting engineering authority | Use for client-side code boundaries, state flow, event channels, gesture input flow, and logic/view separation. |
| `DOC-DATA-001` | Telemetry, analytics, and compliance-aware data collection | `ai/doc-data-001-unity-mobile-gacha-rpg-data-collection-and-compliance.md` | `recruiter/doc-data-001-unity-mobile-gacha-rpg-data-collection-and-compliance.pdf` | Primary telemetry truth | Use first for events, schemas, batching, retention, consent, audit logging. |
| `DOC-OPS-001` | Implementation artifact inventory | `ai/doc-ops-001-project-document-and-telemetry-index.md` | `recruiter/doc-ops-001-project-document-and-telemetry-index.pdf` | Operational manifest | Use for file outputs, naming, CI artifacts, telemetry deliverables, and legal-document inventory. |
| `DOC-OPS-002` | Unity MCP bridge setup and operating rules | `ai/doc-ops-002-unity-mcp-bridge-setup-and-usage.md` | No recruiter PDF in this folder | Operational workflow note | Use for Codex/Claude live Unity-editor access, local MCP setup, safe usage boundaries, and exporter vs. MCP guidance. |
| `DOC-OPS-003` | AI workspace safety and backup plan | `ai/doc-ops-003-ai-workspace-safety-and-backup-plan.md` | No recruiter PDF in this folder | Operational safety note | Use for repo scope rules, Codex/Claude safety posture, backup layers, and recovery workflow after accidental deletion. |
| `DOC-OPS-004` | PixelLab evaluation and sprite production workflow | `ai/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md` | No recruiter PDF in this folder | Operational art-tooling note | Use for PixelLab fit, subscription vs API choice, sprite workflow, animation-size guidance, and AI-assisted art production boundaries. |
| `DOC-OPS-005` | March 2026 repo modernization retrospective | `ai/doc-ops-005-march-2026-repo-modernization-retrospective.md` | No recruiter PDF in this folder | Operational change log | Use when the question is what changed in the March 7, 2026 cleanup/tooling batch and why those pushed commits were structured that way. |
| `DESIGN-001` | Character collection and fantasy strategy | `design/design-001-character-collection-and-fantasy-strategy.md` | No recruiter PDF in this folder | Primary collection-strategy note | Use for collectible fantasy, roster pillars, battle-vs-presentation art lanes, and variant planning. |
| `DESIGN-002` | World pillars and combat identity framework | `design/design-002-world-pillars-and-combat-identity-framework.md` | No recruiter PDF in this folder | Primary roster-identity note | Use for world pillars, elemental affinity, weapon families, martial schools, and how those layers stack into a character identity. |
| `ART-001` | Style bible and visual targets | `art/art-001-style-bible-and-visual-targets.md` | No recruiter PDF in this folder | Primary art-direction note | Use first for silhouette rules, palette direction, detail limits, and gameplay readability targets. |
| `ART-002` | Sprite production pipeline | `art/art-002-sprite-production-pipeline.md` | No recruiter PDF in this folder | Primary art workflow note | Use for the bounded PixelLab-to-Aseprite-to-Unity production flow and trial/pass-fail rules. |
| `ART-006` | Sex appeal and damage-art policy | `art/art-006-sex-appeal-and-damage-art-policy.md` | No recruiter PDF in this folder | Primary fanservice boundary note | Use for sex-appeal limits, FEH-style damage-art interpretation, and what battle animation fanservice is too risky. |
| `ART-003` | Unity 2D import and animation standards | `art/art-003-unity-2d-import-and-animation-standards.md` | No recruiter PDF in this folder | Primary import standard | Use for `.aseprite` import, animation tags, frame-budget targets, and gameplay-facing sprite standards. |
| `ART-004` | Asset provenance and source tracking | `art/art-004-asset-provenance-and-source-tracking.md` | No recruiter PDF in this folder | Primary provenance note | Use for generated-art traceability, manual-edit tracking, and shipped-asset metadata requirements. |
| `DOC-LEGAL-001` | Legal and platform-policy risk overview | `ai/doc-legal-001-ai-generated-assets-legal-considerations.md` | `recruiter/doc-legal-001-ai-generated-assets-legal-considerations.pdf` | Primary legal risk briefing | Use for AI asset usage, privacy, loot box compliance, store-policy risk, and IP questions. |
| `DOC-LEGAL-002` | Legal backlog and next-step roadmap | `ai/doc-legal-002-ai-assets-legal-and-compliance-roadmap.md` | `recruiter/doc-legal-002-ai-assets-legal-and-compliance-roadmap.pdf` | Legal roadmap | Use when prioritizing follow-up research, counsel review, and unresolved compliance work. |
| `DOC-LEGAL-003` | ToS, EULA, and user-facing legal drafting | `ai/doc-legal-003-tos-eula-and-user-facing-legal-documents.md` | `recruiter/doc-legal-003-tos-eula-and-user-facing-legal-documents.pdf` | Primary legal drafting guide | Use first when drafting or revising terms, EULA structure, clickwrap, and acceptance logging. |
| `DOC-REF-001` | Naruto Blazing mechanics inspiration study | `ai/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md` | `recruiter/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.pdf` | Inspiration only | Use for combat and pacing patterns. Do not copy expressive IP. |
| `DOC-REF-002` | Naruto Blazing success, failure, and shutdown analysis | `ai/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md` | `recruiter/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.pdf` | Strategic caution reference | Use for live-ops, retention, monetization, and shutdown-risk lessons. |
| `DOC-REF-003` | One Piece Treasure Cruise longevity analysis | `ai/doc-ref-003-one-piece-treasure-cruise-analysis.md` | `recruiter/doc-ref-003-one-piece-treasure-cruise-analysis.pdf` | Strategic caution reference | Use for long-term sustainability, onboarding, technical debt, and content burden. |
| `DOC-REF-004` | Fire Emblem Heroes comparison source | `ai/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md` | No recruiter PDF in this folder | Secondary source | Open the retained `.docx` only if the original Word source is needed. |

## Source-of-truth hierarchy

### Product and feature intent
1. `DOC-GDD-001`
2. `DOC-ENG-001`, `DOC-ENG-002`, and `DOC-DATA-001` for implementation detail
3. `DOC-REF-001` to `DOC-REF-004` for inspiration and caution only

### Unity engineering, backend, build pipeline
1. `DOC-ENG-001`
2. `DOC-ENG-002`
3. `DOC-OPS-002` when live Unity-editor access or MCP workflow matters
4. `DOC-DATA-001`
5. `DOC-GDD-001` for feature intent only

### Telemetry, analytics, and data collection
1. `DOC-DATA-001`
2. `DOC-OPS-001`
3. `DOC-LEGAL-001`
4. `DOC-GDD-001`

### AI tooling, Unity editor introspection, and MCP workflow
1. `DOC-OPS-002`
2. `DOC-OPS-003`
3. `DOC-OPS-005`
4. `DOC-OPS-004`
5. `DOC-ENG-002`
6. `DOC-LEGAL-001`
7. `DOC-LEGAL-002`

### AI workspace safety, repo scope, and backup recovery
1. `DOC-OPS-003`
2. `DOC-OPS-005`
3. `DOC-OPS-002`
4. `DOC-LEGAL-001`

### AI-assisted sprite production and PixelLab workflow
1. `ART-001`
2. `DESIGN-001`
3. `DESIGN-002`
4. `ART-002`
5. `ART-006`
6. `ART-003`
7. `ART-004`
8. `DOC-OPS-004`
9. `DOC-LEGAL-001`
10. `DOC-LEGAL-002`
11. `DOC-OPS-002` if MCP integration matters

### Art direction, sprite import, and provenance
1. `ART-001`
2. `DESIGN-001`
3. `DESIGN-002`
4. `ART-006`
5. `ART-002`
6. `ART-003`
7. `ART-004`
8. `DOC-OPS-004`
9. `DOC-LEGAL-001`

### Legal, compliance, and AI-asset risk
1. `DOC-LEGAL-001`
2. `DOC-LEGAL-003`
3. `DOC-LEGAL-002`

### ToS, EULA, and acceptance flows
1. `DOC-LEGAL-003`
2. `DOC-OPS-001`
3. `DOC-LEGAL-001`

### Comparative lessons and market framing
1. `DOC-REF-001`
2. `DOC-REF-002`
3. `DOC-REF-003`
4. `DOC-REF-004`

## Conflict rules

1. Legal constraints override design wishes.
2. `DOC-GDD-001` defines what the game is.
3. Engineering and data docs define how to build and observe it.
4. Reference docs are for patterns and warnings, not copying.
5. `DOC-OPS-001` is the file-output manifest when the question is “what artifact should exist?”

## Fast router

### Combat, movement, encounter, and progression design
Open:
1. `DOC-GDD-001`
2. `DOC-REF-001`
3. optionally `DOC-REF-002`

### Unity structure, backend, live-service architecture
Open:
1. `DOC-ENG-001`
2. `DOC-ENG-002`
3. `DOC-OPS-002` if live editor state or MCP access matters
4. `DOC-GDD-001`

### AI tooling, Unity MCP, and live editor access
Open:
1. `DOC-OPS-002`
2. `DOC-OPS-003`
3. `DOC-OPS-005`
4. `DOC-OPS-004`
5. `DOC-ENG-002`
6. `DOC-LEGAL-001`

### AI safety, repo boundaries, and backup recovery
Open:
1. `DOC-OPS-003`
2. `DOC-OPS-005`
3. `DOC-OPS-002`
4. `DOC-LEGAL-001`

### Pixel art workflow, PixelLab, and AI-generated sprite production
Open:
1. `ART-001`
2. `DESIGN-001`
3. `DESIGN-002`
4. `ART-006`
5. `ART-002`
6. `ART-003`
7. `ART-004`
8. `DOC-OPS-004`
9. `DOC-LEGAL-001`
10. `DOC-LEGAL-002`
11. `DOC-OPS-002` if MCP integration or coding-assistant setup matters

### World pillars, elements, martial schools, and roster identity
Open:
1. `DESIGN-001`
2. `DESIGN-002`
3. `ART-001`
4. `DOC-GDD-001`
5. `DOC-REF-001`

### Telemetry, event schemas, consent, and retention
Open:
1. `DOC-DATA-001`
2. `DOC-OPS-001`
3. `DOC-LEGAL-001` if privacy or minors are involved

### Legal text, ToS, EULA, and clickwrap
Open:
1. `DOC-LEGAL-003`
2. `DOC-LEGAL-001`
3. `DOC-OPS-001`

### Monetization, gacha fairness, and live-ops risk
Open:
1. `DOC-GDD-001`
2. `DOC-LEGAL-001`
3. `DOC-REF-002`
4. optionally `DOC-REF-003`

## AI behavior rules

- Prefer `ai/` documents for retrieval speed.
- Prefer `design/` documents when the task is roster identity, world pillars, combat identity, or collection planning.
- Prefer `art/` documents when the task is specifically production-facing art direction, sprite workflow, import rules, or provenance.
- Use PDFs in `recruiter/` when layout fidelity or exact document presentation matters.
- Treat Shogun’s own docs as authoritative; treat comparative docs as inspiration and warnings only.
- Do not copy franchise names, characters, or protected expression from reference docs.
- When documents conflict, follow the hierarchy above and report the conflict explicitly.

## Change rules

Update this file whenever:

- filenames change
- a new canonical document is added
- a Markdown companion is added or removed
- a document becomes superseded
- folder structure changes

## Short prompt for future AI agents

```text
Read PROJECT_CONTEXT_INDEX.md first.
Do not scan the entire folder by default.
Prefer ai/ Markdown companions first, then open recruiter/ PDFs only when needed.
Use DOC-GDD-001 as primary design truth, DOC-ENG-001 and DOC-ENG-002 plus DOC-DATA-001 as implementation truth, DOC-LEGAL-* docs as compliance constraints, and DOC-REF-* docs as inspiration/caution only.
If live Unity editor state or AI tooling workflow matters, open DOC-OPS-002.
If the task is roster identity or world design, open DESIGN_INDEX.md first.
If the task is art direction or sprite production, open ART_INDEX.md first, then the linked DESIGN-* docs if needed, then DOC-OPS-004.
If documents conflict, follow the hierarchy in PROJECT_CONTEXT_INDEX.md and report the conflict explicitly.
```
