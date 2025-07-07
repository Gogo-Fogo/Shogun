#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Shogun.Features.Characters;

public class CharacterAnimatorAutoLinker : EditorWindow
{
    private RuntimeAnimatorController animatorController;
    private List<RuntimeAnimatorController> allControllers = new List<RuntimeAnimatorController>();
    private string[] controllerNames;
    private int selectedControllerIndex = 0;

    [MenuItem("Shogun/Batch Assign Animator Controller (Robust)")]
    public static void ShowWindow()
    {
        GetWindow<CharacterAnimatorAutoLinker>("Batch Assign Animator Controller");
    }

    private void OnEnable()
    {
        LoadAllAnimatorControllers();
    }

    private void LoadAllAnimatorControllers()
    {
        allControllers.Clear();
        List<string> names = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:RuntimeAnimatorController");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
            if (ctrl != null)
            {
                allControllers.Add(ctrl);
                names.Add(ctrl.name + " (" + path + ")");
            }
        }
        controllerNames = names.ToArray();
        if (allControllers.Count > 0 && animatorController == null)
        {
            animatorController = allControllers[0];
            selectedControllerIndex = 0;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Assign Animator Controller", EditorStyles.boldLabel);
        if (allControllers.Count == 0)
        {
            EditorGUILayout.HelpBox("No Animator Controllers found in project.", MessageType.Warning);
            if (GUILayout.Button("Refresh"))
                LoadAllAnimatorControllers();
            return;
        }
        EditorGUILayout.LabelField("Select Animator Controller:");
        int newIndex = EditorGUILayout.Popup(selectedControllerIndex, controllerNames);
        if (newIndex != selectedControllerIndex)
        {
            selectedControllerIndex = newIndex;
            animatorController = allControllers[selectedControllerIndex];
        }
        animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Or Drag Controller Here", animatorController, typeof(RuntimeAnimatorController), false);
        if (GUILayout.Button("Assign to All CharacterDefinitions"))
        {
            AssignAnimatorControllerToAll();
        }
        if (GUILayout.Button("Refresh Controllers List"))
        {
            LoadAllAnimatorControllers();
        }
    }

    private void AssignAnimatorControllerToAll()
    {
        if (animatorController == null)
        {
            Debug.LogError("No Animator Controller selected or assigned. Please select or drag a valid Animator Controller.");
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:CharacterDefinition");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterDefinition def = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);
            if (def != null)
            {
                SerializedObject so = new SerializedObject(def);
                var prop = so.FindProperty("animatorController");
                if (prop != null)
                {
                    prop.objectReferenceValue = animatorController;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(def);
                    count++;
                }
                else
                {
                    Debug.LogWarning($"No 'animatorController' field found on {def.name} at {path}.");
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Assigned Animator Controller to {count} CharacterDefinition assets.");
    }
}
#endif 