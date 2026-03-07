# Shogun Unity Project

## Overview
Shogun is a scalable, modular, turn-based RPG project inspired by best practices from Unity, Boss Room, and industry research. The project is organized for maintainability, team collaboration, and LiveOps.

## Folder Structure
```
Assets/
  _Project/
    Art/                # Global art assets
      Sprites/          # Character sprites organized by type
        Samurai/        # Samurai character sprites
        Ninja/          # Ninja character sprites
        Yokai/          # Mythological creatures
        Demons/         # Demon/dark fantasy characters
        Animals/        # Animal-themed warriors
        Special/        # Unique or special characters
      SourceFiles/      # Editable PSDs and source files (owned assets)
      Licenses/         # License and documentation files for assets
      References/       # Inspiration/reference art (gitignored)
    Audio/              # Global audio assets
    Features/           # Feature-based organization
      Characters/       # Character system (scripts, prefabs, SOs, art)
      Combat/           # Combat system (scripts, prefabs, SOs, art)
      Gacha/            # Gacha system (scripts, prefabs, SOs, art)
      UI/               # UI system (scripts, prefabs, SOs, art)
    Prefabs/            # Global/system prefabs
    Scenes/             # Scenes organized by domain
    ScriptableObjects/  # Global/cross-feature SOs and event channels
    Scripts/            # Core, utilities, networking, input, etc.
    AddressableAssetsData/ # Addressables config
    ...
  _ThirdParty/          # External plugins/assets
  _Sandbox/             # Experimental/prototype work
```

## Naming Conventions
- Use PascalCase for folders, files, and class names.
- Avoid spaces and special characters.
- Match namespaces to folder structure (e.g., Shogun.Features.Combat).

## Architectural Patterns
- **Feature-based organization**: All assets/scripts for a feature are co-located.
- **ScriptableObject architecture**: Data-driven design for characters, skills, etc.
- **Event-driven systems**: Use ScriptableObject event channels for decoupling (see below).
- **Logic/View separation**: Core logic is decoupled from presentation.
- **Assembly definitions**: Each major system/feature has its own .asmdef.
- **Networking separation**: Multiplayer code is isolated in Networking/.
- **Utilities**: Generic helpers/extensions in Core/Utilities.

## Event System (NEW)
- **ScriptableObject Event Channels**: Decoupled communication between systems using SO-based event channels.
- **Location**: `Assets/_Project/Scripts/Core/Architecture/EventChannelSO.cs` and `Assets/_Project/ScriptableObjects/Events/`
- **Usage**: Raise events from any system; listen with `EventListener` components or code.
- **Predefined Channels**: Combat, Character, UI, Gacha, Input, Scene events (see `GameEvents.cs`).
- **Inspector Friendly**: Set event channel descriptions in the Inspector for clarity.

## Art Asset Organization

- Character sprites are organized by type in `Assets/_Project/Features/Characters/Art/Sprites/`.
- Editable source files (PSDs) are in `SourceFiles/`.
- All licenses and documentation for assets are in `Licenses/`.
- Reference/inspiration art is kept in `References/` and is not tracked by git.

## .gitignore Policy for Art Assets

- All reference/inspiration art in `References/` is ignored by git and not tracked in version control.
- All owned/usable art assets, source files, and licenses are tracked for backup and collaboration.

## Input & Battle Drag System (2025-07-06 Update)
- **Complete BattleDragHandler rewrite** with robust tap-to-move and hold-to-drag functionality
- **Tap behavior**: Character runs smoothly to tap position with running animation (no teleport)
- **Hold behavior**: Character instantly teleports under pointer and follows smoothly (no running animation)
- **DragInputPanel**: A single, invisible UI panel under the main Canvas captures all pointer events for character movement
- **BattleDragHandler**: Handles both tap-to-move and drag-to-move, with grid snapping and proper animation state management
- **Input System UI Integration**: The Input System UI Input Module must have the following bindings in the UI action map:
  - `Point`: `<Pointer>/position` and `<Touchscreen>/position`
  - `Click`: `<Pointer>/press` and `<Touchscreen>/press`
- **Production ready**: All test/debug scripts removed, proper coroutine management, and transform handling for both parented/unparented characters
- **Troubleshooting**: If drag does not work, check the Input System UI Input Module bindings and ensure only one DragInputPanel exists under the main Canvas

## Getting Started
- Ensure your Input System UI Input Module is configured as above for pointer events.
- See `docs/PROJECT_CONTEXT_INDEX.md` for the canonical document router.
- Use the Unity Test Runner for automated tests in `Assets/Tests/`.

## Progress & Next Steps
- **✅ Event System Implemented**: ScriptableObject event channels and listeners are in place.
- **✅ Assembly Definitions**: All major systems have proper .asmdef and namespaces.
- **✅ Character Definition System**: Complete character system with Naruto Blazing-inspired mechanics.
  - CharacterDefinition ScriptableObjects for base data
  - CharacterStats with level progression and elemental effectiveness
  - CharacterInstance with runtime state and battle mechanics
  - CharacterFactory for creation and management
  - Comprehensive test suite
- **⏳ Next Up**: Implement Combat State Machine, TurnManager, and BattlefieldManager for turn-based combat.

## Character System Features

The character system implements core mechanics inspired by Naruto Blazing:

### Core Mechanics
- **Free-form Movement**: Characters can move anywhere on the battlefield (not limited to circles)
- **Circular Attack Ranges**: Short (1.5), Mid (3), Long (5) range circles
- **Elemental Effectiveness**: Heart > Skill > Body > Heart with 1.5x advantage/0.5x disadvantage
- **Turn-Based Actions**: Movement and attack tracking per turn
- **Status Effects**: Poison, heal, stun, silence, burn, freeze, bleed

### Character Types
- **Samurai, Ninja, Onmyoji, Monk, Ronin, Yokai, Demon, Animal**
- **Elemental Types**: Heart (Red), Skill (Green), Body (Yellow), Bravery (Blue), Wisdom (Purple)
- **Rarity System**: Common to Legendary with stat scaling
- **Special Abilities**: Unique skills with cooldowns

### Technical Features
- **ScriptableObject Architecture**: Data-driven character definitions
- **Event-Driven System**: Health changes, death, position changes, status effects
- **Factory Pattern**: Character creation and management
- **Comprehensive Testing**: Full test suite for all character functionality

## Design Documents

- **[Project Context Index](docs/PROJECT_CONTEXT_INDEX.md)**  
  Start here. This routes the canonical docs and defines document authority.

- **[Design Documentation Index](docs/design/DESIGN_INDEX.md)**
  Routing file for master game design, cross-disciplinary roster identity, world-pillar, combat-identity, and collection-planning notes.

- **[Art Documentation Index](docs/art/ART_INDEX.md)**
  Routing file for production-facing art standards, including style rules, sprite workflow, Unity import expectations, and provenance tracking.

- **[Operations Documentation Index](docs/ops/OPS_INDEX.md)**
  Routing file for MCP, backups, repo workflow, PixelLab tooling decisions, and retrospective operational notes.

- **[Legal Documentation Index](docs/legal/LEGAL_INDEX.md)**
  Routing file for legal, privacy, compliance, telemetry-compliance, and user-facing policy notes.

- **[Research Documentation Index](docs/research/RESEARCH_INDEX.md)**
  Routing file for engineering studies and comparative/reference analysis.

- **[Canonical GDD](docs/design/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)**  
  The current master design document for gameplay, systems, and product scope.

- **[Engineering Research](docs/research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md)**  
  The primary engineering document for platform, backend, content delivery, and production planning.

- **[Runtime Architecture Patterns](docs/research/doc-eng-002-unity-project-runtime-architecture-patterns.md)**  
  The canonical Unity-side implementation note for module boundaries, combat state flow, and gesture/input architecture.

- **[Unity MCP Workflow](docs/ops/doc-ops-002-unity-mcp-bridge-setup-and-usage.md)**  
  The canonical note for Codex/Claude Unity-editor access, local MCP setup, safe usage boundaries, and exporter-vs-MCP workflow.

- **[AI Safety & Backup Plan](docs/ops/doc-ops-003-ai-workspace-safety-and-backup-plan.md)**  
  The canonical note for repo scope rules, Codex/Claude safety posture, backup layers, and recovery steps after accidental local deletion.

- **[PixelLab Evaluation & Sprite Workflow](docs/ops/doc-ops-004-pixellab-evaluation-and-sprite-production-workflow.md)**  
  The canonical note for whether PixelLab fits `Shogun`, when to use subscription vs API, how it fits Codex/Claude/Unity MCP, and the recommended first sprite-production workflow.

- **[Style Bible & Visual Targets](docs/art/art-001-style-bible-and-visual-targets.md)**
  The current art-direction note for silhouette rules, palette discipline, detail limits, and gameplay readability.

- **[Character Collection & Fantasy Strategy](docs/design/design-001-character-collection-and-fantasy-strategy.md)**
  The strategy note for collectible fantasy, roster pillars, battle-vs-presentation art lanes, and how popularity/variants should shape the cast.

- **[World Pillars & Combat Identity Framework](docs/design/design-002-world-pillars-and-combat-identity-framework.md)**
  The structural note for faction pillars, elemental affinity, weapon families, martial schools, and how those layers combine into a coherent character identity.

- **[Sex Appeal & Damage-Art Policy](docs/art/art-006-sex-appeal-and-damage-art-policy.md)**
  The art-policy note for how far `Shogun` should push fanservice, how damage art should work, and why explicit jiggle-heavy battle animation is the wrong line.

- **[Sprite Production Pipeline](docs/art/art-002-sprite-production-pipeline.md)**
  The production workflow for turning PixelLab output into cleaned, reviewable game assets.

- **[Unity 2D Import & Animation Standards](docs/art/art-003-unity-2d-import-and-animation-standards.md)**
  Standards for `.aseprite` import, tag naming, frame-budget targets, and gameplay-facing animation expectations.

- **[Asset Provenance & Source Tracking](docs/art/art-004-asset-provenance-and-source-tracking.md)**
  The provenance note for tracking generated sources, manual edits, and shipped runtime assets.

- **[March 2026 Repo Modernization Retrospective](docs/ops/doc-ops-005-march-2026-repo-modernization-retrospective.md)**  
  Retroactive explanation for the March 7, 2026 cleanup/tooling batch, including the earlier pushed commits that landed without descriptive bodies.

- **[Naruto Blazing Reference](docs/research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)**  
  A reference document analyzing Naruto Blazing for inspiration and comparison only.

---
For more details, see the canonical docs under `docs/`. Generated exports are auxiliary artifacts, not source-of-truth documentation.

## For AI Assistants or New Contributors

**Onboarding Instructions (do this only at the start of a new session):**
- Before making any suggestions or changes, thoroughly review the entire project as a whole, with special attention to:
  - `docs/PROJECT_CONTEXT_INDEX.md` first, then only the relevant files it routes you to.
  - The `README.md` for project structure, conventions, and architectural patterns.
  - The latest `IMPLEMENTATION_PROGRESS.md` for current project status and next steps.
- Generated exports, when present, live under timestamped subfolders in `/_Generated/ProjectExport/` and are optional fallback snapshots rather than canonical documentation.
- If live Unity editor state matters and MCP is available, prefer the documented local Unity MCP workflow in `docs/ops/doc-ops-002-unity-mcp-bridge-setup-and-usage.md` instead of relying on large blind exports.
- These are the most important sources of truth. You may review any other part of the project as needed to fully understand the context.
- Use these as the source of truth for all design, architecture, and implementation decisions.
- **Do not repeat this process once you have reviewed the files for the current session.**

## Battle System (2025-07-05 Update)
- The battle system now uses a single, generic CharacterPrefab for all characters. All character-specific visuals, stats, and abilities are set at runtime from CharacterDefinition ScriptableObjects.
- Team size is fully flexible: you can start a battle with any number of characters (minimum 1).
- Characters spawn at the position of the CharacterPrefab in the scene, making it easy to control spawn locations visually.
- Character scale is set at runtime from the CharacterDefinition asset, so you can have different-sized characters without changing the prefab.
- The system is robust to missing or incomplete teams and will not throw errors if fewer than 6 characters are assigned.
- **Drag/tap input is fully functional for both mouse and touch, with all required Input System bindings documented above.**

## Character & Asset Folder Structure

- Each character has a unique folder under its category (e.g., `Art/Sprites/Samurai/Ryoma/`).
- All assets (sprites, anims, controllers) are named `[CharacterName]_[Action].ext` (e.g., `Ryoma_Run.png`, `Ryoma_Attack.anim`).
- Asset packs and ambiguous folders are archived or moved out of main asset paths.
- Sprite sheets from asset packs are moved and renamed to the correct character folders.
- Surnames are only used in file/folder names if disambiguation is needed, but both surname and givenName are stored in `CharacterDefinition` for in-game display.

## Project Hygiene & Automation

- Use the `Tools/Shogun/Project Cleanup Tools` Editor window to:
  - Delete empty folders
  - Find and report orphaned `.meta` files
  - Find and report duplicate files by name
- The tool provides a summary report in the Console. Orphaned `.meta` files can be deleted with a toggle.
- - **Important:** If you want to reserve an empty folder for future content, add a `.keep` or `README.txt` file inside it. The cleanup tool will NOT delete folders containing these files.
- All batch renaming, moving, and archiving of assets should be done using provided scripts/tools for consistency.

## Best Practices

- Always use the new Unity Input System for all input handling.
- All character differences are stored in `CharacterDefinition` ScriptableObjects.
- Animator Controllers and AnimationClips are auto-generated and assigned via Editor scripts.
- Maintain consistent naming and folder structure for all assets to ensure scalability and automation.
