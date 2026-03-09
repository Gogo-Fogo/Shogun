// CombatInputHandler.cs
// MonoBehaviour for handling combat-specific input (movement, attack, skill use, etc.).
// Integrates with GestureRecognizer and InputManager to process player input during combat.
// Designed for extensibility and easy integration with the combat system.

using UnityEngine;
using Shogun.Core.Architecture;
using Shogun.Features.Characters;
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
            gestureRecognizer = ResolveDependency<GestureRecognizer>();
            inputManager = ResolveDependency<InputManager>();
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

        private void HandleTap(Vector2 screenPos)
        {
            if (battleManager == null || turnManager == null) return;
            if (!turnManager.IsBattleActive) return;

            var current = turnManager.GetCurrentCombatant();
            if (current == null || !current.CanAttack) return;
            if (!turnManager.IsPlayerUnit(current)) return; // not the player's turn

            // Convert screen position to world position
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 0f));

            // Find a character near the tap (0.5 world-unit radius for mobile tolerance)
            var hit = Physics2D.OverlapCircle(worldPos, 0.5f);
            if (hit == null) return;

            var target = hit.GetComponent<CharacterInstance>();
            if (target == null || !target.IsAlive) return;
            if (turnManager.IsPlayerUnit(target)) return; // can't attack own team

            // Trigger attack animation
            current.PerformBasicAttack();

            // Calculate and apply damage
            float damage = current.CalculateDamageAgainst(target);
            target.TakeDamage(damage);

            Debug.Log($"[Battle] {current.Definition?.CharacterName} → {target.Definition?.CharacterName}: " +
                      $"{damage:F1} dmg  (HP left: {target.CurrentHealth:F1}/{target.MaxHealth:F1})");

            turnManager.EndTurn();
        }

        private void HandleSwipe(Vector2 start, Vector2 end) { /* future: swap / special */ }
        private void HandleHold(Vector2 pos, float duration) { /* future: ability menu */ }
        private void HandleCombatAction() { /* future: mapped action button */ }

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

        private T ResolveDependency<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            component = GetComponentInChildren<T>(true);
            if (component != null)
            {
                return component;
            }

            return GetComponentInParent<T>();
        }
    }
} 
