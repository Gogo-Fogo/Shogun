using UnityEngine;
using Shogun.Features.Characters;

namespace Shogun.Features.Combat
{
    public class BattleFloatingText : MonoBehaviour
    {
        private const float Lifetime = 0.55f;
        private const float RiseDistance = 0.95f;
        private const float PopScale = 0.075f;
        private const int FontSize = 84;

        private TextMesh mainLabel;
        private TextMesh shadowLabel;
        private Vector3 startPosition;
        private float spawnedAt;
        private Color mainColor;
        private Color shadowColor;

        public static void SpawnDamage(CharacterInstance target, float damage)
        {
            if (target == null)
                return;

            GameObject go = new GameObject("DamagePopup");
            BattleFloatingText popup = go.AddComponent<BattleFloatingText>();
            popup.Initialize(target, Mathf.RoundToInt(damage).ToString());
        }

        private void Initialize(CharacterInstance target, string text)
        {
            startPosition = GetPopupWorldPosition(target);
            transform.position = startPosition;

            mainLabel = gameObject.AddComponent<TextMesh>();
            mainLabel.text = text;
            mainLabel.fontSize = FontSize;
            mainLabel.fontStyle = FontStyle.Bold;
            mainLabel.alignment = TextAlignment.Center;
            mainLabel.anchor = TextAnchor.MiddleCenter;
            mainLabel.color = new Color(1f, 0.97f, 0.82f, 1f);
            mainColor = mainLabel.color;

            GameObject shadow = new GameObject("Shadow");
            shadow.transform.SetParent(transform, false);
            shadow.transform.localPosition = new Vector3(0.06f, -0.06f, 0.02f);

            shadowLabel = shadow.AddComponent<TextMesh>();
            shadowLabel.text = text;
            shadowLabel.fontSize = FontSize;
            shadowLabel.fontStyle = FontStyle.Bold;
            shadowLabel.alignment = TextAlignment.Center;
            shadowLabel.anchor = TextAnchor.MiddleCenter;
            shadowLabel.color = new Color(0.18f, 0.05f, 0f, 0.85f);
            shadowColor = shadowLabel.color;

            MeshRenderer renderer = mainLabel.GetComponent<MeshRenderer>();
            MeshRenderer shadowRenderer = shadowLabel.GetComponent<MeshRenderer>();
            SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer != null)
            {
                renderer.sortingLayerID = targetRenderer.sortingLayerID;
                renderer.sortingOrder = targetRenderer.sortingOrder + 6;
                shadowRenderer.sortingLayerID = targetRenderer.sortingLayerID;
                shadowRenderer.sortingOrder = targetRenderer.sortingOrder + 5;
            }

            spawnedAt = Time.time;
        }

        void Update()
        {
            float elapsed = Time.time - spawnedAt;
            float t = Mathf.Clamp01(elapsed / Lifetime);
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            float scale = PopScale * (1f + Mathf.Sin(t * Mathf.PI) * 0.24f);

            transform.position = startPosition + new Vector3(0f, easeOut * RiseDistance, 0f);
            transform.localScale = Vector3.one * scale;

            Color fadedMain = mainColor;
            fadedMain.a = 1f - t;
            mainLabel.color = fadedMain;

            Color fadedShadow = shadowColor;
            fadedShadow.a = shadowColor.a * (1f - t);
            shadowLabel.color = fadedShadow;

            if (t >= 1f)
                Destroy(gameObject);
        }

        private static Vector3 GetPopupWorldPosition(CharacterInstance target)
        {
            CapsuleCollider2D collider = target.GetComponent<CapsuleCollider2D>();
            if (collider != null)
            {
                Vector3 colliderCenter = target.transform.TransformPoint(collider.offset);
                float colliderTop = collider.size.y * 0.5f * Mathf.Abs(target.transform.lossyScale.y);
                return colliderCenter + new Vector3(0f, colliderTop + 0.45f, -0.2f);
            }

            return target.transform.position + new Vector3(0f, 1.6f, -0.2f);
        }
    }
}
