// BattleResultController.cs
// Listens to TurnManager.OnBattleEnded and shows a Win/Loss panel with a Retry button.
// Builds its own UGUI panel in code — no scene prefab required.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Shogun.Features.Combat;

public class BattleResultController : MonoBehaviour
{
    [Header("Dependencies (auto-resolved if left empty)")]
    [SerializeField] private TurnManager turnManager;

    private GameObject panel;
    private Text resultLabel;

    void Start()
    {
        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();

        if (turnManager != null)
            turnManager.OnBattleEnded += ShowResult;

        BuildPanel();
    }

    void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnBattleEnded -= ShowResult;
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    public void ShowResult(BattleResult result)
    {
        if (panel == null) return;
        panel.SetActive(true);
        if (resultLabel != null)
            resultLabel.text = result == BattleResult.Win ? "VICTORY!" : "DEFEAT";
        Debug.Log($"[BattleResult] {result}");
    }

    // -----------------------------------------------------------------------
    // Button callbacks
    // -----------------------------------------------------------------------

    private void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnReturn()
    {
        // No separate menu scene yet — just hide the panel so the sandbox is
        // accessible again.
        if (panel != null) panel.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // UI construction
    // -----------------------------------------------------------------------

    private void BuildPanel()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleResultController] No Canvas found in scene.");
            return;
        }

        // Full-screen dark overlay panel
        panel = new GameObject("BattleResultPanel");
        panel.transform.SetParent(canvas.transform, false);

        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0.05f, 0.85f);

        // Result label
        resultLabel = CreateLabel(panel.transform, "ResultLabel",
            anchoredPos: new Vector2(0f, 100f),
            size: new Vector2(600f, 120f),
            text: "RESULT",
            fontSize: 56,
            color: Color.white);

        // Buttons
        CreateButton(panel.transform, "Retry",
            anchoredPos: new Vector2(0f, -30f),
            label: "RETRY",
            onClick: OnRetry);

        CreateButton(panel.transform, "Return",
            anchoredPos: new Vector2(0f, -110f),
            label: "RETURN TO SANDBOX",
            onClick: OnReturn);

        panel.SetActive(false);
    }

    private static Text CreateLabel(Transform parent, string goName,
        Vector2 anchoredPos, Vector2 size, string text, int fontSize, Color color)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;

        return t;
    }

    private static void CreateButton(Transform parent, string goName,
        Vector2 anchoredPos, string label, UnityEngine.Events.UnityAction onClick)
    {
        // Button background
        var go = new GameObject(goName);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(320f, 60f);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        // Button label
        CreateLabel(go.transform, "Label",
            anchoredPos: Vector2.zero,
            size: new Vector2(320f, 60f),
            text: label,
            fontSize: 22,
            color: Color.white);
    }
}
