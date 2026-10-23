using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Disables the Unity Rendering Debugger overlay (Display Stats)
/// in the Editor, Play Mode, and device builds.
/// Does not modify game UI, rendering quality, or gameplay systems.
/// </summary>
public static class DisableRenderingDebugger
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void DisableOnGameStart()
    {
        DisableRuntimeDebugger();
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void DisableInEditor()
    {
        DisableRuntimeDebugger();
    }
#endif

    private static void DisableRuntimeDebugger()
    {
        DebugManager manager = DebugManager.instance;

        if (manager == null)
            return;

        manager.displayRuntimeUI = false;
        manager.displayPersistentRuntimeUI = false;
        manager.enableRuntimeUI = false;
    }
}
