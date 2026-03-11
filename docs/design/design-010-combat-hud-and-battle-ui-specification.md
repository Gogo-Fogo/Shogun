# DESIGN-010: Combat HUD and Battle UI Specification

**Summary:** `Shogun` should keep the immediacy that made `Naruto Blazing` readable and satisfying on mobile, but improve on it through stronger hierarchy, battlefield-first composition, cleaner drag/release feedback, safer portrait-mobile adaptation, and less ornamental clutter. The battle UI should answer five questions in under one second: whose turn is it, what happens if I release here, who gets hit, what risk am I taking, and what result just occurred.

## Why this note exists

The current docs already establish the right raw goals:

- `DOC-GDD-001` says the UI should be no-guesswork and low-friction on mobile
- `DESIGN-006` says the product should stay portrait-first, safe-area-aware, and readable on mobile screens
- `DESIGN-007` says range circles and threat geometry are core combat identity, not decorative overlays
- `DESIGN-008` says the active slice must clearly show attack intent, turn state, health change, and result state
- `DOC-OPS-011` scopes near-term UI work to the active battle slice instead of a broad UI rewrite

What is still missing is one concrete design note that says what the combat HUD should actually be.

This document is that spec.

## Core conclusion

`Shogun` can beat `Naruto Blazing`'s battle UI, but only by beating it on **clarity**, not by trying to be louder or busier.

Keep from `Blazing`:

- touch immediacy
- clear range-circle interaction
- strong team presence through bottom portraits
- dramatic confirmation moments when an action is committed

Improve beyond `Blazing`:

- reduce ornamental clutter
- make drag state and release state more distinct
- make threat and counter risk easier to parse
- keep the battlefield visible under the HUD
- handle portrait safe areas and device scaling more cleanly
- make accessibility and readability part of the default, not an afterthought

## Reference stance

Use `DOC-REF-001` as an interaction reference, not as a visual template.

That means:

- learn from its battle readability and input loop
- do not copy its franchise expression, art framing, or decorative identity literally
- prefer `Shogun`'s own darker, cleaner, more deliberate visual language

## Primary design goals

1. **Battlefield first**
   The player must still see space, unit placement, and range geometry clearly.
2. **One dominant question at a time**
   The HUD should foreground the most important decision for the current phase of interaction.
3. **Portrait touch clarity**
   The squad bar should feel tangible, easy to read, and easy to control with one hand.
4. **Drag predicts, release confirms**
   Dragging should show possibility; release should show consequence.
5. **Dramatic moments are earned**
   Big bounce, flash, and combo feedback should happen on confirmed follow-through, not during idle or prediction states.
6. **Mobile portrait discipline**
   The HUD must fit portrait safe areas cleanly and avoid turning the bottom of the screen into an opaque wall.
7. **Readable by default**
   If the player needs a tutorial to understand the basic current action state, the HUD is failing.

## Information hierarchy by battle phase

### Passive state

The player should be able to identify:

- whose turn it is
- team health at a glance
- encounter objective or boss state
- utility actions like speed, auto, and menu
- subtle left/right edge framing when it is the player's turn, without dimming the battlefield center too aggressively

This state should stay calm and low-noise.

### Selection and drag state

The player should immediately understand:

- which unit is being controlled
- that unit's attack range
- which enemies are currently in range
- whether a combo opportunity exists
- which allies are feeding critical-rate support into that combo
- whether a counter or major risk exists at release

This state should prioritize geometry and prediction over spectacle.

### Release and resolution state

The player should immediately understand:

- the committed target order
- damage landing on each target
- combo continuation on second and later hits
- where the attacker returns after the sequence

This state is where the strongest motion and punch should live.

### Result state

The player should immediately understand:

- victory or defeat
- restart or return path
- placeholder reward or handoff state if present

This state should be unmistakable and not depend on subtle HUD changes.

## Layout specification

### Top bar

The top bar should use three functional zones inside the safe area.

#### Top-left: turn and encounter context

Use this area for:

- compact turn-order strip or initiative lane
- wave or map count
- compact element and weapon legend strip when those systems are active in the slice
- small objective marker when needed

Rules:

- the active unit should be visually obvious in the turn strip
- future units should be readable but lower contrast
- the legend should read as a fast reminder, not a tutorial wall or full rules matrix
- if the mode does not need a timer, do not force a timer into this zone just because other games do

#### Top-center: encounter focus module

This area should switch based on encounter type.

Normal battle:

- show objective chip such as `Defeat all enemies`
- optionally show brief encounter title or phase note

Boss battle:

- replace the normal objective chip with the boss ribbon
- boss UI should dominate this zone, not a separate giant overlay

#### Top-right: utility controls

Use this area for:

- speed toggle
- auto toggle
- menu or pause

Rules:

- these controls must stay reachable but visually secondary to turn and threat information
- they should read as utility buttons, not primary combat state

#### Below top-right: combo tracker

Use this area for:

- live hit count during confirmed combo resolution
- a tier label under the count that escalates as the chain grows

Rules:

- this tracker should stay off during drag prediction and only appear once confirmed hits begin landing
- the numeric hit total should be the primary read; the tier label should be secondary flair
- it should sit near the top-right without covering the battlefield center or the utility buttons themselves

### Battlefield center

The center of the screen remains the hero area.

It must stay readable for:

- unit placement
- range circles
- target brackets
- danger zones
- motion and attack follow-through

Rules:

- avoid large opaque panels cutting into this space during normal play
- avoid constantly animating UI elements over the battlefield
- range, target, and danger overlays should be crisp and legible against busy backgrounds
- player-controlled range auras can carry the richer fill, glow, and marker treatment; enemy threat circles should stay simpler to avoid overlap clutter

### Bottom squad rail

The bottom HUD should be a portrait-first squad rail anchored above the safe area.

It should contain:

- large character portraits for the active slice party
- visible HP state
- skill readiness or action indicators
- clear active-unit emphasis

#### Portrait behavior

- the active portrait should scale larger than the others and use the clearest highlight
- inactive portraits should remain legible and tappable when interaction rules allow it
- unavailable portraits should dim clearly rather than relying on tiny icon changes
- portrait spacing should preserve individual identity instead of collapsing into one crowded cluster

#### Recommended visual treatment

- a low-profile decorative rail, not a giant opaque slab
- readable medallions or frames with `Shogun` flavor, but less ornamental noise than `Blazing`
- HP arcs or bars that read instantly at small size
- skill-charge indicators that do not overpower the portrait itself

#### Safe-area rule

The bottom rail must never depend on screen corners that become unsafe on modern phones.

## Drag-state overlays

The drag state is the most important interactive clarity test in the current slice.

When the player is holding and dragging a unit, the UI should show:

- the active unit with lowered opacity and running state
- that unit's attack range circle
- currently targetable enemies with clear brackets or ring highlights
- multi-target readiness with a static `X2!`, `X3!`, and so on
- per-participant `Critical Rate Boost xN` callouts for units that will join the combo
- counter-danger or threat-risk markers where relevant
- optional release ghost or commit marker if it improves clarity

### Drag-state rules

- drag-time indicators should be predictive, not celebratory
- drag-time combo counts should stay hidden until confirmed hits begin resolving
- crit-boost preview should appear only for confirmed combo participants and should read as a multiplier, not raw percent math
- enemy-in-range state should use shape and contrast, not color alone
- out-of-range enemies should recede without disappearing entirely
- the player's finger should not hide the main decision readout when possible

## Release, combo, and damage feedback

This is the main improvement opportunity over `Blazing`.

### Required flow

1. The player drags and sees which enemies are in range.
2. The UI shows in-range readiness during drag, but does not surface combo hit counts yet.
3. Combo-eligible frontline allies display their current critical-rate boost multipliers during drag, including reserve-buddy support when applicable.
4. The player releases on an enemy to confirm the attack.
5. The released-on unit attacks that enemy first.
6. Any active frontline allies whose attack circles already cover that same enemy join as follow-up hits without spending their own turns.
7. Damage text appears above the enemy for each confirmed hit, with crit results using the boosted per-unit rate.
8. A top-right combo tracker updates live on each confirmed hit and escalates its tier text as the chain grows.
9. If more valid enemies remain inside the acting unit's release range, the lead unit follows through to them and eligible allies can join again.
10. After the combo sequence finishes, the tracker gets its strongest pulse before fading.
11. The acting unit returns to the release position.

### Presentation rules

- first hit should feel clean and readable, not overloaded
- second and later hits are where combo spectacle should peak
- damage text must appear above the impacted enemy and clear quickly
- combo celebration should not hide the target that was actually hit
- the live combo tracker should update per hit, but only after hits are confirmed
- support-based crit preview should sit above the participating allies instead of covering the battlefield center
- hit-stop and bounce should support readability, not smear it

## Boss UI

Boss presentation should feel important without taking over the entire screen.

### Boss ribbon

When a boss is present, the top-center encounter module becomes a boss ribbon with:

- boss name
- large HP bar
- visible phase or segment markers
- status or debuff indicators if relevant
- optional enrage or telegraph marker when it matters

### Boss rules

- boss state gets top-level visual priority over normal-wave objective chips
- boss UI should stay wide and legible, but not so tall that it crushes the battlefield
- boss warnings should use short, high-contrast telegraphs rather than permanent noisy effects
- normal enemies should not get boss-level UI weight

## Turn-order and action-state rules

The turn-order strip is one of the most valuable readability wins.

It should:

- show the active unit clearly
- preview a small number of upcoming turns
- animate reorder changes in a readable way
- support future hover or tap detail without requiring it for baseline understanding

The player should never have to guess whether the currently highlighted portrait, the active battlefield unit, and the top-bar turn owner are supposed to represent the same actor.

## Visual-language rules

### Shape language

- circles remain the default grammar for basic attack range
- irregular skill shapes should read differently from basic attack circles
- dashed or lighter treatments can mean predicted state
- solid brighter treatments can mean confirmed current threat or active target

### Color language

Use color to reinforce state, not define it alone.

Suggested direction:

- ally-active: cool light, teal, or pale cyan emphasis
- enemy-threat: red, cinnabar, or warning vermilion
- boss-state: gold plus danger accents
- neutral utility: muted parchment, charcoal, lacquer, or bronze tones

### Motion language

- passive HUD motion should be subtle
- drag state should use breathing or steady emphasis, not bounce spam
- confirmed combo follow-through can use bounce, burst, and hit-stop
- result states can use stronger transitions because interactivity has ended

## Accessibility and readability requirements

The combat HUD should ship with readability discipline built in.

Required posture:

- color is never the only signal for threat or state
- important icons need readable silhouettes and outlines
- gesture prompts must stay large enough to read on a phone screen
- reduced shake and reduced flash paths should remain possible
- large-icon treatment for gesture cues should remain compatible with the design

## Anti-patterns to avoid

Avoid these failures:

### 1. Ornament over clarity

If the rail, frame, or badge styling becomes louder than the state it contains, cut it back.

### 2. Constant animation everywhere

If everything pulses, nothing feels important.

### 3. Bottom-wall syndrome

Do not let the bottom HUD become a huge opaque mass that hides too much of the battlefield.

### 4. Tiny-status overload

Do not solve readability with more tiny icons.

### 5. Drag-time spectacle

Do not spend the strongest bounce and celebration during prediction state.

### 6. Boss clutter

Do not combine boss bar, giant portrait, giant warning panel, and constant field overlays all at once.

## Recommended implementation order for the active slice

### Phase 1: must-have readability

- bottom squad rail with clear active portrait emphasis
- top utility and encounter bar
- basic turn-order strip
- readable drag-state range and target overlays
- damage popup and combo follow-through feedback
- minimal win or loss result panel

### Phase 2: slice-quality polish

- stronger boss ribbon behavior
- clearer counter-risk markers
- better reorder animation in the turn strip
- accessibility toggles that materially improve battle readability

### Phase 3: later enhancements

- richer skill-name banners
- optional threat toggles for advanced players
- more complex encounter-state modules beyond the first slice

## Implementation architecture note

The HUD should stay presentation-owned.

That means:

- combat logic decides rules and outcomes
- the HUD subscribes to combat state and combat presentation events
- drag prediction UI reads current state but does not invent outcomes
- result, combo, and boss updates should be driven by clear owners, not ad hoc scene queries

This aligns with `DOC-ENG-002` and keeps the UI portable as the battle slice grows.

## Acceptance checks

This spec is being followed well when:

- the active unit is always obvious
- drag state makes target and danger prediction obvious before release
- combo celebration happens on confirmed follow-through, not during prediction
- boss state is readable without hiding too much of the battlefield
- bottom portraits feel tactile and useful instead of decorative only
- the battle screen still feels calm enough to parse on a phone

## Decision rule

When choosing between:

- more decorative UI weight
- and clearer state communication

choose clearer state communication.

When choosing between:

- more spectacle during drag
- and stronger payoff after release

save the spectacle for after release.




