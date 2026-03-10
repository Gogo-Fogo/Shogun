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

        public bool IsBattleActive { get; private set; } = false;

        /// <summary>
        /// Initialize the turn manager with separate player and enemy lists.
        /// Turn order is players first, then enemies (round-robin).
        /// </summary>
        public void Initialize(List<CharacterInstance> players, List<CharacterInstance> enemies)
        {
            // Clean up subscriptions from any previous battle
            foreach (var kvp in deathCallbacks)
                kvp.Key.OnDeath -= kvp.Value;
            deathCallbacks.Clear();

            combatants.Clear();
            playerCombatants = new List<CharacterInstance>(players);
            enemyCombatants = new List<CharacterInstance>(enemies);

            combatants.AddRange(players);
            combatants.AddRange(enemies);

            foreach (var c in combatants)
                Subscribe(c);

            currentTurnIndex = 0;
            IsBattleActive = combatants.Count > 0;
        }

        private void Subscribe(CharacterInstance c)
        {
            Action callback = () => OnCombatantDied(c);
            deathCallbacks[c] = callback;
            c.OnDeath += callback;
        }

        private void Unsubscribe(CharacterInstance c)
        {
            if (deathCallbacks.TryGetValue(c, out var callback))
            {
                c.OnDeath -= callback;
                deathCallbacks.Remove(c);
            }
        }

        public void StartBattle()
        {
            if (!IsBattleActive) return;
            StartTurn();
        }

        private void StartTurn()
        {
            if (!IsBattleActive || combatants.Count == 0) return;

            var current = combatants[currentTurnIndex];

            // Reset per-turn flags and process status effects (may trigger OnDeath)
            current.StartNewTurn();

            // Status effects may have ended the battle or killed this character
            if (!IsBattleActive) return;
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
            if (!IsBattleActive) return;
            var current = combatants[currentTurnIndex];
            OnTurnEnded?.Invoke(current);
            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            if (!IsBattleActive || combatants.Count == 0) return;
            currentTurnIndex = (currentTurnIndex + 1) % combatants.Count;
            StartTurn();
        }

        private void OnCombatantDied(CharacterInstance died)
        {
            int deadIndex = combatants.IndexOf(died);
            if (deadIndex < 0) return;

            Unsubscribe(died);
            combatants.RemoveAt(deadIndex);

            // Keep currentTurnIndex valid after removal
            if (deadIndex < currentTurnIndex)
                currentTurnIndex--;
            else if (deadIndex == currentTurnIndex && combatants.Count > 0)
                currentTurnIndex = currentTurnIndex % combatants.Count;

            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            if (!IsBattleActive) return;

            bool allPlayersDefeated = playerCombatants.TrueForAll(c => !c.IsAlive);
            bool allEnemiesDefeated = enemyCombatants.TrueForAll(c => !c.IsAlive);

            if (!allPlayersDefeated && !allEnemiesDefeated) return;

            IsBattleActive = false;
            BattleResult result = allEnemiesDefeated ? BattleResult.Win : BattleResult.Loss;
            OnBattleEnded?.Invoke(result);
        }

        /// <summary>Returns the combatant whose turn it currently is.</summary>
        public CharacterInstance GetCurrentCombatant()
        {
            if (!IsBattleActive || combatants.Count == 0) return null;
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

        /// <summary>Returns all enemy combatants (including dead — check IsAlive at call site).</summary>
        public IReadOnlyList<CharacterInstance> GetEnemyCombatants() => enemyCombatants;

        /// <summary>Returns all player combatants (including dead — check IsAlive at call site).</summary>
        public IReadOnlyList<CharacterInstance> GetPlayerCombatants() => playerCombatants;
    }
}

