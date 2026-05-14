using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damage = 20;

    // This triggers when the object touches something else
    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    // Check if the thing we hit has an EnemyHealth script
    //    EnemyHealth enemy = other.GetComponent<EnemyHealth>();

    //    if (enemy != null)
    //    {
    //        enemy.TakeDamage(damage);

    //        // If this is a bullet, destroy the bullet after impact
    //        // Destroy(gameObject); 
    //    }
    //}
    private void OnTriggerEnter(Collider other) // Changed from 2D to 3D
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}