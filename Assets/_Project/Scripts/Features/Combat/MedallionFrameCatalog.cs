using UnityEngine;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Maps a character's SpecialChargeRequirement (hole count) to the correct
    /// medallion frame sprite. Assign in the Inspector on BattleHudController.
    /// Source art lives in Art/Source/Gemini/UI/HUD/.
    /// Production art lives in Art/Production/UI/HUD/.
    /// </summary>
    [CreateAssetMenu(menuName = "Shogun/Combat/Medallion Frame Catalog", fileName = "MedallionFrameCatalog")]
    public sealed class MedallionFrameCatalog : ScriptableObject
    {
        [System.Serializable]
        private struct Entry
        {
            [Tooltip("Number of charge holes this frame has (matches SpecialAbilityChargeRequirement).")]
            public int holeCount;
            [Tooltip("The medallion frame sprite for this hole count.")]
            public Sprite frame;
        }

        [SerializeField]
        [Tooltip("One entry per supported hole count (2, 3, 4, 5, 6, 8, 12 …).")]
        private Entry[] entries = System.Array.Empty<Entry>();

        [SerializeField]
        [Tooltip("Returned when no entry matches the requested hole count.")]
        private Sprite fallbackFrame;

        /// <summary>
        /// Returns the frame sprite for the given hole count, or the fallback if not found.
        /// </summary>
        public Sprite GetFrame(int holeCount)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].holeCount == holeCount && entries[i].frame != null)
                    return entries[i].frame;
            }
            return fallbackFrame;
        }
    }
}
