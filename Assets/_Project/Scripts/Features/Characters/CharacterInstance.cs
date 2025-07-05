using System;
using UnityEngine;
using Shogun.Core.Architecture;

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
    [Serializable]
    public class CharacterInstance
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
        /// Move the character to a new position.
        /// </summary>
        public void MoveTo(Vector2Int newPosition)
        {
            if (!CanMove) return;
            
            position = newPosition;
            hasMovedThisTurn = true;
            
            // Moving breaks stealth
            if (isHidden)
            {
                SetHidden(false);
            }
            
            OnPositionChanged?.Invoke(position);
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
        /// Perform an attack.
        /// </summary>
        public void PerformAttack()
        {
            if (!CanAttack) return;
            
            hasAttackedThisTurn = true;
            
            // Attacking breaks stealth
            if (isHidden)
            {
                SetHidden(false);
            }
        }
        
        /// <summary>
        /// Use special ability.
        /// </summary>
        public void UseSpecialAbility()
        {
            if (!CanUseSpecialAbility) return;
            
            specialAbilityCooldown = definition.SpecialAbilityCooldown;
            
            // Using special ability breaks stealth
            if (isHidden)
            {
                SetHidden(false);
            }
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