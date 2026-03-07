# Research Documentation Index

> Purpose: route engineering studies and comparative game-reference analysis without mixing them into legal or operational workflow notes.

## Use this folder when the question is:

- how Unity runtime or live-service engineering should be structured
- what build, backend, or production architecture patterns fit the project
- what other gacha or mobile RPGs teach us about retention, monetization, shutdown risk, or content burden
- what design or systems inspiration should inform `Shogun` without copying expressive IP

## Document order

1. [`doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md`](./doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md)
   - primary engineering and production architecture research
2. [`doc-eng-002-unity-project-runtime-architecture-patterns.md`](./doc-eng-002-unity-project-runtime-architecture-patterns.md)
   - runtime code-boundary, state-flow, event-channel, and logic/view-separation patterns
3. [`doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md`](./doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)
   - combat and pacing inspiration study
4. [`doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md`](./doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
   - success/failure/shutdown lessons
5. [`doc-ref-003-one-piece-treasure-cruise-analysis.md`](./doc-ref-003-one-piece-treasure-cruise-analysis.md)
   - longevity, technical-debt, and content-burden analysis
6. [`doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md`](./doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md)
   - FEH comparison note; use the retained source doc only if the original Word document is needed

## Companion docs outside this folder

- [`../design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](../design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
  - use for the final product scope before applying research conclusions
- [`../design/DESIGN_INDEX.md`](../design/DESIGN_INDEX.md)
  - use when research needs to be turned into concrete roster/world design
- [`../ops/doc-ops-002-unity-mcp-bridge-setup-and-usage.md`](../ops/doc-ops-002-unity-mcp-bridge-setup-and-usage.md)
  - use when live Unity-editor access or MCP affects engineering workflow
- [`../legal/doc-legal-001-ai-generated-assets-legal-considerations.md`](../legal/doc-legal-001-ai-generated-assets-legal-considerations.md)
  - use when research questions overlap legal or store-policy risk

## Default order for research questions

1. `DOC-ENG-001`
2. `DOC-ENG-002`
3. `DOC-GDD-001`
4. `DOC-REF-001`
5. `DOC-REF-002`
6. `DOC-REF-003`
7. `DOC-REF-004`

## Rule for future updates

Add new research docs here when:

- a strategic or engineering study becomes durable enough to cite repeatedly
- a comparative reference becomes part of the standing design vocabulary
- a runtime architecture note matters beyond one temporary conversation
