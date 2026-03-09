# Art Documentation Index

> Purpose: route art-direction, sprite-production, Unity import, and provenance questions without mixing them into broader AI/legal/tooling notes.

## Use this folder when the question is:

- what visual direction `Shogun` should follow
- how PixelLab output should be cleaned and productionized
- what sprite sizes, frame budgets, and animation tags should be used
- how `.aseprite` files should move into Unity
- how roguelite event scenes should be visualized and produced
- how generated or edited assets should be tracked for provenance

## Document order

1. [`art-001-style-bible-and-visual-targets.md`](./art-001-style-bible-and-visual-targets.md)
   - visual targets, silhouette rules, palette direction, and camera/readability goals
2. [`art-005-legacy-and-production-asset-separation-policy.md`](./art-005-legacy-and-production-asset-separation-policy.md)
   - separation rule for imported legacy packs, AI-generated source assets, and approved runtime-facing production assets
3. [`art-002-sprite-production-pipeline.md`](./art-002-sprite-production-pipeline.md)
   - the actual production workflow from generation through cleanup and review
4. [`art-003-unity-2d-import-and-animation-standards.md`](./art-003-unity-2d-import-and-animation-standards.md)
   - Unity import, `.aseprite` handling, tags, clip expectations, and gameplay-facing standards
5. [`art-006-sex-appeal-and-damage-art-policy.md`](./art-006-sex-appeal-and-damage-art-policy.md)
   - boundaries for sex appeal, FEH-style damage art, and what battle animation fanservice is too risky
6. [`art-007-violence-and-injury-policy.md`](./art-007-violence-and-injury-policy.md)
   - boundaries for blood, bruises, dark-fantasy brutality, and what gore or dismemberment is too risky
7. [`art-008-roguelite-event-vignette-art.md`](./art-008-roguelite-event-vignette-art.md)
   - STS-style event scene art for roguelite choices, shrines, bargains, and atmospheric decision points
8. [`art-009-ukiyo-e-inspired-style-experimentation-plan.md`](./art-009-ukiyo-e-inspired-style-experimentation-plan.md)
   - controlled test plan for mixing ukiyo-e / ink influence with readable, colorful gacha-facing pixel art
9. [`art-004-asset-provenance-and-source-tracking.md`](./art-004-asset-provenance-and-source-tracking.md)
   - provenance rules, metadata, and legal-traceability handling

## Companion docs outside this folder

- [`../design/DESIGN_INDEX.md`](../design/DESIGN_INDEX.md)
  - use first when the question is roster identity, world pillars, elements, martial schools, or collectible fantasy
- [`../design/design-001-character-collection-and-fantasy-strategy.md`](../design/design-001-character-collection-and-fantasy-strategy.md)
  - use for collectible fantasy, battle-vs-presentation lanes, and variant planning
- [`../design/design-002-world-pillars-and-combat-identity-framework.md`](../design/design-002-world-pillars-and-combat-identity-framework.md)
  - use for world pillars, elemental affinity, weapon families, martial schools, and roster identity stacking
- [`../design/design-004-roguelite-replayability-and-run-mode-framework.md`](../design/design-004-roguelite-replayability-and-run-mode-framework.md)
  - use for event-node structure, run flow, and how vignette art should function inside the roguelite mode
- [`../ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md`](../ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md)
  - use first when the question is whether PixelLab is worth using at all
- [`../ops/doc-ops-007-stage-1-unity-project-reality-audit.md`](../ops/doc-ops-007-stage-1-unity-project-reality-audit.md)
  - use when the question is what the current Unity project really contains and why legacy and production assets need separation
- [`../legal/doc-legal-001-ai-generated-assets-legal-considerations.md`](../legal/doc-legal-001-ai-generated-assets-legal-considerations.md)
  - use when AI-art usage or IP risk is part of the question
- [`../legal/doc-legal-002-ai-assets-legal-and-compliance-roadmap.md`](../legal/doc-legal-002-ai-assets-legal-and-compliance-roadmap.md)
  - use when deciding what legal follow-up remains unresolved

## Default order for art-production questions

1. `ART-001`
2. `ART-005`
3. `DESIGN-001`
4. `DESIGN-002`
5. `ART-002`
6. `ART-006`
7. `ART-007`
8. `ART-008`
9. `ART-009`
10. `ART-003`
11. `ART-004`
12. `DOC-OPS-004`
13. `DOC-LEGAL-001`

## Rule for future updates

Add new art docs here when:

- a new production standard becomes stable enough to enforce
- a new tool becomes part of the regular art workflow
- sprite sizing, import, or provenance rules materially change
