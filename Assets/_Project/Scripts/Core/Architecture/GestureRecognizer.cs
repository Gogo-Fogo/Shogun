// GestureRecognizer.cs
// MonoBehaviour for detecting tap, swipe, and hold gestures on screen or UI elements.
// Exposes UnityEvents for tap, swipe, and hold, and provides public methods for manual invocation (for testing).
// Designed for use with the new Input System but can be extended for legacy input.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; // New Input System

namespace Shogun.Core.Architecture
{
    public class GestureRecognizer : MonoBehaviour
    {
        public UnityEvent<Vector2> OnTap;
        public UnityEvent<Vector2, Vector2> OnSwipe; // start, end
        public UnityEvent<Vector2, float> OnHold; // position, duration

        // New Input System
        [SerializeField] private InputActionReference tapAction;
        [SerializeField] private InputActionReference positionAction;
        [SerializeField] private InputActionReference holdAction; // Optional, for custom hold logic

        private Vector2 touchStart;
        private float holdTime;
        private bool isHolding;

        private void OnEnable()
        {
            if (tapAction != null)
                tapAction.action.performed += OnTapPerformed;
            if (positionAction != null)
                positionAction.action.performed += OnPositionPerformed;
            if (tapAction != null)
                tapAction.action.Enable();
            if (positionAction != null)
                positionAction.action.Enable();
        }

        private void OnDisable()
        {
            if (tapAction != null)
                tapAction.action.performed -= OnTapPerformed;
            if (positionAction != null)
                positionAction.action.performed -= OnPositionPerformed;
            if (tapAction != null)
                tapAction.action.Disable();
            if (positionAction != null)
                positionAction.action.Disable();
        }

        private void OnTapPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 pos = positionAction != null ? positionAction.action.ReadValue<Vector2>() : Vector2.zero;
            OnTap?.Invoke(pos);
        }

        private void OnPositionPerformed(InputAction.CallbackContext ctx)
        {
            if (isHolding)
            {
                holdTime += Time.deltaTime;
            }
        }

        // Public methods for manual invocation (for testing)
        public void SimulateTap(Vector2 pos) => OnTap?.Invoke(pos);
        public void SimulateSwipe(Vector2 start, Vector2 end) => OnSwipe?.Invoke(start, end);
        public void SimulateHold(Vector2 pos, float duration) => OnHold?.Invoke(pos, duration);
    }
} 