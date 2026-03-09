# Legacy And Production Asset Separation Policy

> Purpose: keep imported archive assets, generated source assets, and approved runtime assets from collapsing into one ambiguous folder mess.

## Why this exists

`Shogun` already contains a large amount of older imported character art and source material.

That content is useful, but it should not be treated as identical to:

- new Gemini concept art
- new PixelLab sprite output
- Aseprite working files
- approved runtime-ready assets

Without separation, the project will drift into:

- unclear canonical art ownership
- broken provenance
- duplicate or contradictory versions of the same character
- harder cleanup later

## Asset classes

### Legacy

Use for:

- imported packs
- archive material
- earlier exploration assets
- source material that is useful to keep but is not part of the active production path

Legacy content is allowed to inform production, but it is **not** automatically canonical.

### Source

Use for editable working material that feeds production:

- Gemini concept exports
- PixelLab trials and approved source outputs
- Aseprite working files
- event-vignette source compositions

Source assets are not necessarily shippable. They are working material.

### Production

Use for assets that the current game build is intended to consume:

- playable sprites
- approved portraits
- approved event vignette finals
- approved animation assets

Production assets are the canonical runtime-facing art layer.

## Recommended folder model

The long-term target should be:

```text
Assets/_Project/Features/Characters/
  Art/
    Legacy/
      ImportedPacks/
    Production/
      PlayableSprites/
      Animations/
      Portraits/
      EventVignettes/
    Source/
      Gemini/
        Concepts/
        Portraits/
      PixelLab/
        Trials/
        Approved/
      Aseprite/
```

This policy is conceptual first. It does **not** require a reckless immediate move of the existing art tree.

## Migration rule

Do not mass-move the current old asset tree blindly.

Instead:

1. treat the current imported-heavy tree as legacy/archive by default
2. start all new generated and approved work under the new canonical source/production structure
3. migrate or quarantine older folders only when their references are understood

This keeps the project stable while giving new work a cleaner home.

## Canonical rule for new AI-assisted assets

### Gemini

Gemini outputs belong in `Source/Gemini/`.

Typical uses:

- portraits
- busts
- full-body concept art
- event vignette composition references

### PixelLab

PixelLab outputs belong in `Source/PixelLab/`.

Typical uses:

- playable sprite trials
- directional sprite conversions
- style-matched event-scene finals before approval

### Aseprite

Aseprite working files belong in `Source/Aseprite/`.

Final exported approved assets should move into `Production/`.

## Approval rule

An asset becomes production-canonical only when:

1. it has a clear character or feature owner
2. it passed the current style and readability standard
3. its provenance is captured per `ART-004`
4. it is placed in the correct `Production/` folder

If those conditions are not met, the asset remains source or legacy material.

## Practical interpretation for the current repo

For now:

- old imported pack material under `Assets/_Project/Features/Characters/Art/` should be read as legacy/archive unless explicitly promoted
- new Gemini and PixelLab work should not be placed into those old pack paths by default
- approved runtime-facing assets should gradually move toward a cleaner `Production/` structure

## Companion docs

- `ART-002` for the production workflow
- `ART-004` for provenance and source tracking
- `ART-009` for style experimentation lanes
- `DOC-OPS-004` for PixelLab operating guidance
- `DOC-OPS-007` for the broader stage-1 reality audit

