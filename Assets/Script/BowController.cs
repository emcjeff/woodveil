using System.Collections;
using UnityEngine;

public class BowController : MonoBehaviour
{
    public static BowController Instance { get; private set; }

    // --- ADDED THIS VARIABLE ---
    private bool fired = false; 

    private Animator BowAnimator;
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

    // --- ADDED THIS FUNCTION ---
    public bool IsFired()
    {
        if (fired)
        {
            fired = false; // Reset immediately so it only triggers once
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
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) return;
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) return;

        if (Time.time < lastShotTime + shootCooldown) return;

        if (Input.GetMouseButtonDown(0))
        {
            chargeStartTime = Time.time;
            isCharging = true;
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            if (Time.time - chargeStartTime > tapThreshold)
            {
                BowAnimator.SetBool("IsDrawing", true);
            }
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            float holdDuration = Time.time - chargeStartTime;

            if (holdDuration > tapThreshold)
            {
                float chargePercent = Mathf.Clamp01((holdDuration - tapThreshold) / timeToMaxCharge);
                float finalForce = Mathf.Lerp(minForce, maxForce, chargePercent);

                ShootArrow(finalForce);
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
        // --- ADDED THIS LINE ---
        fired = true; // Tell the SelectionManager an arrow was just released

        Vector3 shootingDirection = CalculateDirection().normalized;
        Quaternion arrowRotation = Quaternion.LookRotation(shootingDirection);
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
        if (Physics.Raycast(ray, out hit)) return hit.point - spawnPosition.position;
        return ray.GetPoint(100) - spawnPosition.position;
    }
}