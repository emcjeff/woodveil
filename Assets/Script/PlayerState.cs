using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        // For testing purposes
        if (Input.GetKeyDown(KeyCode.N))
        {
            TakeDamage(10);
        }
    }

    // Detects collision with the enemy
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Slime"))
            
        {
            TakeDamage(20);
        }
    }

    // MAKE SURE THERE IS ONLY ONE OF THESE
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        healthBar.SetHealth(currentHealth);
        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player is Dead!");
        }
    }
}