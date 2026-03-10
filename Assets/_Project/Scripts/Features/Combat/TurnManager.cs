// TurnManager.cs
// Manages the order of turns for all combatants in a turn-based battle.
// Single authoritative combatants list; subscribes to OnDeath to remove dead units.
// Checks win/loss after each death; fires OnBattleEnded with a BattleResult payload.

using System;
using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    public class TurnManager : MonoBehaviour
    {
        private readonly List<CharacterInstance> combatants = new List<CharacterInstance>();
        private List<CharacterInstance> playerCombatants = new List<CharacterInstance>();
        private List<CharacterInstance> enemyCombatants = new List<CharacterInstance>();
        private List<CharacterInstance> playerRoster = new List<CharacterInstance>();

        private BattleManager battleManager;

        // Store per-instance death callbacks so we can unsubscribe cleanly
        private readonly Dictionary<CharacterInstance, Action> deathCallbacks =
            new Dictionary<CharacterInstance, Action>();

        private int currentTurnIndex = 0;

        /// <summary>Index of the current turn within the active combatants list.</summary>
        public int CurrentTurnIndex => currentTurnIndex;

        /// <summary>Read-only view of active turn order (for diagnostics / UI).</summary>
        public IReadOnlyList<CharacterInstance> turnOrder => combatants;

        public event Action<CharacterInstance> OnTurnStarted;
        public event Action<CharacterInstance> OnTurnEnded;
        public event Action<BattleResult> OnBattleEnded;
        public event Action OnTurnOrderChanged;

        public bool IsBattleActive { get; private set; } = false;

        /// <summary>
        /// Initialize the turn manager with separate player and enemy lists.
        /// Turn order is players first, then enemies (round-robin).
        /// </summary>
        public void Initialize(List<CharacterInstance> players, List<CharacterInstance> enemies, List<CharacterInstance> playerRosterOverride = null)
        {
            ClearDeathSubscriptions();

            playerCombatants = players != null ? new List<CharacterInstance>(players) : new List<CharacterInstance>();
            enemyCombatants = enemies != null ? new List<CharacterInstance>(enemies) : new List<CharacterInstance>();
            playerRoster = playerRosterOverride != null ? new List<CharacterInstance>(playerRosterOverride) : new List<CharacterInstance>(playerCombatants);

            RebuildCombatants(preferredCurrentCombatant: null, resetTurnIndex: true);
        }

        public void AttachBattleManager(BattleManager manager)
        {
            if (battleManager == manager)
                return;

            if (battleManager != null)
                battleManager.OnPlayerFormationChanged -= HandlePlayerFormationChanged;

            battleManager = manager;

            if (battleManager != null)
            {
                battleManager.OnPlayerFormationChanged += HandlePlayerFormationChanged;
                playerRoster = battleManager.GetAllPlayerCharacters();
            }
        }

        private void OnDestroy()
        {
            if (battleManager != null)
                battleManager.OnPlayerFormationChanged -= HandlePlayerFormationChanged;

            ClearDeathSubscriptions();
        }

        private void ClearDeathSubscriptions()
        {
            foreach (KeyValuePair<CharacterInstance, Action> pair in deathCallbacks)
            {
                if (pair.Key != null)
                    pair.Key.OnDeath -= pair.Value;
            }

            deathCallbacks.Clear();
        }

        private void Subscribe(CharacterInstance character)
        {
            if (character == null || deathCallbacks.ContainsKey(character))
                return;

            Action callback = () => OnCombatantDied(character);
            deathCallbacks[character] = callback;
            character.OnDeath += callback;
        }

        private void Unsubscribe(CharacterInstance character)
        {
            if (character == null)
                return;

            if (deathCallbacks.TryGetValue(character, out Action callback))
            {
                character.OnDeath -= callback;
                deathCallbacks.Remove(character);
            }
        }

        public void StartBattle()
        {
            if (!IsBattleActive)
                return;

            StartTurn();
        }

        private void StartTurn()
        {
            if (!IsBattleActive || combatants.Count == 0)
                return;

            if (currentTurnIndex < 0 || currentTurnIndex >= combatants.Count)
                currentTurnIndex = 0;

            CharacterInstance current = combatants[currentTurnIndex];

            // Reset per-turn flags and process status effects (may trigger OnDeath)
            current.StartNewTurn();

            // Status effects may have ended the battle or killed this character
            if (!IsBattleActive)
                return;

            if (!current.IsAlive)
            {
                // OnCombatantDied already adjusted currentTurnIndex; recurse to next
                StartTurn();
                return;
            }

            OnTurnStarted?.Invoke(current);
        }

        public void EndTurn()
        {
            if (!IsBattleActive || combatants.Count == 0)
                return;

            CharacterInstance current = combatants[currentTurnIndex];
            OnTurnEnded?.Invoke(current);
            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            if (!IsBattleActive || combatants.Count == 0)
                return;

            currentTurnIndex = (currentTurnIndex + 1) % combatants.Count;
            OnTurnOrderChanged?.Invoke();
            StartTurn();
        }

        private void OnCombatantDied(CharacterInstance died)
        {
            int deadIndex = combatants.IndexOf(died);
            if (deadIndex >= 0)
            {
                Unsubscribe(died);
                combatants.RemoveAt(deadIndex);

                // Keep currentTurnIndex valid after removal
                if (deadIndex < currentTurnIndex)
                    currentTurnIndex--;
                else if (deadIndex == currentTurnIndex && combatants.Count > 0)
                    currentTurnIndex = currentTurnIndex % combatants.Count;
            }
            else
            {
                Unsubscribe(died);
            }

            CheckBattleEnd();
            OnTurnOrderChanged?.Invoke();
        }

        private void HandlePlayerFormationChanged(int laneIndex, CharacterInstance outgoingActive, CharacterInstance incomingActive)
        {
            if (battleManager == null)
                return;

            CharacterInstance currentBefore = GetCurrentCombatant();
            CharacterInstance preferredCurrent = null;

            playerCombatants = new List<CharacterInstance>(battleManager.GetCurrentPlayerCharacters());
            playerRoster = battleManager.GetAllPlayerCharacters();

            if (currentBefore != null && currentBefore == outgoingActive && incomingActive != null && incomingActive.IsAlive)
            {
                incomingActive.PrepareAsSwapInCurrentTurn();
                preferredCurrent = incomingActive;
            }
            else if (currentBefore != null && currentBefore.IsAlive)
            {
                preferredCurrent = currentBefore;
            }

            RebuildCombatants(preferredCurrent, resetTurnIndex: false);
            CheckBattleEnd();
        }

        private void RebuildCombatants(CharacterInstance preferredCurrentCombatant, bool resetTurnIndex)
        {
            CharacterInstance currentBefore = resetTurnIndex ? null : GetCurrentCombatant();
            List<CharacterInstance> rebuiltCombatants = new List<CharacterInstance>(playerCombatants.Count + enemyCombatants.Count);
            AddAliveCombatants(rebuiltCombatants, playerCombatants);
            AddAliveCombatants(rebuiltCombatants, enemyCombatants);

            HashSet<CharacterInstance> rebuiltSet = new HashSet<CharacterInstance>(rebuiltCombatants);
            List<CharacterInstance> existingSubscriptions = new List<CharacterInstance>(deathCallbacks.Keys);
            for (int i = 0; i < existingSubscriptions.Count; i++)
            {
                CharacterInstance existing = existingSubscriptions[i];
                if (existing == null || !rebuiltSet.Contains(existing))
                    Unsubscribe(existing);
            }

            for (int i = 0; i < rebuiltCombatants.Count; i++)
                Subscribe(rebuiltCombatants[i]);

            combatants.Clear();
            combatants.AddRange(rebuiltCombatants);

            if (combatants.Count == 0)
            {
                currentTurnIndex = 0;
                IsBattleActive = false;
                OnTurnOrderChanged?.Invoke();
                return;
            }

            CharacterInstance desiredCurrent = preferredCurrentCombatant;
            if ((desiredCurrent == null || !combatants.Contains(desiredCurrent)) && currentBefore != null && combatants.Contains(currentBefore))
                desiredCurrent = currentBefore;

            if (resetTurnIndex)
                currentTurnIndex = 0;
            else if (desiredCurrent != null)
                currentTurnIndex = combatants.IndexOf(desiredCurrent);
            else
                currentTurnIndex = Mathf.Clamp(currentTurnIndex, 0, combatants.Count - 1);

            IsBattleActive = !AreAllPlayersDefeated() && !AreAllEnemiesDefeated() && combatants.Count > 0;
            OnTurnOrderChanged?.Invoke();
        }

        private static void AddAliveCombatants(List<CharacterInstance> destination, List<CharacterInstance> source)
        {
            if (destination == null || source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                CharacterInstance combatant = source[i];
                if (combatant != null && combatant.IsAlive && !destination.Contains(combatant))
                    destination.Add(combatant);
            }
        }

        private void CheckBattleEnd()
        {
            if (!IsBattleActive && combatants.Count > 0)
                return;

            bool allPlayersDefeated = AreAllPlayersDefeated();
            bool allEnemiesDefeated = AreAllEnemiesDefeated();

            if (!allPlayersDefeated && !allEnemiesDefeated)
            {
                IsBattleActive = combatants.Count > 0;
                return;
            }

            IsBattleActive = false;
            BattleResult result = allEnemiesDefeated ? BattleResult.Win : BattleResult.Loss;
            OnBattleEnded?.Invoke(result);
        }

        private bool AreAllPlayersDefeated()
        {
            if (playerRoster == null || playerRoster.Count == 0)
                return true;

            for (int i = 0; i < playerRoster.Count; i++)
            {
                CharacterInstance player = playerRoster[i];
                if (player != null && player.IsAlive)
                    return false;
            }

            return true;
        }

        private bool AreAllEnemiesDefeated()
        {
            if (enemyCombatants == null || enemyCombatants.Count == 0)
                return true;

            for (int i = 0; i < enemyCombatants.Count; i++)
            {
                CharacterInstance enemy = enemyCombatants[i];
                if (enemy != null && enemy.IsAlive)
                    return false;
            }

            return true;
        }

        /// <summary>Returns the combatant whose turn it currently is.</summary>
        public CharacterInstance GetCurrentCombatant()
        {
            if (!IsBattleActive || combatants.Count == 0)
                return null;

            if (currentTurnIndex < 0 || currentTurnIndex >= combatants.Count)
                currentTurnIndex = 0;

            return combatants[currentTurnIndex];
        }

        /// <summary>Alias for GetCurrentCombatant (backward-compat for CombatInputHandler).</summary>
        public CharacterInstance GetCurrentCharacter() => GetCurrentCombatant();

        /// <summary>Returns true if the given instance belongs to the player team.</summary>
        public bool IsPlayerUnit(CharacterInstance instance) => playerCombatants.Contains(instance);

        /// <summary>Returns true if the given instance belongs to the enemy team.</summary>
        public bool IsEnemyUnit(CharacterInstance instance) => enemyCombatants.Contains(instance);

        /// <summary>
        /// Returns how many turn advances remain before the given combatant acts.
        /// 0 means the combatant is the current active unit.
        /// Returns -1 if the combatant is not in the active turn order.
        /// </summary>
        public int GetTurnsUntilTurn(CharacterInstance instance)
        {
            if (!IsBattleActive || instance == null || combatants.Count == 0)
                return -1;

            int targetIndex = combatants.IndexOf(instance);
            if (targetIndex < 0)
                return -1;

            int turnsUntil = targetIndex - currentTurnIndex;
            if (turnsUntil < 0)
                turnsUntil += combatants.Count;

            return turnsUntil;
        }

        /// <summary>Returns all enemy combatants assigned to the active battle.</summary>
        public IReadOnlyList<CharacterInstance> GetEnemyCombatants() => enemyCombatants;

        /// <summary>Returns the current front-line player combatants for each active lane.</summary>
        public IReadOnlyList<CharacterInstance> GetPlayerCombatants() => playerCombatants;
    }
}
