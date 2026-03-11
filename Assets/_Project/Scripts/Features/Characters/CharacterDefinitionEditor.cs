#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Shogun.Features.Characters
{
    [CustomEditor(typeof(CharacterDefinition))]
    public class CharacterDefinitionEditor : Editor
    {
        private CharacterDefinition characterDef;
        private bool showAnimationMappings = true;
        private bool showValidation = true;
        private Vector2 scrollPosition;
        
        // Common animation actions that most characters will need
        private readonly string[] commonAnimationActions = {
            "IDLE", "RUN", "WALK", "ATTACK 1", "ATTACK 2", "ATTACK 3", 
            "HURT", "HEALING", "DEATH", "DEFEND", "SPECIAL ATTACK",
            "DASH", "JUMP", "JUMP-START", "JUMP-FALL", "JUMP-TRANSITION"
        };
        
        private void OnEnable()
        {
            characterDef = (CharacterDefinition)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Draw the default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            // Animation Mapping Section
            DrawAnimationMappingSection();
            
            // Validation Section
            DrawValidationSection();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawAnimationMappingSection()
        {
            showAnimationMappings = EditorGUILayout.Foldout(showAnimationMappings, "Animation Mappings", true);
            
            if (!showAnimationMappings) return;
            
            EditorGUILayout.BeginVertical("box");
            
            // Quick actions
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Auto-Populate Common Actions"))
            {
                AutoPopulateCommonActions();
            }
            if (GUILayout.Button("Auto-Assign All Clips"))
            {
                AutoAssignAllClips();
            }
            if (GUILayout.Button("Clear All Mappings"))
            {
                ClearAllMappings();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Animation mappings list
            SerializedProperty mappingsProp = serializedObject.FindProperty("animationMappings");
            
            if (mappingsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No animation mappings defined. Click 'Auto-Populate Common Actions' to get started.", MessageType.Info);
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                
                for (int i = 0; i < mappingsProp.arraySize; i++)
                {
                    DrawAnimationMappingItem(mappingsProp.GetArrayElementAtIndex(i), i);
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAnimationMappingItem(SerializedProperty mappingProp, int index)
        {
            EditorGUILayout.BeginVertical("box");
            
            SerializedProperty logicalNameProp = mappingProp.FindPropertyRelative("logicalName");
            SerializedProperty clipProp = mappingProp.FindPropertyRelative("clip");
            SerializedProperty useAutoAssignmentProp = mappingProp.FindPropertyRelative("useAutoAssignment");
            SerializedProperty customSearchPatternProp = mappingProp.FindPropertyRelative("customSearchPattern");
            
            // Header with action name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Action {index + 1}: {logicalNameProp.stringValue}", EditorStyles.boldLabel);
            
            // Quick actions
            if (GUILayout.Button("Auto", GUILayout.Width(50)))
            {
                AutoAssignSingleClip(index);
            }
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                RemoveMapping(index);
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            // Action name
            EditorGUILayout.PropertyField(logicalNameProp, new GUIContent("Action Name"));
            
            // Auto-assignment toggle
            EditorGUILayout.PropertyField(useAutoAssignmentProp, new GUIContent("Use Auto-Assignment"));
            
            if (useAutoAssignmentProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customSearchPatternProp, new GUIContent("Custom Search Pattern"));
                if (string.IsNullOrEmpty(customSearchPatternProp.stringValue))
                {
                    EditorGUILayout.HelpBox($"Will search for: {characterDef.CharacterName}_{logicalNameProp.stringValue}, {logicalNameProp.stringValue}", MessageType.Info);
                }
                EditorGUI.indentLevel--;
            }
            
            // Animation clip
            EditorGUILayout.PropertyField(clipProp, new GUIContent("Animation Clip"));
            
            // Validation indicator
            if (clipProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No animation clip assigned!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Animation clip assigned", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawValidationSection()
        {
            showValidation = EditorGUILayout.Foldout(showValidation, "Validation & Status", true);
            
            if (!showValidation) return;
            
            EditorGUILayout.BeginVertical("box");
            
            if (!characterDef.HasExplicitCharacterId)
            {
                EditorGUILayout.HelpBox("Character ID is missing. It will resolve from the name, but should be explicitly stored.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"Character ID: {characterDef.CharacterId}", MessageType.Info);
            }

            // Character name validation
            if (string.IsNullOrEmpty(characterDef.CharacterName))
            {
                EditorGUILayout.HelpBox("Character name is empty!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox($"Character: {characterDef.CharacterName}", MessageType.Info);
            }
            
            // Animation mappings validation
            int validMappings = characterDef.animationMappings.Count(m => m.IsValid);
            int totalMappings = characterDef.animationMappings.Count;
            
            if (totalMappings == 0)
            {
                EditorGUILayout.HelpBox("No animation mappings defined", MessageType.Warning);
            }
            else if (validMappings == totalMappings)
            {
                EditorGUILayout.HelpBox($"✓ All {validMappings} animation mappings are valid", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"⚠ {validMappings}/{totalMappings} animation mappings are valid", MessageType.Warning);
            }
            
            // Animator controller validation
            if (characterDef.AnimatorController == null)
            {
                EditorGUILayout.HelpBox("No Animator Controller assigned!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Animator Controller assigned", MessageType.Info);
            }

            if (characterDef.SpecialAbilityDefinition == null && !string.IsNullOrWhiteSpace(characterDef.SpecialAbilityName))
            {
                EditorGUILayout.HelpBox("Special ability data is still stored in legacy CharacterDefinition fields. Use Character Database > Sync Character Abilities To Ability Definitions to promote it into the new ability database.", MessageType.Warning);
            }
            else if (characterDef.SpecialAbilityDefinition != null)
            {
                EditorGUILayout.HelpBox($"✓ Special ability definition linked: {characterDef.SpecialAbilityDefinition.DisplayName}", MessageType.Info);
            }

            bool hasSpecialAttackClip = characterDef.animationMappings.Any(mapping => mapping != null && mapping.clip != null && mapping.logicalName == "SPECIAL ATTACK");
            if (characterDef.UltimateAbilityDefinition != null)
            {
                EditorGUILayout.HelpBox($"✓ Ultimate ability definition linked: {characterDef.UltimateAbilityDefinition.DisplayName}", MessageType.Info);
            }
            else if (hasSpecialAttackClip)
            {
                EditorGUILayout.HelpBox("SPECIAL ATTACK animation exists but no ultimate ability definition is linked. Use Character Database > Sync Character Abilities To Ability Definitions.", MessageType.Warning);
            }

            ValidateProductionReference(characterDef.BattleSprite, "Battle Sprite");
            ValidateProductionReference(characterDef.Portrait, "Portrait");
            ValidateProductionReference(characterDef.BannerSprite, "Banner");
            ValidateProductionReference(characterDef.EventVignette, "Event Vignette");
            ValidateProductionReference(characterDef.AnimatorController, "Animator Controller");
            ValidateBattleSpriteImportSettings(characterDef.BattleSprite);

            if (GUILayout.Button("Rebuild Character Catalog"))
            {
                CharacterCatalogEditorUtility.RebuildCatalog();
            }

            if (GUILayout.Button("Fix Production Sprite Import Settings"))
            {
                CharacterCatalogEditorUtility.NormalizeProductionSpriteImportSettings();
            }
            
            EditorGUILayout.EndVertical();
        }

        private static void ValidateProductionReference(Object reference, string label)
        {
            if (reference == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(reference);
            if (!string.IsNullOrEmpty(assetPath) && !assetPath.StartsWith(CharacterCatalogEditorUtility.ProductionRoot))
            {
                EditorGUILayout.HelpBox($"{label} is outside Art/Production: {assetPath}", MessageType.Warning);
            }
        }

        private static void ValidateBattleSpriteImportSettings(Sprite battleSprite)
        {
            if (battleSprite == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(battleSprite);
            if (!CharacterSpriteImportPolicy.ShouldEnforce(assetPath))
                return;

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            if (!CharacterSpriteImportPolicy.IsCompliant(importer, out string issues))
            {
                EditorGUILayout.HelpBox($"Battle Sprite import quality issue: {issues}", MessageType.Warning);
            }
        }
        
        private void AutoPopulateCommonActions()
        {
            // Clear existing mappings
            characterDef.animationMappings.Clear();
            
            // Add common actions
            foreach (string action in commonAnimationActions)
            {
                characterDef.animationMappings.Add(new AnimationMapping(action));
            }
            
            EditorUtility.SetDirty(characterDef);
            Debug.Log($"Auto-populated {commonAnimationActions.Length} common animation actions for {characterDef.CharacterName}");
            CharacterCatalogEditorUtility.RebuildCatalog(false);
        }
        
        private void AutoAssignAllClips()
        {
            int assignedCount = 0;
            
            foreach (var mapping in characterDef.animationMappings)
            {
                if (mapping.TryAutoAssignClip(characterDef.CharacterName))
                {
                    assignedCount++;
                }
            }
            
            EditorUtility.SetDirty(characterDef);
            Debug.Log($"Auto-assigned {assignedCount} animation clips for {characterDef.CharacterName}");
            CharacterCatalogEditorUtility.RebuildCatalog(false);
        }
        
        private void AutoAssignSingleClip(int index)
        {
            if (index >= 0 && index < characterDef.animationMappings.Count)
            {
                var mapping = characterDef.animationMappings[index];
                if (mapping.TryAutoAssignClip(characterDef.CharacterName))
                {
                    EditorUtility.SetDirty(characterDef);
                    Debug.Log($"Auto-assigned clip for {mapping.logicalName}");
                    CharacterCatalogEditorUtility.RebuildCatalog(false);
                }
                else
                {
                    Debug.LogWarning($"Could not auto-assign clip for {mapping.logicalName}");
                }
            }
        }
        
        private void ClearAllMappings()
        {
            if (EditorUtility.DisplayDialog("Clear All Mappings", 
                "Are you sure you want to clear all animation mappings?", "Yes", "No"))
            {
                characterDef.animationMappings.Clear();
                EditorUtility.SetDirty(characterDef);
                Debug.Log("Cleared all animation mappings");
                CharacterCatalogEditorUtility.RebuildCatalog(false);
            }
        }
        
        private void RemoveMapping(int index)
        {
            if (index >= 0 && index < characterDef.animationMappings.Count)
            {
                characterDef.animationMappings.RemoveAt(index);
                EditorUtility.SetDirty(characterDef);
                CharacterCatalogEditorUtility.RebuildCatalog(false);
            }
        }
    }
}
#endif
