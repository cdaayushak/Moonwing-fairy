using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float arrowSpeed = 15f;
    public float spawnDistance = 1.2f;

    void Update()
    {
        // Press SPACE to shoot
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Create the arrow slightly in front of the player
        Vector3 spawnPosition = transform.position + transform.forward * spawnDistance;

        GameObject arrow = Instantiate(
            arrowPrefab,
            spawnPosition,
            transform.rotation
        );

        // Send the arrow forward
        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * arrowSpeed;
        }

        // Clean up missed arrows after 5 seconds
        Destroy(arrow, 5f);
    }
}