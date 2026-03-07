# DESIGN-003: Long-Term Balance and Power-Creep Policy

**Summary:** `Shogun` should accept that some power growth is inevitable in a live-service gacha, but it must control *where* that growth lives. The game should sell characters through fantasy, role identity, and mode fit first, while keeping raw numerical escalation on a short leash and preserving old-unit relevance through refreshes, normalization, and run-level power systems.

## Purpose

Use this note when deciding:

- how strong new banners are allowed to be
- how to keep old units relevant over time
- how PvP, co-op, and roguelite modes should react to power inflation
- what balance cadence and telemetry rules should exist
- how to avoid the “wall of text + must-pull” failure mode seen in long-lived gachas

## Canon already in place

The current GDD already commits `Shogun` to:

- ethical monetization and avoiding overt pay-to-win in PvP
- regular raid and arena seasons
- long-term live-service scope
- formal acknowledgement that power creep and complexity bloat are major risks

See:

- [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [`../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md`](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
- [`../research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md`](../research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md)

## Core conclusion

Some power creep is inevitable.

What is *not* inevitable is letting `Shogun` sell characters mainly through:

- runaway stat inflation
- ever-longer skill text
- mode-warping immunities
- one-banner invalidation of older rosters

The long-term target should be:

- **small controlled vertical growth**
- **large horizontal variety**
- **high run-level power spikes in roguelite content**
- **predictable old-unit refreshes**

## Power model: where strength should live

`Shogun` should operate with three distinct power layers.

### 1. Account power

This should be the flattest layer.

Account progression should mostly unlock:

- mode access
- loadout slots
- relic pool access
- quality-of-life
- extra choices, not raw dominance

Avoid permanent account systems that snowball every future mode through large combat stats.

### 2. Character power

This is the main banner layer, but it should grow in a controlled way.

Characters should primarily differ through:

- role
- element
- weapon family
- martial school
- team synergy
- encounter specialization

New characters may be stronger in *some* contexts, but they should not become automatic best-in-slot in every lane at once.

### 3. Run power

This should be the wildest layer.

Temporary run-level power belongs in:

- roguelite blessings
- curses
- relics
- shrines
- temporary weapon arts
- corruption systems

This is where the game can let players feel “broken” without permanently destroying long-term balance.

For the roguelite framework, see:

- [`design-004-roguelite-replayability-and-run-mode-framework.md`](./design-004-roguelite-replayability-and-run-mode-framework.md)

## What banners should actually sell

Each banner should derive its value from four lanes, not one.

### 1. Aesthetic value

- collectible fantasy
- art quality
- rarity identity
- variant appeal

### 2. Identity value

- new element + weapon + school combinations
- new team-building possibilities
- new faction or world-pillar expression

### 3. Mode value

- especially strong in a certain season, raid type, roguelite path, or event structure
- not necessarily the strongest unit in the entire game

### 4. Power value

- yes, some real strength increase
- but this should be the narrowest lane, not the whole sell

## Anti-creep rules

These rules should be treated as standing balance policy.

### Rule 1: No banner should win on raw numbers alone

If a new unit’s entire sales pitch is:

- more ATK
- more HP
- more multipliers
- more passives

then the design is probably unhealthy.

### Rule 2: No new unit should be best in too many axes at once

Avoid units that simultaneously become:

- top damage
- top mobility
- top survivability
- top utility
- top PvP pick
- top raid pick

One unit may lead in one or two lanes. It should not dominate the entire game.

### Rule 3: Ranked environments must be stricter than casual environments

Ranked PvP should have stronger controls than PvE or casual play:

- legality delays for newly released mechanics
- seasonal restrictions or map rotations
- normalization or stat compression where needed
- fast response to degenerate defender loops or toxic metas

For multiplayer implications, see:

- [`design-005-co-op-pvp-and-social-systems-roadmap.md`](./design-005-co-op-pvp-and-social-systems-roadmap.md)

### Rule 4: Old-unit refreshes must be scheduled, not exceptional

Do not rely on rare “redemption” patches.

Recommended cadence:

- every major season or banner cycle, refresh at least 1–2 older units or archetypes
- every quarter, perform a broader health pass on older mechanics and roles

Refreshes can include:

- stat tuning
- shorter cooldowns
- trait modernization
- new awakening nodes
- PvE-only or mode-specific role value

### Rule 5: Skill text must stay readable

Power creep often hides inside complexity creep.

Avoid turning units into unreadable walls of exceptions.

Default target:

- one clear primary mechanic
- one secondary rider
- one conditional payoff

If a unit needs a paragraph to explain why it exists, that is a warning sign.

### Rule 6: Encounter design must not require the latest banner

Content should reward smart roster building, not only newest-unit ownership.

Endgame fights may *favor* certain units.
They should not make older rosters effectively non-functional.

## PvE, co-op, PvP, and roguelite policy

### Story and general PvE

Allow a little more raw strength here because the mode is less sensitive to competitive fairness.

Still avoid:

- content tuned around only the newest releases
- absurd HP inflation as the primary difficulty tool

### Co-op and raids

Co-op can tolerate stronger “fun power” than PvP, but should still avoid contribution systems that turn success into a whale damage race.

Use:

- role contribution
- mechanic handling
- weak-point timing
- team utility

not only damage output.

### PvP

PvP is where uncontrolled power creep does the most damage.

Treat PvP as the most restrictive balance environment:

- normalize where necessary
- keep new releases on a shorter leash
- separate casual and ranked rulesets
- respond quickly to oppressive metas

### Roguelite modes

Roguelite systems are a pressure-release valve.

They let the game create dramatic power spikes in a place that resets cleanly between runs.

That reduces pressure to print absurd permanent units just to create excitement.

## Old-unit relevance framework

Old units should survive through a mix of:

- scheduled refreshes
- niche excellence
- synergy hooks
- event or rotation advantages
- cheaper build cost or easier accessibility

Not every old unit must remain top tier.
Every old unit should have at least one meaningful reason to exist.

## Balance cadence

Recommended operating rhythm:

- **Weekly:** telemetry review for obvious outliers, failure spikes, and mode health
- **Per banner/event cycle:** check whether new units overperform in too many lanes
- **Monthly:** broader balance summary and targeted adjustments
- **Quarterly:** structured rework pass for older units or stale mechanics

## Telemetry to watch

Balance decisions should be made from both gameplay feel and instrumented evidence.

High-value signals:

- banner-unit usage rate by mode
- old-unit usage decay
- win rate by unit in PvP and raid participation
- clear rate by content tier and team age
- damage-share concentration in raids
- surrender/abandon rates in competitive modes
- “required unit” perception signals from community sentiment and content guides

## Warning states

### Healthy state

- new units are desirable without erasing old ones
- multiple teams can clear current content
- PvP has several viable archetypes
- roguelite mode creates freshness without permanent stat inflation

### Yellow state

- new units dominate one major mode on release
- old units begin disappearing rapidly from endgame usage
- skill descriptions grow notably longer
- content guides converge on a narrow set of must-own answers

### Red state

- newest banners become required for competitive rewards
- older rosters cannot reasonably clear current endgame
- PvP or raid participation drops due to meta frustration
- the game sells mainly through replacing last month’s characters

## Default policy for future decisions

When choosing between:

- “make the new unit numerically stronger”
- or “make the new unit more distinct, desirable, and mode-relevant”

prefer the second.

`Shogun` should accept some creep, but it should make that creep:

- slower
- narrower
- easier to reverse
- less central to monetization

## Related documents

- [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [`design-004-roguelite-replayability-and-run-mode-framework.md`](./design-004-roguelite-replayability-and-run-mode-framework.md)
- [`design-005-co-op-pvp-and-social-systems-roadmap.md`](./design-005-co-op-pvp-and-social-systems-roadmap.md)
- [`../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md`](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
- [`../research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md`](../research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md)
