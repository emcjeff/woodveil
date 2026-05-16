using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int enemyCount = 3;         // How many to spawn
    public float scatterRadius = 2f;   // How far apart they spawn

    [Header("Behavior")]
    public bool spawnOnlyOnce = true;
    public GameObject spawnEffect;

    [Tooltip("Time delay (in seconds) between wave spawns when 'Spawn Only Once' is unchecked")]
    [SerializeField] private float spawnCooldown = 20f;

    private bool hasSpawned = false;
    private bool isSpawnCooldownActive = false; // Prevents overlapping spawn timers

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnOnlyOnce && hasSpawned) return;

            // If it's repeating and already waiting on a cooldown timer, don't start a duplicate one
            if (!spawnOnlyOnce && isSpawnCooldownActive) return;

            StartCoroutine(SpawnSequence());
        }
    }

    private IEnumerator SpawnSequence()
    {
        if (spawnOnlyOnce)
        {
            // Spawn instantly for a single-use narrative encounter/trap
            SpawnEnemies();
        }
        else
        {
            // Lock the trigger gate so overlapping entries don't break your timing loop
            isSpawnCooldownActive = true;

            SpawnEnemies();

            // Wait out your customizable delay (e.g., 20 seconds) before releasing the lock
            yield return new WaitForSeconds(spawnCooldown);

            isSpawnCooldownActive = false;
        }
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("No Enemy Prefab assigned to the spawner!");
            return;
        }

        hasSpawned = true;

        for (int i = 0; i < enemyCount; i++)
        {
            // Determine base position
            Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;

            // Add a random offset so they don't overlap
            Vector3 randomOffset = new Vector3(
                Random.Range(-scatterRadius, scatterRadius),
                0,
                Random.Range(-scatterRadius, scatterRadius)
            );

            Vector3 finalSpawnPos = basePos + randomOffset;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            // Create the enemy
            Instantiate(enemyPrefab, finalSpawnPos, rot);

            // Create visual effect
            if (spawnEffect != null)
            {
                Instantiate(spawnEffect, finalSpawnPos, rot);
            }
        }

        // Clean up the collider component completely if it's a one-time fight trigger
        if (spawnOnlyOnce)
        {
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null) myCollider.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }

        Gizmos.color = Color.cyan;
        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, scatterRadius);
    }
}