using UnityEngine;
using System.Collections.Generic;
using Shogun.Features.Characters;
using Shogun.Features.Combat;

public class TestBattleSetup : MonoBehaviour
{
    public BattleManager battleManager;
    public TurnManager turnManager;
    public List<CharacterDefinition> testTeam; // Assign 6 in Inspector

    void Start()
    {
        if (testTeam == null || testTeam.Count < 1)
        {
            Debug.LogError("Assign at least 1 CharacterDefinition asset to testTeam!");
            return;
        }

        battleManager.StartBattle(testTeam);
        turnManager.turnOrder = new List<CharacterInstance>(battleManager.activeCharacters);
        turnManager.CurrentTurnIndex = 0;
        Debug.Log("Battle started and turn order set!");
    }
} 