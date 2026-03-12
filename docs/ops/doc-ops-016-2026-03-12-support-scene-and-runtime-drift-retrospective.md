# March 12, 2026 Support-Scene and Runtime Drift Retrospective

**Summary:** This note records the support-scene and UI/runtime drift problems encountered across the recent March 2026 Shogun batches, especially the `MainMenu`, `Barracks`, `Summon`, and `Settings` regressions that surfaced during the scene-authoring push. It exists so these failures become standing architecture rules instead of repeated rediscovery.

## Purpose

Use this document when the question is:

- why recent support scenes kept breaking between scene view and play mode
- what actually went wrong in the `MainMenu`, `Barracks`, and `Summon` investigation loop
- what rules should govern runtime-vs-editor ownership going forward
- why stable UI shells should live in scene/prefab instead of being regenerated at runtime
- what validation is required before touching support scenes again

## Why this note exists

During the recent UI and support-scene work, the repo repeatedly hit the same class of failure:

- a scene looked one way in edit mode and another way in play mode
- a support scene rendered as a dark/empty slab even though the controller thought it had built successfully
- emergency/fallback layouts became the de facto implementation path instead of a recovery path
- scene hierarchy and Inspector stopped being trustworthy sources of truth for the actual shipped layout

These incidents happened across a cluster of recent commits, including:

- `997a38a` `Investigate main menu black screen`
- `a52cb77` `Checkpoint UI scene authoring investigation`
- `be8756c` `Checkpoint authored encounter and reference docs`
- `93fea3a` `Polish summon UI and encounter telegraphs`
- `7cc7979` `Fix summon runtime drift and HUD tap blocking`

This note complements [`doc-ops-015-scene-authoring-vs-runtime-ownership-plan.md`](./doc-ops-015-scene-authoring-vs-runtime-ownership-plan.md) by recording the failure pattern that led to that plan.

## What broke

## 1. Support scenes were layout-generated twice

Main files involved:

- [`MainMenuSceneController.cs`](../../Assets/_Project/Scripts/Features/UI/MainMenuSceneController.cs)
- [`BarracksSceneController.cs`](../../Assets/_Project/Scripts/Features/UI/BarracksSceneController.cs)
- [`SummonSceneController.cs`](../../Assets/_Project/Scripts/Features/UI/SummonSceneController.cs)
- [`SettingsSceneController.cs`](../../Assets/_Project/Scripts/Features/UI/SettingsSceneController.cs)

What happened:

- edit mode used `[ExecuteAlways]` and preview rebuild logic
- play mode rebuilt the same screen again from code
- runtime sometimes laid itself out against stale preview children, stale sizing, or fallback roots
- scene view became only an approximation, not the actual layout source

Why this mattered:

- designers could not trust what they saw in hierarchy/Inspector
- fixing one mode often broke the other
- support scenes became harder to reason about than the gameplay they were supposed to support

## 2. Fallback layouts became normal implementation paths

Most obvious in:

- [`SummonSceneController.cs`](../../Assets/_Project/Scripts/Features/UI/SummonSceneController.cs)
- [`MainMenuSceneController.cs`](../../Assets/_Project/Scripts/Features/UI/MainMenuSceneController.cs)
- [`BarracksSceneController.Build.cs`](../../Assets/_Project/Scripts/Features/UI/BarracksSceneController.Build.cs)

What happened:

- fallback/emergency UI trees were introduced to recover from missing roots or runtime failures
- over time, those fallback trees became the main path actually used in play mode
- those generated layouts were more fragile than authored shells and drifted from the scene preview

Why this mattered:

- the project was paying the cost of emergency code on every run
- scenes were no longer truly authored in the editor even when they appeared to be
- regressions in `Summon` and `MainMenu` were caused by tuning fallback layout code rather than stabilizing a real shell

## 3. Scroll/mask chains hid entire screens

Most visible in `Summon`.

What happened:

- the emergency summon layout ran through `ScrollRect -> Viewport -> Mask -> ScrollContent`
- content could be positioned, sized, or clipped incorrectly while the controller still considered the layout "built"
- the user saw a dark slab or nearly empty screen even though the hierarchy contained many generated children

Why this mattered:

- the scene looked dead even though the controller had not actually failed
- diagnosis became misleading because the existence of hierarchy children did not mean the screen was readable

## 4. Canvas and scene-shell ownership was inconsistent

Examples:

- dedicated scene canvases were added late and inconsistently
- `SharedSceneRoot` still existed beside scene-specific canvases in some scenes
- some scenes had a visible scene controller object but no true serialized UI shell

Why this mattered:

- it was unclear which canvas/root was authoritative
- runtime repair code kept having to guess whether to create, reuse, or override roots
- this increased the chance of scene/runtime mismatch and layout drift

## 5. Support-scene fixes shipped as moving checkpoints instead of stable milestones

What happened:

- several batches were pushed as checkpoints while the architecture was still in flux
- scene-specific regressions were fixed incrementally, but the shared root cause remained active
- this made it easy to re-break `Summon` or `Barracks` while improving another part of the same system

Why this mattered:

- future work risked building on unstable assumptions
- handoff quality suffered because a feature could be "partly fixed" but still be on the wrong architecture path

## 6. HUD hit-testing and gameplay input overlapped when stable UI surfaces were not treated as hard blockers

Main files involved:

- [`BattleHudController.cs`](../../Assets/_Project/Scripts/Features/Combat/BattleHudController.cs)
- [`CombatInputHandler.cs`](../../Assets/_Project/Scripts/Features/Combat/CombatInputHandler.cs)

What happened:

- bottom-rail portrait taps could pass through to battlefield movement
- this was another form of unstable ownership: the UI looked like a control surface, but gameplay still treated that screen region as tappable world space

Why this mattered:

- player intent was ambiguous
- HUD affordances became unreliable
- it showed the same broader problem: stable surfaces must have explicit ownership, not just appearance

## Root cause

The root cause was not one bad scene or one bad layout number.

The actual root cause was architectural inconsistency:

- stable UI structure was being treated as runtime-generated content
- editor preview and play mode had separate build paths
- emergency recovery logic was allowed to become the primary layout system
- scene-authored shells, prefab-authored reusable pieces, and runtime-only state were not separated cleanly enough

That is why the regressions repeated across different scenes.

## The rule going forward

This is now the default Shogun rule:

- **Stable scene composition belongs in scene or prefab.**
- **Runtime should bind data, drive simulation, and spawn transient elements.**
- **Emergency runtime root creation is a safety net, not the normal path.**

If a screen exists every time the scene loads and its structure is mostly known ahead of time, it should not be generated from scratch in play mode.

See the canonical planning note:

- [`doc-ops-015-scene-authoring-vs-runtime-ownership-plan.md`](./doc-ops-015-scene-authoring-vs-runtime-ownership-plan.md)

## Permanent rules for future work

## 1. One authoritative shell per scene

Each support scene should stabilize around:

- one scene-specific canvas/root
- one authored safe-area/UI root
- one controller that binds data and wires events
- no competing generated shell trees as the normal path

## 2. Do not use `[ExecuteAlways]` preview generation as the long-term source of truth

Preview generation may exist temporarily while migrating, but:

- it must not be the main architecture
- it must not invent the shipped layout every time
- it must not diverge from the play-mode path

## 3. Fallbacks must stay fallbacks

If runtime creates a missing canvas/root:

- log a warning
- recover narrowly
- do not let that path become the default implementation over time

## 4. Stable lists and panels should use prefab-authored pieces

Examples:

- `CharacterCard`
- `SummonBannerButton`
- `SummonFeaturedCard`
- `SummonResultCard`
- `SettingsRow`
- `BattleHudShell`

The shell should be visible in hierarchy/Inspector. Runtime should only populate the dynamic contents.

## 5. Runtime-only is for state, unknown counts, or transients

Keep runtime-only ownership for:

- `CharacterInstance`
- combat state/session logic
- floating text
- VFX and telegraphs
- combo cut-ins
- temporary encounter overlays
- save/session services

## 6. Validate touched scenes in both edit and play mode before push

Any scene touched in a batch should be checked for:

- scene view visibility
- hierarchy sanity
- play-mode visibility
- input/hit blocking
- canvas root ownership
- safe-area and portrait framing

This is especially required for:

- `MainMenu`
- `Barracks`
- `Summon`
- `Settings`
- `Dev_Sandbox`

## 7. Prefer one clean migration over repeated emergency tuning

If a scene keeps breaking because generated fallback code is too complex, stop tuning the fallback and move the scene to an authored shell. That is cheaper than repeating another cycle of runtime drift fixes.

## 8. `Summon` had a first-frame layout stabilization bug even after the screen was technically "built"

What happened:

- `Summon` stopped rendering as a dead slab, but the first play-mode frame still came up partially collapsed
- the banner selector row could appear as thin bars with clipped text
- action buttons in the detail panel could collapse into narrow strips
- clicking one of those strips immediately made the screen look correct

Why that clue mattered:

- the click was not loading a second page
- it was hitting the same scene and forcing a later state update plus another layout pass
- that proved the main failure was no longer missing content, but incomplete first-frame layout stabilization

What the root cause turned out to be:

- `Summon` was still depending on a startup layout sequence that was not fully settled on the first rendered frame
- some generated children under layout-group parents were being created with anchors that were safe for generic absolute positioning, but unstable for Unity layout-group ownership in play mode
- later interactions such as `SelectBanner()` or `PerformSummon()` triggered another forced relayout, which made the same screen suddenly look correct

What fixed it:

- the minimal summon path was forced to rebuild its fallback shell directly instead of trusting stale authored-shell binding state
- generated `RectTransform` children under `LayoutGroup` parents were normalized to layout-safe top-left anchors/pivot instead of stretch-style anchors
- a short startup stabilization pass was added for the first two play-mode frames so `Summon` re-forces canvas/root/content layout before the player interacts with the scene
- the old top-left `OnGUI` diagnostic overlay was removed after the layout path became stable enough to rely on console logging instead

Operational lesson:

- if a support scene looks broken until the first user interaction, treat that as a lifecycle/layout-settling bug, not a content bug
- if clicking a collapsed control makes the screen "fix itself," the screen is usually already built and just missing a trustworthy first-frame relayout pass
## Immediate implications for current Shogun work

- `Dev_Sandbox` remains the gameplay truth scene, but its stable HUD composition should still move toward a prefab-authored shell.
- `MainMenu`, `Barracks`, `Summon`, and `Settings` should be treated as authored support scenes with runtime data binding only.
- `Summon` is the clearest proof that generated emergency UI should not remain the main implementation path.
- Future support-scene work should be judged by whether hierarchy/Inspector remain trustworthy after the change.

## What this means for code reviews and future planning

When reviewing new scene/UI work, prioritize these questions first:

1. Is this stable structure being authored in scene/prefab, or generated at runtime?
2. Is there one authoritative shell, or multiple roots fighting each other?
3. Is runtime only binding state, or is it inventing layout again?
4. If a fallback exists, is it clearly a fallback or quietly the main path?
5. Will scene view and play mode still match after this change?

If the answer trends the wrong way, stop and correct the ownership model before continuing polish work.

## Related documents

- [`doc-ops-015-scene-authoring-vs-runtime-ownership-plan.md`](./doc-ops-015-scene-authoring-vs-runtime-ownership-plan.md)
- [`doc-ops-008-short-term-implementation-todo.md`](./doc-ops-008-short-term-implementation-todo.md)
- [`doc-ops-011-ui-implementation-todo.md`](./doc-ops-011-ui-implementation-todo.md)
- [`doc-ops-012-2026-03-10-combat-ui-and-auto-positioning-retrospective.md`](./doc-ops-012-2026-03-10-combat-ui-and-auto-positioning-retrospective.md)
- [`doc-eng-002-unity-project-runtime-architecture-patterns.md`](../research/doc-eng-002-unity-project-runtime-architecture-patterns.md)
- [`design-008-active-vertical-slice-definition.md`](../design/design-008-active-vertical-slice-definition.md)
- [`design-009-first-vertical-slice-roster-and-encounter-plan.md`](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md)

