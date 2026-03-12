# DOC-REF-006: NaBlA Folder-By-Folder Study

**Summary:** Category-by-category study of the local `Naruto Blazing` reference pack. This note records what each folder appears to contain, what role it served in the original game, what Shogun can safely learn from it, and what should not be copied literally.

## Purpose

Use this note when the question is:

- which local Naruto reference folder should be studied for a specific Shogun problem
- what each folder actually contains in practice, not just by name
- how to translate those categories into Shogun UI, content, or presentation decisions

This note is based on local sampling of the folder only. It is a **study note**, not a license, and not a justification for redistributing the source assets.

Related docs:

- [DOC-REF-005](./doc-ref-005-nabla-reference-pack-manifest.md)
- [DOC-LEGAL-004](../legal/doc-legal-004-third-party-reference-material-handling.md)
- [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md)
- [ART-012](../art/art-012-combat-cutin-taxonomy-and-shogun-mapping.md)

## Method

This study used:

- folder counts and size totals
- sample filenames
- representative image dimensions
- direct visual inspection of representative assets

The goal was to identify:

- what gameplay or UX job each folder served
- what structural lesson it teaches
- what Shogun should keep as a system pattern versus avoid as literal franchise expression

## Folder-by-folder study

### `App Icons`

Observed pattern:

- tiny set of mobile/store icons
- sample sizes: `170x170`, `192x192`, `130x130`
- sample visual: character face crop inside a rounded app icon

What it appears to do:

- storefront identity
- launcher icon variants
- platform-facing brand recognition

What Shogun should learn:

- app icons need one dominant face/focal shape
- tiny-size readability matters more than detail
- a store icon should communicate one clear hero fantasy, not a full roster story

What not to copy:

- franchise face crops
- Naruto-specific color and headband symbolism
- literal icon framing approach

### `Artwork_Full`

Observed pattern:

- largest content bucket by storage
- sample sizes around `839x651`, `865x621`, `688x563`
- representative art is premium character illustration with effects and composition polish

What it appears to do:

- collector-facing character art
- rarity/value signaling
- summon motivation and roster desirability

What Shogun should learn:

- premium character art must sell identity at a glance
- rarity presentation is mostly composition, effects, and silhouette emphasis
- the collector layer needs stronger finish than runtime battle art

What not to copy:

- Naruto-specific poses, aura motifs, or character rendering language
- direct paintover/reference prompting from these images

### `Backgrounds_Map`

Observed pattern:

- sample size: `1024x1024`
- representative image is a painted battle stage background
- map composition keeps open foreground combat space and rich background storytelling

What it appears to do:

- battle backdrop art
- stage identity and location fantasy
- gameplay-surface framing

What Shogun should learn:

- keep the combat floor visually simpler than the scenic background
- reserve the central lower area for unit readability
- background storytelling should stay behind gameplay clarity

What not to copy:

- specific village/series landmarks
- direct map compositions that read as Naruto world locations

### `Backgrounds_UI`

Observed pattern:

- sample sizes: `1000x1332`, `768x1024`
- representative image is a decorative menu/event backdrop texture

What it appears to do:

- menu atmosphere
- result/panel backdrops
- non-battle UI surfaces

What Shogun should learn:

- UI backdrops can be painterly and thematic without competing with controls
- background art should support panel contrast, not flatten it
- a menu background can carry mood while the interaction layer stays clean

### `Chakra Icon`

Observed pattern:

- large repeated set
- sample size: `128x128`
- representative visual is a character-specific circular orb icon

What it appears to do:

- turn-charge or ability-resource indicators
- segmented orb language around character medallions
- per-unit progression-to-skill readability

What Shogun should learn:

- small repeated icons must read instantly
- a charge system benefits from simple state changes more than ornate detail
- repeated UI tokens should feel systematic and collectible at the same time

Shogun mapping:

- segmented charge-ring or orb logic around medallions
- weapon/element/ability readiness tokens

### `Complete Sprites`

Observed pattern:

- assembled character renders or finalized sprite presentations
- sample sizes vary: `113x173`, `75x210`, `331x467`
- representative sample reads like a finished character cutout, not a sheet

What it appears to do:

- final character display assets
- presentation previews or collection-side sprite use

What Shogun should learn:

- it is useful to study final assembled readability separately from sheets
- finished sprite presentation can reveal scale issues hidden by sheet-only inspection

### `Cut-Ins`

Observed pattern:

- one of the most useful folders for Shogun
- includes `chain`, `cutin`, `rarecutin`, and `sevencutin`
- sample sizes: `256x64`, `452x477`, `377x493`, `512x768`

What it appears to do:

- combo eye-strip flashes
- standard ability/jutsu presentation
- premium rare/ultimate presentation
- occasional full premium splash source art

What Shogun should learn:

- not every combat spectacle moment needs the same art lane
- combo strips are tiny and fast
- premium ultimate presentation deserves a larger, more dramatic lane

Shogun mapping:

- `comboCutInSprite`
- `ultimateCutInSprite`
- regular ability fallback currently stays on `bannerSprite`

See [ART-012](../art/art-012-combat-cutin-taxonomy-and-shogun-mapping.md).

### `Evo Materials`

Observed pattern:

- medium-size collectible item art
- sample sizes around `261x337`, `370x345`, `302x337`
- representative sample reads like a stylized upgrade item

What it appears to do:

- evolution/awakening material inventory
- progression-gate item art

What Shogun should learn:

- progression materials need consistent visual language
- materials should sort into recognizable families by silhouette and border treatment
- these icons are part of progression UX, not just inventory decoration

### `Help Stuff`

Observed pattern:

- many thin strip assets and explanatory UI fragments
- sample size: `564x36`
- representative sample is a thin label/banner describing awakening conditions

What it appears to do:

- help overlays
- tutorial callouts
- character-detail explanation labels

What Shogun should learn:

- onboarding often depends on many small explanatory strips, not one giant tutorial screen
- progression help should be terse and localized into the right screen
- labels need to be readable at very small heights

### `LB Crystals`

Observed pattern:

- premium upgrade-material art with flashy framing
- sample sizes: `351x457`, `457x457`
- representative sample reads like a high-value progression item or shard/crystal

What it appears to do:

- limit break or equivalent upgrade-resource presentation
- high-value enhancement material lane

What Shogun should learn:

- special upgrade materials need stronger visual ceremony than normal materials
- progression rarity can be communicated through framing and glow, not just color

### `Logo`

Observed pattern:

- tiny folder
- sample size: `536x312`
- representative visual is the full Naruto game logo lockup

What it appears to do:

- title branding
- splash/loading/store presentation

What Shogun should learn:

- title marks need hierarchy and silhouette even before the text is readable
- sub-franchise text and primary mark are often layered for brand clarity

What not to copy:

- literal logo framing
- brush shape language that reads as Naruto branding

### `Misc`

Observed pattern:

- mixed collectible/presentation items
- sample sizes around `399x302`, `407x320`, `407x336`
- representative sample includes novelty or thematic item art like food

What it appears to do:

- overflow bucket for assets that do not fit the main family folders
- collectible or event-side support assets

What Shogun should learn:

- a long-running game accumulates many weird support items
- content taxonomy matters because "misc" grows fast and becomes design debt

### `Scenes`

Observed pattern:

- many square event illustrations
- sample size: `512x512`
- representative image is a character-in-environment event/story panel

What it appears to do:

- story nodes
- event-dialogue imagery
- side-scene presentation

What Shogun should learn:

- story/event presentation can live in a compact square lane
- not every narrative beat needs full-screen splash art
- character + environment composition is enough to sell a scene if the crop is strong

Shogun mapping:

- event scenes
- barracks/story snippets
- support-node presentation

### `Spritesheets`

Observed pattern:

- one of the largest useful technical buckets
- sample size: `1024x1024`
- representative filenames are `_unit` sheets

What it appears to do:

- runtime battle animation sheets
- character combat sprite production source

What Shogun should learn:

- sheet size and frame packing are deliberate production constraints
- spectacle is distributed across sprites, cut-ins, and UI instead of living entirely in one lane
- sprite sheets are where long-term content burden really shows up

### `Terrain`

Observed pattern:

- small environmental decal/tile fragments
- sample sizes: `512x256`, `256x256`, `128x128`
- representative sample shows tileable foliage/ground surface chunks

What it appears to do:

- battlefield overlays
- terrain decals
- environmental patchwork or effects surfaces

What Shogun should learn:

- combat spaces benefit from modular ground accents
- tiny terrain pieces can add richness without repainting whole maps

### `Title Screen`

Observed pattern:

- very small bucket
- sample size: `1000x1700`
- representative art is a tall key visual for startup/front-door presentation

What it appears to do:

- title-screen hero illustration
- app-launch presentation

What Shogun should learn:

- front-door key art should read vertically in portrait
- title screen art has different needs from summon art or battle art

### `UI`

Observed pattern:

- mixed bucket of HUD pieces, bars, badges, orbs, and small chrome
- sample sizes range from `25x25` and `32x32` to thin bars like `40x165`
- representative samples include charge pips and opponent HP bar assets

What it appears to do:

- battle HUD atoms
- reusable meter segments
- status and panel pieces

What Shogun should learn:

- the battle HUD is built from many tiny reusable atoms, not only big frames
- repeated small assets are where readability and consistency are won or lost
- tiny HUD pieces deserve their own organized asset lane

### `UI_Gasha`

Observed pattern:

- summon-specific presentation bucket
- sample size: `1024x1024`
- representative sample is a sheet of flare/rarity/reveal effects with text like `Super Rare`

What it appears to do:

- summon reveal ceremony
- rarity callouts and impact FX
- gacha-specific spectacle assets

What Shogun should learn:

- summon UX has its own asset language separate from battle HUD
- rarity reveal relies on layered FX, text, and timing
- gacha spectacle can be modular instead of one giant video sequence

Shogun mapping:

- summon reveal effects
- rarity/burst overlays
- banner-result presentation

### `UI_Text`

Observed pattern:

- many tiny isolated words, numerals, and labels
- sample sizes like `64x45`, `76x51`
- representative sample reads like a small badge label such as `1st`

What it appears to do:

- reusable combat labels
- rank/order markers
- small callout text tokens

What Shogun should learn:

- text-as-image is sometimes used for emphasis and styling, not just typography
- repeated tiny labels are part of the visual identity layer
- only use image text when typography alone cannot sell the moment

### `Unit Icons`

Observed pattern:

- huge repeated set
- sample size: `128x128`
- representative visual is a tight square/bust character icon

What it appears to do:

- roster thumbnails
- party slots
- list/grid collection views

What Shogun should learn:

- a unit icon must survive at very small size without losing identity
- face crop, pose angle, and silhouette discipline matter more than background complexity

Shogun mapping:

- barracks cards
- summon results
- team-slot previews

## Most valuable folders for current Shogun work

For the current slice and support-scene work, the best folders to keep studying are:

1. `Cut-Ins`
2. `UI`
3. `UI_Gasha`
4. `Chakra Icon`
5. `Unit Icons`
6. `Scenes`
7. `Spritesheets`

These are the folders most likely to improve:

- combat spectacle
- HUD structure
- summon flow
- charge-ring readability
- barracks/list presentation
- event/node visuals
- long-term content planning

## Final rule

Study every folder if useful, but only commit the conclusions.

The safe output pattern is:

- local asset study
- repo-safe notes
- original Shogun implementation

Not:

- local asset study
- raw franchise files in GitHub
- direct visual imitation in shipped assets
