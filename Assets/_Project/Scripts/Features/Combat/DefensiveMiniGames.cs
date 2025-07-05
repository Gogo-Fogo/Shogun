// DefensiveMiniGames.cs
// Implements IMiniGame for defense execution mini-games (e.g., block timing, dodge, QTEs).
// Provides a simple example mini-game and is designed for extensibility.

using UnityEngine;
using System.Collections;

namespace Shogun.Features.Combat
{
    public class DefensiveMiniGames : MonoBehaviour, IMiniGame
    {
        public void StartGame(System.Action<bool> onComplete)
        {
            // Example: block timing mini-game
            StartCoroutine(BlockTimingMiniGame(onComplete));
        }

        private IEnumerator BlockTimingMiniGame(System.Action<bool> onComplete)
        {
            float blockWindow = Random.Range(1f, 2f);
            float timer = 0f;
            bool blocked = false;

            Debug.Log($"Block within {blockWindow:F2} seconds!");

            while (timer < 3f && !blocked)
            {
                timer += Time.deltaTime;
                if (Input.GetMouseButtonDown(1)) // Right-click for block
                {
                    blocked = true;
                    bool success = timer <= blockWindow;
                    Debug.Log(success ? "Block Success!" : "Block Failed!");
                    onComplete?.Invoke(success);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("Block Failed! (Timeout)");
            onComplete?.Invoke(false);
        }
    }
} 