using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{
    [Header("Settings")]
    public Transform playerTransform;
    public float detectionRange = 10f;

    private NavMeshAgent agent;
    private Animator anim;
    private EnemyHealth health;

    void Start()
    {
        // Link all the parts of the enemy
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();

        // Safety check in case you forgot to drag the player in
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    void Update()
    {
        // 1. HEALTH CHECK: Stop moving if the enemy is dead
        if (health != null && health.currentHealth <= 0)
        {
            StopMovement();
            return;
        }

        // 2. PLAYER CHECK: Don't do anything if there is no player
        if (playerTransform == null) return;

        // 3. DISTANCE CHECK: Should we chase or stay still?
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Inside range: Start Chase
            agent.destination = playerTransform.position;
            agent.isStopped = false;
            UpdateAnimation(true);
        }
        else
        {
            // Outside range: Stop Chase
            StopMovement();
        }
    }

    // Helper to stop movement and animations at the same time
    void StopMovement()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }
        UpdateAnimation(false);
    }

    // Updates both parameters (since you added both to the Animator)
    void UpdateAnimation(bool isMoving)
    {
        if (anim != null)
        {
            anim.SetBool("isWalking", isMoving);
            anim.SetBool("isRunning", isMoving);
        }
    }

    // Draws the red circle in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}