# DOC-OPS-011 UI Implementation TODO

> Purpose: define the near-term UI backlog for `Shogun` without drifting into a broad UI rewrite that the current vertical slice does not need.

## Current objective

Make `Assets/_Project/Scenes/Dev/Dev_Sandbox.unity` the UI truth surface for the active battle slice, with readable combat feedback, clear state communication, and portrait-first mobile presentation.

This document is for UI work that directly improves:

- combat readability
- touch clarity
- turn-state clarity
- result-state clarity
- mobile portrait presentation

It is not a license to start full menu architecture, gacha flow UI, or a complete client-wide HUD rebuild.

## Authority and source set

This UI TODO is derived primarily from:

- `DOC-GDD-001` for the no-guesswork UI rule, tap-budget rule, combat HUD regions, and accessibility direction
- `DESIGN-006` for portrait-first, safe-area-aware, adaptive mobile UI policy
- `DESIGN-007` for range-circle and threat-readability requirements
- `DESIGN-008` for the current vertical-slice definition and what the slice must visibly prove
- `DESIGN-010` for the concrete combat HUD, drag/release feedback, combo language, and boss UI spec
- `DOC-OPS-008` for short-term scope guardrails
- `DOC-ENG-002` for event-driven UI refresh and logic/presentation separation

If a proposed UI task does not materially help the active slice, it should move to a later roadmap doc instead of this file.

## Scope guardrails

- `Dev_Sandbox` remains the only implementation-truth scene for gameplay UI work.
- UI work should stay portrait-first and safe-area-aware.
- UI should explain battle state, not compete with the battlefield art.
- Prefer narrow, event-driven presentation work over hardwired scene hacks.
- Do not start broad menu architecture or full LiveOps UI flows from this backlog.
- Do not rebuild shell scenes just because they exist.

## Immediate work buckets

### Battle HUD readability

- `[Now]` Add a minimal combat HUD layer that makes turn state, active unit, and encounter objective obvious at a glance.
- `[Now]` Show clear player-facing state for HP, action availability, and current turn ownership.
- `[Now]` Keep the HUD layout consistent with portrait mobile framing and safe-area constraints.
- `[Next]` Add a readable turn-order strip or timeline aligned with the GDD combat HUD direction.
- `[Do not start yet]` Do not build the full final in-combat HUD from the GDD before the slice proves its combat loop.

### Range, threat, and targeting feedback

- `[Now]` Make attack range and enemy-threat feedback consistently readable against real battlefield art.
- `[Now]` Improve overlap readability so multi-threat and combo opportunities are obvious before release.
- `[Now]` Visually mark counter-capable or especially dangerous enemies when that rule is active.
- `[Next]` Add one narrow threat-preview mode for `if I release here, what hits me back?`
- `[Next]` Evaluate a lightweight toggle for wider enemy-threat display only if it helps testing and does not clutter the screen.
- `[Do not start yet]` Do not add heatmaps, dense tactical overlays, or many simultaneous debug visuals to the main slice UI.

### Damage, combo, and action confirmation feedback

- `[Now]` Make damage numbers, combo follow-through, and hit confirmation readable without obscuring sprites.
- `[Now]` Ensure drag-time indicators communicate readiness while release-time indicators communicate confirmed follow-through.
- `[Next]` Standardize combat feedback timing so movement, hit, damage popup, and combo feedback feel responsive and intentional.
- `[Next]` Add one clean visual language for status application, dodge, guard, or counter feedback if those behaviors enter the slice.
- `[Do not start yet]` Do not build a giant VFX-heavy combat feedback stack that hides tactical readability.

### Result and encounter-state UI

- `[Now]` Add or stabilize a minimal win/loss/result presentation with restart and return path clarity.
- `[Now]` Make objective state readable during the encounter, even if it is only `Defeat all enemies`.
- `[Next]` Add a placeholder reward/result handoff panel that proves post-battle flow without requiring real economy UI.
- `[Do not start yet]` Do not build full reward chest, inventory, or summoning result flows from this slice backlog.

### Mobile layout, safe area, and responsiveness

- `[Now]` Audit battle UI prefabs for portrait-first reference behavior and safe-area correctness.
- `[Now]` Remove landscape assumptions from slice-facing battle panels where they break portrait-first behavior.
- `[Next]` Validate the slice on large-screen portrait layouts so extra space becomes breathing room rather than stretched UI.
- `[Next]` Keep future tablet and foldable adaptation in mind, but only through adaptive layout rules, not bespoke large-screen UX.
- `[Do not start yet]` Do not build a separate tablet UX branch or desktop-style combat layout.

### Accessibility and input clarity

- `[Now]` Keep drag, hold, and gesture prompts legible and forgiving on mobile screens.
- `[Next]` Add large-icon or simplified gesture cue options if current combat prompts become too subtle.
- `[Next]` Define a minimal settings surface for reduced effects, reduced shake, and similar readability protections if needed by the slice.
- `[Next]` Review font scale and contrast for battle-critical UI once the core HUD exists.
- `[Do not start yet]` Do not build the full accessibility settings suite until the slice HUD is real enough to evaluate.

### UI architecture and ownership

- `[Now]` Keep UI refresh driven by clear combat events or state owners, not scattered direct scene lookups.
- `[Next]` Start a thin `Shogun.Features.UI` lane only where it reduces coupling and improves reuse.
- `[Next]` Use event channels selectively for combat-result banners, turn-state refresh, and other presentation updates that cross feature boundaries.
- `[Do not start yet]` Do not create a giant generalized UI framework before the current battle UI requirements are proven.

## Acceptance checks for the active slice

The near-term UI pass is good enough when:

- the player can always tell whose turn it is
- the player can judge attack and threat geometry without guesswork
- damage and combo results read clearly during combat motion
- win/loss state is visible and unambiguous
- the battle UI fits portrait mobile framing and safe areas without obvious breakage
- UI additions improve the battle loop instead of hiding it

## Explicitly deferred

These belong elsewhere unless they become hard blockers:

- full main menu UX
- roster-management UI overhaul
- summoning and shop flows
- clan, co-op, or PvP UI
- large-scale inventory and equipment screens
- broad meta-progression UI architecture
- full tablet-exclusive layouts
- broad UI polish passes unrelated to the vertical slice

## Default rule going forward

When deciding between:

- a broad impressive UI ambition
- and a narrow readable UI layer that makes the battle slice trustworthy

pick the second.

`Shogun` needs UI that explains combat cleanly before it needs UI breadth.