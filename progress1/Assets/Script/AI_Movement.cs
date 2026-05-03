using UnityEngine;

public class AI_Movement : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 5.0f; // Higher = faster rotation
    public float walkTime = 2.0f;
    public float waitTime = 2.0f;

    private float walkCounter;
    private float waitCounter;
    public bool isWalking;

    private Rigidbody rb;
    private Animator anim;
    private Quaternion targetRotation; // Stores where we want to face

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        waitCounter = waitTime;
        walkCounter = walkTime;

        // Initialize targetRotation to current rotation so it doesn't snap at start
        targetRotation = transform.rotation;
        ChooseDirection();
    }

    void Update()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;
            if (walkCounter <= 0) StopWalking();
        }
        else
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0) ChooseDirection();
        }

        // SMOOTH ROTATION: Gradually rotate towards the targetRotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (anim != null) anim.SetBool("isRunning", isWalking);
    }

    void FixedUpdate()
    {
        if (isWalking)
        {
            // Move in the direction the slime is currently facing
            rb.linearVelocity = transform.forward * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void ChooseDirection()
    {
        int walkDirection = Random.Range(0, 4);
        isWalking = true;
        walkCounter = walkTime;

        // Instead of setting transform.rotation, we set the targetRotation
        // We still keep your 180 offset logic here
        switch (walkDirection)
        {
            case 0: targetRotation = Quaternion.Euler(0, 180, 0); break; // Forward
            case 1: targetRotation = Quaternion.Euler(0, 270, 0); break; // Right
            case 2: targetRotation = Quaternion.Euler(0, 0, 0); break;   // Backward
            case 3: targetRotation = Quaternion.Euler(0, 90, 0); break;  // Left
        }
    }

    void StopWalking()
    {
        isWalking = false;
        waitCounter = waitTime;
        rb.linearVelocity = Vector3.zero;
    }
}