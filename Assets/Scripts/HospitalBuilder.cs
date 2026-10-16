using UnityEngine;

/// <summary>
/// Builds the hospital shell from primitives (M1). Corridor along Z with five
/// OPD rooms behind the right wall.
/// </summary>
public static class HospitalBuilder
{
    static readonly float[] DoorsZ = { -20f, -10f, 0f, 10f, 20f };
    const float DoorW = 2.6f;
    const float WallH = 3.2f;
    const float CorridorLeftX = -6f;
    const float CorridorRightX = 0f;
    const float Length = 60f; // -30..30

    public static void Build(Transform root)
    {
        // Floor + ceiling
        MakeBox(root, "Floor", new Vector3(6f, -0.05f, 0f), new Vector3(44f, 0.1f, Length), new Color(0.78f, 0.78f, 0.8f));
        MakeBox(root, "Ceiling", new Vector3(6f, WallH, 0f), new Vector3(44f, 0.1f, Length), new Color(0.92f, 0.92f, 0.94f));

        // Left corridor wall
        MakeBox(root, "WallLeft", new Vector3(CorridorLeftX, WallH * 0.5f, 0f), new Vector3(0.2f, WallH, Length), new Color(0.86f, 0.9f, 0.95f));

        // Right corridor wall with door gaps
        float prev = -Length * 0.5f;
        int idx = 0;
        foreach (var dz in DoorsZ)
        {
            float zl = dz - DoorW * 0.5f;
            if (zl > prev)
                MakeBox(root, $"WallR{idx++}", new Vector3(CorridorRightX, WallH * 0.5f, (prev + zl) * 0.5f),
                    new Vector3(0.2f, WallH, zl - prev), new Color(0.9f, 0.9f, 0.95f));
            prev = dz + DoorW * 0.5f;
        }
        if (prev < Length * 0.5f)
            MakeBox(root, $"WallR{idx}", new Vector3(CorridorRightX, WallH * 0.5f, (prev + Length * 0.5f) * 0.5f),
                new Vector3(0.2f, WallH, Length * 0.5f - prev), new Color(0.9f, 0.9f, 0.95f));

        // OPD rooms + door frames + signs
        for (int i = 0; i < DoorsZ.Length; i++)
        {
            var tint = new Color(0.55f + 0.07f * i, 0.72f, 0.92f - 0.05f * i);
            MakeBox(root, $"OPD{i}", new Vector3(11f, WallH * 0.5f, DoorsZ[i]), new Vector3(22f, WallH, 8.5f), tint);

            MakeBox(root, $"FrameL{i}", new Vector3(CorridorRightX, WallH * 0.5f, DoorsZ[i] - DoorW * 0.5f),
                new Vector3(0.25f, WallH, 0.15f), new Color(0.5f, 0.35f, 0.2f));
            MakeBox(root, $"FrameR{i}", new Vector3(CorridorRightX, WallH * 0.5f, DoorsZ[i] + DoorW * 0.5f),
                new Vector3(0.25f, WallH, 0.15f), new Color(0.5f, 0.35f, 0.2f));
            MakeBox(root, $"Header{i}", new Vector3(CorridorRightX, WallH - 0.3f, DoorsZ[i]),
                new Vector3(0.25f, 0.6f, DoorW + 0.3f), new Color(0.5f, 0.35f, 0.2f));

            MakeSign(root, $"OPD {i + 1}", new Vector3(CorridorRightX - 0.15f, 2.6f, DoorsZ[i]), Quaternion.Euler(0f, -90f, 0f));
        }

        // Lighting
        var sun = new GameObject("Sun");
        sun.transform.SetParent(root, false);
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
    }

    static void MakeSign(Transform parent, string text, Vector3 pos, Quaternion rot)
    {
        var go = new GameObject("Sign_" + text);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = rot;
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 64;
        tm.characterSize = 0.05f;
        tm.color = Color.white;
        var font = BuiltinFont();
        if (font != null) tm.font = font;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null && font != null) mr.sharedMaterial = font.material;
    }

    public static GameObject MakeBox(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = size;
        var col = go.GetComponent<Collider>();
        if (col != null) col.isTrigger = false;
        var r = go.GetComponent<Renderer>();
        r.sharedMaterial = MaterialLibrary.Solid(color);
        return go;
    }

    static Font BuiltinFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
