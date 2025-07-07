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

## Asset & Folder Organization

- Characters and their assets are organized by category and unique character folder (e.g., `Art/Sprites/Samurai/Ryoma/`).
- All assets use the `[CharacterName]_[Action].ext` naming convention.
- Asset packs and ambiguous folders are archived or moved out of main asset paths.
- Sprite sheets from asset packs are moved and renamed to the correct character folders.
- Surnames are only used in file/folder names if disambiguation is needed, but both surname and givenName are stored in `CharacterDefinition` for in-game display.

## Automation & Project Hygiene

- Use the `Tools/Shogun/Project Cleanup Tools` Editor window to:
  - Delete empty folders
  - Find and report orphaned `.meta` files
  - Find and report duplicate files by name
- The tool provides a summary report in the Console. Orphaned `.meta` files can be deleted with a toggle.
- - **Important:** If you want to reserve an empty folder for future content, add a `.keep` or `README.txt` file inside it. The cleanup tool will NOT delete folders containing these files.
- All batch renaming, moving, and archiving of assets should be done using provided scripts/tools for consistency. 