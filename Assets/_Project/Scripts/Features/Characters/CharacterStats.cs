using System;
using UnityEngine;

namespace Shogun.Features.Characters
{
    /// <summary>
    /// Encapsulates all character stats (health, attack, defense, level, experience, etc.) and stat progression logic.
    /// Used by CharacterInstance to manage stat changes, leveling, and calculations for combat.
    /// Provides methods for elemental and martial arts effectiveness, reactions, and modifiers.
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        [Header("Base Stats")]
        [SerializeField] private float health;
        [SerializeField] private float attack;
        [SerializeField] private float defense;
        [SerializeField] private float speed;
        
        [Header("Level Information")]
        [SerializeField] private int level = 1;
        [SerializeField] private int maxLevel = 100;
        [SerializeField] private float experience = 0f;
        [SerializeField] private float experienceToNextLevel = 100f;
        
        [Header("Stat Modifiers")]
        [SerializeField] private float healthModifier = 1f;
        [SerializeField] private float attackModifier = 1f;
        [SerializeField] private float defenseModifier = 1f;
        [SerializeField] private float speedModifier = 1f;
        
        // Events
        public event Action<int> OnLevelUp;
        public event Action<float> OnExperienceGained;
        
        // Public properties
        public float Health => health * healthModifier;
        public float Attack => attack * attackModifier;
        public float Defense => defense * defenseModifier;
        public float Speed => speed * speedModifier;
        public int Level => level;
        public int MaxLevel => maxLevel;
        public float Experience => experience;
        public float ExperienceToNextLevel => experienceToNextLevel;
        public float ExperienceProgress => experience / experienceToNextLevel;
        
        // Base stats (without modifiers)
        public float BaseHealth => health;
        public float BaseAttack => attack;
        public float BaseDefense => defense;
        public float BaseSpeed => speed;
        
        /// <summary>
        /// Initialize stats from a character definition.
        /// </summary>
        public void Initialize(CharacterDefinition definition)
        {
            health = definition.BaseHealth;
            attack = definition.BaseAttack;
            defense = definition.BaseDefense;
            speed = definition.BaseSpeed;
            
            CalculateExperienceToNextLevel();
        }
        
        /// <summary>
        /// Add experience and handle level ups.
        /// </summary>
        public void AddExperience(float amount)
        {
            if (level >= maxLevel) return;
            
            experience += amount;
            OnExperienceGained?.Invoke(amount);
            
            while (experience >= experienceToNextLevel && level < maxLevel)
            {
                LevelUp();
            }
        }
        
        /// <summary>
        /// Level up the character and increase stats.
        /// </summary>
        private void LevelUp()
        {
            experience -= experienceToNextLevel;
            level++;
            
            // Increase base stats by 10% per level
            float statIncrease = 1.1f;
            health *= statIncrease;
            attack *= statIncrease;
            defense *= statIncrease;
            speed *= statIncrease;
            
            CalculateExperienceToNextLevel();
            OnLevelUp?.Invoke(level);
        }
        
        /// <summary>
        /// Calculate experience required for next level.
        /// Uses exponential growth: base * (level ^ 1.5)
        /// </summary>
        private void CalculateExperienceToNextLevel()
        {
            if (level >= maxLevel)
            {
                experienceToNextLevel = float.MaxValue;
                return;
            }
            
            float baseExp = 100f;
            experienceToNextLevel = baseExp * Mathf.Pow(level, 1.5f);
        }
        
        /// <summary>
        /// Set stat modifiers (for equipment, buffs, etc.).
        /// </summary>
        public void SetStatModifiers(float healthMod, float attackMod, float defenseMod, float speedMod)
        {
            healthModifier = healthMod;
            attackModifier = attackMod;
            defenseModifier = defenseMod;
            speedModifier = speedMod;
        }
        
        /// <summary>
        /// Get the effectiveness multiplier against a target elemental type.
        /// Based on Genshin Impact's elemental system.
        /// </summary>
        public float GetElementalEffectiveness(ElementalType attackerType, ElementalType defenderType)
        {
            // Elemental advantage loop: Earth > Water > Fire > Wind > Lightning > Ice > Shadow > Earth
            if (attackerType == ElementalType.Earth && defenderType == ElementalType.Water) return 1.25f;
            if (attackerType == ElementalType.Water && defenderType == ElementalType.Fire) return 1.25f;
            if (attackerType == ElementalType.Fire && defenderType == ElementalType.Wind) return 1.25f;
            if (attackerType == ElementalType.Wind && defenderType == ElementalType.Lightning) return 1.25f;
            if (attackerType == ElementalType.Lightning && defenderType == ElementalType.Ice) return 1.25f;
            if (attackerType == ElementalType.Ice && defenderType == ElementalType.Shadow) return 1.25f;
            if (attackerType == ElementalType.Shadow && defenderType == ElementalType.Earth) return 1.25f;
            
            // Elemental disadvantages
            if (attackerType == ElementalType.Earth && defenderType == ElementalType.Shadow) return 0.75f;
            if (attackerType == ElementalType.Water && defenderType == ElementalType.Earth) return 0.75f;
            if (attackerType == ElementalType.Fire && defenderType == ElementalType.Water) return 0.75f;
            if (attackerType == ElementalType.Wind && defenderType == ElementalType.Fire) return 0.75f;
            if (attackerType == ElementalType.Lightning && defenderType == ElementalType.Wind) return 0.75f;
            if (attackerType == ElementalType.Ice && defenderType == ElementalType.Lightning) return 0.75f;
            if (attackerType == ElementalType.Shadow && defenderType == ElementalType.Ice) return 0.75f;
            
            // Neutral matchups (same type or no advantage)
            return 1.0f;
        }
        
        /// <summary>
        /// Get the effectiveness multiplier for martial arts matchups.
        /// </summary>
        public float GetMartialArtsEffectiveness(MartialArtsType attackerType, MartialArtsType defenderType)
        {
            // Martial arts advantages
            if (attackerType == MartialArtsType.Sword && defenderType == MartialArtsType.HeavyWeapons) return 1.25f;
            if (attackerType == MartialArtsType.Spear && defenderType == MartialArtsType.Bow) return 1.25f;
            if (attackerType == MartialArtsType.Unarmed && defenderType == MartialArtsType.DualDaggers) return 1.25f;
            if (attackerType == MartialArtsType.Staff && defenderType == MartialArtsType.Sword) return 1.25f;
            if (attackerType == MartialArtsType.HeavyWeapons && defenderType == MartialArtsType.Spear) return 1.25f;
            if (attackerType == MartialArtsType.Bow && defenderType == MartialArtsType.Staff) return 1.25f;
            if (attackerType == MartialArtsType.DualDaggers && defenderType == MartialArtsType.Unarmed) return 1.25f;
            
            // Martial arts disadvantages (reverse of advantages)
            if (attackerType == MartialArtsType.HeavyWeapons && defenderType == MartialArtsType.Sword) return 0.75f;
            if (attackerType == MartialArtsType.Bow && defenderType == MartialArtsType.Spear) return 0.75f;
            if (attackerType == MartialArtsType.DualDaggers && defenderType == MartialArtsType.Unarmed) return 0.75f;
            if (attackerType == MartialArtsType.Sword && defenderType == MartialArtsType.Staff) return 0.75f;
            if (attackerType == MartialArtsType.Spear && defenderType == MartialArtsType.HeavyWeapons) return 0.75f;
            if (attackerType == MartialArtsType.Staff && defenderType == MartialArtsType.Bow) return 0.75f;
            if (attackerType == MartialArtsType.Unarmed && defenderType == MartialArtsType.DualDaggers) return 0.75f;
            
            // Neutral matchups
            return 1.0f;
        }
        
        /// <summary>
        /// Calculate elemental reaction damage multiplier.
        /// </summary>
        public float GetElementalReactionMultiplier(ElementalType element1, ElementalType element2)
        {
            // Fire + Water = Vaporize
            if ((element1 == ElementalType.Fire && element2 == ElementalType.Water) ||
                (element1 == ElementalType.Water && element2 == ElementalType.Fire))
                return 1.5f;
            
            // Fire + Ice = Melt
            if ((element1 == ElementalType.Fire && element2 == ElementalType.Ice) ||
                (element1 == ElementalType.Ice && element2 == ElementalType.Fire))
                return 2.0f;
            
            // Lightning + Water = Electro-charged
            if ((element1 == ElementalType.Lightning && element2 == ElementalType.Water) ||
                (element1 == ElementalType.Water && element2 == ElementalType.Lightning))
                return 1.3f;
            
            // Wind + Any = Swirl
            if (element1 == ElementalType.Wind || element2 == ElementalType.Wind)
                return 1.2f;
            
            // Ice + Water = Freeze
            if ((element1 == ElementalType.Ice && element2 == ElementalType.Water) ||
                (element1 == ElementalType.Water && element2 == ElementalType.Ice))
                return 1.4f;
            
            // Shadow + Any = Corrupt
            if (element1 == ElementalType.Shadow || element2 == ElementalType.Shadow)
                return 1.1f;
            
            return 1.0f; // No reaction
        }
        
        /// <summary>
        /// Calculate damage dealt to a target.
        /// </summary>
        public float CalculateDamage(CharacterStats target, ElementalType attackerElement, ElementalType defenderElement, 
                                   MartialArtsType attackerMartialArts, MartialArtsType defenderMartialArts)
        {
            float elementalMultiplier = GetElementalEffectiveness(attackerElement, defenderElement);
            float martialArtsMultiplier = GetMartialArtsEffectiveness(attackerMartialArts, defenderMartialArts);
            float damage = Attack * elementalMultiplier * martialArtsMultiplier;
            
            // Apply defense reduction (defense reduces damage by a percentage)
            float defenseReduction = target.Defense / (target.Defense + 100f); // Diminishing returns
            damage *= (1f - defenseReduction);
            
            return Mathf.Max(1f, damage); // Minimum 1 damage
        }
        
        /// <summary>
        /// Get the attack range radius based on the AttackRange enum.
        /// </summary>
        public float GetAttackRangeRadius(AttackRange range)
        {
            switch (range)
            {
                case AttackRange.Short: return 1.5f;
                case AttackRange.Mid: return 3f;
                case AttackRange.Long: return 5f;
                default: return 3f;
            }
        }
        
        /// <summary>
        /// Get stealth effectiveness for a martial arts type.
        /// </summary>
        public float GetStealthEffectiveness(MartialArtsType martialArtsType, int turnsInBush)
        {
            float baseEffectiveness = 0f;
            
            switch (martialArtsType)
            {
                case MartialArtsType.DualDaggers: baseEffectiveness = 90f; break;
                case MartialArtsType.Unarmed: baseEffectiveness = 70f; break;
                case MartialArtsType.Bow: baseEffectiveness = 60f; break;
                case MartialArtsType.Sword: baseEffectiveness = 40f; break;
                case MartialArtsType.Staff: baseEffectiveness = 30f; break;
                case MartialArtsType.Spear: baseEffectiveness = 30f; break;
                case MartialArtsType.HeavyWeapons: baseEffectiveness = 10f; break;
            }
            
            // Apply diminishing returns
            if (turnsInBush == 1) return baseEffectiveness;
            if (turnsInBush == 2) return Mathf.Max(0f, baseEffectiveness - 20f);
            
            // Additional -10% for each turn after the second
            float additionalDecay = (turnsInBush - 2) * 10f;
            return Mathf.Max(0f, baseEffectiveness - 20f - additionalDecay);
        }
        
        /// <summary>
        /// Get counter chance for a martial arts type.
        /// </summary>
        public float GetCounterChance(MartialArtsType martialArtsType)
        {
            switch (martialArtsType)
            {
                case MartialArtsType.Sword: return 50f;
                case MartialArtsType.Unarmed: return 30f;
                case MartialArtsType.Spear: return 40f;
                case MartialArtsType.Bow: return 25f;
                case MartialArtsType.Staff: return 35f;
                case MartialArtsType.DualDaggers: return 15f; // Low counter, high dodge
                case MartialArtsType.HeavyWeapons: return 20f;
                default: return 25f;
            }
        }
        
        /// <summary>
        /// Get counter damage multiplier for a martial arts type.
        /// </summary>
        public float GetCounterDamageMultiplier(MartialArtsType martialArtsType)
        {
            switch (martialArtsType)
            {
                case MartialArtsType.Sword: return 100f; // Normal damage
                case MartialArtsType.Unarmed: return 150f;
                case MartialArtsType.Spear: return 120f;
                case MartialArtsType.Bow: return 110f;
                case MartialArtsType.Staff: return 130f;
                case MartialArtsType.DualDaggers: return 140f;
                case MartialArtsType.HeavyWeapons: return 200f;
                default: return 100f;
            }
        }
    }
} 