using UnityEngine;

public class NPCConversation : MonoBehaviour
{
    public Transform otherNPC;

    public float conversationDistance = 3f;
    public float conversationCooldown = 10f;

    private float nextConversationTime = 0f;

    private string[] caretakerLines =
    {
        "The forest feels different tonight.",
        "Keep watch near the old trees.",
        "Have you noticed how quiet the forest has become?",
        "Stay close. Something may be approaching."
    };

    private string[] spiritLines =
    {
        "I feel it too. The moonlight is restless.",
        "I'll keep watch.",
        "The creatures have been hiding since sunset.",
        "Something is moving beyond the trees."
    };

    void Update()
    {
        if (otherNPC == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            otherNPC.position
        );

        if (distance <= conversationDistance &&
            Time.time >= nextConversationTime)
        {
            HaveConversation();

            nextConversationTime =
                Time.time + conversationCooldown;
        }
    }

    void HaveConversation()
    {
        string caretakerLine =
            caretakerLines[
                Random.Range(0, caretakerLines.Length)
            ];

        string spiritLine =
            spiritLines[
                Random.Range(0, spiritLines.Length)
            ];

        Debug.Log("Caretaker: " + caretakerLine);
        Debug.Log("Forest Spirit: " + spiritLine);
    }
}