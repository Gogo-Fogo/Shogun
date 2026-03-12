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
    public sealed partial class BarracksSceneController : MonoBehaviour
    {
        private const string BarracksSceneName = "Barracks";
        private const bool PreferMinimalBarracksLayout = true;
        private const string DedicatedCanvasName = "BarracksCanvas";
        private const float ContentHorizontalMargin = 28f;
        private const float PhoneContentMaxWidth = 980f;
        private const float TabletContentMaxWidth = 1220f;
        private const float ExpandedContentMaxWidth = 1400f;
        private const float TabletBreakpoint = 1280f;
        private const float ExpandedBreakpoint = 1680f;

        private static readonly string[] DefaultOwnedCharacterIds = { "ryoma", "daichi", "harada", "katsuro", "takeshi", "okami-jin" };
        private static readonly Color BackgroundColor = new Color(0.07f, 0.05f, 0.05f, 1f);
        private static readonly Color HeaderOuterColor = new Color(0.23f, 0.18f, 0.11f, 0.98f);
        private static readonly Color HeaderInnerColor = new Color(0.11f, 0.08f, 0.07f, 0.98f);
        private static readonly Color PanelOuterColor = new Color(0.26f, 0.2f, 0.12f, 0.96f);
        private static readonly Color PanelInnerColor = new Color(0.1f, 0.08f, 0.07f, 0.97f);
        private static readonly Color MutedTextColor = new Color(0.84f, 0.79f, 0.7f, 1f);
        private static readonly Color SecondaryTextColor = new Color(0.72f, 0.69f, 0.64f, 1f);
        private static readonly Color SoftLineColor = new Color(0.8f, 0.68f, 0.36f, 0.36f);
        private static readonly Color PlaceholderPortraitColor = new Color(0.18f, 0.14f, 0.12f, 1f);
        private static Sprite s_WhiteSprite;
        private static Font s_RuntimeFont;

        private Canvas targetCanvas;
        private RectTransform hostRoot;
        private RectTransform screenRoot;
        private RectTransform contentFrame;
        private VerticalLayoutGroup detailLayout;
        private GridLayoutGroup rosterGrid;
        private RectTransform rosterViewport;
        private ScrollRect screenScrollRect;
        private Text ownedCountLabel;
        private Text elementCoverageLabel;
        private Text collectionDepthLabel;
        private Image detailPortraitImage;
        private GameObject detailPortraitPlaceholder;
        private Text detailPortraitPlaceholderLabel;
        private Image detailAccentBand;
        private Text detailNameLabel;
        private Text detailSubtitleLabel;
        private Text detailLoreLabel;
        private Text detailMetadataLabel;
        private Text detailStatsLabel;
        private Text detailSpecialLabel;
        private Text detailTaglineLabel;
        private RectTransform detailChipRow;
        private readonly List<CharacterDefinition> ownedCharacters = new List<CharacterDefinition>();
        private readonly List<CardView> cardViews = new List<CardView>();
        private int selectedIndex;
        private Vector2Int lastScreenSize = new Vector2Int(-1, -1);
        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private float lastParentWidth = -1f;
        private bool usingEmergencyLayout;
        private string diagnosticStatus = "Not started";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllerExists()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.name.Equals(BarracksSceneName, StringComparison.OrdinalIgnoreCase))
                return;
            if (FindFirstObjectByType<BarracksSceneController>() != null)
                return;
            new GameObject("BarracksSceneController").AddComponent<BarracksSceneController>();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                RebuildEditorPreview();
        }
        private void Start()
        {
            if (!SceneManager.GetActiveScene().name.Equals(BarracksSceneName, StringComparison.OrdinalIgnoreCase))
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
            string info = $"[Barracks Diag]\n{diagnosticStatus}\n" +
                          $"Canvas: {(targetCanvas != null ? $"active={targetCanvas.isActiveAndEnabled}, mode={targetCanvas.renderMode}" : "NULL")}\n" +
                          $"Screen: {Screen.width}x{Screen.height}";
            GUI.Box(new Rect(10, 10, 500, 120), info, style);
        }
        #endif

        private void RebuildEditorPreview()
        {
            if (!isActiveAndEnabled)
                return;
            if (!SceneManager.GetActiveScene().name.Equals(BarracksSceneName, StringComparison.OrdinalIgnoreCase))
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
                    Debug.LogError("[BarracksSceneController] FAIL: No canvas found or created.");
                    return;
                }
                diagnosticStatus = "ResolveCanvas complete.";
                usingEmergencyLayout = false;
                ResolveOwnedCharacters();
                diagnosticStatus = $"Owned characters: {ownedCharacters.Count}. Building layout...";
                if (PreferMinimalBarracksLayout)
                {
                    BuildEmergencyFallback(null);
                    SelectCharacter(Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, ownedCharacters.Count - 1)));
                    ApplyResponsiveLayout(true);
                    diagnosticStatus = $"Minimal layout built. {ownedCharacters.Count} characters.";
                    return;
                }
                BuildScreen();
                SelectCharacter(Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, ownedCharacters.Count - 1)));
                ApplyResponsiveLayout(true);
                diagnosticStatus = $"Full layout built. {ownedCharacters.Count} characters.";
            }
            catch (Exception exception)
            {
                diagnosticStatus = $"EXCEPTION: {exception.GetType().Name}: {exception.Message}";
                Debug.LogError($"[BarracksSceneController] BuildSceneContents failed: {exception}");
                try
                {
                    BuildEmergencyFallback(exception);
                    SelectCharacter(Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, ownedCharacters.Count - 1)));
                    ApplyResponsiveLayout(true);
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError($"[BarracksSceneController] Emergency fallback also failed: {fallbackEx}");
                }
            }
        }

        private void ResolveCanvas()
        {
            targetCanvas = FindDedicatedCanvas();
            if (targetCanvas == null)
            {
                Debug.LogWarning("[BarracksSceneController] Dedicated barracks canvas was not found. Creating one now.");
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
        private void ResolveOwnedCharacters()
        {
            ownedCharacters.Clear();
            ownedCharacters.AddRange(TestCollectionService.GetOwnedCharacterDefinitions());
        }

        private void SelectCharacter(int index)
        {
            if (ownedCharacters.Count == 0)
            {
                PopulateEmptyState();
                return;
            }

            selectedIndex = Mathf.Clamp(index, 0, ownedCharacters.Count - 1);
            CharacterDefinition selected = ownedCharacters[selectedIndex];
            for (int i = 0; i < cardViews.Count; i++)
            {
                CardView card = cardViews[i];
                bool isSelected = i == selectedIndex;
                Color accent = card.Definition.PaletteAccentColor;
                card.Accent.color = isSelected ? accent : new Color(accent.r * 0.68f, accent.g * 0.68f, accent.b * 0.68f, 0.9f);
                card.Background.color = isSelected ? new Color(0.2f, 0.16f, 0.11f, 0.98f) : new Color(0.12f, 0.1f, 0.09f, 0.96f);
                card.NameLabel.color = isSelected ? Color.white : new Color(0.94f, 0.9f, 0.8f, 1f);
            }

            detailAccentBand.color = selected.PaletteAccentColor;
            detailNameLabel.text = BarracksCharacterPresentation.GetDisplayName(selected).ToUpperInvariant();
            detailSubtitleLabel.text = $"{BarracksCharacterPresentation.GetElementLabel(selected.ElementalType)} / {BarracksCharacterPresentation.GetWeaponLabel(selected.MartialArtsType)} / {BarracksCharacterPresentation.GetRarityLabel(selected.Rarity)}";
            detailTaglineLabel.text = BarracksCharacterPresentation.BuildTagline(selected);
            detailLoreLabel.text = BarracksCharacterPresentation.BuildLoreText(selected);
            int ownedCopies = TestCollectionService.GetOwnedCount(selected.CharacterId);
            detailMetadataLabel.text = $"{BarracksCharacterPresentation.BuildMetadataText(selected)}\nOwned Copies: {ownedCopies}";
            detailStatsLabel.text = BarracksCharacterPresentation.BuildStatsText(selected);
            detailSpecialLabel.text = BarracksCharacterPresentation.BuildSpecialText(selected);
            RefreshDetailTextLayout();

            ClearChildren(detailChipRow);
            CreateChip(detailChipRow, BarracksCharacterPresentation.GetElementLabel(selected.ElementalType), BarracksCharacterPresentation.GetElementColor(selected.ElementalType));
            CreateChip(detailChipRow, BarracksCharacterPresentation.GetWeaponLabel(selected.MartialArtsType), new Color(0.43f, 0.34f, 0.2f, 1f));
            CreateChip(detailChipRow, BarracksCharacterPresentation.GetRarityLabel(selected.Rarity), BarracksCharacterPresentation.GetRarityColor(selected.Rarity));
            CreateChip(detailChipRow, selected.CollectibleFantasy.ToString().Replace('_', ' ').ToUpperInvariant(), new Color(0.26f, 0.21f, 0.16f, 1f));
            BarracksCharacterPresentation.SetPortraitVisual(selected, detailPortraitImage, detailPortraitPlaceholder, detailPortraitPlaceholderLabel, 64);
        }

        private void PopulateEmptyState()
        {
            detailAccentBand.color = new Color(0.62f, 0.53f, 0.34f, 1f);
            detailNameLabel.text = "NO OWNED UNITS";
            detailSubtitleLabel.text = "No characters are currently registered in the local collection.";
            detailTaglineLabel.text = "Use the Summon Gate or reset the test collection to repopulate the barracks.";
            detailLoreLabel.text = "The barracks is ready to display owned warriors as soon as the collection save resolves unit data again.";
            detailMetadataLabel.text = "Collection profile unavailable.";
            detailStatsLabel.text = "Combat stats unavailable.";
            detailSpecialLabel.text = "Ability data unavailable.";
            RefreshDetailTextLayout();
            ClearChildren(detailChipRow);
            detailPortraitImage.sprite = null;
            detailPortraitImage.enabled = false;
            detailPortraitPlaceholder.SetActive(true);
            detailPortraitPlaceholderLabel.text = "--";
        }

        private void RefreshDetailTextLayout()
        {
            RefreshTextBlockHeight(detailLoreLabel, 72f, 8f);
            RefreshTextBlockHeight(detailMetadataLabel, 104f, 8f);
            RefreshTextBlockHeight(detailStatsLabel, 42f, 6f);
            RefreshTextBlockHeight(detailSpecialLabel, 148f, 8f);
        }

        private static void RefreshTextBlockHeight(Text label, float minHeight, float padding)
        {
            if (label == null)
                return;

            LayoutElement layout = label.GetComponent<LayoutElement>();
            if (layout == null)
                return;

            layout.minHeight = minHeight;
            layout.preferredHeight = Mathf.Max(minHeight, label.preferredHeight + padding);
        }

        private int GetElementCoverageCount()
        {
            HashSet<ElementalType> elements = new HashSet<ElementalType>();
            for (int i = 0; i < ownedCharacters.Count; i++)
                elements.Add(ownedCharacters[i].ElementalType);
            return elements.Count;
        }

        private Rarity GetHighestOwnedRarity()
        {
            Rarity highest = Rarity.Common;
            for (int i = 0; i < ownedCharacters.Count; i++)
            {
                if (ownedCharacters[i].Rarity > highest)
                    highest = ownedCharacters[i].Rarity;
            }

            return highest;
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (contentFrame == null || hostRoot == null)
                return;

            Vector2Int currentScreenSize = GetLayoutSize();
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
            if (usingEmergencyLayout)
            {
                if (screenRoot != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(screenRoot);
                ResetScreenScrollIfNeeded();
                return;
            }
            RefreshDetailTextLayout();
            if (rosterGrid == null || rosterViewport == null)
            {
                if (detailLayout != null)
                    detailLayout.spacing = contentWidth >= 1100f ? 30f : 22f;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
                ResetScreenScrollIfNeeded();
                return;
            }
            rosterGrid.constraintCount = contentWidth >= 1120f ? 4 : contentWidth >= 860f ? 3 : 2;

            int columns = rosterGrid.constraintCount;
            float viewportWidth = Mathf.Max(0f, rosterViewport.rect.width - 20f);
            float availableWidth = Mathf.Max(0f, viewportWidth - (rosterGrid.spacing.x * (columns - 1)));
            float cellWidth = Mathf.Floor(availableWidth / columns);
            rosterGrid.cellSize = new Vector2(Mathf.Max(120f, cellWidth), contentWidth >= 860f ? 176f : 164f);
            if (detailLayout != null)
                detailLayout.spacing = contentWidth >= 1100f ? 30f : 22f;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
            ResetScreenScrollIfNeeded();
        }

        private void ResetScreenScrollIfNeeded()
        {
            if (screenScrollRect == null || screenScrollRect.content == null)
                return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(screenScrollRect.content);
            screenScrollRect.StopMovement();
            screenScrollRect.verticalNormalizedPosition = 1f;
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

        private sealed class CardView
        {
            public CharacterDefinition Definition;
            public Image Background;
            public Image Accent;
            public Text NameLabel;
        }
    }
}









