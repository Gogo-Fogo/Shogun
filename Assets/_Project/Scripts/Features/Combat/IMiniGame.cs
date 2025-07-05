// IMiniGame.cs
// Interface for skill-based mini-games in combat (offensive and defensive).
// Implemented by OffensiveMiniGames and DefensiveMiniGames.

namespace Shogun.Features.Combat
{
    public interface IMiniGame
    {
        void StartGame(System.Action<bool> onComplete);
    }
} 