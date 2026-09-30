using UnityEngine;

public class CaretakerInteraction : MonoBehaviour
{
    public Transform fairy;

    public float talkDistance = 4f;
    public float talkCooldown = 8f;

    private float nextTalkTime = 0f;

    private string[] caretakerLines =
    {
        "Moonwing, don't wander too far from home.",
        "The moonlight is especially bright tonight.",
        "Be careful, little fairy. The forest feels strange.",
        "Your magic is strong, but don't waste all of it.",
        "The forest has been unusually quiet tonight..."
    };

    void Update()
    {
        if (fairy == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            fairy.position
        );

        if (distance <= talkDistance &&
            Time.time >= nextTalkTime)
        {
            TalkToFairy();

            nextTalkTime =
                Time.time + talkCooldown;
        }
    }

    void TalkToFairy()
    {
        string line =
            caretakerLines[
                Random.Range(0, caretakerLines.Length)
            ];

        Debug.Log("Caretaker: " + line);
        Moonwing.Visuals.MoonwingDialogueBubble.Show(transform, line);
    }
}
