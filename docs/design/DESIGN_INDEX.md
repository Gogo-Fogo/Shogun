# Design Documentation Index

> Purpose: route cross-disciplinary notes that sit between game design, worldbuilding, combat identity, lore structure, and collection planning.

## Use this folder when the question is:

- how the roster should be structured beyond simple classes
- what makes a character collectible instead of interchangeable
- how world pillars, factions, bloodlines, and schools should work
- how elements, weapon families, and martial schools should stack together
- how character fantasy should support both monetization and gameplay identity

## Document order

1. [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
   - master product design, systems scope, monetization frame, and overall design truth
2. [`design-001-character-collection-and-fantasy-strategy.md`](./design-001-character-collection-and-fantasy-strategy.md)
   - collectible fantasy, battle-vs-presentation lanes, popularity planning, and variant value
3. [`design-002-world-pillars-and-combat-identity-framework.md`](./design-002-world-pillars-and-combat-identity-framework.md)
   - world pillars, elemental affinity, weapon families, martial schools, and how those layers combine into character identity
4. [`design-003-long-term-balance-and-power-creep-policy.md`](./design-003-long-term-balance-and-power-creep-policy.md)
   - long-term balance governance, power-creep limits, refresh cadence, and banner-value policy
5. [`design-004-roguelite-replayability-and-run-mode-framework.md`](./design-004-roguelite-replayability-and-run-mode-framework.md)
   - replayability structure, roguelite mode positioning, run systems, anti-repetition framework, and the role of STS-style event scenes
6. [`design-005-co-op-pvp-and-social-systems-roadmap.md`](./design-005-co-op-pvp-and-social-systems-roadmap.md)
   - co-op, world bosses, PvP rollout, clans, and social-system phasing
7. [`design-006-mobile-platform-display-and-performance-strategy.md`](./design-006-mobile-platform-display-and-performance-strategy.md)
   - mobile-first platform stance, foldables and tablets, frame-rate policy, graphics settings, and 2D vs 2.5D boundaries
8. [`design-007-range-circles-and-threat-geometry-framework.md`](./design-007-range-circles-and-threat-geometry-framework.md)
   - why range circles matter, how short/mid/long threat should work, and how threat geometry should shape unit identity and encounter readability
9. [`design-008-active-vertical-slice-definition.md`](./design-008-active-vertical-slice-definition.md)
   - the one current implementation target, anchored on `Dev_Sandbox`, with explicit in-scope and out-of-scope rules
10. [`design-009-first-vertical-slice-roster-and-encounter-plan.md`](./design-009-first-vertical-slice-roster-and-encounter-plan.md)
   - the concrete first slice roster, enemy archetypes, authored encounter, and minimal result/reward handoff
11. [`design-010-combat-hud-and-battle-ui-specification.md`](./design-010-combat-hud-and-battle-ui-specification.md)
   - portrait combat HUD, drag/release feedback, combo presentation, boss ribbon behavior, and battlefield-first UI hierarchy

## Companion docs outside this folder

- [`../art/ART_INDEX.md`](../art/ART_INDEX.md)
  - use when the question becomes production-facing art workflow, import rules, or provenance
- [`../art/art-001-style-bible-and-visual-targets.md`](../art/art-001-style-bible-and-visual-targets.md)
  - use for silhouette, palette, and readability standards
- [`../art/art-006-sex-appeal-and-damage-art-policy.md`](../art/art-006-sex-appeal-and-damage-art-policy.md)
  - use for fanservice boundaries, FEH-style damage art, and store-safe limits
- [`../art/art-007-violence-and-injury-policy.md`](../art/art-007-violence-and-injury-policy.md)
  - use for blood, bruising, dark-fantasy brutality, and what violent spectacle is too risky
- [`../art/art-008-roguelite-event-vignette-art.md`](../art/art-008-roguelite-event-vignette-art.md)
  - use for STS-style event-scene presentation, vignette art scope, and how roguelite choice scenes should be visualized
- [`../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md`](../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)
  - use for combat and roster reference patterns only
- [`../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md`](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
  - use for live-service caution, power-creep failure signals, and competitive-mode governance lessons
- [`../research/doc-ref-003-one-piece-treasure-cruise-analysis.md`](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md)
  - use for raid/event burden, alliance-style co-op lessons, and long-run content-sustainability warnings
- [`../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md`](../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md)
  - use when deciding how ambitious online systems should be and what order they should be built in
- [`../ops/doc-ops-011-ui-implementation-todo.md`](../ops/doc-ops-011-ui-implementation-todo.md)
  - use when the question shifts from battle UI design to slice-bound execution backlog
- [`../ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md`](../ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md)
  - use when the question is PixelLab fit and production-tool boundaries rather than world structure

## Default order for roster-design questions

1. `DESIGN-001`
2. `DESIGN-002`
3. `ART-001`
4. `ART-006`
5. `ART-007`
6. `DOC-REF-001`
7. `DOC-OPS-004`

## Default order for long-term systems questions

### Combat readability, threat geometry, and range-circle design

1. `DOC-GDD-001`
2. `DESIGN-007`
3. `DESIGN-010`
4. `DOC-REF-001`
5. `DESIGN-002`
6. `DESIGN-003`

### Battle HUD, combo feedback, and mobile combat presentation

1. `DOC-GDD-001`
2. `DESIGN-010`
3. `DESIGN-006`
4. `DESIGN-007`
5. `ART-001`
6. `DOC-REF-001`
7. `DOC-OPS-011`

### Active implementation slice and rebuild target

1. `DOC-OPS-007`
2. `DESIGN-008`
3. `DESIGN-009`
4. `DOC-GDD-001`
5. `DESIGN-007`
6. `ART-005`

### Balance, power creep, and live-service health

1. `DOC-GDD-001`
2. `DESIGN-003`
3. `DOC-REF-002`
4. `DOC-REF-004`
5. `DESIGN-005`

### Roguelite replayability and repetition control

1. `DOC-GDD-001`
2. `DESIGN-004`
3. `ART-008`
4. `DESIGN-003`
5. `DOC-REF-001`
6. `DOC-REF-003`

### Co-op, PvP, raids, and clans

1. `DOC-GDD-001`
2. `DESIGN-005`
3. `DESIGN-003`
4. `DOC-ENG-001`
5. `DOC-REF-001`
6. `DOC-REF-002`
7. `DOC-REF-003`

### Mobile platform, tablets, foldables, and rendering policy

1. `DOC-GDD-001`
2. `DESIGN-006`
3. `DOC-ENG-001`
4. `ART-001`
5. `ART-008`
6. `DOC-ENG-002`

## Rule for future updates

Add new design docs here when:

- a roster-identity rule becomes stable enough to enforce
- a faction/world structure becomes part of the planned content pipeline
- elements, schools, weapon families, or collection planning materially change
- the active implementation slice changes materially
- the concrete roster or authored encounter for the first slice changes materially
- the battle HUD and combat UI rules become stable enough to enforce
