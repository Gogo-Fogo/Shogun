using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Shogun.Features.Characters;
using System.Collections;

namespace Shogun.Features.Combat
{
    [RequireComponent(typeof(Image))]
    public class BattleDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("References")]
        public TurnManager turnManager;
        [Header("Settings")]
        public float dragThreshold = 10f;
        public float dragSmoothTime = 0.08f;
        public float gridSize = 1f;
        public bool snapToGrid = true;
        public float tapMoveSpeed = 50f;
        private const float tapTimeThreshold = 0.2f;
        private const float tapMoveThreshold = 20f;

        private Camera mainCamera;
        private Vector2 pointerDownPos;
        private float pointerDownTime;
        private bool isDragging = false;
        private Vector3 dragTargetWorldPos;
        private Vector3 dragVelocity = Vector3.zero;
        private CharacterInstance draggingCharacter = null;
        private Animator characterAnimator = null;
        private Coroutine tapMoveCoroutine = null;

        void Awake()
        {
            mainCamera = Camera.main;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownPos = eventData.position;
            pointerDownTime = Time.unscaledTime;
            isDragging = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            float heldTime = Time.unscaledTime - pointerDownTime;
            float movedDist = Vector2.Distance(eventData.position, pointerDownPos);
            
            // CLICK/TAP: Run to position
            if (!isDragging && heldTime < tapTimeThreshold && movedDist < tapMoveThreshold)
            {
                StartTapMove(eventData.position);
            }
            // HOLD END: Snap to final position
            else if (isDragging && draggingCharacter != null)
            {
                Vector3 pointerWorld = GetPointerWorld(eventData.position, draggingCharacter.transform);
                Vector3 finalTargetPos = snapToGrid ? SnapToGrid(pointerWorld) : pointerWorld;
                SetCharacterPosition(draggingCharacter.transform, finalTargetPos);
                if (characterAnimator != null) characterAnimator.SetBool("isRunning", false);
            }
            
            isDragging = false;
            draggingCharacter = null;
            characterAnimator = null;
            dragVelocity = Vector3.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (turnManager == null) return;
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null) return;
            
            // HOLD: Start dragging and teleport character under finger
            if (!isDragging)
            {
                if (tapMoveCoroutine != null) { StopCoroutine(tapMoveCoroutine); tapMoveCoroutine = null; }
                isDragging = true;
                draggingCharacter = currentCharacter;
                characterAnimator = currentCharacter.GetComponentInChildren<Animator>();
                
                // INSTANT teleport under finger
                Transform charTransform = currentCharacter.transform;
                Vector3 pointerWorldPos = GetPointerWorld(eventData.position, charTransform);
                SetCharacterPosition(charTransform, pointerWorldPos);
                if (characterAnimator != null) characterAnimator.SetBool("isRunning", false);
                dragVelocity = Vector3.zero;
            }
            
            // Update drag target
            if (isDragging && draggingCharacter != null)
            {
                Transform charTransform = currentCharacter.transform;
                Vector3 pointerWorldPos = GetPointerWorld(eventData.position, charTransform);
                dragTargetWorldPos = pointerWorldPos;
            }
        }

        void Update()
        {
            if (isDragging && draggingCharacter != null)
            {
                Transform charTransform = draggingCharacter.transform;
                Vector3 before = GetCharacterPosition(charTransform);
                Vector3 newPos = Vector3.SmoothDamp(before, dragTargetWorldPos, ref dragVelocity, dragSmoothTime);
                SetCharacterPosition(charTransform, newPos);
            }
        }

        private void StartTapMove(Vector2 screenPosition)
        {
            if (turnManager == null) return;
            var currentCharacter = turnManager.GetCurrentCharacter();
            if (currentCharacter == null) return;
            if (tapMoveCoroutine != null) StopCoroutine(tapMoveCoroutine);
            Vector3 worldPos = GetPointerWorld(screenPosition, currentCharacter.transform);
            if (snapToGrid) worldPos = SnapToGrid(worldPos);
            tapMoveCoroutine = StartCoroutine(SmoothMoveToPosition(currentCharacter, worldPos));
        }

        private IEnumerator SmoothMoveToPosition(Component character, Vector3 targetWorldPos)
        {
            Transform charTransform = character.transform;
            Transform charParent = charTransform.parent;
            Animator anim = character.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetBool("isRunning", true);

            if (charParent)
            {
                Vector3 startLocal = charTransform.localPosition;
                Vector3 targetLocal = charParent.InverseTransformPoint(targetWorldPos);
                float totalDist = Vector3.Distance(startLocal, targetLocal);
                float minDuration = 0.1f;
                float duration = Mathf.Max(minDuration, totalDist / tapMoveSpeed);
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    Vector3 nextLocal = Vector3.Lerp(startLocal, targetLocal, t);
                    charTransform.localPosition = nextLocal;
                    yield return null;
                }
                charTransform.localPosition = targetLocal;
            }
            else
            {
                Vector3 startWorld = charTransform.position;
                float totalDist = Vector3.Distance(startWorld, targetWorldPos);
                float minDuration = 0.1f;
                float duration = Mathf.Max(minDuration, totalDist / tapMoveSpeed);
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    Vector3 nextWorld = Vector3.Lerp(startWorld, targetWorldPos, t);
                    charTransform.position = nextWorld;
                    yield return null;
                }
                charTransform.position = targetWorldPos;
            }
            if (anim != null) anim.SetBool("isRunning", false);
            tapMoveCoroutine = null;
        }

        private Vector3 GetPointerWorld(Vector2 screenPosition, Transform charTransform)
        {
            Transform charParent = charTransform.parent;
            float z = charParent ? Mathf.Abs(mainCamera.transform.position.z - charParent.position.z) : Mathf.Abs(mainCamera.transform.position.z - charTransform.position.z);
            Vector3 pointerWorld = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, z));
            pointerWorld.z = charTransform.position.z;
            return pointerWorld;
        }

        private Vector3 SnapToGrid(Vector3 worldPosition)
        {
            return new Vector3(
                Mathf.Round(worldPosition.x / gridSize) * gridSize,
                Mathf.Round(worldPosition.y / gridSize) * gridSize,
                worldPosition.z
            );
        }

        private void SetCharacterPosition(Transform charTransform, Vector3 worldPos)
        {
            Transform charParent = charTransform.parent;
            if (charParent)
                charTransform.localPosition = charParent.InverseTransformPoint(worldPos);
            else
                charTransform.position = worldPos;
        }

        private Vector3 GetCharacterPosition(Transform charTransform)
        {
            Transform charParent = charTransform.parent;
            if (charParent)
                return charParent.TransformPoint(charTransform.localPosition);
            else
                return charTransform.position;
        }
    }
} 