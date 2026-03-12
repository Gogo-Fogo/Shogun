#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Shogun.Features.Combat;

/// <summary>
/// One-shot bootstrapper: creates MedallionFrameCatalog.asset and wires all
/// production frame sprites found in Art/Production/UI/HUD/.
///
/// Runs automatically after every script compile (DidReloadScripts).
/// Self-skips once the asset exists. Delete this file when done.
/// </summary>
[InitializeOnLoad]
public static class CreateMedallionFrameCatalog
{
    private const string CatalogPath   = "Assets/_Project/Art/Production/UI/HUD/MedallionFrameCatalog.asset";
    private const string SpritesFolder = "Assets/_Project/Art/Production/UI/HUD";

    // Longer tokens first so "10hole" isn't matched by "0hole".
    private static readonly (string token, int holes)[] HoleMapping =
    {
        ("10hole", 10),
        ("12hole", 12),
        ("2hole",  2),
        ("3hole",  3),
        ("4hole",  4),
        ("5hole",  5),
        ("6hole",  6),
        ("8hole",  8),
        ("9hole",  9),
    };

    static CreateMedallionFrameCatalog()
    {
        // Defer to after domain reload so AssetDatabase is ready.
        EditorApplication.delayCall += TryCreate;
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        EditorApplication.delayCall += TryCreate;
    }

    [MenuItem("Tools/Shogun/MCP/Create Medallion Frame Catalog")]
    public static void TryCreate()
    {
        MedallionFrameCatalog catalog = AssetDatabase.LoadAssetAtPath<MedallionFrameCatalog>(CatalogPath);

        bool isNew = catalog == null;
        if (isNew)
        {
            catalog = ScriptableObject.CreateInstance<MedallionFrameCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            Debug.Log($"[MedallionCatalog] Created new asset at {CatalogPath}");
        }
        else
        {
            // Already exists — skip silent auto-creation; only update via menu.
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { SpritesFolder });

        SerializedObject   so           = new SerializedObject(catalog);
        SerializedProperty entriesProp  = so.FindProperty("entries");
        SerializedProperty fallbackProp = so.FindProperty("fallbackFrame");

        entriesProp.ClearArray();
        int entryIndex = 0;

        foreach ((string token, int holes) in HoleMapping)
        {
            Sprite found = FindSprite(guids, token);
            if (found == null)
            {
                Debug.LogWarning($"[MedallionCatalog] No sprite for token '{token}' (holes={holes}) — skipping.");
                continue;
            }

            entriesProp.InsertArrayElementAtIndex(entryIndex);
            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(entryIndex);
            entry.FindPropertyRelative("holeCount").intValue         = holes;
            entry.FindPropertyRelative("frame").objectReferenceValue = found;
            Debug.Log($"[MedallionCatalog]   holes={holes:D2} → {found.name}");
            entryIndex++;
        }

        fallbackProp.objectReferenceValue = FindSprite(guids, "4hole");

        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(catalog);

        Selection.activeObject = catalog;
        EditorGUIUtility.PingObject(catalog);

        Debug.Log($"[MedallionCatalog] Done — {entryIndex} entries written to {CatalogPath}");
    }

    private static Sprite FindSprite(string[] guids, string token)
    {
        string tokenLower = token.ToLowerInvariant();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.ToLowerInvariant().Contains(tokenLower))
            {
                Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (spr != null)
                    return spr;
            }
        }
        return null;
    }
}
#endif
