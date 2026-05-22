using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public static EquipManager Instance { get; set; }

    [Header("Weapon Models in Hand")]
    public GameObject axeInHand;
    public GameObject bowInHand;

    [Header("Quickslot UI Configuration")]
    [Tooltip("Add a CanvasGroup component to your Axe Quickslot UI object and drag it here")]
    [SerializeField] private CanvasGroup axeQuickslotCanvasGroup;

    [Tooltip("Add a CanvasGroup component to your Bow Quickslot UI object and drag it here")]
    [SerializeField] private CanvasGroup bowQuickslotCanvasGroup;

    [Header("Alpha Settings")]
    [Range(0f, 1f)][SerializeField] private float activeAlpha = 1.0f;       // Completely visible highlighted bright state
    [Range(0f, 1f)][SerializeField] private float unequippedAlpha = 0.25f; // Dark low alpha unselected background state

    [Header("Tracking State")]
    [Tooltip("Keep track of which weapon type is currently active")]
    [SerializeField] private string currentlyEquippedWeapon = "";

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Initial sync when the game boots up
        UpdateQuickslotUI();
    }

    void Update()
    {
        // Change: Pressing '1' now acts as a toggle switch between Axe and Bow
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            HandleWeaponToggle();
        }

        // Continually check inventory bags to hide/reveal quickslots based on ownership dynamically
        UpdateQuickslotUI();
    }

    private void HandleWeaponToggle()
    {
        if (InventorySystem.Instance == null) return;

        // Safety String Checks: Find out exactly what weapons the player actually has in their bags
        bool ownsAxe = InventorySystem.Instance.itemList.Contains("Axe");
        bool ownsBow = InventorySystem.Instance.itemList.Contains("BowUI") ||
                       InventorySystem.Instance.itemList.Contains("Bow");

        // If the player has absolutely no weapons yet, do nothing
        if (!ownsAxe && !ownsBow) return;

        // Case 1: If nothing is equipped, try to pull out the Axe first, fallback to Bow
        if (string.IsNullOrEmpty(currentlyEquippedWeapon))
        {
            if (ownsAxe) EquipWeapon("Axe");
            else if (ownsBow) EquipWeapon(InventorySystem.Instance.itemList.Contains("BowUI") ? "BowUI" : "Bow");
            return;
        }

        // Case 2: Toggle Switch Logic
        if (currentlyEquippedWeapon == "Axe")
        {
            // If currently holding the Axe, try to swap to the Bow (if owned)
            if (ownsBow)
            {
                string bowItemName = InventorySystem.Instance.itemList.Contains("BowUI") ? "BowUI" : "Bow";
                EquipWeapon(bowItemName);
            }
        }
        else // Currently holding the Bow variant
        {
            // If currently holding the Bow, try to swap back to the Axe (if owned)
            if (ownsAxe)
            {
                EquipWeapon("Axe");
            }
        }
    }

    public void EquipWeapon(string name)
    {
        if (InventorySystem.Instance == null) return;

        if (InventorySystem.Instance.itemList.Contains(name))
        {
            if (name == "Axe")
            {
                axeInHand.SetActive(true);
                bowInHand.SetActive(false);

                // FIX: Search children for the AxeController (since it's on the Hitbox)
                AxeController axeScript = axeInHand.GetComponentInChildren<AxeController>();
                if (axeScript != null) axeScript.enabled = true;

                if (BowController.Instance != null) BowController.Instance.enabled = false;

                currentlyEquippedWeapon = "Axe";
                Debug.Log("Equipped: Axe");
            }
            else if (name == "BowUI" || name == "Bow")
            {
                axeInHand.SetActive(false);
                bowInHand.SetActive(true);

                // FIX: Search children to disable the AxeController
                AxeController axeScript = axeInHand.GetComponentInChildren<AxeController>();
                if (axeScript != null) axeScript.enabled = false;

                if (BowController.Instance != null) BowController.Instance.enabled = true;

                currentlyEquippedWeapon = name;
                Debug.Log($"Equipped: {name}");
            }
        }
    }

    /// <summary>
    /// Public helper function so outside scripts can determine if the speed buff applies
    /// </summary>
    public bool IsAxeEquipped()
    {
        return currentlyEquippedWeapon == "Axe";
    }

    /// <summary>
    /// Handles showing/hiding quickslots and updating their dim low alpha state conditions
    /// </summary>
    private void UpdateQuickslotUI()
    {
        if (InventorySystem.Instance == null) return;

        // 1. Check current ownership data lists
        bool ownsAxe = InventorySystem.Instance.itemList.Contains("Axe");
        bool ownsBow = InventorySystem.Instance.itemList.Contains("BowUI") || InventorySystem.Instance.itemList.Contains("Bow");

        // 2. Control AXE slot appearance 
        if (axeQuickslotCanvasGroup != null)
        {
            // Hide the game object entirely if the player hasn't looted it yet
            if (axeQuickslotCanvasGroup.gameObject.activeSelf != ownsAxe)
            {
                axeQuickslotCanvasGroup.gameObject.SetActive(ownsAxe);
            }

            // Set alpha depending on if it is active or sitting unequipped in standby
            if (ownsAxe)
            {
                bool isAxeActive = (currentlyEquippedWeapon == "Axe");
                axeQuickslotCanvasGroup.alpha = isAxeActive ? activeAlpha : unequippedAlpha;
            }
        }

        // 3. Control BOW slot appearance
        if (bowQuickslotCanvasGroup != null)
        {
            // Hide the game object entirely if the player hasn't looted it yet
            if (bowQuickslotCanvasGroup.gameObject.activeSelf != ownsBow)
            {
                bowQuickslotCanvasGroup.gameObject.SetActive(ownsBow);
            }

            // Set alpha depending on if it is active or sitting unequipped in standby
            if (ownsBow)
            {
                bool isBowActive = (currentlyEquippedWeapon == "BowUI" || currentlyEquippedWeapon == "Bow");
                bowQuickslotCanvasGroup.alpha = isBowActive ? activeAlpha : unequippedAlpha;
            }
        }
    }
}