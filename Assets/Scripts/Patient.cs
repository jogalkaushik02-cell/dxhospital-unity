using UnityEngine;

/// <summary>
/// A waiting patient in the corridor (M1 slice). Just a coloured capsule with a
/// floating complaint label; the full case logic comes in a later milestone.
/// </summary>
public class Patient : MonoBehaviour
{
    public string complaint;

    public static Patient Spawn(Vector3 position, string complaint, Color color)
    {
        var go = new GameObject("Patient_" + complaint);
        go.transform.position = position;

        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Body";
        visual.transform.SetParent(go.transform, false);
        visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        visual.transform.localScale = new Vector3(0.72f, 0.9f, 0.72f);
        var capCol = visual.GetComponent<Collider>();
        if (capCol != null) Object.Destroy(capCol);
        visual.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Solid(color);

        var patient = go.AddComponent<Patient>();
        patient.complaint = complaint;
        patient.AddLabel(complaint);
        return patient;
    }

    void AddLabel(string text)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 2.1f, 0f);

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 72;
        tm.characterSize = 0.04f;
        tm.color = new Color(0.1f, 0.1f, 0.1f);

        var font = BuiltinFont();
        if (font != null)
        {
            tm.font = font;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = font.material;
        }

        // Billboard toward the camera each frame.
        var billboard = go.AddComponent<Billboard>();
        billboard.target = Camera.main != null ? Camera.main.transform : null;
    }

    static Font BuiltinFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}

public class Billboard : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            var cam = Camera.main;
            if (cam == null) return;
            target = cam.transform;
        }
        transform.rotation = Quaternion.LookRotation(transform.position - target.position, Vector3.up);
    }
}
