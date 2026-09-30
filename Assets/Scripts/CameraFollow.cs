using UnityEngine;

[DefaultExecutionOrder(-50)]
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float mouseSensitivity = 2.4f;
    public float minimumPitch = 18f;
    public float maximumPitch = 58f;
    public float smoothing = 18f;
    public RectTransform aimMarker;
    float distance, yaw, pitch, smoothYaw, smoothPitch;
    float collisionDistance;
    int capturedFrame = -1;
    public Vector3 PlanarForward => Quaternion.Euler(0, smoothYaw, 0) * Vector3.forward;
    public bool Looking => Cursor.lockState == CursorLockMode.Locked;
    public bool CanShoot => Looking && Time.frameCount > capturedFrame;

    void Start()
    {
        if (!target) return;
        Vector3 offset = transform.position - (target.position + Vector3.up * 0.45f);
        distance = offset.magnitude;
        collisionDistance = distance;
        yaw = Mathf.Atan2(-offset.x, -offset.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(Mathf.Asin(offset.y / Mathf.Max(distance, 0.1f)) * Mathf.Rad2Deg, minimumPitch, maximumPitch);
        smoothYaw = yaw; smoothPitch = pitch;
    }

    public void CapturePointer()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        capturedFrame = Time.frameCount;
    }
    public void ReleasePointer() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    void Update()
    {
        if (Time.timeScale <= 0) { if (Looking) ReleasePointer(); return; }
        if (Input.GetKeyDown(KeyCode.Escape)) { ReleasePointer(); return; }
        if (!Looking)
        {
            if (Input.GetMouseButtonDown(0)) CapturePointer();
            return;
        }
        Orbit(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
    }
    void Orbit(Vector2 delta)
    {
        yaw = Mathf.Repeat(yaw + delta.x * mouseSensitivity, 360f);
        pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, minimumPitch, maximumPitch);
    }

    void LateUpdate()
    {
        if (!target || distance <= 0) return;
        float blend = 1 - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
        if (Time.timeScale > 0)
        {
            smoothYaw = Mathf.LerpAngle(smoothYaw, yaw, blend);
            smoothPitch = Mathf.Lerp(smoothPitch, pitch, blend);
        }
        Quaternion rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);
        Vector3 pivot = target.position + Vector3.up * 0.45f;
        Vector3 desired = pivot - rotation * Vector3.forward * distance;
        float allowedDistance = distance;
        // The original actor/tree colliders remain. Avoid them without adding scenery colliders.
        if (Physics.SphereCast(pivot, 0.2f, (desired - pivot).normalized, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore)
            && hit.transform != target && !hit.transform.IsChildOf(target))
        {
            float clearDistance = Mathf.Max(2.2f, hit.distance - 0.35f);
            allowedDistance = Mathf.Min(distance, clearDistance);
        }
        collisionDistance = allowedDistance < collisionDistance ? allowedDistance : Mathf.Lerp(collisionDistance, allowedDistance, blend);
        desired = pivot - rotation * Vector3.forward * collisionDistance;
        desired.y = Mathf.Max(desired.y, 1.1f);
        transform.SetPositionAndRotation(desired, rotation);
        if (aimMarker)
        {
            aimMarker.gameObject.SetActive(Time.timeScale > 0 && Looking);
            Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position + PlanarForward * 12f);
            aimMarker.anchorMin = aimMarker.anchorMax = new Vector2(point.x, point.y);
            aimMarker.anchoredPosition = Vector2.zero;
        }
    }
    void OnApplicationFocus(bool focus) { if (!focus) ReleasePointer(); }
    void OnDisable() { ReleasePointer(); }
}
