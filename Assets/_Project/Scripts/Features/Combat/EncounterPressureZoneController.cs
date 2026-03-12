using System.Collections;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    [DisallowMultipleComponent]
    public sealed class EncounterPressureZoneController : MonoBehaviour
    {
        private const float FillAlpha = 0.06f;
        private const float CoreAlpha = 0.11f;
        private const float FlashFillAlpha = 0.14f;
        private const float FlashCoreAlpha = 0.22f;
        private const float BracketAlpha = 0.82f;
        private const float FlashBracketAlpha = 1f;
        private const float BracketWidth = 0.08f;
        private const float CoreWidthMultiplier = 0.58f;
        private const float CoreHeightMultiplier = 0.92f;
        private const float MaxBracketHookLength = 0.5f;
        private const float LabelOffset = 0.32f;
        private const float FlashDurationSeconds = 0.22f;

        private static Sprite runtimeWhiteSprite;
        private static Material runtimeLineMaterial;

        private BattleEncounterDefinition encounter;
        private BattleManager battleManager;
        private TurnManager turnManager;

        private GameObject zoneRoot;
        private SpriteRenderer fillRenderer;
        private SpriteRenderer coreRenderer;
        private LineRenderer leftBracketRenderer;
        private LineRenderer rightBracketRenderer;
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
            ApplyVisualColors(FillAlpha, CoreAlpha, BracketAlpha);
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
            fillRenderer.sortingOrder = 2;

            GameObject coreObject = new GameObject("LaneCore");
            coreObject.transform.SetParent(zoneRoot.transform, false);
            coreRenderer = coreObject.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = GetRuntimeWhiteSprite();
            coreRenderer.sortingOrder = 3;

            leftBracketRenderer = CreateBracketRenderer("LeftBracket", zoneRoot.transform, 4);
            rightBracketRenderer = CreateBracketRenderer("RightBracket", zoneRoot.transform, 4);

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(zoneRoot.transform, false);
            labelMesh = labelObject.AddComponent<TextMesh>();
            labelMesh.text = encounter != null ? encounter.PressureZoneLabel : "AMBUSH LANE";
            labelMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (labelMesh.font == null)
                labelMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelMesh.fontSize = 42;
            labelMesh.fontStyle = FontStyle.Bold;
            labelMesh.anchor = TextAnchor.MiddleCenter;
            labelMesh.alignment = TextAlignment.Center;
            labelMesh.characterSize = 0.055f;
            labelMesh.color = new Color(0.95f, 0.89f, 0.74f, 0.9f);

            MeshRenderer labelRenderer = labelMesh.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
                labelRenderer.sortingOrder = 5;
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
            }

            if (coreRenderer != null)
            {
                coreRenderer.transform.position = new Vector3(center.x, center.y, 0.045f);
                coreRenderer.transform.localScale = new Vector3(bounds.size.x * CoreWidthMultiplier, bounds.size.y * CoreHeightMultiplier, 1f);
            }

            RefreshBracket(leftBracketRenderer, bounds, true);
            RefreshBracket(rightBracketRenderer, bounds, false);

            if (labelMesh != null)
            {
                labelMesh.text = encounter.PressureZoneLabel;
                labelMesh.transform.position = new Vector3(center.x, bounds.max.y + LabelOffset, 0.03f);
            }
        }

        private void RefreshBracket(LineRenderer renderer, Bounds bounds, bool leftSide)
        {
            if (renderer == null)
                return;

            float minX = bounds.min.x;
            float maxX = bounds.max.x;
            float minY = bounds.min.y;
            float maxY = bounds.max.y;
            float hookLength = Mathf.Min(MaxBracketHookLength, bounds.size.x * 0.28f);
            float z = 0.04f;

            if (leftSide)
            {
                renderer.SetPosition(0, new Vector3(minX + hookLength, maxY, z));
                renderer.SetPosition(1, new Vector3(minX, maxY, z));
                renderer.SetPosition(2, new Vector3(minX, minY, z));
                renderer.SetPosition(3, new Vector3(minX + hookLength, minY, z));
            }
            else
            {
                renderer.SetPosition(0, new Vector3(maxX - hookLength, maxY, z));
                renderer.SetPosition(1, new Vector3(maxX, maxY, z));
                renderer.SetPosition(2, new Vector3(maxX, minY, z));
                renderer.SetPosition(3, new Vector3(maxX - hookLength, minY, z));
            }
        }

        private static LineRenderer CreateBracketRenderer(string name, Transform parent, int sortingOrder)
        {
            GameObject rendererObject = new GameObject(name);
            rendererObject.transform.SetParent(parent, false);
            LineRenderer renderer = rendererObject.AddComponent<LineRenderer>();
            renderer.material = GetRuntimeLineMaterial();
            renderer.loop = false;
            renderer.positionCount = 4;
            renderer.useWorldSpace = true;
            renderer.alignment = LineAlignment.View;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.numCornerVertices = 2;
            renderer.startWidth = BracketWidth;
            renderer.endWidth = BracketWidth;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void SetZoneVisible(bool visible)
        {
            if (zoneRoot != null)
                zoneRoot.SetActive(visible);
        }

        private void TriggerZoneFlash()
        {
            if (fillRenderer == null && coreRenderer == null)
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
                float fillAlpha = Mathf.Lerp(FlashFillAlpha, FillAlpha, t);
                float coreAlpha = Mathf.Lerp(FlashCoreAlpha, CoreAlpha, t);
                float bracketAlpha = Mathf.Lerp(FlashBracketAlpha, BracketAlpha, t);
                ApplyVisualColors(fillAlpha, coreAlpha, bracketAlpha);
                yield return null;
            }

            ApplyVisualColors(FillAlpha, CoreAlpha, BracketAlpha);
            flashCoroutine = null;
        }

        private void ApplyVisualColors(float fillAlpha, float coreAlpha, float bracketAlpha)
        {
            if (fillRenderer != null)
                fillRenderer.color = ResolveFillColor(fillAlpha);

            if (coreRenderer != null)
                coreRenderer.color = ResolveCoreColor(coreAlpha);

            Color bracketColor = ResolveBracketColor(bracketAlpha);
            if (leftBracketRenderer != null)
            {
                leftBracketRenderer.startColor = bracketColor;
                leftBracketRenderer.endColor = bracketColor;
            }

            if (rightBracketRenderer != null)
            {
                rightBracketRenderer.startColor = bracketColor;
                rightBracketRenderer.endColor = bracketColor;
            }
        }

        private static Color ResolveFillColor(float alpha)
        {
            return new Color(0.22f, 0.07f, 0.05f, Mathf.Clamp01(alpha));
        }

        private static Color ResolveCoreColor(float alpha)
        {
            return new Color(0.46f, 0.14f, 0.1f, Mathf.Clamp01(alpha));
        }

        private static Color ResolveBracketColor(float alpha)
        {
            return new Color(0.94f, 0.69f, 0.39f, Mathf.Clamp01(alpha));
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
