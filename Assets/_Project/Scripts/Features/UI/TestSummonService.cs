using System;
using System.Collections.Generic;
using System.Linq;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.UI
{
    internal static class TestSummonService
    {
        private static readonly string[] SharedPoolCharacterIds =
        {
            "akai",
            "daichi",
            "harada",
            "katsuro",
            "kumada",
            "kuro",
            "okami-jin",
            "reiji",
            "ryoma",
            "takeshi",
            "tsukiko"
        };

        private static readonly TestSummonBanner[] Banners =
        {
            new TestSummonBanner(
                "moonlit-oath",
                "MOONLIT OATH",
                "Event Banner",
                "Shadow and ritual-leaning duelists with a colder, more elegant tone.",
                "Offline local summon simulation. No purchases, no backend entitlement, and rates are disclosed for testing only.",
                new Color(0.24f, 0.3f, 0.46f, 1f),
                5,
                45,
                60f,
                30f,
                10f,
                0f,
                new[] { "kuro", "tsukiko", "reiji" },
                SharedPoolCharacterIds),
            new TestSummonBanner(
                "crimson-vanguard",
                "CRIMSON VANGUARD",
                "Festival Test Banner",
                "More aggressive frontline pressure with the wolf-banner epic as the chase pull.",
                "Offline local summon simulation. No purchases, no backend entitlement, and rates are disclosed for testing only.",
                new Color(0.48f, 0.2f, 0.16f, 1f),
                5,
                45,
                60f,
                30f,
                10f,
                0f,
                new[] { "ryoma", "daichi", "okami-jin" },
                SharedPoolCharacterIds)
        };

        internal sealed class TestSummonBanner
        {
            public TestSummonBanner(string bannerId, string title, string bannerType, string summary, string disclosure, Color accentColor, int singlePullCost, int multiPullCost, float uncommonRate, float rareRate, float epicRate, float legendaryRate, string[] featuredCharacterIds, string[] poolCharacterIds)
            {
                BannerId = bannerId;
                Title = title;
                BannerType = bannerType;
                Summary = summary;
                Disclosure = disclosure;
                AccentColor = accentColor;
                SinglePullCost = singlePullCost;
                MultiPullCost = multiPullCost;
                UncommonRate = uncommonRate;
                RareRate = rareRate;
                EpicRate = epicRate;
                LegendaryRate = legendaryRate;
                FeaturedCharacterIds = featuredCharacterIds ?? Array.Empty<string>();
                PoolCharacterIds = poolCharacterIds ?? Array.Empty<string>();
            }

            public string BannerId { get; }
            public string Title { get; }
            public string BannerType { get; }
            public string Summary { get; }
            public string Disclosure { get; }
            public Color AccentColor { get; }
            public int SinglePullCost { get; }
            public int MultiPullCost { get; }
            public float UncommonRate { get; }
            public float RareRate { get; }
            public float EpicRate { get; }
            public float LegendaryRate { get; }
            public IReadOnlyList<string> FeaturedCharacterIds { get; }
            public IReadOnlyList<string> PoolCharacterIds { get; }

            public int GetCost(int pullCount) => pullCount >= 10 ? MultiPullCost : SinglePullCost * Mathf.Max(1, pullCount);

            public string GetOddsSummary()
            {
                List<string> parts = new List<string>();
                if (UncommonRate > 0f) parts.Add($"UNCOMMON {UncommonRate:0}%");
                if (RareRate > 0f) parts.Add($"RARE {RareRate:0}%");
                if (EpicRate > 0f) parts.Add($"EPIC {EpicRate:0}%");
                if (LegendaryRate > 0f) parts.Add($"LEGENDARY {LegendaryRate:0}%");
                return string.Join("  |  ", parts);
            }

            public bool IsFeatured(string characterId)
            {
                for (int i = 0; i < FeaturedCharacterIds.Count; i++)
                {
                    if (string.Equals(FeaturedCharacterIds[i], characterId, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }
        }

        internal readonly struct TestSummonPullResult
        {
            public TestSummonPullResult(CharacterDefinition definition, bool isNew, int ownedCount)
            {
                Definition = definition;
                IsNew = isNew;
                OwnedCount = ownedCount;
            }

            public CharacterDefinition Definition { get; }
            public bool IsNew { get; }
            public int OwnedCount { get; }
        }

        internal sealed class TestSummonSession
        {
            public TestSummonSession(TestSummonBanner banner, int pullCount, int spentSpiritSeals, int remainingSpiritSeals, IReadOnlyList<TestSummonPullResult> results)
            {
                Banner = banner;
                PullCount = pullCount;
                SpentSpiritSeals = spentSpiritSeals;
                RemainingSpiritSeals = remainingSpiritSeals;
                Results = results;
            }

            public TestSummonBanner Banner { get; }
            public int PullCount { get; }
            public int SpentSpiritSeals { get; }
            public int RemainingSpiritSeals { get; }
            public IReadOnlyList<TestSummonPullResult> Results { get; }
        }

        public static IReadOnlyList<TestSummonBanner> GetBanners() => Banners;

        public static TestSummonBanner GetBanner(string bannerId)
        {
            for (int i = 0; i < Banners.Length; i++)
            {
                if (string.Equals(Banners[i].BannerId, bannerId, StringComparison.OrdinalIgnoreCase))
                    return Banners[i];
            }

            return Banners.Length > 0 ? Banners[0] : null;
        }

        public static bool TryPerformSummon(string bannerId, int pullCount, out TestSummonSession session, out string error)
        {
            session = null;
            error = string.Empty;
            TestSummonBanner banner = GetBanner(bannerId);
            if (banner == null)
            {
                error = "No test banner is available.";
                return false;
            }

            int normalizedPullCount = pullCount >= 10 ? 10 : 1;
            int cost = banner.GetCost(normalizedPullCount);
            if (!TestCollectionService.TrySpendSpiritSeals(cost))
            {
                error = $"Not enough Spirit Seals. Need {cost}, have {TestCollectionService.GetSpiritSeals()}.";
                return false;
            }

            List<TestSummonPullResult> results = new List<TestSummonPullResult>();
            for (int i = 0; i < normalizedPullCount; i++)
            {
                bool guaranteeRareOrBetter = normalizedPullCount >= 10 && i == normalizedPullCount - 1;
                CharacterDefinition definition = RollCharacter(banner, guaranteeRareOrBetter);
                if (definition == null)
                    continue;
                TestCollectionService.OwnershipGrant grant = TestCollectionService.GrantCharacter(definition.CharacterId);
                results.Add(new TestSummonPullResult(definition, grant.IsNew, grant.OwnedCount));
            }

            session = new TestSummonSession(banner, normalizedPullCount, cost, TestCollectionService.GetSpiritSeals(), results);
            return true;
        }

        private static CharacterDefinition RollCharacter(TestSummonBanner banner, bool guaranteeRareOrBetter)
        {
            List<CharacterDefinition> pool = ResolvePoolDefinitions(banner);
            if (pool.Count == 0)
                return null;

            Rarity rolledRarity = RollRarity(banner, guaranteeRareOrBetter);
            List<CharacterDefinition> rarityPool = ResolvePoolForRarity(pool, rolledRarity);
            if (rarityPool.Count == 0)
                rarityPool = pool;

            List<CharacterDefinition> featuredPool = rarityPool.Where(definition => banner.IsFeatured(definition.CharacterId)).ToList();
            List<CharacterDefinition> offBannerPool = rarityPool.Where(definition => !banner.IsFeatured(definition.CharacterId)).ToList();
            float featuredBias = rolledRarity >= Rarity.Epic ? 0.8f : rolledRarity >= Rarity.Rare ? 0.68f : 0.58f;
            List<CharacterDefinition> activePool = featuredPool.Count > 0 && (offBannerPool.Count == 0 || UnityEngine.Random.value < featuredBias)
                ? featuredPool
                : offBannerPool.Count > 0 ? offBannerPool : rarityPool;

            return activePool[UnityEngine.Random.Range(0, activePool.Count)];
        }

        private static Rarity RollRarity(TestSummonBanner banner, bool guaranteeRareOrBetter)
        {
            float uncommonRate = banner.UncommonRate;
            float rareRate = banner.RareRate;
            float epicRate = banner.EpicRate;
            float legendaryRate = banner.LegendaryRate;
            if (guaranteeRareOrBetter)
            {
                rareRate += uncommonRate;
                uncommonRate = 0f;
            }

            float totalRate = uncommonRate + rareRate + epicRate + legendaryRate;
            if (totalRate <= 0f)
                return Rarity.Rare;

            float roll = UnityEngine.Random.value * totalRate;
            if (roll < uncommonRate) return Rarity.Uncommon;
            roll -= uncommonRate;
            if (roll < rareRate) return Rarity.Rare;
            roll -= rareRate;
            if (roll < epicRate) return Rarity.Epic;
            return Rarity.Legendary;
        }

        private static List<CharacterDefinition> ResolvePoolDefinitions(TestSummonBanner banner)
        {
            List<CharacterDefinition> pool = new List<CharacterDefinition>();
            for (int i = 0; i < banner.PoolCharacterIds.Count; i++)
            {
                CharacterDefinition definition = CharacterFactory.GetCharacterDefinitionById(banner.PoolCharacterIds[i]);
                if (definition != null)
                    pool.Add(definition);
            }

            return pool;
        }

        private static List<CharacterDefinition> ResolvePoolForRarity(List<CharacterDefinition> pool, Rarity desiredRarity)
        {
            List<CharacterDefinition> matches = pool.Where(definition => definition.Rarity == desiredRarity).ToList();
            if (matches.Count > 0)
                return matches;

            if (desiredRarity > Rarity.Uncommon)
            {
                for (int rarity = (int)desiredRarity - 1; rarity >= (int)Rarity.Uncommon; rarity--)
                {
                    matches = pool.Where(definition => (int)definition.Rarity == rarity).ToList();
                    if (matches.Count > 0)
                        return matches;
                }
            }

            for (int rarity = (int)desiredRarity + 1; rarity <= (int)Rarity.Legendary; rarity++)
            {
                matches = pool.Where(definition => (int)definition.Rarity == rarity).ToList();
                if (matches.Count > 0)
                    return matches;
            }

            return pool;
        }
    }
}
