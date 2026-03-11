#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shogun.Features.Characters
{
    public static class CharacterCatalogEditorUtility
    {
        public const string DatabaseRoot = "Assets/_Project/Features/Characters/Database";
        public const string DefinitionsRoot = "Assets/_Project/Features/Characters/Database/Definitions";
        public const string ResourcesRoot = "Assets/_Project/Features/Characters/Database/Resources";
        public const string CatalogAssetPath = ResourcesRoot + "/CharacterCatalog.asset";
        public const string ProductionRoot = "Assets/_Project/Features/Characters/Art/Production";
        public const string PlayableSpritesRoot = ProductionRoot + "/PlayableSprites";
        public const string AnimationsRoot = ProductionRoot + "/Animations";
        public const string PortraitsRoot = ProductionRoot + "/Portraits";
        public const string EventVignettesRoot = ProductionRoot + "/EventVignettes";
        public const string SourceRoot = "Assets/_Project/Features/Characters/Art/Source";

        private static readonly string[] CommonAnimationActions =
        {
            "IDLE",
            "RUN",
            "WALK",
            "ATTACK 1",
            "ATTACK 2",
            "ATTACK 3",
            "HURT",
            "HEALING",
            "DEATH",
            "DEFEND",
            "SPECIAL ATTACK",
            "DASH",
            "JUMP",
            "JUMP-START",
            "JUMP-FALL",
            "JUMP-TRANSITION"
        };

        private static readonly string[] PrototypeIdentityPhrases =
        {
            "debug",
            "sandbox",
            "legacy lane",
            "placeholder",
            "promoted from",
            "built from",
            "used to pressure the player",
            "behavior tests",
            "behavior testing",
            "target-facing behavior",
            "mixed enemy silhouettes"
        };

        private static readonly char[] WordSeparators = { ' ', '\t', '\r', '\n' };

        public struct ValidationReport
        {
            public int ErrorCount;
            public int WarningCount;
            public List<string> Messages;
        }

        public struct SyncReport
        {
            public int CreatedDefinitions;
            public int UpdatedDefinitions;
            public int ErrorCount;
            public int WarningCount;
            public bool CatalogRebuilt;
            public List<string> Messages;
        }

        public static CharacterCatalog GetOrCreateCatalog()
        {
            EnsureFoldersExist();

            CharacterCatalog catalog = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CatalogAssetPath);
            if (catalog != null)
                return catalog;

            catalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        public static bool RebuildCatalog(bool logSummary = true)
        {
            EnsureFoldersExist();

            List<CharacterDefinition> definitions = LoadDefinitionsFromDatabase();
            Dictionary<string, List<CharacterDefinition>> duplicates = definitions
                .GroupBy(definition => definition.CharacterId)
                .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
                .ToDictionary(group => group.Key, group => group.ToList());

            if (duplicates.Count > 0)
            {
                foreach (KeyValuePair<string, List<CharacterDefinition>> duplicate in duplicates)
                {
                    Debug.LogError($"CharacterCatalog: Duplicate characterId '{duplicate.Key}' found on {string.Join(", ", duplicate.Value.Select(definition => definition.name))}");
                }

                return false;
            }

            CharacterCatalog catalog = GetOrCreateCatalog();
            catalog.SetDefinitions(definitions);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            if (logSummary)
                Debug.Log($"CharacterCatalog: Rebuilt with {definitions.Count} definitions.");

            return true;
        }

        public static SyncReport SyncProductionDefinitions(bool logSummary = true)
        {
            EnsureFoldersExist();

            SyncReport report = new SyncReport
            {
                CreatedDefinitions = 0,
                UpdatedDefinitions = 0,
                ErrorCount = 0,
                WarningCount = 0,
                CatalogRebuilt = false,
                Messages = new List<string>()
            };

            HashSet<string> productionCharacterIds = GetProductionCharacterIds();
            Dictionary<string, CharacterDefinition> definitionsById = LoadDefinitionsFromDatabase()
                .Where(definition => definition != null)
                .GroupBy(definition => definition.CharacterId)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            foreach (string characterId in productionCharacterIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                if (!definitionsById.TryGetValue(characterId, out CharacterDefinition definition))
                {
                    definition = CreateCharacterScaffold(characterId, BuildDisplayNameFromId(characterId), string.Empty);
                    if (definition == null)
                    {
                        report.ErrorCount++;
                        report.Messages.Add($"ERROR Failed to create CharacterDefinition scaffold for production character '{characterId}'.");
                        continue;
                    }

                    definitionsById[characterId] = definition;
                    report.CreatedDefinitions++;
                    report.Messages.Add($"Created CharacterDefinition for '{characterId}'.");
                }

                bool changed = SyncDefinitionFromProduction(definition, ref report);
                if (changed)
                {
                    report.UpdatedDefinitions++;
                    report.Messages.Add($"Synced production assets into {definition.name}.");
                }
            }

            report.CatalogRebuilt = RebuildCatalog(false);
            if (!report.CatalogRebuilt)
            {
                report.ErrorCount++;
                report.Messages.Add("ERROR CharacterCatalog rebuild failed due to duplicate ids or invalid definitions.");
            }

            AssetDatabase.SaveAssets();

            if (logSummary)
            {
                Debug.Log($"CharacterCatalog: sync completed. Created {report.CreatedDefinitions} definition(s), updated {report.UpdatedDefinitions} definition(s), warnings {report.WarningCount}, errors {report.ErrorCount}.");
                foreach (string message in report.Messages)
                    Debug.Log(message);
            }

            return report;
        }

        public static int NormalizeProductionSpriteImportSettings(bool logSummary = true)
        {
            EnsureFoldersExist();

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { PlayableSpritesRoot });
            int changedCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!CharacterSpriteImportPolicy.ShouldEnforce(assetPath))
                    continue;

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;

                if (!CharacterSpriteImportPolicy.Apply(importer))
                    continue;

                importer.SaveAndReimport();
                changedCount++;
            }

            if (logSummary)
                Debug.Log($"CharacterCatalog: Normalized import settings on {changedCount} production sprite sheets.");

            return changedCount;
        }

        public static ValidationReport ValidateDatabase()
        {
            ValidationReport report = new ValidationReport
            {
                ErrorCount = 0,
                WarningCount = 0,
                Messages = new List<string>()
            };

            List<CharacterDefinition> definitions = LoadDefinitionsFromDatabase();
            Dictionary<string, List<CharacterDefinition>> duplicates = definitions
                .GroupBy(definition => definition.CharacterId)
                .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (KeyValuePair<string, List<CharacterDefinition>> duplicate in duplicates)
            {
                report.ErrorCount++;
                report.Messages.Add($"ERROR Duplicate characterId '{duplicate.Key}' on {string.Join(", ", duplicate.Value.Select(definition => definition.name))}");
            }

            HashSet<string> productionCharacterIds = GetProductionCharacterIds();
            HashSet<string> definitionIds = new HashSet<string>(definitions.Select(definition => definition.CharacterId), StringComparer.OrdinalIgnoreCase);

            foreach (string productionCharacterId in productionCharacterIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                if (!definitionIds.Contains(productionCharacterId))
                {
                    report.ErrorCount++;
                    report.Messages.Add($"ERROR Production character '{productionCharacterId}' is missing a CharacterDefinition asset.");
                }
            }

            foreach (CharacterDefinition definition in definitions)
            {
                ValidateDefinition(definition, productionCharacterIds, ref report);
            }

            ValidateProductionSpriteImports(ref report);
            ValidateCatalogCoverage(definitions, ref report);

            return report;
        }

        public static CharacterDefinition CreateCharacterScaffold(string rawCharacterId, string givenName, string surname)
        {
            string characterId = CharacterKeyUtility.NormalizeCharacterId(rawCharacterId);
            if (string.IsNullOrWhiteSpace(characterId))
                return null;

            EnsureFoldersExist();
            EnsureCharacterFolders(characterId);

            string assetBaseName = ToAssetName(characterId);
            string assetPath = $"{DefinitionsRoot}/{assetBaseName}_CharacterDefinition.asset";
            CharacterDefinition existing = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(assetPath);
            if (existing != null)
                return existing;

            CharacterDefinition definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            SerializedObject serializedObject = new SerializedObject(definition);
            serializedObject.FindProperty("characterId").stringValue = characterId;
            serializedObject.FindProperty("givenName").stringValue = string.IsNullOrWhiteSpace(givenName) ? BuildDisplayNameFromId(characterId) : givenName;
            serializedObject.FindProperty("surname").stringValue = surname ?? string.Empty;
            serializedObject.FindProperty("characterName").stringValue = string.IsNullOrWhiteSpace(givenName) ? BuildDisplayNameFromId(characterId) : givenName;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(definition, assetPath);
            AssetDatabase.SaveAssets();
            RebuildCatalog(false);
            return definition;
        }

        public static void EnsureFoldersExist()
        {
            Directory.CreateDirectory(DatabaseRoot);
            Directory.CreateDirectory(DefinitionsRoot);
            Directory.CreateDirectory(ResourcesRoot);
            Directory.CreateDirectory(ProductionRoot);
            Directory.CreateDirectory(PlayableSpritesRoot);
            Directory.CreateDirectory(AnimationsRoot);
            Directory.CreateDirectory(PortraitsRoot);
            Directory.CreateDirectory(EventVignettesRoot);
            Directory.CreateDirectory(SourceRoot);
            AssetDatabase.Refresh();
        }

        public static void EnsureCharacterFolders(string characterId)
        {
            Directory.CreateDirectory(Path.Combine(PlayableSpritesRoot, characterId));
            Directory.CreateDirectory(Path.Combine(AnimationsRoot, characterId));
            Directory.CreateDirectory(Path.Combine(PortraitsRoot, characterId));
            Directory.CreateDirectory(Path.Combine(EventVignettesRoot, characterId));
            Directory.CreateDirectory(Path.Combine(SourceRoot, "Aseprite", characterId));
            Directory.CreateDirectory(Path.Combine(SourceRoot, "Gemini", characterId));
            Directory.CreateDirectory(Path.Combine(SourceRoot, "PixelLab", characterId));
            AssetDatabase.Refresh();
        }

        private static List<CharacterDefinition> LoadDefinitionsFromDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterDefinition", new[] { DefinitionsRoot });
            List<CharacterDefinition> definitions = new List<CharacterDefinition>(guids.Length);

            foreach (string guid in guids)
            {
                CharacterDefinition definition = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (definition != null)
                    definitions.Add(definition);
            }

            return definitions.OrderBy(definition => definition.CharacterId).ToList();
        }

        private static HashSet<string> GetProductionCharacterIds()
        {
            HashSet<string> characterIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddFolderIds(PlayableSpritesRoot, characterIds);
            AddFolderIds(AnimationsRoot, characterIds);
            return characterIds;
        }

        private static void AddFolderIds(string rootPath, HashSet<string> target)
        {
            if (!AssetDatabase.IsValidFolder(rootPath))
                return;

            foreach (string folderPath in AssetDatabase.GetSubFolders(rootPath))
            {
                string normalized = CharacterKeyUtility.NormalizeCharacterId(Path.GetFileName(folderPath));
                if (!string.IsNullOrWhiteSpace(normalized))
                    target.Add(normalized);
            }
        }

        private static bool SyncDefinitionFromProduction(CharacterDefinition definition, ref SyncReport report)
        {
            bool changed = false;
            string characterId = definition.CharacterId;
            SerializedObject serializedObject = new SerializedObject(definition);

            Sprite battleSprite = FindBattleSprite(characterId);
            RuntimeAnimatorController animatorController = FindAnimatorController(characterId);
            Sprite portrait = FindSupportSprite(PortraitsRoot, characterId, "portrait");
            Sprite pfpSprite = FindSupportSprite(PortraitsRoot, characterId, "pfp");
            Sprite bannerSprite = FindSupportSprite(PortraitsRoot, characterId, "banner", "bannerArt", "attack");
            Sprite comboCutInSprite = FindSupportSprite(PortraitsRoot, characterId, "comboCutIn", "combo", "attackCutIn");
            Sprite ultimateCutInSprite = FindSupportSprite(PortraitsRoot, characterId, "ultimateCutIn", "ultimate", "special");
            Sprite eventVignette = FindSupportSprite(EventVignettesRoot, characterId, "eventVignette", "vignette", "event");

            changed |= AssignReference(serializedObject, "battleSprite", battleSprite);
            changed |= AssignReference(serializedObject, "animatorController", animatorController);
            changed |= AssignReference(serializedObject, "portrait", portrait);
            changed |= AssignReference(serializedObject, "pfpSprite", pfpSprite);
            changed |= AssignReference(serializedObject, "bannerSprite", bannerSprite);
            changed |= AssignReference(serializedObject, "comboCutInSprite", comboCutInSprite);
            changed |= AssignReference(serializedObject, "ultimateCutInSprite", ultimateCutInSprite);
            changed |= AssignReference(serializedObject, "eventVignette", eventVignette);
            changed |= SyncAnimationMappings(definition, characterId);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if (battleSprite == null)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: no production battle sprite found in {PlayableSpritesRoot}/{characterId}.");
            }

            if (animatorController == null)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: no production animator controller found in {AnimationsRoot}/{characterId}.");
            }

            if (changed)
                EditorUtility.SetDirty(definition);

            return changed;
        }

        private static bool SyncAnimationMappings(CharacterDefinition definition, string characterId)
        {
            bool changed = false;
            HashSet<string> existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (definition.animationMappings == null)
                definition.animationMappings = new List<AnimationMapping>();

            for (int i = 0; i < definition.animationMappings.Count; i++)
            {
                AnimationMapping mapping = definition.animationMappings[i];
                if (mapping == null || string.IsNullOrWhiteSpace(mapping.logicalName))
                    continue;
                existingNames.Add(mapping.logicalName);
            }

            foreach (string action in CommonAnimationActions)
            {
                if (existingNames.Contains(action))
                    continue;

                definition.animationMappings.Add(new AnimationMapping(action));
                changed = true;
            }

            foreach (AnimationMapping mapping in definition.animationMappings)
            {
                if (mapping == null || mapping.clip != null || string.IsNullOrWhiteSpace(mapping.logicalName))
                    continue;

                AnimationClip clip = FindAnimationClip(characterId, mapping.logicalName);
                if (clip == null)
                    continue;

                mapping.clip = clip;
                changed = true;
            }

            return changed;
        }

        private static bool AssignReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            if (value == null)
                return false;

            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
                return false;

            property.objectReferenceValue = value;
            return true;
        }

        private static Sprite FindBattleSprite(string characterId)
        {
            string folder = $"{PlayableSpritesRoot}/{characterId}";
            if (!AssetDatabase.IsValidFolder(folder))
                return null;

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            string[] preferredKeywords = { "IDLE", "Idle", "idle", "WALK", "Walk", "walk" };

            foreach (string keyword in preferredKeywords)
            {
                foreach (string guid in textureGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    if (!fileName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (LooksLikeProjectileOrEffect(fileName))
                        continue;

                    Sprite sprite = LoadRepresentativeSprite(assetPath);
                    if (sprite != null)
                        return sprite;
                }
            }

            foreach (string guid in textureGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (LooksLikeProjectileOrEffect(fileName))
                    continue;

                Sprite sprite = LoadRepresentativeSprite(assetPath);
                if (sprite != null)
                    return sprite;
            }

            return null;
        }

        private static RuntimeAnimatorController FindAnimatorController(string characterId)
        {
            string folder = $"{AnimationsRoot}/{characterId}";
            if (!AssetDatabase.IsValidFolder(folder))
                return null;

            string expectedName = $"{ToAssetName(characterId)}_{ToAssetName(characterId)}";
            string[] controllerGuids = AssetDatabase.FindAssets("t:RuntimeAnimatorController", new[] { folder });

            foreach (string guid in controllerGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
                if (controller != null && controller.name.Equals(expectedName, StringComparison.OrdinalIgnoreCase))
                    return controller;
            }

            foreach (string guid in controllerGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
                if (controller != null)
                    return controller;
            }

            return null;
        }

        private static AnimationClip FindAnimationClip(string characterId, string logicalName)
        {
            string folder = $"{AnimationsRoot}/{characterId}";
            if (!AssetDatabase.IsValidFolder(folder))
                return null;

            string[] clipGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folder });
            List<AnimationClip> clips = clipGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<AnimationClip>(path))
                .Where(clip => clip != null)
                .ToList();

            string prefix = ToAssetName(characterId);
            string[] candidates =
            {
                $"{prefix}_{logicalName}",
                $"{prefix}_{logicalName.Replace('_', ' ')}",
                logicalName
            };

            foreach (string candidate in candidates)
            {
                AnimationClip clip = clips.FirstOrDefault(existing => existing.name.Equals(candidate, StringComparison.OrdinalIgnoreCase));
                if (clip != null)
                    return clip;
            }

            string normalizedLogicalName = logicalName.Replace(" ", string.Empty).Replace("-", string.Empty);
            return clips.FirstOrDefault(existing => existing.name.Replace(" ", string.Empty).Replace("-", string.Empty).Contains(normalizedLogicalName, StringComparison.OrdinalIgnoreCase));
        }

        private static Sprite FindSupportSprite(string rootPath, string characterId, params string[] roleTokens)
        {
            if (!AssetDatabase.IsValidFolder(rootPath))
                return null;

            string directFolder = $"{rootPath}/{characterId}";
            string assetPrefix = ToAssetName(characterId);

            if (AssetDatabase.IsValidFolder(directFolder))
            {
                Sprite directMatch = FindSupportSpriteInFolder(directFolder, assetPrefix, roleTokens);
                if (directMatch != null)
                    return directMatch;
            }

            return FindSupportSpriteInFolder(rootPath, assetPrefix, roleTokens);
        }

        private static Sprite FindSupportSpriteInFolder(string folderPath, string assetPrefix, params string[] roleTokens)
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            if (textureGuids == null || textureGuids.Length == 0)
                return null;

            foreach (string normalizedRole in NormalizeRoleTokens(roleTokens))
            {
                foreach (string guid in textureGuids.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    if (!MatchesSupportSprite(fileName, assetPrefix, normalizedRole))
                        continue;

                    Sprite sprite = LoadRepresentativeSprite(assetPath);
                    if (sprite != null)
                        return sprite;
                }
            }

            return null;
        }

        private static IEnumerable<string> NormalizeRoleTokens(IEnumerable<string> roleTokens)
        {
            return (roleTokens ?? Array.Empty<string>())
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Select(NormalizeToken)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static bool MatchesSupportSprite(string fileName, string assetPrefix, string normalizedRole)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(normalizedRole))
                return false;

            string normalizedFileName = NormalizeToken(fileName);
            string normalizedPrefix = NormalizeToken(assetPrefix);

            if (!string.IsNullOrEmpty(normalizedPrefix) && normalizedFileName.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
                normalizedFileName = normalizedFileName.Substring(normalizedPrefix.Length);

            return normalizedFileName.Equals(normalizedRole, StringComparison.OrdinalIgnoreCase)
                || normalizedFileName.EndsWith(normalizedRole, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }

        private static Sprite LoadRepresentativeSprite(string assetPath)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            return assets.OfType<Sprite>().OrderBy(sprite => sprite.name, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        }

        private static bool LooksLikeProjectileOrEffect(string fileName)
        {
            return fileName.Contains("shuriken", StringComparison.OrdinalIgnoreCase)
                || fileName.Contains("projectile", StringComparison.OrdinalIgnoreCase)
                || fileName.Contains("charge", StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateDefinition(CharacterDefinition definition, HashSet<string> productionCharacterIds, ref ValidationReport report)
        {
            string definitionPath = AssetDatabase.GetAssetPath(definition);
            if (!definitionPath.StartsWith(DefinitionsRoot, StringComparison.OrdinalIgnoreCase))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: definition is outside Database/Definitions ({definitionPath})");
            }

            if (!definition.HasExplicitCharacterId)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: characterId is not explicitly serialized.");
            }

            if (!productionCharacterIds.Contains(definition.CharacterId))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: no matching production folder exists for characterId '{definition.CharacterId}'.");
            }

            if (definition.BattleSprite == null)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: missing battle sprite.");
            }

            if (definition.BattleSprite != null && definition.AnimatorController == null)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: missing animator controller.");
            }

            ValidateProductionReference(definition.name, "battleSprite", definition.BattleSprite, ref report);
            ValidateProductionReference(definition.name, "portrait", definition.Portrait, ref report);
            ValidateProductionReference(definition.name, "pfpSprite", definition.PfpSprite, ref report);
            ValidateProductionReference(definition.name, "bannerSprite", definition.BannerSprite, ref report);
            ValidateProductionReference(definition.name, "comboCutInSprite", definition.ComboCutInSprite, ref report);
            ValidateProductionReference(definition.name, "ultimateCutInSprite", definition.UltimateCutInSprite, ref report);
            ValidateProductionReference(definition.name, "eventVignette", definition.EventVignette, ref report);
            ValidateProductionReference(definition.name, "animatorController", definition.AnimatorController, ref report);

            ValidateIdentityText(definition.name, "description", definition.Description, minimumWordCount: 6, checkPrototypePhrases: true, ref report);
            ValidateIdentityText(definition.name, "specialAbilityDescription", definition.SpecialAbilityDescription, minimumWordCount: 4, checkPrototypePhrases: true, ref report);
            ValidateIdentityText(definition.name, "collectibleTone", definition.CollectibleTone, minimumWordCount: 1, checkPrototypePhrases: false, ref report);
            ValidateIdentityText(definition.name, "visualHook", definition.VisualHook, minimumWordCount: 3, checkPrototypePhrases: true, ref report);
            ValidateIdentityText(definition.name, "emotionalHook", definition.EmotionalHook, minimumWordCount: 1, checkPrototypePhrases: false, ref report);
            ValidateIdentityText(definition.name, "loreBlurb", definition.LoreBlurb, minimumWordCount: 7, checkPrototypePhrases: true, ref report);
            ValidateIdentityText(definition.name, "variantPotential", definition.VariantPotential, minimumWordCount: 3, checkPrototypePhrases: true, ref report);
            ValidateCollectibleTone(definition.name, definition.CollectibleTone, ref report);

            foreach (AnimationMapping mapping in definition.animationMappings)
            {
                if (mapping == null)
                    continue;
                ValidateProductionReference(definition.name, $"animationMappings[{mapping.logicalName}]", mapping.clip, ref report);
            }
        }

        private static void ValidateProductionSpriteImports(ref ValidationReport report)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { PlayableSpritesRoot });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!CharacterSpriteImportPolicy.ShouldEnforce(assetPath))
                    continue;

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;

                if (CharacterSpriteImportPolicy.IsCompliant(importer, out string issues))
                    continue;

                report.WarningCount++;
                report.Messages.Add($"WARN Sprite import quality mismatch at {assetPath}: {issues}");
            }
        }

        private static void ValidateCatalogCoverage(List<CharacterDefinition> definitions, ref ValidationReport report)
        {
            CharacterCatalog catalog = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                report.WarningCount++;
                report.Messages.Add("WARN CharacterCatalog asset is missing.");
                return;
            }

            if (catalog.Definitions.Any(definition => definition == null))
            {
                report.ErrorCount++;
                report.Messages.Add("ERROR CharacterCatalog contains null definitions.");
                return;
            }

            HashSet<string> definitionIds = new HashSet<string>(definitions.Select(definition => definition.CharacterId), StringComparer.OrdinalIgnoreCase);
            HashSet<string> catalogIds = new HashSet<string>(catalog.Definitions.Where(definition => definition != null).Select(definition => definition.CharacterId), StringComparer.OrdinalIgnoreCase);

            foreach (string definitionId in definitionIds)
            {
                if (!catalogIds.Contains(definitionId))
                {
                    report.ErrorCount++;
                    report.Messages.Add($"ERROR CharacterCatalog is missing '{definitionId}'.");
                }
            }

            foreach (string catalogId in catalogIds)
            {
                if (!definitionIds.Contains(catalogId))
                {
                    report.ErrorCount++;
                    report.Messages.Add($"ERROR CharacterCatalog references '{catalogId}' which is not present in Database/Definitions.");
                }
            }
        }

        private static void ValidateProductionReference(string definitionName, string label, UnityEngine.Object reference, ref ValidationReport report)
        {
            if (reference == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(reference);
            if (!string.IsNullOrEmpty(assetPath) && !assetPath.StartsWith(ProductionRoot, StringComparison.OrdinalIgnoreCase))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definitionName}: {label} points outside Art/Production ({assetPath})");
            }
        }

        private static void ValidateIdentityText(string definitionName, string fieldLabel, string value, int minimumWordCount, bool checkPrototypePhrases, ref ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definitionName}: missing {fieldLabel}.");
                return;
            }

            int wordCount = CountWords(value);
            if (minimumWordCount > 1 && wordCount < minimumWordCount)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definitionName}: {fieldLabel} is too thin for a production definition ('{value}').");
            }

            if (!checkPrototypePhrases)
                return;

            string lowerValue = value.ToLowerInvariant();
            foreach (string phrase in PrototypeIdentityPhrases)
            {
                if (!lowerValue.Contains(phrase))
                    continue;

                report.WarningCount++;
                report.Messages.Add($"WARN {definitionName}: {fieldLabel} still contains prototype phrasing ('{phrase}').");
                break;
            }
        }

        private static void ValidateCollectibleTone(string definitionName, string collectibleTone, ref ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(collectibleTone))
                return;

            if (CountWords(collectibleTone) > 1)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definitionName}: collectibleTone should stay a compact one-word tone ('{collectibleTone}').");
            }
        }

        private static int CountWords(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? 0
                : value.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        private static string ToAssetName(string characterId)
        {
            return string.Concat(characterId
                .Split('-')
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

        private static string BuildDisplayNameFromId(string characterId)
        {
            string[] parts = characterId
                .Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1))
                .ToArray();
            return parts.Length == 0 ? "Unknown" : string.Join(" ", parts);
        }
    }
}
#endif

