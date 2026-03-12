# Scene Authoring vs Runtime Ownership Plan

> Status date: March 12, 2026  
> Purpose: decide, across the current Shogun Unity project, what should be authored in scenes/prefabs and what should exist only at runtime.

## Why this doc exists

The current project is split between:

- scene-authored foundations such as `SystemRoot`, `SharedSceneRoot`, camera/light/event-system setup, battlefield art, and dedicated scene canvases
- runtime-generated UI trees in `MainMenuSceneController`, `BarracksSceneController`, `SummonSceneController`, `SettingsSceneController`, and parts of `BattleHudController`

That split is currently too runtime-heavy for support scenes. The result has been:

- scene view and play mode drifting apart
- support scenes behaving like generated previews instead of editable screens
- more debugging around layout/bootstrap than around actual product behavior

The default direction for Shogun should now be:

**author stable scene shells and prefab shells in-editor, then let runtime bind data and spawn only the dynamic/transient pieces.**

This matches:

- the mobile-first UI and safe-area goals in [`design-006-mobile-platform-display-and-performance-strategy.md`](../design/design-006-mobile-platform-display-and-performance-strategy.md)
- the definitions-vs-instances architecture in [`doc-eng-002-unity-project-runtime-architecture-patterns.md`](../research/doc-eng-002-unity-project-runtime-architecture-patterns.md)
- the reality audit in [`doc-ops-007-stage-1-unity-project-reality-audit.md`](./doc-ops-007-stage-1-unity-project-reality-audit.md)
- the active-slice rule that `Dev_Sandbox` is still the only gameplay truth scene in [`doc-ops-008-short-term-implementation-todo.md`](./doc-ops-008-short-term-implementation-todo.md), [`design-008-active-vertical-slice-definition.md`](../design/design-008-active-vertical-slice-definition.md), and [`design-009-first-vertical-slice-roster-and-encounter-plan.md`](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md)

---

## Default ownership rule

### Author in scene or prefab when:

- the object is present every time the scene loads
- the object has a stable visual/layout structure
- artists/designers benefit from seeing it in the hierarchy and Inspector
- it needs manual spacing, sizing, anchors, or visual polish
- the slot count is known ahead of time
- the object is part of the baseline readable composition of the scene

Examples:

- camera roots
- canvases and safe-area roots
- panel shells
- top bars, bottom rails, trays, modal roots
- static buttons and layout groups
- battlefield background planes
- authored encounter marker anchors

### Runtime-populate when:

- the shell is stable but the content changes from save data, encounter data, or live state
- the item count is limited but content is data-driven
- the user needs a scene-authored shell with dynamic contents

Examples:

- featured-character cards inside a summon panel
- barracks roster cards inside a scroll list
- current seals/currency labels
- selected-character text/details
- encounter objective text and result copy
- medallion charge state and turn labels

### Pure runtime only when:

- the object is transient, stateful, or simulation-driven
- the count is unknown or highly variable
- the object exists only during animation, VFX, telegraphing, or resolution
- the object is a runtime instance of immutable authored data

Examples:

- `CharacterInstance`
- spawned battle units
- floating damage text
- combo cut-ins
- encounter intro overlays
- ambush telegraphs
- AI turn state
- save/session data services

### Anti-pattern to retire

Do **not** use:

- `[ExecuteAlways]` + `RebuildEditorPreview()` + `BuildSceneContents()` as the primary source of truth for shipped scene layout

That pattern is acceptable only as a temporary preview aid while migrating to scene-authored shells. It should not remain the long-term architecture for support scenes.

---

## Project-wide baseline

### Should stay scene-authored / prefab-authored

- `SystemRoot.prefab`
- `SharedSceneRoot.prefab`
- `Main Camera`
- `Global Light 2D`
- `Canvas`
- `UI_SafeAreaPanel`
- `HUD`, `Menu_Main`, `Popups` roots
- scene backgrounds and decorative layers
- stable layout groups, trays, frames, and header/footer shells

### Should become prefab-authored and instantiated into authored roots

- barracks roster card
- featured summon character card
- summon banner button
- summon result card
- settings row / toggle row / frame-rate row
- top bar module

### Should remain runtime-managed

- `CharacterInstance`
- `BattleManager`, `TurnManager`, `CombatStateMachine`
- summon result sessions
- collection save/profile state
- battle telegraphs and VFX
- floating combat text
- turn countdown attachments on units
- dynamic combo/encounter intro overlays if they are short-lived presentation elements

---

## Current-scene decisions

## 1. `Dev_Sandbox`

**Role:** only gameplay truth scene for the current slice.

### Author in scene

- `SystemRoot`
- battle `SharedSceneRoot`
- main camera + camera framing component
- battlefield background art and combat floor
- authored encounter backdrop composition
- battle HUD shell
  - top bar container
  - bottom medallion tray
  - pause/result roots
  - optional marker roots for encounter overlays

### Runtime-populate

- top-bar text
- objective text
- medallion portrait contents
- charge arcs/dividers/orbs
- result copy
- encounter intro title/subtitle
- boss/turn/range indicators bound to current battle state

### Pure runtime

- spawned combatants
- turn order state
- attack resolution state
- floating damage text
- combo cut-in stripes
- pressure-zone VFX/telegraphs
- temporary warning flashes

### Recommendation

`Dev_Sandbox` should move toward:

- **scene-authored battle composition**
- **prefab-authored HUD shell**
- **runtime combat state + runtime spawned units**

It should not depend on `BattleHudController` inventing the entire HUD tree from nothing once the shell stabilizes.

---

## 2. `Battle_Prototype`

**Role:** non-truth shell / earlier experiment.

### Recommendation

Do one of two things:

1. keep it as an explicit archive/test scene with a minimal authored shell and no new feature work
2. retire it later once `Dev_Sandbox` fully supersedes it

### Author in scene if retained

- only the baseline roots needed to open and inspect it
- no runtime-generated shell scene work beyond safety fallback

### Runtime

- no special new runtime investment

### Decision

Do **not** expand `Battle_Prototype` into a second gameplay truth scene while `Dev_Sandbox` is still the slice target.

---

## 3. `MainMenu`

**Role:** support front door only.

### Must be scene-authored or prefab-authored

- dedicated `MainMenuCanvas`
- safe-area root
- lacquer/gold background layers
- hero panel shell
- action-card grid shell
- status/module shell
- navigation buttons

### Runtime-populate

- featured roster names/portraits
- collection/summon counts
- button wiring
- availability state

### Should not be runtime-generated anymore

- the entire menu hierarchy
- fallback-only main menu layout as the normal path

### Recommendation

Turn `MainMenuSceneController` into a **binder/controller**, not a layout generator.

---

## 4. `Barracks`

**Role:** support collection screen.

### Must be scene-authored or prefab-authored

- dedicated `BarracksCanvas`
- header shell
- detail panel shell
- portrait frame shell
- chip/tag row shell
- scroll viewport shell
- roster list container shell
- footer shell

### Runtime-populate

- owned-character list entries
- selected-character portrait
- stats/lore text
- collection counts
- tag chips

### Pure runtime

- current selected index state
- owned roster pulled from collection/save service

### Recommendation

The screen structure should be fixed in scene. Only the roster cards and selected data should be dynamic.

---

## 5. `Summon`

**Role:** support banner test surface.

### Must be scene-authored or prefab-authored

- dedicated `SummonCanvas`
- atmospheric background shell
- top header shell
- banner-selection panel shell
- banner-focus/detail panel shell
- action-row shell
- reveal/results panel shell
- footer shell

### Runtime-populate

- banner list
- active banner state
- featured trio cards
- currency labels
- summon results
- barracks-sync messages

### Pure runtime

- pull session data
- reveal result list
- collection mutation

### Recommendation

The current generated emergency path is the clearest sign that this screen needs to be re-authored as a real scene/prefab shell. Runtime should supply banner data and result cards only.

---

## 6. `Settings`

**Role:** support utility scene.

### Must be scene-authored or prefab-authored

- dedicated `SettingsCanvas`
- settings panel shell
- grouped rows for frame rate, volume, vibration, shake
- reset/default/back buttons

### Runtime-populate

- current setting values
- toggle states
- slider state
- save/apply wiring

### Pure runtime

- actual persistent settings service/state

### Recommendation

This should be the simplest support scene architecturally: fixed authored shell, runtime values only.

---

## 7. `UI_Demo`

**Role:** UI laboratory, not a shipped-facing scene.

### Must be scene-authored

- experimental static shells
- prefab placement playground
- designer/art iteration surfaces

### Runtime

- only targeted preview scripts if needed for one specific prefab/system

### Recommendation

`UI_Demo` should become the place for visual experimentation with authored prefabs, not another generated runtime shell scene.

---

## Cross-scene ownership matrix

| System / object type | Scene / prefab authored | Runtime-populated | Pure runtime |
| --- | --- | --- | --- |
| `SystemRoot` | Yes | No | No |
| `SharedSceneRoot` | Yes | No | No |
| `Main Camera` / light / `EventSystem` | Yes | No | No |
| dedicated scene canvas | Yes | No | No |
| `UI_SafeAreaPanel` + `SafeAreaHandler` | Yes | Safe area updates only | No |
| panel shells / trays / headers / footers | Yes | No | No |
| scroll viewports and list containers | Yes | No | No |
| list entries / result cards / banner cards | Prefab-authored | Yes | No |
| collection counts / labels / selected detail text | No | Yes | No |
| battle HUD shell | Prefab/scene authored | Yes | No |
| `CharacterDefinition`, `AbilityDefinition`, `BattleEncounterDefinition` | ScriptableObject authored | No | No |
| `CharacterInstance` and combat session state | No | No | Yes |
| floating text / VFX / telegraphs / cut-ins | No | No | Yes |
| save/profile/settings/session services | No | No | Yes |

---

## What runtime is actually for in Shogun

Runtime is more practical than scene authoring when the problem is:

- state mutation
- simulation
- spawning unknown counts
- short-lived presentation
- data binding

Runtime is **not** the best default when the problem is:

- panel layout
- portrait-first spacing
- anchor tuning
- inspector iteration
- scene composition
- deciding where stable buttons and trays should sit

For this project, the expensive mistakes have mostly come from using runtime generation for the second group.

---

## Migration plan

## Phase 1: lock the support-scene architecture

- keep current controllers alive, but demote them to binder/controller role
- stop expanding generated fallback layouts
- create real scene/prefab shells for:
  - `MainMenu`
  - `Barracks`
  - `Summon`
  - `Settings`

## Phase 2: prefab-ize repeatable UI pieces

- `CharacterCard`
- `SummonBannerButton`
- `SummonFeaturedCard`
- `SummonResultCard`
- `SettingsRow`
- `TopBarModule`

## Phase 3: battle HUD shell migration

- keep battle state runtime-driven
- move stable battle HUD composition into a prefab/shell
- reserve runtime creation for transient overlays only

## Phase 4: fallback policy only

Runtime-created canvases and roots should remain only as:

- emergency dev recovery if a scene root is missing
- not the normal authored path

If runtime has to create a missing root, it should log a warning that the scene/prefab authoring is incomplete.

---

## Recommended naming standard

Each scene should stabilize around:

```text
SceneName
  SystemRoot                  (prefab)
  SharedSceneRoot             (prefab, when needed)
  SceneCanvas                 (scene or prefab-authored)
    UI_SafeAreaPanel
      HUD
      Menu_Main
      Popups
  SceneController             (binder/controller, not full layout generator)
```

Where a screen needs reusable sub-surfaces, prefer prefab shells such as:

- `MainMenuShell.prefab`
- `BarracksShell.prefab`
- `SummonShell.prefab`
- `SettingsShell.prefab`
- `BattleHudShell.prefab`

---

## Final decision

For the current Shogun repo:

- prefer **scene-authored and prefab-authored shells**
- use runtime primarily for **data binding, simulation, and transient presentation**
- treat full runtime UI generation as a temporary migration aid, not the target architecture

This is the better fit for:

- mobile portrait layout polish
- Inspector iteration
- scene readability
- shared prefab reuse
- lower drift between scene view and play mode
- the user preference for seeing stable UI and scene structure in the hierarchy

It is also the cleaner path for eventually turning current support scenes into maintainable production surfaces without repeating the `Summon` / `Barracks` drift problems.
