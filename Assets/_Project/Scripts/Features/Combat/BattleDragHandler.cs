using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shogun.Features.Combat
{
    [RequireComponent(typeof(Image))]
    public class BattleDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("References")]
        public TurnManager turnManager; // Assign in Inspector
        
        [Header("Settings")]
        public float dragThreshold = 10f; // pixels
        public bool enableDebugLogs = true;
        
        [Header("Movement")]
        public bool snapToGrid = true;
        public float gridSize = 1f;
        
        private bool isDragging = false;
        private Vector2 dragStartPos;
        private Camera mainCamera;
        private Image panelImage;
        private bool isInitialized = false;
        private float dragFollowLerpSpeed = 20f; // Increased from 10f to 20f for faster following
        private Vector3? dragTargetWorldPos = null;

        void Awake()
        {
            Initialize();
        }

        void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (isInitialized) return;
            
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("BattleDragHandler: No main camera found!");
                return;
            }

            panelImage = GetComponent<Image>();
            if (panelImage == null)
            {
                Debug.LogError("BattleDragHandler: No Image component found! This script requires an Image component.");
                return;
            }

            // Ensure the panel is set up correctly for input
            panelImage.raycastTarget = true;
            panelImage.color = new Color(0, 0, 0, 0); // Transparent but still raycastable
            
            // Get or add GraphicRaycaster if needed
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            isInitialized = true;
            LogDebug("BattleDragHandler initialized successfully");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInitialized) return;
            
            LogDebug($"PointerDown: {eventData.position}");
            dragStartPos = eventData.position;
            isDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("OnDrag called");
            if (!isInitialized) return;
            LogDebug($"Drag: {eventData.position}");
            if (turnManager == null)
            {
                LogDebug("TurnManager is null, cannot process drag");
                return;
            }
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null)
            {
                LogDebug("No current character, cannot process drag");
                return;
            }
            if (!isDragging && Vector2.Distance(eventData.position, dragStartPos) > dragThreshold)
            {
                isDragging = true;
                LogDebug("Drag threshold exceeded, starting drag");
                // Set run animation ON
                var anim = currentCharacter.GetComponent<Animator>();
                if (anim != null) 
                {
                    anim.SetBool("isRunning", true);
                    LogDebug("Set isRunning = true for drag start");
                }
            }
            if (isDragging)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
                worldPos.z = 0;
                if (snapToGrid)
                {
                    worldPos = SnapToGrid(worldPos);
                }
                dragTargetWorldPos = worldPos;
                LogDebug($"Set drag target to: {worldPos}");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isInitialized) return;
            LogDebug($"PointerUp: {eventData.position}");
            if (!isDragging)
            {
                // This was a tap, call tap-to-move logic
                OnTap(eventData.position);
            }
            else
            {
                // Snap to final drag position
                if (turnManager != null)
                {
                    var currentCharacter = turnManager.GetCurrentCharacter();
                    if (currentCharacter != null && dragTargetWorldPos.HasValue)
                    {
                        currentCharacter.transform.position = dragTargetWorldPos.Value;
                        // Set run animation OFF
                        var anim = currentCharacter.GetComponent<Animator>();
                        if (anim != null) 
                        {
                            anim.SetBool("isRunning", false);
                            LogDebug("Set isRunning = false for drag end");
                        }
                    }
                }
            }
            isDragging = false;
            dragTargetWorldPos = null;
        }

        private void OnTap(Vector2 screenPosition)
        {
            if (turnManager == null)
            {
                LogDebug("TurnManager is null, cannot process tap");
                return;
            }
            
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null)
            {
                LogDebug("No current character, cannot process tap");
                return;
            }

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPos.z = 0;
            
            if (snapToGrid)
            {
                worldPos = SnapToGrid(worldPos);
            }
            
            // Use the character's MoveTo method which handles run animation properly
            currentCharacter.MoveTo(screenPosition);
            LogDebug($"Tapped to move character to: {worldPos}");
        }

        private Vector3 SnapToGrid(Vector3 worldPosition)
        {
            return new Vector3(
                Mathf.Round(worldPosition.x / gridSize) * gridSize,
                Mathf.Round(worldPosition.y / gridSize) * gridSize,
                0
            );
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BattleDragHandler] {message}");
            }
        }

        // Public method to test if the handler is working
        public void TestInput()
        {
            LogDebug("Test input called - handler is working!");
        }

        void Update()
        {
            // If dragging, smoothly move the character toward the drag target
            if (isDragging && dragTargetWorldPos.HasValue && turnManager != null)
            {
                var currentCharacter = turnManager.GetCurrentCharacter();
                if (currentCharacter != null)
                {
                    var t = currentCharacter.transform;
                    t.position = Vector3.MoveTowards(t.position, dragTargetWorldPos.Value, dragFollowLerpSpeed * Time.deltaTime);
                }
            }
        }
    }
} 