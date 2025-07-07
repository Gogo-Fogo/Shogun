#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BatchRenameCharacterFolders : EditorWindow
{
    private string spritesRoot = "Assets/_Project/Features/Characters/Art/Sprites";
    private Dictionary<string, string> folderRenameMap = new Dictionary<string, string> {
        // Samurai asset-pack folders to unique names
        {"Samurai #2 2D Pixel Art", "Katsuro"},
        {"Samurai #3 2D Pixel Art v1.2", "Hayato"},
        {"Samurai #4 2D Pixel Art v1.1", "Daichi"},
        {"Samurai #5 2D Pixel Art v1.1", "Shinobu"},
        {"Samurai #6 2D Pixel Art v1.1", "Takeshi"},
        // Add more mappings as needed for other categories
    };

    [MenuItem("Tools/Shogun/Batch Rename Character Folders")]
    public static void ShowWindow()
    {
        GetWindow<BatchRenameCharacterFolders>("Batch Rename Character Folders");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename Character Folders", EditorStyles.boldLabel);
        spritesRoot = EditorGUILayout.TextField("Sprites Root", spritesRoot);
        if (GUILayout.Button("Rename All Generic Character Folders"))
        {
            RenameAllCharacterFolders();
        }
    }

    private void RenameAllCharacterFolders()
    {
        int renamed = 0;
        foreach (var categoryDir in Directory.GetDirectories(spritesRoot))
        {
            foreach (var charDir in Directory.GetDirectories(categoryDir))
            {
                string folderName = Path.GetFileName(charDir);
                if (folderRenameMap.ContainsKey(folderName))
                {
                    string newName = folderRenameMap[folderName];
                    string newPath = Path.Combine(categoryDir, newName).Replace("\\", "/");
                    if (charDir != newPath)
                    {
                        AssetDatabase.MoveAsset(charDir.Replace("\\", "/"), newPath);
                        Debug.Log($"Renamed folder: {folderName} -> {newName}");
                        renamed++;
                    }
                }
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Batch folder rename complete. Folders renamed: {renamed}");
    }
}
#endif 