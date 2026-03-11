using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    [RequireComponent(typeof(CharacterInstance))]
    public sealed class CriticalRateBoostPreview : MonoBehaviour
    {
        private const float LiftAboveHead = 0.58f;
        private const float TextScale = 0.013f;
        private const int FontSize = 86;
        private const float FloatAmplitude = 0.04f;
        private const float FloatSpeed = 3.2f;
        private const float ShadowOffset = 0.07f;

        private static readonly Color MainColor = new Color(1f, 0.96f, 0.88f, 1f);
        private static readonly Color ShadowColor = new Color(0.09f, 0.04f, 0.01f, 0.92f);

        private CharacterInstance target;
        private Transform labelRoot;
        private TextMesh mainLabel;
        private TextMesh shadowLabel;
        private bool isVisible;

        private void Awake()
        {
            target = GetComponent<CharacterInstance>();
            BuildVisuals();
            SetVisible(false);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Destroy(this);
                return;
            }

            if (!target.IsAlive)
            {
                Hide();
                return;
            }

            if (!isVisible)
                return;

            UpdatePlacement();
            ApplySortingFromTarget();
        }

        public void Show(float criticalRateMultiplier)
        {
            if (criticalRateMultiplier <= 1.01f || target == null || !target.IsAlive)
            {
                Hide();
                return;
            }

            EnsureBuilt();
            string labelText = $"Critical Rate Boost x{criticalRateMultiplier:0.0}";
            mainLabel.text = labelText;
            shadowLabel.text = labelText;
            isVisible = true;
            SetVisible(true);
            UpdatePlacement();
            ApplySortingFromTarget();
        }

        public void Hide()
        {
            isVisible = false;
            SetVisible(false);
        }

        private void EnsureBuilt()
        {
            if (labelRoot == null || mainLabel == null || shadowLabel == null)
                BuildVisuals();
        }

        private void BuildVisuals()
        {
            if (labelRoot != null)
                return;

            GameObject root = new GameObject("CriticalRateBoostPreview");
            labelRoot = root.transform;
            labelRoot.SetParent(transform, false);

            GameObject mainObject = new GameObject("Main");
            mainObject.transform.SetParent(labelRoot, false);
            mainLabel = mainObject.AddComponent<TextMesh>();
            mainLabel.fontSize = FontSize;
            mainLabel.fontStyle = FontStyle.Bold;
            mainLabel.alignment = TextAlignment.Center;
            mainLabel.anchor = TextAnchor.MiddleCenter;
            mainLabel.color = MainColor;

            GameObject shadowObject = new GameObject("Shadow");
            shadowObject.transform.SetParent(labelRoot, false);
            shadowObject.transform.localPosition = new Vector3(ShadowOffset, -ShadowOffset, 0.02f);
            shadowLabel = shadowObject.AddComponent<TextMesh>();
            shadowLabel.fontSize = FontSize;
            shadowLabel.fontStyle = FontStyle.Bold;
            shadowLabel.alignment = TextAlignment.Center;
            shadowLabel.anchor = TextAnchor.MiddleCenter;
            shadowLabel.color = ShadowColor;
        }

        private void UpdatePlacement()
        {
            if (labelRoot == null)
                return;

            float scaleX = Mathf.Max(Mathf.Abs(transform.lossyScale.x), 0.001f);
            float scaleY = Mathf.Max(Mathf.Abs(transform.lossyScale.y), 0.001f);
            float bob = Mathf.Sin(Time.unscaledTime * FloatSpeed) * FloatAmplitude;

            Vector3 localPosition = new Vector3(0f, ResolveLocalTopOffset(scaleY) + (bob / scaleY), -0.12f);
            CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
            if (collider != null)
                localPosition.x = collider.offset.x;

            labelRoot.localPosition = localPosition;
            labelRoot.localScale = new Vector3(TextScale / scaleX, TextScale / scaleY, 1f);
        }

        private float ResolveLocalTopOffset(float scaleY)
        {
            CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
            if (collider == null)
                return 1.5f / scaleY;

            return collider.offset.y + collider.size.y * 0.5f + (LiftAboveHead / scaleY);
        }

        private void ApplySortingFromTarget()
        {
            if (mainLabel == null || shadowLabel == null || target == null)
                return;

            SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer == null)
                return;

            MeshRenderer mainRenderer = mainLabel.GetComponent<MeshRenderer>();
            MeshRenderer shadowRenderer = shadowLabel.GetComponent<MeshRenderer>();
            if (mainRenderer != null)
            {
                mainRenderer.sortingLayerID = targetRenderer.sortingLayerID;
                mainRenderer.sortingOrder = targetRenderer.sortingOrder + 18;
            }

            if (shadowRenderer != null)
            {
                shadowRenderer.sortingLayerID = targetRenderer.sortingLayerID;
                shadowRenderer.sortingOrder = targetRenderer.sortingOrder + 17;
            }
        }

        private void SetVisible(bool visible)
        {
            if (mainLabel != null)
                mainLabel.gameObject.SetActive(visible);
            if (shadowLabel != null)
                shadowLabel.gameObject.SetActive(visible);
        }
    }
}
