#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Shogun.Features.Characters;

public class CharacterAssetBatchOrganizer : EditorWindow
{
    private string spritesRoot = "Assets/_Project/Features/Characters/Art/Sprites";
    private string archiveRoot = "Assets/_Project/Features/Characters/Art/Sprites/Archive";
    private Dictionary<string, string> nameMap = new Dictionary<string, string> {
        // Samurai
        {"Samurai_Archer", "Harada"},
        {"Samurai_Commander", "Takeda"},
        {"NightBorne", "Yoruichi"},
        {"NeonPhantom", "Kagero"},
        {"GandalfHardcore Samurai", "Musashi"},
        {"TheDarkRedOne", "Akai"},
        {"EmeraldProtector", "Midori"},
        {"Martial Hero 2", "Hayato"},
        {"Warrior-in-Red Free", "Shinobu"},
        {"MidnightSlash", "Reiji"},
        // Demons
        {"Demon Samurai 2D Pixel Art", "Oniro"},
        {"FrozenDemon", "Hyoga"},
        {"GhostSentinel", "Yurei"},
        {"Executioner 2D Pixel Art", "Shokeinin"},
        {"RedDemon", "Akaoni"},
        // Animals
        {"Samurai Panda 2D Pixel Art", "Kumada"},
        {"Wolf Samurai 2D Pixel Art v1.1", "OkamiJin"},
        // Ninja
        {"ShadowNinja", "Kuro"},
        {"FreeNinja", "Jin"},
        // Special
        {"Cyber samurai girl", "Aiko"},
        // Yokai
        {"Yokai", "Tsukiko"}
    };
    private List<string> archiveFolders = new List<string> {
        // Samurai asset packs
        "FULL_Samurai 2D Pixel Art v1.2", "Samurai #2 2D Pixel Art", "Samurai #3 2D Pixel Art v1.2", "Samurai #4 2D Pixel Art v1.1", "Samurai #5 2D Pixel Art v1.1", "Samurai #6 2D Pixel Art v1.1", "Samurai",
        // Ninja asset packs
        "craftpix-net-407836-free-ninja-sprite-sheets-pixel-art",
        // Demons asset packs (add as needed)
        // Animals asset packs (add as needed)
        // Special asset packs
        "Fantasy Martial Characters", "Fantasy Martial Characters 3", "Fantasy Martial Characters1.1", "Producto",
        // Yokai asset packs
        "craftpix-net-605776-free-yokai-pixel-art-character-sprites"
    };

    [MenuItem("Tools/Shogun/Batch Organize Character Assets")]
    public static void ShowWindow()
    {
        GetWindow<CharacterAssetBatchOrganizer>("Batch Organize Character Assets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Organize Character Assets", EditorStyles.boldLabel);
        spritesRoot = EditorGUILayout.TextField("Sprites Root", spritesRoot);
        archiveRoot = EditorGUILayout.TextField("Archive Root", archiveRoot);
        if (GUILayout.Button("Run Batch Organization"))
        {
            OrganizeAll();
        }
    }

    private void OrganizeAll()
    {
        int movedCharacters = 0;
        int archivedFolders = 0;
        List<string> report = new List<string>();
        foreach (var categoryDir in Directory.GetDirectories(spritesRoot))
        {
            string category = Path.GetFileName(categoryDir);
            foreach (var folder in Directory.GetDirectories(categoryDir))
            {
                string folderName = Path.GetFileName(folder);
                // Archive asset packs and ambiguous folders
                if (archiveFolders.Contains(folderName))
                {
                    string dest = Path.Combine(archiveRoot, folderName).Replace("\\", "/");
                    AssetDatabase.MoveAsset(folder.Replace("\\", "/"), dest);
                    archivedFolders++;
                    report.Add($"Archived: {category}/{folderName}");
                    continue;
                }
                // Rename/move character folders
                string newName = nameMap.ContainsKey(folderName) ? nameMap[folderName] : folderName;
                string newCharPath = Path.Combine(categoryDir, newName).Replace("\\", "/");
                if (folder != newCharPath)
                {
                    AssetDatabase.MoveAsset(folder.Replace("\\", "/"), newCharPath);
                    report.Add($"Renamed: {category}/{folderName} -> {newName}");
                }
                // Rename all assets inside
                RenameCharacterAssets(newCharPath, newName, report);
                movedCharacters++;
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Batch organization complete. Characters moved: {movedCharacters}, Folders archived: {archivedFolders}\n" + string.Join("\n", report));
    }

    private void RenameCharacterAssets(string charFolder, string charName, List<string> report)
    {
        foreach (var subDir in Directory.GetDirectories(charFolder))
        {
            string sub = Path.GetFileName(subDir);
            foreach (var file in Directory.GetFiles(subDir))
            {
                if (file.EndsWith(".meta")) continue;
                string ext = Path.GetExtension(file);
                string action = Path.GetFileNameWithoutExtension(file);
                // Remove any old prefix
                int idx = action.IndexOf('_');
                if (idx > 0) action = action.Substring(idx + 1);
                string newFileName = charName + "_" + action + ext;
                string dest = Path.Combine(subDir, newFileName).Replace("\\", "/");
                if (file != dest)
                {
                    AssetDatabase.MoveAsset(file.Replace("\\", "/"), dest);
                    report.Add($"Renamed asset: {file} -> {dest}");
                }
            }
        }
        // Animator controller at root or in Animations
        foreach (var file in Directory.GetFiles(charFolder))
        {
            if (file.EndsWith(".controller"))
            {
                string dest = Path.Combine(charFolder, charName + ".controller").Replace("\\", "/");
                if (file != dest)
                {
                    AssetDatabase.MoveAsset(file.Replace("\\", "/"), dest);
                    report.Add($"Renamed controller: {file} -> {dest}");
                }
            }
        }
        // TODO: Update CharacterDefinition and Animator references if needed
    }
}
#endif 