using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int enemyCount = 3;         // Number of minions to spawn per wave
    public float scatterRadius = 2f;

    [Header("Respawn Cooldown Behavior")]
    [Tooltip("How many seconds to wait after a wave dies before spawning the next wave.")]
    public float respawnCooldown = 30f;
    public GameObject spawnEffect;

    [Header("One-Time Boss Gate Settings")]
    [Tooltip("The physical wall/barrier GameObject that locks the player inside the arena")]
    public GameObject bossGate;

    [Tooltip("Drag the specific SpiderBoss GameObject from your Hierarchy into this slot!")]
    [SerializeField] private GameObject spiderBoss;

    private List<GameObject> activeMinions = new List<GameObject>(); // Tracks spawned helper enemies
    private bool bossFightStarted = false;
    private bool gatePermanentlyUnlocked = false;
    private bool waveInProgress = false;
    private Collider triggerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. ONE-TIME BOSS ENCOUNTER ACTIVATION
            if (!bossFightStarted)
            {
                bossFightStarted = true;

                if (bossGate != null)
                {
                    bossGate.SetActive(true);
                    Debug.Log("[Arena] Boss Battle Started! Gate locked.");
                }
            }

            // 2. TRIGGER WAVE SPAWN (if a wave isn't already running)
            if (!waveInProgress)
            {
                StartCoroutine(SpawnWaveSequence());
            }
        }
    }

    private IEnumerator SpawnWaveSequence()
    {
        waveInProgress = true;

        // Temporarily disable the zone trigger while enemies are active
        if (triggerCollider != null) triggerCollider.enabled = false;

        activeMinions.Clear();

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

                activeMinions.Add(newEnemy);
            }
        }
        yield return null;
    }

    private void Update()
    {
        // CONSTANT MONITOR: Watch the Spider Boss independently of waves
        if (bossFightStarted && !gatePermanentlyUnlocked)
        {
            // The moment the SpiderBoss object reference is destroyed (He dies!)
            if (spiderBoss == null)
            {
                gatePermanentlyUnlocked = true;

                if (bossGate != null)
                {
                    bossGate.SetActive(false);
                    Debug.Log("[Arena] SpiderBoss defeated! Gate dropped PERMANENTLY.");
                }
            }
        }

        // WAVE MONITOR: Handle the minion respawn loop independently
        if (waveInProgress)
        {
            // Filter out dead minions from our monitoring list
            activeMinions.RemoveAll(minion => minion == null);

            // Once all minions from the current wave are dead
            if (activeMinions.Count == 0)
            {
                waveInProgress = false;
                StartCoroutine(RespawnCooldownRoutine());
            }
        }
    }

    private IEnumerator RespawnCooldownRoutine()
    {
        Debug.Log($"[Spawner] Wave cleared! Next minion respawn available in {respawnCooldown} seconds...");

        yield return new WaitForSeconds(respawnCooldown);

        // Turn the trigger zone back on so the player walking into it fires the next wave
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }

        Debug.Log("[Spawner] Ready for next minion wave!");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null) Gizmos.DrawWireSphere(transform.position, col.radius);
    }
}