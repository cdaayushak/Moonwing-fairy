using UnityEngine;

public class AssassinAI : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2.5f;
    public float detectionRange = 23f;
    public float stopDistance = 1.5f;

    public float drainRange = 3.5f;
    public float magicDrainPerSecond = 8f;

    private Vector3 attackOffset;
    private FairyMagic fairyMagic;

    void Start()
    {
        Vector2 randomOffset = Random.insideUnitCircle.normalized;

        attackOffset = new Vector3(
            randomOffset.x,
            0f,
            randomOffset.y
        ) * Random.Range(1.0f, 2.5f);
    }

    void Update()
    {
        if (player == null)
            return;

        // Find the fairy's magic once the Player has been assigned
        if (fairyMagic == null)
        {
            fairyMagic = player.GetComponent<FairyMagic>();
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Chase the fairy
        if (distance <= detectionRange && distance > stopDistance)
        {
            Vector3 targetPosition;

            if (distance > 4f)
            {
                // Approach from different directions
                targetPosition = player.position + attackOffset;
            }
            else
            {
                // Once close, actually close in
                targetPosition = player.position;
            }

            Vector3 direction =
                (targetPosition - transform.position).normalized;

            direction.y = 0f;

            transform.position +=
                direction * moveSpeed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                transform.rotation =
                    Quaternion.LookRotation(direction);
            }
        }

        // Drain the fairy's magic when close
        if (distance <= drainRange && fairyMagic != null)
        {
            fairyMagic.DrainMagic(
                magicDrainPerSecond * Time.deltaTime
            );
        }
    }
}