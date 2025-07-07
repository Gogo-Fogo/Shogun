#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AnimationClipDiagnosticTool : EditorWindow
{
    private string animClipsFolder = "Assets/_Project/Features/Characters/Art/Animations/Samurai/Clips";
    private bool offerAutoFix = false;

    [MenuItem("Shogun/Diagnose Samurai Animation Clips")]
    public static void ShowWindow()
    {
        GetWindow<AnimationClipDiagnosticTool>("AnimationClip Diagnostic Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Samurai Animation Clip Diagnostic Tool", EditorStyles.boldLabel);
        animClipsFolder = EditorGUILayout.TextField("Anim Clips Folder", animClipsFolder);
        offerAutoFix = EditorGUILayout.Toggle("Offer Auto-Fix", offerAutoFix);
        if (GUILayout.Button("Scan Animation Clips"))
        {
            ScanAnimationClips();
        }
    }

    private void ScanAnimationClips()
    {
        string[] clipGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { animClipsFolder });
        int total = 0, broken = 0;
        foreach (string guid in clipGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;
            total++;
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName == "m_Sprite")
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    bool hasNull = keyframes.Any(kf => kf.value == null);
                    if (hasNull)
                    {
                        broken++;
                        Debug.LogWarning($"[BROKEN] {clip.name} at {path} has null sprite keyframes.");
                        if (offerAutoFix)
                        {
                            var fixedKeyframes = keyframes.Where(kf => kf.value != null).ToArray();
                            AnimationUtility.SetObjectReferenceCurve(clip, binding, fixedKeyframes);
                            EditorUtility.SetDirty(clip);
                            Debug.Log($"[FIXED] Removed null keyframes from {clip.name}.");
                        }
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Scan complete. {total} clips checked, {broken} had issues.");
    }
}
#endif 