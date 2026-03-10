using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Grounded battle strip shown beneath each active combatant.
    /// Shows a compact HP bar, an element-colored martial-arts badge, and the upcoming turn count.
    /// </summary>
    public sealed class TurnCountdownIndicator : MonoBehaviour
    {
        private const float StripGapBelowFeet = 0.4f;
        private const float BarHeight = 0.22f;
        private const float BarMinWidth = 1.02f;
        private const float BarMaxWidth = 1.78f;
        private const float BarWidthScale = 0.9f;
        private const float BarFramePadding = 0.03f;
        private const float BarInnerPadding = 0.026f;
        private const float BadgeSize = 0.32f;
        private const float BadgeOutlineScale = 1.2f;
        private const float BadgeGapFromBar = 0.09f;
        private const float TurnGapFromBar = 0.24f;
        private const float TurnTextScale = 0.0175f;
        private const float BadgeTextScale = 0.0125f;
        private const int TurnFontSize = 128;
        private const int BadgeFontSize = 110;
        private const float OutlineTextOffset = 0.09f;
        private const float TurnBlinkSpeed = 8f;
        private const float SortDepthOffset = -0.18f;

        private static readonly Vector3[] TextOutlineOffsets =
        {
            new Vector3(OutlineTextOffset, 0f, 0.02f),
            new Vector3(-OutlineTextOffset, 0f, 0.02f),
            new Vector3(0f, OutlineTextOffset, 0.02f),
            new Vector3(0f, -OutlineTextOffset, 0.02f)
        };

        private static readonly Color BarFrameColor = new Color(0.03f, 0.03f, 0.04f, 0.96f);
        private static readonly Color BarBackgroundColor = new Color(0.16f, 0.11f, 0.07f, 0.92f);
        private static readonly Color BadgeOutlineColor = new Color(0.03f, 0.03f, 0.04f, 0.96f);
        private static readonly Color BadgeTextColor = new Color(1f, 0.98f, 0.9f, 1f);
        private static readonly Color TextOutlineColor = new Color(0.02f, 0.02f, 0.03f, 0.98f);
        private static readonly Color TurnTextColor = new Color(1f, 0.97f, 0.9f, 1f);
        private static readonly Color NextTurnTextColor = new Color(1f, 0.2f, 0.16f, 1f);

        private static Sprite squareSprite;
        private static Sprite circleSprite;

        private CharacterInstance target;
        private int turnsUntilTurn = -1;
        private bool isCurrentTurn;
        private bool isVisible;

        private SpriteRenderer badgeOutlineRenderer;
        private SpriteRenderer badgeFillRenderer;
        private TextMesh badgeText;
        private TextMesh[] badgeTextOutlines;

        private SpriteRenderer barFrameRenderer;
        private SpriteRenderer barBackgroundRenderer;
        private SpriteRenderer barFillRenderer;

        private TextMesh turnText;
        private TextMesh[] turnTextOutlines;

        private void Awake()
        {
            BuildVisuals();
            SetVisible(false);
        }

        public void Initialize(CharacterInstance targetInstance, bool enemyUnit)
        {
            target = targetInstance;
            UpdateWorldPlacement();
            ApplyIdentityVisuals();
            ApplySortingFromTarget();
        }

        public void RefreshState(int turnsUntil, bool shouldShowDanger, bool activeTurn)
        {
            turnsUntilTurn = turnsUntil;
            isCurrentTurn = activeTurn;
            isVisible = target != null && target.IsAlive;

            if (!isVisible)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            ApplyIdentityVisuals();
            UpdateWorldPlacement();
            UpdateHealthBar();
            UpdateTurnVisuals();
            ApplySortingFromTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            if (!target.IsAlive || !isVisible)
                return;

            UpdateWorldPlacement();
            UpdateHealthBar();
            UpdateTurnVisuals();
            ApplySortingFromTarget();
        }

        private void BuildVisuals()
        {
            Sprite square = GetSquareSprite();
            Sprite circle = GetCircleSprite();

            badgeOutlineRenderer = CreateSpriteRenderer("BadgeOutline", circle, BadgeOutlineColor);
            badgeFillRenderer = CreateSpriteRenderer("BadgeFill", circle, Color.white);
            badgeText = CreateOutlinedText("BadgeText", BadgeFontSize, BadgeTextScale, out badgeTextOutlines);

            barFrameRenderer = CreateSpriteRenderer("BarFrame", square, BarFrameColor);
            barBackgroundRenderer = CreateSpriteRenderer("BarBackground", square, BarBackgroundColor);
            barFillRenderer = CreateSpriteRenderer("BarFill", square, Color.green);

            turnText = CreateOutlinedText("TurnText", TurnFontSize, TurnTextScale, out turnTextOutlines);
        }

        private void UpdateWorldPlacement()
        {
            if (target == null)
                return;

            Bounds groundBounds = GetGroundBounds(target, out bool hasGroundBounds);
            if (!hasGroundBounds)
            {
                transform.position = target.transform.position + new Vector3(0f, -0.36f, SortDepthOffset);
                return;
            }

            Vector3 groundAnchor = ResolveGroundAnchor(target, groundBounds);
            float barWidth = ResolveBarWidth(target, groundBounds);
            float badgeCenterX = -barWidth * 0.5f - BadgeGapFromBar - BadgeSize * 0.5f;
            float turnX = barWidth * 0.5f + TurnGapFromBar;
            float stripCenterY = groundAnchor.y - ResolveStripGap(groundBounds) - BarHeight * 0.5f;

            transform.position = new Vector3(groundAnchor.x, stripCenterY, target.transform.position.z + SortDepthOffset);

            badgeOutlineRenderer.transform.localPosition = new Vector3(badgeCenterX, 0f, 0f);
            badgeOutlineRenderer.transform.localScale = Vector3.one * (BadgeSize * BadgeOutlineScale);
            badgeFillRenderer.transform.localPosition = new Vector3(badgeCenterX, 0f, -0.001f);
            badgeFillRenderer.transform.localScale = Vector3.one * BadgeSize;
            badgeText.transform.localPosition = new Vector3(badgeCenterX, 0.002f, -0.01f);

            barFrameRenderer.transform.localPosition = Vector3.zero;
            barFrameRenderer.transform.localScale = new Vector3(barWidth + BarFramePadding * 2f, BarHeight + BarFramePadding * 2f, 1f);
            barBackgroundRenderer.transform.localPosition = Vector3.zero;
            barBackgroundRenderer.transform.localScale = new Vector3(barWidth, BarHeight, 1f);

            turnText.transform.localPosition = new Vector3(turnX, 0.002f, -0.01f);
        }

        private void UpdateHealthBar()
        {
            if (target == null)
                return;

            float maxHealth = Mathf.Max(1f, target.MaxHealth);
            float healthNormalized = Mathf.Clamp01(target.CurrentHealth / maxHealth);
            float barWidth = barBackgroundRenderer.transform.localScale.x;
            float fillWidth = Mathf.Max(0.0001f, (barWidth - BarInnerPadding * 2f) * healthNormalized);
            float fillHeight = Mathf.Max(0.0001f, BarHeight - BarInnerPadding * 2f);
            float leftEdge = -barWidth * 0.5f + BarInnerPadding;
            float fillCenterX = leftEdge + fillWidth * 0.5f;

            barFillRenderer.transform.localPosition = new Vector3(fillCenterX, 0f, -0.002f);
            barFillRenderer.transform.localScale = new Vector3(fillWidth, fillHeight, 1f);
            barFillRenderer.color = ResolveHealthColor(healthNormalized);
        }

        private void UpdateTurnVisuals()
        {
            bool showTurnNumber = turnsUntilTurn > 0 && !isCurrentTurn;
            SetTextGroupVisible(turnText, turnTextOutlines, showTurnNumber);

            if (!showTurnNumber)
                return;

            string turnValue = turnsUntilTurn.ToString();
            turnText.text = turnValue;
            for (int i = 0; i < turnTextOutlines.Length; i++)
                turnTextOutlines[i].text = turnValue;

            bool blinkNextTurn = turnsUntilTurn == 1;
            float blink01 = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * TurnBlinkSpeed);
            float alpha = blinkNextTurn ? Mathf.Lerp(0.5f, 1f, blink01) : 1f;
            float scale = blinkNextTurn ? Mathf.Lerp(1f, 1.08f, blink01) : 1f;

            Color turnColor = blinkNextTurn ? NextTurnTextColor : TurnTextColor;
            turnColor.a *= alpha;
            turnText.color = turnColor;
            turnText.transform.localScale = Vector3.one * (TurnTextScale * scale);

            Color outlineColor = TextOutlineColor;
            outlineColor.a *= blinkNextTurn ? Mathf.Lerp(0.76f, 1f, blink01) : 1f;
            for (int i = 0; i < turnTextOutlines.Length; i++)
            {
                turnTextOutlines[i].color = outlineColor;
                turnTextOutlines[i].transform.localScale = turnText.transform.localScale;
            }
        }

        private void ApplyIdentityVisuals()
        {
            CharacterDefinition definition = target != null ? target.Definition : null;
            if (definition == null)
                return;

            badgeFillRenderer.color = ResolveElementColor(definition.ElementalType);

            string badgeValue = ResolveMartialArtLabel(definition.MartialArtsType);
            badgeText.text = badgeValue;
            badgeText.color = BadgeTextColor;
            for (int i = 0; i < badgeTextOutlines.Length; i++)
            {
                badgeTextOutlines[i].text = badgeValue;
                badgeTextOutlines[i].color = TextOutlineColor;
            }
        }

        private void ApplySortingFromTarget()
        {
            if (target == null)
                return;

            SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer == null)
                return;

            ApplyRendererSorting(badgeOutlineRenderer, targetRenderer, 10);
            ApplyRendererSorting(badgeFillRenderer, targetRenderer, 11);
            ApplyRendererSorting(barFrameRenderer, targetRenderer, 10);
            ApplyRendererSorting(barBackgroundRenderer, targetRenderer, 11);
            ApplyRendererSorting(barFillRenderer, targetRenderer, 12);
            ApplyTextSorting(badgeText, badgeTextOutlines, targetRenderer, 13, 12);
            ApplyTextSorting(turnText, turnTextOutlines, targetRenderer, 13, 12);
        }

        private void SetVisible(bool visible)
        {
            SetRendererVisible(badgeOutlineRenderer, visible);
            SetRendererVisible(badgeFillRenderer, visible);
            SetRendererVisible(barFrameRenderer, visible);
            SetRendererVisible(barBackgroundRenderer, visible);
            SetRendererVisible(barFillRenderer, visible);
            SetTextGroupVisible(badgeText, badgeTextOutlines, visible);
            SetTextGroupVisible(turnText, turnTextOutlines, visible && turnsUntilTurn > 0 && !isCurrentTurn);
        }

        private static float ResolveBarWidth(CharacterInstance character, Bounds groundBounds)
        {
            if (character == null)
                return BarMinWidth;

            float width = Mathf.Max(groundBounds.size.x * BarWidthScale, CombatMovementUtility.GetVisualHalfWidth(character) * 0.95f);
            return Mathf.Clamp(width, BarMinWidth, BarMaxWidth);
        }

        private static Vector3 ResolveGroundAnchor(CharacterInstance character, Bounds groundBounds)
        {
            if (character == null)
                return groundBounds.center;

            CapsuleCollider2D collider = character.GetComponent<CapsuleCollider2D>();
            if (collider != null)
            {
                Bounds colliderBounds = collider.bounds;
                return new Vector3(colliderBounds.center.x, colliderBounds.min.y, character.transform.position.z);
            }

            return new Vector3(groundBounds.center.x, groundBounds.min.y, character.transform.position.z);
        }

        private static float ResolveStripGap(Bounds groundBounds)
        {
            float height = Mathf.Max(groundBounds.size.y, 0.5f);
            return StripGapBelowFeet + Mathf.Clamp(height * 0.05f, 0.03f, 0.12f);
        }

        private static Color ResolveHealthColor(float normalizedHealth)
        {
            return Color.Lerp(new Color(0.89f, 0.28f, 0.22f, 1f), new Color(0.24f, 0.92f, 0.38f, 1f), normalizedHealth);
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

        private static string ResolveMartialArtLabel(MartialArtsType martialArtsType)
        {
            switch (martialArtsType)
            {
                case MartialArtsType.Unarmed:
                    return "U";
                case MartialArtsType.Sword:
                    return "S";
                case MartialArtsType.Spear:
                    return "P";
                case MartialArtsType.Bow:
                    return "B";
                case MartialArtsType.Staff:
                    return "T";
                case MartialArtsType.DualDaggers:
                    return "D";
                case MartialArtsType.HeavyWeapons:
                    return "H";
                default:
                    return "?";
            }
        }

        private static Bounds GetGroundBounds(CharacterInstance character, out bool hasBounds)
        {
            hasBounds = false;
            Bounds bounds = default;
            if (character == null)
                return bounds;

            SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null || !renderer.enabled)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            CapsuleCollider2D collider = character.GetComponent<CapsuleCollider2D>();
            if (collider != null)
            {
                if (!hasBounds)
                {
                    bounds = collider.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            return bounds;
        }

        private SpriteRenderer CreateSpriteRenderer(string name, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            go.transform.localScale = Vector3.one;
            return renderer;
        }

        private TextMesh CreateOutlinedText(string name, int fontSize, float scale, out TextMesh[] outlines)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.localScale = Vector3.one * scale;

            outlines = new TextMesh[TextOutlineOffsets.Length];
            for (int i = 0; i < TextOutlineOffsets.Length; i++)
            {
                GameObject outlineObject = new GameObject($"{name}_Outline_{i}");
                outlineObject.transform.SetParent(root.transform, false);
                outlineObject.transform.localPosition = TextOutlineOffsets[i];
                TextMesh outline = outlineObject.AddComponent<TextMesh>();
                ConfigureTextMesh(outline, fontSize);
                outlines[i] = outline;
            }

            TextMesh main = root.AddComponent<TextMesh>();
            ConfigureTextMesh(main, fontSize);
            return main;
        }

        private static void ConfigureTextMesh(TextMesh textMesh, int fontSize)
        {
            textMesh.fontSize = fontSize;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = Color.white;
        }

        private static void ApplyRendererSorting(SpriteRenderer renderer, SpriteRenderer targetRenderer, int orderOffset)
        {
            if (renderer == null || targetRenderer == null)
                return;

            renderer.sortingLayerID = targetRenderer.sortingLayerID;
            renderer.sortingOrder = targetRenderer.sortingOrder + 160 + orderOffset;
        }

        private static void ApplyTextSorting(TextMesh main, TextMesh[] outlines, SpriteRenderer targetRenderer, int mainOffset, int outlineOffset)
        {
            if (main != null)
            {
                MeshRenderer renderer = main.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerID = targetRenderer.sortingLayerID;
                    renderer.sortingOrder = targetRenderer.sortingOrder + 160 + mainOffset;
                }
            }

            if (outlines == null)
                return;

            for (int i = 0; i < outlines.Length; i++)
            {
                if (outlines[i] == null)
                    continue;

                MeshRenderer renderer = outlines[i].GetComponent<MeshRenderer>();
                if (renderer == null)
                    continue;

                renderer.sortingLayerID = targetRenderer.sortingLayerID;
                renderer.sortingOrder = targetRenderer.sortingOrder + 160 + outlineOffset;
            }
        }

        private static void SetRendererVisible(Renderer renderer, bool visible)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }

        private static void SetTextGroupVisible(TextMesh main, TextMesh[] outlines, bool visible)
        {
            if (main != null)
            {
                MeshRenderer renderer = main.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.enabled = visible;
            }

            if (outlines == null)
                return;

            for (int i = 0; i < outlines.Length; i++)
            {
                if (outlines[i] == null)
                    continue;

                MeshRenderer renderer = outlines[i].GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.enabled = visible;
            }
        }

        private static Sprite GetSquareSprite()
        {
            if (squareSprite != null)
                return squareSprite;

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return squareSprite;
        }

        private static Sprite GetCircleSprite()
        {
            if (circleSprite != null)
                return circleSprite;

            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.5f - 1.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - distance + 1f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return circleSprite;
        }
    }
}



