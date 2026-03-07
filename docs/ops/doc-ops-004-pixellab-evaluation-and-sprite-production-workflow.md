# PixelLab Evaluation and Sprite Production Workflow

**Summary:** Practical recommendation for whether `Shogun` should use PixelLab, whether to start with subscription or API, how it fits the current Codex/Claude/Unity workflow, and what sprite-production approach is technically sensible for a low-volume character pipeline.

## Executive recommendation

For `Shogun`, PixelLab is worth trying, but only in a controlled, developer-side workflow and not as the only art tool.

Recommended decision:

- use `Gemini web chat / Nano Banana Pro` for concept and presentation art
- use PixelLab for developer-side playable-sprite generation and iteration
- start with a one-month `Tier 1` subscription, not the API
- use Character Creator, Aseprite extension, or Pixelorama first
- optionally add PixelLab MCP later if the visual output is good enough
- do not build around live in-game generation

This is the highest-leverage path for the current project scale:

- expected character throughput is low
- you already use Codex, Claude, and Unity MCP locally
- the main open question is identity retention and cleanup burden, not automation depth

## Why PixelLab is a good fit

PixelLab is unusually aligned with the kind of art pipeline `Shogun` needs:

- directional pixel characters
- animation generation
- tilesets and supporting environment art
- MCP support for AI coding assistants
- editor-side workflows through Aseprite and Pixelorama

That makes it more relevant than generic AI image tools for this specific game.

## Why not over-commit yet

The important unknown is not whether PixelLab can generate assets. It can.

The real unknowns are:

- whether the style is good enough for `Shogun`
- how much manual cleanup each usable character requires
- whether the generated animation quality is good enough for gameplay, not just concept art
- whether consistency across multiple characters is manageable

So the correct first move is a bounded trial, not an engineering integration project.

## Subscription vs API

### Recommended now: subscription

Start with a subscription if the goal is:

- 2 to 3 characters per week
- visual iteration
- trying multiple workflows before committing
- validating whether the art direction works at all

Why:

- PixelLab's MCP tooling requires an active subscription
- several higher-level editor workflows are already built around the subscription product
- the current project does not need batch automation badly enough to justify API-first integration

### Recommended later: API

Consider API only if you later want:

- batch generation scripts
- automated import into Unity
- a formal asset-processing pipeline
- custom tooling beyond what the MCP/editor flows expose

If API is used later, prefer `v2`, not `v1`.

Reason:

- PixelLab's MCP documentation describes `API v1` as legacy/deprecated
- `API v2` exposes the character and animation endpoints that actually matter for this project

## How PixelLab should fit the current tool stack

Current stack:

- Codex
- Claude Code
- Unity MCP
- local Unity project and docs

Recommended fit:

- keep Codex and Claude as the coding/planning layer
- keep Unity MCP as the live Unity-inspection layer
- use Gemini primarily as the concept and portrait layer
- use PixelLab primarily as the playable sprite-generation layer

Recommended order of adoption:

1. lock one character fantasy
2. generate concept art in Gemini
3. PixelLab subscription
4. Character Creator or Aseprite extension or Pixelorama
5. one end-to-end character test
6. only then decide whether PixelLab MCP is worth adding
7. only later consider API automation

This avoids solving the wrong problem too early.

## PixelLab MCP recommendation

PixelLab MCP is promising, but it should not be the first step.

Use it later if:

- the art quality already passes your bar
- you want Claude or Codex to queue generations from the IDE
- you want asset generation to sit closer to your coding workflow

Do not start with it if:

- you have not yet proven the visual style
- you do not yet know whether PixelLab's cleanup burden is acceptable
- you are still choosing between Aseprite and Pixelorama

For `Shogun`, PixelLab MCP is an optional acceleration layer, not the core decision.

## Runtime generation vs developer-side generation

Do not plan around live in-game generation.

Use Gemini and PixelLab as developer-side content tools only.

Reasons:

- your own legal docs already flag that embedding live AI generation inside the shipped game expands legal and platform-policy scope sharply
- runtime generation also adds moderation, privacy, retention, and support complexity that this project does not need
- `Shogun` benefits much more from stable authored assets than from live generation

So the correct boundary is:

- yes to developer-side generation
- no to player-facing runtime generation

For current `Shogun` planning, that means:

- Gemini is acceptable through the web app for concept/presentation work
- PixelLab is acceptable for dev-side concept-to-sprite conversion
- neither should be treated as a live player-facing service inside the shipped game

## Technical recommendation for sprite size and animation

### More frames are not automatically better

Smooth-looking sprite animation does not come mainly from frame count.

It comes from:

- strong silhouettes
- clear key poses
- readable motion arcs
- consistent proportions
- consistent lighting and shading
- manual cleanup where needed

If the poses and silhouettes are bad, adding more frames will not save the animation.

### Recommended starting size

Start with `64x64`.

Reason:

- PixelLab's animation docs show `32x32` and `64x64` as the sizes that get `16` output frames in the higher-level animation tools
- larger sizes drop to `4` frames in those same tools

For this project, `64x64` is the safest first target because it is still game-usable while giving the better frame count in PixelLab's current documented animation flows.

### Recommended directional approach

Start with `4 directions`, not `8`.

Reason:

- cheaper to test
- easier to keep consistent
- lighter production burden
- enough for many RPG combat and field-navigation cases

Move to `8 directions` only if gameplay readability genuinely needs diagonal-facing sprites.

### Recommended animation order

Do not start with a full move set.

Start in this order:

1. base character
2. idle
3. walk
4. hit or damage react
5. basic attack

If those do not hold up visually, do not scale the pipeline yet.

## Recommended production workflow

### Phase 1: style lock

Before generating production assets, lock a small art bible:

- palette direction
- outline thickness
- shading style
- body proportions
- camera/view angle
- level of realism vs stylization

Without this, consistency across characters will drift.

### Phase 2: first character test

Use one single character as the proving ground.

Recommended test:

- one humanoid character with a clear collectible fantasy
- `64x64`
- `4 directions`
- idle + walk

Recommended lane split:

- Gemini: portrait, bust, or full-body concept
- PixelLab: playable character conversion

Then evaluate:

- visual quality
- consistency across directions
- cleanup time in editor
- import quality in Unity
- readability at actual gameplay scale

### Phase 3: cleanup workflow

Do not treat generated output as final.

Expected cleanup:

- silhouette fixes
- weapon or clothing cleanup
- palette correction
- small frame-to-frame consistency fixes
- occasional inpainting or re-generation

### Phase 4: expansion

Only after the first character works should you scale into:

- attack animations
- special animations
- additional characters
- variant outfits
- enemy sets

## Recommended editor workflow

Best first choice if you already use it:

- Aseprite + PixelLab extension

Best fallback if you do not want Aseprite immediately:

- Pixelorama workflow

Why Aseprite first:

- best fit for deliberate pixel cleanup
- local editor workflow is better for fine animation cleanup than pure browser generation
- PixelLab explicitly supports an Aseprite extension

Why Pixelorama remains valid:

- browser-based workflow
- easier trial path if you do not want another local tool first
- still keeps editing and generation in one place

## Provenance and legal handling

PixelLab's terms are workable for commercial use, but that does not eliminate IP and compliance risk.

Project rules:

- keep an asset provenance record
- do not assume provider ownership language guarantees exclusive copyright in all jurisdictions
- manually edit high-value shipped assets
- avoid franchise-copying behavior even if prompts are only "inspired by" external IP
- do not put sensitive internal material into prompts or uploads

Recommended provenance fields per shipped asset:

- source tool
- date
- prompt
- seed
- mode or workflow used
- manual edits performed
- final exported file hash

For hero characters, splash art, store icon work, or marketing-critical visuals:

- apply heavier manual editing, or
- replace with commissioned or largely hand-authored assets

## Tomorrow decision checklist

Use this checklist before spending more time or money:

1. Decide whether to trial Aseprite or stay with Pixelorama first.
2. Buy only the smallest PixelLab tier that unlocks the needed workflow.
3. Generate one `64x64`, `4-direction` base character.
4. Generate `idle` and `walk`.
5. Measure cleanup time honestly.
6. Import into Unity and inspect readability at gameplay scale.
7. Decide whether the visual quality is good enough for production use.
8. Only then decide whether PixelLab MCP should be added.
9. Defer API work unless repeatable batch automation becomes a real bottleneck.

## Bottom line

For `Shogun`, PixelLab is worth evaluating.

But the correct posture is:

- subscription first
- editor workflow first
- one-character trial first
- MCP later
- API much later
- no runtime generation

Treat it as an assisted art-production tool, not an autonomous final-art pipeline.

## Sources reviewed on 2026-03-07

Internal project docs:

- `DOC-LEGAL-001`
- `DOC-LEGAL-002`

Official PixelLab sources:

- MCP docs: `https://api.pixellab.ai/mcp/docs`
- Ways to use PixelLab: `https://www.pixellab.ai/docs/ways-to-use-pixellab`
- API v2 docs dump: `https://api.pixellab.ai/v2/llms.txt`
- Terms of Service: `https://www.pixellab.ai/termsofservice`
- Privacy Policy: `https://www.pixellab.ai/privacypolicy`
- Animate with text (Pro): `https://www.pixellab.ai/docs/tools/animate-with-text-pro`
- Create animated object/character (Pro): `https://www.pixellab.ai/docs/tools/text2animation`
- Animate with skeleton: `https://www.pixellab.ai/docs/tools/animate-with-skeleton`
- Init image options: `https://www.pixellab.ai/docs/options/init-image`
- Camera options: `https://www.pixellab.ai/docs/options/camera`

Official Google Gemini sources:

- Image generation docs: `https://ai.google.dev/gemini-api/docs/image-generation`
- Gemini image generation help: `https://support.google.com/gemini/answer/14286560?hl=en`
- Gemini Apps privacy hub: `https://support.google.com/gemini/answer/13594961`
- Nano Banana Pro announcement: `https://blog.google/innovation-and-ai/products/nano-banana-pro/`

Notes:

- pricing and tier details can change, so validate them on the purchase day
- this recommendation does not depend on exact price numbers
