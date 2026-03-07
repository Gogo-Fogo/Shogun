# ART-002: Sprite Production Pipeline

**Summary:** Production workflow for turning AI-assisted generation into usable `Shogun` character sprites without overcommitting too early.

## Purpose

Use this note when deciding:

- how to trial PixelLab for real production work
- what the first bounded character test should be
- what order generation, cleanup, and Unity review should happen in
- when to stop iterating and reject a workflow

## Current recommended stack

- `Gemini web chat / Nano Banana Pro`: concept art, portraits, and presentation-facing exploration
- `PixelLab`: playable pixel-character generation and concept-to-sprite conversion
- `Aseprite`: primary cleanup and animation editing
- `Unity`: gameplay-scale validation
- `Codex` / `Claude`: planning, scripting, documentation, and optional later MCP-driven assistance

## First trial scope

Do not start with a full production pack.

Start with:

- one humanoid character
- `64x64`
- `4 directions`
- `idle`
- `walk`

Optional only if the first two pass:

- `hit react`

Do not start with:

- `8 directions`
- large bosses
- cut-in art
- full move sets
- rarest or most important hero character

## Production phases

### Phase 1: lock the brief

Before generation, define:

- role or class
- collectible fantasy
- weapon type
- silhouette priorities
- palette direction
- emotional tone

### Phase 2: generate concept candidate

Use Gemini to generate:

- one approved portrait, bust, or full-body concept
- a clear collectible-facing identity
- a version that is strong enough to freeze as the reference target for sprite work

Do not skip this step and go straight to sprite generation unless the concept is already fully locked elsewhere.

### Phase 3: convert concept into playable sprite

Use PixelLab to generate:

- one playable character candidate based on the approved concept reference
- consistent directional presentation
- no attempt yet to cover the whole roster or variant set

### Phase 4: generate bounded animations

Generate only:

- `idle`
- `walk`

The goal is to prove:

- direction consistency
- pose readability
- cleanup burden

### Phase 5: cleanup in Aseprite

Fix:

- silhouette drift
- hand/weapon readability
- unstable face or hair shapes
- palette noise
- frame timing and action clarity

### Phase 6: Unity validation

Import the asset into Unity and evaluate:

- readability at gameplay scale
- readability over actual backgrounds
- animation clarity in motion
- whether the sprite still feels cohesive next to current project assets

## Pass/fail rule for PixelLab

Pass the workflow only if one character can be brought to usable quality without absurd cleanup cost.

Suggested practical threshold:

- if cleanup feels like controlled polishing, continue
- if cleanup feels like redrawing most frames, stop and reassess before scaling

## Recommended production order after the first trial

If the trial passes, scale in this order:

1. more basic humanoid units
2. one attack animation standard
3. one damage/hit standard
4. enemy variants
5. elite or hero characters

Do not scale to the full roster before the import and cleanup workflow feels repeatable.

## Tooling posture

Recommended now:

- subscription first
- Gemini concept workflow first
- browser/editor workflow first
- manual review first

Recommended later:

- PixelLab MCP only after art quality is proven
- API only after manual production becomes the real bottleneck

## Gemini usage boundary

Use Gemini as a developer-side concept tool only.

Do:

- generate portraits, busts, and concept art during development
- use it to lock collectible fantasy and presentation direction

Do not:

- build the shipped game around live Gemini API calls
- upload sensitive internal material or anything you would not want reviewed
- treat raw Gemini output as production-final hero content without manual refinement

## File organization rule

Keep source and output distinct:

- generated/raw candidates should not overwrite refined production files
- retain edited source files for shipped assets
- keep provenance metadata with production-facing assets

## Stop conditions

Pause and reassess the workflow if:

- the generated style drifts too much between characters
- the cleanup burden is consistently too high
- the results still look muddy in Unity after cleanup
- the process feels slower than a more conventional pipeline
