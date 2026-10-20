using UnityEngine;
using System.Reflection;

[DefaultExecutionOrder(100)]
public class PrisonMaskHeroAnchor : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private PrisonDarknessMask darknessMask;

    [Header("INITIAL WINDOW POSITION")]
    [SerializeField, Range(-0.5f, 0.5f)]
    private float offsetX = -0.035f;

    [SerializeField, Range(-0.5f, 0.5f)]
    private float offsetY = 0.12f;

    [Header("SETTINGS")]
    [SerializeField] private bool alignOnStart = true;

    private void Start()
    {
        if (!alignOnStart)
            return;

        if (player == null)
        {
            GameObject obj =
                GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (darknessMask == null)
            darknessMask =
                GetComponent<PrisonDarknessMask>();

        if (player == null ||
            worldCamera == null ||
            darknessMask == null)
        {
            Debug.LogWarning(
                "PrisonMaskHeroAnchor: Missing reference.",
                this);
            return;
        }

        AlignWindow();
    }

    private void AlignWindow()
    {
        Vector3 viewport =
            worldCamera.WorldToViewportPoint(
                player.position);

        if (viewport.z <= 0f)
            return;

        float x = Mathf.Clamp01(
            viewport.x + offsetX);

        float y = Mathf.Clamp01(
            viewport.y + offsetY);

        System.Type maskType =
            darknessMask.GetType();

        BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        FieldInfo centerXField =
            maskType.GetField("centerX", flags);

        FieldInfo centerYField =
            maskType.GetField("centerY", flags);

        if (centerXField == null ||
            centerYField == null)
        {
            Debug.LogError(
                "PrisonMaskHeroAnchor: " +
                "centerX or centerY not found.",
                this);
            return;
        }

        centerXField.SetValue(darknessMask, x);
        centerYField.SetValue(darknessMask, y);

        MethodInfo refreshMethod =
            maskType.GetMethod(
                "RefreshMask", flags);

        if (refreshMethod != null)
            refreshMethod.Invoke(
                darknessMask, null);
        else
            Debug.LogWarning(
                "PrisonMaskHeroAnchor: " +
                "RefreshMask method not found.",
                this);
    }
}