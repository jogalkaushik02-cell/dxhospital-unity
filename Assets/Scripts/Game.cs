using UnityEngine;

/// <summary>
/// Assembles the thin-slice world: hospital shell, player, third-person camera,
/// one patient and the on-screen HUD. (M1 of the Unity port.)
/// </summary>
public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        var env = new GameObject("Environment").transform;
        env.SetParent(transform, false);
        HospitalBuilder.Build(env);

        // Player
        var playerGo = new GameObject("Player");
        playerGo.transform.SetParent(transform, false);
        playerGo.transform.position = new Vector3(0f, 0f, -14f);
        var player = playerGo.AddComponent<PlayerController>();

        // Camera (third-person)
        var camGo = new GameObject("MainCamera");
        camGo.transform.SetParent(transform, false);
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 300f;
        camGo.AddComponent<AudioListener>();
        var rig = camGo.AddComponent<CameraRig>();
        rig.target = playerGo.transform;

        // One patient for the slice
        Patient.Spawn(new Vector3(3.5f, 0f, 6f), "Fever", new Color(0.85f, 0.35f, 0.3f));

        // HUD
        var hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(transform, false);
        var hud = hudGo.AddComponent<Hud>();
        player.hud = hud;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
