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
    [ExecuteAlways]
    [DefaultExecutionOrder(25)]
    public sealed partial class BattleHudController : MonoBehaviour
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
        private const float TurnVignetteWidth = 148f;
        private const float TurnVignetteMaxAlpha = 0f;
        private const float TurnVignetteFadeSpeed = 1.8f;
        private const float ComboTrackerFadeInSpeed = 8.5f;
        private const float ComboTrackerFadeOutSpeed = 3.8f;
        private const float ComboTrackerHitHoldSeconds = 0.85f;
        private const float ComboTrackerFinishHoldSeconds = 1.2f;
        private const float ComboTrackerBasePulseSeconds = 0.18f;
        private const float ComboTrackerFinishPulseSeconds = 0.34f;
        private const int MaxPortraitChargeDividers = 20;
        private const float MedallionOrbRadius = 114f;  // px from circle centre to orb centre (38 × 3)
        private const float MedallionOrbSize   = 24f;   // diameter of each orb dot (9 × 3)
        private const float ComboCutInIntroSeconds = 0.16f;
        private const float ComboCutInHoldSeconds = 0.18f;
        private const float ComboCutInOutroSeconds = 0.16f;
        private const float EncounterIntroFadeInSeconds = 0.18f;
        private const float EncounterIntroHoldSeconds = 1.3f;
        private const float EncounterIntroFadeOutSeconds = 0.22f;
        private const float EncounterIntroPanelWidth = 660f;
        private const float EncounterIntroPanelHeight = 116f;
        private const float ComboCutInStripeWidth = 980f;
        private const float ComboCutInStripeHeight = 124f;
        private const float ComboCutInRotation = -13f;
        private const float HudHorizontalMargin = 18f;
        private const float HudTopMargin = 10f;
        private const float HudTopBottomGap = 14f;
        private const float PhoneHudContentMaxWidth = 1080f;
        private const float TabletHudContentMaxWidth = 1220f;
        private const float ExpandedHudContentMaxWidth = 1320f;
        private const float TabletHudBreakpoint = 1200f;
        private const float ExpandedHudBreakpoint = 1480f;

        [Header("Dependencies (auto-resolved if left empty)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TestBattleSetup battleSetup;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private RectTransform hudPrefab;

        [Header("Layout")]
        [SerializeField] private float topBarHeight = 124f;
        [SerializeField] private float bottomRailHeight = 300f;

        [Header("Medallion")]
        [SerializeField] private MedallionFrameCatalog medallionCatalog;

        [Header("Boss Detection")]
        [SerializeField] private string[] bossCharacterIds = Array.Empty<string>();
        [SerializeField] private bool singleEnemyCanBeBoss = true;

        private RectTransform hudRoot;
        private RectTransform hudContentFrame;
        private Text turnLabel;
        private Text objectiveLabel;
        private Image turnPanelBackground;
        private Image objectivePillBackground;
        private RectTransform pairSlotRow;
        private Image squadHpFill;
        private Text squadHpValueLabel;
        private GameObject pauseMenuPanel;
        private Text speedButtonLabel;
        private Text autoButtonLabel;
        private Text menuButtonLabel;
        private CanvasGroup turnVignetteCanvasGroup;
        private readonly List<LegendTokenView> elementLegendTokens = new List<LegendTokenView>();
        private readonly List<LegendTokenView> weaponLegendTokens = new List<LegendTokenView>();
        private readonly List<PlayerSlotView> playerSlots = new List<PlayerSlotView>();
        private readonly Dictionary<CharacterInstance, Action<float>> healthCallbacks = new Dictionary<CharacterInstance, Action<float>>();
        private readonly Dictionary<CharacterInstance, Action> deathCallbacks = new Dictionary<CharacterInstance, Action>();
        private readonly Dictionary<CharacterInstance, TurnCountdownIndicator> turnIndicators = new Dictionary<CharacterInstance, TurnCountdownIndicator>();
        private readonly HashSet<string> normalizedBossIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static Sprite s_CircleSprite;
        private static Sprite s_RingSprite;
        private static Sprite s_EdgeVignetteSprite;
        private static Sprite s_WhiteSprite;
        private static Sprite s_ComboCutInStripeSprite;
        private static bool s_HudBootstrapRegistered;
        private static readonly Color TurnVignetteColor = new Color(0.05f, 0.03f, 0.02f, 1f);
        private static readonly ElementalType[] ElementLegendOrder =
        {
            ElementalType.Earth,
            ElementalType.Water,
            ElementalType.Fire,
            ElementalType.Wind,
            ElementalType.Lightning,
            ElementalType.Ice,
            ElementalType.Shadow
        };
        private static readonly MartialArtsType[] WeaponLegendOrder =
        {
            MartialArtsType.Sword,
            MartialArtsType.Spear,
            MartialArtsType.Bow,
            MartialArtsType.Staff,
            MartialArtsType.DualDaggers,
            MartialArtsType.HeavyWeapons,
            MartialArtsType.Unarmed
        };
        private Coroutine pollCoroutine;
        private Coroutine autoTurnCoroutine;

        private float speedMultiplier = SpeedNormal;
        private bool autoModeEnabled;
        private bool isPaused;
        private float turnVignetteTargetAlpha;
        private float turnVignetteCurrentAlpha;
        private RectTransform comboTrackerRoot;
        private CanvasGroup comboTrackerCanvasGroup;
        private Text comboHitCountLabel;
        private Text comboHitsSuffixLabel;
        private Text comboTypeLabel;
        private Image comboTrackerGlowImage;
        private Image comboTrackerStreakImage;
        private Vector2 comboTrackerBaseAnchoredPosition;
        private float comboTrackerTargetAlpha;
        private float comboTrackerCurrentAlpha;
        private float comboTrackerHideAt = float.NegativeInfinity;
        private float comboTrackerPulseTimer;
        private float comboTrackerPulseDuration = ComboTrackerBasePulseSeconds;
        private RectTransform comboCutInRoot;
        private CanvasGroup comboCutInCanvasGroup;
        private Coroutine comboCutInCoroutine;
        private readonly List<ComboCutInSlotView> comboCutInSlots = new List<ComboCutInSlotView>();
        private RectTransform encounterIntroRoot;
        private CanvasGroup encounterIntroCanvasGroup;
        private Text encounterIntroTitleLabel;
        private Text encounterIntroSubtitleLabel;
        private Coroutine encounterIntroCoroutine;
        private bool battleHasEnded;
        private BattleResult battleEndResult;
        private Vector2 lastHudViewportSize = new Vector2(-1f, -1f);
        private Vector2Int lastResponsiveScreenSize = new Vector2Int(-1, -1);
        private Rect lastResponsiveSafeArea = new Rect(-1f, -1f, -1f, -1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetHudBootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            s_HudBootstrapRegistered = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterHudBootstrap()
        {
            if (s_HudBootstrapRegistered)
                return;

            SceneManager.sceneLoaded += HandleSceneLoaded;
            s_HudBootstrapRegistered = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureHudControllerExists()
        {
            EnsureHudControllerExists(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            EnsureHudControllerExists(scene);
        }

        private static void EnsureHudControllerExists(Scene scene)
        {
            if (!scene.IsValid() || !TryFindTurnManager(scene, out _))
                return;

            if (HasHudController(scene))
                return;

            GameObject go = new GameObject("BattleHudController");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<BattleHudController>();
        }

        private static bool TryFindTurnManager(Scene scene, out TurnManager resolvedTurnManager)
        {
            TurnManager[] managers = FindObjectsByType<TurnManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < managers.Length; i++)
            {
                TurnManager manager = managers[i];
                if (manager != null && manager.gameObject.scene == scene)
                {
                    resolvedTurnManager = manager;
                    return true;
                }
            }

            resolvedTurnManager = null;
            return false;
        }

        private static bool HasHudController(Scene scene)
        {
            BattleHudController[] controllers = FindObjectsByType<BattleHudController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                BattleHudController controller = controllers[i];
                if (controller != null && controller.gameObject.scene == scene)
                    return true;
            }

            return false;
        }

        private IEnumerator Start()
        {
            if (!Application.isPlaying)
                yield break;

            ResolveDependencies();
            if (turnManager == null)
                yield break;

            yield return WaitForBattleSetup();

            battleHasEnded = false;
            battleEndResult = BattleResult.Win;

            BuildHud();
            BindEvents();
            RebuildPlayerSlots();
            RefreshHud();
            PlayEncounterIntroIfNeeded();

            pollCoroutine = StartCoroutine(PollHud());
            TryQueueAutoTurn();
        }

        private void OnDestroy()
        {
            if (pollCoroutine != null)
                StopCoroutine(pollCoroutine);

            if (autoTurnCoroutine != null)
                StopCoroutine(autoTurnCoroutine);

            if (comboCutInCoroutine != null)
                StopCoroutine(comboCutInCoroutine);

            if (encounterIntroCoroutine != null)
                StopCoroutine(encounterIntroCoroutine);

            UnbindEvents();
            UnbindCharacterEvents();
            ClearTurnIndicators();
            Time.timeScale = SpeedNormal;
        }

        private void Update()
        {
            UpdateTurnVignetteVisual();
            UpdateComboTrackerVisual();
            UpdateResponsiveLayout();
        }

        private void ResolveDependencies()
        {
            if (turnManager == null)
                turnManager = FindFirstObjectByType<TurnManager>();

            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (battleSetup == null)
                battleSetup = FindFirstObjectByType<TestBattleSetup>();

            if (targetCanvas == null)
                targetCanvas = FindFirstObjectByType<Canvas>();

            if (targetCanvas == null)
                targetCanvas = CreateFallbackCanvas();

            RepairCanvasIfNeeded(targetCanvas);
            EnsureHudCanvasHierarchy(targetCanvas);
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

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        private static void EnsureHudCanvasHierarchy(Canvas canvas)
        {
            if (canvas == null)
                return;

            Transform safeAreaPanel = canvas.transform.Find("UI_SafeAreaPanel");
            if (safeAreaPanel == null)
            {
                RectTransform safeAreaRoot = CreateRect("UI_SafeAreaPanel", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                StretchRectTransform(safeAreaRoot);
                safeAreaRoot.gameObject.AddComponent<SafeAreaHandler>();
                CreateRect("HUD", safeAreaRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                CreateRect("Popups", safeAreaRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                return;
            }

            if (safeAreaPanel is RectTransform safeAreaRect)
                StretchRectTransform(safeAreaRect);

            Transform hud = safeAreaPanel.Find("HUD");
            if (hud == null)
            {
                RectTransform hudRoot = CreateRect("HUD", safeAreaPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                StretchRectTransform(hudRoot);
            }
            else if (hud is RectTransform hudRect)
            {
                StretchRectTransform(hudRect);
            }

            Transform popups = safeAreaPanel.Find("Popups");
            if (popups == null)
            {
                RectTransform popupRoot = CreateRect("Popups", safeAreaPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                StretchRectTransform(popupRoot);
            }
            else if (popups is RectTransform popupsRect)
            {
                StretchRectTransform(popupsRect);
            }
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

            CombatComboPresentationBus.ComboStarted += HandleComboStarted;
            CombatComboPresentationBus.ComboHitRegistered += HandleComboHitRegistered;
            CombatComboPresentationBus.ComboSequenceFinished += HandleComboSequenceFinished;
            CombatComboPresentationBus.ComboPresentationReset += HandleComboPresentationReset;
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

            CombatComboPresentationBus.ComboStarted -= HandleComboStarted;
            CombatComboPresentationBus.ComboHitRegistered -= HandleComboHitRegistered;
            CombatComboPresentationBus.ComboSequenceFinished -= HandleComboSequenceFinished;
            CombatComboPresentationBus.ComboPresentationReset -= HandleComboPresentationReset;
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
            battleHasEnded = true;
            battleEndResult = result;
            HideEncounterIntroImmediate();

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
            HideComboTrackerImmediate();
            HideComboCutInImmediate();
            UpdateUtilityLabels();
        }

        private void HandleComboStarted(CharacterInstance[] participants)
        {
            PlayComboCutIn(participants);
        }

        private void HandleComboHitRegistered(int hitCount)
        {
            ApplyComboTrackerState(hitCount, false);
        }

        private void HandleComboSequenceFinished(int hitCount)
        {
            ApplyComboTrackerState(hitCount, true);
        }

        private void HandleComboPresentationReset()
        {
            HideComboTrackerImmediate();
            HideComboCutInImmediate();
        }

        private void BuildHud()
        {
            if (targetCanvas == null || hudRoot != null)
                return;

            RectTransform hudParent = ResolveHudParent();

            hudRoot = hudPrefab != null
                ? Instantiate(hudPrefab, hudParent, false)
                : CreateRect("BattleHudRoot", hudParent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            hudRoot.name = "BattleHudRoot";
            BuildResponsiveLayoutFrame();
            BuildTurnVignette();
            BuildTopBar();
            BuildComboTracker();
            BuildComboCutIn();
            BuildEncounterIntro();
            BuildBottomRail();
            BuildPauseMenu();
            UpdateResponsiveLayout();
            UpdateUtilityLabels();
        }

        private RectTransform ResolveHudParent()
        {
            RectTransform preferredParent = FindPreferredHudParent();
            if (preferredParent != null)
                return preferredParent;

            return targetCanvas != null
                ? targetCanvas.transform as RectTransform
                : null;
        }

        private RectTransform FindPreferredHudParent()
        {
            if (targetCanvas == null)
                return null;

            Transform safeAreaPanel = targetCanvas.transform.Find("UI_SafeAreaPanel");
            if (safeAreaPanel != null)
            {
                Transform hudTransform = safeAreaPanel.Find("HUD");
                if (hudTransform is RectTransform hudRect)
                    return hudRect;

                if (safeAreaPanel is RectTransform safeAreaRect)
                    return safeAreaRect;
            }

            SafeAreaHandler safeAreaHandler = targetCanvas.GetComponentInChildren<SafeAreaHandler>(true);
            if (safeAreaHandler != null)
            {
                Transform hudTransform = safeAreaHandler.transform.Find("HUD");
                if (hudTransform is RectTransform hudRect)
                    return hudRect;

                if (safeAreaHandler.transform is RectTransform safeAreaRect)
                    return safeAreaRect;
            }

            return null;
        }

        private void BuildResponsiveLayoutFrame()
        {
            if (hudRoot == null)
                return;

            RectTransform existing = hudRoot.Find("ResponsiveLayoutFrame") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            RectTransform frame = CreateRect("ResponsiveLayoutFrame", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            frame.pivot = new Vector2(0.5f, 0.5f);
            hudContentFrame = frame;
            ApplyResponsiveLayout(force: true);
        }

        private void UpdateResponsiveLayout()
        {
            if (hudRoot == null || hudContentFrame == null)
                return;

            Vector2 viewportSize = hudRoot.rect.size;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            Rect safeArea = Screen.safeArea;
            if (!HasResponsiveLayoutChanged(viewportSize, screenSize, safeArea))
                return;

            ApplyResponsiveLayout(force: true);
        }

        private bool HasResponsiveLayoutChanged(Vector2 viewportSize, Vector2Int screenSize, Rect safeArea)
        {
            if ((viewportSize - lastHudViewportSize).sqrMagnitude > 0.25f)
                return true;

            if (screenSize != lastResponsiveScreenSize)
                return true;

            if (safeArea != lastResponsiveSafeArea)
                return true;

            return false;
        }

        private void ApplyResponsiveLayout(bool force = false)
        {
            if (hudRoot == null || hudContentFrame == null)
                return;

            Vector2 viewportSize = hudRoot.rect.size;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            Rect safeArea = Screen.safeArea;
            if (!force && !HasResponsiveLayoutChanged(viewportSize, screenSize, safeArea))
                return;

            float contentWidth = ResolveHudContentWidth(viewportSize.x, viewportSize.y);
            hudContentFrame.sizeDelta = new Vector2(contentWidth, 0f);
            hudContentFrame.anchoredPosition = Vector2.zero;

            lastHudViewportSize = viewportSize;
            lastResponsiveScreenSize = screenSize;
            lastResponsiveSafeArea = safeArea;
        }

        private static float ResolveHudContentWidth(float availableWidth, float availableHeight)
        {
            if (availableWidth <= 0.01f || availableHeight <= 0.01f)
                return PhoneHudContentMaxWidth;

            float maxWidth = PhoneHudContentMaxWidth;
            if (availableWidth >= ExpandedHudBreakpoint)
                maxWidth = ExpandedHudContentMaxWidth;
            else if (availableWidth >= TabletHudBreakpoint)
                maxWidth = TabletHudContentMaxWidth;

            float boundedWidth = Mathf.Min(availableWidth, maxWidth);
            float minimumWidth = Mathf.Min(availableWidth, 720f);
            return Mathf.Clamp(boundedWidth, minimumWidth, availableWidth);
        }

        private void BuildTurnVignette()
        {
            RectTransform existing = hudRoot.Find("TurnEdgeVignette") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            RectTransform vignette = CreateRect("TurnEdgeVignette", hudRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            vignette.SetAsFirstSibling();

            turnVignetteCanvasGroup = vignette.gameObject.AddComponent<CanvasGroup>();
            turnVignetteCanvasGroup.alpha = 0f;
            turnVignetteCanvasGroup.interactable = false;
            turnVignetteCanvasGroup.blocksRaycasts = false;

            Sprite edgeSprite = GetEdgeVignetteSprite();

            RectTransform leftEdge = CreateRect("LeftEdge", vignette, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(TurnVignetteWidth, 0f));
            Image leftImage = leftEdge.gameObject.AddComponent<Image>();
            leftImage.sprite = edgeSprite;
            leftImage.color = TurnVignetteColor;
            leftImage.raycastTarget = false;

            RectTransform rightEdge = CreateRect("RightEdge", vignette, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-TurnVignetteWidth, 0f), Vector2.zero);
            Image rightImage = rightEdge.gameObject.AddComponent<Image>();
            rightImage.sprite = edgeSprite;
            rightImage.color = TurnVignetteColor;
            rightImage.raycastTarget = false;
            rightEdge.localScale = new Vector3(-1f, 1f, 1f);

            turnVignetteCurrentAlpha = 0f;
            turnVignetteTargetAlpha = ResolveTurnVignetteTargetAlpha();
        }

        private void BuildTopBar()
        {
            RectTransform existing = hudRoot.Find("TopCombatBar") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);
            elementLegendTokens.Clear();
            weaponLegendTokens.Clear();
            RectTransform layoutParent = hudContentFrame != null ? hudContentFrame : hudRoot;
            RectTransform topBar = CreateRect("TopCombatBar", layoutParent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(HudHorizontalMargin, -topBarHeight - HudTopBottomGap), new Vector2(-HudHorizontalMargin, -HudTopMargin));
            Image topBarBg = topBar.gameObject.AddComponent<Image>();
            topBarBg.color = new Color(0.37f, 0.31f, 0.1f, 0.9f);
            topBarBg.raycastTarget = false;
            Outline topBarOutline = topBar.gameObject.AddComponent<Outline>();
            topBarOutline.effectColor = new Color(0.97f, 0.9f, 0.56f, 0.16f);
            topBarOutline.effectDistance = new Vector2(1.6f, -1.6f);
            RectTransform inner = CreateRect("Inner", topBar, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image innerBg = inner.gameObject.AddComponent<Image>();
            innerBg.color = new Color(0.08f, 0.06f, 0.03f, 0.95f);
            innerBg.raycastTarget = false;
            RectTransform topSheen = CreateRect("TopSheen", inner, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -5f), new Vector2(-12f, -1f));
            Image topSheenImage = topSheen.gameObject.AddComponent<Image>();
            topSheenImage.color = new Color(0.98f, 0.9f, 0.54f, 0.14f);
            topSheenImage.raycastTarget = false;
            RectTransform contentRoot = CreateRect("Content", inner, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup topLayout = contentRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            topLayout.padding = new RectOffset(8, 8, 8, 8);
            topLayout.spacing = 8f;
            topLayout.childForceExpandWidth = false;
            topLayout.childForceExpandHeight = true;
            topLayout.childControlWidth = true;
            topLayout.childControlHeight = true;
            BuildTurnContextModule(contentRoot);
            objectiveLabel = CreatePillLabel(contentRoot, "ObjectivePill", TextAnchor.MiddleCenter, out objectivePillBackground);
            LayoutElement objectiveLayout = objectivePillBackground != null
                ? objectivePillBackground.transform.parent.GetComponent<LayoutElement>()
                : null;
            if (objectiveLayout != null)
            {
                objectiveLayout.flexibleWidth = 1f;
                objectiveLayout.minWidth = 176f;
            }
            BuildUtilityControls(contentRoot);
        }
        private void BuildComboTracker()
        {
            if (hudRoot == null)
                return;

            RectTransform existing = hudRoot.Find("ComboTracker") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            RectTransform layoutParent = hudContentFrame != null ? hudContentFrame : hudRoot;
            RectTransform tracker = CreateRect("ComboTracker", layoutParent, Vector2.one, Vector2.one, Vector2.zero, Vector2.zero);
            tracker.anchorMin = Vector2.one;
            tracker.anchorMax = Vector2.one;
            tracker.pivot = Vector2.one;
            tracker.sizeDelta = new Vector2(336f, 132f);
            tracker.anchoredPosition = new Vector2(-(HudHorizontalMargin + 4f), -topBarHeight - 22f);
            tracker.SetAsLastSibling();

            comboTrackerRoot = tracker;
            comboTrackerBaseAnchoredPosition = tracker.anchoredPosition;
            comboTrackerCanvasGroup = tracker.gameObject.AddComponent<CanvasGroup>();
            comboTrackerCanvasGroup.alpha = 0f;
            comboTrackerCanvasGroup.interactable = false;
            comboTrackerCanvasGroup.blocksRaycasts = false;

            RectTransform glow = CreateRect("Glow", tracker, new Vector2(0.1f, 0.14f), new Vector2(1f, 0.94f), Vector2.zero, Vector2.zero);
            comboTrackerGlowImage = glow.gameObject.AddComponent<Image>();
            comboTrackerGlowImage.color = new Color(1f, 0.72f, 0.24f, 0.1f);
            comboTrackerGlowImage.raycastTarget = false;

            RectTransform streak = CreateRect("Streak", tracker, new Vector2(0.18f, 0.46f), new Vector2(1f, 0.6f), Vector2.zero, Vector2.zero);
            comboTrackerStreakImage = streak.gameObject.AddComponent<Image>();
            comboTrackerStreakImage.color = new Color(1f, 0.82f, 0.28f, 0.24f);
            comboTrackerStreakImage.raycastTarget = false;
            streak.localRotation = Quaternion.Euler(0f, 0f, -5f);

            RectTransform countRoot = CreateRect("CountRoot", tracker, new Vector2(0.12f, 0.36f), new Vector2(0.82f, 1f), Vector2.zero, Vector2.zero);
            comboHitCountLabel = CreateText("Count", countRoot, TextAnchor.LowerRight, 82, FontStyle.Bold);
            comboHitCountLabel.resizeTextForBestFit = true;
            comboHitCountLabel.resizeTextMinSize = 44;
            comboHitCountLabel.resizeTextMaxSize = 82;
            comboHitCountLabel.color = new Color(1f, 0.95f, 0.86f, 1f);
            comboHitCountLabel.text = string.Empty;

            RectTransform hitsRoot = CreateRect("HitsRoot", tracker, new Vector2(0.76f, 0.54f), new Vector2(1f, 0.86f), Vector2.zero, Vector2.zero);
            comboHitsSuffixLabel = CreateText("Hits", hitsRoot, TextAnchor.LowerLeft, 26, FontStyle.BoldAndItalic);
            comboHitsSuffixLabel.resizeTextForBestFit = true;
            comboHitsSuffixLabel.resizeTextMinSize = 16;
            comboHitsSuffixLabel.resizeTextMaxSize = 26;
            comboHitsSuffixLabel.color = new Color(1f, 0.84f, 0.26f, 1f);
            comboHitsSuffixLabel.text = "Hits!";

            RectTransform typeRoot = CreateRect("ComboTypeRoot", tracker, new Vector2(0.04f, 0.02f), new Vector2(1f, 0.48f), Vector2.zero, Vector2.zero);
            typeRoot.localRotation = Quaternion.Euler(0f, 0f, -3.5f);
            comboTypeLabel = CreateText("ComboType", typeRoot, TextAnchor.UpperRight, 42, FontStyle.BoldAndItalic);
            comboTypeLabel.resizeTextForBestFit = true;
            comboTypeLabel.resizeTextMinSize = 20;
            comboTypeLabel.resizeTextMaxSize = 42;
            comboTypeLabel.color = new Color(1f, 0.48f, 0.12f, 1f);
            comboTypeLabel.text = string.Empty;
            Outline comboOutline = comboTypeLabel.gameObject.AddComponent<Outline>();
            comboOutline.effectColor = new Color(0.18f, 0.05f, 0f, 0.52f);
            comboOutline.effectDistance = new Vector2(1.6f, -1.6f);

            HideComboTrackerImmediate();
        }
        private void BuildComboCutIn()
        {
            if (hudRoot == null)
                return;

            RectTransform existing = hudRoot.Find("ComboCutInOverlay") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            comboCutInSlots.Clear();

            RectTransform overlay = CreateRect("ComboCutInOverlay", hudRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            comboCutInRoot = overlay;
            comboCutInCanvasGroup = overlay.gameObject.AddComponent<CanvasGroup>();
            comboCutInCanvasGroup.alpha = 0f;
            comboCutInCanvasGroup.interactable = false;
            comboCutInCanvasGroup.blocksRaycasts = false;

            for (int i = 0; i < 3; i++)
                comboCutInSlots.Add(BuildComboCutInSlot(overlay, i));

            HideComboCutInImmediate();
        }

        private ComboCutInSlotView BuildComboCutInSlot(Transform parent, int index)
        {
            RectTransform slotRoot = CreateRect($"Slot{index}", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            slotRoot.pivot = new Vector2(0.5f, 0.5f);
            slotRoot.sizeDelta = new Vector2(ComboCutInStripeWidth, ComboCutInStripeHeight);
            slotRoot.localRotation = Quaternion.Euler(0f, 0f, ComboCutInRotation);
            slotRoot.gameObject.SetActive(false);

            Image stripe = slotRoot.gameObject.AddComponent<Image>();
            stripe.sprite = GetComboCutInStripeSprite();
            stripe.type = Image.Type.Sliced;
            stripe.color = Color.white;
            stripe.raycastTarget = false;
            Outline stripeOutline = slotRoot.gameObject.AddComponent<Outline>();
            stripeOutline.effectColor = new Color(0f, 0f, 0f, 0.64f);
            stripeOutline.effectDistance = new Vector2(3f, -3f);

            RectTransform topSlash = CreateRect("TopSlash", slotRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(34f, -10f), new Vector2(-34f, -2f));
            Image topSlashImage = topSlash.gameObject.AddComponent<Image>();
            topSlashImage.sprite = GetWhiteSprite();
            topSlashImage.color = new Color(0f, 0f, 0f, 0.9f);
            topSlashImage.raycastTarget = false;
            topSlash.localRotation = Quaternion.Euler(0f, 0f, -1.8f);

            RectTransform bottomSlash = CreateRect("BottomSlash", slotRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(34f, 2f), new Vector2(-34f, 10f));
            Image bottomSlashImage = bottomSlash.gameObject.AddComponent<Image>();
            bottomSlashImage.sprite = GetWhiteSprite();
            bottomSlashImage.color = new Color(0f, 0f, 0f, 0.9f);
            bottomSlashImage.raycastTarget = false;
            bottomSlash.localRotation = Quaternion.Euler(0f, 0f, 1.8f);

            RectTransform portraitMask = CreateRect("PortraitMask", slotRoot, new Vector2(0.15f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            portraitMask.gameObject.AddComponent<RectMask2D>();

            RectTransform portraitRect = CreateRect("Portrait", portraitMask, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-48f, -20f), new Vector2(48f, 20f));
            Image portrait = portraitRect.gameObject.AddComponent<Image>();
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;

            RectTransform tint = CreateRect("Tint", slotRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image tintImage = tint.gameObject.AddComponent<Image>();
            tintImage.sprite = GetWhiteSprite();
            tintImage.color = new Color(1f, 0.58f, 0.08f, 0.1f);
            tintImage.raycastTarget = false;

            RectTransform shine = CreateRect("Shine", slotRoot, new Vector2(0.06f, 0.14f), new Vector2(0.44f, 0.86f), Vector2.zero, Vector2.zero);
            Image shineImage = shine.gameObject.AddComponent<Image>();
            shineImage.sprite = GetWhiteSprite();
            shineImage.color = new Color(1f, 0.98f, 0.88f, 0.18f);
            shineImage.raycastTarget = false;
            shine.localRotation = Quaternion.Euler(0f, 0f, 9f);

            return new ComboCutInSlotView
            {
                Root = slotRoot,
                Stripe = stripe,
                Portrait = portrait,
                PortraitRect = portraitRect,
                Shine = shineImage
            };
        }

        private void BuildEncounterIntro()
        {
            if (hudContentFrame == null)
                return;

            RectTransform existing = hudContentFrame.Find("EncounterIntroOverlay") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            RectTransform overlay = CreateRect(
                "EncounterIntroOverlay",
                hudContentFrame,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-(EncounterIntroPanelWidth * 0.5f), -176f - EncounterIntroPanelHeight),
                new Vector2(EncounterIntroPanelWidth * 0.5f, -176f));
            overlay.pivot = new Vector2(0.5f, 1f);

            Image panel = overlay.gameObject.AddComponent<Image>();
            panel.sprite = GetWhiteSprite();
            panel.color = new Color(0.08f, 0.05f, 0.04f, 0.92f);
            panel.raycastTarget = false;

            Outline panelOutline = overlay.gameObject.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.82f, 0.66f, 0.28f, 0.42f);
            panelOutline.effectDistance = new Vector2(2f, -2f);

            RectTransform titleRoot = CreateRect("Title", overlay, new Vector2(0f, 0.44f), new Vector2(1f, 1f), new Vector2(22f, -18f), new Vector2(-22f, -10f));
            encounterIntroTitleLabel = CreateText("Label", titleRoot, TextAnchor.LowerCenter, 34, FontStyle.Bold);
            encounterIntroTitleLabel.color = new Color(0.99f, 0.95f, 0.84f, 1f);
            encounterIntroTitleLabel.resizeTextForBestFit = true;
            encounterIntroTitleLabel.resizeTextMinSize = 18;
            encounterIntroTitleLabel.resizeTextMaxSize = 34;
            encounterIntroTitleLabel.text = string.Empty;

            RectTransform subtitleRoot = CreateRect("Subtitle", overlay, new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Vector2(28f, 10f), new Vector2(-28f, 12f));
            encounterIntroSubtitleLabel = CreateText("Label", subtitleRoot, TextAnchor.UpperCenter, 18, FontStyle.Normal);
            encounterIntroSubtitleLabel.color = new Color(0.9f, 0.84f, 0.74f, 0.96f);
            encounterIntroSubtitleLabel.resizeTextForBestFit = true;
            encounterIntroSubtitleLabel.resizeTextMinSize = 12;
            encounterIntroSubtitleLabel.resizeTextMaxSize = 18;
            encounterIntroSubtitleLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            encounterIntroSubtitleLabel.verticalOverflow = VerticalWrapMode.Overflow;
            encounterIntroSubtitleLabel.text = string.Empty;

            encounterIntroRoot = overlay;
            encounterIntroCanvasGroup = overlay.gameObject.AddComponent<CanvasGroup>();
            encounterIntroCanvasGroup.alpha = 0f;
            encounterIntroCanvasGroup.interactable = false;
            encounterIntroCanvasGroup.blocksRaycasts = false;
            HideEncounterIntroImmediate();
        }

        private void PlayEncounterIntroIfNeeded()
        {
            if (!Application.isPlaying || battleHasEnded)
                return;

            if (encounterIntroRoot == null || encounterIntroCanvasGroup == null || encounterIntroTitleLabel == null || encounterIntroSubtitleLabel == null)
                return;

            string title = battleSetup != null ? battleSetup.GetEncounterDisplayName() : string.Empty;
            string subtitle = battleSetup != null ? battleSetup.GetEncounterIntroSubtitle() : string.Empty;
            if (string.IsNullOrWhiteSpace(title))
                return;

            encounterIntroTitleLabel.text = title.Trim().ToUpperInvariant();
            encounterIntroSubtitleLabel.text = string.IsNullOrWhiteSpace(subtitle) ? string.Empty : subtitle.Trim();

            if (encounterIntroCoroutine != null)
                StopCoroutine(encounterIntroCoroutine);

            encounterIntroCoroutine = StartCoroutine(AnimateEncounterIntro());
        }

        private IEnumerator AnimateEncounterIntro()
        {
            if (encounterIntroRoot == null || encounterIntroCanvasGroup == null)
                yield break;

            encounterIntroRoot.gameObject.SetActive(true);
            encounterIntroCanvasGroup.alpha = 0f;

            yield return AnimateEncounterIntroPhase(0f, 1f, EncounterIntroFadeInSeconds);
            yield return new WaitForSecondsRealtime(EncounterIntroHoldSeconds);
            yield return AnimateEncounterIntroPhase(1f, 0f, EncounterIntroFadeOutSeconds);

            encounterIntroCoroutine = null;
            HideEncounterIntroImmediate();
        }

        private IEnumerator AnimateEncounterIntroPhase(float fromAlpha, float toAlpha, float duration)
        {
            if (encounterIntroCanvasGroup == null)
                yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0.0001f ? 1f : Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                encounterIntroCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
                yield return null;
            }

            encounterIntroCanvasGroup.alpha = toAlpha;
        }

        private void HideEncounterIntroImmediate()
        {
            if (encounterIntroCoroutine != null)
            {
                StopCoroutine(encounterIntroCoroutine);
                encounterIntroCoroutine = null;
            }

            if (encounterIntroCanvasGroup != null)
                encounterIntroCanvasGroup.alpha = 0f;

            if (encounterIntroRoot != null)
                encounterIntroRoot.gameObject.SetActive(false);
        }
        private void BuildTurnContextModule(Transform parent)
        {
            RectTransform contextRoot = CreateTopBarModule(parent, "TurnContextPill", 246f, 88f, out turnPanelBackground);
            VerticalLayoutGroup layout = contextRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            BuildAffinityLegend(contextRoot);
            RectTransform labelRoot = CreateRect("TurnLabel", contextRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement labelLayout = labelRoot.gameObject.AddComponent<LayoutElement>();
            labelLayout.preferredHeight = 40f;
            labelLayout.minHeight = 40f;
            turnLabel = CreateText("Label", labelRoot, TextAnchor.MiddleLeft, 24, FontStyle.Bold);
            turnLabel.color = new Color(0.98f, 0.95f, 0.84f, 1f);
            turnLabel.resizeTextForBestFit = true;
            turnLabel.resizeTextMinSize = 14;
            turnLabel.resizeTextMaxSize = 24;
            turnLabel.text = "-";
        }
        private void BuildAffinityLegend(Transform parent)
        {
            RectTransform legendRoot = CreateRect("AffinityLegend", parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement legendLayout = legendRoot.gameObject.AddComponent<LayoutElement>();
            legendLayout.preferredHeight = 30f;
            legendLayout.minHeight = 30f;
            VerticalLayoutGroup layout = legendRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            RectTransform elementRow = BuildLegendRow(legendRoot, "ElementLegendRow", "EL");
            for (int i = 0; i < ElementLegendOrder.Length; i++)
            {
                ElementalType element = ElementLegendOrder[i];
                LegendTokenView token = CreateLegendToken(
                    elementRow,
                    $"Element_{element}",
                    ResolveElementLegendLabel(element),
                    ResolveElementColor(element),
                    16f,
                    16f,
                    circular: true);
                token.ElementalType = element;
                elementLegendTokens.Add(token);
            }
            RectTransform weaponRow = BuildLegendRow(legendRoot, "WeaponLegendRow", "AR");
            for (int i = 0; i < WeaponLegendOrder.Length; i++)
            {
                MartialArtsType weapon = WeaponLegendOrder[i];
                LegendTokenView token = CreateLegendToken(
                    weaponRow,
                    $"Weapon_{weapon}",
                    ResolveWeaponLegendLabel(weapon),
                    ResolveWeaponLegendColor(weapon),
                    20f,
                    12f,
                    circular: false);
                token.MartialArtsType = weapon;
                weaponLegendTokens.Add(token);
            }
        }
        private RectTransform BuildLegendRow(Transform parent, string name, string caption)
        {
            RectTransform row = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 14f;
            rowLayout.minHeight = 14f;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            RectTransform captionRoot = CreateRect("Caption", row, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement captionLayout = captionRoot.gameObject.AddComponent<LayoutElement>();
            captionLayout.preferredWidth = 18f;
            captionLayout.minWidth = 18f;
            Text captionLabel = CreateText("Label", captionRoot, TextAnchor.MiddleLeft, 11, FontStyle.Bold);
            captionLabel.text = caption;
            captionLabel.color = new Color(0.91f, 0.84f, 0.68f, 0.92f);
            RectTransform tokenRoot = CreateRect("Tokens", row, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement tokenLayout = tokenRoot.gameObject.AddComponent<LayoutElement>();
            tokenLayout.flexibleWidth = 1f;
            HorizontalLayoutGroup tokenGroup = tokenRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            tokenGroup.spacing = 3f;
            tokenGroup.childAlignment = TextAnchor.MiddleLeft;
            tokenGroup.childControlWidth = true;
            tokenGroup.childControlHeight = true;
            tokenGroup.childForceExpandWidth = false;
            tokenGroup.childForceExpandHeight = false;
            return tokenRoot;
        }
        private LegendTokenView CreateLegendToken(Transform parent, string name, string label, Color baseColor, float width, float height, bool circular)
        {
            RectTransform tokenRoot = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement layoutElement = tokenRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width;
            layoutElement.minWidth = width;
            layoutElement.preferredHeight = height;
            layoutElement.minHeight = height;
            Image background = tokenRoot.gameObject.AddComponent<Image>();
            background.color = baseColor;
            background.raycastTarget = false;
            if (circular)
                background.sprite = GetCircleSprite();
            Outline outline = tokenRoot.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.03f, 0.03f, 0.03f, 0.88f);
            outline.effectDistance = new Vector2(0.6f, -0.6f);
            Text labelText = CreateText("Label", tokenRoot, TextAnchor.MiddleCenter, circular ? 9 : 10, FontStyle.Bold);
            labelText.text = label;
            labelText.color = ResolveLegendTextColor(baseColor);
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = circular ? 7 : 8;
            labelText.resizeTextMaxSize = circular ? 9 : 10;
            return new LegendTokenView
            {
                Root = tokenRoot,
                Background = background,
                Outline = outline,
                Label = labelText,
                BaseColor = baseColor,
                BaseTextColor = labelText.color
            };
        }
        private static RectTransform CreateTopBarModule(Transform parent, string name, float minWidth, float preferredHeight, out Image backgroundImage)
        {
            RectTransform module = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement layoutElement = module.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = minWidth;
            layoutElement.preferredWidth = minWidth;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minHeight = preferredHeight;
            Image frame = module.gameObject.AddComponent<Image>();
            frame.color = new Color(0.43f, 0.39f, 0.14f, 0.96f);
            frame.raycastTarget = false;
            Outline frameOutline = module.gameObject.AddComponent<Outline>();
            frameOutline.effectColor = new Color(0.98f, 0.92f, 0.6f, 0.16f);
            frameOutline.effectDistance = new Vector2(1.1f, -1.1f);
            RectTransform inner = CreateRect("Inner", module, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(4f, 4f), new Vector2(-4f, -4f));
            backgroundImage = inner.gameObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.16f, 0.12f, 0.07f, 0.96f);
            backgroundImage.raycastTarget = false;
            return inner;
        }
        private void BuildBottomRail()
        {
            // Low-profile rail anchored to the bottom edge. The root acts as a
            // full interaction shield so portrait taps never leak through to the
            // battlefield drag/tap layer behind the HUD.
            RectTransform layoutParent = hudContentFrame != null ? hudContentFrame : hudRoot;
            RectTransform existing = layoutParent.Find("BottomSquadRail") as RectTransform;
            if (existing != null)
                Destroy(existing.gameObject);

            RectTransform bottomRail = CreateRect("BottomSquadRail", layoutParent,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(HudHorizontalMargin, 0f), new Vector2(-HudHorizontalMargin, bottomRailHeight));

            Image interactionShield = bottomRail.gameObject.AddComponent<Image>();
            interactionShield.color = new Color(0f, 0f, 0f, 0.002f);
            interactionShield.raycastTarget = true;

            RectTransform tray = CreateRect("Tray", bottomRail,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(10f, 0f), new Vector2(-10f, 172f));
            Image trayFrame = tray.gameObject.AddComponent<Image>();
            trayFrame.color = new Color(0.32f, 0.27f, 0.08f, 0.98f);
            trayFrame.raycastTarget = false;
            Outline trayOutline = tray.gameObject.AddComponent<Outline>();
            trayOutline.effectColor = new Color(0.95f, 0.88f, 0.52f, 0.16f);
            trayOutline.effectDistance = new Vector2(1.3f, -1.3f);

            RectTransform trayInner = CreateRect("Inner", tray,
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(5f, 5f), new Vector2(-5f, -5f));
            Image trayInnerBg = trayInner.gameObject.AddComponent<Image>();
            trayInnerBg.color = new Color(0.13f, 0.09f, 0.05f, 0.96f);
            trayInnerBg.raycastTarget = false;

            RectTransform slotRow = CreateRect("SlotRow", bottomRail,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(20f, 0f), new Vector2(-20f, 282f));
            HorizontalLayoutGroup layout = slotRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 8, 8);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            pairSlotRow = slotRow;
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
            RectTransform pill = CreateTopBarModule(parent, name, 180f, 88f, out Image bg);
            backgroundImage = bg;
            Text label = CreateText("Label", pill, alignment, 24, FontStyle.Bold);
            label.text = "-";
            label.color = new Color(0.97f, 0.94f, 0.84f, 1f);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = 24;
            return label;
        }
        private void BuildUtilityControls(Transform parent)
        {
            RectTransform utilityRoot = CreateTopBarModule(parent, "UtilityPill", 194f, 88f, out _);
            HorizontalLayoutGroup layout = utilityRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 6f;
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
            buttonImage.color = new Color(0.26f, 0.18f, 0.08f, 0.96f);
            Outline buttonOutline = buttonRect.gameObject.AddComponent<Outline>();
            buttonOutline.effectColor = new Color(0.96f, 0.86f, 0.44f, 0.14f);
            buttonOutline.effectDistance = new Vector2(0.8f, -0.8f);
            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(onClick);
            Text label = CreateText("Label", buttonRect, TextAnchor.MiddleCenter, 18, FontStyle.Bold);
            label.color = new Color(0.98f, 0.93f, 0.75f, 1f);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = 18;
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
            const float diameter  = 240f;  // inner portrait circle
            const float ringSize  = 264f;  // outer arc/ring (3× original 88px)
            const float hpBarH    = 8f;
            const float badgeSize = 80f;   // reserve badge (proportional to ring)
            float slotW = ringSize + 8f;   // 96px
            float slotH = ringSize + hpBarH + 10f; // 106px

            // Slot root — LayoutElement tells HorizontalLayoutGroup the preferred size.
            RectTransform slotRoot = CreateRect($"Medallion_{laneIndex}", pairSlotRow,
                new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, Vector2.zero);            LayoutElement le = slotRoot.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth  = slotW;
            le.preferredHeight = slotH;
            le.minWidth        = slotW;

            Image slotHitTarget = slotRoot.gameObject.AddComponent<Image>();
            slotHitTarget.color = new Color(0f, 0f, 0f, 0.001f);
            slotHitTarget.raycastTarget = true;
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
            arcFill.color         = new Color(0.42f, 0.7f, 0.94f, 0.92f);
            arcFill.raycastTarget = false;

            RectTransform dividerRoot = CreateRect("AbilityDividerRoot", circleArea,
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            List<RectTransform> abilityDividerPivots = new List<RectTransform>(MaxPortraitChargeDividers);
            for (int dividerIndex = 0; dividerIndex < MaxPortraitChargeDividers; dividerIndex++)
            {
                RectTransform dividerPivot = CreateRect($"DividerPivot_{dividerIndex}", dividerRoot,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                dividerPivot.sizeDelta = new Vector2(ringSize, ringSize);
                RectTransform dividerBar = CreateRect("Bar", dividerPivot,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(-1.2f, -12f), new Vector2(1.2f, -1f));
                Image dividerImage = dividerBar.gameObject.AddComponent<Image>();
                dividerImage.sprite = GetWhiteSprite();
                dividerImage.color = new Color(0.08f, 0.06f, 0.05f, 0.92f);
                dividerImage.raycastTarget = false;
                dividerPivot.gameObject.SetActive(false);
                abilityDividerPivots.Add(dividerPivot);
            }

            RectTransform thresholdMarkerRoot = CreateRect("AbilityThresholdMarker", circleArea,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            thresholdMarkerRoot.sizeDelta = new Vector2(ringSize, ringSize);
            RectTransform thresholdMarkerRect = CreateRect("Marker", thresholdMarkerRoot,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-2.4f, -14f), new Vector2(2.4f, -1f));
            Image thresholdMarker = thresholdMarkerRect.gameObject.AddComponent<Image>();
            thresholdMarker.sprite = GetWhiteSprite();
            thresholdMarker.color = new Color(0.9f, 0.84f, 0.42f, 0.95f);
            thresholdMarker.raycastTarget = false;
            thresholdMarkerRoot.gameObject.SetActive(false);

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

            // 6. Medallion frame — decorative ring overlay showing the N hole positions.
            //    Sprite is assigned at runtime from MedallionFrameCatalog based on SpecialChargeRequirement.
            RectTransform medallionFrameRect = CreateRect("MedallionFrame", circleArea,
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image medallionFrame = medallionFrameRect.gameObject.AddComponent<Image>();
            medallionFrame.raycastTarget  = false;
            medallionFrame.preserveAspect = false;
            medallionFrame.enabled        = false; // hidden until catalog sprite is assigned

            // 7. Medallion orbs — small dot per charge hole; pooled up to MaxPortraitChargeDividers.
            //    Positioned and coloured each poll tick by UpdateMedallionOrbs().
            RectTransform orbRoot = CreateRect("MedallionOrbRoot", circleArea,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            List<Image> medallionOrbs = new List<Image>(MaxPortraitChargeDividers);
            for (int orbIndex = 0; orbIndex < MaxPortraitChargeDividers; orbIndex++)
            {
                RectTransform orbRect = CreateRect($"Orb_{orbIndex}", orbRoot,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                orbRect.sizeDelta = new Vector2(MedallionOrbSize, MedallionOrbSize);
                Image orbImage = orbRect.gameObject.AddComponent<Image>();
                orbImage.sprite        = circleSpr;
                orbImage.color         = new Color(0.18f, 0.18f, 0.22f, 0f);
                orbImage.raycastTarget = false;
                orbRect.gameObject.SetActive(false);
                medallionOrbs.Add(orbImage);
            }

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

            // Medallion frame overlay on the reserve badge — matches the reserve character's hole count.
            RectTransform swapFrameRect = CreateRect("MedallionFrame", swapRect,
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image swapMedallionFrame = swapFrameRect.gameObject.AddComponent<Image>();
            swapMedallionFrame.raycastTarget  = false;
            swapMedallionFrame.preserveAspect = false;
            swapMedallionFrame.enabled        = false;

            swapRect.gameObject.SetActive(false);

            return new PlayerSlotView
            {
                LaneIndex               = laneIndex,
                Root                    = slotRoot,
                FrontPortrait           = portraitImage,
                FrontHpFill             = hpFill,
                ActiveRing              = activeRing,
                AbilityArcFill          = arcFill,
                AbilityDividerPivots    = abilityDividerPivots,
                AbilityThresholdMarkerRoot = thresholdMarkerRoot,
                AbilityThresholdMarker  = thresholdMarker,
                DeadOverlay             = deadOverlay,
                ReserveButton           = swapButton,
                ReserveButtonBackground = swapBg,
                ReservePortrait         = swapPortrait,
                MedallionFrame          = medallionFrame,
                MedallionOrbs           = medallionOrbs,
                LastOrbCount            = -1,
                ReserveMedallionFrame   = swapMedallionFrame,
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

        private static Sprite GetEdgeVignetteSprite()
        {
            if (s_EdgeVignetteSprite != null)
                return s_EdgeVignetteSprite;

            const int width = 128;
            const int height = 4;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float normalized = 1f - (x / (float)(width - 1));
                    float alpha = normalized * normalized * (3f - (2f * normalized));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            s_EdgeVignetteSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            return s_EdgeVignetteSprite;
        }

        private static Sprite GetWhiteSprite()
        {
            if (s_WhiteSprite != null)
                return s_WhiteSprite;

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            s_WhiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            return s_WhiteSprite;
        }

        private static Sprite GetComboCutInStripeSprite()
        {
            if (s_ComboCutInStripeSprite != null)
                return s_ComboCutInStripeSprite;

            const int width = 256;
            const int height = 40;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            tex.wrapMode = TextureWrapMode.Clamp;

            Color outer = new Color(0.96f, 0.36f, 0.04f, 1f);
            Color inner = new Color(1f, 0.86f, 0.2f, 1f);
            Color flare = new Color(1f, 0.97f, 0.72f, 1f);

            for (int y = 0; y < height; y++)
            {
                float vertical = 1f - Mathf.Abs(((y / (float)(height - 1)) * 2f) - 1f);
                vertical = Mathf.Pow(Mathf.Clamp01(vertical), 0.55f);

                for (int x = 0; x < width; x++)
                {
                    float horizontal = x / (float)(width - 1);
                    float flareBand = Mathf.Clamp01(1f - Mathf.Abs((horizontal - 0.5f) * 2.4f));
                    Color pixel = Color.Lerp(outer, inner, vertical);
                    pixel = Color.Lerp(pixel, flare, flareBand * 0.22f);
                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            s_ComboCutInStripeSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, Vector4.zero, false);
            return s_ComboCutInStripeSprite;
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
            RefreshTurnVignette();
        }

        private void RefreshTurnVignette()
        {
            turnVignetteTargetAlpha = ResolveTurnVignetteTargetAlpha();
            if (turnVignetteCanvasGroup == null)
                return;

            if (!Application.isPlaying)
            {
                turnVignetteCurrentAlpha = turnVignetteTargetAlpha;
                turnVignetteCanvasGroup.alpha = turnVignetteCurrentAlpha;
            }
        }

        private void UpdateTurnVignetteVisual()
        {
            if (turnVignetteCanvasGroup == null)
                return;

            float nextAlpha = Mathf.MoveTowards(
                turnVignetteCurrentAlpha,
                turnVignetteTargetAlpha,
                TurnVignetteFadeSpeed * Time.unscaledDeltaTime);

            if (Mathf.Approximately(nextAlpha, turnVignetteCurrentAlpha))
                return;

            turnVignetteCurrentAlpha = nextAlpha;
            turnVignetteCanvasGroup.alpha = turnVignetteCurrentAlpha;
        }

        private float ResolveTurnVignetteTargetAlpha()
        {
            if (turnManager == null || !turnManager.IsBattleActive)
                return 0f;

            CharacterInstance current = turnManager.GetCurrentCombatant();
            if (current == null || !current.IsAlive || !turnManager.IsPlayerUnit(current))
                return 0f;

            return TurnVignetteMaxAlpha;
        }

        private void UpdateComboTrackerVisual()
        {
            if (comboTrackerRoot == null || comboTrackerCanvasGroup == null)
                return;

            if (comboTrackerPulseTimer > 0f)
                comboTrackerPulseTimer = Mathf.Max(0f, comboTrackerPulseTimer - Time.unscaledDeltaTime);

            comboTrackerTargetAlpha = Time.unscaledTime <= comboTrackerHideAt ? 1f : 0f;
            float fadeSpeed = comboTrackerTargetAlpha > comboTrackerCurrentAlpha ? ComboTrackerFadeInSpeed : ComboTrackerFadeOutSpeed;
            comboTrackerCurrentAlpha = Mathf.MoveTowards(comboTrackerCurrentAlpha, comboTrackerTargetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            comboTrackerCanvasGroup.alpha = comboTrackerCurrentAlpha;

            float pulse = 0f;
            if (comboTrackerPulseTimer > 0f && comboTrackerPulseDuration > 0.001f)
            {
                float normalized = 1f - (comboTrackerPulseTimer / comboTrackerPulseDuration);
                pulse = Mathf.Sin(normalized * Mathf.PI) * Mathf.Lerp(0.16f, 0.05f, normalized);
            }

            comboTrackerRoot.localScale = Vector3.one * (1f + pulse);
            float yOffset = -((1f - comboTrackerCurrentAlpha) * 18f) + pulse * 6f;
            comboTrackerRoot.anchoredPosition = comboTrackerBaseAnchoredPosition + new Vector2(0f, yOffset);
        }

        private void ApplyComboTrackerState(int hitCount, bool finished)
        {
            if (comboHitCountLabel == null || comboHitsSuffixLabel == null || comboTypeLabel == null)
                return;

            int visibleHitCount = Mathf.Max(1, hitCount);
            Color accent = ResolveComboTrackerAccentColor(visibleHitCount);

            comboHitCountLabel.text = visibleHitCount.ToString();
            comboHitCountLabel.color = Color.Lerp(new Color(1f, 0.97f, 0.9f, 1f), accent, 0.22f);
            comboHitsSuffixLabel.text = "Hits!";
            comboHitsSuffixLabel.color = Color.Lerp(accent, Color.white, 0.16f);
            comboTypeLabel.text = ResolveComboTierLabel(visibleHitCount);
            comboTypeLabel.color = Color.Lerp(accent, new Color(1f, 0.96f, 0.9f, 1f), 0.08f);

            if (comboTrackerGlowImage != null)
                comboTrackerGlowImage.color = new Color(accent.r, accent.g, accent.b, finished ? 0.16f : 0.1f);

            if (comboTrackerStreakImage != null)
                comboTrackerStreakImage.color = new Color(accent.r, accent.g, accent.b, finished ? 0.34f : 0.24f);

            comboTrackerHideAt = Time.unscaledTime + (finished ? ComboTrackerFinishHoldSeconds : ComboTrackerHitHoldSeconds);
            comboTrackerPulseDuration = finished ? ComboTrackerFinishPulseSeconds : ComboTrackerBasePulseSeconds;
            comboTrackerPulseTimer = comboTrackerPulseDuration;
            comboTrackerTargetAlpha = 1f;
        }

        private void HideComboTrackerImmediate()
        {
            comboTrackerHideAt = float.NegativeInfinity;
            comboTrackerPulseTimer = 0f;
            comboTrackerPulseDuration = ComboTrackerBasePulseSeconds;
            comboTrackerTargetAlpha = 0f;
            comboTrackerCurrentAlpha = 0f;

            if (comboTrackerCanvasGroup != null)
                comboTrackerCanvasGroup.alpha = 0f;

            if (comboTrackerRoot != null)
            {
                comboTrackerRoot.localScale = Vector3.one;
                comboTrackerRoot.anchoredPosition = comboTrackerBaseAnchoredPosition;
            }
        }

        private void PlayComboCutIn(CharacterInstance[] participants)
        {
            if (comboCutInRoot == null || comboCutInCanvasGroup == null || participants == null)
                return;

            List<CharacterInstance> orderedParticipants = OrderComboCutInParticipants(participants);
            int displayCount = ConfigureComboCutInSlots(orderedParticipants);
            if (displayCount < 2)
            {
                HideComboCutInImmediate();
                return;
            }

            if (comboCutInCoroutine != null)
                StopCoroutine(comboCutInCoroutine);

            comboCutInCoroutine = StartCoroutine(AnimateComboCutIn(displayCount));
        }

        private int ConfigureComboCutInSlots(List<CharacterInstance> orderedParticipants)
        {
            if (orderedParticipants == null)
                return 0;

            int displayCount = Mathf.Min(3, orderedParticipants.Count);
            float[] yPositions = displayCount >= 3
                ? new[] { 116f, 0f, -116f }
                : new[] { 58f, -58f };

            for (int i = 0; i < comboCutInSlots.Count; i++)
            {
                ComboCutInSlotView slot = comboCutInSlots[i];
                if (slot == null || slot.Root == null)
                    continue;

                bool active = i < displayCount;
                slot.Root.gameObject.SetActive(active);
                if (!active)
                    continue;

                CharacterInstance participant = orderedParticipants[i];
                Sprite portrait = ResolveComboCutInPortrait(participant);
                Color accent = ResolveComboCutInAccent(participant);

                slot.Root.localRotation = Quaternion.Euler(0f, 0f, ComboCutInRotation);
                slot.Root.anchoredPosition = new Vector2(-1280f, yPositions[i]);

                if (slot.Stripe != null)
                    slot.Stripe.color = Color.Lerp(Color.white, accent, 0.24f);

                if (slot.Shine != null)
                    slot.Shine.color = Color.Lerp(new Color(1f, 0.98f, 0.88f, 0.16f), Color.white, 0.18f + (0.06f * i));

                if (slot.Portrait != null)
                {
                    slot.Portrait.sprite = portrait;
                    slot.Portrait.enabled = portrait != null;
                    slot.Portrait.color = Color.white;
                }

                if (slot.PortraitRect != null)
                    slot.PortraitRect.anchoredPosition = new Vector2(0f, 0f);
            }

            return displayCount;
        }

        private IEnumerator AnimateComboCutIn(int displayCount)
        {
            float[] yPositions = displayCount >= 3
                ? new[] { 116f, 0f, -116f }
                : new[] { 58f, -58f };
            float[] startX = displayCount >= 3
                ? new[] { -1320f, -1220f, -1120f }
                : new[] { -1280f, -1180f };
            float[] holdX = displayCount >= 3
                ? new[] { -28f, 0f, 28f }
                : new[] { -18f, 18f };
            float[] settleX = displayCount >= 3
                ? new[] { 6f, 34f, 62f }
                : new[] { 14f, 50f };
            float[] endX = displayCount >= 3
                ? new[] { 1240f, 1340f, 1440f }
                : new[] { 1300f, 1400f };

            yield return AnimateComboCutInPhase(displayCount, startX, holdX, yPositions, 0f, 1f, ComboCutInIntroSeconds);
            yield return AnimateComboCutInPhase(displayCount, holdX, settleX, yPositions, 1f, 1f, ComboCutInHoldSeconds);
            yield return AnimateComboCutInPhase(displayCount, settleX, endX, yPositions, 1f, 0f, ComboCutInOutroSeconds);

            comboCutInCoroutine = null;
            if (comboCutInCanvasGroup != null)
                comboCutInCanvasGroup.alpha = 0f;

            for (int i = 0; i < comboCutInSlots.Count; i++)
            {
                ComboCutInSlotView slot = comboCutInSlots[i];
                if (slot != null && slot.Root != null)
                    slot.Root.gameObject.SetActive(false);
            }
        }

        private IEnumerator AnimateComboCutInPhase(int displayCount, float[] fromX, float[] toX, float[] yPositions, float startAlpha, float endAlpha, float duration)
        {
            if (comboCutInCanvasGroup == null)
                yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0.0001f ? 1f : Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);

                comboCutInCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);
                for (int i = 0; i < displayCount && i < comboCutInSlots.Count; i++)
                {
                    ComboCutInSlotView slot = comboCutInSlots[i];
                    if (slot == null || slot.Root == null)
                        continue;

                    slot.Root.anchoredPosition = new Vector2(Mathf.Lerp(fromX[i], toX[i], eased), yPositions[i]);
                }

                yield return null;
            }

            comboCutInCanvasGroup.alpha = endAlpha;
            for (int i = 0; i < displayCount && i < comboCutInSlots.Count; i++)
            {
                ComboCutInSlotView slot = comboCutInSlots[i];
                if (slot == null || slot.Root == null)
                    continue;

                slot.Root.anchoredPosition = new Vector2(toX[i], yPositions[i]);
            }
        }

        private void HideComboCutInImmediate()
        {
            if (comboCutInCoroutine != null)
            {
                StopCoroutine(comboCutInCoroutine);
                comboCutInCoroutine = null;
            }

            if (comboCutInCanvasGroup != null)
                comboCutInCanvasGroup.alpha = 0f;

            for (int i = 0; i < comboCutInSlots.Count; i++)
            {
                ComboCutInSlotView slot = comboCutInSlots[i];
                if (slot == null || slot.Root == null)
                    continue;

                slot.Root.gameObject.SetActive(false);
                slot.Root.anchoredPosition = new Vector2(-1280f, 0f);
            }
        }

        private static List<CharacterInstance> OrderComboCutInParticipants(CharacterInstance[] participants)
        {
            List<CharacterInstance> ordered = new List<CharacterInstance>();
            if (participants == null)
                return ordered;

            for (int i = 0; i < participants.Length; i++)
            {
                CharacterInstance participant = participants[i];
                if (participant != null)
                    ordered.Add(participant);
            }

            if (ordered.Count == 3)
                return new List<CharacterInstance> { ordered[1], ordered[0], ordered[2] };

            if (ordered.Count == 2)
                return new List<CharacterInstance> { ordered[1], ordered[0] };

            return ordered;
        }

        private static Sprite ResolveComboCutInPortrait(CharacterInstance participant)
        {
            if (participant != null && participant.Definition != null)
            {
                Sprite cutInSprite = participant.Definition.ComboCutInSprite;
                if (cutInSprite != null)
                    return cutInSprite;
            }

            SpriteRenderer spriteRenderer = participant != null ? participant.GetComponentInChildren<SpriteRenderer>() : null;
            return spriteRenderer != null ? spriteRenderer.sprite : null;
        }

        private static Color ResolveComboCutInAccent(CharacterInstance participant)
        {
            Color baseAccent = new Color(1f, 0.72f, 0.16f, 1f);
            if (participant == null || participant.Definition == null)
                return baseAccent;

            Color accent = participant.Definition.PaletteAccentColor;
            accent.a = 1f;
            return Color.Lerp(baseAccent, accent, 0.28f);
        }

        private static string ResolveComboTierLabel(int hitCount)
        {
            if (hitCount <= 1)
                return string.Empty;
            if (hitCount >= 30)
                return "Endless Combo";
            if (hitCount >= 20)
                return "Savage Combo";
            if (hitCount >= 10)
                return "Ultra Combo";
            if (hitCount >= 5)
                return "Great Combo";
            return "Combo";
        }

        private static Color ResolveComboTrackerAccentColor(int hitCount)
        {
            if (hitCount >= 30)
                return new Color(1f, 0.1f, 0.14f, 1f);
            if (hitCount >= 20)
                return new Color(1f, 0.2f, 0.08f, 1f);
            if (hitCount >= 10)
                return new Color(1f, 0.34f, 0.08f, 1f);
            if (hitCount >= 5)
                return new Color(1f, 0.48f, 0.12f, 1f);
            return new Color(1f, 0.64f, 0.18f, 1f);
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

            if (battleHasEnded)
            {
                turnLabel.text = ResolveEncounterDisplayNameText();
                objectiveLabel.text = battleEndResult == BattleResult.Win ? "VICTORY" : "DEFEAT";

                if (turnPanelBackground != null)
                    turnPanelBackground.color = new Color(0.16f, 0.12f, 0.07f, 0.96f);

                if (objectivePillBackground != null)
                {
                    objectivePillBackground.color = battleEndResult == BattleResult.Win
                        ? new Color(0.2f, 0.28f, 0.12f, 0.96f)
                        : new Color(0.32f, 0.12f, 0.1f, 0.93f);
                }

                RefreshLegendTokens(null);
                UpdateUtilityLabels();
                return;
            }

            CharacterInstance current = turnManager != null ? turnManager.GetCurrentCombatant() : null;
            string currentName = current != null ? ResolveCharacterName(current) : "-";
            turnLabel.text = $"TURN: {currentName}";
            RefreshTurnContextVisuals(current);
            RefreshLegendTokens(current);
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
                    objectiveLabel.text = ResolveEncounterObjectiveStatusText(aliveEnemies, totalEnemies);
                    if (objectivePillBackground != null)
                        objectivePillBackground.color = new Color(0.16f, 0.12f, 0.07f, 0.96f);
                }
            }
            UpdateUtilityLabels();
        }

        private string ResolveEncounterObjectiveStatusText(int aliveEnemies, int totalEnemies)
        {
            if (battleSetup != null)
                return battleSetup.GetEncounterObjectiveStatusText(aliveEnemies, totalEnemies);

            return $"DEFEAT ALL ENEMIES  {aliveEnemies}/{Mathf.Max(1, totalEnemies)}";
        }

        private string ResolveEncounterDisplayNameText()
        {
            string encounterName = battleSetup != null ? battleSetup.GetEncounterDisplayName() : string.Empty;
            if (string.IsNullOrWhiteSpace(encounterName))
                encounterName = "BATTLE RESULT";

            return encounterName.Trim().ToUpperInvariant();
        }
        private void RefreshTurnContextVisuals(CharacterInstance current)
        {
            if (turnPanelBackground == null)
                return;
            Color baseColor = new Color(0.16f, 0.12f, 0.07f, 0.96f);
            Color accent = ResolveCharacterAccentColor(current);
            float blend = 0f;
            if (current != null && turnManager != null)
                blend = turnManager.IsPlayerUnit(current) ? 0.28f : 0.18f;
            turnPanelBackground.color = Color.Lerp(baseColor, accent, blend);
        }
        private void RefreshLegendTokens(CharacterInstance current)
        {
            ElementalType? activeElement = null;
            MartialArtsType? activeWeapon = null;
            if (current != null && current.Definition != null)
            {
                activeElement = current.Definition.ElementalType;
                activeWeapon = current.Definition.MartialArtsType;
            }
            for (int i = 0; i < elementLegendTokens.Count; i++)
                ApplyLegendTokenState(elementLegendTokens[i], activeElement.HasValue && elementLegendTokens[i].ElementalType == activeElement.Value);
            for (int i = 0; i < weaponLegendTokens.Count; i++)
                ApplyLegendTokenState(weaponLegendTokens[i], activeWeapon.HasValue && weaponLegendTokens[i].MartialArtsType == activeWeapon.Value);
        }
        private static void ApplyLegendTokenState(LegendTokenView token, bool highlighted)
        {
            if (token == null)
                return;
            if (token.Root != null)
                token.Root.localScale = highlighted ? new Vector3(1.08f, 1.08f, 1f) : Vector3.one;
            if (token.Background != null)
            {
                Color background = token.BaseColor;
                background.a = highlighted ? 0.96f : 0.48f;
                token.Background.color = background;
            }
            if (token.Outline != null)
                token.Outline.effectColor = highlighted
                    ? new Color(0.99f, 0.92f, 0.62f, 0.84f)
                    : new Color(0.03f, 0.03f, 0.03f, 0.88f);
            if (token.Label != null)
            {
                Color labelColor = token.BaseTextColor;
                labelColor.a = highlighted ? 1f : 0.76f;
                token.Label.color = labelColor;
            }
        }
        private static Color ResolveCharacterAccentColor(CharacterInstance character)
        {
            if (character != null && character.Definition != null)
            {
                Color accent = character.Definition.PaletteAccentColor;
                accent.a = 0.96f;
                return accent;
            }
            return new Color(0.34f, 0.48f, 0.62f, 0.96f);
        }
        private static Color ResolveElementColor(ElementalType elementalType)
        {
            switch (elementalType)
            {
                case ElementalType.Fire:
                    return new Color(0.92f, 0.3f, 0.2f, 1f);
                case ElementalType.Water:
                    return new Color(0.2f, 0.64f, 0.9f, 1f);
                case ElementalType.Earth:
                    return new Color(0.73f, 0.57f, 0.27f, 1f);
                case ElementalType.Wind:
                    return new Color(0.31f, 0.81f, 0.49f, 1f);
                case ElementalType.Lightning:
                    return new Color(0.93f, 0.77f, 0.21f, 1f);
                case ElementalType.Ice:
                    return new Color(0.49f, 0.89f, 0.97f, 1f);
                case ElementalType.Shadow:
                    return new Color(0.52f, 0.34f, 0.78f, 1f);
                default:
                    return new Color(0.82f, 0.82f, 0.82f, 1f);
            }
        }
        private static Color ResolveWeaponLegendColor(MartialArtsType martialArtsType)
        {
            switch (martialArtsType)
            {
                case MartialArtsType.Sword:
                    return new Color(0.74f, 0.8f, 0.9f, 1f);
                case MartialArtsType.Spear:
                    return new Color(0.83f, 0.63f, 0.28f, 1f);
                case MartialArtsType.Bow:
                    return new Color(0.44f, 0.68f, 0.36f, 1f);
                case MartialArtsType.Staff:
                    return new Color(0.39f, 0.72f, 0.84f, 1f);
                case MartialArtsType.DualDaggers:
                    return new Color(0.75f, 0.36f, 0.45f, 1f);
                case MartialArtsType.HeavyWeapons:
                    return new Color(0.72f, 0.47f, 0.25f, 1f);
                case MartialArtsType.Unarmed:
                    return new Color(0.57f, 0.59f, 0.62f, 1f);
                default:
                    return new Color(0.78f, 0.76f, 0.7f, 1f);
            }
        }
        private static string ResolveElementLegendLabel(ElementalType elementalType)
        {
            switch (elementalType)
            {
                case ElementalType.Fire:
                    return "F";
                case ElementalType.Water:
                    return "Wa";
                case ElementalType.Earth:
                    return "E";
                case ElementalType.Wind:
                    return "Wi";
                case ElementalType.Lightning:
                    return "L";
                case ElementalType.Ice:
                    return "I";
                case ElementalType.Shadow:
                    return "Sh";
                default:
                    return "?";
            }
        }
        private static string ResolveWeaponLegendLabel(MartialArtsType martialArtsType)
        {
            switch (martialArtsType)
            {
                case MartialArtsType.Unarmed:
                    return "UN";
                case MartialArtsType.Sword:
                    return "SW";
                case MartialArtsType.Spear:
                    return "SP";
                case MartialArtsType.Bow:
                    return "BW";
                case MartialArtsType.Staff:
                    return "ST";
                case MartialArtsType.DualDaggers:
                    return "DG";
                case MartialArtsType.HeavyWeapons:
                    return "HV";
                default:
                    return "?";
            }
        }
        private static Color ResolveLegendTextColor(Color backgroundColor)
        {
            float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
            return luminance >= 0.62f
                ? new Color(0.16f, 0.11f, 0.07f, 1f)
                : new Color(0.98f, 0.95f, 0.88f, 1f);
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

            CharacterInstance front = battleManager != null ? battleManager.GetActivePlayerAtLane(slot.LaneIndex) : null;
            CharacterInstance reserve = battleManager != null ? battleManager.GetReservePlayerAtLane(slot.LaneIndex) : null;
            CharacterInstance current = turnManager != null ? turnManager.GetCurrentCombatant() : null;

            bool currentPlayerTurn = current != null && turnManager != null && turnManager.IsPlayerUnit(current);
            bool isCurrentLane = currentPlayerTurn
                && battleManager != null
                && battleManager.GetPlayerLaneForCharacter(current) == slot.LaneIndex;
            bool canSwap = currentPlayerTurn
                && !isPaused
                && !autoModeEnabled
                && reserve != null
                && reserve.IsAlive
                && battleManager != null;

            if (front == null)
            {
                slot.FrontPortrait.sprite = null;
                slot.FrontPortrait.color = new Color(1f, 1f, 1f, 0f);
                slot.FrontHpFill.fillAmount = 0f;
                slot.ActiveRing.color = new Color(1f, 1f, 1f, 0f);
                slot.AbilityArcFill.fillAmount = 0f;
                slot.AbilityArcFill.color = new Color(0.42f, 0.7f, 0.94f, 0.24f);
                HideAbilityChargeDividers(slot);
                slot.DeadOverlay.color = new Color(0.08f, 0.05f, 0.05f, 0.5f);
                slot.Root.localScale = Vector3.one;
            }
            else
            {
                float maxHealth = Mathf.Max(1f, front.MaxHealth);
                float healthNormalized = Mathf.Clamp01(front.CurrentHealth / maxHealth);
                Color accent = front.Definition != null
                    ? front.Definition.PaletteAccentColor
                    : new Color(0.45f, 0.82f, 0.96f, 1f);
                accent.a = 1f;

                float pulse01 = 0.5f + (0.5f * Mathf.Sin(Time.unscaledTime * 5.5f));
                bool specialReady = front.IsAlive && front.CanUseSpecialAbility;
                bool ultimateReady = front.IsAlive && front.CanUseUltimateAbility;

                slot.FrontPortrait.sprite = ResolveHudPortrait(front);
                slot.FrontPortrait.color = front.IsAlive ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.9f);

                slot.FrontHpFill.fillAmount = healthNormalized;
                slot.FrontHpFill.color = Color.Lerp(
                    new Color(0.88f, 0.25f, 0.22f, 1f),
                    new Color(0.2f, 0.92f, 0.38f, 1f),
                    healthNormalized);

                UpdateAbilityChargeMeter(slot, front, accent, specialReady, ultimateReady, pulse01);

                slot.ActiveRing.color = isCurrentLane
                    ? new Color(accent.r, accent.g, accent.b, Mathf.Lerp(0.65f, 0.9f, pulse01))
                    : new Color(1f, 1f, 1f, 0f);

                slot.DeadOverlay.color = front.IsAlive
                    ? new Color(0.08f, 0.05f, 0.05f, 0f)
                    : new Color(0.08f, 0.05f, 0.05f, 0.52f);

                slot.Root.localScale = isCurrentLane
                    ? new Vector3(1.08f + (0.02f * pulse01), 1.08f + (0.02f * pulse01), 1f)
                    : Vector3.one;
            }

            if (reserve != null)
            {
                slot.ReserveButton.gameObject.SetActive(true);
                slot.ReserveButton.interactable = canSwap;
                slot.ReservePortrait.sprite = ResolveHudPortrait(reserve);
                slot.ReservePortrait.color = reserve.IsAlive ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.75f);
                if (slot.ReserveButtonBackground != null)
                {
                    slot.ReserveButtonBackground.color = !reserve.IsAlive
                        ? new Color(0.2f, 0.12f, 0.12f, 0.7f)
                        : canSwap
                            ? new Color(0.34f, 0.24f, 0.12f, 0.96f)
                            : new Color(0.22f, 0.18f, 0.12f, 0.88f);
                }
                // Medallion frame on reserve badge — matches the reserve character's charge hole count.
                if (slot.ReserveMedallionFrame != null && medallionCatalog != null)
                {
                    Sprite reserveFrame = medallionCatalog.GetFrame(reserve.SpecialChargeRequirement);
                    slot.ReserveMedallionFrame.sprite  = reserveFrame;
                    slot.ReserveMedallionFrame.enabled = reserveFrame != null;
                }
            }
            else
            {
                slot.ReserveButton.gameObject.SetActive(false);
            }
        }

        private void UpdateAbilityChargeMeter(PlayerSlotView slot, CharacterInstance front, Color accent, bool specialReady, bool ultimateReady, float pulse01)
        {
            if (slot == null || slot.AbilityArcFill == null)
                return;

            if (front == null || !front.IsAlive)
            {
                slot.AbilityArcFill.fillAmount = 0f;
                slot.AbilityArcFill.color = new Color(0.42f, 0.7f, 0.94f, 0.24f);
                HideAbilityChargeDividers(slot);
                UpdateMedallionOrbs(slot, null, false, false, 0f);
                return;
            }

            int maxDividerCount = slot.AbilityDividerPivots != null ? slot.AbilityDividerPivots.Count : 0;
            int totalSegments = Mathf.Clamp(front.UltimateChargeRequirement, 0, maxDividerCount);
            int currentCharge = Mathf.Clamp(front.SpecialCharge, 0, totalSegments);
            int specialThreshold = Mathf.Clamp(front.SpecialChargeRequirement, 0, totalSegments);

            slot.AbilityArcFill.fillAmount = totalSegments > 0 ? currentCharge / (float)totalSegments : 0f;
            slot.AbilityArcFill.color = ultimateReady
                ? Color.Lerp(new Color(1f, 0.68f, 0.18f, 1f), Color.white, 0.18f + (0.22f * pulse01))
                : specialReady
                    ? Color.Lerp(new Color(0.96f, 0.82f, 0.28f, 1f), accent, 0.18f)
                    : new Color(0.42f, 0.7f, 0.94f, 0.92f);

            UpdateAbilityChargeDividers(slot, totalSegments, specialThreshold, specialReady, ultimateReady, pulse01);
            UpdateMedallionOrbs(slot, front, specialReady, ultimateReady, pulse01);
        }

        private void UpdateAbilityChargeDividers(PlayerSlotView slot, int totalSegments, int specialThreshold, bool specialReady, bool ultimateReady, float pulse01)
        {
            if (slot == null || slot.AbilityDividerPivots == null)
                return;

            float step = totalSegments > 0 ? 360f / totalSegments : 0f;
            for (int i = 0; i < slot.AbilityDividerPivots.Count; i++)
            {
                RectTransform dividerPivot = slot.AbilityDividerPivots[i];
                if (dividerPivot == null)
                    continue;

                bool active = i < totalSegments;
                dividerPivot.gameObject.SetActive(active);
                if (active)
                    dividerPivot.localRotation = Quaternion.Euler(0f, 0f, -(step * i));
            }

            if (slot.AbilityThresholdMarkerRoot != null)
            {
                bool showThreshold = specialThreshold > 0 && specialThreshold < totalSegments;
                slot.AbilityThresholdMarkerRoot.gameObject.SetActive(showThreshold);
                if (showThreshold)
                    slot.AbilityThresholdMarkerRoot.localRotation = Quaternion.Euler(0f, 0f, -(step * specialThreshold));
            }

            if (slot.AbilityThresholdMarker != null)
            {
                slot.AbilityThresholdMarker.color = ultimateReady
                    ? Color.Lerp(new Color(1f, 0.94f, 0.72f, 1f), Color.white, 0.18f + (0.2f * pulse01))
                    : specialReady
                        ? new Color(0.96f, 0.86f, 0.44f, 0.98f)
                        : new Color(0.78f, 0.76f, 0.62f, 0.9f);
            }
        }

        private void HideAbilityChargeDividers(PlayerSlotView slot)
        {
            if (slot == null || slot.AbilityDividerPivots == null)
                return;

            for (int i = 0; i < slot.AbilityDividerPivots.Count; i++)
            {
                RectTransform dividerPivot = slot.AbilityDividerPivots[i];
                if (dividerPivot != null)
                    dividerPivot.gameObject.SetActive(false);
            }

            if (slot.AbilityThresholdMarkerRoot != null)
                slot.AbilityThresholdMarkerRoot.gameObject.SetActive(false);
        }

        /// <summary>
        /// Updates the medallion frame sprite and N orbs for the given slot.
        ///
        /// Cycle 1 (charge 0 → specialReq):  orbs fill blue one per turn.
        /// Cycle 2 (charge specialReq → ultimateReq): same N orbs convert red one by one.
        /// When front is null or dead, frame and all orbs are hidden.
        /// </summary>
        private void UpdateMedallionOrbs(PlayerSlotView slot, CharacterInstance front, bool specialReady, bool ultimateReady, float pulse01)
        {
            if (slot == null || slot.MedallionOrbs == null)
                return;

            int N = front != null ? front.SpecialChargeRequirement : 0;

            // Hide everything when the character is absent / dead / has no charge mechanic.
            if (front == null || !front.IsAlive || N <= 0)
            {
                if (slot.MedallionFrame != null)
                    slot.MedallionFrame.enabled = false;
                for (int i = 0; i < slot.MedallionOrbs.Count; i++)
                {
                    if (slot.MedallionOrbs[i] != null)
                        slot.MedallionOrbs[i].gameObject.SetActive(false);
                }
                slot.LastOrbCount = 0;
                return;
            }

            // Re-assign frame sprite and re-position orbs only when N changes.
            if (slot.LastOrbCount != N)
            {
                slot.LastOrbCount = N;

                if (slot.MedallionFrame != null)
                {
                    Sprite frameSpr = medallionCatalog != null ? medallionCatalog.GetFrame(N) : null;
                    slot.MedallionFrame.sprite  = frameSpr;
                    slot.MedallionFrame.enabled = frameSpr != null;
                }

                PositionMedallionOrbs(slot.MedallionOrbs, N);
            }
            else if (slot.MedallionFrame != null)
            {
                slot.MedallionFrame.enabled = slot.MedallionFrame.sprite != null;
            }

            // Determine how many orbs are filled in each cycle.
            // ultimateReq falls back to N if no ultimate is configured (so cycle 2 is never entered).
            int ultimateReq = front.UltimateChargeRequirement > 0 ? front.UltimateChargeRequirement : N;
            int charge      = Mathf.Clamp(front.SpecialCharge, 0, ultimateReq);

            bool  inCycle2     = charge > N;
            int   cycle1Filled = inCycle2 ? N : charge;             // blue orbs earned in cycle 1
            int   cycle2Filled = inCycle2 ? (charge - N) : 0;       // red orbs earned in cycle 2

            // Colour palette — flat, no pulse per user preference
            Color blueEmpty    = new Color(0.18f, 0.18f, 0.22f, 0.45f);
            Color blueFilled   = new Color(0.42f, 0.70f, 0.94f, 0.95f);
            Color blueReadyCol = specialReady && !ultimateReady
                ? new Color(0.96f, 0.82f, 0.28f, 1f)   // gold when special is ready
                : blueFilled;
            Color redFilled    = new Color(0.95f, 0.28f, 0.22f, 0.95f);
            Color redReadyCol  = ultimateReady
                ? new Color(1f, 0.72f, 0.12f, 1f)       // bright gold when ultimate is ready
                : redFilled;

            for (int i = 0; i < slot.MedallionOrbs.Count; i++)
            {
                Image orb = slot.MedallionOrbs[i];
                if (orb == null)
                    continue;

                // Orbs beyond the hole count stay hidden.
                if (i >= N)
                {
                    orb.gameObject.SetActive(false);
                    continue;
                }

                orb.gameObject.SetActive(true);

                if (i < cycle2Filled)
                    orb.color = redReadyCol;                          // cycle 2 — red
                else if (i < cycle1Filled)
                    orb.color = specialReady && !ultimateReady ? blueReadyCol : blueFilled; // cycle 1 — blue
                else
                    orb.color = blueEmpty;                            // unfilled hole
            }
        }

        /// <summary>
        /// Positions N active orbs evenly around a circle of radius MedallionOrbRadius,
        /// starting at the top (12 o'clock) and going clockwise.
        /// Orbs with index >= N are deactivated.
        /// </summary>
        private static void PositionMedallionOrbs(List<Image> orbs, int N)
        {
            if (orbs == null || N <= 0)
                return;

            float angleStep = 360f / N;
            for (int i = 0; i < orbs.Count; i++)
            {
                Image orb = orbs[i];
                if (orb == null)
                    continue;

                if (i >= N)
                {
                    orb.gameObject.SetActive(false);
                    continue;
                }

                // 90° = top in Unity UI (Y-up), subtract to go clockwise.
                float angleDeg = 90f - (angleStep * i);
                float rad = angleDeg * Mathf.Deg2Rad;
                orb.rectTransform.anchoredPosition = new Vector2(
                    Mathf.Cos(rad) * MedallionOrbRadius,
                    Mathf.Sin(rad) * MedallionOrbRadius);
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
            CharacterInstance finalResolvedTarget = null;
            int resolvedHitCount = 0;
            bool comboCutInShown = false;
            CombatComboPresentationBus.Reset();

            attacker.StopMovement();
            CombatMovementUtility.FaceCharacterTowards(attacker, target);

            Vector3 strikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(attacker, target, GetActiveCombatantsExcept(attacker, target));
            float dynamicTravelTime = GetAutoTurnTravelTime(attacker.transform, strikeWorldPos);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", true);

            yield return CombatMovementUtility.MoveCharacterToWorldPosition(attacker.transform, strikeWorldPos, dynamicTravelTime);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", false);

            if (!CombatMovementUtility.IsTargetWithinAttackRange(attacker, target))
            {
                Vector3 correctedStrikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(attacker, target, GetActiveCombatantsExcept(attacker, target));
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
                List<CharacterInstance> comboParticipants = CombatComboUtility.GetPlayerComboParticipants(turnManager, attacker, target);
                if (!comboCutInShown && comboParticipants.Count >= 2)
                {
                    CombatComboPresentationBus.ReportStarted(comboParticipants);
                    comboCutInShown = true;
                }

                attacker.PerformBasicAttack();
                yield return new WaitForSeconds(AutoTurnHitPause);

                if (CombatCriticalSupportUtility.TryResolveBasicHit(battleManager, attacker, target, comboParticipants, out _))
                {
                    finalResolvedTarget = target;
                    resolvedHitCount++;
                    CombatComboPresentationBus.ReportHit(resolvedHitCount);
                }

                for (int participantIndex = 1; participantIndex < comboParticipants.Count; participantIndex++)
                {
                    CharacterInstance ally = comboParticipants[participantIndex];
                    if (ally == null || !ally.IsAlive || target == null || !target.IsAlive)
                        continue;

                    Vector3 allyStartWorldPos = CombatMovementUtility.GetWorldPosition(ally.transform);
                    Animator allyAnimator = ally.GetComponentInChildren<Animator>();
                    Vector3 allyStrikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(ally, target, GetActiveCombatantsExcept(ally, target));
                    float allyTravelTime = GetAutoTurnTravelTime(ally.transform, allyStrikeWorldPos);

                    CombatMovementUtility.FaceCharacterTowards(ally, target);

                    if (allyAnimator != null)
                        allyAnimator.SetBool("isRunning", true);

                    yield return CombatMovementUtility.MoveCharacterToWorldPosition(ally.transform, allyStrikeWorldPos, allyTravelTime);

                    if (allyAnimator != null)
                        allyAnimator.SetBool("isRunning", false);

                    if (ally.IsAlive && target != null && target.IsAlive)
                    {
                        ally.PerformBasicAttack(consumeAttackAction: false);
                        yield return new WaitForSeconds(AutoTurnHitPause);

                        if (CombatCriticalSupportUtility.TryResolveBasicHit(battleManager, ally, target, comboParticipants, out _))
                        {
                            finalResolvedTarget = target;
                            resolvedHitCount++;
                            CombatComboPresentationBus.ReportHit(resolvedHitCount);
                        }
                    }

                    if (ally != null && ally.IsAlive)
                    {
                        if (allyAnimator != null)
                            allyAnimator.SetBool("isRunning", true);

                        yield return CombatMovementUtility.MoveCharacterToWorldPosition(ally.transform, allyStartWorldPos, allyTravelTime);

                        if (allyAnimator != null)
                            allyAnimator.SetBool("isRunning", false);
                    }
                }
            }

            if (resolvedHitCount >= 2 && finalResolvedTarget != null)
            {
                AttackTargetIndicator targetIndicator = finalResolvedTarget.GetComponent<AttackTargetIndicator>()
                    ?? finalResolvedTarget.gameObject.AddComponent<AttackTargetIndicator>();
                targetIndicator.PlayComboBurst(resolvedHitCount);
            }

            CombatComboPresentationBus.ReportFinished(resolvedHitCount);

            yield return new WaitForSeconds(AutoTurnRecoverDelay);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", false);

            if (turnManager != null && turnManager.IsBattleActive && turnManager.GetCurrentCombatant() == attacker)
                turnManager.EndTurn();

            autoTurnCoroutine = null;
        }

        private float GetAutoTurnTravelTime(Transform characterTransform, Vector3 destinationWorldPos)
        {
            float travelDist = Vector2.Distance(
                (Vector2)CombatMovementUtility.GetWorldPosition(characterTransform),
                (Vector2)destinationWorldPos);
            return Mathf.Clamp(travelDist / Mathf.Max(0.1f, AutoTurnMovementSpeed), AutoTurnMinTravelTime, AutoTurnMaxTravelTime);
        }

        private List<CharacterInstance> GetActiveCombatantsExcept(CharacterInstance attacker, CharacterInstance target)
        {
            return CombatComboUtility.GetActiveCombatantsExcept(turnManager, attacker, target);
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

        private static Sprite ResolveHudPortrait(CharacterInstance character)
        {
            if (character == null || character.Definition == null)
                return null;

            return character.Definition.PfpSprite != null
                ? character.Definition.PfpSprite
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
            GameObject canvasGo = new GameObject("BattleHudCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            RectTransform canvasRect = (RectTransform)canvasGo.transform;
            StretchRectTransform(canvasRect);
            canvasRect.localScale = Vector3.one;

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;

            RectTransform safeAreaRoot = CreateRect("UI_SafeAreaPanel", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            StretchRectTransform(safeAreaRoot);
            safeAreaRoot.gameObject.AddComponent<SafeAreaHandler>();
            RectTransform hudRoot = CreateRect("HUD", safeAreaRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            StretchRectTransform(hudRoot);
            RectTransform popupRoot = CreateRect("Popups", safeAreaRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            StretchRectTransform(popupRoot);
            return canvas;
        }

        private sealed class LegendTokenView
        {
            public RectTransform Root;
            public Image Background;
            public Outline Outline;
            public Text Label;
            public Color BaseColor;
            public Color BaseTextColor;
            public ElementalType? ElementalType;
            public MartialArtsType? MartialArtsType;
        }

        private sealed class PlayerSlotView
        {
            public int               LaneIndex;
            public RectTransform     Root;
            public Image             FrontPortrait;
            public Image             FrontHpFill;
            public Image             ActiveRing;
            public Image             AbilityArcFill;
            public List<RectTransform> AbilityDividerPivots;
            public RectTransform     AbilityThresholdMarkerRoot;
            public Image             AbilityThresholdMarker;
            public Image             DeadOverlay;
            public Button            ReserveButton;
            public Image             ReserveButtonBackground;
            public Image             ReservePortrait;
            // Medallion orb system
            public Image             MedallionFrame;
            public List<Image>       MedallionOrbs;
            public int               LastOrbCount;
            // Reserve badge medallion frame
            public Image             ReserveMedallionFrame;
        }

        private sealed class ComboCutInSlotView
        {
            public RectTransform Root;
            public Image Stripe;
            public Image Portrait;
            public RectTransform PortraitRect;
            public Image Shine;
        }

    }

    internal static class CombatComboPresentationBus
    {
        public static event Action<CharacterInstance[]> ComboStarted;
        public static event Action<int> ComboHitRegistered;
        public static event Action<int> ComboSequenceFinished;
        public static event Action ComboPresentationReset;

        public static void Reset()
        {
            Action handler = ComboPresentationReset;
            if (handler != null)
                handler();
        }

        public static void ReportStarted(IReadOnlyList<CharacterInstance> participants)
        {
            if (participants == null || participants.Count < 2)
                return;

            int participantCount = Mathf.Min(3, participants.Count);
            CharacterInstance[] snapshot = new CharacterInstance[participantCount];
            for (int i = 0; i < participantCount; i++)
                snapshot[i] = participants[i];

            Action<CharacterInstance[]> handler = ComboStarted;
            if (handler != null)
                handler(snapshot);
        }

        public static void ReportHit(int hitCount)
        {
            if (hitCount < 1)
                return;

            Action<int> handler = ComboHitRegistered;
            if (handler != null)
                handler(hitCount);
        }

        public static void ReportFinished(int hitCount)
        {
            if (hitCount >= 1)
            {
                Action<int> finishedHandler = ComboSequenceFinished;
                if (finishedHandler != null)
                    finishedHandler(hitCount);
                return;
            }

            Action resetHandler = ComboPresentationReset;
            if (resetHandler != null)
                resetHandler();
        }
    }
    internal static class CombatComboUtility
    {
        public static List<CharacterInstance> GetPlayerComboParticipants(
            TurnManager turnManager,
            CharacterInstance leadAttacker,
            CharacterInstance target)
        {
            List<CharacterInstance> participants = new List<CharacterInstance>();
            if (turnManager == null || leadAttacker == null || target == null || !leadAttacker.IsAlive || !target.IsAlive)
                return participants;

            participants.Add(leadAttacker);

            IReadOnlyList<CharacterInstance> players = turnManager.GetPlayerCombatants();
            if (players == null)
                return participants;

            List<CharacterInstance> eligibleAllies = new List<CharacterInstance>();
            for (int i = 0; i < players.Count; i++)
            {
                CharacterInstance ally = players[i];
                if (ally == null || ally == leadAttacker || !ally.IsAlive)
                    continue;

                if (CombatMovementUtility.IsTargetWithinAttackRange(ally, target))
                    eligibleAllies.Add(ally);
            }

            Vector3 targetCenter = CombatMovementUtility.GetColliderWorldCenter(target);
            eligibleAllies.Sort((left, right) =>
            {
                float leftDistance = Vector2.SqrMagnitude((Vector2)(CombatMovementUtility.GetColliderWorldCenter(left) - targetCenter));
                float rightDistance = Vector2.SqrMagnitude((Vector2)(CombatMovementUtility.GetColliderWorldCenter(right) - targetCenter));
                return leftDistance.CompareTo(rightDistance);
            });

            participants.AddRange(eligibleAllies);
            return participants;
        }

        public static List<CharacterInstance> GetActiveCombatantsExcept(
            TurnManager turnManager,
            CharacterInstance excludedAttacker,
            CharacterInstance excludedTarget)
        {
            List<CharacterInstance> blockers = new List<CharacterInstance>();
            if (turnManager == null)
                return blockers;

            AddCombatants(blockers, turnManager.GetPlayerCombatants(), excludedAttacker, excludedTarget);
            AddCombatants(blockers, turnManager.GetEnemyCombatants(), excludedAttacker, excludedTarget);
            return blockers;
        }

        public static bool TryResolveBasicHit(CharacterInstance attacker, CharacterInstance target)
        {
            return CombatCriticalSupportUtility.TryResolveBasicHit(null, attacker, target, null, out _);
        }

        private static void AddCombatants(
            List<CharacterInstance> blockers,
            IReadOnlyList<CharacterInstance> combatants,
            CharacterInstance excludedAttacker,
            CharacterInstance excludedTarget)
        {
            if (blockers == null || combatants == null)
                return;

            for (int i = 0; i < combatants.Count; i++)
            {
                CharacterInstance combatant = combatants[i];
                if (combatant == null || !combatant.IsAlive || combatant == excludedAttacker || combatant == excludedTarget)
                    continue;

                blockers.Add(combatant);
            }
        }
    }
}












