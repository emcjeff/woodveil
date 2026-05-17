using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Potion";
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        // Make sure whatever touched us is actually tagged "Player"
        if (other.CompareTag("Player"))
        {
            // Build the alert string (e.g., "Picked up: Potion x1!")
            string alertMessage = $"Picked up: {itemName} x{amount}!";

            // Send it to the UI Manager
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(alertMessage);
            }
            else
            {
                Debug.LogWarning("NotificationManager not found in the scene layout!");
            }

            // Optional: Put your inventory logic or score addition here!

            // Destroy the item object from the world
            Destroy(gameObject);
        }
    }
}