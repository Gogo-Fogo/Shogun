using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject characterPrefab; // Assign in Inspector
    public List<CharacterInstance> activeCharacters = new List<CharacterInstance>();
    public List<CharacterInstance> reserveCharacters = new List<CharacterInstance>();
    public List<CharacterInstance> activeEnemyCharacters = new List<CharacterInstance>();
    public List<CharacterInstance> reserveEnemyCharacters = new List<CharacterInstance>();

    [Header("Battlefield Layout")]
    [SerializeField] private Transform battleCenter;
    [SerializeField] private Vector3 allyFrontlineOrigin = new Vector3(-6.75f, -0.5f, 0f);
    [SerializeField] private Vector3 enemyFrontlineOrigin = new Vector3(2.25f, -0.5f, 0f);
    [SerializeField] private Vector3 allyReserveOrigin = new Vector3(-9f, -3f, 0f);
    [SerializeField] private Vector3 enemyReserveOrigin = new Vector3(4.5f, -3f, 0f);
    [SerializeField] private float frontlineLaneSpacing = 2.25f;
    [SerializeField] private float reserveLaneSpacing = 2.25f;
    [SerializeField] private float enemyFrontlineVerticalSpacing = 1.15f;
    [SerializeField] private float enemyReserveVerticalSpacing = 1.15f;
    [SerializeField] private float enemyVerticalSpacingPadding = 0.8f;

    private float resolvedEnemyFrontlineSpacing;
    private float resolvedEnemyReserveSpacing;
    private int resolvedEnemyActiveCount;
    private int resolvedEnemyReserveCount;

    public void StartBattle(List<CharacterDefinition> playerTeam, List<CharacterDefinition> enemyTeam = null)
    {
        activeCharacters.Clear();
        reserveCharacters.Clear();
        activeEnemyCharacters.Clear();
        reserveEnemyCharacters.Clear();

        SpawnTeam(playerTeam, activeCharacters, reserveCharacters, GetSpawnPosition, GetReservePosition, faceLeft: false);

        resolvedEnemyFrontlineSpacing = Mathf.Max(0.1f, enemyFrontlineVerticalSpacing);
        resolvedEnemyReserveSpacing = Mathf.Max(0.1f, enemyReserveVerticalSpacing);
        resolvedEnemyActiveCount = 0;
        resolvedEnemyReserveCount = 0;

        if (enemyTeam != null && enemyTeam.Count > 0)
        {
            resolvedEnemyActiveCount = Mathf.Min(3, enemyTeam.Count);
            resolvedEnemyReserveCount = Mathf.Max(0, enemyTeam.Count - resolvedEnemyActiveCount);

            resolvedEnemyFrontlineSpacing = CalculateAdaptiveVerticalSpacing(enemyTeam, 0, resolvedEnemyActiveCount, enemyFrontlineVerticalSpacing);
            resolvedEnemyReserveSpacing = CalculateAdaptiveVerticalSpacing(enemyTeam, resolvedEnemyActiveCount, resolvedEnemyReserveCount, enemyReserveVerticalSpacing);

            SpawnTeam(enemyTeam, activeEnemyCharacters, reserveEnemyCharacters, GetEnemySpawnPosition, GetEnemyReservePosition, faceLeft: true);
        }
    }

    private void SpawnTeam(
        List<CharacterDefinition> team,
        List<CharacterInstance> activeList,
        List<CharacterInstance> reserveList,
        System.Func<int, Vector3> activePositionResolver,
        System.Func<int, Vector3> reservePositionResolver,
        bool faceLeft)
    {
        if (team == null || team.Count == 0)
        {
            return;
        }

        int activeCount = Mathf.Min(3, team.Count);
        int reserveCount = team.Count - activeCount;

        for (int i = 0; i < activeCount; i++)
        {
            CharacterInstance instance = SpawnCharacter(team[i], activePositionResolver(i), faceLeft);
            activeList.Add(instance);
        }

        for (int i = 0; i < reserveCount; i++)
        {
            CharacterInstance instance = SpawnCharacter(team[activeCount + i], reservePositionResolver(i), faceLeft);
            reserveList.Add(instance);
        }
    }

    private CharacterInstance SpawnCharacter(CharacterDefinition definition, Vector3 spawnPosition, bool faceLeft)
    {
        Transform parent = characterPrefab != null ? characterPrefab.transform.parent : null;
        GameObject go = parent != null
            ? Instantiate(characterPrefab, spawnPosition, Quaternion.identity, parent)
            : Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        CharacterInstance instance = go.GetComponent<CharacterInstance>();
        instance.Initialize(definition);
        ApplyFacing(instance, faceLeft);
        return instance;
    }

    private void ApplyFacing(CharacterInstance instance, bool faceLeft)
    {
        float facingSign = faceLeft ? -1f : 1f;
        if (instance.Definition != null && instance.Definition.InvertFacingX)
            facingSign *= -1f;

        Vector3 scale = instance.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingSign;
        instance.transform.localScale = scale;
    }

    private Vector3 GetSpawnPosition(int index)
    {
        return GetLanePosition(GetBattleCenter(), allyFrontlineOrigin, frontlineLaneSpacing, index);
    }

    private Vector3 GetReservePosition(int index)
    {
        return GetLanePosition(GetBattleCenter(), allyReserveOrigin, reserveLaneSpacing, index);
    }

    private Vector3 GetEnemySpawnPosition(int index)
    {
        return GetCenteredVerticalLanePosition(GetBattleCenter(), enemyFrontlineOrigin, resolvedEnemyFrontlineSpacing, index, resolvedEnemyActiveCount);
    }

    private Vector3 GetEnemyReservePosition(int index)
    {
        return GetCenteredVerticalLanePosition(GetBattleCenter(), enemyReserveOrigin, resolvedEnemyReserveSpacing, index, resolvedEnemyReserveCount);
    }

    private float CalculateAdaptiveVerticalSpacing(List<CharacterDefinition> team, int startIndex, int count, float minimumSpacing)
    {
        float fallback = Mathf.Max(0.1f, minimumSpacing);
        if (team == null || count <= 0)
            return fallback;

        float maxEstimatedHeight = 0f;
        int start = Mathf.Max(0, startIndex);
        int end = Mathf.Min(team.Count, start + count);

        for (int i = start; i < end; i++)
        {
            CharacterDefinition definition = team[i];
            if (definition == null)
                continue;

            float colliderHeight = Mathf.Abs(definition.ColliderSize.y * definition.CharacterScale.y);
            float spriteHeight = 0f;
            if (definition.BattleSprite != null)
                spriteHeight = Mathf.Abs(definition.BattleSprite.bounds.size.y * definition.CharacterScale.y);

            float estimatedHeight = Mathf.Max(colliderHeight, spriteHeight);
            if (estimatedHeight > maxEstimatedHeight)
                maxEstimatedHeight = estimatedHeight;
        }

        if (maxEstimatedHeight <= 0f)
            return fallback;

        return Mathf.Max(fallback, maxEstimatedHeight + Mathf.Max(0f, enemyVerticalSpacingPadding));
    }

    private Vector3 GetBattleCenter()
    {
        if (battleCenter != null)
        {
            return battleCenter.position;
        }

        return transform.position;
    }

    private static Vector3 GetLanePosition(Vector3 center, Vector3 originOffset, float laneSpacing, int index)
    {
        return center + originOffset + new Vector3(index * laneSpacing, 0f, 0f);
    }

    private static Vector3 GetCenteredVerticalLanePosition(Vector3 center, Vector3 originOffset, float laneSpacing, int index, int rowCount)
    {
        float spacing = Mathf.Max(0.1f, laneSpacing);
        int count = Mathf.Max(1, rowCount);
        float middle = (count - 1) * 0.5f;
        float yOffset = (middle - index) * spacing;
        return center + originOffset + new Vector3(0f, yOffset, 0f);
    }

    public List<CharacterInstance> GetCurrentPlayerCharacters()
    {
        return activeCharacters;
    }

    public List<CharacterInstance> GetCurrentEnemyCharacters()
    {
        return activeEnemyCharacters;
    }

    public List<CharacterInstance> GetAllActiveCombatants()
    {
        List<CharacterInstance> combatants = new List<CharacterInstance>(activeCharacters.Count + activeEnemyCharacters.Count);
        combatants.AddRange(activeCharacters);
        combatants.AddRange(activeEnemyCharacters);
        return combatants;
    }
}
