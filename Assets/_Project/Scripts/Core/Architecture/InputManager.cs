// InputManager.cs
// MonoBehaviour for integrating Unity's new Input System with the game.
// Provides events for action maps and input actions relevant to combat and UI.
// Designed to be extended for more complex input needs and custom bindings.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Shogun.Core.Architecture
{
    public class InputManager : MonoBehaviour
    {
        public UnityEvent OnCombatAction;
        public UnityEvent OnUIAction;

        private void OnEnable()
        {
            // Example: subscribe to input actions (expand for your InputActions asset)
            // InputSystem.onActionChange += ...
        }

        private void OnDisable()
        {
            // Unsubscribe from input actions
        }

        // Example method to invoke combat action
        public void TriggerCombatAction() => OnCombatAction?.Invoke();
        public void TriggerUIAction() => OnUIAction?.Invoke();
    }
} 