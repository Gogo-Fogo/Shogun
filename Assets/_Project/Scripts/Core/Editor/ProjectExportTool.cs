#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Shogun.Core
{
    public class ProjectExportTool
    {
        private static Application.LogCallback _logCallback;

        [MenuItem("Tools/Export Full Project Structure and Asset Details")]
        public static void ExportFullProject()
        {
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

                // Prepare for prefab and scene exports
                string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
                string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

                List<string> summaryLines = new List<string>();
                string assetsExportFolder = Path.Combine(exportFolder, "AssetExports");
                Directory.CreateDirectory(assetsExportFolder);

                int totalCount = prefabGUIDs.Length + sceneGUIDs.Length;
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
                        writer.AppendLine($"Prefab: {path}");
                        writer.AppendLine();

                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab != null)
                        {
                            DumpGameObject(prefab.transform, writer, 0);
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

                // Export Scenes
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
                        writer.AppendLine($"Scene: {path}");
                        writer.AppendLine();

                        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        foreach (GameObject root in scene.GetRootGameObjects())
                            DumpGameObject(root.transform, writer, 0);
                        EditorSceneManager.CloseScene(scene, true);
                        File.WriteAllText(outputPath, writer.ToString());
                        summaryLines.Add($"Scene: {path} => {outputPath}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to export scene {path}: {ex.Message}");
                    }

                    currentIndex++;
                }

                // Write summary
                string summaryPath = Path.Combine(exportFolder, "_ExportSummary.txt");
                File.WriteAllLines(summaryPath, summaryLines);

                Debug.Log($"Full project export complete to {exportFolder}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Application.logMessageReceived -= _logCallback;
            }
        }

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

        private static void DumpGameObject(Transform t, StringBuilder writer, int indent)
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
                DumpGameObject(child, writer, indent + 1);
        }

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
