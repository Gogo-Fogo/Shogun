// CombatStateMachine.cs
// Stack-based manager for combat states in turn-based combat.
// Use PushState() to add a new state, PopState() to remove the current state, and ReplaceState() to swap states.
// Call Update() each frame to update the current state.
// Designed for extensibility: supports states like PlayerTurnState, EnemyTurnState, MiniGameState, etc.

using System.Collections.Generic;
using UnityEngine;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Stack-based state machine for managing combat states.
    /// </summary>
    public class CombatStateMachine
    {
        private readonly Stack<CombatState> stateStack = new Stack<CombatState>();

        /// <summary>
        /// Push a new state onto the stack.
        /// </summary>
        public void PushState(CombatState newState)
        {
            if (stateStack.Count > 0)
                stateStack.Peek().Exit();
            stateStack.Push(newState);
            newState.Enter();
        }

        /// <summary>
        /// Pop the current state from the stack.
        /// </summary>
        public void PopState()
        {
            if (stateStack.Count > 0)
            {
                stateStack.Pop().Exit();
                if (stateStack.Count > 0)
                    stateStack.Peek().Enter();
            }
        }

        /// <summary>
        /// Replace the current state with a new one.
        /// </summary>
        public void ReplaceState(CombatState newState)
        {
            if (stateStack.Count > 0)
                stateStack.Pop().Exit();
            stateStack.Push(newState);
            newState.Enter();
        }

        /// <summary>
        /// Update the current state.
        /// </summary>
        public void Update()
        {
            if (stateStack.Count > 0)
                stateStack.Peek().Update();
        }
    }
} 