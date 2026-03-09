#if UNITY_EDITOR
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

        public struct ValidationReport
        {
            public int ErrorCount;
            public int WarningCount;
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

            foreach (CharacterDefinition definition in definitions)
            {
                ValidateDefinition(definition, ref report);
            }

            ValidateProductionSpriteImports(ref report);

            CharacterCatalog catalog = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                report.WarningCount++;
                report.Messages.Add("WARN CharacterCatalog asset is missing.");
            }
            else if (catalog.Definitions.Any(definition => definition == null))
            {
                report.ErrorCount++;
                report.Messages.Add("ERROR CharacterCatalog contains null definitions.");
            }

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
            serializedObject.FindProperty("givenName").stringValue = string.IsNullOrWhiteSpace(givenName) ? assetBaseName : givenName;
            serializedObject.FindProperty("surname").stringValue = surname ?? string.Empty;
            serializedObject.FindProperty("characterName").stringValue = string.IsNullOrWhiteSpace(givenName) ? assetBaseName : givenName;
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

        private static void ValidateDefinition(CharacterDefinition definition, ref ValidationReport report)
        {
            string definitionPath = AssetDatabase.GetAssetPath(definition);
            if (!definitionPath.StartsWith(DefinitionsRoot))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: definition is outside Database/Definitions ({definitionPath})");
            }

            if (!definition.HasExplicitCharacterId)
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definition.name}: characterId is not explicitly serialized.");
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
            ValidateProductionReference(definition.name, "bannerSprite", definition.BannerSprite, ref report);
            ValidateProductionReference(definition.name, "eventVignette", definition.EventVignette, ref report);
            ValidateProductionReference(definition.name, "animatorController", definition.AnimatorController, ref report);

            foreach (AnimationMapping mapping in definition.animationMappings)
            {
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

        private static void ValidateProductionReference(string definitionName, string label, Object reference, ref ValidationReport report)
        {
            if (reference == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(reference);
            if (!string.IsNullOrEmpty(assetPath) && !assetPath.StartsWith(ProductionRoot))
            {
                report.WarningCount++;
                report.Messages.Add($"WARN {definitionName}: {label} points outside Art/Production ({assetPath})");
            }
        }

        private static string ToAssetName(string characterId)
        {
            return string.Concat(characterId
                .Split('-')
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }
    }
}
#endif
