#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shogun.Features.Characters
{
    public sealed class ProductionAnimationCoverageAuditWindow : EditorWindow
    {
        private const string SpriteRoot = "Assets/_Project/Features/Characters/Art/Production/PlayableSprites";
        private const string AnimationRoot = "Assets/_Project/Features/Characters/Art/Production/Animations";

        private readonly List<CoverageRow> rows = new List<CoverageRow>();
        private Vector2 scrollPosition;
        private bool includeProjectileLikeSheets;
        private string statusMessage = "Run a scan to audit production sprite-sheet coverage.";

        [MenuItem("Shogun/Characters/Production Animation Coverage Audit")]
        public static void ShowWindow()
        {
            GetWindow<ProductionAnimationCoverageAuditWindow>("Production Animation Coverage");
        }

        [MenuItem("Shogun/Characters/Generate Missing Production Animation Clips")]
        public static void GenerateMissingProductionAnimationClipsMenu()
        {
            int generated = ScanAndGenerate(includeProjectileLikeSheets: false, logToConsole: true);
            Debug.Log($"Production animation generation pass complete. Generated {generated} missing clips.");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Production Animation Coverage", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Audits the production sprite-sheet lane by sprite GUID reference, not filename match. This stays inside Art/Production and ignores the legacy archive.", MessageType.Info);

            includeProjectileLikeSheets = EditorGUILayout.ToggleLeft("Include projectile/effect-like sheets", includeProjectileLikeSheets);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scan Coverage", GUILayout.Height(28f)))
            {
                RefreshRows(includeProjectileLikeSheets);
            }

            if (GUILayout.Button("Generate Missing Clips", GUILayout.Height(28f)))
            {
                int generated = ScanAndGenerate(includeProjectileLikeSheets, logToConsole: true);
                RefreshRows(includeProjectileLikeSheets);
                statusMessage = generated > 0
                    ? $"Generated {generated} missing production animation clip(s)."
                    : "No missing production animation clips were found for the current filter.";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(statusMessage, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (CoverageRow row in rows)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(row.CharacterFolder, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Sprite Sheet: {row.SpriteFileName}");
                EditorGUILayout.LabelField($"Status: {(row.HasCoverage ? "Covered" : "Missing clip")}");
                if (!string.IsNullOrEmpty(row.ReferencedClipPath))
                    EditorGUILayout.LabelField($"Clip: {row.ReferencedClipPath}");
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void RefreshRows(bool includeProjectileSheets)
        {
            rows.Clear();
            rows.AddRange(ScanCoverage(includeProjectileSheets));
            int missingCount = rows.Count(row => !row.HasCoverage);
            statusMessage = missingCount > 0
                ? $"Found {missingCount} uncovered production sprite sheet(s)."
                : "All scanned production sprite sheets are already covered by animation clips.";
        }

        private static int ScanAndGenerate(bool includeProjectileLikeSheets, bool logToConsole)
        {
            List<CoverageRow> coverage = ScanCoverage(includeProjectileLikeSheets);
            int generated = 0;
            foreach (CoverageRow row in coverage.Where(entry => !entry.HasCoverage))
            {
                if (CreateAnimationClipFromSpriteSheet(row.SpriteAssetPath, row.CharacterFolder, out string createdPath))
                {
                    generated++;
                    if (logToConsole)
                        Debug.Log($"Generated production animation clip: {createdPath}");
                }
                else if (logToConsole)
                {
                    Debug.LogWarning($"Failed to generate animation clip for {row.SpriteAssetPath}");
                }
            }

            if (generated > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return generated;
        }

        private static List<CoverageRow> ScanCoverage(bool includeProjectileLikeSheets)
        {
            var results = new List<CoverageRow>();
            if (!AssetDatabase.IsValidFolder(SpriteRoot) || !AssetDatabase.IsValidFolder(AnimationRoot))
                return results;

            foreach (string characterFolder in AssetDatabase.GetSubFolders(SpriteRoot))
            {
                string folderName = Path.GetFileName(characterFolder);
                string animationFolder = Path.Combine(AnimationRoot, folderName).Replace('\\', '/');
                string[] clipGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { animationFolder });
                var animationFiles = clipGuids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path))
                    .ToArray();

                string[] spriteGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { characterFolder });
                foreach (string spriteGuid in spriteGuids)
                {
                    string spritePath = AssetDatabase.GUIDToAssetPath(spriteGuid);
                    if (!spritePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!includeProjectileLikeSheets && LooksLikeProjectileOrEffect(spritePath))
                        continue;

                    string referencedClip = animationFiles.FirstOrDefault(path => File.ReadAllText(path).Contains(spriteGuid, StringComparison.OrdinalIgnoreCase));
                    results.Add(new CoverageRow(folderName, spritePath, referencedClip));
                }
            }

            return results.OrderBy(row => row.CharacterFolder, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.SpriteFileName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool CreateAnimationClipFromSpriteSheet(string spritePath, string characterFolderName, out string createdPath)
        {
            createdPath = null;
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritePath);
            Sprite[] sprites = subAssets.OfType<Sprite>().OrderBy(sprite => sprite.name, StringComparer.OrdinalIgnoreCase).ToArray();
            if (sprites.Length == 0)
                return false;

            string animationFolder = Path.Combine(AnimationRoot, characterFolderName).Replace('\\', '/');
            EnsureFolderExists(animationFolder);

            string characterPrefix = BuildCharacterPrefix(characterFolderName);
            string logicalSuffix = BuildLogicalSuffix(Path.GetFileNameWithoutExtension(spritePath));
            createdPath = Path.Combine(animationFolder, $"{characterPrefix}_{logicalSuffix}.anim").Replace('\\', '/');
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(createdPath) != null)
                return false;

            var clip = new AnimationClip
            {
                frameRate = 12f,
                wrapMode = ShouldLoop(logicalSuffix) ? WrapMode.Loop : WrapMode.Once
            };

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            float frameTime = 1f / clip.frameRate;
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameTime,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            SerializedObject serializedClip = new SerializedObject(clip);
            SerializedProperty loopProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
            if (loopProperty != null)
            {
                loopProperty.boolValue = ShouldLoop(logicalSuffix);
                serializedClip.ApplyModifiedPropertiesWithoutUndo();
            }

            AssetDatabase.CreateAsset(clip, createdPath);
            EditorUtility.SetDirty(clip);
            return true;
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent))
                return;

            EnsureFolderExists(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }

        private static string BuildCharacterPrefix(string characterFolderName)
        {
            string[] parts = characterFolderName.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

        private static string BuildLogicalSuffix(string spriteBaseName)
        {
            string normalized = spriteBaseName.Replace('_', ' ').Trim();
            normalized = normalized.Replace("Take Hit", "HURT", StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace("Fall", "JUMP-FALL", StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace("Dead", "DEATH", StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace("Idle", "IDLE", StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace("Run", "RUN", StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace("Walk", "WALK", StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace("Jump", "JUMP", StringComparison.OrdinalIgnoreCase);
            return normalized.ToUpperInvariant();
        }

        private static bool ShouldLoop(string logicalSuffix)
        {
            return logicalSuffix.Contains("IDLE", StringComparison.OrdinalIgnoreCase)
                || logicalSuffix.Contains("RUN", StringComparison.OrdinalIgnoreCase)
                || logicalSuffix.Contains("WALK", StringComparison.OrdinalIgnoreCase)
                || logicalSuffix.Contains("DASH", StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeProjectileOrEffect(string spritePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(spritePath);
            return fileName.Contains("shuriken", StringComparison.OrdinalIgnoreCase)
                || fileName.Contains("projectile", StringComparison.OrdinalIgnoreCase);
        }

        private readonly struct CoverageRow
        {
            public CoverageRow(string characterFolder, string spriteAssetPath, string referencedClipPath)
            {
                CharacterFolder = characterFolder;
                SpriteAssetPath = spriteAssetPath;
                SpriteFileName = Path.GetFileName(spriteAssetPath);
                ReferencedClipPath = referencedClipPath;
                HasCoverage = !string.IsNullOrEmpty(referencedClipPath);
            }

            public string CharacterFolder { get; }
            public string SpriteAssetPath { get; }
            public string SpriteFileName { get; }
            public string ReferencedClipPath { get; }
            public bool HasCoverage { get; }
        }
    }
}
#endif
