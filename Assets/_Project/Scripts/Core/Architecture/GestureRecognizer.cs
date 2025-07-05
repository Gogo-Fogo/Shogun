// GestureRecognizer.cs
// MonoBehaviour for detecting tap, swipe, and hold gestures on screen or UI elements.
// Exposes UnityEvents for tap, swipe, and hold, and provides public methods for manual invocation (for testing).
// Designed for use with the new Input System but can be extended for legacy input.

using UnityEngine;
using UnityEngine.Events;

namespace Shogun.Core.Architecture
{
    public class GestureRecognizer : MonoBehaviour
    {
        public UnityEvent<Vector2> OnTap;
        public UnityEvent<Vector2, Vector2> OnSwipe; // start, end
        public UnityEvent<Vector2, float> OnHold; // position, duration

        // For demonstration: simple mouse/touch detection (expand for full input system integration)
        private Vector2 touchStart;
        private float holdTime;
        private bool isHolding;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStart = Input.mousePosition;
                holdTime = 0f;
                isHolding = true;
            }
            if (Input.GetMouseButton(0) && isHolding)
            {
                holdTime += Time.deltaTime;
            }
            if (Input.GetMouseButtonUp(0) && isHolding)
            {
                Vector2 touchEnd = Input.mousePosition;
                if (holdTime > 0.5f)
                {
                    OnHold?.Invoke(touchStart, holdTime);
                }
                else if (Vector2.Distance(touchStart, touchEnd) > 50f)
                {
                    OnSwipe?.Invoke(touchStart, touchEnd);
                }
                else
                {
                    OnTap?.Invoke(touchEnd);
                }
                isHolding = false;
            }
        }

        // Public methods for manual invocation (for testing)
        public void SimulateTap(Vector2 pos) => OnTap?.Invoke(pos);
        public void SimulateSwipe(Vector2 start, Vector2 end) => OnSwipe?.Invoke(start, end);
        public void SimulateHold(Vector2 pos, float duration) => OnHold?.Invoke(pos, duration);
    }
} 