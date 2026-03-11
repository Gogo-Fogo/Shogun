using System;
using System.Collections.Generic;
using Shogun.Core;
using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shogun.Features.UI
{
    [DefaultExecutionOrder(20)]
    public sealed class MainMenuSceneController : MonoBehaviour
    {
        private const string MainMenuSceneName = "MainMenu";
        private const string BattleSceneName = "Dev_Sandbox";
        private const string BarracksSceneName = "Barracks";
        private const string SummonSceneName = "Summon";
        private const string SettingsSceneName = "Settings";
        private const float ContentHorizontalMargin = 28f;
        private const float PhoneContentMaxWidth = 900f;
        private const float TabletContentMaxWidth = 1120f;
        private const float ExpandedContentMaxWidth = 1280f;
        private const float TabletBreakpoint = 1280f;
        private const float ExpandedBreakpoint = 1680f;
        private const int ActionCount = 5;
        private const int StatusCardCount = 4;

        private static readonly string[] FeaturedCharacterIds = { "ryoma", "kuro", "tsukiko" };

        private static readonly Color BackgroundColor = new Color(0.05f, 0.04f, 0.05f, 1f);
        private static readonly Color CrimsonGlowColor = new Color(0.49f, 0.14f, 0.12f, 0.16f);
        private static readonly Color GoldGlowColor = new Color(0.78f, 0.61f, 0.24f, 0.08f);
        private static readonly Color IndigoBandColor = new Color(0.12f, 0.11f, 0.17f, 0.68f);
        private static readonly Color HeroOuterColor = new Color(0.25f, 0.19f, 0.11f, 0.98f);
        private static readonly Color HeroInnerColor = new Color(0.08f, 0.07f, 0.08f, 0.98f);
        private static readonly Color PanelOuterColor = new Color(0.22f, 0.17f, 0.11f, 0.96f);
        private static readonly Color PanelInnerColor = new Color(0.09f, 0.08f, 0.08f, 0.97f);
        private static readonly Color SoftLineColor = new Color(0.81f, 0.68f, 0.34f, 0.34f);
        private static readonly Color HeadingColor = new Color(0.98f, 0.94f, 0.84f, 1f);
        private static readonly Color BodyColor = new Color(0.84f, 0.8f, 0.74f, 1f);
        private static readonly Color MutedColor = new Color(0.68f, 0.65f, 0.61f, 1f);
        private static readonly Color ActionPrimaryColor = new Color(0.46f, 0.31f, 0.12f, 0.98f);
        private static readonly Color ActionSecondaryColor = new Color(0.18f, 0.27f, 0.34f, 0.98f);
        private static readonly Color ActionTertiaryColor = new Color(0.25f, 0.17f, 0.18f, 0.98f);

        private static readonly Color PlaceholderPortraitColor = new Color(0.16f, 0.14f, 0.14f, 1f);
        private static Sprite s_WhiteSprite;
        private static Font s_RuntimeFont;

        private Canvas targetCanvas;
        private RectTransform hostRoot;
        private RectTransform contentFrame;
        private HorizontalLayoutGroup heroRosterLayout;
        private GridLayoutGroup actionGrid;
        private GridLayoutGroup featuredGrid;
        private GridLayoutGroup moduleGrid;
        private LayoutElement actionPanelLayout;
        private LayoutElement featuredPanelLayout;
        private LayoutElement modulePanelLayout;
        private LayoutElement actionGridLayout;
        private LayoutElement featuredGridLayout;
        private LayoutElement moduleGridLayout;
        private readonly List<CharacterDefinition> featuredCharacters = new List<CharacterDefinition>();
        private Vector2Int lastScreenSize = new Vector2Int(-1, -1);
        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private float lastParentWidth = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllerExists()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.name.Equals(MainMenuSceneName, StringComparison.OrdinalIgnoreCase))
                return;
            if (FindFirstObjectByType<MainMenuSceneController>() != null)
                return;
            new GameObject("MainMenuSceneController").AddComponent<MainMenuSceneController>();
        }

        private void Start()
        {
            if (!SceneManager.GetActiveScene().name.Equals(MainMenuSceneName, StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                return;
            }

            ResolveCanvas();
            ResolveFeaturedCharacters();
            BuildScreen();
            ApplyResponsiveLayout(true);
        }

        private void Update() => ApplyResponsiveLayout(false);

        private void ResolveCanvas()
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogWarning("[MainMenuSceneController] No Canvas found. Creating fallback canvas.");
                targetCanvas = CreateFallbackCanvas();
            }

            Transform safeAreaPanel = targetCanvas.transform.Find("UI_SafeAreaPanel") ?? targetCanvas.transform;
            Transform menuRoot = safeAreaPanel.Find("Menu_Main");
            hostRoot = (menuRoot as RectTransform) ?? (safeAreaPanel as RectTransform);
        }

        private void ResolveFeaturedCharacters()
        {
            featuredCharacters.Clear();
            HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < FeaturedCharacterIds.Length; i++)
            {
                CharacterDefinition definition = CharacterFactory.GetCharacterDefinition(FeaturedCharacterIds[i]);
                if (definition == null)
                {
                    Debug.LogWarning($"[MainMenuSceneController] Featured roster entry '{FeaturedCharacterIds[i]}' could not be resolved.");
                    continue;
                }

                if (seenIds.Add(definition.CharacterId))
                    featuredCharacters.Add(definition);
            }
        }

        private void BuildScreen()
        {
            ClearChildren(hostRoot);
            RectTransform screenRoot = CreateRect("MainMenuScreenRoot", hostRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = screenRoot.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = BackgroundColor;

            CreateBackdropLayer(screenRoot, "CrimsonGlow", new Vector2(-220f, 1040f), new Vector2(1440f, 960f), CrimsonGlowColor, 0f);
            CreateBackdropLayer(screenRoot, "GoldGlow", new Vector2(220f, 1320f), new Vector2(1120f, 780f), GoldGlowColor, 0f);
            CreateBackdropLayer(screenRoot, "IndigoBandA", new Vector2(-460f, 720f), new Vector2(1680f, 220f), IndigoBandColor, 9f);
            CreateBackdropLayer(screenRoot, "IndigoBandB", new Vector2(-420f, 1160f), new Vector2(1620f, 180f), new Color(0.1f, 0.09f, 0.15f, 0.48f), -6f);

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

            RectTransform scrollContent = CreateRect("ScrollContent", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            scrollContent.pivot = new Vector2(0.5f, 1f);
            ContentSizeFitter scrollFitter = scrollContent.gameObject.AddComponent<ContentSizeFitter>();
            scrollFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport;
            scrollRect.content = scrollContent;

            contentFrame = CreateRect("ContentFrame", scrollContent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
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

            BuildHeroPanel();
            BuildActionPanel();
            BuildFeaturedPanel();
            BuildOverviewPanel();
            BuildFooterPanel();
        }

        private void BuildHeroPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "HeroPanel", 408f, HeroOuterColor, HeroInnerColor, out RectTransform inner);
            RectTransform sheen = CreateRect("Sheen", inner, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -9f), new Vector2(-12f, -1f));
            Image sheenImage = sheen.gameObject.AddComponent<Image>();
            sheenImage.sprite = GetWhiteSprite();
            sheenImage.color = new Color(0.95f, 0.82f, 0.4f, 0.12f);

            RectTransform content = CreateRect("HeroContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 14f;
            stack.childControlWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            RectTransform titleBlock = CreateRect("TitleBlock", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            titleBlock.gameObject.AddComponent<LayoutElement>().preferredHeight = 168f;
            Text eyebrow = CreateText("Eyebrow", titleBlock, TextAnchor.UpperLeft, 16, FontStyle.Bold);
            eyebrow.text = "THEATER OF WAR";
            eyebrow.color = new Color(0.91f, 0.8f, 0.48f, 1f);
            eyebrow.rectTransform.offsetMin = new Vector2(0f, 132f);
            Text title = CreateText("Title", titleBlock, TextAnchor.MiddleLeft, 58, FontStyle.Bold);
            title.text = "SHOGUN";
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 36f);
            title.rectTransform.offsetMax = new Vector2(0f, -30f);
            Text subtitle = CreateText("Subtitle", titleBlock, TextAnchor.LowerLeft, 22, FontStyle.Bold);
            subtitle.text = "Flowers Fall in Blood";
            subtitle.color = new Color(0.94f, 0.82f, 0.58f, 1f);
            subtitle.rectTransform.offsetMax = new Vector2(0f, -84f);
            Text summary = CreateText("Summary", titleBlock, TextAnchor.LowerLeft, 17, FontStyle.Normal);
            summary.text = "Enter Courtyard Ambush, review the current roster, spend test Spirit Seals, and tune runtime settings from one lacquer-and-gold front door.";
            summary.color = BodyColor;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            summary.verticalOverflow = VerticalWrapMode.Overflow;
            summary.rectTransform.offsetMax = new Vector2(0f, -122f);

            RectTransform chipRow = CreateRect("ChipRow", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            chipRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;
            HorizontalLayoutGroup chipLayout = chipRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            chipLayout.spacing = 10f;
            chipLayout.childControlWidth = false;
            chipLayout.childForceExpandWidth = false;
            CreateChip(chipRow, "COURTYARD AMBUSH", new Color(0.36f, 0.25f, 0.11f, 1f));
            CreateChip(chipRow, "3v3 SLICE", new Color(0.2f, 0.26f, 0.33f, 1f));
            CreateChip(chipRow, "LOCAL COLLECTION", new Color(0.22f, 0.16f, 0.18f, 1f));

            RectTransform heroRow = CreateRect("HeroRoster", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            heroRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 118f;
            heroRosterLayout = heroRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            heroRosterLayout.spacing = 12f;
            heroRosterLayout.childControlWidth = true;
            heroRosterLayout.childControlHeight = true;
            heroRosterLayout.childForceExpandWidth = true;
            heroRosterLayout.childForceExpandHeight = false;
            if (featuredCharacters.Count == 0)
            {
                CreateEmptyMessageCard(heroRow, "Featured slice warriors could not be resolved from the character catalog.");
            }
            else
            {
                for (int i = 0; i < featuredCharacters.Count; i++)
                    CreateHeroCharacterTile(heroRow, featuredCharacters[i]);
            }

            RectTransform note = CreateRect("Note", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            note.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;
            Text noteLabel = CreateText("NoteLabel", note, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            noteLabel.text = "Courtyard Ambush remains the active combat slice, with Barracks, Summon, and Settings feeding the same local runtime state.";
            noteLabel.color = MutedColor;
        }
        private void BuildActionPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "ActionPanel", 330f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            actionPanelLayout = outer.GetComponent<LayoutElement>();

            RectTransform content = CreateRect("ActionContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "DEPLOY", "Launch directly into the current battle, roster, summon, and settings surfaces from here.");
            CreateDivider(content);
            RectTransform gridRoot = CreateRect("ActionGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            actionGridLayout = gridRoot.gameObject.AddComponent<LayoutElement>();
            actionGridLayout.preferredHeight = 210f;
            actionGrid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            actionGrid.spacing = new Vector2(14f, 14f);
            actionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            actionGrid.constraintCount = 1;
            actionGrid.cellSize = new Vector2(280f, 96f);
            CreateActionCard(gridRoot, "Battle", "ENTER COURTYARD AMBUSH", "Load Dev_Sandbox and run the current authored battle slice.", ActionPrimaryColor, () => LoadSceneIfAvailable(BattleSceneName));
            CreateActionCard(gridRoot, "Summon", "OPEN SUMMON GATE", "Reveal characters from the two live test banners and send pulls straight into your Barracks.", ActionSecondaryColor, () => LoadSceneIfAvailable(SummonSceneName));
            CreateActionCard(gridRoot, "Barracks", "OPEN BARRACKS", "Review owned warriors, authored identities, ability text, and collection depth.", new Color(0.23f, 0.24f, 0.16f, 0.98f), () => LoadSceneIfAvailable(BarracksSceneName));
            CreateActionCard(gridRoot, "Settings", "OPEN SETTINGS", "Adjust frame-rate presets, master volume, and saved feedback preferences.", new Color(0.16f, 0.23f, 0.29f, 0.98f), () => LoadSceneIfAvailable(SettingsSceneName));
            CreateActionCard(gridRoot, "Exit", "EXIT SHOGUN", "Quit the application. In editor this just logs and stays in place.", ActionTertiaryColor, QuitApplication);
        }

        private void BuildFeaturedPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "FeaturedPanel", 406f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            featuredPanelLayout = outer.GetComponent<LayoutElement>();

            RectTransform content = CreateRect("FeaturedContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "SLICE VANGUARD", "Ryoma, Kuro, and Tsukiko remain the recommended front line for the first trustworthy Courtyard Ambush pass.");
            CreateDivider(content);
            RectTransform gridRoot = CreateRect("FeaturedGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            featuredGridLayout = gridRoot.gameObject.AddComponent<LayoutElement>();
            featuredGridLayout.preferredHeight = 234f;
            featuredGrid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            featuredGrid.spacing = new Vector2(14f, 14f);
            featuredGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            featuredGrid.constraintCount = 3;
            featuredGrid.cellSize = new Vector2(260f, 158f);
            if (featuredCharacters.Count == 0)
            {
                CreateEmptyMessageCard(gridRoot, "No featured warriors could be resolved for the current slice trio.");
            }
            else
            {
                for (int i = 0; i < featuredCharacters.Count; i++)
                    CreateFeaturedCharacterCard(gridRoot, featuredCharacters[i]);
            }
        }

        private void BuildOverviewPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "OverviewPanel", 344f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            modulePanelLayout = outer.GetComponent<LayoutElement>();

            RectTransform content = CreateRect("OverviewContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            VerticalLayoutGroup stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childControlWidth = true;
            stack.childControlHeight = false;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;

            CreateSectionHeader(content, "WAR TABLE", "Current slice state pulled from the local collection, live banners, and saved runtime settings.");
            CreateDivider(content);
            RectTransform gridRoot = CreateRect("OverviewGridRoot", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            moduleGridLayout = gridRoot.gameObject.AddComponent<LayoutElement>();
            moduleGridLayout.preferredHeight = 248f;
            moduleGrid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            moduleGrid.spacing = new Vector2(14f, 14f);
            moduleGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            moduleGrid.constraintCount = 2;
            moduleGrid.cellSize = new Vector2(260f, 118f);

            IReadOnlyList<CharacterDefinition> ownedDefinitions = TestCollectionService.GetOwnedCharacterDefinitions();
            IReadOnlyList<TestSummonService.TestSummonBanner> liveBanners = TestSummonService.GetBanners();
            GameSettingsSnapshot settings = GameSettingsService.GetSnapshot();
            int featuredSlots = 0;
            for (int i = 0; i < liveBanners.Count; i++)
                featuredSlots += liveBanners[i].FeaturedCharacterIds.Count;

            CreateStatusCard(gridRoot, "ROSTER", $"{ownedDefinitions.Count} OWNED", $"{CountOwnedElements(ownedDefinitions)} elements represented • {TestCollectionService.GetDuplicateCopies()} duplicate pulls banked.", new Color(0.3f, 0.24f, 0.13f, 0.98f));
            CreateStatusCard(gridRoot, "SPIRIT SEALS", $"{TestCollectionService.GetSpiritSeals()} READY", "Enough currency is loaded to stress-test both banners and barracks updates.", new Color(0.17f, 0.27f, 0.35f, 0.98f));
            CreateStatusCard(gridRoot, "SUMMON GATE", $"{liveBanners.Count} ACTIVE", $"{featuredSlots} featured rate-up slots across the current local banners.", new Color(0.25f, 0.18f, 0.2f, 0.98f));
            CreateStatusCard(gridRoot, "RUNTIME", GameSettingsService.GetFrameRateLabel(settings.FrameRateMode), $"VOL {Mathf.RoundToInt(settings.MasterVolume * 100f)} • {(settings.VibrationEnabled ? "HAPTICS ON" : "HAPTICS OFF")} • {(settings.ScreenShakeEnabled ? "SHAKE ON" : "SHAKE OFF")}", new Color(0.16f, 0.22f, 0.28f, 0.98f));
        }

        private void BuildFooterPanel()
        {
            CreatePanel(contentFrame, "FooterPanel", 118f, new Color(0.16f, 0.13f, 0.11f, 0.96f), new Color(0.08f, 0.07f, 0.07f, 0.98f), out RectTransform inner);
            RectTransform content = CreateRect("FooterContent", inner, Vector2.zero, Vector2.one, new Vector2(24f, 18f), new Vector2(-24f, -18f));
            Text footer = CreateText("FooterLabel", content, TextAnchor.MiddleLeft, 15, FontStyle.Normal);
            footer.text = "Use MainMenu as the slice front door: enter combat, review collection state, pull local banners, and adjust runtime settings from one place.";
            footer.color = BodyColor;
            footer.horizontalOverflow = HorizontalWrapMode.Wrap;
            footer.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (contentFrame == null)
                return;

            Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
            Rect safeArea = Screen.safeArea;
            float parentWidth = ((RectTransform)contentFrame.parent).rect.width;
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
            contentFrame.sizeDelta = new Vector2(contentWidth, 0f);
            float sectionWidth = Mathf.Max(280f, contentWidth - 56f);

            if (heroRosterLayout != null)
                heroRosterLayout.spacing = contentWidth >= 960f ? 16f : 12f;

            ConfigureGrid(actionGrid, actionGridLayout, actionPanelLayout, ActionCount, contentWidth >= 1160f ? 3 : contentWidth >= 720f ? 2 : 1, sectionWidth, 220f, 96f, 112f, 32f);
            ConfigureGrid(featuredGrid, featuredGridLayout, featuredPanelLayout, Mathf.Max(featuredCharacters.Count, 1), contentWidth >= 980f ? 3 : contentWidth >= 620f ? 2 : 1, sectionWidth, 220f, 158f, 112f, 32f);
            ConfigureGrid(moduleGrid, moduleGridLayout, modulePanelLayout, StatusCardCount, contentWidth >= 980f ? 4 : contentWidth >= 640f ? 2 : 1, sectionWidth, 180f, 118f, 112f, 32f);

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
        }

        private static int CountOwnedElements(IReadOnlyList<CharacterDefinition> definitions)
        {
            HashSet<ElementalType> elements = new HashSet<ElementalType>();
            for (int i = 0; i < definitions.Count; i++)
                elements.Add(definitions[i].ElementalType);
            return elements.Count;
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

        private static RectTransform CreateSectionHeader(Transform parent, string titleText, string subtitleText)
        {
            RectTransform root = CreateRect($"{titleText}_Header", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 72f;
            Text title = CreateText("Title", root, TextAnchor.UpperLeft, 28, FontStyle.Bold);
            title.text = titleText;
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 18f);
            Text subtitle = CreateText("Subtitle", root, TextAnchor.LowerLeft, 15, FontStyle.Normal);
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

        private static void CreateHeroCharacterTile(Transform parent, CharacterDefinition definition)
        {
            RectTransform root = CreateRect($"Hero_{definition.CharacterId}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            LayoutElement layout = root.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 118f;
            layout.flexibleWidth = 1f;
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.11f, 0.1f, 0.11f, 0.96f);
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);

            Image accent = CreateRect("Accent", root, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(7f, 0f)).gameObject.AddComponent<Image>();
            accent.sprite = GetWhiteSprite();
            accent.color = definition.PaletteAccentColor;

            RectTransform portraitRect = CreateRect("Portrait", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, -38f), new Vector2(86f, 38f));
            Image portraitFrame = portraitRect.gameObject.AddComponent<Image>();
            portraitFrame.sprite = GetWhiteSprite();
            portraitFrame.color = new Color(0.18f, 0.16f, 0.14f, 1f);
            RectTransform portraitInner = CreateRect("PortraitInner", portraitRect, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image portraitImage = portraitInner.gameObject.AddComponent<Image>();
            portraitImage.preserveAspect = true;
            Text placeholderLabel;
            GameObject placeholder = CreatePlaceholder(portraitInner, out placeholderLabel, 24);
            BarracksCharacterPresentation.SetPortraitVisual(definition, portraitImage, placeholder, placeholderLabel, 24);

            RectTransform info = CreateRect("Info", root, Vector2.zero, Vector2.one, new Vector2(102f, 12f), new Vector2(-12f, -12f));
            Text name = CreateText("Name", info, TextAnchor.UpperLeft, 22, FontStyle.Bold);
            name.text = BarracksCharacterPresentation.GetDisplayName(definition).ToUpperInvariant();
            name.color = HeadingColor;
            name.rectTransform.offsetMin = new Vector2(0f, 48f);
            Text identity = CreateText("Identity", info, TextAnchor.MiddleLeft, 14, FontStyle.Bold);
            identity.text = $"{definition.AttackRange.ToString().ToUpperInvariant()} RANGE • {BarracksCharacterPresentation.GetWeaponLabel(definition.MartialArtsType)}";
            identity.color = BodyColor;
            identity.rectTransform.offsetMin = new Vector2(0f, 10f);
            identity.rectTransform.offsetMax = new Vector2(0f, -32f);
            Text element = CreateText("Element", info, TextAnchor.LowerLeft, 13, FontStyle.Normal);
            element.text = BarracksCharacterPresentation.GetElementLabel(definition.ElementalType);
            element.color = MutedColor;
            element.rectTransform.offsetMax = new Vector2(0f, -62f);
        }

        private static void CreateFeaturedCharacterCard(Transform parent, CharacterDefinition definition)
        {
            RectTransform root = CreateRect($"Featured_{definition.CharacterId}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.11f, 0.1f, 0.1f, 0.96f);
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.38f);
            Image accent = CreateRect("Accent", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), Vector2.zero).gameObject.AddComponent<Image>();
            accent.sprite = GetWhiteSprite();
            accent.color = definition.PaletteAccentColor;

            RectTransform portraitRect = CreateRect("Portrait", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, -54f), new Vector2(104f, 38f));
            Image portraitFrame = portraitRect.gameObject.AddComponent<Image>();
            portraitFrame.sprite = GetWhiteSprite();
            portraitFrame.color = new Color(0.18f, 0.15f, 0.14f, 1f);
            RectTransform portraitInner = CreateRect("PortraitInner", portraitRect, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image portraitImage = portraitInner.gameObject.AddComponent<Image>();
            portraitImage.preserveAspect = true;
            Text placeholderLabel;
            GameObject placeholder = CreatePlaceholder(portraitInner, out placeholderLabel, 28);
            BarracksCharacterPresentation.SetPortraitVisual(definition, portraitImage, placeholder, placeholderLabel, 28);

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

        private static void CreateActionCard(Transform parent, string name, string titleText, string summaryText, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform root = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = backgroundColor;
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(onClick);
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);

            RectTransform accent = CreateRect("Accent", root, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(8f, 0f));
            Image accentImage = accent.gameObject.AddComponent<Image>();
            accentImage.sprite = GetWhiteSprite();
            accentImage.color = new Color(0.97f, 0.88f, 0.68f, 0.86f);

            RectTransform content = CreateRect("Content", root, Vector2.zero, Vector2.one, new Vector2(20f, 14f), new Vector2(-18f, -14f));
            Text title = CreateText("Title", content, TextAnchor.UpperLeft, 22, FontStyle.Bold);
            title.text = titleText;
            title.color = HeadingColor;
            title.rectTransform.offsetMin = new Vector2(0f, 26f);
            Text summary = CreateText("Summary", content, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            summary.text = summaryText;
            summary.color = BodyColor;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            summary.verticalOverflow = VerticalWrapMode.Overflow;
            summary.rectTransform.offsetMax = new Vector2(0f, -44f);
        }

        private static void CreateStatusCard(Transform parent, string eyebrowText, string valueText, string summaryText, Color backgroundColor)
        {
            RectTransform root = CreateRect($"Status_{eyebrowText}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = backgroundColor;
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.32f);

            RectTransform accent = CreateRect("Accent", root, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(7f, 0f));
            Image accentImage = accent.gameObject.AddComponent<Image>();
            accentImage.sprite = GetWhiteSprite();
            accentImage.color = new Color(0.97f, 0.88f, 0.68f, 0.84f);

            RectTransform content = CreateRect("Content", root, Vector2.zero, Vector2.one, new Vector2(20f, 12f), new Vector2(-18f, -12f));
            Text eyebrow = CreateText("Eyebrow", content, TextAnchor.UpperLeft, 12, FontStyle.Bold);
            eyebrow.text = eyebrowText;
            eyebrow.color = new Color(0.94f, 0.88f, 0.7f, 1f);
            eyebrow.rectTransform.offsetMin = new Vector2(0f, 62f);

            Text value = CreateText("Value", content, TextAnchor.MiddleLeft, 22, FontStyle.Bold);
            value.text = valueText;
            value.color = HeadingColor;
            value.rectTransform.offsetMin = new Vector2(0f, 18f);
            value.rectTransform.offsetMax = new Vector2(0f, -36f);

            Text summary = CreateText("Summary", content, TextAnchor.LowerLeft, 13, FontStyle.Normal);
            summary.text = summaryText;
            summary.color = BodyColor;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            summary.verticalOverflow = VerticalWrapMode.Overflow;
            summary.rectTransform.offsetMax = new Vector2(0f, -62f);
        }

        private static void CreateEmptyMessageCard(Transform parent, string message)
        {
            RectTransform root = CreateRect("EmptyState", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            LayoutElement layout = root.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 118f;
            layout.flexibleWidth = 1f;
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.1f, 0.09f, 0.09f, 0.95f);
            Text label = CreateText("Message", root, TextAnchor.MiddleCenter, 16, FontStyle.Normal);
            label.text = message;
            label.color = BodyColor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.rectTransform.offsetMin = new Vector2(18f, 16f);
            label.rectTransform.offsetMax = new Vector2(-18f, -16f);
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

        private static RectTransform CreateChip(Transform parent, string labelText, Color backgroundColor)
        {
            RectTransform chip = CreateRect($"Chip_{labelText}", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            LayoutElement layout = chip.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 36f;
            layout.minWidth = 144f;
            Image chipImage = chip.gameObject.AddComponent<Image>();
            chipImage.sprite = GetWhiteSprite();
            chipImage.color = backgroundColor;
            Outline outline = chip.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.3f);
            outline.effectDistance = new Vector2(1f, -1f);
            Text label = CreateText("Label", chip, TextAnchor.MiddleCenter, 13, FontStyle.Bold);
            label.text = labelText;
            label.color = new Color(0.98f, 0.95f, 0.88f, 1f);
            return chip;
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

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
                return;
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private static Canvas CreateFallbackCanvas()
        {
            GameObject canvasGo = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            RectTransform safeAreaRoot = CreateRect("UI_SafeAreaPanel", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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

            Debug.LogWarning($"[MainMenuSceneController] Scene '{sceneName}' is not loadable.");
        }

        private static void QuitApplication()
        {
            if (Application.isEditor)
                Debug.Log("[MainMenuSceneController] Exit requested while running in the editor. Application.Quit() is a no-op here.");
            Application.Quit();
        }
    }
}

