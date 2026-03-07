# ART-001: Style Bible and Visual Targets

**Summary:** Baseline visual rules for `Shogun` character sprites so generated and hand-edited assets stay readable, cohesive, and production-friendly.

## Purpose

Use this note when deciding:

- what `Shogun` sprites should feel like visually
- whether a generated sprite fits the game
- how much detail is too much for gameplay-facing art
- what should be corrected during cleanup

## Visual target

`Shogun` should read as:

- stylized dark-fantasy feudal Japan
- readable and bold at gameplay scale
- expressive through silhouette and color blocking first, detail second
- dramatic, but not muddy or over-rendered

The goal is not maximal detail. The goal is strong readability on a mobile screen.

## Core style rules

### Silhouette first

- each class or faction should be identifiable from outline alone
- weapons, hair, hats, banners, horns, and shoulder shapes should carry identity
- avoid flat silhouettes where every humanoid unit becomes the same blocky shape

### Controlled detail

- prioritize large readable forms over tiny ornamentation
- avoid excessive belts, cords, buckles, or micro-highlights in gameplay sprites
- if a detail disappears at gameplay scale, it is not carrying its weight

### Consistent camera/view

- keep one stable gameplay-facing character angle across the core cast
- do not mix radically different perspective assumptions between units
- diagonal or near-3/4 presentation is fine only if it stays consistent across the whole battle roster

### Palette discipline

- each unit should have a tight palette
- faction/element differences should read through controlled hue grouping, not noisy rainbow accents
- value contrast must support readability against combat backgrounds

### Shading discipline

- keep shading simple and deliberate
- prefer readable plane changes over soft noisy rendering
- do not let AI-generated texture noise masquerade as craftsmanship

## Character readability rules

At gameplay scale, a character should still clearly communicate:

- class or role
- weapon family
- facing direction
- current action
- rarity or importance, if relevant

If those fail, the sprite is not ready regardless of how impressive it looks zoomed in.

## Size policy

Default gameplay-facing standard:

- standard characters: `64x64`

Escalate only when the asset type truly needs it:

- large bosses or showcase battle entities: consider `128x128`
- portraits, splash art, or cut-in presentation art: separate larger-format pipeline

Do not enlarge standard battle sprites just because the tool allows higher resolutions.

## Animation feel

The target is:

- readable combat intent
- strong poses
- clear anticipation and follow-through
- consistent body volume between frames

The target is not:

- maximum raw frame count
- hyper-fluid motion that becomes muddy on a phone

## Cleanup priorities

When refining generated output, fix in this order:

1. silhouette
2. proportions
3. facing/readability
4. palette cleanup
5. frame-to-frame consistency
6. secondary detail polish

## Rejection signs

Reject or rework a sprite if:

- the silhouette collapses at gameplay scale
- the pose reads only when zoomed in
- directionality is ambiguous
- too much of the visual appeal depends on noisy surface detail
- the character looks like it belongs to a different art style than the rest of the roster

## Decision rule

When unsure between “more detailed” and “more readable,” choose “more readable.”
