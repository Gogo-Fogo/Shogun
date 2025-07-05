// OffensiveMiniGames.cs
// Implements IMiniGame for attack execution mini-games (e.g., timing, quick-tap, swipe challenges).
// Provides a simple example mini-game and is designed for extensibility.

using UnityEngine;
using System.Collections;

namespace Shogun.Features.Combat
{
    public class OffensiveMiniGames : MonoBehaviour, IMiniGame
    {
        public void StartGame(System.Action<bool> onComplete)
        {
            // Example: timing-based tap mini-game
            StartCoroutine(TimingTapMiniGame(onComplete));
        }

        private IEnumerator TimingTapMiniGame(System.Action<bool> onComplete)
        {
            float targetTime = Random.Range(1f, 3f);
            float timer = 0f;
            bool tapped = false;

            Debug.Log($"Tap when the timer reaches {targetTime:F2} seconds!");

            while (timer < 5f && !tapped)
            {
                timer += Time.deltaTime;
                if (Input.GetMouseButtonDown(0))
                {
                    tapped = true;
                    float error = Mathf.Abs(timer - targetTime);
                    bool success = error < 0.3f;
                    Debug.Log(success ? "Success!" : "Failed!");
                    onComplete?.Invoke(success);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("Failed! (Timeout)");
            onComplete?.Invoke(false);
        }
    }
} 