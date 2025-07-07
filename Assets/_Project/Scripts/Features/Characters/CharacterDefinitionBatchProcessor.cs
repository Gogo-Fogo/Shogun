#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Shogun.Features.Characters
{
    public class CharacterDefinitionBatchProcessor : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<CharacterDefinition> allCharacterDefinitions = new List<CharacterDefinition>();
        private bool[] selectedCharacters;
        private bool selectAll = true;
        
        // Common animation actions
        private readonly string[] commonAnimationActions = {
            "IDLE", "RUN", "WALK", "ATTACK 1", "ATTACK 2", "ATTACK 3", 
            "HURT", "HEALING", "DEATH", "DEFEND", "SPECIAL ATTACK",
            "DASH", "JUMP", "JUMP-START", "JUMP-FALL", "JUMP-TRANSITION"
        };
        
        [MenuItem("Shogun/Characters/Batch Process Character Definitions")]
        public static void ShowWindow()
        {
            GetWindow<CharacterDefinitionBatchProcessor>("Character Batch Processor");
        }
        
        private void OnEnable()
        {
            LoadAllCharacterDefinitions();
        }
        
        private void LoadAllCharacterDefinitions()
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterDefinition");
            allCharacterDefinitions.Clear();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CharacterDefinition def = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);
                if (def != null)
                {
                    allCharacterDefinitions.Add(def);
                }
            }
            
            selectedCharacters = new bool[allCharacterDefinitions.Count];
            for (int i = 0; i < selectedCharacters.Length; i++)
            {
                selectedCharacters[i] = selectAll;
            }
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Character Definition Batch Processor", EditorStyles.boldLabel);
            
            // Refresh button
            if (GUILayout.Button("Refresh Character List"))
            {
                LoadAllCharacterDefinitions();
            }
            
            EditorGUILayout.Space(10);
            
            // Select all/none
            EditorGUILayout.BeginHorizontal();
            bool newSelectAll = EditorGUILayout.Toggle("Select All", selectAll);
            if (newSelectAll != selectAll)
            {
                selectAll = newSelectAll;
                for (int i = 0; i < selectedCharacters.Length; i++)
                {
                    selectedCharacters[i] = selectAll;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Character list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            for (int i = 0; i < allCharacterDefinitions.Count; i++)
            {
                var def = allCharacterDefinitions[i];
                if (def == null) continue;
                
                EditorGUILayout.BeginHorizontal("box");
                
                selectedCharacters[i] = EditorGUILayout.Toggle(selectedCharacters[i], GUILayout.Width(20));
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(def.CharacterName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Type: {def.CharacterType}, Element: {def.ElementalType}", EditorStyles.miniLabel);
                
                // Validation info
                int validMappings = def.animationMappings.Count(m => m.IsValid);
                int totalMappings = def.animationMappings.Count;
                string validationText = totalMappings == 0 ? "No mappings" : $"{validMappings}/{totalMappings} valid";
                
                Color originalColor = GUI.color;
                if (totalMappings == 0)
                    GUI.color = Color.yellow;
                else if (validMappings < totalMappings)
                    GUI.color = Color.red;
                else
                    GUI.color = Color.green;
                    
                EditorGUILayout.LabelField($"Animations: {validationText}", EditorStyles.miniLabel);
                GUI.color = originalColor;
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            
            // Batch operations
            GUILayout.Label("Batch Operations", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            
            // Auto-populate common actions
            if (GUILayout.Button("Auto-Populate Common Actions (Selected)"))
            {
                AutoPopulateCommonActions();
            }
            
            // Auto-assign all clips
            if (GUILayout.Button("Auto-Assign All Clips (Selected)"))
            {
                AutoAssignAllClips();
            }
            
            // Assign animator controller
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Animator Controller:", GUILayout.Width(120));
            RuntimeAnimatorController controller = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                null, typeof(RuntimeAnimatorController), false);
            if (GUILayout.Button("Assign to Selected", GUILayout.Width(100)))
            {
                AssignAnimatorController(controller);
            }
            EditorGUILayout.EndHorizontal();
            
            // Validation report
            if (GUILayout.Button("Generate Validation Report"))
            {
                GenerateValidationReport();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void AutoPopulateCommonActions()
        {
            int processedCount = 0;
            
            for (int i = 0; i < allCharacterDefinitions.Count; i++)
            {
                if (!selectedCharacters[i]) continue;
                
                var def = allCharacterDefinitions[i];
                def.animationMappings.Clear();
                
                foreach (string action in commonAnimationActions)
                {
                    def.animationMappings.Add(new AnimationMapping(action));
                }
                
                EditorUtility.SetDirty(def);
                processedCount++;
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Auto-populated common actions for {processedCount} characters");
        }
        
        private void AutoAssignAllClips()
        {
            int processedCount = 0;
            int totalAssigned = 0;
            
            for (int i = 0; i < allCharacterDefinitions.Count; i++)
            {
                if (!selectedCharacters[i]) continue;
                
                var def = allCharacterDefinitions[i];
                int assignedCount = 0;
                
                foreach (var mapping in def.animationMappings)
                {
                    if (mapping.TryAutoAssignClip(def.CharacterName))
                    {
                        assignedCount++;
                    }
                }
                
                EditorUtility.SetDirty(def);
                processedCount++;
                totalAssigned += assignedCount;
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Auto-assigned {totalAssigned} clips across {processedCount} characters");
        }
        
        private void AssignAnimatorController(RuntimeAnimatorController controller)
        {
            if (controller == null)
            {
                Debug.LogError("No Animator Controller selected");
                return;
            }
            
            int processedCount = 0;
            
            for (int i = 0; i < allCharacterDefinitions.Count; i++)
            {
                if (!selectedCharacters[i]) continue;
                
                var def = allCharacterDefinitions[i];
                SerializedObject so = new SerializedObject(def);
                var prop = so.FindProperty("animatorController");
                if (prop != null)
                {
                    prop.objectReferenceValue = controller;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(def);
                    processedCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Assigned Animator Controller to {processedCount} characters");
        }
        
        private void GenerateValidationReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== CHARACTER DEFINITION VALIDATION REPORT ===\n");
            
            int totalCharacters = allCharacterDefinitions.Count;
            int validCharacters = 0;
            int charactersWithAnimations = 0;
            
            foreach (var def in allCharacterDefinitions)
            {
                report.AppendLine($"Character: {def.CharacterName}");
                report.AppendLine($"  Type: {def.CharacterType}, Element: {def.ElementalType}");
                
                // Animation validation
                int validMappings = def.animationMappings.Count(m => m.IsValid);
                int totalMappings = def.animationMappings.Count;
                
                if (totalMappings == 0)
                {
                    report.AppendLine("  ❌ No animation mappings defined");
                }
                else if (validMappings == totalMappings)
                {
                    report.AppendLine($"  ✅ All {validMappings} animation mappings are valid");
                    charactersWithAnimations++;
                    validCharacters++;
                }
                else
                {
                    report.AppendLine($"  ⚠️  {validMappings}/{totalMappings} animation mappings are valid");
                    charactersWithAnimations++;
                }
                
                // Animator controller validation
                if (def.AnimatorController == null)
                {
                    report.AppendLine("  ❌ No Animator Controller assigned");
                }
                else
                {
                    report.AppendLine("  ✅ Animator Controller assigned");
                }
                
                report.AppendLine();
            }
            
            report.AppendLine("=== SUMMARY ===");
            report.AppendLine($"Total Characters: {totalCharacters}");
            report.AppendLine($"Characters with Animations: {charactersWithAnimations}");
            report.AppendLine($"Fully Valid Characters: {validCharacters}");
            
            Debug.Log(report.ToString());
            
            // Save report to file
            string reportPath = "Assets/_Project/CharacterValidationReport.txt";
            System.IO.File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"Validation report saved to: {reportPath}");
        }
    }
}
#endif 