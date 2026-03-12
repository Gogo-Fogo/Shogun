using System.Collections;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    [DisallowMultipleComponent]
    public sealed class EncounterPressureZoneController : MonoBehaviour
    {
        private const float FillAlpha = 0.12f;
        private const float FlashAlpha = 0.22f;
        private const float BorderAlpha = 0.86f;
        private const float BorderWidth = 0.08f;
        private const float LabelOffset = 0.38f;
        private const float FlashDurationSeconds = 0.22f;

        private static Sprite runtimeWhiteSprite;
        private static Material runtimeLineMaterial;

        private BattleEncounterDefinition encounter;
        private BattleManager battleManager;
        private TurnManager turnManager;

        private GameObject zoneRoot;
        private SpriteRenderer fillRenderer;
        private LineRenderer borderRenderer;
        private TextMesh labelMesh;
        private Coroutine flashCoroutine;
        private bool subscribed;

        public void Configure(BattleEncounterDefinition encounterDefinition, BattleManager battleManagerRef, TurnManager turnManagerRef)
        {
            encounter = encounterDefinition;
            battleManager = battleManagerRef;
            turnManager = turnManagerRef;

            Unsubscribe();

            if (encounter == null || !encounter.HasPressureZone || battleManager == null || turnManager == null)
            {
                SetZoneVisible(false);
                return;
            }

            EnsureZoneVisual();
            RefreshZoneVisual();
            SetZoneVisible(true);
            Subscribe();
        }

        private void LateUpdate()
        {
            if (zoneRoot == null || encounter == null || !encounter.HasPressureZone || battleManager == null)
                return;

            RefreshZoneVisual();
        }

        private void OnDestroy()
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            Unsubscribe();

            if (zoneRoot != null)
            {
                if (Application.isPlaying)
                    Destroy(zoneRoot);
                else
                    DestroyImmediate(zoneRoot);
            }
        }

        private void Subscribe()
        {
            if (turnManager == null || subscribed)
                return;

            turnManager.OnTurnEnded += HandleTurnEnded;
            turnManager.OnBattleEnded += HandleBattleEnded;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (turnManager == null || !subscribed)
                return;

            turnManager.OnTurnEnded -= HandleTurnEnded;
            turnManager.OnBattleEnded -= HandleBattleEnded;
            subscribed = false;
        }

        private void HandleTurnEnded(CharacterInstance current)
        {
            if (current == null || encounter == null || battleManager == null || turnManager == null)
                return;

            if (!turnManager.IsPlayerUnit(current) || !current.IsAlive)
                return;

            Bounds bounds = encounter.GetPressureZoneBounds(battleManager.BattleCenter);
            if (!bounds.Contains(current.transform.position))
                return;

            float chipDamage = encounter.PressureZoneChipDamage;
            if (chipDamage <= 0f)
                return;

            current.TakeDamage(chipDamage);
            BattleFloatingText.SpawnDamage(current, chipDamage, false);
            TriggerZoneFlash();
            Debug.Log($"[EncounterPressureZone] {current.Definition.CharacterName} ended the turn inside {encounter.PressureZoneLabel} and took {chipDamage:0.#} chip damage.");
        }

        private void HandleBattleEnded(BattleResult result)
        {
            SetZoneVisible(false);
        }

        private void EnsureZoneVisual()
        {
            if (zoneRoot != null)
                return;

            zoneRoot = new GameObject("EncounterPressureZone");
            zoneRoot.transform.SetParent(transform, false);

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(zoneRoot.transform, false);
            fillRenderer = fillObject.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = GetRuntimeWhiteSprite();
            fillRenderer.color = ResolveFillColor(FillAlpha);
            fillRenderer.sortingOrder = 2;

            GameObject borderObject = new GameObject("Border");
            borderObject.transform.SetParent(zoneRoot.transform, false);
            borderRenderer = borderObject.AddComponent<LineRenderer>();
            borderRenderer.material = GetRuntimeLineMaterial();
            borderRenderer.loop = true;
            borderRenderer.positionCount = 4;
            borderRenderer.useWorldSpace = true;
            borderRenderer.alignment = LineAlignment.View;
            borderRenderer.textureMode = LineTextureMode.Stretch;
            borderRenderer.numCornerVertices = 2;
            borderRenderer.startWidth = BorderWidth;
            borderRenderer.endWidth = BorderWidth;
            borderRenderer.startColor = ResolveBorderColor();
            borderRenderer.endColor = ResolveBorderColor();
            borderRenderer.sortingOrder = 3;

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(zoneRoot.transform, false);
            labelMesh = labelObject.AddComponent<TextMesh>();
            labelMesh.text = encounter != null ? encounter.PressureZoneLabel : "AMBUSH LANE";
            labelMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (labelMesh.font == null)
                labelMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelMesh.fontSize = 44;
            labelMesh.fontStyle = FontStyle.Bold;
            labelMesh.anchor = TextAnchor.MiddleCenter;
            labelMesh.alignment = TextAlignment.Center;
            labelMesh.characterSize = 0.07f;
            labelMesh.color = new Color(0.95f, 0.89f, 0.66f, 0.92f);

            MeshRenderer labelRenderer = labelMesh.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
                labelRenderer.sortingOrder = 4;
        }

        private void RefreshZoneVisual()
        {
            if (zoneRoot == null || encounter == null || battleManager == null)
                return;

            Bounds bounds = encounter.GetPressureZoneBounds(battleManager.BattleCenter);
            Vector3 center = bounds.center;

            if (fillRenderer != null)
            {
                fillRenderer.transform.position = new Vector3(center.x, center.y, 0.05f);
                fillRenderer.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
                fillRenderer.color = ResolveFillColor(FillAlpha);
            }

            if (borderRenderer != null)
            {
                float minX = bounds.min.x;
                float maxX = bounds.max.x;
                float minY = bounds.min.y;
                float maxY = bounds.max.y;
                borderRenderer.SetPosition(0, new Vector3(minX, maxY, 0.04f));
                borderRenderer.SetPosition(1, new Vector3(maxX, maxY, 0.04f));
                borderRenderer.SetPosition(2, new Vector3(maxX, minY, 0.04f));
                borderRenderer.SetPosition(3, new Vector3(minX, minY, 0.04f));
                borderRenderer.startColor = ResolveBorderColor();
                borderRenderer.endColor = ResolveBorderColor();
            }

            if (labelMesh != null)
            {
                labelMesh.text = encounter.PressureZoneLabel;
                labelMesh.transform.position = new Vector3(center.x, bounds.max.y + LabelOffset, 0.03f);
            }
        }

        private void SetZoneVisible(bool visible)
        {
            if (zoneRoot != null)
                zoneRoot.SetActive(visible);
        }

        private void TriggerZoneFlash()
        {
            if (fillRenderer == null)
                return;

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashZoneRoutine());
        }

        private IEnumerator FlashZoneRoutine()
        {
            float elapsed = 0f;
            while (elapsed < FlashDurationSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FlashDurationSeconds);
                float alpha = Mathf.Lerp(FlashAlpha, FillAlpha, t);
                if (fillRenderer != null)
                    fillRenderer.color = ResolveFillColor(alpha);
                yield return null;
            }

            if (fillRenderer != null)
                fillRenderer.color = ResolveFillColor(FillAlpha);

            flashCoroutine = null;
        }

        private static Color ResolveFillColor(float alpha)
        {
            return new Color(0.46f, 0.14f, 0.12f, Mathf.Clamp01(alpha));
        }

        private static Color ResolveBorderColor()
        {
            return new Color(0.91f, 0.63f, 0.3f, BorderAlpha);
        }

        private static Sprite GetRuntimeWhiteSprite()
        {
            if (runtimeWhiteSprite != null)
                return runtimeWhiteSprite;

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "EncounterPressureZoneWhite"
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            runtimeWhiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return runtimeWhiteSprite;
        }

        private static Material GetRuntimeLineMaterial()
        {
            if (runtimeLineMaterial != null)
                return runtimeLineMaterial;

            Shader shader = Shader.Find("Sprites/Default");
            runtimeLineMaterial = new Material(shader)
            {
                name = "EncounterPressureZoneLineMaterial"
            };
            return runtimeLineMaterial;
        }
    }
}
