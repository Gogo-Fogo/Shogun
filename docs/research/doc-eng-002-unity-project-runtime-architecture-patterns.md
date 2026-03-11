# Unity Project Runtime Architecture Patterns

**Document intent:** This is a canonical internal engineering note for the Unity-side code architecture that should govern the Shogun project runtime. Use it alongside `DOC-ENG-001`: `DOC-ENG-001` covers platform, backend, content delivery, and production planning; this document covers in-project code boundaries, combat runtime patterns, and input flow.

**Source note:** Distilled from the legacy architecture blueprint previously stored in `docs/old/Unity RPG Folder Architecture_.txt`, reduced to the guidance that remains useful for active development.

---

## When to use this doc

Open this document when the question is about:

- Unity folder boundaries and module ownership
- assembly definition boundaries and dependency direction
- ScriptableObject data definitions vs runtime instances
- event-driven communication inside the client
- combat state flow and interruption handling
- gesture input recognition and dispatch
- logic/presentation separation for single-player and future multiplayer reuse

Use `DOC-ENG-001` first when the question is about:

- mobile build requirements
- backend vendors or LiveOps infrastructure
- Addressables hosting and release pipeline
- analytics, compliance, or store requirements

---

## Recommended Unity structure

Use a feature-oriented project layout so that assets and scripts for one gameplay domain stay co-located.

```text
Assets/
  _Project/
    _Core/
      Runtime/
      Editor/
      Tests/
    _Client/
      Runtime/
        Bootstrap/
        UI/
        Input/
        Presentation/
      Editor/
      Tests/
    _Net/
      Runtime/
        Api/
        Auth/
        Sync/
        Telemetry/
    _Content/
      ScriptableObjects/
      Art/
      Audio/
  _ThirdParty/
  _Sandbox/
```

Practical rules:

- `_Core` contains deterministic gameplay logic and shared domain types.
- `_Client` contains Unity presentation code, scene orchestration, UI, animation, and local input adapters.
- `_Net` contains transport, auth, sync, and telemetry integrations.
- `_Content` contains authored data assets and referenced content, not gameplay orchestration code.
- `_ThirdParty` isolates vendor packages and imported dependencies.
- `_Sandbox` is for experiments only and must not become a production dependency.

---

## Assembly boundaries

Use `asmdef` files to enforce dependency direction and reduce compile cost.

Required rules:

- low-level assemblies must not depend on high-level feature assemblies
- `_Core` must not depend on `_Client`
- presentation code may depend on runtime logic, but runtime logic must not depend on presentation
- networking adapters may depend on core domain types, not the reverse
- tests should reference the narrowest assembly possible

Recommended baseline:

```text
Shogun.Core
Shogun.Client
Shogun.Net
Shogun.Features.Characters
Shogun.Features.Combat
Shogun.Features.UI
```

If a dependency feels circular, the fix is usually to move shared contracts into `_Core`, not to add a bidirectional reference.

---

## Data model: definitions vs instances

Treat authored game content as immutable definitions and runtime gameplay state as separate instance objects.

Pattern:

- CharacterDefinition ScriptableObject: static identity, authored stats, visuals, ability references, tags
- `CharacterInstance` runtime object: current HP, cooldowns, status effects, turn flags, temporary modifiers
- AbilityDefinition ScriptableObject: targeting rules, authored effect list, UI metadata, charge requirements, animation hooks
- AbilityCatalog ScriptableObject: lookup and validation index of authored ability assets for tooling and runtime discovery
- `EncounterDefinition` ScriptableObject: spawn data, hazards, scripted encounter parameters

This separation keeps balancing data editable in the Inspector while preventing authored assets from becoming mutable runtime state containers.

---

## Event-driven client communication

Use ScriptableObject event channels for cross-system communication where direct references would otherwise create brittle coupling.

Good uses:

- combat result notifications
- UI refresh triggers
- scene transitions
- input intent events
- toast/banner notifications

Do not use event channels to hide ownership of core state mutation. State changes should still originate from a clear owner, usually a manager, service, or state object in `_Core`.

---

## Combat state management

Use a stack-based combat state machine for turn flow, interruptions, and gesture-driven sub-states.

Why this matters:

- a plain finite state machine becomes awkward when actions can be interrupted
- combat skills may temporarily enter a gesture-resolution state
- counter, reaction, or cinematic states should pause and then resume their parent state cleanly

Recommended pattern:

1. Push a root combat state such as `SelectActionState`.
2. Replace it with `PerformActionState` when the player commits an action.
3. Push `GestureQTEState` if the selected action requires gesture resolution.
4. Pop the gesture state when resolved and resume the parent action state.
5. Transition to follow-up states such as enemy turn, death resolution, or victory checks.

This is the default architecture for:

- turn order flow
- reaction windows
- skill execution sub-steps
- boss telegraph/interrupt timing
- later multiplayer synchronization of high-level intent

---

## Gesture input flow

Keep gesture recognition separate from gameplay consequence.

Preferred pipeline:

1. Input adapter reads raw touch or pointer data.
2. Gesture recognizer converts it into high-level intent such as tap, swipe direction, hold duration, or trace result.
3. Recognized intent is dispatched to the active consumer, usually the current combat state or a scoped input service.
4. Gameplay systems interpret that intent and decide whether it changes combat state or resolves a skill.

Rules:

- input code should not directly apply damage, move units, or mutate combat state
- gesture parsing should remain testable without scene dependencies where possible
- gesture thresholds and timing windows should be data-driven
- non-gesture fallback paths should remain possible for accessibility-sensitive actions

---

## Logic and presentation separation

The combat simulation should remain valid even if you strip away animations, particles, and scene objects.

That means:

- logic owns rules, validation, sequencing, and numeric outcomes
- presentation owns animation, audio, VFX, portrait updates, and camera behavior
- views subscribe to state changes or explicit presentation events
- managers in `_Client` should translate runtime state into visuals, not invent game outcomes

This separation is required for:

- reliable tests
- headless or semi-headless battle simulation
- replay tools
- easier future migration to authoritative multiplayer or remote simulation

---

## Practical implementation checklist

- Create or maintain `asmdef` boundaries before feature count grows.
- Keep ScriptableObjects as definitions, not live battle state.
- Route client-wide notifications through event channels only where decoupling pays off.
- Build combat flow on a state stack, not a flat enum switch.
- Keep input recognition and combat resolution in separate layers.
- Keep battle rules executable without presentation objects.
- Prefer adding small, composable effect objects to skills over hardcoding one-off skill branches.

---

## Authority order

When engineering docs disagree:

1. `DOC-GDD-001` defines product intent.
2. `DOC-ENG-001` defines platform, backend, and delivery constraints.
3. This document defines Unity-side runtime architecture patterns for implementing that intent.
4. Reference docs inform inspiration only.

## Implementation note: current Shogun terminology

- Current production code uses AbilityDefinition rather than SkillDefinition for authored combat moves.
- AbilityCatalog is the lookup/index asset for authored ability definitions.
