using System.Collections;
using UnityEngine;

public class AssassinSpawner : MonoBehaviour
{
    public GameObject assassinPrefab;
    public Transform player;
    public GameObject victoryPanel;

    // Hidden total - the player never sees this
    public int totalAssassins = 6;

    public float minimumDistance = 10f;
    public float maximumDistance = 18f;

    // Quiet time after defeating an ambush
    public float minimumDelay = 3f;
    public float maximumDelay = 7f;

    private int assassinsSpawned = 0;
    private int activeAssassins = 0;
    public bool VictoryTriggered { get; private set; }

    void Start()
{
    if (victoryPanel != null)
    {
        victoryPanel.SetActive(false);
    }

    StartCoroutine(AmbushSystem());
}
    IEnumerator AmbushSystem()
    {
        while (assassinsSpawned < totalAssassins)
        {
            // Give the player some breathing room
            float delay = Random.Range(minimumDelay, maximumDelay);
            yield return new WaitForSeconds(delay);

            // Randomly send 1, 2, or 3 assassins
            int groupSize = Random.Range(1, 4);

            // Never exceed the configured hidden total.
            groupSize = Mathf.Min(
                groupSize,
                totalAssassins - assassinsSpawned
            );

            activeAssassins = groupSize;

            for (int i = 0; i < groupSize; i++)
            {
                SpawnAssassin();
                assassinsSpawned++;
            }

            // DO NOT send another ambush until
            // every assassin in this one is defeated
            while (activeAssassins > 0)
            {
                yield return null;
            }
        }

        Debug.Log("All assassins defeated!");// Let the forest stay quiet for a moment
        yield return new WaitForSeconds(3f);

        // One terminal outcome; a capture during the settling delay must not become victory.
        FairyMagic magic = player ? player.GetComponent<FairyMagic>() : null;
        if (VictoryTriggered || (magic && magic.IsCaptured)) yield break;
        VictoryTriggered = true;
        // The existing panel now presents confirmation before its final poem.
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

// Freeze the game
Time.timeScale = 0f;
    }

    void SpawnAssassin()
    {
        Vector3 spawnPosition = FindSpawnPosition();

        GameObject assassin = Instantiate(
            assassinPrefab,
            spawnPosition,
            Quaternion.identity
        );

        AssassinAI ai = assassin.GetComponent<AssassinAI>();

        if (ai != null)
        {
            ai.player = player;
            ai.moveSpeed = Random.Range(2.0f, 3.2f);
            ai.detectionRange = maximumDistance + 5f;
        }

        // Tell this assassin which spawner created it
        EnemyHealth health =
            assassin.GetComponent<EnemyHealth>();

        if (health != null)
        {
            health.spawner = this;
        }
    }

    Vector3 FindSpawnPosition()
    {
        for (int attempt = 0; attempt < 24; attempt++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minimumDistance, maximumDistance);
            Vector3 position = player.position + new Vector3(direction.x * distance, 1f, direction.y * distance);
            if (Moonwing.Visuals.MoonwingForestBoundary.Contains(position, 1f)) return position;
        }
        Vector3 inward = new Vector3(-player.position.x, 0, -player.position.z).normalized;
        if (inward.sqrMagnitude < 0.01f) inward = Vector3.forward;
        return Moonwing.Visuals.MoonwingForestBoundary.Clamp(player.position + inward * minimumDistance + Vector3.up, 1f);
    }

    // EnemyHealth calls this when an assassin is defeated
    public void AssassinDefeated()
    {
        activeAssassins--;
    }
}
