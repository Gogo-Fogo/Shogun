// TurnManager.cs
// Manages the order of turns for all combatants in a turn-based battle.
// Uses a simple round-robin system by default, but is designed for easy extension (e.g., initiative/speed-based order).
// Integrates with CombatStateMachine and provides events for turn start/end.
// Notifies listeners when the turn changes and when the battle ends.

using System;
using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    public class TurnManager : MonoBehaviour
    {
        private readonly List<CharacterInstance> combatants = new List<CharacterInstance>();
        private int currentTurnIndex = 0;
        public int CurrentTurnIndex
        {
            get => currentTurnIndex;
            set => currentTurnIndex = value;
        }
        public event Action<CharacterInstance> OnTurnStarted;
        public event Action<CharacterInstance> OnTurnEnded;
        public event Action OnBattleEnded;
        public bool IsBattleActive { get; private set; } = false;
        public List<CharacterInstance> turnOrder = new List<CharacterInstance>();

        public void Initialize(List<CharacterInstance> participants)
        {
            combatants.Clear();
            combatants.AddRange(participants);
            currentTurnIndex = 0;
            IsBattleActive = combatants.Count > 0;
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
            if (!IsBattleActive) return;
            currentTurnIndex = (currentTurnIndex + 1) % combatants.Count;
            // Optionally: Remove dead combatants here
            if (combatants.Count == 0)
            {
                IsBattleActive = false;
                OnBattleEnded?.Invoke();
                return;
            }
            StartTurn();
        }

        public CharacterInstance GetCurrentCombatant()
        {
            if (!IsBattleActive || combatants.Count == 0) return null;
            return combatants[currentTurnIndex];
        }

        public CharacterInstance GetCurrentCharacter()
        {
            if (turnOrder.Count == 0) return null;
            return turnOrder[currentTurnIndex];
        }

        public void NextTurn()
        {
            if (turnOrder.Count == 0) return;
            currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
        }

        // For future extension: allow dynamic reordering, initiative, etc.
    }
} 