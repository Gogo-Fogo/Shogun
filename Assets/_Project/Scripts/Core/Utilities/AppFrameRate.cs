// AppFrameRate.cs
// Utility MonoBehaviour for applying the current saved frame-rate target at startup.

using Shogun.Core;
using UnityEngine;

public class AppFrameRate : MonoBehaviour
{
    [Header("Fallback FPS if saved settings are unavailable")]
    public int targetFrameRate = 60;

    private static AppFrameRate instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            GameSettingsService.ApplyRuntimeSettings(targetFrameRate);
            targetFrameRate = GameSettingsService.GetSnapshot().TargetFrameRate;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}