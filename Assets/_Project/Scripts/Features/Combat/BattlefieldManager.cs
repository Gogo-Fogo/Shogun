// BattlefieldManager.cs
// Manages free-form movement and positioning of characters on the battlefield.
// Designed for extensibility: supports obstacles, terrain effects, area-of-effect skills, and more.
// Provides methods for moving characters, checking valid positions, and querying nearby units.
// Integrates with the combat system for movement and positioning logic.

using System.Collections.Generic;
using UnityEngine;
using Shogun.Features.Characters;

namespace Shogun.Features.Combat
{
    public class BattlefieldManager : MonoBehaviour
    {
        private readonly Dictionary<CharacterInstance, Vector2> characterPositions = new Dictionary<CharacterInstance, Vector2>();
        private readonly List<Vector2> obstacles = new List<Vector2>(); // For future use
        private float battlefieldWidth = 10f;
        private float battlefieldHeight = 6f;

        public void Initialize(float width, float height)
        {
            battlefieldWidth = width;
            battlefieldHeight = height;
            characterPositions.Clear();
            obstacles.Clear();
        }

        public void PlaceCharacter(CharacterInstance character, Vector2 position)
        {
            characterPositions[character] = ClampToBattlefield(position);
        }

        public void MoveCharacter(CharacterInstance character, Vector2 newPosition)
        {
            if (!characterPositions.ContainsKey(character)) return;
            characterPositions[character] = ClampToBattlefield(newPosition);
        }

        public Vector2 GetCharacterPosition(CharacterInstance character)
        {
            return characterPositions.TryGetValue(character, out var pos) ? pos : Vector2.zero;
        }

        public List<CharacterInstance> GetCharactersInRange(Vector2 center, float radius)
        {
            var result = new List<CharacterInstance>();
            foreach (var kvp in characterPositions)
            {
                if (Vector2.Distance(kvp.Value, center) <= radius)
                    result.Add(kvp.Key);
            }
            return result;
        }

        public bool IsPositionValid(Vector2 position)
        {
            // For now, just check battlefield bounds. Extend for obstacles/terrain later.
            return position.x >= 0 && position.x <= battlefieldWidth && position.y >= 0 && position.y <= battlefieldHeight;
        }

        private Vector2 ClampToBattlefield(Vector2 pos)
        {
            return new Vector2(
                Mathf.Clamp(pos.x, 0, battlefieldWidth),
                Mathf.Clamp(pos.y, 0, battlefieldHeight)
            );
        }
    }
} 