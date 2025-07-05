// AppFrameRate.cs
// Utility MonoBehaviour for setting and managing the application's target frame rate.
// Attach to a GameObject in the initial scene to enforce consistent frame timing.

using UnityEngine;

public class AppFrameRate : MonoBehaviour
{
    [Header("Set your desired FPS here")]
    public int targetFrameRate = 120;

    private static AppFrameRate instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }
}
