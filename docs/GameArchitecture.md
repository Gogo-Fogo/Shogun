# Game Architecture

## Folder Structure
- **_Project/Features/**: Each feature (Characters, Combat, Gacha, UI) contains its own scripts, prefabs, ScriptableObjects, and art assets in subfolders.
- **_Project/ScriptableObjects/**: Global or cross-feature ScriptableObjects (e.g., GameSettings, EventChannels).
- **_Project/Prefabs/**: System/global prefabs (e.g., SystemRoot, SharedSceneRoot).
- **_Project/Art/**: Global art assets. Feature-specific art should be moved to the relevant feature folder.
- **_Project/Scripts/**: Core systems, utilities, networking, input, and editor tools.
- **_ThirdParty/**: External plugins and assets.
- **_Sandbox/**: Experimental and prototype work, not referenced by production scenes.

## Key Patterns
- **Feature-based organization**: All assets/scripts for a feature are co-located for easy refactoring and onboarding.
- **ScriptableObject-driven data**: Use SOs for static data/configuration. Place global SOs in ScriptableObjects/, feature-specific SOs in their feature folder.
- **Event-driven architecture**: Use SO event channels for decoupling systems.
- **Logic/View separation**: Core logic is in POCOs, presentation in MonoBehaviours.
- **Assembly definitions**: Each major system/feature has its own .asmdef for modularity and fast iteration.
- **Networking separation**: Multiplayer code is isolated in Networking/.
- **Utilities**: Generic helpers/extensions in Core/Utilities.

## Onboarding Notes
- Follow naming conventions: PascalCase, no spaces, match namespaces to folder structure.
- Add new features by creating a new folder in Features/ and co-locating all related assets/scripts.
- Place automated tests in Assets/Tests/.
- Update this documentation as the project evolves. 