using System.Collections;
using UnityEngine;

public class BowController : MonoBehaviour
{
    public static BowController Instance { get; private set; }

    private bool fired = false;
    private Animator BowAnimator;

    // Make sure this matches your prefab name in Resources
    public string arrowPrefabPath = "Arrow";
    private GameObject arrowPrefab;
    public Transform spawnPosition;

    [Header("Force Settings")]
    public float minForce = 10f;
    public float maxForce = 60f;
    public float timeToMaxCharge = 1.5f;

    [Header("Interaction Buffers")]
    public float tapThreshold = 0.2f;
    public float shootCooldown = 0.5f;

    [Header("Master Quest Reward")]
    [Tooltip("Managed automatically by the BookManager when Line 7, 8, and 9 are completed")]
    public bool isDoubleShotUnlocked = false;

    [Tooltip("The time delay (in seconds) between the first and second arrow during a double shot")]
    [SerializeField] private float burstDelay = 0.15f;

    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private float lastShotTime = -10f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BowAnimator = GetComponent<Animator>();
        arrowPrefab = Resources.Load<GameObject>(arrowPrefabPath);
    }

    public bool IsFired()
    {
        if (fired)
        {
            fired = false;
            return true;
        }
        return false;
    }

    public bool IsBusy()
    {
        return isCharging && (Time.time - chargeStartTime > tapThreshold);
    }

    private void Update()
    {
        // Don't allow shooting if any UI panels are open
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) return;
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) return;
        if (BookManager.Instance != null && BookManager.Instance.isBookOpen) return;

        // Manage cooldown restrictions
        if (Time.time < lastShotTime + shootCooldown) return;

        // --- CHECK FOR AMMO BEFORE STARTING DRAW ---
        if (Input.GetMouseButtonDown(0))
        {
            if (InventorySystem.Instance != null && InventorySystem.Instance.itemList.Contains("ArrowUI"))
            {
                chargeStartTime = Time.time;
                isCharging = true;
            }
            else
            {
                Debug.Log("No arrows left!");
            }
        }

        // Handle animation state while drawing back
        if (isCharging && Input.GetMouseButton(0))
        {
            if (Time.time - chargeStartTime > tapThreshold)
            {
                BowAnimator.SetBool("IsDrawing", true);
            }
        }

        // Release the arrow on Mouse Up
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            float holdDuration = Time.time - chargeStartTime;

            if (holdDuration > tapThreshold)
            {
                float chargePercent = Mathf.Clamp01((holdDuration - tapThreshold) / timeToMaxCharge);
                float finalForce = Mathf.Lerp(minForce, maxForce, chargePercent);

                // Fire the weapon system
                ShootArrow(finalForce);

                // --- CONSUME ONLY 1 AMMO (Perk bonus!) ---
                if (InventorySystem.Instance != null)
                {
                    InventorySystem.Instance.RemoveItem("ArrowUI", 1);
                }

                lastShotTime = Time.time;
            }

            ResetBow();
        }
    }

    private void ResetBow()
    {
        isCharging = false;
        chargeStartTime = 0f;
        BowAnimator.SetBool("IsDrawing", false);
        BowAnimator.Play("InitialState", 0, 0f);
    }

    private void ShootArrow(float force)
    {
        fired = true;

        Vector3 shootingDirection = CalculateDirection().normalized;
        Vector3 safeSpawnPos = spawnPosition.position + (shootingDirection * 0.5f);

        if (isDoubleShotUnlocked)
        {
            // Run the sequential burst using a Coroutine for the delay
            StartCoroutine(BurstFireRoutine(safeSpawnPos, shootingDirection, force));
        }
        else
        {
            // Standard Single Shot
            Quaternion arrowRotation = Quaternion.LookRotation(shootingDirection);
            GameObject arrow = Instantiate(arrowPrefab, safeSpawnPos, arrowRotation);
            ApplyArrowForce(arrow, shootingDirection, force);
        }
    }

    // Coroutine handles the delayed second shot sequence seamlessly
    private IEnumerator BurstFireRoutine(Vector3 safeSpawnPos, Vector3 shootingDirection, float force)
    {
        // 1. SPAWN FIRST ARROW IMMEDIATELY
        Quaternion arrowRotation1 = Quaternion.LookRotation(shootingDirection);
        GameObject arrow1 = Instantiate(arrowPrefab, safeSpawnPos, arrowRotation1);
        ApplyArrowForce(arrow1, shootingDirection, force);

        // 2. PAUSE THE CODE EXECUTION FOR THE BURST DELAY TIMER
        yield return new WaitForSeconds(burstDelay);

        // Re-calculate modern crosshair directions in case the player flicked their mouse during the delay
        Vector3 freshDirection = CalculateDirection().normalized;
        Vector3 freshSpawnPos = spawnPosition.position + (freshDirection * 0.5f);

        // 3. SPAWN THE SECOND ARROW FOLLOWING THE DELAY
        // Giving it a tiny vertical lift (2 degrees) so they don't cleanly override/clip each other mid-air
        Vector3 angledDirection = Quaternion.Euler(-2f, 0f, 0f) * freshDirection;
        Quaternion arrowRotation2 = Quaternion.LookRotation(angledDirection);

        GameObject arrow2 = Instantiate(arrowPrefab, freshSpawnPos, arrowRotation2);
        ApplyArrowForce(arrow2, angledDirection, force);
    }

    private void ApplyArrowForce(GameObject arrowInstance, Vector3 direction, float force)
    {
        Rigidbody rb = arrowInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    public Vector3 CalculateDirection()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.point - spawnPosition.position;
        return ray.GetPoint(100) - spawnPosition.position;
    }
}