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

        // ========================================================
        // TOP PRIORITY: AGARANG GIGISINGIN ANG SLIME KAPAG TINAMAAN
        // Inuna natin ito para hindi ito maharang sakaling magka-error ang UI sa baba.
        // ========================================================
        SlimeMovement3D slimeMove = GetComponent<SlimeMovement3D>();
        if (slimeMove != null)
        {
            slimeMove.GetHit(); // Tinatawag ang public GetHit() para mag-Agro ang slime
        }
        // ========================================================

        // Pagkatapos ma-agro ang slime, saka babawasan ang buhay at babaguhin ang UI
        currentHealth -= amount;
        
        // Binalot natin sa try-catch para kung sakaling may error sa health bar mo kapag malayo,
        // hindi mapuputol ang buong takbo ng laro.
        try 
        {
            UpdateUI();
        }
        catch (System.Exception e) 
        {
            Debug.LogWarning("[EnemyHealth] May nakitang isyu sa Health Bar UI pero pinatuloy pa rin ang laro: " + e.Message);
        }

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

        // ========================================================
        // CRITICAL FIX: SHUT DOWN THE SLIME HOP MECHANICS INSTANTLY
        // ========================================================
        SlimeMovement3D slimeMove = GetComponent<SlimeMovement3D>();
        if (slimeMove != null)
        {
            slimeMove.enabled = false; // Toggling this off fires our self-disabling code!
        }
        // ========================================================

        // --- SIGNAL ENEMY KILLS TO THE BOOKMANAGER ---
        if (BookManager.Instance != null)
        {
            if (gameObject.name.Contains("SpiderBoss") || gameObject.name.Contains("Spider Boss"))
            {
                BookManager.Instance.RegisterSpiderBossKill();
            }
            else if (gameObject.name.Contains("Spider"))
            {
                BookManager.Instance.RegisterSpiderKill(gameObject.name);
            }
            else if (gameObject.name.Contains("Slime"))
            {
                BookManager.Instance.RegisterSlimeKill(gameObject.name);
            }
        }
        // ------------------------------------------------------

        StartCoroutine(DestroyObject());
    }

    IEnumerator DestroyObject()
    {
        // Wait safely for 2 seconds while the animation finishes on the ground
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