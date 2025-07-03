#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;

namespace Shogun.Core
{
    public class ProjectExportTool
    {
        private static Application.LogCallback _logCallback;

        [MenuItem("Tools/Export Full Project Structure and Asset Details")]
        public static void ExportFullProject()
        {
            // --- Save original open scenes and active scene ---
            var originalOpenScenes = new List<SceneSetup>(EditorSceneManager.GetSceneManagerSetup());
            var originalActiveScene = EditorSceneManager.GetActiveScene();
            bool hadOpenScenes = originalOpenScenes.Count > 0;
            string activeScenePath = hadOpenScenes && originalActiveScene.IsValid() ? originalActiveScene.path : null;

            // Attach log callback to suppress known warnings/errors during export
            _logCallback = (condition, stackTrace, type) =>
            {
                if (type == LogType.Warning && condition.Contains("Unloading the last loaded scene")) return;
                if (type == LogType.Error && condition.Contains("More than one global light on layer Default")) return;
                Debug.unityLogger.logHandler.LogFormat(type, null, "{0}\n{1}", condition, stackTrace);
            };
            Application.logMessageReceived += _logCallback;

            string rootPath = Application.dataPath;
            string projectRoot = rootPath + "/..";
            string exportFolder = Path.Combine(projectRoot, "_FullProjectExport");

            try
            {
                if (Directory.Exists(exportFolder))
                    Directory.Delete(exportFolder, true);
                Directory.CreateDirectory(exportFolder);

                // Export folder/file tree
                string folderTreePath = Path.Combine(exportFolder, "_FolderTree.txt");
                using (StreamWriter folderWriter = new StreamWriter(folderTreePath, false))
                {
                    WriteDirectory(rootPath, folderWriter, "");
                }

                // Export project overview
                ExportProjectOverview(exportFolder, projectRoot);

                // Export package dependencies
                ExportPackageDependencies(exportFolder);

                // Export assembly definitions
                ExportAssemblyDefinitions(exportFolder, rootPath);

                // Prepare for asset exports
                string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
                string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
                string[] scriptableObjectGUIDs = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });

                List<string> summaryLines = new List<string>();
                string assetsExportFolder = Path.Combine(exportFolder, "AssetExports");
                Directory.CreateDirectory(assetsExportFolder);

                int totalCount = prefabGUIDs.Length + sceneGUIDs.Length + scriptableObjectGUIDs.Length;
                int currentIndex = 0;

                // Export Prefabs
                foreach (var guid in prefabGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = Path.GetFileNameWithoutExtension(path);
                    string outputPath = Path.Combine(assetsExportFolder, $"Prefab_{assetName}.txt");

                    EditorUtility.DisplayProgressBar("Exporting Assets", $"Prefab: {assetName}", (float)currentIndex / totalCount);

                    try
                    {
                        StringBuilder writer = new StringBuilder();
                        writer.AppendLine("=== Prefab Export ===");
                        writer.AppendLine($"Prefab Path: {path}");
                        writer.AppendLine($"GUID: {guid}");
                        writer.AppendLine();

                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab != null)
                        {
                            DumpGameObjectDetailed(prefab.transform, writer, 0);
                            File.WriteAllText(outputPath, writer.ToString());
                            summaryLines.Add($"Prefab: {path} => {outputPath}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to export prefab {path}: {ex.Message}");
                    }

                    currentIndex++;
                }

                // Export Scenes - open one at a time non-additively to avoid additive issues
                foreach (var guid in sceneGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = Path.GetFileNameWithoutExtension(path);
                    string outputPath = Path.Combine(assetsExportFolder, $"Scene_{assetName}.txt");

                    EditorUtility.DisplayProgressBar("Exporting Assets", $"Scene: {assetName}", (float)currentIndex / totalCount);

                    try
                    {
                        StringBuilder writer = new StringBuilder();
                        writer.AppendLine("=== Scene Export ===");
                        writer.AppendLine($"Scene Path: {path}");
                        writer.AppendLine($"GUID: {guid}");
                        writer.AppendLine();

                        // Open scene non-additively (single scene mode)
                        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        foreach (GameObject root in scene.GetRootGameObjects())
                            DumpGameObjectDetailed(root.transform, writer, 0);

                        File.WriteAllText(outputPath, writer.ToString());
                        summaryLines.Add($"Scene: {path} => {outputPath}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to export scene {path}: {ex.Message}");
                    }

                    currentIndex++;
                }

                // Export ScriptableObjects
                foreach (var guid in scriptableObjectGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = Path.GetFileNameWithoutExtension(path);
                    string outputPath = Path.Combine(assetsExportFolder, $"ScriptableObject_{assetName}.txt");

                    EditorUtility.DisplayProgressBar("Exporting Assets", $"ScriptableObject: {assetName}", (float)currentIndex / totalCount);

                    try
                    {
                        StringBuilder writer = new StringBuilder();
                        writer.AppendLine("=== ScriptableObject Export ===");
                        writer.AppendLine($"Asset Path: {path}");
                        writer.AppendLine($"GUID: {guid}");
                        writer.AppendLine();

                        ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                        if (so != null)
                        {
                            DumpScriptableObjectDetailed(so, writer);
                            File.WriteAllText(outputPath, writer.ToString());
                            summaryLines.Add($"ScriptableObject: {path} => {outputPath}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to export ScriptableObject {path}: {ex.Message}");
                    }

                    currentIndex++;
                }

                // Export enhanced script analysis
                ExportScriptAnalysis(exportFolder, rootPath, projectRoot);

                // Write summary
                string summaryPath = Path.Combine(exportFolder, "_ExportSummary.txt");
                File.WriteAllLines(summaryPath, summaryLines);

                Debug.Log($"Enhanced project export complete to {exportFolder}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Application.logMessageReceived -= _logCallback;

                // Restore previous open scenes and active scene
                if (originalOpenScenes != null && originalOpenScenes.Count > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalOpenScenes.ToArray());

                    // Re-activate original active scene (if valid)
                    if (!string.IsNullOrEmpty(activeScenePath))
                    {
                        var reloadedScene = EditorSceneManager.GetSceneByPath(activeScenePath);
                        if (reloadedScene.IsValid())
                            EditorSceneManager.SetActiveScene(reloadedScene);
                    }
                }
            }
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
