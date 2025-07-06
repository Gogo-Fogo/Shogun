// CombatInputHandler.cs
// MonoBehaviour for handling combat-specific input (movement, attack, skill use, etc.).
// Integrates with GestureRecognizer and InputManager to process player input during combat.
// Designed for extensibility and easy integration with the combat system.

using UnityEngine;
using Shogun.Core.Architecture;
using System.Collections.Generic;

namespace Shogun.Features.Combat
{
    public class CombatInputHandler : MonoBehaviour
    {
        private GestureRecognizer gestureRecognizer;
        private InputManager inputManager;
        public List<GameObject> activeCharacters = new List<GameObject>();
        public int selectedCharacterIndex = 0;
        public BattleManager battleManager;
        public TurnManager turnManager;

        void Awake()
        {
            gestureRecognizer = GetComponent<GestureRecognizer>();
            inputManager = GetComponent<InputManager>();
        }

        void OnEnable()
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.OnTap.AddListener(HandleTap);
                gestureRecognizer.OnSwipe.AddListener(HandleSwipe);
                gestureRecognizer.OnHold.AddListener(HandleHold);
            }
            if (inputManager != null)
            {
                inputManager.OnCombatAction.AddListener(HandleCombatAction);
            }
        }

        void OnDisable()
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.OnTap.RemoveListener(HandleTap);
                gestureRecognizer.OnSwipe.RemoveListener(HandleSwipe);
                gestureRecognizer.OnHold.RemoveListener(HandleHold);
            }
            if (inputManager != null)
            {
                inputManager.OnCombatAction.RemoveListener(HandleCombatAction);
            }
        }

        private void HandleTap(Vector2 pos) { /* Handle tap input for combat */ }
        private void HandleSwipe(Vector2 start, Vector2 end) { /* Handle swipe input for combat */ }
        private void HandleHold(Vector2 pos, float duration) { /* Handle hold input for combat */ }
        private void HandleCombatAction() { /* Handle combat action input */ }

        public void DebugTap(UnityEngine.Vector2 position)
        {
            if (battleManager == null || turnManager == null)
            {
                UnityEngine.Debug.LogWarning("BattleManager or TurnManager not assigned!");
                return;
            }
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (battleManager.activeCharacters.Contains(currentCharacter))
            {
                currentCharacter.MoveTo(position); // Implement this in CharacterInstance
            }
            else
            {
                UnityEngine.Debug.Log("It's not your turn!");
            }
        }
    }
} 