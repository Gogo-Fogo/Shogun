// RangeCircleDisplay.cs
// Renders a softer combat aura with fill, glow, rim, and optional markers.

using UnityEngine;

namespace Shogun.Features.Combat
{
    public class RangeCircleDisplay : MonoBehaviour
    {
        private const float BreathHz = 0.58f;
        private const float FillAlphaMin = 0.18f;
        private const float FillAlphaMax = 0.28f;
        private const float HaloAlphaMin = 0.14f;
        private const float HaloAlphaMax = 0.24f;
        private const float RimAlphaMin = 0.8f;
        private const float RimAlphaMax = 0.98f;
        private const float MarkerAlphaMin = 0.72f;
        private const float MarkerAlphaMax = 0.95f;
        private const float HaloScaleMin = 1.06f;
        private const float HaloScaleMax = 1.12f;
        private const float RimScaleMin = 1f;
        private const float RimScaleMax = 1.015f;
        private const float MarkerWorldSize = 0.24f;
        private const float MarkerGapWorld = 0.18f;
        private const int SoftDiscResolution = 128;
        private const int RingResolution = 128;
        private const int MarkerResolution = 32;
        private const int HaloSortingOffset = -4;
        private const int FillSortingOffset = -3;
        private const int RimSortingOffset = -2;
        private const int MarkerSortingOffset = -1;

        private static Sprite softDiscSprite;
        private static Sprite rimSprite;
        private static Sprite markerSprite;

        private Transform visualRoot;
        private SpriteRenderer haloRenderer;
        private SpriteRenderer fillRenderer;
        private SpriteRenderer rimRenderer;
        private SpriteRenderer[] markerRenderers;

        private Color baseColor;
        private float worldRadius;
        private bool showDecorativeMarkers;
        private bool isShowing;

        void Awake()
        {
            BuildVisuals();
            SetVisible(false);
        }

        void Update()
        {
            if (!isShowing || visualRoot == null)
                return;

            RefreshAnchor();
            ApplySortingFromTarget();

            float pulse = (Mathf.Sin(Time.time * BreathHz * Mathf.PI * 2f) + 1f) * 0.5f;
            UpdateGeometry(pulse);
            UpdateColors(pulse);
        }

        public void Show(float radiusWorldUnits, Color color, bool decorativeMarkers = true)
        {
            worldRadius = Mathf.Max(0.1f, radiusWorldUnits);
            baseColor = color;
            showDecorativeMarkers = decorativeMarkers;
            isShowing = true;

            RefreshAnchor();
            ApplySortingFromTarget();
            UpdateGeometry(1f);
            UpdateColors(1f);
            SetVisible(true);
        }

        public void Hide()
        {
            isShowing = false;
            SetVisible(false);
        }

        private void BuildVisuals()
        {
            GameObject root = new GameObject("RangeCircleVisual");
            visualRoot = root.transform;
            visualRoot.SetParent(transform, false);
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localScale = Vector3.one;

            haloRenderer = CreateSpriteRenderer("Halo", GetSoftDiscSprite());
            fillRenderer = CreateSpriteRenderer("Fill", GetSoftDiscSprite());
            rimRenderer = CreateSpriteRenderer("Rim", GetRimSprite());

            markerRenderers = new SpriteRenderer[4];
            for (int i = 0; i < markerRenderers.Length; i++)
                markerRenderers[i] = CreateSpriteRenderer($"Marker_{i}", GetMarkerSprite());
        }

        private SpriteRenderer CreateSpriteRenderer(string name, Sprite sprite)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(visualRoot, false);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private void RefreshAnchor()
        {
            CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
            visualRoot.localPosition = col != null
                ? new Vector3(col.offset.x, col.offset.y, 0f)
                : Vector3.zero;
        }

        private void UpdateGeometry(float pulse)
        {
            Vector3 lossScale = transform.lossyScale;
            float sx = Mathf.Abs(lossScale.x) > 0.0001f ? Mathf.Abs(lossScale.x) : 1f;
            float sy = Mathf.Abs(lossScale.y) > 0.0001f ? Mathf.Abs(lossScale.y) : 1f;
            float diameterX = (worldRadius * 2f) / sx;
            float diameterY = (worldRadius * 2f) / sy;
            float haloScale = Mathf.Lerp(HaloScaleMin, HaloScaleMax, pulse);
            float rimScale = Mathf.Lerp(RimScaleMin, RimScaleMax, pulse);

            fillRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
            fillRenderer.transform.localScale = new Vector3(diameterX, diameterY, 1f);

            haloRenderer.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            haloRenderer.transform.localScale = new Vector3(diameterX * haloScale, diameterY * haloScale, 1f);

            rimRenderer.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            rimRenderer.transform.localScale = new Vector3(diameterX * rimScale, diameterY * rimScale, 1f);

            float markerScaleX = MarkerWorldSize / sx;
            float markerScaleY = MarkerWorldSize / sy;
            float pulseGapX = (MarkerGapWorld + Mathf.Lerp(0f, 0.04f, pulse)) / sx;
            float pulseGapY = (MarkerGapWorld + Mathf.Lerp(0f, 0.04f, pulse)) / sy;
            float markerOffsetX = (diameterX * 0.5f) + pulseGapX;
            float markerOffsetY = (diameterY * 0.5f) + pulseGapY;

            ConfigureMarker(markerRenderers[0], new Vector3(0f, markerOffsetY, 0.03f), 0f, markerScaleX, markerScaleY);
            ConfigureMarker(markerRenderers[1], new Vector3(markerOffsetX, 0f, 0.03f), -90f, markerScaleX, markerScaleY);
            ConfigureMarker(markerRenderers[2], new Vector3(0f, -markerOffsetY, 0.03f), 180f, markerScaleX, markerScaleY);
            ConfigureMarker(markerRenderers[3], new Vector3(-markerOffsetX, 0f, 0.03f), 90f, markerScaleX, markerScaleY);
        }

        private static void ConfigureMarker(SpriteRenderer renderer, Vector3 localPosition, float rotationZ, float scaleX, float scaleY)
        {
            if (renderer == null)
                return;

            Transform markerTransform = renderer.transform;
            markerTransform.localPosition = localPosition;
            markerTransform.localEulerAngles = new Vector3(0f, 0f, rotationZ);
            markerTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private void UpdateColors(float pulse)
        {
            Color fillColor = baseColor;
            fillColor.a = baseColor.a * Mathf.Lerp(FillAlphaMin, FillAlphaMax, pulse);
            fillRenderer.color = fillColor;

            Color haloColor = Color.Lerp(baseColor, Color.white, 0.2f);
            haloColor.a = baseColor.a * Mathf.Lerp(HaloAlphaMin, HaloAlphaMax, pulse);
            haloRenderer.color = haloColor;

            Color rimColor = Color.Lerp(baseColor, Color.white, 0.72f);
            rimColor.a = Mathf.Lerp(RimAlphaMin, RimAlphaMax, pulse);
            rimRenderer.color = rimColor;

            Color markerColor = Color.Lerp(baseColor, Color.white, 0.8f);
            markerColor.a = Mathf.Lerp(MarkerAlphaMin, MarkerAlphaMax, pulse);
            for (int i = 0; i < markerRenderers.Length; i++)
                markerRenderers[i].color = markerColor;
        }

        private void SetVisible(bool visible)
        {
            if (haloRenderer != null)
                haloRenderer.enabled = visible;
            if (fillRenderer != null)
                fillRenderer.enabled = visible;
            if (rimRenderer != null)
                rimRenderer.enabled = visible;

            for (int i = 0; i < markerRenderers.Length; i++)
            {
                if (markerRenderers[i] != null)
                    markerRenderers[i].enabled = visible && showDecorativeMarkers;
            }
        }

        private void ApplySortingFromTarget()
        {
            SpriteRenderer targetRenderer = ResolveTargetSpriteRenderer();
            if (targetRenderer == null)
            {
                SetSorting(haloRenderer, SortingLayer.NameToID("Default"), HaloSortingOffset);
                SetSorting(fillRenderer, SortingLayer.NameToID("Default"), FillSortingOffset);
                SetSorting(rimRenderer, SortingLayer.NameToID("Default"), RimSortingOffset);
                for (int i = 0; i < markerRenderers.Length; i++)
                    SetSorting(markerRenderers[i], SortingLayer.NameToID("Default"), MarkerSortingOffset);
                return;
            }

            SetSorting(haloRenderer, targetRenderer.sortingLayerID, targetRenderer.sortingOrder + HaloSortingOffset);
            SetSorting(fillRenderer, targetRenderer.sortingLayerID, targetRenderer.sortingOrder + FillSortingOffset);
            SetSorting(rimRenderer, targetRenderer.sortingLayerID, targetRenderer.sortingOrder + RimSortingOffset);
            for (int i = 0; i < markerRenderers.Length; i++)
                SetSorting(markerRenderers[i], targetRenderer.sortingLayerID, targetRenderer.sortingOrder + MarkerSortingOffset);
        }

        private SpriteRenderer ResolveTargetSpriteRenderer()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                if (visualRoot != null && renderer.transform.IsChildOf(visualRoot))
                    continue;

                return renderer;
            }

            return null;
        }

        private static void SetSorting(SpriteRenderer renderer, int sortingLayerId, int sortingOrder)
        {
            if (renderer == null)
                return;

            renderer.sortingLayerID = sortingLayerId;
            renderer.sortingOrder = sortingOrder;
        }

        private static Sprite GetSoftDiscSprite()
        {
            if (softDiscSprite != null)
                return softDiscSprite;

            Texture2D texture = new Texture2D(SoftDiscResolution, SoftDiscResolution, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            float center = (SoftDiscResolution - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < SoftDiscResolution; y++)
            {
                for (int x = 0; x < SoftDiscResolution; x++)
                {
                    float dx = (x - center) / radius;
                    float dy = (y - center) / radius;
                    float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
                    if (distance > 1f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float edgeBias = Mathf.SmoothStep(0f, 1f, distance);
                    float alpha = Mathf.Lerp(0.12f, 0.36f, edgeBias);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            softDiscSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, SoftDiscResolution, SoftDiscResolution),
                new Vector2(0.5f, 0.5f),
                SoftDiscResolution);
            return softDiscSprite;
        }

        private static Sprite GetRimSprite()
        {
            if (rimSprite != null)
                return rimSprite;

            Texture2D texture = new Texture2D(RingResolution, RingResolution, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            float center = (RingResolution - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < RingResolution; y++)
            {
                for (int x = 0; x < RingResolution; x++)
                {
                    float dx = (x - center) / radius;
                    float dy = (y - center) / radius;
                    float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
                    if (distance > 1f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float innerFade = Mathf.SmoothStep(0.86f, 0.93f, distance);
                    float outerFade = 1f - Mathf.SmoothStep(0.985f, 1f, distance);
                    float alpha = innerFade * outerFade;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            rimSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, RingResolution, RingResolution),
                new Vector2(0.5f, 0.5f),
                RingResolution);
            return rimSprite;
        }

        private static Sprite GetMarkerSprite()
        {
            if (markerSprite != null)
                return markerSprite;

            Texture2D texture = new Texture2D(MarkerResolution, MarkerResolution, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Vector2 a = new Vector2(MarkerResolution * 0.5f, MarkerResolution * 0.92f);
            Vector2 b = new Vector2(MarkerResolution * 0.16f, MarkerResolution * 0.18f);
            Vector2 c = new Vector2(MarkerResolution * 0.84f, MarkerResolution * 0.18f);
            Vector2 innerA = Vector2.Lerp(a, (a + b + c) / 3f, 0.42f);
            Vector2 innerB = Vector2.Lerp(b, (a + b + c) / 3f, 0.42f);
            Vector2 innerC = Vector2.Lerp(c, (a + b + c) / 3f, 0.42f);

            for (int y = 0; y < MarkerResolution; y++)
            {
                for (int x = 0; x < MarkerResolution; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    bool inOuter = IsPointInTriangle(point, a, b, c);
                    bool inInner = IsPointInTriangle(point, innerA, innerB, innerC);
                    texture.SetPixel(x, y, inOuter && !inInner ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            markerSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, MarkerResolution, MarkerResolution),
                new Vector2(0.5f, 0.5f),
                MarkerResolution);
            return markerSprite;
        }

        private static bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(point, a, b);
            float d2 = Sign(point, b, c);
            float d3 = Sign(point, c, a);

            bool hasNegative = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPositive = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNegative && hasPositive);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p1.x - p3.x) * (p2.y - p3.y)) - ((p2.x - p3.x) * (p1.y - p3.y));
        }
    }
}
