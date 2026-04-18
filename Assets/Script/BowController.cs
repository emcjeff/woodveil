using System.Collections;
using UnityEngine;

public class BowController : MonoBehaviour
{
    public static BowController Instance { get; private set; }

    private Animator BowAnimator;
    public string arrowPrefabPath = "Arrow";
    private GameObject arrowPrefab;
    public Transform spawnPosition;

    public float minForce = 20f;
    public float maxForce = 120f;
    public float timeToMaxCharge = 1.2f;

    private bool isAiming = false;
    private float aimStartTime = 0f;
    private bool hasShot = false;

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
        return isAiming;
    }

    private void Update()
    {
        // 1. Right Click to Aim and Start Charging
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            hasShot = false;
            aimStartTime = Time.time; // Mark the exact time we started aiming
            BowAnimator.SetBool("IsDrawing", true);
            BowAnimator.Play("Draw", 0, 0f);
        }

        // 2. Release Right Click to Cancel
        if (Input.GetMouseButtonUp(1))
        {
            ResetBow();
        }

        // 3. Left Click to Release Arrow
        if (isAiming && Input.GetMouseButtonDown(0) && !hasShot)
        {
            // Calculate how long we have been holding Right Click
            float durationHeld = Time.time - aimStartTime;
            float chargePercent = Mathf.Clamp01(durationHeld / timeToMaxCharge);
            float finalForce = Mathf.Lerp(minForce, maxForce, chargePercent);

            ShootArrow(finalForce);
            hasShot = true;
            ResetBow(); // Force the player to re-aim for the next shot
        }
    }

    private void ResetBow()
    {
        isAiming = false;
        aimStartTime = 0f;
        BowAnimator.SetBool("IsDrawing", false);
        BowAnimator.Play("InitialState", 0, 0f);
    }

    private void ShootArrow(float force)
    {
        Vector3 shootingDirection = CalculateDirection().normalized;
        Quaternion arrowRotation = Quaternion.LookRotation(shootingDirection);

        Vector3 safeSpawnPos = spawnPosition.position + (shootingDirection * 1.0f);

        GameObject arrow = Instantiate(arrowPrefab, safeSpawnPos, arrowRotation);
        arrow.transform.SetParent(null);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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