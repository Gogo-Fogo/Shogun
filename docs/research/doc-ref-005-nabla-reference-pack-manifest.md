# DOC-REF-005: NaBlA Reference Pack Manifest And Safe Usage Guide

**Summary:** Repo-safe manifest for the local-only `NaBlA_References_Inspo` folder. Records the reference categories, rough scale, what each category is useful for, and what Shogun should learn structurally without redistributing or copying Naruto franchise assets.

## Purpose

Use this note when:

- studying the local `Naruto Blazing` reference pack for UI, content, and presentation patterns
- deciding which categories are worth mining for Shogun design rules
- turning raw reference material into repo-safe conclusions
- keeping the reference workflow useful without treating the copyrighted source pack as normal project content

This is a **manifest and interpretation layer**, not a license grant and not a justification for redistribution.

## Local-only scope

This manifest refers to the local reference folder:

`docs/NaBlA_References_Inspo/NarAssets`

The raw contents remain **local-only** and should not be committed or redistributed.
For the handling rule, see:

- [DOC-LEGAL-004](../legal/doc-legal-004-third-party-reference-material-handling.md)
- [ART-004](../art/art-004-asset-provenance-and-source-tracking.md)
- [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md)

## Pack scale snapshot

Observed on March 11, 2026:

- total files: `10,508`
- total size: `2426.03 MB`
- top-level content root: `NarAssets`
- top-level category count: `23`

This is large enough that it should be treated as a private study archive, not an ordinary repo folder.

## Category manifest

| Category | Files | Size (MB) | Primary use for Shogun | Copy-risk level |
|---|---:|---:|---|---|
| `Artwork_Full` | 1033 | 1056.50 | Study rarity presentation, character-facing composition, collection-value signaling | Very high |
| `Spritesheets` | 2130 | 541.60 | Study sprite scale, frame economy, combat readability, animation density | Medium |
| `Backgrounds_Map` | 207 | 288.12 | Study battle-stage framing, depth cues, and mobile readability of map art | High |
| `Cut-Ins` | 2173 | 195.55 | Study combo strips, jutsu cut-ins, premium cut-ins, and presentation taxonomy | High |
| `Scenes` | 1085 | 90.95 | Study story/event scene composition and flow pacing | High |
| `LB Crystals` | 247 | 74.11 | Study upgrade-material icon families and progression-material language | Medium |
| `UI` | 113 | 34.78 | Study HUD hierarchy, frame density, and chrome vocabulary | Very high |
| `Help Stuff` | 132 | 27.98 | Study onboarding/help panel structure and tutorial communication | Medium |
| `Unit Icons` | 1361 | 23.59 | Study roster thumbnail readability and silhouette preservation at tiny size | High |
| `Chakra Icon` | 1351 | 21.34 | Study segmented charge orb language and icon-state consistency | Medium |
| `Evo Materials` | 98 | 18.58 | Study evolution/awakening material taxonomy | Medium |
| `Backgrounds_UI` | 17 | 16.22 | Study menu/backdrop atmosphere and panel contrast | High |
| `Misc` | 87 | 12.75 | Catch-all; inspect only when a question is not answered elsewhere | Mixed |
| `UI_Gasha` | 22 | 10.84 | Study summon banner hierarchy, rate communication, and reward reveal framing | High |
| `Complete Sprites` | 120 | 6.81 | Study assembled sprite presentation and final runtime proportions | Medium |
| `UI_Text` | 182 | 2.27 | Study number labels, badge text, and font treatment | High |
| `Title Screen` | 10 | 2.06 | Study front-door composition and startup presentation beats | High |
| `Logo` | 5 | 0.75 | Study logo hierarchy only; do not imitate literal expression | Very high |
| `App Icons` | 12 | 0.68 | Study store-icon hierarchy and focal composition only | Very high |
| `Terrain` | 123 | 0.54 | Study battlefield ground markings and spatial readability | Low to medium |

## What each category can safely teach us

### `Cut-Ins`

Best use:

- identify asset taxonomy such as `chain`, `cutin`, `rarecutin`, and `sevencutin`
- record source dimensions and crop patterns
- decide which lanes Shogun actually needs

Safe repo outputs:

- dimensions
- counts
- naming conventions
- a design note like [ART-012](../art/art-012-combat-cutin-taxonomy-and-shogun-mapping.md)

Do not ship or commit the raw cut-ins.

### `UI` and `UI_Gasha`

Best use:

- analyze hierarchy, spacing, emphasis, and what the player notices first
- extract structural rules for battle HUD, summon banners, menu segmentation, and reward messaging

Safe repo outputs:

- panel hierarchy notes
- spacing and alignment notes
- your own redrawn wireframes
- color-role tables and interaction rules

Do not copy literal frames, corners, icons, or ornamental identity.

### `Spritesheets` and `Complete Sprites`

Best use:

- study scale, silhouette, frame count, and effects density
- compare how much animation spectacle lives in sprites versus cut-ins versus UI

Safe repo outputs:

- frame-budget notes
- per-action timing notes
- sprite-size guidance
- comments on silhouette readability

### `Artwork_Full`

Best use:

- understand what rarity and premium character value look like in a gacha collector
- study how full art supports summon motivation and character identity

This is high-risk reference material because it is close to the core franchise expression.
Use it to understand composition and monetization psychology, not to derive direct visual prompts.

### `Chakra Icon`, `LB Crystals`, `Evo Materials`

Best use:

- understand progression-material families
- study how repeated small icon systems stay readable and collectible
- identify what information is encoded in shape vs color vs border treatment

Safe repo outputs:

- icon taxonomy tables
- progression-lane notes
- recommendations for Shogun weapon/element/charge icon systems

### `Scenes`, `Backgrounds_Map`, `Backgrounds_UI`, `Terrain`

Best use:

- study portrait composition and depth layering
- understand how much background detail survives mobile play
- see how gameplay surfaces, story scenes, and menu surfaces differ in visual density

Safe repo outputs:

- layout notes
- painterly-vs-gameplay-surface comparisons
- background density rules

### `Title Screen`, `Logo`, `App Icons`

These are the least safe categories to imitate directly.

Best use:

- identify focal hierarchy
- note how little information a store icon can carry
- study startup branding flow

Do not derive literal logos, title-lockup styles, or icon motifs from these assets.

## Practical study workflow

Use the pack like this:

1. Inspect a category locally.
2. Write down neutral rules.
3. Convert the rules into Shogun-specific docs, wireframes, or production specs.
4. Build original assets from those specs.

Never skip from `franchise asset` directly to `production asset`.

## Repo-safe output types

Good things to commit after studying the pack:

- manifests
- counts and dimensions
- taxonomy docs
- timing notes
- original mockups
- wireframes
- neutral comparison tables
- your own implementation rules

Bad things to commit:

- raw franchise PNGs
- extracted icons
- raw card art
- UI chrome dumps
- direct paint-overs
- "temporary" borrowed assets left in source folders

## What Shogun should actually copy

Copy the **system logic** and **information hierarchy**:

- how many presentation lanes exist
- which moments deserve spectacle
- how tiny icons stay legible
- how summon UI communicates value
- how portrait playfields retain readability

Do **not** copy the franchise expression:

- Naruto characters
- leaf symbols
- exact frame motifs
- logos
- specific card layouts
- distinctive icon designs
- color palettes that only read as Naruto branding

## Current best use for this pack in Shogun

Highest-value near-term study targets:

1. `Cut-Ins`
2. `UI`
3. `UI_Gasha`
4. `Chakra Icon`
5. `Spritesheets`

Those categories teach the most about:

- battle spectacle
- HUD structure
- summon presentation
- charge-ring language
- sprite/readability balance

## Relationship to existing docs

This manifest complements, but does not replace:

- [DOC-REF-001](./doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md)
- [DOC-REF-002](./doc-ref-002-naruto-ninja-blazing-success-failure-and-shutdown-analysis.md)
- [ART-012](../art/art-012-combat-cutin-taxonomy-and-shogun-mapping.md)
- [DOC-LEGAL-004](../legal/doc-legal-004-third-party-reference-material-handling.md)

Use this file when the question is:

- "what is in the local Naruto pack"
- "which category should I inspect for this design problem"
- "what can I safely extract as a repo-safe conclusion"
