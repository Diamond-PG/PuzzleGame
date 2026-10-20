using UnityEngine;
using UnityEngine.UI;

public class CameraFollow : MonoBehaviour
{
    // ============================================================
    // 01 - PLAYER FOLLOW
    // ============================================================

    [Header("PLAYER FOLLOW")]
    [SerializeField] private Transform target;

    [SerializeField, Min(0.01f)]
    private float smoothTime = 0.15f;

    [SerializeField]
    private Vector3 offset = Vector3.zero;

    // ============================================================
    // 02 - CAMERA REFERENCES
    // ============================================================

    [Header("CAMERA REFERENCES")]
    [SerializeField] private Camera worldCamera;

    [SerializeField]
    private PrisonDarknessMask prisonDarknessMask;

    // ============================================================
    // 03 - TUTORIAL CAMERA POSITION
    // ============================================================

    [Header("TUTORIAL CAMERA POSITION")]

    [Tooltip("During tutorial the hero stays inside the prison window.")]
    [SerializeField]
    private bool useTutorialFraming = true;

    [Tooltip("Hero window position relative to the screen.")]
    [SerializeField]
    private Vector2 tutorialScreenPosition =
        new Vector2(0.208f, 0.8f);

    // ============================================================
    // 04 - NORMAL GAME CAMERA
    // ============================================================

    [Header("NORMAL GAME CAMERA")]

    [SerializeField, Min(0.01f)]
    private float normalSmoothTime = 0.35f;

    [SerializeField]
    private Vector2 normalScreenPosition =
        new Vector2(0.5f, 0.5f);

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private Vector3 velocity = Vector3.zero;

    private RawImage darknessImage;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (target == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;
        }

        if (worldCamera == null)
        {
            worldCamera =
                GetComponentInChildren<Camera>(true);
        }

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (prisonDarknessMask == null)
        {
            prisonDarknessMask =
                FindFirstObjectByType<PrisonDarknessMask>();
        }

        if (prisonDarknessMask != null)
        {
            darknessImage =
                prisonDarknessMask.GetComponent<RawImage>();
        }
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (target == null)
            return;

        bool tutorialActive = IsTutorialMaskVisible();

        transform.position =
            CalculateCameraPosition(tutorialActive);

        velocity = Vector3.zero;
    }

    // ============================================================
    // FOLLOW PLAYER
    // ============================================================

    private void LateUpdate()
    {
        if (target == null)
            return;

        bool tutorialActive = IsTutorialMaskVisible();

        Vector3 desiredPosition =
            CalculateCameraPosition(tutorialActive);

        float currentSmoothTime = tutorialActive
            ? smoothTime
            : normalSmoothTime;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            Mathf.Max(0.01f, currentSmoothTime)
        );
    }

    // ============================================================
    // CHECK TUTORIAL MASK
    // ============================================================

    private bool IsTutorialMaskVisible()
    {
        if (!useTutorialFraming)
            return false;

        if (prisonDarknessMask == null)
            return false;

        if (!prisonDarknessMask.isActiveAndEnabled)
            return false;

        if (darknessImage == null)
        {
            darknessImage =
                prisonDarknessMask.GetComponent<RawImage>();
        }

        return darknessImage != null &&
               darknessImage.enabled &&
               darknessImage.gameObject.activeInHierarchy;
    }

    // ============================================================
    // CALCULATE CAMERA POSITION
    // ============================================================

    private Vector3 CalculateCameraPosition(
        bool tutorialActive)
    {
        Vector3 playerPosition =
            target.position + offset;

        // CameraRoot stays at Z = 0.
        // Main Camera local Z = -10.
        playerPosition.z = offset.z;

        if (worldCamera == null)
            return playerPosition;

        if (!worldCamera.orthographic)
            return playerPosition;

        Vector2 screenPosition = tutorialActive
            ? tutorialScreenPosition
            : normalScreenPosition;

        float verticalSize =
            worldCamera.orthographicSize * 2f;

        float horizontalSize =
            verticalSize * worldCamera.aspect;

        float horizontalOffset =
            (screenPosition.x - 0.5f) *
            horizontalSize;

        float verticalOffset =
            (screenPosition.y - 0.5f) *
            verticalSize;

        playerPosition.x -= horizontalOffset;
        playerPosition.y -= verticalOffset;

        return playerPosition;
    }

    // ============================================================
    // PUBLIC CONTROLS
    // ============================================================

    public void SetTutorialFraming(bool enabled)
    {
        useTutorialFraming = enabled;
    }

    public void SnapToPlayer()
    {
        if (target == null)
            return;

        transform.position =
            CalculateCameraPosition(
                IsTutorialMaskVisible()
            );

        velocity = Vector3.zero;
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private void OnValidate()
    {
        smoothTime =
            Mathf.Max(0.01f, smoothTime);

        normalSmoothTime =
            Mathf.Max(0.01f, normalSmoothTime);

        tutorialScreenPosition.x =
            Mathf.Clamp01(tutorialScreenPosition.x);

        tutorialScreenPosition.y =
            Mathf.Clamp01(tutorialScreenPosition.y);

        normalScreenPosition.x =
            Mathf.Clamp01(normalScreenPosition.x);

        normalScreenPosition.y =
            Mathf.Clamp01(normalScreenPosition.y);
    }
}