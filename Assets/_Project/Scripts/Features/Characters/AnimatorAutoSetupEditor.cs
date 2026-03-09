#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class AnimatorAutoSetupEditor : EditorWindow
{
    private string spritesFolder = "Assets/_Project/Features/Characters/Art/Production/PlayableSprites/ryoma";
    private string animClipsFolder = "Assets/_Project/Features/Characters/Art/Production/Animations/ryoma";
    private string animatorControllerPath = "Assets/_Project/Features/Characters/Art/Production/Animations/ryoma/Ryoma_Ryoma.controller";
    private string[] animationNames = new string[] { "IDLE", "RUN", "ATTACK 1", "ATTACK 2", "ATTACK 3", "HURT", "HEALING", "DEATH", "DEFEND", "SPECIAL ATTACK", "DASH", "CLIMBING", "JUMP", "JUMP-START", "JUMP-FALL", "JUMP-TRANSITION", "THROW", "WALK", "WALL CONTACT", "WALL JUMP", "WALL SLIDE", "HEALING NO EFFECT", "AIR ATTACK" };
    private struct StateParam
    {
        public string state;
        public string param;
        public AnimatorControllerParameterType type;
        public object value;
    }
    private StateParam[] stateParams = new StateParam[] {
        new StateParam { state = "RUN", param = "isRunning", type = AnimatorControllerParameterType.Bool, value = true },
        new StateParam { state = "IDLE", param = "isRunning", type = AnimatorControllerParameterType.Bool, value = false },
        new StateParam { state = "ATTACK 1", param = "AttackTrigger", type = AnimatorControllerParameterType.Trigger },
        new StateParam { state = "ATTACK 2", param = "AttackTrigger", type = AnimatorControllerParameterType.Trigger },
        new StateParam { state = "ATTACK 3", param = "AttackTrigger", type = AnimatorControllerParameterType.Trigger },
        new StateParam { state = "HURT", param = "isHurt", type = AnimatorControllerParameterType.Trigger },
        new StateParam { state = "DEATH", param = "isDead", type = AnimatorControllerParameterType.Bool, value = true },
        new StateParam { state = "DEFEND", param = "isDefending", type = AnimatorControllerParameterType.Trigger },
        new StateParam { state = "HEALING", param = "isHealing", type = AnimatorControllerParameterType.Trigger },
        new StateParam { state = "SPECIAL ATTACK", param = "SpecialTrigger", type = AnimatorControllerParameterType.Trigger },
    };

    [MenuItem("Shogun/Auto-Setup Samurai Animator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorAutoSetupEditor>("Auto-Setup Samurai Animator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto-Setup Samurai Animator", EditorStyles.boldLabel);
        spritesFolder = EditorGUILayout.TextField("Sprites Folder", spritesFolder);
        animClipsFolder = EditorGUILayout.TextField("Anim Clips Folder", animClipsFolder);
        animatorControllerPath = EditorGUILayout.TextField("Animator Controller Path", animatorControllerPath);
        if (GUILayout.Button("Auto-Setup Animator"))
        {
            AutoSetupAnimator();
        }
        if (GUILayout.Button("Fix Existing Animation Clips"))
        {
            FixExistingAnimationClips();
        }
    }

    private void AutoSetupAnimator()
    {
        // Ensure clips folder exists
        if (!AssetDatabase.IsValidFolder(animClipsFolder))
        {
            Directory.CreateDirectory(animClipsFolder);
            AssetDatabase.Refresh();
        }

        // Load or create Animator Controller
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(animatorControllerPath);
        }

        // Remove all existing states except Entry/AnyState/Exit
        var layer = controller.layers[0];
        var stateMachine = layer.stateMachine;
        var statesToRemove = stateMachine.states.Where(s => s.state.name != "Entry" && s.state.name != "AnyState" && s.state.name != "Exit").ToArray();
        foreach (var s in statesToRemove)
        {
            stateMachine.RemoveState(s.state);
        }

        // Add parameters if not present
        foreach (var sp in stateParams)
        {
            if (!controller.parameters.Any(p => p.name == sp.param))
            {
                controller.AddParameter(sp.param, sp.type);
            }
        }

        // For each animation name, look for a matching sprite sheet and create an AnimationClip
        Dictionary<string, AnimatorState> createdStates = new Dictionary<string, AnimatorState>();
        foreach (string animName in animationNames)
        {
            string spriteSheetPath = Path.Combine(spritesFolder, animName + ".png").Replace("\\", "/");
            if (!File.Exists(spriteSheetPath))
                continue;
            // Create or load AnimationClip
            string clipPath = Path.Combine(animClipsFolder, animName + ".anim").Replace("\\", "/");
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = CreateAnimationClipFromSpriteSheet(spriteSheetPath, animName, clipPath);
            }
            // Add state to Animator Controller
            var state = stateMachine.states.FirstOrDefault(s => s.state.name == animName).state;
            if (state == null)
            {
                state = stateMachine.AddState(animName);
            }
            state.motion = clip;
            createdStates[animName] = state;
        }

        // Set up transitions
        // Idle <-> Run (isRunning)
        if (createdStates.ContainsKey("IDLE") && createdStates.ContainsKey("RUN"))
        {
            var idle = createdStates["IDLE"];
            var run = createdStates["RUN"];
            var t1 = idle.AddTransition(run);
            t1.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
            t1.hasExitTime = false;
            t1.exitTime = 0f;
            var t2 = run.AddTransition(idle);
            t2.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
            t2.hasExitTime = false;
            t2.exitTime = 0f;
        }
        // Any State -> Attack (AttackTrigger)
        foreach (var atk in new[] { "ATTACK 1", "ATTACK 2", "ATTACK 3" })
        {
            if (createdStates.ContainsKey(atk))
            {
                var t = stateMachine.AddAnyStateTransition(createdStates[atk]);
                t.AddCondition(AnimatorConditionMode.If, 0, "AttackTrigger");
                t.hasExitTime = false;
            }
        }
        // Any State -> Death (isDead)
        if (createdStates.ContainsKey("DEATH"))
        {
            var t = stateMachine.AddAnyStateTransition(createdStates["DEATH"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "isDead");
            t.hasExitTime = false;
        }
        // Any State -> Hurt (isHurt)
        if (createdStates.ContainsKey("HURT"))
        {
            var t = stateMachine.AddAnyStateTransition(createdStates["HURT"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "isHurt");
            t.hasExitTime = false;
        }
        // Any State -> Defend (isDefending)
        if (createdStates.ContainsKey("DEFEND"))
        {
            var t = stateMachine.AddAnyStateTransition(createdStates["DEFEND"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "isDefending");
            t.hasExitTime = false;
        }
        // Any State -> Healing (isHealing)
        if (createdStates.ContainsKey("HEALING"))
        {
            var t = stateMachine.AddAnyStateTransition(createdStates["HEALING"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "isHealing");
            t.hasExitTime = false;
        }
        // Any State -> Special (SpecialTrigger)
        if (createdStates.ContainsKey("SPECIAL ATTACK"))
        {
            var t = stateMachine.AddAnyStateTransition(createdStates["SPECIAL ATTACK"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "SpecialTrigger");
            t.hasExitTime = false;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Samurai Animator auto-setup (with parameters and transitions) complete.");
    }

    private void FixExistingAnimationClips()
    {
        // Fix existing animation clips to ensure proper loop settings
        string[] clipPaths = {
            Path.Combine(animClipsFolder, "RUN.anim").Replace("\\", "/"),
            Path.Combine(animClipsFolder, "IDLE.anim").Replace("\\", "/")
        };
        
        foreach (string clipPath in clipPaths)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip != null)
            {
                // Get the sprite sheet path to recreate the clip properly
                string animName = Path.GetFileNameWithoutExtension(clipPath);
                string spriteSheetPath = Path.Combine(spritesFolder, animName + ".png").Replace("\\", "/");
                
                if (File.Exists(spriteSheetPath))
                {
                    // Delete the old clip and recreate it with proper settings
                    AssetDatabase.DeleteAsset(clipPath);
                    var newClip = CreateAnimationClipFromSpriteSheet(spriteSheetPath, animName, clipPath);
                    SetLoopTime(newClip, true);
                    Debug.Log($"Recreated {clipPath} with proper loop settings");
                }
                else
                {
                    // Just fix the existing clip settings
                    clip.wrapMode = WrapMode.Loop;
                    clip.frameRate = 12f;
                    SetLoopTime(clip, true);
                    EditorUtility.SetDirty(clip);
                    Debug.Log($"Fixed loop settings for {clipPath}");
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("Fixed existing animation clips loop settings.");
    }

    private AnimationClip CreateAnimationClipFromSpriteSheet(string spriteSheetPath, string animName, string clipPath)
    {
        // Load all sprites from the sheet
        UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spriteSheetPath);
        var spriteFrames = sprites.OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (spriteFrames.Length == 0)
        {
            Debug.LogWarning($"No sprites found in {spriteSheetPath} for {animName}");
            return null;
        }
        
        AnimationClip clip = new AnimationClip();
        
        // Set loop settings - RUN and IDLE should loop, others might not
        bool shouldLoop = animName == "RUN" || animName == "IDLE";
        clip.wrapMode = shouldLoop ? WrapMode.Loop : WrapMode.Once;
        // Set the Unity 'Loop Time' property
        SetLoopTime(clip, shouldLoop);
        
        // Set the clip length to match the animation duration
        clip.frameRate = 12f;
        
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        
        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[spriteFrames.Length];
        float frameTime = 1f / 12f; // 12 FPS
        
        for (int i = 0; i < spriteFrames.Length; i++)
        {
            keyFrames[i] = new ObjectReferenceKeyframe
            {
                time = i * frameTime,
                value = spriteFrames[i]
            };
        }
        
        // For looping animations, add an extra frame at the end that matches the first frame
        // This ensures smooth looping
        if (shouldLoop && spriteFrames.Length > 1)
        {
            Array.Resize(ref keyFrames, keyFrames.Length + 1);
            keyFrames[keyFrames.Length - 1] = new ObjectReferenceKeyframe
            {
                time = spriteFrames.Length * frameTime,
                value = spriteFrames[0] // Loop back to first frame
            };
        }
        
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);
        
        // Note: clip.length is read-only, it's automatically set based on the keyframes
        // The length will be determined by the last keyframe time
        
        AssetDatabase.CreateAsset(clip, clipPath);
        
        // Force the asset to be marked as dirty so it saves properly
        EditorUtility.SetDirty(clip);
        
        Debug.Log($"Created animation clip for {animName} with {spriteFrames.Length} frames, loop: {shouldLoop}, length: {keyFrames[keyFrames.Length - 1].time}s");
        
        return clip;
    }

    private void SetLoopTime(AnimationClip clip, bool shouldLoop)
    {
        if (clip == null) return;
        var so = new SerializedObject(clip);
        var loopProp = so.FindProperty("m_AnimationClipSettings.m_LoopTime");
        if (loopProp != null)
        {
            loopProp.boolValue = shouldLoop;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(clip);
            Debug.Log($"Set Loop Time = {shouldLoop} for {clip.name}");
        }
    }
}
#endif
