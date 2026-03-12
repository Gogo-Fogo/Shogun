# ART-012 — Combat Cut-In Taxonomy And Shogun Mapping

> Established: March 11, 2026
> Based on: local Naruto Blazing cut-in asset study, DESIGN-010, ART-010, ART-011
> Status: first-pass taxonomy and production recommendation

---

## Purpose

This document separates the different presentation-art lanes used by Naruto Blazing and
maps them into a smaller, saner Shogun pipeline.

The goal is not to copy every Blazing variant one-to-one.
The goal is to understand which variants actually serve different gameplay moments, then
keep only the lanes Shogun needs for the vertical slice and near-term production.

---

## Reference set studied

Local asset folder reviewed on March 11, 2026:

`C:\Users\georg\Downloads\Cut-Ins-20260311T202634Z-1-001\Cut-Ins`

Observed filename families:

- `*_chain.png`
- `*_cutin.png`
- `*_rarecutin.png`
- `*_sevencutin.png`

Observed counts in the studied folder:

- `chain`: `933`
- `cutin`: `732`
- `rarecutin`: `502`
- `sevencutin`: `3`

Representative dimensions from the local set:

- `00003_chain.png` → `256x64`
- `00003_cutin.png` → `452x477`
- `00004_rarecutin.png` → `377x493`
- `01082_rarecutin.png` → `512x512`
- `01071_sevencutin.png` → `512x768`

---

## What the variants appear to do

### `chain`

This is the narrow eye-strip art.

Visual characteristics:

- very wide and very short
- face/eyes only
- readable even at tiny display size
- clearly built for a fast flash, not a held showcase

Likely role in Blazing:

- combo confirmation
- team attack / chain presentation
- very brief spectacle layer before hit resolution

This matches the current Shogun `comboCutInSprite` lane.

### `cutin`

This is the standard character cut-in.

Visual characteristics:

- larger than `chain`
- usually bust-up or upper-body action pose
- often shows hand signs, weapon wind-up, or a clear ability pose
- not cropped as tightly as the combo strip

Likely role in Blazing:

- regular jutsu / first active skill presentation
- general battle splash art
- broad UI/promo reuse when a character needs a single readable action image

This is not the combo strip.

### `rarecutin`

This is a premium cut-in variant, not just "another chain".

Visual characteristics:

- often based on a more elaborate illustration than the normal `cutin`
- tighter or stylized crop of premium art
- visually louder, more polished, and more dramatic than the standard `cutin`
- in some cases appears to be a cropped presentation layer derived from larger special art

Likely role in Blazing:

- premium special/ultimate presentation
- higher-rarity or more dramatic ability showcase
- stronger "hero moment" than the standard `cutin`

This maps best to Shogun's `ultimateCutInSprite`.

### `sevencutin`

This does not look like a separate core battle lane for the slice.

Visual characteristics:

- much larger full illustration
- reads like master splash art rather than a tiny runtime overlay
- premium composition with less aggressive crop

Likely role in Blazing:

- special rarity/evolution state presentation
- source/master art that can feed other cropped presentation assets
- premium collection/showcase lane, not the baseline combat lane

Shogun does not need a direct equivalent in the vertical slice.

---

## Shogun recommendation

Do **not** mirror Blazing's full asset explosion yet.

For the current Shogun slice, use these lanes:

1. `pfpSprite`
   - circular HUD face crop only
2. `comboCutInSprite`
   - narrow eye-strip for `2-unit` and `3-unit` combos only
3. `bannerSprite`
   - general large presentation art for menu, summon, barracks, and fallback promo use
4. `ultimateCutInSprite`
   - premium presentation art for the second ability / ultimate

That is enough for the slice.

### Important gap

Blazing appears to have a separate standard ability/jutsu cut-in lane that sits between
`chain` and `rarecutin`.

Shogun currently does **not** have a dedicated `abilityCutInSprite` field.

For now, the clean fallback is:

- regular ability presentation → use `bannerSprite`
- ultimate presentation → use `ultimateCutInSprite`
- combo flash → use `comboCutInSprite`

If first-ability spectacle becomes a real runtime feature later, add:

- `abilityCutInSprite`

Do not add it pre-emptively unless that presentation flow is actually implemented.

---

## Recommended Shogun sizing

### Combo eye strip

Use a dedicated narrow strip.

Recommended authored source size:

- standard: `256x64`
- optional high-res source: `512x128`

Rules:

- face/eyes only
- no full torso
- preserve a strong readable brow/eye line at tiny display size
- keep the focal area centered because slanted masks will eat edge space

### Regular banner / ability art

Use the existing `bannerSprite` lane for now.

Recommended source target:

- around `512x512` or equivalent square-ish splash crop

### Ultimate cut-in

Use a premium, more dramatic crop than the regular banner art.

Recommended source target:

- `512x512` minimum
- can be derived from a taller master illustration if needed

### Full premium master art

Only needed later if Shogun introduces:

- awakened rarity states
- collectible full-screen showcases
- special summon reveal screens

Until then, do not create a `sevencutin` equivalent lane.

---

## File naming recommendation

Current production-safe naming:

- `[AssetName]_pfp.png`
- `[AssetName]_banner.png`
- `[AssetName]_comboCutIn.png`
- `[AssetName]_ultimateCutIn.png`

Deferred future naming if a real first-ability cut-in is implemented:

- `[AssetName]_abilityCutIn.png`

Do not create `_rarecutin` or `_sevencutin` names in Shogun.
Those are useful as research terms, not good internal production names.

---

## Decision summary

Blazing seems to use at least four presentation-art layers:

- tiny combo eye strip
- standard ability cut-in
- premium rare/ultimate cut-in
- very large premium master art

Shogun should copy the structure, not the asset count.

For now:

- `chain` inspiration → `comboCutInSprite`
- `cutin` inspiration → `bannerSprite` for fallback regular ability presentation
- `rarecutin` inspiration → `ultimateCutInSprite`
- `sevencutin` inspiration → no direct slice lane yet

This stays aligned with:

- [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md)
- [ART-010](./art-010-ai-character-art-generation-pipeline.md)
- [ART-011](./art-011-ui-art-generation-pipeline.md)

