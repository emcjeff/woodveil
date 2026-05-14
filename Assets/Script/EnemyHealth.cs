using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // Disable movement scripts so they don't move while dying
        if (GetComponent<AILocomotion>() != null) GetComponent<AILocomotion>().enabled = false;
        if (GetComponent<UnityEngine.AI.NavMeshAgent>() != null) GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;

        StartCoroutine(DestroyObject());
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }
}