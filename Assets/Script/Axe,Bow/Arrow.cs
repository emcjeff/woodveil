using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private bool isStuck = false;
    private bool canStick = false;
    public float stickDepth = 0.2f;

    [Header("Damage Profile")]
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private float upgradedBonusDamage = 20f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        canStick = true;
        //StartCoroutine(EnableSticking());
        Destroy(gameObject, 15f);
    }

    IEnumerator EnableSticking()
    {
        yield return new WaitForSeconds(0.05f);
        canStick = true;
    }

    void FixedUpdate()
    {
        if (isStuck) return;
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
            transform.forward = rb.linearVelocity.normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStuck || !canStick || collision.gameObject.CompareTag("Player")) return;

        // 1. Check if we hit an enemy
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            // Determine final applied damage based on Line4, Line5, and Line6 completion
            float finalDamage = CalculateArrowDamage();
            enemy.TakeDamage(finalDamage);

            // Make the arrow disappear immediately
            Destroy(gameObject);
            return; // Exit the function so it doesn't try to "stick"
        }

        // 2. If it's NOT an enemy (like a wall or the floor), do the sticking logic
        isStuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        GetComponent<Collider>().enabled = false;

        ContactPoint contact = collision.GetContact(0);
        transform.position = contact.point + (transform.forward * stickDepth);
        transform.SetParent(collision.transform);
    }

    // --- DYNAMIC DAMAGE REWARD CHECKER ---
    private float CalculateArrowDamage()
    {
        if (BookManager.Instance != null)
        {
            // Index 3 = Line4, Index 4 = Line5, Index 5 = Line6
            bool line4Done = BookManager.Instance.IsObjectiveComplete(4);
            bool line5Done = BookManager.Instance.IsObjectiveComplete(5);
            bool line6Done = BookManager.Instance.IsObjectiveComplete(6);

            if (line4Done && line5Done && line6Done)
            {
                Debug.Log($"[Arrow System] Upgraded Damage Triggered! Dealing {baseDamage + upgradedBonusDamage} damage.");
                return baseDamage + upgradedBonusDamage; // Deals 40f total damage
            }
        }

        return baseDamage; // Default back to 20f total damage
    }
}