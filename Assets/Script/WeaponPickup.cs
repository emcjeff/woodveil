using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Item Names")]
    public string weaponName = "Bow";
    public string ammoName = "Arrow";
    public int startingAmmo = 40;

    [Header("Interaction Settings")]
    public bool playerInRange = false; // The safety gate

    private bool isPickedUp = false;

    public void Interact()
    {
        // --- DIAGNOSTIC LOG ---
        Debug.LogWarning("Interact() was called on: " + gameObject.name);
        // ----------------------

        if (isPickedUp || !playerInRange) return;

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddToInventory(weaponName);
            InventorySystem.Instance.AddToInventory(ammoName, startingAmmo);
        }

        if (EquipManager.Instance != null)
        {
            EquipManager.Instance.EquipWeapon(weaponName);
        }

        // --- SIGNAL MISSION SYSTEM FOR OBJECTIVE 2 ---
        // Checks for either "Bow" or "BowUI" so the script is completely bulletproof!
        if ((weaponName == "Bow" || weaponName == "BowUI") && BookManager.Instance != null)
        {
            BookManager.Instance.CompleteObjective(2);
            Debug.Log("Success! Sent CompleteObjective(2) to BookManager.");
        }
        else if (BookManager.Instance == null)
        {
            Debug.LogError("WeaponPickup: BookManager.Instance is missing from the scene!");
        }
        // ---------------------------------------------

        isPickedUp = true;
        Debug.Log("Picked up " + weaponName + " and " + startingAmmo + " arrows!");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}