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
        GameObject go = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
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
        Vector3 basePosition = characterPrefab.transform.position;
        float[] laneOffsets = { -2.5f, 0f, 2.5f };
        float offsetX = index >= 0 && index < laneOffsets.Length ? laneOffsets[index] : index * 2.5f;
        return basePosition + new Vector3(offsetX, 0f, 0f);
    }

    private Vector3 GetReservePosition(int index)
    {
        return new Vector3(-5 + index * 2.5f, -3, 0);
    }

    private Vector3 GetEnemySpawnPosition(int index)
    {
        Vector3 basePosition = GetEnemyAnchorPosition();
        float[] laneOffsets = { -2.5f, 0f, 2.5f };
        float offsetX = index >= 0 && index < laneOffsets.Length ? laneOffsets[index] : index * 2.5f;
        return basePosition + new Vector3(-offsetX, 0f, 0f);
    }

    private Vector3 GetEnemyReservePosition(int index)
    {
        Vector3 basePosition = GetEnemyAnchorPosition();
        return basePosition + new Vector3(5f + index * 2.5f, -3f, 0f);
    }

    private Vector3 GetEnemyAnchorPosition()
    {
        if (characterPrefab != null && characterPrefab.transform.parent != null)
        {
            Transform parent = characterPrefab.transform.parent;
            Vector3 mirroredLocalPosition = characterPrefab.transform.localPosition;
            mirroredLocalPosition.x *= -1f;
            return parent.TransformPoint(mirroredLocalPosition);
        }

        return characterPrefab.transform.position + new Vector3(26f, 0f, 0f);
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
