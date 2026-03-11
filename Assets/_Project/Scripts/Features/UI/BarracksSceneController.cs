using System;
using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shogun.Features.UI
{
    [DefaultExecutionOrder(20)]
    public sealed partial class BarracksSceneController : MonoBehaviour
    {
        private const string BarracksSceneName = "Barracks";
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
        private HorizontalLayoutGroup detailLayout;
        private GridLayoutGroup rosterGrid;
        private RectTransform rosterViewport;
        private Text ownedCountLabel;
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

        private void Start()
        {
            if (!SceneManager.GetActiveScene().name.Equals(BarracksSceneName, StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                return;
            }

            ResolveCanvas();
            ResolveOwnedCharacters();
            BuildScreen();
            SelectCharacter(Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, ownedCharacters.Count - 1)));
            ApplyResponsiveLayout(true);
        }

        private void Update() => ApplyResponsiveLayout(false);

        private void ResolveCanvas()
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogWarning("[BarracksSceneController] No Canvas found. Creating fallback canvas.");
                targetCanvas = CreateFallbackCanvas();
            }

            Transform safeAreaPanel = targetCanvas.transform.Find("UI_SafeAreaPanel") ?? targetCanvas.transform;
            Transform menuRoot = safeAreaPanel.Find("Menu_Main");
            hostRoot = (menuRoot as RectTransform) ?? (safeAreaPanel as RectTransform);
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
            detailMetadataLabel.text = BarracksCharacterPresentation.BuildMetadataText(selected);
            detailStatsLabel.text = BarracksCharacterPresentation.BuildStatsText(selected);
            detailSpecialLabel.text = BarracksCharacterPresentation.BuildSpecialText(selected);

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
            detailSubtitleLabel.text = "The barracks placeholder roster did not resolve any character definitions.";
            detailTaglineLabel.text = "Check CharacterCatalog plus the default debug owned ids.";
            detailLoreLabel.text = "This screen expects a temporary ownership list until the real player inventory/save layer exists.";
            detailMetadataLabel.text = "No detail metadata available.";
            detailStatsLabel.text = "No combat data available.";
            detailSpecialLabel.text = "No ability data available.";
            ClearChildren(detailChipRow);
            detailPortraitImage.sprite = null;
            detailPortraitImage.enabled = false;
            detailPortraitPlaceholder.SetActive(true);
            detailPortraitPlaceholderLabel.text = "--";
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (contentFrame == null || rosterGrid == null || hostRoot == null)
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
            rosterGrid.constraintCount = contentWidth >= 1120f ? 4 : contentWidth >= 860f ? 3 : 2;

            int columns = rosterGrid.constraintCount;
            float viewportWidth = Mathf.Max(0f, rosterViewport.rect.width - 20f);
            float availableWidth = Mathf.Max(0f, viewportWidth - (rosterGrid.spacing.x * (columns - 1)));
            float cellWidth = Mathf.Floor(availableWidth / columns);
            rosterGrid.cellSize = new Vector2(Mathf.Max(120f, cellWidth), contentWidth >= 860f ? 176f : 164f);
            if (detailLayout != null)
                detailLayout.spacing = contentWidth >= 1100f ? 30f : 22f;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentFrame);
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

