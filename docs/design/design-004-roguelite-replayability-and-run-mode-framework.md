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

## Event-vignette presentation lane

`Shogun` should give its roguelite event nodes a dedicated presentation lane instead of showing them as plain text-only popups.

Reference target:

- `Slay the Spire`-style event scenes with a strong image, a short narrative setup, and 2-4 meaningful choices

Recommended approach:

- use one strong static vignette image per event family
- pair it with concise atmospheric text and sharply differentiated choices
- reuse the same event image across multiple text/choice variations when they belong to the same fantasy family

This gives the mode:

- more atmosphere
- stronger perceived production value
- better memory anchors for repeat runs
- more room for worldbuilding without requiring full new battle maps

### What these event scenes should do

They should support:

- yokai bargains
- cursed shrines
- ronin duels
- haunted crossroads
- hidden dojos
- execution grounds
- temple trials
- relic discoveries

They should not become:

- giant one-off paintings for every tiny encounter
- long VN-style cutscenes
- production bottlenecks that slow down content iteration

### Production rule

Treat event vignette art as a reusable content layer.

Build:

- `5-8` event images for the first prototype
- each image tied to an event archetype, not one single event

Example:

- one `Yokai Bargain` image can support several different shrine, charm, dream, or seduction events
- one `Blood Moon Duel` image can support multiple ronin challenge variants
- one `Temple Trial` image can support cleansing, sacrifice, blessing, or corruption choices

### Art workflow for vignette events

Recommended stack:

- `Gemini web chat / Nano Banana Pro` for concept composition and collectible-facing mood exploration
- `PixelLab` for style-matched pixel finalization
- `Unity` for event UI framing and readability validation

Unlike battle sprites, these vignettes do not need directional animation coverage.

That makes them one of the highest-value uses of the AI-assisted art lane:

- high atmosphere
- moderate production cost
- low gameplay integration complexity

For the production-facing art workflow, see:

- [`../art/art-008-roguelite-event-vignette-art.md`](../art/art-008-roguelite-event-vignette-art.md)

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

## Delivery ladder: from prototype to real mode

This mode should be built in layers, not as a giant content explosion.

### Slice 0: Internal paper-to-playable prototype

**Goal**

Prove that a short run is more interesting than replaying ordinary authored maps.

**What exists**

- one run path
- one biome theme
- 3–5 map templates
- 3 node types at most
- temporary relic or blessing picks
- persistent HP between fights

**Best first node set**

- `Battle`
- `Shrine`
- `Boss`

This is enough to validate:

- attrition
- route pacing
- run-level power
- replay value

### Slice 1: True v1 run

**Goal**

Ship a version players can understand and replay without the mode feeling like a placeholder.

**What exists**

- short branching node map
- 5 node types
- 1 mini-boss
- 1 boss
- 5–8 relics/blessings
- 3–5 events
- one clear reward loop

Recommended node set for v1:

- `Battle`
- `Elite`
- `Shrine`
- `Event`
- `Boss`

### Slice 2: Seasonalized run mode

**Goal**

Make the mode part of ongoing retention instead of a one-time novelty.

**What exists**

- rotating relic pools
- biome variants
- seasonal mutators
- featured bosses
- score or depth milestones

This is where LiveOps can start treating the mode as a recurring pillar.

## Recommended first run structure

For the first real player-facing version, keep the run short and readable.

Recommended map:

1. opening battle
2. branching choice: shrine or battle
3. event node
4. elite fight
5. branching choice: shrine or risk node
6. boss

That gives you:

- one meaningful route choice early
- one pressure spike in the middle
- one recovery decision
- one strong finale

It is enough to feel like a run without demanding twenty-minute setup comprehension.

## Reward model

The reward structure matters as much as the map structure.

### What the mode should reward

- account materials
- limited premium currency in controlled amounts
- relic-unlock progress
- cosmetics or titles
- lore fragments
- seasonal milestone rewards

### What the mode should not become

- mandatory grind for all core progression
- the only meaningful source of top-end materials
- a giant time tax required to stay competitive in every other mode

### Good reward philosophy

- first clear rewards should feel strong
- repeat rewards should be worthwhile but not mandatory
- high-skill or deep-run rewards should feel prestigious
- cosmetics and flex rewards fit this mode especially well

## Run economy

Runs should have their own temporary economy.

Recommended currencies/resources inside a run:

- `Resolve`
  - general run health / morale / recovery resource
- `Relic Fragments`
  - spend at shrines or smiths for temporary power
- `Corruption`
  - high-risk resource that unlocks strong power at a cost

This keeps decisions inside the run meaningful without bloating the permanent economy.

## First relic/blessing categories

Do not create dozens first.

Start with 5 narrow categories:

1. **Elemental boons**
   - stronger burn, freeze, shock, cleanse, etc.
2. **Weapon arts**
   - sword crit line, spear control line, bow range line
3. **Stance relics**
   - formation, guard, initiative, tempo
4. **Recovery / survival**
   - healing, shields, revive edge, attrition management
5. **Corruption bargains**
   - high upside, clear downside

That is enough to create recognizable builds without turning the mode into a spreadsheet.

## Event-node philosophy

Event nodes are where the mode gets personality.

They should do more than hand out loot.

Good event outcomes:

- choose healing now vs stronger boss reward later
- accept corruption for a rare relic
- rescue an NPC for a support bonus
- swear loyalty to a world pillar for themed bonuses and penalties
- choose between two rival blessings

This is one of the strongest ways to make the run feel like `Shogun`, not a generic roguelite overlay.

## Success criteria for the mode

The mode is working if:

- players make meaningful route decisions
- players talk about builds, not just one solved team
- old and new units can both shine through different relic packages
- repeated runs still produce noticeably different tension and outcomes
- the mode creates excitement without requiring huge permanent stat inflation

## Failure states

The mode is drifting in the wrong direction if:

- it becomes just another HP gauntlet
- rewards force players to grind it constantly
- route choices feel fake
- one relic combination dominates every run
- permanent account power matters more than run decisions
- it feels disconnected from the setting and faction fantasy

## Explicitly defer for later

Do not build these in the first versions:

- giant node maps
- dozens of relic categories
- full procedural map generation
- permanent roguelite talent trees with big stat inflation
- endless mode with no pacing discipline
- online co-op inside the roguelite mode
- PvP-tied roguelite rewards

The first job is to prove:

- this mode is fun
- this mode reduces repetition
- this mode helps long-term balance

Not to make the biggest possible system immediately.

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
