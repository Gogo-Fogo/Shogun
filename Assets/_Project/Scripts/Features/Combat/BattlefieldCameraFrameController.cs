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

        private static bool s_BootstrapRegistered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetBootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoadedBootstrap;
            s_BootstrapRegistered = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterBootstrap()
        {
            if (s_BootstrapRegistered)
                return;

            SceneManager.sceneLoaded += HandleSceneLoadedBootstrap;
            s_BootstrapRegistered = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            EnsureExists(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoadedBootstrap(Scene scene, LoadSceneMode _)
        {
            EnsureExists(scene);
        }

        private static void EnsureExists(Scene scene)
        {
            if (!scene.IsValid() || !TryFindTurnManager(scene))
                return;

            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera cameraRef = cameras[i];
                if (cameraRef == null || cameraRef.gameObject.scene != scene || !cameraRef.CompareTag("MainCamera"))
                    continue;

                if (cameraRef.GetComponent<BattlefieldCameraFrameController>() == null)
                    cameraRef.gameObject.AddComponent<BattlefieldCameraFrameController>();
                return;
            }
        }

        private static bool TryFindTurnManager(Scene scene)
        {
            TurnManager[] managers = FindObjectsByType<TurnManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < managers.Length; i++)
            {
                TurnManager manager = managers[i];
                if (manager != null && manager.gameObject.scene == scene)
                    return true;
            }

            return false;
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
