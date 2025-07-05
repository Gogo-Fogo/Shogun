using UnityEngine;

// SafeAreaHandler.cs
// Utility MonoBehaviour for handling device safe areas (e.g., notches, rounded corners) on mobile devices.
// Adjusts UI RectTransforms to fit within the safe area of the screen.

[RequireComponent(typeof(RectTransform))]
public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
    private ScreenOrientation _lastOrientation = ScreenOrientation.Portrait;
    private Vector2Int _lastResolution = Vector2Int.zero;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != _lastSafeArea || 
            Screen.orientation != _lastOrientation ||
            new Vector2Int(Screen.width, Screen.height) != _lastResolution)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        _lastSafeArea = safeArea;
        _lastOrientation = Screen.orientation;
        _lastResolution = new Vector2Int(Screen.width, Screen.height);

        // Convert safe area Rect from absolute pixels to normalized anchor coordinates
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
}
