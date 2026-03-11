using System;
using System.Collections.Generic;
using System.Linq;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.UI
{
    internal static class TestCollectionService
    {
        private const string SaveKey = "Shogun.TestCollection.v1";
        private const int DefaultSpiritSeals = 900;

        internal static readonly string[] DefaultOwnedCharacterIds =
        {
            "ryoma",
            "daichi",
            "harada",
            "katsuro",
            "takeshi",
            "okami-jin"
        };

        private static bool s_IsLoaded;
        private static readonly HashSet<string> s_OwnedCharacterIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> s_PullCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static int s_SpiritSeals;

        [Serializable]
        private sealed class SaveData
        {
            public List<string> ownedCharacterIds = new List<string>();
            public List<PullCountEntry> pullCounts = new List<PullCountEntry>();
            public int spiritSeals = DefaultSpiritSeals;
        }

        [Serializable]
        private sealed class PullCountEntry
        {
            public string characterId;
            public int count;
        }

        internal readonly struct OwnershipGrant
        {
            public OwnershipGrant(bool isNew, int ownedCount)
            {
                IsNew = isNew;
                OwnedCount = ownedCount;
            }

            public bool IsNew { get; }
            public int OwnedCount { get; }
        }

        public static IReadOnlyList<CharacterDefinition> GetOwnedCharacterDefinitions()
        {
            EnsureLoaded();
            List<CharacterDefinition> definitions = new List<CharacterDefinition>();
            foreach (string characterId in s_OwnedCharacterIds)
            {
                CharacterDefinition definition = CharacterFactory.GetCharacterDefinitionById(characterId);
                if (definition != null)
                    definitions.Add(definition);
            }

            definitions.Sort((a, b) => string.Compare(BarracksCharacterPresentation.GetDisplayName(a), BarracksCharacterPresentation.GetDisplayName(b), StringComparison.OrdinalIgnoreCase));
            return definitions;
        }

        public static int GetSpiritSeals()
        {
            EnsureLoaded();
            return s_SpiritSeals;
        }

        public static bool CanSpendSpiritSeals(int amount)
        {
            EnsureLoaded();
            return amount <= 0 || s_SpiritSeals >= amount;
        }

        public static bool TrySpendSpiritSeals(int amount)
        {
            EnsureLoaded();
            if (amount <= 0)
                return true;
            if (s_SpiritSeals < amount)
                return false;

            s_SpiritSeals -= amount;
            Save();
            return true;
        }

        public static OwnershipGrant GrantCharacter(string characterId)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(characterId))
                return new OwnershipGrant(false, 0);

            bool isNew = s_OwnedCharacterIds.Add(characterId);
            int ownedCount = s_PullCounts.TryGetValue(characterId, out int existingCount)
                ? existingCount + 1
                : 1;
            s_PullCounts[characterId] = ownedCount;
            Save();
            return new OwnershipGrant(isNew, ownedCount);
        }

        public static int GetOwnedCount(string characterId)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(characterId))
                return 0;
            if (s_PullCounts.TryGetValue(characterId, out int count))
                return count;
            return s_OwnedCharacterIds.Contains(characterId) ? 1 : 0;
        }

        public static void ResetToDefaults()
        {
            s_OwnedCharacterIds.Clear();
            s_PullCounts.Clear();
            for (int i = 0; i < DefaultOwnedCharacterIds.Length; i++)
            {
                string characterId = DefaultOwnedCharacterIds[i];
                if (string.IsNullOrWhiteSpace(characterId))
                    continue;
                s_OwnedCharacterIds.Add(characterId);
                s_PullCounts[characterId] = 1;
            }

            s_SpiritSeals = DefaultSpiritSeals;
            s_IsLoaded = true;
            Save();
        }

        private static void EnsureLoaded()
        {
            if (s_IsLoaded)
                return;

            if (!PlayerPrefs.HasKey(SaveKey))
            {
                ResetToDefaults();
                return;
            }

            string raw = PlayerPrefs.GetString(SaveKey, string.Empty);
            SaveData data = string.IsNullOrWhiteSpace(raw) ? null : JsonUtility.FromJson<SaveData>(raw);
            if (data == null)
            {
                ResetToDefaults();
                return;
            }

            s_OwnedCharacterIds.Clear();
            s_PullCounts.Clear();
            if (data.ownedCharacterIds != null)
            {
                for (int i = 0; i < data.ownedCharacterIds.Count; i++)
                {
                    string characterId = data.ownedCharacterIds[i];
                    if (!string.IsNullOrWhiteSpace(characterId))
                        s_OwnedCharacterIds.Add(characterId);
                }
            }

            if (data.pullCounts != null)
            {
                for (int i = 0; i < data.pullCounts.Count; i++)
                {
                    PullCountEntry entry = data.pullCounts[i];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.characterId))
                        continue;
                    s_PullCounts[entry.characterId] = Mathf.Max(1, entry.count);
                    s_OwnedCharacterIds.Add(entry.characterId);
                }
            }

            if (s_OwnedCharacterIds.Count == 0)
            {
                ResetToDefaults();
                return;
            }

            foreach (string characterId in s_OwnedCharacterIds.ToArray())
            {
                if (!s_PullCounts.ContainsKey(characterId))
                    s_PullCounts[characterId] = 1;
            }

            s_SpiritSeals = Mathf.Max(DefaultSpiritSeals, Mathf.Max(0, data.spiritSeals));
            s_IsLoaded = true;
        }

        private static void Save()
        {
            SaveData data = new SaveData
            {
                spiritSeals = s_SpiritSeals,
                ownedCharacterIds = s_OwnedCharacterIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList(),
                pullCounts = s_PullCounts
                    .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(pair => new PullCountEntry { characterId = pair.Key, count = Mathf.Max(1, pair.Value) })
                    .ToList()
            };

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}

