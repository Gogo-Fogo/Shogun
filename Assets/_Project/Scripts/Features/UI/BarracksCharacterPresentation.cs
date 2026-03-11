using System;
using System.Collections.Generic;
using System.Text;
using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Shogun.Features.UI
{
    internal static class BarracksCharacterPresentation
    {
        public static string GetDisplayName(CharacterDefinition definition)
        {
            if (definition == null)
                return "Unknown";
            if (!string.IsNullOrWhiteSpace(definition.DisplayNameEN))
                return definition.DisplayNameEN;
            if (!string.IsNullOrWhiteSpace(definition.CharacterName))
                return definition.CharacterName;
            return definition.CharacterId;
        }

        public static string GetCardSummary(CharacterDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.LoreBlurb))
                return definition.LoreBlurb;
            if (!string.IsNullOrWhiteSpace(definition.VisualHook))
                return definition.VisualHook;
            if (!string.IsNullOrWhiteSpace(definition.Description))
                return definition.Description;
            return "Collection package placeholder. Presentation art and owned-state progression still pending.";
        }

        public static string BuildTagline(CharacterDefinition definition)
        {
            List<string> parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(definition.CollectibleTone))
                parts.Add(definition.CollectibleTone.ToUpperInvariant());
            parts.Add(definition.CollectibleFantasy.ToString().Replace('_', ' ').ToUpperInvariant());
            parts.Add(definition.WorldPillar.ToString().Replace('_', ' ').ToUpperInvariant());
            return string.Join(" • ", parts);
        }

        public static string BuildLoreText(CharacterDefinition definition)
        {
            string lore = !string.IsNullOrWhiteSpace(definition.LoreBlurb) ? definition.LoreBlurb : GetCardSummary(definition);
            return $"\"{lore}\"";
        }

        public static string BuildMetadataText(CharacterDefinition definition)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("COLLECTION PACKAGE");
            if (!string.IsNullOrWhiteSpace(definition.VisualHook)) builder.AppendLine($"\nVisual Hook: {definition.VisualHook}");
            if (!string.IsNullOrWhiteSpace(definition.EmotionalHook)) builder.AppendLine($"Emotional Hook: {definition.EmotionalHook}");
            if (!string.IsNullOrWhiteSpace(definition.CollectibleTone)) builder.AppendLine($"Tone: {definition.CollectibleTone}");
            if (!string.IsNullOrWhiteSpace(definition.VariantPotential)) builder.AppendLine($"Variant Potential: {definition.VariantPotential}");
            return builder.ToString().TrimEnd();
        }

        public static string BuildStatsText(CharacterDefinition definition)
            => $"HP {Mathf.RoundToInt(definition.BaseHealth)}    ATK {Mathf.RoundToInt(definition.BaseAttack)}    DEF {Mathf.RoundToInt(definition.BaseDefense)}    SPD {Mathf.RoundToInt(definition.BaseSpeed)}";

        public static string BuildSpecialText(CharacterDefinition definition)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Special Charge {definition.SpecialAbilityChargeRequirement}  |  Ultimate Charge {definition.UltimateAbilityChargeRequirement}");
            builder.Append(!string.IsNullOrWhiteSpace(definition.SpecialAbilityName) ? $"\n{definition.SpecialAbilityName}: " : "\nAbility: ");
            builder.Append(!string.IsNullOrWhiteSpace(definition.SpecialAbilityDescription) ? definition.SpecialAbilityDescription : "No authored special text assigned yet.");
            return builder.ToString();
        }

        public static string GetElementLabel(ElementalType element) => element.ToString().ToUpperInvariant();

        public static string GetWeaponLabel(MartialArtsType type)
            => type == MartialArtsType.DualDaggers ? "DUAL DAGGERS" : type == MartialArtsType.HeavyWeapons ? "HEAVY" : type.ToString().ToUpperInvariant();

        public static string GetRarityLabel(Rarity rarity) => rarity.ToString().ToUpperInvariant();

        public static Color GetElementColor(ElementalType element)
        {
            switch (element)
            {
                case ElementalType.Fire: return new Color(0.69f, 0.24f, 0.16f, 1f);
                case ElementalType.Water: return new Color(0.2f, 0.4f, 0.66f, 1f);
                case ElementalType.Earth: return new Color(0.43f, 0.33f, 0.18f, 1f);
                case ElementalType.Wind: return new Color(0.24f, 0.52f, 0.4f, 1f);
                case ElementalType.Lightning: return new Color(0.61f, 0.53f, 0.15f, 1f);
                case ElementalType.Ice: return new Color(0.38f, 0.63f, 0.78f, 1f);
                case ElementalType.Shadow: return new Color(0.35f, 0.25f, 0.48f, 1f);
                default: return new Color(0.42f, 0.42f, 0.42f, 1f);
            }
        }

        public static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Legendary: return new Color(0.74f, 0.53f, 0.18f, 1f);
                case Rarity.Epic: return new Color(0.47f, 0.28f, 0.6f, 1f);
                case Rarity.Rare: return new Color(0.23f, 0.46f, 0.68f, 1f);
                default: return new Color(0.36f, 0.33f, 0.31f, 1f);
            }
        }

        public static void SetPortraitVisual(CharacterDefinition definition, Image portraitImage, GameObject placeholderRoot, Text placeholderLabel, int placeholderFontSize)
        {
            Sprite portraitSprite = ResolvePortraitSprite(definition);
            portraitImage.sprite = portraitSprite;
            portraitImage.enabled = portraitSprite != null;
            bool showPlaceholder = portraitSprite == null;
            placeholderRoot.SetActive(showPlaceholder);
            if (showPlaceholder)
            {
                placeholderLabel.fontSize = placeholderFontSize;
                placeholderLabel.text = GetInitials(definition);
            }
        }

        private static Sprite ResolvePortraitSprite(CharacterDefinition definition)
        {
            if (definition == null) return null;
            if (definition.Portrait != null) return definition.Portrait;
            if (definition.BannerSprite != null) return definition.BannerSprite;
            if (definition.BattleSprite != null) return definition.BattleSprite;
            return null;
        }

        private static string GetInitials(CharacterDefinition definition)
        {
            string name = GetDisplayName(definition);
            if (string.IsNullOrWhiteSpace(name)) return "--";
            string[] tokens = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1)
                return tokens[0].Substring(0, Mathf.Min(2, tokens[0].Length)).ToUpperInvariant();
            return $"{char.ToUpperInvariant(tokens[0][0])}{char.ToUpperInvariant(tokens[1][0])}";
        }
    }
}
