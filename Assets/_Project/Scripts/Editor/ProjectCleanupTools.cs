#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public class ProjectCleanupTools : EditorWindow
{
    private string projectRoot = "Assets/_Project";
    private bool deleteEmptyFolders = true;
    private bool reportOrphanedMetas = true;
    private bool reportDuplicates = true;
    private bool deleteOrphanedMetas = false;

    [MenuItem("Tools/Shogun/Project Cleanup Tools")]
    public static void ShowWindow()
    {
        GetWindow<ProjectCleanupTools>("Project Cleanup Tools");
    }

    private void OnGUI()
    {
        GUILayout.Label("Project Cleanup Tools", EditorStyles.boldLabel);
        projectRoot = EditorGUILayout.TextField("Project Root", projectRoot);
        deleteEmptyFolders = EditorGUILayout.Toggle("Delete Empty Folders", deleteEmptyFolders);
        reportOrphanedMetas = EditorGUILayout.Toggle("Report Orphaned .meta Files", reportOrphanedMetas);
        reportDuplicates = EditorGUILayout.Toggle("Report Duplicate Files (by name)", reportDuplicates);
        deleteOrphanedMetas = EditorGUILayout.Toggle("Delete Orphaned .meta Files", deleteOrphanedMetas);
        if (GUILayout.Button("Run Cleanup"))
        {
            RunCleanup();
        }
    }

    private void RunCleanup()
    {
        int emptyFolders = 0;
        int deletedMetas = 0;
        int duplicateFiles = 0;
        int safeDuplicates = 0;
        int potentialConflicts = 0;
        List<string> orphanedMetaList = new List<string>();
        Dictionary<string, List<string>> fileNameMap = new Dictionary<string, List<string>>();

        // 1. Delete empty folders
        if (deleteEmptyFolders)
        {
            emptyFolders = DeleteEmptyFolders(projectRoot);
            Debug.Log($"Deleted {emptyFolders} empty folders.");
        }

        // 2. Find and delete orphaned .meta files
        foreach (var meta in Directory.GetFiles(projectRoot, "*.meta", SearchOption.AllDirectories))
        {
            string asset = meta.Substring(0, meta.Length - 5);
            if (!File.Exists(asset) && !Directory.Exists(asset))
            {
                AssetDatabase.DeleteAsset(meta.Replace("\\", "/"));
                deletedMetas++;
            }
        }
        Debug.Log($"Deleted {deletedMetas} orphaned .meta files.");

        // 3. Find duplicate files by name and compare hashes
        foreach (var file in Directory.GetFiles(projectRoot, "*.*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta")) continue;
            string name = Path.GetFileName(file);
            if (!fileNameMap.ContainsKey(name))
                fileNameMap[name] = new List<string>();
            fileNameMap[name].Add(file);
        }
        foreach (var kvp in fileNameMap)
        {
            if (kvp.Value.Count > 1)
            {
                // Compare hashes
                var hashes = new Dictionary<string, List<string>>();
                foreach (var path in kvp.Value)
                {
                    string hash = GetFileHash(path);
                    if (!hashes.ContainsKey(hash))
                        hashes[hash] = new List<string>();
                    hashes[hash].Add(path);
                }
                if (hashes.Count == 1)
                {
                    safeDuplicates++;
                    Debug.Log($"Safe duplicate: {kvp.Key}\n" + string.Join("\n", kvp.Value));
                }
                else
                {
                    potentialConflicts++;
                    Debug.LogWarning($"Potential conflict (different files with same name): {kvp.Key}\n" + string.Join("\n", kvp.Value));
                }
                duplicateFiles++;
            }
        }
        Debug.Log($"Found {duplicateFiles} duplicate file names. {safeDuplicates} are safe duplicates, {potentialConflicts} are potential conflicts.");
        AssetDatabase.Refresh();
        Debug.Log("Project cleanup complete.");
    }

    private string GetFileHash(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        using (var sha = SHA256.Create())
        {
            var hash = sha.ComputeHash(stream);
            return System.BitConverter.ToString(hash).Replace("-", "");
        }
    }

    private int DeleteEmptyFolders(string root)
    {
        int deleted = 0;
        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories).OrderByDescending(s => s.Length))
        {
            // Skip folders containing a .keep or README.txt file (reserved for future content)
            if (File.Exists(Path.Combine(dir, ".keep")) || File.Exists(Path.Combine(dir, "README.txt")))
                continue;
            if (Directory.Exists(dir) && Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
            {
                AssetDatabase.DeleteAsset(dir.Replace("\\", "/"));
                deleted++;
            }
        }
        return deleted;
    }
}
#endif 