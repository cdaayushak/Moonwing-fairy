using UnityEngine;

public class NPCWander : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float changeDirectionTime = 2f;

    private Vector3 direction;
    private float timer;

    void Start()
    {
        ChooseNewDirection();
    }

    void Update()
    {
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ChooseNewDirection();
        }
    }

    void ChooseNewDirection()
    {
        direction = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        timer = changeDirectionTime;
    }
}