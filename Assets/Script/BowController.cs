using System.Collections;
using UnityEngine;

public class BowController : MonoBehaviour
{
    public static BowController Instance { get; private set; }

    private Animator BowAnimator;
    public string arrowPrefabPath = "Arrow";
    private GameObject arrowPrefab;
    public Transform spawnPosition;

    [Header("Force Settings")]
    public float minForce = 10f;
    public float maxForce = 60f; // Increased for better feel
    public float timeToMaxCharge = 1.5f;

    [Header("Interaction Buffers")]
    public float tapThreshold = 0.2f; // Anything shorter than this is a "click" (for items)
    public float shootCooldown = 0.5f; // Seconds between shots

    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private float lastShotTime = -10f; // Track cooldown

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BowAnimator = GetComponent<Animator>();
        arrowPrefab = Resources.Load<GameObject>(arrowPrefabPath);
    }

    public bool IsBusy()
    {
        // Now "Busy" means we are past the tap threshold and actually drawing
        return isCharging && (Time.time - chargeStartTime > tapThreshold);
    }

    private void Update()
    {
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) return;
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) return;

        // 1. Check Cooldown first
        if (Time.time < lastShotTime + shootCooldown) return;

        // 2. Start Holding Left Mouse
        if (Input.GetMouseButtonDown(0))
        {
            chargeStartTime = Time.time;
            isCharging = true;
        }

        // 3. While Holding: Only show animations if held longer than tapThreshold
        if (isCharging && Input.GetMouseButton(0))
        {
            if (Time.time - chargeStartTime > tapThreshold)
            {
                BowAnimator.SetBool("IsDrawing", true);
            }
        }

        // 4. Release Left Mouse
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            float holdDuration = Time.time - chargeStartTime;

            // Only shoot if they held it longer than a quick tap
            if (holdDuration > tapThreshold)
            {
                float chargePercent = Mathf.Clamp01((holdDuration - tapThreshold) / timeToMaxCharge);
                float finalForce = Mathf.Lerp(minForce, maxForce, chargePercent);

                ShootArrow(finalForce);
                lastShotTime = Time.time; // Start Cooldown
            }

            ResetBow();
        }
    }

    private void ResetBow()
    {
        isCharging = false;
        chargeStartTime = 0f;
        BowAnimator.SetBool("IsDrawing", false);
        // Ensure we go back to idle
        BowAnimator.Play("InitialState", 0, 0f);
    }

    private void ShootArrow(float force)
    {
        Vector3 shootingDirection = CalculateDirection().normalized;
        Quaternion arrowRotation = Quaternion.LookRotation(shootingDirection);

        // Spawn slightly in front to avoid hitting the player's own collider
        Vector3 safeSpawnPos = spawnPosition.position + (shootingDirection * 0.5f);

        GameObject arrow = Instantiate(arrowPrefab, safeSpawnPos, arrowRotation);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(shootingDirection * force, ForceMode.Impulse);
        }
    }

    public Vector3 CalculateDirection()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        // LayerMask check here would be good if you have a specific "Environment" layer
        if (Physics.Raycast(ray, out hit)) return hit.point - spawnPosition.position;
        return ray.GetPoint(100) - spawnPosition.position;
    }
}   