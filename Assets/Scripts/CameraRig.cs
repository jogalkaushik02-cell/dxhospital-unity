using UnityEngine;

/// <summary>
/// Third-person follow camera. Sits close behind and slightly above the player,
/// looking at chest height — deliberately tuned so it does NOT stare at the ceiling.
/// </summary>
public class CameraRig : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.1f, -4.2f);
    public Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);
    public float followLerp = 8f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, followLerp * Time.deltaTime);

        Vector3 lookAt = target.position + lookOffset;
        Vector3 dir = lookAt - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
        {
            var rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, followLerp * Time.deltaTime);
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation((target.position + lookOffset) - transform.position, Vector3.up);
    }
}
