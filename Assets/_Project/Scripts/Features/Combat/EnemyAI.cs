// EnemyAI.cs
// Prototype enemy AI: when an enemy's turn starts, wait briefly then auto-attack
// a random alive player character.

using System.Collections;
using UnityEngine;
using Shogun.Features.Characters;

namespace Shogun.Features.Combat
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("Dependencies (auto-resolved if left empty)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private BattleManager battleManager;

        [Header("AI Timing")]
        [SerializeField] private float attackDelay = 0.8f; // seconds before auto-attack fires

        void Start()
        {
            if (turnManager == null)
                turnManager = FindObjectOfType<TurnManager>();
            if (battleManager == null)
                battleManager = FindObjectOfType<BattleManager>();

            if (turnManager != null)
                turnManager.OnTurnStarted += OnTurnStarted;
        }

        void OnDestroy()
        {
            if (turnManager != null)
                turnManager.OnTurnStarted -= OnTurnStarted;
        }

        private void OnTurnStarted(CharacterInstance current)
        {
            if (turnManager == null || battleManager == null) return;
            if (turnManager.IsPlayerUnit(current)) return; // player turn — do nothing

            StartCoroutine(EnemyTurnRoutine(current));
        }

        private IEnumerator EnemyTurnRoutine(CharacterInstance attacker)
        {
            yield return new WaitForSeconds(attackDelay);

            if (!turnManager.IsBattleActive) yield break;
            if (!attacker.IsAlive) yield break;

            // Pick a random alive player to attack
            var players = battleManager.activeCharacters;
            CharacterInstance target = null;
            int attempts = players.Count;
            while (attempts-- > 0)
            {
                var candidate = players[Random.Range(0, players.Count)];
                if (candidate.IsAlive)
                {
                    target = candidate;
                    break;
                }
            }

            if (target == null)
            {
                // No alive players found — TurnManager win/loss check will handle it
                turnManager.EndTurn();
                yield break;
            }

            attacker.PerformBasicAttack();

            float damage = attacker.CalculateDamageAgainst(target);
            target.TakeDamage(damage);

            Debug.Log($"[EnemyAI] {attacker.Definition?.CharacterName} → {target.Definition?.CharacterName}: " +
                      $"{damage:F1} dmg  (HP left: {target.CurrentHealth:F1}/{target.MaxHealth:F1})");

            turnManager.EndTurn();
        }
    }
}
