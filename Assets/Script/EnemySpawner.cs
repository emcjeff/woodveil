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
    public bool spawnOnlyOnce = false; // Turn this OFF for infinite wave loops!
    public GameObject spawnEffect;

    [Tooltip("Time delay (in seconds) BEFORE spawning the next batch after all current enemies die.")]
    [SerializeField] private float spawnCooldown = 5f;

    private bool hasSpawned = false;
    private bool isWaveActive = false; 

    private List<GameObject> activeEnemies = new List<GameObject>();
    private int lastTrackedCount = 0; // Prevents spamming the Console every single frame

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnOnlyOnce && hasSpawned) return;
            if (isWaveActive) return;

            StartCoroutine(WaveManagerRoutine());
        }
    }

    private IEnumerator WaveManagerRoutine()
    {
        isWaveActive = true;
        hasSpawned = true;

        // Spawn the very first wave immediately
        SpawnEnemies();

        if (spawnOnlyOnce) yield break;

        // INFINITE WAVE LOOP SYSTEM
        while (true)
        {
            // 1. Clean the list of dead/destroyed enemies
            activeEnemies.RemoveAll(enemy => enemy == null);

            // 2. Log to console ONLY when an enemy actually dies (avoids log spamming)
            if (activeEnemies.Count > 0 && activeEnemies.Count != lastTrackedCount)
            {
                lastTrackedCount = activeEnemies.Count;
                Debug.Log($"[Spawner] Enemy killed! Enemies remaining: {activeEnemies.Count}");
            }
            // 3. If all enemies are dead, handle the cooldown countdown!
            else if (activeEnemies.Count == 0)
            {
                Debug.Log("[Spawner] All enemies cleared! Initiating respawn cooldown...");
                
                float timer = spawnCooldown;
                float lastLoggedSecond = Mathf.Ceil(spawnCooldown);

                // Countdown Timer Loop
                while (timer > 0)
                {
                    // Only log once per second so your console stays clean
                    if (Mathf.Ceil(timer) != lastLoggedSecond)
                    {
                        lastLoggedSecond = Mathf.Ceil(timer);
                        Debug.Log($"[Spawner] Next wave respawning in: {lastLoggedSecond}s...");
                    }
                    
                    timer -= Time.deltaTime;
                    yield return null; 
                }

                // Timer hit 0! Spawn the next batch
                SpawnEnemies();
            }

            yield return null; 
        }
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("No Enemy Prefab assigned to the spawner!");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;

            Vector3 randomOffset = new Vector3(
                Random.Range(-scatterRadius, scatterRadius),
                0,
                Random.Range(-scatterRadius, scatterRadius)
            );

            Vector3 finalSpawnPos = basePos + randomOffset;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            GameObject spawnedEnemy = Instantiate(enemyPrefab, finalSpawnPos, rot);
            activeEnemies.Add(spawnedEnemy);

            if (spawnEffect != null)
            {
                Instantiate(spawnEffect, finalSpawnPos, rot);
            }
        }

        // Initialize tracking counters and broadcast the spawn event
        lastTrackedCount = activeEnemies.Count;
        Debug.Log($"[Spawner] Fresh wave deployed! Total Targets: {activeEnemies.Count}");

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