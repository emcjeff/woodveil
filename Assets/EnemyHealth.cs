using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Effects")]
    public GameObject deathEffect; // Optional: Particle system for when they die

    [SerializeField] private FloatingHealthBar healthBar;

    void Start()
    {
        // Initialize health at the start
        currentHealth = maxHealth;

        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Optional: Play a "Hit" animation or sound here
        Debug.Log(gameObject.name + " took damage! Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffect != null) 
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
    }

        // Destroy the enemy object
        Destroy(gameObject);
        
        Debug.Log(gameObject.name + " has died.");
    }
}   