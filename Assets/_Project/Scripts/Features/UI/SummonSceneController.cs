using System;
using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shogun.Features.UI
{
    [ExecuteAlways]
    [DefaultExecutionOrder(20)]
    public sealed class SummonSceneController : MonoBehaviour
    {
        private const string SummonSceneName = "Summon";
        private const string MainMenuSceneName = "MainMenu";
        private const string BarracksSceneName = "Barracks";
        private const string DedicatedCanvasName = "SummonCanvas";
        private const bool PreferMinimalSummonLayout = true;
        private const float ContentHorizontalMargin = 28f;
        private const float PhoneContentMaxWidth = 960f;
        private const float TabletContentMaxWidth = 1160f;
        private const float ExpandedContentMaxWidth = 1320f;
        private const float TabletBreakpoint = 1280f;
        private const float ExpandedBreakpoint = 1680f;

        private static readonly Color BackgroundColor = new Color(0.05f, 0.04f, 0.05f, 1f);
        private static readonly Color HeaderOuterColor = new Color(0.24f, 0.18f, 0.11f, 0.98f);
        private static readonly Color HeaderInnerColor = new Color(0.09f, 0.08f, 0.08f, 0.98f);
        private static readonly Color PanelOuterColor = new Color(0.22f, 0.17f, 0.11f, 0.96f);
        private static readonly Color PanelInnerColor = new Color(0.09f, 0.08f, 0.08f, 0.97f);
        private static readonly Color HeadingColor = new Color(0.98f, 0.94f, 0.84f, 1f);
        private static readonly Color BodyColor = new Color(0.84f, 0.8f, 0.74f, 1f);
        private static readonly Color MutedColor = new Color(0.68f, 0.65f, 0.61f, 1f);
        private static readonly Color SoftLineColor = new Color(0.81f, 0.68f, 0.34f, 0.34f);
        private static readonly Color PlaceholderPortraitColor = new Color(0.16f, 0.14f, 0.14f, 1f);
        private static readonly Color ButtonPrimaryColor = new Color(0.46f, 0.3f, 0.12f, 0.98f);
        private static readonly Color ButtonSecondaryColor = new Color(0.18f, 0.27f, 0.34f, 0.98f);
        private static readonly Color ButtonTertiaryColor = new Color(0.25f, 0.17f, 0.18f, 0.98f);
        private static readonly Color ButtonNeutralColor = new Color(0.17f, 0.15f, 0.14f, 0.98f);
        private static readonly Color ErrorColor = new Color(0.9f, 0.42f, 0.4f, 1f);
        private static Sprite s_WhiteSprite;
        private static Font s_RuntimeFont;

        private Canvas targetCanvas;
        private RectTransform hostRoot;
        private RectTransform contentFrame;
        private GridLayoutGroup bannerGrid;
        private GridLayoutGroup featuredGrid;
        private GridLayoutGroup resultGrid;
        private LayoutElement bannerPanelLayout;
        private LayoutElement detailPanelLayout;
        private LayoutElement resultPanelLayout;
        private LayoutElement bannerGridLayout;
        private LayoutElement featuredGridLayout;
        private LayoutElement resultGridLayout;
        private Image detailAccentBand;
        private Text sealsLabel;
        private Text collectionLabel;
        private Text bannerStateLabel;
        private Text bannerTitleLabel;
        private Text bannerSubtitleLabel;
        private Text bannerSummaryLabel;
        private Text bannerOddsLabel;
        private Text bannerDisclosureLabel;
        private Text featuredSummaryLabel;
        private Text resultStatusLabel;
        private Text resultSummaryLabel;
        private GameObject resultEmptyState;
        private RectTransform featuredGridRoot;
        private RectTransform resultGridRoot;
        private ScrollRect emergencyScrollRect;
        private readonly List<BannerButtonView> bannerButtons = new List<BannerButtonView>();
        private readonly List<TestSummonService.TestSummonBanner> banners = new List<TestSummonService.TestSummonBanner>();
        private readonly List<TestSummonService.TestSummonPullResult> lastResults = new List<TestSummonService.TestSummonPullResult>();
        private string selectedBannerId = string.Empty;
        private Vector2Int lastScreenSize = new Vector2Int(-1, -1);
        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private float lastParentWidth = -1f;
        private bool usingEmergencyLayout;
        private string diagnosticStatus = "Not started";

        private sealed class BannerButtonView
        {
            public TestSummonService.TestSummonBanner Banner;
            public Image Background;
            public Image Accent;
            public Text TitleLabel;
            public Text SubtitleLabel;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllerExists()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.name.Equals(SummonSceneName, StringComparison.OrdinalIgnoreCase))
                return;
            if (FindFirstObjectByType<SummonSceneController>() != null)
                return;
            new GameObject("SummonSceneController").AddComponent<SummonSceneController>();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                RebuildEditorPreview();
        }
        private void Start()
        {
            if (!SceneManager.GetActiveScene().name.Equals(SummonSceneName, StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                return;
            }
            BuildSceneContents();
        }
        private void Update() => ApplyResponsiveLayout(false);

        #if UNITY_EDITOR
        private void OnGUI()
        {
            if (!Application.isPlaying)
                return;
            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                normal = { textColor = Color.yellow }
            };
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.85f);
            string info = $"[Summon Diag]\n{diagnosticStatus}\n" +
                          $"Canvas: {(targetCanvas != null ? $"active={targetCanvas.isActiveAndEnabled}, mode={targetCanvas.renderMode}" : "NULL")}\n" +
                          $"Screen: {Screen.width}x{Screen.height}";
            GUI.Box(new Rect(10, 10, 500, 120), info, style);
        }
        #endif

        private void RebuildEditorPreview()
        {
            if (!isActiveAndEnabled)
                return;
            if (!SceneManager.GetActiveScene().name.Equals(SummonSceneName, StringComparison.OrdinalIgnoreCase))
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
                    Debug.LogError("[SummonSceneController] FAIL: No canvas found or created.");
                    return;
                }
                diagnosticStatus = "ResolveCanvas complete.";
                ResolveBanners();
                diagnosticStatus = $"Banners: {banners.Count}. Building layout...";
                if (PreferMinimalSummonLayout)
                {
                    BuildEmergencyFallback(null);
                    SelectBanner(banners.Count > 0 ? banners[0].BannerId : string.Empty);
                    ApplyResponsiveLayout(true);
                    diagnosticStatus = $"Minimal layout built. {banners.Count} banners.";
                    return;
                }
                BuildScreen();
                SelectBanner(banners.Count > 0 ? banners[0].BannerId : string.Empty);
                ApplyResponsiveLayout(true);
                diagnosticStatus = $"Full layout built. {banners.Count} banners.";
            }
            catch (Exception exception)
            {
                diagnosticStatus = $"EXCEPTION: {exception.GetType().Name}: {exception.Message}";
                Debug.LogError($"[SummonSceneController] BuildSceneContents failed: {exception}");
                try
                {
                    BuildEmergencyFallback(exception);
                    SelectBanner(banners.Count > 0 ? banners[0].BannerId : string.Empty);
                    ApplyResponsiveLayout(true);
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError($"[SummonSceneController] Emergency fallback also failed: {fallbackEx}");
                }
            }
        }

        private void ResolveCanvas()
        {
            targetCanvas = FindDedicatedCanvas();
            if (targetCanvas == null)
            {
                Debug.LogWarning("[SummonSceneController] Dedicated summon canvas was not found. Creating one now.");
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
        private void ResolveBanners()
        {
            banners.Clear();
            IReadOnlyList<TestSummonService.TestSummonBanner> source = TestSummonService.GetBanners();
            for (int i = 0; i < source.Count; i++)
                banners.Add(source[i]);
            TestCollectionService.GetSpiritSeals();
        }

        private void BuildScreen()
        {
            ClearChildren(hostRoot);
            RectTransform screenRoot = CreateRect("SummonScreenRoot", hostRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = screenRoot.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = BackgroundColor;

            CreateBackdropLayer(screenRoot, "CrimsonGlow", new Vector2(-240f, 1080f), new Vector2(1480f, 900f), new Color(0.48f, 0.18f, 0.14f, 0.16f), 0f);
            CreateBackdropLayer(screenRoot, "IndigoGlow", new Vector2(260f, 1320f), new Vector2(1080f, 760f), new Color(0.16f, 0.2f, 0.34f, 0.12f), 0f);
            CreateBackdropLayer(screenRoot, "BandA", new Vector2(-420f, 760f), new Vector2(1640f, 190f), new Color(0.11f, 0.09f, 0.14f, 0.58f), 8f);

            RectTransform scrollRoot = CreateRect("ScrollRoot", screenRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            RectTransform viewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.sprite = GetWhiteSprite();
            viewportImage.color = new Color(0f, 0f, 0f, 0f);
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            scrollRect.viewport = viewport;
            emergencyScrollRect = scrollRect;

            contentFrame = CreateRect("ContentFrame", viewport, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            scrollRect.content = contentFrame;
            contentFrame.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup contentLayout = contentFrame.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 28, 36);
            contentLayout.spacing = 18f;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            ContentSizeFitter frameFitter = contentFrame.gameObject.AddComponent<ContentSizeFitter>();
            frameFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildHeader();
            BuildBannerSelectionPanel();
            BuildDetailPanel();
            BuildResultsPanel();
            BuildFooterPanel();
        }

        private void BuildEmergencyFallback(Exception exception)
        {
            bannerButtons.Clear();
            bannerGrid = null;
            featuredGrid = null;
            resultGrid = null;
            bannerPanelLayout = null;
            detailPanelLayout = null;
            resultPanelLayout = null;
            bannerGridLayout = null;
            featuredGridLayout = null;
            resultGridLayout = null;
            featuredGridRoot = null;
            resultGridRoot = null;
            resultEmptyState = null;
            emergencyScrollRect = null;
            usingEmergencyLayout = true;

            ClearChildren(hostRoot);
            RectTransform screenRoot = CreateRect("EmergencySummonScreen", hostRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = screenRoot.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = BackgroundColor;
            CreateBackdropLayer(screenRoot, "CrimsonVeil", new Vector2(-220f, 980f), new Vector2(1380f, 760f), new Color(0.42f, 0.16f, 0.12f, 0.18f), -5f);
            CreateBackdropLayer(screenRoot, "MoonGlow", new Vector2(280f, 1180f), new Vector2(980f, 620f), new Color(0.16f, 0.22f, 0.38f, 0.12f), 0f);
            CreateBackdropLayer(screenRoot, "LacquerBand", new Vector2(-360f, 720f), new Vector2(1560f, 180f), new Color(0.11f, 0.09f, 0.14f, 0.54f), 8f);

            RectTransform panel = CreateRect("EmergencyPanel", screenRoot, Vector2.zero, Vector2.one, new Vector2(18f, 64f), new Vector2(-18f, -56f));
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = GetWhiteSprite();
            panelImage.color = new Color(0.1f, 0.09f, 0.09f, 0.965f);
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
            outline.effectDistance = new Vector2(3f, -3f);
            Image topLine = CreateRect("TopLine", panel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -6f), Vector2.zero).gameObject.AddComponent<Image>();
            topLine.sprite = GetWhiteSprite();
            topLine.color = new Color(0.78f, 0.62f, 0.28f, 0.85f);

            contentFrame = CreateRect("EmergencyContent", panel, Vector2.zero, Vector2.one, new Vector2(24f, 18f), new Vector2(-24f, -18f));
            VerticalLayoutGroup stack = contentFrame.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(0, 0, 28, 32);
            stack.spacing = 16f;
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            BuildEmergencyHeader(exception);
            BuildEmergencyBannerPanel();
            BuildEmergencyDetailPanel();
            BuildEmergencyResultsPanel();
            BuildFooterPanel();
        }

        private void BuildEmergencyHeader(Exception exception)
        {
            RectTransform root = CreateRect("EmergencyHeader", contentFrame, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = exception == null ? 276f : 372f;
            VerticalLayoutGroup stack = root.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(18, 18, 14, 14);
            stack.spacing = 10f;
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            Text eyebrow = CreateText("Eyebrow", root, TextAnchor.MiddleCenter, 15, FontStyle.Bold);
            eyebrow.text = "SPIRIT GATE";
            eyebrow.color = new Color(0.93f, 0.82f, 0.5f, 1f);
            eyebrow.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;

            Text title = CreateText("Title", root, TextAnchor.MiddleCenter, 44, FontStyle.Bold);
            title.text = "SUMMON GATE";
            title.color = HeadingColor;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 52f;

            Text subtitle = CreateText("Subtitle", root, TextAnchor.MiddleCenter, 16, FontStyle.Bold);
            subtitle.text = "Choose a banner, inspect the rate-up trio, and send fresh pulls straight into the Barracks.";
            subtitle.color = BodyColor;
            subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            subtitle.verticalOverflow = VerticalWrapMode.Overflow;
            subtitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 44f;

            RectTransform statePill = CreatePill(root, "StatePill", Vector2.zero, Vector2.one, new Color(0.16f, 0.15f, 0.14f, 0.98f));
            statePill.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;
            bannerStateLabel = CreateText("StateLabel", statePill, TextAnchor.MiddleCenter, 14, FontStyle.Bold);
            bannerStateLabel.color = HeadingColor;

            RectTransform sealsPill = CreatePill(root, "SealsPill", Vector2.zero, Vector2.one, new Color(0.16f, 0.27f, 0.37f, 0.98f));
            sealsPill.gameObject.AddComponent<LayoutElement>().preferredHeight = 42f;
            sealsLabel = CreateText("SealsLabel", sealsPill, TextAnchor.MiddleCenter, 18, FontStyle.Bold);
            sealsLabel.color = HeadingColor;

            RectTransform collectionPill = CreatePill(root, "CollectionPill", Vector2.zero, Vector2.one, new Color(0.25f, 0.19f, 0.11f, 0.98f));
            collectionPill.gameObject.AddComponent<LayoutElement>().preferredHeight = 36f;
            collectionLabel = CreateText("CollectionLabel", collectionPill, TextAnchor.MiddleCenter, 16, FontStyle.Bold);
            collectionLabel.color = HeadingColor;

            if (exception == null)
                return;

            RectTransform errorRoot = CreateRect("ErrorRoot", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            errorRoot.gameObject.AddComponent<LayoutElement>().preferredHeight = 96f;
            Image errorBackground = errorRoot.gameObject.AddComponent<Image>();
            errorBackground.sprite = GetWhiteSprite();
            errorBackground.color = new Color(0.18f, 0.08f, 0.08f, 0.9f);
            Text errorLabel = CreateText("ErrorLabel", errorRoot, TextAnchor.UpperLeft, 12, FontStyle.Normal);
            errorLabel.text = $"Stable fallback layout is active because the richer summon scene failed:`n{exception.GetType().Name}: {exception.Message}";
            errorLabel.color = new Color(1f, 0.86f, 0.82f, 1f);
            errorLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            errorLabel.verticalOverflow = VerticalWrapMode.Overflow;
            errorLabel.rectTransform.offsetMin = new Vector2(14f, 12f);
            errorLabel.rectTransform.offsetMax = new Vector2(-14f, -12f);
        }

        private void BuildEmergencyBannerPanel()
        {
            CreatePanel(contentFrame, "EmergencyBannerPanel", Mathf.Max(170f, 74f + (banners.Count * 68f)), PanelOuterColor, PanelInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("Content", inner, Vector2.zero, Vector2.one, new Vector2(24f, 20f), new Vector2(-24f, -20f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 10f;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            Text heading = CreateText("Heading", content, TextAnchor.UpperLeft, 26, FontStyle.Bold);
            heading.text = "ACTIVE BANNERS";
            heading.color = HeadingColor;
            heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;

            if (banners.Count == 0)
            {
                CreateEmptyMessageCard(content, "No local test banners are currently registered.");
                return;
            }

            for (int i = 0; i < banners.Count; i++)
                bannerButtons.Add(CreateBannerButton(content, banners[i]));
        }

        private void BuildEmergencyDetailPanel()
        {
            CreatePanel(contentFrame, "EmergencyDetailPanel", 700f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            detailPanelLayout = inner.parent.GetComponent<LayoutElement>();
            detailAccentBand = CreateRect("Accent", inner, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(10f, 0f)).gameObject.AddComponent<Image>();
            detailAccentBand.sprite = GetWhiteSprite();
            RectTransform content = CreateRect("Content", inner, Vector2.zero, Vector2.one, new Vector2(24f, 20f), new Vector2(-24f, -20f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 10f;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "BANNER FOCUS", "Review the current gate, its disclosed odds, and the featured trio before committing seals.");

            bannerTitleLabel = CreateText("BannerTitle", content, TextAnchor.UpperLeft, 34, FontStyle.Bold);
            bannerTitleLabel.color = HeadingColor;
            bannerTitleLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 42f;
            bannerSubtitleLabel = CreateText("BannerSubtitle", content, TextAnchor.UpperLeft, 16, FontStyle.Bold);
            bannerSubtitleLabel.color = BodyColor;
            bannerSubtitleLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            bannerSubtitleLabel.verticalOverflow = VerticalWrapMode.Overflow;
            bannerSubtitleLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;
            bannerSummaryLabel = CreateText("BannerSummary", content, TextAnchor.UpperLeft, 15, FontStyle.Normal);
            bannerSummaryLabel.color = BodyColor;
            bannerSummaryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            bannerSummaryLabel.verticalOverflow = VerticalWrapMode.Overflow;
            bannerSummaryLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 62f;
            bannerOddsLabel = CreateText("BannerOdds", content, TextAnchor.UpperLeft, 14, FontStyle.Bold);
            bannerOddsLabel.color = new Color(0.95f, 0.88f, 0.7f, 1f);
            bannerOddsLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;
            bannerDisclosureLabel = CreateText("BannerDisclosure", content, TextAnchor.UpperLeft, 13, FontStyle.Normal);
            bannerDisclosureLabel.color = MutedColor;
            bannerDisclosureLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            bannerDisclosureLabel.verticalOverflow = VerticalWrapMode.Overflow;
            bannerDisclosureLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;

            CreateDivider(content);

            featuredSummaryLabel = CreateText("FeaturedSummary", content, TextAnchor.UpperLeft, 15, FontStyle.Bold);
            featuredSummaryLabel.color = BodyColor;
            featuredSummaryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            featuredSummaryLabel.verticalOverflow = VerticalWrapMode.Overflow;
            featuredSummaryLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 36f;

            featuredGridRoot = CreateRect("FeaturedGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            featuredGridLayout = featuredGridRoot.gameObject.AddComponent<LayoutElement>();
            featuredGridLayout.preferredHeight = 156f;
            featuredGrid = featuredGridRoot.gameObject.AddComponent<GridLayoutGroup>();
            featuredGrid.spacing = new Vector2(12f, 12f);
            featuredGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            featuredGrid.constraintCount = 2;
            featuredGrid.cellSize = new Vector2(240f, 156f);

            RectTransform actions = CreateRect("Actions", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            actions.gameObject.AddComponent<LayoutElement>().preferredHeight = 72f;
            HorizontalLayoutGroup actionLayout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 10f;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = false;
            CreateActionButton(actions, "SinglePull", "SINGLE PULL", ButtonPrimaryColor, () => PerformSummon(1));
            CreateActionButton(actions, "TenPull", "PULL x10", ButtonSecondaryColor, () => PerformSummon(10));
            CreateActionButton(actions, "Barracks", "BARRACKS", ButtonNeutralColor, () => LoadSceneIfAvailable(BarracksSceneName));
            CreateActionButton(actions, "MainMenu", "MAIN MENU", ButtonTertiaryColor, () => LoadSceneIfAvailable(MainMenuSceneName));
        }

        private void BuildEmergencyResultsPanel()
        {
            CreatePanel(contentFrame, "EmergencyResultsPanel", 392f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            resultPanelLayout = inner.parent.GetComponent<LayoutElement>();
            RectTransform content = CreateRect("Content", inner, Vector2.zero, Vector2.one, new Vector2(24f, 20f), new Vector2(-24f, -20f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 10f;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "REVEAL LEDGER", "Latest pulls resolve here immediately. New units are marked and duplicates update owned copy counts.");

            resultStatusLabel = CreateText("Status", content, TextAnchor.UpperLeft, 26, FontStyle.Bold);
            resultStatusLabel.color = HeadingColor;
            resultStatusLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 36f;
            resultSummaryLabel = CreateText("Summary", content, TextAnchor.UpperLeft, 14, FontStyle.Normal);
            resultSummaryLabel.color = BodyColor;
            resultSummaryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            resultSummaryLabel.verticalOverflow = VerticalWrapMode.Overflow;
            resultSummaryLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;

            resultEmptyState = CreateEmptyState(content, "No summon performed yet. Pull on a banner to reveal characters here and sync them into the Barracks.");

            resultGridRoot = CreateRect("ResultGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            resultGridLayout = resultGridRoot.gameObject.AddComponent<LayoutElement>();
            resultGridLayout.preferredHeight = 0f;
            resultGrid = resultGridRoot.gameObject.AddComponent<GridLayoutGroup>();
            resultGrid.spacing = new Vector2(12f, 12f);
            resultGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            resultGrid.constraintCount = 2;
            resultGrid.cellSize = new Vector2(168f, 170f);
        }

        private void BuildHeader()
        {
            CreatePanel(contentFrame, "HeaderPanel", 198f, HeaderOuterColor, HeaderInnerColor, out RectTransform inner);
            RectTransform content = CreateRect("HeaderContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 22f), new Vector2(-24f, -22f));
            RectTransform titleBlock = CreateRect("TitleBlock", content, new Vector2(0f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            Text eyebrow = CreateText("Eyebrow", titleBlock, TextAnchor.UpperLeft, 16, FontStyle.Bold);
            eyebrow.text = "SPIRIT GATE";
            eyebrow.color = new Color(0.93f, 0.82f, 0.5f, 1f);
            eyebrow.rectTransform.offsetMin = new Vector2(0f, 144f);
            Text title = CreateText("Title", titleBlock, TextAnchor.MiddleLeft, 48, FontStyle.Bold);
            title.text = "SUMMON GATE";
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 54f);
            title.rectTransform.offsetMax = new Vector2(0f, -28f);
            Text subtitle = CreateText("Subtitle", titleBlock, TextAnchor.LowerLeft, 17, FontStyle.Normal);
            subtitle.text = "Choose from two active test banners, review disclosed odds, and send newly revealed warriors straight into the Barracks.";
            subtitle.color = BodyColor;
            subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            subtitle.verticalOverflow = VerticalWrapMode.Overflow;
            subtitle.rectTransform.offsetMax = new Vector2(0f, -102f);

            RectTransform statusBlock = CreateRect("StatusBlock", content, new Vector2(0.72f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform sealsPill = CreatePill(statusBlock, "SealsPill", new Vector2(0.08f, 0.45f), new Vector2(1f, 1f), new Color(0.17f, 0.22f, 0.29f, 0.98f));
            sealsLabel = CreateText("SealsLabel", sealsPill, TextAnchor.MiddleCenter, 22, FontStyle.Bold);
            sealsLabel.color = HeadingColor;
            RectTransform collectionPill = CreatePill(statusBlock, "CollectionPill", new Vector2(0.08f, 0.21f), new Vector2(1f, 0.62f), new Color(0.18f, 0.15f, 0.12f, 0.98f));
            collectionLabel = CreateText("CollectionLabel", collectionPill, TextAnchor.MiddleCenter, 15, FontStyle.Bold);
            collectionLabel.color = BodyColor;
            RectTransform notePill = CreatePill(statusBlock, "NotePill", new Vector2(0.08f, 0f), new Vector2(1f, 0.18f), new Color(0.17f, 0.13f, 0.11f, 0.98f));
            bannerStateLabel = CreateText("BannerStateLabel", notePill, TextAnchor.MiddleCenter, 13, FontStyle.Bold);
            bannerStateLabel.color = MutedColor;
            RefreshCurrencyLabel();
        }

        private void BuildBannerSelectionPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "BannerPanel", 248f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            bannerPanelLayout = outer.GetComponent<LayoutElement>();
            RectTransform content = CreateRect("BannerContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            CreateSectionHeader(content, "ACTIVE BANNERS", "Select one of the current banners below. Each uses disclosed rates and its own featured rate-up trio.");
            CreateDivider(content);
            RectTransform gridRoot = CreateRect("BannerGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bannerGridLayout = gridRoot.gameObject.AddComponent<LayoutElement>();
            bannerGridLayout.preferredHeight = 118f;
            bannerGrid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            bannerGrid.spacing = new Vector2(14f, 14f);
            bannerGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            bannerGrid.constraintCount = 2;
            bannerGrid.cellSize = new Vector2(320f, 104f);
            bannerButtons.Clear();
            for (int i = 0; i < banners.Count; i++)
                bannerButtons.Add(CreateBannerButton(gridRoot, banners[i]));
        }
        private void BuildDetailPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "DetailPanel", 612f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            detailPanelLayout = outer.GetComponent<LayoutElement>();
            detailAccentBand = CreateRect("Accent", inner, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(10f, 0f)).gameObject.AddComponent<Image>();
            detailAccentBand.sprite = GetWhiteSprite();

            RectTransform content = CreateRect("DetailContent", inner, Vector2.zero, Vector2.one, new Vector2(28f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            RectTransform titleBlock = CreateRect("TitleBlock", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            titleBlock.gameObject.AddComponent<LayoutElement>().preferredHeight = 176f;
            bannerTitleLabel = CreateText("BannerTitle", titleBlock, TextAnchor.UpperLeft, 38, FontStyle.Bold);
            bannerTitleLabel.color = HeadingColor;
            bannerTitleLabel.rectTransform.offsetMin = new Vector2(0f, 112f);
            bannerSubtitleLabel = CreateText("BannerSubtitle", titleBlock, TextAnchor.MiddleLeft, 18, FontStyle.Bold);
            bannerSubtitleLabel.color = BodyColor;
            bannerSubtitleLabel.rectTransform.offsetMin = new Vector2(0f, 74f);
            bannerSubtitleLabel.rectTransform.offsetMax = new Vector2(0f, -48f);
            bannerSummaryLabel = CreateText("BannerSummary", titleBlock, TextAnchor.LowerLeft, 16, FontStyle.Normal);
            bannerSummaryLabel.color = BodyColor;
            bannerSummaryLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            bannerSummaryLabel.verticalOverflow = VerticalWrapMode.Overflow;
            bannerSummaryLabel.rectTransform.offsetMax = new Vector2(0f, -88f);
            bannerOddsLabel = CreateText("BannerOdds", titleBlock, TextAnchor.LowerLeft, 15, FontStyle.Bold);
            bannerOddsLabel.color = new Color(0.95f, 0.88f, 0.7f, 1f);
            bannerOddsLabel.rectTransform.offsetMax = new Vector2(0f, -128f);
            bannerDisclosureLabel = CreateText("BannerDisclosure", titleBlock, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            bannerDisclosureLabel.color = MutedColor;
            bannerDisclosureLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            bannerDisclosureLabel.verticalOverflow = VerticalWrapMode.Overflow;
            bannerDisclosureLabel.rectTransform.offsetMax = new Vector2(0f, -156f);

            RectTransform actions = CreateRect("Actions", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            actions.gameObject.AddComponent<LayoutElement>().preferredHeight = 56f;
            HorizontalLayoutGroup actionLayout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 10f;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = false;
            CreateActionButton(actions, "SinglePull", "SINGLE PULL", ButtonPrimaryColor, () => PerformSummon(1));
            CreateActionButton(actions, "TenPull", "PULL x10", ButtonSecondaryColor, () => PerformSummon(10));
            CreateActionButton(actions, "Barracks", "BARRACKS", ButtonNeutralColor, () => LoadSceneIfAvailable(BarracksSceneName));
            CreateActionButton(actions, "MainMenu", "MAIN MENU", ButtonTertiaryColor, () => LoadSceneIfAvailable(MainMenuSceneName));

            RectTransform featuredHeader = CreateSectionHeader(content, "FEATURED UNITS", "These are the rate-up warriors for the selected banner, shown with the current art fallbacks and authored identity text.");
            featuredHeader.GetComponent<LayoutElement>().preferredHeight = 68f;
            CreateDivider(content);
            featuredGridRoot = CreateRect("FeaturedGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            featuredGridLayout = featuredGridRoot.gameObject.AddComponent<LayoutElement>();
            featuredGridLayout.preferredHeight = 180f;
            featuredGrid = featuredGridRoot.gameObject.AddComponent<GridLayoutGroup>();
            featuredGrid.spacing = new Vector2(14f, 14f);
            featuredGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            featuredGrid.constraintCount = 3;
            featuredGrid.cellSize = new Vector2(260f, 156f);
        }

        private void BuildResultsPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "ResultsPanel", 420f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            resultPanelLayout = outer.GetComponent<LayoutElement>();
            RectTransform content = CreateRect("ResultsContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            RectTransform header = CreateRect("ResultsHeader", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 72f;
            Text title = CreateText("Title", header, TextAnchor.UpperLeft, 28, FontStyle.Bold);
            title.text = "LATEST REVEAL";
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 18f);
            resultStatusLabel = CreateText("Status", header, TextAnchor.MiddleRight, 16, FontStyle.Bold);
            resultStatusLabel.color = BodyColor;
            resultStatusLabel.rectTransform.offsetMin = new Vector2(260f, 22f);
            resultSummaryLabel = CreateText("Summary", header, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            resultSummaryLabel.color = MutedColor;
            resultSummaryLabel.rectTransform.offsetMax = new Vector2(0f, -38f);
            CreateDivider(content);

            resultEmptyState = CreateEmptyState(content, "No summon performed yet. Pull on a banner to populate the reveal grid and update your Barracks immediately.");
            resultEmptyState.GetComponent<LayoutElement>().preferredHeight = 88f;
            resultGridRoot = CreateRect("ResultGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            resultGridLayout = resultGridRoot.gameObject.AddComponent<LayoutElement>();
            resultGridLayout.preferredHeight = 194f;
            resultGrid = resultGridRoot.gameObject.AddComponent<GridLayoutGroup>();
            resultGrid.spacing = new Vector2(12f, 12f);
            resultGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            resultGrid.constraintCount = 3;
            resultGrid.cellSize = new Vector2(168f, 170f);
        }

        private void BuildFooterPanel()
        {
            CreatePanel(contentFrame, "FooterPanel", 132f, new Color(0.16f, 0.13f, 0.11f, 0.96f), new Color(0.08f, 0.07f, 0.07f, 0.98f), out RectTransform inner);
            RectTransform content = CreateRect("FooterContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 18f), new Vector2(-24f, -18f));
            Text footer = CreateText("Footer", content, TextAnchor.MiddleLeft, 14, FontStyle.Normal);
            footer.text = "This gate is local-only, but the roster updates, odds disclosure, currency spend, and Barracks sync are all live inside the current slice.";
            footer.color = BodyColor;
            footer.horizontalOverflow = HorizontalWrapMode.Wrap;
            footer.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void SelectBanner(string bannerId)
        {
            TestSummonService.TestSummonBanner banner = TestSummonService.GetBanner(bannerId);
            selectedBannerId = banner != null ? banner.BannerId : string.Empty;
            for (int i = 0; i < bannerButtons.Count; i++)
            {
                BannerButtonView view = bannerButtons[i];
                bool isSelected = banner != null && string.Equals(view.Banner.BannerId, banner.BannerId, StringComparison.OrdinalIgnoreCase);
                view.Background.color = isSelected ? new Color(0.2f, 0.16f, 0.12f, 0.98f) : new Color(0.11f, 0.1f, 0.1f, 0.96f);
                view.Accent.color = isSelected ? view.Banner.AccentColor : new Color(view.Banner.AccentColor.r * 0.66f, view.Banner.AccentColor.g * 0.66f, view.Banner.AccentColor.b * 0.66f, 0.92f);
                view.TitleLabel.color = isSelected ? HeadingColor : new Color(0.92f, 0.88f, 0.8f, 1f);
                view.SubtitleLabel.color = isSelected ? BodyColor : MutedColor;
            }

            if (banner == null)
                return;

            if (detailAccentBand != null)
                detailAccentBand.color = banner.AccentColor;
            bannerTitleLabel.text = banner.Title;
            bannerSubtitleLabel.text = $"{banner.BannerType}  •  {banner.FeaturedCharacterIds.Count} featured  •  Single {banner.SinglePullCost} seals  •  Multi {banner.MultiPullCost} seals";
            bannerSummaryLabel.text = banner.Summary;
            bannerOddsLabel.text = banner.GetOddsSummary();
            bannerDisclosureLabel.text = $"Pool {banner.PoolCharacterIds.Count} warriors • {banner.FeaturedCharacterIds.Count} rate-up units • {banner.Disclosure}";
            PopulateFeaturedUnits(banner);
            resultStatusLabel.text = "Ready";
            resultStatusLabel.color = BodyColor;
            if (lastResults.Count == 0)
                resultSummaryLabel.text = "Pick single or multi pull to reveal warriors and update the Barracks instantly.";
            RefreshCurrencyLabel();
            ApplyResponsiveLayout(true);
        }

        private void PopulateFeaturedUnits(TestSummonService.TestSummonBanner banner)
        {
            List<CharacterDefinition> featuredDefinitions = new List<CharacterDefinition>();
            for (int i = 0; i < banner.FeaturedCharacterIds.Count; i++)
            {
                CharacterDefinition definition = CharacterFactory.GetCharacterDefinitionById(banner.FeaturedCharacterIds[i]);
                if (definition != null)
                    featuredDefinitions.Add(definition);
            }

            if (featuredSummaryLabel != null)
            {
                if (featuredDefinitions.Count == 0)
                {
                    featuredSummaryLabel.text = "RATE-UP FOCUS • no featured character definitions resolved for this banner";
                }
                else
                {
                    List<string> names = new List<string>();
                    for (int i = 0; i < featuredDefinitions.Count; i++)
                        names.Add(BarracksCharacterPresentation.GetDisplayName(featuredDefinitions[i]).ToUpperInvariant());
                    featuredSummaryLabel.text = $"RATE-UP FOCUS • {string.Join(" • ", names)}";
                }
            }

            if (featuredGridRoot == null || featuredGrid == null || featuredGridLayout == null)
                return;

            ClearChildren(featuredGridRoot);
            featuredGrid.spacing = new Vector2(12f, 12f);

            if (featuredDefinitions.Count == 0)
            {
                CreateEmptyMessageCard(featuredGridRoot, "No featured character definitions resolved for this banner.");
                return;
            }

            for (int i = 0; i < featuredDefinitions.Count; i++)
                CreateFeaturedCharacterCard(featuredGridRoot, featuredDefinitions[i], banner.AccentColor);
        }

        private void PerformSummon(int pullCount)
        {
            if (!TestSummonService.TryPerformSummon(selectedBannerId, pullCount, out TestSummonService.TestSummonSession session, out string error))
            {
                resultStatusLabel.text = "Blocked";
                resultStatusLabel.color = ErrorColor;
                resultSummaryLabel.text = error;
                RefreshCurrencyLabel();
                return;
            }

            lastResults.Clear();
            for (int i = 0; i < session.Results.Count; i++)
                lastResults.Add(session.Results[i]);

            resultStatusLabel.text = $"{session.PullCount} PULL{(session.PullCount > 1 ? "S" : string.Empty)}";
            resultStatusLabel.color = HeadingColor;
            int newCount = 0;
            for (int i = 0; i < lastResults.Count; i++)
            {
                if (lastResults[i].IsNew)
                    newCount++;
            }

            resultSummaryLabel.text = newCount > 0
                ? $"Spent {session.SpentSpiritSeals} seals. {newCount} new character{(newCount > 1 ? "s" : string.Empty)} added. Collection now holds {TestCollectionService.GetOwnedCharacterCount()} units."
                : $"Spent {session.SpentSpiritSeals} seals. All pulls were duplicates, so total owned copies climbed to {TestCollectionService.GetTotalOwnedCopies()}.";
            PopulateResults();
            RefreshCurrencyLabel();
            ApplyResponsiveLayout(true);
        }
        private void PopulateResults()
        {
            if (resultGridRoot == null || resultGrid == null || resultGridLayout == null)
                return;

            if (resultEmptyState != null)
                resultEmptyState.SetActive(lastResults.Count == 0);
            ClearChildren(resultGridRoot);
            resultGrid.spacing = new Vector2(12f, 12f);
            resultGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            resultGrid.constraintCount = 3;
            resultGrid.cellSize = new Vector2(168f, 170f);
            for (int i = 0; i < lastResults.Count; i++)
                CreateResultCard(resultGridRoot, lastResults[i]);
        }

        private void RefreshCurrencyLabel()
        {
            if (sealsLabel != null)
                sealsLabel.text = $"SPIRIT SEALS {TestCollectionService.GetSpiritSeals()}";
            if (collectionLabel != null)
                collectionLabel.text = $"OWNED {TestCollectionService.GetOwnedCharacterCount()} • DUPES {TestCollectionService.GetDuplicateCopies()}";
            if (bannerStateLabel != null)
            {
                TestSummonService.TestSummonBanner activeBanner = TestSummonService.GetBanner(selectedBannerId);
                bannerStateLabel.text = activeBanner != null
                    ? $"{activeBanner.Title} ACTIVE • {banners.Count} BANNERS • DISCLOSED RATES"
                    : $"{banners.Count} BANNERS • DISCLOSED RATES";
            }
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (contentFrame == null)
                return;

            Vector2Int currentScreenSize = GetLayoutSize();
            Rect safeArea = Screen.safeArea;
            float parentWidth = ResolveLayoutParentWidth();
            if (!force && currentScreenSize == lastScreenSize && safeArea == lastSafeArea && Mathf.Approximately(parentWidth, lastParentWidth))
                return;

            lastScreenSize = currentScreenSize;
            lastSafeArea = safeArea;
            lastParentWidth = parentWidth;

            float maxWidth = PhoneContentMaxWidth;
            if (currentScreenSize.x >= ExpandedBreakpoint)
                maxWidth = ExpandedContentMaxWidth;
            else if (currentScreenSize.x >= TabletBreakpoint)
                maxWidth = TabletContentMaxWidth;

            float contentWidth = Mathf.Max(320f, Mathf.Min(maxWidth, parentWidth - (ContentHorizontalMargin * 2f)));
            if (usingEmergencyLayout)
            {
                float sideInset = Mathf.Max(ContentHorizontalMargin, (parentWidth - contentWidth) * 0.5f);
                contentFrame.anchorMin = Vector2.zero;
                contentFrame.anchorMax = Vector2.one;
                contentFrame.offsetMin = new Vector2(sideInset, 18f);
                contentFrame.offsetMax = new Vector2(-sideInset, -18f);
                ApplyEmergencyResponsiveLayout(contentWidth);
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
                return;
            }

            contentFrame.anchorMin = new Vector2(0.5f, 1f);
            contentFrame.anchorMax = new Vector2(0.5f, 1f);
            contentFrame.offsetMin = Vector2.zero;
            contentFrame.offsetMax = Vector2.zero;
            contentFrame.sizeDelta = new Vector2(contentWidth, 0f);

            if (bannerGrid == null || featuredGrid == null || resultGrid == null || bannerGridLayout == null || featuredGridLayout == null || resultGridLayout == null || bannerPanelLayout == null || detailPanelLayout == null || resultPanelLayout == null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
                return;
            }
            float sectionWidth = Mathf.Max(280f, contentWidth - 56f);
            ConfigureGrid(bannerGrid, bannerGridLayout, bannerPanelLayout, Mathf.Max(1, bannerButtons.Count), contentWidth >= 720f ? 2 : 1, sectionWidth, 280f, 104f, 124f, 32f);
            ConfigureGrid(featuredGrid, featuredGridLayout, detailPanelLayout, Mathf.Max(1, featuredGridRoot.childCount), contentWidth >= 1000f ? 3 : contentWidth >= 620f ? 2 : 1, sectionWidth, 220f, 156f, 388f, 32f);
            ConfigureGrid(resultGrid, resultGridLayout, resultPanelLayout, Mathf.Max(1, lastResults.Count), contentWidth >= 1080f ? 5 : contentWidth >= 820f ? 4 : contentWidth >= 560f ? 3 : 2, sectionWidth, 150f, 170f, lastResults.Count == 0 ? 214f : 124f, 32f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
        }

        private void ApplyEmergencyResponsiveLayout(float contentWidth)
        {
            float sectionWidth = Mathf.Max(280f, contentWidth - 48f);
            float subtitleHeight = SyncTextBlockHeight(bannerSubtitleLabel, 40f, 8f);
            float summaryHeight = SyncTextBlockHeight(bannerSummaryLabel, 56f, 8f);
            float disclosureHeight = SyncTextBlockHeight(bannerDisclosureLabel, 38f, 8f);
            float featuredLineHeight = SyncTextBlockHeight(featuredSummaryLabel, 34f, 6f);
            float resultSummaryHeight = SyncTextBlockHeight(resultSummaryLabel, 56f, 8f);

            if (featuredGrid != null && featuredGridLayout != null && detailPanelLayout != null)
            {
                int featuredItems = Mathf.Max(1, featuredGridRoot != null ? featuredGridRoot.childCount : 0);
                int featuredColumns = contentWidth >= 760f ? 2 : 1;
                float detailStaticHeight = 72f + 42f + subtitleHeight + summaryHeight + 24f + disclosureHeight + 2f + featuredLineHeight + 72f + 112f;
                ConfigureGrid(featuredGrid, featuredGridLayout, detailPanelLayout, featuredItems, featuredColumns, sectionWidth, 220f, 156f, detailStaticHeight, 36f);
            }

            if (resultPanelLayout != null)
            {
                float resultStaticHeight = 72f + 36f + resultSummaryHeight + 96f;
                if (lastResults.Count <= 0)
                {
                    if (resultGridLayout != null)
                        resultGridLayout.preferredHeight = 0f;
                    resultPanelLayout.preferredHeight = resultStaticHeight + 112f;
                }
                else if (resultGrid != null && resultGridLayout != null)
                {
                    int resultColumns = contentWidth >= 920f ? 4 : contentWidth >= 680f ? 3 : 2;
                    ConfigureGrid(resultGrid, resultGridLayout, resultPanelLayout, Mathf.Max(1, lastResults.Count), resultColumns, sectionWidth, 150f, 170f, resultStaticHeight, 28f);
                }
            }
        }

        private static float SyncTextBlockHeight(Text text, float minHeight, float padding)
        {
            if (text == null)
                return minHeight;

            LayoutElement layout = text.GetComponent<LayoutElement>();
            if (layout == null)
                return minHeight;

            float preferred = Mathf.Max(minHeight, text.preferredHeight + padding);
            layout.preferredHeight = preferred;
            return preferred;
        }

        private void ResetEmergencyScrollPosition()
        {
            if (emergencyScrollRect == null)
                return;

            Canvas.ForceUpdateCanvases();
            emergencyScrollRect.verticalNormalizedPosition = 1f;
        }


        private float ResolveLayoutParentWidth()
        {
            if (usingEmergencyLayout && emergencyScrollRect != null && emergencyScrollRect.viewport != null)
            {
                float viewportWidth = emergencyScrollRect.viewport.rect.width;
                if (viewportWidth > 0f)
                    return viewportWidth;
            }

            RectTransform frameParent = contentFrame != null ? contentFrame.parent as RectTransform : null;
            if (frameParent != null)
            {
                float parentWidth = frameParent.rect.width;
                if (parentWidth > 0f)
                    return parentWidth;
            }

            if (hostRoot != null)
            {
                float hostWidth = hostRoot.rect.width;
                if (hostWidth > 0f)
                    return hostWidth;
            }

            RectTransform canvasRect = targetCanvas != null ? targetCanvas.transform as RectTransform : null;
            if (canvasRect != null)
            {
                float canvasWidth = canvasRect.rect.width;
                if (canvasWidth > 0f)
                    return canvasWidth;
            }

            return Screen.width;
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

        private static void ConfigureGrid(GridLayoutGroup grid, LayoutElement gridLayout, LayoutElement panelLayout, int itemCount, int columns, float availableWidth, float minCellWidth, float cellHeight, float headerHeight, float bottomPadding)
        {
            if (grid == null || gridLayout == null || panelLayout == null)
                return;

            columns = Mathf.Max(1, columns);
            grid.constraintCount = columns;
            float spacingX = grid.spacing.x;
            float cellWidth = Mathf.Floor((availableWidth - (spacingX * (columns - 1))) / columns);
            grid.cellSize = new Vector2(Mathf.Max(minCellWidth, cellWidth), cellHeight);
            int rows = Mathf.CeilToInt(itemCount / (float)columns);
            float gridHeight = (rows * cellHeight) + (Mathf.Max(0, rows - 1) * grid.spacing.y);
            gridLayout.preferredHeight = gridHeight;
            panelLayout.preferredHeight = headerHeight + gridHeight + bottomPadding;
        }

        private BannerButtonView CreateBannerButton(Transform parent, TestSummonService.TestSummonBanner banner)
        {
            RectTransform root = CreateRect($"Banner_{banner.BannerId}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.11f, 0.1f, 0.1f, 0.96f);
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => SelectBanner(banner.BannerId));
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
            Image accent = CreateRect("Accent", root, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(8f, 0f)).gameObject.AddComponent<Image>();
            accent.sprite = GetWhiteSprite();
            accent.color = banner.AccentColor;
            RectTransform content = CreateRect("Content", root, Vector2.zero, Vector2.one, new Vector2(20f, 12f), new Vector2(-18f, -12f));
            Text title = CreateText("Title", content, TextAnchor.UpperLeft, 22, FontStyle.Bold);
            title.text = banner.Title;
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 40f);
            Text subtitle = CreateText("Subtitle", content, TextAnchor.MiddleLeft, 14, FontStyle.Normal);
            subtitle.text = $"{banner.BannerType.ToUpperInvariant()} • {banner.FeaturedCharacterIds.Count} FEATURED";
            subtitle.color = BodyColor;
            subtitle.rectTransform.offsetMin = new Vector2(0f, 14f);
            subtitle.rectTransform.offsetMax = new Vector2(0f, -30f);
            Text costs = CreateText("Costs", content, TextAnchor.LowerLeft, 13, FontStyle.Bold);
            costs.text = $"1x {banner.SinglePullCost} • 10x {banner.MultiPullCost}";
            costs.color = new Color(0.93f, 0.82f, 0.5f, 1f);
            costs.rectTransform.offsetMax = new Vector2(0f, -54f);
            return new BannerButtonView { Banner = banner, Background = background, Accent = accent, TitleLabel = title, SubtitleLabel = subtitle };
        }

        private static void CreateFeaturedCharacterCard(Transform parent, CharacterDefinition definition, Color accentColor)
        {
            RectTransform root = CreateRect($"Featured_{definition.CharacterId}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.11f, 0.1f, 0.1f, 0.96f);
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.36f);
            outline.effectDistance = new Vector2(2f, -2f);
            Image accent = CreateRect("Accent", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), Vector2.zero).gameObject.AddComponent<Image>();
            accent.sprite = GetWhiteSprite();
            accent.color = accentColor;
            RectTransform portraitRect = CreateRect("Portrait", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, -54f), new Vector2(104f, 38f));
            Image portraitFrame = portraitRect.gameObject.AddComponent<Image>();
            portraitFrame.sprite = GetWhiteSprite();
            portraitFrame.color = new Color(0.18f, 0.15f, 0.14f, 1f);
            RectTransform portraitInner = CreateRect("PortraitInner", portraitRect, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image portrait = portraitInner.gameObject.AddComponent<Image>();
            portrait.preserveAspect = true;
            Text placeholderLabel;
            GameObject placeholder = CreatePlaceholder(portraitInner, out placeholderLabel, 28);
            BarracksCharacterPresentation.SetPortraitVisual(definition, portrait, placeholder, placeholderLabel, 28);
            RectTransform info = CreateRect("Info", root, Vector2.zero, Vector2.one, new Vector2(120f, 14f), new Vector2(-14f, -14f));
            Text name = CreateText("Name", info, TextAnchor.UpperLeft, 22, FontStyle.Bold);
            name.text = BarracksCharacterPresentation.GetDisplayName(definition).ToUpperInvariant();
            name.color = HeadingColor;
            name.rectTransform.offsetMin = new Vector2(0f, 88f);
            Text identity = CreateText("Identity", info, TextAnchor.MiddleLeft, 14, FontStyle.Bold);
            identity.text = $"{BarracksCharacterPresentation.GetElementLabel(definition.ElementalType)} • {BarracksCharacterPresentation.GetWeaponLabel(definition.MartialArtsType)}";
            identity.color = BodyColor;
            identity.rectTransform.offsetMin = new Vector2(0f, 50f);
            identity.rectTransform.offsetMax = new Vector2(0f, -56f);
            Text summary = CreateText("Summary", info, TextAnchor.LowerLeft, 13, FontStyle.Normal);
            summary.text = BarracksCharacterPresentation.GetCardSummary(definition);
            summary.color = MutedColor;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            summary.verticalOverflow = VerticalWrapMode.Overflow;
            summary.rectTransform.offsetMax = new Vector2(0f, -88f);
        }

        private static void CreateResultCard(Transform parent, TestSummonService.TestSummonPullResult result)
        {
            RectTransform root = CreateRect($"Result_{result.Definition.CharacterId}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.11f, 0.1f, 0.1f, 0.96f);
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.36f);
            outline.effectDistance = new Vector2(2f, -2f);
            Color rarityColor = BarracksCharacterPresentation.GetRarityColor(result.Definition.Rarity);
            Image accent = CreateRect("Accent", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), Vector2.zero).gameObject.AddComponent<Image>();
            accent.sprite = GetWhiteSprite();
            accent.color = rarityColor;
            RectTransform statusPill = CreateRect("Status", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-96f, -28f), new Vector2(-10f, -8f));
            Image statusImage = statusPill.gameObject.AddComponent<Image>();
            statusImage.sprite = GetWhiteSprite();
            statusImage.color = result.IsNew ? new Color(0.28f, 0.35f, 0.2f, 1f) : new Color(0.17f, 0.15f, 0.14f, 1f);
            Text status = CreateText("StatusLabel", statusPill, TextAnchor.MiddleCenter, 12, FontStyle.Bold);
            status.text = result.IsNew ? "NEW" : $"x{result.OwnedCount}";
            status.color = result.IsNew ? new Color(0.94f, 0.96f, 0.78f, 1f) : BodyColor;
            RectTransform portraitRect = CreateRect("Portrait", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-44f, -56f), new Vector2(44f, 28f));
            Image portraitFrame = portraitRect.gameObject.AddComponent<Image>();
            portraitFrame.sprite = GetWhiteSprite();
            portraitFrame.color = new Color(0.18f, 0.15f, 0.14f, 1f);
            RectTransform portraitInner = CreateRect("PortraitInner", portraitRect, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image portrait = portraitInner.gameObject.AddComponent<Image>();
            portrait.preserveAspect = true;
            Text placeholderLabel;
            GameObject placeholder = CreatePlaceholder(portraitInner, out placeholderLabel, 26);
            BarracksCharacterPresentation.SetPortraitVisual(result.Definition, portrait, placeholder, placeholderLabel, 26);
            Text name = CreateText("Name", root, TextAnchor.LowerCenter, 16, FontStyle.Bold);
            name.text = BarracksCharacterPresentation.GetDisplayName(result.Definition).ToUpperInvariant();
            name.color = HeadingColor;
            name.rectTransform.offsetMin = new Vector2(8f, 52f);
            name.rectTransform.offsetMax = new Vector2(-8f, -42f);
            Text rarity = CreateText("Rarity", root, TextAnchor.LowerCenter, 13, FontStyle.Bold);
            rarity.text = BarracksCharacterPresentation.GetRarityLabel(result.Definition.Rarity);
            rarity.color = rarityColor;
            rarity.rectTransform.offsetMax = new Vector2(0f, -22f);
        }
        private static RectTransform CreatePanel(Transform parent, string name, float preferredHeight, Color outerColor, Color innerColor, out RectTransform inner)
        {
            RectTransform outer = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            LayoutElement layout = outer.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = preferredHeight;
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

        private static RectTransform CreatePill(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            RectTransform pill = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = pill.gameObject.AddComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.color = color;
            return pill;
        }

        private static RectTransform CreateSectionHeader(Transform parent, string titleText, string subtitleText)
        {
            RectTransform root = CreateRect($"{titleText}_Header", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 72f;
            Text title = CreateText("Title", root, TextAnchor.UpperLeft, 28, FontStyle.Bold);
            title.text = titleText;
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 18f);
            Text subtitle = CreateText("Subtitle", root, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            subtitle.text = subtitleText;
            subtitle.color = MutedColor;
            subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            subtitle.verticalOverflow = VerticalWrapMode.Overflow;
            subtitle.rectTransform.offsetMax = new Vector2(0f, -38f);
            return root;
        }

        private static RectTransform CreateDivider(Transform parent)
        {
            RectTransform divider = CreateRect("Divider", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            divider.gameObject.AddComponent<LayoutElement>().preferredHeight = 2f;
            Image dividerImage = divider.gameObject.AddComponent<Image>();
            dividerImage.sprite = GetWhiteSprite();
            dividerImage.color = SoftLineColor;
            return divider;
        }

        private static Button CreateActionButton(Transform parent, string name, string labelText, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform root = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 92f;
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = backgroundColor;
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(onClick);
            Text label = CreateText("Label", root, TextAnchor.MiddleCenter, 16, FontStyle.Bold);
            label.text = labelText;
            label.color = HeadingColor;
            return button;
        }

        private static GameObject CreateEmptyState(Transform parent, string message)
        {
            RectTransform root = CreateRect("EmptyState", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            LayoutElement layout = root.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 88f;
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.1f, 0.09f, 0.09f, 0.95f);
            Text label = CreateText("Label", root, TextAnchor.MiddleCenter, 15, FontStyle.Normal);
            label.text = message;
            label.color = BodyColor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.rectTransform.offsetMin = new Vector2(16f, 12f);
            label.rectTransform.offsetMax = new Vector2(-16f, -12f);
            return root.gameObject;
        }

        private static void CreateEmptyMessageCard(Transform parent, string message)
        {
            RectTransform root = CreateRect("EmptyMessage", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.1f, 0.09f, 0.09f, 0.95f);
            Text label = CreateText("Label", root, TextAnchor.MiddleCenter, 15, FontStyle.Normal);
            label.text = message;
            label.color = BodyColor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.rectTransform.offsetMin = new Vector2(16f, 12f);
            label.rectTransform.offsetMax = new Vector2(-16f, -12f);
        }

        private static GameObject CreatePlaceholder(Transform parent, out Text label, int fontSize)
        {
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(parent, false);
            RectTransform rect = placeholder.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = placeholder.AddComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.color = PlaceholderPortraitColor;
            label = CreateText("Initials", rect, TextAnchor.MiddleCenter, fontSize, FontStyle.Bold);
            label.color = new Color(0.94f, 0.9f, 0.76f, 1f);
            return placeholder;
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

        private static void CreateBackdropLayer(RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color, float zRotation)
        {
            RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localRotation = Quaternion.Euler(0f, 0f, zRotation);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.color = color;
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
        private static void LoadSceneIfAvailable(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            Debug.LogWarning($"[SummonSceneController] Scene '{sceneName}' is not loadable.");
        }
    }
}



















