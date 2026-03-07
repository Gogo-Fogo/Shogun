#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Object = UnityEngine.Object;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shogun.Core
{
    public class ProjectExportTool
    {
        private const string MenuRoot = "Tools/Shogun/Export/";
        private const string ExportRootFolderName = "_Generated";
        private const string ExportFolderName = "ProjectExport";

        [MenuItem(MenuRoot + "Export Current Scene Snapshot")]
        public static void ExportCurrentSceneSnapshot()
        {
            RunWithSceneStateRestore(() =>
            {
                Scene activeScene = EditorSceneManager.GetActiveScene();
                if (!activeScene.IsValid())
                {
                    Debug.LogWarning("[ProjectExportTool] No active scene is open to export.");
                    return;
                }

                string exportFolder = CreateExportFolder("CurrentScene");
                string assetsExportFolder = CreateAssetExportFolder(exportFolder);
                List<string> summaryLines = new List<string>();

                ExportLoadedSceneSnapshot(activeScene, assetsExportFolder, summaryLines, "Current Scene Snapshot");
                WriteSummary(exportFolder, "Current Scene Snapshot", summaryLines);

                Debug.Log($"[ProjectExportTool] Current scene snapshot exported to {exportFolder}");
            });
        }

        [MenuItem(MenuRoot + "Export Selected Asset Snapshot")]
        public static void ExportSelectedSnapshot()
        {
            RunWithSceneStateRestore(() =>
            {
                Object selectedObject = Selection.activeObject;
                GameObject selectedGameObject = Selection.activeGameObject;

                if (selectedObject == null && selectedGameObject == null)
                {
                    Debug.LogWarning("[ProjectExportTool] No asset or scene object is selected.");
                    return;
                }

                string exportFolder = CreateExportFolder("SelectedSnapshot");
                string assetsExportFolder = CreateAssetExportFolder(exportFolder);
                List<string> summaryLines = new List<string>();

                if (selectedGameObject != null && !EditorUtility.IsPersistent(selectedGameObject))
                {
                    ExportSceneGameObjectSnapshot(selectedGameObject, assetsExportFolder, summaryLines);
                }
                else
                {
                    ExportSelectedObjectSnapshot(selectedObject ?? selectedGameObject, assetsExportFolder, summaryLines);
                }

                WriteSummary(exportFolder, "Selected Snapshot", summaryLines);
                Debug.Log($"[ProjectExportTool] Selected snapshot exported to {exportFolder}");
            });
        }

        [MenuItem(MenuRoot + "Export Selected Asset Snapshot", true)]
        private static bool ValidateExportSelectedSnapshot()
        {
            return Selection.activeObject != null || Selection.activeGameObject != null;
        }

        [MenuItem(MenuRoot + "Export Full Project Snapshot (Fallback)")]
        public static void ExportFullProjectSnapshot()
        {
            RunWithSceneStateRestore(() =>
            {
                string rootPath = Application.dataPath;
                string projectRoot = GetProjectRoot();
                string exportFolder = CreateExportFolder("FullProjectFallback");

                string folderTreePath = Path.Combine(exportFolder, "_FolderTree.txt");
                using (StreamWriter folderWriter = new StreamWriter(folderTreePath, false))
                {
                    WriteDirectory(rootPath, folderWriter, "");
                }

                ExportProjectOverview(exportFolder, projectRoot);
                ExportPackageDependencies(exportFolder);
                ExportAssemblyDefinitions(exportFolder, rootPath);

                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
                string[] scriptableObjectGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });

                List<string> summaryLines = new List<string>
                {
                    $"Folder Tree => {folderTreePath}"
                };
                string assetsExportFolder = CreateAssetExportFolder(exportFolder);

                int totalCount = Mathf.Max(1, prefabGuids.Length + sceneGuids.Length + scriptableObjectGuids.Length);
                int currentIndex = 0;

                foreach (string guid in prefabGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = Path.GetFileNameWithoutExtension(path);
                    EditorUtility.DisplayProgressBar("Exporting Assets", $"Prefab: {assetName}", (float)currentIndex / totalCount);

                    try
                    {
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab != null)
                        {
                            ExportPrefabSnapshot(prefab, assetsExportFolder, summaryLines);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ProjectExportTool] Failed to export prefab {path}: {ex.Message}");
                    }

                    currentIndex++;
                }

                foreach (string guid in sceneGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = Path.GetFileNameWithoutExtension(path);
                    EditorUtility.DisplayProgressBar("Exporting Assets", $"Scene: {assetName}", (float)currentIndex / totalCount);

                    try
                    {
                        ExportSceneAssetSnapshot(path, assetsExportFolder, summaryLines);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ProjectExportTool] Failed to export scene {path}: {ex.Message}");
                    }

                    currentIndex++;
                }

                foreach (string guid in scriptableObjectGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = Path.GetFileNameWithoutExtension(path);
                    EditorUtility.DisplayProgressBar("Exporting Assets", $"ScriptableObject: {assetName}", (float)currentIndex / totalCount);

                    try
                    {
                        ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                        if (scriptableObject != null)
                        {
                            ExportScriptableObjectSnapshot(scriptableObject, assetsExportFolder, summaryLines);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ProjectExportTool] Failed to export ScriptableObject {path}: {ex.Message}");
                    }

                    currentIndex++;
                }

                ExportScriptAnalysis(exportFolder, rootPath, projectRoot);
                summaryLines.Add($"Script Analysis => {Path.Combine(exportFolder, "ScriptAnalysis.txt")}");
                WriteSummary(exportFolder, "Full Project Snapshot (Fallback)", summaryLines);

                Debug.Log($"[ProjectExportTool] Full project snapshot exported to {exportFolder}");
            });
        }

        private static void RunWithSceneStateRestore(Action exportAction)
        {
            List<SceneSetup> originalOpenScenes = new List<SceneSetup>(EditorSceneManager.GetSceneManagerSetup());
            Scene originalActiveScene = EditorSceneManager.GetActiveScene();
            string activeScenePath = originalActiveScene.IsValid() ? originalActiveScene.path : null;

            try
            {
                exportAction?.Invoke();
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (originalOpenScenes.Count > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalOpenScenes.ToArray());
                }

                if (!string.IsNullOrEmpty(activeScenePath))
                {
                    Scene restoredScene = EditorSceneManager.GetSceneByPath(activeScenePath);
                    if (restoredScene.IsValid())
                    {
                        EditorSceneManager.SetActiveScene(restoredScene);
                    }
                }
            }
        }

        private static string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static string CreateExportFolder(string label)
        {
            string exportFolder = Path.Combine(
                GetProjectRoot(),
                ExportRootFolderName,
                ExportFolderName,
                $"{DateTime.Now:yyyyMMdd-HHmmssfff}_{SanitizeFileName(label)}");

            Directory.CreateDirectory(exportFolder);
            return exportFolder;
        }

        private static string CreateAssetExportFolder(string exportFolder)
        {
            string assetsExportFolder = Path.Combine(exportFolder, "AssetExports");
            Directory.CreateDirectory(assetsExportFolder);
            return assetsExportFolder;
        }

        private static void WriteSummary(string exportFolder, string exportKind, IEnumerable<string> summaryLines)
        {
            string summaryPath = Path.Combine(exportFolder, "_ExportSummary.txt");
            using (StreamWriter writer = new StreamWriter(summaryPath, false))
            {
                writer.WriteLine($"=== {exportKind} ===");
                writer.WriteLine($"Generated: {DateTime.Now:O}");
                writer.WriteLine($"Unity Version: {Application.unityVersion}");
                writer.WriteLine($"Project: {Application.productName}");
                writer.WriteLine($"Export Folder: {exportFolder}");
                writer.WriteLine();

                foreach (string line in summaryLines.Where(line => !string.IsNullOrWhiteSpace(line)).Distinct())
                {
                    writer.WriteLine(line);
                }
            }
        }

        private static void ExportSelectedObjectSnapshot(Object selectedObject, string assetsExportFolder, List<string> summaryLines)
        {
            if (selectedObject == null)
            {
                Debug.LogWarning("[ProjectExportTool] No selected object was available to export.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            if (!string.IsNullOrEmpty(assetPath) && AssetDatabase.IsValidFolder(assetPath))
            {
                ExportFolderSnapshot(assetPath, assetsExportFolder, summaryLines);
                return;
            }

            if (selectedObject is SceneAsset)
            {
                ExportSceneAssetSnapshot(assetPath, assetsExportFolder, summaryLines);
                return;
            }

            if (selectedObject is GameObject prefabAsset && PrefabUtility.GetPrefabAssetType(prefabAsset) != PrefabAssetType.NotAPrefab)
            {
                ExportPrefabSnapshot(prefabAsset, assetsExportFolder, summaryLines);
                return;
            }

            if (selectedObject is ScriptableObject scriptableObject)
            {
                ExportScriptableObjectSnapshot(scriptableObject, assetsExportFolder, summaryLines);
                return;
            }

            ExportGenericAssetSnapshot(selectedObject, assetsExportFolder, summaryLines);
        }

        private static void ExportLoadedSceneSnapshot(Scene scene, string assetsExportFolder, List<string> summaryLines, string exportTitle)
        {
            string sceneName = string.IsNullOrWhiteSpace(scene.name) ? "UntitledScene" : scene.name;
            string outputPath = Path.Combine(assetsExportFolder, $"Scene_{SanitizeFileName(sceneName)}.txt");
            StringBuilder writer = new StringBuilder();

            writer.AppendLine($"=== {exportTitle} ===");
            writer.AppendLine($"Scene Name: {sceneName}");
            writer.AppendLine($"Scene Path: {(string.IsNullOrEmpty(scene.path) ? "(unsaved scene)" : scene.path)}");
            writer.AppendLine($"Is Loaded: {scene.isLoaded}");
            writer.AppendLine($"Is Dirty: {scene.isDirty}");

            if (!string.IsNullOrEmpty(scene.path))
            {
                string guid = AssetDatabase.AssetPathToGUID(scene.path);
                if (!string.IsNullOrEmpty(guid))
                {
                    writer.AppendLine($"GUID: {guid}");
                }
            }

            writer.AppendLine();

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                DumpGameObjectDetailed(rootObject.transform, writer, 0);
            }

            File.WriteAllText(outputPath, writer.ToString());
            summaryLines.Add($"Scene: {(string.IsNullOrEmpty(scene.path) ? sceneName : scene.path)} => {outputPath}");
        }

        private static void ExportSceneAssetSnapshot(string scenePath, string assetsExportFolder, List<string> summaryLines)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                Debug.LogWarning("[ProjectExportTool] Cannot export a scene asset snapshot without a valid scene path.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ExportLoadedSceneSnapshot(scene, assetsExportFolder, summaryLines, "Scene Asset Snapshot");
        }

        private static void ExportSceneGameObjectSnapshot(GameObject gameObject, string assetsExportFolder, List<string> summaryLines)
        {
            string hierarchyPath = GetHierarchyPath(gameObject.transform);
            string outputPath = Path.Combine(assetsExportFolder, $"SceneObject_{SanitizeFileName(hierarchyPath)}.txt");
            StringBuilder writer = new StringBuilder();

            writer.AppendLine("=== Scene Object Snapshot ===");
            writer.AppendLine($"Object Name: {gameObject.name}");
            writer.AppendLine($"Hierarchy Path: {hierarchyPath}");
            writer.AppendLine($"Scene: {(string.IsNullOrEmpty(gameObject.scene.path) ? gameObject.scene.name : gameObject.scene.path)}");
            writer.AppendLine($"Instance ID: {gameObject.GetInstanceID()}");
            writer.AppendLine();
            DumpGameObjectDetailed(gameObject.transform, writer, 0);

            File.WriteAllText(outputPath, writer.ToString());
            summaryLines.Add($"Scene Object: {hierarchyPath} => {outputPath}");
        }

        private static void ExportPrefabSnapshot(GameObject prefabAsset, string assetsExportFolder, List<string> summaryLines)
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            string outputPath = Path.Combine(
                assetsExportFolder,
                $"Prefab_{SanitizeFileName(Path.GetFileNameWithoutExtension(prefabPath))}.txt");
            StringBuilder writer = new StringBuilder();

            writer.AppendLine("=== Prefab Snapshot ===");
            writer.AppendLine($"Prefab Path: {prefabPath}");
            writer.AppendLine($"GUID: {AssetDatabase.AssetPathToGUID(prefabPath)}");
            writer.AppendLine();
            DumpGameObjectDetailed(prefabAsset.transform, writer, 0);

            File.WriteAllText(outputPath, writer.ToString());
            summaryLines.Add($"Prefab: {prefabPath} => {outputPath}");
        }

        private static void ExportScriptableObjectSnapshot(ScriptableObject scriptableObject, string assetsExportFolder, List<string> summaryLines)
        {
            string assetPath = AssetDatabase.GetAssetPath(scriptableObject);
            string outputPath = Path.Combine(
                assetsExportFolder,
                $"ScriptableObject_{SanitizeFileName(Path.GetFileNameWithoutExtension(assetPath))}.txt");
            StringBuilder writer = new StringBuilder();

            writer.AppendLine("=== ScriptableObject Snapshot ===");
            writer.AppendLine($"Asset Path: {assetPath}");
            writer.AppendLine($"GUID: {AssetDatabase.AssetPathToGUID(assetPath)}");
            writer.AppendLine();
            DumpScriptableObjectDetailed(scriptableObject, writer);

            File.WriteAllText(outputPath, writer.ToString());
            summaryLines.Add($"ScriptableObject: {assetPath} => {outputPath}");
        }

        private static void ExportFolderSnapshot(string folderPath, string assetsExportFolder, List<string> summaryLines)
        {
            string outputPath = Path.Combine(
                assetsExportFolder,
                $"Folder_{SanitizeFileName(Path.GetFileName(folderPath))}.txt");
            string absoluteFolderPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), folderPath));

            using (StreamWriter writer = new StreamWriter(outputPath, false))
            {
                writer.WriteLine("=== Folder Snapshot ===");
                writer.WriteLine($"Folder Path: {folderPath}");
                writer.WriteLine();
                WriteDirectory(absoluteFolderPath, writer, "");
            }

            summaryLines.Add($"Folder: {folderPath} => {outputPath}");
        }

        private static void ExportGenericAssetSnapshot(Object asset, string assetsExportFolder, List<string> summaryLines)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string assetName = !string.IsNullOrEmpty(assetPath) ? Path.GetFileNameWithoutExtension(assetPath) : asset.name;
            string outputPath = Path.Combine(
                assetsExportFolder,
                $"{SanitizeFileName(asset.GetType().Name)}_{SanitizeFileName(assetName)}.txt");
            StringBuilder writer = new StringBuilder();

            writer.AppendLine("=== Asset Snapshot ===");
            writer.AppendLine($"Object Name: {asset.name}");
            writer.AppendLine($"Object Type: {asset.GetType().FullName}");

            if (!string.IsNullOrEmpty(assetPath))
            {
                writer.AppendLine($"Asset Path: {assetPath}");
                writer.AppendLine($"GUID: {AssetDatabase.AssetPathToGUID(assetPath)}");
            }

            writer.AppendLine();

            if (asset is TextAsset textAsset)
            {
                writer.AppendLine("Text Content:");
                writer.AppendLine(textAsset.text);
                writer.AppendLine();
            }

            DumpSerializedObjectDetailed(asset, writer);
            File.WriteAllText(outputPath, writer.ToString());

            summaryLines.Add($"{asset.GetType().Name}: {(string.IsNullOrEmpty(assetPath) ? asset.name : assetPath)} => {outputPath}");
        }

        private static void DumpSerializedObjectDetailed(Object target, StringBuilder writer)
        {
            try
            {
                SerializedObject serializedObject = new SerializedObject(target);
                SerializedProperty property = serializedObject.GetIterator();
                bool first = true;
                bool wroteProperty = false;

                writer.AppendLine("Serialized Properties:");
                while (property.NextVisible(first))
                {
                    first = false;
                    if (property.name == "m_Script")
                    {
                        continue;
                    }

                    writer.AppendLine($"    - {property.displayName}: {GetPropertyValue(property)}");
                    wroteProperty = true;
                }

                if (!wroteProperty)
                {
                    writer.AppendLine("    (No serialized properties found)");
                }
            }
            catch (Exception ex)
            {
                writer.AppendLine($"SerializedObject inspection failed: {ex.Message}");
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            Stack<string> parts = new Stack<string>();
            Transform current = transform;

            while (current != null)
            {
                parts.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", parts);
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "Unnamed";
            }

            StringBuilder builder = new StringBuilder(fileName.Length);
            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            foreach (char character in fileName)
            {
                if (character == '/' || character == '\\' || invalidCharacters.Contains(character))
                {
                    builder.Append('_');
                }
                else if (char.IsWhiteSpace(character))
                {
                    builder.Append('_');
                }
                else
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }

        private static void ExportProjectOverview(string exportFolder, string projectRoot)
        {
            string overviewPath = Path.Combine(exportFolder, "_ProjectOverview.txt");
            using (StreamWriter writer = new StreamWriter(overviewPath, false))
            {
                writer.WriteLine("=== PROJECT OVERVIEW ===");
                writer.WriteLine($"Unity Version: {Application.unityVersion}");
                writer.WriteLine($"Project Name: {Application.productName}");
                writer.WriteLine($"Company Name: {Application.companyName}");
                writer.WriteLine($"Build Target: {EditorUserBuildSettings.activeBuildTarget}");
                var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                writer.WriteLine($"Scripting Backend: {PlayerSettings.GetScriptingBackend(namedBuildTarget)}");
                writer.WriteLine($"API Compatibility Level: {PlayerSettings.GetApiCompatibilityLevel(namedBuildTarget)}");
                writer.WriteLine($"Project Root: {projectRoot}");
                writer.WriteLine();

                // Export build settings
                writer.WriteLine("=== BUILD SETTINGS ===");
                writer.WriteLine($"Scenes in Build ({EditorBuildSettings.scenes.Length}):");
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    var scene = EditorBuildSettings.scenes[i];
                    writer.WriteLine($"  [{i}] {scene.path} (Enabled: {scene.enabled})");
                }
                writer.WriteLine();

                // Export quality settings
                writer.WriteLine("=== QUALITY SETTINGS ===");
                for (int i = 0; i < QualitySettings.names.Length; i++)
                {
                    writer.WriteLine($"  [{i}] {QualitySettings.names[i]} (Active: {i == QualitySettings.GetQualityLevel()})");
                }
                writer.WriteLine();
            }
        }

        private static void ExportPackageDependencies(string exportFolder)
        {
            string packagesPath = Path.Combine(exportFolder, "_PackageDependencies.txt");
            using (StreamWriter writer = new StreamWriter(packagesPath, false))
            {
                writer.WriteLine("=== PACKAGE DEPENDENCIES ===");
                
                var packages = Client.List();
                while (!packages.IsCompleted) { }
                
                if (packages.Status == StatusCode.Success)
                {
                    foreach (var package in packages.Result)
                    {
                        writer.WriteLine($"Package: {package.name} v{package.version}");
                        if (!string.IsNullOrEmpty(package.description))
                            writer.WriteLine($"  Description: {package.description}");
                        writer.WriteLine();
                    }
                }
                else
                {
                    writer.WriteLine("Failed to retrieve package list");
                }
            }
        }

        private static void ExportAssemblyDefinitions(string exportFolder, string rootPath)
        {
            string asmdefPath = Path.Combine(exportFolder, "_AssemblyDefinitions.txt");
            using (StreamWriter writer = new StreamWriter(asmdefPath, false))
            {
                writer.WriteLine("=== ASSEMBLY DEFINITIONS ===");
                
                string[] asmdefFiles = Directory.GetFiles(rootPath, "*.asmdef", SearchOption.AllDirectories);
                foreach (var asmdefFile in asmdefFiles)
                {
                    writer.WriteLine($"Assembly Definition: {asmdefFile.Replace(rootPath + Path.DirectorySeparatorChar, "")}");
                    
                    try
                    {
                        string content = File.ReadAllText(asmdefFile);
                        writer.WriteLine("Content:");
                        writer.WriteLine(content);
                        writer.WriteLine();
                    }
                    catch (System.Exception ex)
                    {
                        writer.WriteLine($"Error reading file: {ex.Message}");
                        writer.WriteLine();
                    }
                }
            }
        }

        private static void ExportScriptAnalysis(string exportFolder, string rootPath, string projectRoot)
        {
            string scriptAnalysisPath = Path.Combine(exportFolder, "ScriptAnalysis.txt");
            using (StreamWriter writer = new StreamWriter(scriptAnalysisPath, false))
            {
                writer.WriteLine("=== SCRIPT ANALYSIS ===");
                
                string[] scriptFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
                var scriptStats = new Dictionary<string, int>();
                var classList = new List<string>();
                var interfaceList = new List<string>();
                var enumList = new List<string>();

                foreach (var scriptFile in scriptFiles)
                {
                    string relativePath = scriptFile.Replace(projectRoot + Path.DirectorySeparatorChar, "");
                    writer.WriteLine($"=== Script: {relativePath} ===");
                    
                    try
                    {
                        string[] lines = File.ReadAllLines(scriptFile);
                        writer.WriteLine($"Total Lines: {lines.Length}");
                        
                        // Count lines by type
                        int commentLines = 0;
                        int codeLines = 0;
                        int emptyLines = 0;
                        
                        foreach (string line in lines)
                        {
                            string trimmed = line.Trim();
                            if (string.IsNullOrEmpty(trimmed))
                                emptyLines++;
                            else if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*"))
                                commentLines++;
                            else
                                codeLines++;
                        }
                        
                        writer.WriteLine($"  Code Lines: {codeLines}");
                        writer.WriteLine($"  Comment Lines: {commentLines}");
                        writer.WriteLine($"  Empty Lines: {emptyLines}");
                        
                        // Find classes, interfaces, enums
                        for (int i = 0; i < Mathf.Min(50, lines.Length); i++)
                        {
                            string line = lines[i].Trim();
                            if (line.StartsWith("public class ") || line.StartsWith("private class ") || line.StartsWith("class "))
                            {
                                string className = ExtractClassName(line);
                                if (!string.IsNullOrEmpty(className))
                                    classList.Add($"{className} ({relativePath})");
                            }
                            else if (line.StartsWith("public interface ") || line.StartsWith("private interface ") || line.StartsWith("interface "))
                            {
                                string interfaceName = ExtractInterfaceName(line);
                                if (!string.IsNullOrEmpty(interfaceName))
                                    interfaceList.Add($"{interfaceName} ({relativePath})");
                            }
                            else if (line.StartsWith("public enum ") || line.StartsWith("private enum ") || line.StartsWith("enum "))
                            {
                                string enumName = ExtractEnumName(line);
                                if (!string.IsNullOrEmpty(enumName))
                                    enumList.Add($"{enumName} ({relativePath})");
                            }
                        }
                        
                        // Show first 30 lines
                        writer.WriteLine("First 30 lines:");
                        int lineCount = Mathf.Min(30, lines.Length);
                        for (int i = 0; i < lineCount; i++)
                        {
                            writer.WriteLine($"  {i + 1:D2}: {lines[i]}");
                        }
                        if (lines.Length > 30)
                        {
                            writer.WriteLine("  ... (truncated)");
                        }
                        writer.WriteLine();
                    }
                    catch (System.Exception ex)
                    {
                        writer.WriteLine($"Error reading file: {ex.Message}");
                        writer.WriteLine();
                    }
                }
                
                // Summary
                writer.WriteLine("=== SCRIPT SUMMARY ===");
                writer.WriteLine($"Total Scripts: {scriptFiles.Length}");
                writer.WriteLine($"Total Classes: {classList.Count}");
                writer.WriteLine($"Total Interfaces: {interfaceList.Count}");
                writer.WriteLine($"Total Enums: {enumList.Count}");
                writer.WriteLine();
                
                if (classList.Count > 0)
                {
                    writer.WriteLine("Classes:");
                    foreach (var className in classList)
                        writer.WriteLine($"  - {className}");
                    writer.WriteLine();
                }
                
                if (interfaceList.Count > 0)
                {
                    writer.WriteLine("Interfaces:");
                    foreach (var interfaceName in interfaceList)
                        writer.WriteLine($"  - {interfaceName}");
                    writer.WriteLine();
                }
                
                if (enumList.Count > 0)
                {
                    writer.WriteLine("Enums:");
                    foreach (var enumName in enumList)
                        writer.WriteLine($"  - {enumName}");
                    writer.WriteLine();
                }
            }
        }

        private static string ExtractClassName(string line)
        {
            // Simple extraction - could be improved with regex
            if (line.Contains("class "))
            {
                int classIndex = line.IndexOf("class ") + 6;
                int endIndex = line.IndexOf(' ', classIndex);
                if (endIndex == -1) endIndex = line.IndexOf(':', classIndex);
                if (endIndex == -1) endIndex = line.IndexOf('{', classIndex);
                if (endIndex == -1) endIndex = line.Length;
                
                return line.Substring(classIndex, endIndex - classIndex).Trim();
            }
            return null;
        }

        private static string ExtractInterfaceName(string line)
        {
            if (line.Contains("interface "))
            {
                int interfaceIndex = line.IndexOf("interface ") + 10;
                int endIndex = line.IndexOf(' ', interfaceIndex);
                if (endIndex == -1) endIndex = line.IndexOf(':', interfaceIndex);
                if (endIndex == -1) endIndex = line.IndexOf('{', interfaceIndex);
                if (endIndex == -1) endIndex = line.Length;
                
                return line.Substring(interfaceIndex, endIndex - interfaceIndex).Trim();
            }
            return null;
        }

        private static string ExtractEnumName(string line)
        {
            if (line.Contains("enum "))
            {
                int enumIndex = line.IndexOf("enum ") + 5;
                int endIndex = line.IndexOf(' ', enumIndex);
                if (endIndex == -1) endIndex = line.IndexOf(':', enumIndex);
                if (endIndex == -1) endIndex = line.IndexOf('{', enumIndex);
                if (endIndex == -1) endIndex = line.Length;
                
                return line.Substring(enumIndex, endIndex - enumIndex).Trim();
            }
            return null;
        }

        private static void DumpScriptableObjectDetailed(ScriptableObject so, StringBuilder writer)
        {
            writer.AppendLine($"ScriptableObject: {so.name} (Type: {so.GetType().Name})");
            var serializedObject = new SerializedObject(so);
            var property = serializedObject.GetIterator();
            bool first = true;
            
            while (property.NextVisible(first))
            {
                first = false;
                if (property.name == "m_Script") continue;
                writer.AppendLine($"    - {property.displayName}: {GetPropertyValue(property)}");
            }
        }

        /// <summary>
        /// Recursively writes the directory and file structure, skipping .meta files.
        /// </summary>
        private static void WriteDirectory(string dir, StreamWriter writer, string indent)
        {
            writer.WriteLine(indent + Path.GetFileName(dir) + "/");
            indent += "    ";
            foreach (var subDir in Directory.GetDirectories(dir))
                WriteDirectory(subDir, writer, indent);

            foreach (var file in Directory.GetFiles(dir))
            {
                string extension = Path.GetExtension(file);
                if (extension == ".meta") continue;
                writer.WriteLine(indent + Path.GetFileName(file));
            }
        }

        /// <summary>
        /// Dumps detailed information about a GameObject and its components, including field values.
        /// </summary>
        private static void DumpGameObjectDetailed(Transform t, StringBuilder writer, int indent)
        {
            string prefix = new string(' ', indent * 4);
            GameObject go = t.gameObject;
            writer.AppendLine($"{prefix}- GameObject: {go.name} (Tag: {go.tag}, Layer: {LayerMask.LayerToName(go.layer)}, Active: {go.activeSelf})");
            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp == null) continue;
                writer.AppendLine($"{prefix}    * {comp.GetType().Name}");
                var so = new SerializedObject(comp);
                var prop = so.GetIterator();
                bool first = true;
                while (prop.NextVisible(first))
                {
                    first = false;
                    if (prop.name == "m_Script") continue;
                    writer.AppendLine($"{prefix}        - {prop.displayName}: {GetPropertyValue(prop)}");
                }
            }
            foreach (Transform child in t)
                DumpGameObjectDetailed(child, writer, indent + 1);
        }

        /// <summary>
        /// Gets a string representation of a SerializedProperty's value.
        /// </summary>
        private static string GetPropertyValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer: return prop.intValue.ToString();
                case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
                case SerializedPropertyType.Float: return prop.floatValue.ToString();
                case SerializedPropertyType.String: return prop.stringValue;
                case SerializedPropertyType.Color: return prop.colorValue.ToString();
                case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue ? prop.objectReferenceValue.name : "null";
                case SerializedPropertyType.LayerMask: return prop.intValue.ToString();
                case SerializedPropertyType.Enum: return prop.enumNames != null && prop.enumValueIndex >= 0 && prop.enumValueIndex < prop.enumNames.Length
                                                        ? prop.enumNames[prop.enumValueIndex] : prop.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2: return prop.vector2Value.ToString();
                case SerializedPropertyType.Vector3: return prop.vector3Value.ToString();
                case SerializedPropertyType.Vector4: return prop.vector4Value.ToString();
                case SerializedPropertyType.Rect: return prop.rectValue.ToString();
                case SerializedPropertyType.AnimationCurve: return prop.animationCurveValue != null ? prop.animationCurveValue.ToString() : "null";
                case SerializedPropertyType.Bounds: return prop.boundsValue.ToString();
                case SerializedPropertyType.Quaternion: return prop.quaternionValue.eulerAngles.ToString();
                default: return "(complex value)";
            }
        }
    }
}
#endif
