# Project Context Index

> Purpose: this is the AI routing layer for the `docs` Shogun project documents. Read this file first, then open the smallest relevant subset of docs.

## Folder layout

```text
docs/
  PROJECT_CONTEXT_INDEX.md
  README.md
  design/
    DESIGN_INDEX.md
    doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md
    design-001-character-collection-and-fantasy-strategy.md
    design-002-world-pillars-and-combat-identity-framework.md
    design-003-long-term-balance-and-power-creep-policy.md
    design-004-roguelite-replayability-and-run-mode-framework.md
    design-005-co-op-pvp-and-social-systems-roadmap.md
    design-006-mobile-platform-display-and-performance-strategy.md
    design-007-range-circles-and-threat-geometry-framework.md
    design-008-active-vertical-slice-definition.md
    design-009-first-vertical-slice-roster-and-encounter-plan.md
  art/
    ART_INDEX.md
    art-001-style-bible-and-visual-targets.md
    art-002-sprite-production-pipeline.md
    art-003-unity-2d-import-and-animation-standards.md
    art-004-asset-provenance-and-source-tracking.md
    art-005-legacy-and-production-asset-separation-policy.md
    art-006-sex-appeal-and-damage-art-policy.md
    art-007-violence-and-injury-policy.md
    art-008-roguelite-event-vignette-art.md
    art-009-ukiyo-e-inspired-style-experimentation-plan.md
  legal/
    LEGAL_INDEX.md
    doc-data-001-unity-mobile-gacha-rpg-data-collection-and-compliance.md
    doc-legal-001-ai-generated-assets-legal-considerations.md
    doc-legal-002-ai-assets-legal-and-compliance-roadmap.md
    doc-legal-003-tos-eula-and-user-facing-legal-documents.md
  ops/
    OPS_INDEX.md
    doc-ops-001-project-document-and-telemetry-index.md
    doc-ops-002-unity-mcp-bridge-setup-and-usage.md
    doc-ops-003-ai-workspace-safety-and-backup-plan.md
    doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md
    doc-ops-005-march-2026-repo-modernization-retrospective.md
    doc-ops-006-unity-editor-version-policy-and-upgrade-checklist.md
    doc-ops-007-stage-1-unity-project-reality-audit.md
    doc-ops-008-short-term-implementation-todo.md
    doc-ops-009-long-term-roadmap-todo.md
    doc-ops-010-claude-code-read-first-handoff.md
  research/
    RESEARCH_INDEX.md
    doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md
    doc-eng-002-unity-project-runtime-architecture-patterns.md
    doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md
    doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md
    doc-ref-003-one-piece-treasure-cruise-analysis.md
    doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md
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
    doc-ref-004-fire-emblem-heroes-success-and-drawbacks.docx
```

## Default operating rule

Do not scan the whole folder by default.

For most tasks:

1. Start with this index.
2. If the task is Claude Code onboarding or next-stage implementation, start with the lean handoff pack in `ops/` before broad scanning.
3. Prefer `design/` for product design, roster/world design, and character identity; `art/` for production-facing art rules; `ops/` for tooling and workflow; `legal/` for compliance and policy; and `research/` for engineering and comparative studies.
4. Open the matching PDF in `recruiter/` only if formatting, page fidelity, or missing detail matters.

## Canonical document catalog

| ID | Topic | Primary AI path | Recruiter PDF path | Authority | Notes |
| --- | --- | --- | --- | --- | --- |
| `DOC-GDD-001` | Shogun product design | `design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md` | `recruiter/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.pdf` | Primary design truth | Use first for gameplay, UX, progression, monetization, and product scope. |
| `DOC-ENG-001` | Unity engineering and production architecture | `research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md` | `recruiter/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.pdf` | Primary engineering truth | Use first for backend, Unity structure, build pipeline, delivery, and scale-path decisions. |
| `DOC-ENG-002` | Unity runtime architecture patterns | `research/doc-eng-002-unity-project-runtime-architecture-patterns.md` | No recruiter PDF in this folder | Supporting engineering authority | Use for client-side code boundaries, state flow, event channels, gesture input flow, and logic/view separation. |
| `DOC-DATA-001` | Telemetry, analytics, and compliance-aware data collection | `legal/doc-data-001-unity-mobile-gacha-rpg-data-collection-and-compliance.md` | `recruiter/doc-data-001-unity-mobile-gacha-rpg-data-collection-and-compliance.pdf` | Primary telemetry truth | Use first for events, schemas, batching, retention, consent, audit logging. |
| `DOC-OPS-001` | Implementation artifact inventory | `ops/doc-ops-001-project-document-and-telemetry-index.md` | `recruiter/doc-ops-001-project-document-and-telemetry-index.pdf` | Operational manifest | Use for file outputs, naming, CI artifacts, telemetry deliverables, and legal-document inventory. |
| `DOC-OPS-002` | Unity MCP bridge setup and operating rules | `ops/doc-ops-002-unity-mcp-bridge-setup-and-usage.md` | No recruiter PDF in this folder | Operational workflow note | Use for Codex/Claude live Unity-editor access, local MCP setup, safe usage boundaries, and exporter vs. MCP guidance. |
| `DOC-OPS-003` | AI workspace safety and backup plan | `ops/doc-ops-003-ai-workspace-safety-and-backup-plan.md` | No recruiter PDF in this folder | Operational safety note | Use for repo scope rules, Codex/Claude safety posture, backup layers, and recovery workflow after accidental deletion. |
| `DOC-OPS-004` | PixelLab evaluation and sprite production workflow | `ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md` | No recruiter PDF in this folder | Operational art-tooling note | Use for PixelLab fit, subscription vs API choice, sprite workflow, animation-size guidance, and AI-assisted art production boundaries. |
| `DOC-OPS-005` | March 2026 repo modernization retrospective | `ops/doc-ops-005-march-2026-repo-modernization-retrospective.md` | No recruiter PDF in this folder | Operational change log | Use when the question is what changed in the March 7, 2026 cleanup/tooling batch and why those pushed commits were structured that way. |
| `DOC-OPS-006` | Unity editor version policy and upgrade checklist | `ops/doc-ops-006-unity-editor-version-policy-and-upgrade-checklist.md` | No recruiter PDF in this folder | Operational upgrade policy | Use for the project-specific Unity baseline, stable upgrade target, alpha/beta exclusion rule, and safe LTS-upgrade checklist. |
| `DOC-OPS-007` | Stage 1 Unity project reality audit | `ops/doc-ops-007-stage-1-unity-project-reality-audit.md` | No recruiter PDF in this folder | Operational reality-check note | Use when the question is what the current Unity project actually contains, what is salvageable, what is placeholder, and whether the project should be rebuilt or restarted. |
| `DOC-OPS-008` | Short-term implementation TODO | `ops/doc-ops-008-short-term-implementation-todo.md` | No recruiter PDF in this folder | Operational execution backlog | Use when the task is active-slice execution and the goal is to keep near-term work tightly scoped to `Dev_Sandbox` and `Courtyard Ambush`. |
| `DOC-OPS-009` | Long-term roadmap TODO | `ops/doc-ops-009-long-term-roadmap-todo.md` | No recruiter PDF in this folder | Operational deferment note | Use when the task is real but explicitly not part of the current slice backlog. |
| `DOC-OPS-010` | Claude Code read-first handoff | `ops/doc-ops-010-claude-code-read-first-handoff.md` | No recruiter PDF in this folder | Operational handoff note | Use first when starting a new Claude Code implementation session and token efficiency matters. |
| `DESIGN-001` | Character collection and fantasy strategy | `design/design-001-character-collection-and-fantasy-strategy.md` | No recruiter PDF in this folder | Primary collection-strategy note | Use for collectible fantasy, roster pillars, battle-vs-presentation art lanes, and variant planning. |
| `DESIGN-002` | World pillars and combat identity framework | `design/design-002-world-pillars-and-combat-identity-framework.md` | No recruiter PDF in this folder | Primary roster-identity note | Use for world pillars, elemental affinity, weapon families, martial schools, and how those layers stack into a character identity. |
| `DESIGN-003` | Long-term balance and power-creep policy | `design/design-003-long-term-balance-and-power-creep-policy.md` | No recruiter PDF in this folder | Primary live-balance note | Use for banner-value policy, power-creep controls, old-unit refresh cadence, and ranked/PvE balance boundaries. |
| `DESIGN-004` | Roguelite replayability and run-mode framework | `design/design-004-roguelite-replayability-and-run-mode-framework.md` | No recruiter PDF in this folder | Primary replayability note | Use for how procedural structure should fit story mode, how a roguelite mode should work, and how to reduce repetition without proceduralizing the whole campaign. |
| `DESIGN-005` | Co-op, PvP, and social systems roadmap | `design/design-005-co-op-pvp-and-social-systems-roadmap.md` | No recruiter PDF in this folder | Primary multiplayer roadmap note | Use for co-op priority, world bosses, PvP rollout, clan timing, and social-system phasing. |
| `DESIGN-006` | Mobile platform, display, and performance strategy | `design/design-006-mobile-platform-display-and-performance-strategy.md` | No recruiter PDF in this folder | Primary platform-strategy note | Use for mobile-first scope, tablets/foldables, frame-rate policy, graphics settings, 2D vs 2.5D boundaries, and PC-later positioning. |
| `DESIGN-007` | Range circles and threat geometry framework | `design/design-007-range-circles-and-threat-geometry-framework.md` | No recruiter PDF in this folder | Primary combat-readability note | Use for why range circles matter, how short/mid/long attack bands should work, how threat overlap should shape tactics, and how to keep range design readable over time. |
| `DESIGN-008` | Active vertical slice definition | `design/design-008-active-vertical-slice-definition.md` | No recruiter PDF in this folder | Primary implementation-target note | Use when deciding what the next real slice is, what `Dev_Sandbox` must prove, and what is explicitly out of scope until later. |
| `DESIGN-009` | First vertical-slice roster and encounter plan | `design/design-009-first-vertical-slice-roster-and-encounter-plan.md` | No recruiter PDF in this folder | Primary slice-content note | Use when deciding which units, enemies, and authored battle scenario should become the first trustworthy gameplay loop. |
| `ART-001` | Style bible and visual targets | `art/art-001-style-bible-and-visual-targets.md` | No recruiter PDF in this folder | Primary art-direction note | Use first for silhouette rules, palette direction, detail limits, and gameplay readability targets. |
| `ART-002` | Sprite production pipeline | `art/art-002-sprite-production-pipeline.md` | No recruiter PDF in this folder | Primary art workflow note | Use for the bounded PixelLab-to-Aseprite-to-Unity production flow and trial/pass-fail rules. |
| `ART-005` | Legacy and production asset separation policy | `art/art-005-legacy-and-production-asset-separation-policy.md` | No recruiter PDF in this folder | Primary asset-organization note | Use for how old imported packs, Gemini/PixelLab working files, and approved runtime assets should be separated. |
| `ART-006` | Sex appeal and damage-art policy | `art/art-006-sex-appeal-and-damage-art-policy.md` | No recruiter PDF in this folder | Primary fanservice boundary note | Use for sex-appeal limits, FEH-style damage-art interpretation, and what battle animation fanservice is too risky. |
| `ART-007` | Violence and injury policy | `art/art-007-violence-and-injury-policy.md` | No recruiter PDF in this folder | Primary violence boundary note | Use for blood, bruises, violent finishers, demon-vs-human dismemberment limits, and dark-fantasy brutality boundaries. |
| `ART-008` | Roguelite event vignette art | `art/art-008-roguelite-event-vignette-art.md` | No recruiter PDF in this folder | Primary event-scene art note | Use for STS-style event-scene presentation, vignette reuse rules, and how roguelite choice nodes should use atmospheric art without exploding scope. |
| `ART-009` | Ukiyo-e-inspired style experimentation plan | `art/art-009-ukiyo-e-inspired-style-experimentation-plan.md` | No recruiter PDF in this folder | Primary style-test note | Use when evaluating how much ukiyo-e / ink / monochrome influence belongs in the default shipped look versus special-purpose scenes. |
| `ART-003` | Unity 2D import and animation standards | `art/art-003-unity-2d-import-and-animation-standards.md` | No recruiter PDF in this folder | Primary import standard | Use for `.aseprite` import, animation tags, frame-budget targets, and gameplay-facing sprite standards. |
| `ART-004` | Asset provenance and source tracking | `art/art-004-asset-provenance-and-source-tracking.md` | No recruiter PDF in this folder | Primary provenance note | Use for generated-art traceability, manual-edit tracking, and shipped-asset metadata requirements. |
| `DOC-LEGAL-001` | Legal and platform-policy risk overview | `legal/doc-legal-001-ai-generated-assets-legal-considerations.md` | `recruiter/doc-legal-001-ai-generated-assets-legal-considerations.pdf` | Primary legal risk briefing | Use for AI asset usage, privacy, loot box compliance, store-policy risk, and IP questions. |
| `DOC-LEGAL-002` | Legal backlog and next-step roadmap | `legal/doc-legal-002-ai-assets-legal-and-compliance-roadmap.md` | `recruiter/doc-legal-002-ai-assets-legal-and-compliance-roadmap.pdf` | Legal roadmap | Use when prioritizing follow-up research, counsel review, and unresolved compliance work. |
| `DOC-LEGAL-003` | ToS, EULA, and user-facing legal drafting | `legal/doc-legal-003-tos-eula-and-user-facing-legal-documents.md` | `recruiter/doc-legal-003-tos-eula-and-user-facing-legal-documents.pdf` | Primary legal drafting guide | Use first when drafting or revising terms, EULA structure, clickwrap, and acceptance logging. |
| `DOC-REF-001` | Naruto Blazing mechanics inspiration study | `research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md` | `recruiter/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.pdf` | Inspiration only | Use for combat and pacing patterns. Do not copy expressive IP. |
| `DOC-REF-002` | Naruto Blazing success, failure, and shutdown analysis | `research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md` | `recruiter/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.pdf` | Strategic caution reference | Use for live-ops, retention, monetization, and shutdown-risk lessons. |
| `DOC-REF-003` | One Piece Treasure Cruise longevity analysis | `research/doc-ref-003-one-piece-treasure-cruise-analysis.md` | `recruiter/doc-ref-003-one-piece-treasure-cruise-analysis.pdf` | Strategic caution reference | Use for long-term sustainability, onboarding, technical debt, and content burden. |
| `DOC-REF-004` | Fire Emblem Heroes comparison source | `research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md` | `recruiter/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.docx` | Secondary source | Open the retained `.docx` only if the original Word source is needed. |

## Source-of-truth hierarchy

### Product and feature intent
1. `DOC-GDD-001`
2. `DESIGN-001` to `DESIGN-009` for living collection, world, combat-identity, range-readability, balance, replayability, multiplayer structure, active-slice definition, and first-slice content planning
3. `DOC-ENG-001`, `DOC-ENG-002`, and `DOC-DATA-001` for implementation detail
4. `DOC-REF-001` to `DOC-REF-004` for inspiration and caution only

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
3. `DOC-OPS-006`
4. `DOC-OPS-007`
5. `DOC-OPS-005`
6. `DOC-OPS-004`
7. `DOC-ENG-002`
8. `DOC-LEGAL-001`
9. `DOC-LEGAL-002`

### AI workspace safety, repo scope, and backup recovery
1. `DOC-OPS-003`
2. `DOC-OPS-005`
3. `DOC-OPS-006`
4. `DOC-OPS-002`
5. `DOC-LEGAL-001`

### Active implementation slice and first-slice content
1. `DOC-OPS-010`
2. `DOC-OPS-008`
3. `DOC-OPS-007`
4. `DESIGN-008`
5. `DESIGN-009`
6. `DOC-GDD-001`
7. `DESIGN-007`
8. `ART-005`

### AI-assisted sprite production and PixelLab workflow
1. `ART-001`
2. `ART-005`
3. `DESIGN-001`
4. `DESIGN-002`
5. `ART-002`
6. `ART-006`
7. `ART-009`
8. `ART-003`
9. `ART-004`
10. `DOC-OPS-004`
11. `DOC-LEGAL-001`
12. `DOC-LEGAL-002`
12. `DOC-OPS-002` if MCP integration matters

### Art direction, sprite import, and provenance
1. `ART-001`
2. `ART-005`
3. `DESIGN-001`
4. `DESIGN-002`
5. `ART-009`
6. `ART-006`
7. `ART-007`
8. `ART-002`
9. `ART-003`
10. `ART-004`
11. `DOC-OPS-004`
12. `DOC-LEGAL-001`

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
2. `DESIGN-007`
3. `DESIGN-003`
4. `DESIGN-004`
5. `DESIGN-005`
6. `DOC-REF-001`
7. optionally `DOC-REF-002`

### Mobile-first platform scope, tablets, foldables, and rendering strategy
Open:
1. `DOC-GDD-001`
2. `DESIGN-006`
3. `DOC-ENG-001`
4. `ART-001`
5. `ART-008`
6. `DOC-ENG-002`

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
3. `DOC-OPS-006`
4. `DOC-OPS-005`
5. `DOC-OPS-004`
6. `DOC-ENG-002`
7. `DOC-LEGAL-001`

### AI safety, repo boundaries, and backup recovery
Open:
1. `DOC-OPS-003`
2. `DOC-OPS-007`
3. `DOC-OPS-005`
4. `DOC-OPS-006`
5. `DOC-OPS-002`
6. `DOC-LEGAL-001`

### Claude handoff / next-stage implementation
Open:
1. `DOC-OPS-010`
2. `DOC-OPS-008`
3. `DESIGN-008`
4. `DESIGN-009`
5. `DOC-OPS-007`
6. `DOC-GDD-001`

### Unity editor baseline, LTS upgrades, and version-move safety
Open:
1. `DOC-OPS-006`
2. `DOC-ENG-001`
3. `DOC-OPS-002`
4. `DESIGN-006`
5. `DOC-OPS-005`

### Pixel art workflow, PixelLab, and AI-generated sprite production
Open:
1. `ART-001`
2. `ART-005`
3. `DESIGN-001`
4. `DESIGN-002`
5. `ART-009`
6. `ART-006`
7. `ART-007`
8. `ART-002`
9. `ART-003`
10. `ART-004`
11. `DOC-OPS-004`
12. `DOC-LEGAL-001`
13. `DOC-LEGAL-002`
14. `DOC-OPS-002` if MCP integration or coding-assistant setup matters

### Project reality, salvage, and restart-vs-rebuild decisions
Open:
1. `DOC-OPS-007`
2. `DESIGN-008`
3. `DOC-GDD-001`
4. `DOC-ENG-001`
5. `DOC-ENG-002`
6. `ART-005`

### World pillars, elements, martial schools, and roster identity
Open:
1. `DESIGN-001`
2. `DESIGN-002`
3. `DESIGN-007`
4. `DESIGN-003`
5. `ART-001`
6. `DOC-GDD-001`
7. `DOC-REF-001`

### Long-term balance, power creep, and banner health
Open:
1. `DOC-GDD-001`
2. `DESIGN-003`
3. `DESIGN-005`
4. `DOC-REF-002`
5. `DOC-REF-004`

### Roguelite structure, replayability, and anti-repetition design
Open:
1. `DOC-GDD-001`
2. `DESIGN-004`
3. `ART-008`
4. `DESIGN-003`
5. `DOC-REF-001`
6. `DOC-REF-003`

### Co-op, PvP, raids, and clans
Open:
1. `DOC-GDD-001`
2. `DESIGN-005`
3. `DESIGN-003`
4. `DOC-ENG-001`
5. `DOC-REF-001`
6. `DOC-REF-002`
7. `DOC-REF-003`

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

- Prefer the smallest relevant Markdown companion in `design/`, `art/`, `legal/`, `ops/`, or `research/` instead of scanning multiple sections by default.
- Prefer `design/` documents when the task is roster identity, world pillars, combat identity, collection planning, long-term balance, replayability structure, or multiplayer/social-roadmap planning.
- Prefer `design/` documents when the task is the current implementation slice, rebuild target, or what the active gameplay scene is supposed to prove next.
- Prefer `art/` documents when the task is specifically production-facing art direction, sprite workflow, import rules, or provenance.
- Prefer `ops/` documents when the task is tooling, MCP, repo safety, backups, or retrospective workflow history.
- Prefer the lean handoff pack in `ops/` when the task is next-stage implementation and broad scanning would waste tokens.
- Prefer `legal/` documents when the task is compliance, policy, privacy, or user-facing legal text.
- Prefer `research/` documents when the task is engineering studies, architecture research, or comparative game analysis.
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
If the task is Claude Code onboarding or next-stage implementation, start with DOC-OPS-010 and DOC-OPS-008 before broad scanning.
Prefer the smallest relevant Markdown companion in design/, art/, legal/, ops/, or research/ first, then open recruiter/ PDFs only when needed.
Use DOC-GDD-001 as primary design truth, DESIGN-* docs as living roster/world identity guidance, DOC-ENG-001 and DOC-ENG-002 plus DOC-DATA-001 as implementation truth, DOC-LEGAL-* docs as compliance constraints, DOC-OPS-* docs as workflow/tooling truth, and DOC-REF-* docs as inspiration/caution only.
If live Unity editor state or AI tooling workflow matters, open OPS_INDEX.md first, then DOC-OPS-002.
If the task is roster identity or world design, open DESIGN_INDEX.md first.
If the task is art direction or sprite production, open ART_INDEX.md first, then the linked DESIGN-* docs if needed, then DOC-OPS-004.
If documents conflict, follow the hierarchy in PROJECT_CONTEXT_INDEX.md and report the conflict explicitly.
```
