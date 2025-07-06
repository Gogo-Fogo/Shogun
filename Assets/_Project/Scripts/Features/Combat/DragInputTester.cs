using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Shogun.Features.Combat
{
    public class DragInputTester : MonoBehaviour
    {
        [Header("Test References")]
        public BattleDragHandler dragHandler;
        public Button testButton;
        
        [Header("Test Settings")]
        public bool autoTestOnStart = true;
        
        void Start()
        {
            if (autoTestOnStart)
            {
                TestSetup();
            }
            
            if (testButton != null)
            {
                testButton.onClick.AddListener(TestSetup);
            }
        }
        
        public void TestSetup()
        {
            Debug.Log("=== DRAG INPUT TEST ===");
            
            // Test 1: Check if drag handler exists
            if (dragHandler == null)
            {
                Debug.LogError("❌ BattleDragHandler is not assigned!");
                return;
            }
            Debug.Log("✅ BattleDragHandler found");
            
            // Test 2: Check if it has required components
            var image = dragHandler.GetComponent<Image>();
            if (image == null)
            {
                Debug.LogError("❌ BattleDragHandler missing Image component!");
                return;
            }
            Debug.Log("✅ Image component found");
            
            // Test 3: Check if it's raycastable
            if (!image.raycastTarget)
            {
                Debug.LogWarning("⚠️ Image raycastTarget is false - this may prevent input!");
            }
            else
            {
                Debug.Log("✅ Image is raycastable");
            }
            
            // Test 4: Check Canvas setup
            var canvas = dragHandler.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("❌ No Canvas found in parent hierarchy!");
                return;
            }
            Debug.Log($"✅ Canvas found: {canvas.name}");
            
            // Test 5: Check GraphicRaycaster
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError("❌ Canvas missing GraphicRaycaster!");
                return;
            }
            Debug.Log("✅ GraphicRaycaster found");
            
            // Test 6: Check EventSystem
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogError("❌ No EventSystem found in scene!");
                return;
            }
            Debug.Log("✅ EventSystem found");
            
            // Test 7: Check TurnManager reference
            if (dragHandler.turnManager == null)
            {
                Debug.LogWarning("⚠️ TurnManager not assigned - drag will work but won't move characters");
            }
            else
            {
                Debug.Log("✅ TurnManager assigned");
            }
            
            Debug.Log("=== TEST COMPLETE ===");
            Debug.Log("Try clicking and dragging on the screen now!");
        }
    }
} 