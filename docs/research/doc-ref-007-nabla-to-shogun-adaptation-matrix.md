# DOC-REF-007: NaBlA To Shogun Adaptation Matrix

**Summary:** Repo-safe translation layer for the local `NaBlA_References_Inspo` pack. This note maps every studied top-level NaBlA folder into a Shogun-specific asset lane, world/lore fit, and active-slice priority so the reference pack teaches structure without pulling the project toward Naruto expression.

## Purpose

Use this note when deciding:

- how each NaBlA reference category should translate into Shogun systems and content lanes
- which folders matter now for the active vertical slice versus later roadmap work
- how UI, collection art, battle presentation, and progression materials should reflect Shogun's world pillars and collectible fantasies
- how to keep the local Naruto pack useful as study material without turning it into a visual template

This note is a design-translation layer, not a license grant, and not permission to copy franchise assets or expression.

Related docs:

- [DOC-REF-005](./doc-ref-005-nabla-reference-pack-manifest.md)
- [DOC-REF-006](./doc-ref-006-nabla-folder-by-folder-study.md)
- [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md)
- [DESIGN-002](../design/design-002-world-pillars-and-combat-identity-framework.md)
- [DESIGN-008](../design/design-008-active-vertical-slice-definition.md)
- [DESIGN-009](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md)
- [ART-004](../art/art-004-asset-provenance-and-source-tracking.md)
- [DOC-LEGAL-004](../legal/doc-legal-004-third-party-reference-material-handling.md)

## Translation rule

The working rule for all NaBlA study is:

1. learn the asset lane
2. learn the information hierarchy
3. translate that lane into Shogun world pillars and collectible fantasies
4. build original assets for Shogun

Do not skip from `Naruto source asset` to `Shogun runtime asset`.

## Shogun-side design anchors

When mapping reference categories into Shogun, use these anchors:

### World pillars

From [DESIGN-002](../design/design-002-world-pillars-and-combat-identity-framework.md):

- `Imperial Court`
- `Ronin Marches`
- `Temple and Veil Orders`
- `Yokai Courts`
- `Corrupted Dominion`

### Collectible-fantasy pillars

From [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md):

- `Tragic Nobility`
- `Rogue Violence`
- `Yokai Elegance`
- `Corrupted Power`
- `Mystic Ritual`

### Active-slice bias

From [DESIGN-008](../design/design-008-active-vertical-slice-definition.md) and [DESIGN-009](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md):

- prioritize battle readability over breadth
- prioritize `Dev_Sandbox` proof content over broad meta shells
- prioritize the current playable roster and enemy set over far-future live-service ornament

## Priority legend

- `Critical now`: directly improves the current playable slice or production-facing art proof
- `High soon`: important support lane that should follow once the slice is stable
- `Later`: valid long-term lane but not required for current slice success
- `Study only`: useful reference, but not a production lane to build yet

## All-folder adaptation matrix

| NaBlA folder | Original role in Blazing | Shogun equivalent | Lore and design fit in Shogun | Slice priority | What to copy structurally | What to avoid visually |
| --- | --- | --- | --- | --- | --- | --- |
| `App Icons` | Store and launcher identity | Shogun app icon set for store, launcher, and install surfaces | Represent one clear hero fantasy, likely a flagship face from `Imperial Court`, `Yokai Courts`, or `Corrupted Dominion`, not a whole roster collage | Later | one-face focal hierarchy, bold silhouette, tiny-size readability | Naruto face crops, headband logic, franchise color coding |
| `Artwork_Full` | Premium collector art and rarity appeal | Collection lane art: portrait, banner, premium reveal, future card art | Directly supports [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md): `Tragic Nobility`, `Rogue Violence`, `Yokai Elegance`, `Corrupted Power`, `Mystic Ritual` | High soon | rarity signaling through composition, effects, and pose hierarchy | Naruto rendering language, pose mimicry, aura motifs |
| `Backgrounds_Map` | Battle-stage backdrops | Battlefield backgrounds for `Dev_Sandbox` and later authored encounters | Each stage should express a world pillar: court courtyards, ronin roads, shrine compounds, moonlit spirit gardens, corrupted war grounds | Critical now | simple combat floor plus scenic depth, lower-center readability, portrait-safe framing | recognizably Naruto map locations or direct compositions |
| `Backgrounds_UI` | Menu and result backdrops | UI atmosphere plates for barracks, summon, settings, event menus | Use world-pillar mood without overpowering text: lacquered court interiors, dusty frontier cloth, shrine paper textures, yokai moonlight haze, ash-black corruption surfaces | High soon | low-contrast mood backdrops behind readable panels | Naruto menu paintings, copied decorative texture language |
| `Chakra Icon` | Charge/resource orb set | Charge-ring tokens, ability readiness pips, element or weapon tokens | Best mapped to Shogun charge logic and elemental or weapon readiness, not chakra. Color/state language should reflect Shogun elements and weapon schools | Critical now | repeated small stateful tokens, clear fill-state progression, consistent ring language | chakra icon shapes, Naruto symbols, direct color semantics |
| `Complete Sprites` | Final assembled unit presentations | Final runtime cutout review lane for Shogun units | Useful across all pillars as a validation lane: does the final unit still read as court, ronin, temple, yokai, or corrupted at gameplay scale | High soon | compare assembled runtime read versus raw sheet read | any direct reuse of finished character silhouettes |
| `Cut-Ins` | Combo strips, jutsu cut-ins, premium rare cut-ins | `comboCutInSprite`, future ability cut-ins, `ultimateCutInSprite` | Combo strips should emphasize eyes and intent; ultimates should express the unit's collectible fantasy and pillar identity. Court and yokai lanes can be elegant; ronin and corrupted lanes can be harsher | Critical now | multiple spectacle lanes by moment importance, fast small combo strip, larger premium ultimate lane | Naruto eye-strip styling, exact slant masks, direct cut-in compositions |
| `Evo Materials` | Evolution/awakening material art | Upgrade materials for training, awakening, or ritual ascension | Best tied to world fiction: court seals, ronin contracts, temple talismans, yokai relics, corrupted shards | Later | clear material families, silhouette grouping, rarity-coded progression items | Naruto material icons and emblem shapes |
| `Help Stuff` | Tutorial strips and explanatory labels | Contextual help ribbons, tooltips, onboarding strips | Fits all pillars only as presentation language. In Shogun, help should feel lacquered, parchment-like, or ritual-scripted depending on mode, but remain readable first | High soon | short in-context labels instead of giant help walls | Naruto-specific badge styles or tutorial branding |
| `LB Crystals` | Premium enhancement items | Limit-break or ascension resources | Strong match for late progression. Could become `Soul Ash`, `Court Seals`, `Veil Relics`, or `Demon Blood Crystals` depending on pillar | Later | special material lane separate from normal materials, stronger ceremony for premium upgrades | Naruto crystal art, copied gem treatments, franchise scarcity language |
| `Logo` | Main brand lockup | Shogun logo system and campaign lockups | Must come from Shogun's own world: feudal-dark-fantasy, lacquer, ink, steel, shrine, ash, or moon motifs that belong to the setting | Later | logo hierarchy, startup branding cadence, one dominant mark plus subtitle support | Naruto lockups, franchise typography echoes, leaf or ninja motifs |
| `Misc` | Catch-all support bucket | Shogun overflow reference bucket for edge cases only | No single lore fit. Use only when another lane does not answer the question | Study only | notice uncategorized support lanes that may need a home later | treating catch-all art as a design foundation |
| `Scenes` | Story or event illustrations | Event vignette, node art, story-screen support art | Strong fit for [ART-008](../art/art-008-roguelite-event-vignette-art.md). Scenes should depict court intrigue, shrine rituals, yokai bargains, ronin campfires, and corrupted aftermaths | High soon | story scenes as their own lane, not battle background reuse; portrait-friendly composition | Naruto characters, copied story compositions, direct event paintings |
| `Spritesheets` | Battle animation sheets | Playable and enemy production sprite sheets | Critical for the current slice roster in [DESIGN-009](../design/design-009-first-vertical-slice-roster-and-encounter-plan.md): Ryoma, Kuro, Tsukiko, Ronin Footman, Oni Brute, Yurei Caster and later roster expansion | Critical now | frame economy, silhouette discipline, action readability, effect restraint | Naruto body language, exact action posing, copied frame timing if it becomes imitation |
| `Terrain` | Ground overlays and small battlefield pieces | Battlefield floor accents, hazard decals, lane markers, shrine circles, ash stains | Use to support stage identity by pillar: court floor seams, ronin dirt tracks, temple wards, yokai petals, corrupted sigils | High soon | modular ground accents that aid space without cluttering unit read | Naruto ground effects, copied terrain motifs or symbols |
| `Title Screen` | Startup presentation art | Main-menu hero background and front-door title scene | Should present the Shogun thesis immediately: dark-feudal tension, collectible fantasy, and one strong battlefield or story mood | Later | staged startup composition, logo plus mood plus primary CTA hierarchy | Naruto title scenes, character lineup composition, brand pacing |
| `UI` | Battle HUD and menu atoms | Combat HUD atoms, shared menu chrome, bars, tabs, borders, chips | Fits [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md). Must express Shogun's lacquer-and-metal language instead of anime franchise chrome | Critical now | UI built from many tiny reusable atoms, clear emphasis hierarchy, mobile readability | Naruto corners, ornaments, button frames, exact color roles |
| `UI_Gasha` | Summon-screen UI and reveal FX | Summon banner screen, reveal FX, rarity overlays, result ceremony | Ties directly to `collection lane` value in [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md). In-world tone should feel ceremonial, cursed, or courtly rather than arcade-bright unless a banner theme calls for it | High soon | modular summon spectacle layers, reveal cadence, banner info hierarchy | Naruto summon FX, copied `Super Rare`-style treatment, franchise banner layouts |
| `UI_Text` | Image-based labels and badge words | Impact words, rank labels, combo callouts, reward tags | Works best as a selective emphasis lane for battle and summon moments. Typography should be Shogun-specific and never depend on Naruto branding or lettering rhythm | High soon | text-as-image only for emphasis moments, small reusable labels, hierarchy by size and placement | copied word art, letterforms, ranking badges, franchise text styling |
| `Unit Icons` | Tiny roster thumbnails | Barracks icons, summon result chips, team-slot busts | Directly supports the collection package idea in [DESIGN-001](../design/design-001-character-collection-and-fantasy-strategy.md). Every icon should preserve the character's world pillar and fantasy at very small size | Critical now | bust readability, consistent crop rules, one clear face/silhouette per icon | Naruto bust crops, exact framing, copied background treatments |

## Lore-first translation guidance by folder family

### Character-facing presentation folders

These folders teach how to sell a unit as collectible value:

- `Artwork_Full`
- `Cut-Ins`
- `Unit Icons`
- `App Icons`
- `Title Screen`
- `Logo`

Shogun rule:

- `Imperial Court` units should read polished, lacquered, and formal
- `Ronin Marches` units should read weathered, practical, scarred, and immediate
- `Temple and Veil Orders` units should read ritualized, disciplined, and symbol-heavy
- `Yokai Courts` units should read elegant, moonlit, and dangerous
- `Corrupted Dominion` units should read desecrated, powerful, and unstable

### Runtime battle and readability folders

These folders teach what survives the real playfield:

- `Spritesheets`
- `Complete Sprites`
- `Backgrounds_Map`
- `Terrain`
- `Chakra Icon`
- `UI`

Shogun rule:

- gameplay readability wins over ornament
- pillar identity should survive through silhouette, accent color, weapon family, and motion
- the current slice should prove this first in `Dev_Sandbox`, not in a broad menu shell

### Meta progression and menu-support folders

These folders matter more once the slice is stable:

- `UI_Gasha`
- `Backgrounds_UI`
- `Help Stuff`
- `UI_Text`
- `Evo Materials`
- `LB Crystals`
- `Scenes`

Shogun rule:

- menu, summon, and upgrade lanes should still feel like the same world as combat
- court, ronin, shrine, yokai, and corrupted presentation should influence materials and menus without fragmenting the app into unrelated art styles

## Recommended Shogun build order from the NaBlA pack

### Phase 1: prove the slice

Study hardest:

1. `Spritesheets`
2. `Cut-Ins`
3. `UI`
4. `Unit Icons`
5. `Chakra Icon`
6. `Backgrounds_Map`

Why:

- these directly improve battle readability, combat spectacle, and character identity inside the current slice target from [DESIGN-008](../design/design-008-active-vertical-slice-definition.md)

### Phase 2: strengthen collection and summon value

Study next:

1. `Artwork_Full`
2. `UI_Gasha`
3. `Scenes`
4. `Backgrounds_UI`
5. `UI_Text`

Why:

- these define how Shogun sells and frames characters outside battle without copying Naruto presentation literally

### Phase 3: build progression and polish lanes

Study later:

1. `Evo Materials`
2. `LB Crystals`
3. `Help Stuff`
4. `Terrain`
5. `App Icons`
6. `Title Screen`
7. `Logo`
8. `Misc`

Why:

- these are real product lanes, but not blockers for proving the active combat slice

## Final rule

Every NaBlA folder can be useful, but only if it is translated through Shogun's own fiction and product priorities.

The safe pattern is:

- study Naruto structure locally
- write neutral rules in repo docs
- rebuild those lanes as Shogun originals using Shogun world pillars and collectible fantasies

The unsafe pattern is:

- keep the franchise files in the repo
- use them as direct prompt targets
- let Shogun inherit Naruto's expression instead of only its structural lessons
