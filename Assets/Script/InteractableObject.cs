using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum InteractionType { Pickable, ObservationOnly }
    public InteractionType type;

    [HideInInspector] // Hidden in inspector since it's tracked automatically by code
    public bool playerInRange;

    [Tooltip("Set this to 'Book', 'Page', 'Headlamp', 'Stone', etc.")]
    public string ItemName;

    [Header("Page Settings")]
    [Tooltip("Only used if ItemName is 'Page'")]
    public int pageIndex; // 0 for Page 1, 1 for Page 2, etc.

    public string GetItemName() { return ItemName; }


    private void OnMouseDown()
    {
        // SAFETY CHECK: Make sure the player is in range AND clicking THIS specific object
        if (playerInRange)
        {
            // Create a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Cast the ray into the 3D world
            if (Physics.Raycast(ray, out hit))
            {
                // ONLY execute PickUp if the ray physically struck THIS exact GameObject
                if (hit.transform == this.transform)
                {
                    PickUp();
                }
            }
        }
    }

    public void PickUp()
    {
        if (type == InteractionType.ObservationOnly) return;

        // Trigger notifications right as the collection succeeds
        TriggerNotification();

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

        // 3. SPECIAL LOGIC: The Headlamp
        if (ItemName.ToLower() == "headlamp")
        {
            if (FlashlightController.Instance != null)
            {
                FlashlightController.Instance.PickUpHelmet(); 

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

    private void TriggerNotification()
    {
        if (NotificationManager.Instance != null)
        {
            string message = "";

            if (ItemName.ToLower() == "page")
            {
                message = $"Picked up: Page {pageIndex + 1}!";
            }
            else
            {
                message = $"Picked up: {ItemName}!";
            }

            NotificationManager.Instance.ShowNotification(message);
        }
    }

    // Trigger volumes now purely act as proximity proximity checks 
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