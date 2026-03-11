// EnemyAI.cs
// Prototype enemy AI: when an enemy's turn starts, move into melee range,
// attack the closest alive player, and hold a resolved spaced position.
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
        [SerializeField] private float attackDelay = 0.8f;
        [SerializeField] private float attackHitPause = 0.18f;
        [SerializeField] private float attackRecoverDelay = 0.22f;

        [Header("Movement")]
        [Tooltip("World units per second the enemy runs toward its target. Keeps the dash visually readable at any distance.")]
        [SerializeField] private float movementSpeed = 8f;
        [Tooltip("Minimum travel time so point-blank attacks still animate.")]
        [SerializeField] private float minTravelTime = 0.08f;
        [Tooltip("Maximum travel time cap so cross-field charges aren't too slow.")]
        [SerializeField] private float maxTravelTime = 0.65f;

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
            if (turnManager.IsPlayerUnit(current)) return;

            UpdateEnemyFacing();
            StartCoroutine(EnemyTurnRoutine(current));
        }

        private IEnumerator EnemyTurnRoutine(CharacterInstance attacker)
        {
            yield return new WaitForSeconds(attackDelay);

            if (!turnManager.IsBattleActive || attacker == null || !attacker.IsAlive)
                yield break;

            CharacterInstance target = FindClosestAlivePlayer(attacker, battleManager.activeCharacters);
            if (target == null)
            {
                turnManager.EndTurn();
                yield break;
            }

            Animator attackerAnimator = attacker.GetComponentInChildren<Animator>();
            Vector3 strikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(attacker, target, GetActiveCombatantsExcept(attacker, target));

            float travelDist = Vector2.Distance(
                (Vector2)CombatMovementUtility.GetWorldPosition(attacker.transform),
                (Vector2)strikeWorldPos);
            float dynamicTravelTime = Mathf.Clamp(travelDist / Mathf.Max(0.1f, movementSpeed), minTravelTime, maxTravelTime);

            CombatMovementUtility.FaceCharacterTowards(attacker, target);
            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", true);

            yield return CombatMovementUtility.MoveCharacterToWorldPosition(attacker.transform, strikeWorldPos, dynamicTravelTime);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", false);

            if (!CombatMovementUtility.IsTargetWithinAttackRange(attacker, target))
            {
                Vector3 correctedStrikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(attacker, target);
                float correctionDistance = Vector2.Distance(
                    (Vector2)CombatMovementUtility.GetWorldPosition(attacker.transform),
                    (Vector2)correctedStrikeWorldPos);

                if (correctionDistance > 0.05f)
                {
                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", true);

                    float correctionTravelTime = Mathf.Clamp(correctionDistance / Mathf.Max(0.1f, movementSpeed), minTravelTime, maxTravelTime);
                    yield return CombatMovementUtility.MoveCharacterToWorldPosition(attacker.transform, correctedStrikeWorldPos, correctionTravelTime);

                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", false);
                }
            }

            bool withinRange = CombatMovementUtility.IsTargetWithinAttackRange(attacker, target);
            float rangeThreshold = CombatMovementUtility.GetAttackRangeThreshold(attacker, target);
            float distToTarget = Vector2.Distance(
                (Vector2)CombatMovementUtility.GetColliderWorldCenter(attacker),
                (Vector2)CombatMovementUtility.GetColliderWorldCenter(target));

            CombatHitResult hitResult = default;
            if (withinRange)
            {
                attacker.PerformBasicAttack();
                yield return new WaitForSeconds(attackHitPause);

                CombatCriticalSupportUtility.TryResolveBasicHit(battleManager, attacker, target, null, out hitResult);
            }
            else
            {
                Debug.LogWarning($"[EnemyAI] {attacker.Definition?.CharacterName} could not reach {target.Definition?.CharacterName} " +
                                 $"({distToTarget:F2} > {rangeThreshold:F2}) - attack skipped.");
            }

            yield return new WaitForSeconds(attackRecoverDelay);

            if (attackerAnimator != null)
                attackerAnimator.SetBool("isRunning", false);

            if (withinRange)
                Debug.Log($"[EnemyAI] {attacker.Definition?.CharacterName} -> {target.Definition?.CharacterName}: " +
                          $"{hitResult.Damage:F1} dmg{(hitResult.WasCritical ? " CRIT" : string.Empty)}  (HP left: {target.CurrentHealth:F1}/{target.MaxHealth:F1})");

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

        private List<CharacterInstance> GetActiveCombatantsExcept(CharacterInstance attacker, CharacterInstance target)
        {
            List<CharacterInstance> blockers = new List<CharacterInstance>();
            if (battleManager == null)
                return blockers;

            List<CharacterInstance> combatants = battleManager.GetAllActiveCombatants();
            for (int i = 0; i < combatants.Count; i++)
            {
                CharacterInstance combatant = combatants[i];
                if (combatant == null || combatant == attacker || combatant == target)
                    continue;

                blockers.Add(combatant);
            }

            return blockers;
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

