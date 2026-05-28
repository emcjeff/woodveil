using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement1 : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;

    public Transform orientation;

    bool jumpRequested = false; // Add this variable at the top

    void Update()
    {
        // Capture the input in Update so we never miss a click
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //checking if we hit the ground to reset our falling velocity, otherwise we will fall faster the next time
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //right is the red Axis, forward is the blue axis
        Vector3 move = orientation.right * x + orientation.forward * z;

        // --- MOVEMENT SPEED BUFF LOGIC ---
        // Calculate current operational speed based on EquipManager status
        float currentSpeed = speed;
        if (EquipManager.Instance != null && EquipManager.Instance.IsAxeEquipped())
        {
            currentSpeed *= 2f; // Double the movement speed!
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        //check if the player is on the ground so he can jump
        if (jumpRequested)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false; // Reset the request
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}