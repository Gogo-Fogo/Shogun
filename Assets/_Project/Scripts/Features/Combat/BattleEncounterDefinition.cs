using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    [CreateAssetMenu(menuName = "Shogun/Combat/Battle Encounter Definition", fileName = "NewBattleEncounter")]
    public sealed class BattleEncounterDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string encounterId = "courtyard-ambush";
        [SerializeField] private string displayName = "Courtyard Ambush";
        [SerializeField] [TextArea(2, 4)] private string introSubtitle =
            "Break the three-point ambush before the line closes around the squad.";
        [SerializeField] [TextArea(2, 4)] private string description =
            "Ryoma, Kuro, and Tsukiko are forced to break a three-unit enemy line before the ambush closes around them.";

        [Header("Objective Copy")]
        [SerializeField] private string objectiveLabel = "DEFEAT ALL ENEMIES";
        [SerializeField] [TextArea(2, 4)] private string victorySubtitle =
            "The ambush is broken. Choose your next step.";
        [SerializeField] [TextArea(2, 4)] private string defeatSubtitle =
            "The ambush held. Regroup and try the encounter again.";

        [Header("Pressure Zone")]
        [SerializeField] private bool usePressureZone = true;
        [SerializeField] private string pressureZoneLabel = "AMBUSH LANE";
        [SerializeField] private Vector2 pressureZoneCenterOffset = new Vector2(-2.35f, 0f);
        [SerializeField] private Vector2 pressureZoneSize = new Vector2(2.8f, 3.3f);
        [SerializeField] private float pressureZoneChipDamage = 10f;

        [Header("Result Preview")]
        [SerializeField] private string victoryRewardPreview = "+120 EXP  •  +1 Court Intel";

        [Header("Rosters")]
        [SerializeField] private List<string> playerCharacterIds = new List<string>();
        [SerializeField] private List<string> enemyCharacterIds = new List<string>();

        [Header("Frontline Layout Offsets")]
        [SerializeField] private List<Vector2> playerFrontlineOffsets = new List<Vector2>();
        [SerializeField] private List<Vector2> enemyFrontlineOffsets = new List<Vector2>();

        public string EncounterId => string.IsNullOrWhiteSpace(encounterId) ? name : encounterId.Trim();
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
        public string IntroSubtitle => string.IsNullOrWhiteSpace(introSubtitle) ? Description : introSubtitle.Trim();
        public string Description => description ?? string.Empty;
        public string ObjectiveLabel => string.IsNullOrWhiteSpace(objectiveLabel) ? "DEFEAT ALL ENEMIES" : objectiveLabel.Trim();
        public string VictorySubtitle => string.IsNullOrWhiteSpace(victorySubtitle) ? "The battle is decided. Choose your next step." : victorySubtitle.Trim();
        public string DefeatSubtitle => string.IsNullOrWhiteSpace(defeatSubtitle) ? "Regroup and try the encounter again." : defeatSubtitle.Trim();
        public bool HasPressureZone => usePressureZone && pressureZoneChipDamage > 0.01f && pressureZoneSize.x > 0.1f && pressureZoneSize.y > 0.1f;
        public string PressureZoneLabel => string.IsNullOrWhiteSpace(pressureZoneLabel) ? "AMBUSH LANE" : pressureZoneLabel.Trim();
        public float PressureZoneChipDamage => Mathf.Max(0f, pressureZoneChipDamage);
        public string VictoryRewardPreview => string.IsNullOrWhiteSpace(victoryRewardPreview) ? "+120 EXP  •  +1 Court Intel" : victoryRewardPreview.Trim();
        public bool HasFrontlineOffsets => (playerFrontlineOffsets != null && playerFrontlineOffsets.Count > 0) || (enemyFrontlineOffsets != null && enemyFrontlineOffsets.Count > 0);

        public List<CharacterDefinition> ResolvePlayerTeam()
        {
            return ResolveRoster(playerCharacterIds, "player");
        }

        public List<CharacterDefinition> ResolveEnemyTeam()
        {
            return ResolveRoster(enemyCharacterIds, "enemy");
        }

        public string BuildObjectiveStatusText(int aliveEnemies, int totalEnemies)
        {
            return $"{ObjectiveLabel}  {aliveEnemies}/{Mathf.Max(1, totalEnemies)}";
        }

        public Vector3 GetPlayerFrontlineOffset(int laneIndex)
        {
            return GetOffset(playerFrontlineOffsets, laneIndex);
        }

        public Vector3 GetEnemyFrontlineOffset(int laneIndex)
        {
            return GetOffset(enemyFrontlineOffsets, laneIndex);
        }

        public Bounds GetPressureZoneBounds(Vector3 battleCenter)
        {
            Vector3 center = battleCenter + new Vector3(pressureZoneCenterOffset.x, pressureZoneCenterOffset.y, 0f);
            Vector3 size = new Vector3(
                Mathf.Max(0.1f, pressureZoneSize.x),
                Mathf.Max(0.1f, pressureZoneSize.y),
                1f);
            return new Bounds(center, size);
        }

        private static Vector3 GetOffset(List<Vector2> offsets, int laneIndex)
        {
            if (offsets == null || laneIndex < 0 || laneIndex >= offsets.Count)
                return Vector3.zero;

            Vector2 offset = offsets[laneIndex];
            return new Vector3(offset.x, offset.y, 0f);
        }

        private static List<CharacterDefinition> ResolveRoster(List<string> ids, string rosterLabel)
        {
            List<CharacterDefinition> roster = new List<CharacterDefinition>();
            if (ids == null || ids.Count == 0)
                return roster;

            HashSet<CharacterDefinition> uniqueDefinitions = new HashSet<CharacterDefinition>();
            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CharacterDefinition definition = CharacterFactory.GetCharacterDefinition(id);
                if (definition == null)
                {
                    Debug.LogWarning($"BattleEncounterDefinition: Could not resolve {rosterLabel} '{id}'.");
                    continue;
                }

                if (uniqueDefinitions.Add(definition))
                    roster.Add(definition);
            }

            return roster;
        }
    }
}
