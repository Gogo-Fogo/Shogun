# DESIGN-006: Mobile Platform, Display, and Performance Strategy

**Summary:** `Shogun` should remain a mobile-first game built primarily for `iOS` and `Android`, with tablets and foldables treated as adaptive large-screen targets rather than separate product lines. The baseline promise should be stable `60 fps`, with `120 fps` offered as an optional premium mode on supported devices. Do not optimize around `4K` mobile rendering. Keep gameplay primarily `2D`, and use selective pseudo-`2.5D` presentation only where it adds atmosphere without multiplying production cost.

## Purpose

Use this note when deciding:

- whether `Shogun` should stay mobile-first or broaden to PC now
- how tablets and foldables should be handled
- what frame-rate strategy is realistic in 2026 mobile hardware conditions
- whether `4K` is a worthwhile target
- whether `Shogun` should stay primarily `2D` or lean further into `2.5D`
- what graphics settings the game should expose
- what current Unity project settings should eventually change to support this strategy

## Core conclusion

`Shogun` should ship from a **mobile-first, portrait-first, adaptive-screen** strategy.

Recommended posture:

- build for `iOS` and `Android` first
- support tablets and foldables through adaptive UI and safe-area-aware layouts
- keep `PC` as a later expansion path if the mobile product proves traction
- promise `60 fps` first
- expose `120 fps` as an optional mode for capable devices
- do not target `4K` mobile rendering as a primary production goal
- keep core gameplay `2D`
- use selective pseudo-`2.5D` only for presentation, depth, and atmosphere

## Why this is the best fit for Shogun

`Shogun` already aligns much more naturally with mobile than with PC:

- portrait-first UI
- touch-first tactical interactions
- short-session gacha loops
- event-driven content cadence
- collection-first character fantasy

Modern phones are strong enough to support:

- sharp UI
- high-quality VFX
- tasteful lighting
- smooth UI transitions
- optional high refresh

But they are still constrained by:

- battery
- thermals
- long-session stability
- memory pressure
- wide device variance

That means `Shogun` should optimize for:

- stable frametimes
- readable art
- controlled VFX richness
- adaptive layouts

not for brute-force “max graphics” thinking.

## Platform strategy

### Recommended now

- `iOS`
- `Android`

### Recommended later, if mobile traction is real

- `PC`

Do not let possible future `PC` or hypothetical Apple foldable support distort the first shipping product.

Instead:

- make the mobile UI and render model adaptable
- keep simulation and content architecture portable
- only invest in a true PC UX pass if the mobile game earns it

## Tablets and foldables

Treat tablets and foldables as **adaptive large screens**, not as separate SKUs.

That means:

- no hard-coded phone-only assumptions
- no reliance on one aspect ratio
- safe-area-aware UI everywhere
- larger devices get better use of space, not a completely different game

### Good v1 target

- portrait phone support
- portrait tablet support
- adaptive handling of foldable inner displays and unusual aspect ratios
- graceful behavior when the available window changes shape or size

### Not required for v1

- bespoke hinge-aware gameplay
- tabletop-specific foldable modes
- dual-pane combat flows
- dedicated desktop-style tablet UX

Those can be later optimizations if the game proves it has an audience on large screens.

## Apple foldables and iPad

Do not plan around Apple foldable rumors.

Plan around what Apple officially supports now:

- iPhone
- iPad
- adaptive layouts
- dynamic windowing and multitasking constraints on larger screens

If `Shogun` supports iPad and flexible layout behavior well, that is the most sensible hedge against any future Apple large-screen form factor.

## Current project audit

The Unity project already has several good foundations:

- [`Assets/_Project/Scripts/Core/Utilities/SafeAreaHandler.cs`](../../Assets/_Project/Scripts/Core/Utilities/SafeAreaHandler.cs)
  - safe-area-aware UI behavior already exists
- [`Assets/_Project/Prefabs/System/SharedSceneRoot.prefab`](../../Assets/_Project/Prefabs/System/SharedSceneRoot.prefab)
  - shared UI root is portrait-first with `1080 x 1920` reference resolution and a safe-area panel
- [`ProjectSettings/ProjectSettings.asset`](../../ProjectSettings/ProjectSettings.asset)
  - `androidUseSwappy: 1` is already enabled

The main strategic mismatches are:

- [`Assets/_Project/Scripts/Core/Utilities/AppFrameRate.cs`](../../Assets/_Project/Scripts/Core/Utilities/AppFrameRate.cs)
  - currently hard-forces `120 fps`
- [`ProjectSettings/ProjectSettings.asset`](../../ProjectSettings/ProjectSettings.asset)
  - `appleEnableProMotion: 0`
  - `uIRequiresFullScreen: 1`
  - aspect-ratio policy is still phone-leaning rather than explicitly large-screen-oriented
- [`Assets/_Project/Features/UI/Prefabs/DragInputPanel.prefab`](../../Assets/_Project/Features/UI/Prefabs/DragInputPanel.prefab)
  - uses a landscape-oriented `1920 x 1080` reference resolution that should be reviewed if this panel is expected to behave consistently inside a portrait-first product

These do not need to be changed immediately, but they should be treated as follow-up items if this strategy becomes canonical.

## Frame-rate policy

### Recommended default promise

- `60 fps`

That should be the baseline quality bar for gameplay feel.

### Recommended optional modes

- `Battery Saver`: `30 fps`
- `Balanced`: `60 fps`
- `High Refresh`: `120 fps` when supported

This is the best balance between ambition and device reality.

### Why 120 fps still makes sense

`120 fps` is absolutely worth supporting as an option because it improves:

- touch feel
- UI transitions
- scrolling
- camera motion
- particle motion
- general polish

### What 120 fps does not require

It does **not** require:

- 120-frame sprite animations
- doubling every character animation frame count
- reauthoring all sprite motion at ultra-high frame counts

Sprite animations can still be authored at low frame counts like:

- `8 fps`
- `12 fps`
- `16 fps`

while the overall game renders at `60` or `120`.

The important rule is:

- keep **render refresh** separate from **authored animation cadence**

## Resolution policy

Do **not** optimize around `4K` mobile rendering.

That is the wrong goal for this product.

### Recommended resolution approach

- use a strong mobile UI reference resolution
- preserve clarity on modern high-density screens
- use scaling and adaptation instead of chasing `4K`
- favor stable frametimes over brute-force native rendering

### Where 4K can matter later

- `PC`
- external displays
- maybe high-end tablets as a byproduct of good scaling

But `4K` should not become the core production target for mobile art, UI, or rendering budgets.

## Graphics settings policy

Yes, `Shogun` should expose graphics settings.

This is especially important because pixel art can look significantly better when paired with:

- disciplined bloom
- clean particles
- tasteful lighting
- parallax
- distortion and hit FX

### Recommended setting groups

#### Frame rate

- `30 / 60 / 120`

#### Quality tier

- `Low`
- `Medium`
- `High`

#### Effects controls

- VFX density
- bloom on/off or intensity presets
- distortion and screen-space flashes
- screen shake
- hit-stop intensity
- background ambience intensity

#### Battery/thermal posture

- battery saver toggle
- reduced effects toggle

#### Accessibility and comfort

- reduce flash intensity
- reduce camera shake
- reduced post-processing mode

### Important design rule

Do not let “graphics settings” become a giant PC-style matrix.

For a mobile game, a smaller number of high-signal switches is better.

## 2D vs 2.5D

### Recommended now

Keep `Shogun` primarily `2D`.

This is the right choice for:

- clarity
- scope control
- asset consistency
- production speed
- PixelLab and sprite pipeline fit

### Recommended enhancement

Use selective pseudo-`2.5D` or presentation-layer depth in:

- parallax backgrounds
- event vignette scenes
- boss entrances
- shrine or roguelite event presentation
- layered VFX
- camera drift and environmental motion

### Not recommended now

Do **not** pivot the core gameplay into a fully `2.5D` production model.

That would raise cost in:

- animation
- camera
- environment construction
- technical art
- content consistency

without solving one of the project’s current central problems.

So the right line is:

- **2D gameplay**
- **2.5D flavor**

## UI and canvas strategy

The current shared UI root is broadly correct:

- portrait-first
- `1080 x 1920`
- safe-area-aware

That should remain the core mobile baseline.

### Recommended policy

- keep portrait-first reference framing
- keep a single shared safe-area layer
- build adaptive layout rules for large screens
- do not rely on one exact aspect ratio or one exact phone size

### What larger screens should get

On tablets and foldables, use extra space for:

- wider event layouts
- more comfortable inventory views
- better roster browsing
- more breathing room in menus
- optional secondary panels where useful

Do not stretch a phone UI blindly and call that tablet support.

## Recommended phased platform plan

### Phase 0: Mobile-first baseline

- portrait-first iPhone and Android phones
- safe-area support
- stable `60 fps`
- simple `Low / Medium / High` quality tiers

### Phase 1: High-refresh support

- settings-driven frame-rate selector
- enable `120 fps` where supported
- keep `60` as default

### Phase 2: Large-screen adaptation

- tablet layout pass
- foldable sanity pass
- adaptive panel behavior
- larger-screen event and roster views

### Phase 3: Thermal and quality tuning

- optional Adaptive Performance integration
- better device profiling
- dynamic resolution or scaling review where needed

### Phase 4: PC evaluation

- only if the mobile product proves traction
- separate UX review before committing

## Project-level follow-up recommendations

These are the most useful follow-ups if the team adopts this strategy:

1. make [`AppFrameRate.cs`](../../Assets/_Project/Scripts/Core/Utilities/AppFrameRate.cs) settings-driven instead of hard-forcing `120`
2. enable iOS ProMotion only when the game has a real high-refresh settings path
3. revisit `uIRequiresFullScreen` when iPad support becomes a real target
4. audit portrait-first prefabs that still use landscape reference resolutions
5. define a small graphics-settings menu instead of relying only on Unity quality defaults
6. prototype `2D + presentation-depth` first before discussing any major `2.5D` shift

## Default policy for future debates

When choosing between:

- “push maximum graphics”
- and “ship beautiful, stable, adaptive mobile presentation”

prefer the second.

`Shogun` should feel:

- sharp
- smooth
- atmospheric
- premium

not merely expensive to render.

## External research basis reviewed on 2026-03-07

Android:

- Android large-screen starter guidance: `https://developer.android.com/guide/topics/large-screens/get-started-with-large-screens`
- Android fold-aware apps: `https://developer.android.com/guide/topics/large-screens/make-your-app-fold-aware`
- Android games and display refresh rate: `https://developer.android.com/games/optimize/display-refresh-rate`
- Android Frame Pacing library: `https://developer.android.com/games/sdk/frame-pacing`
- Android game power optimization: `https://developer.android.com/games/optimize/power`

Apple:

- Apple TN3192 on migrating away from deprecated `UIRequiresFullScreen`: `https://developer.apple.com/documentation/technotes/tn3192-migrating-your-app-from-the-deprecated-uirequiresfullscreen-key`

Unity:

- Adaptive Performance package: `https://docs.unity3d.com/Packages/com.unity.adaptiveperformance@latest`
- Android optimized frame pacing: `https://docs.unity.cn/ScriptReference/PlayerSettings.Android-optimizedFramePacing.html`
- iOS player settings / ProMotion support: `https://docs.unity.cn/Manual/class-PlayerSettingsiOS.html`
- Dynamic Resolution introduction: `https://docs.unity3d.com/6000.0/Documentation/Manual/DynamicResolution-introduction.html`
- Resolution scaling fixed DPI factor: `https://docs.unity3d.org.cn/6000.0/Documentation/ScriptReference/QualitySettings-resolutionScalingFixedDPIFactor.html`
- URP 2D renderer docs: `https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.1/manual/2d-index.html`

## Related documents

- [`doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md`](./doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.md)
- [`../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md`](../research/doc-eng-001-solo-unity-mobile-gacha-rpg-engineering-research.md)
- [`../research/doc-eng-002-unity-project-runtime-architecture-patterns.md`](../research/doc-eng-002-unity-project-runtime-architecture-patterns.md)
- [`design-004-roguelite-replayability-and-run-mode-framework.md`](./design-004-roguelite-replayability-and-run-mode-framework.md)
- [`../art/art-001-style-bible-and-visual-targets.md`](../art/art-001-style-bible-and-visual-targets.md)
- [`../art/art-008-roguelite-event-vignette-art.md`](../art/art-008-roguelite-event-vignette-art.md)
