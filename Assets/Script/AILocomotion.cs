using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{
    public Transform playerTransform;
    public float detectionRange = 10f; 
    
    NavMeshAgent agent;
    private Vector3 startPosition; // Variable to remember the home base

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Record the position where the enemy starts the game
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Inside range: Follow the player
            agent.destination = playerTransform.position;
            agent.isStopped = false; 
        }
        else
        {
            // Outside range: Go back to the starting position
            agent.destination = startPosition;
            
            // Optional: If you want the agent to stop completely once it's "close enough" home
            if (Vector3.Distance(transform.position, startPosition) < 0.5f)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Visualizes the home position in the editor
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(startPosition, Vector3.one);
        }
    }
}