using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    [Header("UI Reference")]
    public floatingHealthBar healthBar;
    public float barVisibleDuration = 3.0f;

    private Animator anim;
    private Coroutine hideTimer;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        if (healthBar != null)
        {
            healthBar.HideBar();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.updateHealthBar(currentHealth, maxHealth);

            if (hideTimer != null)
            {
                StopCoroutine(hideTimer);
            }
            hideTimer = StartCoroutine(HideBarAfterDelay(barVisibleDuration));
        }
    }

    IEnumerator HideBarAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (healthBar != null && !isDead)
        {
            healthBar.HideBar();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (healthBar != null) healthBar.HideBar();

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (GetComponent<NavMeshAgent>() != null)
        {
            GetComponent<NavMeshAgent>().isStopped = true;
            GetComponent<NavMeshAgent>().enabled = false;
        }

        // --- SIGNAL ENEMY KILLS TO THE BOOKMANAGER ---

        if (BookManager.Instance != null)
        {
            // 1. Check for Spider Boss (Explicit validation check goes first!)
            if (gameObject.name.Contains("SpiderBoss") || gameObject.name.Contains("Spider Boss"))
            {
                BookManager.Instance.RegisterSpiderBossKill();
            }
            // 2. Check for regular Spiders (Only tracks if it's not the ultimate boss)
            else if (gameObject.name.Contains("Spider"))
            {
                BookManager.Instance.RegisterSpiderKill(gameObject.name);
            }
            // 3. Check for Slimes
            else if (gameObject.name.Contains("Slime"))
            {
                BookManager.Instance.RegisterSlimeKill(gameObject.name);
            }
        }

        // ------------------------------------------------------

        // Start the timer to drop loot and then destroy the body
        StartCoroutine(DestroyObject());
    }

    IEnumerator DestroyObject()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(2.0f);

        // --- CONNECT TO ENEMY DROP ---
        EnemyDrop dropScript = GetComponent<EnemyDrop>();
        if (dropScript != null)
        {
            dropScript.HandleDeath();
        }

        Destroy(gameObject);
    }
}