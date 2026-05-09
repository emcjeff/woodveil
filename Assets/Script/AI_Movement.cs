using UnityEngine;

public class AI_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 5.0f;
    public float walkTime = 2.0f;
    public float waitTime = 2.0f;

    [Header("Status")]
    public bool isWalking;

    private float timer;
    private Rigidbody rb;
    private Animator anim;
    private Quaternion targetRotation;
    private EnemyHealth health;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();

        targetRotation = transform.rotation;

        // Start by waiting
        isWalking = false;
        timer = waitTime;
    }

    void Update()
    {
        // 1. If the enemy is dead, stop all movement logic
        if (health != null && health.currentHealth <= 0) return;

        // 2. Handle State Switching
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (isWalking) StopWalking();
            else ChooseDirection();
        }

        // 3. Handle Rotation (Happens whether walking or standing)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 4. Update Animator
        if (anim != null)
        {
            // Note: I used "isWalking" to match your spider logic
            anim.SetBool("isWalking", isWalking);

            // If you still use "isRunning" for slimes, 
            // you can add this line too:
            anim.SetBool("isRunning", isWalking);
        }
    }

    void FixedUpdate()
    {
        // Stop physics movement if dead
        if (health != null && health.currentHealth <= 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        if (isWalking)
        {
            rb.linearVelocity = transform.forward * moveSpeed;
        }
        else
        {
            // Clean stop
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    public void ChooseDirection()
    {
        isWalking = true;
        timer = walkTime;

        // Pick a random angle (0 to 360 degrees) for more natural movement
        float randomAngle = Random.Range(0, 4) * 90f;
        targetRotation = Quaternion.Euler(0, randomAngle, 0);
    }

    void StopWalking()
    {
        isWalking = false;
        timer = waitTime;
        rb.linearVelocity = Vector3.zero;
    }
}