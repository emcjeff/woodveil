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

        isStuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false; // Prevents "jitter" during hops

        // slime attach
        ContactPoint contact = collision.GetContact(0);
        transform.position = contact.point + (transform.forward * stickDepth);
        transform.SetParent(collision.transform); // arrow becomes the arrows child
    }
}