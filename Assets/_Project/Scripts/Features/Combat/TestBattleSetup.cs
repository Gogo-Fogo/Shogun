using UnityEngine;
using System.Collections.Generic;
using Shogun.Features.Characters;
using Shogun.Features.Combat;

public class TestBattleSetup : MonoBehaviour
{
    public BattleManager battleManager;
    public TurnManager turnManager;
    public List<CharacterDefinition> testTeam; // Assign 6 in Inspector
    public List<CharacterDefinition> enemyTeam;

    void Start()
    {
        if (testTeam == null || testTeam.Count < 1)
        {
            Debug.LogError("Assign at least 1 CharacterDefinition asset to testTeam!");
            return;
        }

        if (enemyTeam == null || enemyTeam.Count < 1)
        {
            Debug.LogError("Assign at least 1 CharacterDefinition asset to enemyTeam!");
            return;
        }

        battleManager.StartBattle(testTeam, enemyTeam);

        turnManager.Initialize(battleManager.activeCharacters, battleManager.activeEnemyCharacters);
        turnManager.StartBattle();

        Debug.Log($"Battle started with {battleManager.activeCharacters.Count} allied combatants and {battleManager.activeEnemyCharacters.Count} enemy combatants.");
    }
} 
