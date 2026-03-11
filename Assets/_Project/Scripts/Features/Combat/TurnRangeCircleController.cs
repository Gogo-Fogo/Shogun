// TurnRangeCircleController.cs
// Keeps the active player unit's attack range visible on the battlefield.

using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    public class TurnRangeCircleController : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;

        private CharacterInstance activePlayerCharacter;

        private void Awake()
        {
            ResolveDependencies();
        }

        private void OnEnable()
        {
            ResolveDependencies();
            Subscribe();
            RefreshActiveRangeCircle();
        }

        private void OnDisable()
        {
            Unsubscribe();
            HideActiveRangeCircle();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            HideActiveRangeCircle();
        }

        private void LateUpdate()
        {
            RefreshActiveRangeCircle();
        }

        private void ResolveDependencies()
        {
            if (turnManager == null)
                turnManager = FindFirstObjectByType<TurnManager>();
        }

        private void Subscribe()
        {
            if (turnManager == null)
                return;

            turnManager.OnTurnStarted -= HandleTurnStateChanged;
            turnManager.OnTurnEnded -= HandleTurnStateChanged;
            turnManager.OnTurnOrderChanged -= HandleTurnOrderChanged;
            turnManager.OnBattleEnded -= HandleBattleEnded;

            turnManager.OnTurnStarted += HandleTurnStateChanged;
            turnManager.OnTurnEnded += HandleTurnStateChanged;
            turnManager.OnTurnOrderChanged += HandleTurnOrderChanged;
            turnManager.OnBattleEnded += HandleBattleEnded;
        }

        private void Unsubscribe()
        {
            if (turnManager == null)
                return;

            turnManager.OnTurnStarted -= HandleTurnStateChanged;
            turnManager.OnTurnEnded -= HandleTurnStateChanged;
            turnManager.OnTurnOrderChanged -= HandleTurnOrderChanged;
            turnManager.OnBattleEnded -= HandleBattleEnded;
        }

        private void HandleTurnStateChanged(CharacterInstance _)
        {
            RefreshActiveRangeCircle();
        }

        private void HandleTurnOrderChanged()
        {
            RefreshActiveRangeCircle();
        }

        private void HandleBattleEnded(BattleResult _)
        {
            RefreshActiveRangeCircle();
        }

        private void RefreshActiveRangeCircle()
        {
            ResolveDependencies();

            CharacterInstance desiredCharacter = null;
            if (turnManager != null && turnManager.IsBattleActive)
            {
                CharacterInstance current = turnManager.GetCurrentCombatant();
                if (current != null && current.IsAlive && turnManager.IsPlayerUnit(current))
                    desiredCharacter = current;
            }

            if (activePlayerCharacter != null && activePlayerCharacter != desiredCharacter)
                HideRangeCircle(activePlayerCharacter);

            activePlayerCharacter = desiredCharacter;
            if (activePlayerCharacter == null)
                return;

            RangeCircleDisplay display = activePlayerCharacter.GetComponent<RangeCircleDisplay>();
            if (display == null)
                display = activePlayerCharacter.gameObject.AddComponent<RangeCircleDisplay>();

            display.Show(activePlayerCharacter.GetAttackRangeRadius(), ResolveRangeColor(activePlayerCharacter));
        }

        private void HideActiveRangeCircle()
        {
            if (activePlayerCharacter == null)
                return;

            HideRangeCircle(activePlayerCharacter);
            activePlayerCharacter = null;
        }

        private static void HideRangeCircle(CharacterInstance character)
        {
            if (character == null)
                return;

            RangeCircleDisplay display = character.GetComponent<RangeCircleDisplay>();
            if (display != null)
                display.Hide();
        }

        private static Color ResolveRangeColor(CharacterInstance character)
        {
            if (character != null && character.Definition != null)
            {
                Color color = character.Definition.PaletteAccentColor;
                color.a = 0.85f;
                return color;
            }

            return new Color(0.2f, 0.9f, 1f, 0.85f);
        }
    }
}