using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int enemyCount = 3;         // Minions to spawn alongside the boss (if any)
    public float scatterRadius = 2f;

    [Header("Behavior")]
    public bool spawnOnlyOnce = true;
    public GameObject spawnEffect;

    [Header("Boss Gate Arena Settings")]
    [Tooltip("The physical wall/barrier GameObject that locks the player inside the arena")]
    public GameObject bossGate;

    [Tooltip("Drag the specific SpiderBoss GameObject from your Hierarchy into this slot!")]
    [SerializeField] private GameObject spiderBoss;

    private bool hasSpawned = false;
    private bool trackingBoss = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnOnlyOnce && hasSpawned) return;

            StartCoroutine(SpawnSequence());
        }
    }

    private IEnumerator SpawnSequence()
    {
        hasSpawned = true;

        // 1. LOCK THE ARENA: Raise the gate instantly when the player enters
        if (bossGate != null)
        {
            bossGate.SetActive(true);
            Debug.Log("[Arena] Boss Gate Activated! The fight with SpiderBoss has begun!");
        }

        // 2. SPAWN MINIONS/BOSS (If using prefab instantiation)
        // If your SpiderBoss is already sitting raw in the scene hierarchy, 
        // this loop will just spawn its extra minion helper enemies.
        if (enemyPrefab != null)
        {
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

                GameObject newEnemy = Instantiate(enemyPrefab, finalSpawnPos, rot);

                UnityEngine.AI.NavMeshAgent agent = newEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.Warp(finalSpawnPos);

                if (spawnEffect != null) Instantiate(spawnEffect, finalSpawnPos, rot);
            }
        }

        // Start watching the target boss object status explicitly
        trackingBoss = true;

        // Turn off this trigger zone collider so it doesn't double-fire
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = false;

        yield return null;
    }

    private void Update()
    {
        // 3. NARROWED DOWN GATE CHECK: Monitor ONLY the SpiderBoss state
        if (trackingBoss)
        {
            // The exact millisecond the SpiderBoss is killed/destroyed, its reference becomes null
            if (spiderBoss == null)
            {
                trackingBoss = false; // Kill the update monitoring loop

                if (bossGate != null)
                {
                    bossGate.SetActive(false);
                    Debug.Log("[Arena] SpiderBoss has been defeated! Boss Gate lowered permanently.");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null) Gizmos.DrawWireSphere(transform.position, col.radius);
    }
}