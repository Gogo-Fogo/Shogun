using System.Collections.Generic;
using Shogun.Features.Characters;
using UnityEngine;

namespace Shogun.Features.Combat
{
    internal readonly struct CombatHitResult
    {
        public CombatHitResult(float damage, bool wasCritical, float criticalChance, float criticalRateMultiplier)
        {
            Damage = damage;
            WasCritical = wasCritical;
            CriticalChance = criticalChance;
            CriticalRateMultiplier = criticalRateMultiplier;
        }

        public float Damage { get; }
        public bool WasCritical { get; }
        public float CriticalChance { get; }
        public float CriticalRateMultiplier { get; }
    }

    internal static class CombatCriticalSupportUtility
    {
        private const float MaxCriticalChance = 0.85f;
        private const float NeutralCriticalChance = 0.08f;
        private const float WeaponAdvantageCritBonus = 0.2f;
        private const float WeaponDisadvantageCritPenalty = 0.1f;
        private const float SharedSynergyCritBonus = 0.2f;
        private const float SameWeaponFamilyCritBonus = 0.1f;
        private const float MinimumSupportContribution = 0.35f;

        public static bool TryResolveBasicHit(
            BattleManager battleManager,
            CharacterInstance attacker,
            CharacterInstance target,
            IReadOnlyList<CharacterInstance> comboParticipants,
            out CombatHitResult hitResult)
        {
            hitResult = default;
            if (attacker == null || target == null || !attacker.IsAlive || !target.IsAlive)
                return false;

            float critRateMultiplier = GetCriticalRateMultiplier(battleManager, attacker, target, comboParticipants);
            float criticalChance = GetCriticalChance(attacker, critRateMultiplier);
            bool isCritical = Random.value < criticalChance;

            float damage = attacker.CalculateDamageAgainst(target);
            if (isCritical)
                damage *= ResolveCriticalDamageMultiplier(attacker);

            target.TakeDamage(damage);
            BattleFloatingText.SpawnDamage(target, damage, isCritical);

            hitResult = new CombatHitResult(damage, isCritical, criticalChance, critRateMultiplier);
            return true;
        }

        public static float GetCriticalRateMultiplier(
            BattleManager battleManager,
            CharacterInstance attacker,
            CharacterInstance target,
            IReadOnlyList<CharacterInstance> comboParticipants)
        {
            if (attacker == null || target == null || !attacker.IsAlive || !target.IsAlive)
                return 1f;

            float multiplier = 1f;

            if (TryGetAliveReserveBuddy(battleManager, attacker, out CharacterInstance reserveBuddy))
                multiplier += ResolveBuddyContribution(attacker, reserveBuddy, target);

            if (comboParticipants != null)
            {
                for (int i = 0; i < comboParticipants.Count; i++)
                {
                    CharacterInstance contributor = comboParticipants[i];
                    if (contributor == null || contributor == attacker || !contributor.IsAlive)
                        continue;

                    multiplier += ResolveComboContribution(attacker, contributor, target);
                }
            }

            return Mathf.Max(1f, multiplier);
        }

        public static float GetCriticalChance(CharacterInstance attacker, float criticalRateMultiplier)
        {
            float baseChance = ResolveBaseCriticalChance(attacker);
            float scaledChance = baseChance * Mathf.Max(1f, criticalRateMultiplier);
            return Mathf.Clamp(scaledChance, 0f, MaxCriticalChance);
        }

        private static float ResolveBaseCriticalChance(CharacterInstance attacker)
        {
            MartialArtsType weaponType = attacker != null && attacker.Definition != null
                ? attacker.Definition.MartialArtsType
                : MartialArtsType.Sword;

            switch (weaponType)
            {
                case MartialArtsType.DualDaggers:
                    return 0.14f;
                case MartialArtsType.Unarmed:
                    return 0.12f;
                case MartialArtsType.Bow:
                    return 0.11f;
                case MartialArtsType.Sword:
                    return 0.10f;
                case MartialArtsType.Spear:
                    return 0.09f;
                case MartialArtsType.Staff:
                    return 0.08f;
                case MartialArtsType.HeavyWeapons:
                    return 0.07f;
                default:
                    return NeutralCriticalChance;
            }
        }

        private static float ResolveCriticalDamageMultiplier(CharacterInstance attacker)
        {
            MartialArtsType weaponType = attacker != null && attacker.Definition != null
                ? attacker.Definition.MartialArtsType
                : MartialArtsType.Sword;

            switch (weaponType)
            {
                case MartialArtsType.HeavyWeapons:
                    return 1.8f;
                case MartialArtsType.DualDaggers:
                    return 1.75f;
                case MartialArtsType.Unarmed:
                    return 1.7f;
                case MartialArtsType.Bow:
                    return 1.65f;
                case MartialArtsType.Sword:
                    return 1.6f;
                case MartialArtsType.Spear:
                    return 1.55f;
                case MartialArtsType.Staff:
                    return 1.5f;
                default:
                    return 1.6f;
            }
        }

        private static float ResolveComboContribution(CharacterInstance recipient, CharacterInstance contributor, CharacterInstance target)
        {
            float contribution = ResolveComboContributionBase(contributor) + ResolveWeaponAffinityAdjustment(contributor, target);

            if (HasSharedSynergyTag(recipient, contributor))
                contribution += SharedSynergyCritBonus;

            if (recipient != null
                && contributor != null
                && recipient.Definition != null
                && contributor.Definition != null
                && recipient.Definition.MartialArtsType == contributor.Definition.MartialArtsType)
            {
                contribution += SameWeaponFamilyCritBonus;
            }

            return Mathf.Max(MinimumSupportContribution, contribution);
        }

        private static float ResolveBuddyContribution(CharacterInstance recipient, CharacterInstance reserveBuddy, CharacterInstance target)
        {
            float contribution = ResolveBuddyContributionBase(reserveBuddy) + ResolveWeaponAffinityAdjustment(reserveBuddy, target);

            if (HasSharedSynergyTag(recipient, reserveBuddy))
                contribution += SharedSynergyCritBonus;

            return Mathf.Max(MinimumSupportContribution, contribution);
        }

        private static float ResolveComboContributionBase(CharacterInstance contributor)
        {
            MartialArtsType weaponType = contributor != null && contributor.Definition != null
                ? contributor.Definition.MartialArtsType
                : MartialArtsType.Sword;

            switch (weaponType)
            {
                case MartialArtsType.DualDaggers:
                    return 1.0f;
                case MartialArtsType.Unarmed:
                    return 0.9f;
                case MartialArtsType.Staff:
                    return 0.85f;
                case MartialArtsType.Sword:
                    return 0.8f;
                case MartialArtsType.Spear:
                    return 0.75f;
                case MartialArtsType.Bow:
                    return 0.7f;
                case MartialArtsType.HeavyWeapons:
                    return 0.65f;
                default:
                    return 0.8f;
            }
        }

        private static float ResolveBuddyContributionBase(CharacterInstance reserveBuddy)
        {
            MartialArtsType weaponType = reserveBuddy != null && reserveBuddy.Definition != null
                ? reserveBuddy.Definition.MartialArtsType
                : MartialArtsType.Sword;

            switch (weaponType)
            {
                case MartialArtsType.Staff:
                    return 0.7f;
                case MartialArtsType.DualDaggers:
                    return 0.65f;
                case MartialArtsType.Unarmed:
                    return 0.6f;
                case MartialArtsType.Sword:
                    return 0.6f;
                case MartialArtsType.Spear:
                    return 0.55f;
                case MartialArtsType.Bow:
                    return 0.55f;
                case MartialArtsType.HeavyWeapons:
                    return 0.45f;
                default:
                    return 0.6f;
            }
        }

        private static float ResolveWeaponAffinityAdjustment(CharacterInstance contributor, CharacterInstance target)
        {
            if (contributor == null || target == null || contributor.Definition == null || target.Definition == null)
                return 0f;

            float effectiveness = contributor.Stats.GetMartialArtsEffectiveness(
                contributor.Definition.MartialArtsType,
                target.Definition.MartialArtsType);

            if (effectiveness > 1f)
                return WeaponAdvantageCritBonus;
            if (effectiveness < 1f)
                return -WeaponDisadvantageCritPenalty;
            return 0f;
        }

        private static bool TryGetAliveReserveBuddy(BattleManager battleManager, CharacterInstance attacker, out CharacterInstance reserveBuddy)
        {
            reserveBuddy = null;
            if (battleManager == null || attacker == null)
                return false;

            int laneIndex = battleManager.GetPlayerLaneForCharacter(attacker);
            if (laneIndex < 0)
                return false;

            CharacterInstance activeAtLane = battleManager.GetActivePlayerAtLane(laneIndex);
            CharacterInstance reserveAtLane = battleManager.GetReservePlayerAtLane(laneIndex);
            reserveBuddy = activeAtLane == attacker ? reserveAtLane : activeAtLane;

            if (reserveBuddy == null || reserveBuddy == attacker || !reserveBuddy.IsAlive)
            {
                reserveBuddy = null;
                return false;
            }

            return true;
        }

        private static bool HasSharedSynergyTag(CharacterInstance left, CharacterInstance right)
        {
            if (left == null || right == null || left.Definition == null || right.Definition == null)
                return false;

            IReadOnlyList<string> leftTags = left.Definition.SynergyTags;
            IReadOnlyList<string> rightTags = right.Definition.SynergyTags;
            if (leftTags == null || rightTags == null || leftTags.Count == 0 || rightTags.Count == 0)
                return false;

            for (int i = 0; i < leftTags.Count; i++)
            {
                string leftTag = leftTags[i];
                if (string.IsNullOrWhiteSpace(leftTag))
                    continue;

                for (int j = 0; j < rightTags.Count; j++)
                {
                    string rightTag = rightTags[j];
                    if (string.IsNullOrWhiteSpace(rightTag))
                        continue;

                    if (string.Equals(leftTag, rightTag, System.StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
    }
}
