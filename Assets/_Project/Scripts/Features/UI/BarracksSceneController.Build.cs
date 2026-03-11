using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Shogun.Features.UI
{
    public sealed partial class BarracksSceneController
    {
        private void BuildScreen()
        {
            ClearChildren(hostRoot);
            screenRoot = CreateRect("BarracksScreenRoot", hostRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image backdrop = screenRoot.gameObject.AddComponent<Image>();
            backdrop.sprite = GetWhiteSprite();
            backdrop.type = Image.Type.Simple;
            backdrop.color = BackgroundColor;

            RectTransform glow = CreateRect("BackdropGlow", screenRoot, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.9f), Vector2.zero, Vector2.zero);
            Image glowImage = glow.gameObject.AddComponent<Image>();
            glowImage.sprite = GetWhiteSprite();
            glowImage.color = new Color(0.38f, 0.27f, 0.11f, 0.12f);

            contentFrame = CreateRect("ContentFrame", screenRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup contentLayout = contentFrame.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 24, 24);
            contentLayout.spacing = 18f;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            BuildHeader();
            BuildDetailPanel();
            BuildRosterSection();
        }

        private void BuildHeader()
        {
            RectTransform outer = CreatePanel(contentFrame, "HeaderOuter", 164f, HeaderOuterColor, HeaderInnerColor, out RectTransform inner);
            RectTransform sheen = CreateRect("TopSheen", inner, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -7f), new Vector2(-12f, -1f));
            Image sheenImage = sheen.gameObject.AddComponent<Image>();
            sheenImage.sprite = GetWhiteSprite();
            sheenImage.color = new Color(0.95f, 0.86f, 0.53f, 0.16f);

            RectTransform content = CreateRect("HeaderContent", inner, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(24f, 18f), new Vector2(-24f, -18f));
            RectTransform titleBlock = CreateRect("TitleBlock", content, new Vector2(0f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            Text title = CreateText("Title", titleBlock, TextAnchor.UpperLeft, 40, FontStyle.Bold);
            title.text = "BARRACKS";
            title.color = new Color(0.98f, 0.94f, 0.84f, 1f);
            title.rectTransform.offsetMin = new Vector2(0f, 62f);
            Text subtitle = CreateText("Subtitle", titleBlock, TextAnchor.LowerLeft, 18, FontStyle.Normal);
            subtitle.text = "Owned warriors, collectible identity, and battle-readiness at a glance.";
            subtitle.color = MutedTextColor;
            subtitle.rectTransform.offsetMax = new Vector2(0f, -68f);

            RectTransform status = CreateRect("StatusBlock", content, new Vector2(0.72f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform ownedPill = CreatePill(status, "OwnedPill", new Vector2(0.14f, 0.46f), new Vector2(1f, 1f), new Color(0.3f, 0.23f, 0.14f, 0.95f));
            ownedCountLabel = CreateText("OwnedCount", ownedPill, TextAnchor.MiddleCenter, 20, FontStyle.Bold);
            ownedCountLabel.text = $"OWNED {ownedCharacters.Count}";
            ownedCountLabel.color = new Color(0.99f, 0.95f, 0.83f, 1f);
            RectTransform sourcePill = CreatePill(status, "SourcePill", new Vector2(0.14f, 0f), new Vector2(1f, 0.4f), new Color(0.16f, 0.12f, 0.1f, 0.96f));
            Text source = CreateText("SourceLabel", sourcePill, TextAnchor.MiddleCenter, 14, FontStyle.Bold);
            source.text = "DEBUG OWNERSHIP PLACEHOLDER";
            source.color = SecondaryTextColor;
        }

        private void BuildDetailPanel()
        {
            RectTransform outer = CreatePanel(contentFrame, "DetailOuter", 544f, PanelOuterColor, PanelInnerColor, out RectTransform inner);
            detailAccentBand = CreateRect("DetailAccentBand", inner, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(10f, 0f)).gameObject.AddComponent<Image>();
            detailAccentBand.sprite = GetWhiteSprite();
            RectTransform content = CreateRect("DetailContent", inner, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(28f, 24f), new Vector2(-28f, -24f));
            detailLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            detailLayout.spacing = 24f;
            detailLayout.childControlWidth = false;
            detailLayout.childForceExpandWidth = false;

            RectTransform portraitColumn = CreateRect("PortraitColumn", content, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            LayoutElement portraitLayout = portraitColumn.gameObject.AddComponent<LayoutElement>();
            portraitLayout.preferredWidth = 320f;
            portraitLayout.minWidth = 280f;
            RectTransform portraitOuter = CreateRect("PortraitOuter", portraitColumn, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image portraitOuterImage = portraitOuter.gameObject.AddComponent<Image>();
            portraitOuterImage.sprite = GetWhiteSprite();
            portraitOuterImage.color = new Color(0.17f, 0.12f, 0.09f, 1f);
            RectTransform portraitInner = CreateRect("PortraitInner", portraitOuter, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image portraitInnerImage = portraitInner.gameObject.AddComponent<Image>();
            portraitInnerImage.sprite = GetWhiteSprite();
            portraitInnerImage.color = new Color(0.1f, 0.09f, 0.09f, 1f);
            RectTransform portraitArt = CreateRect("PortraitArt", portraitInner, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -18f));
            detailPortraitImage = portraitArt.gameObject.AddComponent<Image>();
            detailPortraitImage.preserveAspect = true;
            detailPortraitPlaceholder = CreatePlaceholder(portraitArt, out detailPortraitPlaceholderLabel);

            RectTransform info = CreateRect("InfoColumn", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            info.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup stack = info.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12f;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            RectTransform nameBlock = CreateRect("NameBlock", info, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            nameBlock.gameObject.AddComponent<LayoutElement>().preferredHeight = 116f;
            detailNameLabel = CreateText("Name", nameBlock, TextAnchor.UpperLeft, 38, FontStyle.Bold);
            detailNameLabel.color = new Color(0.98f, 0.95f, 0.84f, 1f);
            detailNameLabel.rectTransform.offsetMin = new Vector2(0f, 34f);
            detailSubtitleLabel = CreateText("Subtitle", nameBlock, TextAnchor.MiddleLeft, 20, FontStyle.Bold);
            detailSubtitleLabel.color = MutedTextColor;
            detailSubtitleLabel.rectTransform.offsetMin = new Vector2(0f, -6f);
            detailSubtitleLabel.rectTransform.offsetMax = new Vector2(0f, -52f);
            detailTaglineLabel = CreateText("Tagline", nameBlock, TextAnchor.LowerLeft, 16, FontStyle.Normal);
            detailTaglineLabel.color = SecondaryTextColor;
            detailTaglineLabel.rectTransform.offsetMax = new Vector2(0f, -86f);
            detailChipRow = CreateRect("ChipRow", info, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup chipLayout = detailChipRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            chipLayout.spacing = 10f;
            chipLayout.childControlWidth = false;
            chipLayout.childForceExpandWidth = false;
            detailChipRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;
            detailLoreLabel = CreateTextBlock(info, "Lore", 18, 158f, MutedTextColor);
            detailMetadataLabel = CreateTextBlock(info, "Metadata", 16, 134f, SecondaryTextColor);
            detailStatsLabel = CreateTextBlock(info, "Stats", 18, 86f, new Color(0.95f, 0.9f, 0.78f, 1f));
            detailSpecialLabel = CreateTextBlock(info, "Specials", 16, 94f, new Color(0.89f, 0.84f, 0.74f, 1f));
        }

        private void BuildRosterSection()
        {
            RectTransform outer = CreatePanel(contentFrame, "RosterOuter", 0f, PanelOuterColor, PanelInnerColor, out RectTransform inner, true, 420f);
            RectTransform titleRow = CreateRect("RosterTitleRow", inner, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -86f), new Vector2(-24f, -20f));
            Text title = CreateText("Title", titleRow, TextAnchor.UpperLeft, 28, FontStyle.Bold);
            title.text = "OWNED CHARACTERS";
            title.color = new Color(0.98f, 0.94f, 0.84f, 1f);
            title.rectTransform.offsetMin = new Vector2(0f, 22f);
            Text subtitle = CreateText("Subtitle", titleRow, TextAnchor.LowerLeft, 15, FontStyle.Normal);
            subtitle.text = "Collection-facing placeholder roster built from the current six-unit debug player pool.";
            subtitle.color = SecondaryTextColor;
            subtitle.rectTransform.offsetMax = new Vector2(0f, -38f);
            RectTransform divider = CreateRect("Divider", inner, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -96f), new Vector2(-24f, -94f));
            Image dividerImage = divider.gameObject.AddComponent<Image>();
            dividerImage.sprite = GetWhiteSprite();
            dividerImage.color = SoftLineColor;

            RectTransform scrollRoot = CreateRect("RosterScrollView", inner, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -112f));
            ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            rosterViewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image viewportImage = rosterViewport.gameObject.AddComponent<Image>();
            viewportImage.sprite = GetWhiteSprite();
            viewportImage.color = new Color(0.06f, 0.05f, 0.05f, 0.18f);
            Mask mask = rosterViewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            RectTransform content = CreateRect("Content", rosterViewport, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(-10f, 0f));
            content.pivot = new Vector2(0.5f, 1f);
            scrollRect.viewport = rosterViewport;
            scrollRect.content = content;
            rosterGrid = content.gameObject.AddComponent<GridLayoutGroup>();
            rosterGrid.spacing = new Vector2(14f, 14f);
            rosterGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rosterGrid.constraintCount = 2;
            rosterGrid.cellSize = new Vector2(280f, 172f);
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            cardViews.Clear();
            for (int i = 0; i < ownedCharacters.Count; i++)
                cardViews.Add(CreateCharacterCard(content, ownedCharacters[i], i));
        }

        private CardView CreateCharacterCard(Transform parent, CharacterDefinition definition, int index)
        {
            RectTransform cardRoot = CreateRect($"Card_{definition.CharacterId}", parent, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            cardRoot.gameObject.AddComponent<LayoutElement>().preferredHeight = 172f;
            Image background = cardRoot.gameObject.AddComponent<Image>();
            background.sprite = GetWhiteSprite();
            background.color = new Color(0.12f, 0.1f, 0.09f, 0.96f);
            Button button = cardRoot.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            int capturedIndex = index;
            button.onClick.AddListener(() => SelectCharacter(capturedIndex));
            Outline outline = cardRoot.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
            outline.effectDistance = new Vector2(2f, -2f);

            Image accent = CreateRect("Accent", cardRoot, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(8f, 0f)).gameObject.AddComponent<Image>();
            accent.sprite = GetWhiteSprite();
            accent.color = definition.PaletteAccentColor;
            RectTransform content = CreateRect("Content", cardRoot, Vector2.zero, Vector2.one, new Vector2(18f, 16f), new Vector2(-16f, -16f));
            RectTransform portraitRect = CreateRect("Portrait", content, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, -62f), new Vector2(110f, 62f));
            Image portraitFrame = portraitRect.gameObject.AddComponent<Image>();
            portraitFrame.sprite = GetWhiteSprite();
            portraitFrame.color = new Color(0.18f, 0.14f, 0.11f, 1f);
            RectTransform portraitInner = CreateRect("PortraitInner", portraitRect, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image portraitImage = portraitInner.gameObject.AddComponent<Image>();
            portraitImage.preserveAspect = true;
            Text placeholderLabel;
            GameObject placeholder = CreatePlaceholder(portraitInner, out placeholderLabel, 30);
            BarracksCharacterPresentation.SetPortraitVisual(definition, portraitImage, placeholder, placeholderLabel, 30);

            RectTransform info = CreateRect("Info", content, Vector2.zero, Vector2.one, new Vector2(126f, 0f), Vector2.zero);
            Text name = CreateText("Name", info, TextAnchor.UpperLeft, 24, FontStyle.Bold);
            name.text = BarracksCharacterPresentation.GetDisplayName(definition).ToUpperInvariant();
            name.color = new Color(0.98f, 0.94f, 0.84f, 1f);
            name.rectTransform.offsetMin = new Vector2(0f, 74f);
            Text subtitle = CreateText("Subtitle", info, TextAnchor.MiddleLeft, 16, FontStyle.Bold);
            subtitle.text = $"{BarracksCharacterPresentation.GetElementLabel(definition.ElementalType)} • {BarracksCharacterPresentation.GetWeaponLabel(definition.MartialArtsType)} • {BarracksCharacterPresentation.GetRarityLabel(definition.Rarity)}";
            subtitle.color = MutedTextColor;
            subtitle.rectTransform.offsetMin = new Vector2(0f, 28f);
            subtitle.rectTransform.offsetMax = new Vector2(0f, -44f);
            Text summary = CreateText("Summary", info, TextAnchor.LowerLeft, 14, FontStyle.Normal);
            summary.text = BarracksCharacterPresentation.GetCardSummary(definition);
            summary.color = SecondaryTextColor;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            summary.verticalOverflow = VerticalWrapMode.Overflow;
            summary.rectTransform.offsetMax = new Vector2(0f, -90f);

            return new CardView { Definition = definition, Background = background, Accent = accent, NameLabel = name };
        }
    }
}


