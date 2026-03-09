#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BatchMoveAndRenameSpriteSheets : EditorWindow
{
    private string spritesRoot = "Assets/_Project/Features/Characters/Art/Sprites";
    private string archiveRoot = "Assets/_Project/Features/Characters/Art/Sprites/Archive";
    // Map from asset pack file name to (character, action)
    private Dictionary<string, (string character, string action)> spriteSheetMap = new Dictionary<string, (string, string)> {
        // Example: {"cyber_samurai_girl_cast.png", ("Aiko", "CastSpell")}
        // Add more mappings as needed
    };

    [MenuItem("Tools/Shogun/Batch Move & Rename Sprite Sheets")]
    public static void ShowWindow()
    {
        GetWindow<BatchMoveAndRenameSpriteSheets>("Batch Move & Rename Sprite Sheets");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Deprecated. Use Tools/Shogun/Character Database for catalog-managed asset migration.", MessageType.Warning);
        if (GUILayout.Button("Open Character Database"))
        {
            Shogun.Features.Characters.CharacterDatabaseEditorWindow.ShowWindow();
        }
    }

    private void MoveAndRenameSpriteSheets()
    {
        int moved = 0;
        int skipped = 0;
        List<string> report = new List<string>();
        foreach (var categoryDir in Directory.GetDirectories(archiveRoot))
        {
            foreach (var file in Directory.GetFiles(categoryDir, "*.png", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(file);
                if (spriteSheetMap.ContainsKey(fileName))
                {
                    var (character, action) = spriteSheetMap[fileName];
                    string destFolder = Path.Combine(spritesRoot, categoryDir.Substring(categoryDir.LastIndexOf('/') + 1), character, "Sprites").Replace("\\", "/");
                    Directory.CreateDirectory(destFolder);
                    string destFile = Path.Combine(destFolder, character + "_" + action + ".png").Replace("\\", "/");
                    AssetDatabase.MoveAsset(file.Replace("\\", "/"), destFile);
                    report.Add($"Moved & renamed: {fileName} -> {destFile}");
                    moved++;
                }
                else
                {
                    report.Add($"Skipped (no mapping): {fileName}");
                    skipped++;
                }
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Batch move/rename complete. Sprite sheets moved: {moved}, Skipped: {skipped}\n" + string.Join("\n", report));
    }
}
#endif
