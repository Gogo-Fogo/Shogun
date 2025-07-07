#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class BatchRenameAllCharacterSprites : EditorWindow
{
    private string spritesRoot = "Assets/_Project/Features/Characters/Art/Sprites";

    [MenuItem("Tools/Shogun/Batch Rename All Character Sprites")]
    public static void ShowWindow()
    {
        GetWindow<BatchRenameAllCharacterSprites>("Batch Rename All Character Sprites");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename All Character Sprites", EditorStyles.boldLabel);
        spritesRoot = EditorGUILayout.TextField("Sprites Root", spritesRoot);
        if (GUILayout.Button("Rename All Sprites & Anims"))
        {
            RenameAllCharacterSprites();
        }
    }

    private void RenameAllCharacterSprites()
    {
        int totalRenamed = 0;
        int foldersProcessed = 0;
        foreach (var categoryDir in Directory.GetDirectories(spritesRoot))
        {
            foreach (var charDir in Directory.GetDirectories(categoryDir))
            {
                string charName = Path.GetFileName(charDir);
                int renamed = RenameSpritesInFolder(charDir, charName);
                if (renamed > 0)
                    Debug.Log($"Renamed {renamed} files in {charName}");
                totalRenamed += renamed;
                foldersProcessed++;
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Batch rename complete. Folders processed: {foldersProcessed}, Files renamed: {totalRenamed}");
    }

    private int RenameSpritesInFolder(string folder, string charName)
    {
        int renamed = 0;
        var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (file.EndsWith(".meta")) continue;
            string ext = Path.GetExtension(file).ToLower();
            if (ext != ".png" && ext != ".anim") continue;
            string fileName = Path.GetFileName(file);
            if (fileName.StartsWith(charName + "_")) continue;
            string action = Path.GetFileNameWithoutExtension(file);
            // Remove any old prefix
            int idx = action.IndexOf('_');
            if (idx > 0) action = action.Substring(idx + 1);
            string newFileName = charName + "_" + action + ext;
            string dest = Path.Combine(Path.GetDirectoryName(file), newFileName).Replace("\\", "/");
            if (file != dest)
            {
                AssetDatabase.MoveAsset(file.Replace("\\", "/"), dest);
                renamed++;
            }
        }
        return renamed;
    }
}
#endif 