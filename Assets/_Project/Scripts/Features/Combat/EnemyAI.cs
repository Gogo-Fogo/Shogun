// EnemyAI.cs
// Prototype enemy AI: when an enemy's turn starts, wait briefly then auto-attack
// the closest alive player character.

using System.Collections;
using System.Collections.Generic;
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

        [Header("Facing")]
        [SerializeField] private bool faceClosestPlayerContinuously = true;
        [SerializeField] private float facingUpdateInterval = 0.15f;

        private float nextFacingUpdateTime;

        void Start()
        {
            if (turnManager == null)
                turnManager = FindFirstObjectByType<TurnManager>();
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (turnManager != null)
                turnManager.OnTurnStarted += OnTurnStarted;

            UpdateEnemyFacing();
        }

        void Update()
        {
            if (!faceClosestPlayerContinuously) return;
            if (battleManager == null) return;
            if (Time.time < nextFacingUpdateTime) return;

            nextFacingUpdateTime = Time.time + Mathf.Max(0.02f, facingUpdateInterval);
            UpdateEnemyFacing();
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

            UpdateEnemyFacing();
            StartCoroutine(EnemyTurnRoutine(current));
        }

        private IEnumerator EnemyTurnRoutine(CharacterInstance attacker)
        {
            yield return new WaitForSeconds(attackDelay);

            if (!turnManager.IsBattleActive) yield break;
            if (!attacker.IsAlive) yield break;

            // Pick the closest alive player as target.
            CharacterInstance target = FindClosestAlivePlayer(attacker, battleManager.activeCharacters);
            if (target == null)
            {
                // No alive players found — TurnManager win/loss check will handle it
                turnManager.EndTurn();
                yield break;
            }

            FaceEnemyTowards(attacker, target);
            attacker.PerformBasicAttack();

            float damage = attacker.CalculateDamageAgainst(target);
            target.TakeDamage(damage);

            Debug.Log($"[EnemyAI] {attacker.Definition?.CharacterName} → {target.Definition?.CharacterName}: " +
                      $"{damage:F1} dmg  (HP left: {target.CurrentHealth:F1}/{target.MaxHealth:F1})");

            turnManager.EndTurn();
        }

        private void UpdateEnemyFacing()
        {
            if (battleManager == null) return;

            List<CharacterInstance> enemies = battleManager.activeEnemyCharacters;
            List<CharacterInstance> players = battleManager.activeCharacters;
            if (enemies == null || players == null) return;

            for (int i = 0; i < enemies.Count; i++)
            {
                CharacterInstance enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;

                CharacterInstance nearest = FindClosestAlivePlayer(enemy, players);
                if (nearest == null) continue;

                FaceEnemyTowards(enemy, nearest);
            }
        }

        private static CharacterInstance FindClosestAlivePlayer(CharacterInstance source, List<CharacterInstance> players)
        {
            if (source == null || players == null) return null;

            CharacterInstance closest = null;
            float bestSq = float.MaxValue;

            for (int i = 0; i < players.Count; i++)
            {
                CharacterInstance candidate = players[i];
                if (candidate == null || !candidate.IsAlive) continue;

                float sq = (candidate.transform.position - source.transform.position).sqrMagnitude;
                if (sq < bestSq)
                {
                    bestSq = sq;
                    closest = candidate;
                }
            }

            return closest;
        }

        private static void FaceEnemyTowards(CharacterInstance enemy, CharacterInstance target)
        {
            if (enemy == null || target == null) return;

            float direction = target.transform.position.x < enemy.transform.position.x ? -1f : 1f;
            if (enemy.Definition != null && enemy.Definition.InvertFacingX)
                direction *= -1f;

            Vector3 scale = enemy.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            enemy.transform.localScale = scale;
        }
    }
}
