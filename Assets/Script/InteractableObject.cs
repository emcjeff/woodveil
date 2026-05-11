using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum InteractionType { Pickable, ObservationOnly }
    public InteractionType type;

    public bool playerInRange;
    public string ItemName;

    public string GetItemName()
    {
        return ItemName;
    }

    public void PickUp()
    {
        if (type == InteractionType.ObservationOnly)
        {
            return;
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

        // 2. SPECIAL LOGIC: The Helmet (Headlamp)
        if (ItemName.ToLower() == "helmet")
        {
            if (FlashlightController.Instance != null)
            {
                FlashlightController.Instance.PickUpHelmet();
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.LogWarning("Found Helmet, but FlashlightController.Instance is missing from the Player!");
            }
        }

        // 3. REGULAR ITEMS: (Stone, Stick, etc.)
        if (InventorySystem.Instance != null)
        {
            if (!InventorySystem.Instance.CheckIfFull())
            {
                InventorySystem.Instance.AddToInventory(ItemName);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory is full!");
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