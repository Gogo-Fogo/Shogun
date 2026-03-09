# Stage 1 Unity Project Reality Audit

> Status date: March 9, 2026  
> Purpose: record what the current Unity project actually is before further rebuild, cleanup, or documentation changes.

## Executive summary

`Shogun` should **not** be restarted from absolute zero.

It should also **not** be treated as a feature-complete game foundation.

The current project is best understood as:

- a large imported art/content archive
- a useful mobile-ready scene and UI scaffold
- one real combat sandbox
- a richer character-data model than the live gameplay loop currently supports
- documentation that is ahead of implementation in several areas

The correct next move is:

1. keep the reusable foundations
2. separate archive content from new production content
3. reality-sync the docs
4. rebuild from one honest vertical slice

## Current implementation truth

### Scenes

Current scene files:

- `Assets/_Project/Scenes/Dev/Dev_Sandbox.unity`
- `Assets/_Project/Scenes/Core/MainMenu.unity`
- `Assets/_Project/Scenes/Battle/Battle_Prototype.unity`
- `Assets/_Project/Scenes/UI/UI_Demo.unity`

Observed reality:

- `Dev_Sandbox` is the only scene with meaningful gameplay wiring.
- `MainMenu`, `Battle_Prototype`, and `UI_Demo` currently behave more like shells around shared system roots than feature-complete scenes.
- `EditorBuildSettings.asset` currently points only to `Dev_Sandbox`.

### Shared foundations that are worth keeping

- `Assets/_Project/Prefabs/System/SystemRoot.prefab`
- `Assets/_Project/Prefabs/System/SharedSceneRoot.prefab`
- `Assets/_Project/Scripts/Core/Utilities/AppFrameRate.cs`
- `Assets/_Project/Scripts/Core/Utilities/SafeAreaHandler.cs`
- `Assets/_Project/Scripts/Core/Architecture/EventChannelSO.cs`
- `Assets/_Project/Scripts/Core/Architecture/EventListener.cs`

These form a useful mobile-first scaffold:

- safe-area aware UI
- shared camera/light/event-system setup
- ScriptableObject event channel architecture
- reusable scene bootstrapping

### Gameplay code that is real but still prototype-grade

Most real implementation is concentrated in:

- `Assets/_Project/Scripts/Features/Characters/`
- `Assets/_Project/Scripts/Features/Combat/`

The strongest reusable gameplay foundation is the character data layer:

- `CharacterDefinition.cs`
- `CharacterInstance.cs`
- `CharacterStats.cs`

The current combat prototype has meaningful structure in:

- `BattleManager.cs`
- `TurnManager.cs`
- `TestBattleSetup.cs`
- `CombatStateMachine.cs`
- `BattlefieldManager.cs`

But important gameplay resolution is still incomplete:

- attack execution is still animation-first and effect-light
- input handling is partially stubbed
- encounter flow exists mostly in sandbox form

### Systems that exist structurally but are not meaningfully implemented

The repo layout suggests more maturity than the project currently demonstrates.

Folders or modules that exist but are thin, empty, or mostly structural:

- `Assets/_Project/Scripts/Features/Gacha`
- `Assets/_Project/Scripts/Features/UI`
- `Assets/_Project/Scripts/Networking`
- `Assets/_Project/Scripts/Input`

This does **not** mean these directions are wrong.

It does mean they should be treated as planned architecture, not current feature truth.

## Asset reality

### What the art tree currently is

The current character art tree under:

- `Assets/_Project/Features/Characters/Art/`

is primarily an imported/archive-heavy content base with subfolders such as:

- `Animations`
- `Licenses`
- `SourceFiles`
- `Sprites`

This is valuable, but it is not a clean production-facing asset pipeline yet.

### Canonical production problem

Right now, old imported packs and future production assets are too close conceptually.

Without a separation rule, it becomes too easy to:

- mistake archive packs for live canonical content
- mix generated PixelLab/Gemini assets into legacy folders
- keep obsolete reference/import clutter in active asset paths

## Documentation reality

The docs folder is now structurally much better than before, but there is still a difference between:

- **policy/design truth**
- **implementation/reality truth**

Important current drift:

- some docs still describe older Unity baseline assumptions while the live project is now on `6000.3.10f1`
- several repo docs imply broader implementation maturity than the actual project demonstrates
- current gameplay reality is much closer to "one active battle sandbox" than "multiple mature feature lanes"

## What to keep, rebuild, or archive

### Keep

- shared system prefabs and scene scaffold
- safe-area and mobile UI foundation
- character data model
- useful combat prototype code
- Unity MCP tooling and diagnostics
- imported content library as archive material

### Rebuild

- the playable vertical slice around one honest battle flow
- battle input and interaction polish
- scene progression outside `Dev_Sandbox`
- feature claims in docs that currently overstate implementation
- canonical production asset layout for new generated assets

### Archive or mark as non-canonical

- old imported art packs that are not part of the current production path
- placeholder scenes and shell scenes until they are promoted into real gameplay surfaces
- placeholder tests that do not validate real systems

## Recommended next actions

1. Add a legacy-vs-production asset policy before generating more new art.
2. Treat `Dev_Sandbox` as the current implementation truth scene.
3. Rewrite or annotate implementation/progress docs so they match actual project state.
4. Build the next development stage around one honest vertical slice instead of broad system claims.
5. Keep imported packs available, but stop treating them as the same class of asset as new approved production art.

## Decision

Use this audit as the baseline interpretation of the current Unity project until a later audit supersedes it.

The repo should move forward by:

- **salvaging foundations**
- **separating archive content from active production**
- **syncing docs to reality**
- **rebuilding from one truthful slice**

