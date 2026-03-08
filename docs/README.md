# Shogun: Flowers Fall in Blood Documents

This is the canonical documentation root for the repository.

This folder is organized into six documentation areas:

- `design/` contains the master GDD plus cross-disciplinary roster, world, combat-identity, collection-planning, long-term balance, replayability, multiplayer-roadmap, and platform/display strategy notes.
- `art/` contains production-facing art workflow notes, style standards, import rules, and provenance rules.
- `legal/` contains legal, compliance, privacy, telemetry-compliance, and user-facing policy documents.
- `ops/` contains repo workflow, AI tooling, Unity MCP, backup, and operational retrospective notes.
- `research/` contains engineering studies and comparative/reference analysis.
- `recruiter/` contains the recruiter-ready PDFs or retained original source files with matching canonical filenames where available.

Supporting file:

- `PROJECT_CONTEXT_INDEX.md` is the routing file. Read it first.

## Canonical naming

All files use stable document IDs in the filename:

- `doc-gdd-*` for product design
- `doc-eng-*` for engineering and runtime architecture research
- `doc-data-*` for telemetry and data-collection compliance
- `doc-ops-*` for repo workflow, tooling, MCP, and operational notes
- `doc-legal-*` for legal and compliance
- `doc-ref-*` for comparative/reference documents
- `design-*` for cross-disciplinary roster, world, combat-identity, balance, replayability, and multiplayer/social planning
- `art-*` for internal art-production standards and workflow notes

Markdown and PDF companions share the same base filename whenever both versions exist.

## Current state

- Every recruiter PDF now has a matching Markdown companion in `design/`, `legal/`, `ops/`, or `research/`.
- `DOC-REF-004` keeps the retained original `.docx` in `recruiter/` and the AI-friendly `.md` companion in `research/`.
- Some canonical engineering and reference notes may exist only in `research/` when no recruiter PDF is needed.
- Internal operational notes like Unity MCP workflow guidance may exist only in `ops/` when no recruiter PDF is needed.
- Internal Unity editor baseline and upgrade-governance notes may also live in `ops/` when they are workflow policy rather than recruiter-facing engineering research.
- Internal legal/compliance or telemetry-compliance notes may exist only in `legal/` when no recruiter PDF is needed.
- Cross-disciplinary character-identity and worldbuilding notes may exist only in `design/` when they are not purely art or purely gameplay docs.
- Cross-disciplinary live-balance, roguelite, multiplayer, and social-roadmap notes may also live only in `design/` when they cut across art, systems, lore, and live-service planning.
- Mobile platform, foldable/tablet, frame-rate, and rendering-strategy notes may also live in `design/` when they cut across UX, performance, content scope, and platform planning.
- Art workflow, style, import, and provenance notes may exist only in `art/` when they are internal production standards rather than recruiter-facing documents.
- Roguelite event-vignette presentation notes may also live in `art/` when they define reusable atmospheric event-scene production rather than character sprite standards.

## Suggested usage

1. Start with `PROJECT_CONTEXT_INDEX.md`.
2. Open the relevant file in `design/`, `art/`, `legal/`, `ops/`, or `research/`, depending on the task.
3. Open the matching file in `recruiter/` only if you need the PDF version.
