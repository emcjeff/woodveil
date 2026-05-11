using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Item Names")]
    // IMPORTANT: Make sure these match the names in your Inventory and EquipManager exactly
    public string weaponName = "Bow";
    public string ammoName = "Arrow";
    public int startingAmmo = 20;

    private bool isPickedUp = false;

    public void Interact()
    {
        // Prevent double-clicking the same item in the same frame
        if (isPickedUp) return;

        // 1. Add the Bow to the inventory
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddToInventory(weaponName);
            // 2. Add the starting Arrows
            InventorySystem.Instance.AddToInventory(ammoName, startingAmmo);
        }

        // 3. Tell the EquipManager to put the bow in the player's hand
        if (EquipManager.Instance != null)
        {
            EquipManager.Instance.EquipWeapon(weaponName);
        }

        // 4. Mark as picked up and delete from the world
        isPickedUp = true;
        Debug.Log("Picked up " + weaponName + " and " + startingAmmo + " arrows!");

        Destroy(gameObject); // This removes the object from the game entirely
    }
}