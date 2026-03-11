# ART-003: Unity 2D Import and Animation Standards

**Summary:** Standards for bringing character sprite sources into Unity so animation import stays consistent, reviewable, and scalable.

## Purpose

Use this note when deciding:

- how edited sprite sources should enter Unity
- what source format should be preferred
- how animations should be named and tagged
- what counts as acceptable gameplay-facing frame budgets

## Preferred source format

Preferred source asset:

- `.aseprite`

Reason:

- supports frame tags cleanly
- fits the recommended `PixelLab + Aseprite + Unity` workflow
- keeps animation source editable instead of flattening everything into exported PNG sheets too early

## Preferred Unity import path

Preferred importer:

- Unity `2D Aseprite Importer`

Use the importer to preserve:

- frame structure
- tag-based animation groups
- per-frame timing where needed

Do not flatten to manually sliced spritesheets unless the importer path fails for a specific asset.

## Naming standards

Use concise, predictable names.

Character source file:

- `[CharacterName].aseprite`

Animation tags inside Aseprite:

- `idle`
- `walk`
- `hit`
- `attack`
- `skill`
- `death`

Avoid:

- mixed casing inside tags
- spaces in tag names
- overly specific one-off names for common actions

## Baseline gameplay standards

Default character sprite size:

- `64x64`

Default facing scope for first-phase production:

- `4 directions`

Recommended starting frame budgets:

- `idle`: `4` to `6`
- `walk`: `6` to `8`
- `hit`: `3` to `5`
- `basic attack`: `6` to `10`

These are workflow targets, not hard laws. Use the fewest frames that still read clearly.

## Review rules inside Unity

Every imported character should be checked for:

- readability on the real battle background
- consistency of body volume across frames
- clear directional facing
- no muddy weapon trails or unreadable limbs
- no timing that feels too fast or too floaty on device-scale playback
- no promoted production sprite sheet left orphaned without at least one referencing animation clip

## Animation quality rules

Prefer:

- clear key poses
- stable contact frames
- deliberate holds where they improve readability

Avoid:

- filling an animation with extra frames just because the generator produced them
- micro-motion that disappears on a phone screen
- inconsistent frame timing across similar units without good reason

## Asset escalation rule

Escalate above the default standard only when the asset category needs it:

- `128x128` can be justified for large bosses or showcase battle entities
- larger portrait or cut-in assets should live in a separate presentation-art pipeline

Do not let rare exceptions quietly become the default standard for all characters.

