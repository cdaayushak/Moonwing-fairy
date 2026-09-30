using UnityEngine;

[DefaultExecutionOrder(50)]
public class PlayerShoot : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float arrowSpeed = 15f;
    public float spawnDistance = 1.2f;

    private Vector3 aimDirection;
    Camera aimCamera;
    PlayerMovement movement;
    CameraFollow orbit;
    public Vector3 AimDirection => aimDirection;

    void Awake()
    {
        aimDirection = transform.forward;
        aimDirection.y = 0f;
        aimDirection.Normalize();
        movement = GetComponent<PlayerMovement>();
    }

    // CameraFollow has already updated, avoiding a one-frame cursor offset during movement.
    void LateUpdate()
    {
        if (Time.timeScale <= 0) return;
        if (!aimCamera) aimCamera = Camera.main;
        if (!orbit && aimCamera) orbit = aimCamera.GetComponent<CameraFollow>();
        if (orbit)
        {
            aimDirection = orbit.PlanarForward;
        }
        else if (TryResolveAim(aimCamera, Input.mousePosition, out var direction)) aimDirection = direction;
        if (aimDirection.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(aimDirection),
                1 - Mathf.Exp(-(movement ? movement.rotationSpeed : 12f) * Time.deltaTime));
        bool overUI = !orbit && UnityEngine.EventSystems.EventSystem.current &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!overUI && (!orbit || orbit.CanShoot) && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            Shoot();
        }
    }

    public bool TryResolveAim(Camera camera, Vector2 screenPoint, out Vector3 direction)
    {
        direction = aimDirection;
        if (!camera) return false;
        Ray ray = camera.ScreenPointToRay(screenPoint);
        // Aim at torso/arrow height. Scenery cannot steal the cursor ray or deflect shots vertically.
        var plane = new Plane(Vector3.up, transform.position);
        if (!plane.Raycast(ray, out float distance) || distance > 500f) return false;
        Vector3 delta = ray.GetPoint(distance) - transform.position;
        delta.y = 0;
        if (delta.sqrMagnitude < 0.04f) return false; // Stable dead zone directly under the cursor.
        direction = delta.normalized;
        return true;
    }

    void Shoot()
    {
        // Create the arrow slightly in front of the player
        Vector3 direction = aimDirection.sqrMagnitude > 0.01f ? aimDirection : transform.forward;
        Vector3 spawnPosition = transform.position + direction * spawnDistance;

        GameObject arrow = Instantiate(
            arrowPrefab,
            spawnPosition,
            Quaternion.LookRotation(direction, Vector3.up)
        );

        // Send the arrow forward
        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = direction * arrowSpeed;
        }

        // Clean up missed arrows after 5 seconds
        Destroy(arrow, 5f);
    }
}
