using System;
using System.Collections;
using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Runtime battle HUD for the active combat slice.
    /// Keeps battlefield readable while exposing turn, objective, shared squad HP, and pair-swap state.
    /// </summary>
    [DefaultExecutionOrder(25)]
    public sealed class BattleHudController : MonoBehaviour
    {
        private const float PollIntervalSeconds = 0.12f;
        private const float SpeedNormal = 1f;
        private const float SpeedFast = 2f;
        private const float AutoTurnLeadDelay = 0.24f;
        private const float AutoTurnMovementSpeed = 8f;
        private const float AutoTurnMinTravelTime = 0.08f;
        private const float AutoTurnMaxTravelTime = 0.65f;
        private const float AutoTurnHitPause = 0.14f;
        private const float AutoTurnRecoverDelay = 0.22f;
        private const int DangerSpecialTurnThreshold = 2;
        private const bool EnableDangerIndicators = false;
        private const string MainMenuSceneName = "MainMenu";

        [Header("Dependencies (auto-resolved if left empty)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private RectTransform hudPrefab;

        [Header("Layout")]
        [SerializeField] private float topBarHeight = 108f;
        [SerializeField] private float bottomRailHeight = 116f;

        [Header("Boss Detection")]
        [SerializeField] private string[] bossCharacterIds = Array.Empty<string>();
        [SerializeField] private bool singleEnemyCanBeBoss = true;

        private RectTransform hudRoot;
        private Text turnLabel;
        private Text objectiveLabel;
        private Image objectivePillBackground;
        private RectTransform pairSlotRow;
        private Image squadHpFill;
        private Text squadHpValueLabel;
        private GameObject pauseMenuPanel;
        private Text speedButtonLabel;
        private Text autoButtonLabel;
        private Text menuButtonLabel;

        private readonly List<PlayerSlotView> playerSlots = new List<PlayerSlotView>();
        private readonly Dictionary<CharacterInstance, Action<float>> healthCallbacks = new Dictionary<CharacterInstance, Action<float>>();
        private readonly Dictionary<CharacterInstance, Action> deathCallbacks = new Dictionary<CharacterInstance, Action>();
        private readonly Dictionary<CharacterInstance, TurnCountdownIndicator> turnIndicators = new Dictionary<CharacterInstance, TurnCountdownIndicator>();
        private readonly HashSet<string> normalizedBossIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static Sprite s_CircleSprite;
        private static Sprite s_RingSprite;

        private Coroutine pollCoroutine;
        private Coroutine autoTurnCoroutine;

        private float speedMultiplier = SpeedNormal;
        private bool autoModeEnabled;
        private bool isPaused;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureHudControllerExists()
        {
            if (FindFirstObjectByType<TurnManager>() == null)
                return;

            if (FindFirstObjectByType<BattleHudController>() != null)
                return;

            GameObject go = new GameObject("BattleHudController");
            go.AddComponent<BattleHudController>();
        }

        private IEnumerator Start()
        {
            ResolveDependencies();
            if (turnManager == null)
                yield break;

            yield return WaitForBattleSetup();

            BuildHud();
            BindEvents();
            RebuildPlayerSlots();
            RefreshHud();

            pollCoroutine = StartCoroutine(PollHud());
            TryQueueAutoTurn();
        }

        private void OnDestroy()
        {
            if (pollCoroutine != null)
                StopCoroutine(pollCoroutine);

            if (autoTurnCoroutine != null)
                StopCoroutine(autoTurnCoroutine);

            UnbindEvents();
            UnbindCharacterEvents();
            ClearTurnIndicators();
            Time.timeScale = SpeedNormal;
        }

        private void ResolveDependencies()
        {
            if (turnManager == null)
                turnManager = FindFirstObjectByType<TurnManager>();

            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (targetCanvas == null)
                targetCanvas = FindFirstObjectByType<Canvas>();

            if (targetCanvas == null)
                targetCanvas = CreateFallbackCanvas();
        }

        private IEnumerator WaitForBattleSetup()
        {
            const float timeout = 5f;
            float remaining = timeout;

            while (remaining > 0f)
            {
                if (turnManager != null
                    && turnManager.GetPlayerCombatants().Count > 0
                    && turnManager.IsBattleActive
                    && turnManager.GetCurrentCombatant() != null)
                {
                    break;
                }

                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private IEnumerator PollHud()
        {
            WaitForSecondsRealtime wait = new WaitForSecondsRealtime(PollIntervalSeconds);
            while (true)
            {
                RefreshHud();
                yield return wait;
            }
        }

        private void BindEvents()
        {
            if (turnManager != null)
            {
                turnManager.OnTurnStarted += HandleTurnChanged;
                turnManager.OnTurnEnded += HandleTurnChanged;
                turnManager.OnBattleEnded += HandleBattleEnded;
                turnManager.OnTurnOrderChanged += HandleTurnOrderChanged;
            }

            if (battleManager != null)
                battleManager.OnPlayerFormationChanged += HandlePlayerFormationChanged;
        }

        private void UnbindEvents()
        {
            if (turnManager != null)
            {
                turnManager.OnTurnStarted -= HandleTurnChanged;
                turnManager.OnTurnEnded -= HandleTurnChanged;
                turnManager.OnBattleEnded -= HandleBattleEnded;
                turnManager.OnTurnOrderChanged -= HandleTurnOrderChanged;
            }

            if (battleManager != null)
                battleManager.OnPlayerFormationChanged -= HandlePlayerFormationChanged;
        }

        private void HandleTurnChanged(CharacterInstance _)
        {
            RefreshHud();
            TryQueueAutoTurn();
        }

        private void HandleTurnOrderChanged()
        {
            RefreshHud();
            TryQueueAutoTurn();
        }

        private void HandlePlayerFormationChanged(int _, CharacterInstance __, CharacterInstance ___)
        {
            RebuildPlayerSlots();
            RefreshHud();
            TryQueueAutoTurn();
        }

        private void HandleBattleEnded(BattleResult result)
        {
            if (objectiveLabel != null)
                objectiveLabel.text = result == BattleResult.Win ? "VICTORY" : "DEFEAT";

            if (autoTurnCoroutine != null)
            {
                StopCoroutine(autoTurnCoroutine);
                autoTurnCoroutine = null;
            }

            autoModeEnabled = false;
            SetPauseMenuOpen(false);
            Time.timeScale = SpeedNormal;
            ClearTurnIndicators();
            RefreshHud();
            UpdateUtilityLabels();
        }

        private void BuildHud()
        {
            if (targetCanvas == null || hudRoot != null)
                return;

            hudRoot = hudPrefab != null
                ? Instantiate(hudPrefab, targetCanvas.transform, false)
                : CreateRect("BattleHudRoot", targetCanvas.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            hudRoot.name = "BattleHudRoot";
            BuildTopBar();
            BuildBottomRail();
            BuildPauseMenu();
            UpdateUtilityLabels();
        }
        private void BuildTopBar()
        {
            RectTransform existing = hudRoot.Find("TopCombatBar") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            RectTransform topBar = CreateRect("TopCombatBar", hudRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -topBarHeight - 12f), new Vector2(-20f, -12f));
            Image topBarBg = topBar.gameObject.AddComponent<Image>();
            topBarBg.color = new Color(0.05f, 0.07f, 0.1f, 0.62f);
            topBarBg.raycastTarget = false;

            HorizontalLayoutGroup topLayout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            topLayout.padding = new RectOffset(14, 14, 10, 10);
            topLayout.spacing = 10f;
            topLayout.childForceExpandWidth = true;
            topLayout.childForceExpandHeight = true;

            turnLabel = CreatePillLabel(topBar, "TurnPill", TextAnchor.MiddleLeft);
            objectiveLabel = CreatePillLabel(topBar, "ObjectivePill", TextAnchor.MiddleCenter, out objectivePillBackground);
            BuildUtilityControls(topBar);
        }

        private void BuildBottomRail()
        {
            RectTransform existing = hudRoot.Find("BottomSquadRail") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            // Low-profile rail anchored to the bottom edge.
            RectTransform bottomRail = CreateRect("BottomSquadRail", hudRoot,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, bottomRailHeight));

            Image bottomBg = bottomRail.gameObject.AddComponent<Image>();
            bottomBg.color = new Color(0.06f, 0.05f, 0.04f, 0.78f);
            bottomBg.raycastTarget = false;

            HorizontalLayoutGroup layout = bottomRail.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 8, 8);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // The rail itself serves as the medallion row.
            pairSlotRow = bottomRail;
        }


        private void BuildPauseMenu()
        {
            if (hudRoot == null || pauseMenuPanel != null)
                return;

            RectTransform overlay = CreateRect("PauseMenuPanel", hudRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            pauseMenuPanel = overlay.gameObject;

            Image overlayImage = pauseMenuPanel.AddComponent<Image>();
            overlayImage.color = new Color(0.01f, 0.02f, 0.03f, 0.74f);
            overlayImage.raycastTarget = true;

            RectTransform card = CreateRect("PauseMenuCard", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220f, -180f), new Vector2(220f, 180f));
            Image cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.12f, 0.11f, 0.08f, 0.96f);
            cardImage.raycastTarget = true;

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

            Text title = CreateText("Title", card, TextAnchor.MiddleCenter, 30, FontStyle.Bold);
            title.text = "BATTLE MENU";
            title.color = new Color(0.98f, 0.94f, 0.8f, 1f);
            LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 42f;

            Text subtitle = CreateText("Subtitle", card, TextAnchor.MiddleCenter, 16, FontStyle.Normal);
            subtitle.text = "Pause, restart, or return to the main menu.";
            subtitle.color = new Color(0.8f, 0.77f, 0.68f, 1f);
            LayoutElement subtitleLayout = subtitle.gameObject.AddComponent<LayoutElement>();
            subtitleLayout.preferredHeight = 28f;

            CreatePauseMenuButton(card, "ResumeButton", "RESUME", new Color(0.28f, 0.52f, 0.36f, 0.96f), OnResumeButtonPressed);
            CreatePauseMenuButton(card, "RestartButton", "RESTART", new Color(0.42f, 0.3f, 0.16f, 0.96f), OnRestartButtonPressed);
            CreatePauseMenuButton(card, "MainMenuButton", "MAIN MENU", new Color(0.32f, 0.18f, 0.16f, 0.96f), OnMainMenuButtonPressed);

            pauseMenuPanel.SetActive(false);
        }

        private static Button CreatePauseMenuButton(Transform parent, string name, string label, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform buttonRect = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement layout = buttonRect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 56f;
            layout.minHeight = 56f;

            Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
            buttonImage.color = backgroundColor;

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(onClick);

            Text labelText = CreateText("Label", buttonRect, TextAnchor.MiddleCenter, 22, FontStyle.Bold);
            labelText.text = label;
            labelText.color = new Color(0.98f, 0.94f, 0.82f, 1f);
            return button;
        }
        private Text CreatePillLabel(Transform parent, string name, TextAnchor alignment)
        {
            return CreatePillLabel(parent, name, alignment, out _);
        }

        private Text CreatePillLabel(Transform parent, string name, TextAnchor alignment, out Image backgroundImage)
        {
            RectTransform pill = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement layoutElement = pill.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80f;
            layoutElement.minWidth = 220f;

            Image bg = pill.gameObject.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.13f, 0.11f, 0.85f);
            bg.raycastTarget = false;
            backgroundImage = bg;

            Text label = CreateText("Label", pill, alignment, 26, FontStyle.Bold);
            label.text = "-";
            label.color = new Color(0.96f, 0.93f, 0.82f, 1f);
            return label;
        }

        private void BuildUtilityControls(Transform parent)
        {
            RectTransform utilityRoot = CreateRect("UtilityPill", parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement layoutElement = utilityRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80f;
            layoutElement.minWidth = 320f;

            Image bg = utilityRoot.gameObject.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.13f, 0.11f, 0.85f);
            bg.raycastTarget = false;

            HorizontalLayoutGroup layout = utilityRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            speedButtonLabel = CreateUtilityButton(utilityRoot, "SpeedButton", OnSpeedButtonPressed);
            autoButtonLabel = CreateUtilityButton(utilityRoot, "AutoButton", OnAutoButtonPressed);
            menuButtonLabel = CreateUtilityButton(utilityRoot, "MenuButton", OnMenuButtonPressed);
        }

        private static Text CreateUtilityButton(Transform parent, string name, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform buttonRect = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.2f, 0.12f, 0.9f);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(onClick);

            Text label = CreateText("Label", buttonRect, TextAnchor.MiddleCenter, 20, FontStyle.Bold);
            label.color = new Color(0.98f, 0.93f, 0.75f, 1f);
            return label;
        }

        private void RebuildPlayerSlots()
        {
            if (pairSlotRow == null)
                return;

            ClearChildren(pairSlotRow);
            playerSlots.Clear();
            UnbindCharacterEvents();

            int laneCount = 0;
            if (battleManager != null)
                laneCount = Mathf.Max(3, battleManager.GetPlayerLaneCount());
            else if (turnManager != null)
                laneCount = Mathf.Max(3, turnManager.GetPlayerCombatants().Count);

            for (int i = 0; i < laneCount; i++)
                playerSlots.Add(CreatePlayerSlot(i));

            BindRosterCharacterEvents();
        }

        // Creates a circular portrait medallion for one player lane.
        private PlayerSlotView CreatePlayerSlot(int laneIndex)
        {
            const float diameter  = 76f;   // inner portrait circle
            const float ringSize  = 88f;   // outer arc/ring
            const float hpBarH    = 8f;
            const float badgeSize = 34f;
            float slotW = ringSize + 8f;   // 96px
            float slotH = ringSize + hpBarH + 10f; // 106px

            // Slot root — LayoutElement tells HorizontalLayoutGroup the preferred size.
            RectTransform slotRoot = CreateRect($"Medallion_{laneIndex}", pairSlotRow,
                new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, Vector2.zero);

            LayoutElement le = slotRoot.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth  = slotW;
            le.preferredHeight = slotH;
            le.minWidth        = slotW;

            Sprite circleSpr = GetCircleSprite();
            Sprite ringSpr   = GetRingSprite();

            // Circle area — top of slot, horizontally centered.
            float circleX = (slotW - ringSize) * 0.5f;   // 4px each side
            RectTransform circleArea = CreateRect("CircleArea", slotRoot,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(circleX, -ringSize), new Vector2(circleX + ringSize, 0f));

            // 1. Arc background ring — always visible, dim gray.
            Image arcBg = circleArea.gameObject.AddComponent<Image>();
            arcBg.sprite        = ringSpr;
            arcBg.color         = new Color(0.28f, 0.28f, 0.28f, 0.5f);
            arcBg.raycastTarget = false;

            // 2. Ability charge arc fill — grows clockwise as charge builds.
            RectTransform arcFillRect = CreateRect("AbilityArc", circleArea,
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image arcFill = arcFillRect.gameObject.AddComponent<Image>();
            arcFill.sprite        = ringSpr;
            arcFill.type          = Image.Type.Filled;
            arcFill.fillMethod    = Image.FillMethod.Radial360;
            arcFill.fillOrigin    = (int)Image.Origin360.Top;
            arcFill.fillAmount    = 0f;
            arcFill.color         = new Color(0.96f, 0.82f, 0.28f, 1f);
            arcFill.raycastTarget = false;

            // 3. Portrait mask — clips the battle sprite to a circle.
            float inset = (ringSize - diameter) * 0.5f;   // 6px
            RectTransform maskRect = CreateRect("PortraitMask", circleArea,
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(inset, inset), new Vector2(-inset, -inset));
            Image maskImage = maskRect.gameObject.AddComponent<Image>();
            maskImage.sprite        = circleSpr;
            maskImage.raycastTarget = false;
            Mask maskComp = maskRect.gameObject.AddComponent<Mask>();
            maskComp.showMaskGraphic = false;

            RectTransform portraitRect = CreateRect("Portrait", maskRect,
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image portraitImage = portraitRect.gameObject.AddComponent<Image>();
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget  = false;

            // 4. Dead overlay — darkens the portrait when the unit is KO'd.
            RectTransform deadRect = CreateRect("DeadOverlay", circleArea,
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(inset, inset), new Vector2(-inset, -inset));
            Image deadOverlay = deadRect.gameObject.AddComponent<Image>();
            deadOverlay.sprite        = circleSpr;
            deadOverlay.color         = new Color(0.08f, 0.05f, 0.05f, 0f);
            deadOverlay.raycastTarget = false;

            // 5. Active highlight ring — glows with the character's accent on their turn.
            RectTransform activeRingRect = CreateRect("ActiveRing", circleArea,
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image activeRing = activeRingRect.gameObject.AddComponent<Image>();
            activeRing.sprite        = ringSpr;
            activeRing.color         = new Color(1f, 1f, 1f, 0f);
            activeRing.raycastTarget = false;

            // HP bar — slim bar anchored to the bottom of the slot.
            RectTransform hpBgRect = CreateRect("HpBarBg", slotRoot,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(4f, 2f), new Vector2(-4f, hpBarH + 2f));
            Image hpBg = hpBgRect.gameObject.AddComponent<Image>();
            hpBg.color        = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            hpBg.raycastTarget = false;

            RectTransform hpFillRect = CreateRect("HpFill", hpBgRect,
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-1f, -1f));
            Image hpFill = hpFillRect.gameObject.AddComponent<Image>();
            hpFill.type          = Image.Type.Filled;
            hpFill.fillMethod    = Image.FillMethod.Horizontal;
            hpFill.fillAmount    = 1f;
            hpFill.color         = new Color(0.2f, 0.92f, 0.38f, 0.98f);
            hpFill.raycastTarget = false;

            // Reserve swap badge — small circle at bottom-right, above the HP bar.
            RectTransform swapRect = CreateRect("SwapBadge", slotRoot,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-badgeSize - 2f, hpBarH + 6f), new Vector2(-2f, hpBarH + 6f + badgeSize));
            Image swapBg = swapRect.gameObject.AddComponent<Image>();
            swapBg.sprite = circleSpr;
            swapBg.color  = new Color(0.22f, 0.18f, 0.12f, 0.88f);
            Button swapButton = swapRect.gameObject.AddComponent<Button>();
            swapButton.targetGraphic = swapBg;
            swapButton.onClick.AddListener(() => OnReserveButtonPressed(laneIndex));

            RectTransform swapPortraitRect = CreateRect("Portrait", swapRect,
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image swapPortrait = swapPortraitRect.gameObject.AddComponent<Image>();
            swapPortrait.preserveAspect = true;
            swapPortrait.raycastTarget  = false;

            swapRect.gameObject.SetActive(false);

            return new PlayerSlotView
            {
                LaneIndex              = laneIndex,
                Root                   = slotRoot,
                FrontPortrait          = portraitImage,
                FrontHpFill            = hpFill,
                ActiveRing             = activeRing,
                AbilityArcFill         = arcFill,
                DeadOverlay            = deadOverlay,
                ReserveButton          = swapButton,
                ReserveButtonBackground = swapBg,
                ReservePortrait        = swapPortrait,
            };
        }

        // Generates a filled-circle sprite at runtime (used for portrait masking and overlays).
        private static Sprite GetCircleSprite()
        {
            if (s_CircleSprite != null)
                return s_CircleSprite;

            const int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float c = res / 2f;
            float r = c - 0.5f;
            for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                    tex.SetPixel(x, y, (new Vector2(x + 0.5f, y + 0.5f) - new Vector2(c, c)).magnitude <= r ? Color.white : Color.clear);
            tex.Apply();
            s_CircleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
            return s_CircleSprite;
        }

        // Generates a donut/ring sprite at runtime (used for the ability charge arc and active highlight).
        private static Sprite GetRingSprite()
        {
            if (s_RingSprite != null)
                return s_RingSprite;

            const int   res        = 64;
            const float innerRatio = 0.72f;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float c      = res / 2f;
            float outerR = c - 0.5f;
            float innerR = outerR * innerRatio;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float d = (new Vector2(x + 0.5f, y + 0.5f) - new Vector2(c, c)).magnitude;
                    tex.SetPixel(x, y, (d <= outerR && d >= innerR) ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            s_RingSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
            return s_RingSprite;
        }

        private static Text CreateText(string name, Transform parent, TextAnchor alignment, int fontSize, FontStyle style)
        {
            RectTransform rt = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Text text = rt.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            text.alignment = alignment;
            text.fontStyle = style;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.raycastTarget = false;

            Shadow shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(1.4f, -1.4f);

            return text;
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
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private void BindRosterCharacterEvents()
        {
            List<CharacterInstance> roster = ResolvePlayerRosterCharacters();
            for (int i = 0; i < roster.Count; i++)
                BindCharacterEvents(roster[i]);
        }

        private void BindCharacterEvents(CharacterInstance character)
        {
            if (character == null || healthCallbacks.ContainsKey(character))
                return;

            Action<float> healthCallback = _ => RefreshHud();
            Action deathCallback = RefreshHud;

            healthCallbacks[character] = healthCallback;
            deathCallbacks[character] = deathCallback;

            character.OnHealthChanged += healthCallback;
            character.OnDeath += deathCallback;
        }

        private void UnbindCharacterEvents()
        {
            foreach (KeyValuePair<CharacterInstance, Action<float>> pair in healthCallbacks)
            {
                if (pair.Key != null)
                    pair.Key.OnHealthChanged -= pair.Value;
            }

            foreach (KeyValuePair<CharacterInstance, Action> pair in deathCallbacks)
            {
                if (pair.Key != null)
                    pair.Key.OnDeath -= pair.Value;
            }

            healthCallbacks.Clear();
            deathCallbacks.Clear();
        }

        private void RefreshHud()
        {
            RefreshTopBar();
            RefreshSquadHealthBar();
            RefreshSlots();
            RefreshTurnIndicators();
        }

        private void RefreshSquadHealthBar()
        {
            if (squadHpFill == null || squadHpValueLabel == null)
                return;

            float totalCurrent = 0f;
            float totalMax = 0f;

            if (battleManager != null)
            {
                totalCurrent = battleManager.GetTotalPlayerHealth();
                totalMax = battleManager.GetTotalPlayerMaxHealth();
            }
            else if (turnManager != null)
            {
                IReadOnlyList<CharacterInstance> players = turnManager.GetPlayerCombatants();
                for (int i = 0; i < players.Count; i++)
                {
                    CharacterInstance player = players[i];
                    if (player == null)
                        continue;

                    totalCurrent += Mathf.Max(0f, player.CurrentHealth);
                    totalMax += Mathf.Max(0f, player.MaxHealth);
                }
            }

            float maxHealth = Mathf.Max(1f, totalMax);
            float normalized = Mathf.Clamp01(totalCurrent / maxHealth);
            squadHpFill.fillAmount = normalized;
            squadHpFill.color = Color.Lerp(new Color(0.89f, 0.28f, 0.22f, 1f), new Color(0.24f, 0.92f, 0.38f, 1f), normalized);
            squadHpValueLabel.text = $"{Mathf.CeilToInt(totalCurrent)}/{Mathf.CeilToInt(maxHealth)}";
        }

        private void RefreshTurnIndicators()
        {
            if (turnManager == null)
                return;

            IReadOnlyList<CharacterInstance> combatants = turnManager.turnOrder;
            HashSet<CharacterInstance> activeCombatants = new HashSet<CharacterInstance>();

            if (combatants != null)
            {
                for (int i = 0; i < combatants.Count; i++)
                {
                    CharacterInstance combatant = combatants[i];
                    if (combatant == null || !combatant.IsAlive)
                        continue;

                    activeCombatants.Add(combatant);

                    TurnCountdownIndicator indicator = EnsureTurnIndicator(combatant);
                    if (indicator == null)
                        continue;

                    int turnsUntil = turnManager.GetTurnsUntilTurn(combatant);
                    bool isCurrentTurn = turnsUntil == 0;
                    bool showDanger = ShouldShowDangerIndicator(combatant, turnsUntil);
                    indicator.RefreshState(turnsUntil, showDanger, isCurrentTurn);
                }
            }

            if (turnIndicators.Count == 0)
                return;

            List<CharacterInstance> staleCombatants = null;
            foreach (KeyValuePair<CharacterInstance, TurnCountdownIndicator> pair in turnIndicators)
            {
                if (pair.Key == null || !activeCombatants.Contains(pair.Key))
                {
                    staleCombatants ??= new List<CharacterInstance>();
                    staleCombatants.Add(pair.Key);
                }
            }

            if (staleCombatants == null)
                return;

            for (int i = 0; i < staleCombatants.Count; i++)
                ClearTurnIndicator(staleCombatants[i]);
        }

        private TurnCountdownIndicator EnsureTurnIndicator(CharacterInstance combatant)
        {
            if (combatant == null)
                return null;

            if (turnIndicators.TryGetValue(combatant, out TurnCountdownIndicator existing) && existing != null)
                return existing;

            GameObject indicatorObject = new GameObject($"{ResolveCharacterName(combatant)}_TurnIndicator");
            indicatorObject.transform.SetParent(transform, false);

            TurnCountdownIndicator indicator = indicatorObject.AddComponent<TurnCountdownIndicator>();
            indicator.Initialize(combatant, turnManager != null && turnManager.IsEnemyUnit(combatant));
            turnIndicators[combatant] = indicator;
            return indicator;
        }
        private bool ShouldShowDangerIndicator(CharacterInstance combatant, int turnsUntil)
        {
            return EnableDangerIndicators
                   && combatant != null
                   && turnsUntil > 0
                   && turnsUntil <= DangerSpecialTurnThreshold
                   && turnManager != null
                   && turnManager.IsEnemyUnit(combatant)
                   && combatant.CanUseSpecialAbility
                   && combatant.Definition != null
                   && !string.IsNullOrWhiteSpace(combatant.Definition.SpecialAbilityName);
        }

        private void ClearTurnIndicator(CharacterInstance combatant)
        {
            if (!turnIndicators.TryGetValue(combatant, out TurnCountdownIndicator indicator))
                return;

            if (indicator != null)
                Destroy(indicator.gameObject);

            turnIndicators.Remove(combatant);
        }

        private void ClearTurnIndicators()
        {
            foreach (KeyValuePair<CharacterInstance, TurnCountdownIndicator> pair in turnIndicators)
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
            }

            turnIndicators.Clear();
        }

        private void RefreshTopBar()
        {
            if (turnLabel == null || objectiveLabel == null)
                return;

            CharacterInstance current = turnManager != null ? turnManager.GetCurrentCombatant() : null;
            string currentName = current != null ? ResolveCharacterName(current) : "-";
            turnLabel.text = $"TURN: {currentName}";

            int aliveEnemies = 0;
            int totalEnemies = 0;
            IReadOnlyList<CharacterInstance> enemies = turnManager != null ? turnManager.GetEnemyCombatants() : null;
            if (enemies != null)
            {
                totalEnemies = enemies.Count;
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] != null && enemies[i].IsAlive)
                        aliveEnemies++;
                }
            }

            if (turnManager != null && turnManager.IsBattleActive)
            {
                if (TryGetBossEnemy(out CharacterInstance bossEnemy))
                {
                    float bossMaxHealth = Mathf.Max(1f, bossEnemy.MaxHealth);
                    float bossHealth = Mathf.Max(0f, bossEnemy.CurrentHealth);
                    objectiveLabel.text = $"BOSS {ResolveCharacterName(bossEnemy)}  HP {Mathf.CeilToInt(bossHealth)}/{Mathf.CeilToInt(bossMaxHealth)}";
                    if (objectivePillBackground != null)
                        objectivePillBackground.color = new Color(0.32f, 0.12f, 0.1f, 0.93f);
                }
                else
                {
                    objectiveLabel.text = $"DEFEAT ALL ENEMIES  {aliveEnemies}/{Mathf.Max(1, totalEnemies)}";
                    if (objectivePillBackground != null)
                        objectivePillBackground.color = new Color(0.14f, 0.13f, 0.11f, 0.85f);
                }
            }

            UpdateUtilityLabels();
        }

        private void RefreshSlots()
        {
            int expectedCount = battleManager != null ? battleManager.GetPlayerLaneCount() : playerSlots.Count;
            if (expectedCount != playerSlots.Count)
                RebuildPlayerSlots();

            for (int i = 0; i < playerSlots.Count; i++)
                RefreshSlot(playerSlots[i]);
        }

        private void RefreshSlot(PlayerSlotView slot)
        {
            if (slot == null)
                return;

            CharacterInstance front   = battleManager != null ? battleManager.GetActivePlayerAtLane(slot.LaneIndex) : null;
            CharacterInstance reserve = battleManager != null ? battleManager.GetReservePlayerAtLane(slot.LaneIndex) : null;
            CharacterInstance current = turnManager   != null ? turnManager.GetCurrentCombatant() : null;

            bool currentPlayerTurn = current != null && turnManager != null && turnManager.IsPlayerUnit(current);
            bool isCurrentLane     = currentPlayerTurn && battleManager != null &&
                                     battleManager.GetPlayerLaneForCharacter(current) == slot.LaneIndex;
            bool canSwap           = currentPlayerTurn && !isPaused && !autoModeEnabled &&
                                     reserve != null && reserve.IsAlive && battleManager != null;

            if (front == null)
            {
                slot.FrontPortrait.sprite      = null;
                slot.FrontPortrait.color       = new Color(1f, 1f, 1f, 0f);
                slot.FrontHpFill.fillAmount    = 0f;
                slot.ActiveRing.color          = new Color(1f, 1f, 1f, 0f);
                slot.AbilityArcFill.fillAmount = 0f;
                slot.DeadOverlay.color         = new Color(0.08f, 0.05f, 0.05f, 0.5f);
                slot.Root.localScale           = Vector3.one;
            }
            else
            {
                float maxHealth        = Mathf.Max(1f, front.MaxHealth);
                float healthNormalized = Mathf.Clamp01(front.CurrentHealth / maxHealth);
                Color accent           = front.Definition != null
                    ? front.Definition.PaletteAccentColor
                    : new Color(0.45f, 0.82f, 0.96f, 1f);
                accent.a = 1f;
                float pulse01    = 0.5f + (0.5f * Mathf.Sin(Time.unscaledTime * 5.5f));
                bool specialReady = front.IsAlive && front.CanUseSpecialAbility;

                // Portrait.
                slot.FrontPortrait.sprite = ResolvePortrait(front);
                slot.FrontPortrait.color  = front.IsAlive ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.9f);

                // HP bar.
                slot.FrontHpFill.fillAmount = healthNormalized;
                slot.FrontHpFill.color      = Color.Lerp(
                    new Color(0.88f, 0.25f, 0.22f, 1f),
                    new Color(0.2f, 0.92f, 0.38f, 1f),
                    healthNormalized);

                // Ability charge arc — gold when ready, dim blue while charging.
                slot.AbilityArcFill.fillAmount = front.IsAlive ? (specialReady ? 1f : 0.35f) : 0f;
                slot.AbilityArcFill.color      = specialReady
                    ? new Color(0.96f, 0.82f, 0.28f, 1f)
                    : new Color(0.4f, 0.5f, 0.62f, 0.5f);

                // Active highlight ring — glows with character accent on their turn.
                slot.ActiveRing.color = isCurrentLane
                    ? new Color(accent.r, accent.g, accent.b, Mathf.Lerp(0.65f, 0.9f, pulse01))
                    : new Color(1f, 1f, 1f, 0f);

                // Dead overlay — dims portrait for KO'd units.
                slot.DeadOverlay.color = front.IsAlive
                    ? new Color(0.08f, 0.05f, 0.05f, 0f)
                    : new Color(0.08f, 0.05f, 0.05f, 0.52f);

                // Scale pulse when it's this lane's turn.
                slot.Root.localScale = isCurrentLane
                    ? new Vector3(1.08f + (0.02f * pulse01), 1.08f + (0.02f * pulse01), 1f)
                    : Vector3.one;
            }

            // Reserve swap badge.
            if (reserve != null)
            {
                slot.ReserveButton.gameObject.SetActive(true);
                slot.ReserveButton.interactable = canSwap;
                slot.ReservePortrait.sprite     = ResolvePortrait(reserve);
                slot.ReservePortrait.color      = reserve.IsAlive ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.75f);
                if (slot.ReserveButtonBackground != null)
                {
                    slot.ReserveButtonBackground.color = !reserve.IsAlive
                        ? new Color(0.2f, 0.12f, 0.12f, 0.7f)
                        : canSwap
                            ? new Color(0.34f, 0.24f, 0.12f, 0.96f)
                            : new Color(0.22f, 0.18f, 0.12f, 0.88f);
                }
            }
            else
            {
                slot.ReserveButton.gameObject.SetActive(false);
            }
        }

        private void OnReserveButtonPressed(int laneIndex)
        {
            if (battleManager == null || turnManager == null || isPaused || autoModeEnabled)
                return;

            CharacterInstance current = turnManager.GetCurrentCombatant();
            if (current == null || !turnManager.IsPlayerUnit(current))
                return;

            battleManager.TrySwapPlayerLane(laneIndex, out _, out _);
        }

        private void OnSpeedButtonPressed()
        {
            speedMultiplier = Mathf.Approximately(speedMultiplier, SpeedFast) ? SpeedNormal : SpeedFast;
            ApplyTimeScale();
            UpdateUtilityLabels();
        }

        private void OnAutoButtonPressed()
        {
            autoModeEnabled = !autoModeEnabled;
            UpdateUtilityLabels();
            TryQueueAutoTurn();
        }

        private void OnMenuButtonPressed()
        {
            SetPauseMenuOpen(!isPaused);
        }

        private void OnResumeButtonPressed()
        {
            SetPauseMenuOpen(false);
        }

        private void OnRestartButtonPressed()
        {
            Time.timeScale = SpeedNormal;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        private void OnMainMenuButtonPressed()
        {
            Time.timeScale = SpeedNormal;
            if (Application.CanStreamedLevelBeLoaded(MainMenuSceneName))
            {
                SceneManager.LoadScene(MainMenuSceneName);
                return;
            }

            Debug.LogWarning($"[BattleHudController] Scene '{MainMenuSceneName}' is not loadable. Keeping the current battle scene active.");
            SetPauseMenuOpen(false);
        }

        private void SetPauseMenuOpen(bool isOpen)
        {
            isPaused = isOpen;
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(isOpen);

            ApplyTimeScale();
            UpdateUtilityLabels();

            if (!isOpen)
                TryQueueAutoTurn();
        }

        private void ApplyTimeScale()
        {
            Time.timeScale = isPaused ? 0f : Mathf.Max(SpeedNormal, speedMultiplier);
        }

        private void UpdateUtilityLabels()
        {
            if (speedButtonLabel != null)
                speedButtonLabel.text = $"SPD x{Mathf.RoundToInt(speedMultiplier)}";

            if (autoButtonLabel != null)
                autoButtonLabel.text = autoModeEnabled ? "AUTO ON" : "AUTO OFF";

            if (menuButtonLabel != null)
                menuButtonLabel.text = "MENU";
        }

        private void TryQueueAutoTurn()
        {
            if (!autoModeEnabled || isPaused || turnManager == null || !turnManager.IsBattleActive)
                return;

            CharacterInstance current = turnManager.GetCurrentCombatant();
            if (current == null || !current.IsAlive || !turnManager.IsPlayerUnit(current))
                return;

            if (autoTurnCoroutine != null)
                StopCoroutine(autoTurnCoroutine);

            autoTurnCoroutine = StartCoroutine(ResolveAutoPlayerTurn(current));
        }

        private IEnumerator ResolveAutoPlayerTurn(CharacterInstance attacker)
        {
            yield return new WaitForSeconds(AutoTurnLeadDelay);

            if (!IsCurrentPlayerTurn(attacker))
            {
                autoTurnCoroutine = null;
                yield break;
            }

            CharacterInstance target = FindClosestAliveEnemy(attacker);
            if (target == null)
            {
                if (turnManager != null && turnManager.IsBattleActive && turnManager.GetCurrentCombatant() == attacker)
                    turnManager.EndTurn();

                autoTurnCoroutine = null;
                yield break;
            }

            Animator attackerAnimator = attacker.GetComponentInChildren<Animator>();

            attacker.StopMovement();
            CombatMovementUtility.FaceCharacterTowards(attacker, target);

            Vector3 strikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(attacker, target, GetActiveCombatantsExcept(attacker, target));
            float travelDist = Vector2.Distance(
                (Vector2)CombatMovementUtility.GetWorldPosition(attacker.transform),
                (Vector2)strikeWorldPos);
            float dynamicTravelTime = Mathf.Clamp(travelDist / Mathf.Max(0.1f, AutoTurnMovementSpeed), AutoTurnMinTravelTime, AutoTurnMaxTravelTime);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", true);

            yield return CombatMovementUtility.MoveCharacterToWorldPosition(attacker.transform, strikeWorldPos, dynamicTravelTime);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", false);

            if (!CombatMovementUtility.IsTargetWithinAttackRange(attacker, target))
            {
                Vector3 correctedStrikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(attacker, target);
                float correctionDistance = Vector2.Distance(
                    (Vector2)CombatMovementUtility.GetWorldPosition(attacker.transform),
                    (Vector2)correctedStrikeWorldPos);

                if (correctionDistance > 0.05f)
                {
                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", true);

                    float correctionTravelTime = Mathf.Clamp(correctionDistance / Mathf.Max(0.1f, AutoTurnMovementSpeed), AutoTurnMinTravelTime, AutoTurnMaxTravelTime);
                    yield return CombatMovementUtility.MoveCharacterToWorldPosition(attacker.transform, correctedStrikeWorldPos, correctionTravelTime);

                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", false);
                }
            }

            if (attacker.CanAttack && CombatMovementUtility.IsTargetWithinAttackRange(attacker, target))
            {
                attacker.PerformBasicAttack();
                yield return new WaitForSeconds(AutoTurnHitPause);

                float damage = attacker.CalculateDamageAgainst(target);
                target.TakeDamage(damage);
                BattleFloatingText.SpawnDamage(target, damage);
            }

            yield return new WaitForSeconds(AutoTurnRecoverDelay);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", false);

            if (turnManager != null && turnManager.IsBattleActive && turnManager.GetCurrentCombatant() == attacker)
                turnManager.EndTurn();

            autoTurnCoroutine = null;
        }

        private List<CharacterInstance> GetActiveCombatantsExcept(CharacterInstance attacker, CharacterInstance target)
        {
            List<CharacterInstance> blockers = new List<CharacterInstance>();
            AddCombatants(blockers, turnManager != null ? turnManager.GetPlayerCombatants() : null, attacker, target);
            AddCombatants(blockers, turnManager != null ? turnManager.GetEnemyCombatants() : null, attacker, target);
            return blockers;
        }

        private static void AddCombatants(List<CharacterInstance> blockers, IReadOnlyList<CharacterInstance> combatants, CharacterInstance attacker, CharacterInstance target)
        {
            if (blockers == null || combatants == null)
                return;

            for (int i = 0; i < combatants.Count; i++)
            {
                CharacterInstance combatant = combatants[i];
                if (combatant == null || combatant == attacker || combatant == target)
                    continue;

                blockers.Add(combatant);
            }
        }

        private bool IsCurrentPlayerTurn(CharacterInstance attacker)
        {
            return attacker != null
                   && attacker.IsAlive
                   && turnManager != null
                   && turnManager.IsBattleActive
                   && turnManager.GetCurrentCombatant() == attacker
                   && turnManager.IsPlayerUnit(attacker);
        }

        private CharacterInstance FindClosestAliveEnemy(CharacterInstance attacker)
        {
            if (turnManager == null || attacker == null)
                return null;

            IReadOnlyList<CharacterInstance> enemies = turnManager.GetEnemyCombatants();
            CharacterInstance closest = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                CharacterInstance enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                float distance = Vector3.SqrMagnitude(enemy.transform.position - attacker.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closest = enemy;
                }
            }

            return closest;
        }

        private bool TryGetBossEnemy(out CharacterInstance bossEnemy)
        {
            bossEnemy = null;
            if (turnManager == null)
                return false;

            EnsureBossIdCache();
            IReadOnlyList<CharacterInstance> enemies = turnManager.GetEnemyCombatants();
            int aliveEnemyCount = 0;

            for (int i = 0; i < enemies.Count; i++)
            {
                CharacterInstance enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                aliveEnemyCount++;
                if (IsBossEnemy(enemy))
                {
                    bossEnemy = enemy;
                    return true;
                }
            }

            if (singleEnemyCanBeBoss && aliveEnemyCount == 1)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    CharacterInstance enemy = enemies[i];
                    if (enemy != null && enemy.IsAlive)
                    {
                        bossEnemy = enemy;
                        return true;
                    }
                }
            }

            return false;
        }

        private void EnsureBossIdCache()
        {
            if (normalizedBossIds.Count > 0 || bossCharacterIds == null || bossCharacterIds.Length == 0)
                return;

            for (int i = 0; i < bossCharacterIds.Length; i++)
            {
                string normalized = NormalizeBossId(bossCharacterIds[i]);
                if (!string.IsNullOrEmpty(normalized))
                    normalizedBossIds.Add(normalized);
            }
        }
        private bool IsBossEnemy(CharacterInstance enemy)
        {
            if (enemy == null || enemy.Definition == null)
                return false;

            string characterId = NormalizeBossId(enemy.Definition.CharacterId);
            if (!string.IsNullOrEmpty(characterId) && normalizedBossIds.Contains(characterId))
                return true;

            string nameToken = ResolveCharacterName(enemy).ToLowerInvariant();
            return nameToken.Contains("boss");
        }

        private List<CharacterInstance> ResolvePlayerRosterCharacters()
        {
            if (battleManager != null)
                return battleManager.GetAllPlayerCharacters();

            List<CharacterInstance> roster = new List<CharacterInstance>();
            if (turnManager == null)
                return roster;

            IReadOnlyList<CharacterInstance> activePlayers = turnManager.GetPlayerCombatants();
            for (int i = 0; i < activePlayers.Count; i++)
            {
                CharacterInstance player = activePlayers[i];
                if (player != null)
                    roster.Add(player);
            }

            return roster;
        }

        private static Sprite ResolvePortrait(CharacterInstance character)
        {
            if (character == null || character.Definition == null)
                return null;

            return character.Definition.Portrait != null
                ? character.Definition.Portrait
                : character.Definition.BattleSprite;
        }

        private static string ResolveLaneLabel(int laneIndex)
        {
            switch (laneIndex)
            {
                case 0:
                    return "1ST";
                case 1:
                    return "2ND";
                case 2:
                    return "3RD";
                default:
                    return $"{laneIndex + 1}TH";
            }
        }

        private static string ResolveCharacterName(CharacterInstance character)
        {
            if (character == null)
                return string.Empty;

            if (character.Definition != null && !string.IsNullOrWhiteSpace(character.Definition.CharacterName))
                return character.Definition.CharacterName;

            return character.name;
        }

        private static string NormalizeBossId(string raw)
        {
            return string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Trim().ToLowerInvariant();
        }

        private static Canvas CreateFallbackCanvas()
        {
            GameObject canvasGo = new GameObject("BattleHudCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private sealed class PlayerSlotView
        {
            public int           LaneIndex;
            public RectTransform Root;
            public Image         FrontPortrait;
            public Image         FrontHpFill;
            public Image         ActiveRing;
            public Image         AbilityArcFill;
            public Image         DeadOverlay;
            public Button        ReserveButton;
            public Image         ReserveButtonBackground;
            public Image         ReservePortrait;
        }
    }
}






