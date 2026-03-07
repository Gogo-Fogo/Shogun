# Shogun: Flowers Fall in Blood — Master Game Design Document (Synthesized)

**Document intent:** This is an expanded “master GDD” synthesized from the creator’s official project page, the linked assets on that page, and the user-supplied PDF/docx research files. It is written as a **single source of truth** for AI-assisted development, implementation, QA, and publishing diligence.

**Primary sources prioritized:**
- Official project page (links, prototype status, current focus, systems list, inspirations). citeturn2view0  
- Official creator “About” + related project pages (background/credibility context). citeturn5view0turn6view0turn5view2turn5view1  
- Official/compliance references for monetization and shutdown risk cases used for market framing (Apple loot box odds disclosure; official end-of-service notices; store listings for comparable games). citeturn1search3turn0search1turn11view0turn8search0turn9search1turn9search2  

**User-supplied authoritative documents used (not tool-citable in this environment; referenced by filename + internal section names):**
- `recruiter/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.pdf` (59 pages; includes detailed sections on systems, monetization, UI flows, production milestones, LiveOps, formulas).  
- `research/doc-ref-001-naruto-ultimate-ninja-blazing-conceptual-synthesis.md` (personal synthesis; explicitly “not official”; used as inspiration analysis and design lessons).  
- `research/doc-ref-004-fire-emblem-heroes-success-and-drawbacks.md` and `research/doc-ref-003-one-piece-treasure-cruise-analysis.md` (secondary market/system essays; used for design risk guardrails and LiveOps caution).

**Note on Google Docs:** The project page links to Google Docs for the GDD and systems audit; in this environment, Google Docs often require JavaScript/sign-in and may be partially inaccessible. The **uploaded PDFs are treated as authoritative** for content, while the project page is authoritative for what is publicly claimed and currently implemented. citeturn2view0

---

## Table of contents

- [Executive summary](#executive-summary)  
- [Scope, pillars, and product definition](#scope-pillars-and-product-definition)  
  - [High-level concept](#high-level-concept)  
  - [Design pillars](#design-pillars)  
  - [Prototype vs intended full product](#prototype-vs-intended-full-product)  
  - [Target audience and positioning](#target-audience-and-positioning)  
- [Core gameplay design](#core-gameplay-design)  
  - [Core loop](#core-loop)  
  - [Battle system specification](#battle-system-specification)  
  - [Controls and gesture vocabulary](#controls-and-gesture-vocabulary)  
  - [Initiative and turn-order timeline](#initiative-and-turn-order-timeline)  
  - [AI design](#ai-design)  
  - [Enemy and boss design](#enemy-and-boss-design)  
  - [Encounter and level design](#encounter-and-level-design)  
- [Meta systems: progression, economy, LiveOps, monetization](#meta-systems-progression-economy-liveops-monetization)  
  - [Progression systems](#progression-systems)  
  - [Currencies and economy](#currencies-and-economy)  
  - [Events, banners, and battle pass](#events-banners-and-battle-pass)  
  - [Monetization strategy and compliance](#monetization-strategy-and-compliance)  
- [UX/UI specification](#uxui-specification)  
  - [Navigation flows](#navigation-flows)  
  - [Combat HUD](#combat-hud)  
  - [Accessibility and device scalability](#accessibility-and-device-scalability)  
- [Art, audio, tech, production, and appendices](#art-audio-tech-production-and-appendices)  
  - [Art direction and pipelines](#art-direction-and-pipelines)  
  - [Audio plan](#audio-plan)  
  - [Technical stack and build targets](#technical-stack-and-build-targets)  
  - [Backend requirements and telemetry](#backend-requirements-and-telemetry)  
  - [Risk matrix and mitigations](#risk-matrix-and-mitigations)  
  - [Roadmap, milestones, and sprint tasks](#roadmap-milestones-and-sprint-tasks)  
  - [QA and playtest protocols](#qa-and-playtest-protocols)  
  - [AI prompt templates](#ai-prompt-templates)  
  - [Legal/IP checklist and cultural guidance](#legalip-checklist-and-cultural-guidance)  
  - [Asset inventory and direct links](#asset-inventory-and-direct-links)  

---

## Executive summary

Shogun: Flowers Fall in Blood is an independently developed **mobile tactical RPG prototype built in Unity**, featuring **grid-based positioning** with **movement/attack range circles** and a planned “tactics + execution” hybrid where **gesture inputs** (tap/swipe/hold/trace) determine the success and potency of cinematic skills. The official project page states the prototype currently validates **turn order and position-based combat loops**, and that the active implementation focus is an **initiative/turn-order timeline** followed by **basic enemy AI** to close the early combat loop. citeturn2view0

The supplied GDD expands the design into a full live-service product with: hero collection (gacha), unit progression (leveling/awakening/limit break), gear progression, elemental/status systems, PvE modes (campaign, endurance), co-op raids, PvP arena, clans/guild governance, a 28-day battle pass, cosmetics, LiveOps cadence, and internal tool plans (banner scheduling, reward logs, server health). (Source: `recruiter/doc-gdd-001-shogun-flowers-fall-in-blood-game-design-document.pdf`, Sections “Core Gameplay Systems,” “Monetization Systems,” “Studio Production Pipeline,” “Post-Launch Live Ops Design.”)

The central strategic decision for any developer/publisher path is aligning **scope to capacity**:
- If the goal is **publisher-backed live service**, the GDD is already oriented in that direction, but requires backend, analytics, and content pipelines that exceed most solo capabilities. (Source: GDD PDF, “LiveOps Internal Tools,” “Milestones.”)  
- If the goal is a **premium or “small-live” tactics game**, the prototype’s differentiator—**high-readability tactical circles + initiative forecasting + gesture mastery**—can be shipped with far less operational risk, similar to premium tactics releases on mobile that explicitly avoid IAP. citeturn9search2turn8search1  

Compliance and market risk are not theoretical: multiple tactical gacha games have ended service globally, and official end-of-service notices show the operational fragility of long-running live services. citeturn0search1turn11view0

---

## Scope, pillars, and product definition

### High-level concept

**Shogun: Flowers Fall in Blood** is described on the official case study page as a **mobile tactical RPG prototype built in** entity["company","Unity","game engine company"], blending grid tactics inspired by entity["video_game","Naruto Shippuden: Ultimate Ninja Blazing","mobile tactics gacha"] with gesture-driven combat philosophy inspired by entity["video_game","Ronin: The Last Samurai","mobile action game"]. citeturn2view0

Narratively, the GDD frames a grim dark fantasy set in corrupted feudal entity["country","Japan","east asia country"] with a breach into Yomi (Shinto underworld) spilling demonic forces into the mortal realm. (Source: GDD PDF, “Core Premise.”) For cultural framing, external references describe “Yomi” as an underworld concept appearing in Shinto myths (not a central doctrine comparable to Western “hell”), which supports treating Shogun’s Yomi depiction as a **myth-inspired dark fantasy** rather than theological simulation. citeturn12search1

### Design pillars

The GDD’s “four pillars” can be operationalized into implementation requirements:

1) **Precision tactical combat**: Positioning/range control decide outcomes; missteps are costly. (Source: GDD PDF, “Combat Philosophy.”)  
2) **Gesture-based cinematic skills**: Tap/swipe/hold/trace skills with timing windows create execution mastery. (Source: GDD PDF, “Gesture Skill Execution.”)  
3) **Deep meta + collection**: Roster growth via gacha collection, progression, equipment, skill trees; long-term mastery. (Source: GDD PDF, “Character Systems.”)  
4) **Evolving narrative + world**: Dark fantasy with faction choices and ongoing story modularity. (Source: GDD PDF, “Design Vision.”)

The official project page validates the “combat-first” focus: current systems emphasized are movement radius, attack range circles, tactical grid readability, and initiative timeline work-in-progress. citeturn2view0

**Living companion note:** the original GDD remains the primary product-scope document, but newer cross-disciplinary character-planning decisions are maintained in the design set:

- `docs/design/design-001-character-collection-and-fantasy-strategy.md`
- `docs/design/design-002-world-pillars-and-combat-identity-framework.md`

Use those notes as the current operating detail for roster identity, world/faction buckets, collectible fantasy, elemental affinity, weapon families, and martial-school layering.

### Prototype vs intended full product

**Prototype confirmed by public page (implemented / in-progress):**
- Movement within defined movement radius + dynamic attack range circles + grid-based positioning. citeturn2view0turn13view3  
- Initiative/turn-order timeline system is actively being built; next task is enemy AI. citeturn2view0  

**Full product described by GDD (planned / “vertical slice targeted”):**
- Multi-mode PvE (campaign, endurance), co-op raids, PvP arena, clans, gacha economy, battle pass, LiveOps cadence, internal LiveOps tools, localization. (Source: GDD PDF, “Milestones,” “Progression,” “Multiplayer,” “Monetization,” “Live Ops.”)

**[INFERENCE]** The project’s best near-term “truth set” for development is to treat the prototype scope as **combat kernel + UI clarity** (initiative, circles, gesture inputs, enemies, encounter variety) and treat LiveOps/multiplayer as a later “scale layer,” because the official page explicitly prioritizes closing the early combat loop (initiative + enemy AI) before expanding scope. citeturn2view0

### Target audience and positioning

The GDD targets core mobile strategy/gacha players and “samurai culture enthusiasts,” age 16+ due to mature/dark themes. (Source: GDD PDF, “Target Audience.”)

For market positioning, the closest sustained competitors are mobile SRPGs optimized for short tactical sessions and collection-based progression. An official store listing for a leading exemplar explicitly calls out touch-optimized maps, drag controls, auto-battle, and gacha currency (“Orbs”) used for summoning heroes. citeturn8search0turn7search6

**Positioning statement (synthesized):**
- “A touch-native tactical SRPG with readable range circles and a skill-expressive gesture layer—high mastery for bosses/PvP, low menu friction for mobile sessions.”  
This aligns with the GDD’s “single-hand portrait” and “≤4 taps to battle” UI intent (Source: GDD PDF, “UI/UX Philosophy,” “Tap budget”), and the project page’s “readable grid-based decision making.” citeturn2view0

---

## Core gameplay design

### Core loop

**Combat session loop (canonical design intent):**
1) Select mode → select squad → enter encounter (energy/token gating varies by mode). (Source: GDD PDF, “Progression,” “Modes,” “Energy.”)  
2) Tactical combat: positioning + initiative + gesture execution → victory/defeat. (Source: GDD PDF, “Core Gameplay Systems.”)  
3) Rewards: XP, currency, gear/loot, shards/materials → upgrade roster and gear. (Source: GDD PDF, “Battle Rewards,” “Gear Progression,” “Awakening.”)  
4) Meta loop: daily/weekly missions + events + banners → compulsion/collection/social loops. (Source: GDD PDF, “Retention,” “Live Ops.”)

**Prototype loop (confirmed WIP):**
- Combat readability + initiative timeline + enemy AI closure of the “one battle” loop. citeturn2view0

### Battle system specification

**Battlefield**
- Tactical grid with tile properties: empty, obstacle, hazard. (Source: GDD PDF, “Battlefield Structure.”)

**Zones (design heuristic)**
- Frontline / midline / backline roles to inform deployment, enemy spawns, and archetype expectations. (Source: GDD PDF, “Zones.”)

**Actions and turn economy**
- Each unit has movement radius (visual), movement points (1 tile = 1 point), default 2–3 tiles depending on unit type; cannot pass through obstacles/units. (Source: GDD PDF, “Unit Movement.”)  
- One action per turn (after or before movement): basic attack, skill attack, item use, defend (50% reduction), pass. (Source: GDD PDF, “Action Types.”)  
- Victory/defeat: defeat all enemies; lose if all player units die or timer expires in timed boss battles. (Source: GDD PDF, “Victory/Defeat.”)

**Range and targeting**
- Attack radius visualized during targeting phase; melee adjacent; ranged up to ~5 tiles; obstacles block attacks. (Source: GDD PDF, “Range & Targeting.”)

**Synergy link attacks**
- Trigger when 2+ allied units have overlapping attack radii on a target; bonuses depend on weapon affinity and pairing. (Source: GDD PDF, “Synergy & Link Attacks.”)

**Elemental system**
- Five elements and cyclic advantage loop with ±25% damage modifiers. (Source: GDD PDF, “Elemental System.”)  
- Cultural reference note: the system is described as inspired by “The Book of Five Rings” and entity["people","Miyamoto Musashi","japanese swordsman author"] is historically associated with that work. citeturn12search4  
- **Respectful accuracy guidance:** the GDD replaces “Void” with “Shadow.” This is fine as fictionalization, but should be described as “inspired by” rather than “faithful adaptation.” (Source: GDD PDF, “Elemental System”; historical context). citeturn12search4

**Status effects and environment**
- Persistent statuses include poison, bleed, burn, freeze, stun, silence, fear, paralysis, blind, marked; instant effects include knockback/pull/interrupt; resist/cleanse via skills and gear; hazard tiles and destructible objects shape fights. (Source: GDD PDF, “Status Effects,” “Environmental Interactions.”)

**Combat math (starter formulas)**
- Damage formula: (ATK × skill multiplier × element modifier) – DEF, with damage floor; crit and status-chance formulas defined. (Source: GDD PDF, “Combat Formula Examples.”)

### Controls and gesture vocabulary

The GDD establishes a gesture system as the project’s identity layer:

- **Tap** → basic/light attack.  
- **Swipe (directional)** → heavy skill (e.g., spear thrust).  
- **Hold** → charge ultimate meter/skill.  
- **Trace shape (advanced)** → late-game, trace specific shapes (example: kanji) to unleash high-impact skills. (Source: GDD PDF, “Gesture Skill Execution.”)

**Combo windows**
- Certain skills require timing windows where gestures must be executed rapidly after action chains; failure reduces effect or increases cooldown. (Source: GDD PDF, “Combo Window.”)

**UX risks and mitigation (design guardrail)**
- **[INFERENCE]** Gesture systems increase accessibility risk (motor limitations, small screens, fatigue). Mitigation is already partially planned: large icon mode for gesture cues, simplified effects, and adjustable UI. (Source: GDD PDF, “Accessibility.”)  
- **[INFERENCE]** Provide optional “assist mode” for trace skills (e.g., widened hitbox, slowed trace time, or simplified alternates) while keeping high-rank mastery intact in PvP/endgame.

### Initiative and turn-order timeline

**Design intent**
- SPD determines action order; visible turn queue. (Source: GDD PDF, “Turn Order.”)

**Prototype implementation focus**
- Floating timeline of upcoming turns, dynamic head-crop sequencing, and real-time order updates based on outcomes; current task is enemy AI implementing autonomous enemy turns. citeturn2view0

**Implementation specification (recommended)**
- Timeline should support:
  - deterministic ordering for reproducibility (QA, PvP fairness)  
  - clear visualization of “next actor,” “after this action,” and “speed modifiers”  
  - animation-driven updates that show cause → effect, not silent reorder.  
This aligns with the “player should never have to guess” UI rule in the GDD. (Source: GDD PDF, “UI design rule.”)

### AI design

The official page states basic enemy AI is the next milestone to close the early loop. citeturn2view0 The GDD describes PvP defenders using player-saved behavior patterns and decision matrices. (Source: GDD PDF, “PvP Arena Combat System.”)

**AI tiers (recommended spec that stays consistent with stated “basic enemy AI” next step):**

**Tier 0: Deterministic MVP enemy AI (vertical slice)**
- Goal selection:
  - prioritize lethal targets (can kill this turn)  
  - else prioritize lowest effective HP target within range  
  - else prioritize removing support unit (healer/cleanser) if within reachable range  
- Movement:
  - compute reachable tiles (movement radius constraints + obstacles)  
  - compute best tile by scoring: (can attack) + (exposes to fewer enemy ranges) + (enables synergy for allies)  
- Action:
  - use basic attack unless skill is off cooldown and expected value exceeds threshold  
  - defend if cannot safely attack and is in multiple enemy threat ranges

**Tier 1: Telegraph + counterplay AI (boss-ready)**
- Visible “intent” indicator (attack direction/area) one turn ahead.  
- Boss phase transitions on HP thresholds (e.g., 70/40/15%).  
- Hazard spawning rules consistent with environmental systems. (Source alignment: GDD “Dynamic hazards (boss fights).”)

**Tier 2: PvP defense AI (asynchronous)**
- Player-defined behavior profiles per unit (aggressive/defensive/support)  
- “Decision matrix” serialized from player setting (GDD mention), but with guardrails to prevent degenerate loops (stall) and to ensure fairness. (Source: GDD PDF, “PvP defenders follow player-saved behavior patterns.”)

**Telemetry requirement:** Every AI action should log “reason code” (e.g., `LETHAL`, `NO_TARGET_DEFEND`, `SKILL_EV_ADVANTAGE`) to enable balancing and debugging. (Recommended; supports GDD’s production/QA goals and “structured playtests” ethos seen in the creator’s background). citeturn5view0

### Enemy and boss design

The GDD defines enemy pipeline tiers:
- Base grunts: humanoid bandits, corrupted soldiers  
- Mid-tier: demonic beasts, possessed monks, mutated yokai  
- Bosses: giant oni, monstrous spirits, Yomi warlords (with larger rigs and unique skeletal setups). (Source: GDD PDF, “Enemy Design Pipeline.”)

**Boss identity rules (recommended)**
- Every boss should have:
  - a core “threat signature” (hazard spawning, interrupt, pull, AoE)  
  - a counterplay tool (positioning, cleanse, elemental counter, interrupt timing)  
  - phase-based escalation without invalidating previous learning.

### Encounter and level design

The GDD provides tile types, hazards, destructible objects, and victory/defeat conditions, but does not fully specify encounter grammar. (Source: GDD PDF, “Battlefield,” “Environmental Interactions,” “Victory/Defeat.”)

**Encounter grammar (recommended; minimal speculation)**
- Encounters should be built from reusable “puzzle atoms”:
  - overlap-radii synergy puzzles (reward formation)  
  - LOS/obstacle puzzles (turn ranged units into “positioning problems”)  
  - hazard control puzzles (deny tiles, force movement)  
  - tempo puzzles (initiative manipulation, interrupts)

**Objective variety (recommended extension)**
- In addition to “defeat all,” add:
  - survive X turns (fits initiative emphasis)  
  - protect target (promotes defensive play)  
  - capture shrine nodes (forces movement)  
These objectives are consistent with tactical SRPG norms and do not conflict with documented systems.

---

## Meta systems: progression, economy, LiveOps, monetization

**Living companion notes:** this GDD remains the stable product-scope source, but the more opinionated long-term structure for balancing, replayability, and multiplayer phasing now lives in the paired internal design notes:

- `docs/design/design-003-long-term-balance-and-power-creep-policy.md`
- `docs/design/design-004-roguelite-replayability-and-run-mode-framework.md`
- `docs/design/design-005-co-op-pvp-and-social-systems-roadmap.md`

### Progression systems

**Account progression**
- Account level gains XP from all modes, unlocks features at milestones (co-op at level 10), grants small permanent buffs (e.g., extra daily raid attempt). (Source: GDD PDF, “Player Account Level.”)

**Campaign progression**
- Branching map with narrative battles; energy costs; replay for farming. (Source: GDD PDF, “Campaign Progression.”)

**Side content**
- Side quests, emergency missions, “Ninja Road” endurance mode (20–50 sequential stages; no HP regen between stages). (Source: GDD PDF, “Side Quests,” “Emergency Missions,” “Ninja Road.”)

**Unit progression**
- Leveling; awakening at milestone levels requiring rare materials; limit break +1 to +5 using rare crystals; weapon mastery unlocking permanent buffs/finishers. (Source: GDD PDF, “Character Progression Paths.”)

**Gear progression**
- Weapons/armor/accessories enhanced up to +15; gear tier evolution (common → relic) with rare relic gear providing unique modifiers/VFX. (Source: GDD PDF, “Gear Progression.”)

### Currencies and economy

**Premium currency**
- Spirit Seals used for summoning and optional energy refills. (Source: GDD PDF, “Summoning Currency,” “Energy refill.”)

**Rewards**
- Battle rewards include EXP, currency, random loot (weapons/armor/scrolls), shards for awakening or gacha pulls. (Source: GDD PDF, “Battle Rewards.”)

**Daily/weekly mission economy**
- Missions grant EXP, Spirit Seals, materials, raid tokens; daily completion target ~15 minutes. (Source: GDD PDF, “Daily retention loops,” “Daily & Weekly Missions.”)

### Events, banners, and battle pass

**Summoning banners**
- Standard, Event (rate-up), Festival (exclusive) banners. (Source: GDD PDF, “Banner types.”)

**Battle pass**
- “Samurai Path” 28-day season; free + premium tracks; premium includes cosmetics, tickets, extra currency, slightly better material rates. (Source: GDD PDF, “Seasonal Battle Pass.”)

**LiveOps cadence**
- Monthly new playable units; new raid bosses every 2–3 weeks; story side chapters monthly; arena reward resets biweekly seasons; seasonal cosmetics per event cycle. (Source: GDD PDF, “Monthly Content Release Model.”)

**LiveOps tool plans**
- Dashboards for banner schedule automation, raid tuning, event tracking, reward logs, server health monitoring. (Source: GDD PDF, “LiveOps Internal Tools.”)

### Monetization strategy and compliance

**GDD monetization commitment**
- “Ethical monetization,” cosmetic-first where possible, avoid P2W for PvP; disclose pull rates and avoid predatory tactics. (Source: GDD PDF, “Monetization Philosophy,” “Ethical Commitment.”)

**Gacha specifics**
- Example rates: 60/30/8/2; pity after 90 pulls without Tier 4. (Source: GDD PDF, “Example Pull Rates,” “Pity system.”)

**Compliance: odds disclosure**
- Apple’s App Review Guidelines explicitly require that apps offering loot boxes/randomized virtual items for purchase disclose odds prior to purchase. citeturn1search3  
This is directly relevant to Shogun’s gacha proposal and should be treated as a ship-blocking compliance requirement.

**Monetization options (recommended decision frame)**
- **Option A: Full F2P live service (GDD-aligned)**  
  Requires backend + LiveOps + anti-cheat + analytics. Highest upside, highest operational risk. (Anchor examples: long-running mobile SRPGs; store listings show loot box categorization and subscriptions). citeturn8search0turn9search1  
- **Option B: Premium tactics (scope-reducing)**  
  A proven mobile publishing path exists for premium tactics ports and mobile releases that explicitly state “no in-app purchases.” citeturn9search2turn9search5turn8search1  
- **Option C: Hybrid (limited F2P + one-time unlock)**  
  **[INFERENCE]** This can preserve accessibility and monetization while reducing LiveOps complexity, but would require careful economy design to avoid “paywall shock.”

**Operational risk evidence (shutdown caution)**
- Official end-of-service notices for major tactical gacha titles show that even well-known publishers discontinue service when sustaining “satisfactory app experience” becomes difficult. citeturn0search1turn11view0

---

## UX/UI specification

### Navigation flows

The GDD defines a “no guesswork” navigation philosophy and outlines flows for:
- battle modes, hero management, summoning, clan, shop, settings. (Source: GDD PDF, “UI/UX Full Flow Diagrams.”)

The official project page highlights that the prototype is “mobile-first” and oriented to “readable grid-based decision making” and turn pacing clarity. citeturn2view0

**Recommended IA principles (consistent with GDD)**
- Keep “Time to Battle” low (tap budget rule).  
- Ensure every mode has clear: entry cost → goal → reward preview.  
- Make the initiative system readable and teach it early through tutorial battles.

### Combat HUD

The GDD specifies combat HUD layout regions:
- Top-left: turn order timeline; top-center: battle timer; top-right: pause/menu; bottom: squad portraits with HP + skill indicators; bottom-right: gesture input prompt; bottom-left: quick emotes for co-op; overlays for skill names and status icons. (Source: GDD PDF, “In-Combat UI Layout.”)

The official page’s “Current Focus” initiative timeline matches this HUD design. citeturn2view0

### Accessibility and device scalability

GDD accessibility includes:
- colorblind mode, font scaling, simplified effects toggle, large icon mode for gesture cues. (Source: GDD PDF, “Accessibility Features.”)

**Recommended additional accessibility checks**
- gesture alternatives for players who cannot trace  
- haptic intensity toggle  
- “hold vs tap” sensitivity calibration.

---

## Art, audio, tech, production, and appendices

### Art direction and pipelines

**Art direction**
- “Haunting elegance”: serene traditional aesthetics + demonic dread; brushstroke-inspired layered 2D animation with mobile performance constraints. (Source: GDD PDF, “Art Direction Philosophy.”)

**Living companion note:** for the current production-facing interpretation of this art direction, use the paired internal notes rather than trying to overload this GDD with every evolving decision:

- `docs/art/art-001-style-bible-and-visual-targets.md`
- `docs/design/design-001-character-collection-and-fantasy-strategy.md`
- `docs/art/art-006-sex-appeal-and-damage-art-policy.md`
- `docs/design/design-002-world-pillars-and-combat-identity-framework.md`

**Official visual references (from the creator’s site)**
- Banner artwork and prototype screenshots are hosted on the official site. citeturn13view0turn13view1turn13view2  

**Character pipeline**
- hand-drawn model → line art → color render → rigging prep → expression sets; skeletal template consistency. (Source: GDD PDF, “Character Art Pipeline.”)

**Enemies and environments**
- biomes include forest ruins, snow shrine, burning village, misty mountain, cursed battlefield with blood moon/ash particles; destructible props included. (Source: GDD PDF, “Environment Art Pipeline.”)

**VFX**
- rule: never overwhelm battlefield clarity; color-coded elemental VFX; gesture-aligned effects. (Source: GDD PDF, “Battle VFX Design,” “Skill Gesture FX.”)

### Audio plan

The GDD calls for:
- original orchestral soundtrack with traditional instruments (shakuhachi, shamisen, taiko) and dynamic layering by combat state; weapon clashes, environment ambiences, demon howls; Japanese VO for story scenes with optional English subtitles; battle shouts for units/bosses. (Source: GDD PDF, “Audio Design Pipeline.”)

### Technical stack and build targets

**Engine/platform**
- Built in Unity; mobile iOS/Android; portrait, single-hand operation. citeturn2view0  
- The creator’s background emphasizes Unity (C#), QA/live-ops grounding, and player-centered delivery. citeturn5view0

**Build status (public)**
- Prototype validates turn order and position-based combat loops; initiative timeline in progress; enemy AI next. citeturn2view0

**CI/CD (recommended; not specified in sources)**
- **[INFERENCE]** Use a trunk-based Git workflow with automated build + smoke tests:
  - Android: Gradle build validation + device smoke test farm  
  - iOS: archived builds + TestFlight channel  
  - nightly “content bundle” build (for data-only events if LiveOps pursued)

### Backend requirements and telemetry

The GDD’s LiveOps tool plan implies persistent services (banner automation, reward logs, server monitoring). (Source: GDD PDF, “LiveOps Internal Tools.”)

**Backend capability map (recommended)**
- Auth/account binding  
- Inventory + currency ledger (auditable, fraud-resistant)  
- Gacha banner config + odds disclosure UI + pity tracking  
- Remote config (events, balancing knobs, UI experiments)  
- Matchmaking layer (co-op, PvP async ranking)  
- Leaderboards + seasonal resets  
- Customer support hooks (IDs, purchase receipts, refunds where required)  
- Anti-cheat heuristics (especially for PvP)

**Telemetry events (recommended baseline)**
- tutorial completion steps  
- turn duration, undo/cancel actions, mis-taps  
- gesture success/fail by gesture type + marker difficulty  
- initiative reorder events (what caused them)  
- enemy AI reason codes  
- economy sinks/sources per session  
- crash + ANR + device performance profiling

### Risk matrix and mitigations

| Risk | Why it matters | Mitigation (concrete) | Sources anchored |
|---|---|---|---|
| Live-service scope vs solo capacity | Co-op, PvP, clans, LiveOps tools require sustained ops | Ship “combat vertical slice” first; treat online systems as Phase 2; prioritize instrumented playtests | citeturn2view0turn5view2 |
| Monetization backlash / compliance failure | Loot-box odds disclosure is mandatory on iOS; gacha ethics scrutiny | Implement odds UI + pity transparency from day one; legal/compliance checklist before soft launch | citeturn1search3 |
| Power creep / complexity bloat | Long-lived gacha SRPGs often become text-heavy/meta-stale | Formal balance cadence; buff older units; avoid “must-pull” designs; keep skill text concise | (Secondary lesson docs; corroborate with long-running app store update cadence). citeturn8search0 |
| Technical debt / loading latency | Server round-trips can destroy UX over time | Cache, batch requests, offline-friendly menus where possible; performance budgets | (Secondary lesson doc) |
| Cultural misuse (kanji gestures, folklore) | Incorrect kanji or reductive myth depictions harms credibility | Native language review; frame as “inspired fantasy”; avoid claiming historical simulation | citeturn12search1turn12search0turn12search4 |

### Roadmap, milestones, and sprint tasks

The GDD provides a staged milestone plan (pre-production → vertical slice → core systems → multiplayer → monetization/tools → beta → soft launch). (Source: GDD PDF, “Development Milestone Schedule.”) The official page clarifies immediate next tasks (initiative bar, enemy AI). citeturn2view0

**Prioritized roadmap (sprint-ready; recommended)**

**Milestone: Combat loop closure (vertical slice foundation)**
- Initiative timeline UI complete & readable (portraits, reorder animation, hover detail). citeturn2view0  
- Enemy AI Tier 0 (deterministic) + logging reason codes. citeturn2view0  
- Win/lose + reward stub + unit XP progression. (Source alignment: GDD battle rewards + leveling.)

**Milestone: Gesture system validation**
- Implement tap/swipe/hold; instrument success/fail metrics. (Source: GDD gesture system.)  
- Prototype trace-shape as optional endgame action; add accessibility alternative. (Recommended.)

**Milestone: Encounter variety**
- Build 6–10 encounters using hazards/obstacles/destructibles; introduce 1 boss with telegraphed hazards. (Source alignment: environment systems.)  

**Milestone: Meta kernel**
- Minimal roster management + leveling/awakening stub + gear equip/upgrade stub. (Source alignment: progression sections.)

### QA and playtest protocols

The creator’s background highlights structured playtests and bug reporting discipline; the GDD includes retention/KPI targets and a closed beta phase. citeturn5view0turn2view0

**Test plan layers (recommended)**
- Determinism tests: same seed → same initiative and AI decisions  
- Gesture tests: latency, hitbox, device DPI scaling; failure-mode behavior  
- Balance tests: element advantage correct; status immunity/resistance correct  
- UX tests: “≤4 taps to battle” time-to-first-action, tutorial comprehension  
- Performance tests: frame pacing during VFX; low-end device settings using simplified effects

**Playtest protocol (recommended)**
- 10–15 participants in three cohorts: SRPG veterans, mobile gacha players, tactical newcomers  
- Collect: completion rate, time-to-win, deaths by cause, gesture fail rate, “confusion moments” by timestamp  
- Iterate weekly on: UI clarity, gesture calibration, initiative transparency

### AI prompt templates

These are templates to use with AI tools for rapid content generation; they assume the GDD’s tone and systems. Avoid including copyrighted characters or external IP.

```text
PROMPT: New playable hero concept (Shogun)
You are designing a new hero for a dark fantasy mobile tactical RPG set in myth-inspired feudal Japan.
Return JSON with:
- name (unique), clan/faction, archetype (Vanguard/Skirmisher/Marksman/Mystic/Protector)
- weapon type, element, rarity tier (Common/Rare/Super Rare/Festival)
- movement radius tiles, attack radius tiles
- basic attack (tap), skill (swipe), ultimate (hold), advanced (trace-shape) with clear timing windows
- passive synergy link triggers and one anti-synergy weakness
- status effects applied and counterplay (cleanse/resist)
- short lore blurb (grim, poetic, fatalistic)
Constraints:
- must not require pay-only acquisition
- skill text must fit within 240 characters each
```

```text
PROMPT: Encounter design block
Design a 3–5 minute tactical encounter teaching one mechanic.
Return:
- map layout (ASCII grid), obstacles, hazards, destructibles
- enemy roster with elements and statuses
- objective and fail condition
- tutorial callouts (3 max)
- expected player strategy and 2 common failure modes
- telemetry events to log
```

```text
PROMPT: Localization sanity check for kanji trace
Given a proposed kanji (or kana) for a trace-shape ultimate:
- validate meaning, commonness, stroke order plausibility
- flag if inappropriate, archaic, offensive, or misleading
- propose an alternative that matches intended meaning
Output should include “confidence” and a note: “Get native review before shipping.”
```

### Legal/IP checklist and cultural guidance

**Trademarks (title/branding)**
- The entity["organization","U.S. Patent and Trademark Office","us federal agency"] provides trademark basics and emphasizes searching for conflicting marks and understanding scope of protection. citeturn1search0  
**Recommended steps (practical):**
1) Run clearance search (USPTO database + app store search + Steam) for “Shogun: Flowers Fall in Blood” and close variants. citeturn1search0  
2) Audit visual identity (logo) for similarity to existing brands.  
3) Decide: register mark early (if commercial launch planned) vs defer (prototype stage).

**Odds disclosure**
- If randomized paid items exist (loot boxes/gacha), disclose odds before purchase per Apple policy. citeturn1search3  

**Cultural references**
- “Shogun” has a specific historical meaning as a Japanese military ruler title; use as flavor is fine, but avoid implying historical simulation unless researched. citeturn12search0  
- “Yomi” is an underworld concept in Shinto myth as recorded in ancient sources; treat as myth-inspired fantasy with respect, not as a literal depiction of Shinto practice. citeturn12search1  
- Use native review for kanji/shrines/ritual phrasing; avoid sacred-symbol misuse and “ornamental writing” errors.

---

## Asset inventory and direct links

This section provides **direct links** and copy-paste embed snippets. (URLs are placed inside code blocks for portability.)

**Official project hub and supporting pages** (source for public claims and asset discovery). citeturn2view0turn5view0turn6view0  

```text
Project page (primary)
https://www.tsvetanski.com/projects/shogun-flowers-fall-in-blood

Developer About page
https://www.tsvetanski.com/about

Related projects (background credibility)
https://tsvetanski.com/shinobi-story/
https://www.tsvetanski.com/projects/repo-x
https://www.tsvetanski.com/projects/trash-been
```

**Prototype and reference videos** (linked on the project page). citeturn2view0turn13view3turn14view0  

```text
Prototype video (Movement System Prototyping)
https://youtu.be/mTIhaiYbRDk
Thumbnail (used for embed previews)
https://img.youtube.com/vi/mTIhaiYbRDk/maxresdefault.jpg

Taikou Risshiden V reference video
https://youtu.be/3DBdaVHeVnY
Thumbnail
https://img.youtube.com/vi/3DBdaVHeVnY/maxresdefault.jpg
```

**Official images hosted on tsvetanski.com** (banner, prototype screenshot, research showcase). citeturn13view0turn13view1turn13view2  

```text
Banner image
https://www.tsvetanski.com/_next/image?q=75&url=%2Fimages%2FShogunFlowersFallinBlood_banner.png&w=3840

Prototype progress screenshot
https://www.tsvetanski.com/_next/image?q=75&url=%2Fimages%2FShogunFlowersFallinBlood_WhereIAmAtRightNow_Gameplay.png&w=3840

Systems research showcase image
https://www.tsvetanski.com/_next/image?q=75&url=%2Fimages%2FShogunFlowersFallinBlood_HW_NarutoBlazing_Showcase_Example.png&w=3840
```

**Embed snippets (copy into Markdown)**
```markdown
![Shogun banner](https://www.tsvetanski.com/_next/image?q=75&url=%2Fimages%2FShogunFlowersFallinBlood_banner.png&w=3840)

[Watch prototype video](https://youtu.be/mTIhaiYbRDk)
![Prototype thumbnail](https://img.youtube.com/vi/mTIhaiYbRDk/maxresdefault.jpg)
```

---

## Comparable games table (market framing)

This table is provided as a competitive/reference map for mechanics, art direction, platforms, and live status; sources are official store listings or official notices when possible.

| Title | Mechanics overlap | Art style signal | Platforms | Release status evidence |
|---|---|---|---|---|
| **entity["video_game","Fire Emblem Heroes","nintendo mobile srpg"]** | Small-map SRPG; touch-native controls (drag/swipe), auto-battle, summoning currency | Anime/fantasy franchise | iOS | Active App Store listing with frequent updates and “Contains Loot Boxes.” citeturn8search0 |
| **entity["video_game","Langrisser","mobile srpg zilong"]** | Tactical battles + class upgrades; real-time PvP; guild systems | Anime/JRPG | iOS | Active App Store listing and official launch announcement for iOS/Android. citeturn9search1turn7search2 |
| **entity["video_game","Sword of Convallaria","tactical rpg xd"]** | Grid tactics + narrative branching; roster collection; PC+mobile | “NeoPixel” modern pixel aesthetic | PC (Steam) | Steam page shows release date and F2P framing. citeturn7search0 |
| **entity["video_game","WAR OF THE VISIONS FINAL FANTASY BRAVE EXVIUS","square enix tactical gacha"]** | Tactical gacha SRPG; long-form LiveOps model | FF-branded 3D | Mobile | Official end-of-service notice (global) provides EOS date and rationale. citeturn0search1 |
| **entity["video_game","DRAGON QUEST TACT","square enix mobile tactics"]** | Mobile tactical battles; F2P service model | Dragon Quest franchise art | Mobile | Official Square Enix Bridge announcement shows end-of-service schedule for English version. citeturn11view0 |
| **entity["video_game","FINAL FANTASY TACTICS :WotL","square enix mobile premium srpg"]** | Premium tactics SRPG; serves as “depth benchmark” vs LiveOps model | Classic SRPG visuals | iOS | Paid iOS listing remains available. citeturn8search1 |
| **entity["video_game","XCOM 2 Collection","feral interactive mobile port"]** | Premium tactical campaign; touch UI; no gacha | Realistic sci-fi | iOS | iOS listing explicitly notes “no in-app purchases.” citeturn9search2 |

**Interpretation for Shogun (actionable):**
- If Shogun ships as LiveOps gacha, its closest operational peers include WotV and DQT—both of which show documented shutdown risk in global markets via official notices. citeturn0search1turn11view0  
- If Shogun ships as premium tactics, XCOM 2 Collection and FFT:WotL demonstrate viable “no IAP / paid” mobile expectations and messaging. citeturn9search2turn8search1  

---

## Developer background and credibility context

The official site identifies the developer as entity["people","Georgi Tsvetanski","game developer"], describing a “T-shaped developer” profile bridging engineering and human factors, rapid prototyping in Unity (C#), and grounding in QA/live-ops. citeturn5view0

The portfolio cites live-ops and community pipeline contributions on entity["video_game","Shinobi Story","custom naruto mmorpg"] (self-reported metrics and timeline on the project page), which is relevant experience if Shogun pursues event cadence and service operations. citeturn6view0

---

## Closing synthesis

Shogun: Flowers Fall in Blood is best understood as:
- a **combat-readability prototype** focused on “range circles + initiative forecasting,” publicly progressing toward enemy AI closure, citeturn2view0  
- paired with a GDD that fully sketches a **multi-year live-service SRPG** with ethical monetization commitments and extensive systems design (Source: GDD PDF),  
- with meaningful external constraints: odds disclosure compliance for loot boxes (Apple), and real market evidence that tactical gacha services can and do end operations even under major publishers. citeturn1search3turn0search1turn11view0
