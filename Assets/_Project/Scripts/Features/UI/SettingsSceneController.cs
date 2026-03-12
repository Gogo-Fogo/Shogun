using System;
using System.Collections.Generic;
using Shogun.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shogun.Features.UI
{
    [ExecuteAlways]
    [DefaultExecutionOrder(20)]
    public sealed class SettingsSceneController : MonoBehaviour
    {
        private const string SettingsSceneName = "Settings";
        private const string MainMenuSceneName = "MainMenu";
        private const string DedicatedCanvasName = "SettingsCanvas";
        private const bool PreferMinimalSettingsLayout = true;
        private const float ContentHorizontalMargin = 28f;
        private const float PhoneContentMaxWidth = 940f;
        private const float TabletContentMaxWidth = 1140f;
        private const float ExpandedContentMaxWidth = 1280f;
        private const float TabletBreakpoint = 1280f;
        private const float ExpandedBreakpoint = 1680f;

        private static readonly Color BackgroundColor = new Color(0.05f, 0.05f, 0.06f, 1f);
        private static readonly Color PanelOuterColor = new Color(0.2f, 0.17f, 0.11f, 0.96f);
        private static readonly Color PanelInnerColor = new Color(0.09f, 0.09f, 0.1f, 0.97f);
        private static readonly Color HeaderOuterColor = new Color(0.22f, 0.18f, 0.12f, 0.98f);
        private static readonly Color HeaderInnerColor = new Color(0.09f, 0.09f, 0.1f, 0.98f);
        private static readonly Color HeadingColor = new Color(0.98f, 0.94f, 0.84f, 1f);
        private static readonly Color BodyColor = new Color(0.84f, 0.81f, 0.75f, 1f);
        private static readonly Color MutedColor = new Color(0.69f, 0.67f, 0.62f, 1f);
        private static readonly Color SoftLineColor = new Color(0.81f, 0.68f, 0.34f, 0.34f);
        private static readonly Color PrimaryActionColor = new Color(0.19f, 0.33f, 0.45f, 0.98f);
        private static readonly Color SecondaryActionColor = new Color(0.43f, 0.29f, 0.12f, 0.98f);
        private static readonly Color NeutralActionColor = new Color(0.18f, 0.16f, 0.15f, 0.98f);
        private static readonly Color SelectedCardColor = new Color(0.18f, 0.24f, 0.3f, 0.98f);
        private static readonly Color IdleCardColor = new Color(0.12f, 0.11f, 0.11f, 0.96f);
        private static readonly Color SelectedAccentColor = new Color(0.56f, 0.82f, 0.94f, 0.95f);
        private static readonly Color IdleAccentColor = new Color(0.66f, 0.56f, 0.34f, 0.78f);
        private static Sprite s_WhiteSprite;
        private static Font s_RuntimeFont;

        private Canvas targetCanvas;
        private RectTransform hostRoot;
        private RectTransform contentFrame;
        private GridLayoutGroup performanceGrid;
        private GridLayoutGroup feedbackGrid;
        private Text statusFrameRateLabel;
        private Text statusVolumeLabel;
        private Text statusFeedbackLabel;
        private Text volumeValueLabel;
        private Text volumeSummaryLabel;
        private RectTransform volumeFillRect;
        private readonly List<FrameRateCardView> frameRateCards = new List<FrameRateCardView>();
        private readonly List<ToggleCardView> toggleCards = new List<ToggleCardView>();
        private Vector2Int lastScreenSize = new Vector2Int(-1, -1);
        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private float lastParentWidth = -1f;
        private bool usingEmergencyLayout;
        private string diagnosticStatus = "Not started";

        private sealed class FrameRateCardView
        {
            public FrameRateMode Mode;
            public Image Background;
            public Image Accent;
            public Text StatusLabel;
        }

        private sealed class ToggleCardView
        {
            public string Key;
            public Image Background;
            public Image Accent;
            public Text StatusLabel;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllerExists()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.name.Equals(SettingsSceneName, StringComparison.OrdinalIgnoreCase))
                return;
            if (FindFirstObjectByType<SettingsSceneController>() != null)
                return;
            new GameObject("SettingsSceneController").AddComponent<SettingsSceneController>();
        }

        private void OnEnable()
        {
            GameSettingsService.SettingsChanged += RefreshFromSettings;
            if (!Application.isPlaying)
                RebuildEditorPreview();
        }
        private void OnDisable() => GameSettingsService.SettingsChanged -= RefreshFromSettings;

        private void Start()
        {
            if (!SceneManager.GetActiveScene().name.Equals(SettingsSceneName, StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                return;
            }
            BuildSceneContents();
        }
        private void Update() => ApplyResponsiveLayout(false);

        private void RebuildEditorPreview()
        {
            if (!isActiveAndEnabled)
                return;
            if (!SceneManager.GetActiveScene().name.Equals(SettingsSceneName, StringComparison.OrdinalIgnoreCase))
                return;
            BuildSceneContents();
        }
        private void BuildSceneContents()
        {
            try
            {
                diagnosticStatus = "ResolveCanvas...";
                ResolveCanvas();
                if (targetCanvas == null)
                {
                    diagnosticStatus = "FAIL: No canvas after ResolveCanvas.";
                    Debug.LogError("[SettingsSceneController] FAIL: No canvas found or created.");
                    return;
                }
                diagnosticStatus = "ResolveCanvas complete. Building layout...";
                if (PreferMinimalSettingsLayout)
                {
                    BuildEmergencyFallback(null);
                    RefreshFromSettings();
                    ApplyResponsiveLayout(true);
                    diagnosticStatus = "Minimal layout built.";
                    return;
                }
                BuildScreen();
                RefreshFromSettings();
                ApplyResponsiveLayout(true);
                diagnosticStatus = "Full layout built.";
            }
            catch (Exception exception)
            {
                diagnosticStatus = $"EXCEPTION: {exception.GetType().Name}: {exception.Message}";
                Debug.LogError($"[SettingsSceneController] BuildSceneContents failed: {exception}");
                try
                {
                    BuildEmergencyFallback(exception);
                    RefreshFromSettings();
                    ApplyResponsiveLayout(true);
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError($"[SettingsSceneController] Emergency fallback also failed: {fallbackEx}");
                }
            }
        }

        private void ResolveCanvas()
        {
            targetCanvas = FindDedicatedCanvas();
            if (targetCanvas == null)
            {
                Debug.LogWarning("[SettingsSceneController] Dedicated settings canvas was not found. Creating one now.");
                targetCanvas = CreateFallbackCanvas();
            }

            RepairCanvasIfNeeded(targetCanvas);

            Transform safeAreaPanel = targetCanvas.transform.Find("UI_SafeAreaPanel");
            if (safeAreaPanel == null)
            {
                safeAreaPanel = CreateRect("UI_SafeAreaPanel", targetCanvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                safeAreaPanel.gameObject.AddComponent<SafeAreaHandler>();
                CreateRect("HUD", safeAreaPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                CreateRect("Menu_Main", safeAreaPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                CreateRect("Popups", safeAreaPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            RectTransform safeAreaRect = safeAreaPanel as RectTransform;
            if (safeAreaRect != null)
                StretchRectTransform(safeAreaRect);

            Transform menuRoot = safeAreaPanel.Find("Menu_Main");
            if (menuRoot == null)
                menuRoot = CreateRect("Menu_Main", safeAreaPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform menuRootRect = menuRoot as RectTransform;
            if (menuRootRect != null)
                StretchRectTransform(menuRootRect);

            hostRoot = menuRootRect ?? safeAreaRect;
        }

        private static void RepairCanvasIfNeeded(Canvas canvas)
        {
            if (canvas == null)
                return;

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect != null)
            {
                StretchRectTransform(canvasRect);
                canvasRect.localScale = Vector3.one;
                canvasRect.anchoredPosition = Vector2.zero;
                LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRect);
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        private static Canvas FindDedicatedCanvas()
        {
            GameObject existingCanvas = GameObject.Find(DedicatedCanvasName);
            return existingCanvas != null ? existingCanvas.GetComponent<Canvas>() : null;
        }
        private void BuildScreen()
        {
            ClearChildren(hostRoot);
            RectTransform screenRoot = CreateRect("SettingsScreenRoot", hostRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = screenRoot.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = BackgroundColor;

            RectTransform scrollRoot = CreateRect("ScrollRoot", screenRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            RectTransform viewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.sprite = GetWhiteSprite();
            viewportImage.color = new Color(0f, 0f, 0f, 0f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            RectTransform scrollContent = CreateRect("ScrollContent", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            scrollContent.pivot = new Vector2(0.5f, 1f);
            scrollContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport;
            scrollRect.content = scrollContent;

            contentFrame = CreateRect("ContentFrame", scrollContent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            contentFrame.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup layout = contentFrame.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 28, 36);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            contentFrame.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildHeader();
            BuildPerformancePanel();
            BuildAudioPanel();
            BuildFeedbackPanel();
            BuildFooterPanel();
        }
        private void BuildEmergencyFallback(Exception exception)
        {
            frameRateCards.Clear();
            toggleCards.Clear();
            performanceGrid = null;
            feedbackGrid = null;
            usingEmergencyLayout = true;

            ClearChildren(hostRoot);
            RectTransform screen = CreateRect("EmergencySettingsScreen", hostRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = screen.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = BackgroundColor;

            RectTransform panel = CreateRect("EmergencyPanel", screen, Vector2.zero, Vector2.one, new Vector2(24f, 108f), new Vector2(-24f, -96f));
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = GetWhiteSprite();
            panelImage.color = new Color(0.12f, 0.11f, 0.11f, 0.97f);
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
            outline.effectDistance = new Vector2(3f, -3f);

            contentFrame = CreateRect("EmergencyContent", panel, Vector2.zero, Vector2.one, new Vector2(28f, 28f), new Vector2(-28f, -28f));
            VerticalLayoutGroup stack = contentFrame.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 14f;
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            contentFrame.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildEmergencyHeader(exception);
            BuildEmergencyPerformancePanel();
            BuildEmergencyAudioPanel();
            BuildEmergencyFeedbackPanel();
            BuildEmergencyFooterPanel();
        }

        private void BuildEmergencyHeader(Exception exception)
        {
            RectTransform root = CreateRect("EmergencyHeader", contentFrame, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = exception == null ? 232f : 296f;

            Text title = CreateText("Title", root, TextAnchor.UpperCenter, 44, FontStyle.Bold);
            title.text = "SETTINGS";
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 154f);

            Text subtitle = CreateText("Subtitle", root, TextAnchor.MiddleCenter, 18, FontStyle.Bold);
            subtitle.text = "Stable local settings menu while the richer settings layout is under repair.";
            subtitle.color = BodyColor;
            subtitle.rectTransform.offsetMin = new Vector2(0f, 88f);
            subtitle.rectTransform.offsetMax = new Vector2(0f, -92f);

            statusFrameRateLabel = CreatePillLabel(root, "FpsPill", new Vector2(0.04f, 0.42f), new Vector2(0.96f, 0.62f), new Color(0.16f, 0.27f, 0.37f, 0.98f), 18);
            statusVolumeLabel = CreatePillLabel(root, "VolumePill", new Vector2(0.04f, 0.2f), new Vector2(0.96f, 0.4f), new Color(0.25f, 0.19f, 0.11f, 0.98f), 18);
            statusFeedbackLabel = CreatePillLabel(root, "FeedbackPill", new Vector2(0.04f, 0f), new Vector2(0.96f, 0.18f), new Color(0.16f, 0.15f, 0.14f, 0.98f), 14);

            if (exception == null)
                return;

            RectTransform errorRoot = CreateRect("ErrorRoot", root, new Vector2(0.04f, 0f), new Vector2(0.96f, 0.32f), Vector2.zero, Vector2.zero);
            Image errorBackground = errorRoot.gameObject.AddComponent<Image>();
            errorBackground.sprite = GetWhiteSprite();
            errorBackground.color = new Color(0.18f, 0.08f, 0.08f, 0.9f);
            Text errorLabel = CreateText("ErrorLabel", errorRoot, TextAnchor.UpperLeft, 12, FontStyle.Normal);
            errorLabel.text = $"Fallback active because the rich settings layout failed:`n{exception.GetType().Name}: {exception.Message}";
            errorLabel.color = new Color(1f, 0.86f, 0.82f, 1f);
            errorLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            errorLabel.verticalOverflow = VerticalWrapMode.Overflow;
            errorLabel.rectTransform.offsetMin = new Vector2(14f, 12f);
            errorLabel.rectTransform.offsetMax = new Vector2(-14f, -12f);
        }

        private void BuildEmergencyPerformancePanel()
        {
            CreatePanel(contentFrame, "EmergencyPerformancePanel", 182f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("Content", inner, Vector2.zero, Vector2.one, new Vector2(24f, 20f), new Vector2(-24f, -20f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 10f;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            Text heading = CreateText("Heading", content, TextAnchor.UpperLeft, 26, FontStyle.Bold);
            heading.text = "FRAME RATE";
            heading.color = HeadingColor;
            heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;

            Text summary = CreateText("Summary", content, TextAnchor.UpperLeft, 14, FontStyle.Normal);
            summary.text = "Apply the active frame-rate preset immediately.";
            summary.color = BodyColor;
            summary.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;

            RectTransform buttons = CreateRect("Buttons", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            buttons.gameObject.AddComponent<LayoutElement>().preferredHeight = 52f;
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 10f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = false;
            CreateActionButton(buttons, "Mode30", "30 FPS", SecondaryActionColor, () => SetFrameRate(FrameRateMode.BatterySaver));
            CreateActionButton(buttons, "Mode60", "60 FPS", PrimaryActionColor, () => SetFrameRate(FrameRateMode.Balanced));
            CreateActionButton(buttons, "Mode120", "120 FPS", NeutralActionColor, () => SetFrameRate(FrameRateMode.HighRefresh));
        }

        private void BuildEmergencyAudioPanel()
        {
            CreatePanel(contentFrame, "EmergencyAudioPanel", 220f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("Content", inner, Vector2.zero, Vector2.one, new Vector2(24f, 20f), new Vector2(-24f, -20f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 10f;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            volumeValueLabel = CreateText("VolumeValue", content, TextAnchor.UpperLeft, 24, FontStyle.Bold);
            volumeValueLabel.color = HeadingColor;
            volumeValueLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 36f;

            volumeSummaryLabel = CreateText("VolumeSummary", content, TextAnchor.UpperLeft, 14, FontStyle.Normal);
            volumeSummaryLabel.color = BodyColor;
            volumeSummaryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            volumeSummaryLabel.verticalOverflow = VerticalWrapMode.Overflow;
            volumeSummaryLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 42f;

            RectTransform bar = CreateRect("VolumeBar", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bar.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;
            Image barBackground = bar.gameObject.AddComponent<Image>();
            barBackground.sprite = GetWhiteSprite();
            barBackground.color = new Color(0.14f, 0.13f, 0.12f, 1f);
            RectTransform fill = CreateRect("Fill", bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            fill.pivot = new Vector2(0f, 0.5f);
            volumeFillRect = fill;
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.sprite = GetWhiteSprite();
            fillImage.color = SelectedAccentColor;

            RectTransform buttons = CreateRect("Buttons", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            buttons.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 10f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = false;
            CreateActionButton(buttons, "VolumeDown", "-10%", NeutralActionColor, () => AdjustMasterVolume(-0.1f));
            CreateActionButton(buttons, "VolumeMute", "MUTE", SecondaryActionColor, () => SetMasterVolume(0f));
            CreateActionButton(buttons, "VolumeUp", "+10%", PrimaryActionColor, () => AdjustMasterVolume(0.1f));
        }

        private void BuildEmergencyFeedbackPanel()
        {
            CreatePanel(contentFrame, "EmergencyFeedbackPanel", 170f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("Content", inner, Vector2.zero, Vector2.one, new Vector2(24f, 20f), new Vector2(-24f, -20f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 10f;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            Text heading = CreateText("Heading", content, TextAnchor.UpperLeft, 26, FontStyle.Bold);
            heading.text = "FEEDBACK";
            heading.color = HeadingColor;
            heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;

            RectTransform buttons = CreateRect("Buttons", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            buttons.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 10f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = false;
            CreateActionButton(buttons, "VibrationToggle", "TOGGLE VIBRATION", NeutralActionColor, ToggleVibration);
            CreateActionButton(buttons, "ShakeToggle", "TOGGLE SCREEN SHAKE", NeutralActionColor, ToggleScreenShake);
        }

        private void BuildEmergencyFooterPanel()
        {
            CreatePanel(contentFrame, "EmergencyFooterPanel", 112f, new Color(0.16f, 0.13f, 0.11f, 0.96f), new Color(0.08f, 0.08f, 0.09f, 0.98f), out RectTransform inner);
            RectTransform buttons = CreateRect("Buttons", inner, Vector2.zero, Vector2.one, new Vector2(24f, 20f), new Vector2(-24f, -20f));
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 12f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = false;
            CreateActionButton(buttons, "ResetButton", "RESET DEFAULTS", SecondaryActionColor, ResetToDefaults);
            CreateActionButton(buttons, "MainMenuButton", "MAIN MENU", NeutralActionColor, () => LoadSceneIfAvailable(MainMenuSceneName));
        }

        private void BuildHeader()
        {
            CreatePanel(contentFrame, "HeaderPanel", 212f, HeaderOuterColor, HeaderInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("HeaderContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 22f), new Vector2(-24f, -22f));

            RectTransform titleBlock = CreateRect("TitleBlock", content, new Vector2(0f, 0f), new Vector2(0.7f, 1f), Vector2.zero, Vector2.zero);
            Text eyebrow = CreateText("Eyebrow", titleBlock, TextAnchor.UpperLeft, 16, FontStyle.Bold);
            eyebrow.text = "LOCAL SETTINGS SURFACE";
            eyebrow.color = new Color(0.9f, 0.82f, 0.52f, 1f);
            eyebrow.rectTransform.offsetMin = new Vector2(0f, 154f);
            Text title = CreateText("Title", titleBlock, TextAnchor.MiddleLeft, 46, FontStyle.Bold);
            title.text = "SETTINGS";
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 64f);
            title.rectTransform.offsetMax = new Vector2(0f, -30f);
            Text subtitle = CreateText("Subtitle", titleBlock, TextAnchor.LowerLeft, 17, FontStyle.Normal);
            subtitle.text = "Frame-rate presets and master volume apply immediately. Feedback toggles are saved locally now and ready for later gameplay consumers.";
            subtitle.color = BodyColor;
            subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            subtitle.verticalOverflow = VerticalWrapMode.Overflow;
            subtitle.rectTransform.offsetMax = new Vector2(0f, -110f);

            RectTransform statusBlock = CreateRect("StatusBlock", content, new Vector2(0.72f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            statusFrameRateLabel = CreatePillLabel(statusBlock, "FpsPill", new Vector2(0.06f, 0.62f), new Vector2(1f, 1f), new Color(0.16f, 0.27f, 0.37f, 0.98f), 18);
            statusVolumeLabel = CreatePillLabel(statusBlock, "VolumePill", new Vector2(0.06f, 0.31f), new Vector2(1f, 0.62f), new Color(0.25f, 0.19f, 0.11f, 0.98f), 18);
            statusFeedbackLabel = CreatePillLabel(statusBlock, "FeedbackPill", new Vector2(0.06f, 0f), new Vector2(1f, 0.31f), new Color(0.16f, 0.15f, 0.14f, 0.98f), 14);
        }

        private void BuildPerformancePanel()
        {
            CreatePanel(contentFrame, "PerformancePanel", 278f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("PerformanceContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childForceExpandWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "PERFORMANCE", "Choose the default render refresh target for the whole app. Balanced stays the primary intended look for the mobile slice.");
            CreateDivider(content);
            RectTransform gridRoot = CreateRect("PerformanceGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            gridRoot.gameObject.AddComponent<LayoutElement>().preferredHeight = 144f;
            performanceGrid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            performanceGrid.spacing = new Vector2(14f, 14f);
            performanceGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            performanceGrid.constraintCount = 1;
            performanceGrid.cellSize = new Vector2(260f, 144f);
            frameRateCards.Clear();
            frameRateCards.Add(CreateFrameRateCard(gridRoot, FrameRateMode.BatterySaver));
            frameRateCards.Add(CreateFrameRateCard(gridRoot, FrameRateMode.Balanced));
            frameRateCards.Add(CreateFrameRateCard(gridRoot, FrameRateMode.HighRefresh));
        }

        private void BuildAudioPanel()
        {
            CreatePanel(contentFrame, "AudioPanel", 244f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("AudioContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childForceExpandWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "AUDIO", "Use one local master-volume lane for now. The project does not yet have separated music and SFX buses.");
            CreateDivider(content);
            RectTransform readout = CreateRect("Readout", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            readout.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;
            volumeValueLabel = CreateText("VolumeValue", readout, TextAnchor.UpperLeft, 28, FontStyle.Bold);
            volumeValueLabel.color = HeadingColor;
            volumeValueLabel.rectTransform.offsetMin = new Vector2(0f, 18f);
            volumeSummaryLabel = CreateText("VolumeSummary", readout, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            volumeSummaryLabel.color = MutedColor;
            volumeSummaryLabel.rectTransform.offsetMax = new Vector2(0f, -34f);

            RectTransform track = CreateRect("VolumeTrack", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            track.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.sprite = GetWhiteSprite();
            trackImage.color = new Color(0.14f, 0.15f, 0.17f, 1f);
            volumeFillRect = CreateRect("Fill", track, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            Image fillImage = volumeFillRect.gameObject.AddComponent<Image>();
            fillImage.sprite = GetWhiteSprite();
            fillImage.color = new Color(0.56f, 0.82f, 0.94f, 0.92f);

            RectTransform buttons = CreateRect("VolumeButtons", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            buttons.gameObject.AddComponent<LayoutElement>().preferredHeight = 50f;
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 10f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childForceExpandWidth = true;
            CreateActionButton(buttons, "MuteButton", "MUTE", NeutralActionColor, () => SetMasterVolume(0f));
            CreateActionButton(buttons, "DownButton", "-10", PrimaryActionColor, () => AdjustMasterVolume(-0.1f));
            CreateActionButton(buttons, "UpButton", "+10", PrimaryActionColor, () => AdjustMasterVolume(0.1f));
            CreateActionButton(buttons, "FullButton", "FULL", SecondaryActionColor, () => SetMasterVolume(1f));
        }

        private void BuildFeedbackPanel()
        {
            CreatePanel(contentFrame, "FeedbackPanel", 274f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("FeedbackContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childForceExpandWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "FEEDBACK", "These preferences are saved now so later battle and UI systems can consume them without inventing a second settings lane.");
            CreateDivider(content);
            RectTransform gridRoot = CreateRect("FeedbackGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            gridRoot.gameObject.AddComponent<LayoutElement>().preferredHeight = 122f;
            feedbackGrid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            feedbackGrid.spacing = new Vector2(14f, 14f);
            feedbackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            feedbackGrid.constraintCount = 1;
            feedbackGrid.cellSize = new Vector2(320f, 122f);
            toggleCards.Clear();
            toggleCards.Add(CreateToggleCard(gridRoot, "VIBRATION", "Store whether touch feedback should be allowed once haptics are wired into combat and menus.", "vibration", ToggleVibration));
            toggleCards.Add(CreateToggleCard(gridRoot, "SCREEN SHAKE", "Store whether impact-camera shake should be allowed once combat feedback starts reading user preferences.", "screen-shake", ToggleScreenShake));
        }

        private void BuildFooterPanel()
        {
            CreatePanel(contentFrame, "FooterPanel", 156f, new Color(0.16f, 0.13f, 0.11f, 0.96f), new Color(0.08f, 0.08f, 0.09f, 0.98f), out RectTransform inner);
            RectTransform content = CreateRect("FooterContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 18f), new Vector2(-24f, -18f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childForceExpandWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandHeight = false;
            Text note = CreateText("FooterNote", content, TextAnchor.MiddleLeft, 14, FontStyle.Normal);
            note.text = "Settings scene rule: keep the options honest. Apply what the current runtime can actually consume, and persist the rest only when they clearly belong to future gameplay/UI hooks.";
            note.color = BodyColor;
            note.horizontalOverflow = HorizontalWrapMode.Wrap;
            note.verticalOverflow = VerticalWrapMode.Overflow;
            note.gameObject.AddComponent<LayoutElement>().preferredHeight = 68f;
            RectTransform buttons = CreateRect("FooterButtons", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            buttons.gameObject.AddComponent<LayoutElement>().preferredHeight = 46f;
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 12f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childForceExpandWidth = true;
            CreateActionButton(buttons, "ResetButton", "RESET DEFAULTS", SecondaryActionColor, ResetToDefaults);
            CreateActionButton(buttons, "MainMenuButton", "MAIN MENU", NeutralActionColor, () => LoadSceneIfAvailable(MainMenuSceneName));
        }
        private void RefreshFromSettings()
        {
            if (contentFrame == null)
                return;

            GameSettingsSnapshot snapshot = GameSettingsService.GetSnapshot();
            statusFrameRateLabel.text = GameSettingsService.GetFrameRateLabel(snapshot.FrameRateMode);
            statusVolumeLabel.text = $"VOL {Mathf.RoundToInt(snapshot.MasterVolume * 100f)}";
            statusFeedbackLabel.text = $"{(snapshot.VibrationEnabled ? "HAPTICS ON" : "HAPTICS OFF")} • {(snapshot.ScreenShakeEnabled ? "SHAKE ON" : "SHAKE OFF")}";
            volumeValueLabel.text = $"MASTER VOLUME {Mathf.RoundToInt(snapshot.MasterVolume * 100f)}%";
            volumeSummaryLabel.text = snapshot.MasterVolume <= 0.001f ? "Audio is muted globally through AudioListener.volume." : "Applies immediately to the current runtime audio output.";
            volumeFillRect.anchorMax = new Vector2(Mathf.Clamp01(snapshot.MasterVolume), 1f);

            foreach (FrameRateCardView card in frameRateCards)
            {
                bool isSelected = card.Mode == snapshot.FrameRateMode;
                card.Background.color = isSelected ? SelectedCardColor : IdleCardColor;
                card.Accent.color = isSelected ? SelectedAccentColor : IdleAccentColor;
                card.StatusLabel.text = isSelected ? "CURRENT" : $"{(int)card.Mode} FPS";
                card.StatusLabel.color = isSelected ? new Color(0.82f, 0.94f, 1f, 1f) : new Color(0.86f, 0.8f, 0.68f, 1f);
            }

            foreach (ToggleCardView card in toggleCards)
            {
                bool enabledSetting = card.Key == "vibration" ? snapshot.VibrationEnabled : snapshot.ScreenShakeEnabled;
                card.Background.color = enabledSetting ? new Color(0.17f, 0.22f, 0.18f, 0.98f) : IdleCardColor;
                card.Accent.color = enabledSetting ? new Color(0.54f, 0.86f, 0.64f, 0.9f) : IdleAccentColor;
                card.StatusLabel.text = enabledSetting ? "ENABLED" : "DISABLED";
                card.StatusLabel.color = enabledSetting ? new Color(0.86f, 0.96f, 0.86f, 1f) : new Color(0.89f, 0.8f, 0.68f, 1f);
            }
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (contentFrame == null)
                return;

            Vector2Int currentScreenSize = GetLayoutSize();
            Rect safeArea = Screen.safeArea;
            float parentWidth = ((RectTransform)contentFrame.parent).rect.width;
            if (!force && currentScreenSize == lastScreenSize && safeArea == lastSafeArea && Mathf.Approximately(parentWidth, lastParentWidth))
                return;

            lastScreenSize = currentScreenSize;
            lastSafeArea = safeArea;
            lastParentWidth = parentWidth;

            if (usingEmergencyLayout)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(hostRoot);
                return;
            }

            float maxWidth = PhoneContentMaxWidth;
            if (currentScreenSize.x >= ExpandedBreakpoint)
                maxWidth = ExpandedContentMaxWidth;
            else if (currentScreenSize.x >= TabletBreakpoint)
                maxWidth = TabletContentMaxWidth;

            float contentWidth = Mathf.Max(320f, Mathf.Min(maxWidth, parentWidth - (ContentHorizontalMargin * 2f)));
            contentFrame.sizeDelta = new Vector2(contentWidth, 0f);
            if (performanceGrid == null || feedbackGrid == null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
                return;
            }
            performanceGrid.constraintCount = contentWidth >= 980f ? 3 : 1;
            feedbackGrid.constraintCount = contentWidth >= 860f ? 2 : 1;
            float sectionWidth = Mathf.Max(280f, contentWidth - 56f);
            ApplyGridSizing(performanceGrid, sectionWidth, 240f, 144f, frameRateCards.Count);
            ApplyGridSizing(feedbackGrid, sectionWidth, 260f, 122f, toggleCards.Count);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
        }

        private Vector2Int GetLayoutSize()
        {
            RectTransform canvasRect = targetCanvas != null ? targetCanvas.transform as RectTransform : null;
            if (canvasRect != null)
            {
                int width = Mathf.RoundToInt(canvasRect.rect.width);
                int height = Mathf.RoundToInt(canvasRect.rect.height);
                if (width > 0 && height > 0)
                    return new Vector2Int(width, height);
            }

            return new Vector2Int(Screen.width, Screen.height);
        }

        private static void ApplyGridSizing(GridLayoutGroup grid, float availableWidth, float minCellWidth, float cellHeight, int itemCount)
        {
            if (grid == null)
                return;

            int columns = Mathf.Max(1, grid.constraintCount);
            float spacingX = grid.spacing.x;
            float cellWidth = Mathf.Floor((availableWidth - (spacingX * (columns - 1))) / columns);
            grid.cellSize = new Vector2(Mathf.Max(minCellWidth, cellWidth), cellHeight);
            LayoutElement layout = grid.GetComponent<LayoutElement>();
            if (layout != null)
            {
                int rows = Mathf.CeilToInt(itemCount / (float)columns);
                layout.preferredHeight = (rows * cellHeight) + (Mathf.Max(0, rows - 1) * grid.spacing.y);
            }
        }

        private FrameRateCardView CreateFrameRateCard(Transform parent, FrameRateMode mode)
        {
            RectTransform root = CreateCardShell(parent, $"FrameRate_{mode}", out Image background, out Image accent, out Text statusLabel, () => SetFrameRate(mode));
            Text title = CreateText("Title", root, TextAnchor.UpperLeft, 22, FontStyle.Bold);
            title.text = GameSettingsService.GetFrameRateLabel(mode);
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(20f, 42f);
            title.rectTransform.offsetMax = new Vector2(-18f, -80f);
            Text summary = CreateText("Summary", root, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            summary.text = GameSettingsService.GetFrameRateSummary(mode);
            summary.color = BodyColor;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            summary.verticalOverflow = VerticalWrapMode.Overflow;
            summary.rectTransform.offsetMin = new Vector2(20f, 14f);
            summary.rectTransform.offsetMax = new Vector2(-18f, -42f);
            return new FrameRateCardView { Mode = mode, Background = background, Accent = accent, StatusLabel = statusLabel };
        }

        private ToggleCardView CreateToggleCard(Transform parent, string titleText, string summaryText, string key, Action onClick)
        {
            RectTransform root = CreateCardShell(parent, $"Toggle_{key}", out Image background, out Image accent, out Text statusLabel, onClick);
            Text title = CreateText("Title", root, TextAnchor.UpperLeft, 22, FontStyle.Bold);
            title.text = titleText;
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(20f, 36f);
            title.rectTransform.offsetMax = new Vector2(-18f, -68f);
            Text summary = CreateText("Summary", root, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            summary.text = summaryText;
            summary.color = BodyColor;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            summary.verticalOverflow = VerticalWrapMode.Overflow;
            summary.rectTransform.offsetMin = new Vector2(20f, 14f);
            summary.rectTransform.offsetMax = new Vector2(-18f, -40f);
            return new ToggleCardView { Key = key, Background = background, Accent = accent, StatusLabel = statusLabel };
        }

        private static RectTransform CreateCardShell(Transform parent, string name, out Image background, out Image accent, out Text statusLabel, Action onClick)
        {
            RectTransform root = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = IdleCardColor;
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => onClick());
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.38f);
            accent = CreateRect("Accent", root, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(8f, 0f)).gameObject.AddComponent<Image>();
            accent.sprite = GetWhiteSprite();
            accent.color = IdleAccentColor;
            RectTransform pill = CreateRect("StatusPill", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-126f, -30f), new Vector2(-12f, -8f));
            Image pillImage = pill.gameObject.AddComponent<Image>();
            pillImage.sprite = GetWhiteSprite();
            pillImage.color = new Color(0.15f, 0.14f, 0.14f, 1f);
            statusLabel = CreateText("StatusLabel", pill, TextAnchor.MiddleCenter, 12, FontStyle.Bold);
            return root;
        }

        private void SetFrameRate(FrameRateMode mode) => GameSettingsService.SetFrameRateMode(mode);
        private void AdjustMasterVolume(float delta) => SetMasterVolume(GameSettingsService.GetSnapshot().MasterVolume + delta);
        private void SetMasterVolume(float volume) => GameSettingsService.SetMasterVolume(volume);
        private void ToggleVibration() => GameSettingsService.SetVibrationEnabled(!GameSettingsService.GetSnapshot().VibrationEnabled);
        private void ToggleScreenShake() => GameSettingsService.SetScreenShakeEnabled(!GameSettingsService.GetSnapshot().ScreenShakeEnabled);
        private void ResetToDefaults() => GameSettingsService.ResetToDefaults();
        private static Text CreatePillLabel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color, int fontSize)
        {
            RectTransform pill = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = pill.gameObject.AddComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.color = color;
            Outline outline = pill.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(1f, -1f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.32f);
            Text label = CreateText("Label", pill, TextAnchor.MiddleCenter, fontSize, FontStyle.Bold);
            label.color = HeadingColor;
            return label;
        }

        private static RectTransform CreateSectionHeader(Transform parent, string titleText, string subtitleText)
        {
            RectTransform root = CreateRect($"Header_{titleText}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 68f;
            Text title = CreateText("Title", root, TextAnchor.UpperLeft, 28, FontStyle.Bold);
            title.text = titleText;
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 18f);
            Text subtitle = CreateText("Subtitle", root, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            subtitle.text = subtitleText;
            subtitle.color = BodyColor;
            subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            subtitle.verticalOverflow = VerticalWrapMode.Overflow;
            subtitle.rectTransform.offsetMax = new Vector2(0f, -36f);
            return root;
        }

        private static RectTransform CreateDivider(Transform parent)
        {
            RectTransform divider = CreateRect("Divider", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            divider.gameObject.AddComponent<LayoutElement>().preferredHeight = 2f;
            Image image = divider.gameObject.AddComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.color = SoftLineColor;
            return divider;
        }

        private static Button CreateActionButton(Transform parent, string name, string labelText, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform root = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 46f;
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = backgroundColor;
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(onClick);
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.36f);
            Text label = CreateText("Label", root, TextAnchor.MiddleCenter, 15, FontStyle.Bold);
            label.text = labelText;
            label.color = HeadingColor;
            return button;
        }

        private static RectTransform CreatePanel(Transform parent, string name, float preferredHeight, Color outerColor, Color innerColor, out RectTransform inner)
        {
            RectTransform outer = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            outer.gameObject.AddComponent<LayoutElement>().preferredHeight = preferredHeight;
            Image outerImage = outer.gameObject.AddComponent<Image>();
            outerImage.sprite = GetWhiteSprite();
            outerImage.color = outerColor;
            Outline outline = outer.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
            outline.effectDistance = new Vector2(2f, -2f);
            inner = CreateRect("Inner", outer, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image innerImage = inner.gameObject.AddComponent<Image>();
            innerImage.sprite = GetWhiteSprite();
            innerImage.color = innerColor;
            return outer;
        }

        private static Text CreateText(string name, Transform parent, TextAnchor alignment, int fontSize, FontStyle style)
        {
            RectTransform rt = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Text text = rt.gameObject.AddComponent<Text>();
            text.font = GetRuntimeFont();
            text.alignment = alignment;
            text.fontStyle = style;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.raycastTarget = false;
            Shadow shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.62f);
            shadow.effectDistance = new Vector2(1.2f, -1.2f);
            return text;
        }

        private static Font GetRuntimeFont()
        {
            if (s_RuntimeFont != null)
                return s_RuntimeFont;
            s_RuntimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (s_RuntimeFont == null)
                s_RuntimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return s_RuntimeFont;
        }

        private static Sprite GetWhiteSprite()
        {
            if (s_WhiteSprite != null)
                return s_WhiteSprite;
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            s_WhiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            return s_WhiteSprite;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        private static void StretchRectTransform(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
                return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                // These scene controllers generate transient UI trees. Remove them synchronously
                // so play-mode rebuilds do not lay out against stale edit-preview children.
                DestroyImmediate(child);
            }
        }

        private static Canvas CreateFallbackCanvas()
        {
            GameObject canvasGo = new GameObject(DedicatedCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            RectTransform canvasRect = (RectTransform)canvasGo.transform;
            StretchRectTransform(canvasRect);
            canvasRect.localScale = Vector3.one;

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;

            RectTransform safeAreaRoot = CreateRect("UI_SafeAreaPanel", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            StretchRectTransform(safeAreaRoot);
            safeAreaRoot.gameObject.AddComponent<SafeAreaHandler>();
            CreateRect("HUD", safeAreaRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateRect("Menu_Main", safeAreaRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateRect("Popups", safeAreaRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return canvas;
        }
        private static void LoadSceneIfAvailable(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            Debug.LogWarning($"[SettingsSceneController] Scene '{sceneName}' is not loadable.");
        }
    }
}
















