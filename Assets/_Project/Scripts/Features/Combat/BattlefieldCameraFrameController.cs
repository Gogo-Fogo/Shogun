using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shogun.Features.Combat
{
    /// <summary>
    /// Keeps the portrait battle camera framed tightly to the authored battlefield art.
    /// Safe-area handling remains a UI concern; the world camera should simply fit the arena.
    /// </summary>
    public sealed class BattlefieldCameraFrameController : MonoBehaviour
    {
        private const float DefaultPaddingWorldUnits = 0.08f;

        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform battlefieldRoot;
        [SerializeField] private float paddingWorldUnits = DefaultPaddingWorldUnits;
        [SerializeField] private bool refitContinuously = false;

        private SpriteRenderer battlefieldRenderer;
        private Vector2Int lastResolution = Vector2Int.zero;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (FindFirstObjectByType<TurnManager>() == null)
                return;

            Camera cameraRef = Camera.main;
            if (cameraRef == null)
                return;

            if (cameraRef.GetComponent<BattlefieldCameraFrameController>() != null)
                return;

            cameraRef.gameObject.AddComponent<BattlefieldCameraFrameController>();
        }

        private void Awake()
        {
            ResolveDependencies();
        }

        private void Start()
        {
            ApplyFrame();
        }

        private void LateUpdate()
        {
            Vector2Int currentResolution = new Vector2Int(Screen.width, Screen.height);
            if (refitContinuously || currentResolution != lastResolution)
                ApplyFrame();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            ResolveDependencies();
            ApplyFrame();
        }

        private void ResolveDependencies()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>() ?? Camera.main;

            if (battlefieldRoot == null)
            {
                GameObject battlefield = GameObject.Find("BackgroundBattlefield");
                battlefieldRoot = battlefield != null ? battlefield.transform : null;
            }

            if (battlefieldRoot != null)
                battlefieldRenderer = battlefieldRoot.GetComponentInChildren<SpriteRenderer>();
        }

        private void ApplyFrame()
        {
            ResolveDependencies();

            if (targetCamera == null || !targetCamera.orthographic || battlefieldRenderer == null)
                return;

            Bounds bounds = battlefieldRenderer.bounds;
            float halfWidth = bounds.extents.x + paddingWorldUnits;
            float halfHeight = bounds.extents.y + paddingWorldUnits;
            float aspect = Mathf.Max(0.01f, targetCamera.aspect);
            float requiredOrthoSize = Mathf.Max(halfHeight, halfWidth / aspect);

            lastResolution = new Vector2Int(Screen.width, Screen.height);

            Vector3 cameraPosition = targetCamera.transform.position;
            cameraPosition.x = bounds.center.x;
            cameraPosition.y = bounds.center.y;
            targetCamera.transform.position = cameraPosition;
            targetCamera.orthographicSize = requiredOrthoSize;
        }
    }
}
