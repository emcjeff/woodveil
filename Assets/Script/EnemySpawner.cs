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

    [Header("Boss Gate Arena Settings")]
    [Tooltip("The physical wall/barrier GameObject that locks the player inside the arena")]
    public GameObject bossGate;

    [Tooltip("Time delay (in seconds) between wave spawns when 'Spawn Only Once' is unchecked")]
    [SerializeField] private float spawnCooldown = 20f;

    private bool hasSpawned = false;
    private bool isSpawnCooldownActive = false; // Prevents overlapping spawn timers

    // Tracks all currently alive enemies spawned by this specific component
    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool trackingEnemies = false;

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
        aliveEnemies.Clear();

        // 1. RAISE THE BOSS GATE HOOHOOHO! Lock the player in!
        if (bossGate != null)
        {
            bossGate.SetActive(true);
            Debug.Log("[Arena] Boss Gate Activated! Player is trapped!");
        }

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
            GameObject newEnemy = Instantiate(enemyPrefab, finalSpawnPos, rot);

            // Add them to our tracking list so we know when they die
            aliveEnemies.Add(newEnemy);

            // NAVMESH SAFETY CHECK
            UnityEngine.AI.NavMeshAgent agent = newEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(finalSpawnPos);
            }

            // Create visual effect
            if (spawnEffect != null)
            {
                Instantiate(spawnEffect, finalSpawnPos, rot);
            }
        }

        // Start tracking our list of alive monsters
        trackingEnemies = true;

        // Clean up the collider component completely if it's a one-time fight trigger
        if (spawnOnlyOnce)
        {
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null) myCollider.enabled = false;
        }
    }

    private void Update()
    {
        // Only run checking calculations if we actively have monsters to watch
        if (trackingEnemies)
        {
            // Clean up missing/destroyed enemy null references out of the collection array
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (aliveEnemies[i] == null)
                {
                    aliveEnemies.RemoveAt(i);
                }
            }

            // 2. LOWER THE BOSS GATE! If the count drops back down to zero, open the layout back up!
            if (aliveEnemies.Count == 0)
            {
                trackingEnemies = false; // Turn off monitoring loop updates

                if (bossGate != null)
                {
                    bossGate.SetActive(false);
                    Debug.Log("[Arena] All enemies clear! Boss Gate opened.");
                }
            }
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