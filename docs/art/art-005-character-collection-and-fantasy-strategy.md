# ART-005: Character Collection and Fantasy Strategy

**Summary:** Research-backed strategy note for making `Shogun` characters desirable to collect, not just readable to play.

## Purpose

Use this note when deciding:

- what kind of characters `Shogun` should prioritize
- how to split battle readability from collection appeal
- what makes a unit feel collectible instead of interchangeable
- how popularity, costumes, and variants should fit the roadmap

## Core conclusion

For a gacha RPG, art is not only presentation. It is part of the monetization and retention loop.

The outside research is consistent on three points:

1. randomized reward systems create excitement around rare outcomes and can increase impulsive purchase intent
2. players form emotional and parasocial attachments to virtual characters, and those attachments can increase engagement and spending
3. successful live gacha games operationalize that attachment through popularity polls, special outfits, and recurring character variants

For `Shogun`, this means character art has to do two jobs at once:

- communicate clearly in battle
- create collectible fantasy outside battle

## What the research implies

### Rare outcomes are emotionally potent

Research on blind-box uncertainty found that uncertainty can increase impulsive purchase intention, with curiosity acting as part of the mechanism. In practical terms: rarity and surprise are not side effects, they are part of the product appeal.

### Gacha communities reward visible rarity and attachment

Recent research on gacha and problem-gambling risk notes that rare items can signal status and commitment inside player communities, and that emotionally resonant narratives can create strong attachments that drive spending beyond purely rational utility.

### Parasocial attachment matters

Academic work on parasocial relationships in games and adjacent media consistently points in the same direction: players can use virtual characters as emotionally meaningful attachment objects, safe companions, or identity anchors. That does not just affect addiction risk. It also affects what kinds of characters people remember, discuss, and spend for.

### Aesthetic identity matters

Research on the "Aesthetic Self" supports the idea that aesthetic taste is part of how people understand and express identity. For a gacha game, this means players are not only collecting stats. They are collecting identity-signaling fantasies.

### The strongest games turn attachment into content loops

Successful gachas do not stop at "here is a popular character." They repeatedly transform popularity into:

- alternate costumes
- seasonal variants
- themed reruns
- event focus
- player-voted rewards

`Fire Emblem Heroes`, `NIKKE`, and `Bleach: Brave Souls` all provide official examples of this pattern.

## Strategic implication for Shogun

`Shogun` should not operate with a single art lane.

It needs two lanes:

### Lane 1: battle assets

Purpose:

- readable at gameplay scale
- production-efficient
- animation-friendly

Default standard:

- `64x64`
- `4 directions`
- strong silhouette
- controlled palette

### Lane 2: collection assets

Purpose:

- make the character desirable
- carry rarity fantasy
- communicate personality, beauty, danger, or tragedy
- support banners, profile screens, summon reveals, and future variants

Suggested asset types:

- portrait or bust art
- summon or reveal key art
- card or profile presentation art
- later: costume or alt concept sheets

If Lane 1 makes the unit playable, Lane 2 makes the unit collectible.

## Character design rule

Every important `Shogun` character should be built as a **collection package**, not just a sprite.

Each package should include:

1. **Battle role**
   - what the unit does mechanically
2. **Collectible fantasy**
   - what kind of desire the unit is meant to trigger
3. **Silhouette hook**
   - what makes the unit identifiable at a glance
4. **Presentation hook**
   - face, hair, pose, costume, or aura that makes the unit memorable in art
5. **Palette signature**
   - one controlled accent that helps recognition and rarity coding
6. **Emotional hook**
   - tragedy, elegance, danger, devotion, mystery, tenderness, arrogance, etc.
7. **Variant potential**
   - whether the unit can support future costumes, seasonal themes, corrupted forms, younger/older forms, festival attire, and so on

If a unit only has gameplay function and no collectible fantasy, it is roster filler.

## Candidate collectible-fantasy pillars for Shogun

These are the most promising starting pillars for a dark-fantasy feudal-Japan gacha:

1. **Tragic Nobility**
   - elegant samurai, doomed heirs, shrine maidens under burden, grief-coded beauty
2. **Rogue Violence**
   - ronin, assassins, outcasts, executioners, duelists, scarred antiheroes
3. **Yokai Elegance**
   - supernatural women and men whose appeal is beauty mixed with threat
4. **Corrupted Power**
   - demonic warlords, cursed commanders, blood-soaked champions, possession motifs
5. **Mystic Ritual**
   - onmyoji, monks, mediums, fox-priests, occult tacticians

These pillars are useful because they support both:

- clean battle silhouettes
- strong collection-facing fantasies

## Roster structure recommendation

Do not build the cast as a flat spread of unrelated cool designs.

Build the cast as a set of recognizable fantasy families:

- each family needs a clear shape language
- each family needs a color language
- each family needs a social or mythic role in the world
- each family should support future alts without breaking coherence

Recommended first structure:

- `Core cast pillars`: 4 to 5 fantasy families
- `Per pillar`: 3 to 6 launch-relevant characters with different rarity targets
- `Per high-value character`: at least one future alt or costume path identified early

This helps both content planning and player memory.

## Popularity and variant planning

Do not wait until after launch to think about alts.

Plan variant potential up front.

For each high-value character, define:

- base fantasy
- one darker or corrupted variant
- one ceremonial, festival, or elite-attire variant
- one seasonal or event-compatible variant, only if it still fits the world

The point is not to commit to all of them immediately. The point is to avoid designing dead-end characters that cannot carry future banner value.

## PixelLab evaluation implication

Because `Shogun` needs both playability and collectibility, PixelLab should be judged on both lanes.

The first month should not only answer:

- can PixelLab help make battle sprites?

It should also answer:

- can PixelLab help create attractive character presentation assets?

Recommended evaluation order:

1. prove one `64x64` battle-sprite workflow
2. on the same character, generate one larger presentation-facing portrait or bust
3. compare cleanup burden for both lanes
4. decide whether PixelLab is useful only for battle sprites, only for presentation art, or for both

If PixelLab only helps on one lane, that is still strategically useful.

## Practical first-month plan

### Phase 1: define the first collectible-fantasy target

Start with one archetype that is both readable and desirable:

- dark ronin
- tragic samurai woman
- elegant fox-yokai
- cursed onmyoji

### Phase 2: create the same character in two forms

- battle sprite: `64x64`, `4 directions`, `idle + walk`
- presentation art: bust or portrait, larger canvas, same identity and palette

### Phase 3: score the result

Evaluate both outputs on:

- silhouette clarity
- character memorability
- emotional tone
- cleanup time
- consistency between battle and presentation forms
- whether the character feels banner-worthy

## Decision rules

### Continue the workflow if:

- the battle sprite reads clearly in Unity
- the portrait or bust has real collectible appeal
- cleanup feels like refinement rather than re-illustration

### Reassess the workflow if:

- battle assets are readable but the collection-facing art feels generic
- the collection-facing art is attractive but the battle form collapses
- identity does not survive translation between the two lanes

## Legal and provenance rule

Collection-facing assets matter more for marketing, identity, and possible future monetization. Because of that, they deserve stricter provenance and stronger manual editing than throwaway internal tests.

Follow:

- `ART-004` for provenance and source tracking
- `DOC-LEGAL-001` for AI-asset risk and human-authorship constraints
- `DOC-OPS-004` for PixelLab operating boundaries

## Sources

- Blind-box uncertainty and impulsive purchase intention:
  - https://www.frontiersin.org/journals/behavioral-neuroscience/articles/10.3389/fnbeh.2022.946337/full
- Gacha, social quality of life, status signaling, and emotional attachment:
  - https://www.frontiersin.org/journals/psychiatry/articles/10.3389/fpsyt.2025.1663328/full
- Parasocial relationships with virtual characters and game engagement:
  - https://www.mdpi.com/1660-4601/19/24/16909
- Aesthetic identity:
  - https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2020.577703/full
- Character attachment patterns in games including `Genshin Impact`:
  - https://www.essays.se/essay/78c14223a6/
- Official examples of popularity being turned into future character/outfit content:
  - https://vote10.campaigns.fire-emblem-heroes.com/en-US/cyl/about
  - https://nikke-en.com/events/rapiredhoodpoll/
  - https://nikke-en.com/events/nikkevote2025
  - https://www.bleach-bravesouls.com/en/news/240119/
  - https://www.bleach-bravesouls.com/en/news/231114/
