using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;

    [HideInInspector]
    public AssassinSpawner spawner;
    bool defeated;

    public void TakeDamage(int damage)
    {
        if (defeated) return; // Two arrows in one frame must not count one assassin twice.
        health -= damage;

        Debug.Log("Assassin hit! Health: " + health);

        if (health <= 0)
        {
            defeated = true;
            if (spawner != null)
            {
                spawner.AssassinDefeated();
            }

            Destroy(gameObject);
        }
    }
}
