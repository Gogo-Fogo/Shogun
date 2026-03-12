# DOC-OPS-014 NaBlA-Informed Product TODO

> Status date: March 11, 2026  
> Purpose: convert the local NaBlA study pack and comparative live-service lessons into a Shogun-specific product backlog without letting the project drift away from the active vertical slice.

## Why this exists

The repo now has three useful layers of NaBlA study:

- a repo-safe manifest of the local pack
- a folder-by-folder interpretation note
- an all-folder translation matrix into Shogun lanes

What was still missing was the execution layer:

- what should actually be done now
- what should wait until the slice is trustworthy
- what should be studied but not implemented
- what design traps from Blazing, FEH, and OPTC should be treated as red flags

This note is that backlog layer.

Use it together with:

- [DOC-OPS-008](./doc-ops-008-short-term-implementation-todo.md)
- [DOC-OPS-009](./doc-ops-009-long-term-roadmap-todo.md)
- [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md)
- [DESIGN-002](../design/design-002-world-pillars-and-combat-identity-framework.md)
- [DESIGN-008](../design/design-008-active-vertical-slice-definition.md)
- [DESIGN-009](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md)
- [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md)
- [DOC-REF-002](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
- [DOC-REF-003](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md)
- [DOC-REF-004](../research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md)
- [DOC-REF-007](../research/doc-ref-007-nabla-to-shogun-adaptation-matrix.md)
- [DOC-LEGAL-004](../legal/doc-legal-004-third-party-reference-material-handling.md)

## Core rule

The raw NaBlA pack remains local-only.

What belongs in the repo is:

- system lessons
- information hierarchy lessons
- taxonomy and dimensions
- Shogun-specific backlog items

Do not treat NaBlA assets as temporary production assets.
Do not let Naruto-specific expression define Shogun's visual identity.
Copy structure and hierarchy, not raw assets or franchise expression.

## Slice-first priority rule

This document is broad product planning, but it does **not** override the active slice.

Current priority order:

1. make `Dev_Sandbox` and `Courtyard Ambush` trustworthy
2. make the six-unit slice readable and production-organized
3. strengthen collection, summon, and presentation lanes after battle truth exists
4. build progression depth and broader front-door polish after the slice stops breaking

If a task conflicts with [DESIGN-008](../design/design-008-active-vertical-slice-definition.md), the slice wins.

## NaBlA folder coverage map

This table exists to ensure every studied NaBlA folder is represented in backlog planning.

| NaBlA folder | Shogun lane | Primary workstream | Default backlog bucket |
| --- | --- | --- | --- |
| `App Icons` | store icon and install-facing identity | Brand and Front Door | `Later` |
| `Artwork_Full` | premium portrait and collection-value art | Collection and Presentation | `Next` |
| `Backgrounds_Map` | battle backgrounds and encounter staging | Battle Slice First | `Now` |
| `Backgrounds_UI` | menu atmosphere plates | Collection and Presentation | `Next` |
| `Chakra Icon` | charge-ring tokens and readiness icons | Battle Slice First | `Now` |
| `Complete Sprites` | assembled runtime sprite validation lane | Battle Slice First | `Next` |
| `Cut-Ins` | combo strips and ultimate presentation | Battle Slice First | `Now` |
| `Evo Materials` | awakening and evolution material families | Progression and Content Economy | `Later` |
| `Help Stuff` | contextual help strips and onboarding ribbons | Progression and Content Economy | `Later` |
| `LB Crystals` | ascension or limit-break materials | Progression and Content Economy | `Later` |
| `Logo` | title lockup and brand identity | Brand and Front Door | `Later` |
| `Misc` | edge-case study only | Comparative Guardrails | `Do not do` |
| `Scenes` | event vignette and story-scene art | Collection and Presentation | `Next` |
| `Spritesheets` | playable/enemy production sprite sheets | Battle Slice First | `Now` |
| `Terrain` | floor accents, hazards, and battlefield decals | Battle Slice First | `Next` |
| `Title Screen` | startup/title presentation | Brand and Front Door | `Later` |
| `UI` | battle HUD atoms and shared chrome | Battle Slice First | `Now` |
| `UI_Gasha` | summon reveal and banner spectacle | Collection and Presentation | `Next` |
| `UI_Text` | emphasis labels and reward/combo word art | Collection and Presentation | `Next` |
| `Unit Icons` | barracks icons, team slots, summon chips | Battle Slice First | `Now` |

## Workstream 1: Battle Slice First

Source anchors:

- Shogun: [DOC-OPS-008](./doc-ops-008-short-term-implementation-todo.md), [DESIGN-008](../design/design-008-active-vertical-slice-definition.md), [DESIGN-009](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md), [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md)
- Comparative: [DOC-REF-001](../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md), [DOC-REF-002](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md), [DOC-REF-007](../research/doc-ref-007-nabla-to-shogun-adaptation-matrix.md)

### Backlog

- `[Now]` Finish the six-unit slice as the only trustworthy visual truth path: `Spritesheets`, `Unit Icons`, `Chakra Icon`, `Cut-Ins`, `UI`, and `Backgrounds_Map`.
- `[Now]` Keep all active battle presentation rooted in the authored slice roster and enemy trio from `Courtyard Ambush`, not in broad menu or meta features.
- `[Now]` Make the current battle HUD and battlefield readability beat Blazing on clarity, not noise: visible ranges, turn clarity, hit confirmation, combo clarity, and charge readability.
- `[Next]` Use `Complete Sprites` as a validation lane to check whether the final assembled runtime characters still preserve world pillar and weapon/range identity at mobile scale.
- `[Next]` Promote `Terrain` into authored battlefield support only when it improves spatial read, hazards, or encounter identity without cluttering the field.
- `[Do not do]` Do not let support scenes, summon polish, or broad shell-scene work outrank battle-loop trustworthiness in `Dev_Sandbox`.
- `[Do not do]` Do not solve readability by copying Naruto HUD expression, eye-strip framing, or battle-map compositions literally.

## Workstream 2: Collection and Presentation

Source anchors:

- Shogun: [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md), [DESIGN-002](../design/design-002-world-pillars-and-combat-identity-framework.md), [ART-008](../art/art-008-roguelite-event-vignette-art.md)
- Comparative: [DOC-REF-001](../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md), [DOC-REF-004](../research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md), [DOC-REF-007](../research/doc-ref-007-nabla-to-shogun-adaptation-matrix.md)

### Backlog

- `[Next]` Build the collection lane from `Artwork_Full`, `Scenes`, `Backgrounds_UI`, `UI_Gasha`, and `UI_Text` after the battle slice is stable enough to judge which characters deserve premium presentation first.
- `[Next]` Make barracks, summon, and event-vignette presentation reinforce Shogun world pillars: `Imperial Court`, `Ronin Marches`, `Temple and Veil Orders`, `Yokai Courts`, and `Corrupted Dominion`.
- `[Next]` Tie premium portrait art and summon presentation back to collectible fantasies from [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md): `Tragic Nobility`, `Rogue Violence`, `Yokai Elegance`, `Corrupted Power`, and `Mystic Ritual`.
- `[Next]` Keep `Unit Icons` and portrait crops consistent across barracks, summon results, and team rails so the same character reads like the same collectible everywhere.
- `[Later]` Add richer regular-ability cut-in presentation only if the base combo-strip and ultimate lanes are already readable, performant, and worth the added art burden.
- `[Later]` Expand event vignette families only after the current slice and support-scene basics stop fighting basic usability.
- `[Do not do]` Do not start large alt/variant cadence, moving-art ambition, or premium-presentation explosion before base character packages are trustworthy.
- `[Do not do]` Do not let menu ornament or summon spectacle become louder than the collectible fantasy itself.

## Workstream 3: Progression and Content Economy

Source anchors:

- Shogun: [DOC-OPS-009](./doc-ops-009-long-term-roadmap-todo.md), [DESIGN-003](../design/design-003-long-term-balance-and-power-creep-policy.md)
- Comparative: [DOC-REF-002](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md), [DOC-REF-003](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md), [DOC-REF-007](../research/doc-ref-007-nabla-to-shogun-adaptation-matrix.md)

### Backlog

- `[Later]` Design `Evo Materials` as Shogun-specific awakening families tied to the world instead of generic gems: court seals, ronin contracts, temple talismans, yokai relics, corrupted shards.
- `[Later]` Design `LB Crystals` as higher-ceremony ascension materials that communicate rarity and long-term investment without FEH-style mandatory merge pressure.
- `[Later]` Build `Help Stuff` as localized contextual guidance that appears where the player needs it rather than as big tutorial walls.
- `[Later]` Plan progression icons and currencies so they are easy to sort and readable at tiny size before adding a large number of systems.
- `[Do not do]` Do not build a broad economy, material tree, or permanent inventory complexity before the result/reward handoff is real and the slice loop is fun.
- `[Do not do]` Do not copy Blazing's or OPTC's grind-heavy event-material cadence if it would create booster dependency, onboarding pain, or repetitive obligation loops.

## Workstream 4: Brand and Front Door

Source anchors:

- Shogun: [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md), [DESIGN-006](../design/design-006-mobile-platform-display-and-performance-strategy.md), [DOC-OPS-008](./doc-ops-008-short-term-implementation-todo.md)
- Comparative: [DOC-REF-002](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md), [DOC-REF-007](../research/doc-ref-007-nabla-to-shogun-adaptation-matrix.md)

### Backlog

- `[Later]` Design the `Title Screen`, `Logo`, and `App Icons` only after the game's actual collectible identity is stable enough to summarize in one image and one mark.
- `[Later]` Make the front door sell Shogun's own promise: dark-feudal drama, collectible fantasy, and tactical clarity, not just generic anime energy.
- `[Later]` Use the front door to emphasize one dominant fantasy or flagship character package, not a noisy roster collage.
- `[Do not do]` Do not derive title composition, lockup rhythm, icon framing, or startup mood directly from Naruto references.
- `[Do not do]` Do not over-invest in front-door polish while battle, barracks, and summon readability are still brittle.

## Workstream 5: Comparative Guardrails

Source anchors:

- Shogun: [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md), [DESIGN-003](../design/design-003-long-term-balance-and-power-creep-policy.md), [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md)
- Comparative: [DOC-REF-002](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md), [DOC-REF-003](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md), [DOC-REF-004](../research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md)

### Backlog

- `[Now]` Keep from Blazing: fast positioning, combo/team synergy, portrait-mobile immediacy, and clear battle spectacle tied to confirmed actions.
- `[Now]` Keep from FEH: short-session readability, deterministic tactical reads, and strong collection fantasy that works even with a relatively compact battle format.
- `[Now]` Keep from OPTC only where it stays humane: depth through interaction mastery and box-building, not through endless mandatory grind.
- `[Next]` Treat monetization as collection fantasy first and raw power escalation second.
- `[Next]` Treat old-unit relevance as a mandatory design concern early, not as a live-service emergency after a year of drift.
- `[Next]` Define QoL as baseline product quality, not premium monetization bait.
- `[Do not do]` Do not let PvP-first pressure, whale-vs-F2P tension, or meta panic define the roster roadmap.
- `[Do not do]` Do not let skills or weapons bloat into FEH-style essay text that breaks readability and onboarding.
- `[Do not do]` Do not let event structures become OPTC-style obligation loops built around point boosters and exhausting cadence.
- `[Do not do]` Do not let operational trust decay through brittle live-ops, opaque rules, or compensation chaos.

## Explicit priorities by horizon

### Immediate horizon

Focus only on:

- battle readability
- six-unit slice coherence
- charge/cut-in/icon/background lanes that directly improve `Dev_Sandbox`
- one trustworthy collection-facing bridge after the battle slice stops breaking

### Post-slice horizon

Move to these only after the slice is stable:

- richer collection presentation
- summon spectacle and banner framing
- progression materials and upgrade families
- broader menu and event-vignette polish

### Long-horizon polish

Leave these until the game's identity is earned:

- title screen hero art
- logo lockups
- store icons
- broad live-service ornament and variant cadence

## Validation checklist for this note

This note is only useful if the following remain true:

- every studied NaBlA top-level folder appears in the coverage map above
- every backlog line is explicitly tagged `Now`, `Next`, `Later`, or `Do not do`
- each workstream cites both Shogun source docs and comparative lessons when relevant
- the active-slice boundary remains explicit and favors `Dev_Sandbox` over shell-scene ambition
- the legal handling rule stays unchanged: copy structure and hierarchy, not raw assets or franchise expression

## Final rule

Use this document to decide **what to build next**, not to justify broad scope drift.

If a task helps the slice become trustworthy, it can move forward.
If it only makes the app look broader while the slice is still fragile, it waits.
