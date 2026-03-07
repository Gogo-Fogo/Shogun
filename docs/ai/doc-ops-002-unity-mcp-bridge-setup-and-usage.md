# Unity MCP Bridge Setup & Usage

**Summary:** Canonical setup and operating note for live Unity-editor access through MCP. Covers what is installed in this repo, how Codex and Claude connect, what is verified, what remains local-only, and how to use the bridge safely alongside the exporter.

## Purpose

Use the Unity MCP bridge when AI needs **live editor state** that code and static exports do not provide well:

- current open scenes
- current console logs
- current hierarchy objects and component state
- prefab and ScriptableObject inspection by path, name, or instance ID

Do **not** treat MCP as the source of truth for design or engineering intent. Canonical intent still lives in `DOC-GDD-001`, `DOC-ENG-001`, `DOC-ENG-002`, and `DOC-DATA-001`.

## Current implementation in this repo

This project currently uses the `CoderGamester/mcp-unity` Unity package.

Repo files added for this workflow:

- `Packages/manifest.json`
  - Adds `com.gamelovers.mcp-unity` from GitHub.
- `.mcp.json`
  - Repo-level Claude-compatible MCP config.
- `tools/mcp-unity/launch-mcp-unity.cjs`
  - Stable launcher that resolves the Unity package server from `Packages/` or `Library/PackageCache/`.

Machine-local configuration:

- Codex is registered globally as MCP server `unity` in the local user config.
- This is intentionally **not** stored in the repo because it is machine-specific.

## Why the launcher script exists

The Unity MCP window can generate a config that points directly into:

- `Library/PackageCache/com.gamelovers.mcp-unity@<hash>/Server~/build/index.js`

That path is brittle because the hash changes when Unity refreshes or updates the package cache.

The launcher script avoids hard-coding the cache hash and is the preferred entry point for both Codex and Claude.

## Verified status

The bridge has already been verified for read-oriented use in this repo.

Verified actions:

- read active/open scene info
- read Unity console logs
- inspect hierarchy objects by name/path
- inspect component state for runtime scene objects

Examples already confirmed in `Dev_Sandbox`:

- active scene: `Assets/_Project/Scenes/Dev/Dev_Sandbox.unity`
- hierarchy object: `CombatManagers`
- hierarchy object: `Characters/CharacterPrefab`

## Current limitations observed

- The bridge can inspect GameObjects by:
  - name
  - hierarchy path
  - instance ID
- The bridge does **not** currently expose a direct `get currently selected GameObject` command in the tested workflow.
- If selection matters, query the object by its hierarchy path or name instead.

## Local-only and legal handling

Keep this bridge local-only.

Operational rules:

- Leave remote connections disabled.
- Do not use MCP as a reason to paste sensitive data into consumer AI tools.
- Do not send personal data, support transcripts, receipts, secrets, or other sensitive material through AI prompts unless policy explicitly permits it.
- Keep using the exporter for broad snapshots and auditable handoff artifacts.

This aligns with the legal guidance in:

- `DOC-LEGAL-001`
- `DOC-LEGAL-002`

## Recommended workflow

### Default workflow

1. Use canonical docs for intent and architecture.
2. Use code search and normal editor review for implementation detail.
3. Use the exporter for broad, static project snapshots when needed.
4. Use Unity MCP only when **live editor truth** matters.

### Safe first-use workflow

Start read-only or inspection-heavy:

- list open scenes
- read Unity console logs
- inspect a GameObject by hierarchy path
- inspect a ScriptableObject or prefab reference target

Delay write operations until the bridge is trusted.

### Good first prompts

- `List the open Unity scenes`
- `Read the Unity console`
- `Inspect CombatManagers from hierarchy`
- `Inspect Characters/CharacterPrefab`

### Project-specific menu commands

These menu items exist specifically so MCP can do higher-value Shogun checks through `execute_menu_item` and then read the console:

- `Tools/Shogun/MCP/Report Active Scene Roots`
  - Logs root objects, child counts, and component names for the active scene.
- `Tools/Shogun/MCP/Report Selected GameObject`
  - Logs the selected object's hierarchy path, instance ID, and component list. This exists because the tested bridge does not expose direct selection lookup.
- `Tools/Shogun/MCP/Validate Active Scene Combat Setup`
  - Validates core combat-scene wiring: managers, drag handler, input bindings, test setup, main camera, EventSystem, and the `BattleManager.characterPrefab` reference.
- `Tools/Shogun/MCP/Report Battle Runtime State`
  - Logs current battle manager, turn manager, active/reserve characters, turn order, and scene `CharacterInstance` objects.
- `Tools/Shogun/MCP/Validate Render Pipeline Setup`
  - Validates the active render pipeline asset and its default renderer binding.
- `Tools/Shogun/MCP/Apply Default URP Asset`
  - Reassigns the project's default URP asset in editor state and repairs the default 2D renderer asset if the renderer binding has gone null.

### Avoid first

- scene mutation
- component mutation
- package installation through MCP
- asset creation in production scenes

## Relationship to the exporter

The exporter and MCP are complementary.

Use the exporter when you need:

- broad project snapshots
- auditable artifact generation
- file-based context handoff
- stable offline reference

Exporter commands now live under `Tools/Shogun/Export/`:

- `Export Current Scene Snapshot`
- `Export Selected Asset Snapshot`
- `Export Full Project Snapshot (Fallback)`

Exports are written to timestamped folders under `/_Generated/ProjectExport/` so fallback snapshots do not overwrite each other.

Use MCP when you need:

- current scene truth
- current Inspector-like component values
- current console state
- targeted live inspection

## Setup checklist

### Unity side

1. Open the Shogun project in Unity.
2. Let Package Manager resolve `com.gamelovers.mcp-unity`.
3. Open `Tools > MCP Unity > Server Window`.
4. Keep:
   - server enabled
   - remote connections disabled
5. If the Node server build is missing, use the Unity window's install/configure action.

### Codex side

Codex should point at the stable launcher script, not the raw PackageCache hash path.

Expected command shape:

```toml
[mcp_servers.unity]
command = "node"
args = ["G:\\Workspace\\Unity\\Projects\\Shogun\\tools\\mcp-unity\\launch-mcp-unity.cjs"]
```

### Claude side

Claude can use the repo-level `.mcp.json`:

```json
{
  "mcpServers": {
    "unity": {
      "type": "stdio",
      "command": "node",
      "args": ["./tools/mcp-unity/launch-mcp-unity.cjs"]
    }
  }
}
```

## Troubleshooting

### Unity says server is online but no clients are connected

- Restart Codex or Claude after Unity finishes importing packages.
- Confirm the client is using the launcher script, not an outdated PackageCache hash path.
- Confirm `node` is installed and available on the machine.

### MCP calls time out after package changes or script recompiles

- Unity package refreshes and domain reloads can leave the MCP socket in a stale state for the current client session.
- If this happens, wait for Unity to finish compiling, then restart the Codex or Claude client session.
- If needed, stop and start the Unity MCP server again from `Tools > MCP Unity > Server Window`.

### Unity 6000.0 package compatibility for this repo

This project is on Unity `6000.0.69f1`. During setup, newer 2D package lines caused package-level compile failures even though the MCP bridge itself was fine.

Known-good pins in this repo:

- `com.unity.2d.animation`: `10.2.2`
- `com.unity.2d.aseprite`: `1.1.10`
- `com.unity.2d.psdimporter`: `9.1.1`
- `com.unity.2d.spriteshape`: `10.0.7`
- `com.unity.2d.tilemap.extras`: `4.1.0`

If package compile errors reappear after an upgrade, check `Packages/manifest.json` first before assuming the MCP bridge is broken.

### Connection works but selected object lookup fails

- This is expected in the current tested workflow.
- Query by hierarchy path, object name, or instance ID instead.

### Console is flooded with MCP logs

- MCP bridge traffic appears in the Unity console.
- This is normal while testing.
- Filter by `Error` if you only want actionable engine/project issues.

### Active scene validation reports a render pipeline error

- Run `Tools/Shogun/MCP/Validate Render Pipeline Setup`.
- This specifically checks whether the current render pipeline asset has a valid default renderer entry.
- If it reports a null default renderer entry, run `Tools/Shogun/MCP/Apply Default URP Asset` and validate again.

## Status call for this repo

For `Shogun`, Unity MCP is worth keeping as a **small, local, read-first bridge**.

It is useful because the project already relies on:

- scene-wired manager objects
- ScriptableObject-driven content
- inspector-configured input/actions
- runtime editor inspection for combat setup

It should remain a helper layer, not a replacement for:

- canonical docs
- code review
- static exports
