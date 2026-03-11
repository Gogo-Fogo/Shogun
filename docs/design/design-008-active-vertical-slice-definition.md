# Active Vertical Slice Definition

> Status date: March 9, 2026  
> Purpose: define the single active implementation target that should guide near-term Unity work.

## Why this exists

The current project is not empty, but it is also not broad-feature complete.

The repo already contains:

- a useful mobile-ready system scaffold
- a real but thin battle sandbox
- a strong character-data layer
- a large archive of imported content

What it does **not** contain is a single clearly written implementation target that says:

- what the next playable slice actually is
- what counts as "done enough" for that slice
- what is explicitly out of scope until later

This document is that target.

Use it together with:

- [DOC-OPS-007](../ops/doc-ops-007-stage-1-unity-project-reality-audit.md)
- [DOC-GDD-001](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [IMPLEMENTATION_PROGRESS](../../IMPLEMENTATION_PROGRESS.md)

## Active scene baseline

The current live implementation surface is:

- `Assets/_Project/Scenes/Dev/Dev_Sandbox.unity`

Observed current scene truth:

- `TestBattleSetup` exists and points at:
  - `BattleManager`
  - `TurnManager`
  - one test character definition: `Ryoma_CharacterDefinition`
- `BattleManager` has:
  - `characterPrefab`
  - empty `activeCharacters`
  - empty `reserveCharacters`
- `CombatManagers` exists and currently contains:
  - `CombatInputHandler`
  - `GestureRecognizer`
  - `TurnManager`
  - `BattlefieldManager`
  - `MiniGameManager`
  - `InputManager`
- `Characters/CharacterPrefab` is still a generic prototype shell:
  - no assigned `CharacterDefinition`
  - no animator controller
  - empty runtime state until initialized at battle start

Interpretation:

- the sandbox is real enough to anchor a slice
- but it is still closer to "prototype assembly area" than "shippable gameplay loop"

## The one active vertical slice

The near-term target is:

**one complete sandbox battle loop in `Dev_Sandbox` using a tiny roster, a real start state, a real end state, and one production-facing asset lane**

This slice should prove the following loop:

1. a small team is configured from real `CharacterDefinition` assets
2. battle starts reliably in `Dev_Sandbox`
3. player can perform the intended core battle interaction
4. turns advance correctly
5. enemies can be defeated or the player can lose
6. battle ends in a clear result state
7. the result leads into a simple reward/progression handoff, even if placeholder UI is used

## Slice scope

### In scope

#### Battle

- one authored battle scenario in `Dev_Sandbox`
- one small player team
- one small enemy group
- one reliable turn loop
- one clear win condition
- one clear loss condition
- one result/reward handoff

#### Roster

- `2 to 4` real playable units is enough
- `1 to 3` enemy archetypes is enough
- units should prove:
  - element identity
  - weapon/range identity
  - one authored ability path at the first charge threshold
  - one authored ultimate path at full charge when ATTACK 3 and SPECIAL ATTACK production clips exist

#### Presentation

- one approved production-facing character art lane
- at least one unit should use the new `Source -> Production` asset policy
- battle readability matters more than breadth

#### Scene truth

- `Dev_Sandbox` remains the only implementation-truth scene until another scene earns promotion
- shell scenes are not part of this slice

### Explicitly out of scope

Do **not** treat these as part of the first vertical slice:

- full gacha flow
- multiplayer
- co-op
- PvP
- clans
- broad networking architecture
- multiple polished scenes
- large progression tree
- production-ready menu architecture
- large enemy variety
- roguelite mode implementation
- world bosses

These may stay in docs as planned design, but they are not slice requirements.

## Recommended first battle loop

### Minimum battle shape

- player team: `1 to 3` units
- enemy team: `1 to 3` units
- one battlefield layout
- one encounter objective:
  - defeat all enemies

### Minimum character proof points

At least the slice should prove:

- one short-range melee unit
- one mid- or long-range unit
- one meaningful elemental or status-effect interaction

### Minimum reward/result proof

The result layer does not need full economy implementation.

It does need:

- battle end screen or equivalent state
- win/lose distinction
- placeholder reward payload or progression handoff
- a known path back into repeatable testing

## Recommended implementation priorities

### Priority 1

Make the sandbox battle loop reliable:

- start battle
- spawn characters correctly
- initialize runtime state correctly
- take actions
- advance turns
- end battle cleanly

### Priority 2

Make the slice legible:

- show attack/range intent clearly
- show whose turn it is
- show health change clearly
- show result state clearly

### Priority 3

Prove one production-facing art path:

- one unit uses the new source/production separation
- one unit proves the intended future Gemini/PixelLab/Aseprite pipeline can land in runtime-facing content

## What counts as success

This slice is successful when:

- `Dev_Sandbox` can repeatedly run a battle from setup to resolution without manual repair
- the scene demonstrates the core tactical promise of `Shogun`
- the battle loop is readable enough to evaluate range circles, turn flow, and unit identity
- at least one character asset path follows the new production organization rules
- future system work can be judged against this slice instead of against vague repo-wide ambition

## What counts as failure

The slice should be considered failed or incomplete if:

- the battle starts only through brittle manual scene state
- units do not initialize consistently from `CharacterDefinition`
- turn flow is unclear or breaks easily
- battle end state has no clear handoff
- new assets still get dumped into legacy archive folders
- multiple large systems are started before this core loop is trustworthy

## Rule going forward

Until another slice is explicitly defined, this is the default implementation target for the project.

When in doubt:

- prefer improving `Dev_Sandbox`
- prefer proving one complete loop
- prefer small trustworthy systems over broad unfinished ones

