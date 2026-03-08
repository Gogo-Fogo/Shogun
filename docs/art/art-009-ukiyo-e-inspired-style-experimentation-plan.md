# ART-009: Ukiyo-e-Inspired Style Experimentation Plan

**Summary:** Controlled style-test plan for deciding how much `ukiyo-e` / ink / Vagabond-like influence belongs in `Shogun` without weakening readability, collection appeal, or gacha-facing color excitement.

## Why this note exists

`Shogun` has two competing visual instincts:

- a restrained `ukiyo-e` / ink / grayscale / Vagabond-like mood
- a colorful dark-fantasy gacha presentation with strong rarity and collection appeal

Both have value. The mistake would be hard-committing to either extreme before the team has actual Gemini + PixelLab output to compare.

So this note treats the `ukiyo-e` direction as an **experiment lane**, not the locked default.

## Current recommendation

The most likely sweet spot for `Shogun` is:

- `FEH` / `Ninja Blazing` readability
- dark-fantasy gacha color logic
- `ukiyo-e` composition and graphic flavor
- occasional ink / monochrome special moments

In other words:

- borrow `ukiyo-e` as **visual grammar**
- do not turn it into a **full palette prison**

## What to borrow from ukiyo-e

These traits pair well with pixel art and should be explored:

- bold silhouettes
- graphic line rhythm
- elegant asymmetry
- flat layered color zones
- strong composition with controlled empty space
- textile / paper / woodblock texture cues
- ornamental costume patterning that reads in large shapes
- expressive hair, sleeves, banners, and weapon lines

These are useful because they strengthen:

- roster identity
- premium-feeling presentation
- Japanese visual flavor
- strong still-image composition for portraits and event vignettes

## What to avoid

Do **not** make these the main shipped look:

- full grayscale or heavy monochrome for the whole game
- muddy low-contrast palettes
- over-delicate brush nuance that disappears at gameplay scale
- painterly gradients as the default sprite logic
- subtle linework that only works when zoomed in
- desaturated presentation that weakens rarity or element readability

These are dangerous because they can make:

- the roster blend together
- summons feel less exciting
- the game look prestigious but emotionally flat
- combat readability suffer

## Where monochrome belongs

Monochrome or near-monochrome treatment is best used as a **special-purpose accent**, not a constant baseline.

Recommended uses:

- flashbacks
- memory scenes
- cursed-state overlays
- boss intros
- special attack cut-ins
- chapter title cards
- roguelite event vignette art
- shrine / ritual / execution-ground story moments
- premium alternate skins or rare presentation variants

## Where saturated color must stay

Strong color is still required for the game to feel collectible and readable.

Keep saturated or high-clarity accent color in:

- summon reveals
- rarity communication
- element identity
- faction / pillar differentiation
- premium or banner-character presentation
- UI feedback
- VFX accents
- combat readability moments such as attacks, reactions, buffs, and hazards

The game should feel dark and elegant, but not visually starved.

## Recommended style lanes to test

When using Gemini and PixelLab, test at least these `3` lanes side by side:

### Lane A: Baseline dark-fantasy gacha

- readable mobile-first palette
- strong faction / element accents
- minimal ukiyo-e influence beyond costume and composition

Use this as the control.

### Lane B: Ukiyo-e-inflected color

- same readability and color clarity as Lane A
- stronger woodblock / paper / graphic-line influence
- flatter and more deliberate color blocking
- restrained base palette with carefully chosen accents

This is the most likely target lane.

### Lane C: Ink / monochrome special treatment

- grayscale or near-monochrome
- strong ink and paper influence
- selective color accent only where justified

Use this for special moments, not as the roster default.

## Production rule for Gemini + PixelLab experimentation

When the art pipeline enters active style testing:

1. generate one character concept in Gemini for all lanes
2. keep the same character fantasy, silhouette, costume role, and weapon
3. generate at least one portrait / presentation image for each lane
4. generate at least one playable `64x64` sprite direction test in PixelLab for each lane
5. compare them at:
   - portrait scale
   - gameplay scale
   - summon / collectible desirability
   - readability against likely battle backgrounds

Do not compare different characters to judge style. Compare the **same character fantasy** across styles.

## Evaluation criteria

Use these questions to judge the lanes:

### Collection / monetization

- does this make the character feel desirable?
- does this feel premium enough for a banner?
- does the roster still have enough visual dopamine?

### Gameplay readability

- can the character still be read clearly at battle scale?
- do elements and faction identity survive the palette treatment?
- does the sprite still pop against dark-fantasy backgrounds?

### World identity

- does the style feel recognizably `Shogun`?
- does it feel Japanese and dark-fantasy without becoming muddy?
- does it support both human and yokai characters?

### Production sustainability

- can Gemini reliably hit this lane?
- can PixelLab convert it cleanly into playable sprites?
- does this lane require too much cleanup?

## Working default until the experiment is complete

Until the style test proves otherwise, the working visual assumption for `Shogun` should be:

- color-first readability for gameplay
- controlled dark-fantasy palette
- ukiyo-e influence in composition, silhouette, and texture
- monochrome reserved for dramatic special-purpose scenes

## Decision rule

If a style lane looks more “artistic” but makes the roster less collectible or less readable, reject it as the default lane and keep it only as a special treatment.
