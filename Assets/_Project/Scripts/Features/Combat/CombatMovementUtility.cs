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
        private const float SlotSpacingScale = 1.25f;
        private const float MinimumSlotSpacing = 1.15f;
        private const int CandidateSlotDepth = 5;
        private const int CandidateSeparationDepth = 2;
        private const float CandidateSeparationStep = 0.42f;
        private const float CandidateDistancePenalty = 0.08f;
        private const float SlotIndexPenalty = 0.05f;
        private const float SideSwitchPenalty = 0.12f;
        private const float SeparationPenalty = 0.05f;
        private const float BlockerClearancePadding = 0.34f;
        private const float OverlapPenaltyScale = 10f;
        private const float AttackRangeTolerance = 0.25f;
        private const float VariationPreferenceWeight = 0.12f;
        private const float VariationSlotFalloff = 0.28f;
        private const float VariationSeparationFalloff = 0.55f;

        private static readonly int[] SlotOrder = BuildSlotOrder();

        // Movement

        /// <summary>
        /// Smoothly moves <paramref name="characterTransform"/> to a world-space position
        /// over <paramref name="duration"/> seconds. Parent-aware so child combatants stay
        /// correct under the Characters scene container.
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

        // Attack positioning

        /// <summary>
        /// Returns the best world-space position for <paramref name="attacker"/> to stand
        /// when striking <paramref name="target"/>, scoring candidate slots to avoid
        /// overlapping <paramref name="blockers"/> while still respecting the attack range.
        /// </summary>
        public static Vector3 GetAttackApproachPosition(CharacterInstance attacker, CharacterInstance target, IEnumerable<CharacterInstance> blockers = null)
        {
            if (attacker == null || target == null)
                return Vector3.zero;

            Vector3 attackerWorldPos = GetWorldPosition(attacker.transform);
            Vector3 attackerCenter = GetColliderWorldCenter(attacker);
            Vector3 targetCenter = GetColliderWorldCenter(target);
            Vector3 attackerCenterOffset = attackerCenter - attackerWorldPos;

            float preferredSide = ResolveHorizontalAttackDirection(attacker, attackerCenter, targetCenter);
            float maxReach = Mathf.Max(0.2f, GetAttackRangeThreshold(attacker, target) - 0.05f);
            float desiredSeparation = Mathf.Max(0.2f, GetVisualHalfWidth(attacker) + GetVisualHalfWidth(target) + AttackSeparationPadding);
            float baseSeparation = Mathf.Min(desiredSeparation, maxReach);
            float slotSpacing = Mathf.Max(
                MinimumSlotSpacing,
                Mathf.Max(GetFootprintRadius(attacker), GetFootprintRadius(target)) * SlotSpacingScale);

            int variationSeed = BuildVariationSeed(attacker, target, attackerWorldPos, targetCenter);
            int preferredSlotIndex = ResolvePreferredSlotIndex(variationSeed);
            int preferredSeparationStep = ResolvePreferredSeparationStep(variationSeed);

            Vector3 bestWorldPos = BuildWorldPosition(targetCenter, attackerCenterOffset, attackerWorldPos.z, preferredSide, baseSeparation, 0f);
            float bestScore = float.NegativeInfinity;
            bool bestOverlaps = true;

            for (int sidePass = 0; sidePass < 2; sidePass++)
            {
                float side = sidePass == 0 ? preferredSide : -preferredSide;
                float sidePenalty = sidePass == 0 ? 0f : SideSwitchPenalty;

                for (int separationStep = 0; separationStep <= CandidateSeparationDepth; separationStep++)
                {
                    float separation = Mathf.Min(maxReach, baseSeparation + separationStep * CandidateSeparationStep);
                    float extraPenalty = sidePenalty + separationStep * SeparationPenalty;

                    for (int i = 0; i < SlotOrder.Length; i++)
                    {
                        int slotIndex = SlotOrder[i];
                        Vector3 desiredCenter = new Vector3(
                            targetCenter.x - side * separation,
                            targetCenter.y + slotIndex * slotSpacing,
                            targetCenter.z);

                        float candidateDistanceFromTarget = Vector2.Distance((Vector2)desiredCenter, (Vector2)targetCenter);
                        if (candidateDistanceFromTarget > maxReach)
                            continue;

                        Vector3 candidateWorldPos = desiredCenter - attackerCenterOffset;
                        candidateWorldPos.z = attackerWorldPos.z;

                        float candidateScore = ScoreCandidate(
                            candidateWorldPos,
                            attacker,
                            target,
                            blockers,
                            attackerCenterOffset,
                            attackerWorldPos,
                            slotIndex,
                            extraPenalty,
                            out bool candidateOverlaps);

                        if (!float.IsFinite(candidateScore))
                            continue;

                        candidateScore += GetVariationBonus(slotIndex, separationStep, preferredSlotIndex, preferredSeparationStep);

                        if (candidateOverlaps != bestOverlaps)
                        {
                            if (!candidateOverlaps)
                            {
                                bestOverlaps = false;
                                bestScore = candidateScore;
                                bestWorldPos = candidateWorldPos;
                            }

                            continue;
                        }

                        if (candidateScore > bestScore)
                        {
                            bestScore = candidateScore;
                            bestWorldPos = candidateWorldPos;
                        }
                    }
                }
            }

            bestWorldPos.z = attackerWorldPos.z;
            return bestWorldPos;
        }

        // Facing

        /// <summary>
        /// Flips <paramref name="attacker"/>'s X scale so it faces <paramref name="target"/>,
        /// respecting the character definition's facing inversion flag.
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

        // World-position helpers

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

        // Collider / bounds queries

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

        public static float GetAttackRangeThreshold(CharacterInstance attacker, CharacterInstance target)
        {
            if (attacker == null || target == null)
                return 0f;

            return Mathf.Max(0.2f, attacker.GetAttackRangeRadius() + GetColliderHalfWidth(target) + AttackRangeTolerance);
        }

        public static bool IsTargetWithinAttackRange(CharacterInstance attacker, CharacterInstance target)
        {
            if (attacker == null || target == null)
                return false;

            float distance = Vector2.Distance((Vector2)GetColliderWorldCenter(attacker), (Vector2)GetColliderWorldCenter(target));
            return distance <= GetAttackRangeThreshold(attacker, target);
        }

        // Private helpers

        private static Vector3 BuildWorldPosition(Vector3 targetCenter, Vector3 attackerCenterOffset, float worldZ, float side, float separation, float verticalOffset)
        {
            Vector3 desiredCenter = new Vector3(
                targetCenter.x - side * separation,
                targetCenter.y + verticalOffset,
                targetCenter.z);

            Vector3 worldPos = desiredCenter - attackerCenterOffset;
            worldPos.z = worldZ;
            return worldPos;
        }

        private static float ScoreCandidate(
            Vector3 candidateWorldPos,
            CharacterInstance attacker,
            CharacterInstance target,
            IEnumerable<CharacterInstance> blockers,
            Vector3 attackerCenterOffset,
            Vector3 attackerWorldPos,
            int slotIndex,
            float extraPenalty,
            out bool overlaps)
        {
            Vector3 candidateCenter = candidateWorldPos + attackerCenterOffset;
            float minClearance = float.MaxValue;
            float overlapPenalty = 0f;
            overlaps = false;

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
                    {
                        overlaps = true;
                        overlapPenalty += (-clearance + 0.05f) * OverlapPenaltyScale;
                    }
                }
            }

            if (minClearance == float.MaxValue)
                minClearance = 2f;

            float distancePenalty = Vector2.Distance((Vector2)candidateWorldPos, (Vector2)attackerWorldPos) * CandidateDistancePenalty;
            float slotPenalty = Mathf.Abs(slotIndex) * SlotIndexPenalty;
            return minClearance - overlapPenalty - distancePenalty - slotPenalty - extraPenalty;
        }

        private static float GetVariationBonus(int slotIndex, int separationStep, int preferredSlotIndex, int preferredSeparationStep)
        {
            float slotScore = Mathf.Max(0f, 1f - Mathf.Abs(slotIndex - preferredSlotIndex) * VariationSlotFalloff);
            float separationScore = Mathf.Max(0f, 1f - Mathf.Abs(separationStep - preferredSeparationStep) * VariationSeparationFalloff);
            return VariationPreferenceWeight * slotScore * separationScore;
        }

        private static int ResolvePreferredSlotIndex(int variationSeed)
        {
            float normalized = Hash01(variationSeed, 11) * 2f - 1f;
            float scaled = normalized * CandidateSlotDepth * 0.65f;
            return Mathf.Clamp(Mathf.RoundToInt(scaled), -CandidateSlotDepth, CandidateSlotDepth);
        }

        private static int ResolvePreferredSeparationStep(int variationSeed)
        {
            return Mathf.Clamp(Mathf.FloorToInt(Hash01(variationSeed, 29) * (CandidateSeparationDepth + 1)), 0, CandidateSeparationDepth);
        }

        private static int BuildVariationSeed(CharacterInstance attacker, CharacterInstance target, Vector3 attackerWorldPos, Vector3 targetCenter)
        {
            int seed = 17;
            seed = seed * 31 + (attacker != null ? attacker.GetInstanceID() : 0);
            seed = seed * 31 + (target != null ? target.GetInstanceID() : 0);
            seed = seed * 31 + Mathf.RoundToInt(attackerWorldPos.x * 10f);
            seed = seed * 31 + Mathf.RoundToInt(attackerWorldPos.y * 10f);
            seed = seed * 31 + Mathf.RoundToInt(targetCenter.x * 10f);
            seed = seed * 31 + Mathf.RoundToInt(targetCenter.y * 10f);
            return seed;
        }

        private static float Hash01(int seed, int salt)
        {
            uint value = (uint)(seed ^ (salt * 374761393));
            value ^= value >> 16;
            value *= 2246822519u;
            value ^= value >> 13;
            value *= 3266489917u;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
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
