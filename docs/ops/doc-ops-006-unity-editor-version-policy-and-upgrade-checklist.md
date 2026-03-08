# Unity Editor Version Policy and Upgrade Checklist

**Summary:** Project-specific rule for which Unity editor line `Shogun` should use, when upgrades are allowed, why alpha/beta builds are out of scope for mainline work, and how to evaluate a stable upgrade safely.

## Current rule

- The committed project baseline remains Unity `6000.0.69f1` until a deliberate upgrade is approved and validated.
- The current preferred stable upgrade target is Unity `6000.3.10f1`.
- Do **not** open the main working copy in Unity alpha or beta editors.

This note exists because the repo already has a non-trivial package/tooling surface, and random editor churn is more expensive than it looks.

## Why this project should not chase alphas

For `Shogun`, an editor upgrade is not just a binary swap. It touches:

- built-in URP and Shader Graph package versions
- 2D package compatibility
- the Input System
- Aseprite importer behavior
- project settings and importer metadata
- MCP bridge compatibility

That means alpha excitement is not enough to justify a mainline upgrade.

For this repo:

- `Unity 6.5 alpha` is acceptable only in a disposable clone or test branch
- `Unity 6.3 LTS` is the sensible stable line if and when a longer-lived LTS upgrade is desired

## Current project-specific recommendation

### Stay on `6000.0.69f1` when:

- the current editor is stable
- active feature work matters more than editor support runway
- there is no specific fix or package requirement pushing the project forward
- the team cannot afford to lose time to package or importer drift

### Move to `6000.3.10f1` when:

- you want to settle on the newer Unity `6.3 LTS` line
- you want the longer support runway of `6.3 LTS`
- you are prepared to spend one controlled pass validating packages, rendering, builds, and tooling

### Do not move the main project to alpha/beta when:

- the goal is production work
- package compatibility matters
- mobile build stability matters
- the upgrade is motivated mostly by hype rather than a concrete project need

## Why `6000.3.10f1` is the right stable candidate

`6000.3.10f1` is a stable `Unity 6.3 LTS` editor release, not an experimental build.

That makes it the right target if the project wants:

- a more current LTS line
- a longer support window than `6.0 LTS`
- a stable editor family rather than preview features

At the same time, `6000.0.69f1` is already a recent `Unity 6.0 LTS` patch and is still a reasonable place to remain if stability this week matters more than migration.

## Upgrade policy

### Allowed upgrade path

- `6000.0.69f1` -> `6000.3.10f1`

### Disallowed mainline path

- `6000.0.x` -> `6.5 alpha` in the active main working copy

### Safe experimentation path

- clone repo or create disposable evaluation branch
- open **that copy** in the experimental editor
- validate
- discard if noisy or unstable

## Pre-upgrade checklist

Before opening `Shogun` in a different stable Unity editor:

1. Ensure `main` is clean and pushed.
2. Create either:
   - a disposable branch, or
   - a full repo copy
3. Confirm backups are current:
   - GitHub push complete
   - local backup snapshot complete
   - off-site mirror not critically stale
4. Note the current baseline:
   - editor version
   - `Packages/manifest.json`
   - `Packages/packages-lock.json`
   - current known-good URP/mobile settings

## Validation checklist for `6000.3.10f1`

After opening the disposable copy in the new editor, validate all of the following before adopting it:

### Project opening and compilation

- project opens without package-resolution dead ends
- scripts compile cleanly
- no new recurring console errors

### Package/tooling surface

- URP renders correctly
- Shader Graph does not introduce project-level breakage
- Input System remains functional
- Aseprite importer still works
- `mcp-unity` still resolves and connects

### Scene and UI behavior

- portrait UI still scales correctly
- safe area handling still works
- touch and drag input still work
- no obvious Canvas or sorting regressions

### Platform outputs

- Android build still succeeds
- iOS export still succeeds
- mobile startup and scene loading are normal

### Rendering and project settings

- render pipeline asset is still valid
- default renderer binding is still valid
- quality settings do not silently drift into a bad state

## Adoption rule

Only adopt the new Unity editor line if the disposable validation pass is clean enough that the upgrade reduces future risk more than it adds current churn.

If validation fails or becomes noisy:

- do not keep pushing through the migration
- discard the test copy or branch
- stay on the current baseline

## What to commit if the upgrade is adopted

If the project deliberately moves to `6000.3.10f1`, commit the editor/version shift as one intentional upgrade batch that includes:

- `ProjectSettings/ProjectVersion.txt`
- `Packages/manifest.json` if changed
- `Packages/packages-lock.json`
- any required Unity-generated settings changes that are clearly part of the version move

Do **not** mix unrelated gameplay or art work into the same upgrade commit.

## Watchlist for this repo

These areas deserve extra scrutiny on any Unity version change:

- URP / Shader Graph
- 2D package compatibility
- Aseprite importer output
- Input System UI integration
- MCP bridge package resolution
- mobile aspect-ratio and safe-area behavior

## Relationship to other docs

- Use `DOC-ENG-001` for the broad engineering recommendation that `Unity 6.3 LTS` is the current default for a new mobile production.
- Use this document for the **project-specific operational rule** for `Shogun`.
- Use `DOC-OPS-002` when the question is whether Unity MCP or package drift is part of the problem.
- Use `DESIGN-006` when the question is platform/display strategy rather than editor-upgrade governance.

## Official references

- Unity 6 support matrix: `https://unity.com/releases/unity-6/support`
- Unity 6 releases overview: `https://unity.com/releases/unity-6/`
- Unity `6000.3.10f1`: `https://unity.com/releases/editor/whats-new/6000.3.10f1`
- Unity `6000.0.69f1`: `https://unity.com/releases/editor/whats-new/6000.0.69f1`
- Unity `6000.5.0a8` alpha page: `https://unity.com/releases/editor/alpha/6000.5.0a8`
