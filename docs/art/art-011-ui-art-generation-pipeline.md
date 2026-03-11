# ART-011 — UI Art Generation Pipeline

> Established: March 2026
> Based on: BattleHudController, BattleResultController, TurnCountdownIndicator, RangeCircleDisplay, DESIGN-010
> Status: initial spec — prompts proven for Shogun battle UI aesthetic

---

## Purpose

This document covers AI-assisted generation of UI chrome assets — buttons, frames, panel
backgrounds, icon borders, and decorative elements for the battle HUD and result screens.

This is **not** character portrait art. See ART-010 for character art.
This is **not** gameplay sprite art. See ART-010 Phase 4 (PixelLab) for sprites.

---

## Tool split — the scale + aesthetic rule

The key principle: **match the tool to the output size and aesthetic layer.**

| Tool | UI elements | Why |
|---|---|---|
| **Gemini** | Panel frames, button art, medallion borders, portrait borders, background overlays, decorative corner pieces, combo trackers, result cards | Larger surfaces (≥ 64px). Painterly style must match character portrait art — pixel art here would clash |
| **PixelLab** | Skill icons (32–64px), status effect icons, small gameplay-view UI icons | Tiny icons; crisp pixel edges; live next to battle sprites on the field; Gemini smears at small sizes |
| **Unity native** | Button shapes, layout containers, text, HP bar fills, colour tinting at runtime | No AI needed — Unity handles shape and layout; Gemini textures are applied as Sprite overlays |
| **Font tool** | Typography, damage numbers, label rendering | Separate pipeline — see font section below |

**Practical rule:** anything ≤ 64px displayed in the gameplay viewport → PixelLab.
Everything else visible in the UI layer → Gemini.

---

## Visual language (from DESIGN-010)

These rules govern all UI art in Shogun. Do not re-litigate them.

### Colour language
| Colour | Meaning |
|---|---|
| Cool teal / cyan (0.24, 0.76, 1.0) | Ally active, player-owned, safe state |
| Red / cinnabar (0.89, 0.28, 0.22) | Enemy threat, danger, low HP |
| Amber / gold (1.0, 0.72, 0.24) | Combo hit, premium signal, charge threshold |
| Warm parchment / brown (0.12–0.42, 0.11–0.30, 0.07–0.16) | Panel backgrounds, buttons, card surfaces |
| Near-black charcoal (0.03, 0.03, 0.04) | Frames, borders, outlines |
| Cream (0.98, 0.95, 0.84) | Text, labels, top highlights |

### Shape language
- **Circles** = basic attack range, player/ally identity
- **Dashed / lighter** = predicted or passive state
- **Solid / bright** = confirmed threat or active state
- **Angular / chamfered corners** = combat UI (not rounded — avoids soft/consumer feel)
- **Ornamental borders** = kamon motifs, mon engravings, Japanese geometric repeats

### Aesthetic formula
```
Dark charcoal/iron frame + warm parchment/lacquer interior + gold/bronze accent line
```
This matches the character art formula (dark armor + element accent) at the UI scale.
Gold trim on UI = same premium signal as gold trim on armor.

### What to avoid
- Flat solid-colour rectangles with no texture — reads as prototype/placeholder
- Glossy or glowing buttons — too modern/sci-fi
- Perfectly clean sans-serif borders — too flat, no depth
- Neon colours — reserved for element effects on characters, not chrome UI

---

## Gemini prompt rules for UI art

The character art gotchas (ART-010) mostly apply here too. Additional UI-specific rules:

**1. Always specify dimensions**
State the output dimensions in pixels. UI assets must be sized correctly.
Gemini will respect these better than character portrait art, which is more compositional.

**2. State what the interior is for**
The interior of panels, buttons, and frames must be flat/clean — Unity will overlay text or other
elements. Describe this explicitly: "interior is empty — text will be overlaid by Unity."

**3. No glow bleed on functional surfaces**
Decorative glows and textures must not obscure the interior content area.
Specify: "glow/decoration is limited to the outer border only."

**4. Request crisp edges**
UI assets need clean edges for Sprite slicing (9-slice in Unity).
Add: "crisp clean edges, no soft bleed at borders, ready for 9-slice sprite."

**5. White background always**
Same rule as character art: "Solid flat white background. NO checkerboard."

**6. ONE IMAGE ONLY**
Same rule as character art. Always include this.

---

## Proven Gemini prompts — battle UI elements

Send each as a **separate Gemini message**.

---

### Portrait Medallion Frame system — charge orb design

The medallion frame uses a **charge-orb-in-hole** system.
The frame ring has N circular holes punched through it — one per charge slot.

**Hole count = SPECIAL charge requirement** (not ultimate).
N holes = how many turns to fill before the special ability is ready.

**Two-cycle charge system:**
- **Cycle 1 — blue:** holes fill with cyan-blue orbs one per turn. All blue = special ability ready (tap once).
- **Cycle 2 — red:** after special fires (or blue cycle completes), holes refill one-by-one with red orbs. All red = ultimate ability ready (double-tap).
- Using the special ability refunds the red (cycle 2) orbs and resets to the start of the next blue cycle.

The frame itself has no colour. Blue and red orb sprites are placed by Unity at runtime into the hole positions.

**Vertical slice — status:**

| File | Holes | Characters | Status |
|---|---|---|---|
| `MedallionFrame_3hole.png` | 3 | RoninFootman, Kuro (special@3) | Pending — or use 4-hole with dead slot (see Option B below) |
| `MedallionFrame_4hole.png` | 4 | Tsukiko, OniBrute, YureiCaster (special@4) | ✅ Done — confirmed template design |
| `MedallionFrame_5hole.png` | 5 | Ryoma (special@5) | Pending |
| `MedallionFrame_8hole.png` | 8 | future character | ✅ Done — bonus |

**Option B for 3-hole:** Use the 4-hole frame for Ronin Footman and Kuro too.
The 4th socket never lights — permanently dark, a visual cue that slot is unused.
Simpler asset pipeline (all medallions use the same prefab). Recommended for vertical slice.

**Confirmed template design (chrysanthemum + seigaiha + Greek key):**
Dark iron with aged ochre-gold accents. Chrysanthemum flowers and seigaiha (wave scale) pattern
engraved on the ring surface. Greek key (meander) border on the outer rim. Bronze socket collars
around each hole. This aesthetic must be matched across all variants.
Source file: `MedallionFrame_4hole_v1.png`

**Post-processing required:** Holes output as white from Gemini — cut to transparent alpha in Aseprite
(magic wand each hole after background removal). Required for orb sprites to show through.

**Unity orb placement formula:**
`holeAngle = (360 / holeCount) * i − 90°`  (top = 0, clockwise)
`holeX = centerX + orbRadius * cos(holeAngle)`
`holeY = centerY + orbRadius * sin(holeAngle)`
where `orbRadius ≈ 82px` for a 220px frame (hole centres sit ~82px from frame centre).

**Gemini workflow notes (learned from session):**
- When a reference image is attached, Gemini outputs **1 image** (not 4). This is normal.
- Specify hole positions as **clock positions**, not a count: Gemini approximates counts but follows explicit positions.
  - 3-hole: "12, 4, 8 o'clock — triangle, NO fourth hole"
  - 5-hole: "12, 2, 5, 7, 10 o'clock — pentagon"
- For 3-hole specifically: **do NOT attach the reference image** — Gemini biases to 4 holes when the 4-hole template is attached. Describe the style from scratch.
- For all others: attach `MedallionFrame_4hole_v1.png` as reference and change only the socket count + positions.

---

### Portrait Medallion Frame — 6 holes (Kuro / RoninFootman tier)

```
ONE IMAGE ONLY.
Circular portrait frame for a Japanese samurai mobile game.
Dark iron + gold engraving aesthetic. Dimensions: 220 × 220px.
EXACTLY 6 circular holes evenly spaced around the ring — no more, no fewer.
Each hole: approximately 22px diameter. Thin gold ring border around each hole — like a gem setting.
All 6 holes perfectly equidistant, evenly distributed around the full 360° of the ring.
Overhead view. Kamon/sakura/seigaiha wave engravings on the ring surface.
Greek key outer border. Gold accent ring near the inner edge.
Center circle empty — portrait art placed here by Unity.
Background: solid flat white. NO checkerboard.
Square format 220×220. No text. No watermark.
```

---

### Portrait Medallion Frame — 8 holes (Tsukiko / OniBrute / YureiCaster tier)

```
ONE IMAGE ONLY.
Circular portrait frame for a Japanese samurai mobile game.
Dark iron + gold engraving aesthetic. Dimensions: 220 × 220px.
EXACTLY 8 circular holes evenly spaced around the ring — no more, no fewer.
Each hole: approximately 20px diameter. Thin gold ring border around each hole — like a gem setting.
All 8 holes perfectly equidistant, evenly distributed around the full 360° of the ring.
Overhead view. Kamon/sakura/seigaiha wave engravings on the ring surface.
Greek key outer border. Gold accent ring near the inner edge.
Center circle empty — portrait art placed here by Unity.
Background: solid flat white. NO checkerboard.
Square format 220×220. No text. No watermark.
```

---

### Portrait Medallion Frame — 10 holes (Ryoma tier)

```
ONE IMAGE ONLY.
Circular portrait frame for a Japanese samurai mobile game.
Dark iron + gold engraving aesthetic. Dimensions: 220 × 220px.
EXACTLY 10 circular holes evenly spaced around the ring — no more, no fewer.
Each hole: approximately 18px diameter. Thin gold ring border around each hole — like a gem setting.
All 10 holes perfectly equidistant, evenly distributed around the full 360° of the ring.
Overhead view. Kamon/sakura/seigaiha wave engravings on the ring surface.
Greek key outer border. Gold accent ring near the inner edge.
Center circle empty — portrait art placed here by Unity.
Background: solid flat white. NO checkerboard.
Square format 220×220. No text. No watermark.
```

**The 10-hole frame was already generated and approved. Re-generate only if needed.**

---

### Charge Orb sprites — PixelLab (3 states, 24×24px each)

Small size → PixelLab, not Gemini. These sit inside the frame holes at runtime.

```
Three circular orb/gem sprites. Each 24×24px. Pixel art. Transparent background.

Sprite 1 — empty socket (uncharged):
Dark iron-grey circle. Very dim gold ring edge. No glow.
Nearly invisible — just a dark socket impression.

Sprite 2 — special-charging orb (blue):
Vivid cyan-blue glow (RGB 0.24, 0.76, 1.0). Bright center, soft glow edge.
Used during Cycle 1 (charging toward special ability).

Sprite 3 — ultimate-charging orb (red):
Vivid crimson-red glow (RGB 0.89, 0.20, 0.16). Same shape, red instead of blue.
Used during Cycle 2 (charging toward ultimate). Replaces blue orbs one by one after special fires.
```

---

### Battle Result Card Panel

```
ONE IMAGE ONLY.
UI card panel for a Japanese samurai mobile game.
Dimensions: 480 wide × 340 tall.
Style: aged parchment card surface — warm brown (approximately RGBA 0.12, 0.11, 0.08).
Outer border: dark charcoal near-black with thin gold corner accent filigree — like a collected character card.
Border has slight worn leather or lacquer texture — NOT a flat solid colour.
Top of card: ornate horizontal divider band with small mon/crest motif at center.
Bottom of card: matching divider band.
Four corners: subtle kamon-style corner filigree, gold on dark.
Interior: flat empty area — Win/Loss text and buttons will be placed by Unity.
Crisp clean outer edge. No inner texture in the content area.
Background: solid flat white. NO checkerboard.
No text. No watermark.
```

---

### Button Background — Primary Action

```
ONE IMAGE ONLY.
UI button background for a Japanese samurai mobile game.
Dimensions: 200 wide × 58 tall. Rectangle. Angular corners — NOT rounded.
Style: dark warm brown surface (approximately RGBA 0.42, 0.30, 0.16) with
  subtle worn wood-grain or leather texture — very light, not distracting.
Border: 2px dark charcoal outline. Thin gold/bronze accent line along top edge only — highlight.
Interior is flat — "RESTART" or other text will be overlaid by Unity.
NOT glossy. NOT plastic. Flat premium surface with minimal texture.
Crisp clean edges — ready for 9-slice.
Background: solid flat white. NO checkerboard.
No text. No watermark.
```

---

### Button Background — Secondary / Back Action

```
ONE IMAGE ONLY.
Same button as primary above but darker — approximately RGBA (0.28, 0.16, 0.14).
Slightly cooler, more burgundy-dark — this is a secondary/back navigation button.
Same 2px charcoal border and thin gold accent line on top edge.
Interior flat and empty — text overlaid by Unity.
Crisp clean edges.
Background: solid flat white. NO checkerboard.
No text. No watermark.
```

---

### HP Bar Frame (grounded strip under each character)

```
ONE IMAGE ONLY.
UI health bar frame for a Japanese samurai mobile game.
Dimensions: 300 wide × 22 tall. Very thin rectangle.
Outer frame: dark charcoal (approximately RGBA 0.03, 0.03, 0.04) — almost black iron.
Inner background: dark warm brown (approximately RGBA 0.16, 0.11, 0.07) — old wood or leather.
Left end: slight inward notch or inset — marks where HP fill begins.
Corners: slightly chamfered — angular not rounded.
Interior is hollow — runtime gradient fill (red to green) placed inside by Unity.
Crisp edges for 9-slice. Top and bottom borders are the iron frame, interior is the brown fill area.
Background: solid flat white. NO checkerboard.
No text. No watermark.
```

---

### Combo Tracker Panel Background

```
ONE IMAGE ONLY.
UI combo counter background for a mobile battle game.
Dimensions: 220 × 220px.
Style: radial soft glow from center — warm amber-gold (RGBA 1.0, 0.72, 0.24)
  transitioning to orange (RGBA 1.0, 0.82, 0.28), fading to near-transparent at corners.
Overlay: one subtle diagonal ink brushstroke across center — thin calligraphy slash effect,
  slightly brighter than the glow. Like a ghost of a sword arc.
This is an atmospheric background — "47 HITS" text and counter will sit on top.
NOT a solid shape. NOT a card border. Pure atmospheric glow + brushstroke.
Background: solid flat white. NO checkerboard. The glow fades to fully transparent at edges.
Square format. No text. No watermark.
```

---

### Objective Pill

```
ONE IMAGE ONLY.
UI pill-shaped label background for a mobile game HUD.
Dimensions: 320 wide × 44 tall. Fully rounded cap ends (stadium / pill shape).
Style: dark charcoal interior (approximately RGBA 0.10, 0.09, 0.07).
Outer border: thin gold/bronze line — 1px, follows the pill outline.
Very subtle worn paper texture inside — almost flat, not distracting.
"Defeat all enemies" text will be overlaid by Unity — interior must be clean.
Crisp clean edges.
Background: solid flat white. NO checkerboard.
No text. No watermark.
```

---

### Element Badge Frame (for elemental type indicator under characters)

```
ONE IMAGE ONLY.
UI circular badge for a Japanese samurai mobile game — displays an element letter (F, W, E etc.).
Dimensions: 64 × 64px output.
Style: outer ring — thin gold/bronze inlaid ring, like a coin or medal face.
Interior circle: dark iron (approximately RGBA 0.03, 0.03, 0.04), flat and clean.
This is the FRAME only — no colour fill, no letter. The element colour tint and
  letter text are applied at runtime by Unity.
The outer ring engraving should be subtle — a thin geometric kamon-suggest pattern.
Interior is fully flat — must accept colour tinting cleanly.
Crisp edges for circular mask in Unity.
Background: solid flat white. NO checkerboard.
Square format 64×64. No text. No watermark.
```

---

### Combo Cut-In Stripe Texture

```
ONE IMAGE ONLY.
UI diagonal stripe texture for a mobile game combo animation (displayed at -13° rotation).
Dimensions: 980 wide × 124 tall.
Style: dark lacquer-black surface with very subtle horizontal grain — lacquered wood or dark silk.
Left 20% of width: fades to fully transparent (horizontal alpha vignette).
Right 20% of width: fades to fully transparent.
Center 60% of width: fully opaque.
Top edge: very thin gold/bronze horizontal line — 1px accent along the top.
Interior is clean and flat — character portraits placed inside by Unity.
Crisp horizontal edges at top and bottom. Transparent at left and right ends.
Background: solid flat white. NO checkerboard.
Wide landscape format. No text. No watermark.
```

---

### Turn Number Badge Background (under characters, shows whose turn it is)

```
ONE IMAGE ONLY.
UI diamond or square badge for a Japanese samurai mobile game turn indicator.
Dimensions: 48 × 48px.
Style: dark iron plate (approximately RGBA 0.03, 0.03, 0.04), rotated 45° to appear as diamond.
Very thin gold border line around the diamond shape.
Interior is flat — a large turn number will be overlaid by Unity.
When enemy: red tint applied at runtime. When current: amber/yellow tint. Neutral: dark iron.
Crisp edges.
Background: solid flat white. NO checkerboard.
Square format. No text. No watermark.
```

---

## PixelLab — skill icons and small gameplay icons

For the vertical slice, skill icons are deferred (skill design not yet locked).
When skill icons are needed, use PixelLab with this approach:

### Skill icon specification

| Property | Value |
|---|---|
| Size | 48×48px or 64×64px |
| Background | Transparent (PixelLab outputs with alpha) |
| Palette | 2–3 colours max — dark base + one vivid element accent |
| Style | Pixel art, silhouette-readable |
| Shape | Simple — weapon silhouette, elemental symbol, or action shape |

### Skill icon design rules

- **Silhouette-first** — if you can't read the icon from its black shadow, redesign it
- **One element accent** — the skill's element colour is the only vivid colour
- **No text, no fine detail** — invisible at 48px
- **Distinct between skills** — each icon must read differently at a glance

### PixelLab skill icon description template

```
[Character weapon/element] skill icon. [Icon shape/subject in one clear sentence].
Palette: dark [base colour], [element accent colour] only.
Silhouette must read clearly at 48px.
Simple, no fine detail. Pixel art style.
Transparent background.
```

**Example — Ronin Footman basic slash:**
```
Katana slash icon. Single diagonal sword swing with crimson red energy arc.
Palette: dark iron grey, vivid crimson red only.
Silhouette must read clearly at 48px. Simple shape, no fine detail.
Pixel art style. Transparent background.
```

---

## Font strategy — vertical slice

**For vertical slice:** Use Unity default UI font (Arial/LiberationSans).
Focus is on battle loop completion, not visual polish.

**Production font requirements (when needed):**
- Must support CJK characters (Japanese names displayed correctly)
- Must be readable at small sizes (18–22pt on mobile)
- Must feel premium / game-appropriate — not a Google Docs font
- Licensing: must be commercial-use-safe

**Candidate shortlist (evaluate when meta-progression UI is scoped):**
- Noto Sans JP (Google Fonts, free, full CJK support)
- Zen Antique (Google Fonts, free, calligraphic-adjacent)
- Custom bitmap font for damage numbers (PixelLab or Aseprite)

**Damage number recommendation:** custom pixel font at 2× resolution
(renders at 168pt but drawn at 84pt pixel art scale) — crisp at any size, reads as intentional.

---

## Element colour runtime tinting

Most badge and frame assets are generated in **neutral dark iron**.
The element accent colour is applied at **runtime in Unity** via `SpriteRenderer.color`
or `Image.color` tinting.

This means we generate ONE badge frame, ONE icon background — Unity tints it per character.
Do not generate separate versions for Fire, Water, Earth etc.

| Element | Unity tint colour |
|---|---|
| Fire | RGBA(0.92, 0.30, 0.20, 1) |
| Water | RGBA(0.20, 0.64, 0.90, 1) |
| Earth | RGBA(0.73, 0.57, 0.27, 1) |
| Wind | RGBA(0.31, 0.81, 0.49, 1) |
| Lightning | RGBA(0.93, 0.77, 0.21, 1) |
| Ice | RGBA(0.49, 0.89, 0.97, 1) |
| Shadow | RGBA(0.52, 0.34, 0.78, 1) |

---

## File placement — where UI art goes

```
Assets/_Project/Art/Production/UI/
  HUD/
    MedallionFrame_3hole.png     ← pending (or use 4hole with dead slot)
    MedallionFrame_4hole.png     ← ✅ done — confirmed template
    MedallionFrame_5hole.png     ← pending
    MedallionFrame_8hole.png     ← ✅ done — bonus variant
    hp_bar_frame.png
    objective_pill.png
    combo_tracker_bg.png
    combo_cutin_stripe.png
    turn_badge.png
    element_badge_frame.png
  Buttons/
    button_primary.png
    button_secondary.png
  Panels/
    result_card_panel.png
```

Create this folder structure before importing. Unity's asset database needs the folder to exist.

**Source files (Gemini raw output, pre-alpha-cleanup):**
```
Assets/_Project/Art/Source/Gemini/UI/HUD/
  MedallionFrame_4hole_v1.png     ← confirmed template (chrysanthemum + seigaiha + Greek key)
  MedallionFrame_8hole_v1.png     ← bonus variant
```
Naming convention: `MedallionFrame_Nhole_v1.png` where N = hole count. Drop `_v1` suffix for production-ready files.

### Unity import settings for UI art

| Asset type | Texture Type | Sprite Mode | Max Size |
|---|---|---|---|
| Panel frames, result card | Sprite (2D and UI) | Single | 512 |
| Button backgrounds | Sprite (2D and UI) | Single | 256 |
| Medallion frames | Sprite (2D and UI) | Single | 256 |
| Skill icons, badges | Sprite (2D and UI) | Single | 64 |
| Cut-in stripe | Sprite (2D and UI) | Single | 1024 |

**Filter Mode:** Bilinear for panels/frames. Point (no filter) for PixelLab pixel art icons.

---

## Quick reference — UI art troubleshooting

| Problem | Fix |
|---|---|
| Interior shows texture (text unreadable over it) | Add "interior is flat, fully clean — text overlaid by Unity" |
| Edges are soft / bleed | Add "crisp clean edges, no soft bleed, ready for 9-slice" |
| Frame looks too thin/delicate | Specify border thickness explicitly in pixels |
| Glow bleeds into content area | Add "glow limited to outer border ring only, does not bleed interior" |
| Button looks glossy / modern | Add "NOT glossy, NOT plastic — flat premium surface, barely textured" |
| Asset too dark to distinguish from background | Add "against white background, all dark elements must have visible edge definition" |
| Checkerboard appears | Add "Solid flat white background. NO checkerboard." |
| Element badge tinting looks wrong in Unity | Check that badge interior is pure neutral dark — no pre-applied colour |
