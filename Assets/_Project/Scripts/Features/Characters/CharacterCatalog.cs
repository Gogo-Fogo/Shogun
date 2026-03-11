using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shogun.Features.Characters
{
    [CreateAssetMenu(fileName = "CharacterCatalog", menuName = "Shogun/Characters/Character Catalog")]
    public class CharacterCatalog : ScriptableObject
    {
        [SerializeField] private List<CharacterDefinition> definitions = new List<CharacterDefinition>();

        private readonly Dictionary<string, CharacterDefinition> _definitionsById = new Dictionary<string, CharacterDefinition>();
        private readonly Dictionary<string, CharacterDefinition> _definitionsByAlias = new Dictionary<string, CharacterDefinition>();
        private bool _lookupBuilt;

        public IReadOnlyList<CharacterDefinition> Definitions => definitions;

        private void OnEnable()
        {
            // Reset flag so tables are rebuilt lazily on first access,
            // ensuring all referenced CharacterDefinition assets are fully
            // deserialized before we iterate them.
            _lookupBuilt = false;
        }

        public IReadOnlyList<CharacterDefinition> GetAll()
        {
            return definitions;
        }

        public bool TryGetById(string characterId, out CharacterDefinition definition)
        {
            RebuildLookupTables();
            return _definitionsById.TryGetValue(CharacterKeyUtility.NormalizeCharacterId(characterId), out definition);
        }

        public bool TryGetByAlias(string alias, out CharacterDefinition definition)
        {
            RebuildLookupTables();
            return _definitionsByAlias.TryGetValue(CharacterKeyUtility.NormalizeLookupKey(alias), out definition);
        }

        public bool TryResolve(string lookupValue, out CharacterDefinition definition)
        {
            if (TryGetById(lookupValue, out definition))
                return true;

            if (TryGetByAlias(lookupValue, out definition))
                return true;

            // Linear scan fallback — catches any edge case where the lookup tables
            // were built before all referenced assets were fully deserialized.
            string normalizedId     = CharacterKeyUtility.NormalizeCharacterId(lookupValue);
            string normalizedLookup = CharacterKeyUtility.NormalizeLookupKey(lookupValue);
            foreach (CharacterDefinition def in definitions)
            {
                if (def == null)
                    continue;

                if (CharacterKeyUtility.NormalizeCharacterId(def.CharacterId) == normalizedId)
                {
                    definition = def;
                    return true;
                }

                foreach (string term in def.GetLookupTerms())
                {
                    if (CharacterKeyUtility.NormalizeLookupKey(term) == normalizedLookup)
                    {
                        definition = def;
                        return true;
                    }
                }
            }

            definition = null;
            return false;
        }

        public void SetDefinitions(IEnumerable<CharacterDefinition> sourceDefinitions)
        {
            definitions = sourceDefinitions?
                .Where(definition => definition != null)
                .Distinct()
                .OrderBy(definition => definition.CharacterId)
                .ToList()
                ?? new List<CharacterDefinition>();

            _lookupBuilt = false;
            RebuildLookupTables();
        }

        private void RebuildLookupTables()
        {
            if (_lookupBuilt)
                return;

            _definitionsById.Clear();
            _definitionsByAlias.Clear();

            foreach (CharacterDefinition definition in definitions)
            {
                if (definition == null)
                    continue;

                string normalizedId = CharacterKeyUtility.NormalizeCharacterId(definition.CharacterId);
                if (!string.IsNullOrWhiteSpace(normalizedId) && !_definitionsById.ContainsKey(normalizedId))
                    _definitionsById.Add(normalizedId, definition);

                foreach (string lookupTerm in definition.GetLookupTerms())
                {
                    string normalizedLookup = CharacterKeyUtility.NormalizeLookupKey(lookupTerm);
                    if (string.IsNullOrWhiteSpace(normalizedLookup) || _definitionsByAlias.ContainsKey(normalizedLookup))
                        continue;

                    _definitionsByAlias.Add(normalizedLookup, definition);
                }
            }

            _lookupBuilt = true;
        }
    }
}
