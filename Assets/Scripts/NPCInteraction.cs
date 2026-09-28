using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public Transform player;
    public float interactionDistance = 3f;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance)
        {
            Debug.Log("Hello, traveler! Welcome to Moonwing Forest.");
        }
    }
}