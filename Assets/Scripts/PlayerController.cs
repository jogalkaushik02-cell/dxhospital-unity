using UnityEngine;

/// <summary>
/// Third/first-person-ish character controller for the slice. Movement comes
/// from the HUD joystick (touch) with a WASD fallback for desktop testing.
/// The body always turns to face the direction of travel.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 4.5f;
    public float turnSpeed = 720f;
    public Hud hud;

    CharacterController cc;
    float verticalVelocity;
    Transform body;
    Transform nose;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 50f;
        cc.stepOffset = 0.4f;

        var blue = MaterialLibrary.Solid(new Color(0.2f, 0.5f, 0.9f));

        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Body";
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        visual.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        StripCollider(visual);
        visual.GetComponent<Renderer>().sharedMaterial = blue;
        body = visual.transform;

        var noseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        noseGo.name = "Facing";
        noseGo.transform.SetParent(transform, false);
        noseGo.transform.localPosition = new Vector3(0f, 1.35f, 0.38f);
        noseGo.transform.localScale = new Vector3(0.16f, 0.16f, 0.34f);
        StripCollider(noseGo);
        noseGo.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Solid(new Color(0.95f, 0.85f, 0.2f));
        nose = noseGo.transform;
    }

    void Update()
    {
        Vector2 input = Vector2.zero;
        if (hud != null) input += hud.Move;
        input += KeyboardInput();
        if (input.sqrMagnitude > 1f) input = input.normalized;

        if (input.sqrMagnitude > 0.0004f)
        {
            float yaw = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
            Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * new Vector3(input.x, 0f, input.y);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();
                cc.Move(dir * speed * Time.deltaTime);
                var target = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        // Gravity
        if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        cc.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);
    }

    static Vector2 KeyboardInput()
    {
        float x = 0f, y = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
        return new Vector2(x, y);
    }

    static void StripCollider(GameObject go)
    {
        var c = go.GetComponent<Collider>();
        if (c != null) Object.Destroy(c);
    }
}
