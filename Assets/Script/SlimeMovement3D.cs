using System.Collections;
using UnityEngine;

public class SlimeMovement3D : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    public float detectionRadius = 12f;

    [Header("Movement Settings")]
    public float jumpForce = 4f;
    public float forwardSpeed = 15f;
    public float restDuration = 1.2f;

    private Rigidbody rb;
    private bool isResting = false;
    private float searchTimer = 0f;
    private bool isDead = false;

    [Header("Targeting Home-Base")]
    private Vector3 homePosition;
    public float homeReturnThreshold = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        homePosition = transform.position;
        FindPlayerFallback();

        Collider slimeCollider = GetComponent<Collider>();
        if (slimeCollider != null)
        {
            PhysicsMaterial slipperyMat = new PhysicsMaterial("SlipperySlime");
            slipperyMat.dynamicFriction = 0f;
            slipperyMat.staticFriction = 0f;
            slipperyMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            slimeCollider.material = slipperyMat;
        }
    }

    // ========================================================
    // AUTOMATIC SHUTDOWN TRIGGER
    // ========================================================
    private void OnDisable()
    {
        // This ensures the shutdown ONLY affects this single slime instance
        isDead = true;
        isResting = true;

        // 1. Instantly kill this specific slime's active coroutines
        StopAllCoroutines();

        // 2. Kill the velocity completely so it hits the ground naturally
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
        }
    }
    // ========================================================

    void Update()
    {
        if (isDead || !enabled) return;

        if (player == null)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= 1f)
            {
                searchTimer = 0f;
                FindPlayerFallback();
            }
            return;
        }

        if (isResting) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                StartCoroutine(HopRoutine(directionToPlayer.normalized));
            }
        }
        else
        {
            float distanceToHome = Vector3.Distance(transform.position, homePosition);

            if (distanceToHome > homeReturnThreshold)
            {
                Vector3 directionToHome = homePosition - transform.position;
                directionToHome.y = 0;

                if (directionToHome != Vector3.zero)
                {
                    StartCoroutine(HopRoutine(directionToHome.normalized));
                }
            }
        }
    }

    void FindPlayerFallback()
    {
        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }
    }

    IEnumerator HopRoutine(Vector3 jumpDirection)
    {
        isResting = true;
        transform.rotation = Quaternion.LookRotation(jumpDirection);

        Vector3 jumpVelocity = new Vector3(
            jumpDirection.x * forwardSpeed,
            jumpForce,
            jumpDirection.z * forwardSpeed
        );

        if (!isDead && enabled)
        {
            rb.linearVelocity = jumpVelocity;
        }

        yield return new WaitForSeconds(restDuration);

        if (!isDead && enabled)
        {
            isResting = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Application.isPlaying ? homePosition : transform.position, Vector3.one * 0.5f);
    }
}