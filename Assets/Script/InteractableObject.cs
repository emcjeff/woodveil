using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum InteractionType { Pickable, ObservationOnly }
    public InteractionType type;

    public bool playerInRange;

    [Tooltip("Set this to 'Book', 'Page', 'Headlamp', 'Stone', etc.")]
    public string ItemName;

    [Header("Page Settings")]
    [Tooltip("Only used if ItemName is 'Page'")]
    public int pageIndex; // 0 for Page 1, 1 for Page 2, etc.

    public string GetItemName() { return ItemName; }

    public void PickUp()
    {
        if (type == InteractionType.ObservationOnly) return;

        // 1. SPECIAL LOGIC: The Book
        if (ItemName.ToLower() == "book")
        {
            if (BookManager.Instance != null)
            {
                BookManager.Instance.CollectBook();
                Destroy(gameObject);
                return;
            }
        }

        // 2. SPECIAL LOGIC: The Pages
        if (ItemName.ToLower() == "page")
        {
            book pageSystem = Object.FindAnyObjectByType<book>(FindObjectsInactive.Include);

            if (pageSystem != null)
            {
                pageSystem.UnlockPage(pageIndex);
                Debug.Log("Unlocked Page index: " + pageIndex);
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.LogWarning("Could not find the Book script on the UI!");
            }
        }

        // 3. SPECIAL LOGIC: The Headlamp (Changed from helmet!)
        if (ItemName.ToLower() == "headlamp")
        {
            if (FlashlightController.Instance != null)
            {
                FlashlightController.Instance.PickUpHelmet(); // Keeps your old flashlight system hook working

                // Cross out Objective 5 in your Book!
                if (BookManager.Instance != null)
                {
                    BookManager.Instance.CompleteObjective(5);
                }

                Destroy(gameObject);
                return;
            }
        }

        // 4. REGULAR ITEMS
        if (InventorySystem.Instance != null)
        {
            if (!InventorySystem.Instance.CheckIfFull())
            {
                InventorySystem.Instance.AddToInventory(ItemName);
                Destroy(gameObject);
            }
        }
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