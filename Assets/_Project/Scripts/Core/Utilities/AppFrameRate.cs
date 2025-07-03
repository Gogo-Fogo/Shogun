using UnityEngine;

public class AppFrameRate : MonoBehaviour
{
    [Header("Set your desired FPS here")]
    public int targetFrameRate = 120;

    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
        DontDestroyOnLoad(gameObject);
    }
}
