using UnityEngine;

public class AxeController : MonoBehaviour
{
    [Header("References")]
    public Animator axeAnimator;
    public Collider hitboxCollider;

    [Header("Settings")]
    public float axeDamage = 25f;
    public float attackCooldown = 0.8f;
    public float hitboxDuration = 0.2f;

    private float nextAttackTime = 0f;
    private bool canDealDamage = false;

    void Start()
    {
        if (hitboxCollider != null) hitboxCollider.enabled = false;
    }

    void Update()
    {
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) return;

        // CRITICAL BARE-HANDED CHECK: If the axe isn't explicitly active, turn off the script functions
        if (EquipManager.Instance == null || !EquipManager.Instance.IsAxeEquipped())
        {
            if (hitboxCollider != null && hitboxCollider.enabled) DisableHitbox();
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void PerformAttack()
    {
        if (axeAnimator != null) axeAnimator.SetTrigger("Attack");

        if (hitboxCollider != null) hitboxCollider.enabled = true;
        canDealDamage = true;

        Invoke("DisableHitbox", hitboxDuration);
    }

    void DisableHitbox()
    {
        if (hitboxCollider != null) hitboxCollider.enabled = false;
        canDealDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Block phantom collision calculations if weapon isn't equipped
        if (EquipManager.Instance == null || !EquipManager.Instance.IsAxeEquipped()) return;

        if (canDealDamage && other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(axeDamage);
                canDealDamage = false;
            }
        }
    }
}