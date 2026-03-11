using System.Text;
using UnityEngine;

namespace Shogun.Features.Characters
{
    [CreateAssetMenu(fileName = "New Ability Definition", menuName = "Shogun/Characters/Ability Definition")]
    public class AbilityDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string abilityId = "";
        [SerializeField] private string authorCharacterId = "";
        [SerializeField] private string displayName = "New Ability";
        [SerializeField] [TextArea(2, 4)] private string description = "";
        [SerializeField] private AbilitySlot slot = AbilitySlot.Special;

        [Header("Combat")]
        [SerializeField] private AbilityTargetingType targeting = AbilityTargetingType.SingleEnemy;
        [SerializeField] private AbilityEffectType effectType = AbilityEffectType.Damage;
        [SerializeField] private int chargeRequirement = 3;
        [SerializeField] private float powerValue = 100f;
        [SerializeField] private StatusEffectType appliedStatusEffect = StatusEffectType.Stun;
        [SerializeField] private int appliedStatusDuration = 0;
        [SerializeField] private float appliedStatusValue = 0f;

        [Header("Presentation")]
        [SerializeField] private string animationTrigger = "SpecialTrigger";
        [SerializeField] private Color accentColor = Color.white;

        public string AbilityId => abilityId;
        public string AuthorCharacterId => authorCharacterId;
        public string DisplayName => displayName;
        public string Description => description;
        public AbilitySlot Slot => slot;
        public AbilityTargetingType Targeting => targeting;
        public AbilityEffectType EffectType => effectType;
        public int ChargeRequirement => Mathf.Max(1, chargeRequirement);
        public float PowerValue => powerValue;
        public StatusEffectType AppliedStatusEffect => appliedStatusEffect;
        public int AppliedStatusDuration => Mathf.Max(0, appliedStatusDuration);
        public float AppliedStatusValue => appliedStatusValue;
        public string AnimationTrigger => string.IsNullOrWhiteSpace(animationTrigger) ? "SpecialTrigger" : animationTrigger;
        public Color AccentColor => accentColor;
        public bool AppliesStatus => AppliedStatusDuration > 0;

#if UNITY_EDITOR
        private void OnValidate()
        {
            abilityId = NormalizeId(string.IsNullOrWhiteSpace(abilityId) ? displayName : abilityId);
            authorCharacterId = NormalizeId(authorCharacterId);
            chargeRequirement = Mathf.Max(1, chargeRequirement);
            appliedStatusDuration = Mathf.Max(0, appliedStatusDuration);
        }
#endif

        internal static string NormalizeId(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            StringBuilder builder = new StringBuilder(rawValue.Length);
            bool lastWasSeparator = false;
            foreach (char rawChar in rawValue.Trim().ToLowerInvariant())
            {
                if ((rawChar >= 'a' && rawChar <= 'z') || (rawChar >= '0' && rawChar <= '9'))
                {
                    builder.Append(rawChar);
                    lastWasSeparator = false;
                    continue;
                }

                if ((rawChar == ' ' || rawChar == '_' || rawChar == '-') && !lastWasSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                    lastWasSeparator = true;
                }
            }

            return builder.ToString().Trim('-');
        }
    }

    public enum AbilitySlot
    {
        Special = 0,
        Ultimate = 1,
        Passive = 2
    }

    public enum AbilityTargetingType
    {
        Self = 0,
        SingleEnemy = 1,
        AllEnemies = 2,
        SingleAlly = 3,
        AllAllies = 4,
        Battlefield = 5
    }

    public enum AbilityEffectType
    {
        Damage = 0,
        Heal = 1,
        ApplyStatus = 2,
        Cleanse = 3,
        Utility = 4
    }
}