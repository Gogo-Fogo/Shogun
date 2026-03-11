#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shogun.Features.Characters
{
    public static class AbilityCatalogEditorUtility
    {
        public const string AbilitiesRoot = CharacterCatalogEditorUtility.DatabaseRoot + "/Abilities";
        public const string CatalogAssetPath = CharacterCatalogEditorUtility.ResourcesRoot + "/AbilityCatalog.asset";

        private static readonly char[] WordSeparators = { ' ', '\t', '\r', '\n' };
        private static readonly Dictionary<string, (string Name, string Description)> CuratedUltimatePresentation =
            new Dictionary<string, (string Name, string Description)>(StringComparer.OrdinalIgnoreCase)
            {
                ["akai"] = ("Crimson Oath Severance", "Akai answers pressure with a blood-bright execution flurry that tears the target apart before they can recover."),
                ["daichi"] = ("Mountainbreak Verdict", "Daichi brings the whole lane down in a mountain-heavy execution blow meant to finish whatever still stands."),
                ["harada"] = ("Hundred Reed Tempest", "Harada plants his feet and unleashes a disciplined storm of killing arrows that overwhelms the far side of the field."),
                ["katsuro"] = ("Blazing Flash Execution", "Katsuro erupts forward in a fire-bright execution dash, cutting through the target before the afterimage fades."),
                ["kumada"] = ("Earthshaker Rampage", "Kumada crashes ahead in an earthshaking rush that batters the front line with unstoppable weight."),
                ["kuro"] = ("Nightfang Eclipse", "Kuro disappears into a blackened dash and reappears behind the mark, leaving a killing crescent through the lane."),
                ["okami-jin"] = ("White Winter Devourer", "Okami-Jin answers the lane with a freezing beast-lord surge that tears through the target in a white blur."),
                ["oni-brute"] = ("Nine-Hell Breaker", "The Oni Brute hammers forward in a demon-heavy execution slam that crushes anything left in his reach."),
                ["reiji"] = ("Midnight Funeral Draw", "Reiji drags the field into his shadow and finishes the target with a merciless black-moon sword sequence."),
                ["ronin-footman"] = ("Ashen Linebreaker", "The ronin footman commits to a desperate finishing rush that shatters guard and opens the line for the kill."),
                ["ryoma"] = ("Heavensplit Iai", "Ryoma vanishes into a single sky-cleaving draw that punishes the target with overwhelming finishing force."),
                ["takeshi"] = ("Raijin Spearfall", "Takeshi commits fully to a thunder-lanced charge, breaking straight through the line with a killing spearfall."),
                ["tsukiko"] = ("Full Moon Requiem", "Tsukiko floods the field with moon-cold spirit light, crushing enemies beneath a lingering funeral tide."),
                ["yurei-caster"] = ("Lantern Funeral Dirge", "The Yurei Caster lets a grave-cold lament roll across the lane, drowning the target in cursed spirit pressure.")
            };

        public struct SyncReport
        {
            public int CreatedAbilities;
            public int UpdatedAbilities;
            public int LinkedDefinitions;
            public int ErrorCount;
            public int WarningCount;
            public bool CatalogRebuilt;
            public List<string> Messages;
        }

        public struct ValidationReport
        {
            public int ErrorCount;
            public int WarningCount;
            public List<string> Messages;
        }

        public static void EnsureFoldersExist()
        {
            Directory.CreateDirectory(AbilitiesRoot);
            Directory.CreateDirectory(CharacterCatalogEditorUtility.ResourcesRoot);
            AssetDatabase.Refresh();
        }

        public static AbilityCatalog GetOrCreateCatalog()
        {
            EnsureFoldersExist();

            AbilityCatalog catalog = AssetDatabase.LoadAssetAtPath<AbilityCatalog>(CatalogAssetPath);
            if (catalog != null)
                return catalog;

            catalog = ScriptableObject.CreateInstance<AbilityCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        public static bool RebuildCatalog(bool logSummary = true)
        {
            EnsureFoldersExist();
            List<AbilityDefinition> abilities = LoadAbilitiesFromDatabase();
            Dictionary<string, List<AbilityDefinition>> duplicates = abilities
                .GroupBy(definition => definition.AbilityId)
                .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
                .ToDictionary(group => group.Key, group => group.ToList());

            if (duplicates.Count > 0)
            {
                foreach (KeyValuePair<string, List<AbilityDefinition>> duplicate in duplicates)
                {
                    Debug.LogError($"AbilityCatalog: Duplicate abilityId '{duplicate.Key}' found on {string.Join(", ", duplicate.Value.Select(definition => definition.name))}");
                }

                return false;
            }

            AbilityCatalog catalog = GetOrCreateCatalog();
            catalog.SetDefinitions(abilities);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            if (logSummary)
                Debug.Log($"AbilityCatalog: Rebuilt with {abilities.Count} definitions.");

            return true;
        }

        public static SyncReport SyncAbilitiesFromCharacterDefinitions(bool logSummary = true)
        {
            EnsureFoldersExist();

            SyncReport report = new SyncReport
            {
                Messages = new List<string>()
            };

            List<CharacterDefinition> characterDefinitions = LoadCharacterDefinitions();
            foreach (CharacterDefinition definition in characterDefinitions)
            {
                if (definition == null)
                    continue;

                if (NeedsSpecialAbilityAsset(definition))
                    SyncAbilitySlot(definition, AbilitySlot.Special, ref report);

                if (NeedsUltimateAbilityAsset(definition))
                    SyncAbilitySlot(definition, AbilitySlot.Ultimate, ref report);
            }

            report.CatalogRebuilt = RebuildCatalog(false);
            if (!report.CatalogRebuilt)
            {
                report.ErrorCount++;
                report.Messages.Add("ERROR AbilityCatalog rebuild failed due to duplicate ids or invalid assets.");
            }

            AssetDatabase.SaveAssets();

            if (logSummary)
            {
                Debug.Log($"AbilityCatalog: sync completed. Created {report.CreatedAbilities} ability asset(s), updated {report.UpdatedAbilities}, linked {report.LinkedDefinitions}, warnings {report.WarningCount}, errors {report.ErrorCount}.");
                foreach (string message in report.Messages)
                    Debug.Log(message);
            }

            return report;
        }

        public static ValidationReport ValidateDatabase()
        {
            ValidationReport report = new ValidationReport
            {
                Messages = new List<string>()
            };

            List<AbilityDefinition> abilities = LoadAbilitiesFromDatabase();
            Dictionary<string, List<AbilityDefinition>> duplicates = abilities
                .GroupBy(definition => definition.AbilityId)
                .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (KeyValuePair<string, List<AbilityDefinition>> duplicate in duplicates)
            {
                report.ErrorCount++;
                report.Messages.Add($"ERROR Duplicate abilityId '{duplicate.Key}' on {string.Join(", ", duplicate.Value.Select(definition => definition.name))}");
            }

            foreach (AbilityDefinition definition in abilities)
                ValidateAbility(definition, ref report);

            return report;
        }

        private static void SyncAbilitySlot(CharacterDefinition characterDefinition, AbilitySlot slot, ref SyncReport report)
        {
            bool created = false;
            AbilityDefinition ability = GetOrCreateAbilityAsset(characterDefinition, slot, out created);
            if (ability == null)
            {
                report.ErrorCount++;
                report.Messages.Add($"ERROR Failed to create {slot.ToString().ToLowerInvariant()} ability asset for '{characterDefinition.CharacterId}'.");
                return;
            }

            if (created)
                report.CreatedAbilities++;

            if (SyncAbilityAsset(ability, characterDefinition, slot))
            {
                report.UpdatedAbilities++;
                report.Messages.Add($"Synced {slot.ToString().ToLowerInvariant()} ability asset '{ability.name}' from {characterDefinition.name}.");
            }

            string propertyName = slot == AbilitySlot.Special ? "specialAbilityDefinition" : "ultimateAbilityDefinition";
            SerializedObject definitionObject = new SerializedObject(characterDefinition);
            SerializedProperty abilityRef = definitionObject.FindProperty(propertyName);
            if (abilityRef != null && abilityRef.objectReferenceValue != ability)
            {
                abilityRef.objectReferenceValue = ability;
                definitionObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(characterDefinition);
                report.LinkedDefinitions++;
                report.Messages.Add($"Linked {characterDefinition.name} to {ability.name}.");
            }
        }

        private static List<CharacterDefinition> LoadCharacterDefinitions()
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterDefinition", new[] { CharacterCatalogEditorUtility.DefinitionsRoot });
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path))
                .Where(definition => definition != null)
                .OrderBy(definition => definition.CharacterId)
                .ToList();
        }

        private static List<AbilityDefinition> LoadAbilitiesFromDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:AbilityDefinition", new[] { AbilitiesRoot });
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path))
                .Where(definition => definition != null)
                .OrderBy(definition => definition.AbilityId)
                .ToList();
        }

        private static bool NeedsSpecialAbilityAsset(CharacterDefinition definition)
        {
            return definition != null
                   && (!string.IsNullOrWhiteSpace(definition.SpecialAbilityName)
                       || !string.IsNullOrWhiteSpace(definition.SpecialAbilityDescription)
                       || definition.SpecialAbilityDefinition != null);
        }

        private static bool NeedsUltimateAbilityAsset(CharacterDefinition definition)
        {
            return definition != null
                   && (definition.UltimateAbilityDefinition != null || HasAnimationMapping(definition, "SPECIAL ATTACK"));
        }

        private static AbilityDefinition GetOrCreateAbilityAsset(CharacterDefinition characterDefinition, AbilitySlot slot, out bool created)
        {
            created = false;
            string slotSuffix = slot == AbilitySlot.Special ? "Special" : "Ultimate";
            string assetName = $"{ToAssetName(characterDefinition.CharacterId)}_{slotSuffix}Ability";
            string assetPath = $"{AbilitiesRoot}/{assetName}.asset";
            AbilityDefinition ability = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(assetPath);
            if (ability != null)
                return ability;

            ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            AssetDatabase.CreateAsset(ability, assetPath);
            created = true;
            return ability;
        }

        private static bool SyncAbilityAsset(AbilityDefinition ability, CharacterDefinition characterDefinition, AbilitySlot slot)
        {
            bool changed = false;
            SerializedObject serializedObject = new SerializedObject(ability);
            changed |= SetString(serializedObject, "abilityId", $"{characterDefinition.CharacterId}-{(slot == AbilitySlot.Special ? "special" : "ultimate")}");
            changed |= SetString(serializedObject, "authorCharacterId", characterDefinition.CharacterId);
            changed |= SetString(serializedObject, "displayName", slot == AbilitySlot.Special ? GuessSpecialDisplayName(characterDefinition) : GuessUltimateDisplayName(characterDefinition));
            changed |= SetString(serializedObject, "description", slot == AbilitySlot.Special ? GuessSpecialDescription(characterDefinition) : GuessUltimateDescription(characterDefinition));
            changed |= SetEnum(serializedObject, "slot", (int)slot);
            changed |= SetEnum(serializedObject, "targeting", (int)GuessTargeting(characterDefinition, slot));
            changed |= SetEnum(serializedObject, "effectType", (int)GuessEffectType(characterDefinition, slot));
            changed |= SetInt(serializedObject, "chargeRequirement", slot == AbilitySlot.Special ? characterDefinition.SpecialAbilityChargeRequirement : GuessUltimateChargeRequirement(characterDefinition));
            changed |= SetFloat(serializedObject, "powerValue", slot == AbilitySlot.Special ? characterDefinition.SpecialAbilityDamage : GuessUltimatePowerValue(characterDefinition));

            GuessStatus(characterDefinition, slot, out StatusEffectType statusType, out int statusDuration, out float statusValue);
            changed |= SetEnum(serializedObject, "appliedStatusEffect", (int)statusType);
            changed |= SetInt(serializedObject, "appliedStatusDuration", statusDuration);
            changed |= SetFloat(serializedObject, "appliedStatusValue", statusValue);
            changed |= SetString(serializedObject, "animationTrigger", slot == AbilitySlot.Special ? GuessSpecialAnimationTrigger(characterDefinition) : GuessUltimateAnimationTrigger(characterDefinition));
            changed |= SetColor(serializedObject, "accentColor", characterDefinition.PaletteAccentColor);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if (changed)
                EditorUtility.SetDirty(ability);

            return changed;
        }

        private static string GuessSpecialDisplayName(CharacterDefinition definition)
        {
            return string.IsNullOrWhiteSpace(definition.SpecialAbilityName)
                ? $"{definition.GivenName} Technique"
                : definition.SpecialAbilityName;
        }

        private static string GuessSpecialDescription(CharacterDefinition definition)
        {
            return string.IsNullOrWhiteSpace(definition.SpecialAbilityDescription)
                ? $"{definition.GivenName} commits to a signature battlefield technique."
                : definition.SpecialAbilityDescription;
        }

        private static string GuessUltimateDisplayName(CharacterDefinition definition)
        {
            if (definition.UltimateAbilityDefinition != null && !string.IsNullOrWhiteSpace(definition.UltimateAbilityDefinition.DisplayName))
                return definition.UltimateAbilityDefinition.DisplayName;

            if (CuratedUltimatePresentation.TryGetValue(definition.CharacterId, out (string Name, string Description) presentation))
                return presentation.Name;

            return $"{definition.GivenName} Final Art";
        }

        private static string GuessUltimateDescription(CharacterDefinition definition)
        {
            if (definition.UltimateAbilityDefinition != null && !string.IsNullOrWhiteSpace(definition.UltimateAbilityDefinition.Description))
                return definition.UltimateAbilityDefinition.Description;

            if (CuratedUltimatePresentation.TryGetValue(definition.CharacterId, out (string Name, string Description) presentation))
                return presentation.Description;

            return $"{definition.GivenName} commits fully to a finishing technique meant to close the exchange.";
        }

        private static AbilityTargetingType GuessTargeting(CharacterDefinition definition, AbilitySlot slot)
        {
            string description = (slot == AbilitySlot.Special ? GuessSpecialDescription(definition) : GuessUltimateDescription(definition)).ToLowerInvariant();
            AbilityEffectType effectType = GuessEffectType(definition, slot);

            if (description.Contains("all enemies") || description.Contains("wide area") || description.Contains("wide-area") || description.Contains("wave"))
                return AbilityTargetingType.AllEnemies;
            if (description.Contains("all allies") || description.Contains("allied") || description.Contains("party"))
                return AbilityTargetingType.AllAllies;
            if (description.Contains("self") || description.Contains("user"))
                return AbilityTargetingType.Self;
            if (effectType == AbilityEffectType.Heal)
                return AbilityTargetingType.Self;
            return AbilityTargetingType.SingleEnemy;
        }

        private static AbilityEffectType GuessEffectType(CharacterDefinition definition, AbilitySlot slot)
        {
            string combined = GetAbilitySearchText(definition, slot);
            if (combined.Contains("heal") || combined.Contains("recover") || combined.Contains("restore"))
                return AbilityEffectType.Heal;
            if (combined.Contains("stun") || combined.Contains("freeze") || combined.Contains("slow") || combined.Contains("silence") || combined.Contains("poison") || combined.Contains("burn") || combined.Contains("bleed") || combined.Contains("curse"))
                return AbilityEffectType.ApplyStatus;
            return AbilityEffectType.Damage;
        }

        private static string GuessSpecialAnimationTrigger(CharacterDefinition definition)
        {
            AbilityEffectType effectType = GuessEffectType(definition, AbilitySlot.Special);
            if (effectType == AbilityEffectType.Heal)
                return "isHealing";

            return HasAnimationMapping(definition, "ATTACK 3") ? "PlayState:ATTACK 3" : "SpecialTrigger";
        }

        private static string GuessUltimateAnimationTrigger(CharacterDefinition definition)
        {
            return HasAnimationMapping(definition, "SPECIAL ATTACK")
                ? "SpecialTrigger"
                : GuessSpecialAnimationTrigger(definition);
        }

        private static int GuessUltimateChargeRequirement(CharacterDefinition definition)
        {
            return Mathf.Max(definition.SpecialAbilityChargeRequirement, definition.UltimateAbilityChargeRequirement);
        }

        private static float GuessUltimatePowerValue(CharacterDefinition definition)
        {
            if (definition.UltimateAbilityDefinition != null && definition.UltimateAbilityDefinition.PowerValue > 0f)
                return definition.UltimateAbilityDefinition.PowerValue;

            return Mathf.Max(definition.SpecialAbilityDamage, Mathf.Round(definition.SpecialAbilityDamage * 1.65f));
        }

        private static void GuessStatus(CharacterDefinition definition, AbilitySlot slot, out StatusEffectType type, out int duration, out float value)
        {
            GuessBaseStatus(definition, out type, out duration, out value);

            if (slot != AbilitySlot.Ultimate || duration <= 0)
                return;

            duration += 1;
            if (value > 0f)
                value = Mathf.Max(1f, Mathf.Round(value * 1.5f));
        }

        private static void GuessBaseStatus(CharacterDefinition definition, out StatusEffectType type, out int duration, out float value)
        {
            string combined = GetAbilitySearchText(definition, AbilitySlot.Special);
            type = StatusEffectType.Stun;
            duration = 0;
            value = 0f;

            if (combined.Contains("stun"))
            {
                type = StatusEffectType.Stun;
                duration = 1;
                return;
            }

            if (combined.Contains("freeze") || combined.Contains("slow"))
            {
                type = StatusEffectType.Freeze;
                duration = 1;
                return;
            }

            if (combined.Contains("silence") || combined.Contains("curse"))
            {
                type = StatusEffectType.Silence;
                duration = 1;
                return;
            }

            if (combined.Contains("burn"))
            {
                type = StatusEffectType.Burn;
                duration = 2;
                value = Mathf.Max(1f, Mathf.Round(definition.SpecialAbilityDamage * 0.1f));
                return;
            }

            if (combined.Contains("poison"))
            {
                type = StatusEffectType.Poison;
                duration = 2;
                value = Mathf.Max(1f, Mathf.Round(definition.SpecialAbilityDamage * 0.08f));
                return;
            }

            if (combined.Contains("bleed"))
            {
                type = StatusEffectType.Bleed;
                duration = 2;
                value = Mathf.Max(1f, Mathf.Round(definition.SpecialAbilityDamage * 0.08f));
            }
        }

        private static string GetAbilitySearchText(CharacterDefinition definition, AbilitySlot slot)
        {
            string name = slot == AbilitySlot.Special ? GuessSpecialDisplayName(definition) : GuessUltimateDisplayName(definition);
            string description = slot == AbilitySlot.Special ? GuessSpecialDescription(definition) : GuessUltimateDescription(definition);
            return $"{name} {description}".ToLowerInvariant();
        }

        private static bool HasAnimationMapping(CharacterDefinition definition, string logicalName)
        {
            if (definition == null || definition.animationMappings == null || string.IsNullOrWhiteSpace(logicalName))
                return false;

            return definition.animationMappings.Any(mapping =>
                mapping != null
                && !string.IsNullOrWhiteSpace(mapping.logicalName)
                && string.Equals(mapping.logicalName, logicalName, StringComparison.OrdinalIgnoreCase)
                && mapping.clip != null);
        }

        private static void ValidateAbility(AbilityDefinition definition, ref ValidationReport report)
        {
            string assetPath = AssetDatabase.GetAssetPath(definition);
            if (!assetPath.StartsWith(AbilitiesRoot, StringComparison.OrdinalIgnoreCase))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: ability asset is outside Database/Abilities ({assetPath})");
            }

            if (string.IsNullOrWhiteSpace(definition.AbilityId))
            {
                report.ErrorCount++;
                report.Messages.Add($"ERROR {definition.name}: missing abilityId.");
            }

            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: missing displayName.");
            }

            if (string.IsNullOrWhiteSpace(definition.Description) || CountWords(definition.Description) < 4)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: description is too thin for authored combat data.");
            }

            if (definition.ChargeRequirement <= 0)
            {
                report.ErrorCount++;
                report.Messages.Add($"ERROR {definition.name}: chargeRequirement must be at least 1.");
            }

            if (definition.AnimationTrigger.StartsWith("PlayState:", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(definition.AnimationTrigger.Substring("PlayState:".Length)))
            {
                report.ErrorCount++;
                report.Messages.Add($"ERROR {definition.name}: PlayState animationTrigger is missing a state name.");
            }
        }

        private static int CountWords(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? 0
                : value.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static string ToAssetName(string rawValue)
        {
            string normalized = AbilityDefinition.NormalizeId(rawValue);
            if (string.IsNullOrWhiteSpace(normalized))
                return "Ability";

            return string.Concat(normalized
                .Split('-')
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

        private static bool SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.stringValue == (value ?? string.Empty))
                return false;

            property.stringValue = value ?? string.Empty;
            return true;
        }

        private static bool SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.intValue == value)
                return false;

            property.intValue = value;
            return true;
        }

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
                return false;

            property.floatValue = value;
            return true;
        }

        private static bool SetEnum(SerializedObject serializedObject, string propertyName, int enumValue)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.enumValueIndex == enumValue)
                return false;

            property.enumValueIndex = enumValue;
            return true;
        }

        private static bool SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.colorValue == value)
                return false;

            property.colorValue = value;
            return true;
        }
    }
}
#endif