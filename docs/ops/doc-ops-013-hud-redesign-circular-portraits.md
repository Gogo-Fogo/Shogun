# DOC-OPS-013 — HUD Redesign: Circular Portrait Rail

> Created: 2026-03-10
> Status: **[Now]** — actively being implemented

---

## Context

The current battle HUD bottom rail uses large rectangular slot cards (~248 px tall).
This is visually heavy, obscures the battlefield, and reads nothing like the
reference genre (Naruto Ultimate Ninja Blazing and similar mobile RPGs).

**Goal:** Redesign the bottom squad rail around compact circular portrait medallions,
retaining strong team presence while keeping the battlefield visible and interaction
one-thumb-friendly.

---

## How Blazing's Mechanic Works (reference truth)

The arc rings around each circular portrait in Blazing are **ability charge gauges,
not HP**:

- Arc fills gradually as the character takes turns / performs actions.
- **One full arc lap** → character can activate their **special ability** (single tap portrait).
- **Two full arc laps** → character can activate their **ultimate ability** (double-tap portrait).

HP is tracked separately (small bar either below the portrait or as an outer ring
in a distinct colour).

This is the mechanic the redesign must support visually, even if the underlying
game logic isn't wired yet — the slots should be built to receive it.

---

## Design Specification (from design-010 + user direction)

### Bottom Squad Rail

| Property | Target |
|---|---|
| Portrait shape | Circular medallion |
| Portrait diameter | 72–80 px (device-independent, safe-area aware) |
| Number of slots | 3 (active combat team) |
| Spacing | Even distribution; no cluster collapse |
| Rail height | Low-profile — battlefield must stay mostly visible |
| Tap target minimum | 48 dp (Apple/Google HIG) |

### Portrait States

| State | Visual |
|---|---|
| **Active (current turn)** | Scale ×1.2, bright highlight ring |
| **Ready (player's idle)** | Normal scale, neutral frame |
| **Ability ready** | Ability arc complete (1 lap) — glow pulse |
| **Ultimate ready** | Ability arc complete (2 laps) — distinct colour/pulse |
| **Dead** | Desaturate, cross or dim overlay |
| **Unavailable / enemy turn** | Slight dim, no tap feedback |

### Layers Inside Each Portrait Slot (bottom → top)

1. **HP bar** — slim horizontal bar directly below the circle (not inside the arc ring)
2. **Portrait circle** — character face/sprite clipped to circle mask
3. **Frame ring** — thin decorative ring (lacquer/ink aesthetic, not gold chrome)
4. **Ability charge arc** — starts empty, fills clockwise; two distinct fill colours for
   ability vs. ultimate threshold
5. **State overlay** — active highlight or dead desaturation on top

### Top Bar (unchanged from design-010)

- Top-left: turn order strip / initiative lane
- Top-centre: encounter objective / encounter name
- Top-right: speed toggle, auto-play toggle, menu

---

## Implementation Tasks

### Phase 1 — Layout (do first)

- [ ] Replace `SlotCard` rectangular prefab with `PortraitMedallion` circular layout group
- [ ] Build portrait circle using `Image` + circular `Mask` component
- [ ] Position three medallions in a horizontal group anchored bottom-centre
- [ ] Add slim HP bar (`Image` fill type = Filled, Horizontal) below each circle
- [ ] Confirm safe-area bottom padding applied

### Phase 2 — State logic

- [ ] Wire active-unit scale up / highlight from `TurnManager.OnTurnStarted`
- [ ] Wire HP fill from `CharacterInstance.CurrentHealth` / `MaxHealth`
- [ ] Add dead-state desaturation (grayscale material or colour multiply)

### Phase 3 — Ability charge arc

- [ ] Add radial-fill `Image` (fill type = Filled, Radial 360) behind portrait for ability charge
- [ ] Drive fill amount from `CharacterInstance` ability charge value (add field if needed)
- [ ] Switch fill colour / start second lap effect at 1.0 fill for ultimate threshold
- [ ] Wire double-tap recognition on portrait for ultimate activation

---

## What to Beat Blazing On

| Blazing does | We should do |
|---|---|
| Arc is ability charge — good, keep | Arc is ability charge — match this |
| 4–6 small icons crammed per circle | **One clear state per portrait** — cut the noise |
| Heavy gold chrome frame | Thin ink/lacquer ring — lighter weight |
| HP ring conflicts with charge arc | **Separate HP bar below** — no overlap |
| Busy even at idle | **Calm at idle**, obvious when something is ready |

---

## Anti-patterns (from design-010)

- **Bottom-wall syndrome** — the rail must not become a massive opaque block
- **Ornament over clarity** — if the ring/frame chrome is louder than the state it shows, cut it
- **Tiny unreadable status icons** — use shape and colour contrast, not small icons

---

## Files to Touch

| File | Change |
|---|---|
| `BattleHudController.cs` | Replace slot card builder with medallion builder |
| `BattleHudController.cs` | Add `UpdatePortraitState(slot, CharacterInstance)` |
| New: `PortraitMedallionSlot.cs` | Lightweight slot controller (HP bar, arc, state) |
| `Dev_Sandbox.unity` | Updated HUD Canvas layout (via MCP or Play-mode builder) |

---

## Success Criteria

Enter Play mode in Dev_Sandbox:

1. Bottom rail shows 3 circular portrait medallions, not rectangular cards.
2. Each portrait shows the correct character sprite clipped to a circle.
3. HP bar below each portrait reflects live HP.
4. Active unit portrait is visibly larger / highlighted.
5. Dead unit portrait desaturates.
6. Ability arc ring is present and ready to be driven by game logic (can be static/empty for now).
7. Battlefield is clearly visible above the rail — no bottom-wall syndrome.
