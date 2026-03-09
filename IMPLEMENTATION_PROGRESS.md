# Implementation Progress

> Status date: March 9, 2026  
> Read this as a reality-tracking note, not as a marketing summary.

## Current status

`Shogun` is **not** starting from zero, but it is also **not** a feature-complete game foundation.

The current repo is best understood as:

- a large imported content/archive base
- a useful mobile-ready scene and UI scaffold
- one real combat sandbox
- a stronger character-data model than the live gameplay loop currently supports
- several planned systems that exist structurally but are not yet meaningfully implemented

Use [DOC-OPS-007](docs/ops/doc-ops-007-stage-1-unity-project-reality-audit.md) as the fuller explanation of that reality.

## Current implementation truth

### Active Unity/editor baseline

- Current project version: `6000.3.10f1`
- Current mainline editor policy lives in:
  - [DOC-OPS-006](docs/ops/doc-ops-006-unity-editor-version-policy-and-upgrade-checklist.md)

### Current playable surface

- The only scene with meaningful gameplay wiring is:
  - `Assets/_Project/Scenes/Dev/Dev_Sandbox.unity`
- Other scene files currently exist more as shells or scaffolds:
  - `MainMenu`
  - `Battle_Prototype`
  - `UI_Demo`
- Build settings currently point only to:
  - `Dev_Sandbox`

### Foundations that are genuinely useful

#### Shared system scaffold

- `SystemRoot.prefab`
- `SharedSceneRoot.prefab`
- safe-area handling
- mobile-first Canvas/camera setup
- event system / event channel architecture

#### Character/data layer

These remain strong reusable foundations:

- `CharacterDefinition.cs`
- `CharacterStats.cs`
- `CharacterInstance.cs`
- `CharacterFactory.cs`

#### Combat sandbox

These are real, but still prototype-grade:

- `BattleManager.cs`
- `TurnManager.cs`
- `BattlefieldManager.cs`
- `CombatStateMachine.cs`
- `TestBattleSetup.cs`
- `CombatInputHandler.cs`

## What is implemented vs. what is still prototype

### Implemented enough to keep building on

- ScriptableObject event system
- character definitions and runtime instances
- multi-element / weapon / status-effect data model
- scene/system scaffolding for mobile portrait UI
- Unity MCP bridge and project-specific editor validation tools

### Exists but is still incomplete

- battle flow and encounter logic
- drag/tap interaction as a prototype interaction layer
- mini-game framework
- animation-driven combat actions
- scene progression outside the sandbox

### Structural but not meaningfully implemented yet

These directions should be treated as planned architecture, not current feature truth:

- `Features/Gacha`
- `Features/UI` as a broad system layer
- `Networking`
- `Input` as a broader standalone subsystem

## Documentation reality

The docs tree is now well organized, but some older repo-facing summaries were ahead of implementation.

That is why the current guidance is:

- use the design/art/legal/ops/research docs for **policy and intent**
- use this file plus `DOC-OPS-007` for **current implementation reality**

## Art/content reality

The repo already contains a large character-art archive.

That archive should not be confused with the new production pipeline.

Current policy:

- older imported art under `Assets/_Project/Features/Characters/Art/` should be treated as legacy/archive by default
- new Gemini / PixelLab / Aseprite work should use the newer source-vs-production separation rules

See:

- [ART-005](docs/art/art-005-legacy-and-production-asset-separation-policy.md)
- [ART-002](docs/art/art-002-sprite-production-pipeline.md)

## What was completed and is still valid

### Event system

- ScriptableObject event channels and listeners are in place and worth keeping.

### Character system

- Character definitions, stats, runtime state, elemental affinity, weapon family, and status-effect structure all exist in usable form.

### Combat identity direction

- The repo and docs now align around:
  - free-form movement
  - circular attack range bands
  - multi-layer combat identity

### Unity MCP workflow

- The local Unity MCP bridge is set up and useful for inspecting live editor state.

## What should happen next

### Immediate direction

Do **not** try to push many broad systems forward in parallel.

Instead:

1. treat `Dev_Sandbox` as the current implementation truth
2. reality-sync the remaining outdated repo summaries
3. separate legacy art from new production content
4. build one honest vertical slice

### Recommended vertical-slice target

The next meaningful slice should prove:

- one real battle flow
- one real team setup path
- one real encounter result/reward path
- one real character progression loop
- one real production-ready asset lane

Use:

- [DESIGN-008](docs/design/design-008-active-vertical-slice-definition.md)

as the explicit statement of that slice.

### Do not assume these are already solved

- gacha implementation
- networking
- multiplayer
- large scene flow
- polished input architecture
- production-ready UI system

## Relationship to other docs

- Use [DOC-GDD-001](docs/design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md) for intended game scope.
- Use [DOC-OPS-007](docs/ops/doc-ops-007-stage-1-unity-project-reality-audit.md) for the full current-project reality read.
- Use [DOC-OPS-006](docs/ops/doc-ops-006-unity-editor-version-policy-and-upgrade-checklist.md) for the current Unity editor baseline and upgrade rules.
- Use [ART-005](docs/art/art-005-legacy-and-production-asset-separation-policy.md) for legacy-vs-production asset handling.
