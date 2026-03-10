using System.Collections;
using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Shared combat movement and positioning utilities.
    /// Handles parent-aware world-position movement, attack approach slot scoring,
    /// facing, and sprite-bounds queries for CharacterInstance objects.
    /// </summary>
    public static class CombatMovementUtility
    {
        private const float AttackSeparationPadding = 0.12f;
        private const float SlotSpacingScale = 1.15f;
        private const float MinimumSlotSpacing = 0.95f;
        private const int CandidateSlotDepth = 5;
        private const float CandidateDistancePenalty = 0.08f;
        private const float SlotIndexPenalty = 0.05f;
        private const float BlockerClearancePadding = 0.24f;
        private const float OverlapPenaltyScale = 4f;

        private static readonly int[] SlotOrder = BuildSlotOrder();

        // ──────────────────────────────────────────────────────────────────────
        // Movement
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Smoothly moves <paramref name="characterTransform"/> to a world-space position
        /// over <paramref name="duration"/> seconds.  Parent-aware — sets localPosition when
        /// a parent exists so that other parent-relative logic stays consistent.
        /// </summary>
        public static IEnumerator MoveCharacterToWorldPosition(Transform characterTransform, Vector3 worldPos, float duration)
        {
            Vector3 startWorldPos = GetWorldPosition(characterTransform);
            float elapsed = 0f;
            float clampedDuration = Mathf.Max(0.01f, duration);

            while (elapsed < clampedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);
                Vector3 nextWorldPos = Vector3.Lerp(startWorldPos, worldPos, t);
                SetWorldPosition(characterTransform, nextWorldPos);
                yield return null;
            }

            SetWorldPosition(characterTransform, worldPos);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Attack positioning
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the best world-space position for <paramref name="attacker"/> to stand
        /// when striking <paramref name="target"/>, scoring candidate slots to avoid
        /// overlapping <paramref name="blockers"/>.
        /// </summary>
        public static Vector3 GetAttackApproachPosition(CharacterInstance attacker, CharacterInstance target, IEnumerable<CharacterInstance> blockers = null)
        {
            if (attacker == null || target == null)
                return Vector3.zero;

            Vector3 attackerWorldPos = GetWorldPosition(attacker.transform);
            Vector3 attackerCenter = GetColliderWorldCenter(attacker);
            Vector3 targetCenter = GetColliderWorldCenter(target);
            Vector3 attackerCenterOffset = attackerCenter - attackerWorldPos;

            float horizontalDirection = ResolveHorizontalAttackDirection(attacker, attackerCenter, targetCenter);
            float separation = Mathf.Max(0.2f, GetVisualHalfWidth(attacker) + GetVisualHalfWidth(target) + AttackSeparationPadding);
            float slotSpacing = Mathf.Max(MinimumSlotSpacing, GetFootprintRadius(attacker) * SlotSpacingScale);

            // Precompute the direct adjacent position — used as a guaranteed fallback
            // when slot scoring produces NaN or -Infinity for every candidate.
            // This happens when any blocker's GetFootprintRadius() overflows to Infinity
            // (e.g. a character with extreme or malformed sprite bounds), which makes every
            // ScoreCandidate return -Infinity, and -Inf > -Inf is false so bestWorldPos
            // is never written and the attacker silently stays at its spawn point.
            Vector3 directCenter = new Vector3(
                targetCenter.x - horizontalDirection * separation,
                targetCenter.y,
                targetCenter.z);
            Vector3 directWorldPos = directCenter - attackerCenterOffset;
            directWorldPos.z = attackerWorldPos.z;

            Vector3 bestWorldPos = directWorldPos;   // safe fallback — always adjacent
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < SlotOrder.Length; i++)
            {
                int slotIndex = SlotOrder[i];
                Vector3 desiredCenter = new Vector3(
                    targetCenter.x - horizontalDirection * separation,
                    targetCenter.y + slotIndex * slotSpacing,
                    targetCenter.z);

                Vector3 candidateWorldPos = desiredCenter - attackerCenterOffset;
                candidateWorldPos.z = attackerWorldPos.z;

                float candidateScore = ScoreCandidate(candidateWorldPos, attacker, target, blockers, attackerCenterOffset, attackerWorldPos, slotIndex);

                // Skip NaN/-Infinity scores — they indicate overflow in footprint math.
                // Any finite score, even a very negative one, is a valid candidate.
                if (!float.IsFinite(candidateScore))
                    continue;

                if (candidateScore > bestScore)
                {
                    bestScore = candidateScore;
                    bestWorldPos = candidateWorldPos;
                }
            }

            bestWorldPos.z = attackerWorldPos.z;
            return bestWorldPos;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Facing
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Flips <paramref name="attacker"/>'s X scale so it faces <paramref name="target"/>,
        /// respecting the character's <c>InvertFacingX</c> definition flag.
        /// </summary>
        public static void FaceCharacterTowards(CharacterInstance attacker, CharacterInstance target)
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

        // ──────────────────────────────────────────────────────────────────────
        // World-position helpers (parent-aware)
        // ──────────────────────────────────────────────────────────────────────

        public static Vector3 GetWorldPosition(Transform characterTransform)
        {
            if (characterTransform == null)
                return Vector3.zero;

            Transform parent = characterTransform.parent;
            return parent != null ? parent.TransformPoint(characterTransform.localPosition) : characterTransform.position;
        }

        public static void SetWorldPosition(Transform characterTransform, Vector3 worldPos)
        {
            if (characterTransform == null)
                return;

            Transform parent = characterTransform.parent;
            if (parent != null)
                characterTransform.localPosition = parent.InverseTransformPoint(worldPos);
            else
                characterTransform.position = worldPos;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Collider / bounds queries
        // ──────────────────────────────────────────────────────────────────────

        public static Vector3 GetColliderWorldCenter(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col != null)
                return character.transform.TransformPoint(col.offset);

            return character != null ? GetWorldPosition(character.transform) : Vector3.zero;
        }

        public static float GetColliderHalfWidth(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col == null)
                return 0.35f;

            Vector3 lossy = character.transform.lossyScale;
            float scaledWidth = Mathf.Abs(col.size.x * lossy.x);
            // Clamp to sane range — prevents Infinity from malformed/extreme-scale sprites
            // propagating through ScoreCandidate and making every slot score -Infinity.
            return Mathf.Clamp(scaledWidth * 0.5f, 0.05f, 8f);
        }

        public static float GetColliderHalfHeight(CharacterInstance character)
        {
            CapsuleCollider2D col = character != null ? character.GetComponent<CapsuleCollider2D>() : null;
            if (col == null)
                return 0.45f;

            Vector3 lossy = character.transform.lossyScale;
            float scaledHeight = Mathf.Abs(col.size.y * lossy.y);
            return Mathf.Clamp(scaledHeight * 0.5f, 0.05f, 8f);
        }

        public static float GetVisualHalfWidth(CharacterInstance character)
        {
            GetSpriteExtents(character, out float spriteHalfWidth, out _);
            return Mathf.Max(GetColliderHalfWidth(character), spriteHalfWidth);
        }

        public static float GetVisualHalfHeight(CharacterInstance character)
        {
            GetSpriteExtents(character, out _, out float spriteHalfHeight);
            return Mathf.Max(GetColliderHalfHeight(character), spriteHalfHeight);
        }

        public static float GetFootprintRadius(CharacterInstance character)
        {
            float halfWidth = GetVisualHalfWidth(character);
            float halfHeight = GetVisualHalfHeight(character);
            return Mathf.Min(Mathf.Max(halfWidth, halfHeight * 0.8f), 8f);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Private helpers
        // ──────────────────────────────────────────────────────────────────────

        private static float ScoreCandidate(
            Vector3 candidateWorldPos,
            CharacterInstance attacker,
            CharacterInstance target,
            IEnumerable<CharacterInstance> blockers,
            Vector3 attackerCenterOffset,
            Vector3 attackerWorldPos,
            int slotIndex)
        {
            Vector3 candidateCenter = candidateWorldPos + attackerCenterOffset;
            float minClearance = float.MaxValue;
            float overlapPenalty = 0f;

            if (blockers != null)
            {
                foreach (CharacterInstance blocker in blockers)
                {
                    if (blocker == null || !blocker.IsAlive || blocker == attacker || blocker == target)
                        continue;

                    float requiredSpacing = GetFootprintRadius(attacker) + GetFootprintRadius(blocker) + BlockerClearancePadding;
                    float clearance = Vector2.Distance((Vector2)candidateCenter, (Vector2)GetColliderWorldCenter(blocker)) - requiredSpacing;
                    minClearance = Mathf.Min(minClearance, clearance);

                    if (clearance < 0f)
                        overlapPenalty += -clearance * OverlapPenaltyScale;
                }
            }

            if (minClearance == float.MaxValue)
                minClearance = 2f;

            float distancePenalty = Vector2.Distance((Vector2)candidateWorldPos, (Vector2)attackerWorldPos) * CandidateDistancePenalty;
            float slotPenalty = Mathf.Abs(slotIndex) * SlotIndexPenalty;
            return minClearance - overlapPenalty - distancePenalty - slotPenalty;
        }

        private static float ResolveHorizontalAttackDirection(CharacterInstance attacker, Vector3 attackerCenter, Vector3 targetCenter)
        {
            float deltaX = targetCenter.x - attackerCenter.x;
            if (Mathf.Abs(deltaX) > 0.02f)
                return Mathf.Sign(deltaX);

            if (attacker != null && Mathf.Abs(attacker.transform.localScale.x) > 0.001f)
                return Mathf.Sign(attacker.transform.localScale.x);

            return 1f;
        }

        private static int[] BuildSlotOrder()
        {
            int[] slotOrder = new int[CandidateSlotDepth * 2 + 1];
            int nextIndex = 0;
            slotOrder[nextIndex++] = 0;

            for (int i = 1; i <= CandidateSlotDepth; i++)
            {
                slotOrder[nextIndex++] = i;
                slotOrder[nextIndex++] = -i;
            }

            return slotOrder;
        }

        private static void GetSpriteExtents(CharacterInstance character, out float halfWidth, out float halfHeight)
        {
            halfWidth = 0f;
            halfHeight = 0f;
            if (character == null)
                return;

            SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>();
            Bounds combinedBounds = default;
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null || !renderer.enabled)
                    continue;

                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                return;

            Vector3 extents = combinedBounds.extents;
            halfWidth = Mathf.Clamp(Mathf.Abs(extents.x), 0.05f, 8f);
            halfHeight = Mathf.Clamp(Mathf.Abs(extents.y), 0.05f, 8f);
        }
    }
}
