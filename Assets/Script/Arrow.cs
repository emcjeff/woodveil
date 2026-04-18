using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private bool isStuck = false;
    private bool canStick = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Ignore the player's layer specifically if you have one
        // Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Player"));

        StartCoroutine(EnableSticking());
        Destroy(gameObject, 10f);
    }

    IEnumerator EnableSticking()
    {
        // Wait a tiny bit of time so the arrow clears the player's body
        yield return new WaitForSeconds(0.15f);
        canStick = true;
    }

    private void FixedUpdate()
    {
        if (!isStuck && rb.linearVelocity.magnitude > 1f)
        {
            transform.forward = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canStick) return; // Ignore collisions for the first 0.15 seconds
        if (collision.gameObject.CompareTag("Player")) return;

        if (!isStuck)
        {
            isStuck = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            transform.SetParent(collision.transform);
        }
    }
}