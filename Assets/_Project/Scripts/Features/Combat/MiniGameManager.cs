// MiniGameManager.cs
// MonoBehaviour to manage the flow and state of skill-based mini-games during combat.
// Provides events for mini-game start, end, and result. Designed for extensibility to support different mini-game types.
// Integrates with OffensiveMiniGames and DefensiveMiniGames for attack/defense challenges.

using UnityEngine;
using UnityEngine.Events;

namespace Shogun.Features.Combat
{
    public class MiniGameManager : MonoBehaviour
    {
        public UnityEvent OnMiniGameStarted;
        public UnityEvent OnMiniGameEnded;
        public UnityEvent<bool> OnMiniGameResult; // true = success, false = fail

        private IMiniGame currentMiniGame;

        public void StartMiniGame(IMiniGame miniGame)
        {
            currentMiniGame = miniGame;
            OnMiniGameStarted?.Invoke();
            currentMiniGame.StartGame(OnMiniGameCompleted);
        }

        private void OnMiniGameCompleted(bool success)
        {
            OnMiniGameResult?.Invoke(success);
            OnMiniGameEnded?.Invoke();
            currentMiniGame = null;
        }
    }
} 