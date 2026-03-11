# DOC-OPS-008 Short-Term Implementation TODO

> Purpose: keep the near-term execution backlog strict, slice-bound, and low-noise.

## Current objective

Make `Assets/_Project/Scenes/Dev/Dev_Sandbox.unity` the trustworthy implementation slice for the authored `Courtyard Ambush` battle.

Use this file for work that materially advances the active slice only.

Do not add broad feature ambitions here unless they directly unblock the slice.

## Slice scope guardrails

- Only `Dev_Sandbox` is implementation-truth right now.
- `MainMenu` is a support front door only; `Summon`, `Battle_Prototype`, `UI_Demo`, `Barracks`, and `Settings` remain shell/support scenes.
- `Characters` and `Combat` are real but prototype-grade.
- `Summon` may exist as a local test harness, but real `Gacha`, broader `UI`, `Networking`, and broader `Input` remain planned architecture.
- New art/source work goes into the current `Source / Production` lanes.
- Imported-heavy legacy art remains archive material by default.

## Immediate work buckets

### Battle loop reliability

- `[Now]` Make turn order, action consumption, and end-turn flow deterministic in the slice.
- `[Now]` Prove clear win/loss resolution for the `Courtyard Ambush` encounter.
- `[Now]` Remove or isolate debug-only combat assumptions that break authored encounter flow.
- `[Next]` Add one narrow validation pass for battle-start state, missing references, and empty team setup.
- `[Do not start yet]` Do not expand into full combat-state-machine redesign until the current slice loop is reliable.

### Roster asset consistency

- `[Now]` Make the player trio and enemy trio visually coherent enough to evaluate readability in battle.
- `[Now]` Ensure all six slice definitions have trustworthy runtime data: stats, sprite/controller assignment, facing, and scale.
- `[Next]` Normalize import settings, pivots, sorting, and collider assumptions for slice-facing assets.
- `[Blocked by assets]` Higher-quality portrait/presentation replacements depend on new Gemini and PixelLab generation.
- `[Do not start yet]` Do not clean the entire legacy character archive while slice units are still being stabilized.

### Encounter setup for `Courtyard Ambush`

- `[Now]` Turn the scene from a sandbox spawn test into one authored encounter with fixed player/enemy lanes.
- `[Now]` Lock the battle objective, fail condition, and encounter-start presentation for the ambush.
- `[Next]` Add one encounter config or lightweight authored setup layer instead of hand-editing scene state forever.
- `[Next]` Introduce one hazard, pressure point, or spatial rule that proves the range/threat-geometry design.
- `[Do not start yet]` Do not branch into multiple encounters or multiple biomes before `Courtyard Ambush` is solid.

### Result and reward handoff

- `[Now]` Define the minimal post-battle result state the slice must reach: win, loss, restart, and return path.
- `[Next]` Add a simple result/reward placeholder that proves handoff without requiring full progression systems.
- `[Do not start yet]` Do not build full economy, gacha rewards, or permanent inventory grants from this slice.

### Visual consistency and animation minimums

- `[Now]` Reach a readable baseline where units do not spawn at broken scale and the battlefield framing makes tactical space clear.
- `[Next]` Give slice units the minimum viable motion standard: at least stable idle/readability, then narrow combat motion if required.
- `[Blocked by assets]` Stronger animation polish depends on approved production assets, not only legacy imports.
- `[Do not start yet]` Do not build a broad animation content pipeline before the slice proves gameplay value.

## Explicitly deferred

These do not belong in the short-term slice backlog unless they become true blockers:

- co-op, PvP, clans, or social systems
- backend launch infrastructure
- broad gacha implementation
- large-scale UI rewrite
- broad content expansion outside the first slice
- wide project cleanup unrelated to the slice





## Implementation note: authored ability data

- Authored specials may now live in AbilityDefinition assets plus an AbilityCatalog.
- This is a data-separation and validation move for the active slice, not a commitment to Fire Emblem Heroes-style inheritance or freeform loadouts.
