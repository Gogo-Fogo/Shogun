# DESIGN-005: Co-op, PvP, and Social Systems Roadmap

**Summary:** `Shogun` should prioritize cooperative PvE before competitive PvP, and it should introduce social systems in layers rather than trying to ship raids, arena, real-time matchmaking, and clans all at once. The strongest path is friend support -> world bosses / raid-style co-op -> lightweight synchronous co-op -> asynchronous PvP -> deeper clan systems -> optional real-time competitive systems much later.

## Purpose

Use this note when deciding:

- what the current canon already says about co-op, PvP, raids, and clans
- what should ship earlier vs later
- how to make the game fun for friends without overcommitting to risky online scope
- how giant world-boss content should work
- how to avoid PvP becoming the main driver of unhealthy power creep

## Canon already in place

The current GDD already commits `Shogun` to:

- co-op as a planned mode
- co-op unlock around account level 10
- raid boss cadence every 2–3 weeks
- PvP arena
- biweekly arena seasons
- quick co-op emotes in the combat UI
- a clan flow in the UI structure
- PvP defenders that use saved behavior patterns / decision matrices

See:

- [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [`../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md`](../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md)
- [`../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md`](../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)
- [`../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md`](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
- [`../research/doc-ref-003-one-piece-treasure-cruise-analysis.md`](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md)

## Core conclusion

The best multiplayer/social strategy for `Shogun` is:

1. build cooperative PvE first
2. keep PvP more controlled than PvE
3. use asynchronous systems before heavy real-time systems
4. treat clans as a social wrapper for co-op and world-boss play, not a giant permanent-stat machine

This aligns with:

- solo-dev scope reality
- the engineering research’s advice to phase online layers after stable single-player combat
- the shutdown lessons from Blazing and the meta-fatigue lessons from FEH

## Why co-op should come before serious PvP

Co-op gives the game:

- friend play
- spectacle
- social retention
- shared wins
- raid-event hype

without creating the same degree of:

- pay-to-win pressure
- ranking resentment
- meta toxicity
- balance fragility

PvP should still exist, but it should not be the first heavy online priority.

## Recommended rollout order

### Phase 0: Friend support and assist systems

Start with low-risk social glue:

- friend list
- support-unit borrowing
- recommended ally lists
- simple friend-based mission bonuses

This creates social identity before synchronous networking is required.

### Phase 1: World bosses and raid-style co-op

This should be the first major social spectacle.

Recommended format:

- colossal boss encounters
- shared or segmented boss HP
- repeated contribution runs
- asynchronous and/or lightly synchronized participation

This is the best place to capture the “tailed beast boss run” feeling you described.

### Phase 2: Lightweight synchronous co-op

Add smaller live co-op fights after the raid structure works.

Recommended scope:

- 2–3 players
- one boss mission type
- limited communication tools
- server-authoritative turn handling

### Phase 3: Asynchronous PvP arena

Use saved defense patterns and attacker runs.

This fits the GDD, keeps cost lower than real-time PvP, and is easier to police.

### Phase 4: Clans / guilds

Introduce clans once co-op and raid loops exist.

Clans should amplify:

- coordination
- world-boss progress
- shared missions
- identity

not pure account-level stat inflation.

### Phase 5: Optional real-time competitive systems

Only consider this when:

- combat is stable
- online operations are mature
- balance governance is proven
- anti-cheat is credible

This is the most optional phase, not the core promise.

## World bosses: recommended structure

This is where `Shogun` can deliver giant, memorable spectacle safely.

### Boss identity

Use huge threats such as:

- giant oni
- Yomi warlords
- cursed battlefield titans
- yokai monarchs
- colossal shrine beasts

These should feel larger than normal bosses and more event-like than standard encounters.

### Why world bosses are a strong fit

They combine:

- co-op excitement
- repeatable event structure
- shared progression
- faction/worldbuilding spectacle
- social coordination

without requiring the game to lean immediately on ranked PvP.

### Recommended design rules

- use phase-based fights with clear telegraphs
- reward contribution, mechanics, and survival, not only damage
- keep attempts short enough for mobile sessions
- use personal and shared rewards
- prevent whales from trivializing the entire event by pure damage output

### Anti-whale / anti-creep safeguards

Use:

- contribution caps or diminishing-return scoring
- mechanic bonuses for weak-point breaks, interrupts, cleanses, support actions
- role diversity incentives
- normalized or compressed scoring where needed

Do not make world-boss rewards purely a top-damage race.

## Co-op mission design

Co-op should emphasize:

- positional teamwork
- role complementarity
- boss-counterplay timing
- shared objective tension

Good co-op mechanics:

- interrupt windows
- split objectives
- hazard management
- revive or rescue plays
- shield-breaking
- coordinated elemental counters

Bad co-op mechanics:

- pure DPS racing with no coordination value
- communication requirements too complex for a mobile pickup session

## PvP philosophy

PvP should be present, but on a tighter leash than PvE.

### Recommended PvP structure

- asynchronous defense teams first
- clear season boundaries
- ranked and casual separation
- strong anti-stall guardrails
- limited impact of newest mechanics in ranked play at first

### Why this matters

The research references show that competitive modes accelerate:

- pay-to-win perception
- meta frustration
- power creep pressure
- trust damage when balance or cheating goes wrong

That makes PvP the area where `Shogun` should be most conservative.

For long-term balance rules, see:

- [`design-003-long-term-balance-and-power-creep-policy.md`](./design-003-long-term-balance-and-power-creep-policy.md)

## Clans / guilds

Clans should be treated as a long-term social layer, not an early mandatory system.

### What clans should do

- coordinate world-boss participation
- provide shared mission ladders
- enable friend discovery and social identity
- unlock cosmetic or prestige goals
- support clan-vs-environment competition more than raw stat scaling

### What clans should not do

- large permanent combat-stat bonuses
- forcing casual players into guild chores just to stay viable
- creating another whale hierarchy that dominates all rewards

### Good clan features

- shared weekly goals
- clan raid milestones
- badge/banner cosmetics
- donation/request systems for materials
- simple async chat or bulletin boards

## Engineering and scope reality

The engineering research already recommends:

- offline-first combat validation first
- online systems only after the core loop is stable
- server-authoritative co-op state for anything valuable
- asynchronous assists as a lower-cost early stepping stone

That means the roadmap above is not just design preference.
It is the most realistic build order too.

## Recommended launch posture

If `Shogun` were phased sensibly, the social stack should look like:

### Near term / prototype truth

- single-player combat
- support-unit borrowing
- event-like boss encounters in local or simulated form

### Phase 2 online truth

- raid/world boss mode
- lightweight live co-op
- async arena

### Later live-service truth

- clans
- richer seasonal social competition
- optional higher-stakes PvP

## Default policy for future decisions

If choosing between:

- “build a more aggressive PvP feature”
- or “build a better cooperative raid / social progression feature”

prefer the second.

For `Shogun`, co-op and world-boss content are more aligned with:

- fun with friends
- dark-fantasy spectacle
- lower toxicity
- lower pay-to-win pressure
- more stable long-term operations

## Related documents

- [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [`design-003-long-term-balance-and-power-creep-policy.md`](./design-003-long-term-balance-and-power-creep-policy.md)
- [`design-004-roguelite-replayability-and-run-mode-framework.md`](./design-004-roguelite-replayability-and-run-mode-framework.md)
- [`../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md`](../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md)
- [`../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md`](../research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)
- [`../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md`](../research/doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
- [`../research/doc-ref-003-one-piece-treasure-cruise-analysis.md`](../research/doc-ref-003-one-piece-treasure-cruise-analysis.md)
