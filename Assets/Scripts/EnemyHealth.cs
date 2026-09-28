using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;

    [HideInInspector]
    public AssassinSpawner spawner;

    public void TakeDamage(int damage)
    {
        health -= damage;

        Debug.Log("Assassin hit! Health: " + health);

        if (health <= 0)
        {
            if (spawner != null)
            {
                spawner.AssassinDefeated();
            }

            Destroy(gameObject);
        }
    }
}