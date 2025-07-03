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
