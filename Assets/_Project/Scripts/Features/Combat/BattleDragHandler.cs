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

        private Camera mainCamera;
        private Vector2 pointerDownPos;
        private float pointerDownTime;
        private bool hasValidPointerDown = false;
        private bool hasValidPointerSample = false;
        private Vector2 lastValidScreenPos = Vector2.zero;
        private float lastDragMotionTime = float.NegativeInfinity;

        private bool isDragging = false;
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

        void Awake()
        {
            mainCamera = Camera.main;
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
                StartTapMove(releasePos);
                ClearDragState();
                return;
            }

            if (isDragging && draggingCharacter != null)
            {
                CharacterInstance releasedCharacter = draggingCharacter;
                Animator releasedAnimator = characterAnimator;
                Vector3 releaseWorldPos = GetCharacterPosition(releasedCharacter.transform);

                if (TryGetPointerWorld(releasePos, releasedCharacter.transform, out Vector3 pointerWorld))
                {
                    releaseWorldPos = snapToGrid ? SnapToGrid(pointerWorld) : pointerWorld;
                    SetCharacterPosition(releasedCharacter.transform, releaseWorldPos);
                }

                if (releasedAnimator != null)
                    releasedAnimator.SetBool("isRunning", false);

                RestoreDragOpacity();

                List<CharacterInstance> releaseTargets = GetReleaseAttackTargets(releasedCharacter);
                ClearDragState();

                if (releaseTargets.Count > 0)
                {
                    if (attackResolutionCoroutine != null)
                        StopCoroutine(attackResolutionCoroutine);

                    attackResolutionCoroutine = StartCoroutine(ResolveReleaseAttackSequence(releasedCharacter, releaseWorldPos, releaseTargets));
                    return;
                }
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

                    dragRangeCircle.Show(draggingCharacter.GetAttackRangeRadius(), GetPlayerRangeColor(draggingCharacter));

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
                    display.Show(enemy.GetAttackRangeRadius(), EnemyRangeColor);
                    enemiesShowingRange.Add(enemy);
                }
                else if (!inThreatRange && wasShowingThreat)
                {
                    var display = enemy.GetComponent<RangeCircleDisplay>();
                    if (display != null)
                        display.Hide();
                    enemiesShowingRange.Remove(enemy);
                }

                // Attack-ready or ally-combo indicator on the enemy.
                float enemyBodyRadius = GetColliderHalfWidth(enemy);
                float attackThreshold = playerAttackRange + enemyBodyRadius;
                bool canAttack = distSqr <= attackThreshold * attackThreshold;
                bool wasShowingIndicator = enemiesShowingAttackIndicator.Contains(enemy);

                if (canAttack)
                {
                    enemiesInAttackRange++;

                    var indicator = enemy.GetComponent<AttackTargetIndicator>()
                                    ?? enemy.gameObject.AddComponent<AttackTargetIndicator>();

                    int comboPartners = CountComboPartners(enemy, enemyCenter);
                    if (comboPartners > 0)
                        indicator.ShowComboReady(comboPartners + 1);
                    else
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
        }

        // Returns how many OTHER alive player units (not the one being dragged)
        // can also reach this enemy from their current position.
        // 0 = no combo; 1 = ×2 combo; 2 = ×3 combo, etc.
        private int CountComboPartners(CharacterInstance enemy, Vector3 enemyCenter)
        {
            int count = 0;
            var players = turnManager.GetPlayerCombatants();
            foreach (var ally in players)
            {
                if (ally == draggingCharacter) continue;
                if (!ally.IsAlive) continue;
                float allyRange = ally.GetAttackRangeRadius() + GetColliderHalfWidth(enemy);
                float allyDistSqr = (GetColliderWorldCenter(ally) - enemyCenter).sqrMagnitude;
                if (allyDistSqr <= allyRange * allyRange)
                    count++;
            }
            return count;
        }

        private List<CharacterInstance> GetReleaseAttackTargets(CharacterInstance attacker)
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
                for (int i = 0; i < targets.Count; i++)
                {
                    CharacterInstance target = targets[i];
                    if (target == null || !target.IsAlive)
                        continue;

                    Vector3 strikeWorldPos = GetAttackApproachPosition(attacker, target);
                    FaceCharacterTowards(attacker, target);

                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", true);

                    yield return MoveCharacterToWorldPosition(attacker.transform, strikeWorldPos, chainedAttackTravelTime);

                    if (attackerAnimator != null)
                        attackerAnimator.SetBool("isRunning", false);

                    attacker.PerformBasicAttack(consumeAttackAction: i == 0);
                    yield return new WaitForSeconds(chainedAttackHitPause);

                    float damage = attacker.CalculateDamageAgainst(target);
                    target.TakeDamage(damage);
                    BattleFloatingText.SpawnDamage(target, damage);

                    if (i > 0)
                    {
                        AttackTargetIndicator indicator = target.GetComponent<AttackTargetIndicator>()
                            ?? target.gameObject.AddComponent<AttackTargetIndicator>();
                        indicator.PlayComboBurst(i + 1);
                    }

                    yield return new WaitForSeconds(chainedAttackRecoverTime);
                }

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
        {
            Vector3 startWorldPos = GetCharacterPosition(characterTransform);
            float elapsed = 0f;
            float clampedDuration = Mathf.Max(0.01f, duration);

            while (elapsed < clampedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);
                Vector3 nextWorldPos = Vector3.Lerp(startWorldPos, worldPos, t);
                SetCharacterPosition(characterTransform, nextWorldPos);
                yield return null;
            }

            SetCharacterPosition(characterTransform, worldPos);
        }

        private Vector3 GetAttackApproachPosition(CharacterInstance attacker, CharacterInstance target)
        {
            Vector3 attackerCenter = GetColliderWorldCenter(attacker);
            Vector3 targetCenter = GetColliderWorldCenter(target);
            Vector3 direction = targetCenter - attackerCenter;
            if (direction.sqrMagnitude <= 0.0001f)
                direction = target.transform.position.x >= attacker.transform.position.x ? Vector3.right : Vector3.left;

            direction.Normalize();

            float separation = Mathf.Max(0.1f, GetColliderHalfWidth(attacker) + GetColliderHalfWidth(target) - 0.05f);
            Vector3 desiredCenter = targetCenter - direction * separation;
            Vector3 attackerCenterOffset = attackerCenter - attacker.transform.position;
            return desiredCenter - attackerCenterOffset;
        }

        private static void FaceCharacterTowards(CharacterInstance attacker, CharacterInstance target)
        {
            if (attacker == null || target == null)
                return;

            float direction = target.transform.position.x < attacker.transform.position.x ? -1f : 1f;
            if (attacker.Definition != null && attacker.Definition.InvertFacingX)
                direction *= -1f;

            Vector3 scale = attacker.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            attacker.transform.localScale = scale;
        }

        // Returns the range-circle colour for a player unit: reads the character's
        // paletteAccentColor at 0.85 opacity so every fighter has a distinct glow.
        // Falls back to teal when the definition is unavailable.
        private static Color GetPlayerRangeColor(CharacterInstance character)
        {
            if (character != null && character.Definition != null)
            {
                Color c = character.Definition.PaletteAccentColor;
                c.a = 0.85f;
                return c;
            }
            return new Color(0.2f, 0.9f, 1f, 0.85f); // fallback teal
        }

        // Returns the world-space centre of a character's CapsuleCollider2D.
        // Falls back to transform.position when no collider is present.
        private static Vector3 GetColliderWorldCenter(CharacterInstance character)
        {
            var col = character.GetComponent<CapsuleCollider2D>();
            if (col != null)
                return character.transform.TransformPoint(col.offset);
            return character.transform.position;
        }

        // Returns the world-space half-width of a character's CapsuleCollider2D
        // (X axis, scaled).  Used to expand the detection radius so the circle
        // activates when the player's body edge crosses the boundary.
        private static float GetColliderHalfWidth(CharacterInstance character)
        {
            var col = character.GetComponent<CapsuleCollider2D>();
            if (col != null)
                return col.size.x * 0.5f * Mathf.Abs(character.transform.lossyScale.x);
            return 0.5f; // sensible fallback — ~half a world-unit
        }

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
        }

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
            draggingCharacter = null;
            characterAnimator = null;
            draggingSpriteRenderer = null;
            dragVelocity = Vector3.zero;
            hasValidPointerDown = false;
            lastDragMotionTime = float.NegativeInfinity;
        }
    }
}







