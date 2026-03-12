using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shogun.Features.Characters;
using Shogun.Features.Combat;

public class TestBattleSetup : MonoBehaviour
{
    private const string DefaultEncounterResourcePath = "CourtyardAmbush_Encounter";

    public BattleManager battleManager;
    public TurnManager turnManager;
    public List<CharacterDefinition> testTeam; // Inspector fallback only
    public List<CharacterDefinition> enemyTeam;

    [Header("Authored Encounter")]
    [SerializeField] private bool useAuthoredEncounter = true;
    [SerializeField] private BattleEncounterDefinition encounterDefinition;
    [SerializeField] private string encounterResourcePath = DefaultEncounterResourcePath;

    [Header("Debug Player Override")]
    [SerializeField] private bool useDebugPlayerRoster = true;
    [SerializeField] private List<string> debugPlayerNames = new List<string>
    {
        "ryoma",
        "daichi",
        "harada",
        "katsuro",
        "takeshi",
        "okami-jin"
    };

    [Header("Debug Enemy Override")]
    [SerializeField] private bool useDebugEnemyRoster = true;
    [SerializeField] private List<string> debugEnemyNames = new List<string> { "akai", "reiji", "kumada" };
    [SerializeField] private bool useSingleDebugEnemy = true;
    [SerializeField] private string debugEnemyName = "akai";
    [SerializeField] private float extraEnemyDistance = 2.5f;

    private BattleEncounterDefinition resolvedEncounterDefinition;

    void Start()
    {
        resolvedEncounterDefinition = ResolveEncounterDefinition();

        List<CharacterDefinition> selectedPlayers = BuildPlayerTeam();
        if (selectedPlayers == null || selectedPlayers.Count < 1)
        {
            Debug.LogError("No allied definitions resolved. Assign testTeam or provide valid debug player CharacterDefinition ids.");
            return;
        }

        List<CharacterDefinition> selectedEnemies = BuildEnemyTeam();
        if (selectedEnemies == null || selectedEnemies.Count < 1)
        {
            Debug.LogError("No enemy definitions resolved. Assign enemyTeam or provide valid debug CharacterDefinition assets.");
            return;
        }

        battleManager.StartBattle(selectedPlayers, selectedEnemies);
        ApplyEncounterFrontlineOffsets();
        PushEnemiesFurtherAwayIfNeeded(extraEnemyDistance);

        turnManager.AttachBattleManager(battleManager);
        turnManager.Initialize(
            battleManager.activeCharacters,
            battleManager.activeEnemyCharacters,
            battleManager.GetAllPlayerCharacters());
        EnsureEncounterRuntimeControllers();
        turnManager.StartBattle();

        string encounterName = GetEncounterDisplayName();
        Debug.Log($"Battle started for '{encounterName}' with {battleManager.activeCharacters.Count} allied frontliners, {battleManager.reserveCharacters.Count} allied reserves, and {battleManager.activeEnemyCharacters.Count} enemy combatants.");
    }

    public List<CharacterDefinition> GetPreviewPlayerTeam()
    {
        return BuildPlayerTeam();
    }

    public List<CharacterDefinition> GetPreviewEnemyTeam()
    {
        return BuildEnemyTeam();
    }

    public string GetEncounterDisplayName()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        return encounter != null ? encounter.DisplayName : "Sandbox Battle";
    }

    public string GetEncounterIntroSubtitle()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        return encounter != null ? encounter.IntroSubtitle : "Prepare the squad and break the enemy line.";
    }

    public string GetEncounterObjectiveStatusText(int aliveEnemies, int totalEnemies)
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        return encounter != null
            ? encounter.BuildObjectiveStatusText(aliveEnemies, totalEnemies)
            : $"DEFEAT ALL ENEMIES  {aliveEnemies}/{Mathf.Max(1, totalEnemies)}";
    }

    public string GetEncounterVictorySubtitle()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        return encounter != null ? encounter.VictorySubtitle : "The battle is decided. Choose your next step.";
    }

    public string GetEncounterDefeatSubtitle()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        return encounter != null ? encounter.DefeatSubtitle : "Regroup and try the encounter again.";
    }

    public string GetEncounterVictoryRewardPreview()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        return encounter != null ? encounter.VictoryRewardPreview : "+120 EXP  •  +1 Court Intel";
    }

    private void EnsureEncounterRuntimeControllers()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        if (encounter == null)
            return;

        EncounterPressureZoneController pressureZoneController = GetComponent<EncounterPressureZoneController>();
        if (pressureZoneController == null)
            pressureZoneController = gameObject.AddComponent<EncounterPressureZoneController>();

        pressureZoneController.Configure(encounter, battleManager, turnManager);
    }

    private List<CharacterDefinition> BuildPlayerTeam()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        if (encounter != null)
        {
            List<CharacterDefinition> authoredPlayers = encounter.ResolvePlayerTeam();
            if (authoredPlayers.Count > 0)
                return authoredPlayers;

            Debug.LogWarning($"Encounter '{encounter.DisplayName}' did not resolve any player units. Falling back to debug or inspector rosters.");
        }

        if (useDebugPlayerRoster && debugPlayerNames != null && debugPlayerNames.Count > 0)
        {
            List<CharacterDefinition> resolvedPlayers = ResolveRoster(debugPlayerNames, testTeam, "player");
            if (resolvedPlayers.Count > 0)
                return resolvedPlayers;

            Debug.LogWarning("Debug player roster is enabled, but no entries were resolved. Falling back to inspector testTeam.");
        }

        return testTeam != null ? new List<CharacterDefinition>(testTeam) : new List<CharacterDefinition>();
    }

    private List<CharacterDefinition> BuildEnemyTeam()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        if (encounter != null)
        {
            List<CharacterDefinition> authoredEnemies = encounter.ResolveEnemyTeam();
            if (authoredEnemies.Count > 0)
                return authoredEnemies;

            Debug.LogWarning($"Encounter '{encounter.DisplayName}' did not resolve any enemies. Falling back to debug or inspector rosters.");
        }

        if (useDebugEnemyRoster && debugEnemyNames != null && debugEnemyNames.Count > 0)
        {
            List<CharacterDefinition> resolvedEnemies = ResolveRoster(debugEnemyNames, enemyTeam, "enemy");
            if (resolvedEnemies.Count > 0)
                return resolvedEnemies;

            Debug.LogWarning("Debug enemy roster is enabled, but no entries were resolved. Falling back to other enemy sources.");
        }

        if (useSingleDebugEnemy)
        {
            CharacterDefinition debugEnemy = ResolveDefinitionByName(debugEnemyName, enemyTeam);
            if (debugEnemy != null)
            {
                return new List<CharacterDefinition> { debugEnemy };
            }

            Debug.LogWarning($"Debug enemy '{debugEnemyName}' was not found. Falling back to enemyTeam list.");
        }

        return enemyTeam != null ? new List<CharacterDefinition>(enemyTeam) : new List<CharacterDefinition>();
    }

    private BattleEncounterDefinition ResolveEncounterDefinition()
    {
        if (!useAuthoredEncounter)
            return null;

        if (encounterDefinition != null)
        {
            resolvedEncounterDefinition = encounterDefinition;
            return resolvedEncounterDefinition;
        }

        if (resolvedEncounterDefinition != null)
            return resolvedEncounterDefinition;

        string resourcePath = string.IsNullOrWhiteSpace(encounterResourcePath)
            ? DefaultEncounterResourcePath
            : encounterResourcePath.Trim();

        resolvedEncounterDefinition = Resources.Load<BattleEncounterDefinition>(resourcePath);
        if (resolvedEncounterDefinition == null)
        {
            Debug.LogWarning($"TestBattleSetup could not load BattleEncounterDefinition from Resources/{resourcePath}. Falling back to debug or inspector rosters.");
            return null;
        }

        encounterDefinition = resolvedEncounterDefinition;
        return resolvedEncounterDefinition;
    }

    private void ApplyEncounterFrontlineOffsets()
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        if (encounter == null || !encounter.HasFrontlineOffsets || battleManager == null)
            return;

        ApplyOffsetsToRoster(battleManager.activeCharacters, encounter.GetPlayerFrontlineOffset);
        ApplyOffsetsToRoster(battleManager.activeEnemyCharacters, encounter.GetEnemyFrontlineOffset);
    }

    private static void ApplyOffsetsToRoster(List<CharacterInstance> roster, Func<int, Vector3> offsetResolver)
    {
        if (roster == null || offsetResolver == null)
            return;

        for (int i = 0; i < roster.Count; i++)
        {
            CharacterInstance character = roster[i];
            if (character == null)
                continue;

            character.transform.position += offsetResolver(i);
        }
    }

    private void PushEnemiesFurtherAwayIfNeeded(float distance)
    {
        BattleEncounterDefinition encounter = ResolveEncounterDefinition();
        if (encounter != null && encounter.HasFrontlineOffsets)
            return;

        PushEnemiesFurtherAway(distance);
    }

    private List<CharacterDefinition> ResolveRoster(List<string> candidateNames, List<CharacterDefinition> inspectorFallback, string rosterLabel)
    {
        List<CharacterDefinition> resolvedRoster = new List<CharacterDefinition>();
        HashSet<CharacterDefinition> resolvedSet = new HashSet<CharacterDefinition>();

        for (int i = 0; i < candidateNames.Count; i++)
        {
            string candidateName = candidateNames[i];
            CharacterDefinition definition = ResolveDefinitionByName(candidateName, inspectorFallback);
            if (definition == null)
            {
                Debug.LogWarning($"Debug {rosterLabel} '{candidateName}' was not found while building roster.");
                continue;
            }

            if (resolvedSet.Add(definition))
                resolvedRoster.Add(definition);
        }

        return resolvedRoster;
    }

    private CharacterDefinition ResolveDefinitionByName(string characterName, List<CharacterDefinition> inspectorFallback)
    {
        if (string.IsNullOrWhiteSpace(characterName))
            return null;

        if (characterName.Equals("Kuamada", StringComparison.OrdinalIgnoreCase))
            characterName = "kumada";

        if (inspectorFallback != null)
        {
            foreach (CharacterDefinition def in inspectorFallback)
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
