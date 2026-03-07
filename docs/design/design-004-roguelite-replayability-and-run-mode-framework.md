# DESIGN-004: Roguelite Replayability and Run-Mode Framework

**Summary:** `Shogun` should use roguelite structure to fight repetition and create fresh midcore retention, but it should not proceduralize the entire story campaign. The best fit is an authored main story plus a dedicated run mode built from handcrafted encounter templates, branching node maps, temporary relic systems, and persistent attrition.

## Purpose

Use this note when deciding:

- whether story mode should become procedural
- how to make levels and missions less repetitive over time
- how a Slay the Spire-like structure can fit a tactical mobile gacha RPG
- what roguelite features belong in `Shogun`
- how roguelite systems can help with long-term balance and power-creep control

## Canon already in place

The GDD already commits `Shogun` to:

- branching campaign progression
- emergency missions
- side content
- `Ninja Road`-style endurance content with no HP regeneration between stages

See:

- [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [`../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md`](../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)
- [`../research/doc-ref-003-one-piece-treasure-cruise-analysis.md`](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md)

## Core conclusion

Do **not** make the main story fully procedural.

Instead:

- keep the campaign authored
- keep major bosses and narrative set pieces handcrafted
- use a dedicated roguelite mode to provide replayability and long-tail retention

The right structure is:

- **Story/Campaign:** authored
- **Roguelite run mode:** procedural structure built from handcrafted content
- **Short-form rotating missions:** lightweight procedural variation

## Why this is the right fit

Fully procedural story maps would weaken:

- narrative pacing
- faction/world identity
- boss presentation
- tutorialization
- set-piece quality

But a dedicated roguelite lane can solve a real problem:

- repeated daily play becoming stale
- midcore players running out of interesting decisions
- banner units needing new spaces to feel novel

## Recommended mode structure

### 1. Story mode

Keep this mostly authored.

Allow only light variation on repeat clears, such as:

- enemy variant swaps
- optional side objectives
- rotating hazard packages
- bonus event choices after clear

### 2. Roguelite flagship mode

Recommended working name:

- `Yomi Road`
- `Ashen Road`
- `Blood Moon Pilgrimage`

This should be the main replayable run mode.

Core features:

- branching node map
- persistent HP/injury across battles
- temporary blessings, relics, or corruption
- event nodes
- elite fights
- shrine/camp/shop choices
- boss at the end of a path or chapter

### 3. Short-form rotating side mode

Use shorter procedural mission chains for:

- weekly events
- emergency missions
- seasonal challenge ladders

These should be faster than the flagship roguelite mode and easier to rotate.

## Procedural design rule

Do not generate whole maps from nothing.

Instead:

- build a library of handcrafted small tactical maps
- define encounter templates
- let the run system remix those pieces through node order, objectives, hazards, enemy mixes, and reward packages

This gives:

- better quality control
- more readable mobile maps
- easier balance
- stronger biome identity

## Recommended encounter building blocks

Each run encounter should be assembled from reusable “atoms”:

- formation puzzles
- line-of-sight puzzles
- hazard control
- survival pressure
- capture/hold objectives
- protect-target scenarios
- elite pressure fights

These should be layered onto map templates rather than reinvented every stage.

## Node types

Recommended node types for the first version:

- **Battle:** standard fight
- **Elite:** harder fight with better relics
- **Boss:** chapter-ending fight
- **Shrine:** heal, cleanse, or bless
- **Camp:** rest, repair, or train
- **Event:** narrative choice with upside/downside
- **Merchant/Smith:** trade resources for temporary power
- **Ambush/Corruption:** risk-heavy branch with rare rewards

## Run-level power systems

This is where `Shogun` can let players get temporarily overpowered.

Recommended run power categories:

- blessings
- cursed relics
- forbidden techniques
- weapon arts
- elemental boons
- corruption bargains
- squad-wide stances

This layer should be stronger than normal account buffs.

For long-term balance policy, see:

- [`design-003-long-term-balance-and-power-creep-policy.md`](./design-003-long-term-balance-and-power-creep-policy.md)

## Attrition model

The most important carry-over rule should be:

- no automatic full recovery between battles

That creates run tension and makes choices meaningful.

Carry-over systems can include:

- current HP
- injuries or debuffs
- limited consumables
- morale or resolve
- limited revive options

This is one of the cleanest ways to make a run feel different from ordinary story grinding.

## Meta progression policy

Roguelite meta progression should be mostly horizontal.

Recommended unlocks:

- new relic pools
- new event types
- starting loadout options
- extra route choices
- class/school-specific blessings
- visual progression and lore unlocks

Avoid:

- large permanent stat trees
- account-wide damage inflation
- run-starting advantages that invalidate early choices

## How this helps repetition

Roguelite structure fights repetition by varying:

- route choice
- encounter order
- temporary power packages
- risk/reward decisions
- resource pressure

That is much more effective than simply making:

- more nearly identical maps
- more enemies with higher HP
- more daily grind nodes

## How this helps power creep

Roguelite modes are valuable because they move excitement into temporary run power instead of permanent character inflation.

That means:

- old units can still have breakout runs
- new units can be sold as strong *in certain run structures* rather than universally mandatory
- the mode stays fresh without requiring every banner to invalidate the last one

## World and roster integration

The existing `Shogun` world and combat identity layers should feed this mode directly.

Examples:

- `Imperial Court` path nodes emphasize duels, escorts, and politics
- `Ronin Marches` paths emphasize attrition and ambushes
- `Temple and Veil` paths emphasize cleansing, rituals, and curses
- `Yokai Court` paths emphasize illusions, bargains, and charm effects
- `Corrupted Dominion` paths emphasize high risk and high payoff

This keeps runs from feeling system-only and disconnected from the setting.

For world pillars and combat identity, see:

- [`design-002-world-pillars-and-combat-identity-framework.md`](./design-002-world-pillars-and-combat-identity-framework.md)

## First prototype recommendation

Do not build the giant version first.

Prototype scope should be:

- 1 biome or theme
- 1 short node map
- 3–5 handcrafted map templates
- 5–8 relic/blessing options
- 3–5 event nodes
- 1 mini-boss
- 1 boss

That is enough to answer:

- does the run structure feel fresh
- does attrition create tension
- do temporary powers create fun
- does the mode actually reduce repetition

## Relationship to campaign and LiveOps

This mode should support LiveOps, but it should not replace campaign.

Good uses:

- weekly mutators
- seasonal relic pools
- featured boss path variants
- faction-themed runs
- limited-time rewards and cosmetics

Bad use:

- forcing players to run it excessively just to keep up with account progression

## Default policy for future decisions

If choosing between:

- “make story more random”
- or “make a separate run mode richer”

prefer the second.

`Shogun` should let the campaign stay authored and memorable, while using roguelite structure to deliver replayability and daily freshness.

## Related documents

- [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [`design-003-long-term-balance-and-power-creep-policy.md`](./design-003-long-term-balance-and-power-creep-policy.md)
- [`design-005-co-op-pvp-and-social-systems-roadmap.md`](./design-005-co-op-pvp-and-social-systems-roadmap.md)
- [`design-002-world-pillars-and-combat-identity-framework.md`](./design-002-world-pillars-and-combat-identity-framework.md)
- [`../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md`](../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)
- [`../research/doc-ref-003-one-piece-treasure-cruise-analysis.md`](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md)
