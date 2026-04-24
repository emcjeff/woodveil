using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        // Initialize health when the game starts
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Enemy hit! Remaining Health: " + currentHealth);

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy Died!");
        // You can add particle effects or animations here
        Destroy(gameObject);
    }
}
