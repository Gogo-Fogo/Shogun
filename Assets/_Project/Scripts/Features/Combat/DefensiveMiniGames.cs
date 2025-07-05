// DefensiveMiniGames.cs
// Implements IMiniGame for defense execution mini-games (e.g., block timing, dodge, QTEs).
// Provides a simple example mini-game and is designed for extensibility.

using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // New Input System

namespace Shogun.Features.Combat
{
    public class DefensiveMiniGames : MonoBehaviour, IMiniGame
    {
        [SerializeField] private InputActionReference blockAction;
        private System.Action<bool> onCompleteCallback;
        private float blockWindow;
        private float timer;
        private bool blocked;
        private Coroutine miniGameCoroutine;

        public void StartGame(System.Action<bool> onComplete)
        {
            onCompleteCallback = onComplete;
            miniGameCoroutine = StartCoroutine(BlockTimingMiniGame());
            if (blockAction != null)
                blockAction.action.performed += OnBlockPerformed;
            if (blockAction != null)
                blockAction.action.Enable();
        }

        private IEnumerator BlockTimingMiniGame()
        {
            blockWindow = Random.Range(1f, 2f);
            timer = 0f;
            blocked = false;

            Debug.Log($"Block within {blockWindow:F2} seconds!");

            while (timer < 3f && !blocked)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (!blocked)
            {
                Debug.Log("Block Failed! (Timeout)");
                onCompleteCallback?.Invoke(false);
            }
            Cleanup();
        }

        private void OnBlockPerformed(InputAction.CallbackContext ctx)
        {
            if (!blocked && miniGameCoroutine != null)
            {
                blocked = true;
                bool success = timer <= blockWindow;
                Debug.Log(success ? "Block Success!" : "Block Failed!");
                onCompleteCallback?.Invoke(success);
                StopCoroutine(miniGameCoroutine);
                Cleanup();
            }
        }

        private void Cleanup()
        {
            if (blockAction != null)
                blockAction.action.performed -= OnBlockPerformed;
            if (blockAction != null)
                blockAction.action.Disable();
            miniGameCoroutine = null;
        }
    }
} 