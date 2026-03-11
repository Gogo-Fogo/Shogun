# ART-010 — AI Character Art Generation Pipeline

> Established: March 2026
> Based on: live pipeline testing session, Ronin Footman as proof-of-concept character
> Status: proven and locked for production use

---

## Purpose

This document captures the full AI-assisted character art pipeline for Shogun — from
character brief through portrait card art, gameplay sprites, cleanup, and Unity integration.

Use this doc when starting a new character from scratch or when any AI in the pipeline
(Claude, Codex, or manual) is handling character art tasks.

---

## Tool split — what each AI handles

This is not one tool doing everything. Each tool has a lane.

| Tool | Lane | Why |
|---|---|---|
| Gemini (Pro/Flash) | Portrait card art — 4 poses per character | Best painterly anime quality, image generation |
| PixelLab (MCP or web) | Gameplay sprites + animations | Purpose-built for sprite sheets, frame consistency |
| Aseprite | Cleanup and animation correction | Manual pixel editing, Gemini art as reference |
| Unity | Import, CharacterDefinition wiring | Runtime integration |

**These tools are not interchangeable.** Gemini cannot generate sprite sheets with
consistent animation frames. PixelLab cannot generate portrait card art. They do
different jobs.

---

## Art direction — locked decisions

These were reached after deep research comparing Fire Emblem Heroes (FEH) and
Naruto Ninja Blazing. Do not re-litigate them.

### Style target: Blazing painterly, FEH system

**Rendering style:** Naruto Ninja Blazing official art quality.
Painterly, soft shading, anime linework. NOT flat cel-shading. NOT western comic.

**Why not FEH rendering:** FEH art is human-illustrated at professional level.
Gemini can match Blazing's painterly quality more reliably than FEH's crisp
hand-illustrated look.

**What to steal from FEH:**
- The 4-pose system (portrait / attack / damage / special)
- Element colour identity per character
- Gold trim as the universal premium signal
- Transparent/white background output

**What to steal from Blazing:**
- Painterly organic effects — not neon overlays
- Dark vignette / dark character + vivid accent formula
- Ghost/narrative silhouette device in special pose
- Ink brushstroke motion trails for speed effects
- Effects at 60% of special pose image space
- Characters breaking the frame (blade/effects cut off at edges — intentional)

### The portrait/card art formula

```
Dark BASE (armor/clothing) + ONE vivid signature accent colour + gold trim
```

- Dark characters still pop — because the EFFECT colour is vivid, not the costume
- Gold trim on armor = premium signal, always present (even worn/faded)
- Black/dark palette against white background creates maximum contrast

### Ukiyo-e role

Ukiyo-e = **composition and silhouette influence only**. NOT the rendering style.
- Asymmetric composition, fabric flow, dramatic silhouette: from ukiyo-e
- Rendering, shading, effects: from Blazing painterly anime

---

## The full pipeline

```
Phase 1: Character Brief
    ↓
Phase 2: Gemini — 4 portrait poses (portrait/attack/damage/special)
    ↓
Phase 3: Background removal (white → remove.bg or Aseprite)
    ↓
Phase 4: PixelLab — gameplay sprite sheet + animations
    ↓
Phase 5: Aseprite — cleanup using Gemini art as reference
    ↓
Phase 6: Unity — import, slice, wire into CharacterDefinition
```

---

## Phase 1 — Character Brief

Write this once per character before touching any AI tool.
Every prompt in Phase 2 pastes this brief verbatim.

### Brief template

```
CHARACTER: [Name]
Role: [player hero / enemy grunt / boss]
Build: [lean / stocky / tall / short]
Weapon: [be specific — "single one-handed katana (ONE sword only)"]
Clothing: [describe base outfit]
Armor: [describe armor pieces and condition]
Element colour: [ONE vivid accent colour — this is the character's identity]
Gold trim: yes / no (default yes — worn/faded for enemies, bright for heroes)
Hair: [describe]
Skin tone: [be specific — "warm olive" etc. — prevents drift between poses]
Expression (portrait): [be specific — "hollow and cold, empty eyes, no warmth"]
Emotional identity: [one sentence — who is this person, what drives them]
Ghost figure (special pose): [describe the ghost/spirit that appears behind them]
```

### Proven example — Ronin Footman

```
CHARACTER: Ronin Footman
Role: enemy grunt
Build: lean, weathered
Weapon: single one-handed katana (ONE sword only)
  — vivid crimson red tsuka-ito hilt wrapping, diamond pattern
Clothing: tattered dark haori with torn ragged edges
Armor: dark iron chest/shoulder plates, cracked and dented, faded gold trim
Element colour: vivid crimson red
Gold trim: yes — faded and worn
Hair: dark, tied back loosely, disheveled
Skin tone: warm olive, weathered
Expression (portrait): hollow and cold — empty eyes, no warmth, no hope.
  He is past caring.
Emotional identity: a disgraced samurai with nothing left to lose —
  dangerous precisely because he fears nothing
Ghost figure (special): the lord he failed — full armored samurai silhouette,
  head bowed in silent judgment. His presence is corrupting the Ronin.
```

---

## Phase 2 — Gemini portrait art

### Critical Gemini gotchas

**1. White background — not checkerboard**

Gemini has learned that PNGs show a checkerboard. It will DRAW the checkerboard
as pixels. That is not real transparency. Always specify:

```
Solid flat white background. NO checkerboard. NO transparency.
NO gradient. Pure white only. Background removal done in post.
```

**2. ONE IMAGE ONLY**

Without this, Gemini combines all 4 poses into one composite image.
Write it in caps. Put it near the top of every prompt.

**3. Weapon count drift**

Always specify weapon count explicitly:
```
ONE katana — [description]. ONE sword only.
```

Never assume Gemini will remember from the brief. State it in the pose prompt too.

**4. The quality anchor line**

Once the portrait is generated and approved, add this to EVERY subsequent pose prompt:

```
Match the exact rendering quality of the portrait pose —
oil painting texture depth, rich painterly shading on armor
and cloth, soft volumetric light. Same quality level throughout.
Do NOT simplify rendering for complexity.
```

This is the single most effective prompt technique discovered. Without it,
complex poses (attack, special) regress to flat comic style.

**5. Prompt complexity vs quality tradeoff**

Gemini has a fixed "quality budget" per generation. More instructions =
less rendering depth. Keep pose prompts lean — specify what matters,
trust Gemini on the rest.

**6. Image attachment kills drift for bust/pfp crops**

When generating a bust portrait or pfp from an existing full-body portrait,
attach the portrait image directly to the Gemini message alongside the prompt.
Gemini uses the visual reference to match rendering style, hair colour, face
structure, and overall aesthetic far more accurately than text alone.

This is the most effective solution to pfp/portrait consistency drift.
Use it for every bust crop generation after the full portrait is approved.

**7. Prompt-to-prompt drift**

Each generation is independent. Gemini does not remember the previous output.
Character faces, skin tones, and small details drift between poses.
Fix: specify every critical detail in every prompt. Add skin tone and hair
colour explicitly each time.

**8. Simple language sometimes beats technical specification**

If something looks wrong, describe it plainly. Example:
> "Something feels off with one of the lights, I think the glow
> on one of them is misaligned"
This worked better than a technical prompt fix. Gemini understands
natural language descriptions of visual problems.

**9. Happy accidents become design decisions**

Gemini will sometimes add things you didn't spec — a corrupted eye,
an extra flame effect, a compositional element. Evaluate before rejecting.
The Ronin Footman's one blue-corrupted eye (ghost influence bleeding in)
became canon character design.

---

### The 4 pose prompts

Send each pose as a **separate message** in Gemini. One prompt = one image.

---

#### PORTRAIT PROMPT

```
[Character Brief]

ONE IMAGE ONLY. Portrait pose.
Full body, standing 3/4 angle toward viewer.
Eyes engaging camera directly. [Expression from brief].
[Weapon] held loosely at side.

Solid flat white background. NO checkerboard. Pure white only.

Painterly anime illustration style.
Naruto Ninja Blazing / FEH quality.
Soft shading, clean linework, NOT flat comic style.
Vivid [element colour] element glow on weapon edges.
Vertical portrait card format. No text. No watermark.
```

Key rules for portrait:
- **3/4 angle, never full frontal** — 3/4 is more dynamic and FEH-standard
- **Eyes toward viewer** — collection psychology
- **Relaxed/neutral weapon hold** — power at rest reads as confidence
- No effects in the portrait — save effects for action poses

---

#### ATTACK PROMPT

```
Match the exact rendering quality of the portrait pose —
oil painting texture depth, rich painterly shading on armor
and cloth, soft volumetric light. Same quality level throughout.
Do NOT simplify rendering for complexity.

[Character Brief]

ONE IMAGE ONLY. Attack pose.

Pose: Explosive diagonal strike. [Weapon] swinging in a wide arc
toward the viewer — foreshortened [weapon] thrusting forward.
Feet planted, body weight fully committed to the slash.
Expression: cold focused intensity — not rage, not emotion.
Just the mechanical precision of a [emotional identity].

Background: Solid flat white. NO checkerboard. Pure white only.

Effects: Vivid [element colour] energy trailing the [weapon] arc.
Painted organic soft-glow — bleeds and fades naturally.
NOT neon. NOT lightsaber. Ink brushstroke motion trails.
[Clothing] torn edges and hair flying from force of the strike.

Rendering: Painterly anime illustration style.
Naruto Ninja Blazing / FEH quality.
Soft shading, clean linework, NOT flat comic style.

Vertical portrait format. No text. No watermark.
```

Key rules for attack:
- **Foreshortening toward viewer** — weapon or hand breaks the plane toward camera
- **Blazing-style energy trails** — organic, painted, not neon arcs
- **Body fully committed** — no half-measures in the pose

---

#### DAMAGE PROMPT

```
Match the exact rendering quality of the portrait pose —
oil painting texture depth, rich painterly shading on armor
and cloth, soft volumetric light. Same quality level throughout.
Do NOT simplify rendering for complexity.

[Character Brief]

ONE IMAGE ONLY. Damage pose.

Pose: He/she has just taken a heavy blow to the shoulder/chest.
Body twisting away from impact — back half-turned to viewer.
Head down, hair falling forward. One knee slightly bent.
NOT fully fallen. Still gripping the [weapon] — will not drop it.
Expression: teeth clenched in pain, one eye barely open.

Background: Solid flat white. NO checkerboard. Pure white only.

Effects: [Armor piece] cracked and fragmenting at impact point —
armor shards mid-air. [Clothing] torn further from the hit.
Faint [element colour] energy still flickering — dim, not extinguished.
Hurt but not finished.

Rendering: Painterly anime illustration style.
Naruto Ninja Blazing / FEH quality.
Rich cloth and armor detail throughout.

Vertical portrait format. No text. No watermark.
```

Key rules for damage:
- **Still gripping the weapon** — this is the character's core trait
- **Power reacts to being hit** — element colour flickers but doesn't die
- **Armor fragments mid-air** — concrete visual read of impact
- **NOT fully defeated** — damage pose is not death

---

#### SPECIAL PROMPT

```
Match the exact rendering quality of the portrait pose —
oil painting texture depth, rich painterly shading on armor
and cloth, soft volumetric light. Same quality level throughout.
Do NOT simplify rendering for complexity.

[Character Brief]

ONE IMAGE ONLY. Special pose.

Pose: Wide low stance. [Weapon] raised overhead in massive arc,
both hands on hilt. Body fully committed — final desperate strike.
Teeth clenched, hollow eyes burning. Hair and [clothing] exploding outward.

Background: Solid flat white. NO checkerboard. Pure white only.

Ghost: Large semi-transparent [ghost description] looming behind him.
Painted in ink wash style — NOT 3D, NOT CGI, NOT geometrically detailed.
Suggested armor shapes only — form dissolves at edges into the energy.
Eyes: two points of cold [ghost eye colour] light — simple, haunting.
The ghost's glow does NOT cast light onto the foreground character.
Monochrome blue-grey ink wash. [Head position in silent judgment].

Effects: Massive vivid [element colour] energy explosion surrounding him.
Painted organic soft-glow. Fills 60% of image. Bleeds to frame edges.
Character breaks the frame — [weapon/effects] cut off at edges is intentional.

The foreground character's eyes are natural human eyes.
No supernatural glow on him — only the ghost glows.
Eye highlights and light reflections must align precisely with
iris and pupil position. No floating or misplaced glints.

Rendering: Painterly anime illustration style.
Naruto Ninja Blazing / FEH quality.
Soft shading, clean linework, NOT flat comic style.
Rich armor and cloth detail throughout.

Vertical portrait format. No text. No watermark.
```

Key rules for special:
- **Ghost = ink wash suggestion, NOT rendered 3D** — the most important lesson
  from testing. Detailed ghost = CGI quality bleed. Suggested ghost = painterly.
- **Ghost glow stays behind foreground character** — otherwise it bleeds onto
  the character's face and eye as a misaligned glow
- **Effects bleed off frame** — this is intentional Blazing design language,
  not an error. Power too big to contain.
- **60% effect fill** — character is almost secondary to the power moment

---

### Ghost design principles

The ghost narrative device is the most distinctive element of the special pose.
Rules:

1. **Ink wash, not rendered** — "suggested armor shapes only, NOT solid or 3D"
2. **Dissolves at edges** — form fades into the energy around it
3. **Eyes carry it** — two glowing points are more haunting than full facial detail
4. **Monochrome + cool tone** — contrast with the character's warm element colour
5. **Narrative connection** — the ghost must have a story relationship to the character
6. **Does not cast light** — specify explicitly or the glow bleeds onto the character

---

## Phase 3 — Background removal

Gemini outputs white backgrounds. Remove them before Unity import.

**Recommended tool:** remove.bg (free tier handles most cases)

**Aseprite alternative:** open the PNG, use the magic wand on the white areas,
delete. Good for edge cleanup after remove.bg.

**Do not ask Gemini for transparent output.** It will draw a checkerboard.
White background + remove.bg is the reliable pipeline.

---

## Phase 4 — PixelLab sprite generation

### Why PixelLab, not Gemini

Gemini cannot generate sprite sheets with consistent animation frames.
Frame-to-frame character drift that is acceptable in portrait art is
catastrophic in animation — the character morphs as it moves.

PixelLab generates all animation frames from **one locked character base**.
That is the fundamental advantage.

### Sprite specifications

| Property | Value |
|---|---|
| Size | 64×64 px |
| Directions | 2 minimum (player right, enemy left) |
| First animations | idle + attack only — prove pipeline first |
| Full animation set | idle, attack, hit, death, walk |
| Frame budget | 4–6 frames per animation |

### Writing the PixelLab description

Translate the character brief into what survives at 64px.
Most portrait detail is invisible at sprite scale. Focus on:

**Keep:**
- Overall silhouette shape (the most important thing)
- Primary palette (2–3 colours max)
- The one vivid element accent colour
- One identifying costume feature (Ronin Footman: ragged haori edges)

**Drop:**
- Fine armor detail (invisible at 64px)
- Facial expression (invisible at 64px)
- Cloth fold nuance (invisible at 64px)

### The golden rule at 64×64

**Silhouette first.** If the character's black shadow alone does not read as
a specific identity — the sprite has failed.

Distinctive silhouette features to emphasise:
- Tattered/irregular clothing edges
- Unusual weapon shapes
- Distinctive hair silhouette
- Extreme height/build differences between characters

### PixelLab description template

```
[Role] character. [Build] build. [Distinctive silhouette feature — lead with this].
[Clothing in one line]. [Armor in one line]. [Weapon] at [position].
[Hair in one line]. Palette: [dark base colour], [secondary], [vivid accent] only.
```

### Proven example — Ronin Footman

```
Dark samurai grunt. Lean weathered build.
Tattered ragged-edged dark haori cloak — irregular torn silhouette, key feature.
Iron chest armor with gold trim edges. Dark hakama pants.
One katana at side with crimson red glowing blade.
Dark hair tied back loosely.
Palette: iron grey, black, vivid crimson accent on weapon only.
```

### Animation descriptions

Use the Gemini pose art as your reference when writing animation descriptions.
Look at the attack pose and describe what you see:

```
attack: wide diagonal slash, katana swinging overhead right to lower left,
tattered haori trailing behind motion, crimson energy arc following blade path
```

---

## Phase 5 — Aseprite cleanup

### What Aseprite is for

Aseprite cleanup is **correction**, not creation. You are fixing AI output,
not drawing from scratch. The bar is low.

### What to fix

| Issue | Fix |
|---|---|
| Stray pixels | Click and delete |
| Wrong colour on small detail | Recolour individual pixels |
| Misaligned animation frame | Nudge pixels to match adjacent frame |
| Eye highlight position | Correct to align with iris center |
| Silhouette edge not matching portrait | Trim or fill pixels to match |

### Using Gemini art as reference

Open the relevant Gemini pose alongside the sprite in Aseprite:

- **Portrait** → compare idle sprite silhouette. Hair on correct side? Armor proportions match?
- **Attack pose** → compare attack animation keyframe. Weapon angle? Body lean direction?
- **Damage pose** → compare hit animation. Which direction does he lean? Which armor piece cracks?

The Gemini art is your **visual checklist**, not something to redraw.

### When to skip cleanup

For the vertical slice, cleanup priority:

1. **Portrait pose art** — high priority, this is what players see 90% of the time
2. **Idle sprite** — medium priority, always visible in battle
3. **Attack animation** — medium priority, visible on every player action
4. **Hit/damage animation** — low priority for first slice
5. **Special pose art** — very low priority, 1–2 second flash

---

## Phase 6 — Unity integration

### File placement

```
Portrait card art:
Assets/_Project/Art/Production/Characters/[CharacterName]/Portraits/
  [CharacterName]_portrait.png
  [CharacterName]_attack.png
  [CharacterName]_damage.png
  [CharacterName]_special.png

Sprite sheet:
Assets/_Project/Features/Characters/Art/Production/[CharacterName]/
  [CharacterName]_spritesheet.png

Animation clips:
Assets/_Project/Features/Characters/Art/Production/[CharacterName]/Animations/
  [CharacterName]_idle.anim
  [CharacterName]_attack.anim
  [CharacterName]_hit.anim
  [CharacterName]_death.anim
```

### CharacterDefinition wiring

After sprites and anims are imported, wire into the CharacterDefinition SO:
- `battleSprite` → idle frame 0 of sprite sheet
- `animatorController` → the .controller referencing the .anim clips
- `portraitSprite` → the portrait PNG
- Confirm stats, element affinity, weapon family match the character brief

---

## Element colour system

Every character has ONE element colour. This colour:
- Appears as subtle glow on weapon in portrait
- Trails the weapon as energy in attack pose
- Flickers faintly in damage pose (power reacting to hit)
- Dominates the special pose effects (60% of image)
- Defines the special animation particle colour in gameplay

Assign the element colour **before** generating any art. Do not change it.

| Character type | Element colour guidance |
|---|---|
| Player hero (sword) | Gold, cyan, or character-specific signature |
| Player hero (magic) | Element-matched (fire=orange, ice=blue, spirit=purple) |
| Enemy grunt | Muted version of element OR corrupted variant |
| Boss | Dark, desaturated base + one vivid supernatural accent |

---

## Monetisation psychology — the 4-pose system

Each pose serves a specific psychological function. This is why FEH's
4-pose system is a monetisation machine:

| Pose | Psychological function |
|---|---|
| Portrait | Collection desire — "I want this character" |
| Attack | Power fantasy — "this character is strong" |
| Damage | Emotional attachment — "this character has vulnerability" |
| Special | Dopamine hit — fires on every critical hit in battle |

The special pose is not a one-time reveal. It fires **repeatedly** during
gameplay. Every critical hit = mini dopamine reward for owning a rare character.
This is the engine that drives engagement and reroll motivation.

Design consequence: the special pose must feel extraordinary every time it fires,
not just on first view. Composition and effect scale matter more than narrative
detail at this moment.

---

## PFP / HUD medallion prompt — final proven template

**Always attach the approved portrait image alongside this prompt.**

```
[ATTACH portrait image]

Match the face, age, skin, rendering style of the attached
portrait exactly. Do NOT add wrinkles, scars, or skin texture
beyond what is in the reference. Same person. Same age.

ONE IMAGE ONLY. HUD medallion face portrait.

Framing: face fills 85% of frame. Eyes at center.
Chin near bottom edge. Sliver of armor at bottom.

Expression: cold, dangerous, furrowed brow, lips pressed.
Direct stare straight at viewer. NOT neutral. NOT sad.

Eyes: one dark brown — cold, flat.
One corrupted blue — supernatural, haunting, eerie.

Background: solid flat white. NO checkerboard.

Rendering: painterly anime. Same quality as attached portrait.

Square format. No text. No watermark.
```

**Key rules for pfp:**
- Face fills 85% of the square — this displays as a small circle, face must dominate
- Always attach the portrait image — text alone causes face/age drift
- Never describe age, scars, or weathering — let the portrait reference handle it
- "Do NOT add wrinkles or skin texture beyond the reference" is critical

---

## Quick reference — what to do when Gemini output is wrong

| Problem | Fix |
|---|---|
| Fake checkerboard background | Add "Solid flat white background. NO checkerboard." |
| All 4 poses combined in one image | Add "ONE IMAGE ONLY" in caps near top |
| Character has two weapons | Specify "ONE [weapon] only" in both brief and pose prompt |
| Rendering is flat/comic style | Add the quality anchor line + "NOT flat comic style" |
| Ghost looks 3D/CGI | Change to "ink wash style, suggested forms only, NOT solid or 3D" |
| Ghost glow bleeds onto character face | Add "ghost glow does NOT cast light on foreground character" |
| Eye highlight misaligned | Describe plainly: "eye highlights must align with iris position" |
| Character looks too heroic/protagonist | Sharpen the emotional identity + expression description |
| Too many effects, quality drops | Simplify the effects section — less instruction = more quality budget |
| Character drifts between poses | Add skin tone + hair colour explicitly to every prompt |
