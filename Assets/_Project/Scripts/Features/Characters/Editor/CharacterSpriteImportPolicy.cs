#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shogun.Features.Characters
{
    internal static class CharacterSpriteImportPolicy
    {
        private static readonly string[] EnforcedPlatformNames =
        {
            "DefaultTexturePlatform",
            "Standalone",
            "Android",
            "iPhone",
            "WebGL",
            "WindowsStoreApps",
            "Windows Store Apps"
        };

        public static bool ShouldEnforce(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath)
                   && assetPath.StartsWith(CharacterCatalogEditorUtility.PlayableSpritesRoot);
        }

        public static bool IsCompliant(TextureImporter importer, out string issues)
        {
            List<string> issueList = new List<string>();
            TextureImporterSettings textureSettings = GetTextureSettings(importer);

            if (importer.filterMode != FilterMode.Point)
                issueList.Add("filterMode must be Point");

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                issueList.Add("texture compression must be Uncompressed");

            if (textureSettings.spriteMeshType != SpriteMeshType.FullRect)
                issueList.Add("sprite mesh must be Full Rect");

            if (importer.mipmapEnabled)
                issueList.Add("mipmaps must be disabled");

            foreach (string platformName in EnforcedPlatformNames)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platformName);
                if (settings.textureCompression != TextureImporterCompression.Uncompressed)
                    issueList.Add($"{platformName} compression must be Uncompressed");
            }

            issues = string.Join(", ", issueList);
            return issueList.Count == 0;
        }

        public static bool Apply(TextureImporter importer)
        {
            bool changed = false;
            TextureImporterSettings textureSettings = GetTextureSettings(importer);

            changed |= SetIfDifferent(importer.filterMode, FilterMode.Point, value => importer.filterMode = value);
            changed |= SetIfDifferent(importer.textureCompression, TextureImporterCompression.Uncompressed, value => importer.textureCompression = value);
            changed |= SetIfDifferent(importer.mipmapEnabled, false, value => importer.mipmapEnabled = value);
            changed |= SetIfDifferent(textureSettings.spriteMeshType, SpriteMeshType.FullRect, value => textureSettings.spriteMeshType = value);

            foreach (string platformName in EnforcedPlatformNames)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platformName);
                bool platformChanged = false;

                if (settings.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    settings.textureCompression = TextureImporterCompression.Uncompressed;
                    platformChanged = true;
                }

                if (settings.crunchedCompression)
                {
                    settings.crunchedCompression = false;
                    platformChanged = true;
                }

                if (platformChanged)
                {
                    importer.SetPlatformTextureSettings(settings);
                    changed = true;
                }
            }

            if (changed)
                importer.SetTextureSettings(textureSettings);

            return changed;
        }

        private static TextureImporterSettings GetTextureSettings(TextureImporter importer)
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            return settings;
        }

        private static bool SetIfDifferent<T>(T currentValue, T expectedValue, System.Action<T> apply)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, expectedValue))
                return false;

            apply(expectedValue);
            return true;
        }
    }
}
#endif
