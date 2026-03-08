# ART-008: Roguelite Event Vignette Art

**Summary:** `Shogun` should use a dedicated event-vignette art lane for roguelite choice scenes, inspired by `Slay the Spire`-style event presentation. These images should provide atmosphere and decision drama without exploding map-production scope.

## Purpose

Use this note when deciding:

- how roguelite event scenes should look and feel
- when to use AI-assisted art for event choices
- how many unique event images the first prototype actually needs
- how to keep event presentation strong without turning every node into a one-off illustration project

## Core rule

Event vignette art is a **third art lane**.

`Shogun` should have:

- **collection art** for character desire and summon presentation
- **battle art** for playable sprites and combat readability
- **event vignette art** for roguelite decisions, encounters, shrines, bargains, and story beats

This lane should be:

- more atmospheric than battle art
- more reusable than collection art
- cheaper than full map/set-piece production

## What good vignette art should do

A good event vignette should:

- establish the mood in one image
- make the event family instantly recognizable
- support `2-4` meaningful choices
- feel worth reading instead of like a plain text interruption

It should sell:

- danger
- mystery
- temptation
- ritual
- grief
- greed
- corruption
- wonder

## Best event families for the first wave

Strong early categories:

- `Yokai Bargain`
- `Temple Trial`
- `Blood Moon Duel`
- `Cursed Shrine`
- `Wounded Traveler`
- `Forbidden Relic`
- `Execution Ground`
- `Hidden Dojo`

These fit `Shogun`'s world, roguelite structure, and collectible fantasy lanes.

## Production scope rule

Do not create one fully unique painting for every event.

Instead:

- create one strong image per event family
- write multiple event variations around that family
- reuse images when the fantasy frame is the same

Good first-slice target:

- `5-8` event images total
- each tied to a reusable event archetype

## Visual direction

Event vignette art should be:

- more composed and scenic than battle sprites
- darker and more atmospheric than ordinary menu UI
- readable at mobile scale
- emotionally legible before the player reads every line

Preferred traits:

- strong focal subject
- controlled background detail
- strong silhouette or landmark prop
- clear lighting mood
- limited palette accents

Avoid:

- cluttered full scenes with no focal point
- giant panoramic compositions that shrink poorly on phones
- art so dense that the choice UI has nowhere to sit cleanly

## Recommended tool split

Recommended stack:

- `Gemini web chat / Nano Banana Pro`
  - concept composition
  - mood exploration
  - event-scene ideation
- `PixelLab`
  - pixel-style finalization
  - style-matching to the game's sprite language
  - reusable event-scene production
- `Unity`
  - framing, layout, mobile readability, and text/choice validation

Unlike battle characters, event vignettes do not need:

- 4-direction coverage
- attack/hit/walk cycles
- combat timing polish

That makes them a very efficient place to spend AI-assisted art effort.

## Layout rule

Design the event art assuming the final screen also contains:

- event title
- short descriptive text
- `2-4` player choices
- optional status / resource consequences

So the image should leave clean layout room for:

- centered or side-column text
- large touch-friendly choice buttons
- optional reward/cost icons

## Reuse rule

When an image is approved, treat it as an event-family anchor.

Examples:

- one `Yokai Bargain` image can support charm trades, curse cleansing, temptation choices, or sacrifice options
- one `Temple Trial` image can support blessing, discipline, confession, or corruption branches
- one `Blood Moon Duel` image can support duel invitations, revenge encounters, or ambush challenges

## Relationship to the roguelite design

This note supports:

- [`../design/design-004-roguelite-replayability-and-run-mode-framework.md`](../design/design-004-roguelite-replayability-and-run-mode-framework.md)

Use `DESIGN-004` for:

- node types
- run structure
- event role in progression

Use this note for:

- how the event scenes should actually be visualized and produced

## First prototype recommendation

For the first roguelite prototype:

- do not block implementation on a huge art library
- ship a small vignette set first
- validate whether the art makes event choices feel more dramatic and memorable

Suggested first pack:

- `1` shrine scene
- `1` yokai bargain scene
- `1` ronin duel scene
- `1` relic/found-object scene
- `1` corruption scene

If those work, expand by family rather than chasing one-off events.
