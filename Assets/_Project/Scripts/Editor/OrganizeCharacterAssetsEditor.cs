#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Shogun.Features.Characters;

public class OrganizeCharacterAssetsEditor : EditorWindow
{
    private string characterName = "Ryoma";
    private string spriteSourceFolder = "Assets/_Project/Features/Characters/Art/Sprites/Samurai/FULL_Samurai 2D Pixel Art v1.2/FULL_Samurai 2D Pixel Art v1.2/Sprites";
    private string animClipSourceFolder = "Assets/_Project/Features/Characters/Art/Animations/Samurai/Clips";
    private string controllerSourceFolder = "Assets/_Project/Features/Characters/Art/Sprites/Samurai/FULL_Samurai 2D Pixel Art v1.2/FULL_Samurai 2D Pixel Art v1.2/Animations";
    private string charDefPath = "Assets/_Project/Features/Characters/ScriptableObjects/Ryoma_CharacterDefinition.asset";

    private string targetRoot = "Assets/_Project/Features/Characters/Art/Sprites/Samurai";

    [MenuItem("Tools/Shogun/Organize Character Assets")]
    public static void ShowWindow()
    {
        GetWindow<OrganizeCharacterAssetsEditor>("Organize Character Assets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Organize & Rename Character Assets", EditorStyles.boldLabel);
        characterName = EditorGUILayout.TextField("Character Name", characterName);
        spriteSourceFolder = EditorGUILayout.TextField("Sprite Source Folder", spriteSourceFolder);
        animClipSourceFolder = EditorGUILayout.TextField("AnimClip Source Folder", animClipSourceFolder);
        controllerSourceFolder = EditorGUILayout.TextField("Controller Source Folder", controllerSourceFolder);
        charDefPath = EditorGUILayout.TextField("CharacterDefinition Asset", charDefPath);
        targetRoot = EditorGUILayout.TextField("Target Character Root", targetRoot);

        if (GUILayout.Button("Organize & Rename"))
        {
            OrganizeAndRename();
        }
    }

    private void OrganizeAndRename()
    {
        string charFolder = Path.Combine(targetRoot, characterName);
        string spritesTarget = Path.Combine(charFolder, "Sprites").Replace("\\", "/");
        string animsTarget = Path.Combine(charFolder, "Animations").Replace("\\", "/");
        Directory.CreateDirectory(spritesTarget);
        Directory.CreateDirectory(animsTarget);

        // 1. Move & rename sprite sheets
        var spriteFiles = Directory.GetFiles(spriteSourceFolder, "*.png");
        foreach (var file in spriteFiles)
        {
            string fileName = Path.GetFileName(file);
            string newName = characterName + "_" + fileName;
            string destPath = Path.Combine(spritesTarget, newName).Replace("\\", "/");
            AssetDatabase.MoveAsset(file.Replace("\\", "/"), destPath);
            // Move .meta if exists
            string meta = file + ".meta";
            if (File.Exists(meta))
            {
                AssetDatabase.MoveAsset(meta.Replace("\\", "/"), destPath + ".meta");
            }
        }

        // 2. Move & rename animation clips
        var animFiles = Directory.GetFiles(animClipSourceFolder, "*.anim");
        foreach (var file in animFiles)
        {
            string fileName = Path.GetFileName(file);
            string newName = characterName + "_" + fileName;
            string destPath = Path.Combine(animsTarget, newName).Replace("\\", "/");
            AssetDatabase.MoveAsset(file.Replace("\\", "/"), destPath);
            // Move .meta if exists
            string meta = file + ".meta";
            if (File.Exists(meta))
            {
                AssetDatabase.MoveAsset(meta.Replace("\\", "/"), destPath + ".meta");
            }
        }

        // 3. Move & rename controller(s)
        var controllerFiles = Directory.GetFiles(controllerSourceFolder, "*.controller");
        foreach (var file in controllerFiles)
        {
            string fileName = Path.GetFileName(file);
            string newName = characterName + ".controller";
            string destPath = Path.Combine(animsTarget, newName).Replace("\\", "/");
            AssetDatabase.MoveAsset(file.Replace("\\", "/"), destPath);
            // Move .meta if exists
            string meta = file + ".meta";
            if (File.Exists(meta))
            {
                AssetDatabase.MoveAsset(meta.Replace("\\", "/"), destPath + ".meta");
            }
        }

        AssetDatabase.Refresh();

        // 4. Update CharacterDefinition references
        var charDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(charDefPath);
        if (charDef != null)
        {
            // Update animator controller
            string controllerPath = Path.Combine(animsTarget, characterName + ".controller").Replace("\\", "/");
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller != null)
            {
                SerializedObject so = new SerializedObject(charDef);
                var prop = so.FindProperty("animatorController");
                if (prop != null)
                {
                    prop.objectReferenceValue = controller;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(charDef);
                }
            }
            // Update animationMappings (auto-assign by new names)
            foreach (var mapping in charDef.animationMappings)
            {
                string animPath = Path.Combine(animsTarget, characterName + "_" + mapping.logicalName + ".anim").Replace("\\", "/");
                var animClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
                if (animClip != null)
                {
                    mapping.clip = animClip;
                }
            }
            EditorUtility.SetDirty(charDef);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Organized and renamed all assets for {characterName}!");
    }
}
#endif 