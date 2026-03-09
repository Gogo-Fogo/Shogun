using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shogun.Features.Characters
{
    // CharacterFactory.cs
    // Static factory class for creating CharacterInstance objects from CharacterDefinition or by name.
    // Handles initialization, level setting, and provides utility methods for character management and testing.

    /// <summary>
    /// Factory class for creating and managing character instances.
    /// Provides utility methods for character creation and validation.
    /// </summary>
    public static class CharacterFactory
    {
        private static CharacterCatalog _characterCatalog;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Initialize the factory by loading the runtime character catalog.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;
            
            LoadCharacterDefinitions();
            _isInitialized = true;
        }
        
        /// <summary>
        /// Load the character catalog from Resources.
        /// </summary>
        private static void LoadCharacterDefinitions()
        {
            _characterCatalog = Resources.Load<CharacterCatalog>("CharacterCatalog");

            if (_characterCatalog == null)
            {
                Debug.LogError("CharacterFactory: CharacterCatalog could not be loaded from Resources/CharacterCatalog");
                return;
            }
            
            Debug.Log($"CharacterFactory: Loaded {_characterCatalog.GetAll().Count} character definitions from CharacterCatalog");
        }
        
        /// <summary>
        /// Create a character instance from a definition id or legacy lookup string.
        /// </summary>
        public static CharacterInstance CreateCharacter(string characterName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            CharacterDefinition definition = GetCharacterDefinition(characterName);
            if (definition != null)
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
                float experienceNeeded = CalculateExperienceForLevel(level);
                instance.Stats.AddExperience(experienceNeeded);
            }
            return instance;
        }

        public static CharacterDefinition GetCharacterDefinitionById(string characterId)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_characterCatalog != null && _characterCatalog.TryGetById(characterId, out CharacterDefinition definition))
                return definition;

            return null;
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
            
            if (_characterCatalog == null)
                return System.Array.Empty<CharacterDefinition>();

            return _characterCatalog.GetAll().ToArray();
        }
        
        /// <summary>
        /// Get character definitions by type.
        /// </summary>
        public static CharacterDefinition[] GetCharacterDefinitionsByType(CharacterType type)
        {
            return GetAllCharacterDefinitions()
                .Where(definition => definition.CharacterType == type)
                .ToArray();
        }
        
        /// <summary>
        /// Get character definitions by rarity.
        /// </summary>
        public static CharacterDefinition[] GetCharacterDefinitionsByRarity(Rarity rarity)
        {
            return GetAllCharacterDefinitions()
                .Where(definition => definition.Rarity == rarity)
                .ToArray();
        }
        
        /// <summary>
        /// Get character definitions by elemental type.
        /// </summary>
        public static CharacterDefinition[] GetCharacterDefinitionsByElement(ElementalType element)
        {
            return GetAllCharacterDefinitions()
                .Where(definition => definition.ElementalType == element)
                .ToArray();
        }
        
        /// <summary>
        /// Check if a character definition exists.
        /// </summary>
        public static bool HasCharacterDefinition(string characterName)
        {
            return GetCharacterDefinition(characterName) != null;
        }
        
        /// <summary>
        /// Get a character definition by id, alias, or legacy display name.
        /// </summary>
        public static CharacterDefinition GetCharacterDefinition(string characterName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            if (_characterCatalog == null)
                return null;

            _characterCatalog.TryResolve(characterName, out CharacterDefinition definition);
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
            
            HashSet<CharacterType> usedTypes = new HashSet<CharacterType>();
            HashSet<ElementalType> usedElements = new HashSet<ElementalType>();
            
            for (int i = 0; i < teamSize; i++)
            {
                CharacterDefinition bestChoice = null;
                int bestScore = -1;
                
                foreach (CharacterDefinition definition in allDefinitions)
                {
                    int score = 0;
                    
                    if (!usedTypes.Contains(definition.CharacterType))
                        score += 10;
                    
                    if (!usedElements.Contains(definition.ElementalType))
                        score += 5;
                    
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

            CharacterDefinition[] definitions = GetAllCharacterDefinitions();
            Dictionary<CharacterType, int> typeCounts = new Dictionary<CharacterType, int>();
            Dictionary<ElementalType, int> elementCounts = new Dictionary<ElementalType, int>();
            Dictionary<Rarity, int> rarityCounts = new Dictionary<Rarity, int>();
            
            foreach (CharacterDefinition definition in definitions)
            {
                if (!typeCounts.ContainsKey(definition.CharacterType))
                    typeCounts[definition.CharacterType] = 0;
                typeCounts[definition.CharacterType]++;
                
                if (!elementCounts.ContainsKey(definition.ElementalType))
                    elementCounts[definition.ElementalType] = 0;
                elementCounts[definition.ElementalType]++;
                
                if (!rarityCounts.ContainsKey(definition.Rarity))
                    rarityCounts[definition.Rarity] = 0;
                rarityCounts[definition.Rarity]++;
            }
            
            string stats = $"Character Statistics:\n";
            stats += $"Total Characters: {definitions.Length}\n\n";
            
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
