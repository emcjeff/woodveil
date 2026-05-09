using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private bool isStuck = false;
    private bool canStick = false;
    public float stickDepth = 0.2f;

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
            // Deal damage to the enemy (assuming damage is 20, or add a public variable)
            enemy.TakeDamage(20f);

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
}