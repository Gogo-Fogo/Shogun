# DESIGN-007: Range Circles and Threat Geometry Framework

**Summary:** Formalizes range circles as one of `Shogun`'s core combat differentiators, explains why they made `Naruto Blazing` feel good, and defines how short / mid / long threat should shape unit identity, counterplay, encounter design, and long-term combat readability.

## Why this note exists

The current project already contains the right raw ingredients:

- the GDD treats readable range circles as part of the prototype's identity
- the `Naruto Blazing` reference explicitly calls out circular attack ranges as a core engagement rule
- the runtime code already defines `Short`, `Mid`, and `Long` attack radii

But those ideas are still spread across:

- `DOC-GDD-001`
- `DOC-REF-001`
- `CharacterDefinition.cs`
- `CharacterStats.cs`
- `IMPLEMENTATION_PROGRESS.md`

This note turns them into one enforceable design framework.

## Core thesis

`Shogun` should treat **range circles and threat geometry** as one of its primary combat pleasures.

The goal is not only:

- "units can attack from different distances"

The goal is:

- positioning should feel legible
- threat should be easy to read at a glance
- circle overlap should create meaningful tactical tension
- range identity should make units feel different before stats or rarity enter the picture

In short:

- `ranges are not UI decoration`
- `ranges are part of the combat fantasy`

## Current truth set

### Movement

Current project truth:

- movement is **free-form on the battlefield**
- movement is **not** currently defined by a movement circle in the active implementation notes

This was intentionally clarified in prior documentation cleanup.

### Basic attack range

Current live code defines:

- `Short` = `1.5`
- `Mid` = `3.0`
- `Long` = `5.0`

Source:

- `Assets/_Project/Scripts/Features/Characters/CharacterStats.cs`

### Current project rule

For the current combat kernel:

- movement is free-form
- basic attack threat is circular
- counters and target safety should eventually care about whether a unit ends in enemy threat

### Skill shapes

Basic attack range should stay circular and easy to read.

Special abilities are allowed to break out of the simple circle rule later through:

- lines
- cones
- crosses
- bursts
- chains
- directional sweeps

That mirrors the `Naruto Blazing` lesson: **standard attacks stay readable; special attacks create expressive exceptions.**

## Why this was fun in Naruto Blazing

From the reference study, `Blazing` got a lot of tactical feel from a simple visual promise:

- every unit projects threat
- that threat is spatially legible
- team attacks happen when positions and ranges line up

That creates several kinds of satisfaction:

### Threat forecasting

Players can quickly judge:

- can I reach this target
- can this enemy reach me
- how many enemy circles overlap this tile

That lowers cognitive sludge while preserving tactical depth.

### Positioning tension

A tile is rarely just "closer" or "farther."

It can also be:

- inside one enemy circle
- outside another
- good for a link / team attack
- bad because it invites a counter

That makes movement decisions feel rich without requiring giant rules text.

### Unit identity through space

Different range bands immediately change how a character feels:

- short-range units feel brave, violent, risky
- mid-range units feel flexible and dependable
- long-range units feel controlling, surgical, and positional

That is good game feel before raw numbers even matter.

### Overlap as a design language

When circles overlap, several good things become possible:

- kill boxes
- trap setups
- ally synergy
- boss-zone pressure
- meaningful support positioning

This is one of the cleanest ways to make tactical combat feel "smart" on mobile.

## Design goals for Shogun

`Shogun` should use range circles to achieve five goals:

1. **Immediate battlefield readability**
2. **Meaningful unit identity**
3. **Predictable but tense counterplay**
4. **Low-friction mobile tactics**
5. **A strong visual combat signature**

If players remember `Shogun` combat, one of the things they should remember is:

- "I always knew where the danger was, but positioning inside that danger was still hard."

## The three core range bands

Do **not** add too many basic range bands early.

`Short`, `Mid`, and `Long` are enough for the main combat grammar.

### Short range

Current radius target:

- `1.5`

Fantasy:

- killers
- duelists
- brawlers
- committed melee units

Typical feel:

- highest exposure
- highest commitment
- strongest punish if they catch you

Good fit:

- unarmed
- sword duelists
- dual daggers
- heavy-weapon crushers

### Mid range

Current radius target:

- `3.0`

Fantasy:

- balanced tacticians
- flexible skirmishers
- standard reliable fighters

Typical feel:

- easiest to use
- best generalist threat
- strongest "default" range band

Good fit:

- many swords
- some staffs
- mixed-role officers
- support attackers

### Long range

Current radius target:

- `5.0`

Fantasy:

- battlefield controllers
- zone denial units
- archers
- polearm specialists
- ritual casters

Typical feel:

- lower safety if caught
- high value in formation fights
- strongest influence on map geometry

Good fit:

- bows
- spears
- some staffs
- specialist yokai or ritualists

## Weapon-family expectations

The current code only stores one `AttackRange` enum per unit. That is fine for the current stage.

But design-wise, weapon family should still imply a likely default range behavior.

### Sword

Default expectation:

- short to mid

Design role:

- dependable melee pressure
- strong counter identity
- duel-oriented threat

### Spear

Default expectation:

- mid to long

Design role:

- keeps enemies at edge distance
- rewards formation discipline
- should feel different from bow even when both are "long"

### Bow

Default expectation:

- long

Design role:

- clean ranged pressure
- vulnerable when engaged
- should not simply be "best range for free"

Potential later rule:

- weak or no adjacent counter

### Staff

Default expectation:

- mid or long

Design role:

- support, elemental, ritual, control
- can use irregular skill shapes while keeping readable base range

### Dual daggers

Default expectation:

- short

Design role:

- high-risk burst
- flankers
- assassins
- stealth pressure

### Heavy weapons

Default expectation:

- short

Design role:

- terrifying if allowed into contact
- not mobile comfort picks
- range weakness should be part of their identity

### Unarmed

Default expectation:

- short

Design role:

- disciplined melee pressure
- counter, stance, interrupt, or control identity

## Threat geometry rules

### Rule 1: Threat must be legible

The player should quickly understand:

- current unit range
- enemy threat range
- where overlap becomes dangerous
- where counters are likely

If the geometry is unclear, the system loses its main advantage.

### Rule 2: Overlap should matter

A tile inside multiple circles should feel meaningfully different from a tile inside one circle.

Use overlap to support:

- enemy focus fire
- ally link attacks
- boss hazard stacking
- frontline / backline tension

### Rule 3: Circles are the main grammar, exceptions are special

Do not muddy the baseline with too many odd shapes for basic attacks.

The player should internalize:

- basic attacks = predictable circular threat
- special skills = expressive exceptions

### Rule 4: Long range must have tradeoffs

Long range should never mean:

- safest
- strongest
- easiest
- best against everything

Tradeoffs can come through:

- weaker adjacent counter
- lower base defense
- terrain dependence
- charge windows
- commitment after firing

### Rule 5: Short range must feel worth the risk

Short-range units should not simply be "worse because they must walk closer."

They need compensation through:

- damage
- counter threat
- mobility tools
- on-hit control
- stance or guard break

### Rule 6: Mid range is the anchor band

Mid range should be the healthiest generalist band.

It is where:

- most players learn the geometry
- most encounters remain readable
- most standard units feel fair

## Counterattack implications

Counter rules are one of the best ways to make ranges matter.

Design principle:

- entering or ending inside enemy threat should be a meaningful risk

This can later support:

- melee units punishing reckless entry
- ranged units avoiding counter zones
- certain schools or weapons being known for oppressive counter posture

But counter rules must stay readable.

The player should never need to guess:

- whether a counter will trigger
- from how far
- why this unit can counter while another cannot

## Encounter design implications

Range circles should shape encounter design directly.

### Good encounter uses

- choke points where spear and bow control matter
- flank routes where dagger units can exploit long-range backlines
- shrine or hazard zones where stepping into a circle is a meaningful trade
- boss arenas with rotating safe / unsafe distances
- formations that reward breaking enemy overlap before advancing

### Weak encounter uses

- flat arenas where range barely matters
- maps where everyone can hit everyone easily
- obstacle layouts that do not actually change threat geometry
- giant movement freedom that trivializes circle positioning

## UI and readability requirements

If range circles are a core differentiator, the UI must support them aggressively.

Recommended clarity rules:

- active unit range circle always readable
- enemy threat preview readable before confirm
- counter-capable enemies visually marked
- overlap zones show stronger visual treatment
- skill shapes and basic circles are visually distinct
- obstacles / line blockers should be obvious

Later UI opportunities:

- quick toggle for "show all enemy threat"
- color coding by range band
- threat heatmap mode
- preview of where moving will expose the unit to counters

## Long-term expansion path

Do not expand basic range complexity too early.

Start with:

- short / mid / long circles
- counter awareness
- overlap-based tactics

Then later add:

- min-range constraints
- no-counter tags
- piercing or ally-overreach attacks
- delayed zone creation
- stance-modified range
- boss-only custom range behaviors

That keeps the combat kernel strong before adding exceptions.

## Anti-patterns to avoid

Avoid these failures:

### 1. Range inflation

If too many units reach too far, the map collapses and positioning stops mattering.

### 2. Hidden threat

If enemy circles are unreadable or rules are inconsistent, players feel cheated rather than challenged.

### 3. Long-range dominance

If ranged units control space with no downside, melee fantasy collapses.

### 4. Tiny meaningless differences

Do not create many micro-range categories whose differences are invisible in play.

### 5. Special-shape overload

If every attack shape is unique, the baseline tactical clarity disappears.

## Practical guidance for current implementation

For the current stage of `Shogun`, the safest design guidance is:

1. keep `Short`, `Mid`, and `Long` as the only basic attack bands
2. keep their radii easy to remember
3. make long-range identity meaningful but not dominant
4. make short-range units scary enough to justify exposure
5. ensure counter behavior and threat previews become part of combat readability
6. use skill shapes as exceptions, not as replacements for the range-circle grammar

## Decision rule

When deciding whether a combat feature supports the game's identity, ask:

- does this make range circles more meaningful?
- does this make threat easier to understand but harder to master?
- does this make unit identity more spatially distinct?

If the answer is "no," it is probably not strengthening the combat signature that made `Naruto Blazing`'s range design memorable.
