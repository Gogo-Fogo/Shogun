using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shogun.Features.Characters
{
    [CreateAssetMenu(fileName = "AbilityCatalog", menuName = "Shogun/Characters/Ability Catalog")]
    public class AbilityCatalog : ScriptableObject
    {
        [SerializeField] private List<AbilityDefinition> definitions = new List<AbilityDefinition>();

        private readonly Dictionary<string, AbilityDefinition> definitionsById = new Dictionary<string, AbilityDefinition>();
        private bool lookupBuilt;

        public IReadOnlyList<AbilityDefinition> Definitions => definitions;

        private void OnEnable()
        {
            lookupBuilt = false;
        }

        public IReadOnlyList<AbilityDefinition> GetAll()
        {
            return definitions;
        }

        public bool TryGetById(string abilityId, out AbilityDefinition definition)
        {
            RebuildLookupTables();
            return definitionsById.TryGetValue(AbilityDefinition.NormalizeId(abilityId), out definition);
        }

        public void SetDefinitions(IEnumerable<AbilityDefinition> sourceDefinitions)
        {
            definitions = sourceDefinitions?
                .Where(definition => definition != null)
                .Distinct()
                .OrderBy(definition => definition.AbilityId)
                .ToList()
                ?? new List<AbilityDefinition>();

            lookupBuilt = false;
            RebuildLookupTables();
        }

        private void RebuildLookupTables()
        {
            if (lookupBuilt)
                return;

            definitionsById.Clear();
            foreach (AbilityDefinition definition in definitions)
            {
                if (definition == null)
                    continue;

                string normalizedId = AbilityDefinition.NormalizeId(definition.AbilityId);
                if (!string.IsNullOrWhiteSpace(normalizedId) && !definitionsById.ContainsKey(normalizedId))
                    definitionsById.Add(normalizedId, definition);
            }

            lookupBuilt = true;
        }
    }
}