using UnityEngine;

namespace Shogun.Features.Characters
{
    /// <summary>
    /// ScriptableObject that defines the base data for a character (stats, element, martial arts type, abilities, etc.).
    /// Used as a template for creating CharacterInstance objects at runtime.
    /// Editable in the Unity Editor for easy content creation and balancing.
    /// </summary>
    [CreateAssetMenu(fileName = "New Character Definition", menuName = "Shogun/Characters/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string characterName = "Unknown";
        [SerializeField] private string description = "";
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite battleSprite;
        
        [Header("Character Type")]
        [SerializeField] private CharacterType characterType = CharacterType.Samurai;
        [SerializeField] private ElementalType elementalType = ElementalType.Earth;
        [SerializeField] private MartialArtsType martialArtsType = MartialArtsType.Sword;
        [SerializeField] private Rarity rarity = Rarity.Common;
        
        [Header("Combat Properties")]
        [SerializeField] private AttackRange attackRange = AttackRange.Mid;
        [SerializeField] private float baseHealth = 100f;
        [SerializeField] private float baseAttack = 50f;
        [SerializeField] private float baseDefense = 30f;
        [SerializeField] private float baseSpeed = 10f;
        
        [Header("Special Abilities")]
        [SerializeField] private string specialAbilityName = "";
        [SerializeField] private string specialAbilityDescription = "";
        [SerializeField] private int specialAbilityCooldown = 3;
        [SerializeField] private float specialAbilityDamage = 100f;
        
        [Header("Stealth Properties")]
        [SerializeField] private float baseStealthEffectiveness = 50f;
        [SerializeField] private float stealthDecayRate = 20f;
        
        [Header("Counter Properties")]
        [SerializeField] private float baseCounterChance = 25f;
        [SerializeField] private float counterDamageMultiplier = 100f;
        
        [Header("Team Synergy")]
        [SerializeField] private string[] synergyTags = new string[0];
        
        // Public properties
        public string CharacterName => characterName;
        public string Description => description;
        public Sprite Portrait => portrait;
        public Sprite BattleSprite => battleSprite;
        public CharacterType CharacterType => characterType;
        public ElementalType ElementalType => elementalType;
        public MartialArtsType MartialArtsType => martialArtsType;
        public Rarity Rarity => rarity;
        public AttackRange AttackRange => attackRange;
        public float BaseHealth => baseHealth;
        public float BaseAttack => baseAttack;
        public float BaseDefense => baseDefense;
        public float BaseSpeed => baseSpeed;
        public string SpecialAbilityName => specialAbilityName;
        public string SpecialAbilityDescription => specialAbilityDescription;
        public int SpecialAbilityCooldown => specialAbilityCooldown;
        public float SpecialAbilityDamage => specialAbilityDamage;
        public float BaseStealthEffectiveness => baseStealthEffectiveness;
        public float StealthDecayRate => stealthDecayRate;
        public float BaseCounterChance => baseCounterChance;
        public float CounterDamageMultiplier => counterDamageMultiplier;
        public string[] SynergyTags => synergyTags;
        
        /// <summary>
        /// Creates a new CharacterInstance based on this definition.
        /// </summary>
        public CharacterInstance CreateInstance()
        {
            return new CharacterInstance(this);
        }
    }
    
    /// <summary>
    /// Character types that define their role and appearance.
    /// </summary>
    public enum CharacterType
    {
        Samurai,
        Ninja,
        Onmyoji,
        Monk,
        Ronin,
        Yokai,
        Demon,
        Animal
    }
    
    /// <summary>
    /// Elemental types that affect combat effectiveness.
    /// Inspired by Genshin Impact's elemental system.
    /// </summary>
    public enum ElementalType
    {
        Fire,       // Aggressive, high damage, chance to inflict Burn
        Water,      // Adaptable, cleansing abilities, resistance to Fire
        Earth,      // Stability, defense boosts, reduces physical damage
        Wind,       // Speed, evasion boosts, increased movement
        Lightning,  // Fast attacks, chain damage, stun effects
        Ice,        // Control, freeze effects, slows enemies
        Shadow      // Corruption, drains HP, causes debuffs
    }
    
    /// <summary>
    /// Martial arts types that define combat style and abilities.
    /// </summary>
    public enum MartialArtsType
    {
        Unarmed,        // Monk: High speed, low damage, stun effects
        Sword,          // Samurai: Balanced, can parry, good vs armored
        Spear,          // Polearm: Long range, can attack over allies
        Bow,            // Archer: Very long range, can't be countered at range
        Staff,          // Onmyoji: Magic focus, elemental boost, healing
        DualDaggers,    // Ninja: Very high speed, stealth, can attack twice
        HeavyWeapons    // Demon: Low speed, very high damage, breaks armor
    }
    
    /// <summary>
    /// Character rarity that affects stats and availability.
    /// </summary>
    public enum Rarity
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5
    }
    
    /// <summary>
    /// Attack range types that define how far a character can attack.
    /// </summary>
    public enum AttackRange
    {
        Short,  // Small circle - Close combat specialists
        Mid,    // Medium circle - Balanced fighters
        Long    // Large circle - Ranged attackers
    }
} 