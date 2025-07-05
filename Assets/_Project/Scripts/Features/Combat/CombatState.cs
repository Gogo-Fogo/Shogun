// CombatState.cs
// Abstract base class for all combat states in the combat state machine.
// Inherit from this class to implement specific states (e.g., PlayerTurnState, EnemyTurnState, MiniGameState).
// Override Enter(), Exit(), and Update() to define state-specific behavior.

using UnityEngine;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Abstract base class for all combat states.
    /// </summary>
    public abstract class CombatState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public virtual void Exit() { }

        /// <summary>
        /// Called every frame while the state is active.
        /// </summary>
        public virtual void Update() { }
    }
} 