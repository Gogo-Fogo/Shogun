using System.Collections.Generic;
using UnityEngine;
using Shogun.Core.Architecture;

namespace Shogun.Features.Characters
{
    /// <summary>
    /// Factory class for creating and managing character instances.
    /// Provides utility methods for character creation and validation.
    /// </summary>
    public static class CharacterFactory
    {
        private static Dictionary<string, CharacterDefinition> _characterDefinitions = new Dictionary<string, CharacterDefinition>();
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Initialize the factory by loading all character definitions.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;
            
            LoadCharacterDefinitions();
            _isInitialized = true;
        }
        
        /// <summary>
        /// Load all character definitions from Resources.
        /// </summary>
        private static void LoadCharacterDefinitions()
        {
            CharacterDefinition[] definitions = Resources.LoadAll<CharacterDefinition>("Characters");
            
            foreach (CharacterDefinition definition in definitions)
            {
                if (definition != null && !string.IsNullOrEmpty(definition.CharacterName))
                {
                    _characterDefinitions[definition.CharacterName] = definition;
                }
            }
            
            Debug.Log($"CharacterFactory: Loaded {_characterDefinitions.Count} character definitions");
        }
        
        /// <summary>
        /// Create a character instance from a definition name.
        /// </summary>
        public static CharacterInstance CreateCharacter(string characterName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            if (_characterDefinitions.TryGetValue(characterName, out CharacterDefinition definition))
            {
                return new CharacterInstance(definition);
            }
            
            Debug.LogError($"CharacterFactory: Character definition '{characterName}' not found");
            return null;
        }
        
        /// <summary>
        /// Create a character instance from a definition.
        /// </summary>
        public static CharacterInstance CreateCharacter(CharacterDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogError("CharacterFactory: Cannot create character from null definition");
                return null;
            }
            
            return new CharacterInstance(definition);
        }
        
        /// <summary>
        /// Create a character instance with a specific level.
        /// </summary>
        public static CharacterInstance CreateCharacter(string characterName, int level)
        {
            CharacterInstance instance = CreateCharacter(characterName);
            if (instance != null)
            {
                // Add experience to reach the desired level
                float experienceNeeded = CalculateExperienceForLevel(level);
                instance.Stats.AddExperience(experienceNeeded);
            }
            return instance;
        }
        
        /// <summary>
        /// Get all available character definitions.
        /// </summary>
        public static CharacterDefinition[] GetAllCharacterDefinitions()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            CharacterDefinition[] definitions = new CharacterDefinition[_characterDefinitions.Count];
            _characterDefinitions.Values.CopyTo(definitions, 0);
            return definitions;
        }
        
        /// <summary>
        /// Get character definitions by type.
        /// </summary>
        public static CharacterDefinition[] GetCharacterDefinitionsByType(CharacterType type)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            List<CharacterDefinition> filtered = new List<CharacterDefinition>();
            
            foreach (CharacterDefinition definition in _characterDefinitions.Values)
            {
                if (definition.CharacterType == type)
                {
                    filtered.Add(definition);
                }
            }
            
            return filtered.ToArray();
        }
        
        /// <summary>
        /// Get character definitions by rarity.
        /// </summary>
        public static CharacterDefinition[] GetCharacterDefinitionsByRarity(Rarity rarity)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            List<CharacterDefinition> filtered = new List<CharacterDefinition>();
            
            foreach (CharacterDefinition definition in _characterDefinitions.Values)
            {
                if (definition.Rarity == rarity)
                {
                    filtered.Add(definition);
                }
            }
            
            return filtered.ToArray();
        }
        
        /// <summary>
        /// Get character definitions by elemental type.
        /// </summary>
        public static CharacterDefinition[] GetCharacterDefinitionsByElement(ElementalType element)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            List<CharacterDefinition> filtered = new List<CharacterDefinition>();
            
            foreach (CharacterDefinition definition in _characterDefinitions.Values)
            {
                if (definition.ElementalType == element)
                {
                    filtered.Add(definition);
                }
            }
            
            return filtered.ToArray();
        }
        
        /// <summary>
        /// Check if a character definition exists.
        /// </summary>
        public static bool HasCharacterDefinition(string characterName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            return _characterDefinitions.ContainsKey(characterName);
        }
        
        /// <summary>
        /// Get a character definition by name.
        /// </summary>
        public static CharacterDefinition GetCharacterDefinition(string characterName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            _characterDefinitions.TryGetValue(characterName, out CharacterDefinition definition);
            return definition;
        }
        
        /// <summary>
        /// Calculate the experience needed to reach a specific level.
        /// </summary>
        public static float CalculateExperienceForLevel(int targetLevel)
        {
            if (targetLevel <= 1) return 0f;
            
            float totalExperience = 0f;
            float baseExp = 100f;
            
            for (int level = 1; level < targetLevel; level++)
            {
                totalExperience += baseExp * Mathf.Pow(level, 1.5f);
            }
            
            return totalExperience;
        }
        
        /// <summary>
        /// Create a balanced team of characters for testing.
        /// </summary>
        public static CharacterInstance[] CreateBalancedTeam(int teamSize = 3)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            List<CharacterInstance> team = new List<CharacterInstance>();
            CharacterDefinition[] allDefinitions = GetAllCharacterDefinitions();
            
            if (allDefinitions.Length == 0)
            {
                Debug.LogWarning("CharacterFactory: No character definitions found for team creation");
                return team.ToArray();
            }
            
            // Try to create a balanced team with different types and elements
            HashSet<CharacterType> usedTypes = new HashSet<CharacterType>();
            HashSet<ElementalType> usedElements = new HashSet<ElementalType>();
            
            for (int i = 0; i < teamSize; i++)
            {
                CharacterDefinition bestChoice = null;
                int bestScore = -1;
                
                foreach (CharacterDefinition definition in allDefinitions)
                {
                    int score = 0;
                    
                    // Prefer different character types
                    if (!usedTypes.Contains(definition.CharacterType))
                        score += 10;
                    
                    // Prefer different elements
                    if (!usedElements.Contains(definition.ElementalType))
                        score += 5;
                    
                    // Prefer higher rarity
                    score += (int)definition.Rarity;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestChoice = definition;
                    }
                }
                
                if (bestChoice != null)
                {
                    team.Add(CreateCharacter(bestChoice));
                    usedTypes.Add(bestChoice.CharacterType);
                    usedElements.Add(bestChoice.ElementalType);
                }
            }
            
            return team.ToArray();
        }
        
        /// <summary>
        /// Validate a character instance.
        /// </summary>
        public static bool ValidateCharacter(CharacterInstance character)
        {
            if (character == null) return false;
            if (character.Definition == null) return false;
            if (character.Stats == null) return false;
            if (character.CurrentHealth <= 0 && character.IsAlive) return false;
            if (character.CurrentHealth > character.MaxHealth) return false;
            
            return true;
        }
        
        /// <summary>
        /// Get character statistics for debugging.
        /// </summary>
        public static string GetCharacterStatistics()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            Dictionary<CharacterType, int> typeCounts = new Dictionary<CharacterType, int>();
            Dictionary<ElementalType, int> elementCounts = new Dictionary<ElementalType, int>();
            Dictionary<Rarity, int> rarityCounts = new Dictionary<Rarity, int>();
            
            foreach (CharacterDefinition definition in _characterDefinitions.Values)
            {
                // Count by type
                if (!typeCounts.ContainsKey(definition.CharacterType))
                    typeCounts[definition.CharacterType] = 0;
                typeCounts[definition.CharacterType]++;
                
                // Count by element
                if (!elementCounts.ContainsKey(definition.ElementalType))
                    elementCounts[definition.ElementalType] = 0;
                elementCounts[definition.ElementalType]++;
                
                // Count by rarity
                if (!rarityCounts.ContainsKey(definition.Rarity))
                    rarityCounts[definition.Rarity] = 0;
                rarityCounts[definition.Rarity]++;
            }
            
            string stats = $"Character Statistics:\n";
            stats += $"Total Characters: {_characterDefinitions.Count}\n\n";
            
            stats += "By Type:\n";
            foreach (var kvp in typeCounts)
            {
                stats += $"  {kvp.Key}: {kvp.Value}\n";
            }
            
            stats += "\nBy Element:\n";
            foreach (var kvp in elementCounts)
            {
                stats += $"  {kvp.Key}: {kvp.Value}\n";
            }
            
            stats += "\nBy Rarity:\n";
            foreach (var kvp in rarityCounts)
            {
                stats += $"  {kvp.Key}: {kvp.Value}\n";
            }
            
            return stats;
        }
    }
} 