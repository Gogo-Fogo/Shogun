# DESIGN-001: Character Collection and Fantasy Strategy

**Summary:** Research-backed cross-disciplinary note for making `Shogun` characters desirable to collect, not just readable to play.

## Purpose

Use this note when deciding:

- what kind of characters `Shogun` should prioritize
- how to split battle readability from collection appeal
- what makes a unit feel collectible instead of interchangeable
- how popularity, costumes, and variants should fit the roadmap
- how Gemini and PixelLab should split concept versus playable-asset work

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

## What "character fantasy" means

When this document says "pick one character fantasy," it does **not** mean "pick a class."

It means: decide the exact kind of desire, mood, and identity the character is supposed to trigger before generating anything.

Weak version:

- female samurai
- male ninja
- demon boss

Usable version:

- tragic noble samurai widow in moonlit ceremonial armor
- scarred ronin executioner with restrained cruelty
- elegant fox-yokai courtier with hidden threat
- cursed onmyoji prodigy whose body shows ritual corruption

A production-ready character fantasy should define:

1. **Role**
   - samurai, ronin, onmyoji, yokai, demon, etc.
2. **Tone**
   - tragic, seductive, noble, cruel, serene, feral, devotional
3. **Visual hook**
   - crescent helm, fox mask, prayer beads, blood-red scarf, antlers, white hair
4. **Emotional hook**
   - grief, hunger, arrogance, tenderness, obsession, duty, corruption
5. **Rarity fantasy**
   - why this feels premium or banner-worthy rather than generic roster filler

If the character fantasy is vague, the generated art will also be vague.

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

## Gemini and PixelLab workflow implication

Because `Shogun` needs both playability and collectibility, the current recommended split is:

- `Gemini web chat / Nano Banana Pro`: concept and presentation lane
- `PixelLab`: playable pixel-production lane

Use Gemini for:

- portraits
- busts
- full-body concept art
- costume exploration
- mood, expression, and collectible appeal

Use PixelLab for:

- converting approved concept art into playable pixel characters
- generating directional sprite views
- keeping sprite style and animation work consistent

This means the first month should answer two different questions:

- can Gemini produce a collectible-facing concept for this character?
- can PixelLab turn that same identity into a strong playable sprite?

Recommended evaluation order:

1. generate one portrait, bust, or full-body concept in Gemini
2. freeze the approved character identity
3. prove one `64x64` playable-sprite workflow in PixelLab for that same character
4. compare cleanup burden and identity retention across the two lanes
5. decide whether the combined workflow is strong enough to scale

## Practical first-month plan

### Phase 1: define the first collectible-fantasy target

Start with one archetype that is both readable and desirable:

- dark ronin
- tragic samurai woman
- elegant fox-yokai
- cursed onmyoji

### Phase 2: create the same character in two forms

- battle sprite: `64x64`, `4 directions`, `idle + walk`
- presentation art: Gemini-generated bust, portrait, or full-body concept with the same identity and palette

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
- the Gemini concept art has real collectible appeal
- cleanup feels like refinement rather than re-illustration

### Reassess the workflow if:

- battle assets are readable but the Gemini concept feels generic
- the Gemini concept is attractive but the battle form collapses
- identity does not survive translation between the two lanes

## Legal and provenance rule

Collection-facing assets matter more for marketing, identity, and possible future monetization. Because of that, they deserve stricter provenance and stronger manual editing than throwaway internal tests.

Keep Gemini dev-side only:

- use it during development
- do not build the shipped game around live Gemini API calls
- do not route player prompts or runtime asset generation through Gemini in the released product

Reason:

- live AI generation expands privacy, moderation, support, and platform-policy scope
- development-side concept generation is far easier to control and document

Operational rule:

- use Gemini to create candidates
- select and refine manually
- use PixelLab and Aseprite to productionize the approved identity
- track provenance for anything shipped

Follow:

- `ART-004` for provenance and source tracking
- `DOC-LEGAL-001` for AI-asset risk and human-authorship constraints
- `DOC-OPS-004` for PixelLab operating boundaries

## Related documents

- `DESIGN-002` for world pillars, elements, weapon families, and martial schools
- `ART-001` for the visual target, silhouette rules, and palette discipline
- `ART-006` for sex appeal, damage-art, and fanservice boundaries
- `DOC-GDD-001` for overall product-scope truth
- `DOC-REF-001` for roster/combat inspiration only

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
- Gemini image generation and privacy guidance:
  - https://blog.google/innovation-and-ai/products/nano-banana-pro/
  - https://ai.google.dev/gemini-api/docs/image-generation
  - https://support.google.com/gemini/answer/14286560?hl=en
  - https://support.google.com/gemini/answer/13594961
- Official examples of popularity being turned into future character/outfit content:
  - https://vote10.campaigns.fire-emblem-heroes.com/en-US/cyl/about
  - https://nikke-en.com/events/rapiredhoodpoll/
  - https://nikke-en.com/events/nikkevote2025
  - https://www.bleach-bravesouls.com/en/news/240119/
  - https://www.bleach-bravesouls.com/en/news/231114/
