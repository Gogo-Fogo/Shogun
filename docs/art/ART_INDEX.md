# Art Documentation Index

> Purpose: route art-direction, sprite-production, Unity import, and provenance questions without mixing them into broader AI/legal/tooling notes.

## Use this folder when the question is:

- what visual direction `Shogun` should follow
- how PixelLab output should be cleaned and productionized
- what sprite sizes, frame budgets, and animation tags should be used
- how `.aseprite` files should move into Unity
- how generated or edited assets should be tracked for provenance

## Document order

1. [`art-001-style-bible-and-visual-targets.md`](./art-001-style-bible-and-visual-targets.md)
   - visual targets, silhouette rules, palette direction, and camera/readability goals
2. [`art-002-sprite-production-pipeline.md`](./art-002-sprite-production-pipeline.md)
   - the actual production workflow from generation through cleanup and review
3. [`art-003-unity-2d-import-and-animation-standards.md`](./art-003-unity-2d-import-and-animation-standards.md)
   - Unity import, `.aseprite` handling, tags, clip expectations, and gameplay-facing standards
4. [`art-006-sex-appeal-and-damage-art-policy.md`](./art-006-sex-appeal-and-damage-art-policy.md)
   - boundaries for sex appeal, FEH-style damage art, and what battle animation fanservice is too risky
5. [`art-007-violence-and-injury-policy.md`](./art-007-violence-and-injury-policy.md)
   - boundaries for blood, bruises, dark-fantasy brutality, and what gore or dismemberment is too risky
6. [`art-004-asset-provenance-and-source-tracking.md`](./art-004-asset-provenance-and-source-tracking.md)
   - provenance rules, metadata, and legal-traceability handling

## Companion docs outside this folder

- [`../design/DESIGN_INDEX.md`](../design/DESIGN_INDEX.md)
  - use first when the question is roster identity, world pillars, elements, martial schools, or collectible fantasy
- [`../design/design-001-character-collection-and-fantasy-strategy.md`](../design/design-001-character-collection-and-fantasy-strategy.md)
  - use for collectible fantasy, battle-vs-presentation lanes, and variant planning
- [`../design/design-002-world-pillars-and-combat-identity-framework.md`](../design/design-002-world-pillars-and-combat-identity-framework.md)
  - use for world pillars, elemental affinity, weapon families, martial schools, and roster identity stacking
- [`../ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md`](../ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md)
  - use first when the question is whether PixelLab is worth using at all
- [`../legal/doc-legal-001-ai-generated-assets-legal-considerations.md`](../legal/doc-legal-001-ai-generated-assets-legal-considerations.md)
  - use when AI-art usage or IP risk is part of the question
- [`../legal/doc-legal-002-ai-assets-legal-and-compliance-roadmap.md`](../legal/doc-legal-002-ai-assets-legal-and-compliance-roadmap.md)
  - use when deciding what legal follow-up remains unresolved

## Default order for art-production questions

1. `ART-001`
2. `DESIGN-001`
3. `DESIGN-002`
4. `ART-002`
5. `ART-006`
6. `ART-007`
7. `ART-003`
8. `ART-004`
9. `DOC-OPS-004`
10. `DOC-LEGAL-001`

## Rule for future updates

Add new art docs here when:

- a new production standard becomes stable enough to enforce
- a new tool becomes part of the regular art workflow
- sprite sizing, import, or provenance rules materially change
