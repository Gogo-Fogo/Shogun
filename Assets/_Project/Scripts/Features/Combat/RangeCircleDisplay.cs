// RangeCircleDisplay.cs
// Draws a breathing LineRenderer circle around a character to show their attack range.
// Show() starts the animation; Hide() stops it. Added lazily by BattleDragHandler.

using UnityEngine;

namespace Shogun.Features.Combat
{
    public class RangeCircleDisplay : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private Color baseColor;
        private const int Segments = 48;
        private const float BreathHz = 1.4f;   // pulses per second
        private const float MinAlpha = 0.3f;
        private const float MinWidth = 0.04f;
        private const float MaxWidth = 0.13f;

        void Awake()
        {
            var lineGO = new GameObject("RangeCircle");
            lineGO.transform.SetParent(transform, false);
            lineGO.transform.localPosition = Vector3.zero;
            lineGO.transform.localScale = Vector3.one;

            lineRenderer = lineGO.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = Segments;
            lineRenderer.startWidth = MaxWidth;
            lineRenderer.endWidth = MaxWidth;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null) lineRenderer.material = new Material(shader);

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                lineRenderer.sortingLayerID = sr.sortingLayerID;
                lineRenderer.sortingOrder = sr.sortingOrder - 1;
            }
            else
            {
                lineRenderer.sortingLayerName = "Default";
                lineRenderer.sortingOrder = -1;
            }

            lineRenderer.enabled = false;
        }

        void Update()
        {
            if (lineRenderer == null || !lineRenderer.enabled) return;

            float breath = (Mathf.Sin(Time.time * BreathHz * Mathf.PI * 2f) + 1f) * 0.5f; // 0..1

            Color c = baseColor;
            c.a = Mathf.Lerp(MinAlpha, baseColor.a, breath);
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;

            float w = Mathf.Lerp(MinWidth, MaxWidth, breath);
            lineRenderer.startWidth = w;
            lineRenderer.endWidth = w;
        }

        public void Show(float worldRadius, Color color)
        {
            if (lineRenderer == null) return;
            baseColor = color;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            DrawCircle(worldRadius);
            lineRenderer.enabled = true;
        }

        public void Hide()
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }

        private void DrawCircle(float worldRadius)
        {
            Vector3 ls = transform.lossyScale;
            float sx = Mathf.Abs(ls.x) > 0.0001f ? ls.x : 1f;
            float sy = Mathf.Abs(ls.y) > 0.0001f ? ls.y : 1f;
            float rx = worldRadius / sx;
            float ry = worldRadius / sy;

            float step = 360f / Segments * Mathf.Deg2Rad;
            for (int i = 0; i < Segments; i++)
            {
                float angle = i * step;
                lineRenderer.SetPosition(i, new Vector3(
                    Mathf.Cos(angle) * rx,
                    Mathf.Sin(angle) * ry,
                    0f));
            }
        }
    }
}
