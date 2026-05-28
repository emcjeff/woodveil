using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum InteractionType { Pickable, ObservationOnly }
    public InteractionType type;

    public bool playerInRange;

    [Tooltip("Set this to 'Book', 'Page', 'Headlamp', 'Bow', 'Stone', etc.")]
    public string ItemName;

    [Header("Page Settings")]
    [Tooltip("Only used if ItemName is 'Page'")]
    public int pageIndex; // 0 for Page 1, 1 for Page 2, etc.

    public string GetItemName() { return ItemName; }

    public void PickUp()
    {
        if (type == InteractionType.ObservationOnly) return;

        // NOTIFICATION TRIGGER
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
            if (BookManager.Instance != null)
            {
                BookManager.Instance.UnlockPageGlobal(pageIndex);
                Debug.Log("Successfully unlocked global Page index: " + pageIndex);
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.LogWarning("BookManager.Instance is missing in the scene!");
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

        // 4. REGULAR ITEMS (The Bow falls back here cleanly)
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
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}