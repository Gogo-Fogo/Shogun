#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shogun.Core.Architecture;
using Shogun.Features.Characters;
using Shogun.Features.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShogunMcpDiagnosticTools
{
    private const string MenuRoot = "Tools/Shogun/MCP/";
    private const string LogPrefix = "[Shogun MCP]";

    [MenuItem(MenuRoot + "Report Active Scene Roots")]
    public static void ReportActiveSceneRoots()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"{LogPrefix} No active loaded scene is available.");
            return;
        }

        var builder = new StringBuilder();
        GameObject[] roots = scene.GetRootGameObjects();

        builder.AppendLine($"{LogPrefix} Active Scene Roots");
        builder.AppendLine($"Scene: {scene.name}");
        builder.AppendLine($"Path: {scene.path}");
        builder.AppendLine($"Root Count: {roots.Length}");

        foreach (GameObject root in roots)
        {
            string components = string.Join(", ", root.GetComponents<Component>().Select(GetComponentName));
            builder.AppendLine($"- {root.name} | children={root.transform.childCount} | components={components}");
        }

        Debug.Log(builder.ToString());
    }

    [MenuItem(MenuRoot + "Report Selected GameObject")]
    public static void ReportSelectedGameObject()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning($"{LogPrefix} No GameObject is currently selected.");
            return;
        }

        var builder = new StringBuilder();
        Component[] components = selected.GetComponents<Component>();

        builder.AppendLine($"{LogPrefix} Selected GameObject");
        builder.AppendLine($"Name: {selected.name}");
        builder.AppendLine($"Path: {GetHierarchyPath(selected.transform)}");
        builder.AppendLine($"Instance ID: {selected.GetInstanceID()}");
        builder.AppendLine($"Active Self: {selected.activeSelf}");
        builder.AppendLine($"Active In Hierarchy: {selected.activeInHierarchy}");
        builder.AppendLine($"Tag: {selected.tag}");
        builder.AppendLine($"Layer: {LayerMask.LayerToName(selected.layer)} ({selected.layer})");
        builder.AppendLine($"Child Count: {selected.transform.childCount}");
        builder.AppendLine($"Components ({components.Length}):");

        foreach (Component component in components)
        {
            builder.AppendLine($"- {GetComponentName(component)}");
        }

        Debug.Log(builder.ToString());
    }

    [MenuItem(MenuRoot + "Validate Active Scene Combat Setup")]
    public static void ValidateActiveSceneCombatSetup()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        var report = new ValidationReport("Active Scene Combat Setup");

        if (!scene.IsValid() || !scene.isLoaded)
        {
            report.Errors.Add("No active loaded scene is available.");
            report.Log();
            return;
        }

        report.Info.Add($"Scene: {scene.name}");
        report.Info.Add($"Path: {scene.path}");

        BattleManager battleManager = FindSingleInScene<BattleManager>(scene);
        CombatInputHandler combatInput = FindSingleInScene<CombatInputHandler>(scene);
        GestureRecognizer gestureRecognizer = FindSingleInScene<GestureRecognizer>(scene);
        InputManager inputManager = FindSingleInScene<InputManager>(scene);
        TurnManager turnManager = FindSingleInScene<TurnManager>(scene);
        BattlefieldManager battlefieldManager = FindSingleInScene<BattlefieldManager>(scene);
        MiniGameManager miniGameManager = FindSingleInScene<MiniGameManager>(scene);
        TestBattleSetup testBattleSetup = FindSingleInScene<TestBattleSetup>(scene);
        BattleDragHandler[] dragHandlers = FindInScene<BattleDragHandler>(scene);
        Camera mainCamera = Camera.main;
        EventSystem eventSystem = FindSingleInScene<EventSystem>(scene);
        Canvas[] canvases = FindInScene<Canvas>(scene);

        ValidateRequiredComponent(battleManager, "BattleManager", report);
        ValidateRequiredComponent(combatInput, "CombatInputHandler", report);
        ValidateRequiredComponent(gestureRecognizer, "GestureRecognizer", report);
        ValidateRequiredComponent(inputManager, "InputManager", report);
        ValidateRequiredComponent(turnManager, "TurnManager", report);
        ValidateRequiredComponent(battlefieldManager, "BattlefieldManager", report);

        if (miniGameManager == null)
        {
            report.Warnings.Add("MiniGameManager is missing from the active scene.");
        }

        if (mainCamera == null)
        {
            report.Errors.Add("No Main Camera is tagged in the active scene.");
        }

        if (eventSystem == null)
        {
            report.Warnings.Add("No EventSystem was found in the active scene.");
        }

        if (canvases.Length == 0)
        {
            report.Warnings.Add("No Canvas was found in the active scene.");
        }

        ValidateDragHandlers(dragHandlers, turnManager, report);
        ValidateGestureRecognizerBindings(gestureRecognizer, report);
        ValidateCombatInputReferences(combatInput, battleManager, turnManager, report);
        ValidateBattleManagerPrefab(battleManager, report);
        ValidateTestBattleSetup(testBattleSetup, battleManager, turnManager, report);

        report.Log();
    }

    [MenuItem(MenuRoot + "Report Battle Runtime State")]
    public static void ReportBattleRuntimeState()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"{LogPrefix} No active loaded scene is available.");
            return;
        }

        BattleManager battleManager = FindSingleInScene<BattleManager>(scene);
        CombatInputHandler combatInput = FindSingleInScene<CombatInputHandler>(scene);
        TurnManager turnManager = FindSingleInScene<TurnManager>(scene);
        CharacterInstance[] characters = FindInScene<CharacterInstance>(scene);

        var builder = new StringBuilder();
        builder.AppendLine($"{LogPrefix} Battle Runtime State");
        builder.AppendLine($"Scene: {scene.name}");
        builder.AppendLine($"Path: {scene.path}");

        if (battleManager == null)
        {
            builder.AppendLine("BattleManager: missing");
        }
        else
        {
            builder.AppendLine($"BattleManager: {GetHierarchyPath(battleManager.transform)}");
            builder.AppendLine($"Character Prefab Ref: {(battleManager.characterPrefab != null ? battleManager.characterPrefab.name : "null")}");
            builder.AppendLine($"Active Characters: {battleManager.activeCharacters.Count}");
            AppendCharacterList(builder, "Active Character Details", battleManager.activeCharacters);
            builder.AppendLine($"Reserve Characters: {battleManager.reserveCharacters.Count}");
            AppendCharacterList(builder, "Reserve Character Details", battleManager.reserveCharacters);
        }

        if (turnManager == null)
        {
            builder.AppendLine("TurnManager: missing");
        }
        else
        {
            builder.AppendLine($"TurnManager: {GetHierarchyPath(turnManager.transform)}");
            builder.AppendLine($"IsBattleActive: {turnManager.IsBattleActive}");
            builder.AppendLine($"CurrentTurnIndex: {turnManager.CurrentTurnIndex}");
            builder.AppendLine($"TurnOrder Count: {turnManager.turnOrder.Count}");
            builder.AppendLine($"Current Character: {DescribeCharacter(turnManager.GetCurrentCharacter())}");
            builder.AppendLine($"Current Combatant: {DescribeCharacter(turnManager.GetCurrentCombatant())}");
        }

        if (combatInput == null)
        {
            builder.AppendLine("CombatInputHandler: missing");
        }
        else
        {
            builder.AppendLine($"CombatInputHandler: {GetHierarchyPath(combatInput.transform)}");
            builder.AppendLine($"SelectedCharacterIndex: {combatInput.selectedCharacterIndex}");
            builder.AppendLine($"CombatInputHandler Active Characters: {combatInput.activeCharacters.Count}");
        }

        builder.AppendLine($"Scene CharacterInstance Count: {characters.Length}");
        foreach (CharacterInstance character in characters)
        {
            builder.AppendLine($"- {DescribeCharacter(character)}");
        }

        Debug.Log(builder.ToString());
    }

    [MenuItem(MenuRoot + "Validate Render Pipeline Setup")]
    public static void ValidateRenderPipelineSetup()
    {
        var report = new ValidationReport("Render Pipeline Setup");
        RenderPipelineAsset pipelineAsset = GraphicsSettings.defaultRenderPipeline;

        if (pipelineAsset == null)
        {
            report.Errors.Add("GraphicsSettings.defaultRenderPipeline is null.");
            report.Log();
            return;
        }

        string pipelineAssetPath = AssetDatabase.GetAssetPath(pipelineAsset);
        report.Info.Add($"Render Pipeline Asset: {pipelineAsset.name}");
        report.Info.Add($"Asset Path: {pipelineAssetPath}");

        SerializedObject serializedAsset = new SerializedObject(pipelineAsset);
        SerializedProperty rendererList = serializedAsset.FindProperty("m_RendererDataList");
        SerializedProperty defaultRendererIndex = serializedAsset.FindProperty("m_DefaultRendererIndex");

        if (rendererList == null || defaultRendererIndex == null)
        {
            report.Warnings.Add("Could not inspect URP renderer list fields on the current render pipeline asset.");
            report.Log();
            return;
        }

        report.Info.Add($"Renderer List Size: {rendererList.arraySize}");
        report.Info.Add($"Default Renderer Index: {defaultRendererIndex.intValue}");

        if (rendererList.arraySize == 0)
        {
            report.Errors.Add("The current render pipeline asset has no renderer data entries.");
        }
        else if (defaultRendererIndex.intValue < 0 || defaultRendererIndex.intValue >= rendererList.arraySize)
        {
            report.Errors.Add("The default renderer index points outside the renderer data list.");
        }
        else
        {
            SerializedProperty defaultRenderer = rendererList.GetArrayElementAtIndex(defaultRendererIndex.intValue);
            var rendererAsset = defaultRenderer.objectReferenceValue;

            if (rendererAsset == null)
            {
                report.Errors.Add("The default renderer entry is null. This matches the repeated 'Default Renderer is missing' console error.");
            }
            else
            {
                report.Info.Add($"Default Renderer Asset: {rendererAsset.name}");
                report.Info.Add($"Default Renderer Path: {AssetDatabase.GetAssetPath(rendererAsset)}");
            }
        }

        report.Log();
    }

    [MenuItem(MenuRoot + "Apply Default URP Asset")]
    public static void ApplyDefaultUrpAsset()
    {
        const string pipelineAssetPath = "Assets/Settings/UniversalRP.asset";
        const string repairedRendererAssetPath = "Assets/Settings/Renderer2D_Repaired.asset";
        var report = new ValidationReport("Apply Default URP Asset");
        RenderPipelineAsset pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelineAssetPath);

        if (pipelineAsset == null)
        {
            report.Errors.Add($"Could not load render pipeline asset at '{pipelineAssetPath}'.");
            report.Log();
            return;
        }

        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;

        SerializedObject serializedAsset = new SerializedObject(pipelineAsset);
        SerializedProperty rendererData = serializedAsset.FindProperty("m_RendererData");
        SerializedProperty rendererList = serializedAsset.FindProperty("m_RendererDataList");
        SerializedProperty defaultRendererIndex = serializedAsset.FindProperty("m_DefaultRendererIndex");

        ScriptableRendererData resolvedRenderer = null;
        if (rendererList != null && defaultRendererIndex != null &&
            rendererList.arraySize > 0 &&
            defaultRendererIndex.intValue >= 0 &&
            defaultRendererIndex.intValue < rendererList.arraySize)
        {
            resolvedRenderer = rendererList.GetArrayElementAtIndex(defaultRendererIndex.intValue).objectReferenceValue as ScriptableRendererData;
        }

        if (resolvedRenderer == null)
        {
            resolvedRenderer = CreateOrRepair2DRendererAsset(repairedRendererAssetPath, report);
        }

        if (resolvedRenderer != null)
        {
            if (rendererList != null)
            {
                if (rendererList.arraySize == 0)
                {
                    rendererList.InsertArrayElementAtIndex(0);
                    if (defaultRendererIndex != null)
                    {
                        defaultRendererIndex.intValue = 0;
                    }
                }

                if (defaultRendererIndex != null &&
                    defaultRendererIndex.intValue >= 0 &&
                    defaultRendererIndex.intValue < rendererList.arraySize)
                {
                    rendererList.GetArrayElementAtIndex(defaultRendererIndex.intValue).objectReferenceValue = resolvedRenderer;
                }
            }

            if (rendererData != null)
            {
                rendererData.objectReferenceValue = resolvedRenderer;
            }

            serializedAsset.ApplyModifiedPropertiesWithoutUndo();
            report.Info.Add($"Assigned default renderer asset: {resolvedRenderer.name}");
        }

        EditorUtility.SetDirty(pipelineAsset);
        AssetDatabase.SaveAssets();

        report.Info.Add($"Assigned GraphicsSettings.defaultRenderPipeline: {pipelineAsset.name}");
        report.Info.Add($"Assigned QualitySettings.renderPipeline: {pipelineAsset.name}");
        report.Log();
    }

    private static ScriptableRendererData CreateOrRepair2DRendererAsset(string assetPath, ValidationReport report)
    {
        ScriptableRendererData existingRenderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(assetPath);
        if (existingRenderer != null)
        {
            report.Info.Add($"Reusing existing repaired renderer asset: {assetPath}");
            return existingRenderer;
        }

        var rendererData = ScriptableObject.CreateInstance<Renderer2DData>();
        AssetDatabase.CreateAsset(rendererData, assetPath);
        ResourceReloader.ReloadAllNullIn(rendererData, UniversalRenderPipelineAsset.packagePath);

        SerializedObject rendererObject = new SerializedObject(rendererData);
        SerializedProperty postProcessData = rendererObject.FindProperty("m_PostProcessData");
        if (postProcessData != null && postProcessData.objectReferenceValue == null)
        {
            postProcessData.objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            rendererObject.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssets();
        report.Info.Add($"Created repaired renderer asset: {assetPath}");
        return rendererData;
    }

    private static void ValidateRequiredComponent(Component component, string label, ValidationReport report)
    {
        if (component == null)
        {
            report.Errors.Add($"{label} is missing from the active scene.");
            return;
        }

        report.Info.Add($"{label}: {GetHierarchyPath(component.transform)}");
    }

    private static void ValidateCombatInputReferences(
        CombatInputHandler combatInput,
        BattleManager battleManager,
        TurnManager turnManager,
        ValidationReport report)
    {
        if (combatInput == null)
        {
            return;
        }

        if (combatInput.battleManager == null)
        {
            report.Errors.Add("CombatInputHandler.battleManager is not assigned.");
        }
        else if (battleManager != null && combatInput.battleManager != battleManager)
        {
            report.Warnings.Add("CombatInputHandler.battleManager points to a different BattleManager instance than the first one found in scene.");
        }

        if (combatInput.turnManager == null)
        {
            report.Errors.Add("CombatInputHandler.turnManager is not assigned.");
        }
        else if (turnManager != null && combatInput.turnManager != turnManager)
        {
            report.Warnings.Add("CombatInputHandler.turnManager points to a different TurnManager instance than the first one found in scene.");
        }

        if (combatInput.GetComponent<GestureRecognizer>() == null)
        {
            GestureRecognizer resolvedGestureRecognizer = combatInput.GetComponentInChildren<GestureRecognizer>(true) ??
                                                        combatInput.GetComponentInParent<GestureRecognizer>();
            if (resolvedGestureRecognizer == null)
            {
                report.Warnings.Add("CombatInputHandler cannot resolve a GestureRecognizer on itself, its children, or its parents.");
            }
        }

        if (combatInput.GetComponent<InputManager>() == null)
        {
            InputManager resolvedInputManager = combatInput.GetComponentInChildren<InputManager>(true) ??
                                               combatInput.GetComponentInParent<InputManager>();
            if (resolvedInputManager == null)
            {
                report.Warnings.Add("CombatInputHandler cannot resolve an InputManager on itself, its children, or its parents.");
            }
        }
    }

    private static void ValidateGestureRecognizerBindings(GestureRecognizer gestureRecognizer, ValidationReport report)
    {
        if (gestureRecognizer == null)
        {
            return;
        }

        UnityEngine.Object tapAction = GetSerializedReference(gestureRecognizer, "tapAction");
        UnityEngine.Object positionAction = GetSerializedReference(gestureRecognizer, "positionAction");
        UnityEngine.Object holdAction = GetSerializedReference(gestureRecognizer, "holdAction");

        if (tapAction == null)
        {
            report.Errors.Add("GestureRecognizer.tapAction is not assigned.");
        }

        if (positionAction == null)
        {
            report.Errors.Add("GestureRecognizer.positionAction is not assigned.");
        }

        if (holdAction == null)
        {
            report.Warnings.Add("GestureRecognizer.holdAction is not assigned.");
        }
    }

    private static void ValidateDragHandlers(BattleDragHandler[] dragHandlers, TurnManager turnManager, ValidationReport report)
    {
        if (dragHandlers.Length == 0)
        {
            report.Warnings.Add("No BattleDragHandler was found in the active scene.");
            return;
        }

        if (dragHandlers.Length > 1)
        {
            report.Errors.Add($"Found {dragHandlers.Length} BattleDragHandler instances. Expected 0 or 1.");
        }

        foreach (BattleDragHandler dragHandler in dragHandlers)
        {
            report.Info.Add($"BattleDragHandler: {GetHierarchyPath(dragHandler.transform)}");

            if (dragHandler.turnManager == null)
            {
                report.Errors.Add($"BattleDragHandler '{GetHierarchyPath(dragHandler.transform)}' does not have TurnManager assigned.");
            }
            else if (turnManager != null && dragHandler.turnManager != turnManager)
            {
                report.Warnings.Add($"BattleDragHandler '{GetHierarchyPath(dragHandler.transform)}' points to a different TurnManager instance than the first one found in scene.");
            }

            if (dragHandler.GetComponent<Image>() == null)
            {
                report.Errors.Add($"BattleDragHandler '{GetHierarchyPath(dragHandler.transform)}' is missing its required Image component.");
            }
        }
    }

    private static void ValidateBattleManagerPrefab(BattleManager battleManager, ValidationReport report)
    {
        if (battleManager == null)
        {
            return;
        }

        if (battleManager.characterPrefab == null)
        {
            report.Errors.Add("BattleManager.characterPrefab is not assigned.");
            return;
        }

        GameObject prefabReference = battleManager.characterPrefab;
        report.Info.Add($"BattleManager.characterPrefab: {prefabReference.name}");

        if (prefabReference.GetComponent<CharacterInstance>() == null)
        {
            report.Errors.Add("BattleManager.characterPrefab is missing CharacterInstance.");
        }

        if (prefabReference.GetComponent<SpriteRenderer>() == null)
        {
            report.Errors.Add("BattleManager.characterPrefab is missing SpriteRenderer.");
        }

        if (prefabReference.GetComponent<Animator>() == null)
        {
            report.Errors.Add("BattleManager.characterPrefab is missing Animator.");
        }

        if (prefabReference.GetComponent<CapsuleCollider2D>() == null)
        {
            report.Warnings.Add("BattleManager.characterPrefab is missing CapsuleCollider2D.");
        }

        CharacterInstance characterInstance = prefabReference.GetComponent<CharacterInstance>();
        if (characterInstance != null && characterInstance.Definition == null)
        {
            report.Info.Add("BattleManager.characterPrefab CharacterInstance.Definition is null. This is expected if the prefab is used as a generic runtime shell.");
        }

        Animator animator = prefabReference.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            report.Info.Add("BattleManager.characterPrefab Animator has no controller assigned. This is expected if the controller is injected from CharacterDefinition at runtime.");
        }
    }

    private static void ValidateTestBattleSetup(
        TestBattleSetup testBattleSetup,
        BattleManager battleManager,
        TurnManager turnManager,
        ValidationReport report)
    {
        if (testBattleSetup == null)
        {
            report.Info.Add("TestBattleSetup is not present in the active scene.");
            return;
        }

        report.Info.Add($"TestBattleSetup: {GetHierarchyPath(testBattleSetup.transform)}");

        if (testBattleSetup.battleManager == null)
        {
            report.Warnings.Add("TestBattleSetup.battleManager is not assigned.");
        }
        else if (battleManager != null && testBattleSetup.battleManager != battleManager)
        {
            report.Warnings.Add("TestBattleSetup.battleManager points to a different BattleManager instance than the first one found in scene.");
        }

        if (testBattleSetup.turnManager == null)
        {
            report.Warnings.Add("TestBattleSetup.turnManager is not assigned.");
        }
        else if (turnManager != null && testBattleSetup.turnManager != turnManager)
        {
            report.Warnings.Add("TestBattleSetup.turnManager points to a different TurnManager instance than the first one found in scene.");
        }

        if (testBattleSetup.testTeam == null || testBattleSetup.testTeam.Count == 0)
        {
            report.Warnings.Add("TestBattleSetup.testTeam is empty.");
        }
        else
        {
            report.Info.Add($"TestBattleSetup.testTeam Count: {testBattleSetup.testTeam.Count}");
        }
    }

    private static void AppendCharacterList(StringBuilder builder, string heading, IEnumerable<CharacterInstance> characters)
    {
        List<CharacterInstance> materialized = characters?.Where(character => character != null).ToList() ?? new List<CharacterInstance>();
        builder.AppendLine($"{heading}: {materialized.Count}");

        foreach (CharacterInstance character in materialized)
        {
            builder.AppendLine($"- {DescribeCharacter(character)}");
        }
    }

    private static string DescribeCharacter(CharacterInstance character)
    {
        if (character == null)
        {
            return "null";
        }

        string definitionName = character.Definition != null ? character.Definition.DisplayNameEN : "<no definition>";
        return $"{character.name} [{definitionName}] alive={character.IsAlive} hp={character.CurrentHealth:0.##} pos={character.Position}";
    }

    private static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return "<null>";
        }

        var segments = new Stack<string>();
        Transform current = transform;

        while (current != null)
        {
            segments.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", segments);
    }

    private static string GetComponentName(Component component)
    {
        return component == null ? "<missing component>" : component.GetType().Name;
    }

    private static UnityEngine.Object GetSerializedReference(UnityEngine.Object target, string propertyName)
    {
        var serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property?.objectReferenceValue;
    }

    private static T FindSingleInScene<T>(Scene scene) where T : Component
    {
        return FindInScene<T>(scene).FirstOrDefault();
    }

    private static T[] FindInScene<T>(Scene scene) where T : Component
    {
        return Object
            .FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(component => component != null && component.gameObject.scene == scene)
            .ToArray();
    }

    private sealed class ValidationReport
    {
        public readonly List<string> Info = new();
        public readonly List<string> Warnings = new();
        public readonly List<string> Errors = new();

        private readonly string _title;

        public ValidationReport(string title)
        {
            _title = title;
        }

        public void Log()
        {
            var builder = new StringBuilder();
            string status = Errors.Count > 0 ? "FAIL" : Warnings.Count > 0 ? "WARN" : "PASS";

            builder.AppendLine($"{LogPrefix} {_title} [{status}]");

            if (Info.Count > 0)
            {
                builder.AppendLine("Info:");
                foreach (string line in Info)
                {
                    builder.AppendLine($"- {line}");
                }
            }

            if (Warnings.Count > 0)
            {
                builder.AppendLine("Warnings:");
                foreach (string line in Warnings)
                {
                    builder.AppendLine($"- {line}");
                }
            }

            if (Errors.Count > 0)
            {
                builder.AppendLine("Errors:");
                foreach (string line in Errors)
                {
                    builder.AppendLine($"- {line}");
                }
            }

            if (Errors.Count > 0)
            {
                Debug.LogError(builder.ToString());
            }
            else if (Warnings.Count > 0)
            {
                Debug.LogWarning(builder.ToString());
            }
            else
            {
                Debug.Log(builder.ToString());
            }
        }
    }
}
#endif
