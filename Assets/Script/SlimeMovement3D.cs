using System.Collections;
using UnityEngine;

public class SlimeMovement3D : MonoBehaviour
{
    [Header("Targeting & Aggro")]
    public Transform player;

    [Header("Movement Settings")]
    public float jumpForce = 4f;
    public float forwardSpeed = 15f;
    public float restDuration = 1.2f;

    private Rigidbody rb;
    private bool isResting = false;
    private float searchTimer = 0f;
    private bool isDead = false;
    private bool isAgroed = false; // MAGIGING TRUE LANG KAPAG TINAMAAN NG ATTACK

    [Header("Targeting Home-Base")]
    private Vector3 homePosition;
    public float homeReturnThreshold = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        homePosition = transform.position;
        FindPlayerFallback();

        Collider slimeCollider = GetComponent<Collider>();
        if (slimeCollider != null)
        {
            PhysicsMaterial slipperyMat = new PhysicsMaterial("SlipperySlime");
            slipperyMat.dynamicFriction = 0f;
            slipperyMat.staticFriction = 0f;
            slipperyMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            slimeCollider.material = slipperyMat;
        }
    }

    private void OnDisable()
    {
        isDead = true;
        isResting = true;

        StopAllCoroutines();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
        }
    }

    void Update()
    {
        if (isDead || !enabled) return;

        if (player == null)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= 1f)
            {
                searchTimer = 0f;
                FindPlayerFallback();
            }
            return;
        }

        if (isResting) return;

        // KONDISYON NG PAGHABOL:
        // Ngayon, hahabulin ka LANG niya kung tinamaan mo siya (isAgroed == true)
        if (isAgroed)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                StartCoroutine(HopRoutine(directionToPlayer.normalized));
            }
        }
        else
        {
            // Kung hindi mo pa siya tinatamaan, tinitiyak lang natin na nasa home base siya.
            // Kung naitulak siya ng physics, dahan-dahan siyang tatalon pabalik sa tambayan niya.
            float distanceToHome = Vector3.Distance(transform.position, homePosition);

            if (distanceToHome > homeReturnThreshold)
            {
                Vector3 directionToHome = homePosition - transform.position;
                directionToHome.y = 0;

                if (directionToHome != Vector3.zero)
                {
                    StartCoroutine(HopRoutine(directionToHome.normalized));
                }
            }
        }
    }

    void FindPlayerFallback()
    {
        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }
    }

    IEnumerator HopRoutine(Vector3 jumpDirection)
    {
        isResting = true;
        transform.rotation = Quaternion.LookRotation(jumpDirection);

        Vector3 jumpVelocity = new Vector3(
            jumpDirection.x * forwardSpeed,
            jumpForce,
            jumpDirection.z * forwardSpeed
        );

        if (!isDead && enabled)
        {
            rb.linearVelocity = jumpVelocity;
        }

        yield return new WaitForSeconds(restDuration);

        if (!isDead && enabled)
        {
            isResting = false;
        }
    }

    // ========================================================
    // ITO ANG NAG-IISANG TRIGGER PARA MAGISING ANG SLIME
    // ========================================================
    public void GetHit()
    {
        if (isDead) return;
        isAgroed = true; // Dito pa lang siya magsisimulang rumesponde sa Player
    }

    private void OnDrawGizmosSelected()
    {
        // Asul na kahon para sa pinagmulan niyang pwesto (Home)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Application.isPlaying ? homePosition : transform.position, Vector3.one * 0.5f);
    }
}