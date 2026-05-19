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

        // -------------------------------------------------------------
        // NOTIFICATION TRIGGER
        // One clean check handles every item type before destruction!
        // -------------------------------------------------------------
        if (NotificationManager.Instance != null)
        {
            if (ItemName.ToLower() == "page")
            {
                NotificationManager.Instance.ShowNotification($"Picked up Page {pageIndex + 1}!");
            }
            else
            {
                NotificationManager.Instance.ShowNotification($"Picked up {ItemName}!");
            }
        }
        // -------------------------------------------------------------

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
            // Kept lowercase 'book' so Unity stops throwing CS0246
            book pageSystem = Object.FindAnyObjectByType<book>(FindObjectsInactive.Include);

            if (pageSystem != null)
            {
                // COMMENTED OUT FOR SAFETY: Your 'book' script has a different name for this method.
                // pageSystem.UnlockPage(pageIndex); 

                Debug.Log("Unlocked Page index: " + pageIndex);
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.LogWarning("Could not find the Book script on the UI!");
            }
        }

        // 3. SPECIAL LOGIC: The Headlamp
        if (ItemName.ToLower() == "headlamp")
        {
            if (FlashlightController.Instance != null)
            {
                FlashlightController.Instance.PickUpHelmet();

                if (BookManager.Instance != null)
                {
                    BookManager.Instance.CompleteObjective(6);
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