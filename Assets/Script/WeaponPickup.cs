using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Item Names")]
    public string weaponName = "Bow";
    public string ammoName = "Arrow";
    public int startingAmmo = 20;

    [Header("Interaction Settings")]
    public bool playerInRange = false; // The safety gate

    private bool isPickedUp = false;

    public void Interact()
    {
        // Only allow interaction if we are actually standing in the trigger!
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

        isPickedUp = true;
        Debug.Log("Picked up " + weaponName + " and " + startingAmmo + " arrows!");
        Destroy(gameObject);
    }

    // --- ADD THIS LOGIC ---
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