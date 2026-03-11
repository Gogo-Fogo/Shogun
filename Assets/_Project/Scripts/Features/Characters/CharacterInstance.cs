using System;
using UnityEngine;
using UnityEngine.Serialization;
using Shogun.Core.Architecture;
using System.Collections;
using System.Collections.Generic;

namespace Shogun.Features.Characters
{
    // CharacterInstance.cs
    // Represents a runtime instance of a character in battle, including health, status effects, position, and turn state.
    // Used by the combat system to track and manipulate character state during gameplay.
    // Interacts with CharacterDefinition, CharacterStats, and handles status effects, movement, and combat actions.

    /// <summary>
    /// Represents a runtime instance of a character with current state and stats.
    /// This is what gets used in battles and gameplay.
    /// </summary>
    public class CharacterInstance : MonoBehaviour
    {
        [Header("Character Data")]
        [SerializeField] private CharacterDefinition definition;
        [SerializeField] private CharacterStats stats;
        
        [Header("Battle State")]
        [SerializeField] private float currentHealth;
        [SerializeField] private bool isAlive = true;
        [SerializeField] private Vector2Int position = Vector2Int.zero;
        [SerializeField] private bool hasMovedThisTurn = false;
        [SerializeField] private bool hasAttackedThisTurn = false;
        [FormerlySerializedAs("specialAbilityCooldown")]
        [SerializeField] private int specialCharge = 0;
        
        [Header("Stealth State")]
        [SerializeField] private bool isHidden = false;
        [SerializeField] private int turnsInBush = 0;
        [SerializeField] private bool isInBush = false;
        
        [Header("Counter State")]
        [SerializeField] private bool canCounterAttack = true;
        [SerializeField] private float lastDamageTaken = 0f;
        
        [Header("Status Effects")]
        [SerializeField] private StatusEffect[] activeStatusEffects = new StatusEffect[0];

        // Movement tracking
        private Coroutine currentMovementCoroutine = null;
        private Coroutine attackRecoveryCoroutine = null;
        private const float AttackStateDetectTimeoutSeconds = 0.15f;
        private const float AttackRecoveryFallbackSeconds = 0.32f;
        private const float AttackRecoveryMaxSeconds = 1.10f;
        
        // Events
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<Vector2Int> OnPositionChanged;
        public event Action<StatusEffect> OnStatusEffectAdded;
        public event Action<StatusEffect> OnStatusEffectRemoved;
        public event Action<bool> OnStealthChanged;
        public event Action<float> OnCounterAttack;
        public event Action<int> OnSpecialChargeChanged;
        
        // Public properties
        public CharacterDefinition Definition => definition;
        public CharacterStats Stats => stats;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.Health;
        public float HealthPercentage => currentHealth / stats.Health;
        public bool IsAlive => isAlive;
        public Vector2Int Position => position;
        public bool HasMovedThisTurn => hasMovedThisTurn;
        public bool HasAttackedThisTurn => hasAttackedThisTurn;
        public bool CanMove => isAlive && !hasMovedThisTurn;
        public bool CanAttack => isAlive && !hasAttackedThisTurn;
        public int SpecialCharge => specialCharge;
        public int SpecialChargeRequirement => definition != null ? definition.SpecialAbilityChargeRequirement : 0;
        public int UltimateChargeRequirement => definition != null ? definition.UltimateAbilityChargeRequirement : SpecialChargeRequirement;
        public bool CanUseSpecialAbility => isAlive && specialCharge >= SpecialChargeRequirement;
        public bool CanUseUltimateAbility => isAlive && specialCharge >= UltimateChargeRequirement;
        public float SpecialChargeRatio => UltimateChargeRequirement > 0 ? Mathf.Clamp01(specialCharge / (float)UltimateChargeRequirement) : 0f;
        public float JutsuChargeRatio => SpecialChargeRequirement > 0 ? Mathf.Clamp01(specialCharge / (float)SpecialChargeRequirement) : 0f;
        public int SpecialAbilityCooldown => Mathf.Max(0, SpecialChargeRequirement - specialCharge);
        public StatusEffect[] ActiveStatusEffects => activeStatusEffects;
        
        // Stealth properties
        public bool IsHidden => isHidden;
        public bool IsInBush => isInBush;
        public int TurnsInBush => turnsInBush;
        public float StealthEffectiveness => stats.GetStealthEffectiveness(definition.MartialArtsType, turnsInBush);
        
        // Counter properties
        public bool CanCounterAttack => canCounterAttack;
        public float CounterChance => stats.GetCounterChance(definition.MartialArtsType);
        public float CounterDamageMultiplier => stats.GetCounterDamageMultiplier(definition.MartialArtsType);
        
        /// <summary>
        /// Create a new character instance from a definition.
        /// </summary>
        public CharacterInstance(CharacterDefinition characterDefinition)
        {
            definition = characterDefinition;
            stats = new CharacterStats();
            stats.Initialize(characterDefinition);
            
            // Subscribe to stat events
            stats.OnLevelUp += HandleLevelUp;
            
            // Initialize health
            currentHealth = stats.Health;
        }
        
        /// <summary>
        /// Take damage and handle death.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!isAlive) return;
            
            float oldHealth = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            lastDamageTaken = damage;
            
            OnHealthChanged?.Invoke(currentHealth);
            
            if (currentHealth <= 0f && oldHealth > 0f)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Heal the character.
        /// </summary>
        public void Heal(float amount)
        {
            if (!isAlive) return;
            
            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(stats.Health, currentHealth + amount);
            
            if (currentHealth != oldHealth)
            {
                OnHealthChanged?.Invoke(currentHealth);
            }
        }
        
        /// <summary>
        /// Handle character death.
        /// </summary>
        private void Die()
        {
            isAlive = false;
            isHidden = false; // Reveal on death
            OnDeath?.Invoke();
        }
        
        /// <summary>
        /// Stop any current movement and reset animation.
        /// </summary>
        public void StopMovement()
        {
            if (currentMovementCoroutine != null)
            {
                StopCoroutine(currentMovementCoroutine);
                currentMovementCoroutine = null;
                var anim = GetComponent<Animator>();
                if (anim != null) 
                {
                    anim.SetBool("isRunning", false);
                }
            }
        }

        /// <summary>
        /// Move the character to a new position with smooth running animation.
        /// </summary>
        public void MoveTo(Vector2 screenPosition)
        {
            if (!TryNormalizeScreenPosition(screenPosition, out Vector2 normalizedScreenPos))
                return;

            // Stop any existing movement and ensure animation is reset
            StopMovement();
            
            currentMovementCoroutine = StartCoroutine(MoveToRoutine(normalizedScreenPos));
        }

        private System.Collections.IEnumerator MoveToRoutine(Vector2 screenPosition)
        {
            Camera cameraRef = Camera.main;
            if (cameraRef == null)
            {
                currentMovementCoroutine = null;
                yield break;
            }

            Vector3 start = transform.position;
            Vector3 target = cameraRef.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(cameraRef.transform.position.z)));
            target.z = 0;
            float speed = 40f; // Increased for faster tap-to-move
            var anim = GetComponent<Animator>();
            if (anim != null) anim.SetBool("isRunning", true);
            
            // Keep running animation until we actually reach the target
            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }
            
            // Ensure we're exactly at the target
            transform.position = target;
            
            // Only stop running animation after we've reached the destination
            if (anim != null) 
            {
                anim.SetBool("isRunning", false);
                Debug.Log("Tap-to-move completed, stopped running animation");
            }
            
            // Clear the coroutine reference
            currentMovementCoroutine = null;
        }
        
        /// <summary>
        /// Enter a bush and attempt to hide.
        /// </summary>
        public void EnterBush(Vector2Int bushPosition)
        {
            if (!CanMove) return;
            
            position = bushPosition;
            hasMovedThisTurn = true;
            isInBush = true;
            turnsInBush++;
            
            // Attempt to hide based on stealth effectiveness
            float stealthChance = StealthEffectiveness / 100f;
            bool hideSuccessful = UnityEngine.Random.Range(0f, 1f) < stealthChance;
            
            SetHidden(hideSuccessful);
            OnPositionChanged?.Invoke(position);
        }
        
        /// <summary>
        /// Leave the current bush.
        /// </summary>
        public void LeaveBush()
        {
            if (!CanMove) return;
            
            isInBush = false;
            turnsInBush = 0;
            SetHidden(false);
        }
        
        /// <summary>
        /// Set hidden state.
        /// </summary>
        private void SetHidden(bool hidden)
        {
            if (isHidden != hidden)
            {
                isHidden = hidden;
                OnStealthChanged?.Invoke(hidden);
            }
        }
        
        /// <summary>
        /// Perform a Blazing-style basic attack animation.
        /// The first hit in a turn consumes the attack action; chained follow-up hits can opt out.
        /// </summary>
        public void PerformBasicAttack(bool consumeAttackAction = true)
        {
            if (!isAlive)
                return;

            if (consumeAttackAction)
            {
                if (!CanAttack)
                    return;

                hasAttackedThisTurn = true;
            }

            Animator anim = ResolveAnimator();
            if (anim != null)
            {
                anim.SetBool("isRunning", false);
                anim.ResetTrigger("AttackTrigger");
                anim.SetTrigger("AttackTrigger");

                if (attackRecoveryCoroutine != null)
                    StopCoroutine(attackRecoveryCoroutine);

                attackRecoveryCoroutine = StartCoroutine(RecoverToIdleAfterAttack(anim));
            }
        }
        private IEnumerator RecoverToIdleAfterAttack(Animator anim)
        {
            float detectElapsed = 0f;
            float waitSeconds = AttackRecoveryFallbackSeconds;

            while (detectElapsed < AttackStateDetectTimeoutSeconds)
            {
                if (anim == null || !anim.isActiveAndEnabled)
                {
                    attackRecoveryCoroutine = null;
                    yield break;
                }

                AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
                if (IsAttackState(state))
                {
                    waitSeconds = Mathf.Clamp(
                        Mathf.Max(AttackRecoveryFallbackSeconds, state.length + 0.02f),
                        AttackRecoveryFallbackSeconds,
                        AttackRecoveryMaxSeconds);
                    break;
                }

                detectElapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(waitSeconds);

            if (anim == null || !anim.isActiveAndEnabled || !isAlive)
            {
                attackRecoveryCoroutine = null;
                yield break;
            }

            ForceLocomotionState(anim);
            attackRecoveryCoroutine = null;
        }

        private void ForceLocomotionState(Animator anim)
        {
            if (anim == null)
                return;

            string desiredState = anim.GetBool("isRunning") ? "RUN" : "IDLE";
            if (TryPlayState(anim, desiredState))
                return;

            if (!string.Equals(desiredState, "IDLE", StringComparison.Ordinal) && TryPlayState(anim, "IDLE"))
                return;

            anim.Rebind();
            anim.Update(0f);
        }

        private static bool TryPlayState(Animator anim, string stateName)
        {
            if (anim == null || string.IsNullOrWhiteSpace(stateName))
                return false;

            int stateHash = Animator.StringToHash(stateName);
            if (anim.HasState(0, stateHash))
            {
                anim.Play(stateHash, 0, 0f);
                anim.Update(0f);
                return true;
            }

            int baseLayerStateHash = Animator.StringToHash($"Base Layer.{stateName}");
            if (anim.HasState(0, baseLayerStateHash))
            {
                anim.Play(baseLayerStateHash, 0, 0f);
                anim.Update(0f);
                return true;
            }

            return false;
        }

        private static bool IsAttackState(AnimatorStateInfo state)
        {
            return state.IsName("ATTACK 1")
                   || state.IsName("ATTACK 2")
                   || state.IsName("ATTACK 3")
                   || state.IsName("Base Layer.ATTACK 1")
                   || state.IsName("Base Layer.ATTACK 2")
                   || state.IsName("Base Layer.ATTACK 3");
        }

        /// <summary>
        /// Perform the first special ability (e.g., healing jutsu).
        /// </summary>
        public void PerformJutsu()
        {
            if (!CanUseSpecialAbility) return;
            SpendSpecialCharge(SpecialChargeRequirement);
            var anim = GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("isHealing");
            // TODO: Apply healing logic/effects here
        }

        /// <summary>
        /// Perform the ultimate (e.g., special attack, double-tap).
        /// </summary>
        public void PerformUltimate()
        {
            if (!CanUseUltimateAbility) return;
            SpendSpecialCharge(UltimateChargeRequirement);
            var anim = GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("SpecialTrigger");
            // TODO: Apply ultimate logic/effects here
        }

        public void GainSpecialCharge(int amount = 1)
        {
            if (!isAlive || amount <= 0)
                return;

            SetSpecialCharge(specialCharge + amount);
        }

        public void SpendSpecialCharge(int amount)
        {
            if (amount <= 0)
                return;

            SetSpecialCharge(specialCharge - amount);
        }

        private void SetSpecialCharge(int value)
        {
            int maxCharge = Mathf.Max(SpecialChargeRequirement, UltimateChargeRequirement);
            int clamped = Mathf.Clamp(value, 0, maxCharge);
            if (clamped == specialCharge)
                return;

            specialCharge = clamped;
            OnSpecialChargeChanged?.Invoke(specialCharge);
        }

        /// <summary>
        /// Attempt to counter-attack when taking damage.
        /// </summary>
        public bool AttemptCounterAttack()
        {
            if (!canCounterAttack || !isAlive) return false;
            
            float counterChance = CounterChance / 100f;
            bool counterSuccessful = UnityEngine.Random.Range(0f, 1f) < counterChance;
            
            if (counterSuccessful)
            {
                float counterDamage = lastDamageTaken * (CounterDamageMultiplier / 100f);
                OnCounterAttack?.Invoke(counterDamage);
                canCounterAttack = false; // Can only counter once per turn
            }
            
            return counterSuccessful;
        }
        
        /// <summary>
        /// Reset turn-based flags for the next turn.
        /// </summary>
        public void StartNewTurn()
        {
            hasMovedThisTurn = false;
            hasAttackedThisTurn = false;
            canCounterAttack = true;
            GainSpecialCharge(1);
            
            // Process status effects
            ProcessStatusEffects();
        }

        /// <summary>
        /// Makes a swapped-in reserve unit immediately usable for the current player turn
        /// without double-processing cooldowns or status effects.
        /// </summary>
        public void PrepareAsSwapInCurrentTurn()
        {
            hasMovedThisTurn = false;
            hasAttackedThisTurn = false;
            canCounterAttack = true;
        }
        
        /// <summary>
        /// Add a status effect to the character.
        /// </summary>
        public void AddStatusEffect(StatusEffect effect)
        {
            Array.Resize(ref activeStatusEffects, activeStatusEffects.Length + 1);
            activeStatusEffects[activeStatusEffects.Length - 1] = effect;
            OnStatusEffectAdded?.Invoke(effect);
        }
        
        /// <summary>
        /// Remove a status effect from the character.
        /// </summary>
        public void RemoveStatusEffect(StatusEffect effect)
        {
            for (int i = 0; i < activeStatusEffects.Length; i++)
            {
                if (activeStatusEffects[i] == effect)
                {
                    // Remove by shifting array
                    for (int j = i; j < activeStatusEffects.Length - 1; j++)
                    {
                        activeStatusEffects[j] = activeStatusEffects[j + 1];
                    }
                    Array.Resize(ref activeStatusEffects, activeStatusEffects.Length - 1);
                    OnStatusEffectRemoved?.Invoke(effect);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Process all active status effects.
        /// </summary>
        private void ProcessStatusEffects()
        {
            for (int i = activeStatusEffects.Length - 1; i >= 0; i--)
            {
                StatusEffect effect = activeStatusEffects[i];
                effect.ReduceDuration();
                
                if (effect.Duration <= 0)
                {
                    RemoveStatusEffect(effect);
                }
                else
                {
                    // Apply effect
                    ApplyStatusEffect(effect);
                }
            }
        }
        
        /// <summary>
        /// Apply a status effect's effects.
        /// </summary>
        private void ApplyStatusEffect(StatusEffect effect)
        {
            switch (effect.Type)
            {
                case StatusEffectType.Poison:
                    TakeDamage(effect.Value);
                    break;
                case StatusEffectType.Heal:
                    Heal(effect.Value);
                    break;
                case StatusEffectType.StatModifier:
                    // Stat modifiers are handled separately
                    break;
            }
        }
        
        /// <summary>
        /// Handle level up events from stats.
        /// </summary>
        private void HandleLevelUp(int newLevel)
        {
            // Heal to full when leveling up
            currentHealth = stats.Health;
            OnHealthChanged?.Invoke(currentHealth);
        }
        
        /// <summary>
        /// Check if this character can attack a target at the given position.
        /// </summary>
        public bool CanAttackTarget(Vector2Int targetPosition)
        {
            float distance = Vector2Int.Distance(position, targetPosition);
            float attackRange = stats.GetAttackRangeRadius(definition.AttackRange);
            return distance <= attackRange;
        }
        
        /// <summary>
        /// Get the attack range radius for this character.
        /// </summary>
        public float GetAttackRangeRadius()
        {
            return stats.GetAttackRangeRadius(definition.AttackRange);
        }
        
        /// <summary>
        /// Check if this character has a specific synergy tag.
        /// </summary>
        public bool HasSynergyTag(string tag)
        {
            return Array.Exists(definition.SynergyTags, t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Calculate damage against a target with all modifiers.
        /// </summary>
        public float CalculateDamageAgainst(CharacterInstance target)
        {
            return stats.CalculateDamage(
                target.Stats,
                definition.ElementalType,
                target.Definition.ElementalType,
                definition.MartialArtsType,
                target.Definition.MartialArtsType
            );
        }
        
        /// <summary>
        /// Get elemental reaction multiplier with another character.
        /// </summary>
        public float GetElementalReactionMultiplier(CharacterInstance other)
        {
            return stats.GetElementalReactionMultiplier(definition.ElementalType, other.Definition.ElementalType);
        }

        public void Initialize(CharacterDefinition def)
        {
            definition = def;

            if (stats == null)
            {
                stats = new CharacterStats();
            }

            stats.Initialize(def);
            currentHealth = stats.Health;
            isAlive = true;
            hasMovedThisTurn = false;
            hasAttackedThisTurn = false;
            specialCharge = 0;
            isHidden = false;
            turnsInBush = 0;
            isInBush = false;
            canCounterAttack = true;
            lastDamageTaken = 0f;
            activeStatusEffects = System.Array.Empty<StatusEffect>();

            gameObject.name = def.CharacterName;

            SpriteRenderer sr = ResolveSpriteRenderer();
            if (sr != null)
            {
                sr.sprite = def.BattleSprite;
            }

            Animator anim = ResolveAnimator();
            if (anim != null)
            {
                anim.runtimeAnimatorController = def.AnimatorController;
                SetupAnimatorOverrides(anim);
            }

            CapsuleCollider2D col = ResolveCapsuleCollider();
            if (col != null)
            {
                col.size = def.ColliderSize;
                col.offset = def.ColliderOffset;
            }

            // Set scale
            transform.localScale = def.CharacterScale;
            // Set other stats as needed (attack range, etc.)
            // this.attackRange = def.attackRange; // Uncomment if you have this field
        }

        private void Awake()
        {
            SetupAnimatorOverrides();
        }

        private void SetupAnimatorOverrides()
        {
            SetupAnimatorOverrides(ResolveAnimator());
        }

        private void SetupAnimatorOverrides(Animator anim)
        {
            if (anim == null || definition == null)
                return;

            RuntimeAnimatorController baseController = anim.runtimeAnimatorController ?? definition.AnimatorController;
            if (baseController == null)
                return;

            if (definition.animationMappings == null || definition.animationMappings.Count == 0)
            {
                anim.runtimeAnimatorController = baseController;
                anim.Rebind();
                anim.Update(0f);
                return;
            }

            var overrideController = new AnimatorOverrideController(baseController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            var seenBaseClips = new HashSet<AnimationClip>();

            // Build a dictionary for quick lookup
            var mappingDict = new Dictionary<string, AnimationClip>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var mapping in definition.animationMappings)
            {
                if (!string.IsNullOrEmpty(mapping.logicalName) && mapping.clip != null)
                {
                    string directKey = NormalizeAnimationKey(mapping.logicalName);
                    if (!mappingDict.ContainsKey(directKey))
                        mappingDict[directKey] = mapping.clip;
                }
            }

            // For each clip in the base controller, try to override
            foreach (var clip in overrideController.animationClips)
            {
                if (clip == null || !seenBaseClips.Add(clip))
                    continue;

                string normalizedClipName = NormalizeAnimationKey(clip.name);
                if (mappingDict.TryGetValue(normalizedClipName, out var mappedClip))
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, mappedClip));
                }
                else
                {
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, clip)); // fallback to original
                }
            }
            overrideController.ApplyOverrides(overrides);
            anim.runtimeAnimatorController = overrideController;
            anim.Rebind();
            anim.Update(0f);
        }

        private Animator ResolveAnimator()
        {
            return GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
        }

        private SpriteRenderer ResolveSpriteRenderer()
        {
            return GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);
        }

        private CapsuleCollider2D ResolveCapsuleCollider()
        {
            return GetComponent<CapsuleCollider2D>() ?? GetComponentInChildren<CapsuleCollider2D>(true);
        }

        private static string NormalizeAnimationKey(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return string.Empty;

            string candidate = rawName.Trim();
            int separatorIndex = candidate.IndexOf('_');
            if (separatorIndex >= 0 && separatorIndex < candidate.Length - 1)
                candidate = candidate.Substring(separatorIndex + 1);

            return candidate.Trim().ToUpperInvariant();
        }

        private static bool TryNormalizeScreenPosition(Vector2 rawScreenPos, out Vector2 normalizedScreenPos)
        {
            normalizedScreenPos = rawScreenPos;

            if (!IsFinite(rawScreenPos))
                return false;

            if (rawScreenPos.sqrMagnitude <= 0.0001f)
                return false;

            float minX = 0f;
            float minY = 0f;
            float maxX = Screen.width;
            float maxY = Screen.height;
            if (rawScreenPos.x < minX || rawScreenPos.y < minY || rawScreenPos.x > maxX || rawScreenPos.y > maxY)
                return false;

            normalizedScreenPos = new Vector2(
                Mathf.Clamp(rawScreenPos.x, minX, maxX),
                Mathf.Clamp(rawScreenPos.y, minY, maxY));
            return true;
        }

        private static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x)
                   && !float.IsNaN(value.y)
                   && !float.IsInfinity(value.x)
                   && !float.IsInfinity(value.y);
        }
    }
    
    /// <summary>
    /// Represents a status effect that can be applied to characters.
    /// </summary>
    [Serializable]
    public class StatusEffect
    {
        [SerializeField] private StatusEffectType type;
        [SerializeField] private float value;
        [SerializeField] private int duration;
        [SerializeField] private string description;
        
        public StatusEffectType Type => type;
        public float Value => value;
        public int Duration => duration;
        public string Description => description;
        
        public StatusEffect(StatusEffectType effectType, float effectValue, int effectDuration, string effectDescription = "")
        {
            type = effectType;
            value = effectValue;
            duration = effectDuration;
            description = effectDescription;
        }
        
        /// <summary>
        /// Reduce the duration of this status effect by one turn.
        /// </summary>
        public void ReduceDuration()
        {
            if (duration > 0)
            {
                duration--;
            }
        }
    }
    
    /// <summary>
    /// Types of status effects that can be applied to characters.
    /// </summary>
    public enum StatusEffectType
    {
        Poison,         // Damage over time
        Heal,           // Healing over time
        StatModifier,   // Temporary stat changes
        Stun,           // Cannot act
        Silence,        // Cannot use special abilities
        Burn,           // Fire damage over time
        Freeze,         // Movement restriction
        Bleed           // Physical damage over time
    }
} 
