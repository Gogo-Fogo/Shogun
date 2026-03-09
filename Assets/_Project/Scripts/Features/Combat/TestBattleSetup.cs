using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shogun.Features.Characters;
using Shogun.Features.Combat;

public class TestBattleSetup : MonoBehaviour
{
    public BattleManager battleManager;
    public TurnManager turnManager;
    public List<CharacterDefinition> testTeam; // Assign at least one in Inspector
    public List<CharacterDefinition> enemyTeam;

    [Header("Debug Enemy Override")]
    [SerializeField] private bool useDebugEnemyRoster = true;
    [SerializeField] private List<string> debugEnemyNames = new List<string> { "akai", "reiji", "kumada" };
    [SerializeField] private bool useSingleDebugEnemy = true;
    [SerializeField] private string debugEnemyName = "akai";
    [SerializeField] private float extraEnemyDistance = 2.5f;

    void Start()
    {
        if (testTeam == null || testTeam.Count < 1)
        {
            Debug.LogError("Assign at least 1 CharacterDefinition asset to testTeam!");
            return;
        }

        List<CharacterDefinition> selectedEnemies = BuildEnemyTeam();
        if (selectedEnemies == null || selectedEnemies.Count < 1)
        {
            Debug.LogError("No enemy definitions resolved. Assign enemyTeam or provide valid debug CharacterDefinition assets.");
            return;
        }

        battleManager.StartBattle(testTeam, selectedEnemies);
        PushEnemiesFurtherAway(extraEnemyDistance);

        turnManager.Initialize(battleManager.activeCharacters, battleManager.activeEnemyCharacters);
        turnManager.StartBattle();

        Debug.Log($"Battle started with {battleManager.activeCharacters.Count} allied combatants and {battleManager.activeEnemyCharacters.Count} enemy combatants.");
    }

    private List<CharacterDefinition> BuildEnemyTeam()
    {
        if (useDebugEnemyRoster && debugEnemyNames != null && debugEnemyNames.Count > 0)
        {
            List<CharacterDefinition> resolvedEnemies = new List<CharacterDefinition>();
            HashSet<CharacterDefinition> resolvedSet = new HashSet<CharacterDefinition>();

            foreach (string candidateName in debugEnemyNames)
            {
                CharacterDefinition debugEnemy = ResolveDefinitionByName(candidateName);
                if (debugEnemy == null)
                {
                    Debug.LogWarning($"Debug enemy '{candidateName}' was not found while building roster.");
                    continue;
                }

                if (resolvedSet.Add(debugEnemy))
                    resolvedEnemies.Add(debugEnemy);
            }

            if (resolvedEnemies.Count > 0)
                return resolvedEnemies;

            Debug.LogWarning("Debug enemy roster is enabled, but no entries were resolved. Falling back to other enemy sources.");
        }

        if (useSingleDebugEnemy)
        {
            CharacterDefinition debugEnemy = ResolveDefinitionByName(debugEnemyName);
            if (debugEnemy != null)
            {
                return new List<CharacterDefinition> { debugEnemy };
            }

            Debug.LogWarning($"Debug enemy '{debugEnemyName}' was not found. Falling back to enemyTeam list.");
        }

        return enemyTeam != null ? new List<CharacterDefinition>(enemyTeam) : new List<CharacterDefinition>();
    }

    private CharacterDefinition ResolveDefinitionByName(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName))
            return null;

        if (characterName.Equals("Kuamada", StringComparison.OrdinalIgnoreCase))
            characterName = "kumada";

        if (enemyTeam != null)
        {
            foreach (CharacterDefinition def in enemyTeam)
            {
                if (IsDefinitionMatch(def, characterName))
                    return def;
            }
        }

        return CharacterFactory.GetCharacterDefinition(characterName);
    }

    private static bool IsDefinitionMatch(CharacterDefinition def, string characterName)
    {
        if (def == null)
            return false;

        string normalizedLookup = CharacterKeyUtility.NormalizeLookupKey(characterName);
        return def.GetLookupTerms().Any(term => CharacterKeyUtility.NormalizeLookupKey(term) == normalizedLookup)
               || def.CharacterId.Equals(CharacterKeyUtility.NormalizeCharacterId(characterName), StringComparison.OrdinalIgnoreCase)
               || def.GivenName.Equals(characterName, StringComparison.OrdinalIgnoreCase)
               || def.CharacterName.Equals(characterName, StringComparison.OrdinalIgnoreCase)
               || def.name.Equals(characterName, StringComparison.OrdinalIgnoreCase)
               || def.name.Replace("_CharacterDefinition", string.Empty).Equals(characterName, StringComparison.OrdinalIgnoreCase);
    }

    private void PushEnemiesFurtherAway(float distance)
    {
        if (distance <= 0f || battleManager == null)
            return;

        if (battleManager.activeCharacters == null || battleManager.activeCharacters.Count == 0)
            return;

        if (battleManager.activeEnemyCharacters == null || battleManager.activeEnemyCharacters.Count == 0)
            return;

        float absDistance = Mathf.Abs(distance);
        foreach (CharacterInstance enemy in battleManager.activeEnemyCharacters)
        {
            if (enemy == null || !enemy.IsAlive)
                continue;

            CharacterInstance nearestPlayer = FindClosestAliveCharacter(enemy.transform.position, battleManager.activeCharacters);
            if (nearestPlayer == null)
                continue;

            float direction = Mathf.Sign(enemy.transform.position.x - nearestPlayer.transform.position.x);
            if (Mathf.Approximately(direction, 0f))
                direction = 1f;

            Vector3 pos = enemy.transform.position;
            pos.x += direction * absDistance;
            enemy.transform.position = pos;
            FaceEnemyTowards(enemy, nearestPlayer);
        }
    }

    private static void FaceEnemyTowards(CharacterInstance enemy, CharacterInstance target)
    {
        if (enemy == null || target == null)
            return;

        float direction = target.transform.position.x < enemy.transform.position.x ? -1f : 1f;
        if (enemy.Definition != null && enemy.Definition.InvertFacingX)
            direction *= -1f;

        Vector3 scale = enemy.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        enemy.transform.localScale = scale;
    }

    private static CharacterInstance FindClosestAliveCharacter(Vector3 fromPosition, List<CharacterInstance> candidates)
    {
        CharacterInstance closest = null;
        float bestSq = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            CharacterInstance candidate = candidates[i];
            if (candidate == null || !candidate.IsAlive)
                continue;

            float sq = (candidate.transform.position - fromPosition).sqrMagnitude;
            if (sq < bestSq)
            {
                bestSq = sq;
                closest = candidate;
            }
        }

        return closest;
    }
}
