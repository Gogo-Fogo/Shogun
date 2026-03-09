#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class ArchiveOriginalAssetPackEditor : EditorWindow
{
    private string sourceFolder = "Assets/_Project/Features/Characters/Art/Sprites/Samurai/FULL_Samurai 2D Pixel Art v1.2/FULL_Samurai 2D Pixel Art v1.2";
    private string archiveFolder = "Assets/_Project/Features/Characters/Art/Archive/FULL_Samurai_2D_Pixel_Art_v1.2";
    private string[] excludeFolders = new string[] { "Ryoma" }; // Add any character folders to exclude

    [MenuItem("Tools/Shogun/Archive Original Asset Pack")]
    public static void ShowWindow()
    {
        GetWindow<ArchiveOriginalAssetPackEditor>("Archive Original Asset Pack");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Deprecated. Use Tools/Shogun/Character Database for catalog-managed asset migration.", MessageType.Warning);
        if (GUILayout.Button("Open Character Database"))
        {
            Shogun.Features.Characters.CharacterDatabaseEditorWindow.ShowWindow();
        }
    }

    private void ArchivePack()
    {
        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError($"Source folder does not exist: {sourceFolder}");
            return;
        }
        Directory.CreateDirectory(archiveFolder);

        var entries = Directory.GetFileSystemEntries(sourceFolder);
        foreach (var entry in entries)
        {
            string name = Path.GetFileName(entry);
            bool isExcluded = false;
            foreach (var excl in excludeFolders)
            {
                if (!string.IsNullOrEmpty(excl) && name.Equals(excl, System.StringComparison.OrdinalIgnoreCase))
                {
                    isExcluded = true;
                    break;
                }
            }
            if (isExcluded) continue;

            string destPath = Path.Combine(archiveFolder, name).Replace("\\", "/");
            string srcPath = entry.Replace("\\", "/");
            if (Directory.Exists(srcPath))
            {
                AssetDatabase.MoveAsset(srcPath, destPath);
            }
            else if (File.Exists(srcPath))
            {
                AssetDatabase.MoveAsset(srcPath, destPath);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Archived all original pack files from {sourceFolder} to {archiveFolder} (excluding: {string.Join(", ", excludeFolders)})");
    }
}
#endif
