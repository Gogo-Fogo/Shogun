# Shogun Unity Project

## Overview
Shogun is a scalable, modular, turn-based RPG project inspired by best practices from Unity, Boss Room, and industry research. The project is organized for maintainability, team collaboration, and LiveOps.

## Folder Structure
```
Assets/
  _Project/
    Art/                # Global art assets
    Audio/              # Global audio assets
    Features/           # Feature-based organization
      Characters/       # Character system (scripts, prefabs, SOs, art)
      Combat/           # Combat system (scripts, prefabs, SOs, art)
      Gacha/            # Gacha system (scripts, prefabs, SOs, art)
      UI/               # UI system (scripts, prefabs, SOs, art)
    Prefabs/            # Global/system prefabs
    Scenes/             # Scenes organized by domain
    ScriptableObjects/  # Global/cross-feature SOs
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
- **Event-driven systems**: Use SO event channels for decoupling.
- **Logic/View separation**: Core logic is decoupled from presentation.
- **Assembly definitions**: Each major system/feature has its own .asmdef.
- **Networking separation**: Multiplayer code is isolated in Networking/.
- **Utilities**: Generic helpers/extensions in Core/Utilities.

## Getting Started
- See `docs/` for detailed guides (coming soon).
- Use the Unity Test Runner for automated tests in `Assets/Tests/`.

## Design Documents

- **[Game Design Document (GDD) for Shogun: Flowers Fall in Blood](docs/Game%20Design%20Document%20(GDD)%20for%20_Shogun_%20Flowers%20Fall%20in%20Blood_.txt)**  
  The master design document for the project, covering all systems, mechanics, and vision.

- **[Conceptual Synthesis for Naruto Shippuden: Ultimate Ninja Blazing](docs/Conceptual%20Synthesis%20for%20Naruto%20Shippuden_%20Ultimate%20Ninja%20Blazing.txt)**  
  A reference document analyzing the design and systems of Naruto Blazing for inspiration and comparison.

---
For more details, see the architectural blueprint and research sources in the project documentation.
