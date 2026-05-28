using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject Interaction_Info_UI;
    public Image centerDotImage;
    public Image handIcon;
    private TextMeshProUGUI interaction_text;

    [HideInInspector] public bool onTarget;
    [HideInInspector] public GameObject selectedObject;

    private void Awake()
    {
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
        if (Interaction_Info_UI != null)
            interaction_text = Interaction_Info_UI.GetComponentInChildren<TextMeshProUGUI>();

        ResetSelectionUI();
    }

    void Update()
    {
        if (Camera.main == null) return;

        // Create Ray from center of screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f)) // 5f reach distance
        {
            Transform selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();
            WeaponPickup weaponPickup = selectionTransform.GetComponent<WeaponPickup>();
            EnemyHealth enemy = selectionTransform.GetComponent<EnemyHealth>();

            // 1. CHECK FOR INTERACTABLES (Items/Weapons)
            if ((interactable && interactable.playerInRange) || (weaponPickup && weaponPickup.playerInRange))
            {
                onTarget = true;
                selectedObject = selectionTransform.gameObject;
                UpdateUI(interactable, weaponPickup);

                if (Input.GetMouseButtonDown(0))
                {
                    HandleInteraction(interactable, weaponPickup);
                }
            }
            // 2. CHECK FOR ENEMIES
            else if (enemy != null)
            {
                selectedObject = enemy.gameObject;
                ResetSelectionUI(); // Hide "Pick Up" UI but keep enemy tracking active
            }
            else
            {
                selectedObject = null;
                ResetSelectionUI();
            }
        }
        else
        {
            selectedObject = null;
            ResetSelectionUI();
        }

        // REMOVED OLD SECTION 3 HITSCAN DAMAGE BUG FROM HERE
        // Your physical Arrow prefab scripts should apply damage inside their own collision checks!
    }

    private void UpdateUI(InteractableObject interactable, WeaponPickup weapon)
    {
        if (Interaction_Info_UI != null) Interaction_Info_UI.SetActive(true);

        // Update Text
        if (interaction_text != null)
        {
            if (interactable) interaction_text.text = interactable.GetItemName();
            else if (weapon) interaction_text.text = weapon.weaponName;
        }

        // Update Icons (Fixes the Hand Icon issue)
        bool isPickable = weapon != null || (interactable != null && interactable.type == InteractableObject.InteractionType.Pickable);

        if (centerDotImage != null) centerDotImage.gameObject.SetActive(!isPickable);
        if (handIcon != null) handIcon.gameObject.SetActive(isPickable);
    }

    private void HandleInteraction(InteractableObject interactable, WeaponPickup weapon)
    {
        if (weapon != null)
        {
            weapon.Interact();
        }
        else if (interactable != null)
        {
            // Check for door
            DoorInteractable door = interactable.GetComponentInParent<DoorInteractable>();
            if (door != null)
            {
                door.ToogleDoor();
            }
            else
            {
                // General Pickup with Bow check
                if (BowController.Instance == null || !BowController.Instance.IsBusy())
                {
                    interactable.PickUp();
                }
            }
        }
    }

    private void ResetSelectionUI()
    {
        onTarget = false;
        if (Interaction_Info_UI != null) Interaction_Info_UI.SetActive(false);
        if (handIcon != null) handIcon.gameObject.SetActive(false);
        if (centerDotImage != null && this.enabled) centerDotImage.gameObject.SetActive(true);
    }

    public void DisableSelection()
    {
        this.enabled = false;
        ResetSelectionUI();
        if (centerDotImage != null) centerDotImage.gameObject.SetActive(false);
    }

    public void EnableSelection()
    {
        this.enabled = true;
        if (centerDotImage != null) centerDotImage.gameObject.SetActive(true);
    }
}