using System;
using UnityEngine;
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
        [SerializeField] private int specialAbilityCooldown = 0;
        
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
        
        // Events
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<Vector2Int> OnPositionChanged;
        public event Action<StatusEffect> OnStatusEffectAdded;
        public event Action<StatusEffect> OnStatusEffectRemoved;
        public event Action<bool> OnStealthChanged;
        public event Action<float> OnCounterAttack;
        
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
        public bool CanUseSpecialAbility => isAlive && specialAbilityCooldown <= 0;
        public int SpecialAbilityCooldown => specialAbilityCooldown;
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
        /// Move the character to a new position with smooth running animation.
        /// </summary>
        public void MoveTo(Vector2 screenPosition)
        {
            // Stop any existing movement and ensure animation is reset
            if (currentMovementCoroutine != null)
            {
                StopCoroutine(currentMovementCoroutine);
                var anim = GetComponent<Animator>();
                if (anim != null) anim.SetBool("isRunning", false);
            }
            
            currentMovementCoroutine = StartCoroutine(MoveToRoutine(screenPosition));
        }

        private System.Collections.IEnumerator MoveToRoutine(Vector2 screenPosition)
        {
            Vector3 start = transform.position;
            Vector3 target = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));
            target.z = 0;
            float speed = 25f; // Increased from 15f to 25f for faster tap-to-move
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
        /// Perform a Blazing-style basic attack: randomly triggers ATTACK 1, 2, or 3 animation.
        /// </summary>
        public void PerformBasicAttack()
        {
            if (!CanAttack) return;
            hasAttackedThisTurn = true;
            // Randomly pick one of the three attack animations
            string[] attackStates = { "ATTACK 1", "ATTACK 2", "ATTACK 3" };
            string chosen = attackStates[UnityEngine.Random.Range(0, attackStates.Length)];
            var anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("AttackTrigger");
                // Optionally, set a parameter to indicate which attack (if you want to use sub-states)
                // anim.SetInteger("AttackIndex", Array.IndexOf(attackStates, chosen));
            }
            // TODO: Apply attack logic/effects here
        }

        /// <summary>
        /// Perform the first special ability (e.g., healing jutsu).
        /// </summary>
        public void PerformJutsu()
        {
            if (!CanUseSpecialAbility) return;
            specialAbilityCooldown = definition.SpecialAbilityCooldown;
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
            if (!CanUseSpecialAbility) return;
            specialAbilityCooldown = definition.SpecialAbilityCooldown * 2; // Example: ult has longer cooldown
            var anim = GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("SpecialTrigger");
            // TODO: Apply ultimate logic/effects here
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
            
            // Reduce cooldowns
            if (specialAbilityCooldown > 0)
            {
                specialAbilityCooldown--;
            }
            
            // Process status effects
            ProcessStatusEffects();
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
            // Set sprite
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && def.BattleSprite != null)
                sr.sprite = def.BattleSprite;
            // Set animator
            var anim = GetComponent<Animator>();
            if (anim != null && def.AnimatorController != null)
                anim.runtimeAnimatorController = def.AnimatorController;
            // Set collider size/offset if present in def
            var col = GetComponent<CapsuleCollider2D>();
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
            var anim = GetComponent<Animator>();
            if (anim == null || definition == null || definition.animationMappings == null || definition.animationMappings.Count == 0)
                return;

            var baseController = anim.runtimeAnimatorController;
            if (baseController == null)
                return;

            var overrideController = new AnimatorOverrideController(baseController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            // Build a dictionary for quick lookup
            var mappingDict = new Dictionary<string, AnimationClip>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var mapping in definition.animationMappings)
            {
                if (!string.IsNullOrEmpty(mapping.logicalName) && mapping.clip != null)
                    mappingDict[mapping.logicalName] = mapping.clip;
            }

            // For each clip in the base controller, try to override
            foreach (var clip in overrideController.animationClips)
            {
                if (mappingDict.TryGetValue(clip.name, out var mappedClip))
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