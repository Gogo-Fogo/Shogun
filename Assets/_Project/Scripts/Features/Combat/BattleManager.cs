using System;
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
    [SerializeField] private float allyFrontlineVerticalSpacing = 1.15f;
    [SerializeField] private float allyReserveVerticalSpacing = 1.15f;
    [SerializeField] private float allyVerticalSpacingPadding = 0.8f;
    [SerializeField] private float enemyFrontlineVerticalSpacing = 1.15f;
    [SerializeField] private float enemyReserveVerticalSpacing = 1.15f;
    [SerializeField] private float enemyVerticalSpacingPadding = 0.8f;

    private readonly Dictionary<CharacterInstance, Action> playerRosterDeathCallbacks = new Dictionary<CharacterInstance, Action>();

    private float resolvedAllyFrontlineSpacing;
    private float resolvedAllyReserveSpacing;
    private int resolvedAllyActiveCount;
    private int resolvedAllyReserveCount;
    private float resolvedEnemyFrontlineSpacing;
    private float resolvedEnemyReserveSpacing;
    private int resolvedEnemyActiveCount;
    private int resolvedEnemyReserveCount;

    public event Action<int, CharacterInstance, CharacterInstance> OnPlayerFormationChanged;

    private void OnDestroy()
    {
        UnbindPlayerRosterEvents();
    }

    public void StartBattle(List<CharacterDefinition> playerTeam, List<CharacterDefinition> enemyTeam = null)
    {
        UnbindPlayerRosterEvents();

        activeCharacters.Clear();
        reserveCharacters.Clear();
        activeEnemyCharacters.Clear();
        reserveEnemyCharacters.Clear();

        resolvedAllyFrontlineSpacing = Mathf.Max(0.1f, allyFrontlineVerticalSpacing);
        resolvedAllyReserveSpacing = Mathf.Max(0.1f, allyReserveVerticalSpacing);
        resolvedAllyActiveCount = 0;
        resolvedAllyReserveCount = 0;

        if (playerTeam != null && playerTeam.Count > 0)
        {
            resolvedAllyActiveCount = Mathf.Min(3, playerTeam.Count);
            resolvedAllyReserveCount = Mathf.Max(0, playerTeam.Count - resolvedAllyActiveCount);

            resolvedAllyFrontlineSpacing = CalculateAdaptiveVerticalSpacing(
                playerTeam,
                0,
                resolvedAllyActiveCount,
                allyFrontlineVerticalSpacing,
                allyVerticalSpacingPadding);
            resolvedAllyReserveSpacing = CalculateAdaptiveVerticalSpacing(
                playerTeam,
                resolvedAllyActiveCount,
                resolvedAllyReserveCount,
                allyReserveVerticalSpacing,
                allyVerticalSpacingPadding);
        }

        SpawnTeam(playerTeam, activeCharacters, reserveCharacters, GetSpawnPosition, GetReservePosition, faceLeft: false, reservesVisibleOnBattlefield: false);
        BindPlayerRosterEvents();

        resolvedEnemyFrontlineSpacing = Mathf.Max(0.1f, enemyFrontlineVerticalSpacing);
        resolvedEnemyReserveSpacing = Mathf.Max(0.1f, enemyReserveVerticalSpacing);
        resolvedEnemyActiveCount = 0;
        resolvedEnemyReserveCount = 0;

        if (enemyTeam != null && enemyTeam.Count > 0)
        {
            resolvedEnemyActiveCount = Mathf.Min(3, enemyTeam.Count);
            resolvedEnemyReserveCount = Mathf.Max(0, enemyTeam.Count - resolvedEnemyActiveCount);

            resolvedEnemyFrontlineSpacing = CalculateAdaptiveVerticalSpacing(
                enemyTeam,
                0,
                resolvedEnemyActiveCount,
                enemyFrontlineVerticalSpacing,
                enemyVerticalSpacingPadding);
            resolvedEnemyReserveSpacing = CalculateAdaptiveVerticalSpacing(
                enemyTeam,
                resolvedEnemyActiveCount,
                resolvedEnemyReserveCount,
                enemyReserveVerticalSpacing,
                enemyVerticalSpacingPadding);

            SpawnTeam(enemyTeam, activeEnemyCharacters, reserveEnemyCharacters, GetEnemySpawnPosition, GetEnemyReservePosition, faceLeft: true);
        }
    }

    private void SpawnTeam(
        List<CharacterDefinition> team,
        List<CharacterInstance> activeList,
        List<CharacterInstance> reserveList,
        Func<int, Vector3> activePositionResolver,
        Func<int, Vector3> reservePositionResolver,
        bool faceLeft,
        bool reservesVisibleOnBattlefield = true)
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
            SetCharacterBattlefieldActive(instance, true);
            activeList.Add(instance);
        }

        for (int i = 0; i < reserveCount; i++)
        {
            CharacterInstance instance = SpawnCharacter(team[activeCount + i], reservePositionResolver(i), faceLeft);
            SetCharacterBattlefieldActive(instance, reservesVisibleOnBattlefield);
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

    private void BindPlayerRosterEvents()
    {
        UnbindPlayerRosterEvents();
        BindRosterEvents(activeCharacters);
        BindRosterEvents(reserveCharacters);
    }

    private void BindRosterEvents(List<CharacterInstance> roster)
    {
        if (roster == null)
            return;

        for (int i = 0; i < roster.Count; i++)
        {
            CharacterInstance character = roster[i];
            if (character == null || playerRosterDeathCallbacks.ContainsKey(character))
                continue;

            Action callback = () => HandlePlayerRosterDeath(character);
            playerRosterDeathCallbacks[character] = callback;
            character.OnDeath += callback;
        }
    }

    private void UnbindPlayerRosterEvents()
    {
        foreach (KeyValuePair<CharacterInstance, Action> pair in playerRosterDeathCallbacks)
        {
            if (pair.Key != null)
                pair.Key.OnDeath -= pair.Value;
        }

        playerRosterDeathCallbacks.Clear();
    }

    private void HandlePlayerRosterDeath(CharacterInstance character)
    {
        if (character == null)
            return;

        int activeLaneIndex = activeCharacters.IndexOf(character);
        if (activeLaneIndex < 0)
            return;

        CharacterInstance reserve = GetReservePlayerAtLane(activeLaneIndex);
        if (reserve != null && reserve.IsAlive)
        {
            ReplaceDeadPlayerWithReserve(activeLaneIndex, character, reserve);
            return;
        }

        activeCharacters[activeLaneIndex] = null;
        OnPlayerFormationChanged?.Invoke(activeLaneIndex, character, null);
    }

    private void ReplaceDeadPlayerWithReserve(int laneIndex, CharacterInstance outgoingDeadCharacter, CharacterInstance incomingReserve)
    {
        if (laneIndex < 0 || laneIndex >= activeCharacters.Count || incomingReserve == null)
            return;

        Vector3 activeLaneWorldPosition = outgoingDeadCharacter != null
            ? outgoingDeadCharacter.transform.position
            : GetSpawnPosition(laneIndex);

        activeCharacters[laneIndex] = incomingReserve;
        if (laneIndex < reserveCharacters.Count)
            reserveCharacters[laneIndex] = null;

        if (outgoingDeadCharacter != null)
        {
            outgoingDeadCharacter.SpawnDeathAnimationProxy();
            SetCharacterBattlefieldActive(outgoingDeadCharacter, false);
        }

        SetCharacterBattlefieldActive(incomingReserve, true);
        PositionCharacterAtLane(incomingReserve, laneIndex, true, faceLeft: false, worldPositionOverride: activeLaneWorldPosition);
        OnPlayerFormationChanged?.Invoke(laneIndex, outgoingDeadCharacter, incomingReserve);
    }

    public bool TrySwapPlayerLane(int laneIndex, out CharacterInstance outgoingActive, out CharacterInstance incomingReserve)
    {
        outgoingActive = GetActivePlayerAtLane(laneIndex);
        incomingReserve = GetReservePlayerAtLane(laneIndex);

        if (laneIndex < 0 || laneIndex >= activeCharacters.Count)
            return false;

        if (incomingReserve == null || !incomingReserve.IsAlive)
            return false;

        SwapPlayerLaneInternal(laneIndex, outgoingActive, incomingReserve);
        return true;
    }

    private void SwapPlayerLaneInternal(int laneIndex, CharacterInstance outgoingActive, CharacterInstance incomingReserve)
    {
        if (laneIndex < 0 || laneIndex >= activeCharacters.Count || incomingReserve == null)
            return;

        Vector3 activeLaneWorldPosition = outgoingActive != null
            ? outgoingActive.transform.position
            : GetSpawnPosition(laneIndex);

        activeCharacters[laneIndex] = incomingReserve;
        if (laneIndex < reserveCharacters.Count)
            reserveCharacters[laneIndex] = outgoingActive;

        SetCharacterBattlefieldActive(incomingReserve, true);
        PositionCharacterAtLane(incomingReserve, laneIndex, true, faceLeft: false, worldPositionOverride: activeLaneWorldPosition);
        if (outgoingActive != null)
        {
            PositionCharacterAtLane(outgoingActive, laneIndex, false, faceLeft: false);
            SetCharacterBattlefieldActive(outgoingActive, false);
        }

        OnPlayerFormationChanged?.Invoke(laneIndex, outgoingActive, incomingReserve);
    }

    private void PositionCharacterAtLane(CharacterInstance instance, int laneIndex, bool activeLane, bool faceLeft, Vector3? worldPositionOverride = null)
    {
        if (instance == null)
            return;

        Vector3 worldPosition = worldPositionOverride ?? (activeLane ? GetSpawnPosition(laneIndex) : GetReservePosition(laneIndex));
        instance.transform.position = worldPosition;
        ApplyFacing(instance, faceLeft);
        SetCharacterBattlefieldActive(instance, activeLane);

        Animator animator = instance.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetBool("isRunning", false);
        }
    }

    private Vector3 GetSpawnPosition(int index)
    {
        return GetCenteredVerticalLanePosition(GetBattleCenter(), allyFrontlineOrigin, resolvedAllyFrontlineSpacing, index, resolvedAllyActiveCount);
    }

    private Vector3 GetReservePosition(int index)
    {
        return GetCenteredVerticalLanePosition(GetBattleCenter(), allyReserveOrigin, resolvedAllyReserveSpacing, index, resolvedAllyReserveCount);
    }

    private Vector3 GetEnemySpawnPosition(int index)
    {
        return GetCenteredVerticalLanePosition(GetBattleCenter(), enemyFrontlineOrigin, resolvedEnemyFrontlineSpacing, index, resolvedEnemyActiveCount);
    }

    private Vector3 GetEnemyReservePosition(int index)
    {
        return GetCenteredVerticalLanePosition(GetBattleCenter(), enemyReserveOrigin, resolvedEnemyReserveSpacing, index, resolvedEnemyReserveCount);
    }

    private float CalculateAdaptiveVerticalSpacing(List<CharacterDefinition> team, int startIndex, int count, float minimumSpacing, float padding)
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

        return Mathf.Max(fallback, maxEstimatedHeight + Mathf.Max(0f, padding));
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

    private static void SetCharacterBattlefieldActive(CharacterInstance instance, bool isActiveOnBattlefield)
    {
        if (instance == null)
            return;

        if (instance.gameObject.activeSelf != isActiveOnBattlefield)
            instance.gameObject.SetActive(isActiveOnBattlefield);
    }

    public int GetPlayerLaneCount()
    {
        return Mathf.Max(activeCharacters.Count, reserveCharacters.Count);
    }

    public CharacterInstance GetActivePlayerAtLane(int laneIndex)
    {
        return GetCharacterAtLane(activeCharacters, laneIndex);
    }

    public CharacterInstance GetReservePlayerAtLane(int laneIndex)
    {
        return GetCharacterAtLane(reserveCharacters, laneIndex);
    }

    public bool HasReservePlayerAtLane(int laneIndex)
    {
        return GetReservePlayerAtLane(laneIndex) != null;
    }

    public int GetPlayerLaneForCharacter(CharacterInstance character)
    {
        if (character == null)
            return -1;

        int activeLane = activeCharacters.IndexOf(character);
        if (activeLane >= 0)
            return activeLane;

        return reserveCharacters.IndexOf(character);
    }

    public List<CharacterInstance> GetCurrentPlayerCharacters()
    {
        return activeCharacters;
    }

    public List<CharacterInstance> GetCurrentEnemyCharacters()
    {
        return activeEnemyCharacters;
    }

    public List<CharacterInstance> GetAllPlayerCharacters()
    {
        List<CharacterInstance> roster = new List<CharacterInstance>(activeCharacters.Count + reserveCharacters.Count);
        AddUniqueCharacters(roster, activeCharacters);
        AddUniqueCharacters(roster, reserveCharacters);
        return roster;
    }

    public float GetTotalPlayerHealth()
    {
        float total = 0f;
        List<CharacterInstance> roster = GetAllPlayerCharacters();
        for (int i = 0; i < roster.Count; i++)
        {
            CharacterInstance character = roster[i];
            if (character != null)
                total += Mathf.Max(0f, character.CurrentHealth);
        }

        return total;
    }

    public float GetTotalPlayerMaxHealth()
    {
        float total = 0f;
        List<CharacterInstance> roster = GetAllPlayerCharacters();
        for (int i = 0; i < roster.Count; i++)
        {
            CharacterInstance character = roster[i];
            if (character != null)
                total += Mathf.Max(0f, character.MaxHealth);
        }

        return total;
    }

    public List<CharacterInstance> GetAllActiveCombatants()
    {
        List<CharacterInstance> combatants = new List<CharacterInstance>(activeCharacters.Count + activeEnemyCharacters.Count);
        AddAliveCharacters(combatants, activeCharacters);
        AddAliveCharacters(combatants, activeEnemyCharacters);
        return combatants;
    }

    private static CharacterInstance GetCharacterAtLane(List<CharacterInstance> characters, int laneIndex)
    {
        if (characters == null || laneIndex < 0 || laneIndex >= characters.Count)
            return null;

        return characters[laneIndex];
    }

    private static void AddUniqueCharacters(List<CharacterInstance> destination, List<CharacterInstance> source)
    {
        if (destination == null || source == null)
            return;

        for (int i = 0; i < source.Count; i++)
        {
            CharacterInstance character = source[i];
            if (character != null && !destination.Contains(character))
                destination.Add(character);
        }
    }

    private static void AddAliveCharacters(List<CharacterInstance> destination, List<CharacterInstance> source)
    {
        if (destination == null || source == null)
            return;

        for (int i = 0; i < source.Count; i++)
        {
            CharacterInstance character = source[i];
            if (character != null && character.IsAlive)
                destination.Add(character);
        }
    }
}
