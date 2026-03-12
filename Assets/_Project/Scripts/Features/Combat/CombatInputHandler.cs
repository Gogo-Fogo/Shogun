// CombatInputHandler.cs
// MonoBehaviour for handling combat-specific input (movement, attack, skill use, etc.).
// Integrates with GestureRecognizer and InputManager to process player input during combat.
// Designed for extensibility and easy integration with the combat system.

using UnityEngine;
using Shogun.Core.Architecture;
using Shogun.Features.Characters;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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
            if (!TryNormalizeScreenPosition(screenPos, out Vector2 normalizedScreenPos)) return;
            if (IsTapBlockedByCombatUi(normalizedScreenPos)) return;

            CharacterInstance current = turnManager.GetCurrentCombatant();
            if (current == null || !current.CanAttack) return;
            if (!turnManager.IsPlayerUnit(current)) return; // not the player's turn

            Camera cameraRef = Camera.main;
            if (cameraRef == null) return;

            Vector2 worldPos = cameraRef.ScreenToWorldPoint(
                new Vector3(normalizedScreenPos.x, normalizedScreenPos.y, 0f));

            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.5f);
            if (hit == null) return;

            CharacterInstance target = hit.GetComponent<CharacterInstance>();
            if (target == null || !target.IsAlive) return;
            if (turnManager.IsPlayerUnit(target)) return; // can't attack own team
            if (!CombatMovementUtility.IsTargetWithinAttackRange(current, target)) return;

            CombatMovementUtility.FaceCharacterTowards(current, target);
            current.PerformBasicAttack();

            if (!CombatCriticalSupportUtility.TryResolveBasicHit(battleManager, current, target, null, out CombatHitResult hitResult))
                return;

            string criticalLabel = hitResult.WasCritical ? " CRIT" : string.Empty;
            Debug.Log($"[Battle] {current.Definition?.CharacterName} → {target.Definition?.CharacterName}: " +
                      $"{hitResult.Damage:F1} dmg{criticalLabel}  (HP left: {target.CurrentHealth:F1}/{target.MaxHealth:F1})");

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

            if (!TryNormalizeScreenPosition(position, out Vector2 normalizedScreenPos))
                return;

            var currentCharacter = turnManager.GetCurrentCharacter();
            if (battleManager.activeCharacters.Contains(currentCharacter))
            {
                currentCharacter.MoveTo(normalizedScreenPos);
            }
            else
            {
                UnityEngine.Debug.Log("It's not your turn!");
            }
        }

        private static bool IsTapBlockedByCombatUi(Vector2 screenPos)
        {
            if (EventSystem.current == null)
                return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == null)
                    continue;

                Transform hitTransform = result.gameObject.transform;
                if (HasAncestorNamed(hitTransform, "DragInputPanel"))
                    continue;

                if (HasAncestorNamed(hitTransform, "BattleHudRoot")
                    || HasAncestorNamed(hitTransform, "BattleResultPanel"))
                    return true;
            }

            return false;
        }

        private static bool HasAncestorNamed(Transform origin, string targetName)
        {
            Transform current = origin;
            while (current != null)
            {
                if (current.name == targetName)
                    return true;

                current = current.parent;
            }

            return false;
        }

        private static bool TryNormalizeScreenPosition(Vector2 rawScreenPos, out Vector2 normalizedScreenPos)
        {
            normalizedScreenPos = rawScreenPos;
            if (!IsFinite(rawScreenPos))
                return false;

            if (rawScreenPos.sqrMagnitude <= 0.0001f)
            {
                // Legacy Input.mousePosition is unavailable with the Input System package.
                // A zero-vector tap position is invalid — reject it cleanly.
                return false;
            }

            if (rawScreenPos.x < 0f || rawScreenPos.y < 0f || rawScreenPos.x > Screen.width || rawScreenPos.y > Screen.height)
                return false;

            normalizedScreenPos = new Vector2(
                Mathf.Clamp(rawScreenPos.x, 0f, Screen.width),
                Mathf.Clamp(rawScreenPos.y, 0f, Screen.height));
            return true;
        }

        private static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x)
                   && !float.IsNaN(value.y)
                   && !float.IsInfinity(value.x)
                   && !float.IsInfinity(value.y);
        }

        private T ResolveDependency<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component != null)
                return component;

            return FindFirstObjectByType<T>();
        }
    }
}

