using UnityEngine;

/// <summary>
/// Entry point. Runs automatically after the (empty) build scene loads and
/// constructs the game world entirely in code — no scene/prefab assets needed.
/// </summary>
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Object.FindObjectOfType<Game>() != null) return;
        var go = new GameObject("GameRoot");
        go.AddComponent<Game>();
    }
}
