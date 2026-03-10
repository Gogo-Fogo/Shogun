// BattleResultController.cs
// Listens to TurnManager.OnBattleEnded and shows a win/loss panel with restart and main-menu actions.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Shogun.Features.Combat;

public class BattleResultController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    [Header("Dependencies (auto-resolved if left empty)")]
    [SerializeField] private TurnManager turnManager;

    private GameObject panel;
    private Text resultLabel;
    private Text subtitleLabel;

    private void Start()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (turnManager != null)
            turnManager.OnBattleEnded += ShowResult;

        BuildPanel();
    }

    private void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnBattleEnded -= ShowResult;
    }

    public void ShowResult(BattleResult result)
    {
        if (panel == null)
            return;

        panel.SetActive(true);
        if (resultLabel != null)
            resultLabel.text = result == BattleResult.Win ? "VICTORY" : "DEFEAT";
        if (subtitleLabel != null)
            subtitleLabel.text = result == BattleResult.Win
                ? "The battle is decided. Choose your next step."
                : "Regroup and try the encounter again.";

        Time.timeScale = 1f;
        Debug.Log($"[BattleResult] {result}");
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(MainMenuSceneName))
        {
            SceneManager.LoadScene(MainMenuSceneName);
            return;
        }

        Debug.LogWarning($"[BattleResultController] Scene '{MainMenuSceneName}' is not loadable.");
        if (panel != null)
            panel.SetActive(false);
    }

    private void BuildPanel()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleResultController] No Canvas found in scene.");
            return;
        }

        panel = new GameObject("BattleResultPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image overlay = panel.AddComponent<Image>();
        overlay.color = new Color(0.01f, 0.01f, 0.03f, 0.82f);

        RectTransform card = CreateRect("Card", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240f, -170f), new Vector2(240f, 170f));
        Image cardImage = card.gameObject.AddComponent<Image>();
        cardImage.color = new Color(0.12f, 0.11f, 0.08f, 0.97f);

        Outline cardOutline = card.gameObject.AddComponent<Outline>();
        cardOutline.effectDistance = new Vector2(2f, -2f);
        cardOutline.effectColor = new Color(0f, 0f, 0f, 0.55f);

        VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        resultLabel = CreateLabel(card.transform, "ResultLabel", "RESULT", 54, new Color(0.98f, 0.95f, 0.84f, 1f), 60f);
        subtitleLabel = CreateLabel(card.transform, "SubtitleLabel", "The battle outcome is ready.", 18, new Color(0.82f, 0.79f, 0.72f, 1f), 30f);
        CreateActionButton(card.transform, "RestartButton", "RESTART", new Color(0.42f, 0.3f, 0.16f, 0.96f), OnRestart);
        CreateActionButton(card.transform, "MainMenuButton", "MAIN MENU", new Color(0.32f, 0.18f, 0.16f, 0.96f), OnMainMenu);

        panel.SetActive(false);
    }

    private static Text CreateLabel(Transform parent, string goName, string text, int fontSize, Color color, float preferredHeight)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = preferredHeight;

        Text label = go.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (label.font == null)
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.raycastTarget = false;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.68f);
        shadow.effectDistance = new Vector2(1.4f, -1.4f);
        return label;
    }

    private static Button CreateActionButton(Transform parent, string goName, string label, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform buttonRect = CreateRect(goName, parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        LayoutElement layout = buttonRect.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 58f;

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = backgroundColor;

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(onClick);

        Text labelText = CreateLabel(buttonRect, "Label", label, 22, new Color(0.98f, 0.94f, 0.82f, 1f), 58f);
        labelText.raycastTarget = false;
        return button;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }
}
