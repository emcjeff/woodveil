using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 10f;
    public float attackSpeed = 2.0f; // Seconds between attacks

    private float nextAttackTime;

    // Use OnTriggerStay so it keeps attacking if you stand near the enemy
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only deal damage if the current time is past the cooldown
            if (Time.time >= nextAttackTime)
            {
                if (PlayerState.Instance != null)
                {
                    PlayerState.Instance.TakeDamage(damageAmount);

                    // Set the next time the enemy is allowed to attack
                    nextAttackTime = Time.time + attackSpeed;

                    Debug.Log("Enemy Attacked! Next attack in " + attackSpeed + " seconds.");
                }
            }
        }
    }
}