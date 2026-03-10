using System;
using System.Collections;
using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Runtime-built portrait battle HUD for the active combat slice.
    /// Keeps battlefield readable while exposing turn, objective, and squad health state.
    /// </summary>
    [DefaultExecutionOrder(25)]
    public sealed class BattleHudController : MonoBehaviour
    {
        private const float PollIntervalSeconds = 0.12f;
        private const float SpeedNormal = 1f;
        private const float SpeedFast = 2f;
        private const float AutoTurnLeadDelay = 0.24f;
        private const float AutoTurnTravelTime = 0.26f;
        private const float AutoTurnHitPause = 0.14f;
        private const float AutoTurnRecoverDelay = 0.22f;
        private const float AutoTurnReturnTime = 0.26f;

        [Header("Dependencies (auto-resolved if left empty)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private RectTransform hudPrefab;

        [Header("Layout")]
        [SerializeField] private float topBarHeight = 108f;
        [SerializeField] private float bottomRailHeight = 210f;

        [Header("Boss Detection")]
        [SerializeField] private string[] bossCharacterIds = Array.Empty<string>();
        [SerializeField] private bool singleEnemyCanBeBoss = true;

        private RectTransform hudRoot;
        private Text turnLabel;
        private Text objectiveLabel;
        private Image objectivePillBackground;
        private RectTransform squadRail;
        private RectTransform playerSlotTemplate;
        private Text speedButtonLabel;
        private Text autoButtonLabel;
        private Text menuButtonLabel;

        private readonly List<PlayerSlotView> playerSlots = new List<PlayerSlotView>();
        private readonly Dictionary<CharacterInstance, PlayerSlotView> slotLookup = new Dictionary<CharacterInstance, PlayerSlotView>();
        private readonly Dictionary<CharacterInstance, Action<float>> healthCallbacks = new Dictionary<CharacterInstance, Action<float>>();
        private readonly Dictionary<CharacterInstance, Action> deathCallbacks = new Dictionary<CharacterInstance, Action>();
        private readonly HashSet<string> normalizedBossIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
            BindTurnEvents();
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

            UnbindTurnEvents();
            UnbindCharacterEvents();
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
                if (turnManager != null && turnManager.GetPlayerCombatants().Count > 0)
                    break;

                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private IEnumerator PollHud()
        {
            var wait = new WaitForSecondsRealtime(PollIntervalSeconds);
            while (true)
            {
                RefreshHud();
                yield return wait;
            }
        }

        private void BindTurnEvents()
        {
            if (turnManager == null)
                return;

            turnManager.OnTurnStarted += HandleTurnChanged;
            turnManager.OnTurnEnded += HandleTurnChanged;
            turnManager.OnBattleEnded += HandleBattleEnded;
        }

        private void UnbindTurnEvents()
        {
            if (turnManager == null)
                return;

            turnManager.OnTurnStarted -= HandleTurnChanged;
            turnManager.OnTurnEnded -= HandleTurnChanged;
            turnManager.OnBattleEnded -= HandleBattleEnded;
        }

        private void HandleTurnChanged(CharacterInstance _)
        {
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
            isPaused = false;
            Time.timeScale = SpeedNormal;
            RefreshSlots();
            UpdateUtilityLabels();
        }

        private void BuildHud()
        {
            if (targetCanvas == null)
                return;

            if (hudRoot != null)
                return;

            if (TryBuildHudFromPrefab())
                return;

            BuildRuntimeHud();
        }

        private bool TryBuildHudFromPrefab()
        {
            if (hudPrefab == null)
                return false;

            hudRoot = Instantiate(hudPrefab, targetCanvas.transform, false);
            hudRoot.name = hudPrefab.name;

            turnLabel = FindChildComponent<Text>(hudRoot, "TopCombatBar/TurnPill/Label");
            objectiveLabel = FindChildComponent<Text>(hudRoot, "TopCombatBar/ObjectivePill/Label");
            objectivePillBackground = FindChildComponent<Image>(hudRoot, "TopCombatBar/ObjectivePill");
            squadRail = FindChildComponent<RectTransform>(hudRoot, "BottomSquadRail");
            playerSlotTemplate = FindChildComponent<RectTransform>(hudRoot, "BottomSquadRail/PlayerSlotTemplate");

            Button speedButton = FindChildComponent<Button>(hudRoot, "TopCombatBar/UtilityPill/SpeedButton");
            Button autoButton = FindChildComponent<Button>(hudRoot, "TopCombatBar/UtilityPill/AutoButton");
            Button menuButton = FindChildComponent<Button>(hudRoot, "TopCombatBar/UtilityPill/MenuButton");

            speedButtonLabel = FindChildComponent<Text>(hudRoot, "TopCombatBar/UtilityPill/SpeedButton/Label");
            autoButtonLabel = FindChildComponent<Text>(hudRoot, "TopCombatBar/UtilityPill/AutoButton/Label");
            menuButtonLabel = FindChildComponent<Text>(hudRoot, "TopCombatBar/UtilityPill/MenuButton/Label");

            bool isValid = turnLabel != null
                           && objectiveLabel != null
                           && squadRail != null
                           && speedButton != null
                           && autoButton != null
                           && menuButton != null
                           && speedButtonLabel != null
                           && autoButtonLabel != null
                           && menuButtonLabel != null;

            if (!isValid)
            {
                Debug.LogWarning("[BattleHudController] HUD prefab is missing required bindings. Falling back to runtime-generated HUD.");
                Destroy(hudRoot.gameObject);
                hudRoot = null;
                turnLabel = null;
                objectiveLabel = null;
                objectivePillBackground = null;
                squadRail = null;
                playerSlotTemplate = null;
                speedButtonLabel = null;
                autoButtonLabel = null;
                menuButtonLabel = null;
                return false;
            }

            if (playerSlotTemplate != null)
                playerSlotTemplate.gameObject.SetActive(false);

            speedButton.onClick.RemoveListener(OnSpeedButtonPressed);
            speedButton.onClick.AddListener(OnSpeedButtonPressed);

            autoButton.onClick.RemoveListener(OnAutoButtonPressed);
            autoButton.onClick.AddListener(OnAutoButtonPressed);

            menuButton.onClick.RemoveListener(OnMenuButtonPressed);
            menuButton.onClick.AddListener(OnMenuButtonPressed);

            UpdateUtilityLabels();
            return true;
        }

        private static T FindChildComponent<T>(Transform root, string relativePath) where T : Component
        {
            if (root == null || string.IsNullOrEmpty(relativePath))
                return null;

            Transform child = root.Find(relativePath);
            return child != null ? child.GetComponent<T>() : null;
        }

        private void BuildRuntimeHud()
        {
            hudRoot = CreateRect("BattleHudRoot", targetCanvas.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            RectTransform topBar = CreateRect("TopCombatBar", hudRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -topBarHeight - 12f), new Vector2(-20f, -12f));
            Image topBarBg = topBar.gameObject.AddComponent<Image>();
            topBarBg.color = new Color(0.05f, 0.07f, 0.1f, 0.62f);
            topBarBg.raycastTarget = false;

            var topLayout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            topLayout.padding = new RectOffset(14, 14, 10, 10);
            topLayout.spacing = 10f;
            topLayout.childForceExpandWidth = true;
            topLayout.childForceExpandHeight = true;

            turnLabel = CreatePillLabel(topBar, "TurnPill", TextAnchor.MiddleLeft);
            objectiveLabel = CreatePillLabel(topBar, "ObjectivePill", TextAnchor.MiddleCenter, out objectivePillBackground);
            BuildUtilityControls(topBar);

            RectTransform bottomRail = CreateRect("BottomSquadRail", hudRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20f, 12f), new Vector2(-20f, bottomRailHeight + 12f));
            Image bottomBg = bottomRail.gameObject.AddComponent<Image>();
            bottomBg.color = new Color(0.08f, 0.06f, 0.04f, 0.72f);
            bottomBg.raycastTarget = false;

            var bottomLayout = bottomRail.gameObject.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.padding = new RectOffset(20, 20, 18, 18);
            bottomLayout.spacing = 16f;
            bottomLayout.childAlignment = TextAnchor.MiddleCenter;
            bottomLayout.childControlWidth = false;
            bottomLayout.childControlHeight = false;
            bottomLayout.childForceExpandWidth = false;
            bottomLayout.childForceExpandHeight = false;

            squadRail = bottomRail;
            playerSlotTemplate = null;
        }

        private Text CreatePillLabel(Transform parent, string name, TextAnchor alignment)
        {
            return CreatePillLabel(parent, name, alignment, out _);
        }

        private Text CreatePillLabel(Transform parent, string name, TextAnchor alignment, out Image backgroundImage)
        {
            RectTransform pill = CreateRect(name, parent, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            var layoutElement = pill.gameObject.AddComponent<LayoutElement>();
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
            var layoutElement = utilityRoot.gameObject.AddComponent<LayoutElement>();
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
            UpdateUtilityLabels();
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
            if (squadRail == null || turnManager == null)
                return;

            for (int i = squadRail.childCount - 1; i >= 0; i--)
                Destroy(squadRail.GetChild(i).gameObject);

            playerSlots.Clear();
            slotLookup.Clear();
            UnbindCharacterEvents();

            IReadOnlyList<CharacterInstance> players = turnManager.GetPlayerCombatants();
            for (int i = 0; i < players.Count; i++)
            {
                CharacterInstance player = players[i];
                if (player == null)
                    continue;

                PlayerSlotView slot = CreatePlayerSlot(player, i);
                playerSlots.Add(slot);
                slotLookup[player] = slot;
                BindCharacterEvents(player);
            }
        }

        private PlayerSlotView CreatePlayerSlot(CharacterInstance player, int index)
        {
            RectTransform slotRoot = CreateRect($"PlayerSlot_{index}", squadRail, new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, Vector2.zero);
            slotRoot.sizeDelta = new Vector2(190f, bottomRailHeight - 36f);

            var layoutElement = slotRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 190f;
            layoutElement.preferredHeight = bottomRailHeight - 36f;

            Image background = slotRoot.gameObject.AddComponent<Image>();
            background.color = new Color(0.16f, 0.14f, 0.1f, 0.86f);
            background.raycastTarget = false;

            Outline outline = slotRoot.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.55f);

            RectTransform activeFrame = CreateRect("ActiveFrame", slotRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-4f, -4f), new Vector2(4f, 4f));
            Image frameImage = activeFrame.gameObject.AddComponent<Image>();
            frameImage.color = new Color(0.5f, 0.7f, 0.85f, 0f);
            frameImage.raycastTarget = false;

            RectTransform portraitRect = CreateRect("Portrait", slotRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-68f, -140f), new Vector2(68f, -14f));
            Image portraitImage = portraitRect.gameObject.AddComponent<Image>();
            portraitImage.raycastTarget = false;
            portraitImage.preserveAspect = true;
            portraitImage.sprite = player.Definition?.Portrait != null ? player.Definition.Portrait : player.Definition?.BattleSprite;
            portraitImage.color = Color.white;

            Text nameLabel = CreateText("Name", slotRoot, TextAnchor.MiddleCenter, 18, FontStyle.Bold);
            RectTransform nameRect = (RectTransform)nameLabel.transform;
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0f);
            nameRect.offsetMin = new Vector2(10f, 44f);
            nameRect.offsetMax = new Vector2(-10f, 76f);

            RectTransform hpBarBgRect = CreateRect("HpBarBg", slotRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 12f), new Vector2(-16f, 34f));
            Image hpBarBg = hpBarBgRect.gameObject.AddComponent<Image>();
            hpBarBg.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            hpBarBg.raycastTarget = false;

            RectTransform hpFillRect = CreateRect("HpFill", hpBarBgRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f));
            Image hpFill = hpFillRect.gameObject.AddComponent<Image>();
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            hpFill.fillAmount = 1f;
            hpFill.color = new Color(0.2f, 0.92f, 0.38f, 0.98f);
            hpFill.raycastTarget = false;

            Text hpLabel = CreateText("HpText", slotRoot, TextAnchor.MiddleCenter, 16, FontStyle.Bold);
            RectTransform hpRect = (RectTransform)hpLabel.transform;
            hpRect.anchorMin = new Vector2(0f, 0f);
            hpRect.anchorMax = new Vector2(1f, 0f);
            hpRect.offsetMin = new Vector2(10f, 34f);
            hpRect.offsetMax = new Vector2(-10f, 56f);

            Text stateLabel = CreateText("State", slotRoot, TextAnchor.UpperRight, 20, FontStyle.Bold);
            RectTransform stateRect = (RectTransform)stateLabel.transform;
            stateRect.anchorMin = new Vector2(0f, 1f);
            stateRect.anchorMax = new Vector2(1f, 1f);
            stateRect.offsetMin = new Vector2(10f, -34f);
            stateRect.offsetMax = new Vector2(-10f, -8f);
            stateLabel.color = new Color(1f, 0.45f, 0.35f, 1f);

            return new PlayerSlotView
            {
                Character = player,
                Root = slotRoot,
                ActiveFrame = frameImage,
                Portrait = portraitImage,
                NameLabel = nameLabel,
                HpFill = hpFill,
                HpLabel = hpLabel,
                StateLabel = stateLabel
            };
        }
        private bool TryCreatePlayerSlotFromTemplate(CharacterInstance player, int index, out PlayerSlotView slot)
        {
            slot = null;
            if (playerSlotTemplate == null || squadRail == null)
                return false;

            RectTransform slotRoot = Instantiate(playerSlotTemplate, squadRail, false);
            slotRoot.name = $"PlayerSlot_{index}";
            slotRoot.gameObject.SetActive(true);

            Image frameImage = FindChildComponent<Image>(slotRoot, "ActiveFrame");
            Image portraitImage = FindChildComponent<Image>(slotRoot, "Portrait");
            Text nameLabel = FindChildComponent<Text>(slotRoot, "Name");
            Image hpFill = FindChildComponent<Image>(slotRoot, "HpBarBg/HpFill");
            Text hpLabel = FindChildComponent<Text>(slotRoot, "HpText");
            Text stateLabel = FindChildComponent<Text>(slotRoot, "State");

            bool isValid = frameImage != null
                           && portraitImage != null
                           && nameLabel != null
                           && hpFill != null
                           && hpLabel != null
                           && stateLabel != null;

            if (!isValid)
            {
                Destroy(slotRoot.gameObject);
                Debug.LogWarning("[BattleHudController] PlayerSlotTemplate is missing one or more required child bindings. Falling back to runtime slot generation.");
                return false;
            }

            portraitImage.preserveAspect = true;
            portraitImage.sprite = player.Definition?.Portrait != null ? player.Definition.Portrait : player.Definition?.BattleSprite;
            portraitImage.color = Color.white;
            stateLabel.color = new Color(1f, 0.45f, 0.35f, 1f);

            slot = new PlayerSlotView
            {
                Character = player,
                Root = slotRoot,
                ActiveFrame = frameImage,
                Portrait = portraitImage,
                NameLabel = nameLabel,
                HpFill = hpFill,
                HpLabel = hpLabel,
                StateLabel = stateLabel
            };

            return true;
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

        private void BindCharacterEvents(CharacterInstance character)
        {
            if (character == null || healthCallbacks.ContainsKey(character))
                return;

            Action<float> healthCallback = _ => RefreshSlot(character);
            Action deathCallback = () => RefreshSlot(character);

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
            RefreshSlots();
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
            for (int i = 0; i < playerSlots.Count; i++)
                RefreshSlot(playerSlots[i].Character);
        }

        private void RefreshSlot(CharacterInstance character)
        {
            if (character == null || !slotLookup.TryGetValue(character, out PlayerSlotView slot))
                return;

            float maxHealth = Mathf.Max(1f, character.MaxHealth);
            float healthNormalized = Mathf.Clamp01(character.CurrentHealth / maxHealth);
            bool isActive = turnManager != null && turnManager.GetCurrentCombatant() == character && character.IsAlive;

            Color accent = character.Definition != null ? character.Definition.PaletteAccentColor : new Color(0.45f, 0.82f, 0.96f, 1f);
            accent.a = 1f;

            slot.NameLabel.text = ResolveCharacterName(character);
            slot.HpFill.fillAmount = healthNormalized;
            slot.HpFill.color = Color.Lerp(new Color(0.88f, 0.25f, 0.22f, 1f), new Color(0.2f, 0.92f, 0.38f, 1f), healthNormalized);
            slot.HpLabel.text = $"{Mathf.CeilToInt(character.CurrentHealth)}/{Mathf.CeilToInt(maxHealth)}";

            if (!character.IsAlive)
            {
                slot.StateLabel.text = "KO";
                slot.Portrait.color = new Color(0.58f, 0.58f, 0.58f, 0.85f);
                slot.ActiveFrame.color = new Color(0.32f, 0.22f, 0.22f, 0.55f);
                slot.Root.localScale = Vector3.one;
                return;
            }

            slot.StateLabel.text = isActive ? "ACTIVE" : string.Empty;
            slot.Portrait.color = Color.white;
            slot.ActiveFrame.color = isActive
                ? new Color(accent.r, accent.g, accent.b, 0.72f)
                : new Color(0.34f, 0.36f, 0.42f, 0.25f);
            slot.Root.localScale = isActive ? new Vector3(1.06f, 1.06f, 1f) : Vector3.one;
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
            isPaused = !isPaused;
            ApplyTimeScale();
            UpdateUtilityLabels();
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
                menuButtonLabel.text = isPaused ? "RESUME" : "MENU";
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
            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", true);

            yield return CombatMovementUtility.MoveCharacterToWorldPosition(attacker.transform, strikeWorldPos, AutoTurnTravelTime);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", false);

            if (attacker.CanAttack)
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

        private static IEnumerator MoveCharacterToWorldPosition(Transform characterTransform, Vector3 worldPos, float duration)
        {
            Vector3 startWorldPos = characterTransform.position;
            float elapsed = 0f;
            float clampedDuration = Mathf.Max(0.01f, duration);

            while (elapsed < clampedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);
                characterTransform.position = Vector3.Lerp(startWorldPos, worldPos, t);
                yield return null;
            }

            characterTransform.position = worldPos;
        }

        private static Vector3 GetAttackApproachPosition(CharacterInstance attacker, CharacterInstance target)
        {
            Vector3 attackerCenter = GetColliderWorldCenter(attacker);
            Vector3 targetCenter = GetColliderWorldCenter(target);
            Vector3 direction = targetCenter - attackerCenter;
            if (direction.sqrMagnitude <= 0.0001f)
                direction = target.transform.position.x >= attacker.transform.position.x ? Vector3.right : Vector3.left;

            direction.Normalize();

            float separation = Mathf.Max(0.1f, GetColliderHalfWidth(attacker) + GetColliderHalfWidth(target) - 0.05f);
            Vector3 desiredCenter = targetCenter - direction * separation;
            Vector3 attackerCenterOffset = attackerCenter - attacker.transform.position;
            return desiredCenter - attackerCenterOffset;
        }

        private static Vector3 GetColliderWorldCenter(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col != null)
                return character.transform.TransformPoint(col.offset);
            return character != null ? character.transform.position : Vector3.zero;
        }

        private static float GetColliderHalfWidth(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col == null)
                return 0.35f;

            Vector3 lossy = character.transform.lossyScale;
            float scaledWidth = Mathf.Abs(col.size.x * lossy.x);
            return Mathf.Max(0.05f, scaledWidth * 0.5f);
        }

        private static void FaceCharacterTowards(CharacterInstance attacker, CharacterInstance target)
        {
            if (attacker == null || target == null)
                return;

            float direction = target.transform.position.x < attacker.transform.position.x ? -1f : 1f;
            if (attacker.Definition != null && attacker.Definition.InvertFacingX)
                direction *= -1f;

            Vector3 scale = attacker.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            attacker.transform.localScale = scale;
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
            public CharacterInstance Character;
            public RectTransform Root;
            public Image ActiveFrame;
            public Image Portrait;
            public Text NameLabel;
            public Image HpFill;
            public Text HpLabel;
            public Text StateLabel;
        }
    }


    internal static class CombatMovementUtility
    {
        private const float AttackSeparationPadding = 0.12f;
        private const float SlotSpacingScale = 1.15f;
        private const float MinimumSlotSpacing = 0.95f;
        private const int CandidateSlotDepth = 5;
        private const float CandidateDistancePenalty = 0.08f;
        private const float SlotIndexPenalty = 0.05f;
        private const float BlockerClearancePadding = 0.24f;
        private const float OverlapPenaltyScale = 4f;

        private static readonly int[] SlotOrder = BuildSlotOrder();

        public static IEnumerator MoveCharacterToWorldPosition(Transform characterTransform, Vector3 worldPos, float duration)
        {
            Vector3 startWorldPos = GetWorldPosition(characterTransform);
            float elapsed = 0f;
            float clampedDuration = Mathf.Max(0.01f, duration);

            while (elapsed < clampedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);
                Vector3 nextWorldPos = Vector3.Lerp(startWorldPos, worldPos, t);
                SetWorldPosition(characterTransform, nextWorldPos);
                yield return null;
            }

            SetWorldPosition(characterTransform, worldPos);
        }

        public static Vector3 GetAttackApproachPosition(CharacterInstance attacker, CharacterInstance target, IEnumerable<CharacterInstance> blockers = null)
        {
            if (attacker == null || target == null)
                return Vector3.zero;

            Vector3 attackerWorldPos = GetWorldPosition(attacker.transform);
            Vector3 attackerCenter = GetColliderWorldCenter(attacker);
            Vector3 targetCenter = GetColliderWorldCenter(target);
            Vector3 attackerCenterOffset = attackerCenter - attackerWorldPos;

            float horizontalDirection = ResolveHorizontalAttackDirection(attacker, attackerCenter, targetCenter);
            float separation = Mathf.Max(0.2f, GetVisualHalfWidth(attacker) + GetVisualHalfWidth(target) + AttackSeparationPadding);
            float slotSpacing = Mathf.Max(MinimumSlotSpacing, GetFootprintRadius(attacker) * SlotSpacingScale);

            Vector3 bestWorldPos = attackerWorldPos;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < SlotOrder.Length; i++)
            {
                int slotIndex = SlotOrder[i];
                Vector3 desiredCenter = new Vector3(
                    targetCenter.x - horizontalDirection * separation,
                    targetCenter.y + slotIndex * slotSpacing,
                    targetCenter.z);

                Vector3 candidateWorldPos = desiredCenter - attackerCenterOffset;
                candidateWorldPos.z = attackerWorldPos.z;

                float candidateScore = ScoreCandidate(candidateWorldPos, attacker, target, blockers, attackerCenterOffset, attackerWorldPos, slotIndex);
                if (candidateScore > bestScore)
                {
                    bestScore = candidateScore;
                    bestWorldPos = candidateWorldPos;
                }
            }

            bestWorldPos.z = attackerWorldPos.z;
            return bestWorldPos;
        }

        public static void FaceCharacterTowards(CharacterInstance attacker, CharacterInstance target)
        {
            if (attacker == null || target == null)
                return;

            float direction = target.transform.position.x < attacker.transform.position.x ? -1f : 1f;
            if (attacker.Definition != null && attacker.Definition.InvertFacingX)
                direction *= -1f;

            Vector3 scale = attacker.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            attacker.transform.localScale = scale;
        }

        public static Vector3 GetWorldPosition(Transform characterTransform)
        {
            if (characterTransform == null)
                return Vector3.zero;

            Transform parent = characterTransform.parent;
            return parent != null ? parent.TransformPoint(characterTransform.localPosition) : characterTransform.position;
        }

        public static void SetWorldPosition(Transform characterTransform, Vector3 worldPos)
        {
            if (characterTransform == null)
                return;

            Transform parent = characterTransform.parent;
            if (parent != null)
                characterTransform.localPosition = parent.InverseTransformPoint(worldPos);
            else
                characterTransform.position = worldPos;
        }

        public static Vector3 GetColliderWorldCenter(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col != null)
                return character.transform.TransformPoint(col.offset);

            return character != null ? GetWorldPosition(character.transform) : Vector3.zero;
        }

        public static float GetColliderHalfWidth(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col == null)
                return 0.35f;

            Vector3 lossy = character.transform.lossyScale;
            float scaledWidth = Mathf.Abs(col.size.x * lossy.x);
            return Mathf.Max(0.05f, scaledWidth * 0.5f);
        }

        public static float GetColliderHalfHeight(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col == null)
                return 0.45f;

            Vector3 lossy = character.transform.lossyScale;
            float scaledHeight = Mathf.Abs(col.size.y * lossy.y);
            return Mathf.Max(0.05f, scaledHeight * 0.5f);
        }

        public static float GetVisualHalfWidth(CharacterInstance character)
        {
            GetSpriteExtents(character, out float spriteHalfWidth, out _);
            return Mathf.Max(GetColliderHalfWidth(character), spriteHalfWidth);
        }

        public static float GetVisualHalfHeight(CharacterInstance character)
        {
            GetSpriteExtents(character, out _, out float spriteHalfHeight);
            return Mathf.Max(GetColliderHalfHeight(character), spriteHalfHeight);
        }

        public static float GetFootprintRadius(CharacterInstance character)
        {
            float halfWidth = GetVisualHalfWidth(character);
            float halfHeight = GetVisualHalfHeight(character);
            return Mathf.Max(halfWidth, halfHeight * 0.8f);
        }

        private static float ScoreCandidate(
            Vector3 candidateWorldPos,
            CharacterInstance attacker,
            CharacterInstance target,
            IEnumerable<CharacterInstance> blockers,
            Vector3 attackerCenterOffset,
            Vector3 attackerWorldPos,
            int slotIndex)
        {
            Vector3 candidateCenter = candidateWorldPos + attackerCenterOffset;
            float minClearance = float.MaxValue;
            float overlapPenalty = 0f;

            if (blockers != null)
            {
                foreach (CharacterInstance blocker in blockers)
                {
                    if (blocker == null || !blocker.IsAlive || blocker == attacker || blocker == target)
                        continue;

                    float requiredSpacing = GetFootprintRadius(attacker) + GetFootprintRadius(blocker) + BlockerClearancePadding;
                    float clearance = Vector2.Distance((Vector2)candidateCenter, (Vector2)GetColliderWorldCenter(blocker)) - requiredSpacing;
                    minClearance = Mathf.Min(minClearance, clearance);

                    if (clearance < 0f)
                        overlapPenalty += -clearance * OverlapPenaltyScale;
                }
            }

            if (minClearance == float.MaxValue)
                minClearance = 2f;

            float distancePenalty = Vector2.Distance((Vector2)candidateWorldPos, (Vector2)attackerWorldPos) * CandidateDistancePenalty;
            float slotPenalty = Mathf.Abs(slotIndex) * SlotIndexPenalty;
            return minClearance - overlapPenalty - distancePenalty - slotPenalty;
        }

        private static float ResolveHorizontalAttackDirection(CharacterInstance attacker, Vector3 attackerCenter, Vector3 targetCenter)
        {
            float deltaX = targetCenter.x - attackerCenter.x;
            if (Mathf.Abs(deltaX) > 0.02f)
                return Mathf.Sign(deltaX);

            if (attacker != null && Mathf.Abs(attacker.transform.localScale.x) > 0.001f)
                return Mathf.Sign(attacker.transform.localScale.x);

            return 1f;
        }

        private static int[] BuildSlotOrder()
        {
            int[] slotOrder = new int[CandidateSlotDepth * 2 + 1];
            int nextIndex = 0;
            slotOrder[nextIndex++] = 0;

            for (int i = 1; i <= CandidateSlotDepth; i++)
            {
                slotOrder[nextIndex++] = i;
                slotOrder[nextIndex++] = -i;
            }

            return slotOrder;
        }

        private static void GetSpriteExtents(CharacterInstance character, out float halfWidth, out float halfHeight)
        {
            halfWidth = 0f;
            halfHeight = 0f;
            if (character == null)
                return;

            SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>();
            Bounds combinedBounds = default;
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null || !renderer.enabled)
                    continue;

                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                return;

            Vector3 extents = combinedBounds.extents;
            halfWidth = Mathf.Max(0.05f, Mathf.Abs(extents.x));
            halfHeight = Mathf.Max(0.05f, Mathf.Abs(extents.y));
        }
    }
}
