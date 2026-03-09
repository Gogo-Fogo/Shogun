# First Vertical Slice Roster and Encounter Plan

> Status date: March 9, 2026  
> Purpose: turn the active vertical slice into a concrete first battle roster, enemy set, and result/reward handoff.

## Why this exists

[DESIGN-008](./design-008-active-vertical-slice-definition.md) defines the active slice at a high level.

This note answers the next practical question:

- which units should be in the slice
- which enemy archetypes should exist first
- what encounter should prove the combat loop
- what the minimal reward/result handoff should be

Use this together with:

- [DESIGN-008](./design-008-active-vertical-slice-definition.md)
- [DOC-OPS-007](../ops/doc-ops-007-stage-1-unity-project-reality-audit.md)
- [DESIGN-007](./design-007-range-circles-and-threat-geometry-framework.md)
- [ART-005](../art/art-005-legacy-and-production-asset-separation-policy.md)

## Current grounded constraints

Current repo truth:

- only one real `CharacterDefinition` asset is currently present:
  - `Ryoma_CharacterDefinition`
- `Dev_Sandbox` is the only meaningful gameplay scene
- the runtime battle loop is still thin
- the character art archive already contains many named legacy lanes that can be used as source material for the first few definitions

Important implication:

- the first slice should not wait for a broad roster
- it should define the smallest believable team and enemy set that prove combat identity

## Recommended player roster for Slice 1

The first player roster should be `3` units.

That is the best size because it proves:

- team composition
- turn rotation
- range overlap
- more than one combat identity

without creating unnecessary content burden.

### 1. Ryoma

Status:

- required
- already exists as a real `CharacterDefinition`
- already has a prefab and some runtime wiring

Recommended role in the slice:

- stable melee anchor
- baseline samurai duelist

Recommended combat identity:

- `Element`: keep current existing definition value unless a future rebalance changes it
- `Weapon Family`: Sword
- `Range`: Mid
- `Job`: straightforward frontliner / reference baseline

Why he belongs:

- he is the only fully grounded definition already in the repo
- he gives the slice a stable baseline to compare other units against

### 2. Kuro

Status:

- recommended new playable definition
- grounded by existing legacy art naming under the `Ninja` lane

Recommended combat identity:

- `Element`: Wind or Shadow
- `Weapon Family`: DualDaggers
- `Range`: Short
- `Job`: mobile skirmisher / finisher

What he proves:

- short-range circle play
- speed and flanking identity
- ninja-style action rhythm distinct from Ryoma

### 3. Tsukiko

Status:

- recommended new playable definition or support-leaning third slot
- grounded by existing legacy art naming under the `Yokai` lane

Recommended combat identity:

- `Element`: Ice or Shadow
- `Weapon Family`: Staff
- `Range`: Long
- `Job`: ranged debuff / support / control

What she proves:

- long-range circle play
- non-samurai identity
- element/status flavor beyond direct melee trades

## Recommended first enemy set

The first slice should use `3` enemy archetypes, but not necessarily all at once.

That is enough to prove encounter readability and counterplay without needing a full bestiary.

### Enemy Archetype A: Ronin Footman

Recommended identity:

- `Element`: Earth or Fire
- `Weapon Family`: Sword
- `Range`: Mid
- `Job`: disposable melee line holder

Purpose:

- easy baseline enemy
- teaches positioning and range-circle fundamentals

### Enemy Archetype B: Oni Brute

Recommended identity:

- `Element`: Fire
- `Weapon Family`: HeavyWeapons
- `Range`: Short
- `Job`: slow pressure / burst threat

Purpose:

- proves threat-circle danger
- creates urgency around spacing and focus fire

Legacy source guidance:

- use the demon lane first, especially `Akaoni`-style material if it fits

### Enemy Archetype C: Yurei Caster

Recommended identity:

- `Element`: Shadow or Ice
- `Weapon Family`: Staff
- `Range`: Long
- `Job`: ranged pressure / debuff / attrition threat

Purpose:

- proves backline threat
- makes ranged overlap matter
- stops the slice from being a pure melee scrum

Legacy source guidance:

- use the yokai or demon lane first, especially `Yurei`-style material if it fits

## Recommended first authored encounter

The first slice should prove a single authored battle:

### Encounter Name

`Courtyard Ambush`

### Player Team

- Ryoma
- Kuro
- Tsukiko

### Enemy Team

- 1 Ronin Footman
- 1 Oni Brute
- 1 Yurei Caster

### Win Condition

- defeat all enemies

### Loss Condition

- all player characters defeated

### Why this encounter is the right first proof

It tests all three range bands immediately:

- short: Kuro vs Oni pressure
- mid: Ryoma vs ronin baseline
- long: Tsukiko vs Yurei ranged threat

It also forces:

- target-priority decisions
- zone overlap awareness
- at least a small amount of team-role thinking

## Recommended content creation order

Do not try to make every slice unit look final before the loop works.

### Step 1

Keep Ryoma as the anchor and make sure his existing battle flow still works in `Dev_Sandbox`.

### Step 2

Create two additional `CharacterDefinition` assets:

- `Kuro_CharacterDefinition`
- `Tsukiko_CharacterDefinition`

### Step 3

Create minimal enemy definitions:

- `RoninFootman_CharacterDefinition`
- `OniBrute_CharacterDefinition`
- `YureiCaster_CharacterDefinition`

### Step 4

Use the generic runtime shell where possible.

Do not block the slice on bespoke prefabs for every unit unless the generic shell becomes a true blocker.

### Step 5

Only after the battle loop works, start promoting one unit into the new `Source -> Production` asset pipeline as the reference production path.

## Minimum art commitment for Slice 1

The first slice does not need final art for every participant.

Recommended minimum:

- `Ryoma` can remain the baseline reference unit
- one of `Kuro` or `Tsukiko` should become the first art-pipeline proof unit
- enemies can initially use legacy/archive-derived visuals if they are readable

This keeps the art workload honest.

## Minimal reward and result handoff

The first slice should not wait for a real economy backend.

It does need a minimal result state with a stable payload.

### Minimum win payload

- encounter name or id
- victory state
- placeholder experience reward
- placeholder progression token or test reward marker
- replay option
- return-to-sandbox option

### Minimum loss payload

- defeat state
- retry option
- return-to-sandbox option

### What this should not become yet

- full inventory economy
- premium currency
- full mission-star system
- backend-authoritative reward grant

Those are later systems.

## What this slice should teach the team

If this first slice works, the team should learn:

- whether range circles are actually fun in practice
- whether the current combat loop supports unit identity
- whether a three-unit team is enough to feel tactical
- whether the generic runtime shell is still acceptable
- which parts of combat need redesign before more content is added

## Rule for Slice 1

If a new task does not help:

- Ryoma
- Kuro
- Tsukiko
- Ronin Footman
- Oni Brute
- Yurei Caster
- `Courtyard Ambush`

become more real, it is probably not part of the immediate slice.
