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

    // ========================================================
    // NEW HOME-BASE VARIABLES
    // ========================================================
    private Vector3 homePosition;
    public float homeReturnThreshold = 1f; // How close to home before it stops hopping
    // ========================================================

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Save the exact spot the slime was placed in the editor
        homePosition = transform.position;

        FindPlayerFallback();

        // Automatic Frictionless Material for Unity 6
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

    void Update()
    {
        // Handle looking for the player if they haven't loaded yet
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

        // ========================================================
        // UPDATED MOVEMENT LOGIC
        // ========================================================
        if (distanceToPlayer <= detectionRadius)
        {
            // CASE 1: Player is close! Chase the player.
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                StartCoroutine(HopRoutine(directionToPlayer.normalized));
            }
        }
        else
        {
            // CASE 2: Player is too far away! Check if we need to hop back home.
            float distanceToHome = Vector3.Distance(transform.position, homePosition);

            // Only hop if we aren't already sitting directly on our home spot
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
        // ========================================================
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

        // Unity 6 Pure Velocity Punch
        rb.linearVelocity = jumpVelocity;

        yield return new WaitForSeconds(restDuration);

        isResting = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range in Yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw a little blue diamond at its home position in the scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Application.isPlaying ? homePosition : transform.position, Vector3.one * 0.5f);
    }
}