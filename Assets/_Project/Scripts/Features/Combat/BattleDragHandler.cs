using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Shogun.Features.Characters;
using System.Collections;
using System.Collections.Generic;

namespace Shogun.Features.Combat
{
    [RequireComponent(typeof(Image))]
    public class BattleDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("References")]
        public TurnManager turnManager;
        [SerializeField] private BattleManager battleManager;

        [Header("Settings")]
        public float dragThreshold = 10f;
        public float dragSmoothTime = 0.08f;
        public float gridSize = 1f;
        public bool snapToGrid = true;
        public float tapMoveSpeed = 50f;
        [Range(0.1f, 1f)] public float dragOpacity = 0.6f;
        public float dragStopIdleBuffer = 0.08f;

        [Header("Attack Sequence")]
        public float chainedAttackTravelTime = 0.12f;
        public float chainedAttackHitPause = 0.08f;
        public float chainedAttackRecoverTime = 0.10f;
        public float chainedAttackReturnTime = 0.16f;

        private const float tapTimeThreshold = 0.2f;
        private const float tapMoveThreshold = 20f;
        private const float dragIdleDistanceThresholdSqr = 0.0004f;
        private const float movementCommitDistanceThreshold = 0.05f;

        private Camera mainCamera;
        private Vector2 pointerDownPos;
        private float pointerDownTime;
        private bool hasValidPointerDown = false;
        private bool hasValidPointerSample = false;
        private Vector2 lastValidScreenPos = Vector2.zero;
        private float lastDragMotionTime = float.NegativeInfinity;

        private bool isDragging = false;
        private Vector3 dragStartWorldPos = Vector3.zero;
        private Vector3 dragTargetWorldPos;
        private Vector3 dragVelocity = Vector3.zero;
        private CharacterInstance draggingCharacter = null;
        private Animator characterAnimator = null;
        private SpriteRenderer draggingSpriteRenderer = null;
        private Color draggingOriginalColor = Color.white;
        private Coroutine tapMoveCoroutine = null;
        private RangeCircleDisplay dragRangeCircle = null;
        private DragMultiTargetIndicator dragMultiTargetIndicator = null;
        private Coroutine attackResolutionCoroutine = null;
        private bool isResolvingAttackSequence = false;

        private static readonly Color EnemyRangeColor  = new Color(1f, 0.25f, 0.25f, 0.70f);

        // Tracks which enemies currently have their red threat circle visible during a drag
        private readonly HashSet<CharacterInstance> enemiesShowingRange          = new HashSet<CharacterInstance>();
        // Tracks which enemies currently have an attack-ready / combo indicator visible
        private readonly HashSet<CharacterInstance> enemiesShowingAttackIndicator = new HashSet<CharacterInstance>();
        private readonly HashSet<CharacterInstance> alliesShowingCriticalRateBoostPreview = new HashSet<CharacterInstance>();
        private readonly List<CharacterInstance> staleCriticalRateBoostPreviews = new List<CharacterInstance>();

        void Awake()
        {
            mainCamera = Camera.main;
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();
        }

        void OnDisable()
        {
            HideAllEnemyRangeCircles();
            HideAllCriticalRateBoostPreviews();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isResolvingAttackSequence)
                return;

            if (!TryNormalizeScreenPosition(eventData.position, out Vector2 normalizedPos))
            {
                hasValidPointerDown = false;
                return;
            }

            pointerDownPos = normalizedPos;
            pointerDownTime = Time.unscaledTime;
            hasValidPointerDown = true;
            isDragging = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isResolvingAttackSequence)
                return;

            if (!hasValidPointerDown)
            {
                ClearDragState();
                return;
            }

            Vector2 releasePos = pointerDownPos;
            if (!TryNormalizeScreenPosition(eventData.position, out releasePos))
            {
                releasePos = hasValidPointerSample ? lastValidScreenPos : pointerDownPos;
            }

            float heldTime = Time.unscaledTime - pointerDownTime;
            float movedDist = Vector2.Distance(releasePos, pointerDownPos);

            if (!isDragging && heldTime < tapTimeThreshold && movedDist < tapMoveThreshold)
            {
                if (!IsTapOnEnemy(releasePos))
                    StartTapMove(releasePos);

                ClearDragState();
                return;
            }

            if (isDragging && draggingCharacter != null)
            {
                CharacterInstance releasedCharacter = draggingCharacter;
                Animator releasedAnimator = characterAnimator;
                Vector3 releaseWorldPos = GetCharacterPosition(releasedCharacter.transform);
                CharacterInstance releaseTarget = null;

                if (TryGetPointerWorld(releasePos, releasedCharacter.transform, out Vector3 pointerWorld))
                {
                    releaseWorldPos = snapToGrid ? SnapToGrid(pointerWorld) : pointerWorld;
                    SetCharacterPosition(releasedCharacter.transform, releaseWorldPos);
                }

                if (TryGetEnemyAtScreenPosition(releasePos, out CharacterInstance hoveredEnemy))
                    releaseTarget = hoveredEnemy;

                if (releasedAnimator != null)
                    releasedAnimator.SetBool("isRunning", false);

                RestoreDragOpacity();
                List<CharacterInstance> releaseTargets = GetReleaseAttackTargets(releasedCharacter, releaseTarget);
                ClearDragState();

                if (releaseTargets != null && releaseTargets.Count > 0)
                {
                    if (attackResolutionCoroutine != null)
                        StopCoroutine(attackResolutionCoroutine);

                    attackResolutionCoroutine = StartCoroutine(
                        ResolveReleaseAttackSequence(releasedCharacter, releaseWorldPos, releaseTargets));
                    return;
                }

                if (DidCommitMovement(dragStartWorldPos, releaseWorldPos))
                    ConsumeTurnAfterMovement(releasedCharacter);

                return;
            }

            ClearDragState();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isResolvingAttackSequence)
                return;

            if (turnManager == null)
                return;

            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null)
                return;

            if (!hasValidPointerDown)
                return;

            Transform charTransform = currentCharacter.transform;
            if (!TryGetPointerWorld(eventData.position, charTransform, out Vector3 pointerWorldPos))
                return;

            if (!isDragging)
            {
                if (tapMoveCoroutine != null)
                {
                    StopCoroutine(tapMoveCoroutine);
                    tapMoveCoroutine = null;
                }

                isDragging = true;
                draggingCharacter = currentCharacter;
                characterAnimator = currentCharacter.GetComponentInChildren<Animator>();
                draggingSpriteRenderer = currentCharacter.GetComponentInChildren<SpriteRenderer>();

                dragStartWorldPos = GetCharacterPosition(charTransform);
                SetCharacterPosition(charTransform, pointerWorldPos);
                dragTargetWorldPos = pointerWorldPos;
                lastDragMotionTime = Time.unscaledTime;

                if (characterAnimator != null)
                    characterAnimator.SetBool("isRunning", true);

                ApplyDragOpacity();
                dragVelocity = Vector3.zero;

                if (turnManager.IsPlayerUnit(draggingCharacter))
                {
                    dragRangeCircle = draggingCharacter.GetComponent<RangeCircleDisplay>();
                    if (dragRangeCircle == null)
                        dragRangeCircle = draggingCharacter.gameObject.AddComponent<RangeCircleDisplay>();

                    dragRangeCircle.Show(draggingCharacter.GetAttackRangeRadius(), GetPlayerRangeColor());

                    dragMultiTargetIndicator = draggingCharacter.GetComponent<DragMultiTargetIndicator>();
                    if (dragMultiTargetIndicator == null)
                        dragMultiTargetIndicator = draggingCharacter.gameObject.AddComponent<DragMultiTargetIndicator>();

                    dragMultiTargetIndicator.Hide();
                }
            }

            if (isDragging && draggingCharacter != null)
            {
                dragTargetWorldPos = pointerWorldPos;
            }
        }

        void Update()
        {
            if (isResolvingAttackSequence)
                return;

            if (!isDragging || draggingCharacter == null)
                return;

            Transform charTransform = draggingCharacter.transform;
            Vector3 before = GetCharacterPosition(charTransform);
            Vector3 newPos = Vector3.SmoothDamp(before, dragTargetWorldPos, ref dragVelocity, dragSmoothTime);
            SetCharacterPosition(charTransform, newPos);
            UpdateDragRunningState(newPos);

            UpdateEnemyRangeCircles();
        }


        private void UpdateDragRunningState(Vector3 currentWorldPos)
        {
            if (characterAnimator == null)
                return;

            float remainingDistanceSqr = (dragTargetWorldPos - currentWorldPos).sqrMagnitude;
            if (remainingDistanceSqr > dragIdleDistanceThresholdSqr)
                lastDragMotionTime = Time.unscaledTime;

            bool shouldRun = remainingDistanceSqr > dragIdleDistanceThresholdSqr
                             || Time.unscaledTime - lastDragMotionTime <= dragStopIdleBuffer;
            if (characterAnimator.GetBool("isRunning") != shouldRun)
                characterAnimator.SetBool("isRunning", shouldRun);
        }

        private void UpdateEnemyRangeCircles()
        {
            if (turnManager == null || draggingCharacter == null)
                return;

            var enemies = turnManager.GetEnemyCombatants();

            // Use collider centres throughout so detection aligns with visible bodies.
            Vector3 playerCenter = GetColliderWorldCenter(draggingCharacter);
            float playerBodyRadius = GetColliderHalfWidth(draggingCharacter);
            float playerAttackRange = draggingCharacter.GetAttackRangeRadius();
            int enemiesInAttackRange = 0;
            List<CharacterInstance> attackableEnemies = new List<CharacterInstance>();

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;

                Vector3 enemyCenter = GetColliderWorldCenter(enemy);
                float distSqr = (playerCenter - enemyCenter).sqrMagnitude;

                // Red threat circle.
                float threatThreshold = enemy.GetAttackRangeRadius() + playerBodyRadius;
                bool inThreatRange = distSqr <= threatThreshold * threatThreshold;
                bool wasShowingThreat = enemiesShowingRange.Contains(enemy);

                if (inThreatRange && !wasShowingThreat)
                {
                    var display = enemy.GetComponent<RangeCircleDisplay>()
                                  ?? enemy.gameObject.AddComponent<RangeCircleDisplay>();
                    display.Show(enemy.GetAttackRangeRadius(), EnemyRangeColor, decorativeMarkers: false);
                    enemiesShowingRange.Add(enemy);
                }
                else if (!inThreatRange && wasShowingThreat)
                {
                    var display = enemy.GetComponent<RangeCircleDisplay>();
                    if (display != null)
                        display.Hide();
                    enemiesShowingRange.Remove(enemy);
                }

                // Attack-ready indicator on the enemy. Combo counts are shown only
                // after confirmed combo resolution, not during drag prediction.
                float enemyBodyRadius = GetColliderHalfWidth(enemy);
                float attackThreshold = playerAttackRange + enemyBodyRadius;
                bool canAttack = distSqr <= attackThreshold * attackThreshold;
                bool wasShowingIndicator = enemiesShowingAttackIndicator.Contains(enemy);

                if (canAttack)
                {
                    enemiesInAttackRange++;
                    attackableEnemies.Add(enemy);

                    var indicator = enemy.GetComponent<AttackTargetIndicator>()
                                    ?? enemy.gameObject.AddComponent<AttackTargetIndicator>();
                    indicator.ShowAttackReady();

                    if (!wasShowingIndicator)
                        enemiesShowingAttackIndicator.Add(enemy);
                }
                else if (!canAttack && wasShowingIndicator)
                {
                    var indicator = enemy.GetComponent<AttackTargetIndicator>();
                    if (indicator != null)
                        indicator.Hide();
                    enemiesShowingAttackIndicator.Remove(enemy);
                }
            }

            if (dragMultiTargetIndicator != null)
            {
                if (enemiesInAttackRange >= 2)
                    dragMultiTargetIndicator.Show(enemiesInAttackRange);
                else
                    dragMultiTargetIndicator.Hide();
            }

            UpdateCriticalRateBoostPreviews(attackableEnemies);
        }

        private void UpdateCriticalRateBoostPreviews(List<CharacterInstance> attackableEnemies)
        {
            CharacterInstance previewTarget = ResolveCriticalRatePreviewTarget(attackableEnemies);
            if (previewTarget == null || battleManager == null)
            {
                HideAllCriticalRateBoostPreviews();
                return;
            }

            List<CharacterInstance> comboParticipants = CombatComboUtility.GetPlayerComboParticipants(turnManager, draggingCharacter, previewTarget);
            if (comboParticipants.Count < 2)
            {
                HideAllCriticalRateBoostPreviews();
                return;
            }

            HashSet<CharacterInstance> visibleThisFrame = new HashSet<CharacterInstance>();
            for (int i = 0; i < comboParticipants.Count; i++)
            {
                CharacterInstance participant = comboParticipants[i];
                if (participant == null || !participant.IsAlive)
                    continue;

                float criticalRateMultiplier = CombatCriticalSupportUtility.GetCriticalRateMultiplier(
                    battleManager,
                    participant,
                    previewTarget,
                    comboParticipants);

                if (criticalRateMultiplier <= 1.01f)
                    continue;

                CriticalRateBoostPreview preview = participant.GetComponent<CriticalRateBoostPreview>()
                    ?? participant.gameObject.AddComponent<CriticalRateBoostPreview>();
                preview.Show(criticalRateMultiplier);
                visibleThisFrame.Add(participant);
            }

            staleCriticalRateBoostPreviews.Clear();
            foreach (CharacterInstance ally in alliesShowingCriticalRateBoostPreview)
            {
                if (ally == null || visibleThisFrame.Contains(ally))
                    continue;

                staleCriticalRateBoostPreviews.Add(ally);
            }

            for (int i = 0; i < staleCriticalRateBoostPreviews.Count; i++)
            {
                CharacterInstance staleAlly = staleCriticalRateBoostPreviews[i];
                if (staleAlly != null)
                {
                    CriticalRateBoostPreview preview = staleAlly.GetComponent<CriticalRateBoostPreview>();
                    if (preview != null)
                        preview.Hide();
                }

                alliesShowingCriticalRateBoostPreview.Remove(staleAlly);
            }

            staleCriticalRateBoostPreviews.Clear();
            foreach (CharacterInstance visibleAlly in visibleThisFrame)
                alliesShowingCriticalRateBoostPreview.Add(visibleAlly);
        }

        private CharacterInstance ResolveCriticalRatePreviewTarget(List<CharacterInstance> attackableEnemies)
        {
            if (attackableEnemies == null || attackableEnemies.Count == 0 || draggingCharacter == null)
                return null;

            if (hasValidPointerSample
                && TryGetEnemyAtScreenPosition(lastValidScreenPos, out CharacterInstance hoveredEnemy)
                && attackableEnemies.Contains(hoveredEnemy))
            {
                return hoveredEnemy;
            }

            return attackableEnemies.Count == 1 ? attackableEnemies[0] : null;
        }

        private void HideAllCriticalRateBoostPreviews()
        {
            if (alliesShowingCriticalRateBoostPreview.Count == 0)
                return;

            staleCriticalRateBoostPreviews.Clear();
            foreach (CharacterInstance ally in alliesShowingCriticalRateBoostPreview)
                staleCriticalRateBoostPreviews.Add(ally);

            for (int i = 0; i < staleCriticalRateBoostPreviews.Count; i++)
            {
                CharacterInstance ally = staleCriticalRateBoostPreviews[i];
                if (ally == null)
                    continue;

                CriticalRateBoostPreview preview = ally.GetComponent<CriticalRateBoostPreview>();
                if (preview != null)
                    preview.Hide();
            }

            staleCriticalRateBoostPreviews.Clear();
            alliesShowingCriticalRateBoostPreview.Clear();
        }
        private List<CharacterInstance> GetReleaseAttackTargets(
            CharacterInstance attacker,
            CharacterInstance preferredPrimaryTarget = null)
        {
            List<CharacterInstance> targets = new List<CharacterInstance>();
            if (turnManager == null || attacker == null)
                return targets;

            if (!turnManager.IsPlayerUnit(attacker) || !attacker.CanAttack)
                return targets;

            Vector3 attackerCenter = GetColliderWorldCenter(attacker);
            IReadOnlyList<CharacterInstance> enemies = turnManager.GetEnemyCombatants();
            foreach (CharacterInstance enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;

                Vector3 enemyCenter = GetColliderWorldCenter(enemy);
                float attackThreshold = attacker.GetAttackRangeRadius() + GetColliderHalfWidth(enemy);
                float distSqr = (attackerCenter - enemyCenter).sqrMagnitude;
                if (distSqr <= attackThreshold * attackThreshold)
                    targets.Add(enemy);
            }

            targets.Sort((left, right) =>
            {
                if (preferredPrimaryTarget != null)
                {
                    bool leftPreferred = left == preferredPrimaryTarget;
                    bool rightPreferred = right == preferredPrimaryTarget;
                    if (leftPreferred != rightPreferred)
                        return leftPreferred ? -1 : 1;
                }

                float leftSqr = (GetColliderWorldCenter(left) - attackerCenter).sqrMagnitude;
                float rightSqr = (GetColliderWorldCenter(right) - attackerCenter).sqrMagnitude;
                return leftSqr.CompareTo(rightSqr);
            });

            return targets;
        }

        private IEnumerator ResolveReleaseAttackSequence(CharacterInstance attacker, Vector3 releaseWorldPos, List<CharacterInstance> targets)
        {
            isResolvingAttackSequence = true;

            try
            {
                if (attacker == null || !attacker.IsAlive)
                    yield break;

                Animator attackerAnimator = attacker.GetComponentInChildren<Animator>();
                CharacterInstance finalResolvedTarget = null;
                int resolvedHitCount = 0;
                bool consumedAttackAction = false;
                bool comboCutInShown = false;
                CombatComboPresentationBus.Reset();

                for (int i = 0; i < targets.Count; i++)
                {
                    CharacterInstance target = targets[i];
                    if (target == null || !target.IsAlive)
                        continue;

                    Vector3 strikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(
                        attacker,
                        target,
                        CombatComboUtility.GetActiveCombatantsExcept(turnManager, attacker, target));
                    FaceCharacterTowards(attacker, target);

                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", true);

                    yield return MoveCharacterToWorldPosition(attacker.transform, strikeWorldPos, chainedAttackTravelTime);

                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", false);

                    if (target == null || !target.IsAlive || !attacker.IsAlive)
                        continue;

                    List<CharacterInstance> comboParticipants = CombatComboUtility.GetPlayerComboParticipants(turnManager, attacker, target);
                    if (!comboCutInShown && comboParticipants.Count >= 2)
                    {
                        CombatComboPresentationBus.ReportStarted(comboParticipants);
                        comboCutInShown = true;
                    }

                    bool consumeAttackAction = !consumedAttackAction;
                    if (consumeAttackAction && !attacker.CanAttack)
                        continue;

                    attacker.PerformBasicAttack(consumeAttackAction: consumeAttackAction);
                    consumedAttackAction = true;
                    yield return new WaitForSeconds(chainedAttackHitPause);

                    if (CombatCriticalSupportUtility.TryResolveBasicHit(battleManager, attacker, target, comboParticipants, out _))
                    {
                        finalResolvedTarget = target;
                        resolvedHitCount++;
                        CombatComboPresentationBus.ReportHit(resolvedHitCount);
                    }

                    for (int participantIndex = 1; participantIndex < comboParticipants.Count; participantIndex++)
                    {
                        CharacterInstance ally = comboParticipants[participantIndex];
                        if (ally == null || !ally.IsAlive || target == null || !target.IsAlive)
                            continue;

                        Vector3 allyStartWorldPos = CombatMovementUtility.GetWorldPosition(ally.transform);
                        Animator allyAnimator = ally.GetComponentInChildren<Animator>();
                        Vector3 allyStrikeWorldPos = CombatMovementUtility.GetAttackApproachPosition(
                            ally,
                            target,
                            CombatComboUtility.GetActiveCombatantsExcept(turnManager, ally, target));

                        FaceCharacterTowards(ally, target);

                        if (allyAnimator != null)
                            allyAnimator.SetBool("isRunning", true);

                        yield return MoveCharacterToWorldPosition(ally.transform, allyStrikeWorldPos, chainedAttackTravelTime);

                        if (allyAnimator != null)
                            allyAnimator.SetBool("isRunning", false);

                        if (ally.IsAlive && target != null && target.IsAlive)
                        {
                            ally.PerformBasicAttack(consumeAttackAction: false);
                            yield return new WaitForSeconds(chainedAttackHitPause);

                            if (CombatCriticalSupportUtility.TryResolveBasicHit(battleManager, ally, target, comboParticipants, out _))
                            {
                                finalResolvedTarget = target;
                                resolvedHitCount++;
                                CombatComboPresentationBus.ReportHit(resolvedHitCount);
                            }

                            yield return new WaitForSeconds(chainedAttackRecoverTime);
                        }

                        if (ally != null && ally.IsAlive)
                        {
                            if (allyAnimator != null)
                                allyAnimator.SetBool("isRunning", true);

                            yield return MoveCharacterToWorldPosition(ally.transform, allyStartWorldPos, chainedAttackReturnTime);

                            if (allyAnimator != null)
                                allyAnimator.SetBool("isRunning", false);
                        }
                    }

                    yield return new WaitForSeconds(chainedAttackRecoverTime);
                }

                if (resolvedHitCount >= 2 && finalResolvedTarget != null)
                {
                    AttackTargetIndicator indicator = finalResolvedTarget.GetComponent<AttackTargetIndicator>()
                        ?? finalResolvedTarget.gameObject.AddComponent<AttackTargetIndicator>();
                    indicator.PlayComboBurst(resolvedHitCount);
                }

                CombatComboPresentationBus.ReportFinished(resolvedHitCount);

                if (attacker != null && attacker.IsAlive)
                {
                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", true);

                    yield return MoveCharacterToWorldPosition(attacker.transform, releaseWorldPos, chainedAttackReturnTime);

                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", false);
                }

                if (turnManager != null && turnManager.IsBattleActive && turnManager.GetCurrentCombatant() == attacker)
                    turnManager.EndTurn();
            }
            finally
            {
                attackResolutionCoroutine = null;
                isResolvingAttackSequence = false;
            }
        }
        private IEnumerator MoveCharacterToWorldPosition(Transform characterTransform, Vector3 worldPos, float duration)
            => CombatMovementUtility.MoveCharacterToWorldPosition(characterTransform, worldPos, duration);

        // CMU version includes slot-scoring + blocker avoidance — strictly better.
        private Vector3 GetAttackApproachPosition(CharacterInstance attacker, CharacterInstance target)
            => CombatMovementUtility.GetAttackApproachPosition(attacker, target);

        private static void FaceCharacterTowards(CharacterInstance attacker, CharacterInstance target)
            => CombatMovementUtility.FaceCharacterTowards(attacker, target);

        // Player range circles use one shared blue grammar. Character accent colours stay
        // on portraits and identity surfaces instead of overloading the combat boundary.
        private static Color GetPlayerRangeColor()
            => RangeCircleDisplay.DefaultPlayerRangeColor;
        // Collider query forwarding — shared with the rest of the combat pipeline.
        private static Vector3 GetColliderWorldCenter(CharacterInstance character)
            => CombatMovementUtility.GetColliderWorldCenter(character);

        private static float GetColliderHalfWidth(CharacterInstance character)
            => CombatMovementUtility.GetColliderHalfWidth(character);

        private void HideAllEnemyRangeCircles()
        {
            foreach (var enemy in enemiesShowingRange)
            {
                if (enemy == null) continue;
                var display = enemy.GetComponent<RangeCircleDisplay>();
                if (display != null) display.Hide();
            }
            enemiesShowingRange.Clear();

            foreach (var enemy in enemiesShowingAttackIndicator)
            {
                if (enemy == null) continue;
                var indicator = enemy.GetComponent<AttackTargetIndicator>();
                if (indicator != null) indicator.Hide();
            }
            enemiesShowingAttackIndicator.Clear();
            HideAllCriticalRateBoostPreviews();
        }

        private bool IsTapOnEnemy(Vector2 screenPosition)
            => TryGetEnemyAtScreenPosition(screenPosition, out _);

        private bool TryGetEnemyAtScreenPosition(Vector2 screenPosition, out CharacterInstance enemy)
        {
            enemy = null;

            if (turnManager == null)
                return false;

            CharacterInstance currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null || !turnManager.IsPlayerUnit(currentCharacter))
                return false;

            if (!TryGetPointerWorld(screenPosition, currentCharacter.transform, out Vector3 worldPos))
                return false;

            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.5f);
            if (hit == null)
                return false;

            CharacterInstance target = hit.GetComponent<CharacterInstance>();
            if (target == null || !target.IsAlive || !turnManager.IsEnemyUnit(target))
                return false;

            enemy = target;
            return true;
        }

        private void StartTapMove(Vector2 screenPosition)
        {
            if (turnManager == null)
                return;

            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null)
                return;

            if (!TryGetPointerWorld(screenPosition, currentCharacter.transform, out Vector3 worldPos))
                return;

            if (tapMoveCoroutine != null)
                StopCoroutine(tapMoveCoroutine);

            if (snapToGrid)
                worldPos = SnapToGrid(worldPos);

            tapMoveCoroutine = StartCoroutine(SmoothMoveToPosition(currentCharacter, worldPos));
        }

        private IEnumerator SmoothMoveToPosition(Component character, Vector3 targetWorldPos)
        {
            Transform charTransform = character.transform;
            Transform charParent = charTransform.parent;
            Vector3 startWorldPos = GetCharacterPosition(charTransform);
            Animator anim = character.GetComponentInChildren<Animator>();
            if (anim != null)
                anim.SetBool("isRunning", true);

            if (charParent)
            {
                Vector3 startLocal = charTransform.localPosition;
                Vector3 targetLocal = charParent.InverseTransformPoint(targetWorldPos);
                float totalDist = Vector3.Distance(startLocal, targetLocal);
                float minDuration = 0.1f;
                float duration = Mathf.Max(minDuration, totalDist / tapMoveSpeed);
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    Vector3 nextLocal = Vector3.Lerp(startLocal, targetLocal, t);
                    charTransform.localPosition = nextLocal;
                    yield return null;
                }

                charTransform.localPosition = targetLocal;
            }
            else
            {
                Vector3 startWorld = charTransform.position;
                float totalDist = Vector3.Distance(startWorld, targetWorldPos);
                float minDuration = 0.1f;
                float duration = Mathf.Max(minDuration, totalDist / tapMoveSpeed);
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    Vector3 nextWorld = Vector3.Lerp(startWorld, targetWorldPos, t);
                    charTransform.position = nextWorld;
                    yield return null;
                }

                charTransform.position = targetWorldPos;
            }

            if (anim != null)
                anim.SetBool("isRunning", false);

            tapMoveCoroutine = null;

            if (character is not CharacterInstance movedCharacter)
                yield break;

            List<CharacterInstance> releaseTargets = GetReleaseAttackTargets(movedCharacter);
            if (releaseTargets.Count > 0)
            {
                if (attackResolutionCoroutine != null)
                    StopCoroutine(attackResolutionCoroutine);

                attackResolutionCoroutine = StartCoroutine(
                    ResolveReleaseAttackSequence(movedCharacter, targetWorldPos, releaseTargets));
                yield break;
            }

            if (DidCommitMovement(startWorldPos, targetWorldPos))
                ConsumeTurnAfterMovement(movedCharacter);
        }

        private void ConsumeTurnAfterMovement(CharacterInstance movedCharacter)
        {
            if (turnManager == null
                || movedCharacter == null
                || !turnManager.IsBattleActive
                || !movedCharacter.IsAlive
                || !turnManager.IsPlayerUnit(movedCharacter)
                || turnManager.GetCurrentCombatant() != movedCharacter)
            {
                return;
            }

            turnManager.EndTurn();
        }

        private static bool DidCommitMovement(Vector3 startWorldPos, Vector3 endWorldPos)
            => Vector3.Distance(startWorldPos, endWorldPos) > movementCommitDistanceThreshold;

        private bool TryGetPointerWorld(Vector2 screenPosition, Transform charTransform, out Vector3 pointerWorld)
        {
            pointerWorld = charTransform != null ? charTransform.position : Vector3.zero;
            if (charTransform == null)
                return false;

            if (!TryNormalizeScreenPosition(screenPosition, out Vector2 normalizedPos))
                return false;

            if (mainCamera == null)
                mainCamera = Camera.main;
            if (mainCamera == null)
                return false;

            Transform charParent = charTransform.parent;
            float referenceZ = charParent ? charParent.position.z : charTransform.position.z;
            float z = Mathf.Abs(mainCamera.transform.position.z - referenceZ);

            pointerWorld = mainCamera.ScreenToWorldPoint(new Vector3(normalizedPos.x, normalizedPos.y, z));
            pointerWorld.z = charTransform.position.z;
            return true;
        }

        private bool TryNormalizeScreenPosition(Vector2 rawScreenPos, out Vector2 normalizedScreenPos)
        {
            if (TryAcceptScreenPosition(rawScreenPos, out normalizedScreenPos))
                return true;

            if (TryGetCurrentMouseScreenPosition(out Vector2 fallbackMousePos)
                && TryAcceptScreenPosition(fallbackMousePos, out normalizedScreenPos))
            {
                return true;
            }

            normalizedScreenPos = rawScreenPos;
            return false;
        }

        private static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x)
                   && !float.IsNaN(value.y)
                   && !float.IsInfinity(value.x)
                   && !float.IsInfinity(value.y);
        }

        private bool TryAcceptScreenPosition(Vector2 candidateScreenPos, out Vector2 normalizedScreenPos)
        {
            normalizedScreenPos = candidateScreenPos;

            if (!IsFinite(candidateScreenPos))
                return false;

            if (candidateScreenPos.sqrMagnitude <= 0.0001f)
                return false;

            float minX = -2f;
            float minY = -2f;
            float maxX = Screen.width + 2f;
            float maxY = Screen.height + 2f;
            if (candidateScreenPos.x < minX
                || candidateScreenPos.y < minY
                || candidateScreenPos.x > maxX
                || candidateScreenPos.y > maxY)
            {
                return false;
            }

            float clampedX = Mathf.Clamp(candidateScreenPos.x, 0f, Screen.width);
            float clampedY = Mathf.Clamp(candidateScreenPos.y, 0f, Screen.height);
            normalizedScreenPos = new Vector2(clampedX, clampedY);

            hasValidPointerSample = true;
            lastValidScreenPos = normalizedScreenPos;
            return true;
        }

        private static bool TryGetCurrentMouseScreenPosition(out Vector2 mouseScreenPos)
        {
            // Legacy Input.mousePosition throws when the Input System package is active.
            // Use Mouse.current from the new Input System instead.
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                mouseScreenPos = mouse.position.ReadValue();
                return !float.IsNaN(mouseScreenPos.x)
                       && !float.IsNaN(mouseScreenPos.y)
                       && !float.IsInfinity(mouseScreenPos.x)
                       && !float.IsInfinity(mouseScreenPos.y);
            }
            mouseScreenPos = Vector2.zero;
            return false;
        }

        private Vector3 SnapToGrid(Vector3 worldPosition)
        {
            return new Vector3(
                Mathf.Round(worldPosition.x / gridSize) * gridSize,
                Mathf.Round(worldPosition.y / gridSize) * gridSize,
                worldPosition.z
            );
        }

        private void SetCharacterPosition(Transform charTransform, Vector3 worldPos)
        {
            Transform charParent = charTransform.parent;
            if (charParent)
                charTransform.localPosition = charParent.InverseTransformPoint(worldPos);
            else
                charTransform.position = worldPos;
        }

        private Vector3 GetCharacterPosition(Transform charTransform)
        {
            Transform charParent = charTransform.parent;
            if (charParent)
                return charParent.TransformPoint(charTransform.localPosition);

            return charTransform.position;
        }

        private void ApplyDragOpacity()
        {
            if (draggingSpriteRenderer == null)
                return;

            draggingOriginalColor = draggingSpriteRenderer.color;
            Color fadedColor = draggingOriginalColor;
            fadedColor.a = draggingOriginalColor.a * dragOpacity;
            draggingSpriteRenderer.color = fadedColor;
        }

        private void RestoreDragOpacity()
        {
            if (draggingSpriteRenderer == null)
                return;

            draggingSpriteRenderer.color = draggingOriginalColor;
        }

        private void ClearDragState()
        {
            if (dragRangeCircle != null)
            {
                dragRangeCircle.Hide();
                dragRangeCircle = null;
            }

            if (dragMultiTargetIndicator != null)
            {
                dragMultiTargetIndicator.Hide();
                dragMultiTargetIndicator = null;
            }

            HideAllEnemyRangeCircles();

            if (characterAnimator != null)
                characterAnimator.SetBool("isRunning", false);

            isDragging = false;
            dragStartWorldPos = Vector3.zero;
            draggingCharacter = null;
            characterAnimator = null;
            draggingSpriteRenderer = null;
            dragVelocity = Vector3.zero;
            hasValidPointerDown = false;
            lastDragMotionTime = float.NegativeInfinity;
        }
    }
}

