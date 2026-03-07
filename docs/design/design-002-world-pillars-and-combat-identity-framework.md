# DESIGN-002: World Pillars and Combat Identity Framework

**Summary:** Working cross-disciplinary structure for how `Shogun` should organize factions, elemental affinity, weapon families, martial schools, and collectible identity without flattening the roster into generic classes.

## Purpose

Use this note when deciding:

- how the world should be divided into major power centers
- how characters should feel different beyond rarity and stats
- how elements, weapon families, and martial schools should coexist
- how to keep roster planning readable like a character collector, not just a lore bible
- how to evolve the current prototype combat taxonomy into a stronger long-term identity system

## Core conclusion

`Shogun` should not rely on only one identity layer.

The roster should be built from **five stacked layers**:

1. **World pillar**
2. **Element**
3. **Weapon family**
4. **Martial school**
5. **Collectible fantasy**

That gives each important unit a clear answer to:

- where they come from
- what power they channel
- how they fight
- what body language they use
- why players want to own them

## Why this structure is needed

If the game only uses broad classes like:

- samurai
- ninja
- monk
- demon

then the roster becomes too flat.

If the game only uses collectible fantasy like:

- tragic noblewoman
- dangerous seductress
- rogue antihero

then the world can become visually appealing but structurally fuzzy.

The stronger model is:

- **world pillars** for setting clarity
- **combat identity layers** for gameplay clarity
- **collectible fantasy** for monetization and attachment

## Inspiration handling

The current target structure is a hybrid of two useful patterns:

### Atmosphere-first model

Some martial-world games work best as lived-in worlds made of:

- regions
- subcultures
- wandering roles
- martial communities

This is useful for tone, travel, and immersion.

### Faction-readable model

Some long-running franchise rosters work because characters are easy to sort by:

- village
- house
- clan
- order
- bloodline

This is useful for banners, rivalry, memory, and roster readability.

`Shogun` should sit between those two models:

- rich worldbuilding
- clear roster buckets

## Proposed world pillars

These are the current strongest candidates for the major world-level buckets.

### 1. Imperial Court

Use for:

- nobles
- ceremonial samurai
- court retainers
- bodyguards
- official onmyoji
- political marriages and succession drama

Aesthetic lane:

- elegance
- hierarchy
- polish
- silk, lacquer, crests, formal blades

Emotional lane:

- nobility
- repression
- tragedy
- rivalry

### 2. Ronin Marches

Use for:

- exiles
- mercenaries
- duelists
- outlaw bands
- frontier warlords
- wandering killers

Aesthetic lane:

- rough cloth
- dust
- patched armor
- practical weapons

Emotional lane:

- survival
- honor lost or remade
- brutality
- freedom

### 3. Temple and Veil Orders

Use for:

- monks
- shrine maidens
- exorcists
- mediums
- ritual guardians
- sacred bureaucrats

Aesthetic lane:

- paper charms
- prayer beads
- bells
- staffs
- layered robes

Emotional lane:

- discipline
- devotion
- secrecy
- purity versus corruption

### 4. Yokai Courts

Use for:

- fox nobles
- serpent houses
- crow envoys
- dream beings
- spirit aristocracy

Aesthetic lane:

- supernatural beauty
- masks
- tails
- antlers
- moonlit finery

Emotional lane:

- seduction
- mystery
- elegance
- danger

### 5. Corrupted Dominion

Use for:

- demon generals
- cursed warlords
- fallen saints
- Yomi-touched champions
- body-horror elites

Aesthetic lane:

- blood
- ash
- broken armor
- ritual scars
- black-gold corruption

Emotional lane:

- domination
- desecration
- terror
- forbidden power

## World pillars versus collectible fantasy

World pillars are not the same thing as collectible fantasy.

Example:

- **Imperial Court** is a world pillar
- **Tragic Nobility** is a collectible fantasy

One world pillar can carry multiple collectible fantasies.

Example mapping:

- `Imperial Court`
  - tragic nobility
  - dangerous seduction
  - occult power
- `Ronin Marches`
  - rogue violence
  - fallen honor
  - frontier tenderness
- `Yokai Courts`
  - yokai elegance
  - corrupted divinity
  - predatory beauty

The roster should be built by combining both layers, not choosing only one.

## Elements

Yes, `Shogun` should keep elemental affinity as a core identity axis.

The project already has a working seven-element prototype model. The design recommendation is to keep that count, but interpret it more clearly:

- **Common natures**
  - Fire
  - Water
  - Earth
  - Wind
  - Lightning
- **Exceptional natures**
  - Ice
  - Shadow

This gives better world logic:

- common elements are trainable and widely legible
- exceptional elements are rarer, lineage-bound, ritual-bound, outsider-coded, or corruption-linked

### Element role

Elements should mainly define:

- broad affinity
- reactions
- narrative symbolism
- VFX language
- faction or bloodline flavor

Elements should **not** carry the entire burden of combat identity by themselves.

## Weapon family versus martial school

The current prototype's `MartialArtsType` is doing two jobs at once:

- weapon family
- fighting-style identity

That is acceptable for the current playable state, but it is too coarse for long-term roster design.

The recommended long-term split is:

### Weapon family

Use for straightforward combat readability:

- Unarmed
- Sword
- Spear
- Bow
- Staff
- Dual Blades
- Heavy Weapons

Weapon family should mostly govern:

- tactical matchup
- range expectation
- target profile
- readable silhouette

### Martial school

Use for deeper identity:

- stance
- motion language
- passive bonuses
- counter style
- stealth profile
- school fantasy and rivalry

Martial school should answer:

- *how* this character fights, not just *what* they hold

## Real-world martial arts as backstage reference

Real-world martial arts are useful as **design references**, but should not usually appear as literal in-world names.

Good usage:

- use them to shape stance, tempo, body language, and school doctrine
- invent fantasy-feudal names for the actual `Shogun` schools

### Reference inspirations worth using

- `Shotokan-like`
  - formal
  - exact
  - disciplined
  - long-line striking
- `Kyokushin-like`
  - brutal
  - conditioned
  - full-contact
  - relentless pressure
- `Uechi-Ryu-like`
  - compact
  - rooted
  - hard-soft
  - close-range animal energy
- `Taekwondo-like`
  - kicking emphasis
  - outsider or peninsula-coded style
- `Chinese martial arts`
  - circular flow
  - animal schools
  - internal and external contrast
  - strong monk or outsider lineages
- `Jiu-jutsu-like`
  - grappling
  - locks
  - throws
  - bodyguard or arrestor energy
- `Ninjutsu-like`
  - infiltration
  - ambush
  - mobility
  - tools and deception

## Candidate martial-school structure

These are stronger as working internal archetypes than as final lore names.

### Native / central schools

- **Court Blade Doctrine**
  - formal sword discipline
  - precision, posture, restraint
- **Iron Mountain School**
  - hard striking, body conditioning, brutal endurance
- **Binding Hand School**
  - grappling, throws, control, counters
- **Veil Step Method**
  - stealth, infiltration, sudden kill windows

### Peripheral / outsider / border schools

- **Southern Serpent School**
  - compact, hard-soft, close-range pressure
- **Peninsula Kicking School**
  - leg-dominant, mobility-heavy, outsider-coded
- **Continental Flow School**
  - circular movement, deceptive rhythm, animal or scholar-warrior flavor

These can later be renamed into more setting-specific final terms.

## Counter structure recommendation

Do not make every layer a hard-counter system.

That creates unreadable design.

Recommended division:

- **Element**
  - broad affinity and reactions
- **Weapon family**
  - tactical matchup and range logic
- **Martial school**
  - passives, timing, counters, stance identity, animation feel

In other words:

- elements and weapons can carry most of the matchup logic
- martial schools should carry most of the flavor and mastery logic

## Identity stack examples

### Example 1

- World pillar: `Imperial Court`
- Element: `Wind`
- Weapon family: `Sword`
- Martial school: `Court Blade Doctrine`
- Collectible fantasy: `Tragic Noble Duelist`

### Example 2

- World pillar: `Ronin Marches`
- Element: `Shadow`
- Weapon family: `Dual Blades`
- Martial school: `Veil Step Method`
- Collectible fantasy: `Dangerous Seductress Assassin`

### Example 3

- World pillar: `Temple and Veil Orders`
- Element: `Earth`
- Weapon family: `Unarmed`
- Martial school: `Iron Mountain School`
- Collectible fantasy: `Ascetic Monster-Killer`

## Current implementation implication

The project already has:

- a seven-element implementation
- a seven-weapon-style implementation
- martial/elemental effectiveness logic

That means the current recommendation is **not** to rip out the prototype and rebuild it immediately.

Instead:

1. keep the existing playable taxonomy for now
2. use this document as the higher-level content-planning framework
3. later, when combat systems mature, decide whether `MartialArtsType` should be split into:
   - `WeaponType`
   - `MartialSchool`

This keeps the project stable while still improving design coherence.

## Working rule going forward

When planning a new important unit, define the following before generating art or abilities:

1. world pillar
2. element
3. weapon family
4. martial school
5. collectible fantasy

If one of those fields is vague, the character is still under-designed.

## Related documents

- `DESIGN-001` for collectible fantasy, variant value, and battle-vs-presentation character lanes
- `ART-001` for silhouette, palette, and readability constraints
- `ART-006` for sex appeal, damage-art, and safe fanservice boundaries
- `DOC-GDD-001` for product-scope truth
- `DOC-REF-001` for reference patterns only
