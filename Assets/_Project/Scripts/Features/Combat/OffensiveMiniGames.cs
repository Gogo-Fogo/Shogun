// OffensiveMiniGames.cs
// Implements IMiniGame for attack execution mini-games (e.g., timing, quick-tap, swipe challenges).
// Provides a simple example mini-game and is designed for extensibility.

using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // New Input System

namespace Shogun.Features.Combat
{
    public class OffensiveMiniGames : MonoBehaviour, IMiniGame
    {
        [SerializeField] private InputActionReference tapAction;
        private System.Action<bool> onCompleteCallback;
        private float targetTime;
        private float timer;
        private bool tapped;
        private Coroutine miniGameCoroutine;

        public void StartGame(System.Action<bool> onComplete)
        {
            onCompleteCallback = onComplete;
            miniGameCoroutine = StartCoroutine(TimingTapMiniGame());
            if (tapAction != null)
                tapAction.action.performed += OnTapPerformed;
            if (tapAction != null)
                tapAction.action.Enable();
        }

        private IEnumerator TimingTapMiniGame()
        {
            targetTime = Random.Range(1f, 3f);
            timer = 0f;
            tapped = false;

            Debug.Log($"Tap when the timer reaches {targetTime:F2} seconds!");

            while (timer < 5f && !tapped)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (!tapped)
            {
                Debug.Log("Failed! (Timeout)");
                onCompleteCallback?.Invoke(false);
            }
            Cleanup();
        }

        private void OnTapPerformed(InputAction.CallbackContext ctx)
        {
            if (!tapped && miniGameCoroutine != null)
            {
                tapped = true;
                float error = Mathf.Abs(timer - targetTime);
                bool success = error < 0.3f;
                Debug.Log(success ? "Success!" : "Failed!");
                onCompleteCallback?.Invoke(success);
                StopCoroutine(miniGameCoroutine);
                Cleanup();
            }
        }

        private void Cleanup()
        {
            if (tapAction != null)
                tapAction.action.performed -= OnTapPerformed;
            if (tapAction != null)
                tapAction.action.Disable();
            miniGameCoroutine = null;
        }
    }
} 