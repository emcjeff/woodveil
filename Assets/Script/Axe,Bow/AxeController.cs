using UnityEngine;

public class AxeController : MonoBehaviour
{
    [Header("References")]
    public Animator axeAnimator;   // Drag your AxeModel here
    public Collider hitboxCollider; // Drag this object's own BoxCollider here

    [Header("Settings")]
    public float axeDamage = 25f;
    public float attackCooldown = 0.8f;
    public float hitboxDuration = 0.2f; // Keep this short so it matches the "swing"

    private float nextAttackTime = 0f;
    private bool canDealDamage = false;

    void Start()
    {
        // Safety check: turn off the hitbox immediately
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    void Update()
    {
        // 1. Check if the Inventory is open (assuming you have your InventorySystem setup)
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) return;

        // 2. Click to attack
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void PerformAttack()
    {
        // Play the animation
        if (axeAnimator != null)
        {
            axeAnimator.SetTrigger("Attack");
        }

        // Enable the "Wall"
        hitboxCollider.enabled = true;
        canDealDamage = true;

        // Turn it off after the duration
        // Note: Spelling must match DisableHitbox exactly!
        Invoke("DisableHitbox", hitboxDuration);
    }

    void DisableHitbox()
    {
        hitboxCollider.enabled = false;
        canDealDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 3. Collision Logic
        if (canDealDamage && other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(axeDamage);
                Debug.Log("Smashed " + other.name + " with the Hitbox!");

                // Turn off damage so we don't hit the same enemy twice in one swing
                canDealDamage = false;
            }
        }
    }
}