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

    public void StartBattle(List<CharacterDefinition> playerTeam, List<CharacterDefinition> enemyTeam = null)
    {
        activeCharacters.Clear();
        reserveCharacters.Clear();
        activeEnemyCharacters.Clear();
        reserveEnemyCharacters.Clear();

        SpawnTeam(playerTeam, activeCharacters, reserveCharacters, GetSpawnPosition, GetReservePosition, faceLeft: false);

        if (enemyTeam != null && enemyTeam.Count > 0)
        {
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
        Vector3 scale = instance.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceLeft ? -1f : 1f);
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
        return GetLanePosition(GetBattleCenter(), enemyFrontlineOrigin, frontlineLaneSpacing, index);
    }

    private Vector3 GetEnemyReservePosition(int index)
    {
        return GetLanePosition(GetBattleCenter(), enemyReserveOrigin, reserveLaneSpacing, index);
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
