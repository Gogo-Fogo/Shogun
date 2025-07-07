#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class AutoMoveAllSpriteSheets : EditorWindow
{
    private string spritesRoot = "Assets/_Project/Features/Characters/Art/Sprites";
    private string archiveRoot = "Assets/_Project/Features/Characters/Art/Sprites/Archive";

    [MenuItem("Tools/Shogun/Auto Move All Sprite Sheets")]
    public static void ShowWindow()
    {
        GetWindow<AutoMoveAllSpriteSheets>("Auto Move All Sprite Sheets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Move All Sprite Sheets", EditorStyles.boldLabel);
        spritesRoot = EditorGUILayout.TextField("Sprites Root", spritesRoot);
        archiveRoot = EditorGUILayout.TextField("Archive Root", archiveRoot);
        if (GUILayout.Button("Move All Sprite Sheets"))
        {
            MoveAllSpriteSheets();
        }
    }

    private void MoveAllSpriteSheets()
    {
        int moved = 0;
        int skipped = 0;
        foreach (var categoryDir in Directory.GetDirectories(archiveRoot))
        {
            string category = Path.GetFileName(categoryDir);
            foreach (var charDir in Directory.GetDirectories(categoryDir))
            {
                string character = Path.GetFileName(charDir);
                string[] pngs = Directory.GetFiles(charDir, "*.png", SearchOption.AllDirectories);
                foreach (var file in pngs)
                {
                    string action = "SpriteSheet";
                    string origName = Path.GetFileNameWithoutExtension(file);
                    // If the original name contains an action, use it
                    int idx = origName.IndexOf('_');
                    if (idx > 0 && idx < origName.Length - 1)
                        action = origName.Substring(idx + 1);
                    string destFolder = Path.Combine(spritesRoot, category, character, "Sprites").Replace("\\", "/");
                    if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
                    string destFile = Path.Combine(destFolder, character + "_" + action + ".png").Replace("\\", "/");
                    if (AssetDatabase.MoveAsset(file.Replace("\\", "/"), destFile) == "")
                    {
                        Debug.Log($"Moved & renamed: {file} -> {destFile}");
                        moved++;
                    }
                    else
                    {
                        Debug.LogWarning($"Skipped (could not move): {file}");
                        skipped++;
                    }
                }
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Auto move/rename complete. Sprite sheets moved: {moved}, Skipped: {skipped}");
    }
}
#endif 