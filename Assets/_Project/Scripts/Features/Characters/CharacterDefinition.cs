using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [SerializeField] private string characterId = "";
        [SerializeField] private string[] aliases = new string[0];
        [SerializeField] private string surname = "";
        [SerializeField] private string givenName = "Unknown";
        [SerializeField] private string characterName = "Unknown"; // Deprecated, use surname/givenName
        [SerializeField] private string description = "";
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite bannerSprite;
        [SerializeField] private Sprite battleSprite;
        [SerializeField] private Sprite eventVignette;
        [Tooltip("Face close-up for the battle HUD squad rail medallion. Square format. Falls back to portrait if null.")]
        [SerializeField] private Sprite pfpSprite;
        [Tooltip("Combat combo cut-in art. Use a narrow attack-pose face/eye crop, not the HUD pfp.")]
        [SerializeField] private Sprite comboCutInSprite;
        [Tooltip("Combat ultimate cut-in art used for the second ability / ultimate presentation.")]
        [SerializeField] private Sprite ultimateCutInSprite;
        [SerializeField] private RuntimeAnimatorController animatorController;
        
        [Header("Prefab/Collider Settings")]
        [SerializeField] private Vector2 colliderSize = new Vector2(1, 2);
        [SerializeField] private Vector2 colliderOffset = Vector2.zero;
        [SerializeField] private Vector3 characterScale = Vector3.one;
        [SerializeField] private bool invertFacingX = false;

        [Header("Visual Identity")]
        [Tooltip("Palette signature used for cursor hover highlight, selection outline, portrait accents, and identity surfaces. One controlled accent per character — see DESIGN-001 and DESIGN-002.")]
        [SerializeField] private Color paletteAccentColor = Color.white;
        
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
        [SerializeField] private AbilityDefinition specialAbilityDefinition;
        [SerializeField] private AbilityDefinition ultimateAbilityDefinition;
        [SerializeField] private string specialAbilityName = "";
        [SerializeField] private string specialAbilityDescription = "";
        [SerializeField] private int specialAbilityCooldown = 3;
        [SerializeField] private int specialAbilityChargeRequirement = 0;
        [SerializeField] private int ultimateAbilityChargeRequirement = 0;
        [SerializeField] private float specialAbilityDamage = 100f;
        
        [Header("Stealth Properties")]
        [SerializeField] private float baseStealthEffectiveness = 50f;
        [SerializeField] private float stealthDecayRate = 20f;
        
        [Header("Counter Properties")]
        [SerializeField] private float baseCounterChance = 25f;
        [SerializeField] private float counterDamageMultiplier = 100f;
        
        [Header("Team Synergy")]
        [SerializeField] private string[] synergyTags = new string[0];

        [Header("Identity & Lore")]
        [Tooltip("One of the five world-level power centres this character belongs to. See DESIGN-002 for full world-pillar descriptions.")]
        [SerializeField] private WorldPillar worldPillar = WorldPillar.RoninMarches;
        [Tooltip("Fighting-style school beyond the weapon family. See DESIGN-002. Note: MartialArtsType currently conflates weapon + school — this field carries the deeper school identity until MartialArtsType is split.")]
        [SerializeField] private MartialSchool martialSchool = MartialSchool.CourtBladeDoctrine;
        [Tooltip("The collectible-fantasy pillar this unit is built to trigger in players. See DESIGN-001 for pillar descriptions.")]
        [SerializeField] private CollectibleFantasy collectibleFantasy = CollectibleFantasy.RogueViolence;
        [Tooltip("One-word emotional tone: tragic, noble, cruel, serene, feral, devotional, seductive, etc.")]
        [SerializeField] private string collectibleTone = "";
        [Tooltip("What makes this character identifiable at a glance — costume piece, weapon, aura, or marking. E.g. 'crescent helm', 'fox mask', 'prayer beads'.")]
        [SerializeField] private string visualHook = "";
        [Tooltip("The character's emotional anchor. E.g. grief, hunger, arrogance, tenderness, obsession, duty, corruption.")]
        [SerializeField] private string emotionalHook = "";
        [Tooltip("Short grim/poetic lore blurb shown player-facing (banner screen, profile). Different from 'description' which is dev notes.")]
        [SerializeField] [TextArea(2, 4)] private string loreBlurb = "";
        [Tooltip("Future costume/alt potential notes for content planning. E.g. 'corrupted form, ceremonial attire, festival variant'.")]
        [SerializeField] private string variantPotential = "";

        [Header("Animation Mapping")]
        public List<AnimationMapping> animationMappings = new List<AnimationMapping>();
        
        // Public properties
        public string CharacterId => string.IsNullOrWhiteSpace(characterId)
            ? CharacterKeyUtility.NormalizeCharacterId(GetIdentitySeed())
            : characterId;
        public IReadOnlyList<string> Aliases => aliases ?? System.Array.Empty<string>();
        public bool HasExplicitCharacterId => !string.IsNullOrWhiteSpace(characterId);
        public string Surname => surname;
        public string GivenName => givenName;
        public string DisplayNameJP => string.IsNullOrEmpty(surname) ? givenName : $"{surname} {givenName}";
        public string DisplayNameEN => string.IsNullOrEmpty(surname) ? givenName : $"{givenName} {surname}";
        public string CharacterName => string.IsNullOrEmpty(givenName) ? characterName : givenName; // Legacy fallback
        public string Description => description;
        public Sprite Portrait => portrait;
        public Sprite BannerSprite => bannerSprite;
        public Sprite BattleSprite => battleSprite;
        public Sprite EventVignette => eventVignette;
        /// <summary>Face close-up for the battle HUD medallion. Falls back to Portrait if null.</summary>
        public Sprite PfpSprite => pfpSprite != null ? pfpSprite : portrait;
        /// <summary>Combat combo cut-in art. Falls back to banner art, then portrait.</summary>
        public Sprite ComboCutInSprite => comboCutInSprite != null ? comboCutInSprite : (bannerSprite != null ? bannerSprite : portrait);
        /// <summary>Ultimate cut-in art. Falls back to banner art, then portrait.</summary>
        public Sprite UltimateCutInSprite => ultimateCutInSprite != null ? ultimateCutInSprite : (bannerSprite != null ? bannerSprite : portrait);
        public RuntimeAnimatorController AnimatorController => animatorController;
        public CharacterType CharacterType => characterType;
        public ElementalType ElementalType => elementalType;
        public MartialArtsType MartialArtsType => martialArtsType;
        public Rarity Rarity => rarity;
        public AttackRange AttackRange => attackRange;
        public float BaseHealth => baseHealth;
        public float BaseAttack => baseAttack;
        public float BaseDefense => baseDefense;
        public float BaseSpeed => baseSpeed;
        public AbilityDefinition SpecialAbilityDefinition => specialAbilityDefinition;
        public AbilityDefinition UltimateAbilityDefinition => ultimateAbilityDefinition;
        public string SpecialAbilityName => specialAbilityDefinition != null && !string.IsNullOrWhiteSpace(specialAbilityDefinition.DisplayName) ? specialAbilityDefinition.DisplayName : specialAbilityName;
        public string SpecialAbilityDescription => specialAbilityDefinition != null && !string.IsNullOrWhiteSpace(specialAbilityDefinition.Description) ? specialAbilityDefinition.Description : specialAbilityDescription;
        public string UltimateAbilityName => ultimateAbilityDefinition != null && !string.IsNullOrWhiteSpace(ultimateAbilityDefinition.DisplayName) ? ultimateAbilityDefinition.DisplayName : SpecialAbilityName;
        public string UltimateAbilityDescription => ultimateAbilityDefinition != null && !string.IsNullOrWhiteSpace(ultimateAbilityDefinition.Description) ? ultimateAbilityDefinition.Description : SpecialAbilityDescription;
        public int SpecialAbilityChargeRequirement => specialAbilityDefinition != null ? Mathf.Max(1, specialAbilityDefinition.ChargeRequirement) : Mathf.Max(1, specialAbilityChargeRequirement > 0 ? specialAbilityChargeRequirement : specialAbilityCooldown);
        public int UltimateAbilityChargeRequirement => ultimateAbilityDefinition != null ? Mathf.Max(SpecialAbilityChargeRequirement, ultimateAbilityDefinition.ChargeRequirement) : Mathf.Max(SpecialAbilityChargeRequirement, ultimateAbilityChargeRequirement > 0 ? ultimateAbilityChargeRequirement : (SpecialAbilityChargeRequirement * 2));
        public int SpecialAbilityCooldown => SpecialAbilityChargeRequirement;
        public float SpecialAbilityDamage => specialAbilityDefinition != null && specialAbilityDefinition.PowerValue > 0f ? specialAbilityDefinition.PowerValue : specialAbilityDamage;
        public float UltimateAbilityDamage => ultimateAbilityDefinition != null && ultimateAbilityDefinition.PowerValue > 0f ? ultimateAbilityDefinition.PowerValue : SpecialAbilityDamage;
        public float BaseStealthEffectiveness => baseStealthEffectiveness;
        public float StealthDecayRate => stealthDecayRate;
        public float BaseCounterChance => baseCounterChance;
        public float CounterDamageMultiplier => counterDamageMultiplier;
        public string[] SynergyTags => synergyTags;
        public Vector2 ColliderSize => colliderSize;
        public Vector2 ColliderOffset => colliderOffset;
        public Vector3 CharacterScale => characterScale;
        public bool InvertFacingX => invertFacingX;
        public Color PaletteAccentColor => paletteAccentColor;
        public WorldPillar WorldPillar => worldPillar;
        public MartialSchool MartialSchool => martialSchool;
        public CollectibleFantasy CollectibleFantasy => collectibleFantasy;
        public string CollectibleTone => collectibleTone;
        public string VisualHook => visualHook;
        public string EmotionalHook => emotionalHook;
        public string LoreBlurb => loreBlurb;
        public string VariantPotential => variantPotential;
        
        /// <summary>
        /// Creates a new CharacterInstance based on this definition.
        /// </summary>
        public CharacterInstance CreateInstance()
        {
            return new CharacterInstance(this);
        }

        public IEnumerable<string> GetLookupTerms()
        {
            yield return CharacterId;

            if (aliases != null)
            {
                foreach (string alias in aliases)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                        yield return alias;
                }
            }

            if (!string.IsNullOrWhiteSpace(givenName))
                yield return givenName;

            if (!string.IsNullOrWhiteSpace(characterName))
                yield return characterName;

            if (!string.IsNullOrWhiteSpace(DisplayNameEN))
                yield return DisplayNameEN;

            if (!string.IsNullOrWhiteSpace(DisplayNameJP))
                yield return DisplayNameJP;

            string assetName = name.Replace("_CharacterDefinition", string.Empty);
            if (!string.IsNullOrWhiteSpace(assetName))
                yield return assetName;
        }

        private string GetIdentitySeed()
        {
            if (!string.IsNullOrWhiteSpace(givenName) && !givenName.Equals("Unknown"))
                return givenName;

            if (!string.IsNullOrWhiteSpace(characterName) && !characterName.Equals("Unknown"))
                return characterName;

            string assetName = name.Replace("_CharacterDefinition", string.Empty);
            return string.IsNullOrWhiteSpace(assetName) ? "character" : assetName;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            characterId = CharacterKeyUtility.NormalizeCharacterId(string.IsNullOrWhiteSpace(characterId) ? GetIdentitySeed() : characterId);
            aliases = CharacterKeyUtility.NormalizeAliases(aliases);
            specialAbilityCooldown = Mathf.Max(1, specialAbilityCooldown);
            if (specialAbilityChargeRequirement > 0)
                specialAbilityChargeRequirement = Mathf.Max(1, specialAbilityChargeRequirement);
            if (ultimateAbilityChargeRequirement > 0)
                ultimateAbilityChargeRequirement = Mathf.Max(specialAbilityChargeRequirement > 0 ? specialAbilityChargeRequirement : specialAbilityCooldown, ultimateAbilityChargeRequirement);
        }
#endif
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
    /// Inspired by the five-element philosophy of feudal Japan (Go-gyō / The Book of Five Rings).
    /// Common natures (Fire–Lightning) are widely trainable; exceptional natures (Ice, Shadow)
    /// are rarer, lineage-bound, or corruption-linked. See DESIGN-002.
    /// </summary>
    public enum ElementalType
    {
        Fire,       // Aggressive, high damage, chance to inflict Burn
        Water,      // Adaptable, cleansing abilities, resistance to Fire
        Earth,      // Stability, defense boosts, reduces physical damage
        Wind,       // Speed, evasion boosts, increased movement
        Lightning,  // Fast attacks, chain damage, stun effects
        Ice,        // Control, freeze effects, slows enemies        — Exceptional nature
        Shadow      // Corruption, drains HP, causes debuffs          — Exceptional nature
    }

    /// <summary>
    /// Weapon family — governs tactical matchup, range expectation, target profile, and silhouette.
    /// NOTE: This enum currently carries both weapon identity AND school/style identity.
    /// Per DESIGN-002 the long-term plan is to split this into WeaponType + MartialSchool.
    /// The new MartialSchool enum (see below) now carries the deeper school identity.
    /// </summary>
    public enum MartialArtsType
    {
        Unarmed,        // High speed, low damage, stun/control effects
        Sword,          // Balanced, can parry, good vs armoured — Short to Mid range
        Spear,          // Polearm: Long reach, formation pressure — Mid to Long range
        Bow,            // Very long range, vulnerable when engaged — Long range
        Staff,          // Magic focus, elemental boost, healing, irregular skill shapes — Mid to Long
        DualDaggers,    // Very high speed, stealth, flanker/assassin — Short range
        HeavyWeapons    // Low speed, very high damage, breaks armour — Short range
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

    /// <summary>
    /// The five world-level power centres characters belong to. See DESIGN-002.
    /// Governs faction, aesthetic lane, and emotional lane — not gameplay stats directly.
    /// </summary>
    public enum WorldPillar
    {
        ImperialCourt,          // Nobles, ceremonial samurai, court retainers, official onmyoji — elegance/hierarchy/tragedy
        RoninMarches,           // Exiles, mercenaries, duelists, outlaw bands, wandering killers — survival/brutality/freedom
        TempleAndVeilOrders,    // Monks, shrine maidens, exorcists, mediums, ritual guardians — discipline/devotion/purity vs corruption
        YokaiCourts,            // Fox nobles, serpent houses, spirit aristocracy, dream beings — seduction/mystery/elegance/danger
        CorruptedDominion       // Demon generals, cursed warlords, Yomi-touched champions — domination/terror/forbidden power
    }

    /// <summary>
    /// Fighting-style school — carries deeper identity beyond the weapon family.
    /// Governs stance, motion language, passive bonuses, counter style, and school rivalry. See DESIGN-002.
    /// </summary>
    public enum MartialSchool
    {
        // Native / Central schools
        CourtBladeDoctrine,     // Formal sword discipline, precision, posture, restraint — Imperial/samurai identity
        IronMountainSchool,     // Hard striking, body conditioning, brutal endurance — heavy/frontline pressure
        BindingHandSchool,      // Grappling, throws, control, counters — bodyguard/arrest energy
        VeilStepMethod,         // Stealth, infiltration, sudden kill windows — ninja/shadow identity
        // Peripheral / Outsider / Border schools
        SouthernSerpentSchool,  // Compact, hard-soft, close-range pressure — outsider/compact fighters
        PeninsulaKickingSchool, // Leg-dominant, mobility-heavy, outsider-coded — fast skirmishers
        ContinentalFlowSchool   // Circular movement, deceptive rhythm, animal or scholar-warrior flavour — casters/mystics
    }

    /// <summary>
    /// The collectible-fantasy pillar the character is designed to trigger in players. See DESIGN-001.
    /// Governs banner positioning, emotional hook, and variant/costume potential.
    /// </summary>
    public enum CollectibleFantasy
    {
        TragicNobility,     // Elegant samurai, doomed heirs, shrine maidens under burden, grief-coded beauty
        RogueViolence,      // Ronin, assassins, outcasts, executioners, scarred antiheroes
        YokaiElegance,      // Supernatural appeal mixing beauty with threat — fox nobles, spirit courts
        CorruptedPower,     // Demonic warlords, cursed commanders, possession motifs, body-horror elites
        MysticRitual        // Onmyoji, monks, mediums, fox-priests, occult tacticians
    }

    [System.Serializable]
    public class AnimationMapping
    {
        [Header("Animation Action")]
        public string logicalName; // e.g., "Idle", "Run", "Attack1"
        
        [Header("Animation Clip")]
        public AnimationClip clip; // The actual AnimationClip for this action
        
        [Header("Auto-Assignment")]
        [SerializeField] private bool useAutoAssignment = true;
        [SerializeField] private string customSearchPattern = ""; // e.g., "Ryoma_Attack1" or "Attack1"
        
        // Validation and helper properties
        public bool IsValid => !string.IsNullOrEmpty(logicalName) && clip != null;
        public bool UseAutoAssignment => useAutoAssignment;
        public string SearchPattern => string.IsNullOrEmpty(customSearchPattern) ? logicalName : customSearchPattern;
        
        // Constructor for easy creation
        public AnimationMapping(string actionName, AnimationClip animationClip = null)
        {
            logicalName = actionName;
            clip = animationClip;
            useAutoAssignment = true;
            customSearchPattern = "";
        }
        
        // Auto-assign clip based on naming convention
        public bool TryAutoAssignClip(string characterName)
        {
            if (!useAutoAssignment) return false;
            
            // Try different naming patterns
            string[] searchPatterns = {
                $"{characterName}_{logicalName}",
                $"{characterName}{logicalName}",
                logicalName,
                SearchPattern
            };
            
            foreach (string pattern in searchPatterns)
            {
                AnimationClip foundClip = FindAnimationClipByName(pattern);
                if (foundClip != null)
                {
                    clip = foundClip;
                    return true;
                }
            }
            
            return false;
        }
        
        private AnimationClip FindAnimationClipByName(string clipName)
        {
#if UNITY_EDITOR
            // Search in common animation folders
            string[] searchPaths = {
                "Assets/_Project/Features/Characters/Art/Production/Animations"
            };
            
            foreach (string path in searchPaths)
            {
                string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip", new[] { path });
                if (guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                }
            }
#endif
            return null;
        }
    }
} 



