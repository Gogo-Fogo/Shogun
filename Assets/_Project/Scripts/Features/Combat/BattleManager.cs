using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject characterPrefab; // Assign in Inspector
    public List<CharacterInstance> activeCharacters = new List<CharacterInstance>();
    public List<CharacterInstance> reserveCharacters = new List<CharacterInstance>();

    public void StartBattle(List<CharacterDefinition> team)
    {
        activeCharacters.Clear();
        reserveCharacters.Clear();

        int activeCount = Mathf.Min(3, team.Count);
        int reserveCount = team.Count - activeCount;

        // Spawn active characters
        for (int i = 0; i < activeCount; i++)
        {
            var go = Instantiate(characterPrefab, GetSpawnPosition(i), Quaternion.identity);
            var instance = go.GetComponent<CharacterInstance>();
            instance.Initialize(team[i]);
            activeCharacters.Add(instance);
        }

        // Spawn reserve characters (if any)
        for (int i = 0; i < reserveCount; i++)
        {
            var go = Instantiate(characterPrefab, GetReservePosition(i), Quaternion.identity);
            var instance = go.GetComponent<CharacterInstance>();
            instance.Initialize(team[activeCount + i]);
            reserveCharacters.Add(instance);
        }
    }

    private Vector3 GetSpawnPosition(int index)
    {
        // Always spawn at the prefab's current scene position
        return characterPrefab.transform.position;
    }
    private Vector3 GetReservePosition(int index)
    {
        // TODO: Implement spawn logic for reserve characters
        return new Vector3(-5 + index * 2.5f, -3, 0);
    }

    public List<CharacterInstance> GetCurrentPlayerCharacters()
    {
        return activeCharacters;
    }
} 