#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class BatchRenameCharacterSprites : EditorWindow
{
    private string characterName = "Hayato";
    private string targetFolder = "Assets/_Project/Features/Characters/Art/Sprites/Samurai/Hayato";

    [MenuItem("Tools/Shogun/Batch Rename Character Sprites")]
    public static void ShowWindow()
    {
        GetWindow<BatchRenameCharacterSprites>("Batch Rename Character Sprites");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename Character Sprites", EditorStyles.boldLabel);
        characterName = EditorGUILayout.TextField("Character Name", characterName);
        targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
        if (GUILayout.Button("Rename All Sprites & Anims"))
        {
            RenameAllSpritesAndAnims();
        }
    }

    private void RenameAllSpritesAndAnims()
    {
        int renamed = 0;
        if (!Directory.Exists(targetFolder))
        {
            Debug.LogError($"Target folder does not exist: {targetFolder}");
            return;
        }
        var files = Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (file.EndsWith(".meta")) continue;
            string ext = Path.GetExtension(file).ToLower();
            if (ext != ".png" && ext != ".anim") continue;
            string fileName = Path.GetFileName(file);
            if (fileName.StartsWith(characterName + "_")) continue;
            string action = Path.GetFileNameWithoutExtension(file);
            // Remove any old prefix
            int idx = action.IndexOf('_');
            if (idx > 0) action = action.Substring(idx + 1);
            string newFileName = characterName + "_" + action + ext;
            string dest = Path.Combine(Path.GetDirectoryName(file), newFileName).Replace("\\", "/");
            if (file != dest)
            {
                AssetDatabase.MoveAsset(file.Replace("\\", "/"), dest);
                renamed++;
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Batch rename complete. Files renamed: {renamed}");
    }
}
#endif 