using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public bool onTarget;
    public GameObject selectedObject;

    public GameObject Interaction_Info_UI;
    private TextMeshProUGUI interaction_text;

    public Image centerDotImage;
    public Image handIcon;

    public float damage = 20f;

    private void Awake()
    {
        // PERSISTENCE FIX: 
        // This ensures the manager survives scene changes and deletes duplicates
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        onTarget = false;

        if (Interaction_Info_UI != null)
        {
            interaction_text = Interaction_Info_UI.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Interaction_Info_UI is NOT assigned in the Inspector!");
        }
    }

    void Update()
    {
        // 1. CAMERA NULL CHECK (Fixes the Line 51 NullReference error)
        if (Camera.main == null)
        {
            return; // Skip this frame if Unity hasn't found the camera yet
        }

        // Now it's safe to run this line
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;

            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();
            WeaponPickup weaponPickup = selectionTransform.GetComponent<WeaponPickup>();

            if ((interactable && interactable.playerInRange) || weaponPickup != null)
            {
                onTarget = true;
                selectedObject = selectionTransform.gameObject;

                // Set Interaction Text
                if (interaction_text != null)
                {
                    if (interactable)
                    {
                        interaction_text.text = interactable.GetItemName();
                    }
                    else if (weaponPickup)
                    {
                        interaction_text.text = weaponPickup.weaponName.Replace("UI", "");
                    }
                }

                if (Interaction_Info_UI != null) Interaction_Info_UI.SetActive(true);

                // Handle Cursor Icons
                if ((interactable && interactable.CompareTag("pickable")) || weaponPickup != null)
                {
                    if (centerDotImage != null) centerDotImage.gameObject.SetActive(false);
                    if (handIcon != null) handIcon.gameObject.SetActive(true);
                }
                else
                {
                    if (handIcon != null) handIcon.gameObject.SetActive(false);
                    if (centerDotImage != null) centerDotImage.gameObject.SetActive(true);
                }

                // 2. HANDLE INTERACTION
                if (Input.GetMouseButtonDown(0))
                {
                    if (weaponPickup != null)
                    {
                        weaponPickup.Interact();
                    }
                    else if (interactable.GetComponentInParent<DoorInteractable>())
                    {
                        interactable.GetComponentInParent<DoorInteractable>().ToogleDoor();
                    }
                    else
                    {
                        // Check for Bow busy state
                        if (BowController.Instance != null && !BowController.Instance.IsBusy())
                        {
                            interactable.PickUp();
                        }
                        else if (BowController.Instance == null)
                        {
                            interactable.PickUp();
                        }
                    }
                }
            }
            else
            {
                // Look for enemies even if not interactable
                EnemyHealth enemy = selectionTransform.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    selectedObject = enemy.gameObject;
                }
                else
                {
                    selectedObject = null;
                }

                ResetSelectionUI();
            }
        }
        else
        {
            selectedObject = null;
            ResetSelectionUI();
        }

        // 3. COMBAT LOGIC
        if (Input.GetMouseButtonUp(0))
        {
            if (selectedObject != null)
            {
                if (BowController.Instance != null && BowController.Instance.IsFired())
                {
                    EnemyHealth health = selectedObject.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(damage);
                    }
                }
            }
        }
    }

    private void ResetSelectionUI()
    {
        onTarget = false;
        if (Interaction_Info_UI != null) Interaction_Info_UI.SetActive(false);
        if (handIcon != null) handIcon.gameObject.SetActive(false);
        if (centerDotImage != null) centerDotImage.gameObject.SetActive(true);
    }

    public void DisableSelection()
    {
        this.enabled = false;
        if (handIcon != null) handIcon.gameObject.SetActive(false);
        if (centerDotImage != null) centerDotImage.gameObject.SetActive(false);
        if (Interaction_Info_UI != null) Interaction_Info_UI.SetActive(false);
        selectedObject = null;
    }

    public void EnableSelection()
    {
        this.enabled = true;
        if (centerDotImage != null) centerDotImage.gameObject.SetActive(true);
    }
}