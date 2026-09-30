using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f;
    public float sprintMultiplier = 1.65f;
    public bool IsSprinting { get; private set; }
    PlayerShoot shooting;
    CameraFollow view;

    void Awake() { shooting = GetComponent<PlayerShoot>(); if (Camera.main) view = Camera.main.GetComponent<CameraFollow>(); }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Move(new Vector2(horizontal, vertical), Input.GetKey(KeyCode.LeftShift), Time.deltaTime);
    }

    void Move(Vector2 input, bool sprint, float deltaTime)
    {
        Vector3 movement = new Vector3(input.x, 0f, input.y);
        if (view)
            movement = view.PlanarForward * input.y + Vector3.Cross(Vector3.up, view.PlanarForward) * input.x;
        IsSprinting = sprint && movement.magnitude > 0.1f;

        // Move the player
        if (movement.magnitude > 0.1f)
        {
            movement.Normalize();

            transform.position = Moonwing.Visuals.MoonwingForestBoundary.Clamp(
                transform.position + movement * moveSpeed * (IsSprinting ? sprintMultiplier : 1f) * deltaTime);

            // Turn the player toward the direction they are moving
            if (shooting && shooting.enabled) return; // Mouse aim owns facing, independent of WASD.
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * deltaTime
            );
        }
    }
    void OnDisable() { IsSprinting = false; }
}
