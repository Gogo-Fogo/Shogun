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

## Getting Started
- See `docs/` for detailed guides (coming soon).
- Use the Unity Test Runner for automated tests in `Assets/Tests/`.

## Progress & Next Steps
- **✅ Event System Implemented**: ScriptableObject event channels and listeners are in place.
- **✅ Assembly Definitions**: All major systems have proper .asmdef and namespaces.
- **⏳ Next Up**: Implement CharacterDefinition ScriptableObjects, Combat State Machine, and Gesture Input System.

## Design Documents

- **[Game Design Document (GDD) for Shogun: Flowers Fall in Blood](docs/Game%20Design%20Document%20(GDD)%20for%20_Shogun_%20Flowers%20Fall%20in%20Blood_.txt)**  
  The master design document for the project, covering all systems, mechanics, and vision.

- **[Conceptual Synthesis for Naruto Shippuden: Ultimate Ninja Blazing](docs/Conceptual%20Synthesis%20for%20Naruto%20Shippuden_%20Ultimate%20Ninja%20Blazing.txt)**  
  A reference document analyzing the design and systems of Naruto Blazing for inspiration and comparison.

---
For more details, see the architectural blueprint and research sources in the project documentation.

## For AI Assistants or New Contributors

**Onboarding Instructions (do this only at the start of a new session):**
- Before making any suggestions or changes, thoroughly review the entire project as a whole, with special attention to:
  - All files in the `/docs` folder (especially the GDD and architecture docs).
  - The `README.md` for project structure, conventions, and architectural patterns.
  - The latest `IMPLEMENTATION_PROGRESS.md` for current project status and next steps.
  - The `/_FullProjectExport` folder for a snapshot of the current implementation and exported state.
- These are the most important sources of truth, but you may review any other part of the project as needed to fully understand the context.
- Use these as the source of truth for all design, architecture, and implementation decisions.
- **Do not repeat this process once you have reviewed the files for the current session.**
