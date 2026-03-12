using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shogun.Features.Combat
{
    public sealed partial class BattleHudController
    {
        private const int EditorPreviewFrontlineCount = 3;

        private readonly List<CharacterDefinition> editorPreviewPlayers = new List<CharacterDefinition>();
        private readonly List<CharacterDefinition> editorPreviewEnemies = new List<CharacterDefinition>();
        private TestBattleSetup editorPreviewSetup;

        private void OnEnable()
        {
            if (!Application.isPlaying)
                RebuildEditorPreview();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                RebuildEditorPreview();
        }

        private void RebuildEditorPreview()
        {
            if (!ShouldBuildEditorPreview())
                return;

            ResolveDependencies();
            ResolveEditorPreviewRosters();
            ResetEditorPreviewHud();
            BuildHud();
            RebuildEditorPreviewSlots();
            RefreshEditorPreviewHud();
            HideComboTrackerImmediate();
            HideComboCutInImmediate();
            HideEncounterIntroImmediate();

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            ApplyResponsiveLayout(true);
            Canvas.ForceUpdateCanvases();

            if (hudRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(hudRoot);

            if (hudContentFrame != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(hudContentFrame);
        }

        private bool ShouldBuildEditorPreview()
        {
            if (Application.isPlaying || !isActiveAndEnabled)
                return false;

            Scene activeScene = SceneManager.GetActiveScene();
            if (!gameObject.scene.IsValid() || gameObject.scene != activeScene)
                return false;

            return FindFirstObjectByType<TestBattleSetup>() != null;
        }

        private void ResolveEditorPreviewRosters()
        {
            editorPreviewPlayers.Clear();
            editorPreviewEnemies.Clear();

            editorPreviewSetup = FindFirstObjectByType<TestBattleSetup>();
            if (editorPreviewSetup == null)
                return;

            AppendPreviewDefinitions(editorPreviewPlayers, editorPreviewSetup.GetPreviewPlayerTeam(), EditorPreviewFrontlineCount * 2);
            AppendPreviewDefinitions(editorPreviewEnemies, editorPreviewSetup.GetPreviewEnemyTeam(), int.MaxValue);
        }

        private static void AppendPreviewDefinitions(List<CharacterDefinition> destination, List<CharacterDefinition> source, int maxCount)
        {
            if (destination == null || source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                CharacterDefinition definition = source[i];
                if (definition == null)
                    continue;

                destination.Add(definition);
                if (destination.Count >= maxCount)
                    break;
            }
        }

        private void ResetEditorPreviewHud()
        {
            RectTransform hudParent = ResolveHudParent();
            if (hudParent != null)
            {
                RectTransform existingHud = hudParent.Find("BattleHudRoot") as RectTransform;
                if (existingHud != null)
                    SafeDestroy(existingHud.gameObject);
            }

            hudRoot = null;
            hudContentFrame = null;
            turnLabel = null;
            objectiveLabel = null;
            turnPanelBackground = null;
            objectivePillBackground = null;
            pairSlotRow = null;
            squadHpFill = null;
            squadHpValueLabel = null;
            pauseMenuPanel = null;
            speedButtonLabel = null;
            autoButtonLabel = null;
            menuButtonLabel = null;
            turnVignetteCanvasGroup = null;
            comboTrackerRoot = null;
            comboTrackerCanvasGroup = null;
            comboHitCountLabel = null;
            comboHitsSuffixLabel = null;
            comboTypeLabel = null;
            comboTrackerGlowImage = null;
            comboTrackerStreakImage = null;
            comboTrackerBaseAnchoredPosition = Vector2.zero;
            comboTrackerTargetAlpha = 0f;
            comboTrackerCurrentAlpha = 0f;
            comboTrackerHideAt = float.NegativeInfinity;
            comboTrackerPulseTimer = 0f;
            comboTrackerPulseDuration = ComboTrackerBasePulseSeconds;
            comboCutInRoot = null;
            comboCutInCanvasGroup = null;
            comboCutInCoroutine = null;
            encounterIntroRoot = null;
            encounterIntroCanvasGroup = null;
            encounterIntroTitleLabel = null;
            encounterIntroSubtitleLabel = null;
            encounterIntroCoroutine = null;
            battleHasEnded = false;
            battleEndResult = BattleResult.Win;

            elementLegendTokens.Clear();
            weaponLegendTokens.Clear();
            playerSlots.Clear();
            comboCutInSlots.Clear();
            UnbindCharacterEvents();
            ClearEditorPreviewIndicators();
        }

        private void RebuildEditorPreviewSlots()
        {
            if (pairSlotRow == null)
                return;

            SafeClearChildren(pairSlotRow);
            playerSlots.Clear();
            UnbindCharacterEvents();

            for (int i = 0; i < EditorPreviewFrontlineCount; i++)
                playerSlots.Add(CreatePlayerSlot(i));
        }

        private void RefreshEditorPreviewHud()
        {
            RefreshEditorPreviewTopBar();
            RefreshEditorPreviewSquadHealthBar();
            RefreshEditorPreviewSlots();
            turnVignetteTargetAlpha = 0f;
            turnVignetteCurrentAlpha = 0f;

            if (turnVignetteCanvasGroup != null)
                turnVignetteCanvasGroup.alpha = 0f;
        }

        private void RefreshEditorPreviewTopBar()
        {
            if (turnLabel == null || objectiveLabel == null)
                return;

            CharacterDefinition current = ResolvePreviewFrontDefinition(0);
            turnLabel.text = $"TURN: {ResolvePreviewCharacterName(current)}";
            RefreshEditorPreviewTurnContextVisuals(current);
            RefreshEditorPreviewLegendTokens(current);

            int totalEnemies = editorPreviewEnemies.Count;
            if (totalEnemies > 0)
            {
                objectiveLabel.text = editorPreviewSetup != null
                    ? editorPreviewSetup.GetEncounterObjectiveStatusText(totalEnemies, totalEnemies)
                    : $"DEFEAT ALL ENEMIES  {totalEnemies}/{totalEnemies}";
                if (objectivePillBackground != null)
                    objectivePillBackground.color = new Color(0.16f, 0.12f, 0.07f, 0.96f);
            }
            else
            {
                objectiveLabel.text = "DEFEAT ALL ENEMIES";
                if (objectivePillBackground != null)
                    objectivePillBackground.color = new Color(0.16f, 0.12f, 0.07f, 0.96f);
            }

            UpdateUtilityLabels();
        }

        private void RefreshEditorPreviewTurnContextVisuals(CharacterDefinition current)
        {
            if (turnPanelBackground == null)
                return;

            Color baseColor = new Color(0.16f, 0.12f, 0.07f, 0.96f);
            Color accent = ResolvePreviewAccentColor(current);
            turnPanelBackground.color = Color.Lerp(baseColor, accent, current != null ? 0.28f : 0f);
        }

        private void RefreshEditorPreviewLegendTokens(CharacterDefinition current)
        {
            ElementalType? activeElement = current != null ? current.ElementalType : (ElementalType?)null;
            MartialArtsType? activeWeapon = current != null ? current.MartialArtsType : (MartialArtsType?)null;

            for (int i = 0; i < elementLegendTokens.Count; i++)
                ApplyLegendTokenState(elementLegendTokens[i], activeElement.HasValue && elementLegendTokens[i].ElementalType == activeElement.Value);

            for (int i = 0; i < weaponLegendTokens.Count; i++)
                ApplyLegendTokenState(weaponLegendTokens[i], activeWeapon.HasValue && weaponLegendTokens[i].MartialArtsType == activeWeapon.Value);
        }

        private void RefreshEditorPreviewSquadHealthBar()
        {
            if (squadHpFill == null || squadHpValueLabel == null)
                return;

            float totalHealth = 0f;
            for (int i = 0; i < editorPreviewPlayers.Count; i++)
            {
                CharacterDefinition definition = editorPreviewPlayers[i];
                if (definition != null)
                    totalHealth += Mathf.Max(1f, definition.BaseHealth);
            }

            if (totalHealth <= 0f)
            {
                squadHpFill.fillAmount = 0f;
                squadHpFill.color = new Color(0.89f, 0.28f, 0.22f, 1f);
                squadHpValueLabel.text = "0/0";
                return;
            }

            squadHpFill.fillAmount = 1f;
            squadHpFill.color = new Color(0.24f, 0.92f, 0.38f, 1f);
            int roundedHealth = Mathf.CeilToInt(totalHealth);
            squadHpValueLabel.text = $"{roundedHealth}/{roundedHealth}";
        }

        private void RefreshEditorPreviewSlots()
        {
            for (int i = 0; i < playerSlots.Count; i++)
            {
                PlayerSlotView slot = playerSlots[i];
                CharacterDefinition front = ResolvePreviewFrontDefinition(slot.LaneIndex);
                CharacterDefinition reserve = ResolvePreviewReserveDefinition(slot.LaneIndex);
                RefreshEditorPreviewSlot(slot, front, reserve, slot.LaneIndex == 0);
            }
        }

        private void RefreshEditorPreviewSlot(PlayerSlotView slot, CharacterDefinition front, CharacterDefinition reserve, bool isCurrentLane)
        {
            if (slot == null)
                return;

            if (front == null)
            {
                slot.FrontPortrait.sprite = null;
                slot.FrontPortrait.color = new Color(1f, 1f, 1f, 0f);
                slot.FrontHpFill.fillAmount = 0f;
                slot.ActiveRing.color = new Color(1f, 1f, 1f, 0f);
                slot.AbilityArcFill.fillAmount = 0f;
                slot.AbilityArcFill.color = new Color(0.42f, 0.7f, 0.94f, 0.24f);
                HideAbilityChargeDividers(slot);
                slot.DeadOverlay.color = new Color(0.08f, 0.05f, 0.05f, 0.5f);
                slot.Root.localScale = Vector3.one;
            }
            else
            {
                Color accent = ResolvePreviewAccentColor(front);
                accent.a = 1f;

                slot.FrontPortrait.sprite = ResolvePreviewHudPortrait(front);
                slot.FrontPortrait.color = Color.white;
                slot.FrontHpFill.fillAmount = 1f;
                slot.FrontHpFill.color = new Color(0.2f, 0.92f, 0.38f, 1f);
                UpdateEditorPreviewChargeMeter(slot, front, accent);
                slot.ActiveRing.color = isCurrentLane
                    ? new Color(accent.r, accent.g, accent.b, 0.84f)
                    : new Color(1f, 1f, 1f, 0f);
                slot.DeadOverlay.color = new Color(0.08f, 0.05f, 0.05f, 0f);
                slot.Root.localScale = isCurrentLane ? new Vector3(1.08f, 1.08f, 1f) : Vector3.one;
            }

            if (reserve != null)
            {
                slot.ReserveButton.gameObject.SetActive(true);
                slot.ReserveButton.interactable = false;
                slot.ReservePortrait.sprite = ResolvePreviewHudPortrait(reserve);
                slot.ReservePortrait.color = Color.white;

                if (slot.ReserveButtonBackground != null)
                    slot.ReserveButtonBackground.color = new Color(0.22f, 0.18f, 0.12f, 0.88f);
            }
            else
            {
                slot.ReserveButton.gameObject.SetActive(false);
            }
        }

        private void UpdateEditorPreviewChargeMeter(PlayerSlotView slot, CharacterDefinition front, Color accent)
        {
            if (slot == null || slot.AbilityArcFill == null || front == null)
                return;

            int maxDividerCount = slot.AbilityDividerPivots != null ? slot.AbilityDividerPivots.Count : 0;
            int totalSegments = Mathf.Clamp(front.UltimateAbilityChargeRequirement, 0, maxDividerCount);
            int specialThreshold = Mathf.Clamp(front.SpecialAbilityChargeRequirement, 0, totalSegments);
            int currentCharge = totalSegments > 0 ? specialThreshold : 0;

            slot.AbilityArcFill.fillAmount = totalSegments > 0 ? currentCharge / (float)totalSegments : 0f;
            slot.AbilityArcFill.color = Color.Lerp(new Color(0.42f, 0.7f, 0.94f, 0.92f), accent, 0.12f);
            UpdateAbilityChargeDividers(slot, totalSegments, specialThreshold, false, false, 0f);
        }

        private CharacterDefinition ResolvePreviewFrontDefinition(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < editorPreviewPlayers.Count && laneIndex < EditorPreviewFrontlineCount
                ? editorPreviewPlayers[laneIndex]
                : null;
        }

        private CharacterDefinition ResolvePreviewReserveDefinition(int laneIndex)
        {
            int reserveIndex = laneIndex + EditorPreviewFrontlineCount;
            return reserveIndex >= 0 && reserveIndex < editorPreviewPlayers.Count
                ? editorPreviewPlayers[reserveIndex]
                : null;
        }

        private static Sprite ResolvePreviewHudPortrait(CharacterDefinition definition)
        {
            if (definition == null)
                return null;

            Sprite portrait = definition.PfpSprite;
            return portrait != null ? portrait : definition.BattleSprite;
        }

        private static string ResolvePreviewCharacterName(CharacterDefinition definition)
        {
            if (definition == null)
                return "-";

            if (!string.IsNullOrWhiteSpace(definition.CharacterName))
                return definition.CharacterName;

            if (!string.IsNullOrWhiteSpace(definition.DisplayNameEN))
                return definition.DisplayNameEN;

            return definition.name.Replace("_CharacterDefinition", string.Empty);
        }

        private static Color ResolvePreviewAccentColor(CharacterDefinition definition)
        {
            if (definition == null)
                return new Color(0.34f, 0.48f, 0.62f, 0.96f);

            Color accent = definition.PaletteAccentColor;
            accent.a = 0.96f;
            return accent;
        }

        private void ClearEditorPreviewIndicators()
        {
            if (turnIndicators.Count == 0)
                return;

            foreach (KeyValuePair<CharacterInstance, TurnCountdownIndicator> pair in turnIndicators)
            {
                if (pair.Value != null)
                    SafeDestroy(pair.Value.gameObject);
            }

            turnIndicators.Clear();
        }

        private static void SafeClearChildren(Transform parent)
        {
            if (parent == null)
                return;

            for (int i = parent.childCount - 1; i >= 0; i--)
                SafeDestroy(parent.GetChild(i).gameObject);
        }

        private static void SafeDestroy(Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }
    }
}


